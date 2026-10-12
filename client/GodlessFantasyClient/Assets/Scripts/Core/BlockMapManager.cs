using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GodlessFantasy.Core
{
    public enum TerrainBlockType
    {
        SafeStone = 0,    // Nền đá thánh làng tân thủ
        AncientStone = 1, // Đường đá cổ trung tâm
        Grass = 2,        // Cỏ xanh bãi quái
        Water = 3,        // Vùng nước sâu (Chặn di chuyển - có Collider)
        Wall = 4,         // Tường đá / Vách núi (Chặn di chuyển - có Collider)
        Gate = 5,         // Cổng ranh giới
        DirtPath = 6,     // Đường đất nện tự nhiên
        Erase = 99        // Tẩy xóa ô
    }

    [Serializable]
    public class TerrainBlockData
    {
        public int x;
        public int y;
        public int blockType;
    }

    [Serializable]
    public class MapDataWrapper
    {
        public List<TerrainBlockData> blocks = new List<TerrainBlockData>();
    }

    /// <summary>
    /// Engine Quản Lý Địa Hình Dạng Khối & Chunk Streaming Thế Giới Siêu Lớn (2D Open World Chunk Streaming Engine):
    /// - Bản đồ siêu rộng x3+ lần tiêu chuẩn (270m x 132m, diện tích hơn 35.000m²).
    /// - Kiến trúc Chunk Streaming (16m x 16m / chunk):
    ///   + Chỉ nạp động 5x5 chunks (80m x 80m) xung quanh Người Chơi.
    ///   + Chạy mượt mà 60-120 FPS, không gây đứng hình / giật lag khi tải game.
    ///   + Tự động thu hồi các chunk ở quá xa (> 3 chunks).
    /// - Sinh địa hình tự nhiên: Đại Đô Thị Làng Tân Thủ, Đại Lộ Cổ, Rừng Rậm Hắc Ám, Dòng Sông Ma Thuật, Cầu Đá, Cổng Hầm Ngục.
    /// </summary>
    public class BlockMapManager : MonoBehaviour
    {
        public static BlockMapManager Instance { get; private set; }

        public const int CHUNK_SIZE = 16; // 16x16 tiles per chunk

        // Kích thước thế giới siêu rộng lớn x3+ lần (270m x 132m)
        public int MinX { get; private set; } = -135;
        public int MaxX { get; private set; } = 135;
        public int MinY { get; private set; } = -66;
        public int MaxY { get; private set; } = 66;

        private Transform floorParent;
        private Transform propsParent;
        private Transform playerTransform;

        // Quản lý Chunk Streaming
        private Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();

        // Các khối tùy chỉnh được vẽ bằng cọ vẽ (F2 Painter) hoặc tải từ JSON
        private Dictionary<Vector2Int, TerrainBlockType> customBlockTypes = new Dictionary<Vector2Int, TerrainBlockType>();

        // Cache Sprites
        private Sprite safeTileSprite;
        private Sprite stoneTileSprite;
        private Sprite grassTileSprite;
        private Sprite dirtPathSprite;
        private Sprite waterTileSprite;

        // Props Sprites
        private Sprite treeVillageSprite;
        private Sprite treeDarkSprite;
        private Sprite torchPostSprite;
        private Sprite stoneWellSprite;
        private Sprite woodenFenceSprite;
        private Sprite dungeonGateSprite;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "custom_world_map.json");

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadSprites();
        }

        private void LoadSprites()
        {
            safeTileSprite = Resources.Load<Sprite>("tile_safezone");
            stoneTileSprite = Resources.Load<Sprite>("tile_stone");
            grassTileSprite = Resources.Load<Sprite>("tile_grass");
            dirtPathSprite = Resources.Load<Sprite>("tile_dirt_path");
            waterTileSprite = Resources.Load<Sprite>("tile_water");

            treeVillageSprite = Resources.Load<Sprite>("prop_tree_village");
            treeDarkSprite = Resources.Load<Sprite>("prop_tree_dark");
            torchPostSprite = Resources.Load<Sprite>("prop_torch_post");
            stoneWellSprite = Resources.Load<Sprite>("prop_stone_well");
            woodenFenceSprite = Resources.Load<Sprite>("prop_wooden_fence");
            dungeonGateSprite = Resources.Load<Sprite>("prop_dungeon_gate");
        }

        public void InitializeMap(int minX = -135, int maxX = 135, int minY = -66, int maxY = 66)
        {
            MinX = minX; MaxX = maxX; MinY = minY; MaxY = maxY;

            if (floorParent == null)
            {
                var existing = GameObject.Find("Environment_Floor");
                if (existing != null) floorParent = existing.transform;
                else floorParent = new GameObject("Environment_Floor").transform;
            }

            if (propsParent == null)
            {
                var existingProps = GameObject.Find("Environment_Props");
                if (existingProps != null) propsParent = existingProps.transform;
                else propsParent = new GameObject("Environment_Props").transform;
            }

            // Tải file map tùy chỉnh nếu có
            if (File.Exists(SaveFilePath))
            {
                LoadMapFromJson();
            }

            // Đặt các Props cảnh quan toàn thế giới
            SpawnLandscapeProps();

            // Nạp ngay cụm chunk ban đầu quanh vị trí khởi tạo
            FindPlayer();
            Vector3 startPos = playerTransform != null ? playerTransform.position : new Vector3(-70f, 0f, 0f);
            int startChunkX = Mathf.FloorToInt(startPos.x / CHUNK_SIZE);
            int startChunkY = Mathf.FloorToInt(startPos.y / CHUNK_SIZE);

            for (int cx = startChunkX - 2; cx <= startChunkX + 2; cx++)
            {
                for (int cy = startChunkY - 2; cy <= startChunkY + 2; cy++)
                {
                    LoadChunk(cx, cy);
                }
            }
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
            FindPlayer();
            if (playerTransform == null) return;

            int pcx = Mathf.FloorToInt(playerTransform.position.x / CHUNK_SIZE);
            int pcy = Mathf.FloorToInt(playerTransform.position.y / CHUNK_SIZE);

            // 1. Tải các chunk trong bán kính 2 chunk (5x5 chunks = 80m x 80m quanh người chơi)
            for (int cx = pcx - 2; cx <= pcx + 2; cx++)
            {
                for (int cy = pcy - 2; cy <= pcy + 2; cy++)
                {
                    LoadChunk(cx, cy);
                }
            }

            // 2. Gỡ các chunk ở quá xa (> 3 chunks = 48m)
            List<Vector2Int> toUnload = null;
            foreach (var kvp in loadedChunks)
            {
                if (Mathf.Abs(kvp.Key.x - pcx) > 3 || Mathf.Abs(kvp.Key.y - pcy) > 3)
                {
                    if (toUnload == null) toUnload = new List<Vector2Int>();
                    toUnload.Add(kvp.Key);
                }
            }

            if (toUnload != null)
            {
                foreach (var k in toUnload)
                {
                    UnloadChunk(k.x, k.y);
                }
            }
        }

        private void LoadChunk(int cx, int cy)
        {
            Vector2Int key = new Vector2Int(cx, cy);
            if (loadedChunks.ContainsKey(key)) return;

            int startX = cx * CHUNK_SIZE;
            int startY = cy * CHUNK_SIZE;

            // Kiểm tra nếu chunk hoàn toàn nằm ngoài giới hạn thế giới thì bỏ qua
            if (startX > MaxX || startX + CHUNK_SIZE < MinX || startY > MaxY || startY + CHUNK_SIZE < MinY)
                return;

            GameObject chunkObj = new GameObject($"Chunk_{cx}_{cy}");
            chunkObj.transform.parent = floorParent;
            loadedChunks[key] = chunkObj;

            for (int x = startX; x < startX + CHUNK_SIZE; x++)
            {
                if (x < MinX || x > MaxX) continue;
                for (int y = startY; y < startY + CHUNK_SIZE; y++)
                {
                    if (y < MinY || y > MaxY) continue;

                    TerrainBlockType type = GetBlockType(x, y);
                    CreateTileInChunk(x, y, type, chunkObj.transform);
                }
            }
        }

        private void UnloadChunk(int cx, int cy)
        {
            Vector2Int key = new Vector2Int(cx, cy);
            if (loadedChunks.TryGetValue(key, out GameObject chunkObj))
            {
                if (chunkObj != null) Destroy(chunkObj);
                loadedChunks.Remove(key);
            }
        }

        /// <summary>
        /// Tạo một ô địa hình bên trong Chunk GameObject
        /// </summary>
        private void CreateTileInChunk(int x, int y, TerrainBlockType type, Transform chunkParent)
        {
            GameObject tileObj = new GameObject($"T_{x}_{y}");
            tileObj.transform.parent = chunkParent;
            tileObj.transform.position = new Vector3(x, y, 0);

            var sr = tileObj.AddComponent<SpriteRenderer>();

            switch (type)
            {
                case TerrainBlockType.SafeStone:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : grassTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;

                    if (safeTileSprite != null)
                    {
                        GameObject overlayObj = new GameObject("SafeZone_Overlay");
                        overlayObj.transform.parent = tileObj.transform;
                        overlayObj.transform.localPosition = Vector3.zero;
                        var overlaySr = overlayObj.AddComponent<SpriteRenderer>();
                        overlaySr.sprite = safeTileSprite;
                        overlaySr.color = new Color(1f, 1f, 1f, 0.40f);
                        overlaySr.sortingOrder = -8;
                    }
                    break;

                case TerrainBlockType.AncientStone:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : grassTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    break;

                case TerrainBlockType.Grass:
                    sr.sprite = (grassTileSprite != null) ? grassTileSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    break;

                case TerrainBlockType.DirtPath:
                    sr.sprite = (dirtPathSprite != null) ? dirtPathSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    break;

                case TerrainBlockType.Water:
                    sr.sprite = (waterTileSprite != null) ? waterTileSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -9;
                    var wCol = tileObj.AddComponent<BoxCollider2D>();
                    wCol.size = new Vector2(0.98f, 0.98f);
                    break;

                case TerrainBlockType.Wall:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : grassTileSprite;
                    sr.color = new Color(0.32f, 0.25f, 0.22f, 1.0f);
                    sr.sortingOrder = 5;
                    var wallCol = tileObj.AddComponent<BoxCollider2D>();
                    wallCol.size = new Vector2(0.98f, 0.98f);
                    break;

                case TerrainBlockType.Gate:
                    sr.sprite = (safeTileSprite != null) ? safeTileSprite : stoneTileSprite;
                    sr.color = new Color(1.0f, 0.85f, 0.35f, 1.0f);
                    sr.sortingOrder = -9;
                    break;
            }
        }

        /// <summary>
        /// Thuật toán sinh địa hình thế giới mở rộng lớn (270m x 132m)
        /// </summary>
        public TerrainBlockType GetBlockType(int x, int y)
        {
            Vector2Int key = new Vector2Int(x, y);
            if (customBlockTypes.TryGetValue(key, out var customType))
            {
                return customType;
            }

            // 1. Tường thành biên giới bao quanh toàn bộ thế giới
            if (x <= MinX || x >= MaxX || y <= MinY || y >= MaxY)
            {
                return TerrainBlockType.Wall;
            }

            // 2. Dòng Sông Ma Thuật (Magic River) uốn lượn tại x = 44..47
            // Có Cầu Đá Cự Thạch bắc qua ở y = -2..2
            if (x >= 44 && x <= 47)
            {
                if (Mathf.Abs(y) <= 2)
                {
                    return TerrainBlockType.AncientStone; // Cầu đá lớn qua sông
                }
                return TerrainBlockType.Water; // Nước sâu chặn di chuyển
            }

            // 3. Trục Đại Lộ Cổ Chạy Ngang Qua Thế Giới
            bool isMainHighway = (y >= -2 && y <= 2);
            if (isMainHighway)
            {
                return (x <= -20) ? TerrainBlockType.DirtPath : TerrainBlockType.AncientStone;
            }

            // 4. KHU VỰC ĐẠI ĐÔ THỊ LÀNG TÂN THỦ (x <= -20)
            if (x <= -20)
            {
                // Đường đất nội bộ làng kết nối các khu vực
                bool isVillageRoad = (x == -80 || x == -50 || y == 25 || y == -25);
                if (isVillageRoad) return TerrainBlockType.DirtPath;

                // Quảng trường trung tâm làng (-80, 0)
                float distToPlaza = Vector2.Distance(new Vector2(x, y), new Vector2(-80, 0));
                if (distToPlaza <= 14f) return TerrainBlockType.SafeStone;

                // Khu vực ngoại ô cỏ xanh
                if (x <= -110 || y >= 45 || y <= -45)
                {
                    return TerrainBlockType.Grass;
                }

                return ((x + y) % 4 == 0) ? TerrainBlockType.Grass : TerrainBlockType.SafeStone;
            }

            // 5. VÙNG RANH GIỚI CỔNG LÀNG (-20 < x <= 10)
            if (x <= 10)
            {
                return TerrainBlockType.DirtPath;
            }

            // 6. KHU VỰC RỪNG RẬM HOANG DÃ & THUNG LŨNG TÀN TÍCH (x > 10)
            // Lối mòn dẫn lên Cổng Phó Bản Hầm Ngục tại (120, 25)
            if (x >= 80 && y >= 23 && y <= 27)
            {
                return TerrainBlockType.DirtPath;
            }

            // 7. KHU VỰC PHÓ BẢN HẦM NGỤC DI TÍCH (DUNGEON CRYPT FLOOR 1) TẠI (x: -16..16, y: -58..-32)
            if (Mathf.Abs(x) <= 16 && y >= -58 && y <= -32)
            {
                if (Mathf.Abs(x) == 16 || y == -58 || y == -32)
                {
                    return TerrainBlockType.Wall; // Tường đá kiên cố phong ấn hầm ngục
                }
                return TerrainBlockType.AncientStone; // Sàn đá cổ di tích
            }

            // Đất rừng xen kẽ tàn tích đá cổ
            return ((x * 5 + y * 11) % 6 == 0) ? TerrainBlockType.AncientStone : TerrainBlockType.Grass;
        }

        public void SetBlock(int x, int y, TerrainBlockType type, bool recordType = true)
        {
            Vector2Int key = new Vector2Int(x, y);

            if (type == TerrainBlockType.Erase)
            {
                customBlockTypes.Remove(key);
            }
            else
            {
                customBlockTypes[key] = type;
            }

            // Tải lại chunk chứa ô này nếu đang mở
            int cx = Mathf.FloorToInt((float)x / CHUNK_SIZE);
            int cy = Mathf.FloorToInt((float)y / CHUNK_SIZE);
            Vector2Int cKey = new Vector2Int(cx, cy);

            if (loadedChunks.ContainsKey(cKey))
            {
                UnloadChunk(cx, cy);
                LoadChunk(cx, cy);
            }
        }

        private void SpawnLandscapeProps()
        {
            // 1. LÀNG TÂN THỦ: Giếng nước đá cổ ở Quảng trường trung tâm (-80, 0)
            CreateProp("Prop_Stone_Well", stoneWellSprite, new Vector3(-80f, 0f, 0f), propsParent, new Vector2(0.8f, 0.8f));

            // 2. Cây xanh râm mát Làng Tân Thủ
            Vector3[] villageTreePos = {
                new Vector3(-95f, 15f, 0), new Vector3(-85f, 20f, 0), new Vector3(-70f, 18f, 0),
                new Vector3(-95f, -15f, 0), new Vector3(-85f, -20f, 0), new Vector3(-70f, -18f, 0),
                new Vector3(-55f, 12f, 0), new Vector3(-55f, -12f, 0), new Vector3(-40f, 10f, 0),
                new Vector3(-40f, -10f, 0), new Vector3(-105f, 5f, 0), new Vector3(-105f, -5f, 0)
            };
            foreach (var pos in villageTreePos)
            {
                CreateProp("Village_Tree", treeVillageSprite, pos, propsParent, new Vector2(0.7f, 0.4f), new Vector2(0, -0.6f), sortingOrder: 11);
            }

            // 3. Hàng rào gỗ bảo vệ Làng Tân Thủ tại ranh giới x = -20
            for (int fy = -40; fy <= 40; fy += 3)
            {
                if (Mathf.Abs(fy) > 3) // Chừa cổng đại lộ rộng mở ở giữa
                {
                    CreateProp($"Fence_Border_{fy}", woodenFenceSprite, new Vector3(-20f, fy, 0), propsParent, new Vector2(0.95f, 0.4f));
                }
            }

            // 4. Ngọn đuốc thắp sáng bập bùng (Toàn bộ các trục đường chính)
            Vector3[] torchPositions = {
                // Quảng trường làng
                new Vector3(-88f, 4f, 0), new Vector3(-88f, -4f, 0),
                new Vector3(-72f, 4f, 0), new Vector3(-72f, -4f, 0),
                // Trục đường làng ra cổng
                new Vector3(-50f, 4f, 0), new Vector3(-50f, -4f, 0),
                new Vector3(-35f, 4f, 0), new Vector3(-35f, -4f, 0),
                // Cổng ranh giới SafeZone
                new Vector3(-20.5f, 4.5f, 0), new Vector3(-20.5f, -4.5f, 0),
                new Vector3(-5f, 4.5f, 0), new Vector3(-5f, -4.5f, 0),
                new Vector3(15f, 4.5f, 0), new Vector3(15f, -4.5f, 0),
                // Cầu đá qua sông (x = 45)
                new Vector3(42f, 4.5f, 0), new Vector3(42f, -4.5f, 0),
                new Vector3(49f, 4.5f, 0), new Vector3(49f, -4.5f, 0),
                // Cổng Hầm Ngục Di Tích (120, 25)
                new Vector3(118f, 24f, 0), new Vector3(122f, 24f, 0)
            };
            foreach (var tp in torchPositions)
            {
                CreateProp("Torch_Post", torchPostSprite, tp, propsParent, new Vector2(0.4f, 0.4f), new Vector2(0, -0.3f), sortingOrder: 10);
                if (EnvironmentAtmosphereManager.Instance != null)
                {
                    EnvironmentAtmosphereManager.Instance.RegisterTorch(tp);
                }
            }

            // 5. Cây Rừng Rậm Hắc Ám (Phân bố khắp khu vực rừng x = 20 đến 130)
            Vector3[] darkTreePos = {
                new Vector3(25f, 15f, 0), new Vector3(32f, 28f, 0), new Vector3(30f, -20f, 0),
                new Vector3(38f, -35f, 0), new Vector3(55f, 20f, 0), new Vector3(65f, -15f, 0),
                new Vector3(75f, 30f, 0), new Vector3(85f, -25f, 0), new Vector3(95f, 15f, 0),
                new Vector3(105f, -30f, 0), new Vector3(115f, 15f, 0), new Vector3(125f, -15f, 0),
                new Vector3(60f, 38f, 0), new Vector3(90f, 40f, 0), new Vector3(70f, -40f, 0)
            };
            foreach (var pos in darkTreePos)
            {
                CreateProp("Dark_Tree", treeDarkSprite, pos, propsParent, new Vector2(0.7f, 0.4f), new Vector2(0, -0.6f), sortingOrder: 11);
            }

            // 6. CỔNG PHÓ BẢN HẦM NGỤC DI TÍCH (Dungeon Arch Gate tại 120, 25)
            CreateProp("Dungeon_Arch_Gate", dungeonGateSprite, new Vector3(120f, 25f, 0), propsParent, new Vector2(1.2f, 0.6f), new Vector2(0, -0.5f), sortingOrder: 12);
        }

        private GameObject CreateProp(string name, Sprite sprite, Vector3 pos, Transform parent, Vector2? colSize = null, Vector2? colOffset = null, int sortingOrder = 10)
        {
            if (sprite == null) return null;
            GameObject propObj = new GameObject(name);
            propObj.transform.parent = parent;
            propObj.transform.position = pos;

            var sr = propObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            if (colSize.HasValue)
            {
                var col = propObj.AddComponent<BoxCollider2D>();
                col.size = colSize.Value;
                if (colOffset.HasValue) col.offset = colOffset.Value;
            }

            return propObj;
        }

        public void SaveMapToJson()
        {
            try
            {
                var wrapper = new MapDataWrapper();
                foreach (var kvp in customBlockTypes)
                {
                    wrapper.blocks.Add(new TerrainBlockData
                    {
                        x = kvp.Key.x,
                        y = kvp.Key.y,
                        blockType = (int)kvp.Value
                    });
                }
                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"<color=green>[MAP ENGINE]</color> Đã lưu {wrapper.blocks.Count} khối tùy chỉnh vào: {SaveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MAP ENGINE] Lỗi khi lưu map: {ex.Message}");
            }
        }

        public bool LoadMapFromJson()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var wrapper = JsonUtility.FromJson<MapDataWrapper>(json);
                    if (wrapper != null && wrapper.blocks != null)
                    {
                        customBlockTypes.Clear();
                        foreach (var b in wrapper.blocks)
                        {
                            customBlockTypes[new Vector2Int(b.x, b.y)] = (TerrainBlockType)b.blockType;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MAP ENGINE] Lỗi khi tải map: {ex.Message}");
            }
            return false;
        }

        public void ClearEntireMap()
        {
            customBlockTypes.Clear();
            if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);

            foreach (var kvp in loadedChunks)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            loadedChunks.Clear();
            Debug.Log("[MAP ENGINE] Đã dọn sạch các khối tùy chỉnh và đặt lại bản đồ mặc định!");
        }
    }
}
