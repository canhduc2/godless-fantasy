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
            bool walkable, bool water, bool stealth, BlockAnimType anim, float speed, float intensity, Color mapCol, int sortOrder = -999)
        {
            // Tự động phân cấp độ ưu tiên hiển thị (Sorting Layer / Depth Order) nếu không chỉ định rõ:
            if (sortOrder == -999)
            {
                if (water || cat == BlockCategory.Liquid)
                    sortOrder = -25; // Nước sâu, hồ nước nằm dưới cùng
                else if (stealth || cat == BlockCategory.StealthBush)
                    sortOrder = 5;   // Bụi rậm ẩn nấp nằm TRÊN bàn chân/thân dưới người chơi
                else if (cat == BlockCategory.Obstacle)
                    sortOrder = 10;  // Tường đá, cây cối, tảng đá cản đường
                else if (cat == BlockCategory.Foliage)
                    sortOrder = -5;  // Hoa cỏ dại, lau sậy, sỏi bẹp
                else if (code.Contains("safezone") || code.Contains("gold") || code.Contains("rune") || code.Contains("farmland"))
                    sortOrder = -10; // Lớp phủ hoa văn, luống đất cày, viền đá vàng bảo hộ
                else if (code.Contains("plaza") || code.Contains("bridge") || code.Contains("road") || code.Contains("path") || code.Contains("stone") || code.Contains("plank") || code.Contains("cobble"))
                    sortOrder = -15; // Đường đi, vỉa hè đá lát, cầu gỗ (LUÔN NẰM TRÊN NỀN CỎ)
                else
                    sortOrder = -20; // Nền cỏ, bùn thô, bãi cát cơ bản
            }

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

            // -------------------------------------------------------------
            // NHÓM 6: CHUYỂN TIẾP ĐỊA HÌNH (TRANSITION TILES) (IDs 48..57)
            // -------------------------------------------------------------
            Register(48, "trans_grass_to_dirt_n",   "Chuyển Cỏ→Đất (Bắc)",   BlockCategory.Ground, "block_trans_grass_dirt_n",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.52f, 0.73f, 0.38f));
            Register(49, "trans_grass_to_dirt_s",   "Chuyển Cỏ→Đất (Nam)",   BlockCategory.Ground, "block_trans_grass_dirt_s",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.52f, 0.73f, 0.38f));
            Register(50, "trans_grass_to_dirt_e",   "Chuyển Cỏ→Đất (Đông)",  BlockCategory.Ground, "block_trans_grass_dirt_e",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.54f, 0.74f, 0.36f));
            Register(51, "trans_grass_to_dirt_w",   "Chuyển Cỏ→Đất (Tây)",   BlockCategory.Ground, "block_trans_grass_dirt_w",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.54f, 0.74f, 0.36f));
            Register(52, "trans_grass_to_water",    "Bờ Cỏ Ven Nước",        BlockCategory.Ground, "block_trans_grass_water",      true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.38f, 0.72f, 0.52f));
            Register(53, "trans_stone_to_grass",    "Đá Rêu Chuyển Tiếp",    BlockCategory.Ground, "block_trans_stone_grass",      true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.54f, 0.65f, 0.48f));
            Register(54, "trans_sand_to_grass",     "Cát Xen Cỏ",            BlockCategory.Ground, "block_trans_sand_grass",       true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.78f, 0.80f, 0.48f));
            Register(55, "trans_dark_to_light",     "Rừng Tối Chuyển Sáng",  BlockCategory.Ground, "block_trans_dark_light",       true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.28f, 0.52f, 0.30f));
            Register(56, "trans_cobble_to_grass",   "Đá Lát Ven Cỏ",         BlockCategory.Ground, "block_trans_cobble_grass",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.59f, 0.66f, 0.55f));
            Register(57, "trans_dirt_to_mud",       "Đất Chuyển Sang Bùn",   BlockCategory.Ground, "block_trans_dirt_mud",         true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.43f, 0.35f, 0.26f));

            // -------------------------------------------------------------
            // NHÓM 7: KIẾN TRÚC LÀNG & LÂU ĐÀI (VILLAGE ARCHITECTURE) (IDs 58..67)
            // -------------------------------------------------------------
            Register(58, "arch_tile_red_roof",      "Mái Ngói Đỏ Làng",      BlockCategory.Ground, "block_arch_red_roof",          true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.82f, 0.32f, 0.24f));
            Register(59, "arch_tile_blue_roof",     "Mái Ngói Xanh Đền",     BlockCategory.Ground, "block_arch_blue_roof",         true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.28f, 0.45f, 0.75f));
            Register(60, "arch_marble_floor",       "Sàn Đại Lý Đá Cẩm",    BlockCategory.Ground, "block_arch_marble_floor",      true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.90f, 0.90f, 0.92f));
            Register(61, "arch_brick_wall",         "Tường Gạch Đỏ Nung",    BlockCategory.Obstacle,"block_arch_brick_wall",       false, false, false, BlockAnimType.None, 0f, 0f, new Color(0.72f, 0.36f, 0.24f), 5);
            Register(62, "arch_stone_floor_fancy",  "Sàn Đá Khắc Hoa Văn",  BlockCategory.Ground, "block_arch_stone_floor_fancy", true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.70f, 0.70f, 0.75f));
            Register(63, "arch_wood_wall",          "Tường Gỗ Nhà Làng",     BlockCategory.Obstacle,"block_arch_wood_wall",        false, false, false, BlockAnimType.None, 0f, 0f, new Color(0.55f, 0.38f, 0.20f), 5);
            Register(64, "arch_castle_floor",       "Sàn Lâu Đài Hoàng Gia", BlockCategory.Ground, "block_arch_castle_floor",      true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.60f, 0.65f, 0.72f));
            Register(65, "arch_dungeon_crack",      "Sàn Hầm Ngục Nứt Vỡ",  BlockCategory.Ground, "block_arch_dungeon_crack",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.26f, 0.22f, 0.30f));
            Register(66, "arch_lava_floor",         "Sàn Nham Thạch Nóng",   BlockCategory.Liquid, "block_arch_lava_floor",        false, true,  false, BlockAnimType.WaterWave, 1.5f, 0.20f, new Color(0.95f, 0.38f, 0.08f), -9);
            Register(67, "arch_roof_thatch",        "Mái Tranh Nông Thôn",   BlockCategory.Ground, "block_arch_roof_thatch",       true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.80f, 0.72f, 0.35f));

            // -------------------------------------------------------------
            // NHÓM 8: MA PHÁP & TINH THỂ PHÁT SÁNG (MAGIC & CRYSTAL) (IDs 68..77)
            // -------------------------------------------------------------
            Register(68, "magic_void_floor",        "Sàn Hư Không Tím Thẫm", BlockCategory.Ground, "block_magic_void_floor",       true,  false, false, BlockAnimType.CrystalPulse, 1.0f, 0.15f, new Color(0.30f, 0.10f, 0.50f));
            Register(69, "magic_star_field",        "Sàn Sao Đêm Bầu Trời",  BlockCategory.Ground, "block_magic_star_field",       true,  false, false, BlockAnimType.CrystalPulse, 0.8f, 0.20f, new Color(0.08f, 0.12f, 0.45f));
            Register(70, "magic_frost_rune",        "Trận Pháp Băng Giá",    BlockCategory.Ground, "block_magic_frost_rune",       true,  false, false, BlockAnimType.CrystalPulse, 1.2f, 0.18f, new Color(0.55f, 0.85f, 1.0f));
            Register(71, "magic_fire_rune",         "Trận Pháp Hỏa Diễm",   BlockCategory.Ground, "block_magic_fire_rune",        true,  false, false, BlockAnimType.FlamePulse,   2.0f, 0.25f, new Color(1.0f,  0.40f, 0.10f));
            Register(72, "magic_lightning_rune",    "Trận Pháp Lôi Điện",   BlockCategory.Ground, "block_magic_lightning_rune",   true,  false, false, BlockAnimType.CrystalPulse, 2.5f, 0.30f, new Color(0.90f, 0.95f, 0.25f));
            Register(73, "magic_crystal_blue",      "Cột Tinh Thể Xanh Lam", BlockCategory.Obstacle,"block_magic_crystal_blue",    false, false, false, BlockAnimType.CrystalPulse, 1.5f, 0.25f, new Color(0.20f, 0.70f, 1.0f), 8);
            Register(74, "magic_crystal_red",       "Cột Tinh Thể Đỏ Lửa",  BlockCategory.Obstacle,"block_magic_crystal_red",     false, false, false, BlockAnimType.CrystalPulse, 1.5f, 0.25f, new Color(1.0f,  0.25f, 0.25f), 8);
            Register(75, "magic_crystal_green",     "Cột Tinh Thể Xanh Lá", BlockCategory.Obstacle,"block_magic_crystal_green",   false, false, false, BlockAnimType.CrystalPulse, 1.5f, 0.25f, new Color(0.20f, 0.95f, 0.40f), 8);
            Register(76, "magic_altar",             "Bệ Thờ Ma Pháp Cổ",    BlockCategory.InteractiveProp,"block_magic_altar",    false, false, false, BlockAnimType.CrystalPulse, 1.0f, 0.20f, new Color(0.70f, 0.50f, 1.0f), 9);
            Register(77, "magic_portal_ring",       "Cổng Dịch Chuyển Ma Pháp", BlockCategory.InteractiveProp,"block_magic_portal_ring", false, false, false, BlockAnimType.CrystalPulse, 3.0f, 0.40f, new Color(0.50f, 1.0f, 0.80f), 10);

            // -------------------------------------------------------------
            // NHÓM 9: SA MẠC, TUYẾT & KHÍ HẬU ĐẶC BIỆT (IDs 78..87)
            // -------------------------------------------------------------
            Register(78, "desert_sand_bright",      "Cát Sa Mạc Vàng Rực",  BlockCategory.Ground, "block_desert_sand_bright",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.96f, 0.88f, 0.58f));
            Register(79, "desert_sand_dune",        "Cồn Cát Sa Mạc",       BlockCategory.Ground, "block_desert_sand_dune",       true,  false, false, BlockAnimType.WindSway, 0.5f, 0.03f, new Color(0.90f, 0.80f, 0.48f));
            Register(80, "desert_cracked_earth",    "Đất Nứt Nẻ Khô Hạn",  BlockCategory.Ground, "block_desert_cracked_earth",   true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.72f, 0.58f, 0.38f));
            Register(81, "desert_cactus",           "Cây Xương Rồng Gai",   BlockCategory.Obstacle,"block_desert_cactus",         false, false, false, BlockAnimType.WindSway, 0.6f, 0.03f, new Color(0.38f, 0.65f, 0.28f), 8);
            Register(82, "desert_oasis_water",      "Ốc Đảo Nước Trong",   BlockCategory.Liquid, "block_desert_oasis_water",     false, true,  false, BlockAnimType.WaterWave, 1.0f, 0.12f, new Color(0.30f, 0.80f, 0.88f), -9);
            Register(83, "snow_white",              "Tuyết Trắng Tinh Khiết", BlockCategory.Ground, "block_snow_white",            true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.94f, 0.96f, 1.0f));
            Register(84, "snow_packed_path",        "Lối Mòn Tuyết Nện",    BlockCategory.Ground, "block_snow_packed_path",       true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.82f, 0.86f, 0.92f));
            Register(85, "snow_frozen_lake",        "Hồ Đóng Băng Mùa Đông", BlockCategory.Ground, "block_snow_frozen_lake",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.72f, 0.88f, 0.96f));
            Register(86, "snow_pine_tree",          "Cây Thông Phủ Tuyết",  BlockCategory.Obstacle,"block_snow_pine_tree",        false, false, false, BlockAnimType.WindSway, 0.7f, 0.04f, new Color(0.55f, 0.75f, 0.55f), 10);
            Register(87, "volcanic_basalt",         "Đá Bazan Núi Lửa",     BlockCategory.Ground, "block_volcanic_basalt",        true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.22f, 0.18f, 0.18f));

            // -------------------------------------------------------------
            // NHÓM 10: TRANG TRÍ ĐẶC BIỆT & BIỂU TƯỢNG (IDs 88..99)
            // -------------------------------------------------------------
            Register(88, "deco_checker_dark",       "Ô Cờ Vua Tối",         BlockCategory.Ground, "block_deco_checker_dark",      true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.20f, 0.20f, 0.25f));
            Register(89, "deco_checker_light",      "Ô Cờ Vua Sáng",        BlockCategory.Ground, "block_deco_checker_light",     true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.85f, 0.85f, 0.88f));
            Register(90, "deco_mosaic_gold",        "Tranh Khảm Vàng Cổ",   BlockCategory.Ground, "block_deco_mosaic_gold",       true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.92f, 0.78f, 0.28f));
            Register(91, "deco_vine_covered",       "Đá Phủ Dây Leo",        BlockCategory.Foliage, "block_deco_vine_covered",    true,  false, false, BlockAnimType.WindSway, 1.0f, 0.05f, new Color(0.32f, 0.62f, 0.30f), 6);
            Register(92, "deco_flower_garden",      "Vườn Hoa Phong Phú",   BlockCategory.Foliage, "block_deco_flower_garden",    true,  false, false, BlockAnimType.WindSway, 1.6f, 0.08f, new Color(0.88f, 0.55f, 0.78f), 6);
            Register(93, "deco_ancient_symbol",     "Sàn Ký Hiệu Cổ Đại",   BlockCategory.Ground, "block_deco_ancient_symbol",   true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.60f, 0.55f, 0.40f));
            Register(94, "deco_red_carpet",         "Thảm Đỏ Hoàng Gia",    BlockCategory.Ground, "block_deco_red_carpet",        true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.78f, 0.18f, 0.20f));
            Register(95, "deco_blue_carpet",        "Thảm Xanh Ma Pháp",    BlockCategory.Ground, "block_deco_blue_carpet",       true,  false, false, BlockAnimType.None, 0f, 0f, new Color(0.22f, 0.35f, 0.78f));
            Register(96, "prop_market_stall",       "Gian Hàng Chợ Làng",   BlockCategory.InteractiveProp,"block_prop_market_stall",false,false, false, BlockAnimType.None, 0f, 0f, new Color(0.78f, 0.55f, 0.28f), 7);
            Register(97, "prop_stone_arch",         "Vòm Đá Cổng Vào",      BlockCategory.Obstacle,"block_prop_stone_arch",       false, false, false, BlockAnimType.None, 0f, 0f, new Color(0.58f, 0.56f, 0.62f), 9);
            Register(98, "prop_gravestone",         "Bia Mộ Nghĩa Địa",     BlockCategory.Obstacle,"block_prop_gravestone",       false, false, false, BlockAnimType.None, 0f, 0f, new Color(0.45f, 0.45f, 0.52f), 6);
            Register(99, "magic_boss_arena",        "Sàn Đấu Boss Phó Bản", BlockCategory.Ground, "block_magic_boss_arena",       true,  false, false, BlockAnimType.CrystalPulse, 2.0f, 0.30f, new Color(0.60f, 0.10f, 0.80f));
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

            // 1. Thử nạp từ Resources theo đường dẫn đăng ký
            if (!string.IsNullOrEmpty(def.spriteResourcePath))
            {
                def.cachedSprite = Resources.Load<Sprite>(def.spriteResourcePath);
                if (def.cachedSprite == null)
                    def.cachedSprite = Resources.Load<Sprite>("Blocks/" + def.spriteResourcePath);
            }

            // 2. Map trực tiếp với các tài nguyên sprite đã có sẵn trong project
            if (def.cachedSprite == null)
            {
                string c = def.codeName ?? "";
                if (c == "grass_bright") def.cachedSprite = Resources.Load<Sprite>("tile_grass");
                else if (c == "grass_flower_yellow") def.cachedSprite = Resources.Load<Sprite>("tile_grass_flower_yellow");
                else if (c == "grass_flower_red") def.cachedSprite = Resources.Load<Sprite>("tile_grass_flower_red");
                else if (c == "grass_flower_blue") def.cachedSprite = Resources.Load<Sprite>("tile_grass_flower_blue");
                else if (c == "grass_flower_white") def.cachedSprite = Resources.Load<Sprite>("tile_grass_flower_white");
                else if (c == "grass_dense_clover") def.cachedSprite = Resources.Load<Sprite>("tile_grass_dense");
                else if (c.Contains("dirt") || c.Contains("path") || c.Contains("road")) def.cachedSprite = Resources.Load<Sprite>("tile_dirt_path");
                else if (c.Contains("cobble") || c.Contains("stone") || c.Contains("dungeon")) def.cachedSprite = Resources.Load<Sprite>("tile_stone");
                else if (c.Contains("safezone") || c.Contains("gold")) def.cachedSprite = Resources.Load<Sprite>("tile_safezone");
                else if (c.Contains("sand") || c.Contains("desert")) def.cachedSprite = Resources.Load<Sprite>("tile_sand_shore");
                else if (c.Contains("water") || c.Contains("stream") || c.Contains("river")) def.cachedSprite = Resources.Load<Sprite>("tile_water");
                else if (c.Contains("fountain")) def.cachedSprite = Resources.Load<Sprite>("prop_fountain_grand");
                else if (c.Contains("water_pillar") || c.Contains("shrine_water")) def.cachedSprite = Resources.Load<Sprite>("prop_water_pillar");
                else if (c.Contains("bush") || c.Contains("stealth")) def.cachedSprite = Resources.Load<Sprite>("prop_bush_stealth");
                else if (c.Contains("tuft") || c.Contains("reed")) def.cachedSprite = Resources.Load<Sprite>("prop_grass_tuft");
                else if (c.Contains("boulder") || c.Contains("rock") || c.Contains("pebble")) def.cachedSprite = Resources.Load<Sprite>("prop_rock_mossy");
                else if (c.Contains("log") || c.Contains("stump")) def.cachedSprite = Resources.Load<Sprite>("prop_fallen_log");
                else if (c.Contains("tree")) def.cachedSprite = Resources.Load<Sprite>("prop_tree_village");
                else if (c.Contains("wall") || c.Contains("gate")) def.cachedSprite = Resources.Load<Sprite>("prop_dungeon_gate");
                else if (c.Contains("bridge") || c.Contains("fence") || c.Contains("plank")) def.cachedSprite = Resources.Load<Sprite>("prop_wooden_fence");
                else if (c.Contains("well")) def.cachedSprite = Resources.Load<Sprite>("prop_stone_well");
                else if (c.Contains("torch")) def.cachedSprite = Resources.Load<Sprite>("prop_torch_post");
                else if (c.Contains("rune") || c.Contains("magic")) def.cachedSprite = Resources.Load<Sprite>("rune_holy");
            }

            // 3. Nếu vẫn chưa có sprite vật lý, tự động sinh Texture 32x32 Pixel Art sắc nét theo màu sắc & vân gạch đặc trưng
            if (def.cachedSprite == null)
            {
                def.cachedSprite = GenerateProceduralTileSprite(def);
            }

            return def.cachedSprite;
        }

        /// <summary>
        /// Bộ sinh Sprite Pixel Art thủ tục 32x32: Đảm bảo 100% khối trong game luôn có hình ảnh riêng biệt,
        /// sắc nét, viền nổi khối 3D (Bevel), vân gạch, vân gỗ, sóng nước, không bao giờ bị biến thành cỏ xanh mặc định!
        /// </summary>
        private static Sprite GenerateProceduralTileSprite(TerrainBlockDef def)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color baseCol = def.minimapColor;
            if (baseCol.a < 0.1f) baseCol = new Color(0.5f, 0.5f, 0.5f, 1f);

            Color borderLight = Color.Lerp(baseCol, Color.white, 0.28f);
            Color borderDark = Color.Lerp(baseCol, Color.black, 0.32f);
            Color mortarCol = Color.Lerp(baseCol, Color.black, 0.45f);

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isTop = (y == size - 1 || y == size - 2);
                    bool isLeft = (x == 0 || x == 1);
                    bool isBottom = (y == 0 || y == 1);
                    bool isRight = (x == size - 1 || x == size - 2);

                    Color c = baseCol;

                    if (def.category == BlockCategory.Obstacle || def.codeName.Contains("stone") || def.codeName.Contains("cobble") || def.codeName.Contains("wall"))
                    {
                        // Hoa văn gạch đá hoa cương lát đường
                        bool isRowLine = (y % 8 == 0);
                        int rowIdx = y / 8;
                        bool isColLine = ((x + (rowIdx % 2) * 8) % 16 == 0);
                        if (isRowLine || isColLine) c = mortarCol;
                        else
                        {
                            int noise = (x * 7 + y * 13) % 7;
                            c = (noise == 0) ? borderLight : (noise == 1) ? borderDark : baseCol;
                        }
                    }
                    else if (def.codeName.Contains("plank") || def.codeName.Contains("bridge") || def.codeName.Contains("wood"))
                    {
                        // Thớ gỗ xẻ ngang
                        bool isPlankLine = (y % 6 == 0);
                        if (isPlankLine) c = borderDark;
                        else
                        {
                            int woodGrain = (x * 3 + y * 11) % 5;
                            c = (woodGrain == 0) ? borderLight : baseCol;
                        }
                    }
                    else if (def.category == BlockCategory.Liquid || def.isWater)
                    {
                        // Gợn sóng nước sinh động
                        int waveY = Mathf.RoundToInt(Mathf.Sin(x * 0.35f) * 2f) + 16;
                        int waveY2 = Mathf.RoundToInt(Mathf.Sin(x * 0.35f + 1.5f) * 2f) + 8;
                        if (Mathf.Abs(y - waveY) <= 1 || Mathf.Abs(y - waveY2) <= 1) c = borderLight;
                        else c = baseCol;
                    }
                    else if (def.codeName.Contains("farmland"))
                    {
                        // Luống đất cày nông trại
                        bool isFurrow = (y % 4 == 0);
                        c = isFurrow ? borderDark : borderLight;
                    }
                    else if (def.codeName.Contains("gold") || def.codeName.Contains("safezone") || def.codeName.Contains("magic"))
                    {
                        // Vành đai bảo hộ / Pháp trận ma thuật hoàng kim
                        if (isTop || isLeft || isBottom || isRight) c = Color.Lerp(baseCol, Color.white, 0.45f);
                        else
                        {
                            int distCenter = Mathf.Abs(x - 16) + Mathf.Abs(y - 16);
                            if (distCenter == 8 || distCenter == 9) c = Color.white;
                            else c = baseCol;
                        }
                    }
                    else
                    {
                        // Nền đất, thảm cỏ, hoa dại tự nhiên: 100% LIỀN MẠCH, KHÔNG VIỀN KHỐI VUÔNG BỊ NỨT
                        int grain = (x * 11 + y * 19) % 9;
                        if (grain == 0) c = borderLight;
                        else if (grain == 1) c = borderDark;
                        else c = baseCol;
                    }

                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Sprite sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            return sp;
        }
    }
}
