using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GodlessFantasyServer.Data;

namespace GodlessFantasyServer.Network
{
    public class ClientSession
    {
        public string SessionId { get; } = Guid.NewGuid().ToString("N");
        private readonly WebSocket _socket;
        private readonly WebSocketServer _server;
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly CancellationTokenSource _cts = new();

        // Account & Character Context
        public int AccountId { get; set; } = 0;
        public string Username { get; set; } = "";
        public int SelectedUniverseId { get; set; } = 0;
        public CharacterData? ActiveCharacter { get; set; }
        public bool IsInWorld { get; set; } = false;

        // Last Position
        public float PosX { get; set; } = 0f;
        public float PosY { get; set; } = 0f;

        // ================= CHIẾN LƯỢC RATE LIMIT 3 CẤP ĐỘ MỀM DẺO =================
        private int _packetCount = 0;
        private DateTime _lastResetTime = DateTime.UtcNow;
        private int _consecutiveViolations = 0;
        private DateTime _cooldownUntil = DateTime.MinValue;

        private const int MaxPacketsPerSecond = 25; // Ngưỡng an toàn cho game 2D (Level 1)
        private const int KickThreshold = 100;       // Ngưỡng phát hiện tool flood (Level 3)

        public ClientSession(WebSocket socket, WebSocketServer server)
        {
            _socket = socket;
            _server = server;
        }

        public async Task HandleConnectionAsync()
        {
            var buffer = new byte[1024 * 8]; // 8KB buffer

            try
            {
                while (_socket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                {
                    var receiveResult = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                        break;
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                        await ProcessIncomingPacketAsync(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Session {SessionId[..6]}] Disconnected: {ex.Message}");
            }
            finally
            {
                await CleanupAsync();
            }
        }

        private async Task ProcessIncomingPacketAsync(string rawJson)
        {
            DateTime now = DateTime.UtcNow;

            // 1. Kiểm tra Cooldown (Cấp độ 2 đang áp dụng)
            if (now < _cooldownUntil)
            {
                // Đang bị hạn chế thao tác trong 5s, bỏ qua toàn bộ gói tin
                return;
            }

            // 2. Tính toán số lượng gói tin trong cửa sổ 1 giây
            if ((now - _lastResetTime).TotalSeconds >= 1.0)
            {
                if (_packetCount <= MaxPacketsPerSecond)
                {
                    // Trở lại trạng thái bình thường -> reset chuỗi vi phạm
                    _consecutiveViolations = 0;
                }
                _packetCount = 0;
                _lastResetTime = now;
            }

            _packetCount++;

            // 3. CẤP ĐỘ 3: ĐÁ KHỎI SERVER (KICK) NẾU FLOOD KINH HOÀNG
            if (_packetCount > KickThreshold)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[SECURITY] Session {SessionId[..6]} bị kick do gửi {_packetCount} gói/s (Flood Tool Attack)!");
                Console.ResetColor();

                await SendPacketAsync(OpCodes.WarningNotice, new WarningNoticeDto
                {
                    message = "Kết nối bị ngắt do phát hiện gửi dữ liệu bất thường!",
                    cooldownSeconds = 0
                });

                _cts.Cancel();
                await _socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Rate limit flood", CancellationToken.None);
                return;
            }

            // 4. CẤP ĐỘ 1: BỎ QUA GÓI TIN THỪA (DROP PACKET)
            if (_packetCount > MaxPacketsPerSecond)
            {
                _consecutiveViolations++;

                // 5. CẤP ĐỘ 2: TẠM KHÓA THAO TÁC NẾU VƯỢT NGƯỠNG LIÊN TỤC 3 GIÂY
                if (_consecutiveViolations >= 3)
                {
                    _cooldownUntil = now.AddSeconds(5);
                    _consecutiveViolations = 0;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[SECURITY] Session {SessionId[..6]} bị tạm dừng thao tác 5s do vi phạm liên tục!");
                    Console.ResetColor();

                    await SendPacketAsync(OpCodes.WarningNotice, new WarningNoticeDto
                    {
                        message = "Thao tác quá nhanh! Vui lòng thử lại sau 5 giây.",
                        cooldownSeconds = 5
                    });
                }

                // Hủy bỏ gói tin thừa, không xử lý để bảo vệ CPU & SQLite
                return;
            }

            // 6. Xử lý gói tin hợp lệ
            try
            {
                var envelope = JsonSerializer.Deserialize<PacketEnvelope>(rawJson);
                if (envelope != null && !string.IsNullOrEmpty(envelope.op))
                {
                    await _server.DispatchPacketAsync(this, envelope.op, envelope.data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Packet Error] Session {SessionId[..6]}: {ex.Message}");
            }
        }

        public async Task SendPacketAsync<T>(string opCode, T payload)
        {
            if (_socket.State != WebSocketState.Open || _cts.IsCancellationRequested) return;

            string dataJson = JsonSerializer.Serialize(payload);
            var envelope = new PacketEnvelope
            {
                op = opCode,
                data = dataJson
            };
            string fullJson = JsonSerializer.Serialize(envelope);
            byte[] bytes = Encoding.UTF8.GetBytes(fullJson);

            await _sendLock.WaitAsync();
            try
            {
                if (_socket.State == WebSocketState.Open)
                {
                    await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch
            {
                // Ignore socket closing errors
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task CleanupAsync()
        {
            try
            {
                await _server.OnSessionDisconnectedAsync(this);
                _cts.Dispose();
                _sendLock.Dispose();
                _socket.Dispose();
            }
            catch
            {
            }
        }
    }
}
