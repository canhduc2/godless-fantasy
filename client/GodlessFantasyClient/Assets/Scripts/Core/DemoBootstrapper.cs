using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using GodlessFantasy.Indicator;
using GodlessFantasy.UI;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Khởi tạo toàn bộ thế giới Alpha Slice HD Pixel Art:
    /// - Bản đồ rộng 44x24 tiles chia rõ Làng Tân Thủ (Safe Zone) và Rừng Rậm (Wilderness).
    /// - Nhân vật chính (Player Hero) với PlayerStats (Cấp 1, Hệ Hỏa, Thuộc tính INT/STR/VIT/AGI).
    /// - Thú Cưng Sóc Chibi (Pet SquiRRels Studio) chạy theo sau.
    /// - Bù nhìn luyện công có thanh máu trên đầu và tự hồi sinh sau 3s.
    /// - Mũi tên ngắm xuất phát từ ngực nhân vật, lớp hiển thị (sortingOrder) chuẩn chỉnh.
    /// - Đạn ma thuật có TrailRenderer phát sáng bay lướt trên không.
    /// - Giao diện HUD chuẩn wireframe & phong cách viền vàng kim đồng nhất.
    /// </summary>
    public class DemoBootstrapper : MonoBehaviour
    {
        private static bool isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeDemo()
        {
            if (isInitialized) return;

            if (FindFirstObjectByType<PlayerMovement>() != null)
            {
                isInitialized = true;
                return;
            }

            GameObject bootstrapperObj = new GameObject("[DEMO_BOOTSTRAPPER]");
            bootstrapperObj.AddComponent<DemoBootstrapper>();
            isInitialized = true;
        }

        private void Start()
        {
            BuildDemoWorld();
        }

        private void BuildDemoWorld()
        {
            Debug.Log("<color=green>[SquiRRels Studio] Đang khởi tạo thế giới MMORPG Godless Fantasy...</color>");

            // 1. Tải toàn bộ Sprite từ Resources
            Sprite playerSprite = Resources.Load<Sprite>("player_male_base");
            Sprite dummySprite = Resources.Load<Sprite>("dummy");
            Sprite boltSprite = Resources.Load<Sprite>("magic_bolt");
            Sprite arrowSprite = Resources.Load<Sprite>("indicator_arrow");
            Sprite stoneTile = Resources.Load<Sprite>("tile_stone");
            Sprite safeTile = Resources.Load<Sprite>("tile_safezone");
            Sprite grassTile = Resources.Load<Sprite>("tile_grass");
            Sprite petSprite = Resources.Load<Sprite>("pet_squirrel");

            // 2. Xây Dựng Bản Đồ Thế Giới Rộng Lớn (44 x 24 ô)
            // - Phía Tây (x: -22 đến -5): Làng Tân Thủ An Toàn (Sanctuary Safe Zone).
            // - Ở Giữa (x: -4 đến -2): Cổng Làng & Ranh Giới (Boundary Gate).
            // - Phía Đông (x: -1 đến 22): Rừng Rậm & Bãi Quái (Wilderness Danger Zone).
            Transform floorParent = new GameObject("Environment_Floor").transform;

            for (int x = -22; x <= 22; x++)
            {
                for (int y = -12; y <= 12; y++)
                {
                    GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                    tileObj.transform.parent = floorParent;
                    tileObj.transform.position = new Vector3(x, y, 0);

                    var sr = tileObj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = -10; // Nền luôn nằm sâu nhất (-10)

                    if (x <= -6)
                    {
                        // Làng Tân Thủ: Nền đá thánh viền vàng
                        sr.sprite = (safeTile != null) ? safeTile : stoneTile;
                    }
                    else if (x >= 2)
                    {
                        // Rừng rậm bãi quái: Xen kẽ đá cổ và cỏ xanh rêu
                        sr.sprite = ((x + y) % 3 == 0 && grassTile != null) ? grassTile : stoneTile;
                    }
                    else
                    {
                        // Con đường đá trung tâm nối làng với rừng
                        sr.sprite = stoneTile;
                    }
                }
            }

            // 3. Vùng An Toàn (Safe Zone Trigger bao trọn Làng Tân Thủ)
            GameObject safeZoneObj = new GameObject("SafeZone_Village");
            safeZoneObj.transform.position = new Vector3(-14.0f, 0, 0);
            var safeCol = safeZoneObj.AddComponent<BoxCollider2D>();
            safeCol.isTrigger = true;
            safeCol.size = new Vector2(17.0f, 25.0f);
            safeZoneObj.AddComponent<SafeZone>();

            // 4. Prefab Đạn Ma Thuật (MagicBolt Template - Có TrailRenderer rực rỡ)
            GameObject boltTemplate = new GameObject("MagicBolt_Template");
            boltTemplate.transform.position = new Vector3(999, 999, 0);

            var boltSr = boltTemplate.AddComponent<SpriteRenderer>();
            boltSr.sprite = boltSprite;
            boltSr.sortingOrder = 20; // Đạn bay ở layer 20 (NẰM TRÊN NHÂN VẬT VÀ CHỈ BÁO)

            var boltCol = boltTemplate.AddComponent<CircleCollider2D>();
            boltCol.isTrigger = true;
            boltCol.radius = 0.25f;

            var boltRb = boltTemplate.AddComponent<Rigidbody2D>();
            boltRb.gravityScale = 0;

            // Thêm hiệu ứng vệt năng lượng TrailRenderer phát sáng
            var trail = boltTemplate.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.35f;
            trail.endWidth = 0.02f;
            trail.sortingOrder = 20;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.1f, 0.9f, 1f), 0.0f), new GradientColorKey(new Color(0f, 0.4f, 1f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trail.colorGradient = gradient;

            boltTemplate.AddComponent<MagicBolt>();
            boltTemplate.SetActive(false); // Giữ template ở trạng thái tắt, không bị Destroy sớm

            // 5. Nhân Vật Chính (Player Hero)
            GameObject playerObj = new GameObject("Player_Hero");
            playerObj.transform.position = new Vector3(-4.0f, 0, 0); // Đứng ở gần cổng làng

            var pSr = playerObj.AddComponent<SpriteRenderer>();
            pSr.sprite = playerSprite;
            pSr.sortingOrder = 10; // Nhân vật ở layer 10 (NẰM TRÊN MŨI TÊN CHỈ BÁO LAYER 0)

            var pRb = playerObj.AddComponent<Rigidbody2D>();
            pRb.gravityScale = 0;
            pRb.freezeRotation = true;
            pRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var pCol = playerObj.AddComponent<CircleCollider2D>();
            pCol.radius = 0.4f;

            var pMovement = playerObj.AddComponent<PlayerMovement>();
            var pHealth = playerObj.AddComponent<Health>();
            var pStats = playerObj.AddComponent<PlayerStats>();

            // Tạo Indicator Mũi Tên Ngắm Chuẩn:
            // - Gốc đuôi mũi tên bắt đầu ngay trước ngực nhân vật (+0.35f offset).
            // - SortingOrder = 0 (NẰM DƯỚI NHÂN VẬT LAYER 10, ĐÚNG TƯ DUY CĂN BẢN!)
            GameObject indicatorObj = new GameObject("Indicator_Directional");
            indicatorObj.transform.parent = playerObj.transform;
            indicatorObj.transform.localPosition = Vector3.zero;

            GameObject arrowVisual = new GameObject("Arrow_Visual");
            arrowVisual.transform.parent = indicatorObj.transform;
            arrowVisual.transform.localPosition = new Vector3(0.35f, 0, 0); // Bắt đầu từ trước ngực, không xuyên qua người!

            var indSr = arrowVisual.AddComponent<SpriteRenderer>();
            indSr.sprite = arrowSprite;
            indSr.sortingOrder = 0; // LAYER 0: NẰM DƯỚI NHÂN VẬT (10) VÀ DƯỚI HIỆU ỨNG CHIÊU (20)

            var indRenderer = indicatorObj.AddComponent<IndicatorRenderer>();
            SetPrivateField(indRenderer, "indicatorVisual", indicatorObj.transform);
            SetPrivateField(indRenderer, "spriteRenderer", indSr);

            // Gán bộ điều khiển ngắm Two-Tap
            var pCombat = playerObj.AddComponent<SkillAimingController>();
            SetPrivateField(pCombat, "magicBoltPrefab", boltTemplate);
            SetPrivateField(pCombat, "indicatorRenderer", indRenderer);
            SetPrivateField(pCombat, "firePoint", playerObj.transform);
            SetPrivateField(pCombat, "playerStats", pStats);

            // 6. Thú Cưng Mini (Cute Mascot Pet Sóc Chibi của SquiRRels Studio!)
            if (petSprite != null)
            {
                GameObject petObj = new GameObject("Pet_Squirrel_Companion");
                petObj.transform.position = playerObj.transform.position + new Vector3(-1.0f, -0.3f, 0);

                var petSr = petObj.AddComponent<SpriteRenderer>();
                petSr.sprite = petSprite;
                petSr.sortingOrder = 8; // Layer 8 (ngay dưới chân nhân vật 10)

                var petFollower = petObj.AddComponent<PetFollower>();
                petFollower.SetOwner(playerObj.transform);
            }

            // 7. Cấu Hình Camera Centered Parallax Tracking
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 3.6f;
                mainCam.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);

                if (!mainCam.TryGetComponent<CameraController>(out var camCtrl))
                {
                    camCtrl = mainCam.gameObject.AddComponent<CameraController>();
                }
                camCtrl.SetTarget(playerObj.transform);
            }

            // 8. Tạo Bù Nhìn Luyện Công (Monster Targets) Có Thanh Máu Trên Đầu & Tự Hồi Sinh
            // - Quái 1: Trong bãi quái PvP (x = 5.0, y = 0)
            CreateMonsterTarget("Dummy_PvP_Champion", new Vector3(5.0f, 0, 0), dummySprite, false);
            // - Quái 2: Trong bãi quái PvP (x = 9.0, y = 3.5)
            CreateMonsterTarget("Dummy_PvP_Warrior", new Vector3(9.0f, 3.5f, 0), dummySprite, false);
            // - Quái 3: Trong Vùng An Toàn Làng Tân Thủ (Miễn nhiễm sát thương)
            CreateMonsterTarget("Dummy_Village_SafeGuard", new Vector3(-12.0f, 2.0f, 0), dummySprite, true);

            // 9. Khởi tạo Floating Damage Manager & Combat HUD Wireframe
            if (FindFirstObjectByType<FloatingDamageManager>() == null)
            {
                new GameObject("[FLOATING_DAMAGE_MANAGER]").AddComponent<FloatingDamageManager>();
            }

            GameObject hudObj = new GameObject("Combat_HUD");
            hudObj.AddComponent<CombatHUD>();

            Debug.Log("<color=green>[SquiRRels Studio] Thế giới Alpha Slice đã hoàn thiện 100%: Bản đồ rộng, Camera Parallax, Pet theo sau, Skill Layering chuẩn!</color>");
        }

        private void CreateMonsterTarget(string name, Vector3 position, Sprite sprite, bool isSafe)
        {
            GameObject dummyObj = new GameObject(name);
            dummyObj.transform.position = position;

            var sr = dummyObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10; // Layer 10 cùng tầng với nhân vật

            var col = dummyObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.9f);

            var rb = dummyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var health = dummyObj.AddComponent<Health>();
            health.IsInSafeZone = isSafe;

            var monsterTarget = dummyObj.AddComponent<MonsterTarget>();
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
