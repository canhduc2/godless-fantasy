using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GodlessFantasyServer.Data;
using GodlessFantasyServer.Logic;

namespace GodlessFantasyServer.Network
{
    public class WebSocketServer
    {
        private readonly HttpListener _listener = new();
        private readonly ConcurrentDictionary<string, ClientSession> _sessions = new();
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ClientSession>> _universeSessions = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly int _port;

        public WebSocketServer(int port = 8088)
        {
            _port = port;
            _listener.Prefixes.Add($"http://localhost:{_port}/ws/");
            _listener.Prefixes.Add($"http://127.0.0.1:{_port}/ws/");
        }

        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[SERVER] WebSocket Gateway is listening on port {_port} (/ws/)");
                Console.ResetColor();

                _ = Task.Run(PeriodicPositionSaveLoopAsync);

                while (!_cts.IsCancellationRequested)
                {
                    var context = await _listener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = ProcessWebSocketRequestAsync(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER CRITICAL] Listener error: {ex.Message}");
            }
        }

        private async Task ProcessWebSocketRequestAsync(HttpListenerContext context)
        {
            WebSocketContext? wsContext = null;
            try
            {
                wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
                Console.WriteLine($"[WS Error] Accept failed: {ex.Message}");
                return;
            }

            var session = new ClientSession(wsContext.WebSocket, this);
            _sessions[session.SessionId] = session;
            Console.WriteLine($"[CONNECT] New connection [{session.SessionId[..6]}] - Total Online: {_sessions.Count}");

            await session.HandleConnectionAsync();
        }

        public async Task OnSessionDisconnectedAsync(ClientSession session)
        {
            _sessions.TryRemove(session.SessionId, out _);

            if (session.SelectedUniverseId > 0 && _universeSessions.TryGetValue(session.SelectedUniverseId, out var uniDict))
            {
                uniDict.TryRemove(session.SessionId, out _);

                if (session.IsInWorld && session.ActiveCharacter != null)
                {
                    // Lưu tọa độ cuối vào SQLite
                    await DatabaseManager.Instance.SaveCharacterPositionAsync(session.ActiveCharacter.Id, session.PosX, session.PosY);

                    // Báo cho toàn bộ người chơi khác trong vũ trụ biến mất
                    await BroadcastToUniverseAsync(session.SelectedUniverseId, OpCodes.PlayerDespawnBroadcast, new
                    {
                        playerId = session.ActiveCharacter.Id
                    }, excludeSessionId: session.SessionId);

                    Console.WriteLine($"[WORLD] Player {session.ActiveCharacter.Name} left Universe {session.SelectedUniverseId}");
                }
            }

            Console.WriteLine($"[DISCONNECT] Connection [{session.SessionId[..6]}] left - Remaining Online: {_sessions.Count}");
        }

        public async Task BroadcastToUniverseAsync<T>(int universeId, string opCode, T payload, string? excludeSessionId = null)
        {
            if (!_universeSessions.TryGetValue(universeId, out var dict)) return;

            foreach (var kvp in dict)
            {
                if (excludeSessionId != null && kvp.Key == excludeSessionId) continue;
                if (kvp.Value.IsInWorld)
                {
                    await kvp.Value.SendPacketAsync(opCode, payload);
                }
            }
        }

        public async Task DispatchPacketAsync(ClientSession session, string opCode, string dataJson)
        {
            switch (opCode)
            {
                case OpCodes.AuthRegisterReq:
                    await HandleRegisterAsync(session, dataJson);
                    break;

                case OpCodes.AuthLoginReq:
                    await HandleLoginAsync(session, dataJson);
                    break;

                case OpCodes.UniverseListReq:
                    await HandleUniverseListAsync(session);
                    break;

                case OpCodes.UniverseSelectReq:
                    await HandleUniverseSelectAsync(session, dataJson);
                    break;

                case OpCodes.CharRollStatsReq:
                    await HandleCharRollStatsAsync(session, dataJson);
                    break;

                case OpCodes.CharCreateReq:
                    await HandleCharCreateAsync(session, dataJson);
                    break;

                case OpCodes.WorldEnterReq:
                    await HandleWorldEnterAsync(session);
                    break;

                case OpCodes.PlayerMoveReq:
                    await HandlePlayerMoveAsync(session, dataJson);
                    break;

                case OpCodes.ChatSendReq:
                    await HandleChatSendAsync(session, dataJson);
                    break;

                default:
                    Console.WriteLine($"[DISPATCH] Unknown OpCode: {opCode}");
                    break;
            }
        }

