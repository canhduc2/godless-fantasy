using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using GodlessFantasy.Indicator;
using GodlessFantasy.UI;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Tự động khởi tạo toàn bộ sàn đấu Vertical Slice khi bấm Play trong Unity:
    /// - Nhân vật chính (Player) áo ba lỗ quần cộc hoặc giáp
    /// - Tâm ngắm định hướng (Indicator)
    /// - Đạn ma thuật (MagicBolt)
    /// - Bù nhìn luyện võ (Target Dummy) trong Danger Zone và Safe Zone
    /// - Vùng An Toàn (Safe Zone)
    /// - Giao diện HUD chiến đấu
    /// </summary>
    public class DemoBootstrapper : MonoBehaviour
    {
        private static bool isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeDemo()
        {
            if (isInitialized) return;

            // Kiểm tra xem đã có Player chưa
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
            Debug.Log("<color=green>[SquiRRels Studio] Bắt đầu khởi tạo bản Demo Lõi Vertical Slice 5 Bước...</color>");

            // 1. Tải các Sprite từ Resources
            Sprite playerSprite = Resources.Load<Sprite>("player_male_base");
            Sprite dummySprite = Resources.Load<Sprite>("dummy");
            Sprite boltSprite = Resources.Load<Sprite>("magic_bolt");
            Sprite arrowSprite = Resources.Load<Sprite>("indicator_arrow");
            Sprite stoneTile = Resources.Load<Sprite>("tile_stone");
            Sprite safeTile = Resources.Load<Sprite>("tile_safezone");

            // 2. Tạo sàn đá (Floor Arena 11x7 tiles)
            Transform floorParent = new GameObject("Environment_Floor").transform;
            if (stoneTile != null)
            {
                for (int x = -5; x <= 5; x++)
                {
                    for (int y = -3; y <= 3; y++)
                    {
                        GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                        tileObj.transform.parent = floorParent;
                        tileObj.transform.position = new Vector3(x, y, 0);
                        var sr = tileObj.AddComponent<SpriteRenderer>();
                        sr.sprite = (x <= -3) && safeTile != null ? safeTile : stoneTile;
                        sr.sortingOrder = -10;
                    }
                }
            }

            // 3. Tạo Vùng An Toàn (Safe Zone ở góc bên trái x <= -2.5)
            GameObject safeZoneObj = new GameObject("SafeZone_Village");
            safeZoneObj.transform.position = new Vector3(-3.5f, 0, 0);
            var safeCol = safeZoneObj.AddComponent<BoxCollider2D>();
            safeCol.isTrigger = true;
            safeCol.size = new Vector2(3.5f, 7.0f);
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

            // 5. Tạo Nhân Vật Chính (Player)
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
            // Gán các trường private thông qua Reflection để đảm bảo hoạt động trơn tru
            SetPrivateField(pCombat, "magicBoltPrefab", boltTemplate);
            SetPrivateField(pCombat, "indicatorRenderer", indRenderer);
            SetPrivateField(pCombat, "firePoint", playerObj.transform);

            // 6. Tạo Bù Nhìn 1 (Trong Vùng PvP Ngoài Safe Zone để test đòn)
            CreateDummy("Dummy_Target_PvP", new Vector3(3.5f, 0, 0), dummySprite, false);

            // 7. Tạo Bù Nhìn 2 (Trong Vùng Safe Zone để test miễn nhiễm)
            CreateDummy("Dummy_Target_SafeZone", new Vector3(-3.5f, 1.5f, 0), dummySprite, true);

            // 8. Tạo Giao Diện HUD
            GameObject hudObj = new GameObject("Combat_HUD");
            hudObj.AddComponent<CombatHUD>();

            Debug.Log("<color=green>[SquiRRels Studio] Khởi tạo bản Demo Vertical Slice thành công 100%! Bấm chuột trái để chạy, phím 1 để ngắm, Shift để lướt!</color>");
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
