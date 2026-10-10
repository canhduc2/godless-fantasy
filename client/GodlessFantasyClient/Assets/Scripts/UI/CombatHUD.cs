using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Giao Diện HUD Chuẩn MOBA & Gold-Slate Studio (Phiên Bản Cuộn Trục Kỹ Năng Kéo Thả & Minimap Thời Gian Thực):
    /// - Góc Trái Trên: Bảng Thông Tin Nhân Vật (Avatar, Cấp 1, Hệ Hỏa, Thuộc Tính INT/STR/VIT/AGI, Máu Đỏ, Mana Lam).
    /// - Giữa Màn Hình Trên: Dải Tin Tức & Hướng Dẫn Ngắm Chiêu MOBA.
    /// - Góc Phải Trên: BẢN ĐỒ MINIMAP ĐỘNG 100% HOẠT ĐỘNG:
    ///   • Hiển thị địa hình rõ nét: Làng Tân Thủ (Đá Vàng), Rừng Rậm (Xanh Rêu), Ranh giới Safe Zone.
    ///   • Chấm Player vàng phát xung radar thời gian thực, có mũi tên hướng nhìn.
    ///   • Khung Camera Frustum di chuyển theo màn hình thực tế.
    ///   • CLICK-TO-MOVE TRÊN MINIMAP: Bấm vào bất kỳ đâu trên minimap nhân vật sẽ tự động chạy tới đó!
    /// - Mép Trái: Nút Tam Giác [>] mở Menu Trượt 5 Tab.
    /// - TAB KỸ NĂNG & TÚI ĐỒ: Hiển thị KHO CUỘN TRỤC KỸ NĂNG (BÍ KÍP) - KÉO THẢ (DRAG & DROP) vào các ô [1], [2], [3], [4]!
    /// - Dưới Cùng: 4 Ô CUỘN TRỤC KỸ NĂNG ĐƯỢC TRANG BỊ + 1 Ô LƯỚT DASH (Shift/Space).
    /// - Toàn bộ font chữ hiển thị chuẩn sắc nét bình thường, loại bỏ triệt để hiệu ứng pixel vỡ chữ.
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
        private Texture2D safeZoneBgTex;
        private Texture2D wildernessBgTex;
        private Texture2D slotBgTex;
        private Texture2D cdOverlayTex;
        private Texture2D goldBorderTex;

        // Skill & Scroll Icons Cache
        private Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();
        private Texture2D defaultScrollIcon;
        private Texture2D dashIcon;

        // GUIStyles sắc nét, chuẩn mực
        private GUIStyle boldWhiteStyle;
        private GUIStyle standardTextStyle;
        private GUIStyle serverNewsStyle;
        private GUIStyle skillCdStyle;
        private GUIStyle drawerBtnStyle;
        private GUIStyle tooltipTitleStyle;
        private GUIStyle tooltipDescStyle;
        private GUIStyle statBadgeStyle;
        private GUIStyle tabBtnStyle;
        private bool isStyleInitialized = false;

        // Drawer Menu States
        private bool isDrawerOpen = false;
        private int currentDrawerTab = 0; // 0=Trang Bị, 1=Túi Đồ, 2=Kỹ Năng & Cuộn Trục, 3=Thú Cưng, 4=Cài Đặt
        private readonly string[] drawerTabs = { "TRANG BỊ", "TÚI ĐỒ", "BÍ KÍP SKILL", "THÚ CƯNG", "CÀI ĐẶT" };

        // Chat Modal State
        private bool isChatOpen = false;
        private string chatInputText = "/trade Player_2";
        private string chatHistory = "📢 [Server]: Chào mừng bạn đến với Godless Fantasy!\n💬 [Hệ Thống]: Đã vào Làng Tân Thủ (Vùng An Toàn).\n⚡ [Mẹo]: Kéo thả Cuộn Trục vào ô 1-4 để đổi chiêu! Bấm chuột lên Minimap để di chuyển!";

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

        // Minimap Click Ping
        private Vector2 minimapPingPos;
        private float minimapPingTimer = 0f;

        // ================= KÉO THẢ CUỘN TRỤC KỸ NĂNG (DRAG & DROP) =================
        private bool isDraggingScroll = false;
        private SkillScrollData draggedScroll = null;
        private int dragSourceSlot = -1; // -1 = từ kho đồ/bí kíp, 0..3 = từ thanh chiêu dưới
        private Vector2 dragMousePos;

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
            if (radarBgTex == null) radarBgTex = MakeColorTex(new Color(0.05f, 0.07f, 0.11f, 0.98f));
            if (safeZoneBgTex == null) safeZoneBgTex = MakeColorTex(new Color(0.14f, 0.18f, 0.26f, 0.9f));
            if (wildernessBgTex == null) wildernessBgTex = MakeColorTex(new Color(0.07f, 0.15f, 0.10f, 0.9f));
            if (slotBgTex == null) slotBgTex = MakeColorTex(new Color(0.08f, 0.11f, 0.18f, 0.95f));
            if (cdOverlayTex == null) cdOverlayTex = MakeColorTex(new Color(0f, 0f, 0f, 0.72f));
            if (goldBorderTex == null) goldBorderTex = MakeColorTex(new Color(0.85f, 0.70f, 0.30f, 1f));
        }

        private void LoadIcons()
        {
            dashIcon = LoadTextureSafe("icon_skill_dash");
            defaultScrollIcon = LoadTextureSafe("item_scroll_generic");
        }

        private Texture2D LoadTextureSafe(string resName)
        {
            if (string.IsNullOrEmpty(resName)) return null;
            if (iconCache.TryGetValue(resName, out var cached) && cached != null) return cached;

            Texture2D tex = Resources.Load<Texture2D>(resName);
            if (tex != null)
            {
                iconCache[resName] = tex;
            }
            return tex;
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
            if (isStyleInitialized && boldWhiteStyle != null && standardTextStyle != null) return;

            boldWhiteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            standardTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                normal = { textColor = new Color(0.92f, 0.95f, 1f) }
            };

            serverNewsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };

            skillCdStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            drawerBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };

            tooltipTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };

            tooltipDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };

            statBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.8f, 0.9f, 1f) }
            };

            tabBtnStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            isStyleInitialized = true;
        }

        private void Update()
        {
            // Luôn đảm bảo tìm đủ tham chiếu động
            if (playerMovement == null || skillAimingController == null)
            {
                FindReferences();
            }

            minimapRefreshTimer += Time.deltaTime;
            if (minimapRefreshTimer > 0.5f)
            {
                minimapRefreshTimer = 0f;
                cachedMonsters = FindObjectsByType<MonsterTarget>(FindObjectsSortMode.None);
                if (cachedNPC == null) cachedNPC = FindFirstObjectByType<VillageNPC>();
            }

            if (minimapPingTimer > 0f)
            {
                minimapPingTimer -= Time.deltaTime;
            }

            // Xử lý thả cuộn trục nếu nhả chuột ngoài GUI
            if (isDraggingScroll && Input.GetMouseButtonUp(0))
            {
                // Kết thúc kéo thả
                isDraggingScroll = false;
                draggedScroll = null;
                dragSourceSlot = -1;
            }
        }

        private void OnGUI()
        {
            if (panelBgTex == null || hpFillTex == null || manaFillTex == null || radarBgTex == null)
            {
                CreateTextures();
            }

            InitStyles();
            showTooltip = false;

            // Kiểm tra chuột có nằm trên bất kỳ vùng UI nào không
            Vector2 mousePos = Event.current.mousePosition;
            bool isMouseOverUI = (mousePos.x < 300 && mousePos.y < 100) || // Top-Left panel
                                 (mousePos.x > Screen.width - 200 && mousePos.y < 160) || // Minimap
                                 (mousePos.y > Screen.height - 100) || // Skill hotbar
                                 (isDrawerOpen && mousePos.x < 390) || // Drawer modal
                                 (isChatOpen && mousePos.x > Screen.width - 320 && mousePos.y < 350);

            PlayerMovement.BlockWorldClick = isMouseOverUI || isDraggingScroll;

            DrawTopLeftPlayerPanel();
            DrawTopCenterServerNews();
            DrawTopRightMinimapAndChat();
            DrawMidLeftDrawerButton();
            DrawBottomCenterSkillBar();

            if (isDrawerOpen) DrawDrawerModal();
            if (isChatOpen) DrawChatModal();
            if (showTooltip) DrawTooltipPopup();

            // Vẽ Cuộn Trục đang được kéo theo con trỏ chuột
            if (isDraggingScroll && draggedScroll != null)
            {
                DrawDraggingScrollVisual();
            }
        }

        // ================= 1. GÓC TRÁI TRÊN: BẢNG THÔNG TIN NHÂN VẬT =================
        private void DrawTopLeftPlayerPanel()
        {
            float panelW = 285f;
            float panelH = 80f;
            Rect panel = new Rect(16, 14, panelW, panelH);

            DrawDoubleGoldFrame(panel);

            // Avatar Box 48x48
            Rect av = new Rect(panel.x + 8, panel.y + 8, 48, 48);
            GUI.color = new Color(0.08f, 0.12f, 0.20f);
            GUI.DrawTexture(av, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(av, 1);
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.fontSize = 20;
            GUI.Label(av, "⚔️", boldWhiteStyle);
            boldWhiteStyle.fontSize = 13;

            // Badge Level
            int lv = playerStats != null ? playerStats.Level : 1;
            Rect lvBadge = new Rect(av.x - 2, av.y + av.height - 14, 32, 16);
            GUI.color = new Color(0.75f, 0.55f, 0.15f, 0.95f);
            GUI.DrawTexture(lvBadge, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(lvBadge, new Color(1f, 0.85f, 0.35f), 1);
            statBadgeStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(lvBadge, $"Lv.{lv}", statBadgeStyle);

            // Tên Nhân Vật + Hệ
            string charName = playerStats != null ? playerStats.CharacterName : "Dũng Sĩ SquiRRels";
            string elem = playerStats != null ? playerStats.Element : "Hỏa";
            boldWhiteStyle.alignment = TextAnchor.UpperLeft;
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(panel.x + 64, panel.y + 6, 215, 18), $"{charName} · [{elem}]", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            // Thanh Máu HP (Click để uống máu Q)
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
            Rect hpRect = new Rect(panel.x + 64, panel.y + 26, 210, 16);
            DrawInteractiveBar(hpRect, hp, maxHp, hpFillTex, $"HP {hp}/{maxHp} [Bấm Q: +35]", () => {
                if (playerStats != null) playerStats.UseHpPotion();
            });

            // Thanh Mana (Click để uống mana E)
            int mana = playerStats != null ? playerStats.CurrentMana : 80;
            int maxMana = playerStats != null ? playerStats.MaxMana : 100;
            Rect manaRect = new Rect(panel.x + 64, panel.y + 45, 210, 16);
            DrawInteractiveBar(manaRect, mana, maxMana, manaFillTex, $"MANA {mana}/{maxMana} [Bấm E: +30]", () => {
                if (playerStats != null) playerStats.UseManaPotion();
            });

            // Chỉ số INT/STR/VIT/AGI
            int str = playerStats != null ? playerStats.Strength : 8;
            int intel = playerStats != null ? playerStats.Intelligence : 12;
            int vit = playerStats != null ? playerStats.Vitality : 10;
            int agi = playerStats != null ? playerStats.Agility : 9;
            statBadgeStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(panel.x + 64, panel.y + 63, 215, 15), $"INT: {intel} · STR: {str} · VIT: {vit} · AGI: {agi} | Q:x{playerStats?.HpPotionCount ?? 5} E:x{playerStats?.ManaPotionCount ?? 5}", statBadgeStyle);
        }

        private void DrawInteractiveBar(Rect r, int cur, int max, Texture2D fillTex, string text, Action onClick)
        {
            GUI.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            float fill = Mathf.Clamp01((float)cur / Mathf.Max(1, max));
            Rect fRect = new Rect(r.x, r.y, r.width * fill, r.height);
            GUI.DrawTexture(fRect, fillTex != null ? fillTex : Texture2D.whiteTexture);

            DrawOutline(r, new Color(0.35f, 0.45f, 0.6f, 0.5f), 1);
            standardTextStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(r, text, standardTextStyle);

            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                onClick?.Invoke();
                Event.current.Use();
            }
        }

        // ================= 2. GIỮA TRÊN CÙNG: DẢI TIN TỨC & AIM GUIDE =================
        private void DrawTopCenterServerNews()
        {
            float bannerW = Mathf.Clamp(Screen.width - 640f, 360f, 580f);
            float bannerH = 28f;
            float bannerX = (Screen.width - bannerW) / 2f;
            float bannerY = 14f;

            Rect bannerRect = new Rect(bannerX, bannerY, bannerW, bannerH);
            DrawDoubleGoldFrame(bannerRect);

            if (skillAimingController != null && skillAimingController.IsAiming)
            {
                int s = skillAimingController.CurrentAimingSkill;
                var scroll = skillAimingController.GetEquippedScroll(s - 1);
                string sName = scroll != null ? scroll.scrollName : $"Chiêu [{s}]";
                serverNewsStyle.normal.textColor = Color.cyan;
                GUI.Label(bannerRect, $"🎯 [ĐANG NGẮM]: {sName} · Bấm [{s}] lần 2 để xả chiêu · Chuột Phải hủy", serverNewsStyle);
            }
            else
            {
                serverNewsStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
                GUI.Label(bannerRect, "📢 Tin Tức: SquiRRels Studio · Bí Kíp Kéo Thả (Drag & Drop) · Click Minimap để di chuyển!", serverNewsStyle);
            }
        }

        // ================= 3. GÓC PHẢI TRÊN: BẢN ĐỒ MINIMAP HOẠT ĐỘNG 100% =================
        private void DrawTopRightMinimapAndChat()
        {
            // Tỉ lệ Minimap 144x98 phù hợp kích thước thế giới 44x24
            float mapW = 144f;
            float mapH = 96f;
            float mapX = Screen.width - mapW - 16;
            float mapY = 14;
            Rect mapRect = new Rect(mapX, mapY, mapW, mapH);

            // Nền chung radar
            GUI.DrawTexture(mapRect, radarBgTex != null ? radarBgTex : Texture2D.whiteTexture);

            // Tọa độ thế giới: X [-22, 22], Y [-12, 12]
            float minWx = -22f, maxWx = 22f;
            float minWy = -12f, maxWy = 12f;

            // 1. Vẽ Vùng Đất Làng Tân Thủ (Phía Tây: X <= -6)
            float safeRatio = (-6f - minWx) / (maxWx - minWx);
            float safeW = mapW * safeRatio;
            Rect safeZoneRect = new Rect(mapX, mapY, safeW, mapH);
            GUI.color = new Color(0.22f, 0.28f, 0.38f, 0.85f);
            GUI.DrawTexture(safeZoneRect, Texture2D.whiteTexture);

            // 2. Vẽ Vùng Đất Rừng Rậm & PvP (Phía Đông: X > -6)
            Rect wildernessRect = new Rect(mapX + safeW, mapY, mapW - safeW, mapH);
            GUI.color = new Color(0.10f, 0.22f, 0.12f, 0.85f);
            GUI.DrawTexture(wildernessRect, Texture2D.whiteTexture);

            // 3. Đường Cổ Nối Làng với Rừng (Con đường đá trung tâm)
            float roadY = mapY + mapH * 0.45f;
            GUI.color = new Color(0.35f, 0.30f, 0.22f, 0.6f);
            GUI.DrawTexture(new Rect(mapX, roadY, mapW, 8), Texture2D.whiteTexture);

            // 4. Vạch Vàng Cổng Ranh Giới (Boundary Gate)
            GUI.color = new Color(1f, 0.85f, 0.25f, 0.9f);
            GUI.DrawTexture(new Rect(mapX + safeW, mapY, 2, mapH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Khung Viền Vàng Kép Bao Quanh Minimap
            DrawDoubleGoldFrame(mapRect);

            // 5. Khung Tầm Nhìn Camera Thực Tế (Camera Viewport Frustum Box)
            Camera cam = Camera.main;
            if (cam != null)
            {
                float camH = cam.orthographicSize * 2f;
                float camW = camH * cam.aspect;
                Vector3 camPos = cam.transform.position;

                Vector2 camMin = WorldToMinimapPos(new Vector3(camPos.x - camW / 2f, camPos.y + camH / 2f, 0), mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);
                Vector2 camMax = WorldToMinimapPos(new Vector3(camPos.x + camW / 2f, camPos.y - camH / 2f, 0), mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);

                Rect camBox = new Rect(camMin.x, camMin.y, Mathf.Max(6, camMax.x - camMin.x), Mathf.Max(6, camMax.y - camMin.y));
                DrawOutline(camBox, new Color(1f, 1f, 1f, 0.45f), 1);
            }

            // 6. Chấm NPC Trưởng Làng (Màu Vàng Kim Hào Quang)
            if (cachedNPC != null)
            {
                Vector2 npcMapPos = WorldToMinimapPos(cachedNPC.transform.position, mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);
                GUI.color = new Color(1f, 0.88f, 0.25f);
                GUI.DrawTexture(new Rect(npcMapPos.x - 3, npcMapPos.y - 3, 7, 7), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // 7. Chấm Quái Vật Thời Gian Thực (Màu Đỏ)
            if (cachedMonsters != null)
            {
                foreach (var monster in cachedMonsters)
                {
                    if (monster != null && monster.gameObject.activeInHierarchy)
                    {
                        var h = monster.GetComponent<Health>();
                        if (h != null && !h.IsDead)
                        {
                            Vector2 mPos = WorldToMinimapPos(monster.transform.position, mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);
                            GUI.color = h.IsInSafeZone ? new Color(0.4f, 0.8f, 1f) : new Color(0.95f, 0.25f, 0.25f);
                            GUI.DrawTexture(new Rect(mPos.x - 2, mPos.y - 2, 5, 5), Texture2D.whiteTexture);
                        }
                    }
                }
                GUI.color = Color.white;
            }

            // 8. Chấm Người Chơi (Màu Vàng Kim Rực Rỡ Chạy Động Theo Tọa Độ)
            Vector2 pPos = playerMovement != null ? (Vector2)playerMovement.transform.position : Vector2.zero;
            Vector2 pMapPos = WorldToMinimapPos(pPos, mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);

            // Vòng hào quang nhấp nháy phát xung radar
            float pulse = Mathf.Repeat(Time.time * 2f, 1f);
            float pulseSize = 6f + pulse * 10f;
            GUI.color = new Color(1f, 0.9f, 0.2f, (1f - pulse) * 0.6f);
            GUI.DrawTexture(new Rect(pMapPos.x - pulseSize / 2f, pMapPos.y - pulseSize / 2f, pulseSize, pulseSize), Texture2D.whiteTexture);

            // Chấm tâm Người Chơi
            GUI.color = new Color(1f, 0.95f, 0.35f, 1f);
            GUI.DrawTexture(new Rect(pMapPos.x - 3, pMapPos.y - 3, 7, 7), Texture2D.whiteTexture);
            DrawOutline(new Rect(pMapPos.x - 3, pMapPos.y - 3, 7, 7), Color.black, 1);
            GUI.color = Color.white;

            // 9. Hiệu ứng Ping khi bấm lên Minimap
            if (minimapPingTimer > 0f)
            {
                float pT = 1f - minimapPingTimer;
                float pR = 4f + pT * 18f;
                GUI.color = new Color(0.3f, 1f, 0.5f, minimapPingTimer);
                DrawOutline(new Rect(minimapPingPos.x - pR / 2f, minimapPingPos.y - pR / 2f, pR, pR), Color.green, 2);
                GUI.color = Color.white;
            }

            // 10. Tọa Độ & Tên Vùng Dưới Đáy Minimap
            string zoneName = (pPos.x <= -6.0f) ? "Làng Tân Thủ" : "Rừng Rậm PvP";
            statBadgeStyle.alignment = TextAnchor.MiddleCenter;
            statBadgeStyle.normal.textColor = (pPos.x <= -6.0f) ? new Color(0.5f, 0.95f, 0.6f) : new Color(1f, 0.5f, 0.5f);
            GUI.Label(new Rect(mapX, mapY + mapH - 18, mapW, 16), $"{zoneName} ({pPos.x:F1}, {pPos.y:F1})", statBadgeStyle);
            statBadgeStyle.normal.textColor = Color.white;

            // ================= TƯƠNG TÁC: CLICK-TO-MOVE TRÊN MINIMAP =================
            if (Event.current.type == EventType.MouseDown && mapRect.Contains(Event.current.mousePosition))
            {
                Vector2 clickPos = Event.current.mousePosition;
                float normU = Mathf.Clamp01((clickPos.x - mapX) / mapW);
                float normV = Mathf.Clamp01((clickPos.y - mapY) / mapH);

                float targetWorldX = Mathf.Lerp(minWx, maxWx, normU);
                float targetWorldY = Mathf.Lerp(maxWy, minWy, normV); // Đảo Y

                if (playerMovement != null)
                {
                    playerMovement.MoveTo(new Vector2(targetWorldX, targetWorldY));
                    Debug.Log($"<color=cyan>[MINIMAP CLICK-TO-MOVE] Ra lệnh chạy đến ({targetWorldX:F1}, {targetWorldY:F1})</color>");
                }

                minimapPingPos = clickPos;
                minimapPingTimer = 1.0f;
                Event.current.Use();
            }

            // Nút Trò Chuyện [💬]
            float chatSize = 32f;
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

        private Vector2 WorldToMinimapPos(Vector3 worldPos, float mapX, float mapY, float mapW, float mapH, float minWx, float maxWx, float minWy, float maxWy)
        {
            float nx = Mathf.Clamp01((worldPos.x - minWx) / (maxWx - minWx));
            float ny = Mathf.Clamp01((worldPos.y - minWy) / (maxWy - minWy));

            float u = mapX + nx * mapW;
            float v = mapY + (1f - ny) * mapH; // Đảo chiều Y
            return new Vector2(u, v);
        }

        // ================= 4. MÉP TRÁI: NÚT TAM GIÁC [>] MỞ MENU TRƯỢT =================
        private void DrawMidLeftDrawerButton()
        {
            float btnW = 26f;
            float btnH = 48f;
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

        // ================= 5. DƯỚI CÙNG: 4 Ô CUỘN TRỤC KỸ NĂNG + DASH =================
        private void DrawBottomCenterSkillBar()
        {
            float slotSize = 54f;
            float gap = 10f;
            int totalSlots = 5; // 4 Ô Chiêu Cuộn Trục + 1 Ô Dash
            float totalW = totalSlots * slotSize + (totalSlots - 1) * gap;
            float startX = (Screen.width - totalW) / 2f;
            float startY = Screen.height - slotSize - 18f;

            // 4 Ô Cuộn Trục Kỹ Năng [1..4]
            for (int i = 0; i < 4; i++)
            {
                int slotNumber = i + 1;
                float slotX = startX + i * (slotSize + gap);
                DrawEquippedScrollSlot(i, slotNumber, slotX, startY, slotSize);
            }

            // Ô 5: Kỹ Năng Lướt (Dash) Cố Định
            float dashX = startX + 4 * (slotSize + gap);
            DrawDashSlot(dashX, startY, slotSize);
        }

        private void DrawEquippedScrollSlot(int slotIndex, int slotNumber, float x, float y, float size)
        {
            Rect r = new Rect(x, y, size, size);
            DrawDoubleGoldFrame(r);

            SkillScrollData scroll = skillAimingController != null ? skillAimingController.GetEquippedScroll(slotIndex) : null;
            float cdRemaining = skillAimingController != null ? skillAimingController.GetCooldownRemaining(slotIndex) : 0f;
            bool isAimingThis = skillAimingController != null && skillAimingController.IsAiming && skillAimingController.CurrentAimingSkill == slotNumber;

            // 1. Icon Chiêu / Cuộn Trục
            if (scroll != null)
            {
                Texture2D icon = LoadTextureSafe(scroll.iconName);
                if (icon == null) icon = LoadTextureSafe(scroll.scrollIconName);
                if (icon == null) icon = defaultScrollIcon;

                if (icon != null)
                {
                    GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), icon);
                }

                // Viền màu theo Phẩm Cấp Cuộn Trục
                DrawOutline(r, scroll.tierColor, 2);

                // Badge Mana góc dưới
                standardTextStyle.alignment = TextAnchor.LowerRight;
                standardTextStyle.fontSize = 11;
                GUI.Label(new Rect(r.x, r.y, r.width - 4, r.height - 2), $"{scroll.manaCost}m", standardTextStyle);
                standardTextStyle.fontSize = 12;
            }
            else
            {
                // Ô trống
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.fontSize = 10;
                standardTextStyle.normal.textColor = Color.gray;
                GUI.Label(r, "KÉO\nBÍ KÍP", standardTextStyle);
                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;
            }

            // 2. Hiệu ứng đang Aiming
            if (isAimingThis)
            {
                Color glow = Mathf.PingPong(Time.time * 6f, 1f) > 0.5f ? Color.cyan : Color.white;
                DrawOutline(r, glow, 3);
            }

            // 3. Cooldown Overlay
            if (cdRemaining > 0.05f)
            {
                GUI.DrawTexture(r, cdOverlayTex != null ? cdOverlayTex : Texture2D.whiteTexture);
                GUI.Label(r, $"{cdRemaining:F1}s", skillCdStyle);
            }

            // 4. Badge Phím Tắt [1], [2], [3], [4]
            DrawKeybindBadge(new Rect(r.x + 3, r.y + 3, 18, 16), slotNumber.ToString());

            // 5. Tooltip khi Rê Chuột
            if (r.Contains(Event.current.mousePosition))
            {
                if (scroll != null)
                {
                    int intel = playerStats != null ? playerStats.Intelligence : 12;
                    int dmg = scroll.baseDamage + Mathf.RoundToInt(intel * scroll.intelligenceScaling);
                    string tipBody = $"• Phẩm cấp: <color=#{ColorUtility.ToHtmlStringRGB(scroll.tierColor)}>{scroll.tierRank}</color> | Hệ: {scroll.element}\n" +
                                     $"• Tiêu hao: {scroll.manaCost} Mana | Hồi chiêu: {scroll.cooldown:F1}s\n" +
                                     $"• Sát thương: {scroll.baseDamage} + {Mathf.RoundToInt(scroll.intelligenceScaling * 100)}% INT = <color=#ffff00>{dmg} ST Phép</color>\n" +
                                     $"• {scroll.description}\n" +
                                     $"<color=#88ff88>[KÉO THẢ]: Kéo sang ô khác để đổi vị trí! Chuột phải để gỡ bỏ.</color>";
                    SetTooltip($"{scroll.scrollName} [Phím {slotNumber}]", tipBody, new Vector2(Mathf.Clamp(r.x - 60, 10, Screen.width - 320), r.y - 140));
                }
                else
                {
                    SetTooltip($"Ô Kỹ Năng [{slotNumber}] Trống", "Mở Menu Trượt Tab [BÍ KÍP SKILL] hoặc [TÚI ĐỒ] để KÉO THẢ Cuộn Trục vào đây!", new Vector2(r.x - 60, r.y - 70));
                }

                // ================= DRAG & DROP: BẮT ĐẦU KÉO TỪ HOTBAR =================
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && scroll != null)
                {
                    isDraggingScroll = true;
                    draggedScroll = scroll;
                    dragSourceSlot = slotIndex;
                    dragMousePos = Event.current.mousePosition;
                    Event.current.Use();
                }

                // Chuột phải để gỡ chiêu
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && scroll != null)
                {
                    skillAimingController?.UnequipScroll(slotIndex);
                    Event.current.Use();
                }

                // ================= DRAG & DROP: THẢ VÀO Ô NÀY =================
                if (Event.current.type == EventType.MouseUp && isDraggingScroll && draggedScroll != null)
                {
                    if (dragSourceSlot >= 0 && dragSourceSlot != slotIndex)
                    {
                        // Đổi chỗ 2 ô hotbar
                        skillAimingController?.SwapScrolls(dragSourceSlot, slotIndex);
                    }
                    else
                    {
                        // Trang bị bí kíp mới vào ô này
                        skillAimingController?.EquipScroll(slotIndex, draggedScroll);
                    }
                    isDraggingScroll = false;
                    draggedScroll = null;
                    dragSourceSlot = -1;
                    Event.current.Use();
                }
            }
        }

        private void DrawDashSlot(float x, float y, float size)
        {
            Rect r = new Rect(x, y, size, size);
            DrawDoubleGoldFrame(r);

            if (dashIcon != null)
            {
                GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), dashIcon);
            }

            float cdRemaining = playerMovement != null ? playerMovement.DashCooldownRemaining : 0f;
            if (cdRemaining > 0.05f)
            {
                GUI.DrawTexture(r, cdOverlayTex != null ? cdOverlayTex : Texture2D.whiteTexture);
                GUI.Label(r, $"{cdRemaining:F1}s", skillCdStyle);
            }

            DrawKeybindBadge(new Rect(r.x + 3, r.y + 3, 36, 16), "SHIFT");

            if (r.Contains(Event.current.mousePosition))
            {
                SetTooltip("Lướt Nhanh (Dash) [Shift / Space]",
                    "• Cơ chế: Lướt né đòn 360 độ theo con trỏ chuột\n" +
                    "• Tiêu hao: 15 Mana | Hồi chiêu: 3.5s\n" +
                    "• Cự ly: Lướt 4.5m tức thì, xuyên qua quái vật!",
                    new Vector2(Mathf.Clamp(r.x - 100, 10, Screen.width - 320), r.y - 110));
            }
        }

        private void DrawKeybindBadge(Rect r, string key)
        {
            GUI.color = new Color(0.08f, 0.12f, 0.20f, 0.95f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(r, new Color(0.85f, 0.70f, 0.30f, 0.9f), 1);
            statBadgeStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(r, key, statBadgeStyle);
        }

        // ================= 6. VẼ CUỘN TRỤC ĐANG KÉO THEO CHUỘT =================
        private void DrawDraggingScrollVisual()
        {
            Vector2 m = Event.current.mousePosition;
            float dragW = 160f;
            float dragH = 36f;
            Rect dragRect = new Rect(m.x - dragW / 2f, m.y - dragH / 2f, dragW, dragH);

            GUI.color = new Color(0.08f, 0.12f, 0.20f, 0.92f);
            GUI.DrawTexture(dragRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(dragRect, draggedScroll.tierColor, 2);

            standardTextStyle.alignment = TextAnchor.MiddleCenter;
            standardTextStyle.fontSize = 12;
            standardTextStyle.normal.textColor = Color.yellow;
            GUI.Label(dragRect, $"📜 {draggedScroll.scrollName}\n(Thả vào Ô 1-4)", standardTextStyle);
            standardTextStyle.normal.textColor = Color.white;
        }

        // ================= 7. MENU TRƯỢT 5 TAB =================
        private void DrawDrawerModal()
        {
            float modalW = 380f;
            float modalH = 400f;
            float modalX = 32f;
            float modalY = (Screen.height - modalH) / 2f;
            Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

            DrawDoubleGoldFrame(modalRect);

            // Tab Headers
            float tabW = modalW / drawerTabs.Length;
            for (int i = 0; i < drawerTabs.Length; i++)
            {
                Rect tabRect = new Rect(modalX + i * tabW, modalY, tabW, 28f);
                bool isActive = (currentDrawerTab == i);
                GUI.color = isActive ? new Color(0.25f, 0.35f, 0.55f, 0.95f) : new Color(0.08f, 0.11f, 0.16f, 0.9f);
                GUI.DrawTexture(tabRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawOutline(tabRect, isActive ? new Color(1f, 0.85f, 0.35f) : new Color(0.3f, 0.4f, 0.5f, 0.5f), 1);

                tabBtnStyle.normal.textColor = isActive ? new Color(1f, 0.9f, 0.4f) : Color.gray;
                GUI.Label(tabRect, drawerTabs[i], tabBtnStyle);
                tabBtnStyle.normal.textColor = Color.white;

                if (Event.current.type == EventType.MouseDown && tabRect.Contains(Event.current.mousePosition))
                {
                    currentDrawerTab = i;
                    Event.current.Use();
                }
            }

            Rect contentRect = new Rect(modalX + 14, modalY + 36, modalW - 28, modalH - 48);
            switch (currentDrawerTab)
            {
                case 0: DrawTabEquipment(contentRect); break;
                case 1: DrawTabInventory(contentRect); break;
                case 2: DrawTabSkillsAndScrolls(contentRect); break;
                case 3: DrawTabPets(contentRect); break;
                case 4: DrawTabSettings(contentRect); break;
            }
        }

        private void DrawTabEquipment(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "TRANG BỊ NHÂN VẬT (7 Ô CHUẨN GDD)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            string[] eqSlots = { "1. Vũ Khí: Gậy Tập Sự (+8 INT)", "2. Mũ: Mũ Vải Tân Thủ (+4 VIT)", "3. Áo: Áo Vải Thô (+5 GIÁP)", "4. Quần: Quần Vải Thô (+4 GIÁP)", "5. Giày: Giày Da Mềm (+0.4 Tốc)", "6. Vòng Cổ: Dây Chuyền Bạc (+3 INT)", "7. Nhẫn: Nhẫn Đồng Cổ (+2 INT)" };
            for (int i = 0; i < eqSlots.Length; i++)
            {
                Rect slotR = new Rect(r.x, r.y + 26 + i * 38, r.width, 32);
                DrawDoubleGoldFrame(slotR, 1);
                standardTextStyle.alignment = TextAnchor.MiddleLeft;
                GUI.Label(new Rect(slotR.x + 8, slotR.y + 6, slotR.width - 16, 20), $"🛡️ {eqSlots[i]}", standardTextStyle);
            }
        }

        private void DrawTabInventory(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "HÀNH TRANG & BÌNH DƯỢC", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            Rect hpR = new Rect(r.x, r.y + 28, r.width, 44);
            DrawDoubleGoldFrame(hpR, 1);
            standardTextStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(hpR.x + 8, hpR.y + 6, hpR.width - 96, 32), $"🧪 Bình Máu HP: Còn {playerStats?.HpPotionCount ?? 5} bình\n(Hồi +35 Máu tức thì)", standardTextStyle);
            if (GUI.Button(new Rect(hpR.x + hpR.width - 86, hpR.y + 8, 78, 28), "DÙNG [Q]"))
            {
                playerStats?.UseHpPotion();
            }

            Rect manaR = new Rect(r.x, r.y + 78, r.width, 44);
            DrawDoubleGoldFrame(manaR, 1);
            GUI.Label(new Rect(manaR.x + 8, manaR.y + 6, manaR.width - 96, 32), $"🧪 Bình Mana: Còn {playerStats?.ManaPotionCount ?? 5} bình\n(Hồi +30 Mana tức thì)", standardTextStyle);
            if (GUI.Button(new Rect(manaR.x + manaR.width - 86, manaR.y + 8, 78, 28), "DÙNG [E]"))
            {
                playerStats?.UseManaPotion();
            }

            Rect eggR = new Rect(r.x, r.y + 128, r.width, 44);
            DrawDoubleGoldFrame(eggR, 1);
            GUI.Label(new Rect(eggR.x + 8, eggR.y + 6, eggR.width - 16, 32), "🥚 Trứng Thú Cưng Sóc Chibi\n(Vật phẩm đặc quyền không bị xóa khi Rebirth)", standardTextStyle);
        }

        // ================= TAB 2: KHO CUỘN TRỤC KỸ NĂNG (BÍ KÍP) KÉO THẢ =================
        private void DrawTabSkillsAndScrolls(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "KHO CUỘN TRỤC KỸ NĂNG (KÉO THẢ VÀO Ô 1-4)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            standardTextStyle.fontSize = 11;
            standardTextStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);
            GUI.Label(new Rect(r.x, r.y + 20, r.width, 16), "⚡ Nhấn giữ chuột lên Bí Kíp để kéo thả, hoặc bấm [Gán 1-4]:", standardTextStyle);
            standardTextStyle.fontSize = 12;
            standardTextStyle.normal.textColor = Color.white;

            List<SkillScrollData> allScrolls = new List<SkillScrollData>();
            if (skillAimingController != null)
            {
                // Danh sách bí kíp trong kho + các bí kíp đang trang bị
                for (int i = 0; i < 4; i++)
                {
                    var eq = skillAimingController.GetEquippedScroll(i);
                    if (eq != null && !allScrolls.Contains(eq)) allScrolls.Add(eq);
                }
                foreach (var inv in skillAimingController.scrollInventory)
                {
                    if (inv != null && !allScrolls.Contains(inv)) allScrolls.Add(inv);
                }
            }

            float itemH = 50f;
            float startItemY = r.y + 40f;

            for (int i = 0; i < allScrolls.Count; i++)
            {
                SkillScrollData scroll = allScrolls[i];
                Rect itemRect = new Rect(r.x, startItemY + i * (itemH + 6), r.width, itemH);

                DrawDoubleGoldFrame(itemRect, 1);

                // Icon Cuộn Trục
                Texture2D icon = LoadTextureSafe(scroll.scrollIconName);
                if (icon == null) icon = LoadTextureSafe(scroll.iconName);
                if (icon == null) icon = defaultScrollIcon;

                Rect iconR = new Rect(itemRect.x + 5, itemRect.y + 5, 40, 40);
                if (icon != null) GUI.DrawTexture(iconR, icon);
                DrawOutline(iconR, scroll.tierColor, 1);

                // Tên & Thuộc tính Cuộn Trục
                standardTextStyle.alignment = TextAnchor.UpperLeft;
                standardTextStyle.normal.textColor = scroll.tierColor;
                GUI.Label(new Rect(itemRect.x + 50, itemRect.y + 4, itemRect.width - 150, 18), $"📜 {scroll.scrollName} [{scroll.tierRank}]", standardTextStyle);
                standardTextStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);
                GUI.Label(new Rect(itemRect.x + 50, itemRect.y + 24, itemRect.width - 150, 18), $"Hệ {scroll.element} · {scroll.manaCost} MP · CD {scroll.cooldown:F1}s", standardTextStyle);

                // Các nút gán nhanh [1] [2] [3] [4]
                float btnW = 22f;
                for (int slot = 1; slot <= 4; slot++)
                {
                    int targetSlot = slot;
                    Rect quickBtn = new Rect(itemRect.x + itemRect.width - (5 - slot) * (btnW + 2) - 4, itemRect.y + 12, btnW, 26);
                    if (GUI.Button(quickBtn, targetSlot.ToString()))
                    {
                        skillAimingController?.EquipScroll(targetSlot - 1, scroll);
                    }
                }

                // ================= KÉO THẢ TỪ KHO BÍ KÍP =================
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && itemRect.Contains(Event.current.mousePosition))
                {
                    isDraggingScroll = true;
                    draggedScroll = scroll;
                    dragSourceSlot = -1; // Từ kho bí kíp
                    Event.current.Use();
                }
            }
        }

        private void DrawTabPets(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "THÚ CƯNG ĐỒNG HÀNH (PET SQUIRRELS)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(r.x, r.y + 26, r.width, 100),
                "🐿️ <b>Sóc Chibi SquiRRels Studio (Cấp 1)</b>\n" +
                "• Loại: Thú Cưng Đáng Yêu (Độ nhấp nhô nhẹ nhàng 0.025m).\n" +
                "• Tiêu chuẩn 3-KHÔNG: Không cưỡi, Không đánh, Không bị chọn làm mục tiêu.\n" +
                "• Quyền lợi: Không bị xóa hay reset khi Rebirth Chuyển Sinh!",
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
                    $"<color=#ff4444>Đang thả Pet! Cần gõ: 'TÔI XÁC NHẬN'\nĐếm ngược: {Mathf.CeilToInt(petReleaseCountdown)}s mới được xóa!</color>", tooltipDescStyle);

                petConfirmInput = GUI.TextField(new Rect(r.x, r.y + 175, r.width - 80, 24), petConfirmInput);
                if (GUI.Button(new Rect(r.x + r.width - 75, r.y + 175, 75, 24), "HỦY"))
                {
                    isReleasingPet = false;
                }
            }
        }

        private void DrawTabSettings(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "CÀI ĐẶT HỆ THỐNG & GIAO DỊCH", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            autoDeclineTrade = GUI.Toggle(new Rect(r.x, r.y + 32, r.width, 26), autoDeclineTrade, " 🛡️ Tự động TỪ CHỐI mọi yêu cầu Giao Dịch (/trade)");

            GUI.Label(new Rect(r.x, r.y + 68, r.width, 140),
                $"• Trạng thái /trade: {(autoDeclineTrade ? "<color=#ff5555>ĐANG KHÓA GIAO DỊCH</color>" : "<color=#55ff55>SẴN SÀNG GIAO DỊCH</color>")}\n" +
                $"• Giới hạn tốc độ giao dịch: 2 phút / 1 người chơi.\n" +
                $"• Tỉ lệ rơi đồ khi tử nạn ngoài vùng an toàn: 5% theo GDD DOC-01.",
                tooltipDescStyle);
        }

        // ================= 8. CỬA SỔ TRÒ CHUYỆN (CHAT MODAL) =================
        private void DrawChatModal()
        {
            float chatW = 300f;
            float chatH = 240f;
            float chatX = Screen.width - chatW - 16f;
            float chatY = 118f;
            Rect chatRect = new Rect(chatX, chatY, chatW, chatH);

            DrawDoubleGoldFrame(chatRect);

            // Tiêu đề
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(chatRect.x + 8, chatRect.y + 6, chatRect.width - 40, 20), "KÊNH TRÒ CHUYỆN (LOCAL / WORLD)", boldWhiteStyle);

            if (GUI.Button(new Rect(chatRect.x + chatRect.width - 28, chatRect.y + 6, 20, 20), "X"))
            {
                isChatOpen = false;
            }

            // Lịch sử Chat
            Rect histRect = new Rect(chatRect.x + 8, chatRect.y + 30, chatRect.width - 16, chatRect.height - 70);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(histRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(histRect, new Color(0.3f, 0.4f, 0.5f, 0.5f), 1);

            standardTextStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(histRect.x + 6, histRect.y + 6, histRect.width - 12, histRect.height - 12), chatHistory, standardTextStyle);

            // Ô nhập tin nhắn
            chatInputText = GUI.TextField(new Rect(chatRect.x + 8, chatRect.y + chatRect.height - 34, chatRect.width - 70, 26), chatInputText);
            if (GUI.Button(new Rect(chatRect.x + chatRect.width - 58, chatRect.y + chatRect.height - 34, 50, 26), "GỬI"))
            {
                if (!string.IsNullOrEmpty(chatInputText))
                {
                    chatHistory += $"\n💬 [Bạn]: {chatInputText}";
                    chatInputText = "";
                }
            }
        }

        // ================= 9. TOOLTIP POPUP =================
        private void SetTooltip(string title, string body, Vector2 pos)
        {
            activeTooltipTitle = title;
            activeTooltipBody = body;
            activeTooltipPos = pos;
            showTooltip = true;
        }

        private void DrawTooltipPopup()
        {
            float w = 270f;
            float h = 130f;
            Rect r = new Rect(activeTooltipPos.x, activeTooltipPos.y, w, h);

            GUI.color = new Color(0.04f, 0.06f, 0.12f, 0.98f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(r, 1);

            tooltipTitleStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(r.x + 8, r.y + 6, r.width - 16, 20), activeTooltipTitle, tooltipTitleStyle);

            GUI.color = new Color(0.85f, 0.70f, 0.30f, 0.6f);
            GUI.DrawTexture(new Rect(r.x + 8, r.y + 26, r.width - 16, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            tooltipDescStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(r.x + 8, r.y + 30, r.width - 16, r.height - 36), activeTooltipBody, tooltipDescStyle);
        }

        // ================= TIỆN ÍCH VẼ KHUNG VIỀN VÀNG KIM ĐẲNG CẤP =================
        private void DrawDoubleGoldFrame(Rect r, int borderWidth = 2)
        {
            // Lớp nền đen bóng mờ
            GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.94f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Viền ngoài vàng kim
            Color outerGold = new Color(0.85f, 0.70f, 0.30f, 0.95f);
            DrawOutline(r, outerGold, borderWidth);

            // Viền trong vàng nhạt
            if (borderWidth >= 2)
            {
                Rect inner = new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4);
                Color innerGold = new Color(0.40f, 0.32f, 0.15f, 0.6f);
                DrawOutline(inner, innerGold, 1);
            }
        }

        private void DrawOutline(Rect r, Color color, int width)
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