        #region Handlers

        private async Task HandleRegisterAsync(ClientSession session, string dataJson)
        {
            var req = JsonSerializer.Deserialize<AuthRegisterReqDto>(dataJson);
            if (req == null) return;

            var (success, msg, _) = await DatabaseManager.Instance.RegisterAccountAsync(req.username, req.password);
            await session.SendPacketAsync(OpCodes.AuthRegisterResp, new AuthRegisterRespDto
            {
                success = success,
                message = msg
            });

            if (success)
            {
                Console.WriteLine($"[AUTH] Account registered: {req.username}");
            }
        }

        private async Task HandleLoginAsync(ClientSession session, string dataJson)
        {
            var req = JsonSerializer.Deserialize<AuthLoginReqDto>(dataJson);
            if (req == null) return;

            var (success, msg, accountId, username) = await DatabaseManager.Instance.AuthenticateAccountAsync(req.username, req.password);

            if (success)
            {
                session.AccountId = accountId;
                session.Username = username;
                Console.WriteLine($"[AUTH] Account logged in: {username} (ID: {accountId})");
            }

            await session.SendPacketAsync(OpCodes.AuthLoginResp, new AuthLoginRespDto
            {
                success = success,
                message = msg,
                accountId = accountId,
                username = username
            });
        }

        private async Task HandleUniverseListAsync(ClientSession session)
        {
            var dbUniverses = await DatabaseManager.Instance.GetUniversesAsync();
            var dtoList = new List<UniverseInfoDto>();

            foreach (var u in dbUniverses)
            {
                int online = 0;
                if (_universeSessions.TryGetValue(u.Id, out var dict))
                {
                    online = dict.Count;
                }

                bool hasChar = false;
                if (session.AccountId > 0)
                {
                    var charData = await DatabaseManager.Instance.GetCharacterByAccountAndUniverseAsync(session.AccountId, u.Id);
                    hasChar = (charData != null);
                }

                int totalChars = await DatabaseManager.Instance.GetCharacterCountInUniverseAsync(u.Id);

                dtoList.Add(new UniverseInfoDto
                {
                    id = u.Id,
                    name = u.Name,
                    description = u.Description,
                    maxPlayers = u.MaxPlayers,
                    onlinePlayers = online,
                    totalCharacters = totalChars,
                    hasCharacter = hasChar
                });
            }

            await session.SendPacketAsync(OpCodes.UniverseListResp, new UniverseListRespDto
            {
                universes = dtoList
            });
        }

        private async Task HandleUniverseSelectAsync(ClientSession session, string dataJson)
        {
            var req = JsonSerializer.Deserialize<UniverseSelectReqDto>(dataJson);
            if (req == null || session.AccountId <= 0) return;

            // Xóa khỏi vũ trụ cũ nếu có
            if (session.SelectedUniverseId > 0 && _universeSessions.TryGetValue(session.SelectedUniverseId, out var oldDict))
            {
                oldDict.TryRemove(session.SessionId, out _);
            }

            session.SelectedUniverseId = req.universeId;
            var uniDict = _universeSessions.GetOrAdd(req.universeId, _ => new ConcurrentDictionary<string, ClientSession>());
            uniDict[session.SessionId] = session;

            var character = await DatabaseManager.Instance.GetCharacterByAccountAndUniverseAsync(session.AccountId, req.universeId);
            session.ActiveCharacter = character;
            if (character != null)
            {
                session.PosX = character.PosX;
                session.PosY = character.PosY;
            }

            string uniName = (req.universeId == 1) ? "Vũ Trụ 1 - Sơ Khai" : (req.universeId == 2) ? "Vũ Trụ 2 - Hỗn Độn" : $"Vũ Trụ {req.universeId}";

            await session.SendPacketAsync(OpCodes.UniverseSelectResp, new UniverseSelectRespDto
            {
                success = true,
                message = $"Đã kết nối vào {uniName}",
                universeId = req.universeId,
                universeName = uniName,
                hasCharacter = (character != null),
                character = (character != null) ? MapToDto(character) : null
            });

            Console.WriteLine($"[UNIVERSE] Session {session.SessionId[..6]} selected Universe {req.universeId} (HasChar: {character != null})");
        }

