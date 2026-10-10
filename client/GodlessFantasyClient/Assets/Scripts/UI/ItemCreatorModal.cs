using System;
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
        private int studioMode = 0; // 0 = Tạo Vật Phẩm, 1 = Tạo Kỹ Năng
        private readonly string[] modeTabs = { "⚔️ THIẾT KẾ VẬT PHẨM & TRANG BỊ", "📜 THIẾT KẾ KỸ NĂNG & CUỘN TRỤC" };

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
        private readonly string[] itemIconOptions = { "item_weapon", "item_helmet", "item_armor", "item_pants", "item_boots", "item_ring", "item_necklace", "item_potion_hp", "item_potion_mana", "pet_squirrel" };

        private string formItemStats = "+25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)";
        private string formItemDesc = "Bảo kiếm hoàng kim rèn từ thép cổ đại, gia tăng sức mạnh toàn diện cho dũng sĩ.";

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
        private readonly string[] indicatorNames = { "1. Bắn Thẳng", "2. Vùng Đất / Mưa", "3. Hình Quạt", "4. Nổ 360°", "5. Tự Thân" };

        private string formSkillMana = "25";
        private string formSkillCd = "3.5";
        private string formSkillDmg = "45";
        private string formSkillScaling = "80";
        private string formSkillRange = "8.0";

        private int formSkillHotbarIconIndex = 0;
        private readonly string[] skillHotbarIcons = { "icon_skill_meteor", "icon_skill_flame_cone", "icon_skill_divine_nova", "icon_skill_arcane_bolt", "icon_skill_dash", "skill_fireball", "skill_frost", "skill_lightning" };
        private List<string> customHotbarIconsList = new List<string>();

        // Cơ chế nạp ảnh ngoài (External Image Import)
        private string customImageInputPath = "";
        private string customImageStatus = "";

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
            new ItemPreset { name = "Bảo Kiếm Hoàng Kim", id = "custom_wep_gold", typeIdx = 0, slotIdx = 7, tierIdx = 4, icon = "item_weapon", stats = "+25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)", desc = "Kiếm cổ đại dát vàng tăng mạnh sức công kích và ma pháp." },
            new ItemPreset { name = "Mũ Chiến Binh Sắt", id = "custom_helm_iron", typeIdx = 0, slotIdx = 1, tierIdx = 2, icon = "item_helmet", stats = "+12 Thể Lực (VIT) · +5 Sức Mạnh (STR)", desc = "Mũ giáp bền bỉ che chắn cho dũng sĩ trong giao tranh." },
            new ItemPreset { name = "Áo Choàng Thánh Quang", id = "custom_armor_holy", typeIdx = 0, slotIdx = 2, tierIdx = 5, icon = "item_armor", stats = "+30 Thể Lực (VIT) · +15 Trí Tuệ (INT)", desc = "Áo choàng hộ mệnh thần thánh ban phước lành bình an." },
            new ItemPreset { name = "Quần Phù Thủy Gió", id = "custom_pants_wind", typeIdx = 0, slotIdx = 3, tierIdx = 3, icon = "item_pants", stats = "+18 Nhanh Nhẹn (AGI) · +10 INT", desc = "Vải lụa nhẹ như gió giúp dũng sĩ lướt đi linh hoạt." },
            new ItemPreset { name = "Giày Phong Hành Thần", id = "custom_boots_speed", typeIdx = 0, slotIdx = 4, tierIdx = 3, icon = "item_boots", stats = "+20 Nhanh Nhẹn (AGI) · Tốc độ chạy +15%", desc = "Đôi giày tăng mạnh tốc độ di chuyển và giảm hồi chiêu Lướt." },
            new ItemPreset { name = "Nhẫn Hộ Mệnh Cổ Đại", id = "custom_ring_guard", typeIdx = 0, slotIdx = 5, tierIdx = 4, icon = "item_ring", stats = "+15 Thể Lực (VIT) · +15 Sức Mạnh (STR)", desc = "Nhẫn hộ mệnh giúp giảm sát thương nhận vào." },
            new ItemPreset { name = "Dây Chuyền Thần Ma", id = "custom_neck_magic", typeIdx = 0, slotIdx = 6, tierIdx = 5, icon = "item_necklace", stats = "+35 Trí Tuệ (INT) · Mana Tối Đa +60", desc = "Trang sức tỏa hào quang ma thuật vô biên." },
            new ItemPreset { name = "Bình Siêu Máu HP", id = "custom_pot_hp_super", typeIdx = 1, slotIdx = 0, tierIdx = 3, icon = "item_potion_hp", stats = "Hồi +100 HP tức thì", desc = "Bình dược phẩm tinh khiết hồi phục sinh lực thần tốc." },
            new ItemPreset { name = "Bình Đại Mana MP", id = "custom_pot_mp_grand", typeIdx = 1, slotIdx = 0, tierIdx = 3, icon = "item_potion_mana", stats = "Hồi +80 Mana tức thì", desc = "Nước suối pháp thuật tái tạo nguồn năng lượng dồi dào." },
            new ItemPreset { name = "Sóc Thần Hoàng Kim", id = "custom_pet_gold_sq", typeIdx = 2, slotIdx = 0, tierIdx = 5, icon = "pet_squirrel", stats = "Thú Cưng Đồng Hành · Tăng 5% EXP", desc = "Thú cưng linh vật quý tộc đồng hành cùng dũng sĩ." }
        };

        private readonly SkillPreset[] skillPresets = new SkillPreset[]
        {
            new SkillPreset { name = "Cầu Ma Thuật Tuyến Tính", id = "skill_arcane_bolt", classIdx = 1, elemIdx = 4, tierIdx = 1, indicatorIdx = 0, categoryIdx = 0, mana = "15", cd = "1.0", dmg = "25", scaling = "100", range = "8.0", hotbarIcon = "icon_skill_arcane_bolt", desc = "Bắn cầu ma pháp tốc độ cao xuyên thấu theo đường thẳng." },
            new SkillPreset { name = "Mưa Thiên Thạch Oanh Tạc", id = "skill_meteor_shower", classIdx = 1, elemIdx = 0, tierIdx = 2, indicatorIdx = 1, categoryIdx = 0, mana = "25", cd = "3.0", dmg = "40", scaling = "120", range = "7.0", hotbarIcon = "icon_skill_meteor", desc = "Triệu hồi loạt thiên thạch lửa giáng xuống vùng đất chỉ định gây nổ liên hoàn." },
            new SkillPreset { name = "Bão Lửa Quét Hình Nón", id = "skill_flame_wave", classIdx = 1, elemIdx = 0, tierIdx = 3, indicatorIdx = 2, categoryIdx = 0, mana = "20", cd = "2.5", dmg = "30", scaling = "80", range = "4.5", hotbarIcon = "icon_skill_flame_cone", desc = "Quét luồng bão lửa hình quạt 60 độ phía trước thiêu đốt nhiều mục tiêu." },
            new SkillPreset { name = "Bộc Phá Thánh Điển 360°", id = "skill_divine_nova", classIdx = 1, elemIdx = 3, tierIdx = 4, indicatorIdx = 3, categoryIdx = 0, mana = "35", cd = "5.0", dmg = "60", scaling = "150", range = "2.5", hotbarIcon = "icon_skill_divine_nova", desc = "Bùng nổ vòng hào quang thánh khiết 360 độ cực mạnh quét sạch quái vật xung quanh." },
            new SkillPreset { name = "Mưa Sinh Mệnh Hệ Mộc", id = "skill_healing_rain", classIdx = 1, elemIdx = 6, tierIdx = 4, indicatorIdx = 1, categoryIdx = 1, mana = "30", cd = "4.0", dmg = "45", scaling = "100", range = "7.0", hotbarIcon = "icon_skill_divine_nova", desc = "Triệu hồi mưa năng lượng ngọc bích rơi xuống vùng đất hồi phục máu cho toàn bộ đồng minh trong vùng!" },
            new SkillPreset { name = "Hộ Thể Thần Mộc (Buff)", id = "skill_wood_shield", classIdx = 4, elemIdx = 6, tierIdx = 3, indicatorIdx = 4, categoryIdx = 3, mana = "20", cd = "8.0", dmg = "25", scaling = "50", range = "0.0", hotbarIcon = "icon_skill_dash", desc = "Tự kích hoạt lá chắn mộc linh gia tăng giáp và hồi phục sinh lực bản thân!" },
            new SkillPreset { name = "Băng Huyết Tiễn Xuyên Tâm", id = "skill_frost_arrow", classIdx = 3, elemIdx = 1, tierIdx = 5, indicatorIdx = 0, categoryIdx = 0, mana = "22", cd = "2.0", dmg = "48", scaling = "130", range = "8.0", hotbarIcon = "skill_frost", desc = "Mũi tên băng hàn xuyên thấu gây sát thương lớn và làm chậm kẻ địch." },
            new SkillPreset { name = "Lôi Thần Nộ Oanh Kích", id = "skill_thunder_burst", classIdx = 1, elemIdx = 2, tierIdx = 5, indicatorIdx = 1, categoryIdx = 0, mana = "32", cd = "4.0", dmg = "55", scaling = "140", range = "7.0", hotbarIcon = "skill_lightning", desc = "Gọi sấm sét thiên lôi oanh kích dồn dập vào tâm vùng đất đã định." },
            new SkillPreset { name = "Thanh Kiếm Thánh Quang", id = "skill_holy_slash", classIdx = 0, elemIdx = 3, tierIdx = 4, indicatorIdx = 0, categoryIdx = 0, mana = "20", cd = "2.0", dmg = "50", scaling = "90", range = "8.0", hotbarIcon = "icon_skill_meteor", desc = "Chém ra đường kiếm thánh quang rực rỡ trừng phạt tà ma." },
            new SkillPreset { name = "Cuồng Phong Bạo Vũ Quyền", id = "skill_wind_fist", classIdx = 2, elemIdx = 5, tierIdx = 3, indicatorIdx = 2, categoryIdx = 0, mana = "18", cd = "1.8", dmg = "38", scaling = "85", range = "4.5", hotbarIcon = "icon_skill_flame_cone", desc = "Tung loạt quyền phong hình quạt quét ngã kẻ thù." }
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

        private Texture2D GetCachedTexture(string texName)
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

            DrawColumnFrame(col1Rect, "📂 PHÔI MẪU (PRESETS)");
            DrawColumnFrame(col2Rect, "⚙️ THIẾT LẬP THÔNG SỐ");
            DrawColumnFrame(col3Rect, "👁️ XEM TRƯỚC TRỰC TIẾP (1:1)");

            if (studioMode == 0)
            {
                DrawItemPresetsColumn(new Rect(col1Rect.x + 6, col1Rect.y + 26, col1Rect.width - 12, col1Rect.height - 32));
                DrawItemFormColumn(new Rect(col2Rect.x + 8, col2Rect.y + 26, col2Rect.width - 16, col2Rect.height - 32));
                DrawItemPreviewColumn(new Rect(col3Rect.x + 8, col3Rect.y + 26, col3Rect.width - 16, col3Rect.height - 32));
            }
            else
            {
                DrawSkillPresetsColumn(new Rect(col1Rect.x + 6, col1Rect.y + 26, col1Rect.width - 12, col1Rect.height - 32));
                DrawSkillFormColumn(new Rect(col2Rect.x + 8, col2Rect.y + 26, col2Rect.width - 16, col2Rect.height - 32));
                DrawSkillPreviewColumn(new Rect(col3Rect.x + 8, col3Rect.y + 26, col3Rect.width - 16, col3Rect.height - 32));
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
                    formItemIconIndex = Mathf.Max(0, Array.IndexOf(itemIconOptions, p.icon));
                    formItemStats = p.stats;
                    formItemDesc = p.desc;
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
                }
                GUI.color = Color.white;
            }

            GUI.EndScrollView();
        }

        // ================= CỘT 2: BẢNG FORM THIẾT KẾ VẬT PHẨM =================
        private void DrawItemFormColumn(Rect r)
        {
            float totalContentH = 480f;
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

            // 4. KHO ICON DẠNG LƯỚI ẢNH THẬT (Visual Icon Picker) - ĐÃ XÓA SỔ CHỮ RĂNG CƯA CỤT LỦN!
            GUI.Label(new Rect(0, curY, viewRect.width, 20), "🖼️ Kho Icon Mẫu (Click chọn trực tiếp hình ảnh):", lbl);
            curY += 22f;

            float iconSize = 36f;
            float iconGap = 6f;
            int iconCols = 7;
            for (int i = 0; i < itemIconOptions.Length; i++)
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
                Texture2D tex = GetCachedTexture(itemIconOptions[i]);
                GUI.DrawTexture(new Rect(iconSlot.x + 2, iconSlot.y + 2, iconSize - 4, iconSize - 4), tex);

                // Viền chọn vàng kim
                FantasyUITheme.DrawOutline(iconSlot, isChosen ? Color.yellow : new Color(0.35f, 0.45f, 0.6f, 0.5f), isChosen ? 2 : 1);

                if (Event.current.type == EventType.MouseDown && iconSlot.Contains(Event.current.mousePosition))
                {
                    formItemIconIndex = i;
                    Event.current.Use();
                }
            }
            int iconRows = Mathf.CeilToInt(itemIconOptions.Length / (float)iconCols);
            curY += iconRows * (iconSize + iconGap) + 8f;

            // 5. Chỉ Số & Mô Tả
            GUI.Label(new Rect(0, curY, 90, 20), "Chỉ Số Thuộc Tính:", lbl);
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
            float totalContentH = 530f;
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

            // 4. Các Chỉ Số Chiến Đấu (Mana | Hồi Chiêu | Sát Thương/Hồi Phục | % Trí Tuệ | Tầm Đánh)
            string statLabel = (formSkillCategoryIndex == 1) ? "⚡ Mana | Hồi (s) | Hồi HP | % INT | Tầm (m):" :
                               (formSkillCategoryIndex == 2) ? "⚡ Mana | Hồi (s) | Hồi MP | % INT | Tầm (m):" :
                               (formSkillCategoryIndex == 3) ? "⚡ Mana | Hồi (s) | Lực Buff | % INT | Tầm (m):" :
                               "⚡ Mana | Hồi (s) | ST Gốc | % INT | Tầm (m):";
            GUI.Label(new Rect(0, curY, viewRect.width, 18), statLabel, lbl);
            curY += 20f;

            float fieldW = (viewRect.width - 16f) / 5f;
            formSkillMana = GUI.TextField(new Rect(0, curY, fieldW, 22), formSkillMana);
            formSkillCd = GUI.TextField(new Rect(fieldW + 4, curY, fieldW, 22), formSkillCd);
            formSkillDmg = GUI.TextField(new Rect((fieldW + 4) * 2, curY, fieldW, 22), formSkillDmg);
            formSkillScaling = GUI.TextField(new Rect((fieldW + 4) * 3, curY, fieldW, 22), formSkillScaling);
            formSkillRange = GUI.TextField(new Rect((fieldW + 4) * 4, curY, fieldW, 22), formSkillRange);
            curY += 28f;

            // 5. KHO ICON HOTBAR (Ảnh Thật + Icon Tự Nạp)
            GUI.Label(new Rect(0, curY, viewRect.width, 18), "🎯 Icon Ô Chiêu Hotbar [1..4]:", lbl);
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

            Texture2D itemTex = GetCachedTexture(itemIconOptions[formItemIconIndex]);
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
            float btnH = 36f;
            GUI.color = new Color(0.2f, 0.95f, 0.4f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "➕ TẠO VÀO TÚI ĐỒ"))
            {
                CreateAndAddItem();
            }
            pY += btnH + 8f;

            GUI.color = new Color(0.2f, 0.85f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "💾 LƯU DATABASE (JSON)"))
            {
                SaveItemToDatabase();
            }
            GUI.color = Color.white;
        }

        private void DrawSkillPreviewColumn(Rect r)
        {
            float pY = r.y + 4f;

            // 1. Thẻ Xem Trước Ô Kỹ Năng Hotbar
            Rect slotBox = new Rect(r.x + (r.width - 56f) / 2f, pY, 56f, 56f);
            GUI.color = new Color(0.04f, 0.06f, 0.12f, 0.98f);
            GUI.DrawTexture(slotBox, Texture2D.whiteTexture);
            GUI.color = Color.white;

            Texture2D hotbarTex = GetCachedTexture(GetAllHotbarIcons()[formSkillHotbarIconIndex]);
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
            float btnH = 36f;
            GUI.color = new Color(0.2f, 0.95f, 0.4f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "➕ TẠO BÍ KÍP VÀO TÚI"))
            {
                CreateAndAddSkill();
            }
            pY += btnH + 8f;

            GUI.color = new Color(0.2f, 0.85f, 1f);
            if (GUI.Button(new Rect(r.x, pY, r.width, btnH), "💾 LƯU BÍ KÍP (JSON)"))
            {
                SaveSkillToDatabase();
            }
            GUI.color = Color.white;
        }

        private void CreateAndAddItem()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null) return;

            ItemType t = (formTypeIndex == 0) ? ItemType.Equipment :
                         (formTypeIndex == 1) ? ItemType.Consumable :
                         (formTypeIndex == 2) ? ItemType.Pet : ItemType.Material;
            EquipSlot s = (EquipSlot)formSlotIndex;
            string tier = tierNames[formTierIndex];
            Color c = tierColors[formTierIndex];
            string icon = itemIconOptions[formItemIconIndex];

            var newItem = new InventoryItem(formItemId, formItemName, t, s, tier, c, icon, formItemStats, formItemDesc, 1);
            inv.AddItem(newItem);
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
            string icon = itemIconOptions[formItemIconIndex];

            ItemDatabase.Instance.RegisterNewItemFromForm(formItemId, formItemName, t, s, tier, c, icon, formItemStats, formItemDesc, 1);
        }

        private void CreateAndAddSkill()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv == null) return;

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

            string hotbarIcon = GetAllHotbarIcons()[formSkillHotbarIconIndex];
            string scrollIcon = elemScrollIcons[formSkillElemIndex];

            var skillItem = SkillDatabase.Instance.RegisterAndCreateSkillItem(
                formSkillId, formSkillName, elemNames[formSkillElemIndex], tierNames[formTierIndex],
                tierColors[formTierIndex], mana, cd, dmg, scaling, formSkillIndicatorIndex + 1,
                hotbarIcon, scrollIcon, formSkillDesc, rangeVal, classNames[formSkillClassIndex], "",
                formSkillCategoryIndex);

            inv.AddItem(skillItem);
            Debug.Log($"<color=green>[GODLESS STUDIO] Đã tạo thành công Cuộn Trục Bí Kíp '{formSkillName}' vào túi đồ!</color>");
        }

        private void SaveSkillToDatabase()
        {
            CreateAndAddSkill();
        }
    }
}
