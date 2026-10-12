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
    /// Engine Quản Lý Địa Hình Dạng Khối (Block-Based Terrain Engine):
    /// - Không cần code để xây dựng map!
    /// - Cho phép vẽ và thay đổi địa hình trực tiếp trong game hoặc lưu thành file JSON.
    /// - Tự động thiết lập Sprite, lớp hiển thị và Collider va chạm chống đi xuyên vật thể.
    /// </summary>
    public class BlockMapManager : MonoBehaviour
    {
        public static BlockMapManager Instance { get; private set; }

        public int MinX { get; private set; } = -45;
        public int MaxX { get; private set; } = 45;
        public int MinY { get; private set; } = -22;
        public int MaxY { get; private set; } = 22;

        private Transform floorParent;
        private Dictionary<Vector2Int, GameObject> blockObjects = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, TerrainBlockType> blockTypes = new Dictionary<Vector2Int, TerrainBlockType>();

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

        public void InitializeMap(int minX = -45, int maxX = 45, int minY = -22, int maxY = 22)
        {
            MinX = minX; MaxX = maxX; MinY = minY; MaxY = maxY;

            if (floorParent == null)
            {
                var existing = GameObject.Find("Environment_Floor");
                if (existing != null) floorParent = existing.transform;
                else floorParent = new GameObject("Environment_Floor").transform;
            }

            // Nếu đã có file map JSON tùy chỉnh do người dùng vẽ -> Tải map đó lên
            if (File.Exists(SaveFilePath))
            {
                if (LoadMapFromJson()) return;
            }

            // Khởi tạo map thế giới mở rộng lớn (90m x 44m) chia 3 khu vực phong phú
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    // Vách tường bao quanh thế giới chống đi lọt ra ngoài
                    if (x == minX || x == maxX || y == minY || y == maxY)
                    {
                        SetBlock(x, y, TerrainBlockType.Wall, false);
                        continue;
                    }

                    // Dòng suối tự nhiên uốn lượn tại x = 20 với cầu đá bắc qua ở y = 0..1
                    if (x == 20 || (x == 21 && y % 3 == 0))
                    {
                        if (Mathf.Abs(y) <= 1)
                        {
                            SetBlock(x, y, TerrainBlockType.AncientStone, false); // Cầu đá qua sông
                        }
                        else
                        {
                            SetBlock(x, y, TerrainBlockType.Water, false);
                        }
                        continue;
                    }

                    // Đường đất nện tự nhiên nối liền Làng Tân Thủ -> Cổng -> Rừng
                    bool isMainRoad = (y >= -1 && y <= 1);
                    bool isVillageBranch = (x >= -34 && x <= -10 && (y == 8 || y == -8));
                    bool isPath = isMainRoad || isVillageBranch;

                    TerrainBlockType type;
                    if (x <= -8)
                    {
                        // Khu vực Làng Tân Thủ (Safe Zone)
                        if (isPath)
                        {
                            type = TerrainBlockType.DirtPath;
                        }
                        else if (x <= -36 || y >= 14 || y <= -14)
                        {
                            type = TerrainBlockType.Grass;
                        }
                        else
                        {
                            type = TerrainBlockType.SafeStone;
                        }
                    }
                    else if (x <= 2)
                    {
                        // Ranh giới Cổng Làng & Đền Đài Trung Tâm
                        type = isMainRoad ? TerrainBlockType.AncientStone : TerrainBlockType.DirtPath;
                    }
                    else
                    {
                        // Khu vực Rừng Rậm Hoang Dã (PvP & Bãi Quái)
                        if (isMainRoad)
                        {
                            type = TerrainBlockType.DirtPath;
                        }
                        else
                        {
                            type = ((x * 3 + y * 7) % 5 == 0) ? TerrainBlockType.AncientStone : TerrainBlockType.Grass;
                        }
                    }

                    SetBlock(x, y, type, false);
                }
            }

            // Đặt các vật thể cảnh quan Props (Cây cối, Ngọn đuốc, Giếng nước, Hàng rào, Cổng Cổ)
            SpawnLandscapeProps();
        }

        /// <summary>
        /// Vẽ hoặc thay đổi 1 khối địa hình tại tọa độ (x, y)
        /// </summary>
        public void SetBlock(int x, int y, TerrainBlockType type, bool recordType = true)
        {
            Vector2Int key = new Vector2Int(x, y);

            if (type == TerrainBlockType.Erase)
            {
                if (blockObjects.TryGetValue(key, out GameObject oldObj))
                {
                    Destroy(oldObj);
                    blockObjects.Remove(key);
                    blockTypes.Remove(key);
                }
                return;
            }

            GameObject tileObj;
            if (!blockObjects.TryGetValue(key, out tileObj) || tileObj == null)
            {
                tileObj = new GameObject($"Tile_{x}_{y}");
                tileObj.transform.parent = floorParent;
                tileObj.transform.position = new Vector3(x, y, 0);
                blockObjects[key] = tileObj;
            }

            if (recordType)
            {
                blockTypes[key] = type;
            }

            var sr = tileObj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = tileObj.AddComponent<SpriteRenderer>();

            var col = tileObj.GetComponent<BoxCollider2D>();

            Transform overlayTf = tileObj.transform.Find("SafeZone_Overlay");

            switch (type)
            {
                case TerrainBlockType.SafeStone:
                    // 1. LỚP DƯỚI: Địa hình khối bình thường (Đá cổ Làng Tân Thủ)
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : grassTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    if (col != null) Destroy(col);

                    // 2. LỚP TRÊN: Lớp phủ Vùng An Toàn trong suốt / mờ nhẹ hỗ trợ debug
                    if (safeTileSprite != null)
                    {
                        GameObject overlayObj = (overlayTf != null) ? overlayTf.gameObject : new GameObject("SafeZone_Overlay");
                        overlayObj.transform.parent = tileObj.transform;
                        overlayObj.transform.localPosition = Vector3.zero;
                        var overlaySr = overlayObj.GetComponent<SpriteRenderer>();
                        if (overlaySr == null) overlaySr = overlayObj.AddComponent<SpriteRenderer>();
                        overlaySr.sprite = safeTileSprite;
                        overlaySr.color = new Color(1f, 1f, 1f, 0.45f); // Trong suốt mờ nhẹ cho debug
                        overlaySr.sortingOrder = -8; // Lớp trên lớp đất (-10), dưới nhân vật (0)
                    }
                    break;

                case TerrainBlockType.AncientStone:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : grassTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    if (col != null) Destroy(col);
                    if (overlayTf != null) Destroy(overlayTf.gameObject);
                    break;

                case TerrainBlockType.Grass:
                    sr.sprite = (grassTileSprite != null) ? grassTileSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    if (col != null) Destroy(col);
                    if (overlayTf != null) Destroy(overlayTf.gameObject);
                    break;

                case TerrainBlockType.Water:
                    sr.sprite = (waterTileSprite != null) ? waterTileSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -9;
                    if (col == null) col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(0.98f, 0.98f);
                    if (overlayTf != null) Destroy(overlayTf.gameObject);
                    break;

                case TerrainBlockType.Wall:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : grassTileSprite;
                    sr.color = new Color(0.35f, 0.28f, 0.24f, 1.0f); // Màu tường đá trầm kiên cố
                    sr.sortingOrder = 5; // Nổi lên trên mặt đất
                    if (col == null) col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(0.98f, 0.98f);
                    if (overlayTf != null) Destroy(overlayTf.gameObject);
                    break;

                case TerrainBlockType.Gate:
                    sr.sprite = (safeTileSprite != null) ? safeTileSprite : stoneTileSprite;
                    sr.color = new Color(1.0f, 0.85f, 0.35f, 1.0f); // Cổng vàng kim
                    sr.sortingOrder = -9;
                    if (col != null) Destroy(col);
                    if (overlayTf != null) Destroy(overlayTf.gameObject);
                    break;

                case TerrainBlockType.DirtPath:
                    sr.sprite = (dirtPathSprite != null) ? dirtPathSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    if (col != null) Destroy(col);
                    if (overlayTf != null) Destroy(overlayTf.gameObject);
                    break;
            }
        }

        private void SpawnLandscapeProps()
        {
            Transform propsParent = new GameObject("Environment_Props").transform;

            // 1. Giếng nước đá cổ ở trung tâm làng
            CreateProp("Prop_Stone_Well", stoneWellSprite, new Vector3(-22f, 0f, 0f), propsParent, new Vector2(0.8f, 0.8f));

            // 2. Cây xanh Làng Tân Thủ
            Vector3[] villageTreePos = {
                new Vector3(-35f, 10f, 0), new Vector3(-28f, 12f, 0), new Vector3(-18f, 11f, 0),
                new Vector3(-35f, -10f, 0), new Vector3(-28f, -12f, 0), new Vector3(-18f, -11f, 0),
                new Vector3(-12f, 6f, 0), new Vector3(-12f, -6f, 0)
            };
            foreach (var pos in villageTreePos)
            {
                CreateProp("Village_Tree", treeVillageSprite, pos, propsParent, new Vector2(0.7f, 0.4f), new Vector2(0, -0.6f), sortingOrder: 11);
            }

            // 3. Hàng rào gỗ bao quanh Làng Tân Thủ
            for (int fy = -14; fy <= 14; fy += 2)
            {
                if (Mathf.Abs(fy) > 2) // Chừa cổng lối đi ở giữa
                {
                    CreateProp($"Fence_WestBorder_{fy}", woodenFenceSprite, new Vector3(-8f, fy, 0), propsParent, new Vector2(0.95f, 0.4f));
                }
            }

            // 4. Ngọn đuốc thắp sáng (Village Torches & Road Torches)
            Vector3[] torchPositions = {
                new Vector3(-28f, 3f, 0), new Vector3(-28f, -3f, 0),
                new Vector3(-18f, 3f, 0), new Vector3(-18f, -3f, 0),
                new Vector3(-8.5f, 3f, 0), new Vector3(-8.5f, -3f, 0),
                new Vector3(2.5f, 3f, 0), new Vector3(2.5f, -3f, 0),
                new Vector3(18.5f, 3f, 0), new Vector3(18.5f, -3f, 0), // Đầu cầu đá
                new Vector3(32f, 9f, 0), new Vector3(36f, 9f, 0)       // Trước cửa Cổng Phó Bản
            };
            foreach (var tp in torchPositions)
            {
                CreateProp("Torch_Post", torchPostSprite, tp, propsParent, new Vector2(0.4f, 0.4f), new Vector2(0, -0.3f), sortingOrder: 10);
                if (EnvironmentAtmosphereManager.Instance != null)
                {
                    EnvironmentAtmosphereManager.Instance.RegisterTorch(tp);
                }
            }

            // 5. Cây Rừng Rậm Hắc Ám
            Vector3[] darkTreePos = {
                new Vector3(8f, 8f, 0), new Vector3(12f, 14f, 0), new Vector3(15f, -10f, 0),
                new Vector3(26f, 12f, 0), new Vector3(30f, -8f, 0), new Vector3(38f, -14f, 0),
                new Vector3(28f, 6f, 0), new Vector3(36f, 14f, 0), new Vector3(10f, -12f, 0)
            };
            foreach (var pos in darkTreePos)
            {
                CreateProp("Dark_Tree", treeDarkSprite, pos, propsParent, new Vector2(0.7f, 0.4f), new Vector2(0, -0.6f), sortingOrder: 11);
            }

            // 6. Cổng Đá Cổ Phó Bản Hầm Ngục (Dungeon Gate)
            CreateProp("Dungeon_Arch_Gate", dungeonGateSprite, new Vector3(34f, 10f, 0), propsParent, new Vector2(1.2f, 0.6f), new Vector2(0, -0.5f), sortingOrder: 12);
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

        public TerrainBlockType GetBlockType(int x, int y)
        {
            Vector2Int key = new Vector2Int(x, y);
            return blockTypes.TryGetValue(key, out var t) ? t : TerrainBlockType.AncientStone;
        }

        public void SaveMapToJson()
        {
            try
            {
                var wrapper = new MapDataWrapper();
                foreach (var kvp in blockTypes)
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
                Debug.Log($"<color=green>[MAP ENGINE]</color> Đã lưu {wrapper.blocks.Count} khối địa hình vào: {SaveFilePath}");
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
                        // Xóa các ô hiện tại
                        foreach (var kvp in blockObjects)
                        {
                            if (kvp.Value != null) Destroy(kvp.Value);
                        }
                        blockObjects.Clear();
                        blockTypes.Clear();

                        foreach (var b in wrapper.blocks)
                        {
                            SetBlock(b.x, b.y, (TerrainBlockType)b.blockType, true);
                        }
                        Debug.Log($"<color=green>[MAP ENGINE]</color> Đã tải thành công {wrapper.blocks.Count} khối địa hình từ file JSON!");
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
            foreach (var kvp in blockObjects)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            blockObjects.Clear();
            blockTypes.Clear();
            if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
            Debug.Log("[MAP ENGINE] Đã dọn sạch toàn bộ bản đồ!");
        }
    }
}
