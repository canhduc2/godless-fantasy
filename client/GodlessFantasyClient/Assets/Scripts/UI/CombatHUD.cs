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
        [SerializeField] private PetFollower petFollower;
        [SerializeField] private BasicAttackController basicAttackController;

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
        private Texture2D hudArrowLeftTex;
        private Texture2D hudArrowRightTex;

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

        // Chat Modal State (3 Tab: Khu Vực, Thế Giới & Bang Hội)
        private bool isChatOpen = false;
        private bool shouldFocusChatInput = false;
        private int currentChatTab = 0; // 0=Khu Vực (Local), 1=Thế Giới (World), 2=Bang Hội (Guild)
        private string chatInputText = "Chào mọi người!";
        private string localChatHistory = "📢 [Khu Vực]: Chào mừng bạn đến với Làng Tân Thủ!\n💬 [Hệ Thống]: Đã vào Vùng An Toàn (Safe Zone).\n⚡ [Mẹo]: Bấm E khi lại gần NPC để nói chuyện! Phím Q/E tiêu hao bình dược.";
        private string worldChatHistory = "🌍 [Thế Giới] DarkMage: Coi chừng skill lửa của tao nhé ae!\n🌍 [Thế Giới] Knight01: Mày chưởng trúng tao là tao chém đấy :D\n🌍 [Thế Giới] ArcherX: Boss Bìa Rừng ra rồi anh em tập trung!";
        private string guildChatHistory = "🛡️ [Bang] [Chiến Thần Long]: Huynh đệ tập hợp săn Boss phó bản nào!\n🛡️ [Bang] [Băng Tâm Tiên Tử]: Đang qua bãi quái PvP đây!\n🛡️ [Bang] [Lôi Đình Quyền Vương]: Nhớ đứng gần buff số 6 để hưởng aura nhé!";

        // Guild Modal State (Cửa sổ Bang Hội chính giữa màn hình)
        private bool isGuildModalOpen = false;
        private string newGuildNameInput = "Bang SquiRRels";
        private string newGuildPassInput = "8888";
        private string newGuildSloganInput = "Huynh đệ đồng tâm, sinh tử có nhau!";
        private Vector2 guildMemberListScroll = Vector2.zero;
        private Vector2 publicGuildListScroll = Vector2.zero;
        private GodlessFantasy.Guild.GuildMember selectedProfileMember = null;
        private string guildTradeNotification = "";
        private float guildTradeNotifyTimer = 0f;

        // Trạng thái Popup Nhập Mã Code / Pass Gia Nhập Bang
        private bool isJoinCodeModalOpen = false;
        private GodlessFantasy.Guild.GuildData joiningGuildTarget = null;
        private string joinCodeInput = "";
        private string joinCodeError = "";

        // Trạng thái Popup Cài Đặt Bang Hội (Dành cho Bang Chủ)
        private bool isGuildSettingsOpen = false;
        private string settingSloganInput = "";
        private string settingPassCodeInput = "";

        // Settings States
        private bool autoDeclineTrade = false;

        // Tooltip State
        private string activeTooltipTitle = "";
        private string activeTooltipBody = "";
        private string activeTooltipKeybind = null;
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

        // FPS Counter
        private float fpsTimeElapsed = 0f;
        private int fpsFrameCount = 0;
        private int displayedFps = 60;

        public static CombatHUD Instance { get; private set; }
        private float screenDamageFlashTimer = 0f;

        public static void TriggerScreenDamageFlash()
        {
            if (Instance != null)
            {
                Instance.screenDamageFlashTimer = 0.25f;
            }
        }

        public static bool IsAnyModalBlockingScreen(float screenX, float screenY)
        {
            if (Instance == null) return false;
            if (Instance.isDrawerOpen && screenX < 490f) return true;
            if (Instance.isGuildModalOpen)
            {
                float gW = 580f, gH = 460f;
                Rect gRect = new Rect((Screen.width - gW) / 2f, (Screen.height - gH) / 2f, gW, gH);
                if (gRect.Contains(new Vector2(screenX, screenY))) return true;
            }
            if (Instance.isChatOpen)
            {
                float cW = 430f, cH = 270f;
                Rect cRect = new Rect((Screen.width - cW) / 2f, (Screen.height - cH) / 2f, cW, cH);
                if (cRect.Contains(new Vector2(screenX, screenY))) return true;
            }
            if (GodlessFantasy.Player.VillageNPC.IsAnyNPCDialogueOpen)
            {
                float nW = 460f, nH = 150f;
                Rect nRect = new Rect((Screen.width - nW) / 2f, (Screen.height - nH) / 2f, nW, nH);
                if (nRect.Contains(new Vector2(screenX, screenY))) return true;
            }
            if (Instance.inventoryManager != null && Instance.inventoryManager.IsDropModalOpen) return true;
            return false;
        }

        public static bool IsAnyModalOpen()
        {
            if (ItemCreatorModal.Instance != null && ItemCreatorModal.Instance.IsOpen) return true;
            if (GodlessFantasy.Core.TerrainPainterTool.Instance != null && GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode) return true;
            if (SkillTestingMatrixModal.Instance != null && SkillTestingMatrixModal.Instance.IsOpen) return true;
            if (GameSettingsModal.Instance != null && GameSettingsModal.Instance.IsOpen) return true;
            if (GodlessFantasy.Player.VillageNPC.IsAnyNPCDialogueOpen) return true;
            if (Instance == null) return false;
            return Instance.isGuildModalOpen || Instance.isChatOpen ||
                   (Instance.inventoryManager != null && Instance.inventoryManager.IsDropModalOpen) ||
                   Instance.selectedProfileMember != null;
        }

        public bool IsChatFocused() => isChatOpen;

        /// <summary>
        /// Kiểm tra chuột có đang trỏ vào bất kỳ phần tử UI nào (Modal, Popup, Menu trượt, Nút chức năng, HUD) không.
        /// Chống 100% tình trạng Click-to-Move bị nhận lọt qua khi đang thao tác giao diện!
        /// </summary>
        public static bool IsMouseOverAnyUI()
        {
            Vector2 mScreen = Input.mousePosition;
            Vector2 guiPos = new Vector2(mScreen.x, Screen.height - mScreen.y);
            return IsMouseOverAnyUI(guiPos);
        }

        public static bool IsMouseOverAnyUI(Vector2 guiPos)
        {
            float mx = guiPos.x;
            float my = guiPos.y;

            // 1. Tool Studio F1, Testing Matrix F3, Game Settings [O]
            if (ItemCreatorModal.Instance != null && ItemCreatorModal.Instance.IsOpen) return true;
            if (SkillTestingMatrixModal.Instance != null && SkillTestingMatrixModal.Instance.IsOpen) return true;
            if (GameSettingsModal.Instance != null && GameSettingsModal.Instance.IsOpen) return true;

            // 2. Tool Vẽ Bản Đồ F2 (thanh công cụ trên đỉnh)
            if (GodlessFantasy.Core.TerrainPainterTool.Instance != null && GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode)
            {
                float barW = Mathf.Min(Screen.width - 32f, 720f);
                float barH = 68f;
                float barX = (Screen.width - barW) / 2f;
                if (mx >= barX && mx <= barX + barW && my >= 12f && my <= 12f + barH)
                    return true;
            }

            // 3. Hộp thoại NPC
            if (GodlessFantasy.Player.VillageNPC.IsAnyNPCDialogueOpen)
            {
                float nW = 460f, nH = 150f;
                Rect nRect = new Rect((Screen.width - nW) / 2f, (Screen.height - nH) / 2f, nW, nH);
                if (nRect.Contains(guiPos)) return true;
            }

            if (Instance == null) return false;

            // 4. Modal Drop Vật phẩm (Xác nhận vứt bỏ 5s)
            if (Instance.inventoryManager != null && Instance.inventoryManager.IsDropModalOpen)
            {
                float dW = 340f, dH = 180f;
                Rect dRect = new Rect((Screen.width - dW) / 2f, (Screen.height - dH) / 2f, dW, dH);
                if (dRect.Contains(guiPos)) return true;
            }

            // 5. Cửa sổ Bang Hội (Guild Modal)
            if (Instance.isGuildModalOpen)
            {
                float gW = 580f, gH = 460f;
                Rect gRect = new Rect((Screen.width - gW) / 2f, (Screen.height - gH) / 2f, gW, gH);
                if (gRect.Contains(guiPos)) return true;
            }

            // 6. Cửa sổ Chat Modal
            if (Instance.isChatOpen)
            {
                float cW = 430f, cH = 270f;
                Rect cRect = new Rect((Screen.width - cW) / 2f, (Screen.height - cH) / 2f, cW, cH);
                if (cRect.Contains(guiPos)) return true;
            }

            // 7. Menu Trượt 5 Tab (Drawer) & Nút mở rộng
            if (Instance.isDrawerOpen)
            {
                if (mx < 490f) return true;
            }
            else
            {
                // Nút trượt Drawer bên mép trái
                float btnH = 54f;
                if (mx < 40f && Mathf.Abs(my - Screen.height / 2f) < (btnH / 2f + 5f))
                    return true;
            }

            // 8. Bảng thông tin nhân vật nay đã được dời xuống đáy màn hình cạnh Hotbar (không còn ở góc trái trên)

            // 9. Bản đồ nhỏ (Minimap) góc phải trên & nút chat
            if (mx > Screen.width - 200f && my < 165f)
                return true;

            // 10. Widget Active Buffs (dưới minimap mép phải)
            if (Instance.playerStats != null && Instance.playerStats.ActiveBuffs != null && Instance.playerStats.ActiveBuffs.Count > 0)
            {
                float buffH = Instance.playerStats.ActiveBuffs.Count * 40f + 10f;
                if (mx > Screen.width - 190f && my >= 130f && my <= 135f + buffH)
                    return true;
            }

            // 11. Thanh chiêu thức dưới đáy màn hình (Skill Bar) & Thanh tin tức Server
            if (my > Screen.height - 110f)
                return true;

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
            if (petFollower == null) petFollower = FindAnyObjectByType<PetFollower>();
            if (basicAttackController == null) basicAttackController = FindAnyObjectByType<BasicAttackController>();
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

        public static void RegisterExternalIcon(string resName, Texture2D tex)
        {
            if (string.IsNullOrEmpty(resName) || tex == null) return;
            if (Instance != null) Instance.iconCache[resName] = tex;
        }

        private Texture2D LoadTextureSafe(string resName)
        {
            if (string.IsNullOrEmpty(resName)) return null;
            if (iconCache.TryGetValue(resName, out var cached) && cached != null) return cached;

            if (ItemCreatorModal.Instance != null)
            {
                var studioTex = ItemCreatorModal.Instance.GetCachedTexture(resName);
                if (studioTex != null && studioTex != Texture2D.whiteTexture)
                {
                    iconCache[resName] = studioTex;
                    return studioTex;
                }
            }

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
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Overflow,
                normal = { textColor = Color.white }
            };

            standardTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                wordWrap = false,
                clipping = TextClipping.Clip,
                normal = { textColor = new Color(0.92f, 0.95f, 1f) }
            };

            serverNewsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };

            skillCdStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
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
                fontSize = 14,
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
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Overflow,
                normal = { textColor = new Color(0.8f, 0.9f, 1f) }
            };

            tabBtnStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            fantasyBtnLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.90f, 0.45f) }
            };

            keybindBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
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
                cachedMonsters = FindObjectsByType<MonsterTarget>(FindObjectsInactive.Exclude);
                if (cachedNPC == null) cachedNPC = FindAnyObjectByType<VillageNPC>();
            }

            if (minimapPingTimer > 0f)
            {
                minimapPingTimer -= Time.deltaTime;
            }

            if (screenDamageFlashTimer > 0f)
            {
                screenDamageFlashTimer -= Time.deltaTime;
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

            // Phím M: Phím tắt nhanh mở hoặc đóng Menu tính năng (Drawer)
            if (Input.GetKeyDown(KeyCode.M))
            {
                bool isTypingChat = isChatOpen;
                bool isCreatingItem = ItemCreatorModal.Instance != null && ItemCreatorModal.Instance.IsOpen;
                if (!isTypingChat && !isCreatingItem && !isGuildModalOpen && (inventoryManager == null || !inventoryManager.IsDropModalOpen))
                {
                    isDrawerOpen = !isDrawerOpen;
                }
            }

            // Phím Enter: Gửi tin nhắn nếu đang mở Chat, hoặc mở nhanh cửa sổ Chat nếu đang đóng
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (isChatOpen)
                {
                    SendCurrentChatMessage();
                }
                else if (!isDrawerOpen && !isGuildModalOpen && (inventoryManager == null || !inventoryManager.IsDropModalOpen))
                {
                    isChatOpen = true;
                    shouldFocusChatInput = true;
                }
            }

            // Chặn click xuyên thế giới ngay lập tức trong Update
            PlayerMovement.BlockWorldClick = IsMouseOverAnyUI();
        }

        private void SendCurrentChatMessage()
        {
            if (!string.IsNullOrEmpty(chatInputText) && !string.IsNullOrEmpty(chatInputText.Trim()))
            {
                string pName = playerStats?.CharacterName ?? "Bạn";
                if (currentChatTab == 0)
                {
                    localChatHistory += $"\n💬 [Khu Vực] [{pName}]: {chatInputText.Trim()}";
                }
                else if (currentChatTab == 1)
                {
                    worldChatHistory += $"\n🌍 [Thế Giới] [{pName}]: {chatInputText.Trim()}";
                }
                else
                {
                    guildChatHistory += $"\n🛡️ [Bang] [{pName}]: {chatInputText.Trim()}";
                }
                chatInputText = "";
                shouldFocusChatInput = true;
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

            // Chặn Click-to-Move xuyên màn hình khi chuột ở trên bất kỳ vùng UI nào
            PlayerMovement.BlockWorldClick = IsMouseOverAnyUI(Event.current.mousePosition);

            // 1. LỚP HUD CƠ BẢN (Depth 10): Nằm trên thế giới (Depth 20), dưới Menu/Modal (Depth 0)
            GUI.depth = FantasyUITheme.DepthBaseHUD;

            DrawBottomCenterHUD(); // Bảng thông tin Máu/Mana + 5 Ô chiêu thức căn giữa đối xứng sát đáy (luôn hiển thị)
            DrawTopRightMinimapAndChat();
            DrawMidLeftDrawerButton();
            DrawActiveBuffsWidget(); // Widget hiển thị trạng thái hiệu ứng Buff ở mép phải dưới minimap
            DrawBottomCenterServerNews(); // Thanh tin tức gọn nhỏ sát đáy chạy chữ từ phải sang trái!

            // 2. LỚP GIAO DIỆN CÀI ĐẶT / MENU / MODAL (Depth 0): Mặc nhiên luôn ở trên cùng của HUD
            GUI.depth = FantasyUITheme.DepthModal;
            if (isDrawerOpen) DrawDrawerModal();
            if (isChatOpen) DrawChatModal();
            if (isGuildModalOpen) DrawGuildModal();

            // 3. LỚP TRÊN CÙNG TUYỆT ĐỐI (Depth -100): HỘP THOẠI XÁC NHẬN 5S & TOOLTIP
            GUI.depth = FantasyUITheme.DepthPopupTooltip;
            if (showTooltip) DrawTooltipPopup();

            // POPUP DOUBLE CHECK 5S KHI VỨT / XÓA VẬT PHẨM
            if (inventoryManager != null && inventoryManager.IsDropModalOpen)
            {
                DrawDoubleCheckDropModal();
            }

            // 4. HIỆU ỨNG MÀN HÌNH CHỚP ĐỎ KHI NGƯỜI CHƠI BỊ NHẬN SÁT THƯƠNG
            if (screenDamageFlashTimer > 0f)
            {
                GUI.depth = -200; // Trên cùng tuyệt đối
                float flashAlpha = Mathf.Clamp01(screenDamageFlashTimer / 0.25f) * 0.42f;
                GUI.color = new Color(0.95f, 0.1f, 0.1f, flashAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // 5. ĐỒNG HỒ FPS & TRẠNG THÁI HIỂN THỊ
            if (GodlessFantasy.Core.GameSettings.ShowFpsCounter)
            {
                GUI.depth = -150;
                fpsFrameCount++;
                fpsTimeElapsed += Time.unscaledDeltaTime;
                if (fpsTimeElapsed >= 0.5f)
                {
                    displayedFps = Mathf.RoundToInt(fpsFrameCount / fpsTimeElapsed);
                    fpsFrameCount = 0;
                    fpsTimeElapsed = 0f;
                }

                Color fpsCol = displayedFps >= 50 ? new Color(0.35f, 0.95f, 0.45f) :
                               displayedFps >= 30 ? new Color(1f, 0.85f, 0.25f) : new Color(1f, 0.35f, 0.35f);
                statBadgeStyle.alignment = TextAnchor.MiddleLeft;
                statBadgeStyle.normal.textColor = fpsCol;
                GUI.Label(new Rect(14f, 8f, 130f, 18f), $"🟢 {displayedFps} FPS", statBadgeStyle);
                statBadgeStyle.normal.textColor = Color.white;
            }
        }

        // ================= 1. CỤM HUD DƯỚI ĐÁY: BẢNG THÔNG TIN HP/MANA + THANH KỸ NĂNG CĂN GIỮA ĐỐI XỨNG =================
        private void DrawBottomCenterHUD()
        {
            float panelW = 215f;
            float panelH = 72f;
            float slotSize = 54f;
            float slotGap = 9f;
            int totalSlots = 5;
            float hotbarW = totalSlots * slotSize + (totalSlots - 1) * slotGap; // 5 * 54 + 4 * 9 = 270 + 36 = 306f
            float panelToHotbarGap = 10f;

            float totalW = panelW + panelToHotbarGap + hotbarW; // 215 + 10 + 306 = 531f
            float startX = (Screen.width - totalW) / 2f;

            // Đáy của ô chiêu thức cách đáy màn hình 30px (nằm ngay phía trên thanh tin tức 22px tại Screen.height - 24f)
            float slotY = Screen.height - slotSize - 30f; // Screen.height - 84f
            float topY = slotY - 18f;                     // Screen.height - 102f (khớp đỉnh với badge tiêu hao mana -15mn)

            // 1. Bảng chỉ số Máu / Mana / Nhân vật (nằm bên trái cụm hotbar, luôn hiển thị cả khi mở Menu)
            DrawBottomPlayerPanel(new Rect(startX, topY, panelW, panelH));

            // 2. Thanh 5 ô Chiêu thức [1][2][3][4][SHIFT] (nằm bên phải bảng chỉ số, cách 10px)
            float hotbarStartX = startX + panelW + panelToHotbarGap;
            DrawBottomHotbar(hotbarStartX, slotY, slotSize, slotGap);

            // 3. Hai cánh mũi tên trang trí Pixel Art kẹp 2 bên mép ngoài
            DrawHudWingArrows(startX, hotbarStartX + hotbarW, slotY, slotSize);
        }

        private void DrawHudWingArrows(float leftPanelX, float rightHotbarX, float slotY, float slotSize)
        {
            EnsureHudArrowTextures();
            if (hudArrowLeftTex == null || hudArrowRightTex == null) return;

            float arrowW = 22f;
            float arrowH = slotSize; // 54f (khớp chuẩn độ cao cụm ô chiêu thức)
            float arrowGap = 6f;     // Khoảng cách nhỏ thanh thoát theo yêu cầu của user

            // Cánh mũi tên bên trái (cách mép ngoài Bảng nhân vật 6px, chĩa sang trái ◀)
            Rect leftRect = new Rect(leftPanelX - arrowW - arrowGap, slotY, arrowW, arrowH);
            GUI.color = Color.white;
            GUI.DrawTexture(leftRect, hudArrowLeftTex);

            // Cánh mũi tên bên phải (cách mép ngoài ô chiêu [D] 6px, chĩa sang phải ▶)
            Rect rightRect = new Rect(rightHotbarX + arrowGap, slotY, arrowW, arrowH);
            GUI.DrawTexture(rightRect, hudArrowRightTex);
        }

        private void EnsureHudArrowTextures()
        {
            if (hudArrowLeftTex == null)
            {
                hudArrowLeftTex = LoadTextureSafe("hud_arrow_left") ?? GenerateProceduralArrowTex(true);
            }
            if (hudArrowRightTex == null)
            {
                hudArrowRightTex = LoadTextureSafe("hud_arrow_right") ?? GenerateProceduralArrowTex(false);
            }
        }

        private Texture2D GenerateProceduralArrowTex(bool isLeft)
        {
            int w = 22;
            int h = 54;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color cTrans = new Color(0, 0, 0, 0);
            Color cOutline = new Color(0.024f, 0.031f, 0.055f, 1f);
            Color cShadowDark = new Color(0.078f, 0.094f, 0.149f, 1f);
            Color cGoldHi = new Color(1f, 0.96f, 0.69f, 1f);
            Color cGoldMid = new Color(0.94f, 0.77f, 0.24f, 1f);
            Color cGoldDeep = new Color(0.72f, 0.53f, 0.12f, 1f);
            Color cGoldShadow = new Color(0.43f, 0.29f, 0.06f, 1f);
            Color cPlateHi = new Color(0.13f, 0.19f, 0.30f, 1f);
            Color cPlateMid = new Color(0.07f, 0.10f, 0.17f, 1f);
            Color cPlateDark = new Color(0.04f, 0.05f, 0.09f, 1f);
            Color cChevHi = new Color(1f, 0.98f, 0.78f, 1f);
            Color cChevMid = new Color(1f, 0.71f, 0.16f, 1f);
            Color cChevDeep = new Color(0.78f, 0.43f, 0.06f, 1f);
            Color cStudHi = new Color(1f, 1f, 0.86f, 1f);
            Color cStudMid = new Color(0.96f, 0.82f, 0.31f, 1f);
            Color cStudDark = new Color(0.55f, 0.37f, 0.08f, 1f);

            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = cTrans;

            int center_y1 = 26;
            int center_y2 = 27;

            for (int y = 0; y < h; y++)
            {
                int dy = y <= center_y1 ? (center_y1 - y) : (y - center_y2);
                int outer_x = 1;
                if (dy == 0) outer_x = 1;
                else if (dy <= 3) outer_x = 1 + dy;
                else if (dy <= 10) outer_x = 3 + (int)((dy - 3) * 0.75f);
                else if (dy <= 20) outer_x = 8 + (int)((dy - 10) * 0.9f);
                else outer_x = 17 + (int)((dy - 20) * 0.65f);
                outer_x = Mathf.Min(outer_x, w - 1);

                for (int x = outer_x; x < w; x++)
                {
                    Color px = cPlateMid;
                    if (x == outer_x || (x == w - 1 && dy >= 24) || y == 0 || y == h - 1)
                    {
                        px = cOutline;
                    }
                    else
                    {
                        bool isGoldEdge = (x == outer_x + 1 || (x == outer_x + 2 && dy < 20));
                        bool isTopEdge = (y == 1 || (y == 2 && x > outer_x + 2));
                        bool isBotEdge = (y == h - 2 || (y == h - 3 && x > outer_x + 2));

                        if (isGoldEdge || isTopEdge || isBotEdge)
                        {
                            if (y < center_y1)
                                px = (isTopEdge || (x == outer_x + 1 && dy > 10)) ? cGoldHi : cGoldMid;
                            else if (y > center_y2)
                                px = isBotEdge ? cGoldShadow : cGoldDeep;
                            else
                                px = (x == outer_x + 1) ? cGoldHi : cGoldMid;
                        }
                        else if (x == outer_x + 2 || (y == 2 && y < center_y1) || (y == h - 3 && y > center_y2))
                        {
                            px = y > center_y2 ? cShadowDark : cPlateHi;
                        }
                        else
                        {
                            if (y < center_y1)
                                px = (x > 14 && dy > 12) ? cPlateHi : cPlateMid;
                            else
                                px = (x > 14 && dy > 12) ? cPlateDark : cPlateMid;
                        }
                    }

                    int writeX = isLeft ? x : (w - 1 - x);
                    int texY = h - 1 - y; // Unity Texture2D bottom-up
                    cols[texY * w + writeX] = px;
                }
            }

            // Inner chevron
            int chev_tip_x = 7;
            for (int cdy = 0; cdy < 16; cdy++)
            {
                int cy_top = center_y1 - cdy;
                int cy_bot = center_y2 + cdy;
                int cx = chev_tip_x + (int)(cdy * 0.72f);
                if (cx >= 0 && cx < w - 3)
                {
                    if (cdy == 0)
                    {
                        SetPixelSafe(cols, w, h, isLeft ? cx : (w - 1 - cx), h - 1 - center_y1, cChevHi);
                        SetPixelSafe(cols, w, h, isLeft ? cx : (w - 1 - cx), h - 1 - center_y2, cChevHi);
                        SetPixelSafe(cols, w, h, isLeft ? cx + 1 : (w - 1 - (cx + 1)), h - 1 - center_y1, cChevMid);
                        SetPixelSafe(cols, w, h, isLeft ? cx + 1 : (w - 1 - (cx + 1)), h - 1 - center_y2, cChevMid);
                    }
                    else
                    {
                        if (cy_top >= 0 && cy_top < h)
                        {
                            SetPixelSafe(cols, w, h, isLeft ? cx : (w - 1 - cx), h - 1 - cy_top, cdy < 8 ? cChevHi : cChevMid);
                            if (cx + 1 < w - 2) SetPixelSafe(cols, w, h, isLeft ? cx + 1 : (w - 1 - (cx + 1)), h - 1 - cy_top, cChevDeep);
                        }
                        if (cy_bot >= 0 && cy_bot < h)
                        {
                            SetPixelSafe(cols, w, h, isLeft ? cx : (w - 1 - cx), h - 1 - cy_bot, cdy < 8 ? cChevMid : cChevDeep);
                            if (cx + 1 < w - 2) SetPixelSafe(cols, w, h, isLeft ? cx + 1 : (w - 1 - (cx + 1)), h - 1 - cy_bot, cChevDeep);
                        }
                    }
                }
            }

            // Rivets
            int[] studYs = { 8, h - 10, 26 };
            foreach (int sy in studYs)
            {
                int sx = 18;
                SetPixelSafe(cols, w, h, isLeft ? sx : (w - 1 - sx), h - 1 - sy, cStudHi);
                SetPixelSafe(cols, w, h, isLeft ? sx + 1 : (w - 1 - (sx + 1)), h - 1 - sy, cStudMid);
                SetPixelSafe(cols, w, h, isLeft ? sx : (w - 1 - sx), h - 1 - (sy + 1), cStudMid);
                SetPixelSafe(cols, w, h, isLeft ? sx + 1 : (w - 1 - (sx + 1)), h - 1 - (sy + 1), cStudDark);
            }

            tex.SetPixels(cols);
            tex.Apply();
            return tex;
        }

        private static void SetPixelSafe(Color[] cols, int w, int h, int x, int y, Color c)
        {
            if (x >= 0 && x < w && y >= 0 && y < h)
            {
                cols[y * w + x] = c;
            }
        }

        private void DrawBottomPlayerPanel(Rect panel)
        {
            string charClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
            string rawName = playerStats != null ? playerStats.CharacterName : "Dũng Sĩ SquiRRels";

            string titlePrefix = (playerStats != null && playerStats.EquippedTitle != null)
                ? $"<color=#{ColorUtility.ToHtmlStringRGB(playerStats.EquippedTitle.titleColor)}>«{playerStats.EquippedTitle.name}»</color> "
                : "";

            string headerText = $"{titlePrefix}<color=#ffffff>{rawName}</color> · [<color=#00e5ff>{charClass}</color>]";

            string elem = playerStats != null ? playerStats.Element : "Hỏa";
            if (elem.Contains("(")) elem = elem.Substring(0, elem.IndexOf('(')).Trim(); // Bỏ (Fire)

            int str = playerStats != null ? playerStats.Strength : 8;
            int intel = playerStats != null ? playerStats.Intelligence : 12;
            int vit = playerStats != null ? playerStats.Vitality : 10;
            int agi = playerStats != null ? playerStats.Agility : 9;
            string statsText = $"Thuộc tính: <color=#ff6633>{elem}</color> | INT:{intel} · STR:{str} · VIT:{vit} · AGI:{agi}";

            boldWhiteStyle.fontSize = 11;
            statBadgeStyle.fontSize = 10;

            DrawDoubleGoldFrame(panel, 1);

            // Avatar Box 42x42
            Rect av = new Rect(panel.x + 6, panel.y + 5, 42, 42);
            GUI.color = new Color(0.08f, 0.12f, 0.20f);
            GUI.DrawTexture(av, Texture2D.whiteTexture);
            GUI.color = Color.white;
            Texture2D activePortrait = GodlessFantasy.Player.CharacterVisualController8Dir.Instance?.GetPortraitTexture() ?? playerPortraitTex;
            if (activePortrait != null)
            {
                GUI.DrawTexture(new Rect(av.x + 2, av.y + 2, av.width - 4, av.height - 4), activePortrait);
            }

            // Badge Level nằm ngay dưới Avatar (không che mặt nhân vật), thẳng hàng với dòng chỉ số bên cạnh
            int lv = playerStats != null ? playerStats.Level : 1;
            Rect lvBadge = new Rect(av.x, av.y + av.height + 4, av.width, 16);
            GUI.color = new Color(0.85f, 0.25f, 0.10f, 0.98f);
            GUI.DrawTexture(lvBadge, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(lvBadge, new Color(1f, 0.88f, 0.40f, 1f), 1);

            var lvStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                normal = { textColor = Color.white }
            };

            // Chạy chữ tự động (Auto-Scroll Marquee) lặp vòng mượt mà theo yêu cầu
            DrawAutoScrollText(new Rect(lvBadge.x + 1, lvBadge.y, lvBadge.width - 2, lvBadge.height), $"Lv.{lv}", lvStyle, speed: 20f, textColor: Color.white, gap: 14f, forceScroll: true);

            float rightColX = panel.x + 53f;
            float rightColW = panel.width - 58f; // 215 - 58 = 157px

            // DÒNG 1: DANH HIỆU + TÊN NHÂN VẬT & [HỆ]
            boldWhiteStyle.alignment = TextAnchor.MiddleLeft;
            boldWhiteStyle.fontSize = 10;
            boldWhiteStyle.wordWrap = false;
            DrawAutoScrollText(new Rect(rightColX, panel.y + 4, rightColW, 15), headerText, boldWhiteStyle, 25f);

            // Thanh Máu HP (Thu gọn width theo đúng bố cục layout)
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int maxHp = playerHealth != null ? playerHealth.MaxHealth : 100;
            Rect hpRect = new Rect(rightColX, panel.y + 20, rightColW, 13);
            DrawInteractiveBar(hpRect, hp, maxHp, hpFillTex, $"HP {hp}/{maxHp} [Bấm Q]", () => {
                if (playerStats != null) playerStats.UseHpPotion();
            });

            // Thanh Mana (Thu gọn width theo đúng bố cục layout)
            int mana = playerStats != null ? playerStats.CurrentMana : 80;
            int maxMana = playerStats != null ? playerStats.MaxMana : 100;
            Rect manaRect = new Rect(rightColX, panel.y + 35, rightColW, 13);
            DrawInteractiveBar(manaRect, mana, maxMana, manaFillTex, $"MANA {mana}/{maxMana} [Bấm E]", () => {
                if (playerStats != null) playerStats.UseManaPotion();
            });

            // DÒNG DƯỚI CÙNG: THUỘC TÍNH (HỎA) VÀ 4 CHỈ SỐ GỐC (Nằm ngang hàng với Badge Level, tự động chạy chữ nếu dài)
            statBadgeStyle.alignment = TextAnchor.MiddleLeft;
            statBadgeStyle.fontSize = 9;
            statBadgeStyle.clipping = TextClipping.Clip;
            Rect row4 = new Rect(rightColX, panel.y + 51, rightColW, 16);
            DrawAutoScrollText(row4, statsText, statBadgeStyle, 20f);
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
            standardTextStyle.fontSize = 11;
            standardTextStyle.fontStyle = FontStyle.Bold;
            GUI.Label(r, text, standardTextStyle);
            standardTextStyle.fontStyle = FontStyle.Normal;

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
                string newsMsg = "⚔️ SquiRRels Studio chào đón dũng sĩ! · 🗺️ Khám phá Làng Khởi Đầu và Rừng Quái Vật · 🎒 Mở Hành Trang để trang bị cuộn trục bí kíp!";
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

            // 4.5. Guild Teammates Dots (Chấm màu xanh ngọc đồng đội đặc trưng trên Minimap)
            var gm = GodlessFantasy.Guild.GuildManager.Instance;
            if (gm != null && gm.IsInGuild && gm.CurrentGuild != null)
            {
                foreach (var member in gm.CurrentGuild.members)
                {
                    if (member.isOnline && !member.memberName.Contains("SquiRRels") && member.mapPosition != Vector2.zero)
                    {
                        Vector2 allyPos = WorldToMinimapPos(member.mapPosition, mapX, mapY, mapW, mapH, minWx, maxWx, minWy, maxWy);
                        // Chấm màu xanh ngọc đồng minh nổi bật
                        GUI.color = new Color(0.2f, 0.95f, 0.65f, 1f);
                        GUI.DrawTexture(new Rect(allyPos.x - 3, allyPos.y - 3, 6, 6), Texture2D.whiteTexture);
                        DrawOutline(new Rect(allyPos.x - 3, allyPos.y - 3, 6, 6), new Color(0.1f, 0.4f, 0.2f), 1);
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

            // 1. Nút Chat [💬]
            float iconBtnSize = 28f;
            float btnSpacing = 4f;
            float startBtnY = mapY + 2f;

            Rect chatRect = new Rect(mapX - iconBtnSize - 8f, startBtnY, iconBtnSize, iconBtnSize);
            if (DrawFantasyButton(chatRect, "💬", 12, isChatOpen))
            {
                isChatOpen = !isChatOpen;
                if (isChatOpen) shouldFocusChatInput = true;
            }

            // 2. Nút Cài Đặt Hệ Thống & Âm Thanh [⚙️]
            Rect settingsRect = new Rect(mapX - iconBtnSize - 8f, startBtnY + iconBtnSize + btnSpacing, iconBtnSize, iconBtnSize);
            bool isSettingsOpen = GameSettingsModal.Instance != null && GameSettingsModal.Instance.IsOpen;
            if (DrawFantasyButton(settingsRect, "⚙️", 12, isSettingsOpen, new Color(1f, 0.85f, 0.35f)))
            {
                if (GameSettingsModal.Instance != null)
                {
                    GameSettingsModal.Instance.IsOpen = !GameSettingsModal.Instance.IsOpen;
                }
            }

            // 3. Nút Studio 140 Kỹ Năng & Live Arena [⚡]
            Rect matrixRect = new Rect(mapX - iconBtnSize - 8f, startBtnY + (iconBtnSize + btnSpacing) * 2f, iconBtnSize, iconBtnSize);
            bool isMatrixOpen = SkillTestingMatrixModal.Instance != null && SkillTestingMatrixModal.Instance.IsOpen;
            if (DrawFantasyButton(matrixRect, "⚡", 12, isMatrixOpen, new Color(1.0f, 0.45f, 0.2f)))
            {
                if (SkillTestingMatrixModal.Instance != null)
                {
                    SkillTestingMatrixModal.Instance.IsOpen = !SkillTestingMatrixModal.Instance.IsOpen;
                }
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

            if (triRect.Contains(Event.current.mousePosition))
            {
                SetTooltip("Menu Tính Năng", isDrawerOpen ? "Bấm hoặc gõ phím M để thu gọn Menu" : "Bấm hoặc gõ phím M để mở Menu", new Vector2(triRect.x + triRect.width + 12f, triRect.y + 12f), false, "[Phím M]");
            }

            if (DrawFantasyButton(triRect, isDrawerOpen ? "<" : ">", 16, isDrawerOpen))
            {
                isDrawerOpen = !isDrawerOpen;
            }
        }

        // ================= 4.5. WIDGET TRẠNG THÁI HIỆU ỨNG (ACTIVE BUFFS WIDGET) =================
        private void DrawActiveBuffsWidget()
        {
            if (playerStats == null || playerStats.ActiveBuffs == null || playerStats.ActiveBuffs.Count == 0) return;

            // ĐẶT SÁT MÉP PHẢI MÀN HÌNH (DƯỚI MINIMAP) THEO ĐÚNG HÌNH VẼ USER
            float cardW = 165f;
            float cardH = 34f;
            float startX = Screen.width - cardW - 12f;
            float startY = 135f;
            float cardGap = 6f;

            for (int i = 0; i < playerStats.ActiveBuffs.Count; i++)
            {
                var buff = playerStats.ActiveBuffs[i];
                if (buff == null) continue;

                Rect cardRect = new Rect(startX, startY + i * (cardH + cardGap), cardW, cardH);

                // Nền card
                GUI.color = new Color(0.04f, 0.07f, 0.12f, 0.90f);
                GUI.DrawTexture(cardRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                // Viền card (nhấp nháy nếu sắp hết giờ <= 3.0s)
                Color borderCol = (buff.remainingTime <= 3.0f && Mathf.PingPong(Time.time * 6f, 1f) > 0.5f)
                    ? new Color(1f, 0.3f, 0.3f, 0.95f)
                    : new Color(0.85f, 0.70f, 0.30f, 0.75f);
                DrawOutline(cardRect, borderCol, 1);

                // Icon buff 26x26
                Rect iconRect = new Rect(cardRect.x + 4, cardRect.y + 4, 26, 26);
                Texture2D icon = LoadTextureSafe(buff.iconName);
                if (icon != null) GUI.DrawTexture(iconRect, icon);
                Texture2D frameSlotTex = LoadTextureSafe("ui_frame_slot");
                if (frameSlotTex != null) GUI.DrawTexture(iconRect, frameSlotTex);

                // Thời gian đếm ngược — sát mép phải tuyệt đối
                float timeW = 38f;
                float timeX = cardRect.xMax - timeW - 2f; // 2px padding từ mép phải
                Rect timeRect = new Rect(timeX, cardRect.y + 4f, timeW, 14f);

                standardTextStyle.alignment = TextAnchor.MiddleRight;
                standardTextStyle.fontSize = 10;
                standardTextStyle.fontStyle = FontStyle.Bold;
                standardTextStyle.normal.textColor = (buff.remainingTime <= 3.0f) ? new Color(1f, 0.35f, 0.35f) : new Color(1f, 0.90f, 0.45f);
                GUI.Label(timeRect, $"{buff.remainingTime:0.0}s", standardTextStyle);
                standardTextStyle.fontStyle = FontStyle.Normal;

                // Tên buff — chiếm toàn bộ khoảng giữa icon và time label
                float textX = iconRect.xMax + 5f;
                float nameW = Mathf.Max(40f, timeX - textX - 2f);
                Rect nameRect = new Rect(textX, cardRect.y + 3f, nameW, 14f);

                boldWhiteStyle.alignment = TextAnchor.MiddleLeft;
                boldWhiteStyle.fontSize = 10;
                boldWhiteStyle.normal.textColor = new Color(0.40f, 0.92f, 1.0f);
                DrawAutoScrollText(nameRect, buff.name, boldWhiteStyle, 25f, new Color(0.40f, 0.92f, 1.0f));

                // Dòng hiệu ứng bổ trợ phía dưới (cùng chiều rộng với tên)
                Rect subRect = new Rect(textX, cardRect.y + 16f, nameW, 13f);
                standardTextStyle.alignment = TextAnchor.MiddleLeft;
                standardTextStyle.fontSize = 9;
                standardTextStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
                string effectTag = buff.statValue > 0 ? $"+{buff.statValue:0}% Sát Thương" : "Buff Tự Thân";
                GUI.Label(subRect, effectTag, standardTextStyle);
                standardTextStyle.normal.textColor = Color.white;

                // Thanh tiến trình nhỏ dưới đáy card (full width)
                float progress = Mathf.Clamp01(buff.remainingTime / Mathf.Max(0.1f, buff.duration));
                Rect progBar = new Rect(cardRect.x + 2, cardRect.yMax - 3, (cardW - 4) * progress, 2);
                GUI.color = new Color(0.0f, 0.9f, 0.95f, 0.85f);
                GUI.DrawTexture(progBar, Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }

        // ================= 5. THANH 5 Ô CHIÊU THỨC KẾ BÊN BẢNG CHỈ SỐ =================
        private void DrawBottomHotbar(float startX, float startY, float slotSize, float gap)
        {
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
                // Nền slot tối
                GUI.color = new Color(0.06f, 0.08f, 0.12f, 1f);
                GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), Texture2D.whiteTexture);
                GUI.color = Color.white;

                // 1. Vẽ nhân icon ở tâm giữa
                Texture2D icon = LoadTextureSafe(scroll.iconName);
                if (icon == null) icon = LoadTextureSafe(scroll.scrollIconName);
                if (icon != null) GUI.DrawTexture(new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8), icon);

                // 2. Phủ Khung Viền Nút Chiêu Thức Độc Nhất (ui_frame_slot)
                Texture2D frameSlotTex = LoadTextureSafe("ui_frame_slot");
                if (frameSlotTex != null) GUI.DrawTexture(r, frameSlotTex);
                DrawOutline(r, scroll.tierColor, 2);

                // HIỂN THỊ TIÊU HAO MANA Ở PHÍA TRÊN Ô SKILL THEO YÊU CẦU: "-32mn"
                Rect manaTopBadge = new Rect(r.x + (r.width - 48f) / 2f, r.y - 18f, 48f, 15f);
                GUI.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);
                GUI.DrawTexture(manaTopBadge, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawOutline(manaTopBadge, new Color(0.2f, 0.7f, 1.0f, 0.75f), 1);

                statBadgeStyle.fontSize = 10;
                statBadgeStyle.fontStyle = FontStyle.Bold;
                statBadgeStyle.alignment = TextAnchor.MiddleCenter;

                // Đổ bóng chữ đen
                statBadgeStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(manaTopBadge.x + 1, manaTopBadge.y + 1, manaTopBadge.width, manaTopBadge.height), $"-{scroll.manaCost}mn", statBadgeStyle);

                // Chữ xanh mana sáng nổi bật
                statBadgeStyle.normal.textColor = new Color(0.35f, 0.88f, 1.0f);
                GUI.Label(manaTopBadge, $"-{scroll.manaCost}mn", statBadgeStyle);
                statBadgeStyle.fontStyle = FontStyle.Normal;
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
                                     $"• Tiêu hao: {scroll.manaCost} Mana | Hồi: {scroll.cooldown:F1}s | Tầm đánh: <color=#00e5ff>{scroll.RangeFormattedText}</color>\n" +
                                     $"• Sát thương: {scroll.baseDamage} + {Mathf.RoundToInt(scroll.intelligenceScaling * 100)}% Trí Tuệ = <color=#ffff00>{dmg} ST</color>\n" +
                                     $"• {scroll.description}";
                    SetTooltip(scroll.scrollName, tipBody, new Vector2(Mathf.Clamp(r.x + (r.width - 340f) / 2f, 16f, Screen.width - 340f - 16f), r.y - 8f), true, $"Phím [{slotNumber}]");
                }
                else
                {
                    SetTooltip("Ô Kỹ Năng", "Ô trống (Mở Túi Đồ để trang bị)", new Vector2(Mathf.Clamp(r.x + (r.width - 340f) / 2f, 16f, Screen.width - 340f - 16f), r.y - 8f), true, $"Phím [{slotNumber}]");
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

            // Nền slot tối
            GUI.color = new Color(0.06f, 0.08f, 0.12f, 1f);
            GUI.DrawTexture(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (dashIcon != null) GUI.DrawTexture(new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8), dashIcon);

            // Khung viền slot
            Texture2D frameSlotTex = LoadTextureSafe("ui_frame_slot");
            if (frameSlotTex != null) GUI.DrawTexture(r, frameSlotTex);

            // Tiêu hao mana của lướt trên đầu ô: -10mn
            Rect dashManaBadge = new Rect(r.x + (r.width - 48f) / 2f, r.y - 18f, 48f, 15f);
            GUI.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);
            GUI.DrawTexture(dashManaBadge, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(dashManaBadge, new Color(0.2f, 0.7f, 1.0f, 0.75f), 1);

            statBadgeStyle.fontSize = 10;
            statBadgeStyle.fontStyle = FontStyle.Bold;
            statBadgeStyle.alignment = TextAnchor.MiddleCenter;
            statBadgeStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(dashManaBadge.x + 1, dashManaBadge.y + 1, dashManaBadge.width, dashManaBadge.height), "-10mn", statBadgeStyle);
            statBadgeStyle.normal.textColor = new Color(0.35f, 0.88f, 1.0f);
            GUI.Label(dashManaBadge, "-10mn", statBadgeStyle);
            statBadgeStyle.fontStyle = FontStyle.Normal;

            float cdRemaining = playerMovement != null ? playerMovement.DashCooldownRemaining : 0f;
            if (cdRemaining > 0.05f)
            {
                GUI.DrawTexture(r, cdOverlayTex != null ? cdOverlayTex : Texture2D.whiteTexture);
                GUI.Label(r, $"{cdRemaining:F1}s", skillCdStyle);
            }

            DrawKeybindBadge(new Rect(r.x + 7, r.y + 6, 26, 18), "D");

            if (r.Contains(Event.current.mousePosition))
            {
                float curCd = playerMovement != null ? playerMovement.CurrentDashCooldown : 3.5f;
                SetTooltip("Lướt Nhanh (Dash)",
                    "• Cơ chế: Lướt 360 độ theo con trỏ chuột\n" +
                    "• Tiêu hao: 10 Mana\n" +
                    $"• Hồi chiêu: <color=#00e5ff>{curCd:F1}s</color> (Giảm theo Nhanh Nhẹn AGI, giảm tối đa 1.0s)\n" +
                    "• Lướt 4.5m né đòn tức thì!",
                    new Vector2(Mathf.Clamp(r.x + (r.width - 340f) / 2f, 16f, Screen.width - 340f - 16f), r.y - 8f), true, "Phím [D]");
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
        // ================= TAB 1: TRANG BỊ (CHÍNH XÁC 100% THEO WIREFRAME USER) =================
        private void DrawTabEquipment(Rect r)
        {
            // Tiêu đề: Hệ - Thuộc tính - Level
            string charClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
            string elem = playerStats != null ? playerStats.Element : "Hỏa";
            int lv = playerStats != null ? playerStats.Level : 1;
            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.45f);
            GUI.Label(new Rect(r.x, r.y, r.width, 20), $"HỆ [{charClass}] · THUỘC TÍNH [{elem}] · LEVEL [{lv}]", boldWhiteStyle);
            boldWhiteStyle.alignment = TextAnchor.UpperLeft;
            boldWhiteStyle.normal.textColor = Color.white;

            // ================= THANH EXP HIỂN THỊ CHỈ SỐ RÕ RÀNG (EXP BAR) =================
            int curExp = playerStats != null ? playerStats.CurrentExp : 0;
            int maxExp = playerStats != null ? playerStats.MaxExp : 100;
            float expPct = playerStats != null ? playerStats.ExpPercentage : 0f;

            Rect expBarRect = new Rect(r.x + 8f, r.y + 22f, r.width - 16f, 16f);
            // Nền tối thanh EXP
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(expBarRect, Texture2D.whiteTexture);
            // Vệt tiến trình EXP (Cyan / Ngọc Bích sáng)
            GUI.color = new Color(0.12f, 0.72f, 0.92f, 0.90f);
            GUI.DrawTexture(new Rect(expBarRect.x + 1, expBarRect.y + 1, Mathf.Max(2f, (expBarRect.width - 2f) * expPct), expBarRect.height - 2f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(expBarRect, new Color(0.85f, 0.70f, 0.25f, 0.75f), 1);

            // Chữ số EXP hiển thị rõ nét
            standardTextStyle.alignment = TextAnchor.MiddleCenter;
            standardTextStyle.fontSize = 10;
            standardTextStyle.fontStyle = FontStyle.Bold;
            standardTextStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(expBarRect.x + 1, expBarRect.y + 1, expBarRect.width, expBarRect.height), $"EXP: {curExp} / {maxExp} ({expPct * 100f:0.0}%)", standardTextStyle);
            standardTextStyle.normal.textColor = new Color(1f, 0.95f, 0.75f);
            GUI.Label(expBarRect, $"EXP: {curExp} / {maxExp} ({expPct * 100f:0.0}%)", standardTextStyle);
            standardTextStyle.fontStyle = FontStyle.Normal;

            float slotBoxSize = 50f;
            float slotGap = 10f; // Khoảng cách 10px rõ ràng giữa các ô, không bị dính sát vào nhau
            float leftColX = r.x + 8f;
            float rightColX = r.x + r.width - slotBoxSize - 8f;
            float centerBoxX = leftColX + slotBoxSize + 10f;
            float centerBoxW = rightColX - 10f - centerBoxX; // Chiều rộng khung nhân vật giữa
            float centerBoxY = r.y + 44f;
            float centerBoxH = slotBoxSize * 3f + slotGap * 2f + 4f; // 174px

            // KHUNG GIỮA: ẢNH NHÂN VẬT & TÊN
            Rect centerPortrait = new Rect(centerBoxX, centerBoxY, centerBoxW, centerBoxH);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(centerPortrait, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(centerPortrait, 1);

            Texture2D fullBodyTex = GodlessFantasy.Player.CharacterVisualController8Dir.Instance?.GetFullBodyTexture() 
                                 ?? GodlessFantasy.Player.CharacterVisualController8Dir.Instance?.GetPortraitTexture() 
                                 ?? playerPortraitTex;
            if (fullBodyTex != null)
            {
                float pW = 86f;
                float pH = 86f;
                GUI.DrawTexture(new Rect(centerPortrait.x + (centerPortrait.width - pW) / 2f, centerPortrait.y + 10f, pW, pH), fullBodyTex, ScaleMode.ScaleToFit);
            }

            bool isFemale = GodlessFantasy.Player.CharacterVisualController8Dir.Instance != null && GodlessFantasy.Player.CharacterVisualController8Dir.Instance.IsFemale;
            string genderLabel = isFemale ? "Nữ ♀ (Croptop)" : "Nam ♂ (Áo Ba Lỗ)";
            standardTextStyle.alignment = TextAnchor.LowerCenter;
            standardTextStyle.fontSize = 11;
            standardTextStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            GUI.Label(new Rect(centerPortrait.x, centerPortrait.y, centerPortrait.width, centerPortrait.height - 24), $"{playerStats?.CharacterName ?? "Dũng Sĩ SquiRRels"}\n[{genderLabel}]", standardTextStyle);
            standardTextStyle.normal.textColor = Color.white;

            // Nút bấm chuyển đổi giới tính nhanh [🚻 G]
            Rect genderBtnRect = new Rect(centerPortrait.x + (centerPortrait.width - 86f) / 2f, centerPortrait.y + centerPortrait.height - 22f, 86f, 18f);
            if (GUI.Button(genderBtnRect, "🚻 ĐỔI (G)"))
            {
                if (GodlessFantasy.Player.CharacterVisualController8Dir.Instance != null)
                {
                    GodlessFantasy.Player.CharacterVisualController8Dir.Instance.IsFemale = !GodlessFantasy.Player.CharacterVisualController8Dir.Instance.IsFemale;
                }
            }

            // CỘT TRÁI (3 Ô): MŨ, QUẦN, NHẪN (CÓ KHOẢNG CÁCH 10PX RÕ RÀNG GIỮA CÁC Ô)
            float slot1Y = centerBoxY + 2f;
            float slot2Y = slot1Y + slotBoxSize + slotGap;
            float slot3Y = slot2Y + slotBoxSize + slotGap;

            DrawEquipmentSlotButton(EquipSlot.Helmet, "Mũ", new Rect(leftColX, slot1Y, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Pants, "Quần", new Rect(leftColX, slot2Y, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Ring, "Nhẫn", new Rect(leftColX, slot3Y, slotBoxSize, slotBoxSize));

            // CỘT PHẢI (3 Ô): ÁO, GIÀY, VÒNG CỔ (CÓ KHOẢNG CÁCH 10PX RÕ RÀNG GIỮA CÁC Ô)
            DrawEquipmentSlotButton(EquipSlot.Armor, "Áo", new Rect(rightColX, slot1Y, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Boots, "Giày", new Rect(rightColX, slot2Y, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Necklace, "Vòng Cổ", new Rect(rightColX, slot3Y, slotBoxSize, slotBoxSize));

            // HÀNG DƯỚI KHUNG NHÂN VẬT: VŨ KHÍ & NGOẠI TRANG (SKIN) NẰM CẠNH NHAU ĐỐI XỨNG
            float totalPairW = slotBoxSize * 2f + 14f;
            float pairStartX = centerBoxX + (centerBoxW - totalPairW) / 2f;
            float weaponY = centerBoxY + centerBoxH + 12f;
            DrawEquipmentSlotButton(EquipSlot.Weapon, "Vũ Khí", new Rect(pairStartX, weaponY, slotBoxSize, slotBoxSize));
            DrawEquipmentSlotButton(EquipSlot.Costume, "Ngoại Trang", new Rect(pairStartX + slotBoxSize + 14f, weaponY, slotBoxSize, slotBoxSize));

            // KHU VỰC THAO TÁC TRANG BỊ ĐANG CHỌN (NẰM CHÍNH GIỮA DƯỚI VŨ KHÍ - Không viền container, chỉ label + nút)
            float actBoxW = 240f;
            float actBoxH = 62f;
            float actBoxX = r.x + (r.width - actBoxW) / 2f;
            float actBoxY = weaponY + slotBoxSize + 12f;
            Rect actBoxRect = new Rect(actBoxX, actBoxY, actBoxW, actBoxH);

            InventoryItem selectedGear = selectedEquipSlot.HasValue && inventoryManager != null
                ? inventoryManager.GetEquippedGear(selectedEquipSlot.Value)
                : null;

            if (selectedGear != null)
            {
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.fontSize = 11;
                standardTextStyle.wordWrap = false;
                DrawAutoScrollText(new Rect(actBoxRect.x + 8, actBoxRect.y + 6, actBoxRect.width - 16, 18), $"Đang chọn: {selectedGear.itemName}", standardTextStyle, 30f, new Color(0.4f, 0.95f, 1f));
                standardTextStyle.wordWrap = true;
                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;

                Rect unequipBtnRect = new Rect(actBoxRect.x + 16, actBoxRect.y + 27, actBoxRect.width - 32, 28);
                if (DrawFantasyButton(unequipBtnRect, "THÁO RA TÚI ĐỒ", 11, true, new Color(1f, 0.45f, 0.45f)))
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
                GUI.Label(new Rect(actBoxRect.x + 6, actBoxRect.y + 6, actBoxRect.width - 12, 20), $"Đang chọn: Ô [{slotName}] Trống", standardTextStyle);

                Rect unequipBtnRect = new Rect(actBoxRect.x + 16, actBoxRect.y + 27, actBoxRect.width - 32, 28);
                GUI.enabled = false;
                DrawFantasyButton(unequipBtnRect, "THÁO RA TÚI ĐỒ", 11, false);
                GUI.enabled = true;

                standardTextStyle.normal.textColor = Color.white;
                standardTextStyle.fontSize = 12;
            }

            // DƯỚI CÙNG: THANH TIỀN TỆ KÉO DÀI FULL BẢN RỘNG (Tiền · Ma Thạch · RP - Chuẩn xác theo ảnh User)
            float curBarY = r.y + r.height - 30f;
            Rect curRect = new Rect(r.x + 8f, curBarY, r.width - 16f, 26f);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(curRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(curRect, 1);

            standardTextStyle.alignment = TextAnchor.MiddleCenter;
            standardTextStyle.fontSize = 11;
            int goldVal = playerStats != null ? playerStats.Gold : 1500;
            int crystalVal = playerStats != null ? playerStats.MagicCrystals : 25;
            int rpVal = playerStats != null ? playerStats.RebirthPoints : 2;

            float curColW = curRect.width / 3f;
            GUI.Label(new Rect(curRect.x, curRect.y + 4, curColW, 18), $"💰 Vàng: <color=#ffd700>{goldVal:N0}</color>", standardTextStyle);
            GUI.Label(new Rect(curRect.x + curColW, curRect.y + 4, curColW, 18), $"💎 Ma Thạch: <color=#00e5ff>{crystalVal}</color>", standardTextStyle);
            GUI.Label(new Rect(curRect.x + curColW * 2f, curRect.y + 4, curColW, 18), $"✨ RP: <color=#ff66cc>{rpVal} Điểm</color>", standardTextStyle);
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
                case EquipSlot.Costume: return "Ngoại Trang";
                default: return slot.ToString();
            }
        }

        // ================= TAB 2: TÚI ĐỒ (LƯỚI Ô ĐỒ CÓ CON LĂN CUỘN KÉO THẢ) =================
        private void DrawTabInventory(Rect r)
        {
            float slotSize = 42f;
            float gap = 6f;
            int cols = 4;
            var itemsList = inventoryManager != null ? inventoryManager.Items : new List<InventoryItem>();
            int currentSlots = itemsList.Count;
            int maxSlots = InventoryManager.INVENTORY_CAPACITY;
            int totalSlots = Mathf.Max(maxSlots, currentSlots);
            int rows = Mathf.CeilToInt(totalSlots / (float)cols);

            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            string slotCountColor = (currentSlots >= maxSlots) ? "#ff5555" : "#88d8ff";
            GUI.Label(new Rect(r.x, r.y, r.width, 20), $"HÀNH TRANG  <color={slotCountColor}>({currentSlots}/{maxSlots})</color>", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

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

                    // Hiển thị ấn ký không tương thích Hệ / Thuộc Tính cho Cuộn Trục
                    if (item.itemType == ItemType.SkillScroll && item.scrollData != null)
                    {
                        bool canUse = item.scrollData.CanPlayerUse(playerStats?.CharacterClass, playerStats?.Element, out _);
                        if (!canUse)
                        {
                            DrawOutline(slotRect, new Color(1f, 0.25f, 0.25f, 0.9f), 2);
                            Rect lockBadge = new Rect(slotRect.x + slotRect.width - 14, slotRect.y + 2, 12, 12);
                            GUI.color = new Color(0.85f, 0.15f, 0.15f, 0.95f);
                            GUI.DrawTexture(lockBadge, Texture2D.whiteTexture);
                            GUI.color = Color.white;
                            statBadgeStyle.fontSize = 8;
                            statBadgeStyle.alignment = TextAnchor.MiddleCenter;
                            GUI.Label(lockBadge, "✕", statBadgeStyle);
                            statBadgeStyle.fontSize = 11;
                        }
                    }

                    if (item.isEquipped)
                    {
                        Rect eqBadge = new Rect(slotRect.x + 2, slotRect.y + 2, 15, 13);
                        GUI.color = new Color(0.08f, 0.45f, 0.18f, 0.95f);
                        GUI.DrawTexture(eqBadge, Texture2D.whiteTexture);
                        GUI.color = Color.white;
                        DrawOutline(eqBadge, new Color(0.85f, 0.75f, 0.35f, 0.8f), 1);
                        statBadgeStyle.fontSize = 9;
                        statBadgeStyle.fontStyle = FontStyle.Bold;
                        statBadgeStyle.alignment = TextAnchor.MiddleCenter;
                        statBadgeStyle.normal.textColor = Color.white;
                        GUI.Label(eqBadge, "E", statBadgeStyle);
                        statBadgeStyle.fontStyle = FontStyle.Normal;
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

                        // Hover tooltip chi tiết trong túi đồ
                        string itemTipBody = $"• Phẩm: <color=#{ColorUtility.ToHtmlStringRGB(item.tierColor)}>{item.tierRank}</color> | Loại: {item.itemType}\n";
                        if (item.itemType == ItemType.SkillScroll && item.scrollData != null)
                        {
                            bool canUse = item.scrollData.CanPlayerUse(playerStats?.CharacterClass, playerStats?.Element, out string lockReason);
                            string useStatus = canUse ? "<color=#55ff55>[Khả Dụng]</color>" : $"<color=#ff4444>[⛔ {lockReason}]</color>";
                            itemTipBody += $"• Chức Nghiệp: <color=#ffaa00>[{item.scrollData.requiredClass}]</color> | Thuộc Tính: <color=#00e5ff>[{item.scrollData.element}]</color>\n";
                            if (!canUse)
                            {
                                itemTipBody += $"• Trạng thái: {useStatus}\n";
                            }
                            itemTipBody += $"• Tiêu hao: {item.scrollData.manaCost} Mana | Hồi: {item.scrollData.cooldown:F1}s | Tầm đánh: <color=#00e5ff>{item.scrollData.RangeFormattedText}</color>\n";
                        }
                        if (!string.IsNullOrEmpty(item.statsInfo))
                        {
                            string cleanStats = item.statsInfo.TrimStart('•', ' ', '-');
                            itemTipBody += $"• {cleanStats}\n";
                        }
                        if (!string.IsNullOrEmpty(item.description))
                        {
                            string cleanDesc = item.description.TrimStart('•', ' ', '-');
                            itemTipBody += $"• {cleanDesc}";
                        }

                        SetTooltip(item.itemName, itemTipBody, new Vector2(Event.current.mousePosition.x + 16f, Event.current.mousePosition.y + 16f));
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
                GUI.Label(inspectRect, "Chọn một vật phẩm\nđể xem chi tiết.", standardTextStyle);
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
            DrawAutoScrollText(new Rect(r.x + 46, pY, r.width - 50, 18), item.itemName, standardTextStyle, 35f, item.tierColor);

            standardTextStyle.fontSize = 10;
            standardTextStyle.normal.textColor = new Color(0.8f, 0.85f, 0.95f);
            DrawAutoScrollText(new Rect(r.x + 46, pY + 16, r.width - 50, 16), $"[{item.tierRank}] · {item.itemType}", standardTextStyle, 30f);
            pY += 38f;

            // Thông số
            if (!string.IsNullOrEmpty(item.statsInfo))
            {
                standardTextStyle.fontSize = 11;
                DrawAutoScrollText(new Rect(r.x + 8, pY, r.width - 16, 18), item.statsInfo, standardTextStyle, 30f, Color.yellow);
                pY += 18f;
            }

            // Nếu là Bí Kíp: Hiển thị thêm dòng Hệ, Thuộc Tính và Tầm Đánh nổi bật
            if (item.itemType == ItemType.SkillScroll && item.scrollData != null)
            {
                standardTextStyle.fontSize = 11;
                bool canUse = item.scrollData.CanPlayerUse(playerStats?.CharacterClass, playerStats?.Element, out string lockReason);
                string reqClassColor = (playerStats != null && (item.scrollData.requiredClass == "Toàn Hệ" || item.scrollData.requiredClass == playerStats.CharacterClass)) ? "#55ff55" : "#ff4444";
                string elemColor = (playerStats != null && (item.scrollData.element == playerStats.Element || item.scrollData.element == "Vô Thuộc Tính")) ? "#55ff55" : "#ff4444";

                DrawFieldWithAutoScroll(new Rect(r.x + 8, pY, r.width - 16, 16), "• Hệ Yêu Cầu: ", $"<color={reqClassColor}>[{item.scrollData.requiredClass}]</color>", standardTextStyle, 30f);
                pY += 16f;
                DrawFieldWithAutoScroll(new Rect(r.x + 8, pY, r.width - 16, 16), "• Thuộc Tính: ", $"<color={elemColor}>[{item.scrollData.element}]</color>", standardTextStyle, 30f);
                pY += 16f;

                if (!canUse)
                {
                    standardTextStyle.fontSize = 10;
                    DrawFieldWithAutoScroll(new Rect(r.x + 8, pY, r.width - 16, 16), "⛔ Trạng thái: ", lockReason, standardTextStyle, 35f, new Color(1f, 0.35f, 0.35f));
                    pY += 16f;
                }

                standardTextStyle.fontSize = 11;
                DrawFieldWithAutoScroll(new Rect(r.x + 8, pY, r.width - 16, 16), "🎯 Tầm đánh: ", item.scrollData.RangeFormattedText, standardTextStyle, 30f, new Color(0.2f, 0.95f, 1f));
                pY += 18f;
            }

            // Mô tả
            standardTextStyle.fontSize = 10;
            standardTextStyle.normal.textColor = Color.white;
            float descH = Mathf.Clamp(standardTextStyle.CalcHeight(new GUIContent(item.description), r.width - 16), 28f, 54f);
            GUI.Label(new Rect(r.x + 8, pY, r.width - 16, descH), item.description, standardTextStyle);
            pY += descH + 4f;

            // CÁC NÚT HÀNH ĐỘNG ĐỒNG BỘ FANTASY BUTTON
            float btnW = r.width - 16f;
            float btnH = 26f;

            // 1. Nếu là Trang Bị (Equipment) hoặc Ngoại Trang (Costume)
            if (item.itemType == ItemType.Equipment || item.itemType == ItemType.Costume)
            {
                if (!item.isEquipped)
                {
                    string equipBtnText = (item.itemType == ItemType.Costume) ? "MẶC NGOẠI TRANG" : "MẶC TRANG BỊ";
                    Color btnTint = (item.itemType == ItemType.Costume) ? new Color(1f, 0.85f, 0.35f) : Color.white;
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), equipBtnText, 11, true, btnTint))
                    {
                        inventoryManager?.EquipGearItem(item);
                    }
                    pY += btnH + 5;
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ", 11, false, new Color(1f, 0.45f, 0.45f)))
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

                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ", 11, false, new Color(1f, 0.45f, 0.45f)))
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
            // 3. Nếu là Tiêu Hao (Dược phẩm / Thức ăn / Nước uống / Đồ ăn Thú cưng)
            else if (item.itemType == ItemType.Consumable)
            {
                if (item.id == "item_pet_food")
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "CHO THÚ ĂN (+60 EXP)", 11, true, new Color(1f, 0.75f, 0.2f)))
                    {
                        if (petFollower != null)
                        {
                            petFollower.FeedPet(60);
                            inventoryManager?.RemoveItem(item.id, 1);
                            FloatingDamageManager.ShowFloatingText(petFollower.transform.position, "🐿️ Pet +60 EXP!", Color.yellow);
                            if (item.quantity <= 0) selectedInventoryItem = null;
                        }
                        else
                        {
                            Debug.LogWarning("Chưa có thú cưng xuất chiến để cho ăn!");
                        }
                    }
                }
                else if (item.id == "item_roast_meat")
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "ĂN THỊT (+50 HP / +20 MP)", 11, true, new Color(1f, 0.6f, 0.3f)))
                    {
                        playerStats?.UseFoodOrHpRecovery();
                        if (item.quantity <= 0) selectedInventoryItem = null;
                    }
                }
                else if (item.id == "item_spring_water")
                {
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "UỐNG NƯỚC (+40 MP)", 11, true, new Color(0.2f, 0.8f, 1f)))
                    {
                        playerStats?.UseDrinkOrManaRecovery();
                        if (item.quantity <= 0) selectedInventoryItem = null;
                    }
                }
                else
                {
                    string useBtnText = item.id.Contains("hp") ? "UỐNG BÌNH MÁU" : (item.id.Contains("mp") ? "UỐNG BÌNH MANA" : "SỬ DỤNG NGAY");
                    Color useBtnCol = item.id.Contains("hp") ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 0.8f, 1f);
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), useBtnText, 11, true, useBtnCol))
                    {
                        if (item.id.Contains("hp")) playerStats?.UseHpPotion();
                        else if (item.id.Contains("mp")) playerStats?.UseManaPotion();
                        if (item.quantity <= 0) selectedInventoryItem = null;
                    }
                }
                pY += btnH + 5;
                if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "VỨT BỎ", 11, false, new Color(1f, 0.45f, 0.45f)))
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
                    if (DrawFantasyButton(new Rect(r.x + 8, pY, btnW, btnH), "THẢ THÚ", 11, false, new Color(1f, 0.45f, 0.45f)))
                    {
                        inventoryManager?.RequestDropItem(item);
                    }
                }
            }
        }

        // ================= TAB 3: KỸ NĂNG & THUỘC TÍNH (CÓ CON LĂN CUỘN TRÁNH TRÀN CHỮ) =================
        private void DrawTabSkills(Rect r)
        {
            float contentH = 1100f;
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

            // CỘT 1 (Bên trái): Trí Tuệ (INT) & Sức Mạnh (STR)
            float col1X = statBox.x + 14f;
            float col1BtnX = statBox.x + colW - btnSize - 14f;

            GUI.Label(new Rect(col1X, statBox.y + 8, 140, 22), $"Trí Tuệ (INT): <color=#00e5ff><b>{intVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col1BtnX, statBox.y + 8, btnSize, btnSize), canAdd)) playerStats?.AddIntelligence();

            GUI.Label(new Rect(col1X, statBox.y + 40, 140, 22), $"Sức Mạnh (STR): <color=#ff7755><b>{strVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col1BtnX, statBox.y + 40, btnSize, btnSize), canAdd)) playerStats?.AddStrength();

            // CỘT 2 (Bên phải): Thể Lực (VIT) & Nhanh Nhẹn (AGI)
            float col2X = statBox.x + colW + 14f;
            float col2BtnX = statBox.x + statBox.width - btnSize - 14f;

            GUI.Label(new Rect(col2X, statBox.y + 8, 140, 22), $"Thể Lực (VIT): <color=#55ff55><b>{vitVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col2BtnX, statBox.y + 8, btnSize, btnSize), canAdd)) playerStats?.AddVitality();

            GUI.Label(new Rect(col2X, statBox.y + 40, 140, 22), $"Nhanh Nhẹn (AGI): <color=#ffdd44><b>{agiVal}</b></color>", standardTextStyle);
            if (FantasyUITheme.DrawSquarePlusButton(new Rect(col2BtnX, statBox.y + 40, btnSize, btnSize), canAdd)) playerStats?.AddAgility();
            pY += 82f;

            // 2. BẢNG THÔNG TIN CHỈ SỐ CHIẾN ĐẤU (TẤN CÔNG & PHÒNG THỦ)
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(0, pY, viewRect.width, 18), "CHỈ SỐ CHIẾN ĐẤU (TẤN CÔNG & PHÒNG THỦ):", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            Rect combatStatBox = new Rect(0, pY, viewRect.width, 148f);
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(combatStatBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(combatStatBox, 1);

            int titleMagicAtk = playerStats != null ? playerStats.TitleBonusMagicAtk : 0;
            int titlePhysAtk = playerStats != null ? playerStats.TitleBonusPhysAtk : 0;
            int titleDef = playerStats != null ? playerStats.TitleBonusArmor : 0;
            float titleMoveSpd = playerStats != null ? playerStats.TitleBonusSpeed : 0f;

            int magicAtk = 20 + Mathf.RoundToInt(intVal * 2.2f) + (playerStats != null ? playerStats.InherentBonusMagicAtk : 0) + titleMagicAtk;
            int physAtk = 15 + Mathf.RoundToInt(strVal * 1.5f + agiVal * 0.5f) + (playerStats != null ? playerStats.InherentBonusAttackDmg : 0) + titlePhysAtk;
            int defVal = 10 + Mathf.RoundToInt(vitVal * 1.5f + strVal * 0.3f) + (playerStats != null ? playerStats.InherentBonusArmor : 0) + titleDef;
            float defDmgReduc = Mathf.Clamp((defVal / (defVal + 80f)) * 100f, 5f, 45f);
            int magicRes = 8 + Mathf.RoundToInt(intVal * 1.0f);
            float basicAtkRange = (playerStats != null) ? playerStats.DynamicBasicAttackRange : 1.85f;
            float moveSpd = (playerMovement != null) ? playerMovement.CurrentMoveSpeed : (5.0f + (playerStats != null ? playerStats.DynamicMoveSpeedBonus : 0f));
            float dashCd = (playerMovement != null) ? playerMovement.CurrentDashCooldown : 3.5f;
            float atkCd = (basicAttackController != null) ? basicAttackController.AttackCooldown : Mathf.Max(0.30f, 0.65f - (playerStats != null ? playerStats.DynamicAttackSpeedBonus : 0f));
            float atkPerSec = 1.0f / atkCd;
            float critRate = (playerStats != null) ? playerStats.CriticalRate : 5.0f;
            float dodgeRate = (playerStats != null) ? playerStats.DodgeRate : 3.0f;

            float buffMult = playerStats != null ? playerStats.GetDamageBuffMultiplier() : 0f;
            string buffTag = buffMult > 0f ? $" <color=#00e5ff>(+{Mathf.RoundToInt(buffMult * 100)}% Buff)</color>" : "";
            int baseMinDmg = 16 + Mathf.RoundToInt(strVal * 0.3f + agiVal * 0.1f);
            int baseMaxDmg = 16 + Mathf.RoundToInt(intVal * 0.4f + strVal * 0.5f + agiVal * 0.2f);
            int buffedMinDmg = Mathf.RoundToInt(baseMinDmg * (1f + buffMult));
            int buffedMaxDmg = Mathf.RoundToInt(baseMaxDmg * (1f + buffMult));

            float csColW = combatStatBox.width * 0.5f;
            DrawOutline(new Rect(combatStatBox.x + csColW, combatStatBox.y + 8, 1, 108), new Color(0.40f, 0.45f, 0.55f, 0.25f), 1);

            standardTextStyle.fontSize = 11;
            // CỘT TRÁI: SỨC TẤN CÔNG & ĐÒN ĐÁNH
            float csLeftX = combatStatBox.x + 12f;
            DrawFieldWithAutoScroll(new Rect(csLeftX, combatStatBox.y + 6, csColW - 16, 20), "⚔️ Công Phép: ", $"<color=#00e5ff><b>{magicAtk}</b></color> | Vật Lý: <color=#ff7755><b>{physAtk}</b></color>", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csLeftX, combatStatBox.y + 28, csColW - 16, 20), "🎯 Đánh Thường: ", $"<color=#ffff00><b>{buffedMinDmg}-{buffedMaxDmg} ST</b></color>{buffTag}", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csLeftX, combatStatBox.y + 50, csColW - 16, 20), "🏹 Tầm Đánh: ", $"<color=#55ff55><b>{basicAtkRange:F2}m</b></color>", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csLeftX, combatStatBox.y + 72, csColW - 16, 20), "⚡ Tốc Đánh: ", $"<color=#ffdd44><b>{atkPerSec:F1}/s</b></color> (Hồi {atkCd:F2}s)", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csLeftX, combatStatBox.y + 94, csColW - 16, 20), "❤️ Hồi Máu: ", $"<color=#55ff55><b>+{playerStats?.HpRegenAmount ?? 1} HP / {playerStats?.HpRegenInterval ?? 30f:F0}s</b></color>", standardTextStyle, 30f);

            // CỘT PHẢI: SỨC PHÒNG THỦ & ĐỘ CƠ ĐỘNG
            float csRightX = combatStatBox.x + csColW + 12f;
            DrawFieldWithAutoScroll(new Rect(csRightX, combatStatBox.y + 6, csColW - 16, 20), "🛡️ Giáp: ", $"<color=#ffdd44><b>{defVal}</b></color> (-{defDmgReduc:F0}% ST)", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csRightX, combatStatBox.y + 28, csColW - 16, 20), "🔮 Kháng Phép: ", $"<color=#cc77ff><b>{magicRes}</b></color> (-{(magicRes / (magicRes + 80f) * 100f):F0}% ST)", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csRightX, combatStatBox.y + 50, csColW - 16, 20), "🏃 Tốc Độ: ", $"<color=#55ff55><b>{moveSpd:F2} m/s</b></color> (Lướt: {dashCd:F2}s)", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csRightX, combatStatBox.y + 72, csColW - 16, 20), "💥 Bạo Kích: ", $"<color=#ffd700><b>{critRate:F1}%</b></color> | Né: <color=#00e5ff><b>{dodgeRate:F1}%</b></color>", standardTextStyle, 30f);
            DrawFieldWithAutoScroll(new Rect(csRightX, combatStatBox.y + 94, csColW - 16, 20), "💧 Hồi Mana: ", $"<color=#00e5ff><b>+{playerStats?.ManaRegenAmount ?? 2} MP / {playerStats?.ManaRegenInterval ?? 3f:F0}s</b></color>", standardTextStyle, 30f);

            // DÒNG DƯỚI CÙNG: PERK BẨM SINH ĐỘC NHẤT THEO HỆ
            DrawOutline(new Rect(combatStatBox.x + 8, combatStatBox.y + 118, combatStatBox.width - 16, 1), new Color(0.40f, 0.45f, 0.55f, 0.25f), 1);
            string perkName = playerStats?.InherentPerkName ?? "Chưa kích hoạt";
            string perkDesc = playerStats?.InherentPerkDesc ?? "Không";
            DrawFieldWithAutoScroll(new Rect(csLeftX, combatStatBox.y + 122, combatStatBox.width - 24, 20), $"🌟 Perk Bẩm Sinh [{perkName}]: ", $"<color=#ffd700>{perkDesc}</color>", standardTextStyle, 30f);

            pY += 156f;

            // 3. PHẦN GIỮA: DANH SÁCH CÁC KỸ NĂNG ĐANG SỬ DỤNG
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(0, pY, viewRect.width, 18), "DANH SÁCH KỸ NĂNG ĐANG SỬ DỤNG:", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            Rect skillListBox = new Rect(0, pY, viewRect.width, 168f);
            DrawDoubleGoldFrame(skillListBox, 1);

            // 0. Đòn Đánh Thường (Chuột Trái) - Hiển thị Tầm Đánh Thường chuẩn theo yêu cầu User
            standardTextStyle.fontSize = 11;
            DrawFieldWithAutoScroll(new Rect(skillListBox.x + 8, skillListBox.y + 6, skillListBox.width - 16, 22),
                "🎯 Chuột Trái: ", $"Đòn Đánh Thường — <color=#00e5ff>Tầm: {basicAtkRange:F1}m</color> | <color=#ffff00>ST: {buffedMinDmg}-{buffedMaxDmg} ST{buffTag} | Hồi: 0.65s | Mana: 0</color>", standardTextStyle, 30f);

            for (int s = 0; s < 4; s++)
            {
                var scroll = skillAimingController?.GetEquippedScroll(s);
                string sName = scroll != null ? scroll.scrollName : "(Chưa lắp cuộn trục)";
                string sInfo;
                if (scroll != null)
                {
                    bool canUse = scroll.CanPlayerUse(playerStats?.CharacterClass, playerStats?.Element, out string lockReason);
                    string lockTag = canUse ? $"<color=#00e5ff>[{scroll.requiredClass} · {scroll.element}]</color>" : $"<color=#ff4444>[⛔ {lockReason}]</color>";
                    sInfo = $"{lockTag} Hồi: {scroll.cooldown:F1}s | Mana: {scroll.manaCost} | Tầm: {scroll.RangeFormattedText}";
                }
                else
                {
                    sInfo = "Mở Túi Đồ để lắp";
                }
                standardTextStyle.fontSize = 11;
                DrawFieldWithAutoScroll(new Rect(skillListBox.x + 8, skillListBox.y + 6 + (s + 1) * 26, skillListBox.width - 16, 22),
                    $"⚡ Ô [{s + 1}]: ", $"{sName} — <color=#ffff00>{sInfo}</color>", standardTextStyle, 30f);
            }
            // Chiêu Lướt
            float curDashCd = playerMovement != null ? playerMovement.CurrentDashCooldown : 3.5f;
            DrawFieldWithAutoScroll(new Rect(skillListBox.x + 8, skillListBox.y + 6 + 5 * 26, skillListBox.width - 16, 22),
                "🏃 Ô [D]: ", $"Lướt Nhanh 360° Né Đòn — <color=#00e5ff>Tầm: 4.5m</color> | <color=#ffff00>Hồi: {curDashCd:F1}s | Mana: 10</color>", standardTextStyle, 30f);
            pY += 176f;

            // 4. PHẦN DANH SÁCH PERK (ĐÃ BỎ GHI CHÚ BẨM SINH & TẬP SỰ)
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(0, pY, viewRect.width, 18), "DANH SÁCH PERK:", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            Rect perkBox = new Rect(0, pY, viewRect.width, 120f);
            DrawDoubleGoldFrame(perkBox, 1);

            DrawPerkRow(perkBox.x + 6, perkBox.y + 6, perkBox.width - 12, "🔥", "Hỏa Thần Quyết (Perk Hệ Hỏa)", "Tăng 10% sát thương cho toàn bộ chiêu thức ma pháp thuộc tính Hỏa.");
            DrawPerkRow(perkBox.x + 6, perkBox.y + 42, perkBox.width - 12, "⚡", "Tốc Biến Tân Thủ", "Tăng 0.5m/s tốc độ chạy cơ bản khi ở trạng thái ngoài giao tranh.");
            DrawPerkRow(perkBox.x + 6, perkBox.y + 78, perkBox.width - 12, "🛡️", "Hộ Thể Kim Cang", "Giảm 5% sát thương nhận vào khi chiến đấu trong phạm vi làng.");
            pY += 128f;

            // 5. PHẦN HỆ THỐNG DANH HIỆU (TITLE SYSTEM)
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(0, pY, viewRect.width, 18), "🏆 HỆ THỐNG DANH HIỆU (TITLE):", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 20f;

            var curTitle = playerStats?.EquippedTitle;
            string cleanCurTitle = curTitle != null ? curTitle.name.Trim('[', ']') : "";
            string curTitleVal = curTitle != null 
                ? $"★ [{cleanCurTitle}] — {curTitle.statDesc}" 
                : "(Chưa đeo danh hiệu)";
            standardTextStyle.fontSize = 11;
            DrawFieldWithAutoScroll(new Rect(0, pY, viewRect.width, 18), "🏆 Đang đeo: ", curTitleVal, standardTextStyle, 30f, curTitle != null ? curTitle.titleColor : Color.gray);
            pY += 22f;

            var titles = playerStats?.TitleList;
            int tCount = titles != null ? titles.Count : 0;
            float titleBoxH = Mathf.Max(64f, tCount * 30f + 8f);
            Rect titleBox = new Rect(0, pY, viewRect.width, titleBoxH);
            DrawDoubleGoldFrame(titleBox, 1);

            if (titles != null)
            {
                for (int t = 0; t < titles.Count; t++)
                {
                    var tItem = titles[t];
                    float rowY = titleBox.y + 4f + t * 30f;
                    string cleanTName = tItem.name.Trim('[', ']');
                    standardTextStyle.fontSize = 10;
                    DrawFieldWithAutoScroll(new Rect(titleBox.x + 8, rowY + 5, titleBox.width - 86, 20), $"★ [{cleanTName}]: ", tItem.statDesc, standardTextStyle, 30f, tItem.titleColor);

                    bool isCurrent = curTitle != null && curTitle.id == tItem.id;
                    string btnTxt = isCurrent ? "THÁO" : "ĐEO";
                    Color btnColor = isCurrent ? new Color(1f, 0.5f, 0.5f) : new Color(0.4f, 1f, 0.5f);
                    if (DrawFantasyButton(new Rect(titleBox.x + titleBox.width - 74, rowY + 3, 66, 22), btnTxt, 10, true, btnColor))
                    {
                        if (isCurrent) playerStats?.UnequipTitle();
                        else playerStats?.EquipTitle(tItem.id);
                    }
                }
            }
            pY += titleBoxH + 18f;

            // 6. TÍNH NĂNG CHUYỂN SINH (REBIRTH) Ở CUỐI CÙNG - YÊU CẦU LEVEL 100
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.45f, 0.85f);
            GUI.Label(new Rect(0, pY, viewRect.width, 18), "✨ TÍNH NĂNG CHUYỂN SINH (REBIRTH) [YÊU CẦU LEVEL 100]:", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            pY += 22f;

            Rect rebirthBox = new Rect(0, pY, viewRect.width, 82f);
            DrawFantasyFrame(rebirthBox, new Color(0.08f, 0.05f, 0.10f, 0.95f), new Color(0.85f, 0.35f, 0.75f, 0.8f), new Color(1f, 0.6f, 0.9f, 0.95f));

            int curLv = playerStats != null ? playerStats.Level : 1;
            int rCount = playerStats != null ? playerStats.RebirthCount : 0;
            int rPoints = playerStats != null ? playerStats.RebirthPoints : 2;
            bool canRebirth = (playerStats != null && playerStats.CanRebirth);

            standardTextStyle.fontSize = 11;
            standardTextStyle.normal.textColor = new Color(0.92f, 0.88f, 1f);
            GUI.Label(new Rect(rebirthBox.x + 12f, rebirthBox.y + 10f, rebirthBox.width - 165f, 40f),
                $"• Đã Chuyển Sinh: <color=#ff66cc><b>{rCount} lần</b></color> (Điểm RP: <color=#ffd700><b>{rPoints}</b></color>)\n" +
                $"• Yêu cầu: <color={(canRebirth ? "#55ff55" : "#ffaa44")}><b>Cấp {curLv}/100</b></color> · Thưởng +1 RP, +10 Điểm Tiềm Năng!", standardTextStyle);

            string rebirthBtnText = canRebirth ? "✨ CHUYỂN SINH" : "KHÓA (CẦN LV.100)";
            Color rebirthBtnCol = canRebirth ? new Color(1f, 0.45f, 0.85f) : new Color(0.55f, 0.55f, 0.60f);

            if (DrawFantasyButton(new Rect(rebirthBox.x + rebirthBox.width - 150f, rebirthBox.y + 22f, 138f, 38f), rebirthBtnText, 11, canRebirth, rebirthBtnCol))
            {
                if (canRebirth && playerStats != null && playerStats.PerformRebirth())
                {
                    FloatingDamageManager.ShowFloatingText(playerMovement != null ? playerMovement.transform.position : Vector3.zero, "✨ CHUYỂN SINH THÀNH CÔNG! (+1 RP, +10 STAT)", new Color(1f, 0.4f, 0.9f));
                }
            }

            pY += 82f + 40f; // Padding rộng rãi giúp không bao giờ bị nuốt footer khi cuộn hết nấc

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
            DrawAutoScrollText(new Rect(x + 28, y + 15, w - 28, 16), desc, standardTextStyle, 30f, new Color(0.85f, 0.9f, 0.95f));
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

            // PHẢI: NÚT THÁO RA, NÚT THẢ THÚ VÀ NÚT CHO ĂN
            float btnX = petImgRect.x + boxSize + 20f;
            float btnW = r.width - (btnX - r.x) - 10f;
            float btnH = 28f;

            if (DrawFantasyButton(new Rect(btnX, pY + 4, btnW, btnH), "THÁO RA (CẤT LỒNG)", 11))
            {
                var petItem = inventoryManager?.Items.Find(it => it.itemType == ItemType.Pet);
                if (petItem != null) inventoryManager?.TogglePetSummon(petItem);
            }

            // NÚT CHO THÚ CƯNG ĂN TĂNG LEVEL & TIẾN HÓA
            var petFoodItem = inventoryManager?.Items.Find(it => it.id == "item_pet_food");
            int foodCount = petFoodItem != null ? petFoodItem.quantity : 0;
            string feedLabel = foodCount > 0 ? $"CHO ĂN HẠT DẺ ({foodCount})" : "CHO ĂN (HẾT THỨC ĂN)";
            Color feedColor = foodCount > 0 ? new Color(1f, 0.75f, 0.2f) : new Color(0.6f, 0.6f, 0.6f);

            if (DrawFantasyButton(new Rect(btnX, pY + 38, btnW, btnH), feedLabel, 10, foodCount > 0, feedColor))
            {
                if (petFollower != null)
                {
                    petFollower.FeedPet(60);
                    inventoryManager?.RemoveItem("item_pet_food", 1);
                    FloatingDamageManager.ShowFloatingText(petFollower.transform.position, "🐿️ Pet +60 EXP!", Color.yellow);
                }
                else
                {
                    Debug.LogWarning("Thú cưng chưa xuất chiến để cho ăn!");
                }
            }

            if (DrawFantasyButton(new Rect(btnX, pY + 72, btnW, btnH), "THẢ THÚ", 10, false, new Color(1f, 0.45f, 0.45f)))
            {
                var petItem = inventoryManager?.Items.Find(it => it.itemType == ItemType.Pet);
                if (petItem != null)
                {
                    if (petItem.isEquipped) petItem.isEquipped = false;
                    inventoryManager?.RequestDropItem(petItem);
                }
            }

            // DƯỚI: THÔNG TIN CHI TIẾT & TIẾN HÓA THEO LEVEL
            float infoY = pY + boxSize + 10f;
            Rect infoRect = new Rect(r.x + 10, infoY, r.width - 20, 130f);
            DrawDoubleGoldFrame(infoRect, 1);

            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(infoRect.x + 10, infoRect.y + 6, infoRect.width - 20, 18), "TIẾN HÓA & TRẠNG THÁI THÚ CƯNG:", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;

            int pLv = petFollower != null ? petFollower.PetLevel : 1;
            int pExp = petFollower != null ? petFollower.PetExp : 0;
            int pMaxExp = petFollower != null ? petFollower.PetMaxExp : 100;
            string stageName = petFollower != null ? petFollower.StageName : "Sóc Bé Nhỏ (Baby)";
            float bonusSpd = petFollower != null ? petFollower.GetBonusMoveSpeed() * 100f : 5f;
            int bonusDef = petFollower != null ? petFollower.GetBonusDefense() : 2;

            standardTextStyle.fontSize = 11;
            GUI.Label(new Rect(infoRect.x + 10, infoRect.y + 26, infoRect.width - 20, 18),
                $"🐿️ <b>Sóc Chibi SquiRRels Studio</b> — <color=#ffd700>Lv.{pLv} / 50</color> · <color=#00e5ff>[{stageName}]</color>", standardTextStyle);

            // Thanh EXP Thú Cưng
            float barW = infoRect.width - 20f;
            float barH = 14f;
            Rect expBgRect = new Rect(infoRect.x + 10, infoRect.y + 48, barW, barH);
            GUI.color = new Color(0.1f, 0.12f, 0.16f, 0.95f);
            GUI.DrawTexture(expBgRect, Texture2D.whiteTexture);
            float expPct = Mathf.Clamp01((float)pExp / pMaxExp);
            GUI.color = new Color(0.95f, 0.65f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(expBgRect.x, expBgRect.y, barW * expPct, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawOutline(expBgRect, new Color(0.4f, 0.45f, 0.55f, 0.6f), 1);

            var expTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(expBgRect, $"EXP: {pExp} / {pMaxExp} ({expPct * 100:F0}%)", expTextStyle);

            // Thông số thưởng thuộc tính và tiến trình tiến hóa
            standardTextStyle.fontSize = 10;
            GUI.Label(new Rect(infoRect.x + 10, infoRect.y + 68, infoRect.width - 20, 56),
                $"• <b>Chỉ số cộng thêm</b>: +{bonusSpd:F0}% Tốc chạy | +{bonusDef} Giáp Hộ Thể\n" +
                $"• <b>Quy luật tiến hóa</b>: Cho ăn hạt dẻ tăng EXP. Đạt Cấp 10 tiến hóa thành Sóc Chiến Binh (to hơn, sáng lông). Đạt Cấp 25 tiến hóa thành Sóc Thần Thoại (hào quang hoàng kim)!",
                standardTextStyle);
        }

        // ================= TAB 5: CÀI ĐẶT =================
        private void DrawTabSettings(Rect r)
        {
            float curY = r.y;

            // 1. Tiêu đề
            boldWhiteStyle.fontSize = 15;
            boldWhiteStyle.alignment = TextAnchor.UpperLeft;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, curY, r.width, 24), "⚙️ CÀI ĐẶT HỆ THỐNG", boldWhiteStyle);
            boldWhiteStyle.normal.textColor = Color.white;
            curY += 30f;

            // 2. Toggle Tự chối Giao Dịch
            standardTextStyle.fontSize = 12;
            autoDeclineTrade = GUI.Toggle(new Rect(r.x, curY, r.width, 24), autoDeclineTrade, " 🛡️ Tự động TỪ CHỐI mọi yêu cầu Giao Dịch (/trade)");
            curY += 30f;

            // 3. Test Hệ Phái
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(r.x, curY, r.width, 20), "⚡ TEST HỆ PHÁI (CLASS):", boldWhiteStyle);
            curY += 22f;

            string[] classes = { "Kị Sĩ", "Pháp Sư", "Võ Sĩ", "Xạ Thủ" };
            float cBtnW = (r.width - 15f) / 4f;
            for (int i = 0; i < classes.Length; i++)
            {
                string cName = classes[i];
                bool isCurrent = playerStats != null && playerStats.CharacterClass == cName;
                Color btnCol = isCurrent ? new Color(0.4f, 1f, 0.5f) : Color.white;
                if (DrawFantasyButton(new Rect(r.x + i * (cBtnW + 5f), curY, cBtnW, 26), cName, 12, true, btnCol))
                {
                    playerStats?.SetCharacterClass(cName);
                }
            }
            curY += 32f;

            // 4. Test Thuộc Tính
            GUI.Label(new Rect(r.x, curY, r.width, 20), "🔥 TEST THUỘC TÍNH (ELEMENT):", boldWhiteStyle);
            curY += 22f;

            string[] elements = { "Hỏa", "Băng", "Lôi", "Quang", "Ám", "Phong", "Mộc" };
            float eBtnW = (r.width - 24f) / 7f;
            for (int i = 0; i < elements.Length; i++)
            {
                string eName = elements[i];
                bool isCurrent = playerStats != null && playerStats.Element.StartsWith(eName);
                Color btnCol = isCurrent ? new Color(1f, 0.85f, 0.3f) : Color.white;
                if (DrawFantasyButton(new Rect(r.x + i * (eBtnW + 4f), curY, eBtnW, 26), eName, 11, true, btnCol))
                {
                    playerStats?.SetElement(eName);
                }
            }
            curY += 34f;

            // 5. Thông tin hệ & phím tắt (đủ chiều cao 42px)
            tooltipDescStyle.fontSize = 12;
            GUI.Label(new Rect(r.x, curY, r.width, 42),
                $"• Hệ hiện tại: <color=#00e5ff><b>{playerStats?.CharacterClass}</b></color> | Thuộc tính: <color=#ffaa00><b>{playerStats?.Element}</b></color>\n" +
                $"• Phím tắt: [Q] Tự dùng máu/thức ăn | [E] Tự dùng mana/nước.",
                tooltipDescStyle);
            curY += 46f;

            // 6. Bộ công cụ Engine No-Code Studio
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(0.2f, 0.9f, 1f);
            GUI.Label(new Rect(r.x, curY, r.width, 20), "🛠️ BỘ CÔNG CỤ ENGINE THIẾT KẾ (NO-CODE STUDIO):", boldWhiteStyle);
            curY += 22f;

            float devBtnW = (r.width - 12f) / 2f;
            if (DrawFantasyButton(new Rect(r.x, curY, devBtnW, 32), "🛠️ TẠO ASSET [F1]", 12, ItemCreatorModal.Instance != null && ItemCreatorModal.Instance.IsOpen))
            {
                if (ItemCreatorModal.Instance != null)
                {
                    ItemCreatorModal.Instance.IsOpen = !ItemCreatorModal.Instance.IsOpen;
                }
            }

            if (DrawFantasyButton(new Rect(r.x + devBtnW + 12f, curY, devBtnW, 32), "🗺️ VẼ ĐỊA HÌNH [F2]", 12, GodlessFantasy.Core.TerrainPainterTool.Instance != null && GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode))
            {
                if (GodlessFantasy.Core.TerrainPainterTool.Instance != null)
                {
                    GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode = !GodlessFantasy.Core.TerrainPainterTool.Instance.IsPaintingMode;
                }
            }
            curY += 38f;

            // 6.5. Bảng Testing Toàn Bộ 140 Kỹ Năng & VFX (Auto Set Hệ Khi Chọn)
            if (DrawFantasyButton(new Rect(r.x, curY, r.width, 32), "⚡ TESTING 140 KỸ NĂNG & VFX [F3] (AUTO ĐỔI HỆ)", 11, SkillTestingMatrixModal.Instance != null && SkillTestingMatrixModal.Instance.IsOpen, new Color(1.0f, 0.45f, 0.2f)))
            {
                if (SkillTestingMatrixModal.Instance != null)
                {
                    SkillTestingMatrixModal.Instance.IsOpen = !SkillTestingMatrixModal.Instance.IsOpen;
                }
            }
            curY += 40f;

            // 7. Quản lý Bang Hội
            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.25f);
            GUI.Label(new Rect(r.x, curY, r.width, 20), "🛡️ TÍNH NĂNG ĐỒNG ĐỘI & BANG HỘI (GUILD):", boldWhiteStyle);
            curY += 22f;

            if (DrawFantasyButton(new Rect(r.x, curY, r.width, 34), "🛡️ MỞ QUẢN LÝ BANG HỘI", 12, isGuildModalOpen, new Color(1f, 0.88f, 0.25f)))
            {
                isGuildModalOpen = !isGuildModalOpen;
            }
        }

        // ================= POPUP DOUBLE CHECK 5S KHI VỨT BỎ VẬT PHẨM =================
        private void DrawDoubleCheckDropModal()
        {
            var pendingItem = inventoryManager?.PendingDropItem;
            if (pendingItem == null) return;

            float modalW = 350f;
            float modalH = 195f;
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
            GUI.Label(new Rect(r.x + 12, r.y + 34, r.width - 24, 46),
                $"Bạn có chắc chắn muốn vứt bỏ vĩnh viễn:\n<color=yellow>{pendingItem.itemName}</color> [{pendingItem.tierRank}]?\nVật phẩm sẽ biến mất khỏi thế giới!", standardTextStyle);

            float countdown = inventoryManager != null ? inventoryManager.DropCountdown : 0f;
            bool isLocked = countdown > 0f;

            if (isLocked)
            {
                // Thông báo khóa bảo hộ 30s cho phẩm B trở lên
                standardTextStyle.fontSize = 10;
                standardTextStyle.normal.textColor = new Color(1f, 0.8f, 0.3f);
                GUI.Label(new Rect(r.x + 12, r.y + 80, r.width - 24, 34),
                    $"🔒 <color=#ffbb33>Vật phẩm từ bậc B trở lên có giá trị cao!</color>\nKhóa bảo hộ an toàn: <color=#ff4444><b>{Mathf.CeilToInt(countdown)}s</b></color> (chống xóa nhầm)", standardTextStyle);

                // Nút bị khóa, mờ xám không bấm được
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.65f);
                DrawFantasyButton(new Rect(r.x + 20, r.y + 130, 145, 34), $"🔒 KHÓA ({Mathf.CeilToInt(countdown)}s)", 11, false, Color.gray);
                GUI.color = Color.white;
            }
            else
            {
                string confirmText = "🗑️ XÁC NHẬN XÓA";
                if (DrawFantasyButton(new Rect(r.x + 20, r.y + 130, 145, 34), confirmText, 11, true, new Color(1f, 0.35f, 0.35f)))
                {
                    inventoryManager?.ConfirmDropItem();
                    selectedInventoryItem = null;
                }
            }

            if (DrawFantasyButton(new Rect(r.x + 185, r.y + 130, 145, 34), "HỦY BỎ", 11))
            {
                inventoryManager?.CancelDropItem();
            }
        }

        // ================= CỬA SỔ TRÒ CHUYỆN 3 TAB (KHU VỰC · THẾ GIỚI · BANG HỘI) =================
        private void DrawChatModal()
        {
            float chatW = 430f;
            float chatH = 270f;
            float chatX = (Screen.width - chatW) / 2f;
            float chatY = (Screen.height - chatH) / 2f;
            Rect chatRect = new Rect(chatX, chatY, chatW, chatH);

            // Nền tối đặc chống nhìn xuyên thấu
            GUI.color = new Color(0.04f, 0.05f, 0.09f, 1.0f);
            GUI.DrawTexture(chatRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(chatRect);

            // 3 TAB HEADER THEO ĐÚNG YÊU CẦU USER (KHU VỰC, THẾ GIỚI, BANG HỘI)
            float tabBtnW = 124f;
            float tabBtnH = 24f;

            bool isLocalTab = (currentChatTab == 0);
            if (DrawFantasyButton(new Rect(chatRect.x + 8, chatRect.y + 6, tabBtnW, tabBtnH), "📍 KHU VỰC", 10, isLocalTab))
            {
                currentChatTab = 0;
            }

            bool isWorldTab = (currentChatTab == 1);
            if (DrawFantasyButton(new Rect(chatRect.x + 136, chatRect.y + 6, tabBtnW, tabBtnH), "🌍 THẾ GIỚI", 10, isWorldTab))
            {
                currentChatTab = 1;
            }

            bool isGuildTab = (currentChatTab == 2);
            if (DrawFantasyButton(new Rect(chatRect.x + 264, chatRect.y + 6, tabBtnW, tabBtnH), "🛡️ BANG HỘI", 10, isGuildTab, new Color(1f, 0.88f, 0.25f)))
            {
                currentChatTab = 2;
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
            string currentHistory = (currentChatTab == 0) ? localChatHistory : (currentChatTab == 1) ? worldChatHistory : guildChatHistory;
            GUI.Label(new Rect(histRect.x + 6, histRect.y + 6, histRect.width - 12, histRect.height - 12), currentHistory, standardTextStyle);

            // KHUNG NHẬP TIN NHẮN & NÚT GỬI
            float sendBtnW = 54f;

            Event currentEv = Event.current;
            bool enterPressed = false;
            if (currentEv != null && (currentEv.type == EventType.KeyDown || currentEv.rawType == EventType.KeyDown))
            {
                if (currentEv.keyCode == KeyCode.Return || currentEv.keyCode == KeyCode.KeypadEnter)
                {
                    enterPressed = true;
                    currentEv.Use();
                }
            }

            GUI.SetNextControlName("ChatInputField");
            chatInputText = GUI.TextField(new Rect(chatRect.x + 8, chatRect.y + chatRect.height - 34, chatRect.width - sendBtnW - 14, 26), chatInputText);

            if (shouldFocusChatInput)
            {
                GUI.FocusControl("ChatInputField");
                shouldFocusChatInput = false;
            }

            bool sendClicked = DrawFantasyButton(new Rect(chatRect.x + chatRect.width - sendBtnW - 6, chatRect.y + chatRect.height - 34, sendBtnW, 26), "GỬI", 11, true);

            if (sendClicked || enterPressed)
            {
                SendCurrentChatMessage();
            }
        }

        // ================= POPUP QUẢN LÝ BANG HỘI (GUILD WINDOW CHÍNH GIỮA MÀN HÌNH) =================
        private void DrawGuildModal()
        {
            float modalW = 660f;
            float modalH = 510f;
            float modalX = (Screen.width - modalW) / 2f;
            float modalY = (Screen.height - modalH) / 2f;
            Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

            // Nền tối đặc chống nhìn xuyên thấu
            GUI.color = new Color(0.04f, 0.05f, 0.09f, 1.0f);
            GUI.DrawTexture(modalRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(modalRect);

            // Header Tiêu Đề Cửa Sổ
            boldWhiteStyle.fontSize = 15;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);
            GUI.Label(new Rect(modalX, modalY + 6f, modalW, 24f), "🛡️ HỆ THỐNG BANG HỘI & ĐỒNG ĐỘI (GUILD)", boldWhiteStyle);

            // Nút đóng [✕]
            if (DrawFantasyButton(new Rect(modalX + modalW - 34f, modalY + 6f, 26f, 24f), "✕", 12))
            {
                isGuildModalOpen = false;
            }

            var gm = GodlessFantasy.Guild.GuildManager.Instance;
            if (gm == null) return;

            // Thông báo Giao dịch nếu có
            if (guildTradeNotifyTimer > 0f)
            {
                guildTradeNotifyTimer -= Time.deltaTime;
                Rect notifyRect = new Rect(modalX + 16f, modalY + 30f, modalW - 32f, 22f);
                GUI.color = new Color(0.1f, 0.4f, 0.2f, 0.95f);
                GUI.DrawTexture(notifyRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                standardTextStyle.alignment = TextAnchor.MiddleCenter;
                standardTextStyle.normal.textColor = Color.yellow;
                GUI.Label(notifyRect, guildTradeNotification, standardTextStyle);
            }

            // ─────────────────────────────────────────────────────────────
            // TRƯỜNG HỢP 1: NGƯỜI CHƠI CHƯA GIA NHẬP BANG HỘI NÀO
            // ─────────────────────────────────────────────────────────────
            if (!gm.IsInGuild)
            {
                float contentY = modalY + 36f;

                // 1. Khung Thành Lập Bang Hội Mới (Tên, Pass Code, Khẩu hiệu tối đa 50 ký tự)
                float createH = 96f;
                Rect createBox = new Rect(modalX + 16f, contentY, modalW - 32f, createH);
                DrawFantasyFrame(createBox, new Color(0.06f, 0.08f, 0.12f, 0.95f), new Color(0.85f, 0.70f, 0.25f, 0.75f), new Color(1f, 0.88f, 0.45f, 0.95f));

                boldWhiteStyle.fontSize = 11;
                boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);
                boldWhiteStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(createBox.x + 12f, createBox.y + 6f, 400f, 18f), "👑 THÀNH LẬP BANG HỘI MỚI (CHI PHÍ: 200 VÀNG - AI CŨNG TẠO ĐƯỢC):", boldWhiteStyle);

                // Dòng 1: Tên Bang + Pass Code + Nút Tạo Bang
                standardTextStyle.fontSize = 11;
                standardTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(createBox.x + 12f, createBox.y + 28f, 65f, 22f), "Tên Bang:", standardTextStyle);
                newGuildNameInput = GUI.TextField(new Rect(createBox.x + 78f, createBox.y + 28f, 170f, 22f), newGuildNameInput);

                GUI.Label(new Rect(createBox.x + 256f, createBox.y + 28f, 70f, 22f), "Pass Code:", standardTextStyle);
                newGuildPassInput = GUI.TextField(new Rect(createBox.x + 328f, createBox.y + 28f, 75f, 22f), newGuildPassInput);

                if (DrawFantasyButton(new Rect(createBox.x + 415f, createBox.y + 26f, 180f, 26f), "TẠO BANG (-200V)", 11, true, new Color(1f, 0.88f, 0.25f)))
                {
                    gm.CreateGuild(newGuildNameInput, newGuildPassInput, newGuildSloganInput, playerStats);
                }

                // Dòng 2: Khẩu hiệu bang (giới hạn tối đa 50 ký tự)
                GUI.Label(new Rect(createBox.x + 12f, createBox.y + 54f, 65f, 22f), "Khẩu hiệu:", standardTextStyle);
                newGuildSloganInput = GUI.TextField(new Rect(createBox.x + 78f, createBox.y + 54f, 400f, 22f), newGuildSloganInput);
                if (newGuildSloganInput.Length > 50) newGuildSloganInput = newGuildSloganInput.Substring(0, 50);

                standardTextStyle.fontSize = 10;
                standardTextStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);
                GUI.Label(new Rect(createBox.x + 485f, createBox.y + 56f, 115f, 20f), $"[{newGuildSloganInput.Length}/50 ký tự]", standardTextStyle);

                standardTextStyle.fontSize = 9;
                standardTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
                GUI.Label(new Rect(createBox.x + 12f, createBox.y + 78f, createBox.width - 24f, 15f), "• Thành lập bang giúp hiển thị Tên Bang trên đầu, chia sẻ EXP/Vàng khi đánh quái, và hưởng buff số 6!", standardTextStyle);

                // 2. Khung Danh Sách Bang Hội Trên Server (Có thanh cuộn ScrollView mượt mà)
                float listY = contentY + createH + 8f;
                Rect listBox = new Rect(modalX + 16f, listY, modalW - 32f, modalH - listY + modalY - 12f);
                DrawFantasyFrame(listBox, new Color(0.05f, 0.07f, 0.11f, 0.95f), new Color(0.45f, 0.55f, 0.75f, 0.6f), new Color(0.6f, 0.75f, 0.95f, 0.8f));

                boldWhiteStyle.fontSize = 11;
                boldWhiteStyle.normal.textColor = new Color(0.4f, 0.9f, 1f);
                GUI.Label(new Rect(listBox.x + 12f, listBox.y + 6f, 400f, 18f), $"📜 DANH SÁCH BANG HỘI TRÊN SERVER ({gm.PublicGuilds.Count} Bang):", boldWhiteStyle);

                // Thanh cuộn danh sách bang hội server
                Rect publicScrollArea = new Rect(listBox.x + 6f, listBox.y + 26f, listBox.width - 12f, listBox.height - 32f);
                float totalGuildH = gm.PublicGuilds.Count * 58f + 4f;
                Rect publicViewRect = new Rect(0, 0, publicScrollArea.width - 18f, Mathf.Max(totalGuildH, publicScrollArea.height + 5f));

                publicGuildListScroll = GUI.BeginScrollView(publicScrollArea, publicGuildListScroll, publicViewRect, false, true);

                for (int gIdx = 0; gIdx < gm.PublicGuilds.Count; gIdx++)
                {
                    var guild = gm.PublicGuilds[gIdx];
                    Rect gRow = new Rect(2f, gIdx * 58f, publicViewRect.width - 4f, 52f);
                    GUI.color = new Color(0.08f, 0.11f, 0.16f, 0.95f);
                    GUI.DrawTexture(gRow, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    DrawOutline(gRow, new Color(0.3f, 0.45f, 0.65f, 0.5f), 1);

                    standardTextStyle.alignment = TextAnchor.UpperLeft;
                    standardTextStyle.fontSize = 12;
                    standardTextStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);
                    GUI.Label(new Rect(gRow.x + 8f, gRow.y + 5f, 280f, 18f), $"🛡️ {guild.guildName} [Cấp {guild.guildLevel}]", standardTextStyle);

                    standardTextStyle.fontSize = 10;
                    standardTextStyle.normal.textColor = Color.white;
                    string dispSlogan = guild.slogan.Length > 50 ? guild.slogan.Substring(0, 50) : guild.slogan;
                    GUI.Label(new Rect(gRow.x + 8f, gRow.y + 25f, 380f, 20f), $"Bang chủ: {guild.leaderName} · {guild.members.Count} thành viên · \"{dispSlogan}\"", standardTextStyle);

                    if (DrawFantasyButton(new Rect(gRow.x + gRow.width - 98f, gRow.y + 11f, 90f, 30f), "GIA NHẬP", 11, true, new Color(0.3f, 0.9f, 0.5f)))
                    {
                        joiningGuildTarget = guild;
                        joinCodeInput = "";
                        joinCodeError = "";
                        isJoinCodeModalOpen = true;
                    }
                }
                GUI.EndScrollView();

                // Popup Nhập Mã Code Gia Nhập Bang
                if (isJoinCodeModalOpen && joiningGuildTarget != null)
                {
                    DrawJoinCodeModal(joiningGuildTarget, playerStats);
                }
            }
            // ─────────────────────────────────────────────────────────────
            // TRƯỜNG HỢP 2: NGƯỜI CHƠI ĐÃ CÓ BANG HỘI
            // ─────────────────────────────────────────────────────────────
            else
            {
                var curGuild = gm.CurrentGuild;
                bool isLeader = gm.IsPlayerLeader(playerStats);
                float curY = modalY + 36f;

                // 1. Thẻ Thông Tin Bang Hội (Chiều cao 96px, rộng rãi, chuẩn Fantasy)
                float infoH = 96f;
                Rect infoCard = new Rect(modalX + 16f, curY, modalW - 32f, infoH);
                DrawFantasyFrame(infoCard, new Color(0.06f, 0.08f, 0.12f, 0.95f), new Color(0.85f, 0.70f, 0.25f, 0.85f), new Color(1f, 0.88f, 0.45f, 0.95f));

                // Dòng 1: TÊN BANG + CẤP BANG + NÚT CÀI ĐẶT
                // Nút cài đặt (bên phải cùng nếu là Bang chủ)
                if (isLeader)
                {
                    Rect btnRect = new Rect(infoCard.x + infoCard.width - 122f, infoCard.y + 6f, 112f, 24f);
                    if (DrawFantasyButton(btnRect, "⚙️ CÀI ĐẶT", 10, true, new Color(1f, 0.85f, 0.35f)))
                    {
                        settingSloganInput = curGuild.slogan;
                        settingPassCodeInput = curGuild.guildPassCode;
                        isGuildSettingsOpen = !isGuildSettingsOpen;
                    }

                    // Cấp Bang nằm sát cạnh nút cài đặt
                    Rect levelRect = new Rect(infoCard.x + infoCard.width - 215f, infoCard.y + 6f, 88f, 24f);
                    boldWhiteStyle.fontSize = 12;
                    boldWhiteStyle.alignment = TextAnchor.MiddleRight;
                    boldWhiteStyle.wordWrap = false;
                    boldWhiteStyle.clipping = TextClipping.Overflow;
                    boldWhiteStyle.normal.textColor = new Color(0.35f, 0.85f, 1f);
                    GUI.Label(levelRect, $"[CẤP {curGuild.guildLevel}]", boldWhiteStyle);
                }
                else
                {
                    // Thành viên thường: Cấp Bang ở góc phải
                    Rect levelRect = new Rect(infoCard.x + infoCard.width - 105f, infoCard.y + 6f, 95f, 24f);
                    boldWhiteStyle.fontSize = 12;
                    boldWhiteStyle.alignment = TextAnchor.MiddleRight;
                    boldWhiteStyle.wordWrap = false;
                    boldWhiteStyle.clipping = TextClipping.Overflow;
                    boldWhiteStyle.normal.textColor = new Color(0.35f, 0.85f, 1f);
                    GUI.Label(levelRect, $"[CẤP {curGuild.guildLevel}]", boldWhiteStyle);
                }

                // Tên Bang: Chiều rộng hào phóng tới 395px (Leader) hoặc 500px (Member), dùng DrawFieldWithAutoScroll không bao giờ bị cắt xén pixel (truncate xấu)
                float nameFieldW = isLeader ? (infoCard.width - 228f) : (infoCard.width - 118f);
                Rect nameRect = new Rect(infoCard.x + 12f, infoCard.y + 6f, nameFieldW, 24f);
                boldWhiteStyle.fontSize = 14;
                boldWhiteStyle.alignment = TextAnchor.MiddleLeft;
                boldWhiteStyle.wordWrap = false;
                boldWhiteStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
                DrawFieldWithAutoScroll(nameRect, "👑 BANG: ", $"< {curGuild.guildName} >", boldWhiteStyle, 25f, new Color(1f, 0.95f, 0.45f));

                // Dòng 2: Bang Chủ, Quỹ Bang, Thành Viên (Phân chia 3 cột rộng thoáng, không rớt dòng đè nhau)
                standardTextStyle.alignment = TextAnchor.MiddleLeft;
                standardTextStyle.fontSize = 12;
                standardTextStyle.wordWrap = false;
                standardTextStyle.normal.textColor = new Color(0.85f, 0.90f, 0.95f);

                // Cột 1: Bang chủ (Rộng 235px, tự cuộn marquee nếu tên siêu dài, triệt tiêu lỗi rớt dòng đè lên khẩu hiệu)
                Rect col1 = new Rect(infoCard.x + 12f, infoCard.y + 36f, 235f, 22f);
                DrawFieldWithAutoScroll(col1, "Bang chủ: ", curGuild.leaderName, standardTextStyle, 25f, new Color(1f, 0.70f, 0.15f));

                // Cột 2: Quỹ bang (Rộng 190px)
                Rect col2 = new Rect(infoCard.x + 255f, infoCard.y + 36f, 190f, 22f);
                GUI.Label(col2, $"Quỹ bang: <color=#ffd700><b>{curGuild.guildGoldFund:N0} Vàng</b></color>", standardTextStyle);

                // Cột 3: Số lượng thành viên (Rộng 160px)
                Rect col3 = new Rect(infoCard.x + 455f, infoCard.y + 36f, 160f, 22f);
                GUI.Label(col3, $"Thành viên: <color=#00e5ff><b>{curGuild.members.Count}/50</b></color>", standardTextStyle);

                // Dòng 3: Khẩu hiệu bang (Cách an toàn 6px bên dưới Dòng 2, rộng 604px, cuộn chữ tự động nếu dài)
                standardTextStyle.fontSize = 11;
                standardTextStyle.wordWrap = false;
                standardTextStyle.normal.textColor = new Color(0.65f, 0.75f, 0.85f);
                Rect sloganRect = new Rect(infoCard.x + 12f, infoCard.y + 64f, infoCard.width - 24f, 22f);
                DrawFieldWithAutoScroll(sloganRect, "Khẩu hiệu: ", $"\"{curGuild.slogan}\"", standardTextStyle, 22f, new Color(0.88f, 0.92f, 0.98f));

                // 2. Danh Sách Thành Viên Bang Hội (Scrollable chuẩn Unity, không có thanh vàng thừa)
                float memBoxY = curY + infoH + 8f;
                float bottomH = 44f;
                float bottomY = modalY + modalH - bottomH - 12f;
                float memBoxH = bottomY - memBoxY - 8f;
                Rect memBox = new Rect(modalX + 16f, memBoxY, modalW - 32f, memBoxH);
                DrawFantasyFrame(memBox, new Color(0.05f, 0.07f, 0.11f, 0.95f), new Color(0.45f, 0.55f, 0.75f, 0.6f), new Color(0.6f, 0.75f, 0.95f, 0.8f));
                DrawFantasyFrame(memBox, new Color(0.05f, 0.07f, 0.11f, 0.95f), new Color(0.45f, 0.55f, 0.75f, 0.6f), new Color(0.6f, 0.75f, 0.95f, 0.8f));

                boldWhiteStyle.fontSize = 13;
                boldWhiteStyle.normal.textColor = new Color(0.35f, 0.85f, 1f);
                GUI.Label(new Rect(memBox.x + 12f, memBox.y + 6f, 400f, 18f), $"👥 DANH SÁCH THÀNH VIÊN ({curGuild.members.Count} người):", boldWhiteStyle);

                float viewH = curGuild.members.Count * 52f + 8f;
                Rect scrollArea = new Rect(memBox.x + 6f, memBox.y + 26f, memBox.width - 12f, memBoxH - 32f);
                Rect viewRect = new Rect(0, 0, scrollArea.width - 18f, Mathf.Max(viewH, scrollArea.height + 10f));
                guildMemberListScroll = GUI.BeginScrollView(scrollArea, guildMemberListScroll, viewRect, false, true);

                for (int i = 0; i < curGuild.members.Count; i++)
                {
                    var m = curGuild.members[i];
                    Rect mRow = new Rect(2f, i * 52f, viewRect.width - 4f, 46f);

                    GUI.color = (i % 2 == 0) ? new Color(0.07f, 0.10f, 0.15f, 0.95f) : new Color(0.09f, 0.12f, 0.18f, 0.95f);
                    GUI.DrawTexture(mRow, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    DrawOutline(mRow, new Color(0.25f, 0.35f, 0.50f, 0.4f), 1);

                    // Trạng thái Online / Offline
                    string statusIcon = m.isOnline ? "<color=#55ff55>● Online</color>" : "<color=#888888>○ Offline</color>";
                    string leaderTag = m.isLeader ? " <color=#ffaa00>[Bang Chủ]</color>" : "";

                    standardTextStyle.fontSize = 13;
                    standardTextStyle.normal.textColor = Color.white;
                    GUI.Label(new Rect(mRow.x + 8f, mRow.y + 4f, 220f, 18f), $"{m.memberName}{leaderTag}", standardTextStyle);

                    standardTextStyle.fontSize = 11;
                    standardTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
                    GUI.Label(new Rect(mRow.x + 8f, mRow.y + 24f, 310f, 18f), $"Lv.{m.level} · {m.characterClass} ({m.element}) · {statusIcon} · Map: {m.currentMap}", standardTextStyle);

                    bool canKick = isLeader && !m.isLeader && (playerStats == null || m.memberName != playerStats.CharacterName);

                    if (canKick)
                    {
                        // Bang Chủ: Nút Profile (58px), Nút Giao Dịch (66px), Nút Kích (50px)
                        if (DrawFantasyButton(new Rect(mRow.x + mRow.width - 188f, mRow.y + 9f, 58f, 28f), "PROFILE", 10))
                        {
                            selectedProfileMember = m;
                        }
                        if (DrawFantasyButton(new Rect(mRow.x + mRow.width - 124f, mRow.y + 9f, 66f, 28f), "GIAO DỊCH", 10, true, new Color(0.35f, 0.85f, 1f)))
                        {
                            guildTradeNotification = $"🤝 Đã gửi lời mời giao dịch đến [{m.memberName}]!";
                            guildTradeNotifyTimer = 3.5f;
                        }
                        if (DrawFantasyButton(new Rect(mRow.x + mRow.width - 54f, mRow.y + 9f, 50f, 28f), "KÍCH", 10, true, new Color(1f, 0.35f, 0.35f)))
                        {
                            gm.KickMember(m.memberName, playerStats);
                            guildTradeNotification = $"⚠️ Đã kích [{m.memberName}] khỏi bang hội!";
                            guildTradeNotifyTimer = 3.5f;
                        }
                    }
                    else
                    {
                        // Thành viên thường: Profile (78px), Giao Dịch (82px)
                        if (DrawFantasyButton(new Rect(mRow.x + mRow.width - 170f, mRow.y + 9f, 78f, 28f), "PROFILE", 11))
                        {
                            selectedProfileMember = m;
                        }
                        if (DrawFantasyButton(new Rect(mRow.x + mRow.width - 88f, mRow.y + 9f, 82f, 28f), "GIAO DỊCH", 11, true, new Color(0.35f, 0.85f, 1f)))
                        {
                            guildTradeNotification = $"🤝 Đã gửi lời mời giao dịch đến [{m.memberName}]!";
                            guildTradeNotifyTimer = 3.5f;
                        }
                    }
                }
                GUI.EndScrollView();

                // 3. Đáy Cửa Sổ: Giải Tán Bang (Nếu là Bang Chủ) hoặc Rời Bang (Nếu là Thành Viên)
                bool isActionRequested = gm.IsLeaveRequested || gm.IsDisbandRequested;
                if (!isActionRequested)
                {
                    Rect actionBtnRect = new Rect(modalX + modalW - 16f - 145f, bottomY + 3f, 145f, 38f);
                    string actionLabel = isLeader ? "GIẢI TÁN BANG" : "RỜI BANG";
                    Color actionColor = isLeader ? new Color(1f, 0.28f, 0.28f) : new Color(1f, 0.38f, 0.38f);

                    if (DrawFantasyButton(actionBtnRect, actionLabel, 12, true, actionColor))
                    {
                        if (isLeader)
                        {
                            gm.RequestDisbandGuild();
                        }
                        else
                        {
                            gm.RequestLeaveGuild();
                        }
                    }
                }
                else
                {
                    // Đang trong tiến trình Double-Check bảo vệ đếm ngược 60s
                    float cd = gm.LeaveGuildCountdown;
                    bool unlocked = gm.IsDisbandRequested ? gm.IsDisbandUnlocked : gm.IsLeaveUnlocked;

                    Rect leaveBar = new Rect(modalX + 16f, bottomY - 2f, modalW - 32f, 44f);
                    DrawFantasyFrame(leaveBar, new Color(0.12f, 0.05f, 0.06f, 0.98f), new Color(0.85f, 0.3f, 0.3f, 0.65f), new Color(1f, 0.4f, 0.4f, 0.85f));

                    boldWhiteStyle.fontSize = 11;
                    boldWhiteStyle.alignment = TextAnchor.MiddleLeft;
                    boldWhiteStyle.normal.textColor = unlocked ? new Color(1f, 0.4f, 0.4f) : new Color(1f, 0.85f, 0.3f);

                    string statusMsg;
                    if (gm.IsDisbandRequested)
                    {
                        statusMsg = unlocked
                            ? "⚠️ ĐÃ MỞ KHÓA! XÁC NHẬN GIẢI TÁN BANG:"
                            : $"⏳ BẢO VỆ: CHỜ {cd:0.0}s ĐỂ GIẢI TÁN BANG";
                    }
                    else
                    {
                        statusMsg = unlocked
                            ? "⚠️ ĐÃ MỞ KHÓA! XÁC NHẬN RỜI BANG:"
                            : $"⏳ BẢO VỆ: CHỜ {cd:0.0}s ĐỂ RỜI BANG";
                    }

                    GUI.Label(new Rect(leaveBar.x + 10f, leaveBar.y + 10f, leaveBar.width - 200f, 24f), statusMsg, boldWhiteStyle);

                    if (DrawFantasyButton(new Rect(leaveBar.x + leaveBar.width - 188f, leaveBar.y + 6f, 75f, 32f), "✕ HỦY", 12))
                    {
                        if (gm.IsDisbandRequested)
                            gm.CancelDisbandGuild();
                        else
                            gm.CancelLeaveGuild();
                    }

                    string confirmBtnText = unlocked
                        ? (gm.IsDisbandRequested ? "⚠️ GIẢI TÁN" : "⚠️ XÁC NHẬN")
                        : $"KHÓA ({cd:0}s)";
                    Color confirmBtnColor = unlocked ? new Color(1f, 0.25f, 0.25f) : new Color(0.5f, 0.5f, 0.5f);

                    if (DrawFantasyButton(new Rect(leaveBar.x + leaveBar.width - 108f, leaveBar.y + 6f, 102f, 32f), confirmBtnText, 11, unlocked, confirmBtnColor))
                    {
                        if (unlocked)
                        {
                            if (gm.IsDisbandRequested)
                            {
                                gm.ConfirmDisbandGuild(playerStats);
                            }
                            else
                            {
                                gm.ConfirmLeaveGuild(playerStats);
                            }
                        }
                    }
                }

                // Popup Cài Đặt Bang Hội (Dành cho Bang Chủ)
                if (isGuildSettingsOpen && isLeader)
                {
                    DrawGuildSettingsPopup(curGuild, playerStats);
                }

                // Popup Profile Xem Thông Tin Thành Viên nếu được chọn
                if (selectedProfileMember != null)
                {
                    DrawMemberProfilePopup(selectedProfileMember);
                }
            }
        }

        private void DrawJoinCodeModal(GodlessFantasy.Guild.GuildData targetGuild, GodlessFantasy.Player.PlayerStats playerStats)
        {
            float mWidth = 360f;
            float mHeight = 225f;
            float mX = (Screen.width - mWidth) / 2f;
            float mY = (Screen.height - mHeight) / 2f;
            Rect modalRect = new Rect(mX, mY, mWidth, mHeight);

            GUI.depth = -200;
            DrawFantasyFrame(modalRect, new Color(0.04f, 0.06f, 0.10f, 0.98f), new Color(0.35f, 0.85f, 1f, 0.95f), new Color(0.6f, 0.95f, 1f, 1f));

            boldWhiteStyle.fontSize = 12;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(0.35f, 0.85f, 1f);
            GUI.Label(new Rect(modalRect.x, modalRect.y + 10f, modalRect.width, 20f), $"🔐 XÁC THỰC GIA NHẬP: {targetGuild.guildName}", boldWhiteStyle);

            standardTextStyle.fontSize = 11;
            standardTextStyle.alignment = TextAnchor.MiddleCenter;
            standardTextStyle.normal.textColor = new Color(0.85f, 0.9f, 1f);
            GUI.Label(new Rect(modalRect.x + 12f, modalRect.y + 34f, modalRect.width - 24f, 18f), $"Bang chủ: {targetGuild.leaderName} · Cấp {targetGuild.guildLevel}", standardTextStyle);
            GUI.Label(new Rect(modalRect.x + 12f, modalRect.y + 52f, modalRect.width - 24f, 18f), "Nhập Mã Code / Pass Bang do Bang Chủ cấp:", standardTextStyle);

            joinCodeInput = GUI.TextField(new Rect(modalRect.x + 40f, modalRect.y + 74f, modalRect.width - 80f, 24f), joinCodeInput);

            var gm = GodlessFantasy.Guild.GuildManager.Instance;
            int attemptsLeft = (gm != null) ? gm.RemainingDailyJoinAttempts : 3;

            standardTextStyle.fontSize = 10;
            standardTextStyle.alignment = TextAnchor.MiddleCenter;
            standardTextStyle.normal.textColor = attemptsLeft > 1 ? new Color(0.4f, 0.9f, 1f) : new Color(1f, 0.45f, 0.3f);
            GUI.Label(new Rect(modalRect.x + 12f, modalRect.y + 100f, modalRect.width - 24f, 18f), $"⏳ Giới hạn bảo vệ: Còn {attemptsLeft}/3 lượt thử hôm nay", standardTextStyle);

            if (!string.IsNullOrEmpty(joinCodeError))
            {
                standardTextStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);
                GUI.Label(new Rect(modalRect.x + 12f, modalRect.y + 120f, modalRect.width - 24f, 20f), joinCodeError, standardTextStyle);
            }

            bool canTryJoin = attemptsLeft > 0;
            Color joinBtnCol = canTryJoin ? new Color(0.3f, 0.9f, 0.5f) : new Color(0.5f, 0.5f, 0.5f);
            if (DrawFantasyButton(new Rect(modalRect.x + 30f, modalRect.y + 160f, 140f, 32f), canTryJoin ? "GIA NHẬP" : "HẾT LƯỢT", 12, canTryJoin, joinBtnCol))
            {
                if (gm != null)
                {
                    if (gm.JoinGuildWithCode(targetGuild.guildId, joinCodeInput, playerStats, out string err))
                    {
                        isJoinCodeModalOpen = false;
                        joiningGuildTarget = null;
                        joinCodeError = "";
                    }
                    else
                    {
                        joinCodeError = err;
                    }
                }
            }

            if (DrawFantasyButton(new Rect(modalRect.x + 190f, modalRect.y + 160f, 140f, 32f), "✕ HỦY", 12))
            {
                isJoinCodeModalOpen = false;
                joiningGuildTarget = null;
                joinCodeError = "";
            }
        }

        private void DrawGuildSettingsPopup(GodlessFantasy.Guild.GuildData curGuild, GodlessFantasy.Player.PlayerStats playerStats)
        {
            float sWidth = 400f;
            float sHeight = 250f;
            float sX = (Screen.width - sWidth) / 2f;
            float sY = (Screen.height - sHeight) / 2f;
            Rect sRect = new Rect(sX, sY, sWidth, sHeight);

            GUI.depth = -200;
            DrawFantasyFrame(sRect, new Color(0.04f, 0.06f, 0.10f, 0.98f), new Color(1f, 0.85f, 0.35f, 0.95f), new Color(1f, 0.95f, 0.6f, 1f));

            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);
            GUI.Label(new Rect(sRect.x, sRect.y + 10f, sRect.width, 22f), "⚙️ THIẾT LẬP BANG HỘI (BANG CHỦ)", boldWhiteStyle);

            standardTextStyle.fontSize = 11;
            standardTextStyle.alignment = TextAnchor.MiddleLeft;
            standardTextStyle.normal.textColor = Color.white;

            // 1. Pass Code Gia Nhập
            GUI.Label(new Rect(sRect.x + 16f, sRect.y + 40f, 180f, 20f), "🔑 Mã Code Gia Nhập Bang:", standardTextStyle);
            settingPassCodeInput = GUI.TextField(new Rect(sRect.x + 190f, sRect.y + 40f, 190f, 22f), settingPassCodeInput);

            // 2. Khẩu Hiệu Bang (Tối đa 50 ký tự)
            GUI.Label(new Rect(sRect.x + 16f, sRect.y + 75f, 300f, 20f), "📜 Khẩu Hiệu Bang (Tối đa 50 ký tự):", standardTextStyle);
            settingSloganInput = GUI.TextField(new Rect(sRect.x + 16f, sRect.y + 98f, sRect.width - 32f, 24f), settingSloganInput);
            if (settingSloganInput.Length > 50) settingSloganInput = settingSloganInput.Substring(0, 50);

            standardTextStyle.fontSize = 10;
            standardTextStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);
            GUI.Label(new Rect(sRect.x + 16f, sRect.y + 126f, sRect.width - 32f, 18f), $"• Độ dài hiện tại: [{settingSloganInput.Length}/50 ký tự]", standardTextStyle);

            // Nút Lưu & Đóng
            if (DrawFantasyButton(new Rect(sRect.x + 30f, sRect.y + 185f, 160f, 34f), "💾 LƯU THAY ĐỔI", 12, true, new Color(1f, 0.85f, 0.35f)))
            {
                var gm = GodlessFantasy.Guild.GuildManager.Instance;
                if (gm != null)
                {
                    gm.UpdateGuildSettings(settingSloganInput, settingPassCodeInput, playerStats);
                    guildTradeNotification = "✅ Đã lưu Cài Đặt Bang Hội thành công!";
                    guildTradeNotifyTimer = 3.5f;
                    isGuildSettingsOpen = false;
                }
            }

            if (DrawFantasyButton(new Rect(sRect.x + 210f, sRect.y + 185f, 160f, 34f), "✕ ĐÓNG", 12))
            {
                isGuildSettingsOpen = false;
            }
        }

        private void DrawMemberProfilePopup(GodlessFantasy.Guild.GuildMember member)
        {
            float pW = 320f;
            float pH = 210f;
            float pX = (Screen.width - pW) / 2f;
            float pY = (Screen.height - pH) / 2f;
            Rect pRect = new Rect(pX, pY, pW, pH);

            GUI.depth = -150;
            DrawFantasyFrame(pRect, new Color(0.04f, 0.06f, 0.10f, 0.98f), new Color(0.85f, 0.70f, 0.25f, 0.95f), new Color(1f, 0.88f, 0.45f, 1f));

            boldWhiteStyle.fontSize = 13;
            boldWhiteStyle.alignment = TextAnchor.MiddleCenter;
            boldWhiteStyle.normal.textColor = new Color(1f, 0.88f, 0.25f);
            GUI.Label(new Rect(pRect.x, pRect.y + 10f, pRect.width, 22f), $"👤 HỒ SƠ THÀNH VIÊN", boldWhiteStyle);

            standardTextStyle.alignment = TextAnchor.UpperLeft;
            standardTextStyle.fontSize = 11;
            standardTextStyle.normal.textColor = Color.white;

            string info =
                $"• Tên nhân vật: <color=#00e5ff><b>{member.memberName}</b></color>\n" +
                $"• Cấp độ: <color=yellow><b>Cấp {member.level}</b></color>\n" +
                $"• Chức nghiệp: <color=#ffaa00><b>{member.characterClass}</b></color>\n" +
                $"• Thuộc tính: <color=#55ff55><b>{member.element}</b></color>\n" +
                $"• Vị trí hiện tại: <b>{member.currentMap}</b>\n" +
                $"• Trạng thái: {(member.isOnline ? "<color=#55ff55>Online</color>" : "<color=#888888>Offline</color>")}";

            GUI.Label(new Rect(pRect.x + 16f, pRect.y + 40f, pRect.width - 32f, 120f), info, standardTextStyle);

            if (DrawFantasyButton(new Rect(pRect.x + (pRect.width - 100f) / 2f, pRect.y + pRect.height - 38f, 100f, 28f), "ĐÓNG", 11, true))
            {
                selectedProfileMember = null;
            }
        }

        // ================= TOOLTIP POPUP (GIỚI HẠN MÀN HÌNH & PADDING CHUẨN) =================
        private void SetTooltip(string title, string body, Vector2 pos, bool anchorFromBottom = false, string keybindTag = null)
        {
            activeTooltipTitle = title;
            activeTooltipBody = body;
            activeTooltipPos = pos;
            activeTooltipAnchorBottom = anchorFromBottom;
            activeTooltipKeybind = keybindTag;
            showTooltip = true;
        }

        private void DrawTooltipPopup()
        {
            float w = 380f;
            if (tooltipDescStyle == null) InitStyles();
            float titleH = 24f;
            float bodyH = tooltipDescStyle.CalcHeight(new GUIContent(activeTooltipBody), w - 24f);
            float h = Mathf.Max(64f, titleH + bodyH + 20f);

            float clampedX = Mathf.Clamp(activeTooltipPos.x, 16f, Screen.width - w - 16f);
            float tooltipY = activeTooltipAnchorBottom ? (activeTooltipPos.y - h) : activeTooltipPos.y;
            float clampedY = Mathf.Clamp(tooltipY, 16f, Screen.height - h - 16f);
            Rect r = new Rect(clampedX, clampedY, w, h);

            DrawFantasyFrame(r, new Color(0.04f, 0.06f, 0.12f, 0.98f), new Color(0.85f, 0.70f, 0.30f, 0.95f), new Color(1f, 0.90f, 0.45f, 1f));

            // Tách Keybind tag: ưu tiên activeTooltipKeybind rõ ràng, hoặc trích xuất từ tiêu đề
            string displayTitle = activeTooltipTitle;
            string keyBadge = activeTooltipKeybind;
            if (string.IsNullOrEmpty(keyBadge))
            {
                int keyIdx = activeTooltipTitle.LastIndexOf("[Phím ");
                if (keyIdx >= 0)
                {
                    keyBadge = activeTooltipTitle.Substring(keyIdx).Trim();
                    displayTitle = activeTooltipTitle.Substring(0, keyIdx).Trim();
                }
            }

            float badgeW = !string.IsNullOrEmpty(keyBadge) ? 76f : 0f;
            Rect titleRect = new Rect(r.x + 12, r.y + 7, r.width - 24 - (badgeW > 0 ? (badgeW + 8f) : 0f), 20);

            // Vẽ tiêu đề: Nếu dài quá sẽ chạy chữ mượt mà từ phải sang trái (Marquee Auto-Scroll)
            DrawAutoScrollText(titleRect, displayTitle, tooltipTitleStyle, 35f, new Color(1f, 0.88f, 0.45f));

            // Vẽ Badge phím nóng cố định bên phải (Sáng rõ, nổi bật, tuyệt đối không bao giờ bị cắt mất số phím!)
            if (!string.IsNullOrEmpty(keyBadge))
            {
                Rect badgeRect = new Rect(r.x + r.width - 12 - badgeW, r.y + 6, badgeW, 20);
                GUI.color = new Color(0.10f, 0.16f, 0.28f, 0.98f);
                GUI.DrawTexture(badgeRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawOutline(badgeRect, new Color(1f, 0.82f, 0.35f, 0.9f), 1);

                keybindBadgeStyle.alignment = TextAnchor.MiddleCenter;
                keybindBadgeStyle.fontSize = 11;
                keybindBadgeStyle.fontStyle = FontStyle.Bold;
                keybindBadgeStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);
                GUI.Label(badgeRect, keyBadge, keybindBadgeStyle);
            }

            // Vạch phân cách vàng kim
            GUI.color = new Color(0.85f, 0.70f, 0.30f, 0.6f);
            GUI.DrawTexture(new Rect(r.x + 12, r.y + 29, r.width - 24, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            tooltipDescStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(r.x + 12, r.y + 34, r.width - 24, bodyH + 4f), activeTooltipBody, tooltipDescStyle);
        }

        /// <summary>
        /// Quy tắc hiển thị trường chuẩn: Tên trường: thông tin (mô tả)
        /// Đoạn tiền tố (Tên trường:) CỐ ĐỊNH ở bên trái (không bao giờ bị trôi).
        /// Đoạn thông tin (mô tả) nằm bên phải, và CHỈ đoạn này mới tự động chạy chữ (marquee) nếu vượt quá chiều rộng.
        /// </summary>
        public static void DrawFieldWithAutoScroll(Rect totalRect, string labelPrefix, string scrollableValue, GUIStyle style, float speed = 30f, Color? valueColor = null)
        {
            if (string.IsNullOrEmpty(labelPrefix))
            {
                DrawAutoScrollText(totalRect, scrollableValue, style, speed, valueColor);
                return;
            }

            // 1. Tính toán chiều rộng của nhãn tiền tố cố định
            string plainPrefix = labelPrefix.Contains("<")
                ? System.Text.RegularExpressions.Regex.Replace(labelPrefix, "<.*?>", string.Empty)
                : labelPrefix;
            float prefixW = style.CalcSize(new GUIContent(plainPrefix)).x;
            if (labelPrefix.Contains("<b>")) prefixW *= 1.15f;

            Rect labelRect = new Rect(totalRect.x, totalRect.y, prefixW, totalRect.height);

            // Vẽ tiền tố nhãn CỐ ĐỊNH (Không bao giờ bị trôi/chạy mất)
            var prevClip = style.clipping;
            style.clipping = TextClipping.Overflow;
            GUI.Label(labelRect, labelPrefix, style);
            style.clipping = prevClip;

            // 2. Phần giá trị thông tin nằm bên phải nhãn
            float valX = totalRect.x + prefixW;
            float valW = Mathf.Max(10f, totalRect.width - prefixW);
            Rect valRect = new Rect(valX, totalRect.y, valW, totalRect.height);

            // Chỉ phần giá trị này mới tự động chạy chữ (marquee) nếu bị vượt quá chiều rộng
            DrawAutoScrollText(valRect, scrollableValue, style, speed, valueColor);
        }

        /// <summary>
        /// Vẽ text tự động chạy chữ (Marquee Auto-Scroll) từ phải qua trái nếu text dài quá kích thước khung Rect.
        /// </summary>
        public static void DrawAutoScrollText(Rect r, string text, GUIStyle style, float speed = 35f, Color? textColor = null, float gap = 36f, bool forceScroll = false)
        {
            if (string.IsNullOrEmpty(text)) return;

            bool prevWrap = style.wordWrap;
            style.wordWrap = false;

            string plainText = text.Contains("<")
                ? System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty)
                : text;
            GUIContent gc = new GUIContent(plainText);
            float textW = style.CalcSize(gc).x;
            if (text.Contains("<b>") || text.Contains("<color"))
            {
                textW *= 1.15f; // Bù kích thước thẻ đậm và ký tự có định dạng màu
            }

            Color originalColor = style.normal.textColor;
            Color drawColor = textColor ?? originalColor;

            // Nếu vừa vặn thoải mái trong khung (lề 4px) và không ép buộc chạy chữ thì vẽ tĩnh bình thường
            if (!forceScroll && textW <= r.width - 4f)
            {
                // Đổ bóng nếu không chứa thẻ màu inline (để tránh bóng bị đè màu inline)
                if (!text.Contains("<color"))
                {
                    style.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
                    GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, style);
                }

                // Chữ chính
                style.normal.textColor = drawColor;
                GUI.Label(r, text, style);
                style.normal.textColor = originalColor;
                style.wordWrap = prevWrap;
                return;
            }

            // Nếu dài quá hoặc được yêu cầu thì cuộn từ phải sang trái (Marquee) bên trong khung cắt BeginGroup
            var prevAlign = style.alignment;
            style.alignment = TextAnchor.MiddleLeft;

            GUI.BeginGroup(r);

            float travelDistance = textW + gap;
            float offset = -(Time.time * speed % travelDistance);

            // Đổ bóng đen nếu không chứa thẻ màu inline
            if (!text.Contains("<color"))
            {
                style.normal.textColor = new Color(0f, 0f, 0f, 0.9f);
                GUI.Label(new Rect(offset + 1, 1, textW + 30, r.height), text, style);
                GUI.Label(new Rect(offset + travelDistance + 1, 1, textW + 30, r.height), text, style);
                if (offset + travelDistance * 2 < r.width)
                {
                    GUI.Label(new Rect(offset + travelDistance * 2 + 1, 1, textW + 30, r.height), text, style);
                }
            }

            // Chữ chính (rộng rãi textW + 30 không bao giờ bị cắt ở cuối)
            style.normal.textColor = drawColor;
            GUI.Label(new Rect(offset, 0, textW + 30, r.height), text, style);
            GUI.Label(new Rect(offset + travelDistance, 0, textW + 30, r.height), text, style);
            if (offset + travelDistance * 2 < r.width)
            {
                GUI.Label(new Rect(offset + travelDistance * 2, 0, textW + 30, r.height), text, style);
            }

            GUI.EndGroup();

            style.alignment = prevAlign;
            style.normal.textColor = originalColor;
            style.wordWrap = prevWrap;
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

        // ================= TIỆN ÍCH KHUNG VIỀN VÀNG ĐỒNG BỘ FANTASY THEME =================
        private void DrawDoubleGoldFrame(Rect r, int borderWidth = 1)
        {
            FantasyUITheme.DrawDoubleGoldFrame(r, borderWidth);
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
