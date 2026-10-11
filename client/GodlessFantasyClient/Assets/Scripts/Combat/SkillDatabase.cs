using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GodlessFantasy.Player;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Engine Quản Lý Kỹ Năng / Bí Kíp Tập Trung (Skill Database Engine):
    /// - Không cần code C# để tạo kỹ năng mới!
    /// - Lưu trữ các chiêu thức tùy biến thành file JSON và tự động đồng bộ vào game.
    /// </summary>
    public class SkillDatabase : MonoBehaviour
    {
        public static SkillDatabase Instance { get; private set; }

        private List<SkillScrollData> registeredSkills = new List<SkillScrollData>();
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "custom_skill_database.json");

        private void Awake()
        {
            if (Instance == null) Instance = this;
            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            LoadDatabaseFromJson();

            // Nạp toàn bộ 20 Trục Độc Quyền + 10 Trục Đa Hệ chuẩn từ Thảo luận 10
            var coreAxes = GetCoreSkillAxes();
            bool hasNew = false;
            foreach (var core in coreAxes)
            {
                var existing = registeredSkills.Find(s => s.id == core.id);
                if (existing == null)
                {
                    registeredSkills.Add(core);
                    hasNew = true;
                }
                else
                {
                    existing.baseShapeMask = core.baseShapeMask;
                    existing.trajectoryType = core.trajectoryType;
                    existing.flySpeed = core.flySpeed;
                    existing.explosionRadius = core.explosionRadius;
                    existing.rainFallAngle = core.rainFallAngle;
                    existing.rainFallStyle = core.rainFallStyle;
                }
            }

            if (hasNew || registeredSkills.Count == 0)
            {
                SaveDatabaseToJson();
            }
        }

        public List<SkillScrollData> GetAllRegisteredSkills() => new List<SkillScrollData>(registeredSkills);

        public SkillScrollData GetSkillById(string id) => registeredSkills.Find(s => s.id == id);

        public List<InventoryItem> GetAllSkillItems()
        {
            var list = new List<InventoryItem>();
            foreach (var s in registeredSkills)
            {
                string effectLabel = s.IsSupport ? s.CategoryName : "Sát thương";
                string stats = $"[{s.requiredClass} · {s.element}] {effectLabel}: {s.baseDamage} + {Mathf.RoundToInt(s.intelligenceScaling * 100)}% Thuộc Tính | Hồi: {s.cooldown:F1}s | Mana: {s.manaCost} | Tầm: {s.RangeFormattedText}";
                string actualIcon = !string.IsNullOrEmpty(s.scrollIconName) ? s.scrollIconName : s.iconName;
                var item = new InventoryItem(s.id, s.scrollName, ItemType.SkillScroll, EquipSlot.None, s.tierRank, s.tierColor, actualIcon, stats, s.description, 1, s.Clone());
                list.Add(item);
            }
            return list;
        }

        public InventoryItem RegisterAndCreateSkillItem(string id, string name, string element, string tierRank,
                                                        Color tierColor, int manaCost, float cooldown, int baseDamage,
                                                        float intelScaling, int indicatorType, string hotbarIcon,
                                                        string scrollIcon, string desc, float castRange = 0f,
                                                        string requiredClass = "Toàn Hệ", string customVfxSprite = "",
                                                        int skillCategory = 0,
                                                        int baseShapeMask = 0, int trajectoryType = 0,
                                                        float flySpeed = 16.0f, float explosionRadius = 1.8f,
                                                        bool hasTrail = true, bool hasScreenShake = false,
                                                        int rainFallAngle = 0, int rainFallStyle = 0)
        {
            var existing = registeredSkills.Find(s => s.id == id);
            if (existing != null)
            {
                registeredSkills.Remove(existing);
            }

            var scroll = new SkillScrollData(id, name, tierRank, tierColor, element, manaCost, cooldown, baseDamage, intelScaling, indicatorType, hotbarIcon, scrollIcon, desc, castRange, requiredClass, customVfxSprite, skillCategory);
            scroll.baseShapeMask = baseShapeMask;
            scroll.trajectoryType = trajectoryType;
            scroll.flySpeed = flySpeed;
            scroll.explosionRadius = explosionRadius;
            scroll.hasTrail = hasTrail;
            scroll.hasScreenShake = hasScreenShake;
            scroll.rainFallAngle = rainFallAngle;
            scroll.rainFallStyle = rainFallStyle;

            registeredSkills.Add(scroll);
            SaveDatabaseToJson();

            // Đóng gói thành InventoryItem chuẩn để xuất hiện trong Túi Đồ
            string effectLabel = scroll.IsSupport ? scroll.CategoryName : "Sát thương";
            string stats = $"[{requiredClass} · {element}] {effectLabel}: {baseDamage} + {Mathf.RoundToInt(intelScaling * 100)}% Thuộc Tính | Hồi: {cooldown:F1}s | Mana: {manaCost} | Tầm: {scroll.RangeFormattedText}";
            string actualIcon = !string.IsNullOrEmpty(scrollIcon) ? scrollIcon : hotbarIcon;
            var item = new InventoryItem(id, name, ItemType.SkillScroll, EquipSlot.None, tierRank, tierColor, actualIcon, stats, desc, 1, scroll);
            return item;
        }

        /// <summary>
        /// Danh mục 20 Trục Kỹ Năng Độc Quyền (5 phẩm cấp x 4 hệ) + 10 Trục Đa Hệ chuẩn theo BIÊN BẢN THẢO LUẬN 10
        /// </summary>
        public static List<SkillScrollData> GetCoreSkillAxes()
        {
            var list = new List<SkillScrollData>();

            // ==========================================
            // NHÓM 1: KỊ SĨ (KNIGHT) - 5 TRỤC ĐỘC QUYỀN
            // ==========================================
            list.Add(new SkillScrollData(
                "knight_x_angel_domain", "Thiên Thần Hộ Giới", "Phẩm X", new Color(1.0f, 0.25f, 0.35f), "Hỏa",
                95, 18.0f, 100, 2.6f, 6, "icon_skill_divine_nova", "item_scroll_fire",
                "Triệu hồi kết giới thần thánh 360 độ, miễn nhiễm sát thương 3.0s và phản chấn 50% đòn đánh.",
                6.5f, "Kị Sĩ", "", 3) { baseShapeMask = 14, trajectoryType = 5, hasScreenShake = true });

            list.Add(new SkillScrollData(
                "knight_s_holy_ground", "Liệt Địa Thánh Kiếm", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Hỏa",
                70, 8.5f, 185, 2.1f, 2, "icon_skill_meteor", "item_scroll_fire",
                "Cắm cự kiếm khổng lồ từ không trung xuống đất, nổ tung địa chấn và hất tung mục tiêu.",
                7.0f, "Kị Sĩ", "", 0) { baseShapeMask = 4, trajectoryType = 3, rainFallAngle = 0, rainFallStyle = 2, hasScreenShake = true });

            list.Add(new SkillScrollData(
                "knight_a_shield_charge", "Khiên Thành Xung Kích", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Hỏa",
                48, 6.0f, 115, 1.6f, 1, "icon_skill_dash", "item_scroll_fire",
                "Giơ đại khiên lao nhanh về phía trước hất văng mọi kẻ cản đường, làm choáng 1.5s.",
                6.0f, "Kị Sĩ", "", 0) { baseShapeMask = 19, trajectoryType = 0, flySpeed = 22f });

            list.Add(new SkillScrollData(
                "knight_b_whirlwind_slash", "Toàn Phong Trảm Kích", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Hỏa",
                36, 4.0f, 78, 1.3f, 4, "icon_skill_flame_cone", "item_scroll_fire",
                "Xoay kiếm hình lốc xoáy quanh thân 2 vòng, chém quét mọi kẻ thù xung quanh.",
                3.2f, "Kị Sĩ", "", 0) { baseShapeMask = 3, trajectoryType = 5 });

            list.Add(new SkillScrollData(
                "knight_c_straight_sword_wave", "Kiếm Khí Trực Phá", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "Hỏa",
                26, 2.8f, 52, 0.95f, 1, "icon_skill_meteor", "item_scroll_fire",
                "Vung kiếm chém ra luồng kiếm khí bay thẳng xé gió, xuyên qua mục tiêu đầu tiên.",
                8.0f, "Kị Sĩ", "", 0) { baseShapeMask = 1, trajectoryType = 0, flySpeed = 18f });

            // ==========================================
            // NHÓM 2: PHÁP SƯ (MAGE) - 5 TRỤC ĐỘC QUYỀN
            // ==========================================
            list.Add(new SkillScrollData(
                "mage_x_world_collapse", "Thế Giới Băng Diệt", "Phẩm X", new Color(1.0f, 0.25f, 0.35f), "Băng",
                110, 20.0f, 320, 2.8f, 2, "icon_skill_meteor", "item_scroll_ice",
                "Ngưng đọng không gian, giáng mưa thiên thạch ma pháp đa tầng tàn phá toàn bộ khu vực.",
                8.5f, "Pháp Sư", "", 0) { baseShapeMask = 10, trajectoryType = 3, rainFallAngle = 0, rainFallStyle = 3, hasScreenShake = true });

            list.Add(new SkillScrollData(
                "mage_s_meteor_crash", "Thiên Thạch Ma Trụy", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Hỏa",
                75, 8.0f, 195, 2.2f, 2, "icon_skill_meteor", "item_scroll_fire",
                "Triệu hồi vẫn thạch khổng lồ từ tầng mây rơi xuống tâm chỉ định, đốt cháy mặt đất 4 giây.",
                7.0f, "Pháp Sư", "", 0) { baseShapeMask = 4, trajectoryType = 3, rainFallAngle = 1, rainFallStyle = 1, hasScreenShake = true });

            list.Add(new SkillScrollData(
                "mage_a_arcane_blast", "Bí Pháp Oanh Kích", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Hỏa",
                52, 5.5f, 125, 1.7f, 3, "icon_skill_flame_cone", "item_scroll_fire",
                "Vận tụ ma lực xả luồng sóng xung kích hồ quang hình nón cực đại thiêu đốt kẻ địch trước mặt.",
                5.0f, "Pháp Sư", "", 0) { baseShapeMask = 3, trajectoryType = 0, flySpeed = 14f });

            list.Add(new SkillScrollData(
                "mage_b_elemental_tide", "Nguyên Tố Triều Tịch", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Băng",
                38, 4.2f, 85, 1.35f, 1, "icon_skill_arcane_bolt", "item_scroll_ice",
                "Phóng đợt sóng ma pháp thủy triều cuồn cuộn bay thẳng, đẩy lùi kẻ địch về phía sau.",
                8.0f, "Pháp Sư", "", 0) { baseShapeMask = 3, trajectoryType = 0, flySpeed = 15f });

            list.Add(new SkillScrollData(
                "mage_c_triple_arcane_orb", "Ma Cầu Liên Xạ", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "Hỏa",
                25, 2.5f, 48, 1.0f, 1, "icon_skill_arcane_bolt", "item_scroll_fire",
                "Bắn liên tiếp 3 quả cầu ma pháp tốc độ cao bám đuổi nhẹ vào hướng con trỏ chuột.",
                8.0f, "Pháp Sư", "", 0) { baseShapeMask = 0, trajectoryType = 1, flySpeed = 16f });

            // ==========================================
            // NHÓM 3: XẠ THỦ (ARCHER) - 5 TRỤC ĐỘC QUYỀN
            // ==========================================
            list.Add(new SkillScrollData(
                "archer_x_thousand_arrows", "Vạn Tiễn Quy Tông", "Phẩm X", new Color(1.0f, 0.25f, 0.35f), "Phong",
                98, 19.0f, 290, 2.7f, 2, "icon_skill_meteor", "item_scroll_wind",
                "Bắn mũi tên dẫn đạo lên trời, triệu hồi hàng vạn mũi tên nguyên tố xối xả như bão quét sạch màn hình.",
                8.0f, "Xạ Thủ", "", 0) { baseShapeMask = 15, trajectoryType = 3, rainFallAngle = 2, rainFallStyle = 2, hasScreenShake = true });

            list.Add(new SkillScrollData(
                "archer_s_cloud_piercer", "Xuyên Vân Thần Tiễn", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Phong",
                68, 7.5f, 180, 2.3f, 1, "skill_frost", "item_scroll_wind",
                "Tụ lực bắn ra mũi đại tiễn ma thuật xuyên thủng mọi hàng phòng ngự, sát thương tăng theo cự ly.",
                10.0f, "Xạ Thủ", "", 0) { baseShapeMask = 2, trajectoryType = 0, flySpeed = 24f });

            list.Add(new SkillScrollData(
                "archer_a_soul_barrage", "Bão Tiễn Đoạt Hồn", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Lôi",
                50, 5.2f, 120, 1.65f, 3, "icon_skill_flame_cone", "item_scroll_thunder",
                "Bắn một chùm 7 mũi tên xòe hình quạt càn quét diện rộng, gây hiệu ứng xuất huyết.",
                5.5f, "Xạ Thủ", "", 0) { baseShapeMask = 2, trajectoryType = 0, flySpeed = 16f });

            list.Add(new SkillScrollData(
                "archer_b_magic_arrow_rain", "Ma Lực Tiễn Vũ", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Băng",
                38, 4.5f, 82, 1.25f, 2, "icon_skill_meteor", "item_scroll_ice",
                "Thả chùm tên ma thuật rơi thẳng xuống vùng đất 2.5m, tạo bãi gai năng lượng làm chậm 60%.",
                7.0f, "Xạ Thủ", "", 0) { baseShapeMask = 10, trajectoryType = 3, rainFallAngle = 0, rainFallStyle = 0 });

            list.Add(new SkillScrollData(
                "archer_c_homing_snipe", "Truy Hồn Xạ Kích", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "Phong",
                28, 2.6f, 55, 1.05f, 1, "skill_frost", "item_scroll_wind",
                "Bắn mũi tên thanh mảnh có khả năng uốn lượn tự tìm đến hitbox mục tiêu gần nhất.",
                8.5f, "Xạ Thủ", "", 0) { baseShapeMask = 2, trajectoryType = 2, flySpeed = 18f });

            // ==========================================
            // NHÓM 4: VÕ SĨ (BRAWLER) - 5 TRỤC ĐỘC QUYỀN
            // ==========================================
            list.Add(new SkillScrollData(
                "brawler_x_overlord_smash", "Bá Vương Phá Thiên", "Phẩm X", new Color(1.0f, 0.25f, 0.35f), "Hỏa",
                90, 16.0f, 280, 2.6f, 4, "icon_skill_divine_nova", "item_scroll_fire",
                "Dồn toàn bộ chân khí tung một quyền vỡ nát mặt đất, tạo chấn động sóng âm làm choáng toàn bộ 3.0s.",
                3.5f, "Võ Sĩ", "", 0) { baseShapeMask = 23, trajectoryType = 5, hasScreenShake = true });

            list.Add(new SkillScrollData(
                "brawler_s_earthquake_leap", "Chấn Địa Liệt Quyền", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Lôi",
                65, 7.0f, 175, 2.15f, 2, "icon_skill_meteor", "item_scroll_thunder",
                "Nhảy vọt lên không trung dậm mạnh xuống vị trí chỉ định, tạo hố nứt nguyên tố sát thương cực lớn.",
                4.5f, "Võ Sĩ", "", 0) { baseShapeMask = 22, trajectoryType = 3, hasScreenShake = true });

            list.Add(new SkillScrollData(
                "brawler_a_cyclone_kicks", "Bạo Phong Liên Cước", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Phong",
                46, 5.0f, 110, 1.55f, 3, "icon_skill_flame_cone", "item_scroll_wind",
                "Tung chuỗi cước ảnh liên hoàn quét hình nón phía trước, đẩy lùi và phá giáp đối thủ 30%.",
                4.0f, "Võ Sĩ", "", 0) { baseShapeMask = 3, trajectoryType = 0, flySpeed = 14f });

            list.Add(new SkillScrollData(
                "brawler_b_vacuum_palm", "Khí Kình Phá Không", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Phong",
                35, 3.8f, 75, 1.2f, 1, "icon_skill_flame_cone", "item_scroll_wind",
                "Tung cú đấm thẳng phát ra luồng sóng khí kình bay vụt đi, thổi bay đường đạn của kẻ thù.",
                6.5f, "Võ Sĩ", "", 0) { baseShapeMask = 1, trajectoryType = 0, flySpeed = 19f });

            list.Add(new SkillScrollData(
                "brawler_c_penetrating_strike", "Xung Quyền Thấu Kích", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "Hỏa",
                22, 2.2f, 50, 0.95f, 1, "icon_skill_dash", "item_scroll_fire",
                "Lướt tới 2 mét đấm thẳng vào mục tiêu, gây sát thương bỏ qua 25% phòng ngự.",
                2.5f, "Võ Sĩ", "", 0) { baseShapeMask = 1, trajectoryType = 0, flySpeed = 22f });

            // ==========================================
            // NHÓM 5: TRỤC ĐA HỆ / TOÀN HỆ (UNIVERSAL TIERS D & E)
            // ==========================================
            list.Add(new SkillScrollData(
                "uni_poison_pillar", "Nhiễm Độc Trụ", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "Mộc",
                20, 3.0f, 35, 0.75f, 2, "icon_skill_meteor", "item_scroll_wood",
                "Cắm một cột năng lượng độc tại tâm đất; kẻ địch bước vào bị trúng độc rút máu trong 6s.",
                7.0f, "Toàn Hệ", "", 0) { baseShapeMask = 9, trajectoryType = 3 });

            list.Add(new SkillScrollData(
                "uni_soul_mist", "Thực Hồn Độc Vụ", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "Mộc",
                22, 3.5f, 38, 0.8f, 4, "icon_skill_divine_nova", "item_scroll_wood",
                "Bốc ra màn sương độc quanh thân; gây sát thương DoT và hút 15% sát thương hồi lại HP.",
                3.0f, "Toàn Hệ", "", 0) { baseShapeMask = 11, trajectoryType = 5 });

            list.Add(new SkillScrollData(
                "uni_body_fortify", "Hộ Thể Cương Khí", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "Toàn Hệ",
                22, 8.0f, 25, 0.6f, 5, "icon_skill_dash", "item_scroll_generic",
                "Tụ khí quanh người tăng +25 Giáp và giảm 15% sát thương nhận vào trong 10 giây.",
                0.0f, "Toàn Hệ", "", 3) { baseShapeMask = 19, trajectoryType = 5 });

            list.Add(new SkillScrollData(
                "uni_diamond_shield", "Kim Cang Thuẫn", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "Ánh Sáng",
                25, 10.0f, 30, 0.7f, 5, "icon_skill_divine_nova", "item_scroll_light",
                "Tạo lớp khiên chắn năng lượng hấp thụ 120 sát thương trước khi vỡ.",
                0.0f, "Toàn Hệ", "", 3) { baseShapeMask = 19, trajectoryType = 5 });

            list.Add(new SkillScrollData(
                "uni_life_blessing", "Sinh Mệnh Chúc", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "Mộc",
                22, 4.5f, 40, 0.8f, 2, "icon_skill_divine_nova", "item_scroll_wood",
                "Thả vòng hồi sinh xuống đất, đồng đội bước vào hồi +40 HP và phục hồi thể lực.",
                7.0f, "Toàn Hệ", "", 1) { baseShapeMask = 11, trajectoryType = 3 });

            list.Add(new SkillScrollData(
                "uni_mind_clarity", "Tĩnh Tâm Quyết", "Phẩm E", Color.gray, "Toàn Hệ",
                5, 6.0f, 30, 0.5f, 5, "icon_skill_divine_nova", "item_scroll_generic",
                "Đứng yên tụ khí 1.5s hồi phục ngay lập tức +30 Mana để tiếp tục giao tranh.",
                0.0f, "Toàn Hệ", "", 2) { baseShapeMask = 14, trajectoryType = 5 });

            list.Add(new SkillScrollData(
                "uni_purify_mantra", "Thanh Tẩy Chú", "Phẩm E", Color.gray, "Ánh Sáng",
                12, 5.0f, 20, 0.4f, 5, "icon_skill_divine_nova", "item_scroll_light",
                "Hóa giải ngay lập tức hiệu ứng làm chậm, xuất huyết hoặc trúng độc trên bản thân.",
                0.0f, "Toàn Hệ", "", 3) { baseShapeMask = 14, trajectoryType = 5 });

            list.Add(new SkillScrollData(
                "uni_speed_burst", "Tốc Bộ Khí", "Phẩm E", Color.gray, "Phong",
                14, 7.0f, 20, 0.4f, 5, "icon_skill_dash", "item_scroll_wind",
                "Tăng +35% tốc độ di chuyển trong 4.0 giây, hỗ trợ truy đuổi hoặc rút lui nhanh.",
                0.0f, "Toàn Hệ", "", 3) { baseShapeMask = 15, trajectoryType = 5 });

            list.Add(new SkillScrollData(
                "uni_paralyze_trap", "Tê Liệt Khí Trận", "Phẩm E", Color.gray, "Lôi",
                14, 3.5f, 20, 0.4f, 2, "icon_skill_meteor", "item_scroll_thunder",
                "Đặt bẫy năng lượng tại chuột; khi giẫm phải kẻ địch bị trói chân trong 1.8 giây.",
                6.0f, "Toàn Hệ", "", 3) { baseShapeMask = 13, trajectoryType = 3 });

            list.Add(new SkillScrollData(
                "uni_energy_spark", "Nguyên Khí Đạn", "Phẩm E", Color.gray, "Toàn Hệ",
                8, 0.9f, 20, 0.5f, 1, "icon_skill_arcane_bolt", "item_scroll_generic",
                "Bắn một tia đạn khí sơ cấp thẳng tắp, hồi chiêu nhanh (0.9s), tiêu hao rất ít Mana.",
                8.0f, "Toàn Hệ", "", 0) { baseShapeMask = 0, trajectoryType = 0, flySpeed = 22f });

            // ==========================================
            // NHÓM 6: BÍ KÍP HUYỀN THOẠI MỚI (8 PHÔI PIXEL ĐỘC BẢN)
            // ==========================================
            var sDragon = new SkillScrollData(
                "mage_s_draconic_breath", "Hỏa Long Bộc Phá", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Hỏa",
                75, 7.5f, 190, 2.1f, 1, "skill_dragon_breath", "item_scroll_fire",
                "Triệu hồi thủ linh Hỏa Long gầm thét xé gió bay thẳng thiêu rụi mục tiêu.",
                9.0f, "Pháp Sư", "", 0);
            sDragon.baseShapeMask = 17; sDragon.trajectoryType = 0; sDragon.flySpeed = 22f; sDragon.hasScreenShake = true;
            list.Add(sDragon);

            var sPhoenix = new SkillScrollData(
                "mage_x_phoenix_rebirth", "Phượng Hoàng Niết Bàn", "Phẩm X", new Color(1.0f, 0.25f, 0.35f), "Hỏa",
                105, 20.0f, 310, 2.8f, 6, "skill_phoenix_rebirth", "item_scroll_fire",
                "Thần thú Phượng Hoàng giáng thế sải cánh rực lửa, hồi phục toàn bộ HP và kích nổ kết giới hủy diệt.",
                8.0f, "Pháp Sư", "", 3);
            sPhoenix.baseShapeMask = 23; sPhoenix.trajectoryType = 5; sPhoenix.hasScreenShake = true;
            list.Add(sPhoenix);

            var sSkull = new SkillScrollData(
                "mage_s_death_skull", "Tử Linh Oán Hồn", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Bóng Tối",
                68, 7.0f, 180, 2.2f, 1, "skill_death_skull", "item_scroll_dark",
                "Phóng đầu lâu tử linh ma quái tự bẻ lái đuổi theo kẻ địch và bộc nổ sát thương ám thuật.",
                9.5f, "Pháp Sư", "", 0);
            sSkull.baseShapeMask = 16; sSkull.trajectoryType = 2; sSkull.flySpeed = 17f;
            list.Add(sSkull);

            var sHammer = new SkillScrollData(
                "brawler_s_thunder_hammer", "Lôi Thần Trọng Chùy", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Lôi",
                70, 7.2f, 185, 2.15f, 2, "skill_thunder_hammer", "item_scroll_thunder",
                "Triệu hồi cự chùy lôi đình giáng mạnh xuống đất nứt toác địa tầng gây choáng 2.0s.",
                6.5f, "Võ Sĩ", "", 0);
            sHammer.baseShapeMask = 22; sHammer.trajectoryType = 3; sHammer.explosionRadius = 2.8f; sHammer.hasScreenShake = true;
            list.Add(sHammer);

            var sBlood = new SkillScrollData(
                "knight_s_crimson_blood_spikes", "Huyết Ma Địa Ngục", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Bóng Tối",
                72, 8.0f, 195, 2.05f, 2, "skill_blood_spikes", "item_scroll_dark",
                "Ngưng kết huyết châu ma đạo, làm trồi hàng loạt gai máu sắc nhọn từ lòng đất hút máu mục tiêu.",
                6.5f, "Kị Sĩ", "", 0);
            sBlood.baseShapeMask = 20; sBlood.trajectoryType = 3; sBlood.explosionRadius = 2.6f;
            list.Add(sBlood);

            var sVortex = new SkillScrollData(
                "mage_x_void_blackhole", "Hư Không Thôn Tính", "Phẩm X", new Color(1.0f, 0.25f, 0.35f), "Bóng Tối",
                115, 22.0f, 330, 2.9f, 2, "skill_void_blackhole", "item_scroll_dark",
                "Xé rách không gian tạo hố đen xoáy hư không khổng lồ nuốt chửng mọi kẻ địch trong vùng.",
                8.5f, "Pháp Sư", "", 0);
            sVortex.baseShapeMask = 21; sVortex.trajectoryType = 3; sVortex.explosionRadius = 3.6f; sVortex.hasScreenShake = true;
            list.Add(sVortex);

            var sShield = new SkillScrollData(
                "knight_a_aegis_bulwark", "Kim Cang Hộ Thuẫn", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Ánh Sáng",
                50, 6.5f, 120, 1.6f, 1, "skill_aegis_bulwark", "item_scroll_light",
                "Dựng khiên kim cương thánh quang cản phá toàn bộ đòn đánh và phản hồi 40% sát thương.",
                5.5f, "Kị Sĩ", "", 3);
            sShield.baseShapeMask = 19; sShield.trajectoryType = 0; sShield.flySpeed = 15f;
            list.Add(sShield);

            var sDagger = new SkillScrollData(
                "archer_s_flying_daggers", "Ám Ảnh Phi Đao", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "Phong",
                65, 6.8f, 185, 2.25f, 1, "skill_flying_daggers", "item_scroll_wind",
                "Phóng phi đao song nhận thích khách tốc biến cực nhanh xuyên thủng hàng phòng ngự đối phương.",
                10.5f, "Xạ Thủ", "", 0);
            sDagger.baseShapeMask = 18; sDagger.trajectoryType = 0; sDagger.flySpeed = 26f;
            list.Add(sDagger);

            return list;
        }

        /// <summary>
        /// Tạo trọn bộ 140 Kỹ Năng Độc Bản (4 Phái x 7 Nguyên Tố x 5 Cấp Phẩm = 140 VFX):
        /// Đầy đủ thông số vật lý đạn đạo, hoạt ảnh frame-by-frame, góc rơi và kiểu rơi!
        /// </summary>
        public static List<SkillScrollData> GetFull140SkillMatrix()
        {
            var list = new List<SkillScrollData>();

            string[] classes = { "Kị Sĩ", "Pháp Sư", "Võ Sĩ", "Xạ Thủ" };
            string[] elements = { "Hỏa", "Băng", "Lôi", "Quang", "Ám", "Phong", "Mộc" };
            string[] elemScrolls = { "item_scroll_fire", "item_scroll_ice", "item_scroll_thunder", "item_scroll_light", "item_scroll_dark", "item_scroll_wind", "item_scroll_wood" };
            string[] elemHotbar = { "skill_fireball", "skill_frost", "skill_lightning", "icon_skill_divine_nova", "icon_skill_arcane_bolt", "icon_skill_flame_cone", "icon_skill_dash" };

            string[] tierRanks = { "Phẩm C", "Phẩm B", "Phẩm A", "Phẩm S", "Phẩm X" };
            Color[] tierColors = {
                new Color(0.35f, 0.65f, 1.0f),  // C: Xanh lam
                new Color(0.85f, 0.45f, 1.0f),  // B: Tím
                new Color(1.0f, 0.65f, 0.2f),   // A: Cam
                new Color(1.0f, 0.85f, 0.25f),  // S: Vàng kim
                new Color(1.0f, 0.25f, 0.35f)   // X: Đỏ thần thoại
            };

            for (int c = 0; c < classes.Length; c++)
            {
                string cls = classes[c];

                for (int e = 0; e < elements.Length; e++)
                {
                    string elem = elements[e];
                    string scrollIcon = elemScrolls[e];
                    string hotbarIcon = elemHotbar[e];

                    for (int t = 0; t < 5; t++)
                    {
                        string tier = tierRanks[t];
                        Color tCol = tierColors[t];

                        string skillName = "";
                        string id = $"{cls.ToLower().Replace(" ", "")}_{elem.ToLower()}_t{t}";
                        string desc = "";
                        int indType = 1;
                        int shape = 0;
                        int traj = 0;
                        float spd = 16f;
                        float rad = 1.8f;
                        float range = 8.0f;
                        int dmg = 45;
                        int mana = 20;
                        float cd = 2.5f;
                        float scaling = 0.8f;
                        int fallAngle = 0;
                        int fallStyle = 0;
                        bool trail = true;
                        bool shake = (t >= 3);

                        // Thiết lập theo Phái & Bậc (Tier 0=C, 1=B, 2=A, 3=S, 4=X) & Nguyên Tố
                        switch (c)
                        {
                            case 0: // KỊ SĨ
                                if (t == 0) // C: Trảm Khí
                                {
                                    skillName = $"{elem} Kiếm Khí Trực Phá";
                                    desc = $"Vung kiếm chém ra luồng kiếm khí hệ {elem} xé gió bay thẳng xuyên mục tiêu.";
                                    indType = 1;
                                    shape = (elem == "Quang") ? 14 : (elem == "Phong") ? 18 : (elem == "Mộc") ? 9 : 1;
                                    traj = 0; spd = 18f; range = 8.0f; dmg = 52; mana = 22; cd = 2.5f; scaling = 0.95f;
                                }
                                else if (t == 1) // B: Mưa Cự Kiếm / Gai Băng / Trồi Đất
                                {
                                    skillName = $"{elem} Kiếm Địa Oanh Tạc";
                                    desc = $"Triệu hồi cự kiếm năng lượng hệ {elem} cắm thẳng từ trời xuống vùng đất, nổ tung sóng xung kích.";
                                    indType = 2;
                                    shape = (elem == "Băng") ? 10 : (elem == "Hỏa") ? 12 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Ám") ? 8 : (elem == "Mộc") ? 9 : (elem == "Phong") ? 15 : 4;
                                    traj = 3; spd = 14f; range = 7.0f; rad = 2.4f; dmg = 85; mana = 36; cd = 4.2f; scaling = 1.3f;
                                    fallAngle = 0; fallStyle = 2; // Thẳng đứng 90°, Oanh tạc
                                }
                                else if (t == 2) // A: Toàn Phong Quẹt Hình Quạt
                                {
                                    skillName = $"{elem} Toàn Phong Trảm Kích";
                                    desc = $"Xoay kiếm quét luồng sóng hình quạt 60° hệ {elem} hất lùi kẻ địch.";
                                    indType = 3; shape = 3; traj = 0; spd = 13f; range = 5.0f; rad = 2.0f; dmg = 125; mana = 48; cd = 5.5f; scaling = 1.6f;
                                }
                                else if (t == 3) // S: Địa Chấn 360° / Đột Phá
                                {
                                    skillName = $"{elem} Xích Kiếm Liệt Địa";
                                    desc = $"Dậm mạnh đại kiếm xuống đất tạo vụ nổ 360° hệ {elem} rung chuyển màn hình.";
                                    indType = 4;
                                    shape = (elem == "Hỏa") ? 17 : (elem == "Băng") ? 19 : (elem == "Lôi") ? 22 : (elem == "Quang") ? 19 : (elem == "Ám") ? 16 : (elem == "Mộc") ? 11 : (elem == "Phong") ? 18 : 5;
                                    traj = 5; spd = 10f; range = 3.5f; rad = 3.0f; dmg = 195; mana = 72; cd = 8.0f; scaling = 2.1f;
                                }
                                else // X: Vạn Kiếm Quy Thần
                                {
                                    skillName = $"Vạn Kiếm Quy Thần [{elem}]";
                                    desc = $"Kêu gọi trận địa thiên thần hộ giới và cự kiếm thần thoại cắm xuyên tâm địa.";
                                    indType = 6;
                                    shape = (elem == "Hỏa") ? 23 : (elem == "Băng") ? 10 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Ám") ? 21 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 9 : 4;
                                    traj = 3; spd = 15f; range = 7.5f; rad = 3.8f; dmg = 310; mana = 100; cd = 18.0f; scaling = 2.7f;
                                    fallAngle = 0; fallStyle = 1;
                                }
                                break;

                            case 1: // PHÁP SƯ
                                if (t == 0) // C: Ma Cầu Liên Xạ
                                {
                                    skillName = $"{elem} Ma Cầu Liên Xạ";
                                    desc = $"Bắn quả cầu ma pháp hệ {elem} bay lượn sóng tự đuổi vào kẻ địch.";
                                    indType = 1;
                                    shape = (elem == "Ám") ? 16 : (elem == "Quang") ? 14 : (elem == "Lôi") ? 13 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 9 : 0;
                                    traj = 1; spd = 16f; range = 8.5f; dmg = 48; mana = 25; cd = 2.4f; scaling = 1.0f;
                                }
                                else if (t == 1) // B: Thiên Thạch / Trồi Đất / Bụi Gai
                                {
                                    skillName = $"Thiên Thạch {elem} Giáng Lâm";
                                    desc = $"Triệu hồi ma pháp nguyên tố {elem} bộc phát càn quét vùng chỉ định.";
                                    indType = 2;
                                    shape = (elem == "Băng") ? 10 : (elem == "Mộc") ? 9 : (elem == "Ám") ? 8 : (elem == "Hỏa") ? 12 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Phong") ? 15 : 4;
                                    traj = 3; spd = 13f; range = 7.5f; rad = 2.5f; dmg = 90; mana = 40; cd = 4.5f; scaling = 1.4f;
                                    fallAngle = 1; fallStyle = 1;
                                }
                                else if (t == 2) // A: Bí Pháp Ba Động
                                {
                                    skillName = $"{elem} Bí Pháp Oanh Kích";
                                    desc = $"Xả luồng sóng xung kích ma pháp hệ {elem} hình nón thiêu đốt diện rộng.";
                                    indType = 3; shape = 3; traj = 0; spd = 12f; range = 5.5f; rad = 2.2f; dmg = 130; mana = 52; cd = 5.5f; scaling = 1.7f;
                                }
                                else if (t == 3) // S: Vòng Bão Ma Pháp 360°
                                {
                                    skillName = $"{elem} Linh Ba Động 360°";
                                    desc = $"Kích nổ bão nguyên tố hệ {elem} đa tầng quanh tâm thi triển.";
                                    indType = 4;
                                    shape = (elem == "Hỏa") ? 17 : (elem == "Băng") ? 10 : (elem == "Lôi") ? 22 : (elem == "Quang") ? 14 : (elem == "Ám") ? 20 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 11 : 5;
                                    traj = 5; spd = 10f; range = 4.0f; rad = 3.2f; dmg = 205; mana = 75; cd = 8.2f; scaling = 2.2f;
                                }
                                else // X: Thế Giới Diệt Thần
                                {
                                    skillName = $"Thế Giới {elem} Diệt Thần";
                                    desc = $"Ngưng đọng không gian, triệu hồi đôi cánh ma thần và năng lượng hủy diệt tối thượng.";
                                    indType = 6;
                                    shape = (elem == "Hỏa") ? 23 : (elem == "Băng") ? 17 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 23 : (elem == "Ám") ? 21 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 11 : 5;
                                    traj = 5; spd = 14f; range = 8.5f; rad = 4.0f; dmg = 330; mana = 110; cd = 20.0f; scaling = 2.85f;
                                    fallAngle = 3; fallStyle = 3;
                                }
                                break;

                            case 2: // VÕ SĨ
                                if (t == 0) // C: Xung Quyền
                                {
                                    skillName = $"{elem} Quyền Ảnh Phá Không";
                                    desc = $"Tung quyền kình hệ {elem} xé gió bay thẳng, phá vỡ phòng ngự mục tiêu.";
                                    indType = 1;
                                    shape = (elem == "Phong") ? 18 : (elem == "Ám") ? 16 : (elem == "Quang") ? 14 : 1;
                                    traj = 0; spd = 20f; range = 6.5f; dmg = 55; mana = 20; cd = 2.2f; scaling = 0.95f;
                                }
                                else if (t == 1) // B: Chấn Địa Liệt Quyền
                                {
                                    skillName = $"{elem} Chấn Địa Liệt Quyền";
                                    desc = $"Dậm mạnh xuống đất phóng sóng địa chấn hệ {elem} oanh tạc vùng tâm.";
                                    indType = 2;
                                    shape = (elem == "Hỏa") ? 12 : (elem == "Băng") ? 10 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Ám") ? 8 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 9 : 4;
                                    traj = 3; spd = 15f; range = 5.5f; rad = 2.6f; dmg = 85; mana = 35; cd = 4.0f; scaling = 1.35f;
                                    fallAngle = 0; fallStyle = 3;
                                }
                                else if (t == 2) // A: Cuồng Phong Liên Cước
                                {
                                    skillName = $"{elem} Cuồng Phong Liên Cước";
                                    desc = $"Tung chuỗi cước ảnh liên hoàn quét hình nón hệ {elem} đẩy lùi mục tiêu.";
                                    indType = 3; shape = 3; traj = 0; spd = 13f; range = 4.5f; rad = 2.0f; dmg = 120; mana = 46; cd = 5.0f; scaling = 1.6f;
                                }
                                else if (t == 3) // S: Bá Vương Phá Thiên
                                {
                                    skillName = $"Bá Vương {elem} Phá Thiên";
                                    desc = $"Dồn chân khí tung quyền 360° chấn động mặt đất làm choáng mọi kẻ địch.";
                                    indType = 4;
                                    shape = (elem == "Hỏa") ? 17 : (elem == "Băng") ? 19 : (elem == "Lôi") ? 22 : (elem == "Quang") ? 19 : (elem == "Ám") ? 16 : (elem == "Phong") ? 18 : (elem == "Mộc") ? 11 : 5;
                                    traj = 5; spd = 10f; range = 3.8f; rad = 3.0f; dmg = 195; mana = 70; cd = 7.5f; scaling = 2.15f;
                                }
                                else // X: Thần Ma Diệt Thế Quyền
                                {
                                    skillName = $"Thần Ma {elem} Diệt Thế Quyền";
                                    desc = $"Tuyệt kỹ quyền vương tột đỉnh giáng đòn hủy diệt toàn khu vực kết giới.";
                                    indType = 6;
                                    shape = (elem == "Hỏa") ? 23 : (elem == "Băng") ? 10 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Ám") ? 21 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 9 : 5;
                                    traj = 5; spd = 14f; range = 6.0f; rad = 3.6f; dmg = 300; mana = 95; cd = 16.0f; scaling = 2.65f;
                                }
                                break;

                            case 3: // XẠ THỦ
                                if (t == 0) // C: Truy Hồn Xạ Kích
                                {
                                    skillName = $"{elem} Truy Hồn Thần Tiễn";
                                    desc = $"Bắn mũi tên ma thuật hệ {elem} tự uốn lượn tìm đến kẻ địch gần nhất.";
                                    indType = 1;
                                    shape = (elem == "Phong") ? 18 : (elem == "Ám") ? 16 : (elem == "Quang") ? 14 : (elem == "Lôi") ? 13 : (elem == "Mộc") ? 9 : 2;
                                    traj = 2; spd = 19f; range = 9.0f; dmg = 55; mana = 26; cd = 2.5f; scaling = 1.05f;
                                }
                                else if (t == 1) // B: Bão Tiễn Ma Lực
                                {
                                    skillName = $"{elem} Bão Tiễn Ma Lực";
                                    desc = $"Bắn chùm tên ma thuật hệ {elem} rơi tản mác xối xả làm chậm diện rộng.";
                                    indType = 2;
                                    shape = (elem == "Băng") ? 10 : (elem == "Hỏa") ? 12 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Ám") ? 8 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 9 : 2;
                                    traj = 3; spd = 14f; range = 8.0f; rad = 2.6f; dmg = 82; mana = 38; cd = 4.5f; scaling = 1.25f;
                                    fallAngle = 2; fallStyle = 0;
                                }
                                else if (t == 2) // A: Tiễn Khí Tản Xạ
                                {
                                    skillName = $"{elem} Tiễn Khí Tản Xạ";
                                    desc = $"Bắn chùm 7 mũi tên xòe hình quạt hệ {elem} càn quét diện rộng.";
                                    indType = 3;
                                    shape = (elem == "Phong") ? 18 : 2;
                                    traj = 0; spd = 16f; range = 6.0f; rad = 2.2f; dmg = 122; mana = 50; cd = 5.2f; scaling = 1.65f;
                                }
                                else if (t == 3) // S: Vạn Tiễn Quy Tông
                                {
                                    skillName = $"Vạn Tiễn {elem} Quy Tông";
                                    desc = $"Kích hoạt vòng tròn tiễn trận 360° bắn ra hàng trăm tia tên ma thuật.";
                                    indType = 4;
                                    shape = (elem == "Hỏa") ? 17 : (elem == "Băng") ? 10 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Ám") ? 16 : (elem == "Phong") ? 18 : (elem == "Mộc") ? 11 : 5;
                                    traj = 5; spd = 12f; range = 4.5f; rad = 3.2f; dmg = 190; mana = 70; cd = 8.0f; scaling = 2.2f;
                                }
                                else // X: Xuyên Vân Thần Tiễn Vũ
                                {
                                    skillName = $"Xuyên Vân {elem} Thần Tiễn Vũ";
                                    desc = $"Bắn mũi đại tiễn xuyên thủng mây xanh, triệu hồi mưa bão tiễn thần thoại quét màn hình.";
                                    indType = 6;
                                    shape = (elem == "Hỏa") ? 23 : (elem == "Băng") ? 10 : (elem == "Lôi") ? 13 : (elem == "Quang") ? 14 : (elem == "Ám") ? 21 : (elem == "Phong") ? 15 : (elem == "Mộc") ? 9 : 2;
                                    traj = 3; spd = 16f; range = 9.5f; rad = 3.8f; dmg = 305; mana = 100; cd = 19.0f; scaling = 2.75f;
                                    fallAngle = 3; fallStyle = 2;
                                }
                                break;
                        }

                        var scroll = new SkillScrollData(
                            id, skillName, tier, tCol, elem, mana, cd, dmg, scaling,
                            indType, hotbarIcon, scrollIcon, desc, range, cls, "", 0);

                        scroll.baseShapeMask = shape;
                        scroll.trajectoryType = traj;
                        scroll.flySpeed = spd;
                        scroll.explosionRadius = rad;
                        scroll.hasTrail = trail;
                        scroll.hasScreenShake = shake;
                        scroll.rainFallAngle = fallAngle;
                        scroll.rainFallStyle = fallStyle;

                        list.Add(scroll);
                    }
                }
            }

            return list;
        }

        public void SaveDatabaseToJson()
        {
            try
            {
                var list = new List<SkillJsonData>();
                foreach (var s in registeredSkills)
                {
                    list.Add(SkillJsonData.FromSkill(s));
                }
                string json = JsonUtility.ToJson(new SkillListWrapper { skills = list }, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"<color=green>[SKILL ENGINE]</color> Đã lưu {list.Count} kỹ năng vào: {SaveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SKILL ENGINE] Lỗi khi lưu database kỹ năng: {ex.Message}");
            }
        }

        public void LoadDatabaseFromJson()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var wrapper = JsonUtility.FromJson<SkillListWrapper>(json);
                    if (wrapper != null && wrapper.skills != null)
                    {
                        registeredSkills.Clear();
                        foreach (var data in wrapper.skills)
                        {
                            registeredSkills.Add(data.ToSkill());
                        }
                        Debug.Log($"<color=green>[SKILL ENGINE]</color> Đã tải {registeredSkills.Count} kỹ năng từ file JSON!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SKILL ENGINE] Chưa có database kỹ năng JSON tùy chỉnh: {ex.Message}");
            }
        }

        [Serializable]
        private class SkillListWrapper
        {
            public List<SkillJsonData> skills;
        }
    }
}
