using UnityEngine;
using GodlessFantasy.Combat;
using GodlessFantasy.Player;

namespace GodlessFantasy.UI
{
    public enum OverheadType
    {
        Player,
        NPC,
        Monster
    }

    /// <summary>
    /// Thanh Máu & Danh Xưng Trên Đầu Đơn Vị (Overhead Entity Status Bar):
    /// - Áp dụng đồng bộ cho Người Chơi (Player), NPC Dân Làng, và Quái Vật (Monster).
    /// - Hiển thị: Tên, Cấp Độ, Hệ, Thanh Máu HP và Thanh Mana (dành cho Player).
    /// - Có hiệu ứng vệt trắng tụt máu trễ (Damage Lag Catch-up) cực kỳ chuẩn MMO/ARPG!
    /// </summary>
    public class OverheadStatusBar : MonoBehaviour
    {
        [Header("Entity Configuration")]
        [SerializeField] private OverheadType entityType = OverheadType.Monster;
        [SerializeField] private string displayName = "Quái Vật";
        [SerializeField] private string titleOrLevel = "Lv.1";
        [SerializeField] private float heightOffset = 0.85f;

        [Header("References")]
        [SerializeField] private Health health;
        [SerializeField] private PlayerStats playerStats;

        private Camera mainCam;
        private static Texture2D whiteTex;
        private static GUIStyle nameStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle hpNumberStyle;

        private float displayedHpPercent = 1.0f;
        private float lagCatchUpSpeed = 1.5f;

        public void Setup(OverheadType type, string name, string levelStr, float offset = 0.85f)
        {
            entityType = type;
            displayName = name;
            titleOrLevel = levelStr;
            heightOffset = offset;
        }

        private void Awake()
        {
            mainCam = Camera.main;
            if (health == null) health = GetComponent<Health>();
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();

            if (whiteTex == null)
            {
                whiteTex = new Texture2D(1, 1);
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
            }
        }

        private void Update()
        {
            if (health == null) return;

            float targetHpPercent = Mathf.Clamp01((float)health.CurrentHealth / health.MaxHealth);
            if (displayedHpPercent > targetHpPercent)
            {
                // Vệt trắng từ từ tụt xuống bắt kịp máu thật
                displayedHpPercent = Mathf.MoveTowards(displayedHpPercent, targetHpPercent, lagCatchUpSpeed * Time.deltaTime);
            }
            else
            {
                displayedHpPercent = targetHpPercent;
            }
        }

