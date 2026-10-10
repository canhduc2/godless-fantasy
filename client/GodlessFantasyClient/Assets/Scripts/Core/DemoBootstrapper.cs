using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using GodlessFantasy.Indicator;
using GodlessFantasy.UI;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Tự động khởi tạo sàn đấu và bối cảnh chuẩn HD Pixel Art khi bấm Play trong Unity:
    /// - Setup Camera theo dõi nhân vật, màu nền xanh đen Dark Fantasy, Zoom mượt mà.
    /// - Tạo sàn đấu đá cổ đại (15x9 tiles) và Vùng An Toàn (Safe Zone).
    /// - Nhân vật chính (Player Hero) sắc nét.
    /// - Bù nhìn rơm thử đòn trong Danger Zone và Safe Zone.
    /// - Giao diện HUD đúng 100% bản vẽ tay wireframe của Studio.
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
            BuildDemoArena();
        }

        private void BuildDemoArena()
        {
            Debug.Log("<color=green>[SquiRRels Studio] Khởi tạo sàn đấu HD Pixel Art...</color>");

            // 1. Tải Sprite từ Resources
            Sprite playerSprite = Resources.Load<Sprite>("player_male_base");
            Sprite dummySprite = Resources.Load<Sprite>("dummy");
            Sprite boltSprite = Resources.Load<Sprite>("magic_bolt");
            Sprite arrowSprite = Resources.Load<Sprite>("indicator_arrow");
            Sprite stoneTile = Resources.Load<Sprite>("tile_stone");
            Sprite safeTile = Resources.Load<Sprite>("tile_safezone");

            // 2. Tạo sàn đá (Floor Arena 15x9 tiles)
            Transform floorParent = new GameObject("Environment_Floor").transform;
            if (stoneTile != null)
            {
                for (int x = -7; x <= 7; x++)
                {
                    for (int y = -4; y <= 4; y++)
                    {
                        GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                        tileObj.transform.parent = floorParent;
                        tileObj.transform.position = new Vector3(x, y, 0);
                        var sr = tileObj.AddComponent<SpriteRenderer>();
                        sr.sprite = (x <= -4 && safeTile != null) ? safeTile : stoneTile;
                        sr.sortingOrder = -10;
                    }
                }
            }

            // 3. Tạo Vùng An Toàn (Safe Zone bên trái)
            GameObject safeZoneObj = new GameObject("SafeZone_Village");
            safeZoneObj.transform.position = new Vector3(-5.5f, 0, 0);
            var safeCol = safeZoneObj.AddComponent<BoxCollider2D>();
            safeCol.isTrigger = true;
            safeCol.size = new Vector2(4.5f, 9.0f);
            safeZoneObj.AddComponent<SafeZone>();

            // 4. Tạo Prefab Đạn Ma Thuật (MagicBolt)
            GameObject boltTemplate = new GameObject("MagicBolt_Template");
            boltTemplate.transform.position = new Vector3(999, 999, 0);
            var boltSr = boltTemplate.AddComponent<SpriteRenderer>();
            boltSr.sprite = boltSprite;
            boltSr.sortingOrder = 5;
            var boltCol = boltTemplate.AddComponent<CircleCollider2D>();
            boltCol.isTrigger = true;
            boltCol.radius = 0.25f;
            var boltRb = boltTemplate.AddComponent<Rigidbody2D>();
            boltRb.gravityScale = 0;
            boltTemplate.AddComponent<MagicBolt>();

            // 5. Tạo Nhân Vật Chính (Player Hero)
            GameObject playerObj = new GameObject("Player_Hero");
            playerObj.transform.position = Vector3.zero;
            var pSr = playerObj.AddComponent<SpriteRenderer>();
            pSr.sprite = playerSprite;
            pSr.sortingOrder = 2;

            var pRb = playerObj.AddComponent<Rigidbody2D>();
            pRb.gravityScale = 0;
            pRb.freezeRotation = true;
            pRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var pCol = playerObj.AddComponent<CircleCollider2D>();
            pCol.radius = 0.4f;

            var pMovement = playerObj.AddComponent<PlayerMovement>();
            var pHealth = playerObj.AddComponent<Health>();

            // Tạo Indicator mũi tên bám chuột
            GameObject indicatorObj = new GameObject("Indicator_Directional");
            indicatorObj.transform.parent = playerObj.transform;
            indicatorObj.transform.localPosition = Vector3.zero;
            var indSr = indicatorObj.AddComponent<SpriteRenderer>();
            indSr.sprite = arrowSprite;
            indSr.sortingOrder = 4;
            var indRenderer = indicatorObj.AddComponent<IndicatorRenderer>();

            // Thêm bộ ngắm Two-Tap
            var pCombat = playerObj.AddComponent<SkillAimingController>();
            SetPrivateField(pCombat, "magicBoltPrefab", boltTemplate);
            SetPrivateField(pCombat, "indicatorRenderer", indRenderer);
            SetPrivateField(pCombat, "firePoint", playerObj.transform);

            // 6. Cấu hình Camera chính bám theo Player và hỗ trợ Zoom mượt mà
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 3.6f;
                mainCam.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f); // Nền xanh đen Dark Fantasy huyền bí

                if (!mainCam.TryGetComponent<CameraController>(out var camCtrl))
                {
                    camCtrl = mainCam.gameObject.AddComponent<CameraController>();
                }
                camCtrl.SetTarget(playerObj.transform);
            }

            // 7. Tạo Bù Nhìn 1 (Trong Danger Zone để test Friendly Fire / Sát thương thật)
            CreateDummy("Dummy_PvP_Target", new Vector3(4.0f, 0, 0), dummySprite, false);

            // 8. Tạo Bù Nhìn 2 (Trong Safe Zone để test Miễn Nhiễm Sát Thương)
            CreateDummy("Dummy_SafeZone_Target", new Vector3(-5.5f, 1.5f, 0), dummySprite, true);

            // 9. Tạo HUD hiển thị đúng bản vẽ Wireframe
            GameObject hudObj = new GameObject("Combat_HUD");
            hudObj.AddComponent<CombatHUD>();

            Debug.Log("<color=green>[SquiRRels Studio] Đã nạp thành công sàn đấu HD Pixel Art và Giao diện Wireframe chuẩn!</color>");
        }

        private void CreateDummy(string name, Vector3 position, Sprite sprite, bool isSafe)
        {
            GameObject dummyObj = new GameObject(name);
            dummyObj.transform.position = position;
            var sr = dummyObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;

            var col = dummyObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.9f);

            var rb = dummyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var health = dummyObj.AddComponent<Health>();
            health.IsInSafeZone = isSafe;
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
