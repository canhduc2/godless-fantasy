using UnityEngine;
using GodlessFantasy.Combat;
using GodlessFantasy.UI;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// NPC Dân Làng / Trưởng Làng Tân Thủ:
    /// - Đứng trong Vùng An Toàn (Safe Zone), miễn nhiễm mọi sát thương.
    /// - Khi người chơi đến gần hoặc click vào: Mở hộp thoại nhiệm vụ hướng dẫn người mới.
    /// </summary>
    public class VillageNPC : MonoBehaviour
    {
        [Header("NPC Specs")]
        [SerializeField] private string npcName = "Trưởng Làng Eldrin";
        [SerializeField] private string dialogue = "Chào mừng dũng sĩ đến với Godless Fantasy! Hãy sử dụng Chiêu 1 [Phím 1] và Lướt [Shift] để luyện tập với các Hình Nộm gỗ ngoài kia trước khi bước vào vùng chiến trường đẫm máu!";

        public static bool IsPlayerNearAnyNPC { get; private set; } = false;
        public static bool IsAnyNPCDialogueOpen { get; private set; } = false;

        private bool isPlayerNearby = false;
        private bool showDialogue = false;
        private Transform playerTransform;

        private void Start()
        {
            var health = GetComponent<Health>();
            if (health != null) health.IsInSafeZone = true;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerMovement>();
                if (player != null) playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                isPlayerNearby = (dist <= 2.8f);
                if (!isPlayerNearby) showDialogue = false;
            }

            IsPlayerNearAnyNPC = isPlayerNearby;
            IsAnyNPCDialogueOpen = showDialogue;

            // BẤM PHÍM E ĐỂ NÓI CHUYỆN KHI LẠI GẦN
            if (isPlayerNearby && Core.GameInput.GetInteractKeyDown())
            {
                showDialogue = !showDialogue;
            }
        }

        private void OnMouseDown()
        {
            if (isPlayerNearby)
            {
                showDialogue = !showDialogue;
            }
        }

        private void OnGUI()
        {
            // LỚP 1 (Depth 20): Bảng gợi ý NPC ở thế giới luôn nằm dưới HUD và Menu
            GUI.depth = FantasyUITheme.DepthOverhead;

            if (isPlayerNearby && !showDialogue)
            {
                Camera cam = Camera.main;
                if (cam == null) return;
                Vector3 screenPos = cam.WorldToScreenPoint(transform.position + new Vector3(0, 1.25f, 0));
                if (screenPos.z > 0)
                {
                    float promptW = 160f;
                    float promptH = 22f;
                    Rect promptRect = new Rect(screenPos.x - promptW / 2f, Screen.height - screenPos.y - 14, promptW, promptH);

                    // Không vẽ đè lên Menu trượt / Modal / Cài đặt khi đang mở
                    if (CombatHUD.IsAnyModalBlockingScreen(promptRect.x + promptRect.width / 2f, promptRect.y + promptRect.height / 2f)) return;

                    GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.85f);
                    GUI.DrawTexture(promptRect, Texture2D.whiteTexture);
                    GUI.color = new Color(1f, 0.88f, 0.35f);

                    var promptStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 11,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(1f, 0.88f, 0.35f) }
                    };
                    GUI.Label(promptRect, "[ BẤM E ĐỂ NÓI CHUYỆN 💬 ]", promptStyle);
                    GUI.color = Color.white;
                }
            }

            if (showDialogue)
            {
                // Lớp 3 (Depth 0): Hộp thoại NPC hiển thị ngang hàng các Modal
                GUI.depth = FantasyUITheme.DepthModal;
                float boxW = 380f;
                float boxH = 110f;
                float boxX = (Screen.width - boxW) / 2f;
                float boxY = Screen.height - boxH - 90f;

                Rect r = new Rect(boxX, boxY, boxW, boxH);
                GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.96f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = new Color(0.85f, 0.70f, 0.28f);
                // Viền vàng
                GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, r.width, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y, 2, r.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y, 2, r.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 0.85f, 0.35f) }
                };
                var bodyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    wordWrap = true,
                    normal = { textColor = Color.white }
                };

                GUI.Label(new Rect(r.x + 12, r.y + 8, r.width - 40, 20), $"📜 {npcName} (Trưởng Làng Tân Thủ)", titleStyle);
                if (GUI.Button(new Rect(r.x + r.width - 28, r.y + 6, 22, 20), "X"))
                {
                    showDialogue = false;
                }

                GUI.Label(new Rect(r.x + 12, r.y + 30, r.width - 24, 70), dialogue, bodyStyle);
            }
        }
    }
}
