using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Combat;
using GodlessFantasy.Core;
using GodlessFantasy.UI;
using GodlessFantasy.AI;
using GodlessFantasy.Player;

namespace GodlessFantasy.World
{
    /// <summary>
    /// Engine Quản Lý Phân Vùng Đa Bản Đồ & Chuyển Vùng Ranh Giới 4 Hướng (Multi-Region & 4-Way Seamless Boundary Engine):
    /// - Quản lý nạp động từng Khu Vực (World Region) từ JSON.
    /// - Cơ chế chuyển tiếp 4 hướng:
    ///   + Chạy hết mép Đông (X >= MaxX) -> Hủy vùng cũ, nạp vùng mới, xuất hiện ở mép Tây (X = MinX + 1.8), giữ nguyên Y!
    ///   + Chạy hết mép Tây (X <= MinX) -> Xuất hiện ở mép Đông (X = MaxX - 1.8), giữ nguyên Y!
    ///   + Chạy hết mép Bắc (Y >= MaxY) -> Xuất hiện ở mép Nam (Y = MinY + 1.8), giữ nguyên X!
    ///   + Chạy hết mép Nam (Y <= MinY) -> Xuất hiện ở mép Bắc (Y = MaxY - 1.8), giữ nguyên X!
    /// - Cơ chế Phó Bản Hầm Ngục (Dungeon Instances): Nạp độc lập khi qua Cổng Portal, thoát ra trả về đúng vị trí bên ngoài.
    /// - Tự động quản lý vòng đời Bãi Quái (Monster Camps) và Hồi Sinh (Respawn).
    /// </summary>
    public class RegionManager : MonoBehaviour
    {
        public static RegionManager Instance { get; private set; }

        public RegionData CurrentRegion { get; private set; }
        public string CurrentRegionId => CurrentRegion != null ? CurrentRegion.regionId : "region_village_0_0";

        private Transform playerTransform;
        private float transitionCooldown = 0f;

        // Banner thông báo chuyển vùng
        private string bannerText = "";
        private float bannerTimer = 0f;
        private GUIStyle bannerStyle;
        private GUIStyle promptStyle;

        // Quản lý quái vật và cổng của vùng hiện tại
        private List<GameObject> activeMonsters = new List<GameObject>();
        private List<GameObject> activePortals = new List<GameObject>();
        private Transform regionEntitiesParent;

        // Cache sprites quái vật
        private Sprite slimeGrassSprite;
        private Sprite slimeWaterSprite;
        private Sprite wolfSprite;
        private Sprite lichBossSprite;
        private Sprite dummySprite;
        private Sprite portalSprite;

        private PortalData nearbyPortal = null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadCachedSprites();
        }

        private void LoadCachedSprites()
        {
            dummySprite = Resources.Load<Sprite>("dummy");
            slimeGrassSprite = Resources.Load<Sprite>("monster_slime_grass") ?? dummySprite;
            slimeWaterSprite = Resources.Load<Sprite>("monster_slime_water") ?? dummySprite;
            wolfSprite = Resources.Load<Sprite>("monster_forest_wolf") ?? dummySprite;
            lichBossSprite = Resources.Load<Sprite>("monster_lich_boss") ?? dummySprite;
            portalSprite = Resources.Load<Sprite>("prop_dungeon_gate");
        }

        public void InitializeFirstRegion(string defaultRegionId = "region_village_0_0")
        {
            if (regionEntitiesParent == null)
            {
                var existing = GameObject.Find("[REGION_DYNAMIC_ENTITIES]");
                if (existing != null) regionEntitiesParent = existing.transform;
                else regionEntitiesParent = new GameObject("[REGION_DYNAMIC_ENTITIES]").transform;
            }

            LoadRegion(defaultRegionId, null, "📍 KHỞI ĐẦU TẠI: LÀNG TÂN THỦ & ĐẠI LỘ CỔ");
        }

