using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Giao Diện HUD Chuẩn Thiết Kế Wireframe Của Studio:
    /// - Góc Trái Trên: Thông Tin Nhân Vật (Avatar, HP, Mana, Tên).
    /// - Giữa Màn Hình Trên: Dải Tin Tức Toàn Server (Server Announcement Banner).
    /// - Góc Phải Trên: Minimap Vuông Vắn + Nút Chat Tròn Nhỏ Nằm Ngay Bên Cạnh.
    /// - Giữa Mép Trái: Nút Menu Trượt Hình Tam Giác ▷.
    /// - Giữa Dưới Cùng: 4 Ô Kỹ Năng Vuông Vắn (Bottom-Center Skill Bar).
    /// </summary>
    public class CombatHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private SkillAimingController skillAimingController;
        [SerializeField] private Health playerHealth;

        private Texture2D panelBgTex;
        private Texture2D hpFillTex;
        private Texture2D manaFillTex;
        private Texture2D radarBgTex;

        private GUIStyle boldWhiteStyle;
        private GUIStyle smallWhiteStyle;
        private GUIStyle serverNewsStyle;
        private GUIStyle skillBoxStyle;
        private GUIStyle triangleStyle;
        private bool isStyleInitialized = false;

        private float serverNewsScrollX = 0f;
        private string serverNewsText = "📢 [THÔNG BÁO TOÀN SERVER] Chào mừng anh em SquiRRels Studio đến với Alpha Test Godless Fantasy! Port 5454 · Server Authoritative · Friendly Fire 100% · Lệnh giao dịch: /trade <Tên>";

        private void Start()
        {
            if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (skillAimingController == null) skillAimingController = FindFirstObjectByType<SkillAimingController>();
            if (playerHealth == null && playerMovement != null) playerHealth = playerMovement.GetComponent<Health>();

            CreateTextures();
        }

        private void CreateTextures()
        {
            panelBgTex = MakeColorTex(new Color(0.07f, 0.09f, 0.15f, 0.90f));
            hpFillTex = MakeColorTex(new Color(0.85f, 0.20f, 0.22f, 1f));
            manaFillTex = MakeColorTex(new Color(0.18f, 0.55f, 0.95f, 1f));
            radarBgTex = MakeColorTex(new Color(0.04f, 0.06f, 0.10f, 0.95f));
        }

        private Texture2D MakeColorTex(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void InitStyles()
        {
            if (isStyleInitialized && boldWhiteStyle != null && skillBoxStyle != null) return;

            boldWhiteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            smallWhiteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };

            serverNewsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.90f, 0.45f) }
            };

            skillBoxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            triangleStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.8f, 0.4f) }
            };

            isStyleInitialized = true;
        }

        private void OnGUI()
        {
            if (panelBgTex == null || hpFillTex == null || manaFillTex == null || radarBgTex == null)
            {
                CreateTextures();
            }

            InitStyles();

            DrawTopLeftBox();
            DrawTopCenterServerNews();
            DrawTopRightMinimapAndChat();
            DrawMidLeftTriangleMenu();
            DrawBottomCenterSkillBar();
        }

        // ================= 1. GÓC TRÁI TRÊN: THÔNG TIN NHÂN VẬT =================
        private void DrawTopLeftBox()
        {
            float w = 210f;
            float h = 54f;
            Rect panel = new Rect(16, 14, w, h);

            GUI.DrawTexture(panel, panelBgTex);
            DrawOutline(panel, new Color(0.75f, 0.62f, 0.28f, 0.8f), 1);

            // Avatar Icon nhỏ 36x36
            Rect av = new Rect(panel.x + 8, panel.y + 9, 36, 36);
            GUI.color = new Color(0.12f, 0.16f, 0.25f);
            GUI.DrawTexture(av, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(av, new Color(0.85f, 0.7f, 0.3f), 1);
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(av, "⚔️", boldWhiteStyle);
            boldWhiteStyle.alignment = TextAnchor.UpperLeft;

            // Thanh Máu HP
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
            DrawBar(panel.x + 50, panel.y + 9, 150, 15, hp, maxHp, hpFillTex, $"HP {hp}/{maxHp}");

            // Thanh Mana
            DrawBar(panel.x + 50, panel.y + 29, 150, 15, 80, 100, manaFillTex, "MANA 80/100");
        }

        private void DrawBar(float x, float y, float w, float h, int cur, int max, Texture2D fillTex, string text)
        {
            Rect bg = new Rect(x, y, w, h);
            GUI.color = new Color(0.06f, 0.08f, 0.12f, 0.9f);
            GUI.DrawTexture(bg, Texture2D.whiteTexture);
            GUI.color = Color.white;

            float fill = Mathf.Clamp01((float)cur / max);
            Rect fRect = new Rect(x, y, w * fill, h);
            GUI.DrawTexture(fRect, fillTex);
            DrawOutline(bg, new Color(0.35f, 0.45f, 0.6f, 0.4f), 1);

            GUI.Label(bg, text, smallWhiteStyle);
        }

        // ================= 2. GIỮA TRÊN CÙNG: DẢI TIN TỨC TOÀN SERVER =================
        private void DrawTopCenterServerNews()
        {
            float bannerW = Mathf.Clamp(Screen.width - 480f, 320f, 540f);
            float bannerH = 26f;
            float bannerX = (Screen.width - bannerW) / 2f;
            float bannerY = 14f;

            Rect bannerRect = new Rect(bannerX, bannerY, bannerW, bannerH);
            GUI.DrawTexture(bannerRect, panelBgTex);
            DrawOutline(bannerRect, new Color(0.85f, 0.7f, 0.3f, 0.85f), 1);

            // Nếu đang ở chế độ Aiming -> đổi nội dung thành nhắc ngắm chiêu cực gọn!
            if (skillAimingController != null && skillAimingController.IsAiming)
            {
                serverNewsStyle.normal.textColor = Color.cyan;
                GUI.Label(bannerRect, "🎯 [ĐANG NGẮM SKILL 1]: Click Trái né đòn · Bấm 1 để xả đạn · Chuột Phải để hủy", serverNewsStyle);
            }
            else
            {
                serverNewsStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
                GUI.Label(bannerRect, "tin tức toàn server: SquiRRels Studio · Godless Fantasy Alpha 0.1", serverNewsStyle);
            }
        }

        // ================= 3. GÓC PHẢI TRÊN: MINIMAP + NÚT CHAT TRÒN =================
        private void DrawTopRightMinimapAndChat()
        {
            float mapSize = 96f;
            float mapX = Screen.width - mapSize - 16;
            float mapY = 14;
            Rect mapRect = new Rect(mapX, mapY, mapSize, mapSize);

            // Nền Minimap
            GUI.DrawTexture(mapRect, radarBgTex);
            DrawOutline(mapRect, new Color(0.75f, 0.65f, 0.35f, 0.85f), 1);

            // Tọa độ radar
            float cx = mapX + mapSize / 2f;
            float cy = mapY + mapSize / 2f;
            GUI.color = new Color(0.2f, 0.3f, 0.45f, 0.4f);
            GUI.DrawTexture(new Rect(mapX, cy, mapSize, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx, mapY, 1, mapSize), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Chấm Player (Vàng kim)
            GUI.color = Color.yellow;
            GUI.DrawTexture(new Rect(cx - 3, cy - 3, 6, 6), Texture2D.whiteTexture);

            // Chấm Dummy Quái (Đỏ)
            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(cx + 26, cy, 5, 5), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Nhãn tên khu vực
            smallWhiteStyle.normal.textColor = new Color(0.75f, 0.85f, 0.95f, 0.9f);
            GUI.Label(new Rect(mapX, mapY + mapSize - 16, mapSize, 16), "Tân Thủ Thôn", smallWhiteStyle);

            // NÚT TRÒ CHUYỆN HÌNH TRÒN NHỎ NẰM NGAY BÊN TRÁI MINIMAP (Y HỆT BẢN VẼ!)
            float chatSize = 28f;
            float chatX = mapX - chatSize - 10f;
            float chatY = mapY + 6f;
            Rect chatRect = new Rect(chatX, chatY, chatSize, chatSize);

            GUI.DrawTexture(chatRect, panelBgTex);
            DrawOutline(chatRect, new Color(0.85f, 0.72f, 0.35f, 0.9f), 1);
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(chatRect, "💬", boldWhiteStyle);
            boldWhiteStyle.alignment = TextAnchor.UpperLeft;
        }

        // ================= 4. CHÍNH GIỮA MÉP TRÁI: NÚT TAM GIÁC ▷ MENU TRƯỢT =================
        private void DrawMidLeftTriangleMenu()
        {
            float btnW = 20f;
            float btnH = 34f;
            float btnX = 4f;
            float btnY = (Screen.height - btnH) / 2f;
            Rect triRect = new Rect(btnX, btnY, btnW, btnH);

            GUI.DrawTexture(triRect, panelBgTex);
            DrawOutline(triRect, new Color(0.85f, 0.7f, 0.3f, 0.85f), 1);
            GUI.Label(triRect, "▷", triangleStyle);
        }

        // ================= 5. CHÍNH GIỮA DƯỚI CÙNG: 4 Ô KỸ NĂNG VUÔNG VẮN =================
        private void DrawBottomCenterSkillBar()
        {
            float slotSize = 48f;
            float gap = 8f;
            float totalW = (slotSize * 4) + (gap * 3);
            float startX = (Screen.width - totalW) / 2f;
            float startY = Screen.height - slotSize - 16f;

            // Ô 1
            bool isAiming = skillAimingController != null && skillAimingController.IsAiming;
            float s1Cd = skillAimingController != null ? skillAimingController.Skill1CooldownRemaining : 0f;
            bool s1Ready = s1Cd <= 0.05f;

            Rect s1Rect = new Rect(startX, startY, slotSize, slotSize);
            GUI.DrawTexture(s1Rect, panelBgTex);

            Color s1Border = isAiming ? (Mathf.PingPong(Time.time * 5f, 1f) > 0.5f ? Color.cyan : Color.white) : (s1Ready ? new Color(0.85f, 0.7f, 0.3f) : Color.gray);
            DrawOutline(s1Rect, s1Border, isAiming ? 2 : 1);

            string s1Text = isAiming ? "NGẮM\n[1]" : (s1Ready ? "CHIÊU 1\n[1]" : $"{s1Cd:F1}s");
            GUI.Label(s1Rect, s1Text, skillBoxStyle);

            // Ô 2, 3, 4
            for (int i = 2; i <= 4; i++)
            {
                Rect slotRect = new Rect(startX + (i - 1) * (slotSize + gap), startY, slotSize, slotSize);
                GUI.DrawTexture(slotRect, panelBgTex);
                DrawOutline(slotRect, new Color(0.35f, 0.4f, 0.5f, 0.5f), 1);
                int reqLv = i == 2 ? 25 : (i == 3 ? 50 : 75);
                GUI.Label(slotRect, $"🔒\nLv.{reqLv}", skillBoxStyle);
            }
        }

        private void DrawOutline(Rect r, Color c, int width)
        {
            GUI.color = c;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - width, r.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, width, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - width, r.y, width, r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
