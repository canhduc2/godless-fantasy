using System;
using System.Collections.Generic;
using UnityEngine;

namespace GodlessFantasy.World
{
    public enum BlockCategory
    {
        Ground = 0,         // Nền đất, cỏ đi lại bình thường
        Liquid = 1,         // Nước, đầm lầy (chặn di chuyển hoặc làm chậm)
        Obstacle = 2,       // Tường, vách đá, chướng ngại vật cản đường
        Foliage = 3,        // Cỏ dại, hoa, lau sậy (đi xuyên qua được, có hiệu ứng đung đưa)
        StealthBush = 4,    // Bụi rậm ẩn nấp (chui vào tàng hình, quái mất dấu)
        InteractiveProp = 5 // Đuốc, Trụ Hồi Sinh, Cổng Hầm Ngục
    }

    public enum BlockAnimType
    {
        None = 0,
        WindSway = 1,     // Đung đưa nhẹ nhàng trong gió (Cỏ, hoa, cành lá, bụi rậm)
        WaterWave = 2,    // Gợn sóng bọt nước lấp lánh (Nước, thác nước)
        FlamePulse = 3,   // Lửa đuốc chập chờn
        CrystalPulse = 4  // Tinh thể ma thuật lơ lửng phát sáng
    }

    [Serializable]
    public class TerrainBlockDef
    {
        public ushort id;
        public string codeName;
        public string displayName;
        public BlockCategory category;
        public string spriteResourcePath;
        public bool isWalkable;
        public bool isWater;
        public bool isStealth;
        public BlockAnimType animType;
        public float animSpeed;
        public float animIntensity;
        public Color minimapColor;
        public int sortingOrder;

        [NonSerialized] public Sprite cachedSprite;
    }

    /// <summary>
    /// Bảng Đăng Ký Khối Địa Hình Toàn Cầu (Scaleable Tile & Block Registry Engine):
    /// - Quản lý hơn 48 - 100 loại khối địa hình khác nhau trong thế giới Godless Fantasy.
    /// - Mỗi khối được định danh duy nhất bằng mã khối (ushort blockId), không hardcode chuỗi ký tự.
    /// - Cung cấp metadata đầy đủ: Độ cản đường, thuộc tính nước, ẩn nấp trong bụi rậm, kiểu hoạt ảnh (WindSway, WaterWave, FlamePulse, CrystalPulse).
    /// </summary>
    public static class TerrainBlockRegistry
    {
        private static Dictionary<ushort, TerrainBlockDef> blockDefs = new Dictionary<ushort, TerrainBlockDef>();
        private static Dictionary<string, ushort> codeToId = new Dictionary<string, ushort>();
        private static bool isInitialized = false;

        public static void EnsureInitialized()
        {
            if (isInitialized) return;
            InitializeRegistry();
            isInitialized = true;
        }

        private static void Register(ushort id, string code, string name, BlockCategory cat, string path, 
            bool walkable, bool water, bool stealth, BlockAnimType anim, float speed, float intensity, Color mapCol, int sortOrder = -10)
        {
            var def = new TerrainBlockDef
            {
                id = id,
                codeName = code,
                displayName = name,
                category = cat,
                spriteResourcePath = path,
                isWalkable = walkable,
                isWater = water,
                isStealth = stealth,
                animType = anim,
                animSpeed = speed,
                animIntensity = intensity,
                minimapColor = mapCol,
                sortingOrder = sortOrder
            };

            blockDefs[id] = def;
            codeToId[code] = id;
        }