        private async Task HandleCharRollStatsAsync(ClientSession session, string dataJson)
        {
            if (session.AccountId <= 0 || session.SelectedUniverseId <= 0)
            {
                await session.SendPacketAsync(OpCodes.CharRollStatsResp, new CharRollStatsRespDto
                {
                    message = "Chưa đăng nhập hoặc chưa chọn vũ trụ!",
                    rerollsLeft = 0
                });
                return;
            }

            var req = JsonSerializer.Deserialize<CharRollStatsReqDto>(dataJson);
            string cls = req?.className ?? "Kị Sĩ";
            bool isReroll = req?.isReroll ?? false;

            var pending = await DatabaseManager.Instance.GetPendingRollAsync(session.AccountId, session.SelectedUniverseId);

            if (pending == null)
            {
                // Chưa có lần đổ xúc xắc nào: Khởi tạo lần đầu tiên (Initial Roll)
                var rolled = CharacterGenerator.RollStats(cls);
                pending = new PendingCharacterRoll
                {
                    AccountId = session.AccountId,
                    UniverseId = session.SelectedUniverseId,
                    ClassName = cls,
                    Element = rolled.element,
                    Strength = rolled.str,
                    Intelligence = rolled.intStat,
                    Vitality = rolled.vit,
                    Agility = rolled.agi,
                    RerollsLeft = 1 // Cho phép quay thêm đúng 1 lần duy nhất theo GDD
                };
                await DatabaseManager.Instance.SaveOrUpdatePendingRollAsync(pending);

                await session.SendPacketAsync(OpCodes.CharRollStatsResp, new CharRollStatsRespDto
                {
                    element = pending.Element,
                    str = pending.Strength,
                    intStat = pending.Intelligence,
                    vit = pending.Vitality,
                    agi = pending.Agility,
                    totalPoints = pending.Strength + pending.Intelligence + pending.Vitality + pending.Agility,
                    rerollsLeft = pending.RerollsLeft,
                    isExistingRoll = false,
                    message = "Khởi tạo thuộc tính thành công!"
                });
                Console.WriteLine($"[ROLL] Initial roll for Acc {session.AccountId} in Uni {session.SelectedUniverseId}: {pending.Element} (Rerolls left: {pending.RerollsLeft})");
            }
            else
            {
                // Đã có bản roll trước đó
                if (isReroll)
                {
                    // Người dùng bấm nút Đổ Lại
                    if (pending.RerollsLeft <= 0)
                    {
                        // Đã hết lượt re-roll -> Từ chối và trả về kết quả cũ
                        await session.SendPacketAsync(OpCodes.CharRollStatsResp, new CharRollStatsRespDto
                        {
                            element = pending.Element,
                            str = pending.Strength,
                            intStat = pending.Intelligence,
                            vit = pending.Vitality,
                            agi = pending.Agility,
                            totalPoints = pending.Strength + pending.Intelligence + pending.Vitality + pending.Agility,
                            rerollsLeft = 0,
                            isExistingRoll = true,
                            message = "Bạn đã hết lượt đổ xúc xắc miễn phí (Tối đa 1 lần đổi)!"
                        });
                        Console.WriteLine($"[ROLL BLOCKED] Acc {session.AccountId} attempted re-roll but has 0 rerolls left.");
                        return;
                    }

                    // Tiêu hao 1 lượt re-roll
                    var rolled = CharacterGenerator.RollStats(cls);
                    pending.ClassName = cls;
                    pending.Element = rolled.element;
                    pending.Strength = rolled.str;
                    pending.Intelligence = rolled.intStat;
                    pending.Vitality = rolled.vit;
                    pending.Agility = rolled.agi;
                    pending.RerollsLeft = 0; // Hết lượt!
                    await DatabaseManager.Instance.SaveOrUpdatePendingRollAsync(pending);

                    await session.SendPacketAsync(OpCodes.CharRollStatsResp, new CharRollStatsRespDto
                    {
                        element = pending.Element,
                        str = pending.Strength,
                        intStat = pending.Intelligence,
                        vit = pending.Vitality,
                        agi = pending.Agility,
                        totalPoints = pending.Strength + pending.Intelligence + pending.Vitality + pending.Agility,
                        rerollsLeft = 0,
                        isExistingRoll = false,
                        message = "Đã sử dụng lượt đổ xúc xắc lại cuối cùng!"
                    });
                    Console.WriteLine($"[ROLL] Re-rolled for Acc {session.AccountId}: {pending.Element} (0 rerolls left)");
                }
                else
                {
                    // Người chơi vào lại cửa sổ tạo nhân vật (ví dụ ấn Quay lại rồi vào lại)
                    // Server giữ nguyên kết quả trước đó và số lượt re-roll còn lại, KHÔNG tự động re-roll!
                    await session.SendPacketAsync(OpCodes.CharRollStatsResp, new CharRollStatsRespDto
                    {
                        element = pending.Element,
                        str = pending.Strength,
                        intStat = pending.Intelligence,
                        vit = pending.Vitality,
                        agi = pending.Agility,
                        totalPoints = pending.Strength + pending.Intelligence + pending.Vitality + pending.Agility,
                        rerollsLeft = pending.RerollsLeft,
                        isExistingRoll = true,
                        message = pending.RerollsLeft > 0 ? "Khôi phục bộ thuộc tính đã đổ trước đó." : "Khôi phục bộ thuộc tính (Đã hết lượt đổ lại)."
                    });
                    Console.WriteLine($"[ROLL RESTORE] Restored roll for Acc {session.AccountId}: {pending.Element} (Rerolls left: {pending.RerollsLeft})");
                }
            }
        }