        public void LoadRegion(string regionId, Vector2? entryPosition = null, string bannerMessage = null)
        {
            TextAsset mapJson = Resources.Load<TextAsset>($"Maps/{regionId}");
            if (mapJson == null)
            {
                Debug.LogError($"[REGION MANAGER] Không tìm thấy tệp bản đồ: Resources/Maps/{regionId}.json");
                return;
            }

            RegionData data = null;
            try
            {
                data = JsonUtility.FromJson<RegionData>(mapJson.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[REGION MANAGER] Lỗi phân tích JSON cho {regionId}: {ex.Message}");
                return;
            }

            if (data == null) return;

            string oldRegionName = CurrentRegion != null ? CurrentRegion.displayName : "Khởi Tạo";
            CurrentRegion = data;

            // 1. Dọn dẹp quái vật & cổng của vùng cũ
            DespawnCurrentRegionEntities();

            // 2. Nạp địa hình & props vào BlockMapManager
            if (BlockMapManager.Instance != null)
            {
                BlockMapManager.Instance.ApplyRegionData(data);
            }

            // 3. Đặt vị trí người chơi
            FindPlayer();
            Vector2 spawnPos = entryPosition ?? (data.defaultSpawn != null ? new Vector2(data.defaultSpawn.x, data.defaultSpawn.y) : Vector2.zero);
            if (playerTransform != null)
            {
                playerTransform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

                // Reset vận tốc / đích đến di chuyển của người chơi để tránh giật
                var movement = playerTransform.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.StopMovement();
                }

                // Snap camera mượt mà
                var cam = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
                if (cam != null)
                {
                    cam.SnapToTarget();
                }
            }

            // 4. Sinh quái vật động từ danh sách bãi quái
            SpawnMonsterCamps(data.monsterCamps);

            // 5. Sinh cổng phó bản / portal
            SpawnPortals(data.portals);

            // 6. Hiển thị Banner chuyển cảnh
            bannerText = !string.IsNullOrEmpty(bannerMessage) ? bannerMessage : $"📍 BẠN ĐÃ BƯỚC VÀO: {data.displayName.ToUpper()}";
            bannerTimer = 3.8f;
            transitionCooldown = 1.0f; // Chống lặp chuyển vùng tức thời

            Debug.Log($"<color=cyan>[REGION ENGINE]</color> Chuyển tiếp thành công: <color=yellow>[{oldRegionName}]</color> ➔ <color=green>[{data.displayName}]</color> tại tọa độ ({spawnPos.x:F1}, {spawnPos.y:F1})");
        }

        private void FindPlayer()
        {
            if (playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
                if (p != null) playerTransform = p.transform;
            }
        }

        private void Update()
        {
            if (bannerTimer > 0f) bannerTimer -= Time.deltaTime;
            if (transitionCooldown > 0f)
            {
                transitionCooldown -= Time.deltaTime;
                return;
            }

            FindPlayer();
            if (playerTransform == null || CurrentRegion == null || CurrentRegion.dimensions == null) return;

            Vector3 pPos = playerTransform.position;
            var dims = CurrentRegion.dimensions;
            var conns = CurrentRegion.connections;

            // =========================================================
            // CƠ CHẾ TỰ ĐỘNG CHUYỂN VÙNG RANH GIỚI 4 HƯỚNG
            // =========================================================
            // 1. Chạm Rìa ĐÔNG (East Edge: X >= MaxX)
            if (pPos.x >= dims.maxX - 0.2f && conns != null && conns.east != null && conns.east.enabled && !string.IsNullOrEmpty(conns.east.targetRegionId))
            {
                // Sang vùng Đông -> Xuất hiện ở mép Tây của vùng mới (X = -135 + 1.8), giữ nguyên Y!
                float newX = -135f + 1.8f;
                float newY = pPos.y;
                LoadRegion(conns.east.targetRegionId, new Vector2(newX, newY), $"➡️ TIẾN VÀO VÙNG PHÍA ĐÔNG: {conns.east.targetDisplayName.ToUpper()}");
                return;
            }

            // 2. Chạm Rìa TÂY (West Edge: X <= MinX)
            if (pPos.x <= dims.minX + 0.2f && conns != null && conns.west != null && conns.west.enabled && !string.IsNullOrEmpty(conns.west.targetRegionId))
            {
                // Sang vùng Tây -> Xuất hiện ở mép Đông của vùng mới (X = 135 - 1.8), giữ nguyên Y!
                float newX = 135f - 1.8f;
                float newY = pPos.y;
                LoadRegion(conns.west.targetRegionId, new Vector2(newX, newY), $"⬅️ TIẾN VÀO VÙNG PHÍA TÂY: {conns.west.targetDisplayName.ToUpper()}");
                return;
            }

            // 3. Chạm Rìa BẮC (North Edge: Y >= MaxY)
            if (pPos.y >= dims.maxY - 0.2f && conns != null && conns.north != null && conns.north.enabled && !string.IsNullOrEmpty(conns.north.targetRegionId))
            {
                // Sang vùng Bắc -> Xuất hiện ở mép Nam của vùng mới (Y = -66 + 1.8), giữ nguyên X!
                float newX = pPos.x;
                float newY = -66f + 1.8f;
                LoadRegion(conns.north.targetRegionId, new Vector2(newX, newY), $"⬆️ TIẾN VÀO VÙNG PHÍA BẮC: {conns.north.targetDisplayName.ToUpper()}");
                return;
            }

            // 4. Chạm Rìa NAM (South Edge: Y <= MinY)
            if (pPos.y <= dims.minY + 0.2f && conns != null && conns.south != null && conns.south.enabled && !string.IsNullOrEmpty(conns.south.targetRegionId))
            {
                // Sang vùng Nam -> Xuất hiện ở mép Bắc của vùng mới (Y = 66 - 1.8), giữ nguyên X!
                float newX = pPos.x;
                float newY = 66f - 1.8f;
                LoadRegion(conns.south.targetRegionId, new Vector2(newX, newY), $"⬇️ TIẾN VÀO VÙNG PHÍA NAM: {conns.south.targetDisplayName.ToUpper()}");
                return;
            }

            // =========================================================
            // KIỂM TRA TƯƠNG TÁC CỔNG PHÓ BẢN / PORTALS
            // =========================================================
            nearbyPortal = null;
            if (CurrentRegion.portals != null)
            {
                foreach (var portal in CurrentRegion.portals)
                {
                    float dist = Vector2.Distance(pPos, new Vector2(portal.x, portal.y));
                    if (dist <= 2.2f)
                    {
                        nearbyPortal = portal;
                        if (Input.GetKeyDown(KeyCode.F))
                        {
                            Vector2 targetPos = new Vector2(portal.targetX, portal.targetY);
                            LoadRegion(portal.targetRegionId, targetPos, $"⛩️ BƯỚC QUA CỔNG: {portal.name.ToUpper()}");
                        }
                        break;
                    }
                }
            }
        }

