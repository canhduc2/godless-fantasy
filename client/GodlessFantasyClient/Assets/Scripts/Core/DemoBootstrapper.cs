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
            // TỐI ƯU HIỆU NĂNG CỰC ĐẠI (300+ FPS): Tắt VSync, mở khóa Frame Rate và tối ưu Physics 2D
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            GamePhysicsOptimizer.Initialize();

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

            // 2. Xây Dựng Bản Đồ Thế Giới Khối Địa Hình & Chunk Streaming Siêu Rộng x3 (270m x 132m)
            gameObject.AddComponent<EnvironmentAtmosphereManager>();
            var mapMgr = gameObject.AddComponent<BlockMapManager>();
            mapMgr.InitializeMap(-135, 135, -66, 66);
            gameObject.AddComponent<GodlessFantasy.AI.MonsterAIManager>();
            gameObject.AddComponent<TerrainPainterTool>();
            gameObject.AddComponent<ItemCreatorModal>();
            gameObject.AddComponent<GodlessFantasy.UI.SkillTestingMatrixModal>();
            gameObject.AddComponent<GodlessFantasy.UI.GameSettingsModal>();
            gameObject.AddComponent<GodlessFantasy.UI.WorldDistanceRuler>();
            gameObject.AddComponent<GodlessFantasy.Player.RebirthVaultManager>();
            gameObject.AddComponent<GodlessFantasy.UI.VillageShopNPCModal>();
            gameObject.AddComponent<GodlessFantasy.UI.DeathScreenModal>();
            gameObject.AddComponent<GodlessFantasy.Quest.QuestManager>();
            gameObject.AddComponent<ItemDatabase>();
            gameObject.AddComponent<SkillDatabase>();
            gameObject.AddComponent<SkillVFXManager>();
            gameObject.AddComponent<GodlessFantasy.Guild.GuildManager>();
            gameObject.AddComponent<GodlessFantasy.Network.GameNetworkClient>();
            gameObject.AddComponent<GodlessFantasy.Network.NetworkPlayerManager>();
            gameObject.AddComponent<GodlessFantasy.UI.AuthUniverseModal>();
            gameObject.AddComponent<GodlessFantasy.UI.CharacterCreationModal>();
            GameSettings.Initialize();
            AudioManager.EnsureExists();

            // 3. Vùng An Toàn (Safe Zone Trigger bao trọn Đại Đô Thị Làng Tân Thủ rộng 120m)
            GameObject safeZoneObj = new GameObject("SafeZone_Village");
            safeZoneObj.transform.position = new Vector3(-80.0f, 0, 0);
            var safeCol = safeZoneObj.AddComponent<BoxCollider2D>();
            safeCol.isTrigger = true;
            safeCol.size = new Vector2(120.0f, 140.0f);
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
            playerObj.transform.position = new Vector3(-70.0f, 0, 0); // Đứng ở quảng trường Đại Đô Thị Làng Tân Thủ
            GamePhysicsOptimizer.SetLayer(playerObj, GamePhysicsOptimizer.LayerPlayer);

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

            // 7. Tạo Dàn NPC Làng Tân Thủ Hoàn Chỉnh (Trưởng Làng, Shop, Thợ Rèn, Dược Sư, Sứ Giả RP)
            Sprite npcElderSprite = Resources.Load<Sprite>("npc_elder") ?? dummySprite;
            Sprite npcMerchantSprite = Resources.Load<Sprite>("npc_merchant") ?? npcElderSprite;
            Sprite npcBlacksmithSprite = Resources.Load<Sprite>("npc_blacksmith") ?? npcElderSprite;
            Sprite npcAlchemistSprite = Resources.Load<Sprite>("npc_alchemist") ?? npcElderSprite;
            Sprite npcRebirthSprite = Resources.Load<Sprite>("npc_rebirth") ?? npcElderSprite;

            CreateVillageNPC("NPC_Elder_Eldrin", "Trưởng Làng Eldrin", "NPC 💬", new Vector3(-72.0f, 2.0f, 0), npcElderSprite, false, GodlessFantasy.UI.VillageNPCType.Merchant,
                "Chào mừng dũng sĩ đến với Đại Đô Thị Làng Tân Thủ! Hãy ghé thăm các hàng quán chuẩn bị hành trang trước khi ra trận.");

            CreateVillageNPC("NPC_Merchant_Barnaby", "Thương Nhân Barnaby", "Shop 🛒", new Vector3(-65.0f, 6.0f, 0), npcMerchantSprite, true, GodlessFantasy.UI.VillageNPCType.Merchant,
                "Cửa hàng bán đầy đủ bình máu HP, bình mana MP và cuộn chiêu sơ cấp!");

            CreateVillageNPC("NPC_Blacksmith_Vulkan", "Thợ Rèn Vulkan", "Thợ Rèn 🔨", new Vector3(-65.0f, -6.0f, 0), npcBlacksmithSprite, true, GodlessFantasy.UI.VillageNPCType.Blacksmith,
                "Ta có thể cường hóa đập đồ trang bị của ngươi lên +1 đến +10!");

            CreateVillageNPC("NPC_Alchemist_Morgana", "Phù Thủy Morgana", "Dược Sư 🔮", new Vector3(-55.0f, 6.0f, 0), npcAlchemistSprite, true, GodlessFantasy.UI.VillageNPCType.Alchemist,
                "Thuốc lắc cuồng nộ và nước thánh giải nguyền của ta sẽ tăng cường sức mạnh chiến đấu của ngươi!");

            CreateVillageNPC("NPC_Rebirth_Kaelen", "Sứ Giả Kaelen", "Chuyển Sinh 🏛️", new Vector3(-92.0f, 15.0f, 0), npcRebirthSprite, true, GodlessFantasy.UI.VillageNPCType.Rebirth,
                "Đổi điểm RP lấy bí thuật cổ đại, hoặc mở Rương Chuyển Sinh lưu giữ 2 món bảo vật!");

            // 8. Cấu Hình Camera Centered Parallax Tracking
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                float aspect = (mainCam.aspect > 0.1f) ? mainCam.aspect : (16f / 9f);
                mainCam.orthographicSize = 20.0f / aspect; // Tầm nhìn mặc định ~20m mỗi bên (tổng chiều ngang ~40m)
                mainCam.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);

                if (!mainCam.TryGetComponent<CameraController>(out var camCtrl))
                {
                    camCtrl = mainCam.gameObject.AddComponent<CameraController>();
                }
                camCtrl.SetTarget(playerObj.transform);
            }

            // 9. Tạo Quái Vật & Bãi Luyện Công (Monster Targets & Mini-Boss Trên Thế Giới Siêu Rộng)
            Sprite slimeGrassSprite = Resources.Load<Sprite>("monster_slime_grass") ?? dummySprite;
            Sprite slimeWaterSprite = Resources.Load<Sprite>("monster_slime_water") ?? dummySprite;
            Sprite wolfSprite = Resources.Load<Sprite>("monster_forest_wolf") ?? dummySprite;
            Sprite lichBossSprite = Resources.Load<Sprite>("monster_lich_boss") ?? dummySprite;

            // - Bù Nhìn Gỗ trong Vùng An Toàn Làng Tân Thủ tại bãi võ
            CreateMonsterTarget("Dummy_Village_SafeGuard", "Bù Nhìn Gỗ (Làng)", "SafeZone", new Vector3(-76.0f, -12.0f, 0), dummySprite, true, hasAI: false, isBoss: false, maxHp: 9999, scale: 1.0f);

            // - Quái 1: Slime Thảo Nguyên Bìa Rừng
            CreateMonsterTarget("Monster_PvP_Warrior_1", "Slime Thảo Nguyên", "Lv.1 · Ma Vật", new Vector3(18.0f, 4.0f, 0), slimeGrassSprite, false, hasAI: true, isBoss: false, maxHp: 120, attackRange: 1.8f, attackDamage: 16, scale: 1.0f);

            // - Quái 2: Slime Nước Tuần Tra Ven Sông
            CreateMonsterTarget("Monster_PvP_Warrior_2", "Slime Thủy Tinh (Ven Sông)", "Lv.2 · Hệ Thủy", new Vector3(35.0f, -8.0f, 0), slimeWaterSprite, false, hasAI: true, isBoss: false, maxHp: 140, attackRange: 1.8f, attackDamage: 18, scale: 1.0f);

            // - Quái 3: Dã Lang Rừng Sâu
            CreateMonsterTarget("Monster_PvP_Warrior_3", "Dã Lang Rừng Sâu", "Lv.3 · Dã Thú 🐺", new Vector3(65.0f, 8.0f, 0), wolfSprite, false, hasAI: true, isBoss: false, maxHp: 180, attackRange: 2.0f, attackDamage: 22, scale: 1.1f);

            // - Quái 4: Dã Lang Hắc Ám Gần Tàn Tích
            CreateMonsterTarget("Monster_PvP_Warrior_4", "Dã Lang Hắc Ám (Tàn Tích)", "Lv.4 · Tinh Anh 🐺", new Vector3(85.0f, -12.0f, 0), wolfSprite, false, hasAI: true, isBoss: false, maxHp: 240, attackRange: 2.2f, attackDamage: 26, scale: 1.25f);

            // - Quái 5: Mini-Boss Thủ Lĩnh Ma Thú (Tại Đấu Trường Tàn Tích 110, -10)
            CreateMonsterTarget("Boss_PvP_Champion", "Thủ Lĩnh Ma Thú (Mini-Boss)", "Lv.5 · Mini-Boss 🔥", new Vector3(110.0f, -10.0f, 0), lichBossSprite, false, hasAI: true, isBoss: true, maxHp: 650, attackRange: 3.4f, attackDamage: 40, scale: 1.4f);

            // 10. Cổng Dịch Chuyển Không Gian 2 Chiều (Map Portals: Làng Tân Thủ <-> Rừng Rậm Hắc Ám)
            CreateMapPortal("Portal_Village_To_Wilderness", "Cổng Đến Rừng Rậm Hắc Ám", "Rừng Rậm Hắc Ám",
                new Vector3(-18.0f, 3.5f, 0), new Vector3(15.0f, 0f, 0), BgmTrack.Combat, new Color(0.68f, 0.25f, 1.0f, 0.85f));

            CreateMapPortal("Portal_Wilderness_To_Village", "Cổng Về Làng Tân Thủ", "Làng Tân Thủ",
                new Vector3(15.0f, 3.5f, 0), new Vector3(-22.0f, 0f, 0), BgmTrack.Village, new Color(0.25f, 0.95f, 0.55f, 0.85f));

            // Cổng Vào Phó Bản Hầm Ngục Di Tích (Dungeon Crypt Floor 1)
            CreateMapPortal("Portal_To_Dungeon_Crypt", "Cổng Phó Bản Hầm Ngục Di Tích", "Hầm Ngục Di Tích (Tầng 1)",
                new Vector3(120.0f, 25.0f, 0), new Vector3(0.0f, -48.0f, 0), BgmTrack.Combat, new Color(0.85f, 0.2f, 1.0f, 0.9f));

            CreateMapPortal("Portal_Dungeon_To_Surface", "Cổng Lên Mặt Đất", "Rừng Rậm Hắc Ám",
                new Vector3(0.0f, -50.0f, 0), new Vector3(118.0f, 22.0f, 0), BgmTrack.Combat, new Color(0.2f, 0.85f, 1.0f, 0.9f));

            // Quái Vật Trong Phó Bản Hầm Ngục Di Tích (Crypt Floor 1)
            CreateMonsterTarget("Monster_Crypt_Skeleton_1", "Cốt Binh Di Tích (Tả)", "Lv.4 · Quái Ngục 💀", new Vector3(-6.0f, -42.0f, 0), wolfSprite, false, hasAI: true, isBoss: false, maxHp: 220, attackRange: 1.8f, attackDamage: 24, scale: 1.1f);
            CreateMonsterTarget("Monster_Crypt_Skeleton_2", "Cốt Binh Di Tích (Hữu)", "Lv.4 · Quái Ngục 💀", new Vector3(6.0f, -42.0f, 0), wolfSprite, false, hasAI: true, isBoss: false, maxHp: 220, attackRange: 1.8f, attackDamage: 24, scale: 1.1f);
            CreateMonsterTarget("Boss_Crypt_Lich", "Đại Vu Yêu Hắc Ám (Dungeon Boss)", "Lv.6 · BOSS NGỤC 🔥", new Vector3(0.0f, -36.0f, 0), lichBossSprite, false, hasAI: true, isBoss: true, maxHp: 850, attackRange: 3.6f, attackDamage: 45, scale: 1.6f);

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

            // 13. Khởi tạo Hệ Thống Nhiệm Vụ & Sổ Tay Nhiệm Vụ Phím [L]
            if (FindAnyObjectByType<Quest.QuestManager>() == null)
            {
                new GameObject("[QUEST_MANAGER]").AddComponent<Quest.QuestManager>();
            }

            // 14. Khởi tạo Môi Trường & Ánh Sáng 2D Động, Sương Mù & Chu Kỳ Ngày/Đêm
            if (FindAnyObjectByType<EnvironmentAtmosphereManager>() == null)
            {
                new GameObject("[ENVIRONMENT_ATMOSPHERE_MANAGER]").AddComponent<EnvironmentAtmosphereManager>();
            }

            // 15. Khởi tạo Rương Chuyển Sinh, Màn Hình Tử Trận & Modal NPC
            if (FindAnyObjectByType<Player.RebirthVaultManager>() == null)
            {
                new GameObject("[REBIRTH_VAULT_MANAGER]").AddComponent<Player.RebirthVaultManager>();
            }
            if (FindAnyObjectByType<UI.DeathScreenModal>() == null)
            {
                new GameObject("[DEATH_SCREEN_MODAL]").AddComponent<UI.DeathScreenModal>();
            }
            if (FindAnyObjectByType<UI.VillageShopNPCModal>() == null)
            {
                new GameObject("[VILLAGE_SHOP_NPC_MODAL]").AddComponent<UI.VillageShopNPCModal>();
            }

            GameObject hudObj = new GameObject("Combat_HUD");
            hudObj.AddComponent<CombatHUD>();

            Debug.Log("<color=green>[SquiRRels Studio] Thế giới Alpha Slice đã hoàn thiện 100%: Dàn NPC & Quái AI & Cổng Dịch Chuyển & Chu Kỳ Ngày Đêm & Hệ Thống Nhiệm Vụ [L] đã kích hoạt!</color>");
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
            GamePhysicsOptimizer.SetLayer(dummyObj, GamePhysicsOptimizer.LayerEnemy);

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

        private void CreateVillageNPC(string id, string name, string roleStr, Vector3 pos, Sprite sprite, bool isShop, GodlessFantasy.UI.VillageNPCType shopType, string dial)
        {
            if (sprite == null) return;
            GameObject npcObj = new GameObject(id);
            npcObj.transform.position = pos;

            var sr = npcObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;

            var col = npcObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.0f);

            var health = npcObj.AddComponent<Combat.Health>();
            health.IsInSafeZone = true;

            var vNpc = npcObj.AddComponent<Player.VillageNPC>();
            vNpc.Setup(name, dial, isShop, shopType);

            var overhead = npcObj.AddComponent<UI.OverheadStatusBar>();
            overhead.Setup(UI.OverheadType.NPC, name, roleStr, 0.95f);
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
