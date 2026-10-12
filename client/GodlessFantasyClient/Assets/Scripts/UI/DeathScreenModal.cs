using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Combat;
using GodlessFantasy.Player;
using GodlessFantasy.Core;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Màn Hình Tử Trận & Cơ Chế Hồi Sinh (Death Screen & PvP Loot Loss Modal):
    /// - Khi người chơi hết máu:
    ///   + Màn hình tối sầm đen kịt (Blackout Vignette), hiệu ứng máu u tối.
    ///   + Kích hoạt cơ chế rủi ro rơi đồ 50% ra sàn (loại trừ 5 ô bảo vệ vĩnh viễn).
    ///   + Khóa di chuyển và tương tác chiến đấu.
    ///   + Nút bấm [HỒI SINH VỀ LÀNG AN TOÀN] (hoặc phím SPACE):
    ///     * Dịch chuyển về Làng Tân Thủ an toàn.
    ///     * Hồi phục 100% Máu và 100% Mana.
    ///     * Mở lại tầm nhìn thế giới.
    /// </summary>
    public class DeathScreenModal : MonoBehaviour
    {
        public static DeathScreenModal Instance { get; private set; }

        public bool IsDead { get; private set; } = false;

        private List<string> droppedItemNames = new List<string>();
        private float deathTime = 0f;

        private GUIStyle titleStyle;
        private GUIStyle warningStyle;
        private GUIStyle itemStyle;
        private GUIStyle btnStyle;
        private bool isStyleInit = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void InitStyles()
        {
            if (isStyleInit) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.2f, 0.2f, 1f) }
            };

            warningStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.85f, 0.35f, 0.95f) }
            };

            itemStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = new Color(0.9f, 0.65f, 0.35f, 1f) }
            };

            btnStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            isStyleInit = true;
        }

        public static void TriggerPlayerDeath(List<string> droppedItems)
        {
            if (Instance == null)
            {
                var go = new GameObject("[DEATH_SCREEN_MODAL]");
                go.AddComponent<DeathScreenModal>();
            }
            Instance.ShowDeathScreen(droppedItems);
        }

        public void ShowDeathScreen(List<string> droppedItems)
        {
            IsDead = true;
            deathTime = Time.time;
            droppedItemNames = droppedItems ?? new List<string>();
            PlayerMovement.BlockWorldClick = true;

            AudioManager.PlayExplosion();
            Debug.Log("<color=red>[TỬ TRẬN]</color> Người chơi đã hy sinh trên chiến trường!");
        }

        private void Update()
        {
            if (IsDead)
            {
                PlayerMovement.BlockWorldClick = true;

                // Phím SPACE để hồi sinh nhanh
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    RespawnAtSavedPoint();
                }
            }
        }

        private void OnGUI()
        {
            if (!IsDead) return;

            InitStyles();
            GUI.depth = -300; // Nằm trên tất cả các cửa sổ khác

            // 1. MÀN HÌNH ĐEN NGÒM TỐI TĂM (BLACKOUT VIGNETTE)
            float alpha = Mathf.Clamp01((Time.time - deathTime) * 1.8f);
            GUI.color = new Color(0.02f, 0.02f, 0.04f, 0.94f * alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            // Viền máu đỏ mờ bốn góc màn hình
            GUI.color = new Color(0.8f, 0.05f, 0.05f, 0.22f * (0.8f + Mathf.Sin(Time.time * 3f) * 0.2f));
            GUI.DrawTexture(new Rect(0, 0, Screen.width, 18), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height - 18, Screen.width, 18), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, 0, 18, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - 18, 0, 18, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. KHUNG HỘP THÔNG BÁO TỬ TRẬN
            float boxW = Mathf.Min(520f, Screen.width - 40f);
            float boxH = 340f;
            float boxX = (Screen.width - boxW) / 2f;
            float boxY = (Screen.height - boxH) / 2f;
            Rect boxRect = new Rect(boxX, boxY, boxW, boxH);

            // Nền hộp thông báo
            GUI.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(boxRect, new Color(0.85f, 0.25f, 0.25f, 0.95f), new Color(0.45f, 0.15f, 0.15f, 0.95f));

            // Tiêu đề
            GUI.Label(new Rect(boxX, boxY + 22, boxW, 36), "☠️ BẠN ĐÃ TỬ TRẬN TRÊN CHIẾN TRƯỜNG! ☠️", titleStyle);

            // Cảnh báo mất mát theo đúng luật MMORPG Hardcore
            string warnText = "<color=#ff5252>⚠️ RỦI RO CHIẾN TRANH:</color> 50% Vật phẩm không nằm trong 5 Ô Bảo Vệ Vĩnh Viễn đã rơi tự do ra sàn đất cho người khác nhặt!";
            GUI.Label(new Rect(boxX + 24, boxY + 70, boxW - 48, 44), warnText, warningStyle);

            // Thông báo 5 ô bảo vệ vĩnh viễn
            string protectText = "<color=#69f0ae>🛡️ 5 Ô BẢO VỆ VĨNH VIỄN:</color> Đồ trong 5 ô này được niêm phong an toàn tuyệt đối 100%!";
            GUI.Label(new Rect(boxX + 24, boxY + 118, boxW - 48, 26), protectText, warningStyle);

            // Danh sách đồ rơi
            Rect dropListBox = new Rect(boxX + 30, boxY + 152, boxW - 60, 95);
            GUI.color = new Color(0.03f, 0.04f, 0.07f, 0.85f);
            GUI.DrawTexture(dropListBox, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (droppedItemNames.Count > 0)
            {
                string droppedSummary = $"Đã rơi {droppedItemNames.Count} món:\n" + string.Join(", ", droppedItemNames);
                GUI.Label(new Rect(dropListBox.x + 8, dropListBox.y + 8, dropListBox.width - 16, dropListBox.height - 16), droppedSummary, itemStyle);
            }
            else
            {
                GUI.Label(new Rect(dropListBox.x + 8, dropListBox.y + 30, dropListBox.width - 16, 30), "May mắn! Toàn bộ đồ đạc của bạn nằm trong vùng bảo vệ, không bị rơi món nào.", itemStyle);
            }

            // Nút Hồi sinh
            string locName = GodlessFantasy.World.WaystoneShrine.GetRespawnLocationName();
            Rect btnRect = new Rect(boxX + (boxW - 320f) / 2f, boxY + boxH - 65, 320f, 44);
            GUI.color = new Color(0.2f, 0.75f, 0.35f, 1f);
            if (GUI.Button(btnRect, $"✨ HỒI SINH: {locName.ToUpper()} [SPACE]", btnStyle))
            {
                RespawnAtSavedPoint();
            }
            GUI.color = Color.white;
        }

        public void RespawnAtSavedPoint()
        {
            var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
            if (player != null)
            {
                // Dịch chuyển về Trụ Hồi Sinh gần nhất đã lưu
                Vector3 respawnPos = GodlessFantasy.World.WaystoneShrine.GetRespawnPosition();
                string locName = GodlessFantasy.World.WaystoneShrine.GetRespawnLocationName();
                player.transform.position = respawnPos;

                // Hồi phục đầy 100% Máu & Mana
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.ResetHealth();
                }

                var stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.CurrentMana = stats.MaxMana;
                }

                FloatingDamageManager.ShowFloatingText(player.transform.position + Vector3.up * 1.2f, $"✨ Hồi Sinh Thành Công Tại {locName}!", Color.green);
            }

            IsDead = false;
            PlayerMovement.BlockWorldClick = false;
            AudioManager.PlayTeleport();
            Debug.Log("<color=green>[HỒI SINH]</color> Đã hồi sinh về Trụ Hồi Sinh đã lưu an toàn!");
        }

        private void DrawDoubleGoldFrame(Rect r, Color outerCol, Color innerCol)
        {
            GUI.color = outerCol;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, 2, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y, 2, r.height), Texture2D.whiteTexture);

            GUI.color = innerCol;
            GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + 3, r.y + r.height - 4, r.width - 6, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, 1, r.height - 6), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 4, r.y + 3, 1, r.height - 6), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
