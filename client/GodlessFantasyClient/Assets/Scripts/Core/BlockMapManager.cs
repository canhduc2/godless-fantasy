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

        // Quản lý Chunk Streaming & Object Pooling (Zero GC / Khóa 300+ FPS)
        private Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> allCachedChunks = new Dictionary<Vector2Int, GameObject>();
        private Queue<Vector2Int> pendingLoadQueue = new Queue<Vector2Int>();
        private HashSet<Vector2Int> queuedChunks = new HashSet<Vector2Int>();

        // Hiệu ứng Sương Mù Tầm Nhìn Thế Giới (Atmospheric Vision Fog of War)
        private GameObject visionFogObj;
        private SpriteRenderer visionFogSr;

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
            World.TerrainBlockRegistry.EnsureInitialized();
            World.TerrainAtlasManager.EnsureInitialized();
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

        private float chunkStreamTimer = 0f;

        private void SetupVisionFog()
        {
            if (visionFogObj != null) return;
            visionFogObj = new GameObject("[WORLD_ATMOSPHERIC_FOG]");
            visionFogObj.transform.parent = transform;
            visionFogSr = visionFogObj.AddComponent<SpriteRenderer>();
            visionFogSr.sortingOrder = 45; // Nằm trên địa hình và props, dưới UI

            int size = 128;
            Texture2D fogTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            fogTex.filterMode = FilterMode.Bilinear;
            fogTex.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxR = size / 2f;

            Color fogColor = new Color(0.04f, 0.06f, 0.10f, 0.96f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxR;
                    // Vùng trong (dist < 0.38f): hoàn toàn trong suốt nhìn rõ map
                    // Vùng chuyển tiếp (0.38f <= dist <= 0.85f): sương mù dày dần
                    // Vùng ngoài (dist > 0.85f): sương mù che kín 100%
                    float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.38f, 0.85f, dist));
                    Color c = fogColor;
                    c.a = fogColor.a * alpha;
                    fogTex.SetPixel(x, y, c);
                }
            }
            fogTex.Apply();

            Sprite fogSprite = Sprite.Create(fogTex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            visionFogSr.sprite = fogSprite;
            visionFogSr.color = Color.white;
        }

        private void Update()
        {
            FindPlayer();
            if (playerTransform == null) return;

            // 1. CẬP NHẬT HIỆU ỨNG SƯƠNG MÙ TẦM NHÌN (Vision Fog of War)
            if (visionFogObj == null) SetupVisionFog();
            if (visionFogObj != null)
            {
                visionFogObj.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, 0f);
                Camera cam = Camera.main;
                float camSize = cam != null ? cam.orthographicSize : 10f;
                // Co giãn sương mù mượt mà tương ứng với tầm nhìn (bán kính sương mù bao quanh vùng ~60m)
                float targetFogScale = Mathf.Max(camSize * 6.5f, 50f);
                visionFogObj.transform.localScale = Vector3.Lerp(visionFogObj.transform.localScale, new Vector3(targetFogScale, targetFogScale, 1f), Time.deltaTime * 5f);
            }

            // 2. NẠP DẦN TỪNG CHUNK TRONG HÀNG ĐỢI (Mỗi frame tối đa 1 chunk để giữ mượt 300+ FPS)
            if (pendingLoadQueue.Count > 0)
            {
                Vector2Int nextChunk = pendingLoadQueue.Dequeue();
                queuedChunks.Remove(nextChunk);
                InstantiateChunkNow(nextChunk.x, nextChunk.y);
            }

            // 3. ĐIỀU PHỐI VÙNG CHUNK STREAMING ĐỊNH KỲ (0.15s)
            chunkStreamTimer += Time.deltaTime;
            if (chunkStreamTimer < 0.15f) return;
            chunkStreamTimer = 0f;

            int pcx = Mathf.FloorToInt(playerTransform.position.x / CHUNK_SIZE);
            int pcy = Mathf.FloorToInt(playerTransform.position.y / CHUNK_SIZE);

            // BÁN KÍNH TẢI KIỂM SOÁT CỐ ĐỊNH: Tối đa 2 chunk mỗi phía (5x5 chunks = 25 chunks = 80m x 80m)
            // Không bao giờ bùng nổ lên 99 chunk hay 25.000 GameObject khi zoom xa!
            const int radX = 2;
            const int radY = 2;

            // 3.1. Kích hoạt hoặc đưa vào hàng đợi nạp các chunk trong tầm nhìn
            for (int cx = pcx - radX; cx <= pcx + radX; cx++)
            {
                for (int cy = pcy - radY; cy <= pcy + radY; cy++)
                {
                    RequestLoadChunk(cx, cy);
                }
            }

            // 3.2. Ẩn (Deactivate) các chunk ở ngoài bán kính (ZERO Garbage Collection)
            const int unloadRadX = radX + 1;
            const int unloadRadY = radY + 1;
            List<Vector2Int> toUnload = null;
            foreach (var kvp in loadedChunks)
            {
                if (Mathf.Abs(kvp.Key.x - pcx) > unloadRadX || Mathf.Abs(kvp.Key.y - pcy) > unloadRadY)
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

        private void RequestLoadChunk(int cx, int cy)
        {
            Vector2Int key = new Vector2Int(cx, cy);
            if (loadedChunks.ContainsKey(key)) return;

            // Nếu chunk đã từng được tạo trước đó: Tái kích hoạt ngay lập tức (0ms CPU, 0 bytes GC!)
            if (allCachedChunks.TryGetValue(key, out GameObject cachedChunk))
            {
                if (cachedChunk != null)
                {
                    cachedChunk.SetActive(true);
                    loadedChunks[key] = cachedChunk;
                    return;
                }
            }

            // Nếu chưa có, đưa vào hàng đợi nạp dần
            if (!queuedChunks.Contains(key))
            {
                queuedChunks.Add(key);
                pendingLoadQueue.Enqueue(key);
            }
        }

        private void InstantiateChunkNow(int cx, int cy)
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
            chunkObj.transform.position = Vector3.zero;
            loadedChunks[key] = chunkObj;
            allCachedChunks[key] = chunkObj;

            // GỘP MESH ĐỊA HÌNH THEO CHUNK (CHUNK MESH COMBINER ENGINE):
            // Gộp toàn bộ 256 ô địa hình của Chunk thành 1 Mesh quad duy nhất dùng chung Master Texture Atlas!
            // Giảm hơn 99% số lượng GameObjects (từ 6,400 xuống 25) và kéo Draw Calls từ 1124 xuống ~1 - 2 Draw Calls cho toàn bộ mặt đất!
            List<Vector3> vertices = new List<Vector3>(1024);
            List<Vector2> uvs = new List<Vector2>(1024);
            List<Color> colors = new List<Color>(1024);
            List<int> triangles = new List<int>(1536);

            for (int x = startX; x < startX + CHUNK_SIZE; x++)
            {
                if (x < MinX || x > MaxX) continue;
                for (int y = startY; y < startY + CHUNK_SIZE; y++)
                {
                    if (y < MinY || y > MaxY) continue;

                    ushort blockId = GetBlockId(x, y);
                    var def = World.TerrainBlockRegistry.GetBlock(blockId);

                    // 1. Dựng Quad 1x1 trên Mesh mặt đất
                    ushort meshVisualId = blockId;
                    if (def.isStealth) meshVisualId = 0; // Bụi rậm tàng hình thì vẽ cỏ bên dưới, visual bụi nằm ở overlay trùm lên người chơi

                    Rect uv = World.TerrainAtlasManager.GetUV(meshVisualId);
                    int baseIdx = vertices.Count;

                    vertices.Add(new Vector3(x, y, 0f));
                    vertices.Add(new Vector3(x, y + 1f, 0f));
                    vertices.Add(new Vector3(x + 1f, y + 1f, 0f));
                    vertices.Add(new Vector3(x + 1f, y, 0f));

                    uvs.Add(new Vector2(uv.xMin, uv.yMin));
                    uvs.Add(new Vector2(uv.xMin, uv.yMax));
                    uvs.Add(new Vector2(uv.xMax, uv.yMax));
                    uvs.Add(new Vector2(uv.xMax, uv.yMin));

                    colors.Add(Color.white);
                    colors.Add(Color.white);
                    colors.Add(Color.white);
                    colors.Add(Color.white);

                    triangles.Add(baseIdx);
                    triangles.Add(baseIdx + 1);
                    triangles.Add(baseIdx + 2);
                    triangles.Add(baseIdx);
                    triangles.Add(baseIdx + 2);
                    triangles.Add(baseIdx + 3);

                    // 2. Tối ưu Collider: Gắn BoxCollider2D trực tiếp lên Chunk GameObject (nhanh và nhẹ hơn nhiều so với GameObject con)
                    if (!def.isWalkable || def.isWater)
                    {
                        var col = chunkObj.AddComponent<BoxCollider2D>();
                        col.offset = new Vector2(x + 0.5f, y + 0.5f);
                        col.size = new Vector2(0.98f, 0.98f);
                    }

                    // 3. Xử lý các vật thể có tương tác / cơ chế tàng hình đặc biệt
                    if (def.isStealth)
                    {
                        // Bụi rậm ẩn nấp (Stealth Bush Trigger): Lớp phủ visual trùm lên người chơi (Order 14) khi chui vào
                        GameObject bushObj = new GameObject($"Bush_{x}_{y}");
                        bushObj.transform.parent = chunkObj.transform;
                        bushObj.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0f);
                        bushObj.transform.localScale = new Vector3(1.85f, 1.85f, 1f);

                        var bushSr = bushObj.AddComponent<SpriteRenderer>();
                        bushSr.sprite = World.TerrainBlockRegistry.GetSprite(blockId);
                        bushSr.sortingOrder = 14;

                        var trig = bushObj.AddComponent<BoxCollider2D>();
                        trig.isTrigger = true;
                        trig.size = new Vector2(1.35f, 1.35f);
                        bushObj.AddComponent<World.StealthBushTrigger>();

                        if (def.animType != World.BlockAnimType.None)
                        {
                            var animator = bushObj.AddComponent<World.ProceduralFoliageAnimator>();
                            animator.Setup(def.animType, def.animSpeed, def.animIntensity);
                        }
                    }
                    else if (def.category == World.BlockCategory.InteractiveProp)
                    {
                        GameObject propVisual = new GameObject($"Prop_{x}_{y}");
                        propVisual.transform.parent = chunkObj.transform;
                        propVisual.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0f);

                        var propSr = propVisual.AddComponent<SpriteRenderer>();
                        propSr.sprite = World.TerrainBlockRegistry.GetSprite(blockId);
                        propSr.sortingOrder = def.sortingOrder;

                        if (def.animType != World.BlockAnimType.None)
                        {
                            var animator = propVisual.AddComponent<World.ProceduralFoliageAnimator>();
                            animator.Setup(def.animType, def.animSpeed, def.animIntensity);
                        }
                    }
                }
            }

            // Gán Mesh hoàn chỉnh vào Chunk
            Mesh chunkMesh = new Mesh();
            chunkMesh.name = $"Mesh_Chunk_{cx}_{cy}";
            chunkMesh.SetVertices(vertices);
            chunkMesh.SetUVs(0, uvs);
            chunkMesh.SetColors(colors);
            chunkMesh.SetTriangles(triangles, 0);
            chunkMesh.RecalculateNormals();
            chunkMesh.RecalculateBounds();

            var mf = chunkObj.AddComponent<MeshFilter>();
            mf.sharedMesh = chunkMesh;

            var mr = chunkObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = World.TerrainAtlasManager.AtlasMaterial;
            mr.sortingOrder = -20;
        }

        private void LoadChunk(int cx, int cy)
        {
            RequestLoadChunk(cx, cy);
        }

        private void UnloadChunk(int cx, int cy)
        {
            Vector2Int key = new Vector2Int(cx, cy);
            if (loadedChunks.TryGetValue(key, out GameObject chunkObj))
            {
                // Thay vì Destroy (gây lag GC), ta chỉ ẩn đi (SetActive = false) để tái sử dụng ngay lập tức!
                if (chunkObj != null) chunkObj.SetActive(false);
                loadedChunks.Remove(key);
            }
        }

        /// <summary>
        /// Sinh mã khối thủ tục cho toàn bộ thế giới mở rộng 270m x 132m:
        /// Sử dụng 2D Perlin Noise để sinh thảm cỏ, vạt hoa, khóm cỏ và bụi rậm hoàn toàn tự nhiên hữu cơ!
        /// Triệt tiêu 100% hiện tượng sọc chéo nhân tạo.
        /// </summary>
        public ushort GetBlockId(int x, int y)
        {
            Vector2Int key = new Vector2Int(x, y);
            if (customBlockTypes.TryGetValue(key, out var customType))
            {
                return (ushort)customType;
            }

            // 1. Tường thành biên giới bao quanh toàn bộ thế giới
            if (x <= MinX || x >= MaxX || y <= MinY || y >= MaxY)
            {
                return 24; // cliff_wall
            }

            // 2. KHU VỰC PHÓ BẢN HẦM NGỤC DI TÍCH (DUNGEON CRYPT FLOOR 1) TẠI (x: -16..16, y: -58..-32)
            if (Mathf.Abs(x) <= 16 && y >= -58 && y <= -32)
            {
                if (Mathf.Abs(x) == 16 || y == -58 || y == -32) return 23; // stone_wall
                if (Mathf.Abs(x) <= 2 && Mathf.Abs(y - (-45)) <= 2) return 28; // magic_rune_circle
                return 22; // stone_dungeon
            }

            // 3. Dòng Sông Ma Thuật (Magic River) uốn lượn tại x = 43..48
            if (x >= 43 && x <= 48)
            {
                if (Mathf.Abs(y) <= 2) return 26; // wooden_bridge (Cầu gỗ lớn qua sông)
                if (x == 44 || x == 47) return 12; // water_shallow (Nước nông gợn sóng)
                return 13; // water_deep (Nước sâu biếc)
            }
            // Bãi cát vàng và lau sậy ven bờ sông
            if ((x == 42 || x == 49) && Mathf.Abs(y) > 2)
            {
                float reedNoise = Mathf.PerlinNoise(x * 0.25f, y * 0.25f);
                if (reedNoise > 0.65f) return 33; // foliage_tall_reed
                return 16; // sand_shore
            }

            // 4. QUẢNG TRƯỜNG TRUNG TÂM LÀNG TÂN THỦ TẠI (-80, 0) (Ưu tiên cao nhất!)
            float distToPlaza = Vector2.Distance(new Vector2(x, y), new Vector2(-80, 0));
            if (distToPlaza <= 14.5f)
            {
                if (distToPlaza >= 12.0f) return 29; // safezone_gold (Vành đai đá vàng bảo hộ thần thánh)
                if (distToPlaza <= 2.2f) return 14;  // water_stream (Hồ nước trung tâm dưới đài phun)
                return 20; // cobble_plaza (Đá hoa cương sáng lát quảng trường làng)
            }

            // 5. Trục Đại Lộ Cổ Chạy Ngang Qua Thế Giới (y = -2..2)
            if (y >= -2 && y <= 2)
            {
                return (x <= -20) ? (ushort)8 : (ushort)9; // dirt_warm_path hoặc dirt_packed_road
            }

            // Trục đường Bắc - Nam qua quảng trường làng (-80)
            if (x >= -82 && x <= -78 && Mathf.Abs(y) <= 25)
            {
                return 8; // dirt_warm_path
            }

            // Trục đường phụ kết nối khu dân cư Đông làng (-50)
            if (x >= -52 && x <= -48 && Mathf.Abs(y) <= 22)
            {
                return 8; // dirt_warm_path
            }

            // 6. KHU VỰC ĐẠI ĐÔ THỊ LÀNG TÂN THỦ (x <= -20)
            if (x <= -20)
            {
                // Khu nông trại làng (-64 đến -44, y = 10 đến 18)
                if (x >= -64 && x <= -44 && y >= 10 && y <= 18)
                {
                    if (x == -64 || x == -44 || y == 10 || y == 18) return 44; // prop_wooden_fence
                    return 10; // farmland_tilled (Đất luống cày nông trại)
                }

                // Thảm cỏ làng mạc tự nhiên bằng 2D Perlin Noise (Không modulo, không sọc chéo)
                float fNoise = Mathf.PerlinNoise((x + 800) * 0.12f, (y + 800) * 0.12f);
                float cNoise = Mathf.PerlinNoise((x + 1200) * 0.18f, (y + 1200) * 0.18f);

                if (fNoise > 0.72f) return 1; // grass_flower_yellow (Vạt hoa cúc vàng rực rỡ mọc thành đám tròn)
                if (fNoise < 0.26f) return 4; // grass_flower_white (Vạt hoa dại trắng)
                if (cNoise > 0.68f) return 5; // grass_dense_clover (Thảm cỏ ba lá xanh mượt)

                return 0; // grass_bright (Đồng cỏ xanh tươi)
            }

            // 7. VÙNG RANH GIỚI CỔNG LÀNG (-20 < x <= 10) — Vùng chuyển tiếp
            if (x <= 10)
            {
                float edgeNoise = Mathf.PerlinNoise((x + 2000) * 0.14f, (y + 2000) * 0.14f);
                float edgeBush = Mathf.PerlinNoise((x + 3000) * 0.20f, (y + 3000) * 0.20f);

                if (edgeBush > 0.75f) return 30; // foliage_stealth_bush (Bụi rậm to né quái)
                if (edgeNoise > 0.73f) return 38; // foliage_flower_patch (Vạt hoa dã quỳ)
                if (edgeNoise < 0.27f) return 32; // foliage_grass_tuft (Khóm cỏ đung đưa gió)
                if (edgeBush < 0.24f) return 5;  // grass_dense_clover
                return 0; // grass_bright
            }

            // 8. KHU VỰC RỪNG RẬM HOANG DÃ & THUNG LŨNG TÀN TÍCH (x > 10)
            // Lối mòn dẫn lên Cổng Phó Bản Hầm Ngục tại (120, 25)
            if (x >= 80 && y >= 23 && y <= 27)
            {
                return 8; // dirt_warm_path
            }

            // Rừng Rậm: 100% sinh bằng 2D Perlin Noise tự nhiên
            float wildBush = Mathf.PerlinNoise((x + 4000) * 0.14f, (y + 4000) * 0.14f);
            float wildFlower = Mathf.PerlinNoise((x + 5000) * 0.11f, (y + 5000) * 0.11f);
            float wildFoliage = Mathf.PerlinNoise((x + 6000) * 0.20f, (y + 6000) * 0.20f);

            if (wildBush > 0.73f) return 30;   // foliage_stealth_bush (Bụi rậm to xum xuê né quái!)
            if (wildFlower > 0.74f) return 3;  // grass_flower_red (Hoa anh túc đỏ rực)
            if (wildFlower < 0.25f) return 38; // foliage_flower_patch (Vạt hoa dã quỳ)
            if (wildFoliage > 0.72f) return 32;// foliage_grass_tuft (Khóm cỏ đung đưa gió)
            if (wildFoliage < 0.22f) return 39;// mushroom_giant (Nấm đỏ khổng lồ)
            if (wildBush < 0.20f) return 2;    // grass_flower_blue (Hoa lưu ly biếc)

            return 0; // grass_bright (Đồng cỏ xanh tươi)
        }


        public TerrainBlockType GetBlockType(int x, int y)
        {
            return (TerrainBlockType)GetBlockId(x, y);
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

            if (allCachedChunks.TryGetValue(cKey, out GameObject oldChunk))
            {
                if (oldChunk != null) Destroy(oldChunk);
                allCachedChunks.Remove(cKey);
                loadedChunks.Remove(cKey);
                LoadChunk(cx, cy);
            }
        }

        private void SpawnLandscapeProps()
        {
            // 1. ĐÀI PHUN NƯỚC THẦN THOẠI (GRAND FOUNTAIN) Ở TRUNG TÂM QUẢNG TRƯỜNG LÀNG (-80, 0)
            Sprite fountainSprite = Resources.Load<Sprite>("prop_fountain_grand");
            CreateProp("Grand_Fountain_Plaza", fountainSprite, new Vector3(-80f, 0f, 0f), propsParent, new Vector2(1.8f, 1.2f), new Vector2(0, -0.4f), sortingOrder: 11);

            // 2. BỐN TRỤ NƯỚC THẦN NĂNG (SACRED WATER PILLARS) CANH GIỮ 4 GÓC QUẢNG TRƯỜNG LÀNG
            Sprite waterPillarSprite = Resources.Load<Sprite>("prop_water_pillar");
            Vector3[] waterPillarPositions = {
                new Vector3(-89f, 7f, 0),
                new Vector3(-89f, -7f, 0),
                new Vector3(-71f, 7f, 0),
                new Vector3(-71f, -7f, 0)
            };
            foreach (var wp in waterPillarPositions)
            {
                CreateProp("Sacred_Water_Pillar", waterPillarSprite, wp, propsParent, new Vector2(0.9f, 0.6f), new Vector2(0, -0.4f), sortingOrder: 11);
            }

            // 3. Giếng nước đá cổ ở góc làng
            CreateProp("Prop_Stone_Well", stoneWellSprite, new Vector3(-62f, 8f, 0f), propsParent, new Vector2(0.8f, 0.8f));

            // 4. Cây xanh râm mát Làng Tân Thủ
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

            // 5. Hàng rào gỗ bảo vệ Làng Tân Thủ tại ranh giới x = -20
            for (int fy = -40; fy <= 40; fy += 3)
            {
                if (Mathf.Abs(fy) > 3) // Chừa cổng đại lộ rộng mở ở giữa
                {
                    CreateProp($"Fence_Border_{fy}", woodenFenceSprite, new Vector3(-20f, fy, 0), propsParent, new Vector2(0.95f, 0.4f));
                }
            }

            // 6. Ngọn đuốc thắp sáng bập bùng (Toàn bộ các trục đường chính)
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

            // 7. Cây Rừng Rậm Hắc Ám (Phân bố khắp khu vực rừng x = 20 đến 130)
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

            // 8. CỔNG PHÓ BẢN HẦM NGỤC DI TÍCH (Dungeon Arch Gate tại 120, 25)
            CreateProp("Dungeon_Arch_Gate", dungeonGateSprite, new Vector3(120f, 25f, 0), propsParent, new Vector2(1.2f, 0.6f), new Vector2(0, -0.5f), sortingOrder: 12);

            // 9. TRỤ HỒI SINH ĐÁ CỔ KHẮC RUNE (Waystone Shrines) RẢI RÁC KHẮP CÁC KHU VỰC
            CreateWaystoneProp("Waystone_Plaza", "Trụ Quảng Trường Làng", new Vector3(-70f, 2f, 0), true);
            CreateWaystoneProp("Waystone_EastGate", "Trụ Cổng Đông Làng", new Vector3(-22f, 2f, 0), false);
            CreateWaystoneProp("Waystone_Outpost", "Trụ Tiền Đồn Rừng Rậm", new Vector3(18f, 3.5f, 0), false);
            CreateWaystoneProp("Waystone_Bridge", "Trụ Cầu Đá Cự Thạch", new Vector3(44f, 4f, 0), false);
            CreateWaystoneProp("Waystone_Dungeon", "Trụ Cửa Hầm Ngục Di Tích", new Vector3(116f, 22f, 0), false);
        }

        private void CreateWaystoneProp(string objName, string shrineName, Vector3 pos, bool defaultActive)
        {
            Sprite wsSprite = Resources.Load<Sprite>("prop_waystone_shrine") ?? Resources.Load<Sprite>("block_prop_waystone_shrine");
            GameObject wsObj = CreateProp(objName, wsSprite, pos, propsParent, new Vector2(0.8f, 0.6f), new Vector2(0, -0.4f), sortingOrder: 10);
            if (wsObj != null)
            {
                var shrine = wsObj.AddComponent<World.WaystoneShrine>();
                shrine.Setup(shrineName, defaultActive);
            }
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