        private void OnGUI()
        {
            // LỚP 1 (Depth 20): Luôn nằm dưới toàn bộ HUD và Menu
            GUI.depth = FantasyUITheme.DepthOverhead;

            if (health != null && health.IsDead) return;
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            Vector3 worldPos = transform.position + new Vector3(0, heightOffset, 0);
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

            // Bỏ qua nếu nằm sau lưng camera
            if (screenPos.z < 0) return;

            InitStyles();

            float guiX = Mathf.Round(screenPos.x);
            float guiY = Mathf.Round(Screen.height - screenPos.y);

            // Tuyệt đối không vẽ đè lên Menu trượt / Modal / Cài đặt khi đang mở
            if (CombatHUD.IsAnyModalBlockingScreen(guiX, guiY)) return;

            // Kích thước thanh trên đầu
            float barW = (entityType == OverheadType.Player) ? 76f : 60f;
            float barH = (entityType == OverheadType.Player) ? 6f : 5f;
            float startX = guiX - barW / 2f;

            // 0. DÒNG DANH HIỆU TRÊN CÙNG (TITLE LABEL - CHO NGƯỜI CHƠI)
            if (entityType == OverheadType.Player && playerStats != null && playerStats.EquippedTitle != null)
            {
                var curTitle = playerStats.EquippedTitle;
                string cleanTitle = curTitle.name.Trim('[', ']');
                string titleText = $"« {cleanTitle} »";
                float titleW = 240f;
                float titleH = 16f;
                Rect titleRect = new Rect(guiX - titleW / 2f, guiY - 40f, titleW, titleH);

                titleStyle.normal.textColor = new Color(0f, 0f, 0f, 0.95f);
                GUI.Label(new Rect(titleRect.x + 1, titleRect.y + 1, titleRect.width, titleRect.height), titleText, titleStyle);

                titleStyle.normal.textColor = curTitle.titleColor;
                GUI.Label(titleRect, titleText, titleStyle);
            }

            // 1. DÒNG TÊN & CẤP ĐỘ / HỆ / NGUYÊN TỐ (Bản rộng 240px thoáng đãng)
            string nameString;
            if (entityType == OverheadType.Player && playerStats != null)
            {
                string pClass = playerStats.CharacterClass;
                string pElem = playerStats.Element;
                int pLv = playerStats.Level;
                nameString = $"{displayName} [Lv.{pLv} · {pClass} · {pElem}]";
            }
            else
            {
                nameString = $"{displayName} [{titleOrLevel}]";
            }
            Color nameColor = GetNameColor();

            float plateW = 240f;
            float plateH = 20f;
            Rect namePlateRect = new Rect(guiX - plateW / 2f, guiY - 24f, plateW, plateH);

            // Đổ bóng đen 1px giúp nét chữ nổi khối sắc sảo trên mọi bối cảnh sàn gạch
            nameStyle.normal.textColor = new Color(0f, 0f, 0f, 0.95f);
            GUI.Label(new Rect(namePlateRect.x + 1, namePlateRect.y + 1, namePlateRect.width, namePlateRect.height), nameString, nameStyle);

            nameStyle.normal.textColor = nameColor;
            GUI.Label(namePlateRect, nameString, nameStyle);

            // 2. KHUNG NỀN THANH MÁU
            Rect hpBgRect = new Rect(startX, guiY, barW, barH);
            GUI.color = new Color(0.08f, 0.10f, 0.14f, 0.95f);
            GUI.DrawTexture(hpBgRect, Texture2D.whiteTexture);

            // 3. VỆT TRẮNG TỤT MÁU TRỄ (Lag Drop)
            if (displayedHpPercent > 0.001f)
            {
                Rect lagRect = new Rect(startX, guiY, barW * displayedHpPercent, barH);
                GUI.color = new Color(1f, 0.9f, 0.7f, 0.8f);
                GUI.DrawTexture(lagRect, Texture2D.whiteTexture);
            }

            // 4. MÁU HIỆN TẠI (HP Fill)
            float realHpPercent = (health != null) ? Mathf.Clamp01((float)health.CurrentHealth / health.MaxHealth) : 1f;
            if (realHpPercent > 0.001f)
            {
                Rect hpFillRect = new Rect(startX, guiY, barW * realHpPercent, barH);
                GUI.color = GetHpColor();
                GUI.DrawTexture(hpFillRect, Texture2D.whiteTexture);
            }

            // 5. VIỀN VÀNG / ĐEN CHO THANH MÁU
            GUI.color = new Color(0.75f, 0.65f, 0.35f, 0.85f);
            DrawOutline(hpBgRect, 1);
            GUI.color = Color.white;

            // 6. THANH MANA (CHỈ HIỂN THỊ CHO NGƯỜI CHƠI)
            if (entityType == OverheadType.Player && playerStats != null)
            {
                float manaH = 3f;
                float manaY = guiY + barH + 2f;
                Rect manaBgRect = new Rect(startX, manaY, barW, manaH);

                // Nền mana
                GUI.color = new Color(0.05f, 0.08f, 0.15f, 0.9f);
                GUI.DrawTexture(manaBgRect, Texture2D.whiteTexture);

                // Mana fill
                float manaPercent = Mathf.Clamp01((float)playerStats.CurrentMana / playerStats.MaxMana);
                if (manaPercent > 0.001f)
                {
                    GUI.color = new Color(0.20f, 0.65f, 1f, 1f); // Xanh lam rực rỡ
                    GUI.DrawTexture(new Rect(startX, manaY, barW * manaPercent, manaH), Texture2D.whiteTexture);
                }

                // Viền mana
                GUI.color = new Color(0.3f, 0.5f, 0.7f, 0.6f);
                DrawOutline(manaBgRect, 1);
                GUI.color = Color.white;
            }
        }

        private Color GetNameColor()
        {
            switch (entityType)
            {
                case OverheadType.Player: return new Color(0.6f, 1f, 0.65f); // Xanh lục đồng minh
                case OverheadType.NPC: return new Color(1f, 0.88f, 0.35f);    // Vàng kim NPC
                case OverheadType.Monster: return new Color(1f, 0.45f, 0.45f);// Đỏ quái vật
                default: return Color.white;
            }
        }

        private Color GetHpColor()
        {
            switch (entityType)
            {
                case OverheadType.Player: return new Color(0.22f, 0.85f, 0.35f); // Xanh lá Máu Player
                case OverheadType.NPC: return new Color(0.85f, 0.75f, 0.25f);    // Vàng kim NPC
                case OverheadType.Monster: return new Color(0.92f, 0.22f, 0.24f);// Đỏ rực Quái
                default: return Color.red;
            }
        }

        private void DrawOutline(Rect r, int width)
        {
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - width, r.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, width, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - width, r.y, width, r.height), Texture2D.whiteTexture);
        }

        private static void InitStyles()
        {
            if (nameStyle == null)
            {
                nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false,
                    clipping = TextClipping.Overflow
                };
            }

            if (hpNumberStyle == null)
            {
                hpNumberStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }
        }
    }
}
