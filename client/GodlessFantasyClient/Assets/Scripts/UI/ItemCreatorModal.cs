using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Game Studio Thiết Kế Asset Trò Chơi (Widescreen 3-Column Studio - No Code):
    /// - Cột 1: Danh sách Phôi Mẫu (Presets) - Click để load nhanh thông số mẫu.
    /// - Cột 2: Bảng Thiết Kế Thông Số & Lưới Chọn Icon Bằng Hình Ảnh Thật (Visual Icon Picker).
    /// - Cột 3: Khung Xem Trước Trực Tiếp (Live 1:1 Preview) & Nút Tạo / Lưu Database.
    /// - Phím tắt [F1] để Bật / Tắt Studio.
    /// </summary>
    public class ItemCreatorModal : MonoBehaviour
    {
        public static ItemCreatorModal Instance { get; private set; }

        public bool IsOpen = false;
        private int studioMode = 0; // 0 = Tạo Vật Phẩm, 1 = Tạo Kỹ Năng, 2 = Live Inventory Inspector
        private readonly string[] modeTabs = { "⚔️ THIẾT KẾ VẬT PHẨM & TRANG BỊ", "📜 THIẾT KẾ KỸ NĂNG (140 VFX)", "🎒 CHỈNH SỬA TÚI ĐỒ (LIVE INSPECTOR)" };

        private Vector2 presetScrollPos;
        private Vector2 formScrollPos;

        // Texture Cache để vẽ icon trực quan
        private Dictionary<string, Texture2D> iconTexCache = new Dictionary<string, Texture2D>();

        // ================= FORM 1: VẬT PHẨM =================
        private string formItemName = "Bảo Kiếm Hoàng Kim";
        private string formItemId = "custom_wep_1";
        private int formTypeIndex = 0; // 0=Equipment, 1=Consumable, 2=Pet, 3=Material
        private readonly string[] typeNames = { "Trang Bị", "Dược Phẩm", "Thú Cưng", "Nguyên Liệu" };

        private int formSlotIndex = 7; // 0=Không, 1=Mũ, 2=Áo, 3=Quần, 4=Giày, 5=Nhẫn, 6=Dây Chuyền, 7=Vũ Khí
        private readonly string[] slotNames = { "Không", "Mũ", "Áo", "Quần", "Giày", "Nhẫn", "Dây Chuyền", "Vũ Khí" };

        private int formTierIndex = 4;
        private readonly string[] tierNames = { "Phẩm E", "Phẩm D", "Phẩm C", "Phẩm B", "Phẩm A", "Phẩm S", "Phẩm X" };
        private readonly Color[] tierColors = {
            Color.gray,
            new Color(0.35f, 0.85f, 0.45f), // D: Xanh lá
            new Color(0.35f, 0.65f, 1.0f),  // C: Xanh dương
            new Color(0.85f, 0.45f, 1.0f),  // B: Tím
            new Color(1.0f, 0.65f, 0.2f),   // A: Cam
            new Color(1.0f, 0.85f, 0.25f),  // S: Vàng kim
            new Color(1.0f, 0.25f, 0.35f)   // X: Đỏ thần thoại
        };

        private int formItemIconIndex = 0;
        private readonly string[] defaultItemIconOptions = { "item_weapon", "item_helmet", "item_armor", "item_pants", "item_boots", "item_ring", "item_necklace", "item_potion_hp", "item_potion_mana", "pet_squirrel" };
        private List<string> customItemIconsList = new List<string>();

        // Cơ chế nạp ảnh ngoài cho vật phẩm (Custom External Item Icon)
        private string customItemImageInputPath = "";
        private string customItemImageStatus = "";

        public string[] GetAllItemIcons()
        {
            if (customItemIconsList.Count == 0) return defaultItemIconOptions;
            List<string> combined = new List<string>(defaultItemIconOptions);
            combined.AddRange(customItemIconsList);
            return combined.ToArray();
        }

        private void LoadCustomExternalItemImage()
        {
            if (string.IsNullOrEmpty(customItemImageInputPath))
            {
                customItemImageStatus = "⚠️ Hãy nhập đường dẫn file ảnh PNG!";
                return;
            }

            try
            {
                string cleanPath = customItemImageInputPath.Trim('\"', '\'');
                if (!System.IO.File.Exists(cleanPath))
                {
                    customItemImageStatus = "❌ Không tìm thấy file tại đường dẫn!";
                    return;
                }

                byte[] fileData = System.IO.File.ReadAllBytes(cleanPath);
                Texture2D customTex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                if (customTex.LoadImage(fileData))
                {
                    customTex.filterMode = FilterMode.Point;
                    string customId = "ext_item_" + System.IO.Path.GetFileNameWithoutExtension(cleanPath);
                    iconTexCache[customId] = customTex;
                    if (!customItemIconsList.Contains(customId))
                    {
                        customItemIconsList.Add(customId);
                    }
                    CombatHUD.RegisterExternalIcon(customId, customTex);
                    string[] all = GetAllItemIcons();
                    formItemIconIndex = Mathf.Max(0, Array.IndexOf(all, customId));
                    customItemImageStatus = $"✅ Đã nạp thành công icon vật phẩm: {customId}!";
                    Debug.Log($"<color=green>[GODLESS STUDIO]</color> Đã nạp icon vật phẩm từ: {cleanPath}");
                }
                else
                {
                    customItemImageStatus = "❌ Không thể giải mã dữ liệu ảnh PNG!";
                }
            }
            catch (Exception ex)
            {
                customItemImageStatus = $"❌ Lỗi nạp ảnh: {ex.Message}";
            }
        }

        private string formItemStats = "+20 Công (Dam) · +25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)";
        private string formItemDesc = "Bảo kiếm hoàng kim rèn từ thép cổ đại, gia tăng sức mạnh toàn diện cho dũng sĩ.";

        // Các biến chỉ số thật của vật phẩm (Real Item Stats)
        private string formItemBonusDmg = "20";
        private string formItemBonusArmor = "0";
        private string formItemBonusHp = "0";
        private string formItemBonusMp = "0";
        private string formItemBonusInt = "25";
        private string formItemBonusStr = "15";
        private string formItemBonusVit = "0";
        private string formItemBonusAgi = "0";

        private void SyncItemStatsString()
        {
            List<string> parts = new List<string>();
            if (int.TryParse(formItemBonusDmg, out int dmg) && dmg > 0) parts.Add($"+{dmg} Công (Dam)");
            if (int.TryParse(formItemBonusArmor, out int def) && def > 0) parts.Add($"+{def} Giáp");
            if (int.TryParse(formItemBonusHp, out int hp) && hp > 0) parts.Add($"+{hp} Máu (HP)");
            if (int.TryParse(formItemBonusMp, out int mp) && mp > 0) parts.Add($"+{mp} Mana");
            if (int.TryParse(formItemBonusInt, out int intel) && intel > 0) parts.Add($"+{intel} Trí Tuệ (INT)");
            if (int.TryParse(formItemBonusStr, out int str) && str > 0) parts.Add($"+{str} Sức Mạnh (STR)");
            if (int.TryParse(formItemBonusVit, out int vit) && vit > 0) parts.Add($"+{vit} Thể Lực (VIT)");
            if (int.TryParse(formItemBonusAgi, out int agi) && agi > 0) parts.Add($"+{agi} Nhanh Nhẹn (AGI)");

            if (parts.Count > 0)
            {
                formItemStats = string.Join(" · ", parts);
            }
        }

        // ================= FORM 2: KỸ NĂNG =================
        private string formSkillName = "Hỏa Long Bộc Phá";
        private string formSkillId = "skill_fire_dragon";
        
        private int formSkillClassIndex = 1; // 0=Kị Sĩ, 1=Pháp Sư, 2=Võ Sĩ, 3=Xạ Thủ, 4=Toàn Hệ
        private readonly string[] classNames = { "Kị Sĩ", "Pháp Sư", "Võ Sĩ", "Xạ Thủ", "Toàn Hệ" };

        private int formSkillElemIndex = 0; // 0..6
        private readonly string[] elemNames = { "Hỏa", "Băng", "Lôi", "Ánh Sáng", "Bóng Tối", "Phong", "Mộc" };
        private readonly string[] elemScrollIcons = { "item_scroll_fire", "item_scroll_ice", "item_scroll_thunder", "item_scroll_light", "item_scroll_dark", "item_scroll_wind", "item_scroll_wood" };
        private readonly string[] elemRuneIcons = { "rune_fire", "rune_ice", "rune_thunder", "rune_light", "rune_dark", "rune_wind", "rune_wood" };

        private int formSkillCategoryIndex = 0; // 0=Tấn Công, 1=Hồi Máu, 2=Hồi Mana, 3=Cường Hóa
        private readonly string[] categoryNames = { "Tấn Công", "Hồi Máu", "Hồi Mana", "Cường Hóa" };

        private int formSkillIndicatorIndex = 0;
        private readonly string[] indicatorNames = { "1. Bắn Thẳng", "2. Vùng Đất / Mưa", "3. Hình Quạt", "4. Nổ 360°", "5. Tự Thân", "6. Kết Giới Đội" };

        private string formSkillMana = "25";
        private string formSkillCd = "3.5";
        private string formSkillDmg = "45";
        private string formSkillScaling = "80";
        private string formSkillRange = "8.0";

        // Thống số Hoạt Ảnh Thủ Tục & Đạn Đạo (Procedural Ballistics - Discussion 10)
        private int formSkillShapeIndex = 0; // 0..7
        private readonly string[] shapeNames = {
            "0. Cầu (Orb)", "1. Kiếm Khí (Slash)", "2. Đại Tiễn (Arrow)", "3. Sóng Quạt (Wave)",
            "4. Sao Băng (Meteor)", "5. Vòng 360° (Nova)", "6. Cột Sáng (Pillar)", "7. Tia Pháo (Beam)",
            "8. Xúc Tu (Tentacle)", "9. Rễ Gai Dây Leo (VineSprout)", "10. Bụi Gai Băng (IceSpikes)", "11. Hoa Ma Pháp (FloraBloom)",
            "12. Cột Lửa Phun (FlameGeyser)", "13. Sấm Sét Giáng (ThunderStrike)", "14. Thánh Giá Quang (HolyCross)", "15. Cơn Lốc Xoáy (TornadoWhirl)",
            "16. Đầu Lâu Tử Linh (Skull)", "17. Hỏa Long Thần Thú (DragonHead)", "18. Phi Đao Song Nhận (Dagger)", "19. Kim Cang Hộ Thuẫn (Shield)",
            "20. Huyết Châu Gai (BloodDrop)", "21. Hố Đen Hư Không (VoidVortex)", "22. Chiến Chùy Lôi Đình (GiantHammer)", "23. Phượng Hoàng Lửa (Phoenix)"
        };

        private int formSkillTrajectoryIndex = 0; // 0..5
        private readonly string[] trajectoryNames = { "0. Tuyến Tính (Linear)", "1. Lượn Sóng (Sine)", "2. Tự Đuổi (Homing)", "3. Cầu Vồng (Lobbed)", "4. Lộn Về (Boomerang)", "5. Tức Thời (Instant)" };

        private string formSkillFlySpeed = "16.0";
        private string formSkillExplosionRadius = "1.8";
        private bool formSkillHasTrail = true;
        private bool formSkillHasScreenShake = false;

        private int formSkillRainFallAngle = 0; // 0=Thẳng đứng 90°, 1=Chéo trái -30°, 2=Chéo phải +30°, 3=Từ người chơi
        private readonly string[] fallAngleNames = { "90° Thẳng đứng", "-30° Chéo trái", "+30° Chéo phải", "Từ Người Chơi" };

        private int formSkillRainFallStyle = 0; // 0=Tản mác, 1=Xoáy ốc, 2=Oanh tạc, 3=Sóng quét
        private readonly string[] fallStyleNames = { "Tản Mác (Scatter)", "Xoáy Ốc (Spiral)", "Oanh Tạc (Carpet)", "Sóng Quét (Wave)" };

        // ================= TAB 3: LIVE INVENTORY INSPECTOR =================
        private int selectedInventoryIndex = 0;
        private Vector2 invSlotsScrollPos;
        private Vector2 invEditorScrollPos;

        private string editItemName = "";
        private int editTierIndex = 0;
        private string editItemQuantity = "1";
        private string editItemStats = "";
        private string editItemDesc = "";
        private string editItemReqClass = "Toàn Hệ";
        private string editItemElement = "Vô Hệ";

        // Chỉ số Kỹ năng nếu ô được chọn là SkillScroll:
        private string editSkillMana = "25";
        private string editSkillCd = "3.5";
        private string editSkillDmg = "45";
        private string editSkillScaling = "80";
        private string editSkillRange = "8.0";
        private int editSkillShapeIndex = 0;
        private int editSkillTrajectoryIndex = 0;
        private int editSkillRainFallAngle = 0;
        private int editSkillRainFallStyle = 0;
        private string editSkillFlySpeed = "16.0";
        private string editSkillExplosionRadius = "1.8";
        private bool editSkillHasTrail = true;
        private bool editSkillHasScreenShake = false;

        private bool formUseSkillHotbarIcon = true;
        private int formSkillHotbarIconIndex = 0;
        private readonly string[] skillHotbarIcons = { "icon_skill_meteor", "icon_skill_flame_cone", "icon_skill_divine_nova", "icon_skill_arcane_bolt", "icon_skill_dash", "skill_fireball", "skill_frost", "skill_lightning" };
        private List<string> customHotbarIconsList = new List<string>();

        // Cơ chế nạp ảnh ngoài (External Image Import)
        private string customImageInputPath = "";
        private string customImageStatus = "";
        private string studioExportStatus = "";

        private string formSkillDesc = "Triệu hồi luồng hỏa long cuồng nộ thiêu rụi mục tiêu trên đường bay!";

        private string[] GetAllHotbarIcons()
        {
            if (customHotbarIconsList.Count == 0) return skillHotbarIcons;
            List<string> combined = new List<string>(skillHotbarIcons);
            combined.AddRange(customHotbarIconsList);
            return combined.ToArray();
        }

        private void LoadCustomExternalImage()
        {
            if (string.IsNullOrEmpty(customImageInputPath))
            {
                customImageStatus = "⚠️ Hãy nhập đường dẫn file ảnh PNG!";
                return;
            }

            try
            {
                string cleanPath = customImageInputPath.Trim('\"', '\'');
                if (!System.IO.File.Exists(cleanPath))
                {
                    customImageStatus = "❌ Không tìm thấy file tại đường dẫn!";
                    return;
                }

                byte[] fileData = System.IO.File.ReadAllBytes(cleanPath);
                Texture2D customTex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                if (customTex.LoadImage(fileData))
                {
                    customTex.filterMode = FilterMode.Point;
                    string customId = "ext_" + System.IO.Path.GetFileNameWithoutExtension(cleanPath);
                    iconTexCache[customId] = customTex;
                    if (!customHotbarIconsList.Contains(customId))
                    {
                        customHotbarIconsList.Add(customId);
                    }
                    string[] all = GetAllHotbarIcons();
                    formSkillHotbarIconIndex = Mathf.Max(0, Array.IndexOf(all, customId));
                    customImageStatus = $"✅ Đã nạp thành công icon: {customId}!";
                }
                else
                {
                    customImageStatus = "❌ Không thể giải mã dữ liệu ảnh PNG!";
                }
            }
            catch (Exception ex)
            {
                customImageStatus = $"❌ Lỗi nạp ảnh: {ex.Message}";
            }
        }

        // ================= DANH SÁCH MẪU CÓ SẴN (PRESETS) =================
        private struct ItemPreset
        {
            public string name;
            public string id;
            public int typeIdx;
            public int slotIdx;
            public int tierIdx;
            public string icon;
            public string stats;
            public string desc;
            public string dmg;
            public string armor;
            public string hp;
            public string mp;
            public string intVal;
            public string strVal;
            public string vitVal;
            public string agiVal;
        }

        private struct SkillPreset
        {
            public string name;
            public string id;
            public int classIdx;
            public int elemIdx;
            public int tierIdx;
            public int indicatorIdx;
            public int categoryIdx; // 0=Damage, 1=Heal HP, 2=Heal Mana, 3=Buff Stat
            public string mana;
            public string cd;
            public string dmg;
            public string scaling;
            public string range;
            public string hotbarIcon;
            public string desc;
        }

        private readonly ItemPreset[] itemPresets = new ItemPreset[]
        {
            new ItemPreset { name = "Bảo Kiếm Hoàng Kim", id = "custom_wep_gold", typeIdx = 0, slotIdx = 7, tierIdx = 4, icon = "item_weapon", stats = "+20 Công (Dam) · +25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)", desc = "Kiếm cổ đại dát vàng tăng mạnh sức công kích và ma pháp.", dmg = "20", armor = "0", hp = "0", mp = "0", intVal = "25", strVal = "15", vitVal = "0", agiVal = "0" },
            new ItemPreset { name = "Mũ Chiến Binh Sắt", id = "custom_helm_iron", typeIdx = 0, slotIdx = 1, tierIdx = 2, icon = "item_helmet", stats = "+10 Giáp · +12 Thể Lực (VIT) · +5 Sức Mạnh (STR)", desc = "Mũ giáp bền bỉ che chắn cho dũng sĩ trong giao tranh.", dmg = "0", armor = "10", hp = "0", mp = "0", intVal = "0", strVal = "5", vitVal = "12", agiVal = "0" },
            new ItemPreset { name = "Áo Choàng Thánh Quang", id = "custom_armor_holy", typeIdx = 0, slotIdx = 2, tierIdx = 5, icon = "item_armor", stats = "+18 Giáp · +30 Thể Lực (VIT) · +15 Trí Tuệ (INT)", desc = "Áo choàng hộ mệnh thần thánh ban phước lành bình an.", dmg = "0", armor = "18", hp = "0", mp = "0", intVal = "15", strVal = "0", vitVal = "30", agiVal = "0" },
            new ItemPreset { name = "Quần Phù Thủy Gió", id = "custom_pants_wind", typeIdx = 0, slotIdx = 3, tierIdx = 3, icon = "item_pants", stats = "+8 Giáp · +18 Nhanh Nhẹn (AGI) · +10 Trí Tuệ (INT)", desc = "Vải lụa nhẹ như gió giúp dũng sĩ lướt đi linh hoạt.", dmg = "0", armor = "8", hp = "0", mp = "0", intVal = "10", strVal = "0", vitVal = "0", agiVal = "18" },
            new ItemPreset { name = "Giày Phong Hành Thần", id = "custom_boots_speed", typeIdx = 0, slotIdx = 4, tierIdx = 3, icon = "item_boots", stats = "+6 Giáp · +20 Nhanh Nhẹn (AGI)", desc = "Đôi giày tăng mạnh tốc độ di chuyển và giảm hồi chiêu Lướt.", dmg = "0", armor = "6", hp = "0", mp = "0", intVal = "0", strVal = "0", vitVal = "0", agiVal = "20" },
            new ItemPreset { name = "Nhẫn Hộ Mệnh Cổ Đại", id = "custom_ring_guard", typeIdx = 0, slotIdx = 5, tierIdx = 4, icon = "item_ring", stats = "+15 Thể Lực (VIT) · +15 Sức Mạnh (STR)", desc = "Nhẫn hộ mệnh giúp giảm sát thương nhận vào.", dmg = "0", armor = "0", hp = "0", mp = "0", intVal = "0", strVal = "15", vitVal = "15", agiVal = "0" },
            new ItemPreset { name = "Dây Chuyền Thần Ma", id = "custom_neck_magic", typeIdx = 0, slotIdx = 6, tierIdx = 5, icon = "item_necklace", stats = "+35 Trí Tuệ (INT) · +60 Mana", desc = "Trang sức tỏa hào quang ma thuật vô biên.", dmg = "0", armor = "0", hp = "0", mp = "60", intVal = "35", strVal = "0", vitVal = "0", agiVal = "0" },
            new ItemPreset { name = "Bình Siêu Máu HP", id = "custom_pot_hp_super", typeIdx = 1, slotIdx = 0, tierIdx = 3, icon = "item_potion_hp", stats = "Hồi +100 Máu (HP) tức thì", desc = "Bình dược phẩm tinh khiết hồi phục sinh lực thần tốc.", dmg = "0", armor = "0", hp = "100", mp = "0", intVal = "0", strVal = "0", vitVal = "0", agiVal = "0" },
            new ItemPreset { name = "Bình Đại Mana MP", id = "custom_pot_mp_grand", typeIdx = 1, slotIdx = 0, tierIdx = 3, icon = "item_potion_mana", stats = "Hồi +80 Mana tức thì", desc = "Nước suối pháp thuật tái tạo nguồn năng lượng dồi dào.", dmg = "0", armor = "0", hp = "0", mp = "80", intVal = "0", strVal = "0", vitVal = "0", agiVal = "0" },
            new ItemPreset { name = "Sóc Thần Hoàng Kim", id = "custom_pet_gold_sq", typeIdx = 2, slotIdx = 0, tierIdx = 5, icon = "pet_squirrel", stats = "Thú Cưng Đồng Hành · +5 Thể Lực (VIT) · +5 Nhanh Nhẹn (AGI)", desc = "Thú cưng linh vật quý tộc đồng hành cùng dũng sĩ.", dmg = "0", armor = "0", hp = "0", mp = "0", intVal = "0", strVal = "0", vitVal = "5", agiVal = "5" }
        };

        private readonly SkillPreset[] skillPresets = new SkillPreset[]
        {
            // === 1. KỊ SĨ (KNIGHT) ===
            new SkillPreset { name = "Thiên Thần Hộ Giới", id = "knight_x_angel_domain", classIdx = 0, elemIdx = 0, tierIdx = 6, indicatorIdx = 5, categoryIdx = 3, mana = "95", cd = "18.0", dmg = "100", scaling = "260", range = "6.5", hotbarIcon = "icon_skill_divine_nova", desc = "Triệu hồi kết giới thần thánh 360 độ, miễn nhiễm sát thương 3.0s và phản chấn 50% đòn đánh." },
            new SkillPreset { name = "Liệt Địa Thánh Kiếm", id = "knight_s_holy_ground", classIdx = 0, elemIdx = 0, tierIdx = 5, indicatorIdx = 1, categoryIdx = 0, mana = "70", cd = "8.5", dmg = "185", scaling = "210", range = "7.0", hotbarIcon = "icon_skill_meteor", desc = "Cắm cự kiếm khổng lồ từ không trung xuống đất, nổ tung địa chấn và hất tung mục tiêu." },
            new SkillPreset { name = "Khiên Thành Xung Kích", id = "knight_a_shield_charge", classIdx = 0, elemIdx = 0, tierIdx = 4, indicatorIdx = 0, categoryIdx = 0, mana = "48", cd = "6.0", dmg = "115", scaling = "160", range = "6.0", hotbarIcon = "icon_skill_dash", desc = "Giơ đại khiên lao nhanh về phía trước hất văng mọi kẻ cản đường, làm choáng 1.5s." },
            new SkillPreset { name = "Toàn Phong Trảm Kích", id = "knight_b_whirlwind_slash", classIdx = 0, elemIdx = 0, tierIdx = 3, indicatorIdx = 3, categoryIdx = 0, mana = "36", cd = "4.0", dmg = "78", scaling = "130", range = "3.2", hotbarIcon = "icon_skill_flame_cone", desc = "Xoay kiếm hình lốc xoáy quanh thân 2 vòng, chém quét mọi kẻ thù xung quanh." },
            new SkillPreset { name = "Kiếm Khí Trực Phá", id = "knight_c_straight_sword_wave", classIdx = 0, elemIdx = 0, tierIdx = 2, indicatorIdx = 0, categoryIdx = 0, mana = "26", cd = "2.8", dmg = "52", scaling = "95", range = "8.0", hotbarIcon = "icon_skill_meteor", desc = "Vung kiếm chém ra luồng kiếm khí bay thẳng xé gió, xuyên qua mục tiêu đầu tiên." },

            // === 2. PHÁP SƯ (MAGE) ===
            new SkillPreset { name = "Thế Giới Băng Diệt", id = "mage_x_world_collapse", classIdx = 1, elemIdx = 1, tierIdx = 6, indicatorIdx = 1, categoryIdx = 0, mana = "110", cd = "20.0", dmg = "320", scaling = "280", range = "8.5", hotbarIcon = "icon_skill_meteor", desc = "Ngưng đọng không gian, giáng mưa thiên thạch ma pháp đa tầng tàn phá toàn bộ khu vực." },
            new SkillPreset { name = "Thiên Thạch Ma Trụy", id = "mage_s_meteor_crash", classIdx = 1, elemIdx = 0, tierIdx = 5, indicatorIdx = 1, categoryIdx = 0, mana = "75", cd = "8.0", dmg = "195", scaling = "220", range = "7.0", hotbarIcon = "icon_skill_meteor", desc = "Triệu hồi vẫn thạch khổng lồ từ tầng mây rơi xuống tâm chỉ định, đốt cháy mặt đất 4 giây." },
            new SkillPreset { name = "Bí Pháp Oanh Kích", id = "mage_a_arcane_blast", classIdx = 1, elemIdx = 0, tierIdx = 4, indicatorIdx = 2, categoryIdx = 0, mana = "52", cd = "5.5", dmg = "125", scaling = "170", range = "5.0", hotbarIcon = "icon_skill_flame_cone", desc = "Vận tụ ma lực xả luồng sóng xung kích hồ quang hình nón cực đại thiêu đốt kẻ địch trước mặt." },
            new SkillPreset { name = "Nguyên Tố Triều Tịch", id = "mage_b_elemental_tide", classIdx = 1, elemIdx = 1, tierIdx = 3, indicatorIdx = 0, categoryIdx = 0, mana = "38", cd = "4.2", dmg = "85", scaling = "135", range = "8.0", hotbarIcon = "icon_skill_arcane_bolt", desc = "Phóng đợt sóng ma pháp thủy triều cuồn cuộn bay thẳng, đẩy lùi kẻ địch về phía sau." },
            new SkillPreset { name = "Ma Cầu Liên Xạ", id = "mage_c_triple_arcane_orb", classIdx = 1, elemIdx = 0, tierIdx = 2, indicatorIdx = 0, categoryIdx = 0, mana = "25", cd = "2.5", dmg = "48", scaling = "100", range = "8.0", hotbarIcon = "icon_skill_arcane_bolt", desc = "Bắn liên tiếp 3 quả cầu ma pháp tốc độ cao bám đuổi nhẹ vào hướng con trỏ chuột." },

            // === 3. XẠ THỦ (ARCHER) ===
            new SkillPreset { name = "Vạn Tiễn Quy Tông", id = "archer_x_thousand_arrows", classIdx = 3, elemIdx = 5, tierIdx = 6, indicatorIdx = 1, categoryIdx = 0, mana = "98", cd = "19.0", dmg = "290", scaling = "270", range = "8.0", hotbarIcon = "icon_skill_meteor", desc = "Bắn mũi tên dẫn đạo lên trời, triệu hồi hàng vạn mũi tên nguyên tố xối xả như bão quét sạch màn hình." },
            new SkillPreset { name = "Xuyên Vân Thần Tiễn", id = "archer_s_cloud_piercer", classIdx = 3, elemIdx = 5, tierIdx = 5, indicatorIdx = 0, categoryIdx = 0, mana = "68", cd = "7.5", dmg = "180", scaling = "230", range = "10.0", hotbarIcon = "skill_frost", desc = "Tụ lực bắn ra mũi đại tiễn ma thuật xuyên thủng mọi hàng phòng ngự, sát thương tăng theo cự ly." },
            new SkillPreset { name = "Bão Tiễn Đoạt Hồn", id = "archer_a_soul_barrage", classIdx = 3, elemIdx = 2, tierIdx = 4, indicatorIdx = 2, categoryIdx = 0, mana = "50", cd = "5.2", dmg = "120", scaling = "165", range = "5.5", hotbarIcon = "icon_skill_flame_cone", desc = "Bắn một chùm 7 mũi tên xòe hình quạt càn quét diện rộng, gây hiệu ứng xuất huyết." },
            new SkillPreset { name = "Ma Lực Tiễn Vũ", id = "archer_b_magic_arrow_rain", classIdx = 3, elemIdx = 1, tierIdx = 3, indicatorIdx = 1, categoryIdx = 0, mana = "38", cd = "4.5", dmg = "82", scaling = "125", range = "7.0", hotbarIcon = "icon_skill_meteor", desc = "Thả chùm tên ma thuật rơi thẳng xuống vùng đất 2.5m, tạo bãi gai năng lượng làm chậm 60%." },
            new SkillPreset { name = "Truy Hồn Xạ Kích", id = "archer_c_homing_snipe", classIdx = 3, elemIdx = 5, tierIdx = 2, indicatorIdx = 0, categoryIdx = 0, mana = "28", cd = "2.6", dmg = "55", scaling = "105", range = "8.5", hotbarIcon = "skill_frost", desc = "Bắn mũi tên thanh mảnh có khả năng uốn lượn tự tìm đến hitbox mục tiêu gần nhất." },

            // === 4. VÕ SĨ (BRAWLER) ===
            new SkillPreset { name = "Bá Vương Phá Thiên", id = "brawler_x_overlord_smash", classIdx = 2, elemIdx = 0, tierIdx = 6, indicatorIdx = 3, categoryIdx = 0, mana = "90", cd = "16.0", dmg = "280", scaling = "260", range = "3.5", hotbarIcon = "icon_skill_divine_nova", desc = "Dồn toàn bộ chân khí tung một quyền vỡ nát mặt đất, tạo chấn động sóng âm làm choáng toàn bộ 3.0s." },
            new SkillPreset { name = "Chấn Địa Liệt Quyền", id = "brawler_s_earthquake_leap", classIdx = 2, elemIdx = 2, tierIdx = 5, indicatorIdx = 1, categoryIdx = 0, mana = "65", cd = "7.0", dmg = "175", scaling = "215", range = "4.5", hotbarIcon = "icon_skill_meteor", desc = "Nhảy vọt lên không trung dậm mạnh xuống vị trí chỉ định, tạo hố nứt nguyên tố sát thương cực lớn." },
            new SkillPreset { name = "Bạo Phong Liên Cước", id = "brawler_a_cyclone_kicks", classIdx = 2, elemIdx = 5, tierIdx = 4, indicatorIdx = 2, categoryIdx = 0, mana = "46", cd = "5.0", dmg = "110", scaling = "155", range = "4.0", hotbarIcon = "icon_skill_flame_cone", desc = "Tung chuỗi cước ảnh liên hoàn quét hình nón phía trước, đẩy lùi và phá giáp đối thủ 30%." },
            new SkillPreset { name = "Khí Kình Phá Không", id = "brawler_b_vacuum_palm", classIdx = 2, elemIdx = 5, tierIdx = 3, indicatorIdx = 0, categoryIdx = 0, mana = "35", cd = "3.8", dmg = "75", scaling = "120", range = "6.5", hotbarIcon = "icon_skill_flame_cone", desc = "Tung cú đấm thẳng phát ra luồng sóng khí kình bay vụt đi, thổi bay đường đạn của kẻ thù." },
            new SkillPreset { name = "Xung Quyền Thấu Kích", id = "brawler_c_penetrating_strike", classIdx = 2, elemIdx = 0, tierIdx = 2, indicatorIdx = 0, categoryIdx = 0, mana = "22", cd = "2.2", dmg = "50", scaling = "95", range = "2.5", hotbarIcon = "icon_skill_dash", desc = "Lướt tới 2 mét đấm thẳng vào mục tiêu, gây sát thương bỏ qua 25% phòng ngự." },

            // === 5. TOÀN HỆ / ĐA HỆ (UNIVERSAL TIERS D & E) ===
            new SkillPreset { name = "Nhiễm Độc Trụ", id = "uni_poison_pillar", classIdx = 4, elemIdx = 6, tierIdx = 1, indicatorIdx = 1, categoryIdx = 0, mana = "20", cd = "3.0", dmg = "35", scaling = "75", range = "7.0", hotbarIcon = "icon_skill_meteor", desc = "Cắm một cột năng lượng độc tại tâm đất; kẻ địch bước vào bị trúng độc rút máu trong 6s." },
            new SkillPreset { name = "Thực Hồn Độc Vụ", id = "uni_soul_mist", classIdx = 4, elemIdx = 6, tierIdx = 1, indicatorIdx = 3, categoryIdx = 0, mana = "22", cd = "3.5", dmg = "38", scaling = "80", range = "3.0", hotbarIcon = "icon_skill_divine_nova", desc = "Bốc ra màn sương độc quanh thân; gây sát thương DoT và hút 15% sát thương hồi lại HP." },
            new SkillPreset { name = "Hộ Thể Cương Khí", id = "uni_body_fortify", classIdx = 4, elemIdx = 0, tierIdx = 1, indicatorIdx = 4, categoryIdx = 3, mana = "22", cd = "8.0", dmg = "25", scaling = "60", range = "0.0", hotbarIcon = "icon_skill_dash", desc = "Tụ khí quanh người tăng +25 Giáp và giảm 15% sát thương nhận vào trong 10 giây." },
            new SkillPreset { name = "Kim Cang Thuẫn", id = "uni_diamond_shield", classIdx = 4, elemIdx = 3, tierIdx = 1, indicatorIdx = 4, categoryIdx = 3, mana = "25", cd = "10.0", dmg = "30", scaling = "70", range = "0.0", hotbarIcon = "icon_skill_divine_nova", desc = "Tạo lớp khiên chắn năng lượng hấp thụ 120 sát thương trước khi vỡ." },
            new SkillPreset { name = "Sinh Mệnh Chúc", id = "uni_life_blessing", classIdx = 4, elemIdx = 6, tierIdx = 1, indicatorIdx = 1, categoryIdx = 1, mana = "22", cd = "4.5", dmg = "40", scaling = "80", range = "7.0", hotbarIcon = "icon_skill_divine_nova", desc = "Thả vòng hồi sinh xuống đất, đồng đội bước vào hồi +40 HP và phục hồi thể lực." },
            new SkillPreset { name = "Tĩnh Tâm Quyết", id = "uni_mind_clarity", classIdx = 4, elemIdx = 0, tierIdx = 0, indicatorIdx = 4, categoryIdx = 2, mana = "5", cd = "6.0", dmg = "30", scaling = "50", range = "0.0", hotbarIcon = "icon_skill_divine_nova", desc = "Đứng yên tụ khí 1.5s hồi phục ngay lập tức +30 Mana để tiếp tục giao tranh." },
            new SkillPreset { name = "Thanh Tẩy Chú", id = "uni_purify_mantra", classIdx = 4, elemIdx = 3, tierIdx = 0, indicatorIdx = 4, categoryIdx = 3, mana = "12", cd = "5.0", dmg = "20", scaling = "40", range = "0.0", hotbarIcon = "icon_skill_divine_nova", desc = "Hóa giải ngay lập tức hiệu ứng làm chậm, xuất huyết hoặc trúng độc trên bản thân." },
            new SkillPreset { name = "Tốc Bộ Khí", id = "uni_speed_burst", classIdx = 4, elemIdx = 5, tierIdx = 0, indicatorIdx = 4, categoryIdx = 3, mana = "14", cd = "7.0", dmg = "20", scaling = "40", range = "0.0", hotbarIcon = "icon_skill_dash", desc = "Tăng +35% tốc độ di chuyển trong 4.0 giây, hỗ trợ truy đuổi hoặc rút lui nhanh." },
            new SkillPreset { name = "Tê Liệt Khí Trận", id = "uni_paralyze_trap", classIdx = 4, elemIdx = 2, tierIdx = 0, indicatorIdx = 1, categoryIdx = 3, mana = "14", cd = "3.5", dmg = "20", scaling = "40", range = "6.0", hotbarIcon = "icon_skill_meteor", desc = "Đặt bẫy năng lượng tại chuột; khi giẫm phải kẻ địch bị trói chân trong 1.8 giây." },
            new SkillPreset { name = "Nguyên Khí Đạn", id = "uni_energy_spark", classIdx = 4, elemIdx = 0, tierIdx = 0, indicatorIdx = 0, categoryIdx = 0, mana = "8", cd = "0.9", dmg = "20", scaling = "50", range = "8.0", hotbarIcon = "icon_skill_arcane_bolt", desc = "Bắn một tia đạn khí sơ cấp thẳng tắp, hồi chiêu nhanh (0.9s), tiêu hao rất ít Mana." }
        };

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                IsOpen = !IsOpen;
                Debug.Log($"<color=cyan>[ASSET STUDIO]</color> Studio thiết kế: {(IsOpen ? "MỞ" : "ĐÓNG")}");
            }
        }

        public Texture2D GetCachedTexture(string texName)
        {
            if (string.IsNullOrEmpty(texName)) return Texture2D.whiteTexture;
            if (!iconTexCache.TryGetValue(texName, out var tex) || tex == null)
            {
                tex = Resources.Load<Texture2D>(texName);
                if (tex == null) tex = Texture2D.whiteTexture;
                iconTexCache[texName] = tex;
            }
            return tex;
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.depth = FantasyUITheme.DepthModal;

            // 1. Phủ mờ toàn màn hình tạo không gian tập trung cho Studio
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. Khung Studio Widescreen 3 Cột (960 x 580)
            float w = Mathf.Clamp(Screen.width * 0.90f, 920f, 1020f);
            float h = Mathf.Clamp(Screen.height * 0.88f, 540f, 620f);
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            Rect studioRect = new Rect(x, y, w, h);

            GUI.color = new Color(0.04f, 0.05f, 0.09f, 0.98f);
            GUI.DrawTexture(studioRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawDoubleGoldFrame(studioRect, 2);

            // 3. Header Thanh Lịch
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };
            GUI.Label(new Rect(x + 16, y + 10, w - 80, 24), "🛠️ GODLESS STUDIO · ENGINE THIẾT KẾ ASSET & KỸ NĂNG (NO-CODE)", titleStyle);

            // Nút Đóng [✕]
            var closeBtnStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold };
            if (GUI.Button(new Rect(x + w - 38, y + 10, 28, 24), "✕", closeBtnStyle))
            {
                IsOpen = false;
            }

            // 4. Tab Chọn Loại Thiết Kế (Vật Phẩm vs Kỹ Năng)
            studioMode = GUI.Toolbar(new Rect(x + 16, y + 40, w - 32, 28), studioMode, modeTabs);

            // 5. Layout 3 Cột Hoàn Hảo
            float contentY = y + 76f;
            float contentH = h - 86f;

            float col1W = 210f; // Cột 1: Presets
            float col3W = 270f; // Cột 3: Live Preview
            float col2W = w - col1W - col3W - 32f; // Cột 2: Bảng Form thiết kế

            Rect col1Rect = new Rect(x + 12, contentY, col1W, contentH);
            Rect col2Rect = new Rect(x + 16 + col1W, contentY, col2W, contentH);
            Rect col3Rect = new Rect(x + w - col3W - 12, contentY, col3W, contentH);

            if (studioMode == 0)
            {
                DrawColumnFrame(col1Rect, "📂 PHÔI MẪU (PRESETS)");
                DrawColumnFrame(col2Rect, "⚙️ THIẾT LẬP THÔNG SỐ");
                DrawColumnFrame(col3Rect, "👁️ XEM TRƯỚC TRỰC TIẾP (1:1)");
                DrawItemPresetsColumn(new Rect(col1Rect.x + 6, col1Rect.y + 26, col1Rect.width - 12, col1Rect.height - 32));
                DrawItemFormColumn(new Rect(col2Rect.x + 8, col2Rect.y + 26, col2Rect.width - 16, col2Rect.height - 32));
                DrawItemPreviewColumn(new Rect(col3Rect.x + 8, col3Rect.y + 26, col3Rect.width - 16, col3Rect.height - 32));
            }
            else if (studioMode == 1)
            {
                DrawColumnFrame(col1Rect, "📂 140 PHÔI CHIÊU (PRESETS)");
                DrawColumnFrame(col2Rect, "⚙️ VẬT LÝ VFX & ĐẠN ĐẠO");
                DrawColumnFrame(col3Rect, "👁️ XEM TRƯỚC & TEST CAST");
                DrawSkillPresetsColumn(new Rect(col1Rect.x + 6, col1Rect.y + 26, col1Rect.width - 12, col1Rect.height - 32));
                DrawSkillFormColumn(new Rect(col2Rect.x + 8, col2Rect.y + 26, col2Rect.width - 16, col2Rect.height - 32));
                DrawSkillPreviewColumn(new Rect(col3Rect.x + 8, col3Rect.y + 26, col3Rect.width - 16, col3Rect.height - 32));
            }
            else
            {
                DrawColumnFrame(col1Rect, "🎒 24 Ô HÀNH TRANG (SLOTS)");
                DrawColumnFrame(col2Rect, "🛠️ LIVE INSPECTOR (CHỈNH TRỰC TIẾP)");
                DrawColumnFrame(col3Rect, "👁️ XEM TRƯỚC & THỬ CHIÊU");
                DrawInventorySlotsListColumn(new Rect(col1Rect.x + 6, col1Rect.y + 26, col1Rect.width - 12, col1Rect.height - 32));
                DrawInventoryItemEditorColumn(new Rect(col2Rect.x + 8, col2Rect.y + 26, col2Rect.width - 16, col2Rect.height - 32));
                DrawInventoryLivePreviewColumn(new Rect(col3Rect.x + 8, col3Rect.y + 26, col3Rect.width - 16, col3Rect.height - 32));
            }
        }

        private void DrawColumnFrame(Rect r, string title)
        {
            GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.90f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(r, new Color(0.40f, 0.50f, 0.65f, 0.6f), 1);

            // Header nhỏ của từng cột
            Rect headerRect = new Rect(r.x, r.y, r.width, 22);
            GUI.color = new Color(0.10f, 0.15f, 0.25f, 0.95f);
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            var hStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };
            GUI.Label(headerRect, title, hStyle);
        }

        // ================= CỘT 1: PRESETS (PHÔI MẪU CÓ SẴN) =================
        private void DrawItemPresetsColumn(Rect r)
        {
            float itemH = 34f;
            float totalH = itemPresets.Length * (itemH + 4f);
            Rect viewRect = new Rect(0, 0, r.width - 16f, Mathf.Max(r.height, totalH));

            presetScrollPos = GUI.BeginScrollView(r, presetScrollPos, viewRect, false, true);

            var pStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };

            for (int i = 0; i < itemPresets.Length; i++)
            {
                var p = itemPresets[i];
                Rect btnRect = new Rect(0, i * (itemH + 4f), viewRect.width, itemH);

                bool isSelected = (formItemId == p.id);
                if (isSelected) GUI.color = new Color(1f, 0.85f, 0.3f);

                string iconLabel = (p.typeIdx == 0) ? "⚔️" : (p.typeIdx == 1) ? "🧪" : "🐿️";
                if (GUI.Button(btnRect, $"{iconLabel} {p.name}", pStyle))
                {
                    formItemName = p.name;
                    formItemId = p.id;
                    formTypeIndex = p.typeIdx;
                    formSlotIndex = p.slotIdx;
                    formTierIndex = p.tierIdx;
                    formItemIconIndex = Mathf.Max(0, Array.IndexOf(GetAllItemIcons(), p.icon));
                    formItemStats = p.stats;
                    formItemDesc = p.desc;
                    formItemBonusDmg = p.dmg ?? "0";
                    formItemBonusArmor = p.armor ?? "0";
                    formItemBonusHp = p.hp ?? "0";
                    formItemBonusMp = p.mp ?? "0";
                    formItemBonusInt = p.intVal ?? "0";
                    formItemBonusStr = p.strVal ?? "0";
                    formItemBonusVit = p.vitVal ?? "0";
                    formItemBonusAgi = p.agiVal ?? "0";
                }
                GUI.color = Color.white;
            }

            GUI.EndScrollView();
        }

        private void DrawSkillPresetsColumn(Rect r)
        {
            float itemH = 34f;
            float totalH = skillPresets.Length * (itemH + 4f);
            Rect viewRect = new Rect(0, 0, r.width - 16f, Mathf.Max(r.height, totalH));

            presetScrollPos = GUI.BeginScrollView(r, presetScrollPos, viewRect, false, true);

            var pStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };

            for (int i = 0; i < skillPresets.Length; i++)
            {
                var p = skillPresets[i];
                Rect btnRect = new Rect(0, i * (itemH + 4f), viewRect.width, itemH);

                bool isSelected = (formSkillId == p.id);
                if (isSelected) GUI.color = new Color(1f, 0.85f, 0.3f);

                string iconLabel = (p.elemIdx == 0) ? "🔥" : (p.elemIdx == 1) ? "❄️" : (p.elemIdx == 2) ? "⚡" : (p.elemIdx == 3) ? "✨" : "🔮";
                if (GUI.Button(btnRect, $"{iconLabel} {p.name}", pStyle))
                {
                    formSkillName = p.name;
                    formSkillId = p.id;
                    formSkillClassIndex = p.classIdx;
                    formSkillElemIndex = p.elemIdx;
                    formTierIndex = p.tierIdx;
                    formSkillIndicatorIndex = p.indicatorIdx;
                    formSkillCategoryIndex = p.categoryIdx;
                    formSkillMana = p.mana;
                    formSkillCd = p.cd;
                    formSkillDmg = p.dmg;
                    formSkillScaling = p.scaling;
                    formSkillRange = p.range;
                    formSkillHotbarIconIndex = Mathf.Max(0, Array.IndexOf(GetAllHotbarIcons(), p.hotbarIcon));
                    formSkillDesc = p.desc;

                    // Tự động suy ra cấu hình Procedural VFX 140 biến thể phù hợp nhất
                    if (p.indicatorIdx == 0) // Bắn Thẳng
                    {
                        formSkillShapeIndex = (p.classIdx == 3) ? 2 : (p.name.Contains("Kiếm") || p.name.Contains("Trảm") ? 1 : (p.name.Contains("Sóng") ? 3 : 0));
                        formSkillTrajectoryIndex = (p.name.Contains("Liên Xạ") || p.name.Contains("Đuổi")) ? 1 : 0;
                        formSkillFlySpeed = "18.0";
                        formSkillExplosionRadius = "1.5";
                    }
                    else if (p.indicatorIdx == 1) // Mưa / Vùng Đất AoE
                    {
                        formSkillShapeIndex = 4; // Meteor
                        formSkillTrajectoryIndex = 3; // Lobbed
                        formSkillFlySpeed = "14.0";
                        formSkillExplosionRadius = "2.5";
                    }
                    else if (p.indicatorIdx == 2) // Hình Quạt
                    {
                        formSkillShapeIndex = 3; // Wave
                        formSkillTrajectoryIndex = 0;
                        formSkillFlySpeed = "12.0";
                        formSkillExplosionRadius = "2.0";
                    }
                    else // Nova 360° / Buff
                    {
                        formSkillShapeIndex = 5; // NovaRing
                        formSkillTrajectoryIndex = 5; // Instant
                        formSkillFlySpeed = "10.0";
                        formSkillExplosionRadius = "3.0";
                    }
                    formSkillHasTrail = true;
                    formSkillHasScreenShake = (p.tierIdx >= 4); // Phẩm A, S, X rung màn hình uy lực!
                }
                GUI.color = Color.white;
            }

            GUI.EndScrollView();
        }

        // ================= CỘT 2: BẢNG FORM THIẾT KẾ VẬT PHẨM =================
        private void DrawItemFormColumn(Rect r)
        {
            float totalContentH = 670f;
            Rect viewRect = new Rect(0, 0, r.width - 16f, Mathf.Max(r.height, totalContentH));
            formScrollPos = GUI.BeginScrollView(r, formScrollPos, viewRect, false, true);

            float curY = 4f;
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.95f, 1f) } };

            // 1. Tên & ID
            GUI.Label(new Rect(0, curY, 90, 20), "Tên Vật Phẩm:", lbl);
            formItemName = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), formItemName);
            curY += 26f;

            GUI.Label(new Rect(0, curY, 90, 20), "Mã ID:", lbl);
            formItemId = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), formItemId);
            curY += 28f;

            // 2. Phân Loại & Ô Trang Bị
            GUI.Label(new Rect(0, curY, 90, 20), "Phân Loại:", lbl);
            formTypeIndex = GUI.Toolbar(new Rect(95, curY, viewRect.width - 95, 24), formTypeIndex, typeNames);
            curY += 28f;

            if (formTypeIndex == 0) // Nếu là Trang Bị mới chọn ô
            {
                GUI.Label(new Rect(0, curY, 90, 20), "Vị Trí Mặc:", lbl);
                formSlotIndex = GUI.SelectionGrid(new Rect(95, curY, viewRect.width - 95, 38), formSlotIndex, slotNames, 4);
                curY += 42f;
            }

            // 3. Phẩm Cấp (Các nút to có màu riêng biệt, không bị cụt chữ)
            GUI.Label(new Rect(0, curY, 90, 20), "Phẩm Cấp:", lbl);
            float tierBtnW = (viewRect.width - 95f - 18f) / 7f;
            for (int t = 0; t < tierNames.Length; t++)
            {
                bool isSelected = (formTierIndex == t);
                Color tCol = tierColors[t];
                GUI.color = isSelected ? tCol : new Color(tCol.r * 0.4f, tCol.g * 0.4f, tCol.b * 0.4f, 0.9f);

                string tShort = tierNames[t].Replace("Phẩm ", "");
                if (GUI.Button(new Rect(95 + t * (tierBtnW + 3), curY, tierBtnW, 24), tShort))
                {
                    formTierIndex = t;
                }
            }
            GUI.color = Color.white;
            curY += 28f;

            // 4. KHO ICON DẠNG LƯỚI ẢNH THẬT (Visual Icon Picker)
            GUI.Label(new Rect(0, curY, viewRect.width, 20), "🖼️ Kho Icon Mẫu (Click chọn trực tiếp hình ảnh):", lbl);
            curY += 22f;

            string[] currentItemIcons = GetAllItemIcons();
            float iconSize = 36f;
            float iconGap = 6f;
            int iconCols = 7;
            for (int i = 0; i < currentItemIcons.Length; i++)
            {
                int col = i % iconCols;
                int row = i / iconCols;
                Rect iconSlot = new Rect(col * (iconSize + iconGap), curY + row * (iconSize + iconGap), iconSize, iconSize);

                bool isChosen = (formItemIconIndex == i);

                // Nền slot
                GUI.color = isChosen ? new Color(0.2f, 0.4f, 0.8f, 0.9f) : new Color(0.06f, 0.08f, 0.14f, 0.9f);
                GUI.DrawTexture(iconSlot, Texture2D.whiteTexture);
                GUI.color = Color.white;

                // Ảnh icon thật
                Texture2D tex = GetCachedTexture(currentItemIcons[i]);
                GUI.DrawTexture(new Rect(iconSlot.x + 2, iconSlot.y + 2, iconSize - 4, iconSize - 4), tex);

                // Viền chọn vàng kim
                FantasyUITheme.DrawOutline(iconSlot, isChosen ? Color.yellow : new Color(0.35f, 0.45f, 0.6f, 0.5f), isChosen ? 2 : 1);

                if (Event.current.type == EventType.MouseDown && iconSlot.Contains(Event.current.mousePosition))
                {
                    formItemIconIndex = i;
                    Event.current.Use();
                }
            }
            int iconRows = Mathf.CeilToInt(currentItemIcons.Length / (float)iconCols);
            curY += iconRows * (iconSize + iconGap) + 8f;

            // 4.5. CÔNG CỤ TẢI LÊN / NẠP ẢNH NGOÀI (.PNG) CHO ICON VẬT PHẨM
            GUI.Label(new Rect(0, curY, viewRect.width, 18), "📂 Tải Lên / Nạp Icon Ngoài Từ Máy Tính (.png):", lbl);
            curY += 20f;
            customItemImageInputPath = GUI.TextField(new Rect(0, curY, viewRect.width - 90f, 22), customItemImageInputPath);
            if (GUI.Button(new Rect(viewRect.width - 86f, curY, 86f, 22), "Nạp Ảnh"))
            {
                LoadCustomExternalItemImage();
            }
            curY += 24f;
            if (!string.IsNullOrEmpty(customItemImageStatus))
            {
                var sStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = customItemImageStatus.StartsWith("✅") ? Color.green : Color.yellow } };
                GUI.Label(new Rect(0, curY, viewRect.width, 18), customItemImageStatus, sStyle);
                curY += 20f;
            }

            // 5. Ô Điền Biến Chỉ Số Thật Của Vật Phẩm (Dam, Giáp, HP, MP, INT, STR, VIT, AGI)
            GUI.Label(new Rect(0, curY, viewRect.width, 18), "📊 Ô Điền Biến Chỉ Số Thật:", lbl);
            curY += 20f;

            float statBoxW = viewRect.width;
            Rect iStatBox = new Rect(0, curY, statBoxW, 98f);
            GUI.color = new Color(0.06f, 0.10f, 0.18f, 0.85f);
            GUI.DrawTexture(iStatBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(iStatBox, new Color(0.35f, 0.55f, 0.85f, 0.7f), 1);

            float colW = (statBoxW - 24f) / 2f;
            float isY = curY + 4f;

            // Hàng 1: Công (Dam) & Giáp (Armor)
            GUI.Label(new Rect(8f, isY, colW * 0.55f, 20), "⚔️ Công (Dam):", lbl);
            string oldDmg = formItemBonusDmg;
            formItemBonusDmg = GUI.TextField(new Rect(8f + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusDmg);

            GUI.Label(new Rect(16f + colW, isY, colW * 0.55f, 20), "🛡️ Giáp Thủ:", lbl);
            string oldArm = formItemBonusArmor;
            formItemBonusArmor = GUI.TextField(new Rect(16f + colW + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusArmor);
            isY += 22f;

            // Hàng 2: Máu & Mana
            GUI.Label(new Rect(8f, isY, colW * 0.55f, 20), "❤️ Máu (HP):", lbl);
            string oldHp = formItemBonusHp;
            formItemBonusHp = GUI.TextField(new Rect(8f + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusHp);

            GUI.Label(new Rect(16f + colW, isY, colW * 0.55f, 20), "💧 Mana (MP):", lbl);
            string oldMp = formItemBonusMp;
            formItemBonusMp = GUI.TextField(new Rect(16f + colW + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusMp);
            isY += 22f;

            // Hàng 3: INT & STR
            GUI.Label(new Rect(8f, isY, colW * 0.55f, 20), "🧠 Trí Tuệ (INT):", lbl);
            string oldInt = formItemBonusInt;
            formItemBonusInt = GUI.TextField(new Rect(8f + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusInt);

            GUI.Label(new Rect(16f + colW, isY, colW * 0.55f, 20), "💪 Sức Mạnh (STR):", lbl);
            string oldStr = formItemBonusStr;
            formItemBonusStr = GUI.TextField(new Rect(16f + colW + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusStr);
            isY += 22f;

            // Hàng 4: VIT & AGI
            GUI.Label(new Rect(8f, isY, colW * 0.55f, 20), "🛡️ Thể Lực (VIT):", lbl);
            string oldVit = formItemBonusVit;
            formItemBonusVit = GUI.TextField(new Rect(8f + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusVit);

            GUI.Label(new Rect(16f + colW, isY, colW * 0.55f, 20), "⚡ Nhanh Nhẹn (AGI):", lbl);
            string oldAgi = formItemBonusAgi;
            formItemBonusAgi = GUI.TextField(new Rect(16f + colW + colW * 0.55f, isY, colW * 0.45f, 20), formItemBonusAgi);

            if (oldDmg != formItemBonusDmg || oldArm != formItemBonusArmor || oldHp != formItemBonusHp || oldMp != formItemBonusMp ||
                oldInt != formItemBonusInt || oldStr != formItemBonusStr || oldVit != formItemBonusVit || oldAgi != formItemBonusAgi)
            {
                SyncItemStatsString();
            }

            curY += 104f;

            // 6. Chỉ Số Thuộc Tính & Mô Tả
            GUI.Label(new Rect(0, curY, 90, 20), "Chuỗi Chỉ Số:", lbl);
            formItemStats = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), formItemStats);
            curY += 26f;

            GUI.Label(new Rect(0, curY, 90, 20), "Mô Tả Vật Phẩm:", lbl);
            formItemDesc = GUI.TextArea(new Rect(95, curY, viewRect.width - 95, 42), formItemDesc);
            curY += 48f;

            GUI.EndScrollView();
        }

        // ================= CỘT 2: BẢNG FORM THIẾT KẾ KỸ NĂNG =================
        private void DrawSkillFormColumn(Rect r)
        {
            float totalContentH = 800f;
            Rect viewRect = new Rect(0, 0, r.width - 16f, Mathf.Max(r.height, totalContentH));
            formScrollPos = GUI.BeginScrollView(r, formScrollPos, viewRect, false, true);

            float curY = 4f;
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.95f, 1f) } };

            // 1. Tên Chiêu & ID
            GUI.Label(new Rect(0, curY, 90, 20), "Tên Kỹ Năng:", lbl);
            formSkillName = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), formSkillName);
            curY += 26f;

            GUI.Label(new Rect(0, curY, 90, 20), "Mã Chiêu (ID):", lbl);
            formSkillId = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), formSkillId);
            curY += 28f;

            // 1.5. Chức Nghiệp Yêu Cầu
            GUI.Label(new Rect(0, curY, 90, 20), "Chức Nghiệp:", lbl);
            formSkillClassIndex = GUI.Toolbar(new Rect(95, curY, viewRect.width - 95, 24), formSkillClassIndex, classNames);
            curY += 28f;

            // 2. Hệ Nguyên Tố & Phẩm Cấp
            GUI.Label(new Rect(0, curY, 90, 20), "Hệ Nguyên Tố:", lbl);
            formSkillElemIndex = GUI.Toolbar(new Rect(95, curY, viewRect.width - 95, 24), formSkillElemIndex, elemNames);
            curY += 28f;

            // 2.5. Tác Dụng Chiêu Thức (Tấn Công vs Hồi Phục vs Buff)
            GUI.Label(new Rect(0, curY, 90, 20), "Tác Dụng:", lbl);
            formSkillCategoryIndex = GUI.Toolbar(new Rect(95, curY, viewRect.width - 95, 24), formSkillCategoryIndex, categoryNames);
            curY += 28f;

            GUI.Label(new Rect(0, curY, 90, 20), "Phẩm Cấp:", lbl);
            float tierBtnW = (viewRect.width - 95f - 18f) / 7f;
            for (int t = 0; t < tierNames.Length; t++)
            {
                bool isSelected = (formTierIndex == t);
                Color tCol = tierColors[t];
                GUI.color = isSelected ? tCol : new Color(tCol.r * 0.4f, tCol.g * 0.4f, tCol.b * 0.4f, 0.9f);

                string tShort = tierNames[t].Replace("Phẩm ", "");
                if (GUI.Button(new Rect(95 + t * (tierBtnW + 3), curY, tierBtnW, 24), tShort))
                {
                    formTierIndex = t;
                }
            }
            GUI.color = Color.white;
            curY += 28f;

            // 3. Cơ Chế Tâm Ngắm (Line, AoE, Cone, Nova, Buff)
            GUI.Label(new Rect(0, curY, 90, 20), "Tâm Ngắm:", lbl);
            formSkillIndicatorIndex = GUI.SelectionGrid(new Rect(95, curY, viewRect.width - 95, 46), formSkillIndicatorIndex, indicatorNames, 3);
            curY += 50f;

            // 4. CÁC Ô ĐIỀN BIẾN CHỈ SỐ THẬT (SÁT THƯƠNG, MANA, TẦM ĐÁNH, HỒI CHIÊU, % TRÍ TUỆ)
            GUI.Label(new Rect(0, curY, viewRect.width, 18), "🎯 Các Ô Điền Biến Chỉ Số Thật:", lbl);
            curY += 20f;

            float statBoxW = viewRect.width;
            Rect sStatBox = new Rect(0, curY, statBoxW, 98f);
            GUI.color = new Color(0.06f, 0.10f, 0.18f, 0.85f);
            GUI.DrawTexture(sStatBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(sStatBox, new Color(0.35f, 0.55f, 0.85f, 0.7f), 1);

            float colW = (statBoxW - 24f) / 2f;
            float sY = curY + 4f;

            // Hàng 1: Dam (Sát thương) & Mana
            string dmgTitle = (formSkillCategoryIndex == 1) ? "❤️ Lượng Hồi HP:" :
                              (formSkillCategoryIndex == 2) ? "💧 Lượng Hồi Mana:" :
                              (formSkillCategoryIndex == 3) ? "🛡️ Lực Cường Hóa:" : "⚔️ Sát Thương (Dam):";
            GUI.Label(new Rect(8f, sY, colW * 0.55f, 20), dmgTitle, lbl);
            formSkillDmg = GUI.TextField(new Rect(8f + colW * 0.55f, sY, colW * 0.45f, 20), formSkillDmg);

            GUI.Label(new Rect(16f + colW, sY, colW * 0.55f, 20), "💧 Mất Mấy Mana:", lbl);
            formSkillMana = GUI.TextField(new Rect(16f + colW + colW * 0.55f, sY, colW * 0.45f, 20), formSkillMana);
            sY += 24f;

            // Hàng 2: Tầm đánh (Range) & Hồi chiêu (CD)
            GUI.Label(new Rect(8f, sY, colW * 0.55f, 20), "🎯 Tầm Đánh (mét):", lbl);
            formSkillRange = GUI.TextField(new Rect(8f + colW * 0.55f, sY, colW * 0.45f, 20), formSkillRange);

            GUI.Label(new Rect(16f + colW, sY, colW * 0.55f, 20), "⏱️ Hồi Chiêu (giây):", lbl);
            formSkillCd = GUI.TextField(new Rect(16f + colW + colW * 0.55f, sY, colW * 0.45f, 20), formSkillCd);
            sY += 24f;

            // Hàng 3: Tỉ lệ Trí tuệ (% INT) & Live Preview tính toán
            GUI.Label(new Rect(8f, sY, colW * 0.55f, 20), "🧠 Tỉ Lệ (% INT):", lbl);
            formSkillScaling = GUI.TextField(new Rect(8f + colW * 0.55f, sY, colW * 0.45f, 20), formSkillScaling);

            int baseD = int.TryParse(formSkillDmg, out int bd) ? bd : 0;
            float sc = float.TryParse(formSkillScaling, out float scp) ? (scp / 100f) : 0f;
            var pStats = FindAnyObjectByType<PlayerStats>();
            int playerInt = (pStats != null) ? pStats.Intelligence : 12;
            int totalD = baseD + Mathf.RoundToInt(playerInt * sc);
            string previewCalc = $"💥 Vào Dam: <color=#ffff00>{totalD} ST</color> ({baseD} + {scp:0}% INT)";
            var calcStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleLeft };
            GUI.Label(new Rect(16f + colW, sY, colW, 20), previewCalc, calcStyle);

            curY += 104f;

            // 4.5. THÔNG SỐ ĐƯỜNG ĐẠN & HOẠT ẢNH THỦ TỤC (PROCEDURAL BALLISTICS 140 BIẾN THỂ)
            GUI.Label(new Rect(0, curY, viewRect.width, 18), "🚀 Hoạt Ảnh & Đạn Đạo Thủ Tục (140 Biến Thể):", lbl);
            curY += 20f;

            float vfxBoxW = viewRect.width;
            Rect vfxBox = new Rect(0, curY, vfxBoxW, 138f);
            GUI.color = new Color(0.08f, 0.12f, 0.22f, 0.90f);
            GUI.DrawTexture(vfxBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(vfxBox, new Color(0.85f, 0.55f, 0.15f, 0.7f), 1);

            float vY = curY + 6f;
            GUI.Label(new Rect(8f, vY, 90, 20), "Phôi Khối:", lbl);
            formSkillShapeIndex = GUI.SelectionGrid(new Rect(100f, vY, vfxBoxW - 108f, 38f), formSkillShapeIndex, shapeNames, 4);
            vY += 42f;

            GUI.Label(new Rect(8f, vY, 90, 20), "Quỹ Đạo:", lbl);
            formSkillTrajectoryIndex = GUI.SelectionGrid(new Rect(100f, vY, vfxBoxW - 108f, 38f), formSkillTrajectoryIndex, trajectoryNames, 3);
            vY += 42f;

            // ⚡ TỐC ĐỘ HOẠT ẢNH & BAY (ANIMATION & FLY SPEED)
            float curFlySpd = float.TryParse(formSkillFlySpeed, out float fsp) ? fsp : 16.0f;
            GUI.Label(new Rect(8f, vY, 150, 20), $"⏱️ Tốc Độ Hoạt Ảnh: {curFlySpd:0.#}m/s", lbl);
            float newFlySpd = GUI.HorizontalSlider(new Rect(160f, vY + 4f, vfxBoxW - 230f, 18f), curFlySpd, 3.0f, 35.0f);
            if (Mathf.Abs(newFlySpd - curFlySpd) > 0.1f)
            {
                formSkillFlySpeed = $"{newFlySpd:0.#}";
            }
            formSkillFlySpeed = GUI.TextField(new Rect(vfxBoxW - 62f, vY, 54f, 20f), formSkillFlySpeed);
            vY += 24f;

            // Nút Preset Tốc Độ Hoạt Ảnh Nhanh
            float spdBtnW = (vfxBoxW - 24f) / 3f;
            if (GUI.Button(new Rect(8f, vY, spdBtnW, 20f), "🐢 Chậm (6 m/s)")) formSkillFlySpeed = "6.0";
            if (GUI.Button(new Rect(8f + spdBtnW + 4f, vY, spdBtnW, 20f), "▶️ Chuẩn (16 m/s)")) formSkillFlySpeed = "16.0";
            if (GUI.Button(new Rect(8f + (spdBtnW + 4f) * 2f, vY, spdBtnW, 20f), "⚡ Siêu Tốc (28 m/s)")) formSkillFlySpeed = "28.0";
            vY += 25f;

            float halfColW = (vfxBoxW - 24f) / 2f;
            GUI.Label(new Rect(8f, vY, halfColW * 0.55f, 20), "💥 Bán Kính Nổ:", lbl);
            formSkillExplosionRadius = GUI.TextField(new Rect(8f + halfColW * 0.55f, vY, halfColW * 0.45f, 20), formSkillExplosionRadius);

            formSkillHasTrail = GUI.Toggle(new Rect(16f + halfColW, vY, halfColW, 20), formSkillHasTrail, " ✨ Trail Vệt Sáng");
            vY += 24f;

            formSkillHasScreenShake = GUI.Toggle(new Rect(8f, vY, halfColW, 20), formSkillHasScreenShake, " 📳 Rung Màn Hình");
            curY += 168f;

            if (formSkillIndicatorIndex == 1) // Vùng Đất / Mưa (AoE Rain)
            {
                Rect rainBox = new Rect(0, curY, vfxBoxW, 86f);
                GUI.color = new Color(0.10f, 0.16f, 0.28f, 0.95f);
                GUI.DrawTexture(rainBox, Texture2D.whiteTexture);
                GUI.color = Color.white;
                FantasyUITheme.DrawOutline(rainBox, new Color(0.35f, 0.85f, 1.0f, 0.8f), 1);

                float rY = curY + 6f;
                GUI.Label(new Rect(8f, rY, 110, 20), "📐 Hướng Rơi Mưa:", lbl);
                formSkillRainFallAngle = GUI.Toolbar(new Rect(120f, rY, vfxBoxW - 128f, 22f), formSkillRainFallAngle, fallAngleNames);
                rY += 26f;

                GUI.Label(new Rect(8f, rY, 110, 20), "🌀 Kiểu Rơi Mưa:", lbl);
                formSkillRainFallStyle = GUI.Toolbar(new Rect(120f, rY, vfxBoxW - 128f, 22f), formSkillRainFallStyle, fallStyleNames);
                rY += 26f;

                var tipStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.5f, 0.9f, 1f) } };
                GUI.Label(new Rect(8f, rY, vfxBoxW - 16f, 18), "💡 Đạn mưa phát nổ chính xác tại mặt đất trong vòng chọn, không văng xuyên màn hình!", tipStyle);
                curY += 92f;
            }

            // 5. KHO ICON HOTBAR (Ảnh Thật + Icon Tự Nạp HOẶC Fallback Cuộn Trục)
            formUseSkillHotbarIcon = GUI.Toggle(new Rect(0, curY, viewRect.width, 22), formUseSkillHotbarIcon, " ✨ Sử dụng Icon Ô Chiêu riêng (Tắt: Fallback sang Cuộn Trục)");
            curY += 24f;

            if (formUseSkillHotbarIcon)
            {
                GUI.Label(new Rect(0, curY, viewRect.width, 18), "🎯 Chọn Icon Ô Chiêu Hotbar [1..4]:", lbl);
                curY += 20f;
                string[] currentHotbarList = GetAllHotbarIcons();
                DrawVisualIconGrid(ref formSkillHotbarIconIndex, currentHotbarList, ref curY, viewRect.width);

                // 5.5. CÔNG CỤ NẠP ẢNH NGOÀI (.PNG) CHO MODDER / CREATOR
                GUI.Label(new Rect(0, curY, viewRect.width, 18), "📂 Nạp Ảnh Ngoài Từ Máy Tính (.png):", lbl);
                curY += 20f;
                customImageInputPath = GUI.TextField(new Rect(0, curY, viewRect.width - 90f, 22), customImageInputPath);
                if (GUI.Button(new Rect(viewRect.width - 86f, curY, 86f, 22), "Nạp Ảnh"))
                {
                    LoadCustomExternalImage();
                }
                curY += 24f;
                if (!string.IsNullOrEmpty(customImageStatus))
                {
                    var sStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = customImageStatus.StartsWith("✅") ? Color.green : Color.yellow } };
                    GUI.Label(new Rect(0, curY, viewRect.width, 18), customImageStatus, sStyle);
                    curY += 20f;
                }
            }
            else
            {
                var fbStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.35f, 0.92f, 1f) } };
                GUI.Label(new Rect(0, curY, viewRect.width, 36), "📜 Đang áp dụng Phong Cách Cổ Điển (Huyền Bí):\nÔ chiêu sẽ hiển thị Cuộn Trục Cổ Xưa của Hệ thay vì Icon riêng!", fbStyle);
                curY += 40f;
            }

            // 6. CUỘN TRỤC TÚI ĐỒ (Tự động gán theo Hệ mang Ấn Ký)
            GUI.Label(new Rect(0, curY, viewRect.width, 18), $"🎒 Cuộn Trục Túi Đồ: Hệ {elemNames[formSkillElemIndex]} (Tự động gán)", lbl);
            curY += 20f;
            Texture2D scrollPreview = GetCachedTexture(elemScrollIcons[formSkillElemIndex]);
            Rect scrollSlotRect = new Rect(0, curY, 36f, 36f);
            GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.9f);
            GUI.DrawTexture(scrollSlotRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(scrollSlotRect.x + 2, scrollSlotRect.y + 2, 32f, 32f), scrollPreview);
            FantasyUITheme.DrawOutline(scrollSlotRect, tierColors[formTierIndex], 1);
            curY += 44f;

            // 7. Mô Tả Chiêu
            GUI.Label(new Rect(0, curY, 90, 20), "Mô Tả Kỹ Năng:", lbl);
            formSkillDesc = GUI.TextArea(new Rect(95, curY, viewRect.width - 95, 42), formSkillDesc);
            curY += 48f;

            GUI.EndScrollView();
        }

        private void DrawVisualIconGrid(ref int selectedIndex, string[] iconNames, ref float curY, float totalW)
        {
            float iconSize = 34f;
            float gap = 6f;
            int cols = 6;

            for (int i = 0; i < iconNames.Length; i++)
            {
                int col = i % cols;
                int row = i / cols;
                Rect slot = new Rect(col * (iconSize + gap), curY + row * (iconSize + gap), iconSize, iconSize);

                bool isChosen = (selectedIndex == i);

                GUI.color = isChosen ? new Color(0.2f, 0.4f, 0.8f, 0.9f) : new Color(0.06f, 0.08f, 0.14f, 0.9f);
                GUI.DrawTexture(slot, Texture2D.whiteTexture);
                GUI.color = Color.white;

                Texture2D tex = GetCachedTexture(iconNames[i]);
                GUI.DrawTexture(new Rect(slot.x + 2, slot.y + 2, iconSize - 4, iconSize - 4), tex);

                FantasyUITheme.DrawOutline(slot, isChosen ? Color.yellow : new Color(0.35f, 0.45f, 0.6f, 0.5f), isChosen ? 2 : 1);

                if (Event.current.type == EventType.MouseDown && slot.Contains(Event.current.mousePosition))
                {
                    selectedIndex = i;
                    Event.current.Use();
                }
            }
            int totalRows = Mathf.CeilToInt(iconNames.Length / (float)cols);
            curY += totalRows * (iconSize + gap) + 6f;
        }

        // ================= CỘT 3: LIVE PREVIEW 1:1 & CÁC NÚT TẠO =================
        private void DrawItemPreviewColumn(Rect r)
        {
            float pY = r.y + 4f;

            // 1. Thẻ Xem Trước Ô Đồ (Slot Preview)
            Rect slotBox = new Rect(r.x + (r.width - 56f) / 2f, pY, 56f, 56f);
            GUI.color = new Color(0.04f, 0.06f, 0.12f, 0.98f);
            GUI.DrawTexture(slotBox, Texture2D.whiteTexture);
            GUI.color = Color.white;

            string[] allItemIcons = GetAllItemIcons();
            string currentItemIcon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];
            Texture2D itemTex = GetCachedTexture(currentItemIcon);
            GUI.DrawTexture(new Rect(slotBox.x + 4, slotBox.y + 4, 48, 48), itemTex);
            FantasyUITheme.DrawOutline(slotBox, tierColors[formTierIndex], 2);
            pY += 66f;

            // 2. Khung Giả Lập Tooltip Thực Tế Trong Game
            Rect tipBox = new Rect(r.x, pY, r.width, 180f);
            FantasyUITheme.DrawDoubleGoldFrame(tipBox, 1);

            var tTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };
            GUI.Label(new Rect(tipBox.x + 6, tipBox.y + 6, tipBox.width - 12, 18), formItemName, tTitleStyle);

            // Vạch vàng
            GUI.color = new Color(0.85f, 0.70f, 0.30f, 0.6f);
            GUI.DrawTexture(new Rect(tipBox.x + 8, tipBox.y + 26, tipBox.width - 16, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var tBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            string slotStr = (formTypeIndex == 0) ? $"Vị Trí: {slotNames[formSlotIndex]}" : $"Loại: {typeNames[formTypeIndex]}";
            string bodyText = $"• Phẩm: <color=#{ColorUtility.ToHtmlStringRGB(tierColors[formTierIndex])}>{tierNames[formTierIndex]}</color> | {slotStr}\n" +
                              $"• {formItemStats}\n" +
                              $"• {formItemDesc}";

            GUI.Label(new Rect(tipBox.x + 8, tipBox.y + 30, tipBox.width - 16, 140f), bodyText, tBodyStyle);
            pY += 194f;

            // 3. CÁC NÚT HÀNH ĐỘNG NỔI BẬT
            float btnH = 26f;
            float btnGap = 4f;
            GUI.color = new Color(0.2f, 0.95f, 0.4f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "➕ TẠO VÀO TÚI ĐỒ"))
            {
                CreateAndAddItem();
            }
            pY += btnH + btnGap;

            GUI.color = new Color(0.2f, 0.85f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "💾 LƯU DATABASE (JSON)"))
            {
                SaveItemToDatabase();
                studioExportStatus = $"✅ Đã lưu '{formItemName}' vào JSON database!";
            }
            pY += btnH + btnGap;

            GUI.color = new Color(1f, 0.85f, 0.35f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "📋 SAO CHÉP MÃ C#"))
            {
                GUIUtility.systemCopyBuffer = GenerateItemCSharpCode();
                studioExportStatus = "✅ Đã copy mã C# vật phẩm vào Clipboard!";
            }
            pY += btnH + btnGap;
            GUI.color = Color.white;

            if (!string.IsNullOrEmpty(studioExportStatus))
            {
                var stStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = studioExportStatus.StartsWith("✅") ? Color.green : Color.yellow } };
                GUI.Label(new Rect(r.x, pY, r.width, 30), studioExportStatus, stStyle);
            }
        }

        private void DrawSkillPreviewColumn(Rect r)
        {
            float pY = r.y + 4f;

            // 1. Thẻ Xem Trước Ô Kỹ Năng Hotbar
            Rect slotBox = new Rect(r.x + (r.width - 56f) / 2f, pY, 56f, 56f);
            GUI.color = new Color(0.04f, 0.06f, 0.12f, 0.98f);
            GUI.DrawTexture(slotBox, Texture2D.whiteTexture);
            GUI.color = Color.white;

            string previewHotbarIconName = formUseSkillHotbarIcon ? GetAllHotbarIcons()[formSkillHotbarIconIndex] : elemScrollIcons[formSkillElemIndex];
            Texture2D hotbarTex = GetCachedTexture(previewHotbarIconName);
            GUI.DrawTexture(new Rect(slotBox.x + 4, slotBox.y + 4, 48, 48), hotbarTex);
            FantasyUITheme.DrawOutline(slotBox, tierColors[formTierIndex], 2);

            // Badge phím mẫu
            var bStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.9f, 0.4f) } };
            GUI.Label(new Rect(slotBox.x + 6, slotBox.y + 4, 20, 16), "1", bStyle);

            // Badge mana mẫu ở góc trên
            var mStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.35f, 0.88f, 1.0f) } };
            GUI.Label(new Rect(slotBox.x + (slotBox.width - 48f) / 2f, slotBox.y - 18f, 48f, 16f), $"-{formSkillMana}mn", mStyle);
            pY += 66f;

            // 2. Khung Giả Lập Tooltip Kỹ Năng
            Rect tipBox = new Rect(r.x, pY, r.width, 180f);
            FantasyUITheme.DrawDoubleGoldFrame(tipBox, 1);

            var tTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };
            GUI.Label(new Rect(tipBox.x + 8, tipBox.y + 6, tipBox.width - 16, 18), formSkillName, tTitleStyle);

            // Vạch vàng
            GUI.color = new Color(0.85f, 0.70f, 0.30f, 0.6f);
            GUI.DrawTexture(new Rect(tipBox.x + 8, tipBox.y + 26, tipBox.width - 16, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var tBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            int baseD = int.TryParse(formSkillDmg, out int dVal) ? dVal : 45;
            int sc = int.TryParse(formSkillScaling, out int sVal) ? sVal : 80;
            int calcDmg = baseD + Mathf.RoundToInt(12 * (sc / 100f)); // Với INT mẫu = 12

            string effectName = (formSkillCategoryIndex == 1) ? "Hồi HP" :
                                (formSkillCategoryIndex == 2) ? "Hồi MP" :
                                (formSkillCategoryIndex == 3) ? "Lực Buff" : "Sát thương";

            string bodyText = $"• Phẩm: <color=#{ColorUtility.ToHtmlStringRGB(tierColors[formTierIndex])}>{tierNames[formTierIndex]}</color> | Hệ: {elemNames[formSkillElemIndex]} | Phái: {classNames[formSkillClassIndex]}\n" +
                              $"• Tác dụng: <color=#00e5ff>{categoryNames[formSkillCategoryIndex]}</color> | Tầm: {formSkillRange}m\n" +
                              $"• Tiêu hao: {formSkillMana} Mana | Hồi: {formSkillCd}s\n" +
                              $"• {effectName}: {baseD} + {sc}% INT = <color=yellow>{calcDmg}</color>\n" +
                              $"• {formSkillDesc}";

            GUI.Label(new Rect(tipBox.x + 8, tipBox.y + 30, tipBox.width - 16, 140f), bodyText, tBodyStyle);
            pY += 194f;

            // 3. CÁC NÚT HÀNH ĐỘNG
            float btnH = 25f;
            float btnGap = 4f;

            // NÚT BẮN THỬ CHIÊU TRỰC TIẾP TRONG GAME (TEST CAST PROCEDURAL VFX)
            GUI.color = new Color(1.0f, 0.40f, 0.15f);
            if (GUI.Button(new Rect(r.x, pY, r.width, 28f), "🔥 BẮN THỬ CHIÊU (TEST CAST 140 BIẾN THỂ)"))
            {
                TestCastProceduralSkill();
            }
            pY += 32f;

            GUI.color = new Color(0.2f, 0.95f, 0.4f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "➕ TẠO BÍ KÍP VÀO TÚI"))
            {
                CreateAndAddSkill();
            }
            pY += btnH + btnGap;

            GUI.color = new Color(0.2f, 0.85f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "💾 LƯU BÍ KÍP (JSON)"))
            {
                SaveSkillToDatabase();
                studioExportStatus = $"✅ Đã lưu '{formSkillName}' vào JSON database!";
            }
            pY += btnH + btnGap;

            GUI.color = new Color(1f, 0.85f, 0.35f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "📋 SAO CHÉP MÃ C#"))
            {
                GUIUtility.systemCopyBuffer = GenerateSkillCSharpCode();
                studioExportStatus = "✅ Đã copy mã C# vào Clipboard! Paste thẳng vào code là chạy!";
            }
            pY += btnH + btnGap;

            GUI.color = new Color(0.6f, 0.85f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "📁 XUẤT RA FILE C# DỰ ÁN"))
            {
                ExportSkillToCSharpFile();
            }
            pY += btnH + btnGap;

            GUI.color = new Color(0.9f, 0.6f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "🌐 MỞ WEB STUDIO CHO TEAM"))
            {
                OpenWebStudio();
            }
            pY += btnH + btnGap;
            GUI.color = Color.white;

            if (!string.IsNullOrEmpty(studioExportStatus))
            {
                var stStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = studioExportStatus.StartsWith("✅") ? Color.green : Color.yellow } };
                GUI.Label(new Rect(r.x, pY, r.width, 32), studioExportStatus, stStyle);
            }
        }

        private void CreateAndAddItem()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null) return;

            if (inv.IsFull)
            {
                studioExportStatus = "⚠️ Hành trang đã đầy (24/24)! Hãy vứt bớt đồ trước khi tạo.";
                return;
            }

            ItemType t = (formTypeIndex == 0) ? ItemType.Equipment :
                         (formTypeIndex == 1) ? ItemType.Consumable :
                         (formTypeIndex == 2) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string[] allItemIcons = GetAllItemIcons();
            string icon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];

            var newItem = new InventoryItem(formItemId, formItemName, t, s, tier, c, icon, formItemStats, formItemDesc, 1);
            if (!inv.AddItem(newItem))
            {
                studioExportStatus = "⚠️ Hành trang đã đầy (24/24)! Không thể tạo thêm.";
                return;
            }
            studioExportStatus = $"✅ Đã tạo thành công '{formItemName}' vào túi đồ!";
            Debug.Log($"<color=green>[GODLESS STUDIO] Đã tạo thành công vật phẩm '{formItemName}' vào túi đồ!</color>");
        }

        private void SaveItemToDatabase()
        {
            if (ItemDatabase.Instance == null)
            {
                var obj = new GameObject("ItemDatabase");
                obj.AddComponent<ItemDatabase>();
            }

            ItemType t = (formTypeIndex == 0) ? ItemType.Equipment :
                         (formTypeIndex == 1) ? ItemType.Consumable :
                         (formTypeIndex == 2) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string[] allItemIcons = GetAllItemIcons();
            string icon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];

            ItemDatabase.Instance.RegisterNewItemFromForm(formItemId, formItemName, t, s, tier, c, icon, formItemStats, formItemDesc, 1);
            studioExportStatus = $"✅ Đã lưu '{formItemName}' vào JSON database!";
        }

        private void CreateAndAddSkill()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null) return;

            if (inv.IsFull)
            {
                studioExportStatus = "⚠️ Hành trang đã đầy (24/24)! Hãy vứt bớt đồ trước khi tạo.";
                return;
            }

            if (SkillDatabase.Instance == null)
            {
                var sObj = new GameObject("SkillDatabase");
                sObj.AddComponent<SkillDatabase>();
            }

            int mana = int.TryParse(formSkillMana, out int mVal) ? mVal : 25;
            float cd = float.TryParse(formSkillCd, out float cdVal) ? cdVal : 3.5f;
            int dmg = int.TryParse(formSkillDmg, out int dmgVal) ? dmgVal : 45;
            float scaling = float.TryParse(formSkillScaling, out float scVal) ? (scVal / 100f) : 0.8f;
            float rangeVal = float.TryParse(formSkillRange, out float rVal) ? rVal : 8.0f;

            string scrollIcon = elemScrollIcons[formSkillElemIndex];
            string hotbarIcon = formUseSkillHotbarIcon ? GetAllHotbarIcons()[formSkillHotbarIconIndex] : scrollIcon;

            var skillItem = SkillDatabase.Instance.RegisterAndCreateSkillItem(
                formSkillId, formSkillName, elemNames[formSkillElemIndex], tierNames[formTierIndex],
                tierColors[formTierIndex], mana, cd, dmg, scaling, formSkillIndicatorIndex + 1,
                hotbarIcon, scrollIcon, formSkillDesc, rangeVal, classNames[formSkillClassIndex], "",
                formSkillCategoryIndex,
                formSkillShapeIndex, formSkillTrajectoryIndex,
                float.TryParse(formSkillFlySpeed, out float fs) ? fs : 16.0f,
                float.TryParse(formSkillExplosionRadius, out float er) ? er : 1.8f,
                formSkillHasTrail, formSkillHasScreenShake,
                formSkillRainFallAngle, formSkillRainFallStyle);

            if (!inv.AddItem(skillItem))
            {
                studioExportStatus = "⚠️ Hành trang đã đầy (24/24)! Không thể tạo thêm.";
                return;
            }
            studioExportStatus = $"✅ Đã tạo thành công Cuộn Trục Bí Kíp '{formSkillName}' vào túi đồ!";
            Debug.Log($"<color=green>[GODLESS STUDIO] Đã tạo thành công Cuộn Trục Bí Kíp '{formSkillName}' vào túi đồ!</color>");
        }

        private void TestCastProceduralSkill()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                studioExportStatus = "⚠️ Không tìm thấy Player_Hero trong màn chơi!";
                return;
            }

            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld = (Camera.main != null)
                ? Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 10f))
                : (player.transform.position + Vector3.right * 3f);
            mouseWorld.z = 0f;

            Vector2 aimDir = ((Vector2)(mouseWorld - player.transform.position)).normalized;
            if (aimDir.sqrMagnitude < 0.001f) aimDir = Vector2.right;

            int dmg = int.TryParse(formSkillDmg, out int dVal) ? dVal : 45;
            float spd = float.TryParse(formSkillFlySpeed, out float sp) ? sp : 16.0f;
            float rad = float.TryParse(formSkillExplosionRadius, out float rd) ? rd : 1.8f;
            float rangeVal = float.TryParse(formSkillRange, out float rg) ? rg : 8.0f;
            string elem = elemNames[formSkillElemIndex];
            BaseShapeMask shape = (BaseShapeMask)formSkillShapeIndex;
            TrajectoryType traj = (TrajectoryType)formSkillTrajectoryIndex;
            Vector3 spawnPos = player.transform.position + (Vector3)(aimDir * 0.5f);

            if (ProceduralVFXEngine.Instance == null)
            {
                studioExportStatus = "⚠️ ProceduralVFXEngine chưa khởi tạo trong Scene!";
                return;
            }

            switch (formSkillIndicatorIndex)
            {
                case 1: // Vùng Đất / Mưa (AoE Rain)
                    Vector3 rainTarget = mouseWorld;
                    if (Vector3.Distance(player.transform.position, rainTarget) > rangeVal)
                    {
                        rainTarget = player.transform.position + (Vector3)(aimDir * rangeVal);
                    }
                    ProceduralVFXEngine.Instance.SpawnProceduralAoERain(
                        rainTarget, elem, rad, dmg, player.name,
                        formSkillRainFallAngle, formSkillRainFallStyle, player.transform.position);
                    studioExportStatus = $"🌧️ ĐÃ BẮN THỬ: Mưa [{elem} · {shape}] - Hướng: {fallAngleNames[formSkillRainFallAngle]} · Kiểu: {fallStyleNames[formSkillRainFallStyle]}!";
                    break;

                case 2: // Hình Quạt (Cone)
                    ProceduralVFXEngine.Instance.SpawnProceduralConeWave(
                        player.transform.position, aimDir, elem, rad * 2.2f, dmg, player.name);
                    studioExportStatus = $"🔥 ĐÃ BẮN THỬ: Sóng Quạt [{elem}] Góc 60° Tầm {rad * 2.2f:0.#}m!";
                    break;

                case 3: // Nổ 360° (Nova Ring)
                    ProceduralVFXEngine.Instance.SpawnProceduralRadialNova(
                        player.transform.position, elem, rad * 2f, dmg, player.name);
                    studioExportStatus = $"💥 ĐÃ BẮN THỬ: Nổ Vòng Tròn 360° [{elem}] Bán kính {rad * 2f:0.#}m!";
                    break;

                case 4: // Tự Thân (Self Buff Aura)
                    ProceduralVFXEngine.Instance.SpawnProceduralSelfAura(
                        player.transform, elem, 3.5f);
                    studioExportStatus = $"✨ ĐÃ BẮN THỬ: Hào Quang Tự Thân [{elem}] kèm Cánh Thiên Thần!";
                    break;

                case 5: // Kết Giới Đội (Team Zone)
                    Vector3 zoneTarget = mouseWorld;
                    if (Vector3.Distance(player.transform.position, zoneTarget) > rangeVal)
                    {
                        zoneTarget = player.transform.position + (Vector3)(aimDir * rangeVal);
                    }
                    ProceduralVFXEngine.Instance.SpawnProceduralTeamBarrier(
                        zoneTarget, elem, rad * 2.5f, 4.0f);
                    studioExportStatus = $"🛡️ ĐÃ BẮN THỬ: Kết Giới Đội [{elem}] Bán kính {rad * 2.5f:0.#}m!";
                    break;

                case 0: // Bắn Thẳng (Linear / Sine / Homing)
                default:
                    ProceduralVFXEngine.Instance.FireProceduralSkill(
                        spawnPos, aimDir, shape, elem, traj, dmg, spd, rad,
                        formSkillHasTrail, formSkillHasScreenShake, player.name, null, rangeVal);
                    studioExportStatus = $"🔥 ĐÃ BẮN THỬ: [{shape} · {elem} · {traj}] Tốc độ {spd:0.#}m/s Tầm {rangeVal:0.#}m!";
                    break;
            }

            GodlessFantasy.Core.AudioManager.PlayCast();
            Debug.Log($"<color=orange>[TEST CAST]</color> Đã bắn thử kỹ năng thủ tục: {shape} ({elem})!");
        }

        private void SaveSkillToDatabase()
        {
            CreateAndAddSkill();
        }

        private string GenerateItemCSharpCode()
        {
            ItemType t = (formTypeIndex == 0) ? ItemType.Equipment :
                         (formTypeIndex == 1) ? ItemType.Consumable :
                         (formTypeIndex == 2) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string[] allItemIcons = GetAllItemIcons();
            string icon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];

            return $"// Vật phẩm: {formItemName}\n" +
                   $"var {formItemId.Replace("-", "_")} = new InventoryItem(\"{formItemId}\", \"{formItemName}\", ItemType.{t}, EquipSlot.{s}, \"{tier}\", new Color({c.r:F2}f, {c.g:F2}f, {c.b:F2}f), \"{icon}\", \"{formItemStats}\", \"{formItemDesc}\", 1);";
        }

        private string GenerateSkillCSharpCode()
        {
            int mana = int.TryParse(formSkillMana, out int mVal) ? mVal : 25;
            float cd = float.TryParse(formSkillCd, out float cdVal) ? cdVal : 3.5f;
            int dmg = int.TryParse(formSkillDmg, out int dmgVal) ? dmgVal : 45;
            float scaling = float.TryParse(formSkillScaling, out float scVal) ? (scVal / 100f) : 0.8f;
            float rangeVal = float.TryParse(formSkillRange, out float rVal) ? rVal : 8.0f;

            string scrollIcon = elemScrollIcons[formSkillElemIndex];
            string hotbarIcon = formUseSkillHotbarIcon ? GetAllHotbarIcons()[formSkillHotbarIconIndex] : scrollIcon;
            Color tCol = tierColors[formTierIndex];
            string tierName = tierNames[formTierIndex];
            string elemName = elemNames[formSkillElemIndex];
            string reqClass = classNames[formSkillClassIndex];
            int indType = formSkillIndicatorIndex + 1;
            int catIdx = formSkillCategoryIndex;

            string effectName = (catIdx == 1) ? "Hồi HP" : (catIdx == 2) ? "Hồi MP" : (catIdx == 3) ? "Lực Buff" : "ST";
            string statsStr = $"• [{reqClass} · {elemName}] | {effectName}: {dmg} + {Mathf.RoundToInt(scaling * 100)}% INT | CD: {cd:F1}s | Tầm: {rangeVal:F1}m";
            float spd = float.TryParse(formSkillFlySpeed, out float fsVal) ? fsVal : 16.0f;
            float rad = float.TryParse(formSkillExplosionRadius, out float erVal) ? erVal : 1.8f;
            string varName = formSkillId.Replace("-", "_");

            return $"// Cuộn Trục Bí Kíp: {formSkillName} (140 Biến Thể Procedural VFX)\n" +
                   $"var {varName}_scroll = new SkillScrollData(\"{formSkillId}\", \"{formSkillName}\", \"{tierName}\", new Color({tCol.r:F2}f, {tCol.g:F2}f, {tCol.b:F2}f), \"{elemName}\", {mana}, {cd:F1}f, {dmg}, {scaling:F2}f, {indType}, \"{hotbarIcon}\", \"{scrollIcon}\", \"{formSkillDesc}\", {rangeVal:F1}f, \"{reqClass}\", \"\", {catIdx}) {{\n" +
                   $"    baseShapeMask = {formSkillShapeIndex}, trajectoryType = {formSkillTrajectoryIndex}, flySpeed = {spd:0.#}f, explosionRadius = {rad:0.#}f, hasTrail = {formSkillHasTrail.ToString().ToLower()}, hasScreenShake = {formSkillHasScreenShake.ToString().ToLower()}\n" +
                   $"}};\n" +
                   $"var {varName} = new InventoryItem(\"{formSkillId}\", \"{formSkillName}\", ItemType.SkillScroll, EquipSlot.None, \"{tierName}\", new Color({tCol.r:F2}f, {tCol.g:F2}f, {tCol.b:F2}f), \"{scrollIcon}\", \"{statsStr}\", \"{formSkillDesc}\", 1, {varName}_scroll);";
        }

        private void ExportSkillToCSharpFile()
        {
            try
            {
                string dir = Path.Combine(Application.dataPath, "Scripts", "Database");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string filePath = Path.Combine(dir, "GeneratedSkillsDatabase.cs");

                string skillCode = GenerateSkillCSharpCode();

                if (!File.Exists(filePath))
                {
                    string initialCode = 
@"// =========================================================================
// GODLESS FANTASY - GENERATED SKILLS DATABASE
// Tự động sinh từ Godless Studio Engine (F1)
// Các anh em lập trình viên có thể dùng trực tiếp hoặc gọi hàm nạp vào game!
// =========================================================================
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using System.Collections.Generic;

namespace GodlessFantasy.Database
{
    public static class GeneratedSkillsDatabase
    {
        public static List<InventoryItem> GetGeneratedSkillPresets()
        {
            var list = new List<InventoryItem>();
            // [GEN_START]
" + "            " + skillCode.Replace("\n", "\n            ") + @"
            list.Add(" + formSkillId.Replace("-", "_") + @");
            // [GEN_END]
            return list;
        }
    }
}
";
                    File.WriteAllText(filePath, initialCode);
                }
                else
                {
                    string existing = File.ReadAllText(filePath);
                    string snippet = "            " + skillCode.Replace("\n", "\n            ") + $"\n            list.Add({formSkillId.Replace("-", "_")});\n";
                    if (existing.Contains("// [GEN_END]"))
                    {
                        existing = existing.Replace("// [GEN_END]", snippet + "            // [GEN_END]");
                        File.WriteAllText(filePath, existing);
                    }
                    else
                    {
                        File.AppendAllText(filePath, "\n" + skillCode);
                    }
                }
                studioExportStatus = "✅ Đã lưu vào Assets/Scripts/Database/GeneratedSkillsDatabase.cs!";
                Debug.Log($"<color=green>[GODLESS STUDIO]</color> Đã xuất mã C# kỹ năng '{formSkillName}' vào: {filePath}");
            }
            catch (Exception ex)
            {
                studioExportStatus = $"❌ Lỗi xuất file: {ex.Message}";
            }
        }

        private void OpenWebStudio()
        {
            try
            {
                string[] candidates = new string[]
                {
                    Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "tools", "GodlessSkillStudio", "index.html")),
                    Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "tools", "GodlessSkillStudio", "index.html")),
                    Path.GetFullPath(Path.Combine(Application.dataPath, "..", "tools", "GodlessSkillStudio", "index.html")),
                    "d:\\godless-fantasy\\tools\\GodlessSkillStudio\\index.html",
                    "d:\\godless-fantasy\\client\\tools\\GodlessSkillStudio\\index.html"
                };

                string foundFile = null;
                foreach (var c in candidates)
                {
                    if (File.Exists(c))
                    {
                        foundFile = c;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(foundFile))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = foundFile,
                        UseShellExecute = true
                    });
                    studioExportStatus = "✅ Đã mở Web Studio trên trình duyệt!";
                    Debug.Log($"<color=green>[GODLESS STUDIO]</color> Đã mở Web Studio từ: {foundFile}");
                }
                else
                {
                    studioExportStatus = "⚠️ Không tìm thấy file tools/GodlessSkillStudio/index.html!";
                }
            }
            catch (Exception ex)
            {
                studioExportStatus = $"❌ Lỗi mở web: {ex.Message}";
            }
        }

        // ================= TAB 3: LIVE INVENTORY INSPECTOR (CHỈNH SỬA TÚI ĐỒ LIVE) =================
        private void DrawInventorySlotsListColumn(Rect r)
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null)
            {
                GUI.Label(r, "Không tìm thấy InventoryManager!");
                return;
            }

            var items = inv.Items;
            float itemH = 38f;
            float totalH = Mathf.Max(r.height, items.Count * (itemH + 4f) + 40f);
            Rect viewRect = new Rect(0, 0, r.width - 16f, totalH);

            invSlotsScrollPos = GUI.BeginScrollView(r, invSlotsScrollPos, viewRect, false, true);

            var slotStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                Rect btnRect = new Rect(0, i * (itemH + 4f), viewRect.width, itemH);

                bool isSelected = (selectedInventoryIndex == i);
                if (isSelected) GUI.color = new Color(1f, 0.85f, 0.3f);

                string typeEmoji = (it.itemType == ItemType.SkillScroll) ? "📜" :
                                   (it.itemType == ItemType.Equipment) ? "⚔️" :
                                   (it.itemType == ItemType.Consumable) ? "🧪" :
                                   (it.itemType == ItemType.Pet) ? "🐿️" : "📦";

                string label = $"{typeEmoji} [{i + 1}] {it.itemName} (x{it.quantity})";
                if (GUI.Button(btnRect, label, slotStyle))
                {
                    selectedInventoryIndex = i;
                    LoadItemToLiveEditor(it);
                }
                GUI.color = Color.white;
            }

            GUI.EndScrollView();
        }

        private void LoadItemToLiveEditor(InventoryItem it)
        {
            if (it == null) return;
            editItemName = it.itemName;
            editTierIndex = Mathf.Max(0, Array.IndexOf(tierNames, it.tierRank));
            if (editTierIndex < 0) editTierIndex = 0;
            editItemQuantity = it.quantity.ToString();
            editItemStats = it.statsInfo ?? "";
            editItemDesc = it.description ?? "";
            editItemReqClass = it.requiredClass ?? "Toàn Hệ";
            editItemElement = it.element ?? "Vô Hệ";

            if (it.scrollData != null)
            {
                var s = it.scrollData;
                editSkillMana = s.manaCost.ToString();
                editSkillCd = s.cooldown.ToString("0.#");
                editSkillDmg = s.baseDamage.ToString();
                editSkillScaling = Mathf.RoundToInt(s.intelligenceScaling * 100).ToString();
                editSkillRange = s.castRange.ToString("0.#");
                editSkillShapeIndex = Mathf.Clamp(s.baseShapeMask, 0, shapeNames.Length - 1);
                editSkillTrajectoryIndex = Mathf.Clamp(s.trajectoryType, 0, trajectoryNames.Length - 1);
                editSkillRainFallAngle = Mathf.Clamp(s.rainFallAngle, 0, fallAngleNames.Length - 1);
                editSkillRainFallStyle = Mathf.Clamp(s.rainFallStyle, 0, fallStyleNames.Length - 1);
                editSkillFlySpeed = s.flySpeed.ToString("0.#");
                editSkillExplosionRadius = s.explosionRadius.ToString("0.#");
                editSkillHasTrail = s.hasTrail;
                editSkillHasScreenShake = s.hasScreenShake;
            }
        }

        private void DrawInventoryItemEditorColumn(Rect r)
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null || selectedInventoryIndex < 0 || selectedInventoryIndex >= inv.Items.Count)
            {
                var emptyStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.gray } };
                GUI.Label(r, "👈 Chọn một vật phẩm từ cột bên trái để chỉnh sửa trực tiếp!", emptyStyle);
                return;
            }

            var it = inv.Items[selectedInventoryIndex];
            float totalH = (it.itemType == ItemType.SkillScroll) ? 780f : 520f;
            Rect viewRect = new Rect(0, 0, r.width - 16f, Mathf.Max(r.height, totalH));
            invEditorScrollPos = GUI.BeginScrollView(r, invEditorScrollPos, viewRect, false, true);

            float curY = 4f;
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.95f, 1f) } };

            GUI.Label(new Rect(0, curY, 90, 20), "Tên Ô Đồ:", lbl);
            editItemName = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), editItemName);
            curY += 26f;

            GUI.Label(new Rect(0, curY, 90, 20), "Số Lượng:", lbl);
            editItemQuantity = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), editItemQuantity);
            curY += 26f;

            GUI.Label(new Rect(0, curY, 90, 20), "Phẩm Cấp:", lbl);
            float tierBtnW = (viewRect.width - 95f - 18f) / 7f;
            for (int t = 0; t < tierNames.Length; t++)
            {
                bool isSelected = (editTierIndex == t);
                Color tCol = tierColors[t];
                GUI.color = isSelected ? tCol : new Color(tCol.r * 0.4f, tCol.g * 0.4f, tCol.b * 0.4f, 0.9f);
                string tShort = tierNames[t].Replace("Phẩm ", "");
                if (GUI.Button(new Rect(95 + t * (tierBtnW + 3), curY, tierBtnW, 24), tShort))
                {
                    editTierIndex = t;
                }
            }
            GUI.color = Color.white;
            curY += 30f;

            GUI.Label(new Rect(0, curY, 90, 20), "Chuỗi Chỉ Số:", lbl);
            editItemStats = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), editItemStats);
            curY += 26f;

            GUI.Label(new Rect(0, curY, 90, 20), "Mô Tả:", lbl);
            editItemDesc = GUI.TextArea(new Rect(95, curY, viewRect.width - 95, 42), editItemDesc);
            curY += 48f;

            if (it.itemType == ItemType.SkillScroll && it.scrollData != null)
            {
                GUI.Label(new Rect(0, curY, viewRect.width, 18), "📜 CHỈNH SỬA THÔNG SỐ VẬT LÝ KỸ NĂNG (140 VFX):", lbl);
                curY += 20f;

                float boxW = viewRect.width;
                Rect sBox = new Rect(0, curY, boxW, 280f);
                GUI.color = new Color(0.08f, 0.12f, 0.22f, 0.90f);
                GUI.DrawTexture(sBox, Texture2D.whiteTexture);
                GUI.color = Color.white;
                FantasyUITheme.DrawOutline(sBox, new Color(0.35f, 0.75f, 1f, 0.8f), 1);

                float sy = curY + 6f;
                float halfColW = (boxW - 24f) / 2f;

                GUI.Label(new Rect(8f, sy, halfColW * 0.55f, 20), "⚔️ Sát Thương:", lbl);
                editSkillDmg = GUI.TextField(new Rect(8f + halfColW * 0.55f, sy, halfColW * 0.45f, 20), editSkillDmg);

                GUI.Label(new Rect(16f + halfColW, sy, halfColW * 0.55f, 20), "💧 Mana Cost:", lbl);
                editSkillMana = GUI.TextField(new Rect(16f + halfColW + halfColW * 0.55f, sy, halfColW * 0.45f, 20), editSkillMana);
                sy += 24f;

                GUI.Label(new Rect(8f, sy, halfColW * 0.55f, 20), "🎯 Tầm Đánh:", lbl);
                editSkillRange = GUI.TextField(new Rect(8f + halfColW * 0.55f, sy, halfColW * 0.45f, 20), editSkillRange);

                GUI.Label(new Rect(16f + halfColW, sy, halfColW * 0.55f, 20), "⏱️ Hồi Chiêu:", lbl);
                editSkillCd = GUI.TextField(new Rect(16f + halfColW + halfColW * 0.55f, sy, halfColW * 0.45f, 20), editSkillCd);
                sy += 24f;

                GUI.Label(new Rect(8f, sy, 90, 20), "Phôi Khối:", lbl);
                editSkillShapeIndex = GUI.SelectionGrid(new Rect(100f, sy, boxW - 108f, 38f), editSkillShapeIndex, shapeNames, 4);
                sy += 42f;

                GUI.Label(new Rect(8f, sy, 90, 20), "Quỹ Đạo:", lbl);
                editSkillTrajectoryIndex = GUI.SelectionGrid(new Rect(100f, sy, boxW - 108f, 38f), editSkillTrajectoryIndex, trajectoryNames, 3);
                sy += 42f;

                GUI.Label(new Rect(8f, sy, 100, 20), "Hướng Rơi:", lbl);
                editSkillRainFallAngle = GUI.Toolbar(new Rect(100f, sy, boxW - 108f, 20f), editSkillRainFallAngle, fallAngleNames);
                sy += 24f;

                GUI.Label(new Rect(8f, sy, 100, 20), "Kiểu Rơi:", lbl);
                editSkillRainFallStyle = GUI.Toolbar(new Rect(100f, sy, boxW - 108f, 20f), editSkillRainFallStyle, fallStyleNames);
                sy += 24f;

                // ⚡ TỐC ĐỘ HOẠT ẢNH & BAY (ANIMATION & FLY SPEED)
                float curEditSpd = float.TryParse(editSkillFlySpeed, out float efsp) ? efsp : 16.0f;
                GUI.Label(new Rect(8f, sy, 150, 20), $"⏱️ Tốc Độ Hoạt Ảnh: {curEditSpd:0.#}m/s", lbl);
                float newEditSpd = GUI.HorizontalSlider(new Rect(160f, sy + 4f, boxW - 230f, 18f), curEditSpd, 3.0f, 35.0f);
                if (Mathf.Abs(newEditSpd - curEditSpd) > 0.1f)
                {
                    editSkillFlySpeed = $"{newEditSpd:0.#}";
                }
                editSkillFlySpeed = GUI.TextField(new Rect(boxW - 62f, sy, 54f, 20f), editSkillFlySpeed);
                sy += 24f;

                // Nút Preset
                float eSpdBtnW = (boxW - 24f) / 3f;
                if (GUI.Button(new Rect(8f, sy, eSpdBtnW, 20f), "🐢 Chậm (6 m/s)")) editSkillFlySpeed = "6.0";
                if (GUI.Button(new Rect(8f + eSpdBtnW + 4f, sy, eSpdBtnW, 20f), "▶️ Chuẩn (16 m/s)")) editSkillFlySpeed = "16.0";
                if (GUI.Button(new Rect(8f + (eSpdBtnW + 4f) * 2f, sy, eSpdBtnW, 20f), "⚡ Siêu Tốc (28 m/s)")) editSkillFlySpeed = "28.0";
                sy += 25f;

                GUI.Label(new Rect(8f, sy, halfColW * 0.55f, 20), "💥 Bán Kính:", lbl);
                editSkillExplosionRadius = GUI.TextField(new Rect(8f + halfColW * 0.55f, sy, halfColW * 0.45f, 20), editSkillExplosionRadius);

                editSkillHasTrail = GUI.Toggle(new Rect(16f + halfColW, sy, halfColW, 20), editSkillHasTrail, " ✨ Trail Vệt Sáng");
                sy += 24f;

                editSkillHasScreenShake = GUI.Toggle(new Rect(8f, sy, halfColW, 20), editSkillHasScreenShake, " 📳 Rung Màn");

                curY += 310f;
            }

            // NÚT LƯU THAY ĐỔI TRỰC TIẾP VÀO Ô ĐỒ
            curY += 10f;
            GUI.color = new Color(0.2f, 0.95f, 0.4f);
            if (GUI.Button(new Rect(0, curY, viewRect.width, 32f), "💾 LƯU THAY ĐỔI VÀO Ô HÀNH TRANG TRỰC TIẾP"))
            {
                ApplyChangesToInventorySlot(it);
            }
            GUI.color = Color.white;
            curY += 36f;

            GUI.EndScrollView();
        }

        private void ApplyChangesToInventorySlot(InventoryItem it)
        {
            if (it == null) return;
            it.itemName = editItemName;
            it.tierRank = tierNames[editTierIndex];
            it.tierColor = tierColors[editTierIndex];
            if (int.TryParse(editItemQuantity, out int q)) it.quantity = Mathf.Max(1, q);
            it.statsInfo = editItemStats;
            it.description = editItemDesc;

            if (it.scrollData != null)
            {
                var s = it.scrollData;
                s.scrollName = editItemName;
                s.tierRank = it.tierRank;
                s.tierColor = it.tierColor;
                if (int.TryParse(editSkillMana, out int m)) s.manaCost = m;
                if (float.TryParse(editSkillCd, out float cd)) s.cooldown = cd;
                if (int.TryParse(editSkillDmg, out int dmg)) s.baseDamage = dmg;
                if (float.TryParse(editSkillScaling, out float sc)) s.intelligenceScaling = sc / 100f;
                if (float.TryParse(editSkillRange, out float rg)) s.castRange = rg;
                s.baseShapeMask = editSkillShapeIndex;
                s.trajectoryType = editSkillTrajectoryIndex;
                s.rainFallAngle = editSkillRainFallAngle;
                s.rainFallStyle = editSkillRainFallStyle;
                if (float.TryParse(editSkillFlySpeed, out float spd)) s.flySpeed = spd;
                if (float.TryParse(editSkillExplosionRadius, out float rad)) s.explosionRadius = rad;
                s.hasTrail = editSkillHasTrail;
                s.hasScreenShake = editSkillHasScreenShake;
            }

            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv != null)
            {
                inv.SyncEquippedScrollsToCombat();
            }

            studioExportStatus = $"✅ Đã cập nhật ô đồ '{it.itemName}' ngay tức thì!";
            Debug.Log($"<color=green>[INVENTORY INSPECTOR]</color> Đã cập nhật ô đồ: {it.itemName}");
        }

        private void DrawInventoryLivePreviewColumn(Rect r)
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null || selectedInventoryIndex < 0 || selectedInventoryIndex >= inv.Items.Count)
            {
                var emptyStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.gray } };
                GUI.Label(r, "Chưa chọn vật phẩm nào.", emptyStyle);
                return;
            }

            var it = inv.Items[selectedInventoryIndex];
            float pY = r.y + 4f;

            // 1. Thẻ Xem Trước Ô Đồ
            Rect slotBox = new Rect(r.x + (r.width - 56f) / 2f, pY, 56f, 56f);
            GUI.color = new Color(0.04f, 0.06f, 0.12f, 0.98f);
            GUI.DrawTexture(slotBox, Texture2D.whiteTexture);
            GUI.color = Color.white;

            Texture2D itemTex = GetCachedTexture(it.iconName);
            GUI.DrawTexture(new Rect(slotBox.x + 4, slotBox.y + 4, 48, 48), itemTex);
            FantasyUITheme.DrawOutline(slotBox, it.tierColor, 2);
            pY += 66f;

            // 2. Tooltip Thực Tế
            Rect tipBox = new Rect(r.x, pY, r.width, 180f);
            FantasyUITheme.DrawDoubleGoldFrame(tipBox, 1);

            var tTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };
            GUI.Label(new Rect(tipBox.x + 6, tipBox.y + 6, tipBox.width - 12, 18), it.itemName, tTitleStyle);

            GUI.color = new Color(0.85f, 0.70f, 0.30f, 0.6f);
            GUI.DrawTexture(new Rect(tipBox.x + 8, tipBox.y + 26, tipBox.width - 16, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var tBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            string bodyText = $"• Phẩm: <color=#{ColorUtility.ToHtmlStringRGB(it.tierColor)}>{it.tierRank}</color> | {it.itemType}\n" +
                              $"• Số lượng: x{it.quantity} | {it.RequirementTag}\n" +
                              $"• {it.statsInfo}\n" +
                              $"• {it.description}";
            GUI.Label(new Rect(tipBox.x + 8, tipBox.y + 30, tipBox.width - 16, 140f), bodyText, tBodyStyle);
            pY += 194f;

            // 3. CÁC NÚT THAO TÁC TRỰC TIẾP
            float btnH = 26f;
            float btnGap = 4f;

            if (it.itemType == ItemType.SkillScroll && it.scrollData != null)
            {
                GUI.color = new Color(1.0f, 0.40f, 0.15f);
                if (GUI.Button(new Rect(r.x, pY, r.width, 28f), "🔥 TEST CAST CHIÊU NÀY"))
                {
                    TestCastSpecificSkill(it.scrollData);
                }
                pY += 32f;
            }

            GUI.color = new Color(0.2f, 0.85f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "📋 SAO CHÉP (NHÂN ĐÔI Ô)"))
            {
                if (inv.IsFull)
                {
                    studioExportStatus = "⚠️ Túi đồ đã đầy (24/24)!";
                }
                else
                {
                    var clone = new InventoryItem(
                        it.id + "_copy", it.itemName + " (Bản Sao)", it.itemType, it.equipSlot,
                        it.tierRank, it.tierColor, it.iconName, it.statsInfo, it.description,
                        it.quantity, it.scrollData?.Clone(), it.requiredClass, it.element);
                    inv.AddItem(clone);
                    studioExportStatus = $"✅ Đã nhân đôi ô '{it.itemName}' vào túi!";
                }
            }
            pY += btnH + btnGap;

            GUI.color = new Color(1f, 0.35f, 0.35f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "🗑️ XÓA Ô NÀY KHỎI TÚI"))
            {
                string removedName = it.itemName;
                inv.Items.RemoveAt(selectedInventoryIndex);
                if (selectedInventoryIndex >= inv.Items.Count) selectedInventoryIndex = Mathf.Max(0, inv.Items.Count - 1);
                studioExportStatus = $"🗑️ Đã xóa '{removedName}' khỏi túi đồ!";
            }
            pY += btnH + btnGap;
            GUI.color = Color.white;

            if (!string.IsNullOrEmpty(studioExportStatus))
            {
                var stStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = studioExportStatus.StartsWith("✅") ? Color.green : Color.yellow } };
                GUI.Label(new Rect(r.x, pY, r.width, 30), studioExportStatus, stStyle);
            }
        }

        private void TestCastSpecificSkill(SkillScrollData s)
        {
            if (s == null) return;
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                studioExportStatus = "⚠️ Không tìm thấy Player_Hero!";
                return;
            }

            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld = (Camera.main != null)
                ? Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 10f))
                : (player.transform.position + Vector3.right * 3f);
            mouseWorld.z = 0f;

            Vector2 aimDir = ((Vector2)(mouseWorld - player.transform.position)).normalized;
            if (aimDir.sqrMagnitude < 0.001f) aimDir = Vector2.right;

            Vector3 spawnPos = player.transform.position + (Vector3)(aimDir * 0.5f);
            BaseShapeMask shape = (BaseShapeMask)s.baseShapeMask;
            TrajectoryType traj = (TrajectoryType)s.trajectoryType;

            if (ProceduralVFXEngine.Instance == null) return;

            switch (s.indicatorType)
            {
                case 2: // AoE Rain
                    Vector3 rainTarget = mouseWorld;
                    if (Vector3.Distance(player.transform.position, rainTarget) > s.castRange)
                    {
                        rainTarget = player.transform.position + (Vector3)(aimDir * s.castRange);
                    }
                    ProceduralVFXEngine.Instance.SpawnProceduralAoERain(
                        rainTarget, s.element, s.explosionRadius, s.baseDamage, player.name,
                        s.rainFallAngle, s.rainFallStyle, player.transform.position);
                    studioExportStatus = $"🌧️ ĐÃ BẮN: Mưa [{s.scrollName}] Hướng: {s.rainFallAngle} · Kiểu: {s.rainFallStyle}!";
                    break;
                case 3: // Cone
                    ProceduralVFXEngine.Instance.SpawnProceduralConeWave(
                        player.transform.position, aimDir, s.element, s.explosionRadius * 2.2f, s.baseDamage, player.name);
                    studioExportStatus = $"🔥 ĐÃ BẮN: Sóng Quạt [{s.scrollName}]!";
                    break;
                case 4: // Nova
                    ProceduralVFXEngine.Instance.SpawnProceduralRadialNova(
                        player.transform.position, s.element, s.explosionRadius * 2f, s.baseDamage, player.name);
                    studioExportStatus = $"💥 ĐÃ BẮN: Nổ 360° [{s.scrollName}]!";
                    break;
                case 5: // Self Aura
                    ProceduralVFXEngine.Instance.SpawnProceduralSelfAura(
                        player.transform, s.element, 3.5f);
                    studioExportStatus = $"✨ ĐÃ BẮN: Hào Quang [{s.scrollName}]!";
                    break;
                case 6: // Team Zone
                    Vector3 zoneTarget = mouseWorld;
                    if (Vector3.Distance(player.transform.position, zoneTarget) > s.castRange)
                    {
                        zoneTarget = player.transform.position + (Vector3)(aimDir * s.castRange);
                    }
                    ProceduralVFXEngine.Instance.SpawnProceduralTeamBarrier(
                        zoneTarget, s.element, s.explosionRadius * 2.5f, 4.0f);
                    studioExportStatus = $"🛡️ ĐÃ BẮN: Kết Giới Đội [{s.scrollName}]!";
                    break;
                case 1:
                default:
                    ProceduralVFXEngine.Instance.FireProceduralSkill(
                        spawnPos, aimDir, shape, s.element, traj, s.baseDamage, s.flySpeed, s.explosionRadius,
                        s.hasTrail, s.hasScreenShake, player.name, null, s.castRange);
                    studioExportStatus = $"🔥 ĐÃ BẮN: [{s.scrollName}] Tốc độ {s.flySpeed}m/s Tầm {s.castRange}m!";
                    break;
            }

            GodlessFantasy.Core.AudioManager.PlayCast();
        }
    }
}
