using System;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Giao Diện HUD Chuẩn MOBA & Gold-Slate Studio:
    /// - Góc Trái Trên: Bảng Thông Tin Nhân Vật (Avatar, Tên, Cấp 1, Hệ Hỏa, INT/STR/VIT/AGI, Máu Đỏ, Mana Xanh).
    /// - Giữa Màn Hình Trên: Dải Tin Tức Toàn Server (Server Announcement Banner & Aim Guide).
    /// - Góc Phải Trên: Minimap Động Thời Gian Thực (Chấm Player vàng chạy theo tọa độ, quái đỏ, NPC xanh, vạch ranh giới Safe Zone/PvP) + Nút Chat [💬].
    /// - Mép Trái: Nút Menu Trượt [>] mở Bảng 5 Tab (Trang Bị 7 ô, Túi Đồ, Kỹ Năng, Thú Cưng, Cài Đặt).
    /// - Dưới Cùng: 4 Ô CHIÊU MOBA ĐA DẠNG ĐƯỢC MỞ KHÓA TOÀN BỘ + 1 Ô LƯỚT (DASH), hiển thị ICON hình ảnh, COOLDOWN và TOOLTIP tự tính theo Trí Tuệ!
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
        private Texture2D skill2Icon;
        private Texture2D skill3Icon;
        private Texture2D skill4Icon;
        private Texture2D dashIcon;

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

        // Cache Monsters & NPC cho Minimap
        private MonsterTarget[] cachedMonsters;
        private VillageNPC cachedNPC;
        private float minimapRefreshTimer = 0f;

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
            if (skill2Icon == null) skill2Icon = Resources.Load<Texture2D>("icon_skill_meteor");
            if (skill3Icon == null) skill3Icon = Resources.Load<Texture2D>("icon_skill_flame_cone");
            if (skill4Icon == null) skill4Icon = Resources.Load<Texture2D>("icon_skill_divine_nova");
            if (dashIcon == null) dashIcon = Resources.Load<Texture2D>("icon_skill_dash");
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

        private void Update()
        {
            minimapRefreshTimer += Time.deltaTime;
            if (minimapRefreshTimer > 1.0f)
            {
                minimapRefreshTimer = 0f;
                cachedMonsters = FindObjectsByType<MonsterTarget>(FindObjectsSortMode.None);
                if (cachedNPC == null) cachedNPC = FindFirstObjectByType<VillageNPC>();
            }
        }

        private void OnGUI()
        {
            if (panelBgTex == null || hpFillTex == null || manaFillTex == null || radarBgTex == null || slotBgTex == null || cdOverlayTex == null)
            {
                CreateTextures();
            }
            if (skill1Icon == null || skill2Icon == null || skill3Icon == null || skill4Icon == null || dashIcon == null)
            {
                LoadIcons();
            }

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

        // ================= 1. GÓC TRÁI TRÊN: BẢNG THÔNG TIN NHÂN VẬT =================
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

            // Badge Level
            int lv = playerStats != null ? playerStats.Level : 1;
            Rect lvBadge = new Rect(av.x - 2, av.y + av.height - 12, 28, 14);
            GUI.color = new Color(0.75f, 0.55f, 0.15f, 0.95f);
            GUI.DrawTexture(lvBadge, Texture2D.whiteTexture);
            GUI.color = Color.white;
            smallWhiteStyle.fontSize = 9;
            GUI.Label(lvBadge, $"Lv.{lv}", smallWhiteStyle);
            smallWhiteStyle.fontSize = 10;

            // Nhãn Tên Nhân Vật + Hệ
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

            // Thuộc tính INT/STR/VIT/AGI
            int str = playerStats != null ? playerStats.Strength : 8;
            int intel = playerStats != null ? playerStats.Intelligence : 12;
            int vit = playerStats != null ? playerStats.Vitality : 10;
            int agi = playerStats != null ? playerStats.Agility : 9;
            GUI.Label(new Rect(panel.x + 58, panel.y + 59, 205, 14), $"INT: {intel} · STR: {str} · VIT: {vit} · AGI: {agi} | Q:x{playerStats?.HpPotionCount ?? 5} E:x{playerStats?.ManaPotionCount ?? 5}", statBadgeStyle);
        }

        private void DrawInteractiveBar(Rect r, int cur, int max, Texture2D fillTex, string text, Action onClick)
        {
            GUI.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            float fill = Mathf.Clamp01((float)cur / max);
            Rect fRect = new Rect(r.x, r.y, r.width * fill, r.height);
            GUI.DrawTexture(fRect, fillTex != null ? fillTex : Texture2D.whiteTexture);

            DrawOutline(r, new Color(0.35f, 0.45f, 0.6f, 0.5f), 1);
            GUI.Label(r, text, smallWhiteStyle);

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
                int s = skillAimingController.CurrentAimingSkill;
                string sName = (s == 1) ? "Cầu Ma Thuật [1] (Định hướng)" : (s == 2) ? "Thiên Thạch [2] (Thả vòng tròn đất)" : (s == 3) ? "Sóng Lửa [3] (Quét hình quạt)" : "Bộc Phá [4] (Nổ hào quang 360°)";
                serverNewsStyle.normal.textColor = Color.cyan;
                GUI.Label(bannerRect, $"🎯 [ĐANG NGẮM]: {sName} · Bấm [{s}] lần 2 để xả chiêu · Chuột Phải hủy", serverNewsStyle);
            }
            else
            {
                serverNewsStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
                GUI.Label(bannerRect, "📢 Tin Tức: SquiRRels Studio · Đã mở khóa trọn bộ 4 Chiêu MOBA · Lệnh /trade", serverNewsStyle);
            }
        }

        // ================= 3. GÓC PHẢI TRÊN: MINIMAP ĐỘNG THỜI GIAN THỰC =================
        private void DrawTopRightMinimapAndChat()
        {
            float mapSize = 104f;
            float mapX = Screen.width - mapSize - 16;
            float mapY = 14;
            Rect mapRect = new Rect(mapX, mapY, mapSize, mapSize);

            // Nền Radar
            GUI.DrawTexture(mapRect, radarBgTex != null ? radarBgTex : Texture2D.whiteTexture);
            DrawDoubleGoldFrame(mapRect);

            // World Coordinate Bounds: X [-22, 22], Y [-12, 12]
            float minWx = -22f, maxWx = 22f;
            float minWy = -12f, maxWy = 12f;

            // 1. Phân Vùng Safe Zone (X <= -6) trên Minimap (Nền vàng mờ bên trái)
            float safeDivideU = mapX + (( -6f - minWx ) / (maxWx - minWx)) * mapSize;
            Rect safeZoneMinimapRect = new Rect(mapX, mapY, safeDivideU - mapX, mapSize);
            GUI.color = new Color(0.85f, 0.75f, 0.2f, 0.12f);
            GUI.DrawTexture(safeZoneMinimapRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Vạch ranh giới làng và rừng (Đường kẻ đứt màu vàng kim)
            GUI.color = new Color(0.85f, 0.70f, 0.3f, 0.6f);
            GUI.DrawTexture(new Rect(safeDivideU, mapY, 1, mapSize), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. Chấm NPC Trưởng Làng (Màu Vàng Kim / Xanh Ngọc)
            if (cachedNPC != null)
            {
                Vector2 npcMapPos = WorldToMinimapPos(cachedNPC.transform.position, mapX, mapY, mapSize, minWx, maxWx, minWy, maxWy);
                GUI.color = new Color(1f, 0.88f, 0.25f);
                GUI.DrawTexture(new Rect(npcMapPos.x - 2, npcMapPos.y - 2, 5, 5), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // 3. Chấm Quái Vật Thời Gian Thực (Màu Đỏ)
            if (cachedMonsters != null)
            {
                foreach (var monster in cachedMonsters)
                {
                    if (monster != null && monster.gameObject.activeInHierarchy)
                    {
                        var h = monster.GetComponent<Health>();
                        if (h != null && !h.IsDead)
                        {
                            Vector2 mPos = WorldToMinimapPos(monster.transform.position, mapX, mapY, mapSize, minWx, maxWx, minWy, maxWy);
                            GUI.color = h.IsInSafeZone ? new Color(0.5f, 0.9f, 0.5f) : new Color(0.95f, 0.25f, 0.25f);
                            GUI.DrawTexture(new Rect(mPos.x - 2, mPos.y - 2, 5, 5), Texture2D.whiteTexture);
                        }
                    }
                }
                GUI.color = Color.white;
            }

            // 4. Chấm Người Chơi Thời Gian Thực (Màu Vàng Rực Rỡ Chạy Theo Tọa Độ)
            Vector2 pPos = playerMovement != null ? (Vector2)playerMovement.transform.position : Vector2.zero;
            Vector2 pMapPos = WorldToMinimapPos(pPos, mapX, mapY, mapSize, minWx, maxWx, minWy, maxWy);

            // Vòng hào quang quanh Player trên map
            GUI.color = new Color(1f, 0.85f, 0.2f, 0.4f);
            GUI.DrawTexture(new Rect(pMapPos.x - 4, pMapPos.y - 4, 9, 9), Texture2D.whiteTexture);
            // Chấm tâm Player
            GUI.color = new Color(1f, 0.95f, 0.4f, 1f);
            GUI.DrawTexture(new Rect(pMapPos.x - 2, pMapPos.y - 2, 5, 5), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 5. Tọa Độ & Tên Vùng Dưới Đáy Minimap
            string zoneName = (pPos.x <= -6.0f) ? "Làng Tân Thủ" : "Rừng Rậm PvP";
            smallWhiteStyle.fontSize = 9;
            smallWhiteStyle.normal.textColor = (pPos.x <= -6.0f) ? new Color(0.4f, 0.95f, 0.5f) : new Color(1f, 0.45f, 0.45f);
            GUI.Label(new Rect(mapX, mapY + mapSize - 16, mapSize, 14), $"{zoneName} ({pPos.x:F0},{pPos.y:F0})", smallWhiteStyle);
            smallWhiteStyle.fontSize = 10;
            smallWhiteStyle.normal.textColor = Color.white;

            // Nút Trò Chuyện Tròn [💬] Bên Cạnh Minimap
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

        private Vector2 WorldToMinimapPos(Vector3 worldPos, float mapX, float mapY, float mapSize, float minWx, float maxWx, float minWy, float maxWy)
        {
            float nx = Mathf.Clamp01((worldPos.x - minWx) / (maxWx - minWx));
            float ny = Mathf.Clamp01((worldPos.y - minWy) / (maxWy - minWy));

            float u = mapX + nx * mapSize;
            float v = mapY + (1f - ny) * mapSize; // Invert Y
            return new Vector2(u, v);
        }

        // ================= 4. MÉP TRÁI: NÚT TAM GIÁC [>] MENU TRƯỢT =================
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

        // ================= 5. DƯỚI CÙNG: 4 Ô CHIÊU MOBA MỞ KHÓA TOÀN BỘ + 1 Ô LƯỚT DASH =================
        private void DrawBottomCenterSkillBar()
        {
            float slotSize = 52f;
            float gap = 8f;
            // 4 Ô Chiêu thức MOBA + 1 Ô Lướt Dash = 5 Ô
            float totalW = (slotSize * 5) + (gap * 4);
            float startX = (Screen.width - totalW) / 2f;
            float startY = Screen.height - slotSize - 16f;

            int intel = playerStats != null ? playerStats.Intelligence : 12;

            // --- Ô 1: CHIÊU 1 CẦU MA THUẬT (LINE SKILLSHOT) ---
            DrawSkillSlot(1, startX, startY, slotSize, skill1Icon, "1",
                skillAimingController != null ? skillAimingController.Skill1CooldownRemaining : 0f,
                skillAimingController != null && skillAimingController.IsAiming && skillAimingController.CurrentAimingSkill == 1,
                "Cầu Ma Thuật [Phím 1] - Line Skillshot",
                $"• Cơ chế: Mũi tên định hướng (Two-Tap)\n" +
                $"• Tiêu hao: {skillAimingController?.Skill1ManaCost ?? 15} Mana | Cooldown: 1.0s\n" +
                $"• Sát thương: 25 + 100% INT = <color=#ffff00>{25 + intel} Sát Thương Phép</color>\n" +
                $"• Bắn đạn ma thuật bay thẳng xuyên quái vật.");

            // --- Ô 2: CHIÊU 2 MƯA THIÊN THẠCH (GROUND AOE CIRCLE) ---
            DrawSkillSlot(2, startX + (slotSize + gap), startY, slotSize, skill2Icon, "2",
                skillAimingController != null ? skillAimingController.Skill2CooldownRemaining : 0f,
                skillAimingController != null && skillAimingController.IsAiming && skillAimingController.CurrentAimingSkill == 2,
                "Mưa Thiên Thạch [Phím 2] - Ground AoE",
                $"• Cơ chế: Vòng tròn thả đất tại chuột (Tầm tối đa 7m)\n" +
                $"• Tiêu hao: {skillAimingController?.Skill2ManaCost ?? 25} Mana | Cooldown: 3.0s\n" +
                $"• Sát thương: 40 + 120% INT = <color=#ffff00>{40 + Mathf.RoundToInt(intel * 1.2f)} Sát Thương AoE</color>\n" +
                $"• Thả thiên thạch nổ diện rộng lên mọi mục tiêu trong vòng.");

            // --- Ô 3: CHIÊU 3 SÓNG LỬA HÌNH QUẠT (CONE SWEEP) ---
            DrawSkillSlot(3, startX + 2 * (slotSize + gap), startY, slotSize, skill3Icon, "3",
                skillAimingController != null ? skillAimingController.Skill3CooldownRemaining : 0f,
                skillAimingController != null && skillAimingController.IsAiming && skillAimingController.CurrentAimingSkill == 3,
                "Sóng Lửa Kiếm Khí [Phím 3] - Cone Sweep",
                $"• Cơ chế: Hình quạt quét 60 độ phía trước mặt (Tầm 5m)\n" +
                $"• Tiêu hao: {skillAimingController?.Skill3ManaCost ?? 20} Mana | Cooldown: 2.5s\n" +
                $"• Sát thương: 30 + 80% INT = <color=#ffff00>{30 + Mathf.RoundToInt(intel * 0.8f)} Sát Thương Lửa</color>\n" +
                $"• Quét luồng kiếm khí lửa thiêu đốt tất cả kẻ địch trước mặt.");

            // --- Ô 4: CHIÊU 4 BỘC PHÁ THẦN THÁNH (SELF NOVA ULTIMATE) ---
            DrawSkillSlot(4, startX + 3 * (slotSize + gap), startY, slotSize, skill4Icon, "4",
                skillAimingController != null ? skillAimingController.Skill4CooldownRemaining : 0f,
                skillAimingController != null && skillAimingController.IsAiming && skillAimingController.CurrentAimingSkill == 4,
                "Bộc Phá Thần Thánh [Phím 4] - Self Nova",
                $"• Cơ chế: Vòng hào quang bộc phá 360 độ quanh người (Bán kính 4.5m)\n" +
                $"• Tiêu hao: {skillAimingController?.Skill4ManaCost ?? 35} Mana | Cooldown: 5.0s\n" +
                $"• Sát thương: 60 + 150% INT = <color=#ffff00>{60 + Mathf.RoundToInt(intel * 1.5f)} Sát Thương Phép Cực Đại</color>\n" +
                $"• Nổ tung sóng thần thánh 360 độ quét sạch kẻ địch xung quanh!");

            // --- Ô 5: KỸ NĂNG LƯỚT (DASH) CHUYÊN DỤNG ---
            DrawSkillSlot(5, startX + 4 * (slotSize + gap), startY, slotSize, dashIcon, "SHIFT",
                playerMovement != null ? playerMovement.DashCooldownRemaining : 0f,
                false,
                "Lướt Nhanh (Dash) [Shift / Space]",
                "• Cơ chế: Lướt cơ động 360 độ theo con trỏ chuột\n" +
                "• Tiêu hao: 15 Mana | Cooldown: 3.5s\n" +
                "• Cự ly: Lướt 4.5m né tránh mọi đòn đánh tức thì!");
        }

        private void DrawSkillSlot(int slotIndex, float x, float y, float size, Texture2D icon, string keybind, float cdRemaining, bool isAimingThis, string tipTitle, string tipDesc)
        {
            Rect r = new Rect(x, y, size, size);
            DrawDoubleGoldFrame(r);

            // Icon Chiêu Thức
            if (icon != null)
            {
                GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), icon);
            }

            // Hiệu ứng đang Aiming: Viền Cyan nhấp nháy
            if (isAimingThis)
            {
                Color glow = Mathf.PingPong(Time.time * 6f, 1f) > 0.5f ? Color.cyan : Color.white;
                DrawOutline(r, glow, 2);
            }

            // Cooldown đếm ngược overlay
            if (cdRemaining > 0.05f)
            {
                GUI.DrawTexture(r, cdOverlayTex != null ? cdOverlayTex : Texture2D.whiteTexture);
                GUI.Label(r, $"{cdRemaining:F1}s", skillCdStyle);
            }

            // Badge phím tắt
            float badgeW = keybind.Length > 2 ? 34f : 16f;
            DrawKeybindBadge(new Rect(r.x + 3, r.y + 3, badgeW, 14), keybind);

            // Hover Tooltip
            if (r.Contains(Event.current.mousePosition))
            {
                SetTooltip(tipTitle, tipDesc, new Vector2(Mathf.Clamp(r.x - 50, 10, Screen.width - 300), r.y - 130));
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

        // ================= 6. MENU TRƯỢT 5 TAB =================
        private void DrawDrawerModal()
        {
            float modalW = 340f;
            float modalH = 360f;
            float modalX = 32f;
            float modalY = (Screen.height - modalH) / 2f;
            Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

            DrawDoubleGoldFrame(modalRect);

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

            Rect hpR = new Rect(r.x, r.y + 26, r.width, 42);
            DrawDoubleGoldFrame(hpR, 1);
            GUI.Label(new Rect(hpR.x + 8, hpR.y + 6, hpR.width - 90, 30), $"🧪 Bình Máu HP: Còn {playerStats?.HpPotionCount ?? 5} bình\n(Hồi +35 Máu tức thì)", boldWhiteStyle);
            if (GUI.Button(new Rect(hpR.x + hpR.width - 80, hpR.y + 8, 72, 26), "DÙNG [Q]"))
            {
                playerStats?.UseHpPotion();
            }

            Rect manaR = new Rect(r.x, r.y + 74, r.width, 42);
            DrawDoubleGoldFrame(manaR, 1);
            GUI.Label(new Rect(manaR.x + 8, manaR.y + 6, manaR.width - 90, 30), $"🧪 Bình Mana: Còn {playerStats?.ManaPotionCount ?? 5} bình\n(Hồi +30 Mana tức thì)", boldWhiteStyle);
            if (GUI.Button(new Rect(manaR.x + manaR.width - 80, manaR.y + 8, 72, 26), "DÙNG [E]"))
            {
                playerStats?.UseManaPotion();
            }

            Rect eggR = new Rect(r.x, r.y + 122, r.width, 42);
            DrawDoubleGoldFrame(eggR, 1);
            GUI.Label(new Rect(eggR.x + 8, eggR.y + 6, eggR.width - 16, 30), "🥚 Trứng Thú Cưng Sóc (Đang mang theo)\nMiễn trừ xóa khi Chuyển Sinh (Rebirth)", boldWhiteStyle);
        }

        private void DrawTabSkills(Rect r)
        {
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "BẢNG 4 CHIÊU MOBA ĐÃ MỞ KHÓA 100%", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            int intVal = playerStats?.Intelligence ?? 12;

            GUI.Label(new Rect(r.x, r.y + 24, r.width, 260),
                $"⚡ <b>[1] Cầu Ma Thuật (Line Skillshot)</b>: {25 + intVal} Sát Thương | 15 Mana | CD 1.0s\n\n" +
                $"☄️ <b>[2] Mưa Thiên Thạch (Ground AoE)</b>: {40 + Mathf.RoundToInt(intVal * 1.2f)} Sát Thương | 25 Mana | CD 3.0s\n\n" +
                $"🔥 <b>[3] Sóng Lửa (Cone 60°)</b>: {30 + Mathf.RoundToInt(intVal * 0.8f)} Sát Thương | 20 Mana | CD 2.5s\n\n" +
                $"🌟 <b>[4] Bộc Phá Thần Thánh (Nova 360°)</b>: {60 + Mathf.RoundToInt(intVal * 1.5f)} Sát Thương | 35 Mana | CD 5.0s\n\n" +
                $"🏃 <b>[Shift/Space] Lướt (Dash)</b>: Lướt 4.5m 360° | 15 Mana | CD 3.5s",
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

            boldWhiteStyle.fontSize = 11;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(chatRect.x + 8, chatRect.y + 6, chatRect.width - 40, 18), "💬 KÊNH TRÒ CHUYỆN & GIAO DỊCH", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            if (GUI.Button(new Rect(chatRect.x + chatRect.width - 26, chatRect.y + 4, 22, 20), "X"))
            {
                isChatOpen = false;
            }

            GUI.Label(new Rect(chatRect.x + 8, chatRect.y + 26, chatRect.width - 16, 140), chatHistory, tooltipDescStyle);

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
            GUI.DrawTexture(r, panelBgTex != null ? panelBgTex : Texture2D.whiteTexture);
            DrawOutline(r, new Color(0.85f, 0.70f, 0.28f, 0.95f), borderWidth);

            if (r.width > 12 && r.height > 12)
            {
                Rect inner = new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2);
                DrawOutline(inner, new Color(0.12f, 0.18f, 0.28f, 0.8f), 1);
            }

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
