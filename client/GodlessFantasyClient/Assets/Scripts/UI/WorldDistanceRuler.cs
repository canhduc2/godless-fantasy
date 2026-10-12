using System;
using UnityEngine;
using GodlessFantasy.Core;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Thanh Thước Đo Khoảng Cách Thế Giới Mỏng Gọn Nhẹ Không Nền (World Distance Metric Ruler):
    /// - 100% Không nền (Transparent Overlay), giữ tầm nhìn thế giới rộng mở, thoáng đãng.
    /// - Tự động thu phóng (Scale/Zoom) theo con lăn chuột: 1 mét thế giới (World Unit) luôn khớp chuẩn xác
    ///   với số pixel thực tế trên màn hình theo Camera.main.orthographicSize!
    /// - Vạch [0m] neo thẳng đứng theo vị trí X của nhân vật Người Chơi (Hero).
    /// - Vạch chia mét trực quan:
    ///   + Vạch 1m: Vạch mỏng thanh mảnh.
    ///   + Vạch 2m & 5m: Vạch trung bình kèm số mét rõ ràng.
    ///   + Vạch 10m, 15m, 20m...: Vạch màu vàng hoàng kim nổi bật.
    /// - Đánh dấu cự ly thông minh:
    ///   + Vạch Tầm Đánh Thường (Basic Attack Range): Kị Sĩ 2.35m, Võ Sĩ 1.85m, Xạ Thủ 9.5m.
    ///   + Vạch Tầm Chiêu Thực Đang Ngắm (Cast Range): ví dụ 8.0m.
    ///   + Vạch Con Trỏ Chuột (Mouse Crosshair Distance): hiển thị cự ly chuột từ người chơi theo thời gian thực!
    /// - Phím nóng [R]: Bật / Tắt thanh thước tức thì.
    /// </summary>
    public class WorldDistanceRuler : MonoBehaviour
    {
        public static WorldDistanceRuler Instance { get; private set; }

        public bool IsEnabled = true;

        private GUIStyle tickLabelStyle;
        private GUIStyle markerLabelStyle;
        private GUIStyle zeroLabelStyle;
        private bool isStyleInitialized = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            IsEnabled = GameSettings.IsDistanceRulerEnabled;
        }

        private void Update()
        {
            // Phím [R]: Bật / Tắt thanh thước khoảng cách
            if (Input.GetKeyDown(KeyCode.R))
            {
                bool isTypingChat = CombatHUD.Instance != null && CombatHUD.Instance.IsChatFocused();
                if (!isTypingChat)
                {
                    IsEnabled = !IsEnabled;
                    GameSettings.SetDistanceRulerEnabled(IsEnabled);

                    string status = IsEnabled ? "BẬT" : "TẮT";
                    FloatingDamageManager.ShowFloatingText(
                        Vector3.zero,
                        $"📏 Thước Khoảng Cách: {status} [R]",
                        IsEnabled ? new Color(0.35f, 0.95f, 0.5f) : new Color(0.85f, 0.4f, 0.4f)
                    );
                }
            }
        }

        private void InitStyles()
        {
            if (isStyleInitialized && tickLabelStyle != null) return;

            tickLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                normal = { textColor = new Color(0.9f, 0.95f, 1f, 0.92f) }
            };

            markerLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                normal = { textColor = new Color(1f, 0.85f, 0.25f, 0.95f) }
            };

            zeroLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                normal = { textColor = new Color(0.2f, 1f, 0.5f, 1f) }
            };

            isStyleInitialized = true;
        }

        private void OnGUI()
        {
            if (!IsEnabled && !GameSettings.IsDistanceRulerEnabled) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            var player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player_Hero");
            if (player == null)
            {
                var pm = FindAnyObjectByType<GodlessFantasy.Player.PlayerMovement>();
                if (pm != null) player = pm.gameObject;
            }
            if (player == null) return;

            InitStyles();

            GUI.depth = FantasyUITheme.DepthBaseHUD - 1; // Nằm trên nền game, dưới các cửa sổ popup modal

            Vector3 playerWorldPos = player.transform.position;
            Vector3 playerScreen = cam.WorldToScreenPoint(playerWorldPos);

            // Tọa độ màn hình của người chơi
            float zeroScreenX = playerScreen.x;
            float zeroScreenY = Screen.height - playerScreen.y;

            // Tính toán tỉ lệ Pixels / Mét trên màn hình dựa vào Camera zoom hiện tại
            Vector3 oneMeterRight = cam.WorldToScreenPoint(playerWorldPos + Vector3.right * 1.0f);
            float pixelsPerMeterX = Mathf.Abs(oneMeterRight.x - zeroScreenX);

            if (pixelsPerMeterX < 4f) return; // Quá xa, tránh vẽ dày đặc

            // ─────────────────────────────────────────────────────────────
            // 1. THANH THƯỚC NGANG TRÊN ĐỈNH MÀN HÌNH (TOP DISTANCE RULER)
            // ─────────────────────────────────────────────────────────────
            float rulerY = 16f; // Vị trí thanh thước chạy ngang ở đỉnh màn hình

            // Trục chính của thước: Đường kẻ mảnh mờ (100% không nền khối đen)
            GUI.color = new Color(0.35f, 0.55f, 0.85f, 0.40f);
            GUI.DrawTexture(new Rect(0, rulerY, Screen.width, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Điểm mốc [0m] ngay trên đầu Người Chơi (Hero Center)
            DrawZeroTick(zeroScreenX, rulerY);

            // Tầm đánh cơ bản của người chơi (Basic Attack Range)
            var pStats = player.GetComponent<PlayerStats>();
            float basicAtkRange = (pStats != null) ? pStats.DynamicBasicAttackRange : 2.35f;
            string atkIcon = (pStats != null && pStats.CharacterClass.Contains("Võ")) ? "🥊" :
                             (pStats != null && pStats.CharacterClass.Contains("Xạ")) ? "🏹" :
                             (pStats != null && pStats.CharacterClass.Contains("Pháp")) ? "🔮" : "⚔️";

            string atkRangeStr = basicAtkRange.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "m";
            DrawRangeMarker(zeroScreenX + basicAtkRange * pixelsPerMeterX, rulerY, basicAtkRange, $"{atkIcon} {atkRangeStr}", new Color(1.0f, 0.55f, 0.15f));
            DrawRangeMarker(zeroScreenX - basicAtkRange * pixelsPerMeterX, rulerY, basicAtkRange, $"{atkIcon} {atkRangeStr}", new Color(1.0f, 0.55f, 0.15f));

            // Tầm ngắm chiêu đang kích hoạt (nếu có)
            var aimingCtrl = player.GetComponent<SkillAimingController>();
            if (aimingCtrl != null && aimingCtrl.IsAiming)
            {
                var scroll = aimingCtrl.GetEquippedScroll(aimingCtrl.CurrentAimingSlot - 1);
                float castRange = (scroll != null && scroll.castRange > 0) ? scroll.castRange : 8.0f;
                string castRangeStr = castRange.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "m";
                DrawRangeMarker(zeroScreenX + castRange * pixelsPerMeterX, rulerY, castRange, $"🎯 {castRangeStr}", new Color(1.0f, 0.25f, 0.35f));
                DrawRangeMarker(zeroScreenX - castRange * pixelsPerMeterX, rulerY, castRange, $"🎯 {castRangeStr}", new Color(1.0f, 0.25f, 0.35f));
            }

            // Vạch con trỏ chuột (Mouse Distance Tracking - Bám chính xác 1:1 theo tọa độ chuột màn hình)
            Vector3 mouseScreen = GameInput.GetMousePosition();
            Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
            float distToMouse = Vector2.Distance(playerWorldPos, mouseWorld);
            float mouseTickX = mouseScreen.x; // Chính xác tuyệt đối theo vị trí con trỏ chuột thực tế
            if (mouseTickX >= 0 && mouseTickX <= Screen.width && !CombatHUD.IsMouseOverAnyUI())
            {
                DrawMouseMarker(mouseTickX, rulerY, distToMouse);
            }

            // Vẽ các vạch chia sang bên PHẢI (+X)
            int maxMetersRight = Mathf.CeilToInt((Screen.width - zeroScreenX) / pixelsPerMeterX) + 2;
            for (int m = 1; m <= maxMetersRight; m++)
            {
                float tickX = zeroScreenX + m * pixelsPerMeterX;
                if (tickX > Screen.width) break;
                DrawDistanceTick(tickX, rulerY, m, pixelsPerMeterX);
            }

            // Vẽ các vạch chia sang bên TRÁI (-X)
            int maxMetersLeft = Mathf.CeilToInt(zeroScreenX / pixelsPerMeterX) + 2;
            for (int m = 1; m <= maxMetersLeft; m++)
            {
                float tickX = zeroScreenX - m * pixelsPerMeterX;
                if (tickX < 0) break;
                DrawDistanceTick(tickX, rulerY, m, pixelsPerMeterX);
            }
        }

        private void DrawZeroTick(float x, float y)
        {
            // Vạch 0m thẳng đứng
            GUI.color = new Color(0.2f, 1f, 0.5f, 0.95f);
            GUI.DrawTexture(new Rect(x - 1, y - 6, 2, 14), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Nhãn 0m
            DrawShadowedText(new Rect(x - 24, y - 18, 48, 16), "▲ 0m", zeroLabelStyle, new Color(0.2f, 1f, 0.5f));
        }

        private void DrawDistanceTick(float x, float y, int meters, float ppm)
        {
            bool isMajor = (meters % 5 == 0);
            bool isSubMajor = (meters % 2 == 0);

            float tickH;
            Color tickCol;

            if (isMajor)
            {
                tickH = (meters % 10 == 0) ? 10f : 8f;
                tickCol = (meters % 10 == 0) ? new Color(1f, 0.88f, 0.35f, 0.95f) : new Color(0.35f, 0.85f, 1f, 0.85f);
            }
            else if (isSubMajor)
            {
                tickH = 5f;
                tickCol = new Color(0.6f, 0.8f, 0.95f, 0.60f);
            }
            else
            {
                // Vạch nhỏ 1m
                if (ppm < 14f) return; // Nếu zoom quá xa thì ẩn bớt vạch 1m để không bị rối mắt
                tickH = 3f;
                tickCol = new Color(0.85f, 0.9f, 1f, 0.30f);
            }

            // Kẻ vạch
            GUI.color = tickCol;
            GUI.DrawTexture(new Rect(x, y - tickH, 1, tickH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // In chữ số khoảng cách
            if (isMajor || (isSubMajor && ppm >= 28f))
            {
                string txt = $"{meters}m";
                Color textCol = isMajor ? new Color(1f, 0.9f, 0.4f) : new Color(0.85f, 0.92f, 1f);
                DrawShadowedText(new Rect(x - 20, y - 16, 40, 14), txt, tickLabelStyle, textCol);
            }
        }

        private void DrawRangeMarker(float x, float y, float range, string label, Color color)
        {
            if (x < 0 || x > Screen.width) return;

            // Vạch đánh dấu tầm chiêu
            GUI.color = color;
            GUI.DrawTexture(new Rect(x - 1, y - 8, 2, 10), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Nhãn
            DrawShadowedText(new Rect(x - 30, y + 2, 60, 14), label, markerLabelStyle, color);
        }

        private void DrawMouseMarker(float x, float y, float dist)
        {
            // Vạch con trỏ chuột chạy theo chuột
            Color mouseCol = new Color(0.2f, 0.95f, 1f, 0.95f);
            GUI.color = mouseCol;
            GUI.DrawTexture(new Rect(x, y - 8, 1, 12), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Nhãn cự ly chuột chính xác
            DrawShadowedText(new Rect(x - 30, y + 2, 60, 14), $"📍 {dist:F1}m", markerLabelStyle, mouseCol);
        }

        private void DrawShadowedText(Rect r, string text, GUIStyle style, Color textColor)
        {
            style.richText = true;

            // 1. Viền đen 8 hướng xung quanh
            string outlineStr = $"<color=#000000>{text}</color>";
            GUI.Label(new Rect(r.x - 1, r.y - 1, r.width, r.height), outlineStr, style);
            GUI.Label(new Rect(r.x,     r.y - 1, r.width, r.height), outlineStr, style);
            GUI.Label(new Rect(r.x + 1, r.y - 1, r.width, r.height), outlineStr, style);
            GUI.Label(new Rect(r.x - 1, r.y,     r.width, r.height), outlineStr, style);
            GUI.Label(new Rect(r.x + 1, r.y,     r.width, r.height), outlineStr, style);
            GUI.Label(new Rect(r.x - 1, r.y + 1, r.width, r.height), outlineStr, style);
            GUI.Label(new Rect(r.x,     r.y + 1, r.width, r.height), outlineStr, style);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), outlineStr, style);

            // 2. Chữ nhân màu nguyên bản ở giữa (Xanh lam, Vàng kim, Xanh lá 0m...)
            string hex = ColorUtility.ToHtmlStringRGBA(textColor);
            GUI.Label(r, $"<color=#{hex}>{text}</color>", style);
        }
    }
}