        private async Task HandleCharCreateAsync(ClientSession session, string dataJson)
        {
            var req = JsonSerializer.Deserialize<CharCreateReqDto>(dataJson);
            if (req == null || session.AccountId <= 0 || session.SelectedUniverseId <= 0)
            {
                await session.SendPacketAsync(OpCodes.CharCreateResp, new CharCreateRespDto
                {
                    success = false,
                    message = "Chưa đăng nhập hoặc chưa chọn vũ trụ!"
                });
                return;
            }

            // Lấy thông tin pending roll có thật từ Database
            var pending = await DatabaseManager.Instance.GetPendingRollAsync(session.AccountId, session.SelectedUniverseId);
            string finalElement = pending != null ? pending.Element : req.element;
            int finalStr = pending != null ? pending.Strength : req.str;
            int finalInt = pending != null ? pending.Intelligence : req.intStat;
            int finalVit = pending != null ? pending.Vitality : req.vit;
            int finalAgi = pending != null ? pending.Agility : req.agi;

            var (success, msg, newChar) = await DatabaseManager.Instance.CreateCharacterAsync(
                session.AccountId, session.SelectedUniverseId, req.name, req.gender,
                req.className, finalElement, finalStr, finalInt, finalVit, finalAgi
            );

            if (success && newChar != null)
            {
                // Xóa pending roll vì nhân vật đã được tạo thành công!
                await DatabaseManager.Instance.DeletePendingRollAsync(session.AccountId, session.SelectedUniverseId);

                session.ActiveCharacter = newChar;
                session.PosX = newChar.PosX;
                session.PosY = newChar.PosY;
                Console.WriteLine($"[CHAR] Created character: {newChar.Name} ({newChar.ClassName} - {newChar.Element}) in Universe {session.SelectedUniverseId}");
            }

            await session.SendPacketAsync(OpCodes.CharCreateResp, new CharCreateRespDto
            {
                success = success,
                message = msg,
                character = (newChar != null) ? MapToDto(newChar) : null
            });
        }