        private static void InitializeRegistry()
        {
            blockDefs.Clear();
            codeToId.Clear();

            // -------------------------------------------------------------
            // NHÓM 1: HỆ ĐỒNG CỎ & ĐẤT (IDs 0..11)
            // -------------------------------------------------------------
            Register(0,  "grass_bright",            "Đồng Cỏ Xanh Tươi",      BlockCategory.Ground, "block_grass_bright",            true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.44f, 0.80f, 0.28f));
            Register(1,  "grass_flower_yellow",     "Cỏ Hoa Cúc Vàng",        BlockCategory.Ground, "block_grass_flower_yellow",     true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.55f, 0.85f, 0.25f));
            Register(2,  "grass_flower_blue",       "Cỏ Hoa Lưu Ly Xanh",     BlockCategory.Ground, "block_grass_flower_blue",       true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.40f, 0.78f, 0.45f));
            Register(3,  "grass_flower_red",        "Cỏ Hoa Anh Túc Đỏ",      BlockCategory.Ground, "block_grass_flower_red",        true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.50f, 0.75f, 0.30f));
            Register(4,  "grass_flower_white",      "Cỏ Hoa Dại Trắng",       BlockCategory.Ground, "block_grass_flower_white",      true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.52f, 0.82f, 0.35f));
            Register(5,  "grass_dense_clover",      "Cỏ Rậm Ba Lá",           BlockCategory.Ground, "block_grass_dense_clover",      true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.32f, 0.67f, 0.20f));
            Register(6,  "grass_autumn",            "Đồng Cỏ Thu Úa",         BlockCategory.Ground, "block_grass_autumn",            true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.76f, 0.63f, 0.25f));
            Register(7,  "grass_dark_forest",       "Thảm Cỏ Rừng Sâu",       BlockCategory.Ground, "block_grass_dark_forest",       true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.14f, 0.37f, 0.22f));
            Register(8,  "dirt_warm_path",          "Lối Mòn Đất Nện Ấm",     BlockCategory.Ground, "block_dirt_warm_path",          true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.83f, 0.70f, 0.50f));
            Register(9,  "dirt_packed_road",        "Đại Lộ Đất Đá Sỏi",      BlockCategory.Ground, "block_dirt_packed_road",        true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.68f, 0.57f, 0.43f));
            Register(10, "farmland_tilled",         "Đất Cày Nông Trại",      BlockCategory.Ground, "block_farmland_tilled",         true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.47f, 0.33f, 0.22f));
            Register(11, "mud_swamp",               "Đất Bùn Đầm Lầy",        BlockCategory.Ground, "block_mud_swamp",               true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.35f, 0.29f, 0.22f));

            // -------------------------------------------------------------
            // NHÓM 2: HỆ NƯỚC, SÔNG, BIỂN & BỜ CÁT (IDs 12..19)
            // -------------------------------------------------------------
            Register(12, "water_shallow",           "Nước Nông Gợn Sóng",     BlockCategory.Liquid, "block_water_shallow",           false, true,  false, BlockAnimType.WaterWave,    1.2f, 0.15f, new Color(0.22f, 0.72f, 0.92f), -9);
            Register(13, "water_deep",              "Nước Sâu Biển Hồ",       BlockCategory.Liquid, "block_water_deep",              false, true,  false, BlockAnimType.WaterWave,    0.8f, 0.10f, new Color(0.10f, 0.41f, 0.72f), -9);
            Register(14, "water_stream",            "Dòng Suối Chảy Xiết",    BlockCategory.Liquid, "block_water_stream",            false, true,  false, BlockAnimType.WaterWave,    2.0f, 0.20f, new Color(0.30f, 0.78f, 0.95f), -9);
            Register(15, "waterfall",               "Thác Nước Bọt Trắng",    BlockCategory.Liquid, "block_waterfall",               false, true,  false, BlockAnimType.WaterWave,    3.0f, 0.35f, new Color(0.80f, 0.95f, 1.0f),  -8);
            Register(16, "sand_shore",              "Bãi Cát Vàng Ven Hồ",    BlockCategory.Ground, "block_sand_shore",              true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.94f, 0.85f, 0.58f));
            Register(17, "sand_wet",                "Bãi Cát Ẩm Ven Sóng",    BlockCategory.Ground, "block_sand_wet",                true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.76f, 0.67f, 0.45f));
            Register(18, "ice_frozen",              "Băng Tuyết Đóng Băng",   BlockCategory.Ground, "block_ice_frozen",              true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.68f, 0.88f, 0.96f));
            Register(19, "poison_swamp",            "Đầm Nước Độc Xanh Lá",   BlockCategory.Liquid, "block_poison_swamp",            false, true,  false, BlockAnimType.WaterWave,    1.0f, 0.15f, new Color(0.18f, 0.43f, 0.18f), -9);

            // -------------------------------------------------------------
            // NHÓM 3: HỆ ĐÁ, TƯỜNG, HẦM NGỤC & ĐƯỜNG PHỐ (IDs 20..29)
            // -------------------------------------------------------------
            Register(20, "cobble_plaza",            "Đá Hoa Cương Lát Làng",  BlockCategory.Ground, "block_cobble_plaza",            true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.65f, 0.68f, 0.75f));
            Register(21, "cobble_mossy",            "Đá Lát Rêu Phong",       BlockCategory.Ground, "block_cobble_mossy",            true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.55f, 0.65f, 0.50f));
            Register(22, "stone_dungeon",           "Sàn Đá Hầm Ngục Crypt",  BlockCategory.Ground, "block_stone_dungeon",           true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.33f, 0.29f, 0.37f));
            Register(23, "stone_wall",              "Tường Gạch Đá Kiên Cố",  BlockCategory.Obstacle,"block_stone_wall",             false, false, false, BlockAnimType.None,     0f,   0f,    new Color(0.45f, 0.43f, 0.47f), 5);
            Register(24, "cliff_wall",              "Vách Đá Núi Cao",        BlockCategory.Obstacle,"block_cliff_wall",             false, false, false, BlockAnimType.None,     0f,   0f,    new Color(0.43f, 0.37f, 0.31f), 5);
            Register(25, "wooden_plank",            "Sàn Gỗ Lát Nhà",         BlockCategory.Ground, "block_wooden_plank",            true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.65f, 0.45f, 0.27f));
            Register(26, "wooden_bridge",           "Cầu Gỗ Bắc Qua Sông",    BlockCategory.Ground, "block_wooden_bridge",           true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.55f, 0.38f, 0.22f), -8);
            Register(27, "ruins_pillar",            "Trụ Cột Tàn Tích Cổ",    BlockCategory.Obstacle,"block_ruins_pillar",           false, false, false, BlockAnimType.None,     0f,   0f,    new Color(0.57f, 0.59f, 0.65f), 4);
            Register(28, "magic_rune_circle",       "Trận Pháp Ma Thuật",     BlockCategory.Ground, "block_magic_rune_circle",       true,  false, false, BlockAnimType.CrystalPulse, 1.5f, 0.2f, new Color(0.71f, 0.37f, 1.0f));
            Register(29, "safezone_gold",           "Đá Thiêng Vùng An Toàn", BlockCategory.Ground, "block_safezone_gold",           true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(1.0f,  0.84f, 0.25f));

            // -------------------------------------------------------------
            // NHÓM 4: THỰC VẬT & BỤI RẬM & ANIMATED FOLIAGE (IDs 30..39)
            // -------------------------------------------------------------
            Register(30, "foliage_stealth_bush",    "Bụi Rậm Ẩn Nấp (Tàng Hình)", BlockCategory.StealthBush, "block_foliage_stealth_bush", true, false, true, BlockAnimType.WindSway, 1.5f, 0.08f, new Color(0.19f, 0.61f, 0.23f), 8);
            Register(31, "foliage_bush_berries",    "Bụi Cây Quả Ngọt",       BlockCategory.Foliage, "block_foliage_bush_berries",   true,  false, false, BlockAnimType.WindSway, 1.4f, 0.07f, new Color(0.70f, 0.25f, 0.30f), 8);
            Register(32, "foliage_grass_tuft",      "Khóm Cỏ Dại Đung Đưa",   BlockCategory.Foliage, "block_foliage_grass_tuft",     true,  false, false, BlockAnimType.WindSway, 2.2f, 0.12f, new Color(0.43f, 0.86f, 0.29f), 6);
            Register(33, "foliage_tall_reed",       "Bụi Lau Sậy Bờ Sông",    BlockCategory.Foliage, "block_foliage_tall_reed",      true,  false, false, BlockAnimType.WindSway, 2.5f, 0.15f, new Color(0.53f, 0.69f, 0.29f), 7);
            Register(34, "tree_oak",                "Cây Sồi Xum Xuê",        BlockCategory.Obstacle,"block_tree_oak",               false, false, false, BlockAnimType.WindSway, 0.9f, 0.04f, new Color(0.22f, 0.63f, 0.27f), 10);
            Register(35, "tree_apple",              "Cây Táo Làng Thôn",      BlockCategory.Obstacle,"block_tree_apple",             false, false, false, BlockAnimType.WindSway, 0.9f, 0.04f, new Color(0.75f, 0.20f, 0.25f), 10);
            Register(36, "tree_pine",               "Cây Thông Lá Kim",       BlockCategory.Obstacle,"block_tree_pine",              false, false, false, BlockAnimType.WindSway, 1.1f, 0.05f, new Color(0.12f, 0.37f, 0.25f), 10);
            Register(37, "tree_dark",               "Cây Hắc Ám Quỷ Dị",      BlockCategory.Obstacle,"block_tree_dark",              false, false, false, BlockAnimType.WindSway, 0.8f, 0.04f, new Color(0.29f, 0.14f, 0.41f), 10);
            Register(38, "foliage_flower_patch",    "Vạt Hoa Dã Quỳ",         BlockCategory.Foliage, "block_foliage_flower_patch",   true,  false, false, BlockAnimType.WindSway, 1.8f, 0.09f, new Color(1.0f,  0.88f, 0.18f), 6);
            Register(39, "mushroom_giant",          "Nấm Đỏ Khổng Lồ",        BlockCategory.Foliage, "block_mushroom_giant",         true,  false, false, BlockAnimType.WindSway, 1.2f, 0.06f, new Color(0.88f, 0.18f, 0.22f), 7);

            // -------------------------------------------------------------
            // NHÓM 5: ĐẠO CỤ & CHƯỚNG NGẠI VẬT & TƯƠNG TÁC (IDs 40..47)
            // -------------------------------------------------------------
            Register(40, "prop_mossy_boulder",      "Tảng Đá Cuội Rêu Phong", BlockCategory.Obstacle,"block_prop_mossy_boulder",     false, false, false, BlockAnimType.None,     0f,   0f,    new Color(0.53f, 0.57f, 0.63f), 4);
            Register(41, "prop_pebble_cluster",     "Cụm Sỏi Đá Nhỏ",         BlockCategory.Foliage, "block_prop_pebble_cluster",    true,  false, false, BlockAnimType.None,     0f,   0f,    new Color(0.59f, 0.61f, 0.65f), 2);
            Register(42, "prop_fallen_log",         "Khúc Gỗ Mục Có Rêu",     BlockCategory.Obstacle,"block_prop_fallen_log",        false, false, false, BlockAnimType.None,     0f,   0f,    new Color(0.41f, 0.25f, 0.16f), 4);
            Register(43, "prop_tree_stump",         "Gốc Cây Cổ Thụ",         BlockCategory.Obstacle,"block_prop_tree_stump",        false, false, false, BlockAnimType.None,     0f,   0f,    new Color(0.35f, 0.22f, 0.12f), 4);
            Register(44, "prop_wooden_fence",       "Hàng Rào Gỗ",            BlockCategory.Obstacle,"block_prop_wooden_fence",      false, false, false, BlockAnimType.None,     0f,   0f,    new Color(0.57f, 0.37f, 0.22f), 5);
            Register(45, "prop_torch_post",         "Trụ Đuốc Phát Sáng",     BlockCategory.InteractiveProp, "block_prop_torch_post",false, false, false, BlockAnimType.FlamePulse, 5.0f, 0.15f, new Color(1.0f,  0.73f, 0.16f), 9);
            Register(46, "prop_waystone_shrine",    "Trụ Hồi Sinh Đá Cổ",     BlockCategory.InteractiveProp, "block_prop_waystone_shrine",false, false, false, BlockAnimType.CrystalPulse, 1.8f, 0.25f, new Color(0.27f, 0.90f, 1.0f), 9);
            Register(47, "prop_dungeon_gate",       "Cổng Hầm Ngục Cổ Đại",   BlockCategory.InteractiveProp, "block_prop_dungeon_gate",false, false, false, BlockAnimType.CrystalPulse, 2.5f, 0.30f, new Color(0.71f, 0.33f, 0.96f), 9);
        }

        public static TerrainBlockDef GetBlock(ushort id)
        {
            EnsureInitialized();
            if (blockDefs.TryGetValue(id, out var def)) return def;
            return blockDefs[0]; // Mặc định là grass_bright
        }

        public static ushort GetId(string codeName)
        {
            EnsureInitialized();
            if (codeToId.TryGetValue(codeName, out var id)) return id;
            return 0;
        }

        public static Sprite GetSprite(ushort id)
        {
            var def = GetBlock(id);
            if (def.cachedSprite != null) return def.cachedSprite;

            def.cachedSprite = Resources.Load<Sprite>(def.spriteResourcePath);
            if (def.cachedSprite == null)
            {
                // Fallback nạp theo tên trực tiếp trong Resources
                def.cachedSprite = Resources.Load<Sprite>("Blocks/" + def.spriteResourcePath);
            }
            if (def.cachedSprite == null)
            {
                def.cachedSprite = Resources.Load<Sprite>("tile_grass");
            }
            return def.cachedSprite;
        }
    }
}
