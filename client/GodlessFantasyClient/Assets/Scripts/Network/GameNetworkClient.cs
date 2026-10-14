using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GodlessFantasy.Network
{
    /// <summary>
    /// Client WebSocket Network Manager cho Unity:
    /// - Kết nối bất đồng bộ không gây khựng màn hình (Non-blocking async).
    /// - Điều phối an toàn các gói tin từ Thread mạng về Unity Main Thread bằng ConcurrentQueue.
    /// - Đạt chuẩn bảo mật và hiệu năng thời gian thực.
    /// </summary>
    public class GameNetworkClient : MonoBehaviour
    {
        public static GameNetworkClient Instance { get; private set; }

        [Header("Connection Config")]
        [SerializeField] private string serverUrl = "ws://localhost:8088/ws/";
        public string ServerUrl => serverUrl;

        private ClientWebSocket socket;
        private CancellationTokenSource cts;
        private readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        public bool IsConnected => socket != null && socket.State == WebSocketState.Open;

        // Session State
        public int LoggedInAccountId { get; private set; } = 0;
        public string LoggedInUsername { get; private set; } = "";
        public int CurrentUniverseId { get; private set; } = 0;
        public string CurrentUniverseName { get; private set; } = "";
        public CharacterInfoDto CurrentCharacter { get; private set; }

        // Events
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<AuthRegisterRespDto> OnRegisterResult;
        public event Action<AuthLoginRespDto> OnLoginResult;
        public event Action<UniverseListRespDto> OnUniverseListReceived;
        public event Action<UniverseSelectRespDto> OnUniverseSelected;
        public event Action<CharRollStatsRespDto> OnRollStatsResult;
        public event Action<CharCreateRespDto> OnCharacterCreated;
        public event Action<WorldEnterRespDto> OnWorldEnterResult;
        public event Action<CharacterInfoDto> OnRemotePlayerSpawned;
        public event Action<int> OnRemotePlayerDespawned;
        public event Action<PlayerMoveBroadcastDto> OnRemotePlayerMoved;
        public event Action<ChatBroadcastDto> OnChatReceived;
        public event Action<WarningNoticeDto> OnWarningNotice;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Thực thi an toàn trên Unity Main Thread
            while (mainThreadQueue.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Network MainThread Error]: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        public async Task<bool> ConnectAsync(string customUrl = null)
        {
            if (IsConnected) return true;

            string url = customUrl ?? serverUrl;
            serverUrl = url;

            try
            {
                cts?.Cancel();
                cts = new CancellationTokenSource();
                socket = new ClientWebSocket();

                Debug.Log($"<color=cyan>[NETWORK]</color> Connecting to WebSocket Server at {url}...");
                await socket.ConnectAsync(new Uri(url), cts.Token);

                if (socket.State == WebSocketState.Open)
                {
                    Debug.Log("<color=green>[NETWORK]</color> Connected successfully to Godless Fantasy Server!");
                    Enqueue(() => OnConnected?.Invoke());
                    _ = Task.Run(ReceiveLoopAsync, cts.Token);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"<color=orange>[NETWORK]</color> Connect failed: {ex.Message}");
                Enqueue(() => OnDisconnected?.Invoke($"Không thể kết nối đến máy chủ: {ex.Message}"));
            }

            return false;
        }

        public async void Disconnect()
        {
            if (socket == null) return;
            try
            {
                cts?.Cancel();
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
                }
            }
            catch { }
            finally
            {
                socket?.Dispose();
                socket = null;
                Enqueue(() => OnDisconnected?.Invoke("Đã ngắt kết nối khỏi máy chủ."));
            }
        }

        public async void SendPacket<T>(string opCode, T payload)
        {
            if (!IsConnected)
            {
                Debug.LogWarning($"[NETWORK] Cannot send {opCode}: Socket is not connected!");
                return;
            }

            try
            {
                string dataJson = JsonUtility.ToJson(payload);
                var envelope = new PacketEnvelope
                {
                    op = opCode,
                    data = dataJson
                };
                string fullJson = JsonUtility.ToJson(envelope);
                byte[] bytes = Encoding.UTF8.GetBytes(fullJson);

                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NETWORK] Send {opCode} error: {ex.Message}");
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[1024 * 16]; // 16KB buffer

            try
            {
                while (socket != null && socket.State == WebSocketState.Open && !cts.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Enqueue(() => OnDisconnected?.Invoke("Máy chủ đã đóng kết nối."));
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        DispatchPacket(json);
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Enqueue(() => OnDisconnected?.Invoke($"Mất kết nối: {ex.Message}"));
            }
        }

        private void DispatchPacket(string rawJson)
        {
            try
            {
                var envelope = JsonUtility.FromJson<PacketEnvelope>(rawJson);
                if (envelope == null || string.IsNullOrEmpty(envelope.op)) return;

                switch (envelope.op)
                {
                    case OpCodes.AuthRegisterResp:
                        var regResp = JsonUtility.FromJson<AuthRegisterRespDto>(envelope.data);
                        Enqueue(() => OnRegisterResult?.Invoke(regResp));
                        break;

                    case OpCodes.AuthLoginResp:
                        var loginResp = JsonUtility.FromJson<AuthLoginRespDto>(envelope.data);
                        if (loginResp.success)
                        {
                            LoggedInAccountId = loginResp.accountId;
                            LoggedInUsername = loginResp.username;
                        }
                        Enqueue(() => OnLoginResult?.Invoke(loginResp));
                        break;

                    case OpCodes.UniverseListResp:
                        var uniList = JsonUtility.FromJson<UniverseListRespDto>(envelope.data);
                        Enqueue(() => OnUniverseListReceived?.Invoke(uniList));
                        break;

                    case OpCodes.UniverseSelectResp:
                        var uniSelect = JsonUtility.FromJson<UniverseSelectRespDto>(envelope.data);
                        if (uniSelect.success)
                        {
                            CurrentUniverseId = uniSelect.universeId;
                            CurrentUniverseName = uniSelect.universeName;
                            CurrentCharacter = uniSelect.character;
                        }
                        Enqueue(() => OnUniverseSelected?.Invoke(uniSelect));
                        break;

                    case OpCodes.CharRollStatsResp:
                        var rollResp = JsonUtility.FromJson<CharRollStatsRespDto>(envelope.data);
                        Enqueue(() => OnRollStatsResult?.Invoke(rollResp));
                        break;

                    case OpCodes.CharCreateResp:
                        var createResp = JsonUtility.FromJson<CharCreateRespDto>(envelope.data);
                        if (createResp.success && createResp.character != null)
                        {
                            CurrentCharacter = createResp.character;
                        }
                        Enqueue(() => OnCharacterCreated?.Invoke(createResp));
                        break;

                    case OpCodes.WorldEnterResp:
                        var enterResp = JsonUtility.FromJson<WorldEnterRespDto>(envelope.data);
                        if (enterResp.success && enterResp.self != null)
                        {
                            CurrentCharacter = enterResp.self;
                        }
                        Enqueue(() => OnWorldEnterResult?.Invoke(enterResp));
                        break;

                    case OpCodes.PlayerSpawnBroadcast:
                        var spawnDto = JsonUtility.FromJson<CharacterInfoDto>(envelope.data);
                        Enqueue(() => OnRemotePlayerSpawned?.Invoke(spawnDto));
                        break;

                    case OpCodes.PlayerDespawnBroadcast:
                        var despawnDto = JsonUtility.FromJson<PlayerDespawnBroadcastDto>(envelope.data);
                        Enqueue(() => OnRemotePlayerDespawned?.Invoke(despawnDto.playerId));
                        break;

                    case OpCodes.PlayerMoveBroadcast:
                        var moveDto = JsonUtility.FromJson<PlayerMoveBroadcastDto>(envelope.data);
                        Enqueue(() => OnRemotePlayerMoved?.Invoke(moveDto));
                        break;

                    case OpCodes.ChatBroadcast:
                        var chatDto = JsonUtility.FromJson<ChatBroadcastDto>(envelope.data);
                        Enqueue(() => OnChatReceived?.Invoke(chatDto));
                        break;

                    case OpCodes.WarningNotice:
                        var warnDto = JsonUtility.FromJson<WarningNoticeDto>(envelope.data);
                        Enqueue(() => OnWarningNotice?.Invoke(warnDto));
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NETWORK DISPATCH ERROR]: {ex.Message}\nRaw: {rawJson}");
            }
        }

        private void Enqueue(Action action)
        {
            mainThreadQueue.Enqueue(action);
        }

        private void OnDestroy()
        {
            Disconnect();
        }
    }
}