        private async Task HandleWorldEnterAsync(ClientSession session)
        {
            if (session.ActiveCharacter == null || session.SelectedUniverseId <= 0)
            {
                await session.SendPacketAsync(OpCodes.WorldEnterResp, new WorldEnterRespDto
                {
                    success = false,
                    message = "Nhân vật chưa sẵn sàng để vào thế giới!"
                });
                return;
            }

            session.IsInWorld = true;

            // 1. Tập hợp danh sách người chơi khác đang trong cùng vũ trụ
            var otherPlayers = new List<CharacterInfoDto>();
            if (_universeSessions.TryGetValue(session.SelectedUniverseId, out var uniDict))
            {
                foreach (var kvp in uniDict)
                {
                    if (kvp.Key != session.SessionId && kvp.Value.IsInWorld && kvp.Value.ActiveCharacter != null)
                    {
                        var dto = MapToDto(kvp.Value.ActiveCharacter);
                        dto.posX = kvp.Value.PosX;
                        dto.posY = kvp.Value.PosY;
                        otherPlayers.Add(dto);
                    }
                }
            }

            var selfDto = MapToDto(session.ActiveCharacter);
            selfDto.posX = session.PosX;
            selfDto.posY = session.PosY;

            await session.SendPacketAsync(OpCodes.WorldEnterResp, new WorldEnterRespDto
            {
                success = true,
                message = "Gia nhập thế giới thành công!",
                self = selfDto,
                otherPlayers = otherPlayers
            });

            // 2. Broadcast cho toàn bộ người chơi khác trong vũ trụ biết người chơi mới đã xuất hiện!
            await BroadcastToUniverseAsync(session.SelectedUniverseId, OpCodes.PlayerSpawnBroadcast, selfDto, excludeSessionId: session.SessionId);

            Console.WriteLine($"[WORLD] {session.ActiveCharacter.Name} entered Universe {session.SelectedUniverseId} at ({session.PosX:F1}, {session.PosY:F1}) - Visible Others: {otherPlayers.Count}");
        }

        private async Task HandlePlayerMoveAsync(ClientSession session, string dataJson)
        {
            if (!session.IsInWorld || session.ActiveCharacter == null) return;

            var req = JsonSerializer.Deserialize<PlayerMoveReqDto>(dataJson);
            if (req == null) return;

            // Cập nhật vị trí hiện tại
            session.PosX = req.posX;
            session.PosY = req.posY;

            // Broadcast cho tất cả người chơi khác trong vũ trụ để họ nội suy mượt mà (Smoothing Interpolation)
            var moveBroadcast = new PlayerMoveBroadcastDto
            {
                playerId = session.ActiveCharacter.Id,
                posX = req.posX,
                posY = req.posY,
                targetX = req.targetX,
                targetY = req.targetY,
                isMoving = req.isMoving,
                direction = req.direction
            };

            await BroadcastToUniverseAsync(session.SelectedUniverseId, OpCodes.PlayerMoveBroadcast, moveBroadcast, excludeSessionId: session.SessionId);
        }

        private async Task HandleChatSendAsync(ClientSession session, string dataJson)
        {
            if (!session.IsInWorld || session.ActiveCharacter == null) return;

            var req = JsonSerializer.Deserialize<ChatSendReqDto>(dataJson);
            if (req == null || string.IsNullOrWhiteSpace(req.message)) return;

            string cleanMsg = req.message.Trim();
            if (cleanMsg.Length > 120) cleanMsg = cleanMsg[..120];

            var broadcast = new ChatBroadcastDto
            {
                senderId = session.ActiveCharacter.Id,
                senderName = session.ActiveCharacter.Name,
                channel = req.channel,
                message = cleanMsg,
                element = session.ActiveCharacter.Element
            };

            await BroadcastToUniverseAsync(session.SelectedUniverseId, OpCodes.ChatBroadcast, broadcast);
            Console.WriteLine($"[CHAT] [{req.channel}] {session.ActiveCharacter.Name}: {cleanMsg}");
        }

        private static CharacterInfoDto MapToDto(CharacterData c)
        {
            return new CharacterInfoDto
            {
                id = c.Id,
                name = c.Name,
                gender = c.Gender,
                className = c.ClassName,
                element = c.Element,
                str = c.Strength,
                intStat = c.Intelligence,
                vit = c.Vitality,
                agi = c.Agility,
                level = c.Level,
                hp = c.CurrentHp,
                maxHp = c.MaxHp,
                mana = c.CurrentMana,
                maxMana = c.MaxMana,
                exp = c.CurrentExp,
                maxExp = c.MaxExp,
                posX = c.PosX,
                posY = c.PosY
            };
        }

        #endregion

        private async Task PeriodicPositionSaveLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), _cts.Token);

                    foreach (var session in _sessions.Values)
                    {
                        if (session.IsInWorld && session.ActiveCharacter != null)
                        {
                            await DatabaseManager.Instance.SaveCharacterPositionAsync(session.ActiveCharacter.Id, session.PosX, session.PosY);
                        }
                    }
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Periodic Save Error]: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            try
            {
                _cts.Cancel();
                if (_listener.IsListening)
                {
                    _listener.Stop();
                }
            }
            catch { }
        }
    }
}
