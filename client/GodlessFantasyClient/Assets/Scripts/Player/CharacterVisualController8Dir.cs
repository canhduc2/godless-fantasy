using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Core;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Player
{
    public enum SkinType
    {
        None = 0,         // Phôi Gốc Tách Lớp Modular Paperdoll (Áo ba lỗ / croptop, quần đùi, giáp, nón, khăn quàng)
        SoulKnight = 1,   // Ngoại Trang: Hiệp Sĩ Lam Giáp Soul Knight (Mũ sắt khe vàng, khăn quàng cam tung bay)
        VoidReaper = 2    // Ngoại Trang: Vu Sư Tử Linh Void Reaper (Mũ trùm xám vương miện vàng, mắt thập tự đỏ, xúc tu bay)
    }

    /// <summary>
    /// Bộ Điều Khiển Đồ Họa & Animation Đa Lớp 8 Hướng (8-Directional Soul Knight Chibi Paperdoll Engine):
    /// - Chuẩn phong cách Chibi Soul Knight: Đầu to 2 tầng, chân ngắn (stubby feet), viền đen sắc nét.
    /// - Quản lý ngoại hình nhân vật theo cơ chế Modular Layered Paperdoll:
    ///   + Lớp Thân (Body: Nam / Nữ cute anime)
    ///   + Lớp Quần (Pants)
    ///   + Lớp Giáp/Áo (Armor)
    ///   + Lớp Mũ/Nón (Helmet)
    ///   + Lớp Khăn Quàng (Scarf - Khăn quàng cam Soul Knight bay bồng bềnh)
    ///   + Lớp Vũ Khí (Weapon - Tự xoay 360° theo hướng ngắm chuột & vung chém khi tấn công)
    ///   + Lớp Vệt Chém Nguyên Tố (Elemental Slash Arc VFX: Hỏa, Băng, Lôi, Phong, Quang, Ám, Mộc)
    ///   + Lớp Ngoại Trang (Costume / Full Skin: Soul Knight Paladin, Void Reaper)
    ///   + Lớp Bóng Đổ (Ground Shadow)
    /// - Hoạt ảnh 8 hướng x 4 Frame bước đi (Walk Cycle) nhịp nhàng tự nhiên.
    /// - Khử 100% micro-stutter / lag di chuyển bằng RigidbodyInterpolation2D.
    /// - Phím tắt nhanh: [G] Đổi Nam/Nữ, [K] Đổi Ngoại Trang (Skins).
    /// </summary>
    public class CharacterVisualController8Dir : MonoBehaviour
    {
        public static CharacterVisualController8Dir Instance { get; private set; }

        [Header("Gốc Hoạt Ảnh & Phân Tầng")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer shadowRenderer;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer pantsRenderer;
        [SerializeField] private SpriteRenderer armorRenderer;
        [SerializeField] private SpriteRenderer helmetRenderer;
        [SerializeField] private SpriteRenderer scarfRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private SpriteRenderer slashRenderer;
        [SerializeField] private SpriteRenderer costumeRenderer;

        [Header("Trạng Thái Ngoại Hình")]
        public bool IsFemale = false;
        public SkinType CurrentSkin = SkinType.None;
        public Direction8 CurrentDirection = Direction8.South;

        private Camera mainCamera;
        private Rigidbody2D rb;
        private PlayerMovement playerMovement;
        private InventoryManager inventoryManager;

        // Bộ đệm Sprite tránh tải trùng lặp
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        // Hoạt ảnh
        private float walkCycleTimer = 0f;
        private float ghostSpawnTimer = 0f;
        private Vector3 initialVisualPos;
        private Vector3 initialWeaponPos;
        private bool isAttacking = false;

        private void Awake()
        {
            Instance = this;
            spriteCache.Clear();
            rb = GetComponent<Rigidbody2D>();
            playerMovement = GetComponent<PlayerMovement>();
            inventoryManager = GetComponent<InventoryManager>();
            mainCamera = Camera.main;

            // Khử triệt để hiện tượng giật lag / jitter khi di chuyển bằng nội suy vật lý
            if (rb != null)
            {
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            SetupVisualHierarchy();
        }

        private void SetupVisualHierarchy()
        {
            if (visualRoot == null)
            {
                var existingRoot = transform.Find("VisualRoot");
                if (existingRoot != null) visualRoot = existingRoot;
                else
                {
                    var rootObj = new GameObject("VisualRoot");
                    rootObj.transform.parent = transform;
                    rootObj.transform.localPosition = new Vector3(0f, -0.48f, 0f);
                    visualRoot = rootObj.transform;
                }
            }
            visualRoot.localPosition = new Vector3(0f, -0.48f, 0f);
            initialVisualPos = visualRoot.localPosition;

            // Tắt SpriteRenderer ở GameObject gốc (nếu có) để tránh xung đột
            var rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null) rootSr.enabled = false;

            // 1. Bóng đổ dưới chân (SortingOrder: 7) - Khớp 100% tâm bàn chân
            if (shadowRenderer == null)
            {
                var sObj = new GameObject("Shadow");
                sObj.transform.parent = transform;
                sObj.transform.localPosition = new Vector3(0f, -0.48f, 0f);
                shadowRenderer = sObj.AddComponent<SpriteRenderer>();
                shadowRenderer.sortingOrder = 7;
                shadowRenderer.color = new Color(0, 0, 0, 0.35f);
                shadowRenderer.sprite = LoadSprite("indicator_circle");
                sObj.transform.localScale = new Vector3(0.55f, 0.28f, 1f);
            }
            else
            {
                shadowRenderer.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            }

            // 2. Thân nhân vật (SortingOrder: 10)
            if (bodyRenderer == null)
            {
                var bObj = CreateLayerObject("Layer_Body", 10);
                bodyRenderer = bObj.GetComponent<SpriteRenderer>();
            }
            string initGender = IsFemale ? "female" : "male";
            if (bodyRenderer != null && bodyRenderer.sprite == null)
            {
                bodyRenderer.sprite = LoadSprite($"Characters/player_base_{initGender}_south_0");
            }

            // 3. Quần (SortingOrder: 11)
            if (pantsRenderer == null)
            {
                var pObj = CreateLayerObject("Layer_Pants", 11);
                pantsRenderer = pObj.GetComponent<SpriteRenderer>();
            }

            // 4. Giáp / Áo (SortingOrder: 12)
            if (armorRenderer == null)
            {
                var aObj = CreateLayerObject("Layer_Armor", 12);
                armorRenderer = aObj.GetComponent<SpriteRenderer>();
            }

            // 5. Mũ / Nón (SortingOrder: 13)
            if (helmetRenderer == null)
            {
                var hObj = CreateLayerObject("Layer_Helmet", 13);
                helmetRenderer = hObj.GetComponent<SpriteRenderer>();
            }

            // 6. Khăn Quàng Soul Knight (SortingOrder: 14)
            if (scarfRenderer == null)
            {
                var scObj = CreateLayerObject("Layer_Scarf", 14);
                scarfRenderer = scObj.GetComponent<SpriteRenderer>();
            }

            // 7. Ngoại Trang Toàn Thần (Costume / Skin) (SortingOrder: 14)
            if (costumeRenderer == null)
            {
                var cObj = CreateLayerObject("Layer_Costume", 14);
                costumeRenderer = cObj.GetComponent<SpriteRenderer>();
                costumeRenderer.enabled = false;
            }

            // 8. Vũ Khí (Weapon) (SortingOrder: 15)
            if (weaponRenderer == null)
            {
                var wObj = CreateLayerObject("Layer_Weapon", 15);
                wObj.transform.localPosition = new Vector3(0.24f, -0.05f, 0);
                weaponRenderer = wObj.GetComponent<SpriteRenderer>();
                weaponRenderer.sprite = LoadSprite("item_weapon");
                wObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }
            initialWeaponPos = weaponRenderer.transform.localPosition;

            // 9. Vệt Chém Đao Khí Nguyên Tố (Slash Arc VFX) (SortingOrder: 16)
            if (slashRenderer == null)
            {
                var slObj = CreateLayerObject("Layer_Slash", 16);
                slObj.transform.localPosition = new Vector3(0.35f, 0, 0);
                slashRenderer = slObj.GetComponent<SpriteRenderer>();
                slashRenderer.enabled = false;
                slObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }
        }

        private GameObject CreateLayerObject(string name, int sortingOrder)
        {
            var child = visualRoot.Find(name);
            GameObject obj;
            if (child != null) obj = child.gameObject;
            else
            {
                obj = new GameObject(name);
                obj.transform.parent = visualRoot;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
            }

            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            return obj;
        }

        private void Update()
        {
            HandleHotkeys();
            UpdateDirectionFromAim();
            UpdateAnimations();
            SyncEquippedVisuals();
        }

        private void HandleHotkeys()
        {
            bool isTypingChat = (GodlessFantasy.UI.CombatHUD.Instance != null && GodlessFantasy.UI.CombatHUD.Instance.IsChatFocused());
            if (isTypingChat) return;

            // Phím [G]: Đổi giới tính Nam <-> Nữ
            if (Input.GetKeyDown(KeyCode.G))
            {
                IsFemale = !IsFemale;
                string genderText = IsFemale ? "Nữ Anime (Croptop)" : "Nam Anime (Áo Ba Lỗ)";
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.3f, $"🚻 Giới Tính: {genderText} [G]", new Color(0.3f, 0.9f, 1f));
            }

            // Phím [K]: Chuyển đổi Ngoại Trang (Skins)
            if (Input.GetKeyDown(KeyCode.K))
            {
                int nextSkin = ((int)CurrentSkin + 1) % 3;
                CurrentSkin = (SkinType)nextSkin;
                string skinName = CurrentSkin == SkinType.SoulKnight ? "Hiệp Sĩ Soul Knight" :
                                 (CurrentSkin == SkinType.VoidReaper ? "Vu Sư Tử Linh (Void Reaper)" : "Phôi Gốc Tách Lớp");
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f, $"🎭 Ngoại Trang: {skinName} [K]", new Color(1f, 0.85f, 0.2f));
            }
        }

        /// <summary>
        /// Kích hoạt trạng thái Ẩn Thân trong bụi rậm (Stealth Camouflage):
        /// - Làm mờ trong suốt toàn bộ cơ thể (Body, Quần, Giáp, Mũ, Khăn, Vũ khí) với Alpha = 0.35f
        /// - Giúp người chơi cảm nhận rõ rệt đang ẩn nấp sau tán lá cây!
        /// </summary>
        public void SetStealth(bool inStealth)
        {
            float targetAlpha = inStealth ? 0.35f : 1.0f;
            SetLayerAlpha(bodyRenderer, targetAlpha);
            SetLayerAlpha(pantsRenderer, targetAlpha);
            SetLayerAlpha(armorRenderer, targetAlpha);
            SetLayerAlpha(helmetRenderer, targetAlpha);
            SetLayerAlpha(scarfRenderer, targetAlpha);
            SetLayerAlpha(weaponRenderer, targetAlpha);
            SetLayerAlpha(costumeRenderer, targetAlpha);
        }

        private void SetLayerAlpha(SpriteRenderer sr, float alpha)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        private void UpdateDirectionFromAim()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector2 aimDir;
            // Nếu người chơi đang di chuyển thì xoay mặt theo hướng di chuyển
            if (playerMovement != null && playerMovement.IsMoving && rb != null && rb.linearVelocity.sqrMagnitude > 0.04f)
            {
                aimDir = rb.linearVelocity.normalized;
            }
            else
            {
                Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
                aimDir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
            }

            CurrentDirection = Direction8Utils.VectorToDirection8(aimDir);

            // Giữ vững kích thước gốc (Scale 1:1), tuyệt đối không nhân dồn gây bẹp/mỏng nhân vật
            visualRoot.localScale = Vector3.one;
        }

        private void UpdateAnimations()
        {
            bool isDashing = playerMovement != null && playerMovement.IsDashing;
            bool isMoving = (playerMovement != null && playerMovement.IsMoving) || (rb != null && rb.linearVelocity.sqrMagnitude > 0.04f) || isDashing;

            // ================= TRẠNG THÁI DASH (LƯỚT) =================
            if (isDashing)
            {
                visualRoot.localScale = new Vector3(1.15f, 0.88f, 1f);
                visualRoot.localPosition = initialVisualPos;
                ghostSpawnTimer += Time.deltaTime;
                if (ghostSpawnTimer > 0.04f)
                {
                    ghostSpawnTimer = 0f;
                    SpawnGhostAfterimage();
                }
                UpdateWeaponVisuals(isMoving: true, bob: 0f, breath: 0f);
                UpdateFrameSprites(isMoving: true, isDash: true);
                return;
            }

            // ================= TRẠNG THÁI DI CHUYỂN (WALK) =================
            if (isMoving)
            {
                walkCycleTimer += Time.deltaTime * 9.5f;
                float bob = Mathf.Abs(Mathf.Sin(walkCycleTimer)) * 0.05f;
                float tilt = Mathf.Sin(walkCycleTimer) * 2.8f;

                visualRoot.localScale = Vector3.one;
                visualRoot.localPosition = initialVisualPos + new Vector3(0, bob, 0);
                visualRoot.localRotation = Quaternion.Euler(0, 0, tilt);

                UpdateWeaponVisuals(isMoving: true, bob: bob, breath: 0f);
                UpdateFrameSprites(isMoving: true, isDash: false);
            }
            // ================= TRẠNG THÁI NGHỈ (IDLE) =================
            else
            {
                walkCycleTimer = 0f;
                visualRoot.localRotation = Quaternion.identity;

                // Nhịp thở nhấp nhô nhẹ nhàng
                float breath = Mathf.Sin(Time.time * 2.8f);
                float squash = breath * 0.015f;
                float stretch = breath * 0.025f;

                visualRoot.localScale = new Vector3(1f - squash, 1f + stretch, 1f);
                visualRoot.localPosition = initialVisualPos;

                UpdateWeaponVisuals(isMoving: false, bob: 0f, breath: breath);
                UpdateFrameSprites(isMoving: false, isDash: false);
            }
        }

        /// <summary>
        /// Quản lý vị trí, xoay, độ sâu (depth) và bám 8 hướng cho Vũ Khí (Weapon 8-Direction Tracking & Orbit)
        /// Cách nhân vật 1 khoảng vừa vặn, lơ lửng ma pháp không đâm xuyên vào người.
        /// </summary>
        private void UpdateWeaponVisuals(bool isMoving, float bob, float breath)
        {
            if (weaponRenderer == null || isAttacking) return;

            // 1. Xác định vị trí mục tiêu (Socket Offset), góc xoay (Angle), lật X (FlipX) và Thứ tự hiển thị (SortingOrder) theo 8 hướng
            Vector3 targetOffset;
            float targetAngle;
            bool flipX = false;
            int sortingOrder = 15;

            // Tọa độ socket theo từng hướng với khoảng cách orbit ~0.38f - 0.44f
            switch (CurrentDirection)
            {
                case Direction8.East:
                    targetOffset = new Vector3(0.40f, -0.04f, 0f);
                    targetAngle = -15f;
                    flipX = false;
                    sortingOrder = 15;
                    break;

                case Direction8.West:
                    targetOffset = new Vector3(-0.40f, -0.04f, 0f);
                    targetAngle = 15f;
                    flipX = true;
                    sortingOrder = 15;
                    break;

                case Direction8.South:
                    // Cầm chếch sang bên hông phải phía trước, nổi bật ở mặt tiền, không che háng/chân
                    targetOffset = new Vector3(0.32f, -0.16f, 0f);
                    targetAngle = -22f;
                    flipX = false;
                    sortingOrder = 16;
                    break;

                case Direction8.SouthEast:
                    targetOffset = new Vector3(0.42f, -0.15f, 0f);
                    targetAngle = -32f;
                    flipX = false;
                    sortingOrder = 16;
                    break;

                case Direction8.SouthWest:
                    targetOffset = new Vector3(-0.42f, -0.15f, 0f);
                    targetAngle = 32f;
                    flipX = true;
                    sortingOrder = 16;
                    break;

                case Direction8.North:
                    // Ẩn sau lưng khi nhìn lên hướng Bắc (SortingOrder 8)
                    targetOffset = new Vector3(0.28f, 0.18f, 0f);
                    targetAngle = 18f;
                    flipX = false;
                    sortingOrder = 8;
                    break;

                case Direction8.NorthEast:
                    targetOffset = new Vector3(0.38f, 0.15f, 0f);
                    targetAngle = 12f;
                    flipX = false;
                    sortingOrder = 8;
                    break;

                case Direction8.NorthWest:
                    targetOffset = new Vector3(-0.38f, 0.15f, 0f);
                    targetAngle = -12f;
                    flipX = true;
                    sortingOrder = 8;
                    break;

                default:
                    targetOffset = new Vector3(0.35f, -0.05f, 0f);
                    targetAngle = 0f;
                    break;
            }

            // 2. Tinh chỉnh vi góc theo hướng ngắm chuột (Micro-aim dynamic tilt)
            if (mainCamera != null)
            {
                Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
                Vector2 mouseAim = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
                float aimAngle = Mathf.Atan2(mouseAim.y, mouseAim.x) * Mathf.Rad2Deg;
                float aimTilt = Mathf.Sin(aimAngle * Mathf.Deg2Rad) * 10f;
                if (flipX) aimTilt = -aimTilt;
                targetAngle += aimTilt;
            }

            // 3. Hiệu ứng bồng bềnh lơ lửng / nhấp nhô theo nhịp thở và bước chân
            float floatY = isMoving ? (-bob * 0.7f) : (breath * 0.025f + Mathf.Sin(Time.time * 2.8f) * 0.015f);
            targetOffset.y += floatY;

            // 4. Nội suy mượt mà (Smooth Interpolation)
            weaponRenderer.transform.localPosition = Vector3.Lerp(weaponRenderer.transform.localPosition, targetOffset, Time.deltaTime * 16f);
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            weaponRenderer.transform.localRotation = Quaternion.Slerp(weaponRenderer.transform.localRotation, targetRot, Time.deltaTime * 16f);

            weaponRenderer.flipX = flipX;
            weaponRenderer.sortingOrder = sortingOrder;
        }

        private void UpdateFrameSprites(bool isMoving, bool isDash)
        {
            // Xác định frame bước đi: 0=Idle/Tiếp đất, 1=Bước trái, 2=Thu chân, 3=Bước phải
            int frameIdx = isMoving ? (((int)walkCycleTimer) % 4) : 0;
            string dirName = GetDirectionSpriteName(CurrentDirection);

            // 1. Trường hợp sử dụng Ngoại Trang Toàn Thần (Full Skin)
            if (CurrentSkin == SkinType.SoulKnight)
            {
                SetLayerActive(false);
                costumeRenderer.enabled = true;
                costumeRenderer.sprite = LoadSprite($"Characters/skin_soul_knight_{dirName}_{frameIdx}");
                return;
            }
            else if (CurrentSkin == SkinType.VoidReaper)
            {
                SetLayerActive(false);
                costumeRenderer.enabled = true;
                costumeRenderer.sprite = LoadSprite($"Characters/skin_void_reaper_{dirName}_{frameIdx}");
                return;
            }

            // 2. Trường hợp Phôi Gốc Cậu Bé / Cô Bé (Base Character Body - 8 Hướng Anime)
            costumeRenderer.enabled = false;
            if (pantsRenderer != null) pantsRenderer.enabled = false;
            if (armorRenderer != null) armorRenderer.enabled = false;
            if (helmetRenderer != null) helmetRenderer.enabled = false;
            if (scarfRenderer != null) scarfRenderer.enabled = false;

            string genderStr = IsFemale ? "female" : "male";

            // Tầng Thân Gốc (Body) - Luôn hiển thị cậu bé / cô bé anime theo đúng 8 hướng và frame bước đi
            if (bodyRenderer != null)
            {
                bodyRenderer.enabled = true;
                bodyRenderer.sprite = LoadSprite($"Characters/player_base_{genderStr}_{dirName}_{frameIdx}");
            }
        }

        private void SetLayerActive(bool active)
        {
            if (bodyRenderer != null) bodyRenderer.enabled = active;
            if (pantsRenderer != null) pantsRenderer.enabled = active;
            if (armorRenderer != null) armorRenderer.enabled = active;
            if (helmetRenderer != null) helmetRenderer.enabled = active;
            if (scarfRenderer != null) scarfRenderer.enabled = active;
        }

        private string GetDirectionSpriteName(Direction8 dir)
        {
            switch (dir)
            {
                case Direction8.South: return "south";
                case Direction8.SouthEast: return "south_east";
                case Direction8.East: return "east";
                case Direction8.NorthEast: return "north_east";
                case Direction8.North: return "north";
                case Direction8.NorthWest: return "north_west";
                case Direction8.West: return "west";
                case Direction8.SouthWest: return "south_west";
                default: return "south";
            }
        }

        /// <summary>
        /// Lấy Avatar Texture tương ứng chính xác 100% với Nhân Vật đang hiển thị (HUD & Trang Bị)!
        /// </summary>
        public Texture2D GetPortraitTexture()
        {
            if (CurrentSkin == SkinType.SoulKnight)
                return Resources.Load<Texture2D>("Characters/avatar_soul_knight");
            if (CurrentSkin == SkinType.VoidReaper)
                return Resources.Load<Texture2D>("Characters/avatar_void_reaper");
            return Resources.Load<Texture2D>(IsFemale ? "Characters/avatar_female" : "Characters/avatar_male");
        }

        /// <summary>
        /// Lấy Texture Toàn Thân (Full Body) theo đúng góc nhìn và giới tính hiện tại cho Tab Trang Bị!
        /// </summary>
        public Texture2D GetFullBodyTexture()
        {
            string genderStr = IsFemale ? "female" : "male";
            string dirName = GetDirectionSpriteName(CurrentDirection);
            return Resources.Load<Texture2D>($"Characters/player_base_{genderStr}_{dirName}_0")
                ?? Resources.Load<Texture2D>($"Characters/player_base_{genderStr}_south_0")
                ?? GetPortraitTexture();
        }

        private void SyncEquippedVisuals()
        {
            if (inventoryManager == null) return;

            // Kiểm tra vũ khí đang trang bị
            var wep = inventoryManager.GetEquippedGear(EquipSlot.Weapon);
            if (weaponRenderer != null)
            {
                weaponRenderer.enabled = (wep != null);
                if (wep != null && !string.IsNullOrEmpty(wep.iconName))
                {
                    Sprite s = LoadSprite(wep.iconName) ?? LoadSprite("item_weapon");
                    if (s != null) weaponRenderer.sprite = s;
                }
            }
        }

        /// <summary>
        /// Kích hoạt vung vũ khí chém và bung vệt đao khí / kiếm khí nguyên tố (Elemental Slash VFX)!
        /// </summary>
        public void TriggerAttack(string element = "Hỏa", bool isCast = false)
        {
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(AttackSwingRoutine(element, isCast));
        }

        private IEnumerator AttackSwingRoutine(string element, bool isCast)
        {
            isAttacking = true;

            string cleanElem = element?.ToLower().Trim() ?? "fire";
            if (cleanElem.Contains("hỏa") || cleanElem.Contains("fire")) cleanElem = "fire";
            else if (cleanElem.Contains("băng") || cleanElem.Contains("ice")) cleanElem = "ice";
            else if (cleanElem.Contains("lôi") || cleanElem.Contains("thunder") || cleanElem.Contains("lightning")) cleanElem = "thunder";
            else if (cleanElem.Contains("phong") || cleanElem.Contains("wind")) cleanElem = "wind";
            else if (cleanElem.Contains("quang") || cleanElem.Contains("light") || cleanElem.Contains("holy")) cleanElem = "holy";
            else if (cleanElem.Contains("ám") || cleanElem.Contains("dark")) cleanElem = "dark";
            else if (cleanElem.Contains("mộc") || cleanElem.Contains("nature") || cleanElem.Contains("wood")) cleanElem = "nature";
            else cleanElem = "fire";

            // 1. Vung vũ khí nhanh theo hướng đang quay mặt
            Quaternion startRot = weaponRenderer != null ? weaponRenderer.transform.localRotation : Quaternion.identity;
            bool isFacingLeft = (CurrentDirection == Direction8.West || CurrentDirection == Direction8.NorthWest || CurrentDirection == Direction8.SouthWest);
            float swingDir = isFacingLeft ? -75f : 75f;
            Quaternion swingRot = startRot * Quaternion.Euler(0, 0, swingDir);

            if (weaponRenderer != null) weaponRenderer.transform.localRotation = swingRot;

            // 2. Chạy 3 frame vệt chém đao khí nguyên tố theo đúng 8 hướng
            if (slashRenderer != null)
            {
                Vector3 slashPos = new Vector3(0.42f, 0f, 0f);
                float slashRot = 0f;
                bool slashFlipX = false;

                switch (CurrentDirection)
                {
                    case Direction8.West:
                    case Direction8.SouthWest:
                    case Direction8.NorthWest:
                        slashPos = new Vector3(-0.45f, 0f, 0f);
                        slashFlipX = true;
                        break;
                    case Direction8.South:
                        slashPos = new Vector3(0.12f, -0.38f, 0f);
                        slashRot = -90f;
                        break;
                    case Direction8.North:
                        slashPos = new Vector3(0.12f, 0.38f, 0f);
                        slashRot = 90f;
                        break;
                    case Direction8.SouthEast:
                        slashPos = new Vector3(0.40f, -0.22f, 0f);
                        slashRot = -35f;
                        break;
                    case Direction8.NorthEast:
                        slashPos = new Vector3(0.40f, 0.22f, 0f);
                        slashRot = 35f;
                        break;
                    default:
                        slashPos = new Vector3(0.45f, 0f, 0f);
                        break;
                }

                slashRenderer.transform.localPosition = slashPos;
                slashRenderer.transform.localRotation = Quaternion.Euler(0, 0, slashRot);
                slashRenderer.flipX = slashFlipX;
                slashRenderer.sortingOrder = 17;

                slashRenderer.enabled = true;
                for (int f = 0; f < 3; f++)
                {
                    slashRenderer.sprite = LoadSprite($"Characters/vfx_slash_{cleanElem}_{f}");
                    yield return new WaitForSeconds(0.045f);
                }
                slashRenderer.enabled = false;
            }
            else
            {
                yield return new WaitForSeconds(0.12f);
            }

            if (weaponRenderer != null) weaponRenderer.transform.localRotation = startRot;
            isAttacking = false;
        }

        private void SpawnGhostAfterimage()
        {
            GameObject ghostObj = new GameObject("GhostAfterimage");
            ghostObj.transform.position = visualRoot.position;
            ghostObj.transform.rotation = visualRoot.rotation;
            ghostObj.transform.localScale = visualRoot.lossyScale;

            var sr = ghostObj.AddComponent<SpriteRenderer>();
            sr.sprite = (costumeRenderer != null && costumeRenderer.enabled) ? costumeRenderer.sprite : (bodyRenderer != null ? bodyRenderer.sprite : null);
            sr.sortingOrder = 8;
            sr.color = new Color(0.15f, 0.9f, 1f, 0.50f);

            StartCoroutine(FadeGhost(ghostObj, sr));
        }

        private IEnumerator FadeGhost(GameObject obj, SpriteRenderer sr)
        {
            float elapsed = 0f;
            float dur = 0.25f;
            Color initColor = sr.color;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                sr.color = new Color(initColor.r, initColor.g, initColor.b, Mathf.Lerp(initColor.a, 0f, t));
                yield return null;
            }
            Destroy(obj);
        }

        private Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (spriteCache.TryGetValue(path, out var s) && s != null) return s;

            // 1. Thử Resources.Load<Sprite> theo nhiều format path
            s = Resources.Load<Sprite>(path);
            if (s == null && !path.StartsWith("Characters/"))
            {
                s = Resources.Load<Sprite>($"Characters/{path}");
            }
            string cleanName = path.StartsWith("Characters/") ? path.Substring("Characters/".Length) : path;
            if (s == null)
            {
                s = Resources.Load<Sprite>(cleanName);
            }

            // 2. Fallback sang Texture2D rồi tạo Sprite runtime (Đảm bảo 100% không bao giờ null)
            if (s == null)
            {
                Texture2D tex = Resources.Load<Texture2D>(path) 
                             ?? Resources.Load<Texture2D>($"Characters/{cleanName}") 
                             ?? Resources.Load<Texture2D>(cleanName);
                if (tex != null)
                {
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.1f), 32f);
                }
            }

            // 3. Fallback an toàn tuyệt đối về phôi gốc đã được kiểm chứng hoạt động
            if (s == null)
            {
                string fallbackBase = IsFemale ? "player_female_base" : "player_male_base";
                s = Resources.Load<Sprite>(fallbackBase);
                if (s == null)
                {
                    Texture2D fTex = Resources.Load<Texture2D>(fallbackBase);
                    if (fTex != null)
                    {
                        s = Sprite.Create(fTex, new Rect(0, 0, fTex.width, fTex.height), new Vector2(0.5f, 0.5f), 32f);
                    }
                }
            }

            if (s != null) spriteCache[path] = s;
            return s;
        }

        public void SetGender(bool isFemale)
        {
            IsFemale = isFemale;
        }

        public void SetSkin(SkinType skin)
        {
            CurrentSkin = skin;
        }
    }
}
