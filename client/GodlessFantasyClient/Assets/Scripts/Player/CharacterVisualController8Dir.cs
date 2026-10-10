using System.Collections;
using UnityEngine;
using GodlessFantasy.Core;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Bộ Điều Khiển Đồ Họa & Animation Đa Lớp 8 Hướng (8-Directional Layered Paperdoll Engine):
    /// - Quản lý ngoại hình nhân vật theo chuẩn Game MMORPG 8 hướng:
    ///   + Lớp Thân (Body)
    ///   + Lớp Giáp/Áo (Armor)
    ///   + Lớp Quần (Pants)
    ///   + Lớp Mũ/Nón (Helmet)
    ///   + Lớp Vũ Khí (Weapon - Tự xoay và chuyển vị trí tay theo hướng ngắm)
    ///   + Lớp Bóng Đổ (Ground Shadow)
    /// - Tự động đồng bộ ngoại hình khi thay đổi trang bị trong Túi Đồ.
    /// - Tích hợp toàn diện các trạng thái hoạt ảnh chuyển động:
    ///   + IDLE: Nhịp thở nhấp nhô nhẹ nhàng
    ///   + WALK: Bước đi nảy nhịp, lắc hông tự nhiên
    ///   + DASH (LƯỚT): Đổ người về phía trước, để lại vệt bóng mờ ảo (Ghost Trail)
    ///   + ATTACK/CAST: Vung vũ khí theo đường chém / giật gậy ma pháp
    /// </summary>
    public class CharacterVisualController8Dir : MonoBehaviour
    {
        public static CharacterVisualController8Dir Instance { get; private set; }

        [Header("Gốc Hoạt Ảnh")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer armorRenderer;
        [SerializeField] private SpriteRenderer pantsRenderer;
        [SerializeField] private SpriteRenderer helmetRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private SpriteRenderer shadowRenderer;

        [Header("Trạng Thái Hướng")]
        public Direction8 CurrentDirection = Direction8.South;

        private Camera mainCamera;
        private Rigidbody2D rb;
        private PlayerMovement playerMovement;
        private InventoryManager inventoryManager;

        // Hoạt ảnh
        private float walkCycleTimer = 0f;
        private float ghostSpawnTimer = 0f;
        private Vector3 initialVisualPos;
        private Vector3 initialWeaponPos;

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
            playerMovement = GetComponent<PlayerMovement>();
            inventoryManager = GetComponent<InventoryManager>();
            mainCamera = Camera.main;

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
                    rootObj.transform.localPosition = Vector3.zero;
                    visualRoot = rootObj.transform;
                }
            }
            initialVisualPos = visualRoot.localPosition;

            // 1. Bóng đổ dưới chân
            if (shadowRenderer == null)
            {
                var sObj = new GameObject("Shadow");
                sObj.transform.parent = transform;
                sObj.transform.localPosition = new Vector3(0, -0.38f, 0);
                shadowRenderer = sObj.AddComponent<SpriteRenderer>();
                shadowRenderer.sortingOrder = 7;
                shadowRenderer.color = new Color(0, 0, 0, 0.35f);
                shadowRenderer.sprite = Resources.Load<Sprite>("indicator_circle");
                sObj.transform.localScale = new Vector3(0.55f, 0.28f, 1f);
            }

            // 2. Thân nhân vật
            if (bodyRenderer == null)
            {
                var oldSr = GetComponent<SpriteRenderer>();
                if (oldSr != null)
                {
                    bodyRenderer = oldSr;
                }
                else
                {
                    bodyRenderer = visualRoot.gameObject.AddComponent<SpriteRenderer>();
                }
            }
            bodyRenderer.sortingOrder = 10;
            if (bodyRenderer.sprite == null) bodyRenderer.sprite = Resources.Load<Sprite>("player_male_base");

            // 3. Quần (Pants Layer)
            if (pantsRenderer == null)
            {
                var pObj = new GameObject("Layer_Pants");
                pObj.transform.parent = visualRoot;
                pObj.transform.localPosition = new Vector3(0, -0.15f, 0);
                pantsRenderer = pObj.AddComponent<SpriteRenderer>();
                pantsRenderer.sortingOrder = 11;
                pantsRenderer.sprite = Resources.Load<Sprite>("item_pants");
                pantsRenderer.color = new Color(0.85f, 0.85f, 0.95f, 0.9f);
                pObj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            }

            // 4. Giáp / Áo (Armor Layer)
            if (armorRenderer == null)
            {
                var aObj = new GameObject("Layer_Armor");
                aObj.transform.parent = visualRoot;
                aObj.transform.localPosition = new Vector3(0, -0.02f, 0);
                armorRenderer = aObj.AddComponent<SpriteRenderer>();
                armorRenderer.sortingOrder = 12;
                armorRenderer.sprite = Resources.Load<Sprite>("item_armor");
                aObj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            // 5. Mũ / Nón (Helmet Layer)
            if (helmetRenderer == null)
            {
                var hObj = new GameObject("Layer_Helmet");
                hObj.transform.parent = visualRoot;
                hObj.transform.localPosition = new Vector3(0, 0.32f, 0);
                helmetRenderer = hObj.AddComponent<SpriteRenderer>();
                helmetRenderer.sortingOrder = 13;
                helmetRenderer.sprite = Resources.Load<Sprite>("item_helmet");
                hObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            }

            // 6. Vũ Khí (Weapon Layer)
            if (weaponRenderer == null)
            {
                var wObj = new GameObject("Layer_Weapon");
                wObj.transform.parent = visualRoot;
                wObj.transform.localPosition = new Vector3(0.32f, -0.05f, 0);
                weaponRenderer = wObj.AddComponent<SpriteRenderer>();
                weaponRenderer.sortingOrder = 14;
                weaponRenderer.sprite = Resources.Load<Sprite>("item_weapon");
                wObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }
            initialWeaponPos = weaponRenderer.transform.localPosition;
        }

        private void Update()
        {
            UpdateDirectionFromAim();
            UpdateAnimations();
            SyncEquippedVisuals();
        }

        private void UpdateDirectionFromAim()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
            Vector2 aimDir = (mouseWorld - transform.position).normalized;

            CurrentDirection = Direction8Utils.VectorToDirection8(aimDir);
            bool isFacingLeft = Direction8Utils.IsFacingLeft(CurrentDirection);

            // Xoay toàn bộ visualRoot hoặc lật đối xứng Trái - Phải
            Vector3 rootScale = visualRoot.localScale;
            rootScale.x = isFacingLeft ? -Mathf.Abs(rootScale.x) : Mathf.Abs(rootScale.x);
            visualRoot.localScale = rootScale;

            // Điều chỉnh vị trí vũ khí theo hướng ngắm
            if (weaponRenderer != null)
            {
                float weaponAngle = Mathf.Atan2(aimDir.y, Mathf.Abs(aimDir.x)) * Mathf.Rad2Deg;
                weaponRenderer.transform.localRotation = Quaternion.Euler(0, 0, weaponAngle * (isFacingLeft ? -1f : 1f) * 0.4f);

                // Khi quay lên Bắc thì vũ khí và giáp có chiều sâu thích hợp
                if (CurrentDirection == Direction8.North || CurrentDirection == Direction8.NorthEast || CurrentDirection == Direction8.NorthWest)
                {
                    weaponRenderer.sortingOrder = 9; // Ra sau lưng
                }
                else
                {
                    weaponRenderer.sortingOrder = 14; // Ra trước mặt
                }
            }
        }

        private void UpdateAnimations()
        {
            bool isDashing = playerMovement != null && playerMovement.IsDashing;
            bool isMoving = (rb != null && rb.linearVelocity.sqrMagnitude > 0.05f) || (playerMovement != null && isDashing);

            // ================= TRẠNG THÁI DASH (LƯỚT) =================
            if (isDashing)
            {
                visualRoot.localScale = new Vector3(Mathf.Sign(visualRoot.localScale.x) * 1.15f, 0.88f, 1f);
                ghostSpawnTimer += Time.deltaTime;
                if (ghostSpawnTimer > 0.04f)
                {
                    ghostSpawnTimer = 0f;
                    SpawnGhostAfterimage();
                }
                return;
            }

            // ================= TRẠNG THÁI ĐI BỘ (WALK) =================
            if (isMoving)
            {
                walkCycleTimer += Time.deltaTime * 14f;
                float bob = Mathf.Abs(Mathf.Sin(walkCycleTimer)) * 0.07f;
                float tilt = Mathf.Sin(walkCycleTimer) * 3.5f;

                visualRoot.localPosition = initialVisualPos + new Vector3(0, bob, 0);
                visualRoot.localRotation = Quaternion.Euler(0, 0, tilt);

                if (weaponRenderer != null)
                {
                    weaponRenderer.transform.localPosition = initialWeaponPos + new Vector3(0, -bob * 0.8f, 0);
                }
            }
            // ================= TRẠNG THÁI NGHỈ (IDLE) =================
            else
            {
                walkCycleTimer = 0f;
                visualRoot.localRotation = Quaternion.identity;

                // Nhịp thở nhấp nhô nhẹ nhàng
                float breath = Mathf.Sin(Time.time * 2.8f);
                float squash = breath * 0.02f;
                float stretch = breath * 0.035f;

                float signX = Mathf.Sign(visualRoot.localScale.x);
                visualRoot.localScale = new Vector3(signX * (1f - squash), 1f + stretch, 1f);
                visualRoot.localPosition = initialVisualPos;

                if (weaponRenderer != null)
                {
                    weaponRenderer.transform.localPosition = initialWeaponPos + new Vector3(0, breath * 0.025f, 0);
                }
            }
        }

        private void SpawnGhostAfterimage()
        {
            GameObject ghostObj = new GameObject("GhostAfterimage");
            ghostObj.transform.position = visualRoot.position;
            ghostObj.transform.rotation = visualRoot.rotation;
            ghostObj.transform.localScale = visualRoot.lossyScale;

            var sr = ghostObj.AddComponent<SpriteRenderer>();
            sr.sprite = bodyRenderer.sprite;
            sr.sortingOrder = 8;
            sr.color = new Color(0.1f, 0.9f, 1f, 0.55f);

            StartCoroutine(FadeGhost(ghostObj, sr));
        }

        private IEnumerator FadeGhost(GameObject obj, SpriteRenderer sr)
        {
            float elapsed = 0f;
            float dur = 0.28f;
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

        private void SyncEquippedVisuals()
        {
            if (inventoryManager == null) return;

            // Kiểm tra vũ khí đang mặc
            var wep = inventoryManager.GetEquippedGear(EquipSlot.Weapon);
            if (weaponRenderer != null)
            {
                weaponRenderer.enabled = (wep != null);
                if (wep != null && !string.IsNullOrEmpty(wep.iconName))
                {
                    Sprite s = Resources.Load<Sprite>(wep.iconName);
                    if (s != null) weaponRenderer.sprite = s;
                }
            }

            // Kiểm tra áo giáp đang mặc
            var armor = inventoryManager.GetEquippedGear(EquipSlot.Armor);
            if (armorRenderer != null)
            {
                armorRenderer.enabled = (armor != null);
                if (armor != null && !string.IsNullOrEmpty(armor.iconName))
                {
                    Sprite s = Resources.Load<Sprite>(armor.iconName);
                    if (s != null) armorRenderer.sprite = s;
                }
            }

            // Kiểm tra mũ đang mặc
            var helm = inventoryManager.GetEquippedGear(EquipSlot.Helmet);
            if (helmetRenderer != null)
            {
                helmetRenderer.enabled = (helm != null);
                if (helm != null && !string.IsNullOrEmpty(helm.iconName))
                {
                    Sprite s = Resources.Load<Sprite>(helm.iconName);
                    if (s != null) helmetRenderer.sprite = s;
                }
            }

            // Kiểm tra quần đang mặc
            var pants = inventoryManager.GetEquippedGear(EquipSlot.Pants);
            if (pantsRenderer != null)
            {
                pantsRenderer.enabled = (pants != null);
                if (pants != null && !string.IsNullOrEmpty(pants.iconName))
                {
                    Sprite s = Resources.Load<Sprite>(pants.iconName);
                    if (s != null) pantsRenderer.sprite = s;
                }
            }
        }
    }
}
