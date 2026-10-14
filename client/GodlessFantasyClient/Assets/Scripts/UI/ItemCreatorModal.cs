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
        private int studioMode = 0; // 0 = Tạo Vật Phẩm, 1 = Tạo Kỹ Năng, 2 = Live Inventory Inspector, 3 = Xưởng Pixel Art & Hoạt Ảnh 8 Hướng
        private readonly string[] modeTabs = { "⚔️ THIẾT KẾ VẬT PHẨM & TRANG BỊ", "📜 THIẾT KẾ KỸ NĂNG (140 VFX)", "🎒 CHỈNH SỬA TÚI ĐỒ (LIVE INSPECTOR)", "🎨 XƯỞNG PIXEL ART & HOẠT ẢNH 8 HƯỚNG" };

        private Vector2 presetScrollPos;
        private Vector2 formScrollPos;

        // Texture Cache để vẽ icon trực quan
        private Dictionary<string, Texture2D> iconTexCache = new Dictionary<string, Texture2D>();

        // ================= FORM 4: XƯỞNG PIXEL ART & HOẠT ẢNH 8 HƯỚNG (FRAME-BY-FRAME STUDIO) =================
        private const int CANVAS_SIZE = 32;
        private const int TOTAL_STUDIO_FRAMES = 8;
        private int currentFrameIndex = 0; // 0..7
        private int currentEditDirIndex = 0; // 0..7 (South, SouthEast, East, NorthEast, North, NorthWest, West, SouthWest)
        private readonly string[] studioDirLabels = { "Nam (S)", "Đ.Nam (SE)", "Đông (E)", "Đ.Bắc (NE)", "Bắc (N)", "T.Bắc (NW)", "Tây (W)", "T.Nam (SW)" };
        private readonly string[] studioDirCodes = { "south", "south_east", "east", "north_east", "north", "north_west", "west", "south_west" };

        private Color32[][][] multiFramePixels; // [8 hướng][8 frames][CANVAS_SIZE * CANVAS_SIZE]
        private Texture2D[] frameTextures; // 8 textures xem trước cho 8 frame của hướng đang chọn
        private Texture2D canvasTex;
        private Color32[] canvasPixels;

        // Bật/tắt Onion Skin (vệt mờ frame trước để căn nhịp bước đi)
        private bool enableOnionSkin = true;
        private float onionSkinAlpha = 0.35f;

        // Clipboard Copy / Paste / Move
        private Color32[] clipboardPixels = null;
        private int clipboardW = 0;
        private int clipboardH = 0;

        // Trình chiếu xem trước (Live Preview)
        private bool isPreviewPlaying = true;
        private float previewFps = 8.5f;
        private float previewTimer = 0f;
        private int previewCurrentFrame = 0;
        private int previewDirIndex = 0;

        public enum CostumeTool { Pencil, Eraser, FillBucket, EyeDropper, BoxSelect, MoveSelection }
        private CostumeTool currentCostumeTool = CostumeTool.Pencil;
        private Color32 currentCostumeColor = new Color32(235, 185, 50, 255); // Hoàng Kim
        private RectInt costumeBoxSelection = new RectInt(0, 0, 0, 0);
        private bool isBoxDragging = false;
        private Vector2Int boxDragStart;

        private EquipSlot targetCostumeSlot = EquipSlot.Costume;
        private string costumeItemName = "Pháp Sư Áo Choàng Đỏ";
        private string costumeItemId = "custom_mage_costume";
        private int costumeTierIdx = 5; // S
        private Sprite liveWearableSprite;
        private Texture2D customIconPreviewTex;

        // ================= FORM 1: VẬT PHẨM & TRANG BỊ =================
        private string formItemName = "Bảo Kiếm Hoàng Kim";
        private string formItemId = "custom_wep_1";
        private int formTypeIndex = 0; // 0=Equipment, 1=Costume, 2=Consumable, 3=Pet, 4=Material
        private readonly string[] typeNames = { "Trang Bị", "Ngoại Trang", "Dược Phẩm", "Thú Cưng", "Nguyên Liệu" };

        private int formSlotIndex = 7; // 0=Không, 1=Mũ, 2=Áo, 3=Quần, 4=Giày, 5=Nhẫn, 6=Dây Chuyền, 7=Vũ Khí, 8=Ngoại Trang
        private readonly string[] slotNames = { "Không", "Mũ", "Áo", "Quần", "Giày", "Nhẫn", "Dây Chuyền", "Vũ Khí", "Ngoại Trang" };

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

        // RÀNG BUỘC: GIỚI TÍNH, HỆ & THUỘC TÍNH NGUYÊN TỐ
        private int formItemGenderIndex = 0; // 0=All, 1=Male, 2=Female
        private readonly string[] genderNames = { "Chung (Cả Nam & Nữ)", "Chỉ Nam ♂", "Chỉ Nữ ♀" };

        private string formItemReqClass = "Toàn Hệ";
        private string formItemElement = "Toàn Thuộc Tính";

        // Thuộc tính ẩn (Special Traits)
        private bool formItemHasGenderSwapTrait = false;
        private bool formItemHasSpeedTrait = false;
        private bool formItemHasLifestealTrait = false;
        private bool formItemHasInvulTrait = false;

        private int formItemIconIndex = 0;
        private readonly string[] defaultItemIconOptions = { "item_weapon", "item_helmet", "item_armor", "item_pants", "item_boots", "item_ring", "item_necklace", "item_costume", "item_potion_hp", "item_potion_mana", "pet_squirrel" };
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
        private int editItemGenderIndex = 0;
        private string editItemReqClass = "Toàn Hệ";
        private string editItemElement = "Toàn Thuộc Tính";
        private bool editItemHasGenderSwap = false;

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
            public int genderIdx; // 0=All, 1=Male, 2=Female
            public string reqClass; // "Toàn Hệ", "Kị Sĩ, Võ Sĩ", etc.
            public string reqElement; // "Toàn Thuộc Tính", "Hỏa, Lôi", "Hỏa, Băng, Lôi", "Hỏa, Băng, Phong, Lôi"
            public bool hasGenderSwap;
            public bool hasSpeed;
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
            // === 1. BỘ TRANG BỊ ĐA HỆ & ĐA THUỘC TÍNH (2, 3, 4 NGUYÊN TỐ) ===
            new ItemPreset { name = "Chiến Giáp Hỏa Long", id = "preset_flame_armor", typeIdx = 0, slotIdx = 2, tierIdx = 5, icon = "item_armor", stats = "+28 Giáp · +45 Thể Lực (VIT)", desc = "Chiến giáp đúc từ vảy rồng lửa, hỗ trợ cả Kị Sĩ và Võ Sĩ mang nguyên tố Hỏa hoặc Lôi.", dmg = "0", armor = "28", hp = "0", mp = "0", intVal = "0", strVal = "15", vitVal = "45", agiVal = "0", genderIdx = 0, reqClass = "Kị Sĩ, Võ Sĩ", reqElement = "Hỏa, Lôi", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Chiến Bào Nam Vương", id = "preset_male_king", typeIdx = 0, slotIdx = 2, tierIdx = 5, icon = "item_armor", stats = "+32 Giáp · +50 Sức Mạnh (STR)", desc = "Chiến bào hùng dũng chỉ dành riêng cho đấng mày râu (Nam ♂). Nữ nhân không thể mặc!", dmg = "0", armor = "32", hp = "0", mp = "0", intVal = "0", strVal = "50", vitVal = "25", agiVal = "0", genderIdx = 1, reqClass = "Kị Sĩ, Võ Sĩ", reqElement = "Hỏa, Lôi", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Váy Chiến Nữ Valkyrie", id = "preset_female_valkyrie", typeIdx = 0, slotIdx = 2, tierIdx = 5, icon = "item_armor", stats = "+30 Giáp · +45 Nhanh Nhẹn (AGI)", desc = "Trang phục thanh tao may đo riêng cho nữ chiến binh (Nữ ♀). Nam nhân không thể mặc!", dmg = "0", armor = "30", hp = "0", mp = "0", intVal = "15", strVal = "0", vitVal = "20", agiVal = "45", genderIdx = 2, reqClass = "Kị Sĩ, Xạ Thủ", reqElement = "Hỏa, Băng, Quang", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Nhẫn Tam Nguyên (3 Hệ)", id = "preset_tri_ring", typeIdx = 0, slotIdx = 5, tierIdx = 5, icon = "item_ring", stats = "+25 Công · +20 Toàn Chỉ Số", desc = "Bảo nhẫn hội tụ Tam Đại Nguyên Tố (Hỏa, Băng, Lôi). Cả 3 hệ này đều kích hoạt uy lực!", dmg = "25", armor = "0", hp = "50", mp = "50", intVal = "20", strVal = "20", vitVal = "20", agiVal = "20", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Hỏa, Băng, Lôi", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Thánh Bùa Tứ Tượng (4 Hệ)", id = "preset_quad_amulet", typeIdx = 0, slotIdx = 6, tierIdx = 5, icon = "item_necklace", stats = "+20 Giáp · +35 Thể Lực (VIT)", desc = "Bùa hộ mệnh thần bí tương thích 4 nguyên tố Tứ Tượng (Hỏa, Băng, Phong, Lôi).", dmg = "0", armor = "20", hp = "100", mp = "80", intVal = "15", strVal = "15", vitVal = "35", agiVal = "15", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Hỏa, Băng, Phong, Lôi", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Võ Khố Lửa Băng (2 Hệ)", id = "preset_dual_pants", typeIdx = 0, slotIdx = 3, tierIdx = 4, icon = "item_pants", stats = "+22 Giáp · +30 Thể Lực", desc = "Quần giáp tôi luyện từ băng hỏa song hàn, tương thích cho Kị Sĩ và Võ Sĩ mang nguyên tố Hỏa hoặc Băng.", dmg = "0", armor = "22", hp = "60", mp = "0", intVal = "0", strVal = "10", vitVal = "30", agiVal = "10", genderIdx = 0, reqClass = "Kị Sĩ, Võ Sĩ", reqElement = "Hỏa, Băng", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Pháp Bào Băng Sương", id = "preset_mage_frost_robe", typeIdx = 0, slotIdx = 2, tierIdx = 4, icon = "item_armor", stats = "+12 Giáp · +40 Trí Tuệ (INT)", desc = "Áo dệt từ tơ tuyết vĩnh cửu. Chỉ Pháp Sư hệ Băng hoặc Mộc mới lĩnh hội được.", dmg = "0", armor = "12", hp = "0", mp = "100", intVal = "40", strVal = "0", vitVal = "15", agiVal = "0", genderIdx = 0, reqClass = "Pháp Sư", reqElement = "Băng, Mộc", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Áo Da Phong Linh", id = "preset_wind_vest", typeIdx = 0, slotIdx = 2, tierIdx = 5, icon = "item_armor", stats = "+16 Giáp · +38 Nhanh Nhẹn (AGI)", desc = "Áo da nhẹ bẫng tựa gió thoảng, hỗ trợ thần tiễn thủ và pháp sư mang nguyên tố Phong hoặc Ánh Sáng.", dmg = "0", armor = "16", hp = "0", mp = "40", intVal = "15", strVal = "0", vitVal = "15", agiVal = "38", genderIdx = 0, reqClass = "Xạ Thủ, Pháp Sư", reqElement = "Phong, Quang", hasGenderSwap = false, hasSpeed = false },

            // === 2. BỘ NGOẠI TRANG CÓ THUỘC TÍNH ẨN (CHUYỂN GIỚI [G] & TỐC BIẾN) ===
            new ItemPreset { name = "Ngoại Trang Thánh Nữ", id = "preset_costume_valkyrie", typeIdx = 1, slotIdx = 8, tierIdx = 5, icon = "item_costume", stats = "+18 Giáp • [Thuộc Tính Ẩn: Chuyển Giới (G)]", desc = "Chiến bào thánh khiết của các nữ chiến thần phương Bắc, chỉ phái Nữ ♀ mới có thể khoác lên người!", dmg = "0", armor = "18", hp = "0", mp = "0", intVal = "0", strVal = "0", vitVal = "0", agiVal = "0", genderIdx = 2, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = true, hasSpeed = false },
            new ItemPreset { name = "Ngoại Trang Thiết Giáp Chiến Thần", id = "preset_costume_champion", typeIdx = 1, slotIdx = 8, tierIdx = 5, icon = "item_costume", stats = "+15 Giáp • [Thuộc Tính Ẩn: Chuyển Giới (G)]", desc = "Ngoại trang hoàng gia tuyệt mỹ sở hữu Thuộc Tính Ẩn: Cho phép chủ nhân chuyển đổi tự do giữa hình thái Nam ♂ và Nữ ♀ khi bấm phím [G]!", dmg = "0", armor = "15", hp = "0", mp = "0", intVal = "0", strVal = "0", vitVal = "0", agiVal = "0", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = true, hasSpeed = false },
            new ItemPreset { name = "Ngoại Trang Hắc Ám Dạ Hành", id = "preset_costume_shadow", typeIdx = 1, slotIdx = 8, tierIdx = 4, icon = "item_costume", stats = "+12 Giáp • [Thuộc Tính Ẩn: Tốc Biến]", desc = "Ngoại trang sát thủ bóng đêm, tăng tốc độ di chuyển trong màn đêm nhưng không có chức năng Chuyển Giới.", dmg = "0", armor = "12", hp = "0", mp = "0", intVal = "0", strVal = "0", vitVal = "0", agiVal = "0", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = true },

            // === 3. TRANG BỊ CƠ BẢN & VẬT PHẨM ===
            new ItemPreset { name = "Bảo Kiếm Hoàng Kim", id = "custom_wep_gold", typeIdx = 0, slotIdx = 7, tierIdx = 4, icon = "item_weapon", stats = "+20 Công (Dam) · +25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)", desc = "Kiếm cổ đại dát vàng tăng mạnh sức công kích và ma pháp.", dmg = "20", armor = "0", hp = "0", mp = "0", intVal = "25", strVal = "15", vitVal = "0", agiVal = "0", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Mũ Chiến Binh Sắt", id = "custom_helm_iron", typeIdx = 0, slotIdx = 1, tierIdx = 2, icon = "item_helmet", stats = "+10 Giáp · +12 Thể Lực (VIT) · +5 Sức Mạnh (STR)", desc = "Mũ giáp bền bỉ che chắn cho dũng sĩ trong giao tranh.", dmg = "0", armor = "10", hp = "0", mp = "0", intVal = "0", strVal = "5", vitVal = "12", agiVal = "0", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Áo Choàng Thánh Quang", id = "custom_armor_holy", typeIdx = 0, slotIdx = 2, tierIdx = 5, icon = "item_armor", stats = "+18 Giáp · +30 Thể Lực (VIT) · +15 Trí Tuệ (INT)", desc = "Áo choàng hộ mệnh thần thánh ban phước lành bình an.", dmg = "0", armor = "18", hp = "0", mp = "0", intVal = "15", strVal = "0", vitVal = "30", agiVal = "0", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Quần Phù Thủy Gió", id = "custom_pants_wind", typeIdx = 0, slotIdx = 3, tierIdx = 3, icon = "item_pants", stats = "+8 Giáp · +18 Nhanh Nhẹn (AGI) · +10 Trí Tuệ (INT)", desc = "Vải lụa nhẹ như gió giúp dũng sĩ lướt đi linh hoạt.", dmg = "0", armor = "8", hp = "0", mp = "0", intVal = "10", strVal = "0", vitVal = "0", agiVal = "18", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Giày Phong Hành Thần", id = "custom_boots_speed", typeIdx = 0, slotIdx = 4, tierIdx = 3, icon = "item_boots", stats = "+6 Giáp · +20 Nhanh Nhẹn (AGI)", desc = "Đôi giày tăng mạnh tốc độ di chuyển và giảm hồi chiêu Lướt.", dmg = "0", armor = "6", hp = "0", mp = "0", intVal = "0", strVal = "0", vitVal = "0", agiVal = "20", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Bình Siêu Máu HP", id = "custom_pot_hp_super", typeIdx = 2, slotIdx = 0, tierIdx = 3, icon = "item_potion_hp", stats = "Hồi +100 Máu (HP) tức thì", desc = "Bình dược phẩm tinh khiết hồi phục sinh lực thần tốc.", dmg = "0", armor = "0", hp = "100", mp = "0", intVal = "0", strVal = "0", vitVal = "0", agiVal = "0", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Bình Đại Mana MP", id = "custom_pot_mp_grand", typeIdx = 2, slotIdx = 0, tierIdx = 3, icon = "item_potion_mana", stats = "Hồi +80 Mana tức thì", desc = "Nước suối pháp thuật tái tạo nguồn năng lượng dồi dào.", dmg = "0", armor = "0", hp = "0", mp = "80", intVal = "0", strVal = "0", vitVal = "0", agiVal = "0", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false },
            new ItemPreset { name = "Sóc Thần Hoàng Kim", id = "custom_pet_gold_sq", typeIdx = 3, slotIdx = 0, tierIdx = 5, icon = "pet_squirrel", stats = "Thú Cưng Đồng Hành · +5 Thể Lực (VIT) · +5 Nhanh Nhẹn (AGI)", desc = "Thú cưng linh vật quý tộc đồng hành cùng dũng sĩ.", dmg = "0", armor = "0", hp = "0", mp = "0", intVal = "0", strVal = "0", vitVal = "5", agiVal = "5", genderIdx = 0, reqClass = "Toàn Hệ", reqElement = "Toàn Thuộc Tính", hasGenderSwap = false, hasSpeed = false }
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
            else if (studioMode == 2)
            {
                DrawColumnFrame(col1Rect, "🎒 24 Ô HÀNH TRANG (SLOTS)");
                DrawColumnFrame(col2Rect, "🛠️ LIVE INSPECTOR (CHỈNH TRỰC TIẾP)");
                DrawColumnFrame(col3Rect, "👁️ XEM TRƯỚC & THỬ CHIÊU");
                DrawInventorySlotsListColumn(new Rect(col1Rect.x + 6, col1Rect.y + 26, col1Rect.width - 12, col1Rect.height - 32));
                DrawInventoryItemEditorColumn(new Rect(col2Rect.x + 8, col2Rect.y + 26, col2Rect.width - 16, col2Rect.height - 32));
                DrawInventoryLivePreviewColumn(new Rect(col3Rect.x + 8, col3Rect.y + 26, col3Rect.width - 16, col3Rect.height - 32));
            }
            else
            {
                EnsureCostumeCanvas();
                DrawColumnFrame(col1Rect, "🎬 8 HƯỚNG & CÔNG CỤ PIXEL");
                DrawColumnFrame(col2Rect, "🖌️ CANVAS TỪNG FRAME & ONION SKIN");
                DrawColumnFrame(col3Rect, "▶️ XEM HOẠT ẢNH & XUẤT SPRITE");
                DrawCostumeToolsColumn(new Rect(col1Rect.x + 6, col1Rect.y + 26, col1Rect.width - 12, col1Rect.height - 32));
                DrawCostumeCanvasColumn(new Rect(col2Rect.x + 8, col2Rect.y + 26, col2Rect.width - 16, col2Rect.height - 32));
                DrawCostumePreviewActionsColumn(new Rect(col3Rect.x + 8, col3Rect.y + 26, col3Rect.width - 16, col3Rect.height - 32));
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

                string iconLabel = (p.typeIdx == 0) ? "⚔️" : (p.typeIdx == 1) ? "👘" : (p.typeIdx == 2) ? "🧪" : "🐿️";
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
                    formItemGenderIndex = p.genderIdx;
                    formItemReqClass = string.IsNullOrEmpty(p.reqClass) ? "Toàn Hệ" : p.reqClass;
                    formItemElement = string.IsNullOrEmpty(p.reqElement) ? "Toàn Thuộc Tính" : p.reqElement;
                    formItemHasGenderSwapTrait = p.hasGenderSwap;
                    formItemHasSpeedTrait = p.hasSpeed;
                    formItemHasLifestealTrait = false;
                    formItemHasInvulTrait = false;
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
            float totalContentH = 920f;
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
            else if (formTypeIndex == 1) // Ngoại Trang (Costume)
            {
                GUI.Label(new Rect(0, curY, viewRect.width, 20), "👘 Vị Trí Mặc: <color=#00e5ff>Ngoại Trang (Costume Slot) [Tự Động]</color>", lbl);
                curY += 24f;
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

            // 3.1 RÀNG BUỘC GIỚI TÍNH (Gender Restriction)
            GUI.Label(new Rect(0, curY, 130, 20), "⚧ Ràng Buộc Giới Tính:", lbl);
            formItemGenderIndex = GUI.Toolbar(new Rect(135, curY, viewRect.width - 135, 24), formItemGenderIndex, genderNames);
            curY += 28f;

            // 3.2 RÀNG BUỘC HỆ / CHỨC NGHIỆP (Multi-class Restriction)
            GUI.Label(new Rect(0, curY, 130, 20), "⚔️ Hệ / Phái Cho Phép:", lbl);
            formItemReqClass = GUI.TextField(new Rect(135, curY, viewRect.width - 135, 22), formItemReqClass);
            curY += 24f;

            float classBtnW = (viewRect.width - 16f) / 5f;
            if (GUI.Button(new Rect(0, curY, classBtnW, 20), "Toàn Hệ")) formItemReqClass = "Toàn Hệ";
            if (GUI.Button(new Rect(classBtnW + 4, curY, classBtnW, 20), "⚔️ Kị Sĩ")) formItemReqClass = "Kị Sĩ";
            if (GUI.Button(new Rect((classBtnW + 4) * 2, curY, classBtnW, 20), "🔮 Pháp Sư")) formItemReqClass = "Pháp Sư";
            if (GUI.Button(new Rect((classBtnW + 4) * 3, curY, classBtnW, 20), "🥊 Võ Sĩ")) formItemReqClass = "Võ Sĩ";
            if (GUI.Button(new Rect((classBtnW + 4) * 4, curY, classBtnW, 20), "🏹 Xạ Thủ")) formItemReqClass = "Xạ Thủ";
            curY += 22f;

            float multiClassW = (viewRect.width - 12f) / 4f;
            if (GUI.Button(new Rect(0, curY, multiClassW, 20), "⚔️🥊 Kị Sĩ, Võ Sĩ")) formItemReqClass = "Kị Sĩ, Võ Sĩ";
            if (GUI.Button(new Rect(multiClassW + 4, curY, multiClassW, 20), "🔮🏹 Pháp Sư, Xạ Thủ")) formItemReqClass = "Pháp Sư, Xạ Thủ";
            if (GUI.Button(new Rect((multiClassW + 4) * 2, curY, multiClassW, 20), "⚔️🏹 Kị Sĩ, Xạ Thủ")) formItemReqClass = "Kị Sĩ, Xạ Thủ";
            if (GUI.Button(new Rect((multiClassW + 4) * 3, curY, multiClassW, 20), "🔮🥊 Pháp Sư, Võ Sĩ")) formItemReqClass = "Pháp Sư, Võ Sĩ";
            curY += 26f;

            // 3.3 RÀNG BUỘC THUỘC TÍNH NGUYÊN TỐ (Multi-element: 2, 3, 4 thuộc tính)
            GUI.Label(new Rect(0, curY, 130, 20), "🔥 Thuộc Tính Nguyên Tố:", lbl);
            formItemElement = GUI.TextField(new Rect(135, curY, viewRect.width - 135, 22), formItemElement);
            curY += 24f;

            float elemBtnW = (viewRect.width - 28f) / 8f;
            if (GUI.Button(new Rect(0, curY, elemBtnW, 20), "Toàn Hệ")) formItemElement = "Toàn Thuộc Tính";
            if (GUI.Button(new Rect(elemBtnW + 4, curY, elemBtnW, 20), "🔥 Hỏa")) formItemElement = "Hỏa";
            if (GUI.Button(new Rect((elemBtnW + 4) * 2, curY, elemBtnW, 20), "❄️ Băng")) formItemElement = "Băng";
            if (GUI.Button(new Rect((elemBtnW + 4) * 3, curY, elemBtnW, 20), "⚡ Lôi")) formItemElement = "Lôi";
            if (GUI.Button(new Rect((elemBtnW + 4) * 4, curY, elemBtnW, 20), "🌪️ Phong")) formItemElement = "Phong";
            if (GUI.Button(new Rect((elemBtnW + 4) * 5, curY, elemBtnW, 20), "🌿 Mộc")) formItemElement = "Mộc";
            if (GUI.Button(new Rect((elemBtnW + 4) * 6, curY, elemBtnW, 20), "✨ Quang")) formItemElement = "Quang";
            if (GUI.Button(new Rect((elemBtnW + 4) * 7, curY, elemBtnW, 20), "🔮 Ám")) formItemElement = "Ám";
            curY += 22f;

            float dualElemW = (viewRect.width - 16f) / 5f;
            if (GUI.Button(new Rect(0, curY, dualElemW, 20), "🔥❄️ Hỏa, Băng")) formItemElement = "Hỏa, Băng";
            if (GUI.Button(new Rect(dualElemW + 4, curY, dualElemW, 20), "🔥⚡ Hỏa, Lôi")) formItemElement = "Hỏa, Lôi";
            if (GUI.Button(new Rect((dualElemW + 4) * 2, curY, dualElemW, 20), "❄️🌿 Băng, Mộc")) formItemElement = "Băng, Mộc";
            if (GUI.Button(new Rect((dualElemW + 4) * 3, curY, dualElemW, 20), "🌪️✨ Phong, Quang")) formItemElement = "Phong, Quang";
            if (GUI.Button(new Rect((dualElemW + 4) * 4, curY, dualElemW, 20), "✨🔮 Quang, Ám")) formItemElement = "Quang, Ám";
            curY += 22f;

            float triElemW = (viewRect.width - 8f) / 3f;
            if (GUI.Button(new Rect(0, curY, triElemW, 20), "🔥❄️⚡ Hỏa, Băng, Lôi (3 Hệ)")) formItemElement = "Hỏa, Băng, Lôi";
            if (GUI.Button(new Rect(triElemW + 4, curY, triElemW, 20), "🔥❄️✨ Hỏa, Băng, Quang (3 Hệ)")) formItemElement = "Hỏa, Băng, Quang";
            if (GUI.Button(new Rect((triElemW + 4) * 2, curY, triElemW, 20), "🛡️ Tứ Tượng: Hỏa, Băng, Phong, Lôi (4 Hệ)")) formItemElement = "Hỏa, Băng, Phong, Lôi";
            curY += 26f;

            // 3.4 THUỘC TÍNH ẨN (Special Traits)
            GUI.Label(new Rect(0, curY, viewRect.width, 18), "✨ Thuộc Tính Ẩn Đặc Biệt (Traits):", lbl);
            curY += 20f;
            Rect traitBox = new Rect(0, curY, viewRect.width, 46f);
            GUI.color = new Color(0.08f, 0.12f, 0.20f, 0.85f);
            GUI.DrawTexture(traitBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(traitBox, new Color(0.35f, 0.65f, 0.95f, 0.6f), 1);

            float trY = curY + 2f;
            float trColW = (viewRect.width - 16f) / 2f;
            formItemHasGenderSwapTrait = GUI.Toggle(new Rect(8f, trY, trColW, 20f), formItemHasGenderSwapTrait, " 🔄 Chuyển Giới [G] (Gender Swap)");
            formItemHasSpeedTrait = GUI.Toggle(new Rect(8f + trColW, trY, trColW, 20f), formItemHasSpeedTrait, " ⚡ Tăng Tốc Độ (Speed Boost)");
            trY += 22f;
            formItemHasLifestealTrait = GUI.Toggle(new Rect(8f, trY, trColW, 20f), formItemHasLifestealTrait, " 🩸 Hút Máu Khi Đánh (Lifesteal)");
            formItemHasInvulTrait = GUI.Toggle(new Rect(8f + trColW, trY, trColW, 20f), formItemHasInvulTrait, " 🛡️ Hộ Thể Kim Cang (Invulnerable)");
            curY += 52f;

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
            pY += 62f;

            // 2. Khung Giả Lập Tooltip Thực Tế Trong Game
            Rect tipBox = new Rect(r.x, pY, r.width, 142f);
            FantasyUITheme.DrawDoubleGoldFrame(tipBox, 1);

            var tTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };
            GUI.Label(new Rect(tipBox.x + 6, tipBox.y + 4, tipBox.width - 12, 18), formItemName, tTitleStyle);

            GUI.color = new Color(0.85f, 0.70f, 0.30f, 0.6f);
            GUI.DrawTexture(new Rect(tipBox.x + 8, tipBox.y + 24, tipBox.width - 16, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var tBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            string slotStr = (formTypeIndex == 1) ? "Ngoại Trang (Skin)" : (formTypeIndex == 0 ? $"Vị Trí: {slotNames[formSlotIndex]}" : $"Loại: {typeNames[formTypeIndex]}");
            string genderStr = (formItemGenderIndex == 0) ? "Cả Nam & Nữ [Unisex]" : (formItemGenderIndex == 1) ? "Nam ♂" : "Nữ ♀";
            string traitStr = formItemHasGenderSwapTrait ? " · [G] Chuyển Giới" : "";
            string bodyText = $"• Phẩm: <color=#{ColorUtility.ToHtmlStringRGB(tierColors[formTierIndex])}>{tierNames[formTierIndex]}</color> | {slotStr}\n" +
                              $"• Yêu Cầu: {genderStr} | {formItemReqClass} | {formItemElement}{traitStr}\n" +
                              $"• {formItemStats}\n" +
                              $"• {formItemDesc}";

            GUI.Label(new Rect(tipBox.x + 8, tipBox.y + 28, tipBox.width - 16, 108f), bodyText, tBodyStyle);
            pY += 148f;

            // 3. ĐÁNH GIÁ TƯƠNG THÍCH NHÂN VẬT THỰC TẾ (LIVE HERO COMPATIBILITY)
            bool heroFemale = ProceduralCharacterRig2D.Instance != null ? ProceduralCharacterRig2D.Instance.IsFemale
                            : (CharacterVisualController8Dir.Instance != null && CharacterVisualController8Dir.Instance.IsFemale);
            string heroClass = PlayerStats.Instance != null ? PlayerStats.Instance.CharacterClass : "Kị Sĩ";
            string heroElem = PlayerStats.Instance != null ? PlayerStats.Instance.Element : "Hỏa";

            bool genderOk = (formItemGenderIndex == 0) || (formItemGenderIndex == 1 && !heroFemale) || (formItemGenderIndex == 2 && heroFemale);
            bool classOk = (formItemReqClass == "Toàn Hệ") || formItemReqClass.Contains(heroClass);
            bool elemOk = (formItemElement == "Toàn Thuộc Tính") || formItemElement.Contains(heroElem);
            bool canEquip = genderOk && classOk && elemOk;

            Rect evalBox = new Rect(r.x, pY, r.width, 36f);
            GUI.color = canEquip ? new Color(0.08f, 0.25f, 0.12f, 0.95f) : new Color(0.28f, 0.08f, 0.08f, 0.95f);
            GUI.DrawTexture(evalBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(evalBox, canEquip ? Color.green : Color.red, 1);

            var evalStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            if (canEquip)
            {
                evalStyle.normal.textColor = Color.green;
                GUI.Label(evalBox, $"🟢 ĐỦ ĐIỀU KIỆN MẶC CHO HERO\n({(heroFemale ? "Nữ ♀" : "Nam ♂")} · {heroClass} · {heroElem})", evalStyle);
            }
            else
            {
                evalStyle.normal.textColor = new Color(1f, 0.45f, 0.45f);
                string failReason = !genderOk ? "Sai Giới Tính" : (!classOk ? "Khác Hệ" : "Khác Thuộc Tính");
                GUI.Label(evalBox, $"⛔ BỊ KHÓA ({failReason}) CHO HERO\n({(heroFemale ? "Nữ ♀" : "Nam ♂")} · {heroClass} · {heroElem})", evalStyle);
            }
            pY += 40f;

            // 4. CÁC NÚT HÀNH ĐỘNG NỔI BẬT
            float btnH = 22f;
            float btnGap = 3f;

            // Nút Mặc Thử Ngay
            GUI.color = canEquip ? new Color(1.0f, 0.85f, 0.25f) : new Color(0.7f, 0.7f, 0.7f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), canEquip ? "⚡ MẶC THỬ NGAY (EQUIP)" : "⚠️ MẶC CƯỠNG CHẾ (TEST)"))
            {
                EquipDirectlyFromStudio();
            }
            pY += btnH + btnGap;

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

            // 5. QUICK SANDBOX TOGGLES CHO TESTER
            float sbW = (r.width - 6f) / 2f;
            if (GUI.Button(new Rect(r.x, pY, sbW, 20f), heroFemale ? "Đổi: Nam ♂" : "Đổi: Nữ ♀"))
            {
                if (ProceduralCharacterRig2D.Instance != null) ProceduralCharacterRig2D.Instance.ForceSetGender(!heroFemale);
                if (CharacterVisualController8Dir.Instance != null) CharacterVisualController8Dir.Instance.ForceSetGender(!heroFemale);
            }
            if (GUI.Button(new Rect(r.x + sbW + 6f, pY, sbW, 20f), "Đổi Hệ/Element"))
            {
                if (PlayerStats.Instance != null)
                {
                    string[] cList = { "Kị Sĩ", "Pháp Sư", "Võ Sĩ", "Xạ Thủ" };
                    int curCIdx = System.Array.IndexOf(cList, PlayerStats.Instance.CharacterClass);
                    PlayerStats.Instance.SetCharacterClass(cList[(curCIdx + 1 + cList.Length) % cList.Length]);

                    string[] eList = { "Hỏa", "Băng", "Lôi", "Phong", "Mộc", "Quang", "Ám" };
                    int curEIdx = System.Array.IndexOf(eList, PlayerStats.Instance.Element);
                    PlayerStats.Instance.SetElement(eList[(curEIdx + 1 + eList.Length) % eList.Length]);
                }
            }
            pY += 24f;

            if (!string.IsNullOrEmpty(studioExportStatus))
            {
                var stStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = studioExportStatus.StartsWith("✅") ? Color.green : Color.yellow } };
                GUI.Label(new Rect(r.x, pY, r.width, 24), studioExportStatus, stStyle);
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

        private void EquipDirectlyFromStudio()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null) return;

            ItemType t = (formTypeIndex == 0) ? ItemType.Equipment :
                         (formTypeIndex == 1) ? ItemType.Costume :
                         (formTypeIndex == 2) ? ItemType.Consumable :
                         (formTypeIndex == 3) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (formTypeIndex == 1) ? EquipSlot.Costume : (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string[] allItemIcons = GetAllItemIcons();
            string icon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];

            var newItem = new InventoryItem(formItemId, formItemName, t, s, tier, c, icon, formItemStats, formItemDesc, 1, null, formItemReqClass, formItemElement, (ItemGender)formItemGenderIndex);
            if (formItemHasGenderSwapTrait) newItem.AddTrait(ItemTraits.GENDER_SWAP);

            // Kiểm tra điều kiện mặc
            bool heroFemale = ProceduralCharacterRig2D.Instance != null ? ProceduralCharacterRig2D.Instance.IsFemale
                            : (CharacterVisualController8Dir.Instance != null && CharacterVisualController8Dir.Instance.IsFemale);
            string heroClass = PlayerStats.Instance != null ? PlayerStats.Instance.CharacterClass : "Kị Sĩ";
            string heroElem = PlayerStats.Instance != null ? PlayerStats.Instance.Element : "Hỏa";
            if (!newItem.CanPlayerEquip(heroFemale, heroClass, heroElem, out string reason))
            {
                studioExportStatus = $"⛔ Không thể mặc: {reason}";
                return;
            }

            // Tự động thêm vào túi đồ và mặc ngay lập tức
            if (!inv.AddItem(newItem))
            {
                studioExportStatus = "⚠️ Túi đồ đầy, không thể thêm đồ để mặc!";
                return;
            }

            inv.EquipGearItem(newItem, false);
            studioExportStatus = $"⚡ ĐÃ MẶC THÀNH CÔNG '{formItemName}' VÀO Ô {s}!";
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
                         (formTypeIndex == 1) ? ItemType.Costume :
                         (formTypeIndex == 2) ? ItemType.Consumable :
                         (formTypeIndex == 3) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (formTypeIndex == 1) ? EquipSlot.Costume : (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string[] allItemIcons = GetAllItemIcons();
            string icon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];

            var newItem = new InventoryItem(formItemId, formItemName, t, s, tier, c, icon, formItemStats, formItemDesc, 1, null, formItemReqClass, formItemElement, (ItemGender)formItemGenderIndex);
            if (formItemHasGenderSwapTrait) newItem.AddTrait(ItemTraits.GENDER_SWAP);

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
                         (formTypeIndex == 1) ? ItemType.Costume :
                         (formTypeIndex == 2) ? ItemType.Consumable :
                         (formTypeIndex == 3) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (formTypeIndex == 1) ? EquipSlot.Costume : (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string[] allItemIcons = GetAllItemIcons();
            string icon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];

            var traits = new List<string>();
            if (formItemHasGenderSwapTrait) traits.Add(ItemTraits.GENDER_SWAP);

            ItemDatabase.Instance.RegisterNewItemFromForm(formItemId, formItemName, t, s, tier, c, icon, formItemStats, formItemDesc, 1, formItemReqClass, formItemElement, (ItemGender)formItemGenderIndex, traits);
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
                         (formTypeIndex == 1) ? ItemType.Costume :
                         (formTypeIndex == 2) ? ItemType.Consumable :
                         (formTypeIndex == 3) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (formTypeIndex == 1) ? EquipSlot.Costume : (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string[] allItemIcons = GetAllItemIcons();
            string icon = allItemIcons[Mathf.Clamp(formItemIconIndex, 0, allItemIcons.Length - 1)];

            string genderStr = ((ItemGender)formItemGenderIndex).ToString();
            string traitCode = formItemHasGenderSwapTrait ? $"\n    .WithTrait(ItemTraits.GENDER_SWAP)" : "";

            return $"// Vật phẩm: {formItemName}\n" +
                   $"var {formItemId.Replace("-", "_")} = new InventoryItem(\"{formItemId}\", \"{formItemName}\", ItemType.{t}, EquipSlot.{s}, \"{tier}\", new Color({c.r:F2}f, {c.g:F2}f, {c.b:F2}f), \"{icon}\", \"{formItemStats}\", \"{formItemDesc}\", 1)\n" +
                   $"    .WithGender(ItemGender.{genderStr})\n" +
                   $"    .WithClass(\"{formItemReqClass}\")\n" +
                   $"    .WithElement(\"{formItemElement}\"){traitCode};";
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
            editItemReqClass = it.requiredClass ?? "Toàn Hệ";
            editItemElement = it.element ?? "Toàn Thuộc Tính";
            editItemGenderIndex = (int)it.requiredGender;
            editItemHasGenderSwap = it.HasTrait(ItemTraits.GENDER_SWAP);

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

            if (it.itemType == ItemType.Equipment || it.itemType == ItemType.Costume)
            {
                GUI.Label(new Rect(0, curY, 90, 20), "Giới Tính:", lbl);
                editItemGenderIndex = GUI.Toolbar(new Rect(95, curY, viewRect.width - 95, 22), editItemGenderIndex, genderNames);
                curY += 26f;

                GUI.Label(new Rect(0, curY, 90, 20), "Hệ Yêu Cầu:", lbl);
                editItemReqClass = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), editItemReqClass);
                curY += 26f;

                GUI.Label(new Rect(0, curY, 90, 20), "Thuộc Tính:", lbl);
                editItemElement = GUI.TextField(new Rect(95, curY, viewRect.width - 95, 22), editItemElement);
                curY += 26f;

                editItemHasGenderSwap = GUI.Toggle(new Rect(95, curY, viewRect.width - 95, 20), editItemHasGenderSwap, " 🚻 Thuộc tính ẩn: [G] Chuyển Giới (Gender Swap)");
                curY += 24f;
            }

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
            it.requiredGender = (ItemGender)editItemGenderIndex;
            it.requiredClass = editItemReqClass;
            it.element = editItemElement;
            if (editItemHasGenderSwap) it.AddTrait(ItemTraits.GENDER_SWAP);
            else it.RemoveTrait(ItemTraits.GENDER_SWAP);

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

        // ==============================================================================================
        //  FORM 4: XƯỞNG THIẾT KẾ PHỤC TRANG & RIG (ALL-IN-ONE COSTUME WORKSHOP)
        // ==============================================================================================
        // ==============================================================================================
        //  FORM 4: XƯỞNG PIXEL ART & HOẠT ẢNH 8 HƯỚNG (FRAME-BY-FRAME PIXEL STUDIO)
        // ==============================================================================================
        private void EnsureCostumeCanvas()
        {
            if (canvasTex != null && canvasPixels != null && multiFramePixels != null) return;

            multiFramePixels = new Color32[8][][];
            for (int d = 0; d < 8; d++)
            {
                multiFramePixels[d] = new Color32[TOTAL_STUDIO_FRAMES][];
                for (int f = 0; f < TOTAL_STUDIO_FRAMES; f++)
                {
                    multiFramePixels[d][f] = new Color32[CANVAS_SIZE * CANVAS_SIZE];
                    for (int i = 0; i < CANVAS_SIZE * CANVAS_SIZE; i++)
                    {
                        multiFramePixels[d][f][i] = new Color32(0, 0, 0, 0); // Trong suốt mặc định
                    }
                }
            }

            frameTextures = new Texture2D[TOTAL_STUDIO_FRAMES];
            for (int f = 0; f < TOTAL_STUDIO_FRAMES; f++)
            {
                frameTextures[f] = new Texture2D(CANVAS_SIZE, CANVAS_SIZE, TextureFormat.RGBA32, false);
                frameTextures[f].filterMode = FilterMode.Point;
                frameTextures[f].wrapMode = TextureWrapMode.Clamp;
            }

            canvasPixels = new Color32[CANVAS_SIZE * CANVAS_SIZE];
            canvasTex = new Texture2D(CANVAS_SIZE, CANVAS_SIZE, TextureFormat.RGBA32, false);
            canvasTex.filterMode = FilterMode.Point;
            canvasTex.wrapMode = TextureWrapMode.Clamp;

            // Tự động nạp mẫu gốc Pháp Sư cho cả 8 hướng ban đầu
            for (int d = 0; d < 8; d++)
            {
                LoadMagePresetForDirection(d, silent: true);
            }

            // Nạp frame 0 của hướng hiện tại lên canvas
            LoadFrameIntoCanvas(0);
        }

        private void LoadMagePresetForDirection(int dirIdx, bool silent = false)
        {
            if (dirIdx < 0 || dirIdx >= 8) return;
            string dirCode = studioDirCodes[dirIdx];

            for (int f = 0; f < TOTAL_STUDIO_FRAMES; f++)
            {
                Texture2D rawTex = Resources.Load<Texture2D>($"Characters/Mage/mage_{dirCode}_{f}");
                if (rawTex != null)
                {
                    for (int y = 0; y < CANVAS_SIZE; y++)
                    {
                        for (int x = 0; x < CANVAS_SIZE; x++)
                        {
                            float u = (x + 0.5f) / CANVAS_SIZE;
                            float v = (y + 0.5f) / CANVAS_SIZE;
                            Color c = rawTex.GetPixelBilinear(u, v);
                            multiFramePixels[dirIdx][f][y * CANVAS_SIZE + x] = (c.a > 0.15f) ? (Color32)c : new Color32(0, 0, 0, 0);
                        }
                    }
                }
            }

            RefreshFrameTexturesForCurrentDir();
            if (!silent)
            {
                LoadFrameIntoCanvas(currentFrameIndex);
                studioExportStatus = $"✅ Đã nạp mẫu Pháp Sư ({studioDirLabels[dirIdx]}) đầy đủ 8 Frame!";
                GodlessFantasy.Core.AudioManager.PlayPickup();
            }
        }

        private void RefreshFrameTexturesForCurrentDir()
        {
            if (frameTextures == null || multiFramePixels == null) return;
            for (int f = 0; f < TOTAL_STUDIO_FRAMES; f++)
            {
                if (frameTextures[f] != null)
                {
                    frameTextures[f].SetPixels32(multiFramePixels[currentEditDirIndex][f]);
                    frameTextures[f].Apply();
                }
            }
        }

        private void LoadFrameIntoCanvas(int frameIdx)
        {
            currentFrameIndex = Mathf.Clamp(frameIdx, 0, TOTAL_STUDIO_FRAMES - 1);
            if (multiFramePixels == null || canvasPixels == null || canvasTex == null) return;

            System.Array.Copy(multiFramePixels[currentEditDirIndex][currentFrameIndex], canvasPixels, canvasPixels.Length);
            canvasTex.SetPixels32(canvasPixels);
            canvasTex.Apply();
            UpdateLiveWearableSprite();
        }

        private void SaveCanvasIntoCurrentFrame()
        {
            if (multiFramePixels == null || canvasPixels == null) return;
            System.Array.Copy(canvasPixels, multiFramePixels[currentEditDirIndex][currentFrameIndex], canvasPixels.Length);
            if (frameTextures != null && frameTextures[currentFrameIndex] != null)
            {
                frameTextures[currentFrameIndex].SetPixels32(canvasPixels);
                frameTextures[currentFrameIndex].Apply();
            }
        }

        private void SwitchDirection(int newDirIdx)
        {
            if (newDirIdx == currentEditDirIndex) return;
            SaveCanvasIntoCurrentFrame();
            currentEditDirIndex = Mathf.Clamp(newDirIdx, 0, 7);
            RefreshFrameTexturesForCurrentDir();
            LoadFrameIntoCanvas(currentFrameIndex);
        }

        // ─── CỘT 1: HƯỚNG, TIMELINE FRAME, CÔNG CỤ VẼ & BẢNG MÀU ──────────────────────────────────
        private void DrawCostumeToolsColumn(Rect r)
        {
            float curY = r.y + 4f;
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.95f, 1f) } };
            var subLbl = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(0.7f, 0.85f, 1f) } };

            // 1. CHỌN HƯỚNG BIÊN TẬP (8 DIRECTIONS)
            GUI.Label(new Rect(r.x, curY, r.width, 18), "🧭 HƯỚNG ĐANG VẼ:", lbl);
            curY += 20f;
            int newDir = GUI.SelectionGrid(new Rect(r.x, curY, r.width, 42f), currentEditDirIndex, studioDirLabels, 4);
            if (newDir != currentEditDirIndex)
            {
                SwitchDirection(newDir);
            }
            curY += 46f;

            // 2. TIMELINE CHỌN FRAME (8 FRAMES: 0..7)
            GUI.Label(new Rect(r.x, curY, r.width, 18), $"🎞️ FRAME HIỆN TẠI: [{currentFrameIndex + 1}/8]", lbl);
            curY += 20f;

            float frameBtnW = (r.width - 14f) / 8f;
            for (int f = 0; f < TOTAL_STUDIO_FRAMES; f++)
            {
                Rect fRect = new Rect(r.x + f * (frameBtnW + 2f), curY, frameBtnW, 26f);
                bool isActive = (f == currentFrameIndex);
                GUI.color = isActive ? new Color(1f, 0.85f, 0.25f) : new Color(0.80f, 0.85f, 0.95f);
                if (GUI.Button(fRect, $"F{f}"))
                {
                    SaveCanvasIntoCurrentFrame();
                    LoadFrameIntoCanvas(f);
                }
                if (isActive)
                {
                    FantasyUITheme.DrawOutline(fRect, Color.yellow, 2);
                }
                GUI.color = Color.white;
            }
            curY += 30f;

            // Nút thao tác Frame nhanh (Nạp mẫu, nhân bản, xóa)
            float actionBtnW = (r.width - 6f) / 3f;
            if (GUI.Button(new Rect(r.x, curY, actionBtnW, 22f), "📥 Nạp Mẫu"))
            {
                LoadMagePresetForDirection(currentEditDirIndex);
            }
            if (GUI.Button(new Rect(r.x + actionBtnW + 3f, curY, actionBtnW, 22f), "📑 Nhân Bản"))
            {
                SaveCanvasIntoCurrentFrame();
                int nextFrame = (currentFrameIndex + 1) % TOTAL_STUDIO_FRAMES;
                System.Array.Copy(canvasPixels, multiFramePixels[currentEditDirIndex][nextFrame], canvasPixels.Length);
                RefreshFrameTexturesForCurrentDir();
                LoadFrameIntoCanvas(nextFrame);
                studioExportStatus = $"📑 Đã nhân bản Frame {currentFrameIndex} sang Frame {nextFrame}!";
            }
            GUI.color = new Color(1f, 0.45f, 0.45f);
            if (GUI.Button(new Rect(r.x + (actionBtnW + 3f) * 2f, curY, actionBtnW, 22f), "🗑️ Xóa Frame"))
            {
                ClearCostumeCanvas();
            }
            GUI.color = Color.white;
            curY += 28f;

            // 3. BỘ CÔNG CỤ VẼ & THAO TÁC PIXEL
            GUI.Label(new Rect(r.x, curY, r.width, 18), "🖌️ BỘ CÔNG CỤ PIXEL:", lbl);
            curY += 20f;
            string[] toolNames = { "✏️ Vẽ", "🧹 Tẩy", "🪣 Đổ", "💧 Hút", "🔲 Chọn", "✋ Dời" };
            int curToolIdx = (int)currentCostumeTool;
            int newToolIdx = GUI.SelectionGrid(new Rect(r.x, curY, r.width, 48f), curToolIdx, toolNames, 3);
            if (newToolIdx != curToolIdx) currentCostumeTool = (CostumeTool)newToolIdx;
            curY += 52f;

            // 4. THAO TÁC VÙNG CHỌN (COPY / PASTE / DI DỜI CHÍNH XÁC)
            GUI.Label(new Rect(r.x, curY, r.width, 18), "📋 THAO TÁC VÙNG CHỌN:", lbl);
            curY += 20f;

            float cpBtnW = (r.width - 4f) / 2f;
            if (GUI.Button(new Rect(r.x, curY, cpBtnW, 22f), "📋 Copy Vùng"))
            {
                CopySelection();
            }
            if (GUI.Button(new Rect(r.x + cpBtnW + 4f, curY, cpBtnW, 22f), "📥 Dán (Paste)"))
            {
                PasteClipboard();
            }
            curY += 26f;

            // 4 nút dời pixel vi mô: ⬅️ ➡️ ⬆️ ⬇️
            GUI.Label(new Rect(r.x, curY, 80, 20), "Dời 1 Pixel:", subLbl);
            float arrowW = 26f;
            float arrowStartX = r.x + 85f;
            if (GUI.Button(new Rect(arrowStartX, curY, arrowW, 20f), "⬅️")) MoveSelectedPixels(-1, 0);
            if (GUI.Button(new Rect(arrowStartX + arrowW + 3f, curY, arrowW, 20f), "➡️")) MoveSelectedPixels(1, 0);
            if (GUI.Button(new Rect(arrowStartX + (arrowW + 3f) * 2f, curY, arrowW, 20f), "⬆️")) MoveSelectedPixels(0, 1);
            if (GUI.Button(new Rect(arrowStartX + (arrowW + 3f) * 3f, curY, arrowW, 20f), "⬇️")) MoveSelectedPixels(0, -1);
            curY += 26f;

            // 5. BẢNG MÀU PIXEL ART CHUẨN
            GUI.Label(new Rect(r.x, curY, r.width, 18), "🎨 BẢNG MÀU CHỌN NHANH:", lbl);
            curY += 20f;

            Color32[] palette = {
                new Color32(235, 185, 50, 255),  // Hoàng Kim
                new Color32(230, 40, 40, 255),   // Đỏ Huyết / Áo Choàng Pháp Sư
                new Color32(144, 36, 0, 255),    // Đỏ Thẫm Áo Choàng
                new Color32(50, 150, 250, 255),  // Lam Băng
                new Color32(50, 210, 90, 255),   // Lục Bảo
                new Color32(180, 70, 240, 255),  // Tím Ma Thuật
                new Color32(255, 130, 40, 255),  // Cam Lửa
                new Color32(255, 255, 255, 255), // Trắng Tinh
                new Color32(180, 190, 205, 255), // Bạc Thiết Giáp
                new Color32(36, 36, 0, 255),     // Đen Thẫm Mũ Trùm
                new Color32(25, 28, 38, 255),    // Đen Obsidian
                new Color32(245, 190, 160, 255)  // Màu Da Người
            };

            float swatchSize = (r.width - 18f) / 4f;
            for (int i = 0; i < palette.Length; i++)
            {
                int row = i / 4;
                int col = i % 4;
                Rect swRect = new Rect(r.x + col * (swatchSize + 6f), curY + row * (swatchSize + 6f), swatchSize, swatchSize);

                GUI.color = palette[i];
                GUI.DrawTexture(swRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                bool isSelected = (currentCostumeColor.r == palette[i].r && currentCostumeColor.g == palette[i].g && currentCostumeColor.b == palette[i].b && currentCostumeColor.a == palette[i].a);
                if (isSelected)
                {
                    FantasyUITheme.DrawOutline(swRect, Color.yellow, 2);
                }
                else
                {
                    FantasyUITheme.DrawOutline(swRect, Color.black, 1);
                }

                if (GUI.Button(swRect, GUIContent.none, GUIStyle.none))
                {
                    currentCostumeColor = palette[i];
                    if (currentCostumeTool == CostumeTool.Eraser) currentCostumeTool = CostumeTool.Pencil;
                }
            }
            curY += (palette.Length / 4) * (swatchSize + 6f) + 8f;

            // Màu Đang Chọn
            GUI.Label(new Rect(r.x, curY, 80, 20), "Màu Hiện Tại:", subLbl);
            Rect curColRect = new Rect(r.x + 85, curY, r.width - 85, 20);
            GUI.color = currentCostumeColor;
            GUI.DrawTexture(curColRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(curColRect, Color.white, 1);
        }

        // ─── CỘT 2: CANVAS PIXEL & ONION SKINNING ──────────────────────────────────────────────────
        private void DrawCostumeCanvasColumn(Rect r)
        {
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.95f, 1f) } };
            var tipStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.6f, 0.85f, 1f) } };

            // Điều khiển Onion Skin
            float optY = r.y + 4f;
            enableOnionSkin = GUI.Toggle(new Rect(r.x, optY, 150, 18), enableOnionSkin, " 👻 Bật Onion Skin");
            if (enableOnionSkin)
            {
                GUI.Label(new Rect(r.x + 160, optY, 80, 18), "Độ Mờ:", tipStyle);
                onionSkinAlpha = GUI.HorizontalSlider(new Rect(r.x + 220, optY, r.width - 230, 16), onionSkinAlpha, 0.1f, 0.8f);
            }
            optY += 22f;

            float canvasDispSize = Mathf.Min(r.width - 20f, 260f);
            float canvasX = r.x + (r.width - canvasDispSize) / 2f;
            float canvasY = optY;
            Rect canvasRect = new Rect(canvasX, canvasY, canvasDispSize, canvasDispSize);

            // 1. Nền Checkerboard tối
            GUI.color = new Color(0.08f, 0.10f, 0.14f, 1f);
            GUI.DrawTexture(canvasRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. Lớp Onion Skin (Vệt mờ frame trước để căn chỉnh bước đi)
            if (enableOnionSkin && frameTextures != null)
            {
                int prevFrame = (currentFrameIndex > 0) ? (currentFrameIndex - 1) : (TOTAL_STUDIO_FRAMES - 1);
                Texture2D prevTex = frameTextures[prevFrame];
                if (prevTex != null)
                {
                    GUI.color = new Color(1f, 1f, 1f, onionSkinAlpha);
                    GUI.DrawTexture(canvasRect, prevTex, ScaleMode.StretchToFill);
                    GUI.color = Color.white;
                }
            }

            // 3. Lớp Bản Vẽ Pixel Frame Hiện Tại
            if (canvasTex != null)
            {
                GUI.DrawTexture(canvasRect, canvasTex, ScaleMode.StretchToFill);
            }

            // 4. Viền Canvas
            FantasyUITheme.DrawDoubleGoldFrame(canvasRect, 1);

            // 5. Khung Vùng Chọn (Box Selection)
            if (costumeBoxSelection.width > 0 && costumeBoxSelection.height > 0)
            {
                float pxW = canvasDispSize / CANVAS_SIZE;
                float pxH = canvasDispSize / CANVAS_SIZE;
                float selX = canvasX + costumeBoxSelection.x * pxW;
                float selY = canvasY + (CANVAS_SIZE - costumeBoxSelection.y - costumeBoxSelection.height) * pxH;
                Rect selScreenRect = new Rect(selX, selY, costumeBoxSelection.width * pxW, costumeBoxSelection.height * pxH);

                GUI.color = new Color(1f, 0.85f, 0.2f, 0.25f);
                GUI.DrawTexture(selScreenRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                FantasyUITheme.DrawOutline(selScreenRect, new Color(1f, 0.9f, 0.2f, 0.95f), 2);
            }

            // 6. Xử Lý Tương Tác Chuột Vẽ Pixel
            HandleCanvasMouseInput(canvasRect);

            // 7. Gán Nhanh Vùng Chọn Vào Trang Bị
            float barY = canvasY + canvasDispSize + 8f;
            GUI.Label(new Rect(r.x, barY, r.width, 18), "👉 GÁN VÙNG VẼ VÀO TRANG BỊ / NGOẠI TRANG:", lbl);
            barY += 20f;

            float slotBtnW = (r.width - 16f) / 5f;
            EquipSlot[] quickSlots = { EquipSlot.Helmet, EquipSlot.Armor, EquipSlot.Pants, EquipSlot.Boots, EquipSlot.Costume };
            string[] slotLabels = { "🪖 Mũ", "🥋 Áo", "👖 Quần", "👢 Giày", "👘 Ngoại Trang" };

            for (int i = 0; i < quickSlots.Length; i++)
            {
                Rect bRect = new Rect(r.x + i * (slotBtnW + 4f), barY, slotBtnW, 24f);
                bool isCurrent = (targetCostumeSlot == quickSlots[i]);
                GUI.color = isCurrent ? new Color(1f, 0.85f, 0.25f) : new Color(0.85f, 0.9f, 1f);
                if (GUI.Button(bRect, slotLabels[i]))
                {
                    targetCostumeSlot = quickSlots[i];
                    costumeItemName = (targetCostumeSlot == EquipSlot.Helmet) ? "Mũ Pháp Sư Cổ Đại" :
                                      (targetCostumeSlot == EquipSlot.Armor) ? "Áo Choàng Đỏ Viền Vàng" :
                                      (targetCostumeSlot == EquipSlot.Pants) ? "Quần Pháp Sư" :
                                      (targetCostumeSlot == EquipSlot.Boots) ? "Chiến Ngoa Phép Thuật" : "Pháp Sư Toàn Thân 8 Hướng";
                    UpdateLiveWearableSprite();
                }
                GUI.color = Color.white;
            }
            barY += 28f;

            GUI.Label(new Rect(r.x, barY, r.width, 36f), "💡 Mẹo: Dùng Onion Skin để quan sát bước chân của frame trước. Dùng [🔲 Chọn] và [✋ Dời] hoặc các nút mũi tên để di chuyển vị trí chi tiết tùy ý!", tipStyle);
        }

        private void HandleCanvasMouseInput(Rect canvasRect)
        {
            Event e = Event.current;
            if (canvasRect.Contains(e.mousePosition))
            {
                float localX = e.mousePosition.x - canvasRect.x;
                float localY = e.mousePosition.y - canvasRect.y;
                int px = Mathf.Clamp(Mathf.FloorToInt(localX / (canvasRect.width / CANVAS_SIZE)), 0, CANVAS_SIZE - 1);
                int py = Mathf.Clamp(CANVAS_SIZE - 1 - Mathf.FloorToInt(localY / (canvasRect.height / CANVAS_SIZE)), 0, CANVAS_SIZE - 1);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (currentCostumeTool == CostumeTool.Pencil)
                    {
                        SetCanvasPixel(px, py, currentCostumeColor);
                    }
                    else if (currentCostumeTool == CostumeTool.Eraser)
                    {
                        SetCanvasPixel(px, py, new Color32(0, 0, 0, 0));
                    }
                    else if (currentCostumeTool == CostumeTool.FillBucket)
                    {
                        FloodFillCanvas(px, py, currentCostumeColor);
                    }
                    else if (currentCostumeTool == CostumeTool.EyeDropper)
                    {
                        Color32 sampled = canvasPixels[py * CANVAS_SIZE + px];
                        if (sampled.a > 0) currentCostumeColor = sampled;
                        currentCostumeTool = CostumeTool.Pencil;
                    }
                    else if (currentCostumeTool == CostumeTool.BoxSelect)
                    {
                        isBoxDragging = true;
                        boxDragStart = new Vector2Int(px, py);
                        costumeBoxSelection = new RectInt(px, py, 1, 1);
                    }
                    else if (currentCostumeTool == CostumeTool.MoveSelection)
                    {
                        isBoxDragging = true;
                        boxDragStart = new Vector2Int(px, py);
                    }
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    if (currentCostumeTool == CostumeTool.Pencil)
                    {
                        SetCanvasPixel(px, py, currentCostumeColor);
                    }
                    else if (currentCostumeTool == CostumeTool.Eraser)
                    {
                        SetCanvasPixel(px, py, new Color32(0, 0, 0, 0));
                    }
                    else if (currentCostumeTool == CostumeTool.BoxSelect && isBoxDragging)
                    {
                        int minX = Mathf.Min(boxDragStart.x, px);
                        int maxX = Mathf.Max(boxDragStart.x, px);
                        int minY = Mathf.Min(boxDragStart.y, py);
                        int maxY = Mathf.Max(boxDragStart.y, py);
                        costumeBoxSelection = new RectInt(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
                    }
                    else if (currentCostumeTool == CostumeTool.MoveSelection && isBoxDragging)
                    {
                        int dx = px - boxDragStart.x;
                        int dy = py - boxDragStart.y;
                        if (dx != 0 || dy != 0)
                        {
                            MoveSelectedPixels(dx, dy);
                            boxDragStart = new Vector2Int(px, py);
                        }
                    }
                    e.Use();
                }
                else if (e.type == EventType.MouseUp && e.button == 0)
                {
                    if (isBoxDragging)
                    {
                        isBoxDragging = false;
                        SaveCanvasIntoCurrentFrame();
                        UpdateLiveWearableSprite();
                    }
                }
            }
        }

        private void SetCanvasPixel(int x, int y, Color32 col)
        {
            if (x < 0 || x >= CANVAS_SIZE || y < 0 || y >= CANVAS_SIZE) return;
            canvasPixels[y * CANVAS_SIZE + x] = col;
            canvasTex.SetPixel(x, y, col);
            canvasTex.Apply();
            SaveCanvasIntoCurrentFrame();
            UpdateLiveWearableSprite();
        }

        private void FloodFillCanvas(int startX, int startY, Color32 fillCol)
        {
            if (startX < 0 || startX >= CANVAS_SIZE || startY < 0 || startY >= CANVAS_SIZE) return;
            Color32 targetCol = canvasPixels[startY * CANVAS_SIZE + startX];
            if (targetCol.r == fillCol.r && targetCol.g == fillCol.g && targetCol.b == fillCol.b && targetCol.a == fillCol.a) return;

            Queue<Vector2Int> q = new Queue<Vector2Int>();
            q.Enqueue(new Vector2Int(startX, startY));
            bool[] visited = new bool[CANVAS_SIZE * CANVAS_SIZE];

            while (q.Count > 0)
            {
                var pt = q.Dequeue();
                int idx = pt.y * CANVAS_SIZE + pt.x;
                if (pt.x < 0 || pt.x >= CANVAS_SIZE || pt.y < 0 || pt.y >= CANVAS_SIZE) continue;
                if (visited[idx]) continue;
                visited[idx] = true;

                Color32 c = canvasPixels[idx];
                if (c.r != targetCol.r || c.g != targetCol.g || c.b != targetCol.b || c.a != targetCol.a) continue;

                canvasPixels[idx] = fillCol;
                canvasTex.SetPixel(pt.x, pt.y, fillCol);

                if (pt.x > 0) q.Enqueue(new Vector2Int(pt.x - 1, pt.y));
                if (pt.x < CANVAS_SIZE - 1) q.Enqueue(new Vector2Int(pt.x + 1, pt.y));
                if (pt.y > 0) q.Enqueue(new Vector2Int(pt.x, pt.y - 1));
                if (pt.y < CANVAS_SIZE - 1) q.Enqueue(new Vector2Int(pt.x, pt.y + 1));
            }
            canvasTex.Apply();
            SaveCanvasIntoCurrentFrame();
            UpdateLiveWearableSprite();
        }

        private void CopySelection()
        {
            if (costumeBoxSelection.width <= 0 || costumeBoxSelection.height <= 0)
            {
                studioExportStatus = "⚠️ Hãy dùng [🔲 Chọn] để khoanh vùng trước khi Copy!";
                return;
            }

            clipboardW = costumeBoxSelection.width;
            clipboardH = costumeBoxSelection.height;
            clipboardPixels = new Color32[clipboardW * clipboardH];

            for (int y = 0; y < clipboardH; y++)
            {
                for (int x = 0; x < clipboardW; x++)
                {
                    int srcX = costumeBoxSelection.x + x;
                    int srcY = costumeBoxSelection.y + y;
                    clipboardPixels[y * clipboardW + x] = canvasPixels[srcY * CANVAS_SIZE + srcX];
                }
            }
            studioExportStatus = $"📋 Đã copy vùng {clipboardW}x{clipboardH} pixel vào clipboard!";
            GodlessFantasy.Core.AudioManager.PlayPickup();
        }

        private void PasteClipboard()
        {
            if (clipboardPixels == null || clipboardW <= 0 || clipboardH <= 0)
            {
                studioExportStatus = "⚠️ Clipboard rỗng! Hãy chọn vùng và bấm [Copy Vùng] trước.";
                return;
            }

            int dstX = (costumeBoxSelection.width > 0) ? costumeBoxSelection.x : 0;
            int dstY = (costumeBoxSelection.height > 0) ? costumeBoxSelection.y : 0;
            dstX = Mathf.Clamp(dstX, 0, CANVAS_SIZE - clipboardW);
            dstY = Mathf.Clamp(dstY, 0, CANVAS_SIZE - clipboardH);

            for (int y = 0; y < clipboardH; y++)
            {
                for (int x = 0; x < clipboardW; x++)
                {
                    Color32 c = clipboardPixels[y * clipboardW + x];
                    if (c.a > 0)
                    {
                        canvasPixels[(dstY + y) * CANVAS_SIZE + (dstX + x)] = c;
                    }
                }
            }

            costumeBoxSelection = new RectInt(dstX, dstY, clipboardW, clipboardH);
            canvasTex.SetPixels32(canvasPixels);
            canvasTex.Apply();
            SaveCanvasIntoCurrentFrame();
            UpdateLiveWearableSprite();
            studioExportStatus = $"📥 Đã dán vùng {clipboardW}x{clipboardH} pixel vào canvas!";
            GodlessFantasy.Core.AudioManager.PlayDrop();
        }

        private void MoveSelectedPixels(int deltaX, int deltaY)
        {
            if (costumeBoxSelection.width <= 0 || costumeBoxSelection.height <= 0)
            {
                studioExportStatus = "⚠️ Hãy dùng [🔲 Chọn] để khoanh vùng trước khi di chuyển!";
                return;
            }

            Color32[] temp = new Color32[costumeBoxSelection.width * costumeBoxSelection.height];
            for (int y = 0; y < costumeBoxSelection.height; y++)
            {
                for (int x = 0; x < costumeBoxSelection.width; x++)
                {
                    int srcX = costumeBoxSelection.x + x;
                    int srcY = costumeBoxSelection.y + y;
                    temp[y * costumeBoxSelection.width + x] = canvasPixels[srcY * CANVAS_SIZE + srcX];
                    canvasPixels[srcY * CANVAS_SIZE + srcX] = new Color32(0, 0, 0, 0); // Xóa vị trí cũ
                }
            }

            int newX = Mathf.Clamp(costumeBoxSelection.x + deltaX, 0, CANVAS_SIZE - costumeBoxSelection.width);
            int newY = Mathf.Clamp(costumeBoxSelection.y + deltaY, 0, CANVAS_SIZE - costumeBoxSelection.height);

            for (int y = 0; y < costumeBoxSelection.height; y++)
            {
                for (int x = 0; x < costumeBoxSelection.width; x++)
                {
                    int dstX = newX + x;
                    int dstY = newY + y;
                    canvasPixels[dstY * CANVAS_SIZE + dstX] = temp[y * costumeBoxSelection.width + x];
                }
            }

            costumeBoxSelection = new RectInt(newX, newY, costumeBoxSelection.width, costumeBoxSelection.height);
            canvasTex.SetPixels32(canvasPixels);
            canvasTex.Apply();
            SaveCanvasIntoCurrentFrame();
            UpdateLiveWearableSprite();
        }

        private void ClearCostumeCanvas()
        {
            if (canvasPixels == null || canvasTex == null) return;
            for (int i = 0; i < canvasPixels.Length; i++) canvasPixels[i] = new Color32(0, 0, 0, 0);
            canvasTex.SetPixels32(canvasPixels);
            canvasTex.Apply();
            costumeBoxSelection = new RectInt(0, 0, 0, 0);
            SaveCanvasIntoCurrentFrame();
            UpdateLiveWearableSprite();
            studioExportStatus = $"🗑️ Đã xóa sạch Frame {currentFrameIndex}!";
        }

        private void UpdateLiveWearableSprite()
        {
            if (canvasTex == null) return;

            Rect srcRect;
            Vector2 pivot;

            if (costumeBoxSelection.width > 0 && costumeBoxSelection.height > 0)
            {
                srcRect = new Rect(costumeBoxSelection.x, costumeBoxSelection.y, costumeBoxSelection.width, costumeBoxSelection.height);
                pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                srcRect = new Rect(0, 0, CANVAS_SIZE, CANVAS_SIZE);
                pivot = new Vector2(0.5f, 0.12f);
            }

            liveWearableSprite = Sprite.Create(canvasTex, srcRect, pivot, 32f);
        }

        // ─── CỘT 3: XEM TRƯỚC HOẠT ẢNH THỜI GIAN THỰC & XUẤT SPRITE ──────────────────────────────
        private void DrawCostumePreviewActionsColumn(Rect r)
        {
            float pY = r.y + 4f;
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.95f, 1f) } };
            var subLbl = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(0.7f, 0.85f, 1f) } };

            // Cập nhật nhịp vòng lặp hoạt ảnh xem trước (Live Loop)
            if (isPreviewPlaying)
            {
                previewTimer += Time.deltaTime * previewFps;
                previewCurrentFrame = ((int)previewTimer) % TOTAL_STUDIO_FRAMES;
            }

            // 1. KHUNG XEM TRƯỚC HOẠT ẢNH THỜI GIAN THỰC (LIVE ANIMATION PREVIEW BOX)
            GUI.Label(new Rect(r.x, pY, r.width, 18), $"▶️ HOẠT ẢNH: Frame {previewCurrentFrame} [{previewFps:F1} FPS]", lbl);
            pY += 20f;

            Rect previewBox = new Rect(r.x + (r.width - 96f) / 2f, pY, 96f, 96f);
            GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.98f);
            GUI.DrawTexture(previewBox, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Hiển thị frame animation hiện tại của hướng preview
            if (frameTextures != null && frameTextures[previewCurrentFrame] != null)
            {
                GUI.DrawTexture(new Rect(previewBox.x + 6, previewBox.y + 6, previewBox.width - 12, previewBox.height - 12), frameTextures[previewCurrentFrame], ScaleMode.ScaleToFit);
            }

            FantasyUITheme.DrawOutline(previewBox, tierColors[costumeTierIdx], 2);
            pY += 102f;

            // Điều khiển Play/Pause và Tốc độ FPS
            float ctrlW = (r.width - 6f) / 2f;
            GUI.color = isPreviewPlaying ? new Color(1f, 0.5f, 0.5f) : new Color(0.5f, 1f, 0.5f);
            if (GUI.Button(new Rect(r.x, pY, ctrlW, 22f), isPreviewPlaying ? "⏸ Tạm Dừng" : "▶ Tiếp Tục"))
            {
                isPreviewPlaying = !isPreviewPlaying;
            }
            GUI.color = Color.white;

            // Nút Đồng Bộ Frame Xem Trước với Canvas
            if (GUI.Button(new Rect(r.x + ctrlW + 4f, pY, ctrlW, 22f), "👁️ Xem Frame Vẽ"))
            {
                isPreviewPlaying = false;
                previewCurrentFrame = currentFrameIndex;
            }
            pY += 26f;

            // Slider điều chỉnh tốc độ hoạt ảnh
            GUI.Label(new Rect(r.x, pY, 100, 18), $"Tốc Độ: {previewFps:F1} FPS", subLbl);
            previewFps = GUI.HorizontalSlider(new Rect(r.x + 105, pY, r.width - 105, 16), previewFps, 4f, 16f);
            pY += 24f;

            // 2. THIẾT LẬP THÔNG SỐ VẬT PHẨM TRANG PHỤC
            GUI.Label(new Rect(r.x, pY, 80, 20), "Tên Món Đồ:", lbl);
            costumeItemName = GUI.TextField(new Rect(r.x + 85, pY, r.width - 85, 20), costumeItemName);
            pY += 24f;

            GUI.Label(new Rect(r.x, pY, 80, 20), "Phẩm Cấp:", lbl);
            costumeTierIdx = GUI.SelectionGrid(new Rect(r.x + 85, pY, r.width - 85, 34f), costumeTierIdx, tierNames, 4);
            pY += 38f;

            // 3. CÁC NÚT HÀNH ĐỘNG CỰC KỲ MẠNH MẼ (LIVE SPRITE ACTIONS)
            float btnH = 24f;
            float btnGap = 4f;

            // ⚡ NÚT 1: MẶC THỬ TRỰC TIẾP LÊN NHÂN VẬT (Live Apply To Player)
            GUI.color = new Color(1.0f, 0.88f, 0.25f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "⚡ MẶC THỬ NGAY LÊN NHÂN VẬT"))
            {
                LiveTestCostumeOnRig();
            }
            pY += btnH + btnGap;

            // 💾 NÚT 2: LƯU TOÀN BỘ 8 FRAMES CỦA HƯỚNG (.PNG)
            GUI.color = new Color(0.3f, 0.85f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), $"💾 LƯU 8 FRAMES [{studioDirCodes[currentEditDirIndex]}] (.PNG)"))
            {
                SaveAll8FramesPng();
            }
            pY += btnH + btnGap;

            // 💾 NÚT 3: LƯU FRAME HIỆN TẠI (.PNG)
            GUI.color = new Color(0.85f, 0.9f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), $"💾 LƯU FRAME {currentFrameIndex} (.PNG)"))
            {
                SaveCostumePng();
            }
            pY += btnH + btnGap;

            // 🎒 NÚT 4: TẠO VÀO TÚI ĐỒ (Add To Inventory)
            GUI.color = new Color(0.2f, 0.95f, 0.45f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "🎒 TẠO VÀO TÚI ĐỒ [B]"))
            {
                AddCostumeToBag();
            }
            pY += btnH + btnGap;
            GUI.color = Color.white;

            // Trạng thái thông báo Studio
            if (!string.IsNullOrEmpty(studioExportStatus))
            {
                var stStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = studioExportStatus.StartsWith("✅") ? Color.green : Color.yellow } };
                GUI.Label(new Rect(r.x, pY, r.width, 36), studioExportStatus, stStyle);
            }
        }

        private void LiveTestCostumeOnRig()
        {
            SaveCanvasIntoCurrentFrame();
            UpdateLiveWearableSprite();

            var player = CharacterVisualController8Dir.Instance;
            if (player != null)
            {
                var anim = player.GetComponent<CharacterSpriteAnimator8Dir>();
                if (anim == null) anim = player.gameObject.AddComponent<CharacterSpriteAnimator8Dir>();

                player.useTraditionalFrameAnimation = true;
                player.useProceduralRig = false;

                // Gán frame hiện tại làm avatar / sprite ngay lập tức
                var sr = player.GetComponent<SpriteRenderer>();
                if (sr != null && liveWearableSprite != null)
                {
                    sr.sprite = liveWearableSprite;
                }

                studioExportStatus = $"✅ ĐÃ MẶC THỬ '{costumeItemName}' TRỰC TIẾP LÊN NHÂN VẬT!";
                GodlessFantasy.Core.AudioManager.PlayEquip();
            }
            else
            {
                studioExportStatus = "⚠️ Không tìm thấy CharacterVisualController8Dir trong thế giới!";
            }
        }

        private void AddCostumeToBag()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null) return;

            if (inv.IsFull)
            {
                studioExportStatus = "⚠️ Hành trang đã đầy (24/24)! Hãy vứt bớt đồ trước.";
                return;
            }

            SaveCanvasIntoCurrentFrame();
            UpdateLiveWearableSprite();

            ItemType t = (targetCostumeSlot == EquipSlot.Costume) ? ItemType.Costume : ItemType.Equipment;
            string tier = tierNames[costumeTierIdx];
            Color c = tierColors[costumeTierIdx];

            string stats = (targetCostumeSlot == EquipSlot.Helmet) ? "+25 Giáp, +80 HP" :
                           (targetCostumeSlot == EquipSlot.Armor) ? "+45 Giáp, +150 HP" :
                           (targetCostumeSlot == EquipSlot.Pants) ? "+30 Giáp, +10 Tốc Độ" :
                           (targetCostumeSlot == EquipSlot.Boots) ? "+18 Tốc Độ, +12 AGI" : "+50 HP, +50 MP, +15 Toàn Chỉ Số";

            string desc = $"Trang phục thủ công 8 hướng được thiết kế từ Godless Studio! Khớp chuẩn bước đi truyền thống.";

            var newItem = new InventoryItem(
                $"custom_{targetCostumeSlot}_{DateTime.Now.Ticks % 10000}",
                costumeItemName,
                t,
                targetCostumeSlot,
                tier,
                c,
                "item_costume",
                stats,
                desc,
                1,
                null,
                "Toàn Hệ",
                "Toàn Thuộc Tính",
                ItemGender.All
            ).WithCustomSprite(liveWearableSprite);

            if (inv.AddItem(newItem))
            {
                studioExportStatus = $"✅ Đã tạo thành công '{costumeItemName}' vào túi đồ [B]!";
                GodlessFantasy.Core.AudioManager.PlayDrop();
            }
            else
            {
                studioExportStatus = "⚠️ Lỗi khi thêm vật phẩm vào hành trang!";
            }
        }

        private void SaveCostumePng()
        {
            if (canvasTex == null) return;
            try
            {
                string dir = Path.Combine(Application.persistentDataPath, "custom_characters");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string fileName = $"char_{studioDirCodes[currentEditDirIndex]}_f{currentFrameIndex}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string fullPath = Path.Combine(dir, fileName);

                byte[] bytes = canvasTex.EncodeToPNG();
                File.WriteAllBytes(fullPath, bytes);

                studioExportStatus = $"✅ Đã lưu file PNG frame {currentFrameIndex} tại:\n{fileName}";
                Debug.Log($"<color=green>[PIXEL STUDIO] Đã lưu file PNG:</color> {fullPath}");
                GodlessFantasy.Core.AudioManager.PlayPickup();
            }
            catch (Exception ex)
            {
                studioExportStatus = $"⚠️ Lỗi lưu file: {ex.Message}";
            }
        }

        private void SaveAll8FramesPng()
        {
            try
            {
                string dir = Path.Combine(Application.persistentDataPath, "custom_characters", studioDirCodes[currentEditDirIndex]);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                for (int f = 0; f < TOTAL_STUDIO_FRAMES; f++)
                {
                    if (frameTextures[f] != null)
                    {
                        byte[] bytes = frameTextures[f].EncodeToPNG();
                        string path = Path.Combine(dir, $"frame_{f}.png");
                        File.WriteAllBytes(path, bytes);
                    }
                }

                studioExportStatus = $"✅ Đã lưu đủ 8 Frames hướng {studioDirLabels[currentEditDirIndex]} vào PersistentData!";
                Debug.Log($"<color=green>[PIXEL STUDIO] Đã lưu đủ 8 Frames vào:</color> {dir}");
                GodlessFantasy.Core.AudioManager.PlayLevelUp();
            }
            catch (Exception ex)
            {
                studioExportStatus = $"⚠️ Lỗi lưu 8 frames: {ex.Message}";
            }
        }
    }
}
