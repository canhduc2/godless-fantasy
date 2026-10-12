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
        [SerializeField] private bool opensShopModal = false;
        [SerializeField] private VillageNPCType shopType = VillageNPCType.Merchant;

        public static bool IsPlayerNearAnyNPC { get; private set; } = false;
        public static bool IsAnyNPCDialogueOpen { get; private set; } = false;

        private bool isPlayerNearby = false;
        private bool showDialogue = false;
        private Transform playerTransform;

        public void Setup(string name, string dial, bool isShop = false, VillageNPCType role = VillageNPCType.Merchant)
        {
            npcName = name;
            dialogue = dial;
            opensShopModal = isShop;
            shopType = role;
        }

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

            // BẤM PHÍM E HOẶC ESC ĐỂ ĐÓNG/MỞ HỘP THOẠI HOẶC SHOP
            if (isPlayerNearby && Core.GameInput.GetInteractKeyDown())
            {
                if (opensShopModal)
                {
                    VillageShopNPCModal.OpenNPC(shopType);
                }
                else
                {
                    showDialogue = !showDialogue;
                }
            }
            if (showDialogue && Input.GetKeyDown(KeyCode.Escape))
            {
                showDialogue = false;
            }
        }

        private void OnMouseDown()
        {
            if (isPlayerNearby)
            {
                if (opensShopModal)
                {
                    VillageShopNPCModal.OpenNPC(shopType);
                }
                else
                {
                    showDialogue = !showDialogue;
                }
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
                    string promptText = "[ BẤM E ĐỂ NÓI CHUYỆN 💬 ]";
                    var promptStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 11,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = false,
                        clipping = TextClipping.Overflow,
                        normal = { textColor = new Color(1f, 0.88f, 0.35f) }
                    };
                    Vector2 promptSize = promptStyle.CalcSize(new GUIContent(promptText));
                    float promptW = Mathf.Max(195f, promptSize.x + 24f);
                    float promptH = 22f;
                    Rect promptRect = new Rect(screenPos.x - promptW / 2f, Screen.height - screenPos.y - 14, promptW, promptH);

                    // Không vẽ đè lên Menu trượt / Modal / Cài đặt khi đang mở
                    if (CombatHUD.IsAnyModalBlockingScreen(promptRect.x + promptRect.width / 2f, promptRect.y + promptRect.height / 2f)) return;

                    GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.90f);
                    GUI.DrawTexture(promptRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    FantasyUITheme.DrawDoubleGoldFrame(promptRect, 1);

                    GUI.Label(promptRect, promptText, promptStyle);
                }
            }

            if (showDialogue)
            {
                // Lớp 3 (Depth 0): Hộp thoại NPC hiển thị ngay CHÍNH GIỮA MÀN HÌNH (tránh hoàn toàn việc chồng chéo lên thanh chiêu thức)
                GUI.depth = FantasyUITheme.DepthModal;
                float boxW = 460f;
                float boxH = 145f;
                float boxX = (Screen.width - boxW) / 2f;
                float boxY = (Screen.height - boxH) / 2f; // Chính giữa tâm màn hình tuyệt đối

                Rect r = new Rect(boxX, boxY, boxW, boxH);
                GUI.color = new Color(0.04f, 0.06f, 0.11f, 0.98f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = Color.white;
                FantasyUITheme.DrawDoubleGoldFrame(r, 2);

                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 0.88f, 0.35f) }
                };
                var bodyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    wordWrap = true,
                    normal = { textColor = Color.white }
                };

                GUI.Label(new Rect(r.x + 14, r.y + 10, r.width - 50, 22), $"📜 {npcName} (Trưởng Làng Tân Thủ)", titleStyle);
                if (GUI.Button(new Rect(r.x + r.width - 32, r.y + 8, 24, 22), "✕"))
                {
                    showDialogue = false;
                }

                GUI.Label(new Rect(r.x + 14, r.y + 36, r.width - 28, 70), dialogue, bodyStyle);

                // Nút đóng tiện lợi góc dưới
                if (GUI.Button(new Rect(r.x + r.width - 110, r.y + r.height - 30, 96, 22), "Đóng [ESC]"))
                {
                    showDialogue = false;
                }
            }
        }
    }
}
