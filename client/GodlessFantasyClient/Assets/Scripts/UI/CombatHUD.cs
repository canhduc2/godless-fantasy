using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// HUD Hiển Thị Trực Quan Trong Bản Demo Vertical Slice:
    /// - Vẽ thanh HP & Mana
    /// - Hiển thị Cooldown Skill 1 và Dash
    /// - Hướng dẫn phím điều khiển thời gian thực
    /// - Hiển thị trạng thái hiện tại (Idle / Aiming / In Safe Zone)
    /// </summary>
    public class CombatHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private SkillAimingController skillAimingController;
        [SerializeField] private Health playerHealth;

        private GUIStyle titleStyle;
        private GUIStyle statusStyle;
        private GUIStyle infoStyle;
        private GUIStyle boxStyle;
        private bool isStyleInitialized = false;

        private void Start()
        {
            if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (skillAimingController == null) skillAimingController = FindFirstObjectByType<SkillAimingController>();
            if (playerHealth == null && playerMovement != null) playerHealth = playerMovement.GetComponent<Health>();
        }

        private void InitStyles()
        {
            if (isStyleInitialized) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };

            statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.cyan }
            };

            infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = Color.white }
            };

            boxStyle = new GUIStyle(GUI.skin.box);

            isStyleInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // Khung góc trái trên cùng: Thông tin nhân vật
            GUILayout.BeginArea(new Rect(20, 20, 360, 260), boxStyle);
            GUILayout.Space(8);
            GUILayout.Label("⚔️ GODLESS FANTASY — DEMO LÕI (VERTICAL SLICE)", titleStyle);
            GUILayout.Label("SquiRRels Studio · Version 0.1.0", infoStyle);
            GUILayout.Space(6);

            // HP Bar
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
            DrawBar("MÁU (HP)", hp, maxHp, new Color(0.85f, 0.2f, 0.2f));

            GUILayout.Space(4);

            // Mana Bar (Giả lập 100 max)
            DrawBar("NĂNG LƯỢNG (MANA)", 80, 100, new Color(0.2f, 0.5f, 0.95f));

            GUILayout.Space(8);

            // Trạng thái chiến đấu
            bool isAiming = skillAimingController != null && skillAimingController.IsAiming;
            bool inSafe = playerHealth != null && playerHealth.IsInSafeZone;

            if (inSafe)
            {
                GUI.color = Color.green;
                GUILayout.Label("🛡️ VÙNG AN TOÀN (SAFE ZONE) — Miễn nhiễm sát thương PvP", statusStyle);
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(1f, 0.4f, 0.4f);
                GUILayout.Label("⚔️ VÙNG CHIẾN ĐẤU (PVP) — Friendly Fire 100% Kích Hoạt!", statusStyle);
                GUI.color = Color.white;
            }

            if (isAiming)
            {
                GUI.color = Color.yellow;
                GUILayout.Label("🎯 ĐANG NGẮM SKILL 1: Chuột trái chạy né đòn, Bấm 1 để xả, Chuột phải để hủy!", statusStyle);
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.Label("🟢 TRẠNG THÁI: Tự do di chuyển (Idle)", infoStyle);
            }

            GUILayout.EndArea();

            // Khung góc phải trên cùng: Bảng hướng dẫn phím bấm
            GUILayout.BeginArea(new Rect(Screen.width - 340, 20, 320, 220), boxStyle);
            GUILayout.Space(6);
            GUILayout.Label("🎮 HƯỚNG DẪN ĐIỀU KHIỂN (CONTROLS)", titleStyle);
            GUILayout.Space(4);
            GUILayout.Label("• Chuột trái: Click-to-Move (Tự tìm đường)", infoStyle);
            GUILayout.Label("• W / A / S / D: Ghi đè di chuyển thủ công", infoStyle);
            GUILayout.Label("• Shift: Dash lướt thẳng theo hướng chuột", infoStyle);
            GUILayout.Label("• Phím 1 (Lần 1): Bật chế độ ngắm (Aiming)", infoStyle);
            GUILayout.Label("• Phím 1 (Lần 2): Xác nhận xả đạn ma thuật", infoStyle);
            GUILayout.Label("• Chuột phải: Hủy ngắm lập tức (0 Mana/CD)", infoStyle);
            GUILayout.EndArea();

            // Khung góc dưới màn hình: Thanh Cooldown
            DrawBottomCooldowns();
        }

        private void DrawBar(string label, int current, int max, Color barColor)
        {
            GUILayout.Label($"{label}: {current} / {max}", infoStyle);
            Rect rect = GUILayoutUtility.GetRect(320, 16);
            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            float fill = Mathf.Clamp01((float)current / max);
            Rect fillRect = new Rect(rect.x, rect.y, rect.width * fill, rect.height);
            GUI.color = barColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawBottomCooldowns()
        {
            float boxWidth = 360f;
            float boxHeight = 70f;
            Rect bottomRect = new Rect((Screen.width - boxWidth) / 2f, Screen.height - boxHeight - 20, boxWidth, boxHeight);

            GUILayout.BeginArea(bottomRect, boxStyle);
            GUILayout.BeginHorizontal();

            // Skill 1 Box
            float s1Cd = skillAimingController != null ? skillAimingController.Skill1CooldownRemaining : 0f;
            bool s1Ready = s1Cd <= 0.05f;
            GUI.color = s1Ready ? Color.cyan : Color.gray;
            GUILayout.Box(s1Ready ? "[PHÍM 1]\nSkill Sẵn Sàng" : $"[PHÍM 1]\nHồi {s1Cd:F1}s", GUILayout.Width(160), GUILayout.Height(50));
            GUI.color = Color.white;

            GUILayout.Space(10);

            // Dash Box
            float dashCd = playerMovement != null ? playerMovement.DashCooldownRemaining : 0f;
            bool dashReady = dashCd <= 0.05f;
            GUI.color = dashReady ? Color.green : Color.gray;
            GUILayout.Box(dashReady ? "[SHIFT]\nDash Sẵn Sàng" : $"[SHIFT]\nHồi {dashCd:F1}s", GUILayout.Width(160), GUILayout.Height(50));
            GUI.color = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
