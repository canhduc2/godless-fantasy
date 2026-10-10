using System;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Giao Diện HUD Hoàn Chỉnh Chuẩn Phong Cách Gold-Slate Studio:
    /// - Góc Trái Trên: Bảng Thông Tin Nhân Vật (Avatar, Tên, Cấp 1, Hệ Hỏa, INT/STR/VIT/AGI, Máu Đỏ, Mana Xanh).
    /// - Giữa Màn Hình Trên: Dải Tin Tức Toàn Server (Server Announcement Banner).
    /// - Góc Phải Trên: Minimap Tọa Độ + Nút Chat Tròn Nhỏ [💬].
    /// - Mép Trái: Nút Menu Trượt Hình [>] mở Bảng 5 Tab (Trang Bị 7 ô, Túi Đồ, Kỹ Năng, Thú Cưng, Cài Đặt).
    /// - Dưới Cùng: 4 Ô Chiêu Thức + 1 Ô LƯỚT (DASH) chuyên dụng, hiển thị ICON hình ảnh, COOLDOWN và TOOLTIP tự tính theo chỉ số!
    /// </summary>
    public class CombatHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private SkillAimingController skillAimingController;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerStats playerStats;

        // Textures giao diện
        private Texture2D panelBgTex;
        private Texture2D hpFillTex;
        private Texture2D manaFillTex;
        private Texture2D radarBgTex;
        private Texture2D slotBgTex;
        private Texture2D cdOverlayTex;

        // Skill Icons
        private Texture2D skill1Icon;
        private Texture2D dashIcon;
        private Texture2D hpPotionIcon;
        private Texture2D manaPotionIcon;

        // GUIStyles
        private GUIStyle boldWhiteStyle;
        private GUIStyle smallWhiteStyle;
        private GUIStyle serverNewsStyle;
        private GUIStyle skillCdStyle;
        private GUIStyle drawerBtnStyle;
        private GUIStyle tooltipTitleStyle;
        private GUIStyle tooltipDescStyle;
        private GUIStyle statBadgeStyle;
        private bool isStyleInitialized = false;

        // Drawer Menu States
        private bool isDrawerOpen = false;
        private int currentDrawerTab = 0; // 0=Trang Bị, 1=Túi Đồ, 2=Kỹ Năng, 3=Thú Cưng, 4=Cài Đặt
        private readonly string[] drawerTabs = { "TRANG BỊ", "TÚI ĐỒ", "KỸ NĂNG", "THÚ CƯNG", "CÀI ĐẶT" };

        // Chat Modal State
        private bool isChatOpen = false;
        private string chatInputText = "/trade Player_2";
        private string chatHistory = "📢 [Server]: Chào mừng bạn đến với Godless Fantasy!\n💬 [Hệ Thống]: Đã vào Làng Tân Thủ (Vùng An Toàn).\n⚡ [Mẹo]: Dùng phím [Q] hồi Máu, [E] hồi Mana, [Shift/Space] để Lướt!";

        // Pet Release Confirm Modal State
        private bool isReleasingPet = false;
        private string petConfirmInput = "";
        private float petReleaseCountdown = 60f;

        // Settings States
        private bool autoDeclineTrade = false;

        // Tooltip State
        private string activeTooltipTitle = "";
        private string activeTooltipBody = "";
        private Vector2 activeTooltipPos;
        private bool showTooltip = false;

        private void Start()
        {
            FindReferences();
            CreateTextures();
            LoadIcons();
        }

        private void FindReferences()
        {
            if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (skillAimingController == null) skillAimingController = FindFirstObjectByType<SkillAimingController>();
            if (playerMovement != null)
            {
                if (playerHealth == null) playerHealth = playerMovement.GetComponent<Health>();
                if (playerStats == null) playerStats = playerMovement.GetComponent<PlayerStats>();
            }
        }

        private void CreateTextures()
        {
            if (panelBgTex == null) panelBgTex = MakeColorTex(new Color(0.06f, 0.08f, 0.14f, 0.94f));
            if (hpFillTex == null) hpFillTex = MakeColorTex(new Color(0.90f, 0.22f, 0.24f, 1f));
            if (manaFillTex == null) manaFillTex = MakeColorTex(new Color(0.16f, 0.52f, 0.92f, 1f));
            if (radarBgTex == null) radarBgTex = MakeColorTex(new Color(0.04f, 0.06f, 0.10f, 0.96f));
            if (slotBgTex == null) slotBgTex = MakeColorTex(new Color(0.08f, 0.11f, 0.18f, 0.95f));
            if (cdOverlayTex == null) cdOverlayTex = MakeColorTex(new Color(0f, 0f, 0f, 0.70f));
        }

        private void LoadIcons()
        {
            if (skill1Icon == null) skill1Icon = Resources.Load<Texture2D>("icon_skill_arcane_bolt");
            if (dashIcon == null) dashIcon = Resources.Load<Texture2D>("icon_skill_dash");
            if (hpPotionIcon == null) hpPotionIcon = Resources.Load<Texture2D>("item_potion_hp");
            if (manaPotionIcon == null) manaPotionIcon = Resources.Load<Texture2D>("item_potion_mana");
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
            if (isStyleInitialized && boldWhiteStyle != null && skillCdStyle != null) return;

            boldWhiteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            smallWhiteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };

            serverNewsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };

            skillCdStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            drawerBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.80f, 0.35f) }
            };

            tooltipTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };

            tooltipDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };

            statBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.75f, 0.85f, 0.95f) }
            };

            isStyleInitialized = true;
        }

        private void OnGUI()
        {
            if (panelBgTex == null) CreateTextures();
            InitStyles();
            showTooltip = false;

            DrawTopLeftPlayerPanel();
            DrawTopCenterServerNews();
            DrawTopRightMinimapAndChat();
            DrawMidLeftDrawerButton();
            DrawBottomCenterSkillBar();

            if (isDrawerOpen) DrawDrawerModal();
            if (isChatOpen) DrawChatModal();
            if (showTooltip) DrawTooltipPopup();
        }

        // ================= 1. GÓC TRÁI TRÊN: BẢNG THÔNG TIN NHÂN VẬT (CHUẨN CROP 3) =================
        private void DrawTopLeftPlayerPanel()
        {
            float panelW = 270f;
            float panelH = 76f;
            Rect panel = new Rect(16, 14, panelW, panelH);

            DrawDoubleGoldFrame(panel);

            // Avatar Box 44x44
            Rect av = new Rect(panel.x + 8, panel.y + 8, 44, 44);
            GUI.color = new Color(0.08f, 0.12f, 0.20f);
            GUI.DrawTexture(av, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(av, 1);
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(av, "⚔️", boldWhiteStyle);

            // Badge Level ở góc Avatar
            int lv = playerStats != null ? playerStats.Level : 1;
            Rect lvBadge = new Rect(av.x - 2, av.y + av.height - 12, 28, 14);
            GUI.color = new Color(0.75f, 0.55f, 0.15f, 0.95f);
            GUI.DrawTexture(lvBadge, Texture2D.whiteTexture);
            GUI.color = Color.white;
            smallWhiteStyle.fontSize = 9;
            GUI.Label(lvBadge, $"Lv.{lv}", smallWhiteStyle);
            smallWhiteStyle.fontSize = 10;

            // Nhãn Tên Nhân Vật + Hệ Nguyên Tố
            string charName = playerStats != null ? playerStats.CharacterName : "Anh Hùng";
            string elem = playerStats != null ? playerStats.Element : "Hỏa (Fire)";
            boldWhiteStyle.alignment = TextAnchor.UpperLeft;
            boldWhiteStyle.fontSize = 11;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(panel.x + 58, panel.y + 6, 205, 16), $"{charName} · [{elem}]", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            boldWhiteStyle.fontSize = 12;

            // Thanh Máu HP (Click được để uống máu phím Q!)
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
            Rect hpRect = new Rect(panel.x + 58, panel.y + 24, 200, 15);
            DrawInteractiveBar(hpRect, hp, maxHp, hpFillTex, $"HP {hp}/{maxHp} [Bấm Q: +35]", () => {
                if (playerStats != null) playerStats.UseHpPotion();
            });

            // Thanh Mana (Click được để uống mana phím E!)
            int mana = playerStats != null ? playerStats.CurrentMana : 80;
            int maxMana = playerStats != null ? playerStats.MaxMana : 100;
            Rect manaRect = new Rect(panel.x + 58, panel.y + 42, 200, 15);
            DrawInteractiveBar(manaRect, mana, maxMana, manaFillTex, $"MANA {mana}/{maxMana} [Bấm E: +30]", () => {
                if (playerStats != null) playerStats.UseManaPotion();
            });

            // Dòng hiển thị thuộc tính cơ bản STR/INT/VIT/AGI dưới đáy panel
            int str = playerStats != null ? playerStats.Strength : 8;
            int intel = playerStats != null ? playerStats.Intelligence : 12;
            int vit = playerStats != null ? playerStats.Vitality : 10;
            int agi = playerStats != null ? playerStats.Agility : 9;
            GUI.Label(new Rect(panel.x + 58, panel.y + 59, 205, 14), $"INT: {intel} · STR: {str} · VIT: {vit} · AGI: {agi} | Q:x{playerStats?.HpPotionCount ?? 5} E:x{playerStats?.ManaPotionCount ?? 5}", statBadgeStyle);
        }

        private void DrawInteractiveBar(Rect r, int cur, int max, Texture2D fillTex, string text, Action onClick)
        {
            // Nền tối
            GUI.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Phần thanh đầy
            float fill = Mathf.Clamp01((float)cur / max);
            Rect fRect = new Rect(r.x, r.y, r.width * fill, r.height);
            GUI.DrawTexture(fRect, fillTex);

            // Viền vàng mảnh
            DrawOutline(r, new Color(0.35f, 0.45f, 0.6f, 0.5f), 1);

            // Chữ
            GUI.Label(r, text, smallWhiteStyle);

            // Click trực tiếp để dùng
            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                onClick?.Invoke();
                Event.current.Use();
            }
        }

        // ================= 2. GIỮA TRÊN CÙNG: DẢI TIN TỨC TOÀN SERVER =================
        private void DrawTopCenterServerNews()
        {
            float bannerW = Mathf.Clamp(Screen.width - 640f, 340f, 580f);
            float bannerH = 28f;
            float bannerX = (Screen.width - bannerW) / 2f;
            float bannerY = 14f;

            Rect bannerRect = new Rect(bannerX, bannerY, bannerW, bannerH);
            DrawDoubleGoldFrame(bannerRect);

            if (skillAimingController != null && skillAimingController.IsAiming)
            {
                serverNewsStyle.normal.textColor = Color.cyan;
                GUI.Label(bannerRect, "🎯 [ĐANG NGẮM]: Chuột Trái né đòn · Bấm [1] phóng chiêu · Chuột Phải hủy", serverNewsStyle);
            }
            else
            {
                serverNewsStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
                GUI.Label(bannerRect, "📢 Tin Tức: SquiRRels Studio · Alpha Slice · Friendly Fire 100% · Lệnh /trade", serverNewsStyle);
            }
        }

        // ================= 3. GÓC PHẢI TRÊN: MINIMAP + NÚT CHAT TRÒN =================
        private void DrawTopRightMinimapAndChat()
        {
            float mapSize = 98f;
            float mapX = Screen.width - mapSize - 16;
            float mapY = 14;
            Rect mapRect = new Rect(mapX, mapY, mapSize, mapSize);

            // Nền Radar Minimap
            GUI.DrawTexture(mapRect, radarBgTex);
            DrawDoubleGoldFrame(mapRect);

            // Tọa độ radar & chấm
            float cx = mapX + mapSize / 2f;
            float cy = mapY + mapSize / 2f;
            GUI.color = new Color(0.2f, 0.35f, 0.5f, 0.4f);
            GUI.DrawTexture(new Rect(mapX, cy, mapSize, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx, mapY, 1, mapSize), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Chấm Player Vàng
            GUI.color = new Color(1f, 0.85f, 0.2f);
            GUI.DrawTexture(new Rect(cx - 3, cy - 3, 6, 6), Texture2D.whiteTexture);

            // Chấm Quái Đỏ
            GUI.color = new Color(0.95f, 0.25f, 0.25f);
            GUI.DrawTexture(new Rect(cx + 26, cy - 4, 5, 5), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx + 36, cy + 18, 5, 5), Texture2D.whiteTexture);

            // Chấm Safe Guard Xanh
            GUI.color = new Color(0.25f, 0.85f, 0.35f);
            GUI.DrawTexture(new Rect(cx - 32, cy + 10, 5, 5), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Nhãn tên khu vực & tọa độ
            Vector2 pPos = playerMovement != null ? (Vector2)playerMovement.transform.position : Vector2.zero;
            string zoneName = (pPos.x <= -5.5f) ? "Làng Tân Thủ (An Toàn)" : "Rừng Rậm (PvP 100%)";
            smallWhiteStyle.fontSize = 9;
            smallWhiteStyle.normal.textColor = (pPos.x <= -5.5f) ? new Color(0.4f, 0.95f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            GUI.Label(new Rect(mapX, mapY + mapSize - 16, mapSize, 14), $"{zoneName} ({pPos.x:F0},{pPos.y:F0})", smallWhiteStyle);
            smallWhiteStyle.fontSize = 10;
            smallWhiteStyle.normal.textColor = Color.white;

            // Nút Trò Chuyện Tròn [💬] bên trái Minimap
            float chatSize = 30f;
            float chatX = mapX - chatSize - 8f;
            float chatY = mapY + 4f;
            Rect chatRect = new Rect(chatX, chatY, chatSize, chatSize);

            DrawDoubleGoldFrame(chatRect);
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(chatRect, "💬", boldWhiteStyle);

            if (Event.current.type == EventType.MouseDown && chatRect.Contains(Event.current.mousePosition))
            {
                isChatOpen = !isChatOpen;
                Event.current.Use();
            }
        }

        // ================= 4. MÉP TRÁI: NÚT TAM GIÁC [>] MENU TRƯỢT (CHUẨN CROP 1) =================
        private void DrawMidLeftDrawerButton()
        {
            float btnW = 24f;
            float btnH = 46f;
            float btnX = 2f;
            float btnY = (Screen.height - btnH) / 2f;
            Rect triRect = new Rect(btnX, btnY, btnW, btnH);

            DrawDoubleGoldFrame(triRect);
            GUI.Label(triRect, isDrawerOpen ? "<" : ">", drawerBtnStyle);

            if (Event.current.type == EventType.MouseDown && triRect.Contains(Event.current.mousePosition))
            {
                isDrawerOpen = !isDrawerOpen;
                Event.current.Use();
            }
        }

        // ================= 5. DƯỚI CÙNG: 4 Ô CHIÊU + 1 Ô LƯỚT (DASH) CHUYÊN DỤNG =================
        private void DrawBottomCenterSkillBar()
        {
            float slotSize = 52f;
            float gap = 8f;
            // 4 Ô Chiêu thức + 1 Ô Lướt Dash = 5 Ô
            float totalW = (slotSize * 5) + (gap * 4);
            float startX = (Screen.width - totalW) / 2f;
            float startY = Screen.height - slotSize - 16f;

            // --- Ô 1: CHIÊU 1 CẦU MA THUẬT (ARCANE BOLT) ---
            Rect s1Rect = new Rect(startX, startY, slotSize, slotSize);
            bool isAiming = skillAimingController != null && skillAimingController.IsAiming;
            float s1Cd = skillAimingController != null ? skillAimingController.Skill1CooldownRemaining : 0f;
            bool s1Ready = s1Cd <= 0.05f;

            DrawDoubleGoldFrame(s1Rect);

            // Vẽ Icon Chiêu 1
            if (skill1Icon != null)
            {
                GUI.DrawTexture(new Rect(s1Rect.x + 3, s1Rect.y + 3, s1Rect.width - 6, s1Rect.height - 6), skill1Icon);
            }

            // Hiệu ứng đang Aiming: Viền Cyan phát sáng
            if (isAiming)
            {
                Color glow = Mathf.PingPong(Time.time * 6f, 1f) > 0.5f ? Color.cyan : Color.white;
                DrawOutline(s1Rect, glow, 2);
            }

            // Cooldown đếm ngược overlay như Crop 2
            if (!s1Ready)
            {
                GUI.DrawTexture(s1Rect, cdOverlayTex);
                GUI.Label(s1Rect, $"{s1Cd:F1}s", skillCdStyle);
            }

            // Badge phím [1] ở góc
            DrawKeybindBadge(new Rect(s1Rect.x + 3, s1Rect.y + 3, 16, 14), "1");

            // Xử lý Tooltip khi rê chuột vào Ô 1
            if (s1Rect.Contains(Event.current.mousePosition))
            {
                int intDmg = playerStats != null ? playerStats.GetSkill1TotalDamage(25) : 37;
                SetTooltip("Cầu Ma Thuật (Arcane Bolt) [Phím 1]",
                    $"• Tiêu hao: 15 Mana | Cooldown: 1.0s | Tầm bắn: 8m\n" +
                    $"• Cơ chế: Two-Tap Skillshot (Bấm [1] ngắm, bấm [1] phóng chiêu, Chuột phải hủy)\n" +
                    $"• Sát thương: 25 Cơ bản + {playerStats?.Intelligence ?? 12} (100% INT) = <color=#ffff00>{intDmg} Sát Thương Phép</color>\n" +
                    $"• Friendly Fire 100% khi ngoài Vùng An Toàn!",
                    new Vector2(s1Rect.x, s1Rect.y - 140));
            }

            // --- CÁC Ô 2, 3, 4: KHÓA LV.25, 50, 75 ---
            for (int i = 2; i <= 4; i++)
            {
                Rect slotRect = new Rect(startX + (i - 1) * (slotSize + gap), startY, slotSize, slotSize);
                DrawDoubleGoldFrame(slotRect);
                GUI.DrawTexture(slotRect, slotBgTex);
                DrawOutline(slotRect, new Color(0.35f, 0.4f, 0.5f, 0.5f), 1);

                int reqLv = i == 2 ? 25 : (i == 3 ? 50 : 75);
                smallWhiteStyle.normal.textColor = new Color(0.6f, 0.7f, 0.8f);
                GUI.Label(slotRect, $"🔒\nLv.{reqLv}", smallWhiteStyle);
                smallWhiteStyle.normal.textColor = Color.white;

                DrawKeybindBadge(new Rect(slotRect.x + 3, slotRect.y + 3, 16, 14), $"{i}");

                if (slotRect.Contains(Event.current.mousePosition))
                {
                    SetTooltip($"Kỹ Năng Cấp Cao [Phím {i}]",
                        $"• Yêu cầu: Đạt Cấp độ Lv.{reqLv} để mở khóa kỹ năng nâng cao của Hệ.\n" +
                        $"• Sẽ tự động kích hoạt khi nhân vật thăng cấp!",
                        new Vector2(slotRect.x, slotRect.y - 80));
                }
            }

            // --- Ô 5: KỸ NĂNG LƯỚT (DASH) CHUYÊN DỤNG PHÍM SHIFT / SPACE ---
            Rect dashRect = new Rect(startX + 4 * (slotSize + gap), startY, slotSize, slotSize);
            float dashCd = playerMovement != null ? playerMovement.DashCooldownRemaining : 0f;
            bool dashReady = dashCd <= 0.05f;

            DrawDoubleGoldFrame(dashRect);

            if (dashIcon != null)
            {
                GUI.DrawTexture(new Rect(dashRect.x + 3, dashRect.y + 3, dashRect.width - 6, dashRect.height - 6), dashIcon);
            }

            if (!dashReady)
            {
                GUI.DrawTexture(dashRect, cdOverlayTex);
                GUI.Label(dashRect, $"{dashCd:F1}s", skillCdStyle);
            }

            DrawKeybindBadge(new Rect(dashRect.x + 3, dashRect.y + 3, 30, 14), "SHIFT");

            if (dashRect.Contains(Event.current.mousePosition))
            {
                SetTooltip("Lướt Nhanh (Dash) [Shift / Space]",
                    "• Tiêu hao: 15 Mana | Cooldown: 3.5s\n" +
                    "• Tác dụng: Lướt nhanh 4.5m theo hướng con trỏ chuột né đòn.\n" +
                    "• Miễn nhiễm khống chế trong 0.2s khi đang lướt!",
                    new Vector2(dashRect.x - 80, dashRect.y - 100));
            }
        }

        private void DrawKeybindBadge(Rect r, string key)
        {
            GUI.color = new Color(0.08f, 0.12f, 0.20f, 0.9f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(r, new Color(0.75f, 0.6f, 0.25f, 0.8f), 1);
            smallWhiteStyle.fontSize = 9;
            GUI.Label(r, key, smallWhiteStyle);
            smallWhiteStyle.fontSize = 10;
        }

        // ================= 6. MENU TRƯỢT 5 TAB (DRAWER MODAL) =================
        private void DrawDrawerModal()
        {
            float modalW = 340f;
            float modalH = 360f;
            float modalX = 32f;
            float modalY = (Screen.height - modalH) / 2f;
            Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

            DrawDoubleGoldFrame(modalRect);

            // Tab Bar trên đỉnh
            float tabW = modalW / drawerTabs.Length;
            for (int i = 0; i < drawerTabs.Length; i++)
            {
                Rect tabRect = new Rect(modalX + i * tabW, modalY, tabW, 26f);
                bool isActive = (currentDrawerTab == i);
                GUI.color = isActive ? new Color(0.25f, 0.35f, 0.55f, 0.95f) : new Color(0.08f, 0.11f, 0.16f, 0.9f);
                GUI.DrawTexture(tabRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawOutline(tabRect, isActive ? new Color(1f, 0.85f, 0.35f) : new Color(0.3f, 0.4f, 0.5f, 0.5f), 1);

                smallWhiteStyle.fontSize = 9;
                smallWhiteStyle.normal.textColor = isActive ? new Color(1f, 0.9f, 0.4f) : Color.gray;
                GUI.Label(tabRect, drawerTabs[i], smallWhiteStyle);
                smallWhiteStyle.normal.textColor = Color.white;
                smallWhiteStyle.fontSize = 10;

                if (Event.current.type == EventType.MouseDown && tabRect.Contains(Event.current.mousePosition))
                {
                    currentDrawerTab = i;
                    Event.current.Use();
                }
            }

            // Nội dung theo Tab
            Rect contentRect = new Rect(modalX + 12, modalY + 34, modalW - 24, modalH - 44);
            switch (currentDrawerTab)
            {
                case 0: DrawTabEquipment(contentRect); break;
                case 1: DrawTabInventory(contentRect); break;
                case 2: DrawTabSkills(contentRect); break;
                case 3: DrawTabPets(contentRect); break;
                case 4: DrawTabSettings(contentRect); break;
            }
        }

        private void DrawTabEquipment(Rect r)
        {
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "TRANG BỊ NHÂN VẬT (7 Ô CHUẨN GDD)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            string[] eqSlots = { "1. Vũ Khí: Gậy Tập Sự", "2. Mũ: Mũ Vải Tân Thủ", "3. Áo: Áo Vải Thô", "4. Quần: Quần Vải Thô", "5. Giày: Giày Da Mềm", "6. Vòng Cổ: Dây Chuyền Bạc", "7. Nhẫn: Nhẫn Đồng Cổ" };
            for (int i = 0; i < eqSlots.Length; i++)
            {
                Rect slotR = new Rect(r.x, r.y + 24 + i * 36, r.width, 30);
                DrawDoubleGoldFrame(slotR, 1);
                GUI.Label(new Rect(slotR.x + 8, slotR.y + 6, slotR.width - 16, 20), $"🛡️ {eqSlots[i]}", boldWhiteStyle);
            }
        }

        private void DrawTabInventory(Rect r)
        {
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "HÀNH TRANG (CLICK ĐỂ DÙNG NHANH)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            // Ô 1: Bình HP
            Rect hpR = new Rect(r.x, r.y + 26, r.width, 42);
            DrawDoubleGoldFrame(hpR, 1);
            GUI.Label(new Rect(hpR.x + 8, hpR.y + 6, hpR.width - 90, 30), $"🧪 Bình Máu HP: Còn {playerStats?.HpPotionCount ?? 5} bình\n(Hồi +35 Máu tức thì)", boldWhiteStyle);
            if (GUI.Button(new Rect(hpR.x + hpR.width - 80, hpR.y + 8, 72, 26), "DÙNG [Q]"))
            {
                playerStats?.UseHpPotion();
            }

            // Ô 2: Bình Mana
            Rect manaR = new Rect(r.x, r.y + 74, r.width, 42);
            DrawDoubleGoldFrame(manaR, 1);
            GUI.Label(new Rect(manaR.x + 8, manaR.y + 6, manaR.width - 90, 30), $"🧪 Bình Mana: Còn {playerStats?.ManaPotionCount ?? 5} bình\n(Hồi +30 Mana tức thì)", boldWhiteStyle);
            if (GUI.Button(new Rect(manaR.x + manaR.width - 80, manaR.y + 8, 72, 26), "DÙNG [E]"))
            {
                playerStats?.UseManaPotion();
            }

            // Ô 3: Trứng Thú Cưng Sóc Chibi
            Rect eggR = new Rect(r.x, r.y + 122, r.width, 42);
            DrawDoubleGoldFrame(eggR, 1);
            GUI.Label(new Rect(eggR.x + 8, eggR.y + 6, eggR.width - 16, 30), "🥚 Trứng Thú Cưng Sóc (Đang mang theo)\nMiễn trừ xóa khi Chuyển Sinh (Rebirth)", boldWhiteStyle);
        }

        private void DrawTabSkills(Rect r)
        {
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "BẢNG KỸ NĂNG & CÔNG THỨC SÁT THƯƠNG", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            int intVal = playerStats?.Intelligence ?? 12;
            int total = playerStats != null ? playerStats.GetSkill1TotalDamage(25) : 37;

            GUI.Label(new Rect(r.x, r.y + 26, r.width, 240),
                $"⚡ <b>Chiêu 1: Cầu Ma Thuật (Two-Tap Skillshot)</b>\n" +
                $"   • Sát thương: 25 + 100% INT = 25 + {intVal} = <color=#ffff00>{total} Sát Thương Phép</color>\n" +
                $"   • Tiêu hao: 15 Mana | Cooldown: 1.0s | Tầm: 8m\n\n" +
                $"🏃 <b>Kỹ Năng: Lướt Nhanh (Dash)</b>\n" +
                $"   • Phím tắt: [Shift] hoặc [Space]\n" +
                $"   • Tiêu hao: 15 Mana | Cooldown: 3.5s\n" +
                $"   • Tác dụng: Lướt 4.5m né đòn, miễn nhiễm khống chế 0.2s.\n\n" +
                $"🔒 <b>Kỹ Năng Khóa:</b>\n" +
                $"   • Chiêu 2 (Lv.25) · Chiêu 3 (Lv.50) · Chiêu 4 (Lv.75)",
                tooltipDescStyle);
        }

        private void DrawTabPets(Rect r)
        {
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "QUẢN LÝ THÚ CƯNG (PET COMPANION)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(r.x, r.y + 24, r.width, 100),
                "🐿️ <b>Sóc Chibi SquiRRels Studio (Cấp 1)</b>\n" +
                "• Loại: Thú Cưng Đồng Hành (Vật phẩm đặc biệt).\n" +
                "• Tiêu chuẩn 3-KHÔNG: Không cưỡi, Không đánh, Không bị chọn làm mục tiêu.\n" +
                "• Đặc quyền: Không thể bị xóa hay reset khi Rebirth Chuyển Sinh!",
                tooltipDescStyle);

            // Nút Thả Pet bảo mật 1 phút
            if (!isReleasingPet)
            {
                if (GUI.Button(new Rect(r.x + 20, r.y + 130, r.width - 40, 32), "THẢ THÚ CƯNG ĐI (NHẬN VÀNG AN ỦI)"))
                {
                    isReleasingPet = true;
                    petReleaseCountdown = 60f;
                    petConfirmInput = "";
                }
            }
            else
            {
                petReleaseCountdown -= Time.deltaTime;
                GUI.Label(new Rect(r.x, r.y + 130, r.width, 40),
                    $"<color=#ff4444>Đang thả Pet! Vui lòng gõ: 'TÔI XÁC NHẬN'\nĐếm ngược: {Mathf.CeilToInt(petReleaseCountdown)} giây nữa mới có thể xóa!</color>", tooltipDescStyle);

                petConfirmInput = GUI.TextField(new Rect(r.x, r.y + 175, r.width - 80, 24), petConfirmInput);
                if (GUI.Button(new Rect(r.x + r.width - 75, r.y + 175, 75, 24), "HỦY BỎ"))
                {
                    isReleasingPet = false;
                }
            }
        }

        private void DrawTabSettings(Rect r)
        {
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "CÀI ĐẶT HỆ THỐNG & GIAO DỊCH", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            autoDeclineTrade = GUI.Toggle(new Rect(r.x, r.y + 30, r.width, 26), autoDeclineTrade, " 🛡️ Tự động TỪ CHỐI mọi yêu cầu Giao Dịch (/trade)");

            GUI.Label(new Rect(r.x, r.y + 65, r.width, 140),
                $"• Trạng thái /trade: {(autoDeclineTrade ? "<color=#ff5555>ĐANG KHÓA GIAO DỊCH</color>" : "<color=#55ff55>SẴN SÀNG GIAO DỊCH</color>")}\n" +
                $"• Giới hạn tốc độ: 2 phút / 1 người chơi.\n" +
                $"• Phạm vi: Chỉ giao dịch vật phẩm trong túi, không giao dịch Pet & trang bị đang mặc.", tooltipDescStyle);
        }

        // ================= 7. MODAL CHAT & LỆNH /TRADE =================
        private void DrawChatModal()
        {
            float chatW = 320f;
            float chatH = 220f;
            float chatX = Screen.width - chatW - 16;
            float chatY = 120f;
            Rect chatRect = new Rect(chatX, chatY, chatW, chatH);

            DrawDoubleGoldFrame(chatRect);

            // Tiêu đề
            boldWhiteStyle.fontSize = 11;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(chatRect.x + 8, chatRect.y + 6, chatRect.width - 40, 18), "💬 KÊNH TRÒ CHUYỆN & GIAO DỊCH", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            if (GUI.Button(new Rect(chatRect.x + chatRect.width - 26, chatRect.y + 4, 22, 20), "X"))
            {
                isChatOpen = false;
            }

            // Lịch sử chat
            GUI.Label(new Rect(chatRect.x + 8, chatRect.y + 26, chatRect.width - 16, 140), chatHistory, tooltipDescStyle);

            // Ô nhập chat
            chatInputText = GUI.TextField(new Rect(chatRect.x + 8, chatRect.y + chatH - 30, chatRect.width - 66, 24), chatInputText);
            if (GUI.Button(new Rect(chatRect.x + chatRect.width - 54, chatRect.y + chatH - 30, 46, 24), "GỬI"))
            {
                SendChatMessage(chatInputText);
            }
        }

        private void SendChatMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;

            if (msg.StartsWith("/trade"))
            {
                string[] parts = msg.Split(' ');
                string target = parts.Length > 1 ? parts[1] : "Player_Target";
                chatHistory += $"\n<color=#ffff55>[TRADE]</color> Đã gửi lời mời giao dịch đến {target}! (Cooldown 2 phút bắt đầu tính)";
            }
            else
            {
                chatHistory += $"\n<color=#55ffff>[Tôi]:</color> {msg}";
            }
            chatInputText = "";
        }

        // ================= 8. TOOLTIP HOVER POPUP =================
        private void SetTooltip(string title, string body, Vector2 pos)
        {
            activeTooltipTitle = title;
            activeTooltipBody = body;
            activeTooltipPos = pos;
            showTooltip = true;
        }

        private void DrawTooltipPopup()
        {
            float tipW = 280f;
            float tipH = 120f;
            Rect tipRect = new Rect(activeTooltipPos.x, activeTooltipPos.y, tipW, tipH);

            DrawDoubleGoldFrame(tipRect);

            GUI.Label(new Rect(tipRect.x + 10, tipRect.y + 8, tipW - 20, 20), activeTooltipTitle, tooltipTitleStyle);
            GUI.Label(new Rect(tipRect.x + 10, tipRect.y + 28, tipW - 20, tipH - 34), activeTooltipBody, tooltipDescStyle);
        }

        // ================= HÀM TIỆN ÍCH VẼ KHUNG ĐÔI VÀNG KIM CHUẨN CROP =================
        private void DrawDoubleGoldFrame(Rect r, int borderWidth = 1)
        {
            // Nền tối Slate Navy
            GUI.DrawTexture(r, panelBgTex);

            // Khung ngoài vàng kim (#cda240)
            DrawOutline(r, new Color(0.85f, 0.70f, 0.28f, 0.95f), borderWidth);

            // Viền tối bên trong 1 pixel tạo hiệu ứng chạm khắc kim loại
            if (r.width > 12 && r.height > 12)
            {
                Rect inner = new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2);
                DrawOutline(inner, new Color(0.12f, 0.18f, 0.28f, 0.8f), 1);
            }

            // Điểm nhấn 4 góc vàng sáng (Golden Corner Rivets)
            Color goldCorner = new Color(1f, 0.90f, 0.45f, 1f);
            GUI.color = goldCorner;
            GUI.DrawTexture(new Rect(r.x, r.y, 2, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y, 2, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, 2, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y + r.height - 2, 2, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;
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
