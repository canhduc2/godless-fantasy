using UnityEngine;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// BỘ TEMPLATE GIAO DIỆN FANTASY TÁI SỬ DỤNG (REUSABLE FANTASY UI THEME DESIGN SYSTEM):
    /// Chuẩn hóa phong cách Retro Fantasy UI độc quyền SquiRRels Studio:
    /// - Quản lý phân lớp giao diện (UI Depth Layers): Overhead (20) -> Base HUD (10) -> Menu/Modal (0) -> Popup/Tooltip (-100).
    /// - Nút vuông icon viền vàng kim chốt góc 4 phía theo đúng mẫu người dùng yêu cầu (Square Golden Corner-Notched Button).
    /// - Nút chữ nhật Fantasy chuẩn game MMO (Rectangular Button).
    /// - Thanh chữ chạy tin tức từ phải sang trái (Marquee Ticker Banner).
    /// - Khung viền đôi mạ vàng nổi khối (Double Gold Embossed Frame).
    /// - Hiệu ứng hào quang chọn (Selection Glow Pulse).
    /// </summary>
    public static class FantasyUITheme
    {
        // ================= HỆ THỐNG THỨ TỰ PHÂN LỚP GIAO DIỆN (UI DEPTH HIERARCHY) =================
        /// <summary>
        /// Lớp 1 (Sâu nhất trong UI): Thanh máu, tên hiển thị trên đầu nhân vật/quái/NPC trong thế giới.
        /// </summary>
        public const int DepthOverhead = 20;

        /// <summary>
        /// Lớp 2: HUD cơ bản thường trực (Avatar người chơi, Minimap, Hotbar 1-4, Thanh Ticker dưới đáy).
        /// </summary>
        public const int DepthBaseHUD = 10;

        /// <summary>
        /// Lớp 3: Cửa sổ tính năng, Menu cài đặt, Drawer trượt 5 tab, Cửa sổ Chat, Hội thoại NPC.
        /// </summary>
        public const int DepthModal = 0;
        public const int DepthModalBase = 0;

        /// <summary>
        /// Lớp 4 (Trên cùng tuyệt đối): Hộp thoại cảnh báo bảo vệ xóa đồ (5s Double-check), Tooltip hover chi tiết.
        /// </summary>
        public const int DepthPopupTooltip = -100;

        public static void DrawGradientGoldTitle(Rect r, string title, int fontSize = 16)
        {
            EnsureStyles();
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            };

            // Đổ bóng đen
            titleStyle.normal.textColor = new Color(0f, 0f, 0f, 0.95f);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), title, titleStyle);

            // Chữ hoàng kim
            titleStyle.normal.textColor = ColorTextGold;
            GUI.Label(r, title, titleStyle);
        }

        // ================= BẢNG MÀU CHUẨN ĐỒNG BỘ (DESIGN SYSTEM COLOR TOKENS) =================
        public static readonly Color ColorBgDark = new Color(0.06f, 0.08f, 0.14f, 0.98f);
        public static readonly Color ColorBgNavyButton = new Color(0.09f, 0.13f, 0.22f, 0.98f);
        public static readonly Color ColorBgHighlighted = new Color(0.16f, 0.23f, 0.36f, 1.0f);
        public static readonly Color ColorBgHovered = new Color(0.14f, 0.19f, 0.30f, 1.0f);

        public static readonly Color ColorBorderGold = new Color(0.88f, 0.74f, 0.32f, 1.0f);
        public static readonly Color ColorBorderHighlight = new Color(1.0f, 0.94f, 0.55f, 1.0f);
        public static readonly Color ColorBorderInner = new Color(0.38f, 0.30f, 0.14f, 0.65f);

        public static readonly Color ColorCornerStud = new Color(1.0f, 0.95f, 0.62f, 1.0f);
        public static readonly Color ColorTextGold = new Color(1.0f, 0.90f, 0.45f, 1.0f);
        public static readonly Color ColorTextCyan = new Color(0.20f, 0.92f, 1.0f, 1.0f);
        public static readonly Color ColorTextRed = new Color(1.0f, 0.45f, 0.45f, 1.0f);

        private static Font gameFont;
        public static Font GetGameFont()
        {
            if (gameFont == null)
            {
                // Nạp phông chữ hệ thống sắc nét (Segoe UI, Tahoma, Arial) hỗ trợ trọn vẹn tiếng Việt có dấu
                gameFont = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI Semibold", "Segoe UI", "Tahoma", "Verdana", "Arial" }, 12);
            }
            return gameFont;
        }

        private static GUIStyle squareIconStyle;
        private static GUIStyle rectBtnStyle;
        private static GUIStyle tickerBadgeStyle;
        private static GUIStyle tickerTextStyle;

        private static void EnsureStyles()
        {
            if (squareIconStyle == null)
            {
                squareIconStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    contentOffset = Vector2.zero,
                    wordWrap = false,
                    clipping = TextClipping.Overflow
                };
            }
            if (rectBtnStyle == null)
            {
                rectBtnStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    contentOffset = Vector2.zero,
                    wordWrap = false,
                    clipping = TextClipping.Overflow
                };
            }
            if (tickerBadgeStyle == null)
            {
                tickerBadgeStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 10,
                    wordWrap = false
                };
            }
            if (tickerTextStyle == null)
            {
                tickerTextStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold,
                    fontSize = 11,
                    wordWrap = false
                };
            }
        }

        private static Texture2D slotFrameTex;

        public static Texture2D GetSlotFrameTexture()
        {
            if (slotFrameTex != null) return slotFrameTex;
            int size = 32;
            slotFrameTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            slotFrameTex.filterMode = FilterMode.Point;
            slotFrameTex.wrapMode = TextureWrapMode.Clamp;

            Color[] px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = (x == 0 || x == size - 1 || y == 0 || y == size - 1 || x == 1 || x == size - 2 || y == 1 || y == size - 2);
                    bool isInnerBevel = (x == 2 || x == size - 3 || y == 2 || y == size - 3);
                    bool isCornerStud = ((x <= 2 || x >= size - 3) && (y <= 2 || y >= size - 3));

                    if (isCornerStud) px[y * size + x] = ColorCornerStud;
                    else if (isBorder) px[y * size + x] = ColorBorderGold;
                    else if (isInnerBevel) px[y * size + x] = ColorBorderInner;
                    else px[y * size + x] = ColorBgNavyButton;
                }
            }
            slotFrameTex.SetPixels(px);
            slotFrameTex.Apply();
            return slotFrameTex;
        }

        // ================= 1. NÚT VUÔNG ICON VIỀN VÀNG CHỐT GÓC (CHUẨN ẢNH MẪU USER) =================
        /// <summary>
        /// Vẽ nút bấm ô vuông viền vàng, 4 chốt góc, nền xanh than ma thuật với icon/ký tự nổi khối (Ví dụ nút [+], [-], [⚙]).
        /// Căn giữa tuyệt đối (Absolute Center) cả ngang lẫn dọc, chống lệch tuyệt đối.
        /// </summary>
        public static bool DrawSquareIconButton(Rect r, string iconText, int fontSize = 14, bool isEnabled = true, bool isHighlighted = false, Color? iconColor = null)
        {
            EnsureStyles();
            bool isHovered = isEnabled && r.Contains(Event.current.mousePosition);
            bool isPressed = isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0;

            // Tối ưu hóa cực đại: Vẽ 1 Texture gộp duy nhất (1 call) thay vì 13 calls!
            GUI.color = isHighlighted ? ColorBorderHighlight : isHovered ? new Color(1.0f, 0.95f, 0.60f, 1f) : Color.white;
            GUI.DrawTexture(r, GetSlotFrameTexture());
            GUI.color = Color.white;

            // 5. Icon/Ký tự ở giữa với đổ bóng đen nổi khối (Căn giữa tuyệt đối chính tâm nút)
            squareIconStyle.alignment = TextAnchor.MiddleCenter;
            squareIconStyle.fontStyle = FontStyle.Bold;
            squareIconStyle.fontSize = fontSize;
            squareIconStyle.padding = new RectOffset(0, 0, 0, 0);
            squareIconStyle.margin = new RectOffset(0, 0, 0, 0);
            squareIconStyle.contentOffset = Vector2.zero;
            float textOffset = isPressed ? 1f : 0f;
            float yNudge = (iconText == "+") ? 1.5f : 0f;

            // Đổ bóng sắc nét
            squareIconStyle.normal.textColor = new Color(0.04f, 0.05f, 0.08f, 0.95f);
            GUI.Label(new Rect(r.x + 1 + textOffset, r.y + 1 + textOffset + yNudge, r.width, r.height), iconText, squareIconStyle);

            // Màu chính
            Color fg = !isEnabled ? Color.gray
                     : iconColor ?? (isHighlighted ? ColorTextGold : isHovered ? Color.white : ColorTextGold);
            squareIconStyle.normal.textColor = fg;
            GUI.Label(new Rect(r.x + textOffset, r.y + textOffset + yNudge, r.width, r.height), iconText, squareIconStyle);

            if (isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Event.current.Use();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Nút [+] chuyên dụng (Cộng điểm tiềm năng / thêm slot) chuẩn ảnh mẫu của user
        /// </summary>
        public static bool DrawSquarePlusButton(Rect r, bool canAdd = true)
        {
            return DrawSquareIconButton(r, "+", 14, canAdd, canAdd, canAdd ? ColorTextGold : Color.gray) && canAdd;
        }

        // ================= 2. NÚT CHỮ NHẬT FANTASY CHUẨN =================
        /// <summary>
        /// Vẽ nút bấm Fantasy hình chữ nhật có viền vàng, 4 chốt góc, hiệu ứng hover/click và chữ đổ bóng chống răng cưa.
        /// </summary>
        public static bool DrawFantasyButton(Rect r, string text, int fontSize = 12, bool isHighlighted = false, Color? textColorOverride = null)
        {
            return DrawButton(r, text, fontSize, isHighlighted, textColorOverride);
        }

        public static bool DrawButton(Rect r, string text, int fontSize = 12, bool isHighlighted = false, Color? textColorOverride = null)
        {
            EnsureStyles();
            bool isHovered = r.Contains(Event.current.mousePosition);

            // Nền
            Color bg = isHighlighted ? ColorBgHighlighted : isHovered ? ColorBgHovered : ColorBgDark;
            GUI.color = bg;
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            // Viền vàng
            Color border = isHighlighted ? ColorBorderHighlight : isHovered ? new Color(0.96f, 0.82f, 0.40f, 1f) : ColorBorderGold;
            DrawOutline(r, border, 1);

            // Viền bevel nội khối
            if (r.width > 8 && r.height > 8)
            {
                Rect inner = new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2);
                DrawOutline(inner, ColorBorderInner, 1);
            }

            // Chốt góc
            if (r.width >= 12 && r.height >= 12)
            {
                GUI.color = ColorCornerStud;
                int cSize = (r.height < 24) ? 2 : 3;
                GUI.DrawTexture(new Rect(r.x, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // Text có đổ bóng đen mảnh (Căn giữa tuyệt đối)
            rectBtnStyle.alignment = TextAnchor.MiddleCenter;
            rectBtnStyle.fontStyle = FontStyle.Bold;
            rectBtnStyle.fontSize = fontSize;
            rectBtnStyle.padding = new RectOffset(0, 0, 0, 0);
            rectBtnStyle.contentOffset = Vector2.zero;

            rectBtnStyle.normal.textColor = new Color(0f, 0f, 0f, 0.90f);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, rectBtnStyle);

            Color txtCol = textColorOverride ?? (isHighlighted ? ColorTextGold : isHovered ? Color.white : ColorTextGold);
            rectBtnStyle.normal.textColor = txtCol;
            GUI.Label(r, text, rectBtnStyle);

            if (isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Event.current.Use();
                return true;
            }
            return false;
        }

        // ================= 3. THANH TIN TỨC CHẠY CHỮ TỪ PHẢI SANG TRÁI (MARQUEE TICKER) =================
        /// <summary>
        /// Vẽ thanh chạy chữ tin tức / thông báo (Marquee Ticker) cuộn mượt mà từ phải sang trái, có khung viền mạ vàng và huy hiệu tiêu đề cố định.
        /// </summary>
        public static void DrawMarqueeTicker(Rect r, string badgeText, string marqueeContent, float speed = 65f, Color? badgeColor = null)
        {
            EnsureStyles();

            // 1. Nền tối 100% đặc
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.98f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(r, ColorBorderGold, 1);

            // Bốn góc chốt nhỏ
            GUI.color = ColorCornerStud;
            int cSize = 2;
            GUI.DrawTexture(new Rect(r.x, r.y, cSize, cSize), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y, cSize, cSize), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. Huy hiệu danh mục cố định bên trái (Ví dụ 📢 TIN TỨC hoặc 🎯 HƯỚNG DẪN)
            float badgeW = 96f;
            Rect badgeRect = new Rect(r.x + 1, r.y + 1, badgeW, r.height - 2);
            GUI.color = new Color(0.12f, 0.16f, 0.26f, 0.98f);
            GUI.DrawTexture(badgeRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(badgeRect, ColorBorderInner, 1);

            tickerBadgeStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(badgeRect.x + 1, badgeRect.y + 1, badgeRect.width, badgeRect.height), badgeText, tickerBadgeStyle);
            tickerBadgeStyle.normal.textColor = badgeColor ?? ColorTextGold;
            GUI.Label(badgeRect, badgeText, tickerBadgeStyle);

            // 3. Viewport cắt viền chữ chạy (GUI.BeginGroup tự động clip không tràn ra ngoài)
            float clipX = r.x + badgeW + 8f;
            float clipW = r.width - badgeW - 14f;
            Rect clipRect = new Rect(clipX, r.y, clipW, r.height);

            GUI.BeginGroup(clipRect);

            tickerTextStyle.fontSize = 11;
            GUIContent content = new GUIContent(marqueeContent);
            float textW = tickerTextStyle.CalcSize(content).x;
            float travelDistance = clipW + textW + 80f; // 80px khoảng cách ngắt nhịp giữa các lượt lặp
            float currentOffset = clipW - ((Time.time * speed) % travelDistance);

            // Đổ bóng chữ đen mảnh
            tickerTextStyle.normal.textColor = new Color(0f, 0f, 0f, 0.90f);
            GUI.Label(new Rect(currentOffset + 1, 1, textW, r.height), marqueeContent, tickerTextStyle);
            // Bản sao thứ 2 — lặp liền mạch không hở
            GUI.Label(new Rect(currentOffset + travelDistance + 1, 1, textW, r.height), marqueeContent, tickerTextStyle);

            // Chữ hiển thị chính màu trắng vàng dịu mắt
            tickerTextStyle.normal.textColor = new Color(0.96f, 0.94f, 0.85f);
            GUI.Label(new Rect(currentOffset, 0, textW, r.height), marqueeContent, tickerTextStyle);
            // Bản sao thứ 2 — đảm bảo màn hình không bao giờ trống
            GUI.Label(new Rect(currentOffset + travelDistance, 0, textW, r.height), marqueeContent, tickerTextStyle);

            GUI.EndGroup();
        }

        // ================= 4. KHUNG VIỀN ĐÔI MẠ VÀNG (PANEL / MODAL FRAME) =================
        /// <summary>
        /// Vẽ Khung Viền Đôi Mạ Vàng (Double Gold Frame) dùng cho Panel / Modal / Box.
        /// </summary>
        public static void DrawDoubleGoldFrame(Rect r, int borderWidth = 2, Color? customBg = null)
        {
            GUI.color = customBg ?? new Color(0.04f, 0.05f, 0.09f, 1.0f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            DrawOutline(r, ColorBorderGold, borderWidth);

            if (borderWidth >= 2)
            {
                Rect inner = new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4);
                DrawOutline(inner, ColorBorderInner, 1);
            }

            // Đính 4 góc hoàng kim
            if (r.width >= 16 && r.height >= 16)
            {
                GUI.color = ColorCornerStud;
                int cSize = 3;
                GUI.DrawTexture(new Rect(r.x, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }

        // ================= 5. HÀO QUANG CHỌN PHÁT SÁNG (SELECTION GLOW) =================
        /// <summary>
        /// Hiệu ứng hào quang phát sáng nhịp nhàng (Selection Glow Pulse) cho ô trang bị / vật phẩm đang chọn.
        /// </summary>
        public static void DrawSelectionGlow(Rect r, float speed = 4f)
        {
            float pulse = Mathf.PingPong(Time.time * speed, 1f);
            Color glowBorder = Color.Lerp(ColorBorderGold, ColorTextCyan, pulse);

            // Nền bóng phát sáng
            GUI.color = new Color(0.2f, 0.45f, 0.8f, 0.30f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            DrawOutline(r, glowBorder, 2);

            // 4 chốt góc vươn nổi bật
            int sCorner = 4;
            GUI.color = glowBorder;
            GUI.DrawTexture(new Rect(r.x - 2, r.y - 2, sCorner, sCorner), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - sCorner + 2, r.y - 2, sCorner, sCorner), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x - 2, r.y + r.height - sCorner + 2, sCorner, sCorner), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - sCorner + 2, r.y + r.height - sCorner + 2, sCorner, sCorner), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // ================= TIỆN ÍCH VẼ ĐƯỜNG VIỀN KHUNG =================
        public static void DrawOutline(Rect r, Color color, int width = 1)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - width, r.width, width), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, width, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - width, r.y, width, r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
