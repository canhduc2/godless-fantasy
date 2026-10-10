using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Giao Diện HUD Chuẩn Wireframe của User & Hệ Thống Hành Trang Thống Nhất:
    /// - TAB 1 [TRANG BỊ] (Theo Bản Vẽ 2): 
    ///   • Cột Trái: Mũ, Quần, Nhẫn
    ///   • Giữa: Ảnh chân dung nhân vật lớn
    ///   • Cột Phải: Áo, Giày, Vòng Cổ, Vũ Khí (dưới góc phải)
    ///   • Góc Trái Dưới: Tiền (Vàng), Ma Thạch, RP (Chuyển Sinh)
    /// - TAB 2 [TÚI ĐỒ] (Theo Bản Vẽ 1):
    ///   • Lưới 24 ô icon ô vuông (4 cột x 6 hàng)
    ///   • Khi trỏ vào hoặc ấn vào thì hiển thị bảng thông tin chi tiết và các nút thao tác
    ///   • Nút thao tác: Sử Dụng / Tháo Ra / Vứt Bỏ
    ///   • Cơ chế BẢO VỆ: Nếu đang dùng (trang bị trên người / ô skill 1-4 / thú cưng) thì ĐƯỢC BẢO VỆ không cho vứt/xóa nhầm!
    ///   • Cơ chế DOUBLE CHECK: Khi bấm Vứt, đếm ngược 5 giây mới cho bấm nút xác nhận xóa!
    ///   • Sách kỹ năng là 1 ô hành trang bình thường, có thể bấm Sử Dụng để lắp vào ô (1 2 3 4), nếu ô chưa mở khóa cấp độ sẽ không lắp được!
    /// - TAB 3 [KỸ NĂNG] (Theo Bản Vẽ 3):
    ///   • Trên: Nút cộng điểm thuộc tính (INT, STR, VIT, AGI) và điểm tiềm năng
    ///   • Giữa: Danh sách các kỹ năng đang sử dụng kèm thông tin chi tiết
    ///   • Dưới: Danh sách Perk gồm Icon ảnh + Title + Thông tin
    /// - TAB 4 [THÚ CƯNG] (Theo Bản Vẽ 4):
    ///   • Trái: Ảnh Thú Cưng lớn
    ///   • Phải: Nút [Tháo Ra] (Thu hồi) và Nút [Thả Đi] (Có đếm ngược 5s)
    ///   • Dưới: Thông tin Thú Cưng
    /// - NPC: Lại gần hiện [Bấm E để nói chuyện 💬] và tương tác chuẩn phím E.
    /// </summary>
    public class CombatHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private SkillAimingController skillAimingController;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private InventoryManager inventoryManager;

        // Textures giao diện
        private Texture2D panelBgTex;
        private Texture2D hpFillTex;
        private Texture2D manaFillTex;
        private Texture2D radarBgTex;
        private Texture2D cdOverlayTex;
        private Texture2D playerPortraitTex;
        private Texture2D petPortraitTex;

        // Skill & Item Icons Cache
        private Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();
        private Texture2D dashIcon;

        // GUIStyles sắc nét
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
        private int currentDrawerTab = 0; // 0=Trang Bị, 1=Túi Đồ, 2=Kỹ Năng, 3=Thú Cưng, 4=Cài Đặt
        private readonly string[] drawerTabs = { "TRANG BỊ", "TÚI ĐỒ", "KỸ NĂNG", "THÚ CƯNG", "CÀI ĐẶT" };

        // Selected Item in Inventory Grid
        private InventoryItem selectedInventoryItem = null;

        // Chat Modal State
        private bool isChatOpen = false;
        private string chatInputText = "/trade Player_2";
        private string chatHistory = "📢 [Server]: Chào mừng bạn đến với Godless Fantasy!\n💬 [Hệ Thống]: Đã vào Làng Tân Thủ (Vùng An Toàn).\n⚡ [Mẹo]: Bấm E khi lại gần NPC để nói chuyện! Các vật phẩm đang dùng được bảo vệ chống xóa nhầm.";

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

        private void Start()
        {
            FindReferences();
            CreateTextures();
            LoadIcons();
        }

        private void FindReferences()
        {
            if (playerMovement == null) playerMovement = FindAnyObjectByType<PlayerMovement>();
            if (skillAimingController == null) skillAimingController = FindAnyObjectByType<SkillAimingController>();
            if (playerMovement != null)
            {
                if (playerHealth == null) playerHealth = playerMovement.GetComponent<Health>();
                if (playerStats == null) playerStats = playerMovement.GetComponent<PlayerStats>();
                if (inventoryManager == null) inventoryManager = playerMovement.GetComponent<InventoryManager>();
            }
            if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();
        }

        private void CreateTextures()
        {
            if (panelBgTex == null) panelBgTex = MakeColorTex(new Color(0.06f, 0.08f, 0.14f, 0.94f));
            if (hpFillTex == null) hpFillTex = MakeColorTex(new Color(0.90f, 0.22f, 0.24f, 1f));
            if (manaFillTex == null) manaFillTex = MakeColorTex(new Color(0.16f, 0.52f, 0.92f, 1f));
            if (radarBgTex == null) radarBgTex = MakeColorTex(new Color(0.05f, 0.07f, 0.11f, 0.98f));
            if (cdOverlayTex == null) cdOverlayTex = MakeColorTex(new Color(0f, 0f, 0f, 0.72f));
        }

        private void LoadIcons()
        {
            dashIcon = LoadTextureSafe("icon_skill_dash");
            playerPortraitTex = LoadTextureSafe("player_male_base");
            petPortraitTex = LoadTextureSafe("pet_squirrel");
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
            if (playerMovement == null || inventoryManager == null || skillAimingController == null)
            {
                FindReferences();
            }

            minimapRefreshTimer += Time.deltaTime;
            if (minimapRefreshTimer > 0.5f)
            {
                minimapRefreshTimer = 0f;
                cachedMonsters = FindObjectsByType<MonsterTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                if (cachedNPC == null) cachedNPC = FindAnyObjectByType<VillageNPC>();
            }

            if (minimapPingTimer > 0f)
            {
                minimapPingTimer -= Time.deltaTime;
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

            // Kiểm tra chuột có nằm trên vùng UI không để tránh Click-to-Move xuyên màn hình
            Vector2 mousePos = Event.current.mousePosition;
            bool isMouseOverUI = (mousePos.x < 300 && mousePos.y < 100) ||
                                 (mousePos.x > Screen.width - 200 && mousePos.y < 160) ||
                                 (mousePos.y > Screen.height - 100) ||
                                 (isDrawerOpen && mousePos.x < 460) ||
                                 (isChatOpen && mousePos.x > Screen.width - 320 && mousePos.y < 350) ||
                                 (inventoryManager != null && inventoryManager.IsDropModalOpen);

            PlayerMovement.BlockWorldClick = isMouseOverUI;

            DrawTopLeftPlayerPanel();
            DrawTopCenterServerNews();
            DrawTopRightMinimapAndChat();
            DrawMidLeftDrawerButton();
            DrawBottomCenterSkillBar();

            if (isDrawerOpen) DrawDrawerModal();
            if (isChatOpen) DrawChatModal();
            if (showTooltip) DrawTooltipPopup();

            // POPUP DOUBLE CHECK 5S KHI VỨT / XÓA VẬT PHẨM
            if (inventoryManager != null && inventoryManager.IsDropModalOpen)
            {
                DrawDoubleCheckDropModal();
            }
        }

        // ================= 1. BẢNG THÔNG TIN NHÂN VẬT GÓC TRÁI TRÊN =================
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
            if (playerPortraitTex != null)
            {
                GUI.DrawTexture(new Rect(av.x + 2, av.y + 2, av.width - 4, av.height - 4), playerPortraitTex);
            }

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

            // Thanh Máu HP
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
            Rect hpRect = new Rect(panel.x + 64, panel.y + 26, 210, 16);
            DrawInteractiveBar(hpRect, hp, maxHp, hpFillTex, $"HP {hp}/{maxHp} [Bấm Q: +35]", () => {
                if (playerStats != null) playerStats.UseHpPotion();
            });

            // Thanh Mana
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
            GUI.Label(new Rect(panel.x + 64, panel.y + 63, 215, 15), $"INT: {intel} · STR: {str} · VIT: {vit} · AGI: {agi} | Tiềm Năng: {playerStats?.AvailableStatPoints ?? 0}", statBadgeStyle);
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
                GUI.Label(bannerRect, "📢 Tin Tức: SquiRRels Studio · Lại gần NPC bấm E · Đồ đang dùng được bảo vệ chống xóa!", serverNewsStyle);
            }
        }

        // ================= 3. GÓC PHẢI TRÊN: BẢN ĐỒ MINIMAP CLICK-TO-MOVE =================
        private void DrawTopRightMinimapAndChat()
        {
            float mapW = 144f;
            float mapH = 96f;
            float mapX = Screen.width - mapW - 16;
            float mapY = 14;
            Rect mapRect = new Rect(mapX, mapY, mapW, mapH);

            GUI.DrawTexture(mapRect, radarBgTex != null ? radarBgTex : Texture2D.whiteTexture);

            float minWx = -22f, maxWx = 22f;
            float minWy = -12f, maxWy = 12f;

            // 1. Phân vùng Làng và Rừng
            float safeRatio = (-6f - minWx) / (maxWx - minWx);
            float safeW = mapW * safeRatio;
            GUI.color = new Color(0.22f, 0.28f, 0.38f, 0.85f);
            GUI.DrawTexture(new Rect(mapX, mapY, safeW, mapH), Texture2D.whiteTexture);
            GUI.color = new Color(0.10f, 0.22f, 0.12f, 0.85f);
            GUI.DrawTexture(new Rect(mapX + safeW, mapY, mapW - safeW, mapH), Texture2D.whiteTexture);

            // Đường cổ trung tâm
            GUI.color = new Color(0.35f, 0.30f, 0.22f, 0.6f);
            GUI.DrawTexture(new Rect(mapX, mapY + mapH * 0.45f, mapW, 8), Texture2D.whiteTexture);

            // Vạch ranh giới
            GUI.color = new Color(1f, 0.85f, 0.25f, 0.9f);
            GUI.DrawTexture(new Rect(mapX + safeW, mapY, 2, mapH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            DrawDoubleGoldFrame(mapRect);

            // 2. Camera Frustum
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

            // 3. NPC Dot
            if (cachedNPC != null)
            {
                Vector2 npcMapPos = WorldToMinimapPos(cachedNPC.transform.position, mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);
                GUI.color = new Color(1f, 0.88f, 0.25f);
                GUI.DrawTexture(new Rect(npcMapPos.x - 3, npcMapPos.y - 3, 7, 7), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // 4. Monsters Dots
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

            // 5. Player Dot & Radar Pulse
            Vector2 pPos = playerMovement != null ? (Vector2)playerMovement.transform.position : Vector2.zero;
            Vector2 pMapPos = WorldToMinimapPos(pPos, mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);

            float pulse = Mathf.Repeat(Time.time * 2f, 1f);
            float pulseSize = 6f + pulse * 10f;
            GUI.color = new Color(1f, 0.9f, 0.2f, (1f - pulse) * 0.6f);
            GUI.DrawTexture(new Rect(pMapPos.x - pulseSize / 2f, pMapPos.y - pulseSize / 2f, pulseSize, pulseSize), Texture2D.whiteTexture);

            GUI.color = new Color(1f, 0.95f, 0.35f, 1f);
            GUI.DrawTexture(new Rect(pMapPos.x - 3, pMapPos.y - 3, 7, 7), Texture2D.whiteTexture);
            DrawOutline(new Rect(pMapPos.x - 3, pMapPos.y - 3, 7, 7), Color.black, 1);
            GUI.color = Color.white;

            // Ping Effect
            if (minimapPingTimer > 0f)
            {
                float pR = 4f + (1f - minimapPingTimer) * 18f;
                DrawOutline(new Rect(minimapPingPos.x - pR / 2f, minimapPingPos.y - pR / 2f, pR, pR), Color.green, 2);
            }

            // Zone & Coord Label
            string zoneName = (pPos.x <= -6.0f) ? "Làng Tân Thủ" : "Rừng Rậm PvP";
            statBadgeStyle.alignment = TextAnchor.MiddleCenter;
            statBadgeStyle.normal.textColor = (pPos.x <= -6.0f) ? new Color(0.5f, 0.95f, 0.6f) : new Color(1f, 0.5f, 0.5f);
            GUI.Label(new Rect(mapX, mapY + mapH - 18, mapW, 16), $"{zoneName} ({pPos.x:F1}, {pPos.y:F1})", statBadgeStyle);
            statBadgeStyle.normal.textColor = Color.white;

            // Click-To-Move
            if (Event.current.type == EventType.MouseDown && mapRect.Contains(Event.current.mousePosition))
            {
                Vector2 clickPos = Event.current.mousePosition;
                float normU = Mathf.Clamp01((clickPos.x - mapX) / mapW);
                float normV = Mathf.Clamp01((clickPos.y - mapY) / mapH);
                float targetWorldX = Mathf.Lerp(minWx, maxWx, normU);
                float targetWorldY = Mathf.Lerp(maxWy, minWy, normV);

                if (playerMovement != null)
                {
                    playerMovement.MoveTo(new Vector2(targetWorldX, targetWorldY));
                }

                minimapPingPos = clickPos;
                minimapPingTimer = 1.0f;
                Event.current.Use();
            }

            // Nút Chat [💬]
            float chatSize = 32f;
            Rect chatRect = new Rect(mapX - chatSize - 8f, mapY + 4f, chatSize, chatSize);
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
            return new Vector2(mapX + nx * mapW, mapY + (1f - ny) * mapH);
        }

        // ================= 4. MÉP TRÁI: NÚT TAM GIÁC [>] MỞ MENU TRƯỢT =================
        private void DrawMidLeftDrawerButton()
        {
            float btnW = 26f;
            float btnH = 50f;
            Rect triRect = new Rect(2f, (Screen.height - btnH) / 2f, btnW, btnH);
            DrawDoubleGoldFrame(triRect);
            GUI.Label(triRect, isDrawerOpen ? "<" : ">", drawerBtnStyle);

            if (Event.current.type == EventType.MouseDown && triRect.Contains(Event.current.mousePosition))
            {
                isDrawerOpen = !isDrawerOpen;
                Event.current.Use();
            }
        }

        // ================= 5. DƯỚI CÙNG: 4 Ô KỸ NĂNG + DASH =================
        private void DrawBottomCenterSkillBar()
        {
            float slotSize = 54f;
            float gap = 10f;
            int totalSlots = 5;
            float totalW = totalSlots * slotSize + (totalSlots - 1) * gap;
            float startX = (Screen.width - totalW) / 2f;
            float startY = Screen.height - slotSize - 18f;

            for (int i = 0; i < 4; i++)
            {
                int slotNumber = i + 1;
                float slotX = startX + i * (slotSize + gap);
                DrawEquippedScrollSlot(i, slotNumber, slotX, startY, slotSize);
            }

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

            if (scroll != null)
            {
                Texture2D icon = LoadTextureSafe(scroll.iconName);
                if (icon == null) icon = LoadTextureSafe(scroll.scrollIconName);
                if (icon != null) GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), icon);
                DrawOutline(r, scroll.tierColor, 2);

                standardTextStyle.alignment = TextAnchor.LowerRight;
                standardTextStyle.fontSize = 11;
                GUI.Label(new Rect(r.x, r.y, r.width - 4, r.height - 2), $"{scroll.manaCost}m", standardTextStyle);
                standardTextStyle.fontSize = 12;
            }
            else
            {
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.fontSize = 10;
                standardTextStyle.normal.textColor = Color.gray;
                GUI.Label(r, $"Ô {slotNumber}\nTRỐNG", standardTextStyle);
                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;
            }

            if (isAimingThis)
            {
                Color glow = Mathf.PingPong(Time.time * 6f, 1f) > 0.5f ? Color.cyan : Color.white;
                DrawOutline(r, glow, 3);
            }

            if (cdRemaining > 0.05f)
            {
                GUI.DrawTexture(r, cdOverlayTex != null ? cdOverlayTex : Texture2D.whiteTexture);
                GUI.Label(r, $"{cdRemaining:F1}s", skillCdStyle);
            }

            DrawKeybindBadge(new Rect(r.x + 3, r.y + 3, 18, 16), slotNumber.ToString());

            if (r.Contains(Event.current.mousePosition))
            {
                if (scroll != null)
                {
                    int intel = playerStats != null ? playerStats.Intelligence : 12;
                    int dmg = scroll.baseDamage + Mathf.RoundToInt(intel * scroll.intelligenceScaling);
                    string tipBody = $"• Phẩm: <color=#{ColorUtility.ToHtmlStringRGB(scroll.tierColor)}>{scroll.tierRank}</color> | Hệ: {scroll.element}\n" +
                                     $"• Tiêu hao: {scroll.manaCost} Mana | Hồi: {scroll.cooldown:F1}s\n" +
                                     $"• Sát thương: {scroll.baseDamage} + {Mathf.RoundToInt(scroll.intelligenceScaling * 100)}% INT = <color=#ffff00>{dmg} ST</color>\n" +
                                     $"• {scroll.description}\n" +
                                     $"<color=#55ff55>🛡️ Đang được bảo vệ chống xóa/vứt nhầm!</color>";
                    SetTooltip($"{scroll.scrollName} [Phím {slotNumber}]", tipBody, new Vector2(Mathf.Clamp(r.x - 60, 10, Screen.width - 320), r.y - 130));
                }
                else
                {
                    SetTooltip($"Ô Kỹ Năng [{slotNumber}]", "Mở Menu Túi Đồ [Tab 2] để chọn Sách Kỹ Năng lắp vào ô này!", new Vector2(r.x - 60, r.y - 60));
                }

                // Chuột phải để tháo chiêu
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && scroll != null)
                {
                    inventoryManager?.UnequipSkillScrollFromHotbar(slotIndex);
                    Event.current.Use();
                }
            }
        }

        private void DrawDashSlot(float x, float y, float size)
        {
            Rect r = new Rect(x, y, size, size);
            DrawDoubleGoldFrame(r);

            if (dashIcon != null) GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), dashIcon);

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
                    "• Cơ chế: Lướt 360 độ theo con trỏ chuột\n" +
                    "• Tiêu hao: 15 Mana | Hồi: 3.5s\n" +
                    "• Lướt 4.5m né đòn tức thì!",
                    new Vector2(Mathf.Clamp(r.x - 100, 10, Screen.width - 320), r.y - 100));
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

        // ================= 6. MENU TRƯỢT 5 TAB CHUẨN WIREFRAME USER =================
        private void DrawDrawerModal()
        {
            float modalW = 430f;
            float modalH = 460f;
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

            Rect contentRect = new Rect(modalX + 12, modalY + 36, modalW - 24, modalH - 46);
            switch (currentDrawerTab)
            {
                case 0: DrawTabEquipment(contentRect); break;
                case 1: DrawTabInventory(contentRect); break;
                case 2: DrawTabSkills(contentRect); break;
                case 3: DrawTabPets(contentRect); break;
                case 4: DrawTabSettings(contentRect); break;
            }
        }

        // ================= TAB 1: TRANG BỊ (CHÍNH XÁC THEO BẢN VẼ 2) =================
        private void DrawTabEquipment(Rect r)
        {
            // Tiêu đề: Hệ - Thuộc tính - Level (Đúng bản vẽ)
            string elem = playerStats != null ? playerStats.Element : "Hỏa";
            int lv = playerStats != null ? playerStats.Level : 1;
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), $"HỆ [{elem}] · THUỘC TÍNH [INT/STR] · LEVEL [{lv}]", boldWhiteStyle);
            boldWhiteStyle.alignment = TextAnchor.UpperLeft;
            boldWhiteStyle.normal.textColor = Color.white;

            float slotBoxSize = 52f;
            float leftColX = r.x + 10f;
            float rightColX = r.x + r.width - slotBoxSize - 10f;
            float centerBoxX = r.x + 75f;
            float centerBoxY = r.y + 32f;
            float centerBoxW = r.width - 150f;
            float centerBoxH = 175f;

            // KHUNG GIỮA: ẢNH NHÂN VẬT LỚN (Đúng bản vẽ 2)
            Rect centerPortrait = new Rect(centerBoxX, centerBoxY, centerBoxW, centerBoxH);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(centerPortrait, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(centerPortrait, 1);

            if (playerPortraitTex != null)
            {
                float pW = 80f;
                float pH = 80f;
                GUI.DrawTexture(new Rect(centerPortrait.x + (centerPortrait.width - pW) / 2f, centerPortrait.y + 20f, pW, pH), playerPortraitTex);
            }
            standardTextStyle.alignment = TextAnchor.LowerCenter;
            standardTextStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            GUI.Label(new Rect(centerPortrait.x, centerPortrait.y, centerPortrait.width, centerPortrait.height - 10), $"{playerStats?.CharacterName ?? "Anh Hùng"}\n(Hiệp Sĩ Áo Ba Lỗ)", standardTextStyle);
            standardTextStyle.normal.textColor = Color.white;

            // CỘT TRÁI (3 Ô): MŨ, QUẦN, NHẪN (Đúng bản vẽ 2)
            DrawEquipmentSlotButton(EquipSlot.Helmet, "Mũ", new Rect(leftColX, centerBoxY + 5, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Pants, "Quần", new Rect(leftColX, centerBoxY + 65, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Ring, "Nhẫn", new Rect(leftColX, centerBoxY + 125, slotBoxSize, slotBoxSize));

            // CỘT PHẢI (4 Ô): ÁO, GIÀY, VÒNG CỔ, VŨ KHÍ (Đúng bản vẽ 2)
            DrawEquipmentSlotButton(EquipSlot.Armor, "Áo", new Rect(rightColX, centerBoxY + 5, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Boots, "Giày", new Rect(rightColX, centerBoxY + 65, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Necklace, "Vòng Cổ", new Rect(rightColX, centerBoxY + 125, slotBoxSize, slotBoxSize));

            // VŨ KHÍ (Nằm phía dưới bên phải theo bản vẽ)
            DrawEquipmentSlotButton(EquipSlot.Weapon, "Vũ Khí", new Rect(rightColX, centerBoxY + 195, slotBoxSize, slotBoxSize));

            // GÓC TRÁI DƯỚI: TIỀN, MA THẠCH, RP (Đúng bản vẽ 2)
            float curY = centerBoxY + 200f;
            Rect curRect = new Rect(leftColX, curY, 200f, 60f);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.85f);
            GUI.DrawTexture(curRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(curRect, 1);

            standardTextStyle.alignment = TextAnchor.MiddleLeft;
            standardTextStyle.fontSize = 12;
            int goldVal = playerStats != null ? playerStats.Gold : 1500;
            int crystalVal = playerStats != null ? playerStats.MagicCrystals : 25;
            int rpVal = playerStats != null ? playerStats.RebirthPoints : 2;

            GUI.Label(new Rect(curRect.x + 8, curRect.y + 4, curRect.width - 12, 16), $"💰 Tiền: <color=#ffd700>{goldVal:N0} Vàng</color>", standardTextStyle);
            GUI.Label(new Rect(curRect.x + 8, curRect.y + 22, curRect.width - 12, 16), $"💎 Ma Thạch: <color=#00e5ff>{crystalVal}</color>", standardTextStyle);
            GUI.Label(new Rect(curRect.x + 8, curRect.y + 40, curRect.width - 12, 16), $"✨ RP (Chuyển Sinh): <color=#ff66cc>{rpVal} Điểm</color>", standardTextStyle);

            // Ghi chú cơ chế
            standardTextStyle.fontSize = 11;
            standardTextStyle.normal.textColor = new Color(0.7f, 0.85f, 0.95f);
            GUI.Label(new Rect(leftColX, curY + 66, r.width - 20, 36), "💡 Đồ ở đây lấy từ Hành Trang khi mặc lên. Đồ đang mặc được bảo vệ chống xóa/vứt nhầm!", standardTextStyle);
            standardTextStyle.fontSize = 12;
            standardTextStyle.normal.textColor = Color.white;
        }

        private void DrawEquipmentSlotButton(EquipSlot slot, string slotName, Rect r)
        {
            DrawDoubleGoldFrame(r, 1);
            InventoryItem eq = inventoryManager != null ? inventoryManager.GetEquippedGear(slot) : null;

            if (eq != null)
            {
                Texture2D icon = LoadTextureSafe(eq.iconName);
                if (icon != null) GUI.DrawTexture(new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8), icon);
                DrawOutline(r, eq.tierColor, 2);

                if (r.Contains(Event.current.mousePosition))
                {
                    SetTooltip($"{eq.itemName} ({slotName})", $"• Phẩm: {eq.tierRank}\n• Chỉ số: {eq.statsInfo}\n• {eq.description}\n<color=#ffff00>Click để THÁO RA cất vào túi đồ!</color>", new Vector2(r.x - 120, r.y - 80));

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        inventoryManager?.UnequipGearItem(slot);
                        Event.current.Use();
                    }
                }
            }
            else
            {
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.fontSize = 10;
                standardTextStyle.normal.textColor = Color.gray;
                GUI.Label(r, slotName, standardTextStyle);
                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;

                if (r.Contains(Event.current.mousePosition))
                {
                    SetTooltip($"Ô {slotName} Trống", "Mở Tab [TÚI ĐỒ] click vào trang bị phù hợp để mặc vào ô này!", new Vector2(r.x - 100, r.y - 50));
                }
            }
        }

        // ================= TAB 2: TÚI ĐỒ (LƯỚI 24 Ô ICON CHUẨN BẢN VẼ 1) =================
        private void DrawTabInventory(Rect r)
        {
            // Tiêu đề
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 18), "HÀNH TRANG (LƯỚI 24 Ô - CLICK ĐỂ XEM & DÙNG)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            // Lưới 4 cột x 6 hàng = 24 ô icon vuông (Đúng bản vẽ 1)
            float slotSize = 42f;
            float gap = 6f;
            int cols = 4;
            int rows = 6;
            float gridStartX = r.x + 10f;
            float gridStartY = r.y + 24f;

            var itemsList = inventoryManager != null ? inventoryManager.Items : new List<InventoryItem>();

            for (int index = 0; index < InventoryManager.INVENTORY_CAPACITY; index++)
            {
                int c = index % cols;
                int row = index / cols;
                Rect slotRect = new Rect(gridStartX + c * (slotSize + gap), gridStartY + row * (slotSize + gap), slotSize, slotSize);

                DrawDoubleGoldFrame(slotRect, 1);

                InventoryItem item = index < itemsList.Count ? itemsList[index] : null;

                if (item != null)
                {
                    // Vẽ Icon vật phẩm
                    Texture2D icon = LoadTextureSafe(item.iconName);
                    if (icon == null && item.scrollData != null) icon = LoadTextureSafe(item.scrollData.iconName);
                    if (icon != null)
                    {
                        GUI.DrawTexture(new Rect(slotRect.x + 3, slotRect.y + 3, slotRect.width - 6, slotRect.height - 6), icon);
                    }

                    // Viền theo phẩm cấp
                    DrawOutline(slotRect, item.tierColor, 1);

                    // Badge [E] nếu đang được trang bị / sử dụng
                    if (item.isEquipped)
                    {
                        Rect eqBadge = new Rect(slotRect.x + 2, slotRect.y + 2, 14, 12);
                        GUI.color = new Color(0.1f, 0.8f, 0.2f, 0.95f);
                        GUI.DrawTexture(eqBadge, Texture2D.whiteTexture);
                        GUI.color = Color.white;
                        statBadgeStyle.fontSize = 9;
                        statBadgeStyle.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(eqBadge, "E", statBadgeStyle);
                        statBadgeStyle.fontSize = 11;
                    }

                    // Số lượng stack
                    if (item.quantity > 1)
                    {
                        statBadgeStyle.fontSize = 10;
                        statBadgeStyle.alignment = TextAnchor.LowerRight;
                        GUI.Label(new Rect(slotRect.x, slotRect.y, slotRect.width - 2, slotRect.height - 2), $"{item.quantity}", statBadgeStyle);
                        statBadgeStyle.fontSize = 11;
                    }

                    // Đang được chọn: Viền vàng kim nhấp nháy
                    if (selectedInventoryItem == item)
                    {
                        DrawOutline(slotRect, Color.yellow, 2);
                    }

                    // Click để chọn xem thông tin
                    if (slotRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            selectedInventoryItem = item;
                            Event.current.Use();
                        }
                    }
                }
            }

            // BẢNG THÔNG TIN CHI TIẾT VẬT PHẨM BÊN PHẢI / DƯỚI (Đúng chú thích bản vẽ 1)
            float inspectX = gridStartX + cols * (slotSize + gap) + 12f;
            float inspectY = gridStartY;
            float inspectW = r.width - (inspectX - r.x);
            float inspectH = rows * (slotSize + gap);

            Rect inspectRect = new Rect(inspectX, inspectY, inspectW, inspectH);
            DrawDoubleGoldFrame(inspectRect, 1);

            if (selectedInventoryItem != null)
            {
                DrawItemInspector(inspectRect, selectedInventoryItem);
            }
            else
            {
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.fontSize = 11;
                standardTextStyle.normal.textColor = Color.gray;
                GUI.Label(inspectRect, "Ấn vào 1 ô icon\nđể xem thông tin\nvà các nút hành động!", standardTextStyle);
                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;
            }
        }

        // ================= VẼ BẢNG THÔNG TIN & CÁC NÚT TÙY VÀO VẬT PHẨM =================
        private void DrawItemInspector(Rect r, InventoryItem item)
        {
            float pY = r.y + 8f;

            // Icon + Tên
            Texture2D icon = LoadTextureSafe(item.iconName);
            if (icon != null) GUI.DrawTexture(new Rect(r.x + 8, pY, 32, 32), icon);

            standardTextStyle.alignment = TextAnchor.UpperLeft;
            standardTextStyle.fontSize = 12;
            standardTextStyle.normal.textColor = item.tierColor;
            GUI.Label(new Rect(r.x + 46, pY, r.width - 50, 18), item.itemName, standardTextStyle);

            standardTextStyle.fontSize = 10;
            standardTextStyle.normal.textColor = new Color(0.8f, 0.85f, 0.95f);
            GUI.Label(new Rect(r.x + 46, pY + 16, r.width - 50, 16), $"[{item.tierRank}] · {item.itemType}", standardTextStyle);
            pY += 38f;

            // Thông số
            if (!string.IsNullOrEmpty(item.statsInfo))
            {
                standardTextStyle.fontSize = 11;
                standardTextStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(r.x + 8, pY, r.width - 16, 16), item.statsInfo, standardTextStyle);
                pY += 18f;
            }

            // Mô tả
            standardTextStyle.fontSize = 10;
            standardTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(r.x + 8, pY, r.width - 16, 44), item.description, standardTextStyle);
            pY += 46f;

            // Trạng thái bảo vệ
            if (item.isEquipped)
            {
                standardTextStyle.fontSize = 10;
                standardTextStyle.normal.textColor = new Color(0.4f, 1f, 0.4f);
                string eqLocation = (item.itemType == ItemType.SkillScroll) ? $"Ô Phím [{item.equippedHotbarSlot + 1}]" : (item.itemType == ItemType.Equipment) ? $"{item.equipSlot}" : "Đang Đi Theo";
                GUI.Label(new Rect(r.x + 8, pY, r.width - 16, 28), $"🛡️ ĐANG DÙNG ({eqLocation})\n(Được bảo vệ chống xóa/vứt!)", standardTextStyle);
                pY += 30f;
            }

            // CÁC NÚT HÀNH ĐỘNG TÙY VÀO VẬT PHẨM (Sử dụng / Tháo ra / Vứt)
            float btnW = r.width - 16f;
            float btnH = 24f;

            // 1. Nếu là Trang Bị (Equipment)
            if (item.itemType == ItemType.Equipment)
            {
                if (!item.isEquipped)
                {
                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "MẶC TRANG BỊ"))
                    {
                        inventoryManager?.EquipGearItem(item);
                    }
                    pY += btnH + 4;
                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ (5S CONFIRM)"))
                    {
                        inventoryManager?.RequestDropItem(item);
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "THÁO RA TÚI ĐỒ"))
                    {
                        inventoryManager?.UnequipGearItem(item.equipSlot);
                    }
                }
            }
            // 2. Nếu là Sách Kỹ Năng (Skill Scroll)
            else if (item.itemType == ItemType.SkillScroll)
            {
                if (!item.isEquipped)
                {
                    // Các nút lắp vào ô 1..4 (Có kiểm tra điều kiện mở khóa)
                    standardTextStyle.fontSize = 10;
                    standardTextStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
                    GUI.Label(new Rect(r.x + 8, pY, btnW, 14), "LẮP VÀO Ô CHIÊU:", standardTextStyle);
                    pY += 16f;

                    float miniBtnW = (btnW - 6f) / 4f;
                    for (int slot = 0; slot < 4; slot++)
                    {
                        int targetSlot = slot;
                        bool unlocked = inventoryManager != null && inventoryManager.IsSlotUnlocked(slot);
                        string slotLabel = unlocked ? $"[{slot + 1}]" : "🔒";

                        if (GUI.Button(new Rect(r.x + 8 + slot * (miniBtnW + 2), pY, miniBtnW, 22), slotLabel))
                        {
                            if (unlocked)
                            {
                                inventoryManager?.EquipSkillScrollToHotbar(item, targetSlot);
                            }
                            else
                            {
                                int reqLv = inventoryManager?.GetRequiredLevelForSlot(targetSlot) ?? 1;
                                Debug.LogWarning($"[KHÓA KỸ NĂNG] Ô [{targetSlot + 1}] yêu cầu Level {reqLv} mới mở khóa!");
                            }
                        }
                    }
                    pY += 26f;

                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ (5S CONFIRM)"))
                    {
                        inventoryManager?.RequestDropItem(item);
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), $"THÁO KHỎI Ô [{item.equippedHotbarSlot + 1}]"))
                    {
                        inventoryManager?.UnequipSkillScrollFromHotbar(item.equippedHotbarSlot);
                    }
                }
            }
            // 3. Nếu là Bình Dược (Potion)
            else if (item.itemType == ItemType.Consumable)
            {
                if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "SỬ DỤNG NGAY"))
                {
                    if (item.id.Contains("hp")) playerStats?.UseHpPotion();
                    else if (item.id.Contains("mp")) playerStats?.UseManaPotion();
                }
                pY += btnH + 4;
                if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ (5S CONFIRM)"))
                {
                    inventoryManager?.RequestDropItem(item);
                }
            }
            // 4. Nếu là Thú Cưng (Pet)
            else if (item.itemType == ItemType.Pet)
            {
                if (item.isEquipped)
                {
                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "THU HỒI VÀO LỒNG"))
                    {
                        inventoryManager?.TogglePetSummon(item);
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "GỌI XUẤT CHIẾN"))
                    {
                        inventoryManager?.TogglePetSummon(item);
                    }
                    pY += btnH + 4;
                    if (GUI.Button(new Rect(r.x + 8, pY, btnW, btnH), "THẢ THÚ (5S CONFIRM)"))
                    {
                        inventoryManager?.RequestDropItem(item);
                    }
                }
            }
        }

        // ================= TAB 3: KỸ NĂNG & THUỘC TÍNH (CHÍNH XÁC THEO BẢN VẼ 3) =================
        private void DrawTabSkills(Rect r)
        {
            float pY = r.y;

            // 1. PHẦN TRÊN: LABEL, CÁC NÚT CỘNG ĐIỂM THUỘC TÍNH => VÀ THUỘC TÍNH (Đúng bản vẽ 3)
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, pY, r.width, 20), $"ĐIỂM TIỀM NĂNG KHẢ DỤNG: {playerStats?.AvailableStatPoints ?? 0} ĐIỂM", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 24f;

            // Bảng 4 dòng thuộc tính INT, STR, VIT, AGI kèm nút [+]
            Rect statBox = new Rect(r.x, pY, r.width, 70f);
            DrawDoubleGoldFrame(statBox, 1);

            int intVal = playerStats?.Intelligence ?? 12;
            int strVal = playerStats?.Strength ?? 8;
            int vitVal = playerStats?.Vitality ?? 10;
            int agiVal = playerStats?.Agility ?? 9;
            bool canAdd = (playerStats != null && playerStats.AvailableStatPoints > 0);

            // Cột 1: INT & STR
            standardTextStyle.fontSize = 12;
            GUI.Label(new Rect(statBox.x + 10, statBox.y + 8, 120, 20), $"Trí Tuệ (INT): {intVal}", standardTextStyle);
            if (GUI.Button(new Rect(statBox.x + 130, statBox.y + 6, 24, 22), "+") && canAdd) playerStats?.AddIntelligence();

            GUI.Label(new Rect(statBox.x + 10, statBox.y + 38, 120, 20), $"Sức Mạnh (STR): {strVal}", standardTextStyle);
            if (GUI.Button(new Rect(statBox.x + 130, statBox.y + 36, 24, 22), "+") && canAdd) playerStats?.AddStrength();

            // Cột 2: VIT & AGI
            GUI.Label(new Rect(statBox.x + 190, statBox.y + 8, 120, 20), $"Thể Lực (VIT): {vitVal}", standardTextStyle);
            if (GUI.Button(new Rect(statBox.x + 310, statBox.y + 6, 24, 22), "+") && canAdd) playerStats?.AddVitality();

            GUI.Label(new Rect(statBox.x + 190, statBox.y + 38, 120, 20), $"Nhanh Nhẹn (AGI): {agiVal}", standardTextStyle);
            if (GUI.Button(new Rect(statBox.x + 310, statBox.y + 36, 24, 22), "+") && canAdd) playerStats?.AddAgility();
            pY += 76f;

            // 2. PHẦN GIỮA: DANH SÁCH CÁC KỸ NĂNG ĐANG SỬ DỤNG KÈM THÔNG TIN (Đúng bản vẽ 3)
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, pY, r.width, 18), "DANH SÁCH KỸ NĂNG ĐANG SỬ DỤNG:", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            Rect skillListBox = new Rect(r.x, pY, r.width, 140f);
            DrawDoubleGoldFrame(skillListBox, 1);

            for (int s = 0; s < 4; s++)
            {
                var scroll = skillAimingController?.GetEquippedScroll(s);
                string sName = scroll != null ? scroll.scrollName : "(Chưa lắp cuộn trục)";
                string sInfo = scroll != null ? $"CD: {scroll.cooldown:F1}s | Mana: {scroll.manaCost}" : "Mở Túi Đồ để lắp";
                standardTextStyle.fontSize = 11;
                GUI.Label(new Rect(skillListBox.x + 8, skillListBox.y + 6 + s * 26, skillListBox.width - 16, 22),
                    $"⚡ <b>Ô [{s + 1}]</b>: {sName} — <color=#ffff00>{sInfo}</color>", standardTextStyle);
            }
            // Chiêu Lướt
            GUI.Label(new Rect(skillListBox.x + 8, skillListBox.y + 6 + 4 * 26, skillListBox.width - 16, 22),
                "🏃 <b>Ô [Shift]</b>: Lướt Nhanh 360° Né Đòn — <color=#ffff00>CD: 3.5s | Mana: 15</color>", standardTextStyle);
            pY += 146f;

            // 3. PHẦN DƯỚI CÙNG: DANH SÁCH PERK GỒM ICON ẢNH + TITLE VÀ THÔNG TIN (Đúng bản vẽ 3)
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, pY, r.width, 18), "DANH SÁCH PERK (BẨM SINH & TẬP SỰ):", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            Rect perkBox = new Rect(r.x, pY, r.width, 120f);
            DrawDoubleGoldFrame(perkBox, 1);

            DrawPerkRow(perkBox.x + 6, perkBox.y + 6, perkBox.width - 12, "🔥", "Hỏa Thần Quyết (Perk Hệ Hỏa)", "Tăng 10% sát thương cho toàn bộ chiêu thức ma pháp thuộc tính Hỏa.");
            DrawPerkRow(perkBox.x + 6, perkBox.y + 42, perkBox.width - 12, "⚡", "Tốc Biến Tân Thủ", "Tăng 0.5m/s tốc độ chạy cơ bản khi ở trạng thái ngoài giao tranh.");
            DrawPerkRow(perkBox.x + 6, perkBox.y + 78, perkBox.width - 12, "🛡️", "Hộ Thể Kim Cang", "Giảm 5% sát thương nhận vào khi chiến đấu trong phạm vi làng.");
        }

        private void DrawPerkRow(float x, float y, float w, string iconEmoji, string title, string desc)
        {
            boldWhiteStyle.fontSize = 14;
            GUI.Label(new Rect(x, y, 24, 24), iconEmoji, boldWhiteStyle);

            standardTextStyle.fontSize = 11;
            standardTextStyle.fontStyle = FontStyle.Bold;
            standardTextStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(x + 28, y, w - 28, 16), title, standardTextStyle);

            standardTextStyle.fontStyle = FontStyle.Normal;
            standardTextStyle.fontSize = 10;
            standardTextStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);
            GUI.Label(new Rect(x + 28, y + 15, w - 28, 16), desc, standardTextStyle);
            standardTextStyle.fontSize = 12;
            standardTextStyle.normal.textColor = Color.white;
        }

        // ================= TAB 4: THÚ CƯNG (CHÍNH XÁC THEO BẢN VẼ 4) =================
        private void DrawTabPets(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "QUẢN LÝ THÚ CƯNG (THEO BẢN VẼ 4)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            float boxSize = 130f;
            float pY = r.y + 30f;

            // TRÁI: ẢNH THÚ CƯNG LỚN (Đúng bản vẽ 4)
            Rect petImgRect = new Rect(r.x + 10, pY, boxSize, boxSize);
            DrawDoubleGoldFrame(petImgRect, 1);
            if (petPortraitTex != null)
            {
                GUI.DrawTexture(new Rect(petImgRect.x + 15, petImgRect.y + 15, boxSize - 30, boxSize - 30), petPortraitTex);
            }

            // PHẢI: NÚT [THÁO RA] VÀ NÚT [THẢ] (Đúng bản vẽ 4)
            float btnX = petImgRect.x + boxSize + 20f;
            float btnW = r.width - (btnX - r.x) - 10f;
            float btnH = 34f;

            // Nút Tháo Ra (Thu hồi / Cất pet vào lồng)
            if (GUI.Button(new Rect(btnX, pY + 15, btnW, btnH), "THÁO RA (CẤT VÀO LỒNG)"))
            {
                var petItem = inventoryManager?.Items.Find(it => it.itemType == ItemType.Pet);
                if (petItem != null) inventoryManager?.TogglePetSummon(petItem);
            }

            // Nút Thả (Release Pet có đếm ngược 5s)
            if (GUI.Button(new Rect(btnX, pY + 65, btnW, btnH), "THẢ THÚ (5S CONFIRM)"))
            {
                var petItem = inventoryManager?.Items.Find(it => it.itemType == ItemType.Pet);
                if (petItem != null)
                {
                    // Nếu đang đi theo -> Cho phép tháo ra trước rồi mới thả
                    if (petItem.isEquipped) petItem.isEquipped = false;
                    inventoryManager?.RequestDropItem(petItem);
                }
            }

            // DƯỚI: THÔNG TIN (Đúng bản vẽ 4)
            float infoY = pY + boxSize + 20f;
            Rect infoRect = new Rect(r.x + 10, infoY, r.width - 20, 160f);
            DrawDoubleGoldFrame(infoRect, 1);

            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(infoRect.x + 10, infoRect.y + 8, infoRect.width - 20, 20), "THÔNG TIN CHI TIẾT THÚ CƯNG:", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            standardTextStyle.fontSize = 12;
            GUI.Label(new Rect(infoRect.x + 10, infoRect.y + 30, infoRect.width - 20, 120),
                "🐿️ <b>Sóc Chibi SquiRRels Studio (Cấp 1)</b>\n" +
                "• Loại: Thú Cưng Đồng Hành (Vật Phẩm Đặc Quyền)\n" +
                "• Chuyển động: Nhấp nhô nhẹ 0.025m quanh chân nhân vật\n" +
                "• Tiêu chuẩn 3-KHÔNG: Không cưỡi, Không đánh, Không bị chọn làm mục tiêu\n" +
                "• Quyền lợi tối thượng: Không bị xóa hay reset khi Rebirth Chuyển Sinh!", standardTextStyle);
        }

        // ================= TAB 5: CÀI ĐẶT =================
        private void DrawTabSettings(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "CÀI ĐẶT HỆ THỐNG & GIAO DỊCH", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            autoDeclineTrade = GUI.Toggle(new Rect(r.x, r.y + 32, r.width, 26), autoDeclineTrade, " 🛡️ Tự động TỪ CHỐI mọi yêu cầu Giao Dịch (/trade)");

            GUI.Label(new Rect(r.x, r.y + 68, r.width, 180),
                $"• Trạng thái /trade: {(autoDeclineTrade ? "<color=#ff5555>ĐANG KHÓA GIAO DỊCH</color>" : "<color=#55ff55>SẴN SÀNG GIAO DỊCH</color>")}\n" +
                $"• Giới hạn tốc độ giao dịch: 2 phút / 1 người chơi.\n" +
                $"• Tỉ lệ rơi đồ khi tử nạn ngoài vùng an toàn: 5% theo GDD DOC-01.\n" +
                $"• Phím tắt: [Q] Uống máu, [E] Uống mana (hoặc Nói chuyện khi gần NPC), [Shift] Lướt, [1..4] Kỹ năng.",
                tooltipDescStyle);
        }

        // ================= POPUP DOUBLE CHECK 5S KHI VỨT BỎ VẬT PHẨM =================
        private void DrawDoubleCheckDropModal()
        {
            var pendingItem = inventoryManager?.PendingDropItem;
            if (pendingItem == null) return;

            float modalW = 340f;
            float modalH = 180f;
            float modalX = (Screen.width - modalW) / 2f;
            float modalY = (Screen.height - modalH) / 2f;
            Rect r = new Rect(modalX, modalY, modalW, modalH);

            // Nền đen đậm viền đỏ cảnh báo
            GUI.color = new Color(0.08f, 0.04f, 0.05f, 0.98f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(r, new Color(1f, 0.25f, 0.25f, 1f), 2);

            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);
            GUI.Label(new Rect(r.x, r.y + 10, r.width, 22), "⚠️ XÁC NHẬN VỨT BỎ VẬT PHẨM", boldWhiteStyle);

            standardTextStyle.fontSize = 11;
            standardTextStyle.alignment = TextAnchor.MiddleCenter;
            standardTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(r.x + 12, r.y + 36, r.width - 24, 46),
                $"Bạn có chắc chắn muốn vứt bỏ vĩnh viễn:\n<color=yellow>{pendingItem.itemName}</color> [{pendingItem.tierRank}]?\nVật phẩm sẽ biến mất khỏi thế giới!", standardTextStyle);

            float cd = inventoryManager.DropCountdown;
            bool canConfirm = (cd <= 0.001f);

            // Nút Xác Nhận (Đếm ngược 5s mới cho ấn đúng yêu cầu user!)
            string confirmText = canConfirm ? "🗑️ XÁC NHẬN XÓA" : $"⏳ Vui lòng chờ ({Mathf.CeilToInt(cd)}s)";
            GUI.enabled = canConfirm;
            GUI.color = canConfirm ? new Color(0.9f, 0.2f, 0.2f) : Color.gray;
            if (GUI.Button(new Rect(r.x + 20, r.y + 120, 140, 32), confirmText))
            {
                inventoryManager?.ConfirmDropItem();
                selectedInventoryItem = null;
            }
            GUI.enabled = true;
            GUI.color = Color.white;

            // Nút Hủy Bỏ
            if (GUI.Button(new Rect(r.x + 180, r.y + 120, 140, 32), "HỦY BỎ"))
            {
                inventoryManager?.CancelDropItem();
            }
        }

        // ================= CỬA SỔ TRÒ CHUYỆN (CHAT MODAL) =================
        private void DrawChatModal()
        {
            float chatW = 300f;
            float chatH = 240f;
            float chatX = Screen.width - chatW - 16f;
            float chatY = 118f;
            Rect chatRect = new Rect(chatX, chatY, chatW, chatH);

            DrawDoubleGoldFrame(chatRect);

            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(chatRect.x + 8, chatRect.y + 6, chatRect.width - 40, 20), "KÊNH TRÒ CHUYỆN (LOCAL / WORLD)", boldWhiteStyle);

            if (GUI.Button(new Rect(chatRect.x + chatRect.width - 28, chatRect.y + 6, 20, 20), "X"))
            {
                isChatOpen = false;
            }

            Rect histRect = new Rect(chatRect.x + 8, chatRect.y + 30, chatRect.width - 16, chatRect.height - 70);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(histRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(histRect, new Color(0.3f, 0.4f, 0.5f, 0.5f), 1);

            standardTextStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(histRect.x + 6, histRect.y + 6, histRect.width - 12, histRect.height - 12), chatHistory, standardTextStyle);

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

        // ================= TOOLTIP POPUP =================
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

        // ================= TIỆN ÍCH KHUNG VIỀN VÀNG =================
        private void DrawDoubleGoldFrame(Rect r, int borderWidth = 2)
        {
            GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.94f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            Color outerGold = new Color(0.85f, 0.70f, 0.30f, 0.95f);
            DrawOutline(r, outerGold, borderWidth);

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
