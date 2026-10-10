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

        private Transform floorParent;
        private Dictionary<Vector2Int, GameObject> blockObjects = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, TerrainBlockType> blockTypes = new Dictionary<Vector2Int, TerrainBlockType>();

        // Cache Sprites
        private Sprite safeTileSprite;
        private Sprite stoneTileSprite;
        private Sprite grassTileSprite;

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
        }

        public void InitializeMap(int minX = -22, int maxX = 22, int minY = -12, int maxY = 12)
        {
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

            // Ngược lại khởi tạo map mặc định chia 2 vùng Làng Tân Thủ & Rừng Rậm
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    TerrainBlockType type;
                    if (x <= -6) type = TerrainBlockType.SafeStone;
                    else if (x >= 2) type = ((x + y) % 3 == 0) ? TerrainBlockType.Grass : TerrainBlockType.AncientStone;
                    else type = TerrainBlockType.AncientStone;

                    SetBlock(x, y, type, false);
                }
            }
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

            switch (type)
            {
                case TerrainBlockType.SafeStone:
                    sr.sprite = (safeTileSprite != null) ? safeTileSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    if (col != null) Destroy(col);
                    break;

                case TerrainBlockType.AncientStone:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : safeTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    if (col != null) Destroy(col);
                    break;

                case TerrainBlockType.Grass:
                    sr.sprite = (grassTileSprite != null) ? grassTileSprite : stoneTileSprite;
                    sr.color = Color.white;
                    sr.sortingOrder = -10;
                    if (col != null) Destroy(col);
                    break;

                case TerrainBlockType.Water:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : safeTileSprite;
                    sr.color = new Color(0.12f, 0.45f, 0.95f, 0.95f); // Màu xanh dương nước biển
                    sr.sortingOrder = -9;
                    if (col == null) col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(0.98f, 0.98f);
                    break;

                case TerrainBlockType.Wall:
                    sr.sprite = (stoneTileSprite != null) ? stoneTileSprite : safeTileSprite;
                    sr.color = new Color(0.35f, 0.28f, 0.24f, 1.0f); // Màu tường đá trầm kiên cố
                    sr.sortingOrder = 5; // Nổi lên trên mặt đất
                    if (col == null) col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(0.98f, 0.98f);
                    break;

                case TerrainBlockType.Gate:
                    sr.sprite = (safeTileSprite != null) ? safeTileSprite : stoneTileSprite;
                    sr.color = new Color(1.0f, 0.85f, 0.35f, 1.0f); // Cổng vàng kim
                    sr.sortingOrder = -9;
                    if (col != null) Destroy(col);
                    break;
            }
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
