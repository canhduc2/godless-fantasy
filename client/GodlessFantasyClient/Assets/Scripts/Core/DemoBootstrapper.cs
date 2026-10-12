using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using GodlessFantasy.Indicator;
using GodlessFantasy.UI;
using GodlessFantasy.AI;

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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeDemo()
        {
            // Kiểm tra trực tiếp xem trong Scene đang mở đã có Player hoặc DemoBootstrapper chưa
            if (FindAnyObjectByType<PlayerMovement>() != null || GameObject.Find("[DEMO_BOOTSTRAPPER]") != null)
            {
                return;
            }

            GameObject bootstrapperObj = new GameObject("[DEMO_BOOTSTRAPPER]");
            bootstrapperObj.AddComponent<DemoBootstrapper>();
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

            // 2. Xây Dựng Bản Đồ Thế Giới Khối Địa Hình (Block-Based Terrain Engine) & Ánh Sáng Môi Trường
            gameObject.AddComponent<EnvironmentAtmosphereManager>();
            var mapMgr = gameObject.AddComponent<BlockMapManager>();
            mapMgr.InitializeMap(-45, 45, -22, 22);
            gameObject.AddComponent<TerrainPainterTool>();
            gameObject.AddComponent<ItemCreatorModal>();
            gameObject.AddComponent<GodlessFantasy.UI.SkillTestingMatrixModal>();
            gameObject.AddComponent<GodlessFantasy.UI.GameSettingsModal>();
            gameObject.AddComponent<GodlessFantasy.UI.WorldDistanceRuler>();
            gameObject.AddComponent<ItemDatabase>();
            gameObject.AddComponent<SkillDatabase>();
            gameObject.AddComponent<SkillVFXManager>();
            gameObject.AddComponent<GodlessFantasy.Guild.GuildManager>();
            GameSettings.Initialize();
            AudioManager.EnsureExists();

            // 3. Vùng An Toàn (Safe Zone Trigger bao trọn Làng Tân Thủ)
            GameObject safeZoneObj = new GameObject("SafeZone_Village");
            safeZoneObj.transform.position = new Vector3(-24.0f, 0, 0);
            var safeCol = safeZoneObj.AddComponent<BoxCollider2D>();
            safeCol.isTrigger = true;
            safeCol.size = new Vector2(34.0f, 42.0f);
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
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(-16.0f, 0, 0); // Đứng ở quảng trường Làng Tân Thủ

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
            var pInventory = playerObj.AddComponent<InventoryManager>();
            var pVisual = playerObj.AddComponent<CharacterVisualController8Dir>();
            var pBasicAttack = playerObj.AddComponent<BasicAttackController>();

            // THANH MÁU TRÊN ĐẦU NGƯỜI CHƠI (Overhead Status Bar: Danh Hiệu, Tên, Cấp, Hệ, Nguyên Tố, Máu, Mana)
            var pOverhead = playerObj.AddComponent<OverheadStatusBar>();
            pOverhead.Setup(OverheadType.Player, "Dũng Sĩ SquiRRels", "Lv.1 · Kị Sĩ · Hỏa", 1.05f);

            // Tạo Indicator Mũi Tên Ngắm Chuẩn:
            GameObject indicatorObj = new GameObject("Indicator_Directional");
            indicatorObj.transform.parent = playerObj.transform;
            indicatorObj.transform.localPosition = Vector3.zero;

            var indRenderer = indicatorObj.AddComponent<IndicatorRenderer>();
            if (arrowSprite != null) indRenderer.SetSprites(arrowSprite, Resources.Load<Sprite>("indicator_ground_circle"), Resources.Load<Sprite>("indicator_cone"));

            // Gán bộ điều khiển ngắm Two-Tap & Cuộn Trục Kỹ Năng Kéo Thả
            SetPrivateField(pMovement, "playerStats", pStats);
            var pCombat = playerObj.AddComponent<SkillAimingController>();
            SetPrivateField(pCombat, "magicBoltPrefab", boltTemplate);
            SetPrivateField(pCombat, "indicatorRenderer", indRenderer);
            SetPrivateField(pCombat, "firePoint", playerObj.transform);
            SetPrivateField(pCombat, "playerStats", pStats);
            pCombat.InitializeDefaultScrolls();

            // Đồng bộ túi đồ với bộ chiêu thức chiến đấu
            SetPrivateField(pInventory, "skillAimingController", pCombat);
            SetPrivateField(pInventory, "playerStats", pStats);
            pInventory.SyncEquippedScrollsToCombat();

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
                SetPrivateField(pInventory, "petFollower", petFollower);
            }

            // 7. Tạo NPC Trưởng Làng Tân Thủ (Có thanh tên/máu NPC và Hộp thoại nhiệm vụ)
            Sprite npcSprite = Resources.Load<Sprite>("npc_elder");
            if (npcSprite != null)
            {
                GameObject npcObj = new GameObject("NPC_Elder_Eldrin");
                npcObj.transform.position = new Vector3(-22.0f, 1.8f, 0);

                var nSr = npcObj.AddComponent<SpriteRenderer>();
                nSr.sprite = npcSprite;
                nSr.sortingOrder = 10;

                var nCol = npcObj.AddComponent<BoxCollider2D>();
                nCol.size = new Vector2(0.8f, 1.0f);

                var nHealth = npcObj.AddComponent<Health>();
                nHealth.IsInSafeZone = true;

                npcObj.AddComponent<VillageNPC>();

                var nOverhead = npcObj.AddComponent<OverheadStatusBar>();
                nOverhead.Setup(OverheadType.NPC, "Trưởng Làng Eldrin", "NPC 💬", 0.95f);
            }

            // 8. Cấu Hình Camera Centered Parallax Tracking
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

            // 9. Tạo Quái Vật & Bãi Luyện Công (Monster Targets & Mini-Boss Với AI Telegraph)
            // - Quái 1: Mini-Boss Thủ Lĩnh (Máu dày 450, vòng đỏ Telegraph 3.2m rộng lớn, sâu trong rừng qua bờ suối)
            CreateMonsterTarget("Boss_PvP_Champion", "Thủ Lĩnh Ma Thú (Mini-Boss)", "Lv.5 · Mini-Boss 🔥", new Vector3(28.0f, -4.0f, 0), dummySprite, false, hasAI: true, isBoss: true, maxHp: 450, attackRange: 3.2f, attackDamage: 35, scale: 1.4f);

            // - Quái 2: Quái Thường Tuần Tra (Tầm đánh gần 1.8m, tốc độ nhanh, tuần tra bìa rừng)
            CreateMonsterTarget("Monster_PvP_Warrior", "Chiến Binh Quái (Thường)", "Lv.1 · Cận Chiến", new Vector3(10.0f, 2.0f, 0), dummySprite, false, hasAI: true, isBoss: false, maxHp: 120, attackRange: 1.8f, attackDamage: 16, scale: 1.0f);

            // - Quái 3: Quái Thường Tuần Tra 2
            CreateMonsterTarget("Monster_PvP_Warrior_2", "Chiến Binh Quái (Tuần Tra)", "Lv.2 · Cận Chiến", new Vector3(14.0f, -6.0f, 0), dummySprite, false, hasAI: true, isBoss: false, maxHp: 140, attackRange: 1.8f, attackDamage: 18, scale: 1.0f);

            // - Quái 4: Bù Nhìn Gỗ trong Vùng An Toàn Làng Tân Thủ (Miễn nhiễm sát thương, đứng im không đánh để test combo)
            CreateMonsterTarget("Dummy_Village_SafeGuard", "Bù Nhìn Gỗ (Làng)", "SafeZone", new Vector3(-26.0f, 2.0f, 0), dummySprite, true, hasAI: false, isBoss: false, maxHp: 9999, scale: 1.0f);

            // 10. Cổng Dịch Chuyển Không Gian 2 Chiều (Map Portals: Làng Tân Thủ <-> Rừng Rậm Hắc Ám)
            CreateMapPortal("Portal_Village_To_Wilderness", "Cổng Đến Rừng Rậm Hắc Ám", "Rừng Rậm Hắc Ám",
                new Vector3(-7.5f, 3.5f, 0), new Vector3(8.0f, 0f, 0), BgmTrack.Combat, new Color(0.68f, 0.25f, 1.0f, 0.85f));

            CreateMapPortal("Portal_Wilderness_To_Village", "Cổng Về Làng Tân Thủ", "Làng Tân Thủ",
                new Vector3(8.0f, 3.5f, 0), new Vector3(-9.0f, 0f, 0), BgmTrack.Village, new Color(0.25f, 0.95f, 0.55f, 0.85f));

            // 11. Khởi tạo Floating Damage Manager & Combat HUD Wireframe
            if (FindAnyObjectByType<FloatingDamageManager>() == null)
            {
                new GameObject("[FLOATING_DAMAGE_MANAGER]").AddComponent<FloatingDamageManager>();
            }

            // 12. Khởi tạo Skill VFX Engine & Procedural Modular VFX Engine (140 Biến Thể Nguyên Tố)
            if (FindAnyObjectByType<SkillVFXManager>() == null)
            {
                new GameObject("[SKILL_VFX_MANAGER]").AddComponent<SkillVFXManager>();
            }
            if (FindAnyObjectByType<ProceduralVFXEngine>() == null)
            {
                new GameObject("[PROCEDURAL_VFX_ENGINE]").AddComponent<ProceduralVFXEngine>();
            }

            GameObject hudObj = new GameObject("Combat_HUD");
            hudObj.AddComponent<CombatHUD>();

            Debug.Log("<color=green>[SquiRRels Studio] Thế giới Alpha Slice đã hoàn thiện 100%: Quái AI Telegraph & Cổng Dịch Chuyển & Hệ Thống Procedural VFX 140 Biến Thể đã kích hoạt!</color>");
        }

        private void CreateMapPortal(string objName, string portalTitle, string destZoneName, Vector3 worldPos, Vector3 destCoord, BgmTrack bgm, Color glowColor)
        {
            GameObject portalObj = new GameObject(objName);
            portalObj.transform.position = worldPos;
            var portalCtrl = portalObj.AddComponent<MapPortalController>();
            portalCtrl.SetupPortal(portalTitle, destZoneName, destCoord, bgm, glowColor);
        }

        private void CreateMonsterTarget(string name, string displayName, string levelStr, Vector3 position, Sprite sprite, bool isSafe, bool hasAI = false, bool isBoss = false, int maxHp = 100, float attackRange = 1.8f, int attackDamage = 18, float scale = 1.0f)
        {
            GameObject dummyObj = new GameObject(name);
            dummyObj.transform.position = position;
            if (Mathf.Abs(scale - 1.0f) > 0.01f)
            {
                dummyObj.transform.localScale = new Vector3(scale, scale, 1f);
            }

            var sr = dummyObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10; // Layer 10 cùng tầng với nhân vật
            if (isBoss)
            {
                sr.color = new Color(1f, 0.82f, 0.82f, 1f); // Boss có ánh đỏ hung quyền uy
            }

            var col = dummyObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.9f);

            var rb = dummyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var health = dummyObj.AddComponent<Health>();
            health.SetMaxHealth(maxHp);
            health.IsInSafeZone = isSafe;

            dummyObj.AddComponent<MonsterTarget>();

            var overhead = dummyObj.AddComponent<OverheadStatusBar>();
            overhead.Setup(OverheadType.Monster, displayName, isSafe ? "SafeZone" : levelStr, isBoss ? 1.25f : 0.85f);

            if (hasAI && !isSafe)
            {
                var ai = dummyObj.AddComponent<MonsterAIController>();
                ai.spawnPoint = position;
                ai.isBoss = isBoss;
                ai.attackRange = attackRange;
                ai.attackDamage = attackDamage;
                if (isBoss)
                {
                    ai.wanderRadius = 3.5f;
                    ai.leashRadius = 14.0f;
                    ai.wanderSpeed = 1.1f;
                    ai.chaseSpeed = 1.9f;
                    ai.detectionRadius = 6.8f;
                    ai.attackCooldown = 4.0f;
                    ai.telegraphDuration = 1.5f;
                }
                else
                {
                    ai.wanderRadius = 4.0f;
                    ai.leashRadius = 10.0f;
                    ai.wanderSpeed = 1.4f;
                    ai.chaseSpeed = 2.3f;
                    ai.detectionRadius = 5.2f;
                    ai.attackCooldown = 3.2f;
                    ai.telegraphDuration = 1.2f;
                }
            }
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
