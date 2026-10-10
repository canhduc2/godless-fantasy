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
        private GUIStyle fantasyBtnLabelStyle;
        private GUIStyle keybindBadgeStyle;
        private bool isStyleInitialized = false;

        // Drawer Menu States
        private bool isDrawerOpen = false;
        private int currentDrawerTab = 0; // 0=Trang Bị, 1=Túi Đồ, 2=Kỹ Năng, 3=Thú Cưng, 4=Cài Đặt
        private readonly string[] drawerTabs = { "TRANG BỊ", "TÚI ĐỒ", "KỸ NĂNG", "THÚ CƯNG", "CÀI ĐẶT" };

        // Scroll States cho danh sách & ô đồ (Con lăn kéo thả)
        private Vector2 inventoryScrollPos = Vector2.zero;
        private Vector2 skillTabScrollPos = Vector2.zero;

        // Selected Item in Inventory Grid & Selected Equipment Slot
        private InventoryItem selectedInventoryItem = null;
        private EquipSlot? selectedEquipSlot = EquipSlot.Helmet;

        // Chat Modal State (2 Tab chuẩn đặc tả: Vị Trí Hiện Tại & Thế Giới)
        private bool isChatOpen = false;
        private int currentChatTab = 0; // 0=Vị Trí Hiện Tại (Local), 1=Thế Giới (World)
        private string chatInputText = "Chào mọi người!";
        private string localChatHistory = "📢 [Khu Vực]: Chào mừng bạn đến với Làng Tân Thủ!\n💬 [Hệ Thống]: Đã vào Vùng An Toàn (Safe Zone).\n⚡ [Mẹo]: Bấm E khi lại gần NPC để nói chuyện! Phím Q/E tiêu hao bình dược.";
        private string worldChatHistory = "🌍 [Thế Giới] DarkMage: Coi chừng skill lửa của tao nhé ae!\n🌍 [Thế Giới] Knight01: Mày chưởng trúng tao là tao chém đấy :D\n🌍 [Thế Giới] ArcherX: Boss Bìa Rừng ra rồi anh em tập trung!";

        // Settings States
        private bool autoDeclineTrade = false;

        // Tooltip State
        private string activeTooltipTitle = "";
        private string activeTooltipBody = "";
        private Vector2 activeTooltipPos;
        private bool activeTooltipAnchorBottom = false;
        private bool showTooltip = false;

        // Cache Monsters & NPC cho Minimap
        private MonsterTarget[] cachedMonsters;
        private VillageNPC cachedNPC;
        private float minimapRefreshTimer = 0f;

        // Minimap Click Ping
        private Vector2 minimapPingPos;
        private float minimapPingTimer = 0f;

        public static CombatHUD Instance { get; private set; }

        public static bool IsAnyModalBlockingScreen(float screenX, float screenY)
        {
            if (Instance == null) return false;
            if (Instance.isDrawerOpen && screenX < 480f) return true;
            if (Instance.isChatOpen && screenX > Screen.width - 330f && screenY < 380f) return true;
            if (Instance.inventoryManager != null && Instance.inventoryManager.IsDropModalOpen) return true;
            return false;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

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
                tex.filterMode = FilterMode.Point; // Chống mờ nhòe pixel art
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
                wordWrap = false,
                clipping = TextClipping.Overflow,
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
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
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
                wordWrap = false,
                clipping = TextClipping.Overflow,
                normal = { textColor = new Color(0.8f, 0.9f, 1f) }
            };

            tabBtnStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            fantasyBtnLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.90f, 0.45f) }
            };

            keybindBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(1f, 0.90f, 0.45f) }
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

            // HỖ TRỢ PHÍM TAB CHUYỂN NHANH QUA LẠI CÁC TAB KHI ĐANG MỞ MENU TRƯỢT
            if (isDrawerOpen && Input.GetKeyDown(KeyCode.Tab))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    currentDrawerTab = (currentDrawerTab - 1 + drawerTabs.Length) % drawerTabs.Length;
                }
                else
                {
                    currentDrawerTab = (currentDrawerTab + 1) % drawerTabs.Length;
                }
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

            // Ngăn chặn IMGUI focus nhảy loạn xạ khi bấm phím Tab trong menu
            if (isDrawerOpen && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
            {
                Event.current.Use();
            }

            // Kiểm tra chuột có nằm trên vùng UI không để tránh Click-to-Move xuyên màn hình
            Vector2 mousePos = Event.current.mousePosition;
            bool isMouseOverUI = (mousePos.x < 380 && mousePos.y < 105) ||
                                 (mousePos.x > Screen.width - 200 && mousePos.y < 160) ||
                                 (mousePos.y > Screen.height - 100) ||
                                 (isDrawerOpen && mousePos.x < 495) ||
                                 (!isDrawerOpen && mousePos.x < 36 && Mathf.Abs(mousePos.y - Screen.height / 2f) < 40) ||
                                 (isChatOpen && mousePos.x > Screen.width - 320 && mousePos.y < 370) ||
                                 (inventoryManager != null && inventoryManager.IsDropModalOpen);

            PlayerMovement.BlockWorldClick = isMouseOverUI;

            // 1. LỚP HUD CƠ BẢN (Depth 10): Nằm trên thế giới (Depth 20), dưới Menu/Modal (Depth 0)
            GUI.depth = FantasyUITheme.DepthBaseHUD;

            DrawTopLeftPlayerPanel();
            DrawTopRightMinimapAndChat();
            DrawMidLeftDrawerButton();
            DrawBottomCenterSkillBar();
            DrawBottomCenterServerNews(); // Thanh tin tức gọn nhỏ sát đáy chạy chữ từ phải sang trái!

            // 2. LỚP GIAO DIỆN CÀI ĐẶT / MENU / MODAL (Depth 0): Mặc nhiên luôn ở trên cùng của HUD
            GUI.depth = FantasyUITheme.DepthModal;
            if (isDrawerOpen) DrawDrawerModal();
            if (isChatOpen) DrawChatModal();

            // 3. LỚP TRÊN CÙNG TUYỆT ĐỐI (Depth -100): HỘP THOẠI XÁC NHẬN 5S & TOOLTIP
            GUI.depth = FantasyUITheme.DepthPopupTooltip;
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
            float panelW = 360f; // Độ rộng 360px rộng rãi, tên dài hiển thị đầy đủ không bị ép truncate
            float panelH = 82f;  // Chiều cao 82px gọn gàng, dòng dưới là 1 hàng chung kéo dài từ mép trái sang phải
            Rect panel = new Rect(16, 14, panelW, panelH);

            DrawDoubleGoldFrame(panel);

            // Avatar Box 48x48
            Rect av = new Rect(panel.x + 8, panel.y + 7, 48, 48);
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
            string elem = playerStats != null ? playerStats.Element : "Hỏa";
            if (elem.Contains("(")) elem = elem.Substring(0, elem.IndexOf('(')).Trim(); // Bỏ (Fire)

            Rect lvBadge = new Rect(av.x - 1, av.y + av.height - 14, 36, 15);
            GUI.color = new Color(0.75f, 0.55f, 0.15f, 0.95f);
            GUI.DrawTexture(lvBadge, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(lvBadge, new Color(1f, 0.85f, 0.35f), 1);
            statBadgeStyle.alignment = TextAnchor.MiddleCenter;
            statBadgeStyle.fontSize = 10;
            GUI.Label(lvBadge, $"Lv.{lv}", statBadgeStyle);

            // DÒNG 1: HỆ & TÊN (Hệ = Kị Sĩ/Pháp Sư/Võ Sĩ/Xạ Thủ, hiển thị đầy đủ tên người chơi: Dũng Sĩ SquiRRels)
            string charClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
            string rawName = playerStats != null ? playerStats.CharacterName : "Dũng Sĩ SquiRRels";
            string displayName = (rawName.Length > 20) ? (rawName.Substring(0, 18) + "...") : rawName;

            float rightColX = panel.x + 62f;
            float rightColW = panelW - 70f;

            boldWhiteStyle.alignment = TextAnchor.UpperLeft;
            boldWhiteStyle.fontSize = 11;
            boldWhiteStyle.wordWrap = false;
            boldWhiteStyle.clipping = TextClipping.Overflow;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(rightColX, panel.y + 6, rightColW, 16), $"Hệ: <color=#00e5ff>{charClass}</color> · Tên: <color=#ffffff>{displayName}</color>", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            // Thanh Máu HP (Chiều cao 14px gọn gàng)
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
            Rect hpRect = new Rect(rightColX, panel.y + 23, rightColW, 14);
            DrawInteractiveBar(hpRect, hp, maxHp, hpFillTex, $"HP {hp}/{maxHp} [Bấm Q: +35]", () => {
                if (playerStats != null) playerStats.UseHpPotion();
            });

            // Thanh Mana (Chiều cao 14px gọn gàng)
            int mana = playerStats != null ? playerStats.CurrentMana : 80;
            int maxMana = playerStats != null ? playerStats.MaxMana : 100;
            Rect manaRect = new Rect(rightColX, panel.y + 39, rightColW, 14);
            DrawInteractiveBar(manaRect, mana, maxMana, manaFillTex, $"MANA {mana}/{maxMana} [Bấm E: +30]", () => {
                if (playerStats != null) playerStats.UseManaPotion();
            });

            // DÒNG DƯỚI CÙNG (DÒNG GỘP FULL CHIỀU NGANG): THUỘC TÍNH (HỎA), BỎ HOÀN TOÀN TIỀM NĂNG THEO YÊU CẦU USER!
            int str = playerStats != null ? playerStats.Strength : 8;
            int intel = playerStats != null ? playerStats.Intelligence : 12;
            int vit = playerStats != null ? playerStats.Vitality : 10;
            int agi = playerStats != null ? playerStats.Agility : 9;
            statBadgeStyle.alignment = TextAnchor.MiddleLeft;
            statBadgeStyle.fontSize = 11;
            statBadgeStyle.wordWrap = false;
            statBadgeStyle.clipping = TextClipping.Overflow;
            Rect row4 = new Rect(panel.x + 8, panel.y + 58, panelW - 16, 18);
            GUI.Label(row4, $"Thuộc tính: <color=#ff6633>{elem}</color> | INT: {intel} · STR: {str} · VIT: {vit} · AGI: {agi}", statBadgeStyle);
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

        // ================= 2. DƯỚI ĐÁY MÀN HÌNH: THANH TIN TỨC GỌN NHỎ CHẠY CHỮ TỪ PHẢI SANG TRÁI =================
        private void DrawBottomCenterServerNews()
        {
            float bannerW = Mathf.Clamp(Screen.width * 0.55f, 480f, 680f);
            float bannerH = 22f; // Gọn nhỏ thanh mảnh đúng chuẩn thanh chạy chữ
            float bannerX = (Screen.width - bannerW) / 2f;
            float bannerY = Screen.height - bannerH - 2f; // Sát mép đáy màn hình

            Rect bannerRect = new Rect(bannerX, bannerY, bannerW, bannerH);

            if (skillAimingController != null && skillAimingController.IsAiming)
            {
                int s = skillAimingController.CurrentAimingSkill;
                var scroll = skillAimingController.GetEquippedScroll(s - 1);
                string sName = scroll != null ? scroll.scrollName : $"Chiêu [{s}]";
                string guideMsg = $"🎯 [ĐANG NGẮM]: Chiêu [{s}] {sName} — Bấm CHUỘT PHẢI để Tung Chiêu · Bấm [{s}] hoặc ESC để Hủy Ngắm · Chuột Trái vẫn dùng để Di Chuyển.";
                FantasyUITheme.DrawMarqueeTicker(bannerRect, "🎯 HƯỚNG DẪN", guideMsg, 75f, Color.cyan);
            }
            else
            {
                string newsMsg = "⚔️ SquiRRels Studio chào đón dũng sĩ! · 💡 Lại gần NPC Trưởng Làng bấm E để nhận nhiệm vụ · 🛡️ Vật phẩm đang dùng & ô chiêu được bảo vệ chống xóa · ⚡ Bấm Q hồi 35 HP, bấm E hồi 30 Mana · 🎯 Chiêu 1-4: Bấm để ngắm, Chuột Phải để tung chiêu · 🎒 Mở Túi Đồ để lắp hoặc tháo cuộn trục bí kíp!";
                FantasyUITheme.DrawMarqueeTicker(bannerRect, "📢 TIN TỨC", newsMsg, 60f, FantasyUITheme.ColorTextGold);
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
            if (DrawFantasyButton(chatRect, "💬", 14, isChatOpen))
            {
                isChatOpen = !isChatOpen;
            }
        }

        private Vector2 WorldToMinimapPos(Vector3 worldPos, float mapX, float mapY, float mapW, float mapH, float minWx, float maxWx, float minWy, float maxWy)
        {
            float nx = Mathf.Clamp01((worldPos.x - minWx) / (maxWx - minWx));
            float ny = Mathf.Clamp01((worldPos.y - minWy) / (maxWy - minWy));
            return new Vector2(mapX + nx * mapW, mapY + (1f - ny) * mapH);
        }

        // ================= 4. NÚT MỞ / ĐÓNG MENU TRƯỢT =================
        private void DrawMidLeftDrawerButton()
        {
            float btnW = 26f;
            float btnH = 54f;
            float modalW = 440f;
            float modalX = 16f;

            // UX CẢI TIẾN: Khi mở menu thì nút di chuyển về bên phải ngoài cùng của Menu thay vì ở nguyên bên trái!
            float btnX = isDrawerOpen ? (modalX + modalW + 2f) : 4f;
            float btnY = (Screen.height - btnH) / 2f;
            Rect triRect = new Rect(btnX, btnY, btnW, btnH);

            if (DrawFantasyButton(triRect, isDrawerOpen ? "<" : ">", 16, isDrawerOpen))
            {
                isDrawerOpen = !isDrawerOpen;
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
            float startY = Screen.height - slotSize - 34f; // Nâng lên 34px tạo khoảng cách 10px thoáng đãng phía trên thanh tin tức

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

                // Huy hiệu nền tối + Chữ Mana trắng đậm sáng nét có đổ bóng đen (khắc phục hoàn toàn lỗi mờ/răng cưa)
                Rect manaBadge = new Rect(r.x + r.width - 32f, r.y + r.height - 18f, 28f, 15f);
                GUI.color = new Color(0.04f, 0.06f, 0.12f, 0.88f);
                GUI.DrawTexture(manaBadge, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawOutline(manaBadge, new Color(0.25f, 0.55f, 0.9f, 0.65f), 1);

                boldWhiteStyle.fontSize = 11;
                boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
                // Đổ bóng chữ đen
                boldWhiteStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(manaBadge.x + 1, manaBadge.y + 1, manaBadge.width, manaBadge.height), $"{scroll.manaCost}m", boldWhiteStyle);

                // Chữ trắng đậm sáng nổi bật
                boldWhiteStyle.normal.textColor = Color.white;
                GUI.Label(manaBadge, $"{scroll.manaCost}m", boldWhiteStyle);
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

            DrawKeybindBadge(new Rect(r.x + 7, r.y + 6, 26, 18), slotNumber.ToString());

            if (r.Contains(Event.current.mousePosition))
            {
                if (scroll != null)
                {
                    int intel = playerStats != null ? playerStats.Intelligence : 12;
                    int dmg = scroll.baseDamage + Mathf.RoundToInt(intel * scroll.intelligenceScaling);
                    string tipBody = $"• Phẩm: <color=#{ColorUtility.ToHtmlStringRGB(scroll.tierColor)}>{scroll.tierRank}</color> | Thuộc tính: {scroll.element}\n" +
                                     $"• Tiêu hao: {scroll.manaCost} Mana | Hồi: {scroll.cooldown:F1}s\n" +
                                     $"• Sát thương: {scroll.baseDamage} + {Mathf.RoundToInt(scroll.intelligenceScaling * 100)}% Trí Tuệ = <color=#ffff00>{dmg} ST</color>\n" +
                                     $"• {scroll.description}\n" +
                                     $"<color=#55ff55>🛡️ Đang được bảo vệ chống xóa/vứt nhầm!</color>";
                    SetTooltip($"{scroll.scrollName} [Phím {slotNumber}]", tipBody, new Vector2(Mathf.Clamp(r.x + (r.width - 290f) / 2f, 16f, Screen.width - 290f - 16f), r.y - 8f), true);
                }
                else
                {
                    SetTooltip($"Ô Kỹ Năng [{slotNumber}]", "Mở Menu Túi Đồ [Tab 2] để chọn Sách Kỹ Năng lắp vào ô này!", new Vector2(Mathf.Clamp(r.x + (r.width - 290f) / 2f, 16f, Screen.width - 290f - 16f), r.y - 8f), true);
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

            DrawKeybindBadge(new Rect(r.x + 7, r.y + 6, 42, 18), "SHIFT");

            if (r.Contains(Event.current.mousePosition))
            {
                float curCd = playerMovement != null ? playerMovement.CurrentDashCooldown : 3.5f;
                SetTooltip("Lướt Nhanh (Dash) [Shift / Phím Cách]",
                    "• Cơ chế: Lướt 360 độ theo con trỏ chuột\n" +
                    "• Tiêu hao: 10 Mana\n" +
                    $"• Hồi chiêu: <color=#00e5ff>{curCd:F1}s</color> (Giảm theo Nhanh Nhẹn AGI, tối thiểu 1.0s)\n" +
                    "• Lướt 4.5m né đòn tức thì!",
                    new Vector2(Mathf.Clamp(r.x + (r.width - 290f) / 2f, 16f, Screen.width - 290f - 16f), r.y - 8f), true);
            }
        }

        private void DrawKeybindBadge(Rect r, string key)
        {
            // BỎ HẲN NỀN VIỀN THEO ĐÚNG YÊU CẦU USER - CHỈ GIỮ TEXT
            if (keybindBadgeStyle == null) InitStyles();
            keybindBadgeStyle.alignment = TextAnchor.UpperLeft;
            keybindBadgeStyle.fontSize = 11;
            keybindBadgeStyle.fontStyle = FontStyle.Bold;

            // Đổ bóng chữ đen mảnh để nổi rõ trên mọi nền icon
            keybindBadgeStyle.normal.textColor = new Color(0f, 0f, 0f, 0.9f);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), key, keybindBadgeStyle);

            // Chữ vàng sáng
            keybindBadgeStyle.normal.textColor = new Color(1f, 0.90f, 0.45f, 1f);
            GUI.Label(r, key, keybindBadgeStyle);
        }

        // ================= 6. MENU TRƯỢT 5 TAB CHUẨN WIREFRAME USER =================
        private void DrawDrawerModal()
        {
            float modalW = 440f;
            float modalH = 480f;
            float modalX = 16f;
            float modalY = (Screen.height - modalH) / 2f;
            Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

            // Nền tối 100% đặc chống tuyệt đối tình trạng chữ từ HUD dưới bị nhìn xuyên qua
            GUI.color = new Color(0.04f, 0.05f, 0.09f, 1.0f);
            GUI.DrawTexture(modalRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(modalRect);

            // Tab Headers sử dụng DrawFantasyButton đồng bộ viền đẹp
            float tabW = modalW / drawerTabs.Length;
            for (int i = 0; i < drawerTabs.Length; i++)
            {
                Rect tabRect = new Rect(modalX + i * tabW, modalY, tabW, 30f);
                bool isActive = (currentDrawerTab == i);

                if (DrawFantasyButton(tabRect, drawerTabs[i], 11, isActive))
                {
                    currentDrawerTab = i;
                }
            }

            Rect contentRect = new Rect(modalX + 12, modalY + 38, modalW - 24, modalH - 50);
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
            // Tiêu đề: Hệ - Thuộc tính - Level (Đúng quy ước: Hệ = Kị Sĩ/Pháp Sư..., Thuộc tính = Hỏa/Lôi...)
            string charClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
            string elem = playerStats != null ? playerStats.Element : "Hỏa";
            int lv = playerStats != null ? playerStats.Level : 1;
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), $"HỆ [{charClass}] · THUỘC TÍNH [{elem}] · LEVEL [{lv}]", boldWhiteStyle);
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

            // GÓC TRÁI DƯỚI: TIỀN, MA THẠCH, RP (Đúng bản vẽ 2 - Bản rộng 150px vừa vặn không thừa)
            float curY = centerBoxY + 195f;
            Rect curRect = new Rect(leftColX, curY, 150f, 64f);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.85f);
            GUI.DrawTexture(curRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(curRect, 1);

            standardTextStyle.alignment = TextAnchor.MiddleLeft;
            standardTextStyle.fontSize = 11;
            int goldVal = playerStats != null ? playerStats.Gold : 1500;
            int crystalVal = playerStats != null ? playerStats.MagicCrystals : 25;
            int rpVal = playerStats != null ? playerStats.RebirthPoints : 2;

            GUI.Label(new Rect(curRect.x + 8, curRect.y + 4, curRect.width - 12, 18), $"💰 Tiền: <color=#ffd700>{goldVal:N0}</color>", standardTextStyle);
            GUI.Label(new Rect(curRect.x + 8, curRect.y + 23, curRect.width - 12, 18), $"💎 Ma Thạch: <color=#00e5ff>{crystalVal}</color>", standardTextStyle);
            GUI.Label(new Rect(curRect.x + 8, curRect.y + 42, curRect.width - 12, 18), $"✨ RP: <color=#ff66cc>{rpVal} Điểm</color>", standardTextStyle);

            // KHU VỰC THAO TÁC TRANG BỊ ĐANG CHỌN (MỞ RỘNG 202PX - KHÔNG BAO GIỜ BỊ TRUNCATE / XUỐNG DÒNG)
            float actX = leftColX + 158f;
            float actW = rightColX - actX - 8f;
            Rect actBoxRect = new Rect(actX, curY, actW, 64f);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.85f);
            GUI.DrawTexture(actBoxRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(actBoxRect, 1);

            InventoryItem selectedGear = selectedEquipSlot.HasValue && inventoryManager != null
                ? inventoryManager.GetEquippedGear(selectedEquipSlot.Value)
                : null;

            if (selectedGear != null)
            {
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.fontSize = 11;
                standardTextStyle.wordWrap = false;
                standardTextStyle.normal.textColor = new Color(0.4f, 0.95f, 1f);
                GUI.Label(new Rect(actBoxRect.x + 4, actBoxRect.y + 5, actBoxRect.width - 8, 18), $"Đang chọn: <color=#ffffff>{selectedGear.itemName}</color>", standardTextStyle);
                standardTextStyle.wordWrap = true;
                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;

                Rect unequipBtnRect = new Rect(actBoxRect.x + 10, actBoxRect.y + 26, actBoxRect.width - 20, 30);
                if (DrawFantasyButton(unequipBtnRect, "⚔️ THÁO RA TÚI ĐỒ", 11, true, new Color(1f, 0.45f, 0.45f)))
                {
                    inventoryManager?.UnequipGearItem(selectedEquipSlot.Value);
                }
            }
            else
            {
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.fontSize = 10;
                standardTextStyle.normal.textColor = Color.gray;
                string slotName = selectedEquipSlot.HasValue ? GetSlotVietnameseName(selectedEquipSlot.Value) : "Trang Bị";
                GUI.Label(new Rect(actBoxRect.x + 4, actBoxRect.y + 14, actBoxRect.width - 8, 36), $"Ô [{slotName}]\nChưa có đồ mặc", standardTextStyle);
                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;
            }

            // Ghi chú cơ chế
            standardTextStyle.fontSize = 11;
            standardTextStyle.normal.textColor = new Color(0.7f, 0.85f, 0.95f);
            GUI.Label(new Rect(leftColX, curY + 68, r.width - 20, 36), "💡 Đồ ở đây lấy từ Hành Trang khi mặc lên. Đồ đang mặc được bảo vệ chống xóa/vứt nhầm!", standardTextStyle);
            standardTextStyle.fontSize = 12;
            standardTextStyle.normal.textColor = Color.white;
        }

        private void DrawEquipmentSlotButton(EquipSlot slot, string slotName, Rect r)
        {
            DrawDoubleGoldFrame(r, 1);
            InventoryItem eq = inventoryManager != null ? inventoryManager.GetEquippedGear(slot) : null;
            bool isSelected = (selectedEquipSlot == slot);

            // HIỆU ỨNG SELECT ĐÀNG HOÀNG: Vừa ngắm icon sắc nét vừa phát sáng hào quang
            if (isSelected)
            {
                float pulse = Mathf.PingPong(Time.time * 4f, 1f);
                Color glowBorder = Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(0.2f, 0.95f, 1f), pulse);
                GUI.color = new Color(0.2f, 0.45f, 0.8f, 0.28f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawOutline(r, glowBorder, 2);

                int sCorner = 4;
                GUI.color = glowBorder;
                GUI.DrawTexture(new Rect(r.x - 2, r.y - 2, sCorner, sCorner), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - sCorner + 2, r.y - 2, sCorner, sCorner), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x - 2, r.y + r.height - sCorner + 2, sCorner, sCorner), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - sCorner + 2, r.y + r.height - sCorner + 2, sCorner, sCorner), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            if (eq != null)
            {
                Texture2D icon = LoadTextureSafe(eq.iconName);
                if (icon != null) GUI.DrawTexture(new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8), icon);
                if (!isSelected) DrawOutline(r, eq.tierColor, 2);

                if (r.Contains(Event.current.mousePosition))
                {
                    // Vị trí bảng tooltip né sang cạnh để KHÔNG BAO GIỜ CHE ICON HOẶC ẢNH NHÂN VẬT
                    float tipX = (r.x < Screen.width * 0.4f) ? (r.x + r.width + 14f) : (r.x - 295f - 14f);
                    float tipY = Mathf.Clamp(r.y - 10f, 16f, Screen.height - 150f);
                    string tipBody = $"• Phẩm: {eq.tierRank}\n• Chỉ số: {eq.statsInfo}\n• {eq.description}";

                    SetTooltip($"{eq.itemName} ({slotName})", tipBody, new Vector2(tipX, tipY));

                    // Click chuột trái: CHỌN ĐỂ XEM (Không tháo tức thì!)
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        selectedEquipSlot = slot;
                        Event.current.Use();
                    }
                    // Click chuột phải: Thao tác nhanh tháo đồ nếu người chơi muốn
                    else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
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
                    float tipX = (r.x < Screen.width * 0.4f) ? (r.x + r.width + 14f) : (r.x - 295f - 14f);
                    float tipY = Mathf.Clamp(r.y - 10f, 16f, Screen.height - 150f);
                    SetTooltip($"Ô {slotName} Trống", "Mở Tab [TÚI ĐỒ] click vào trang bị phù hợp để mặc vào ô này!", new Vector2(tipX, tipY));

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        selectedEquipSlot = slot;
                        Event.current.Use();
                    }
                }
            }
        }

        private string GetSlotVietnameseName(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Helmet: return "Mũ";
                case EquipSlot.Armor: return "Áo";
                case EquipSlot.Pants: return "Quần";
                case EquipSlot.Boots: return "Giày";
                case EquipSlot.Ring: return "Nhẫn";
                case EquipSlot.Necklace: return "Vòng Cổ";
                case EquipSlot.Weapon: return "Vũ Khí";
                default: return slot.ToString();
            }
        }

        // ================= TAB 2: TÚI ĐỒ (LƯỚI Ô ĐỒ CÓ CON LĂN CUỘN KÉO THẢ) =================
        private void DrawTabInventory(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), "HÀNH TRANG (CHỌN ĐỂ XEM & DÙNG · CUỘN CON LĂN ĐỒ)", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            float slotSize = 42f;
            float gap = 6f;
            int cols = 4;
            var itemsList = inventoryManager != null ? inventoryManager.Items : new List<InventoryItem>();
            int totalSlots = Mathf.Max(InventoryManager.INVENTORY_CAPACITY, itemsList.Count);
            int rows = Mathf.CeilToInt(totalSlots / (float)cols);

            float gridAreaW = cols * (slotSize + gap) + 18f; // ~210px
            float gridAreaH = r.height - 28f;
            Rect scrollAreaRect = new Rect(r.x, r.y + 24f, gridAreaW, gridAreaH);
            float contentH = rows * (slotSize + gap) + 8f;
            Rect viewRect = new Rect(0, 0, gridAreaW - 16f, Mathf.Max(gridAreaH, contentH));

            // Con lăn chuột + Kéo thanh cuộn (Đúng yêu cầu user: có con lăn kéo thả khi ô đồ tăng lên)
            inventoryScrollPos = GUI.BeginScrollView(scrollAreaRect, inventoryScrollPos, viewRect, false, true);

            for (int index = 0; index < totalSlots; index++)
            {
                int c = index % cols;
                int row = index / cols;
                Rect slotRect = new Rect(c * (slotSize + gap), row * (slotSize + gap), slotSize, slotSize);

                DrawDoubleGoldFrame(slotRect, 1);

                InventoryItem item = index < itemsList.Count ? itemsList[index] : null;

                if (item != null)
                {
                    Texture2D icon = LoadTextureSafe(item.iconName);
                    if (icon == null && item.scrollData != null) icon = LoadTextureSafe(item.scrollData.iconName);
                    if (icon != null)
                    {
                        GUI.DrawTexture(new Rect(slotRect.x + 3, slotRect.y + 3, slotRect.width - 6, slotRect.height - 6), icon);
                    }

                    DrawOutline(slotRect, item.tierColor, 1);

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

                    if (item.quantity > 1)
                    {
                        statBadgeStyle.fontSize = 10;
                        statBadgeStyle.alignment = TextAnchor.LowerRight;
                        GUI.Label(new Rect(slotRect.x, slotRect.y, slotRect.width - 2, slotRect.height - 2), $"{item.quantity}", statBadgeStyle);
                        statBadgeStyle.fontSize = 11;
                    }

                    if (selectedInventoryItem == item)
                    {
                        DrawOutline(slotRect, Color.yellow, 2);
                    }

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

            GUI.EndScrollView();

            // BẢNG THÔNG TIN CHI TIẾT VẬT PHẨM BÊN PHẢI (INSPECTOR)
            float inspectX = r.x + gridAreaW + 8f;
            float inspectW = r.width - gridAreaW - 10f;
            float inspectH = gridAreaH;

            Rect inspectRect = new Rect(inspectX, r.y + 24f, inspectW, inspectH);
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
                GUI.Label(inspectRect, "Ấn vào 1 ô icon\nđể xem thông tin\nvà các nút hành động!\n\n(Dùng con lăn chuột\nđể cuộn danh sách)", standardTextStyle);
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

            // CÁC NÚT HÀNH ĐỘNG ĐỒNG BỘ FANTASY BUTTON
            float btnW = r.width - 16f;
            float btnH = 26f;

            // 1. Nếu là Trang Bị (Equipment)
            if (item.itemType == ItemType.Equipment)
            {
                if (!item.isEquipped)
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "MẶC TRANG BỊ", 11, true))
                    {
                        inventoryManager?.EquipGearItem(item);
                    }
                    pY += btnH + 5;
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ (XÁC NHẬN 5S)", 11, false, new Color(1f, 0.45f, 0.45f)))
                    {
                        inventoryManager?.RequestDropItem(item);
                    }
                }
                else
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "THÁO RA TÚI ĐỒ", 11))
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

                        if (DrawFantasyButton(new Rect(r.x + 8 + slot * (miniBtnW + 2), pY, miniBtnW, 24), slotLabel, 10, unlocked))
                        {
                            if (unlocked)
                            {
                                inventoryManager?.EquipSkillScrollToHotbar(item, targetSlot);
                            }
                            else
                            {
                                int reqLv = inventoryManager?.GetRequiredLevelForSlot(targetSlot) ?? 1;
                                Debug.LogWarning($"[KHÓA KỸ NĂNG] Ô [{targetSlot + 1}] yêu cầu Cấp {reqLv} mới mở khóa!");
                            }
                        }
                    }
                    pY += 28f;

                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ (XÁC NHẬN 5S)", 11, false, new Color(1f, 0.45f, 0.45f)))
                    {
                        inventoryManager?.RequestDropItem(item);
                    }
                }
                else
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), $"THÁO KHỎI Ô [{item.equippedHotbarSlot + 1}]", 11))
                    {
                        inventoryManager?.UnequipSkillScrollFromHotbar(item.equippedHotbarSlot);
                    }
                }
            }
            // 3. Nếu là Bình Dược (Potion)
            else if (item.itemType == ItemType.Consumable)
            {
                if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "SỬ DỤNG NGAY", 11, true, new Color(0.4f, 1f, 0.4f)))
                {
                    if (item.id.Contains("hp")) playerStats?.UseHpPotion();
                    else if (item.id.Contains("mp")) playerStats?.UseManaPotion();
                    if (item.quantity <= 0) selectedInventoryItem = null;
                }
                pY += btnH + 5;
                if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ (XÁC NHẬN 5S)", 11, false, new Color(1f, 0.45f, 0.45f)))
                {
                    inventoryManager?.RequestDropItem(item);
                }
            }
            // 4. Nếu là Thú Cưng (Pet)
            else if (item.itemType == ItemType.Pet)
            {
                if (item.isEquipped)
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "THU HỒI VÀO LỒNG", 11))
                    {
                        inventoryManager?.TogglePetSummon(item);
                    }
                }
                else
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "GỌI XUẤT CHIẾN", 11, true, new Color(0.4f, 1f, 0.6f)))
                    {
                        inventoryManager?.TogglePetSummon(item);
                    }
                    pY += btnH + 5;
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "THẢ THÚ (XÁC NHẬN 5S)", 11, false, new Color(1f, 0.45f, 0.45f)))
                    {
                        inventoryManager?.RequestDropItem(item);
                    }
                }
            }
        }

        // ================= TAB 3: KỸ NĂNG & THUỘC TÍNH (CÓ CON LĂN CUỘN TRÁNH TRÀN CHỮ) =================
        private void DrawTabSkills(Rect r)
        {
            float contentH = 460f;
            Rect viewRect = new Rect(0, 0, r.width - 16f, Mathf.Max(r.height, contentH));
            skillTabScrollPos = GUI.BeginScrollView(r, skillTabScrollPos, viewRect, false, true);

            float pY = 0f;

            // 1. PHẦN TRÊN: ĐIỂM TIỀM NĂNG & CỘNG ĐIỂM THUỘC TÍNH
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(0, pY, viewRect.width, 20), $"ĐIỂM TIỀM NĂNG KHẢ DỤNG: {playerStats?.AvailableStatPoints ?? 0} ĐIỂM", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 24f;

            // Bảng 4 dòng thuộc tính Trí Tuệ, Sức Mạnh, Thể Lực, Nhanh Nhẹn
            Rect statBox = new Rect(0, pY, viewRect.width, 74f);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(statBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(statBox, 1);

            int intVal = playerStats?.Intelligence ?? 12;
            int strVal = playerStats?.Strength ?? 8;
            int vitVal = playerStats?.Vitality ?? 10;
            int agiVal = playerStats?.Agility ?? 9;
            bool canAdd = (playerStats != null && playerStats.AvailableStatPoints > 0);

            // Bảng 2 Cột Đối Xứng Hoàn Hảo (Symmetrical Two-Column Stat Card)
            float colW = statBox.width * 0.5f;
            float btnSize = 22f;

            // Vạch chia cột tinh tế ở giữa
            DrawOutline(new Rect(statBox.x + colW, statBox.y + 8, 1, 58), new Color(0.40f, 0.45f, 0.55f, 0.25f), 1);

            standardTextStyle.fontSize = 12;
            standardTextStyle.alignment = TextAnchor.MiddleLeft;

            // CỘT 1 (Bên trái): Trí Tuệ & Sức Mạnh
            float col1X = statBox.x + 14f;
            float col1BtnX = statBox.x + colW - btnSize - 14f;

            GUI.Label(new Rect(col1X, statBox.y + 8, 110, 22), $"Trí Tuệ: <color=#00e5ff><b>{intVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col1BtnX, statBox.y + 8, btnSize, btnSize), canAdd)) playerStats?.AddIntelligence();

            GUI.Label(new Rect(col1X, statBox.y + 40, 110, 22), $"Sức Mạnh: <color=#ff7755><b>{strVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col1BtnX, statBox.y + 40, btnSize, btnSize), canAdd)) playerStats?.AddStrength();

            // CỘT 2 (Bên phải): Thể Lực & Nhanh Nhẹn
            float col2X = statBox.x + colW + 14f;
            float col2BtnX = statBox.x + statBox.width - btnSize - 14f;

            GUI.Label(new Rect(col2X, statBox.y + 8, 110, 22), $"Thể Lực: <color=#55ff55><b>{vitVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col2BtnX, statBox.y + 8, btnSize, btnSize), canAdd)) playerStats?.AddVitality();

            GUI.Label(new Rect(col2X, statBox.y + 40, 110, 22), $"Nhanh Nhẹn: <color=#ffdd44><b>{agiVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col2BtnX, statBox.y + 40, btnSize, btnSize), canAdd)) playerStats?.AddAgility();
            pY += 82f;

            // 2. PHẦN GIỮA: DANH SÁCH CÁC KỸ NĂNG ĐANG SỬ DỤNG
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(0, pY, viewRect.width, 18), "DANH SÁCH KỸ NĂNG ĐANG SỬ DỤNG:", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            Rect skillListBox = new Rect(0, pY, viewRect.width, 140f);
            DrawDoubleGoldFrame(skillListBox, 1);

            for (int s = 0; s < 4; s++)
            {
                var scroll = skillAimingController?.GetEquippedScroll(s);
                string sName = scroll != null ? scroll.scrollName : "(Chưa lắp cuộn trục)";
                string sInfo = scroll != null ? $"Hồi: {scroll.cooldown:F1}s | Mana: {scroll.manaCost}" : "Mở Túi Đồ để lắp";
                standardTextStyle.fontSize = 11;
                GUI.Label(new Rect(skillListBox.x + 8, skillListBox.y + 6 + s * 26, skillListBox.width - 16, 22),
                    $"⚡ <b>Ô [{s + 1}]</b>: {sName} — <color=#ffff00>{sInfo}</color>", standardTextStyle);
            }
            // Chiêu Lướt
            GUI.Label(new Rect(skillListBox.x + 8, skillListBox.y + 6 + 4 * 26, skillListBox.width - 16, 22),
                "🏃 <b>Ô [Shift]</b>: Lướt Nhanh 360° Né Đòn — <color=#ffff00>Hồi: 3.5s | Mana: 15</color>", standardTextStyle);
            pY += 148f;

            // 3. PHẦN DƯỚI CÙNG: DANH SÁCH PERK (BẨM SINH & TẬP SỰ)
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(0, pY, viewRect.width, 18), "DANH SÁCH PERK (BẨM SINH & TẬP SỰ):", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            Rect perkBox = new Rect(0, pY, viewRect.width, 120f);
            DrawDoubleGoldFrame(perkBox, 1);

            DrawPerkRow(perkBox.x + 6, perkBox.y + 6, perkBox.width - 12, "🔥", "Hỏa Thần Quyết (Perk Hệ Hỏa)", "Tăng 10% sát thương cho toàn bộ chiêu thức ma pháp thuộc tính Hỏa.");
            DrawPerkRow(perkBox.x + 6, perkBox.y + 42, perkBox.width - 12, "⚡", "Tốc Biến Tân Thủ", "Tăng 0.5m/s tốc độ chạy cơ bản khi ở trạng thái ngoài giao tranh.");
            DrawPerkRow(perkBox.x + 6, perkBox.y + 78, perkBox.width - 12, "🛡️", "Hộ Thể Kim Cang", "Giảm 5% sát thương nhận vào khi chiến đấu trong phạm vi làng.");

            GUI.EndScrollView();
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

        // ================= TAB 4: THÚ CƯNG (ĐỒNG BỘ NÚT BẤM FANTASY) =================
        private void DrawTabPets(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "QUẢN LÝ THÚ CƯNG", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            float boxSize = 130f;
            float pY = r.y + 30f;

            // TRÁI: ẢNH THÚ CƯNG LỚN
            Rect petImgRect = new Rect(r.x + 10, pY, boxSize, boxSize);
            DrawDoubleGoldFrame(petImgRect, 1);
            if (petPortraitTex != null)
            {
                GUI.DrawTexture(new Rect(petImgRect.x + 15, petImgRect.y + 15, boxSize - 30, boxSize - 30), petPortraitTex);
            }

            // PHẢI: NÚT THÁO RA VÀ NÚT THẢ THÚ
            float btnX = petImgRect.x + boxSize + 20f;
            float btnW = r.width - (btnX - r.x) - 10f;
            float btnH = 34f;

            if (DrawFantasyButton(new Rect(btnX, pY + 15, btnW, btnH), "THÁO RA (CẤT LỒNG)", 11))
            {
                var petItem = inventoryManager?.Items.Find(it => it.itemType == ItemType.Pet);
                if (petItem != null) inventoryManager?.TogglePetSummon(petItem);
            }

            if (DrawFantasyButton(new Rect(btnX, pY + 65, btnW, btnH), "THẢ THÚ (XÁC NHẬN 5S)", 11, false, new Color(1f, 0.45f, 0.45f)))
            {
                var petItem = inventoryManager?.Items.Find(it => it.itemType == ItemType.Pet);
                if (petItem != null)
                {
                    if (petItem.isEquipped) petItem.isEquipped = false;
                    inventoryManager?.RequestDropItem(petItem);
                }
            }

            // DƯỚI: THÔNG TIN CHI TIẾT
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
                "• Chuyển động: Nhấp nhô nhẹ quanh chân nhân vật\n" +
                "• Tiêu chuẩn 3-KHÔNG: Không cưỡi, Không đánh, Không bị chọn làm mục tiêu\n" +
                "• Quyền lợi: Không bị xóa hay mất khi Chuyển Sinh!", standardTextStyle);
        }

        // ================= TAB 5: CÀI ĐẶT =================
        private void DrawTabSettings(Rect r)
        {
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, r.y, r.width, 22), "CÀI ĐẶT HỆ THỐNG & GIAO DỊCH", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            autoDeclineTrade = GUI.Toggle(new Rect(r.x, r.y + 32, r.width, 26), autoDeclineTrade, " 🛡️ Tự động TỪ CHỐI mọi yêu cầu Giao Dịch (/trade)");

            GUI.Label(new Rect(r.x, r.y + 68, r.width, 100),
                $"• Trạng thái Giao Dịch: {(autoDeclineTrade ? "<color=#ff5555>ĐANG KHÓA GIAO DỊCH</color>" : "<color=#55ff55>SẴN SÀNG GIAO DỊCH</color>")}\n" +
                $"• Giới hạn tốc độ giao dịch: 2 phút / 1 người chơi.\n" +
                $"• Tỉ lệ rơi đồ khi tử nạn ngoài vùng an toàn: 5% theo GDD DOC-01.\n" +
                $"• Phím tắt: [Q] Uống máu, [E] Uống mana (hoặc Nói chuyện khi gần NPC), [Shift] Lướt (10 Mana, giảm theo AGI), [1..4] Kỹ năng.",
                tooltipDescStyle);

            // BỘ CÔNG CỤ ENGINE THIẾT KẾ (NO-CODE STUDIO)
            float devBtnW = (r.width - 12f) / 2f;
            float devY = r.y + 175f;
            
            boldWhiteStyle.fontSize = 11;
            boldWhiteStyle.normal.textColor = new Color(0.2f, 0.9f, 1f);
            GUI.Label(new Rect(r.x, devY - 22f, r.width, 20), "🛠️ BỘ CÔNG CỤ ENGINE THIẾT KẾ (NO-CODE STUDIO):", boldWhiteStyle);

            if (DrawFantasyButton(new Rect(r.x, devY, devBtnW, 36), "🛠️ TẠO ASSET [F1]", 11, ItemCreatorModal.Instance != null && ItemCreatorModal.Instance.IsOpen))
            {
                if (ItemCreatorModal.Instance != null)
                {
                    ItemCreatorModal.Instance.IsOpen = !ItemCreatorModal.Instance.IsOpen;
                }
            }

            if (DrawFantasyButton(new Rect(r.x + devBtnW + 12f, devY, devBtnW, 36), "🗺️ VẼ ĐỊA HÌNH [F2]", 11, GodlessFantasy.Core.TerrainPainterTool.Instance != null && GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode))
            {
                if (GodlessFantasy.Core.TerrainPainterTool.Instance != null)
                {
                    GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode = !GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode;
                }
            }
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

            DrawFantasyFrame(r, new Color(0.08f, 0.04f, 0.05f, 0.98f), new Color(1f, 0.25f, 0.25f, 1f), new Color(1f, 0.4f, 0.4f, 1f));

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

            string confirmText = canConfirm ? "🗑️ XÁC NHẬN XÓA" : $"⏳ Chờ ({Mathf.CeilToInt(cd)}s)";
            if (DrawFantasyButton(new Rect(r.x + 20, r.y + 120, 140, 32), confirmText, 11, canConfirm, canConfirm ? new Color(1f, 0.35f, 0.35f) : Color.gray))
            {
                if (canConfirm)
                {
                    inventoryManager?.ConfirmDropItem();
                    selectedInventoryItem = null;
                }
            }

            if (DrawFantasyButton(new Rect(r.x + 180, r.y + 120, 140, 32), "HỦY BỎ", 11))
            {
                inventoryManager?.CancelDropItem();
            }
        }

        // ================= CỬA SỔ TRÒ CHUYỆN 2 TAB (VỊ TRÍ HIỆN TẠI & THẾ GIỚI) =================
        private void DrawChatModal()
        {
            float chatW = 340f;
            float chatH = 260f;
            float chatX = Screen.width - chatW - 16f;
            float chatY = 118f;
            Rect chatRect = new Rect(chatX, chatY, chatW, chatH);

            // Nền tối đặc chống nhìn xuyên thấu
            GUI.color = new Color(0.04f, 0.05f, 0.09f, 1.0f);
            GUI.DrawTexture(chatRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(chatRect);

            // 2 TAB HEADER THEO ĐÚNG ĐẶC TẢ GDD
            float tabBtnW = 140f;
            float tabBtnH = 24f;

            bool isLocalTab = (currentChatTab == 0);
            if (DrawFantasyButton(new Rect(chatRect.x + 8, chatRect.y + 6, tabBtnW, tabBtnH), "📍 VỊ TRÍ HIỆN TẠI", 10, isLocalTab))
            {
                currentChatTab = 0;
            }

            bool isWorldTab = (currentChatTab == 1);
            if (DrawFantasyButton(new Rect(chatRect.x + 152, chatRect.y + 6, tabBtnW, tabBtnH), "🌍 THẾ GIỚI", 10, isWorldTab))
            {
                currentChatTab = 1;
            }

            // Nút đóng [X]
            if (DrawFantasyButton(new Rect(chatRect.x + chatRect.width - 32, chatRect.y + 6, 24, tabBtnH), "✕", 11))
            {
                isChatOpen = false;
            }

            // KHUNG LỊCH SỬ CHAT CỦA TAB TƯƠNG ỨNG
            Rect histRect = new Rect(chatRect.x + 8, chatRect.y + 34, chatRect.width - 16, chatRect.height - 76);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(histRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(histRect, new Color(0.35f, 0.45f, 0.6f, 0.5f), 1);

            standardTextStyle.alignment = TextAnchor.UpperLeft;
            standardTextStyle.fontSize = 11;
            string currentHistory = (currentChatTab == 0) ? localChatHistory : worldChatHistory;
            GUI.Label(new Rect(histRect.x + 6, histRect.y + 6, histRect.width - 12, histRect.height - 12), currentHistory, standardTextStyle);

            // KHUNG NHẬP TIN NHẮN & NÚT GỬI
            float sendBtnW = 54f;
            chatInputText = GUI.TextField(new Rect(chatRect.x + 8, chatRect.y + chatRect.height - 34, chatRect.width - sendBtnW - 14, 26), chatInputText);
            if (DrawFantasyButton(new Rect(chatRect.x + chatRect.width - sendBtnW - 6, chatRect.y + chatRect.height - 34, sendBtnW, 26), "GỬI", 11, true))
            {
                if (!string.IsNullOrEmpty(chatInputText))
                {
                    if (currentChatTab == 0)
                    {
                        localChatHistory += $"\n💬 [Vị Trí] [Bạn]: {chatInputText}";
                    }
                    else
                    {
                        worldChatHistory += $"\n🌍 [Thế Giới] [Bạn]: {chatInputText}";
                    }
                    chatInputText = "";
                }
            }
        }

        // ================= TOOLTIP POPUP (GIỚI HẠN MÀN HÌNH & PADDING CHUẨN) =================
        private void SetTooltip(string title, string body, Vector2 pos, bool anchorFromBottom = false)
        {
            activeTooltipTitle = title;
            activeTooltipBody = body;
            activeTooltipPos = pos;
            activeTooltipAnchorBottom = anchorFromBottom;
            showTooltip = true;
        }

        private void DrawTooltipPopup()
        {
            float w = 290f;
            if (tooltipDescStyle == null) InitStyles();
            float titleH = 22f;
            float bodyH = tooltipDescStyle.CalcHeight(new GUIContent(activeTooltipBody), w - 24f);
            float h = Mathf.Max(56f, titleH + bodyH + 18f);

            float clampedX = Mathf.Clamp(activeTooltipPos.x, 16f, Screen.width - w - 16f);
            float tooltipY = activeTooltipAnchorBottom ? (activeTooltipPos.y - h) : activeTooltipPos.y;
            float clampedY = Mathf.Clamp(tooltipY, 16f, Screen.height - h - 16f);
            Rect r = new Rect(clampedX, clampedY, w, h);

            DrawFantasyFrame(r, new Color(0.04f, 0.06f, 0.12f, 0.98f), new Color(0.85f, 0.70f, 0.30f, 0.95f), new Color(1f, 0.90f, 0.45f, 1f));

            tooltipTitleStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(r.x + 12, r.y + 8, r.width - 24, 20), activeTooltipTitle, tooltipTitleStyle);

            GUI.color = new Color(0.85f, 0.70f, 0.30f, 0.6f);
            GUI.DrawTexture(new Rect(r.x + 12, r.y + 28, r.width - 24, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            tooltipDescStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(r.x + 12, r.y + 32, r.width - 24, bodyH + 4f), activeTooltipBody, tooltipDescStyle);
        }

        // ================= TIỆN ÍCH NÚT FANTASY ĐỒNG BỘ CHUẨN MẪU USER =================
        private bool DrawFantasyButton(Rect r, string text, int fontSize = 12, bool isHighlighted = false, Color? textColorOverride = null)
        {
            bool isHovered = r.Contains(Event.current.mousePosition);

            // 1. Nền tối
            Color bgCol = isHighlighted ? new Color(0.16f, 0.22f, 0.35f, 0.98f)
                         : isHovered ? new Color(0.12f, 0.16f, 0.26f, 0.96f)
                         : new Color(0.06f, 0.08f, 0.14f, 0.94f);
            GUI.color = bgCol;
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            // 2. Viền chính vàng kim
            Color borderCol = isHighlighted ? new Color(1f, 0.88f, 0.45f, 1f)
                            : isHovered ? new Color(0.96f, 0.82f, 0.40f, 1f)
                            : new Color(0.72f, 0.58f, 0.25f, 0.92f);
            DrawOutline(r, borderCol, 1);

            // 3. Viền bevel bên trong
            if (r.width > 8 && r.height > 8)
            {
                Rect inner = new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2);
                Color innerTint = new Color(borderCol.r * 0.45f, borderCol.g * 0.45f, borderCol.b * 0.45f, 0.45f);
                DrawOutline(inner, innerTint, 1);
            }

            // 4. Các góc hoa văn nổi bật (Embossed Gold Corner Studs theo ảnh mẫu của User)
            if (r.width >= 12 && r.height >= 12)
            {
                Color cornerCol = isHighlighted || isHovered ? new Color(1f, 0.95f, 0.6f, 1f) : new Color(0.95f, 0.82f, 0.38f, 1f);
                GUI.color = cornerCol;
                int cSize = (r.height < 24) ? 2 : 3;
                GUI.DrawTexture(new Rect(r.x, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // 5. Text nhãn
            if (fantasyBtnLabelStyle == null) InitStyles();
            fantasyBtnLabelStyle.fontSize = fontSize;
            fantasyBtnLabelStyle.alignment = TextAnchor.MiddleCenter;
            fantasyBtnLabelStyle.fontStyle = FontStyle.Bold;

            // Đổ bóng chữ đen
            fantasyBtnLabelStyle.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, fantasyBtnLabelStyle);

            // Chữ chính
            Color txtCol = textColorOverride ?? (isHighlighted ? new Color(1f, 0.92f, 0.5f) : isHovered ? Color.white : new Color(0.95f, 0.85f, 0.5f));
            fantasyBtnLabelStyle.normal.textColor = txtCol;
            GUI.Label(r, text, fantasyBtnLabelStyle);

            // Xử lý Click chuột trái
            if (isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Event.current.Use();
                return true;
            }
            return false;
        }

        private void DrawFantasyFrame(Rect r, Color bgColor, Color borderColor, Color cornerColor)
        {
            GUI.color = bgColor;
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            DrawOutline(r, borderColor, 1);

            if (r.width > 8 && r.height > 8)
            {
                Rect inner = new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2);
                Color innerTint = new Color(borderColor.r * 0.45f, borderColor.g * 0.45f, borderColor.b * 0.45f, 0.45f);
                DrawOutline(inner, innerTint, 1);
            }

            if (r.width >= 12 && r.height >= 12)
            {
                GUI.color = cornerColor;
                int cSize = 3;
                GUI.DrawTexture(new Rect(r.x, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - cSize, r.y + r.height - cSize, cSize, cSize), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
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
