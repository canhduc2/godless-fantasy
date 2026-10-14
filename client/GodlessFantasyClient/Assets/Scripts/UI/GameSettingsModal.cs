using System;
using UnityEngine;
using GodlessFantasy.Core;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Cửa Sổ Cài Đặt Hệ Thống & Tốc Độ Giả Lập Hiệu Ứng (System & Audio Settings Modal):
    /// - Điều khiển Âm thanh toàn diện: Master Volume, BGM, SFX với các thanh trượt Slider & Nút Toggle Mute riêng biệt.
    /// - Thanh gạt Tốc độ Hiệu Ứng (Simulation Speed / Slow-Motion Slider): từ 0.1x đến 2.0x.
    ///   Giúp tester/dev soi từng frame hiệu ứng bộc phá, phôi đạn bay chậm rãi cực kỳ chuẩn xác và đã mắt!
    /// - Cài đặt Hiển thị: Bật/Tắt Số Sát Thương, Rung Màn Hình, Đồng hồ FPS.
    /// - Mở nhanh bằng phím [O] hoặc nút [⚙️ Cài Đặt] trên HUD.
    /// </summary>
    public class GameSettingsModal : MonoBehaviour
    {
        public static GameSettingsModal Instance { get; private set; }

        public bool IsOpen = false;

        private GUIStyle titleStyle;
        private GUIStyle groupHeaderStyle;
        private GUIStyle labelStyle;
        private GUIStyle valueStyle;
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

        private void Update()
        {
            // Vô hiệu hóa hoàn toàn khi đang ở màn hình Đăng Nhập / Tạo Nhân Vật
            bool isAuthActive = (AuthUniverseModal.Instance != null && AuthUniverseModal.Instance.IsOpen)
                             || (CharacterCreationModal.Instance != null && CharacterCreationModal.Instance.IsOpen);
            if (isAuthActive)
            {
                if (IsOpen) IsOpen = false;
                return;
            }

            // Phím [O]: Bật / Tắt cửa sổ Cài Đặt
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (!CombatHUD.IsAnyModalOpen() || IsOpen)
                {
                    IsOpen = !IsOpen;
                }
            }
            // Phím [Escape]: Đóng cửa sổ Cài Đặt nếu đang mở
            else if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
            {
                IsOpen = false;
            }
        }

        private void InitStyles()
        {
            if (isStyleInitialized && titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.88f, 0.35f) }
            };

            groupHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.35f, 0.85f, 1f) }
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };

            valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(1f, 0.85f, 0.25f) }
            };

            isStyleInitialized = true;
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            InitStyles();

            GUI.depth = FantasyUITheme.DepthModal - 5; // Luôn hiển thị trên cùng

            // 1. Phủ mờ màn hình nhẹ nhàng
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. Khung Window Chính Cài Đặt
            float modalW = 540f;
            float modalH = 460f;
            float modalX = (Screen.width - modalW) / 2f;
            float modalY = (Screen.height - modalH) / 2f;
            Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

            // Nền tối và viền Fantasy Gold sang trọng
            GUI.color = new Color(0.04f, 0.05f, 0.09f, 0.98f);
            GUI.DrawTexture(modalRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawDoubleGoldFrame(modalRect, 2);

            // 3. Header Tiêu Đề
            GUI.Label(new Rect(modalX, modalY + 8f, modalW, 26f), "⚙️ CÀI ĐẶT HỆ THỐNG & ÂM THANH (SETTINGS)", titleStyle);

            // Nút Đóng [✕]
            if (GUI.Button(new Rect(modalX + modalW - 34f, modalY + 8f, 26f, 24f), "✕"))
            {
                IsOpen = false;
            }

            float curY = modalY + 42f;

            // ─────────────────────────────────────────────────────────────
            // MỤC 1: 🔊 ÂM LƯỢNG & NHẠC NỀN
            // ─────────────────────────────────────────────────────────────
            Rect groupAudio = new Rect(modalX + 16f, curY, modalW - 32f, 138f);
            DrawGroupFrame(groupAudio, "🔊 ÂM LƯỢNG & NHẠC NỀN");

            float audioY = curY + 26f;

            // 1.1 Master Volume
            DrawVolumeRow(modalX + 26f, audioY, modalW - 52f, "Âm Lượng Tổng:",
                GameSettings.MasterVolume, GameSettings.IsMasterMuted,
                (v) => GameSettings.SetMasterVolume(v),
                () => GameSettings.SetMasterMute(!GameSettings.IsMasterMuted));
            audioY += 34f;

            // 1.2 BGM Volume
            DrawVolumeRow(modalX + 26f, audioY, modalW - 52f, "Nhạc Nền (BGM):",
                GameSettings.BgmVolume, GameSettings.IsBgmMuted,
                (v) => GameSettings.SetBgmVolume(v),
                () => GameSettings.SetBgmMute(!GameSettings.IsBgmMuted));
            audioY += 34f;

            // 1.3 SFX Volume
            DrawVolumeRow(modalX + 26f, audioY, modalW - 52f, "Hiệu Ứng (SFX):",
                GameSettings.SfxVolume, GameSettings.IsSfxMuted,
                (v) => GameSettings.SetSfxVolume(v),
                () => GameSettings.SetSfxMute(!GameSettings.IsSfxMuted));

            curY += 146f;

            // ─────────────────────────────────────────────────────────────
            // MỤC 2: ⏱️ TỐC ĐỘ HIỆU ỨNG & GIẢ LẬP (SLOW-MOTION TESTING)
            // ─────────────────────────────────────────────────────────────
            Rect groupSpeed = new Rect(modalX + 16f, curY, modalW - 32f, 108f);
            DrawGroupFrame(groupSpeed, "⏱️ TỐC ĐỘ HIỆU ỨNG & GIẢ LẬP (SLOW-MOTION)");

            float speedY = curY + 26f;

            // Nhãn tốc độ + Giá trị hiển thị
            float curSpeed = GameSettings.SimulationSpeed;
            string slowSpeedStr = (1f / curSpeed).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            string fastSpeedStr = curSpeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            string speedDesc = (curSpeed < 0.95f) ? $" (Chậm {slowSpeedStr}x - Soi từng frame)" :
                               (curSpeed > 1.05f) ? $" (Tua nhanh {fastSpeedStr}x)" : " (Chuẩn 100%)";

            GUI.Label(new Rect(modalX + 26f, speedY, 180f, 22f), "Tốc Độ Hiệu Ứng:", labelStyle);
            GUI.Label(new Rect(modalX + modalW - 200f, speedY, 174f, 22f), $"{curSpeed:0.00}x{speedDesc}", valueStyle);
            speedY += 24f;

            // Thanh trượt tốc độ (0.1x -> 2.0x)
            float newSpeed = GUI.HorizontalSlider(new Rect(modalX + 26f, speedY, modalW - 52f, 20f), curSpeed, 0.1f, 2.0f);
            if (Mathf.Abs(newSpeed - curSpeed) > 0.005f)
            {
                GameSettings.SetSimulationSpeed(newSpeed);
            }
            speedY += 22f;

            // Các nút preset chọn nhanh
            float btnW = (modalW - 52f - 16f) / 5f;
            float btnX = modalX + 26f;

            if (DrawPresetButton(new Rect(btnX, speedY, btnW, 26f), "🐢 0.2x", curSpeed, 0.2f))
                GameSettings.SetSimulationSpeed(0.2f);
            btnX += btnW + 4f;

            if (DrawPresetButton(new Rect(btnX, speedY, btnW, 26f), "0.5x", curSpeed, 0.5f))
                GameSettings.SetSimulationSpeed(0.5f);
            btnX += btnW + 4f;

            if (DrawPresetButton(new Rect(btnX, speedY, btnW, 26f), "▶️ 1.0x", curSpeed, 1.0f))
                GameSettings.SetSimulationSpeed(1.0f);
            btnX += btnW + 4f;

            if (DrawPresetButton(new Rect(btnX, speedY, btnW, 26f), "1.5x", curSpeed, 1.5f))
                GameSettings.SetSimulationSpeed(1.5f);
            btnX += btnW + 4f;

            if (DrawPresetButton(new Rect(btnX, speedY, btnW, 26f), "🚀 2.0x", curSpeed, 2.0f))
                GameSettings.SetSimulationSpeed(2.0f);

            curY += 116f;

            // ─────────────────────────────────────────────────────────────
            // MỤC 3: 🎮 TIỆN ÍCH HIỂN THỊ
            // ─────────────────────────────────────────────────────────────
            Rect groupToggles = new Rect(modalX + 16f, curY, modalW - 32f, 80f);
            DrawGroupFrame(groupToggles, "🎮 TIỆN ÍCH HIỂN THỊ");

            float togY = curY + 28f;
            float togColW = (modalW - 52f) / 4f;

            // Toggle 1: Số sát thương
            bool curDmg = GameSettings.ShowDamageNumbers;
            bool newDmg = GUI.Toggle(new Rect(modalX + 26f, togY, togColW, 22f), curDmg, " Sát Thương");
            if (newDmg != curDmg) GameSettings.SetShowDamageNumbers(newDmg);

            // Toggle 2: Rung màn hình
            bool curShake = GameSettings.ScreenShakeEnabled;
            bool newShake = GUI.Toggle(new Rect(modalX + 26f + togColW, togY, togColW, 22f), curShake, " Rung Màn");
            if (newShake != curShake) GameSettings.SetScreenShake(newShake);

            // Toggle 3: Thước đo khoảng cách [R]
            bool curRuler = GameSettings.IsDistanceRulerEnabled;
            bool newRuler = GUI.Toggle(new Rect(modalX + 26f + togColW * 2f, togY, togColW, 22f), curRuler, " Thước [R]");
            if (newRuler != curRuler)
            {
                GameSettings.SetDistanceRulerEnabled(newRuler);
                if (WorldDistanceRuler.Instance != null) WorldDistanceRuler.Instance.IsEnabled = newRuler;
            }

            // Toggle 4: Đồng hồ FPS
            bool curFps = GameSettings.ShowFpsCounter;
            bool newFps = GUI.Toggle(new Rect(modalX + 26f + togColW * 3f, togY, togColW, 22f), curFps, " FPS Counter");
            if (newFps != curFps) GameSettings.SetShowFpsCounter(newFps);

            curY += 88f;

            // Nút Đóng & Lưu
            float closeBtnW = 160f;
            if (GUI.Button(new Rect(modalX + (modalW - closeBtnW) / 2f, curY + 2f, closeBtnW, 30f), "💾 ĐÓNG CÀI ĐẶT [ESC]"))
            {
                IsOpen = false;
            }
        }

        private void DrawGroupFrame(Rect r, string title)
        {
            GUI.color = new Color(0.06f, 0.08f, 0.13f, 0.95f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(r, new Color(0.35f, 0.45f, 0.65f, 0.6f), 1);

            // Header nền nhỏ
            Rect headR = new Rect(r.x, r.y, r.width, 24f);
            GUI.color = new Color(0.08f, 0.12f, 0.20f, 0.95f);
            GUI.DrawTexture(headR, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(r.x + 8f, r.y + 2f, r.width - 16f, 20f), title, groupHeaderStyle);
        }

        private void DrawVolumeRow(float x, float y, float w, string label, float vol, bool isMuted, Action<float> onVolChange, Action onToggleMute)
        {
            // Nhãn tên
            GUI.Label(new Rect(x, y, 120f, 22f), label, labelStyle);

            // Slider thanh trượt âm lượng
            float sliderX = x + 125f;
            float sliderW = w - 125f - 80f - 65f;
            float newVol = GUI.HorizontalSlider(new Rect(sliderX, y + 4f, sliderW, 20f), isMuted ? 0f : vol, 0f, 1f);
            if (!isMuted && Mathf.Abs(newVol - vol) > 0.01f)
            {
                onVolChange(newVol);
            }

            // Phần trăm hiển thị
            int pct = isMuted ? 0 : Mathf.RoundToInt(vol * 100f);
            GUI.Label(new Rect(sliderX + sliderW + 6f, y, 55f, 22f), $"{pct}%", valueStyle);

            // Nút Bật/Tắt Mute
            string btnLabel = isMuted ? "🔇 TẮT" : "🔊 BẬT";
            Color btnColor = isMuted ? new Color(0.85f, 0.35f, 0.35f) : new Color(0.35f, 0.85f, 0.45f);
            GUI.color = btnColor;
            if (GUI.Button(new Rect(x + w - 70f, y, 70f, 22f), btnLabel))
            {
                onToggleMute();
            }
            GUI.color = Color.white;
        }

        private bool DrawPresetButton(Rect r, string label, float curSpeed, float targetSpeed)
        {
            bool isCurrent = Mathf.Abs(curSpeed - targetSpeed) < 0.04f;
            if (isCurrent)
            {
                GUI.color = new Color(1f, 0.85f, 0.25f);
            }
            else
            {
                GUI.color = new Color(0.85f, 0.90f, 1f);
            }

            bool clicked = GUI.Button(r, label);
            GUI.color = Color.white;
            return clicked;
        }
    }
}