        // =========================================================
        // QUẢN LÝ BÃI QUÁI ĐỘNG & TÁI SINH (MONSTER CAMPS)
        // =========================================================
        private void SpawnMonsterCamps(List<MonsterCampData> camps)
        {
            if (camps == null) return;

            foreach (var camp in camps)
            {
                StartCoroutine(SpawnCampRoutine(camp));
            }
        }

        private IEnumerator SpawnCampRoutine(MonsterCampData camp)
        {
            int toSpawn = Mathf.Max(1, camp.count);
            for (int i = 0; i < toSpawn; i++)
            {
                // Phân bổ ngẫu nhiên trong bán kính bãi
                Vector2 offset = UnityEngine.Random.insideUnitCircle * (camp.radius * 0.75f);
                Vector3 spawnPos = new Vector3(camp.x + offset.x, camp.y + offset.y, 0f);

                SpawnSingleMonster(camp, spawnPos, i);
                yield return null;
            }
        }

        private void SpawnSingleMonster(MonsterCampData camp, Vector3 pos, int index)
        {
            Sprite spr = GetMonsterSprite(camp.monsterType);
            string mName = $"{camp.campId}_Monster_{index}";
            string lvlStr = $"Lv.{camp.level} · {(camp.isBoss ? "BOSS 🔥" : "Ma Vật")}";

            int hp = camp.isBoss ? 750 + camp.level * 80 : 100 + camp.level * 30;
            int dmg = camp.isBoss ? 35 + camp.level * 4 : 14 + camp.level * 3;
            float scale = camp.isBoss ? 1.45f : 1.0f;

            GameObject mObj = new GameObject(mName);
            mObj.transform.parent = regionEntitiesParent;
            mObj.transform.position = pos;
            if (scale > 1.01f) mObj.transform.localScale = new Vector3(scale, scale, 1f);

            GamePhysicsOptimizer.SetLayer(mObj, GamePhysicsOptimizer.LayerEnemy);

            var sr = mObj.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = 10;
            if (camp.isBoss) sr.color = new Color(1f, 0.82f, 0.82f, 1f);

            var col = mObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.85f, 0.95f);

            var rb = mObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var health = mObj.AddComponent<Health>();
            health.SetMaxHealth(hp);
            health.IsInSafeZone = false;

            // Hook sự kiện chết để rơi đồ, thưởng exp và hồi sinh bãi
            health.OnDeath += () => OnMonsterKilled(camp, mObj);

            mObj.AddComponent<MonsterTarget>();

            var overhead = mObj.AddComponent<OverheadStatusBar>();
            overhead.Setup(OverheadType.Monster, camp.displayName, lvlStr, camp.isBoss ? 1.25f : 0.85f);

            var ai = mObj.AddComponent<MonsterAIController>();
            ai.spawnPoint = pos;
            ai.isBoss = camp.isBoss;
            ai.attackRange = camp.isBoss ? 3.2f : 1.8f;
            ai.attackDamage = dmg;
            ai.wanderRadius = Mathf.Min(camp.radius * 0.6f, 4.5f);
            ai.leashRadius = camp.radius * 1.5f;

