using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using GodlessFantasy.UI;

namespace GodlessFantasy.Network
{
    /// <summary>
    /// Quản lý Thực Thể Người Chơi Mạng & Đồng Bộ Di Chuyển Thời Gian Thực:
    /// - Quản lý sinh / hủy các NetworkPlayerEntity trong cùng Vũ Trụ.
    /// - Đồng bộ di chuyển của Local Player lên Server với cơ chế Delta-Throttling (120ms heartbeat + Waypoint Trigger).
    /// - Không làm nghẽn mạng, giữ độ trễ siêu thấp và chuyển động mượt mà như game offline.
    /// </summary>
    public class NetworkPlayerManager : MonoBehaviour
    {
        public static NetworkPlayerManager Instance { get; private set; }

        private readonly Dictionary<int, NetworkPlayerEntity> remotePlayers = new Dictionary<int, NetworkPlayerEntity>();

        // Local Player Sync Context
        private Transform localPlayerTransform;
        private PlayerMovement localPlayerMovement;
        private Vector3 lastSentPosition;
        private Vector3 lastSentTarget;
        private bool lastSentIsMoving;
        private float lastSyncTime;
        private const float SyncInterval = 0.12f; // 120ms = ~8 lần/giây (Cực kỳ nhẹ mạng và mượt mà)

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

        private void Start()
        {
            var net = GameNetworkClient.Instance;
            if (net != null)
            {
                net.OnWorldEnterResult += HandleWorldEnter;
                net.OnRemotePlayerSpawned += HandlePlayerSpawned;
                net.OnRemotePlayerDespawned += HandlePlayerDespawned;
                net.OnRemotePlayerMoved += HandlePlayerMoved;
                net.OnChatReceived += HandleChatReceived;
            }
        }

        private void OnDestroy()
        {
            var net = GameNetworkClient.Instance;
            if (net != null)
            {
                net.OnWorldEnterResult -= HandleWorldEnter;
                net.OnRemotePlayerSpawned -= HandlePlayerSpawned;
                net.OnRemotePlayerDespawned -= HandlePlayerDespawned;
                net.OnRemotePlayerMoved -= HandlePlayerMoved;
                net.OnChatReceived -= HandleChatReceived;
            }
        }

        private void Update()
        {
            SyncLocalPlayerMovement();
        }

        private void SyncLocalPlayerMovement()
        {
            var net = GameNetworkClient.Instance;
            if (net == null || !net.IsConnected || net.CurrentCharacter == null) return;

            if (localPlayerTransform == null)
            {
                var pObj = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
                if (pObj != null)
                {
                    localPlayerTransform = pObj.transform;
                    localPlayerMovement = pObj.GetComponent<PlayerMovement>();
                }
            }
            if (localPlayerTransform == null) return;

            Vector3 currentPos = localPlayerTransform.position;
            Vector3 currentTarget = (localPlayerMovement != null) ? localPlayerMovement.TargetPosition : currentPos;
            bool isMoving = (localPlayerMovement != null) && localPlayerMovement.IsMoving;

            float timeSinceLastSync = Time.time - lastSyncTime;
            bool positionChanged = Vector3.Distance(currentPos, lastSentPosition) > 0.15f;
            bool targetChanged = Vector3.Distance(currentTarget, lastSentTarget) > 0.25f;
            bool stateChanged = (isMoving != lastSentIsMoving);

            // Gửi gói tin chỉ khi: Có đổi trạng thái, hoặc đổi đích đến, hoặc chu kỳ nhịp tim 120ms khi đang chạy
            if (stateChanged || targetChanged || (isMoving && positionChanged && timeSinceLastSync >= SyncInterval))
            {
                lastSyncTime = Time.time;
                lastSentPosition = currentPos;
                lastSentTarget = currentTarget;
                lastSentIsMoving = isMoving;

                string dirStr = (ProceduralCharacterRig2D.Instance != null) ? ProceduralCharacterRig2D.Instance.FacingDir.ToString() : "South";

                net.SendPacket(OpCodes.PlayerMoveReq, new PlayerMoveReqDto
                {
                    posX = currentPos.x,
                    posY = currentPos.y,
                    targetX = currentTarget.x,
                    targetY = currentTarget.y,
                    isMoving = isMoving,
                    direction = dirStr
                });
            }
        }

        private void HandleWorldEnter(WorldEnterRespDto resp)
        {
            if (!resp.success) return;

            // Xóa người chơi cũ nếu có
            ClearAllRemotePlayers();

            // Sinh toàn bộ người chơi khác đang trong cùng vũ trụ
            if (resp.otherPlayers != null)
            {
                foreach (var p in resp.otherPlayers)
                {
                    SpawnRemotePlayer(p);
                }
            }

            Debug.Log($"<color=green>[MULTIPLAYER]</color> Gia nhập vũ trụ thành công! Đang hiển thị {remotePlayers.Count} người chơi khác.");
        }

        private void HandlePlayerSpawned(CharacterInfoDto info)
        {
            // Bỏ qua nếu là chính mình
            var net = GameNetworkClient.Instance;
            if (net != null && net.CurrentCharacter != null && net.CurrentCharacter.id == info.id) return;

            SpawnRemotePlayer(info);
            FloatingDamageManager.ShowFloatingText(new Vector3(info.posX, info.posY + 1.2f, 0),
                $"✨ {info.name} [{info.className} · {info.element}] đã giáng sinh!", new Color(0.4f, 1f, 0.6f));
        }

        private void SpawnRemotePlayer(CharacterInfoDto info)
        {
            if (remotePlayers.ContainsKey(info.id)) return;

            var playerObj = new GameObject($"RemotePlayer_{info.id}_{info.name}");
            var entity = playerObj.AddComponent<NetworkPlayerEntity>();
            entity.Initialize(info);

            remotePlayers[info.id] = entity;
        }

        private void HandlePlayerDespawned(int playerId)
        {
            if (remotePlayers.TryGetValue(playerId, out var entity))
            {
                if (entity != null)
                {
                    FloatingDamageManager.ShowFloatingText(entity.transform.position + Vector3.up * 1.2f,
                        $"💨 {entity.Info.name} đã rời thế giới.", new Color(0.7f, 0.7f, 0.8f));
                    Destroy(entity.gameObject);
                }
                remotePlayers.Remove(playerId);
            }
        }

        private void HandlePlayerMoved(PlayerMoveBroadcastDto moveDto)
        {
            if (remotePlayers.TryGetValue(moveDto.playerId, out var entity))
            {
                entity.UpdateMovement(moveDto);
            }
        }

        private void HandleChatReceived(ChatBroadcastDto chat)
        {
            // Đưa tin nhắn vào kênh chat của CombatHUD
            if (CombatHUD.Instance != null)
            {
                CombatHUD.Instance.ReceiveNetworkChatMessage(chat.senderName, chat.message, chat.channel, chat.element);
            }
        }

        private void ClearAllRemotePlayers()
        {
            foreach (var kvp in remotePlayers)
            {
                if (kvp.Value != null) Destroy(kvp.Value.gameObject);
            }
            remotePlayers.Clear();
        }
    }
}