            activeMonsters.Add(mObj);
        }

        private void OnMonsterKilled(MonsterCampData camp, GameObject monsterObj)
        {
            if (monsterObj != null)
            {
                activeMonsters.Remove(monsterObj);
            }

            // 1. Thưởng Exp và Vàng cho Người Chơi
            if (PlayerStats.Instance != null)
            {
                int expGain = camp.isBoss ? 150 + camp.level * 25 : 25 + camp.level * 10;
                int goldGain = camp.isBoss ? 80 + camp.level * 15 : 8 + camp.level * 3;
                PlayerStats.Instance.GainExp(expGain);
                PlayerStats.Instance.AddGold(goldGain);
            }

            // 2. Kích hoạt Coroutine hồi sinh sau respawnSeconds
            float delay = camp.respawnSeconds > 0 ? camp.respawnSeconds : 20f;
            StartCoroutine(RespawnSingleRoutine(camp, delay));
        }

        private IEnumerator RespawnSingleRoutine(MonsterCampData camp, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Nếu người chơi vẫn đang ở trong khu vực này thì mới hồi sinh
            if (CurrentRegion != null && CurrentRegion.monsterCamps != null && CurrentRegion.monsterCamps.Contains(camp))
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * (camp.radius * 0.75f);
                Vector3 spawnPos = new Vector3(camp.x + offset.x, camp.y + offset.y, 0f);
                SpawnSingleMonster(camp, spawnPos, UnityEngine.Random.Range(100, 999));
            }
        }

        private Sprite GetMonsterSprite(string monsterType)
        {
            if (monsterType.Contains("water")) return slimeWaterSprite;
            if (monsterType.Contains("wolf")) return wolfSprite;
            if (monsterType.Contains("boss") || monsterType.Contains("lich")) return lichBossSprite;
            return slimeGrassSprite;
        }

        // =========================================================
        // QUẢN LÝ CỔNG DỊCH CHUYỂN (PORTALS)
        // =========================================================
        private void SpawnPortals(List<PortalData> portals)
        {
            if (portals == null) return;

            foreach (var p in portals)
            {
                GameObject portalObj = new GameObject($"Portal_{p.portalId}");
                portalObj.transform.parent = regionEntitiesParent;
                portalObj.transform.position = new Vector3(p.x, p.y, 0f);

                var sr = portalObj.AddComponent<SpriteRenderer>();
                sr.sprite = portalSprite;
                sr.sortingOrder = 9;

                var col = portalObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.8f, 1.8f);

                activePortals.Add(portalObj);
            }
        }

        private void DespawnCurrentRegionEntities()
        {
            StopAllCoroutines();

            foreach (var m in activeMonsters)
            {
                if (m != null) Destroy(m);
            }
            activeMonsters.Clear();

            foreach (var p in activePortals)
            {
                if (p != null) Destroy(p);
            }
            activePortals.Clear();
        }

        // =========================================================
        // HIỂN THỊ BANNER & GỢI Ý TƯƠNG TÁC
        // =========================================================
        private void OnGUI()
        {
            InitStyles();

            // 1. Banner chuyển vùng trên cùng
            if (bannerTimer > 0f)
            {
                float alpha = Mathf.Clamp01(bannerTimer / 0.8f);
                Color oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);

                float bannerW = 540f;
                float bannerH = 48f;
                float bX = (Screen.width - bannerW) / 2f;
                float bY = 24f;

                Rect bRect = new Rect(bX, bY, bannerW, bannerH);
                GUI.Box(bRect, "", bannerStyle);

                GUI.Label(new Rect(bX, bY + 10f, bannerW, 28f), bannerText, bannerStyle);
                GUI.color = oldC;
            }

            // 2. Gợi ý tương tác Cổng Hầm Ngục khi lại gần
            if (nearbyPortal != null)
            {
                float pW = 380f;
                float pH = 40f;
                float px = (Screen.width - pW) / 2f;
                float py = Screen.height - 180f;

                Rect pRect = new Rect(px, py, pW, pH);
                GUI.Box(pRect, "", promptStyle);

                string msg = $"⛩️ [F] Bước Vào: {nearbyPortal.name}";
                GUI.Label(pRect, msg, promptStyle);
            }
        }

        private void InitStyles()
        {
            if (bannerStyle == null)
            {
                bannerStyle = new GUIStyle(GUI.skin.box);
                bannerStyle.alignment = TextAnchor.MiddleCenter;
                bannerStyle.fontSize = 16;
                bannerStyle.fontStyle = FontStyle.Bold;
                bannerStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);
            }

            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.box);
                promptStyle.alignment = TextAnchor.MiddleCenter;
                promptStyle.fontSize = 14;
                promptStyle.fontStyle = FontStyle.Bold;
                promptStyle.normal.textColor = new Color(0.2f, 0.95f, 1.0f);
            }
        }
    }
}
