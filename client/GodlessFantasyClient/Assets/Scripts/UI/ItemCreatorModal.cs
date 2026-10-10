using System;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Studio Thiết Kế Vật Phẩm & Kỹ Năng Trực Tiếp Trong Engine (Game Asset Studio - No Code):
    /// - Không cần code C# khi tạo Trang Bị, Dược Phẩm hoặc Kỹ Năng mới!
    /// - Hỗ trợ phân biệt độc lập icon cuộn trục trong túi đồ và icon kỹ năng trên ô phím nóng.
    /// - Phím tắt F1 để Bật / Tắt Studio.
    /// </summary>
    public class ItemCreatorModal : MonoBehaviour
    {
        public static ItemCreatorModal Instance { get; private set; }

        public bool IsOpen = false;
        private int studioMode = 0; // 0 = Tạo Vật Phẩm, 1 = Tạo Kỹ Năng
        private readonly string[] modeNames = { "⚔️ TẠO VẬT PHẨM & TRANG BỊ", "📜 TẠO KỸ NĂNG & BÍ KÍP" };

        // ================= FORM 1: VẬT PHẨM =================
        private string formItemName = "Bảo Kiếm Hoàng Kim";
        private string formItemId = "custom_wep_1";
        private int formTypeIndex = 0; // 0=Equipment, 1=SkillScroll, 2=Consumable, 3=Pet, 4=Material
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
        private readonly string[] itemIconOptions = { "item_weapon", "item_helmet", "item_armor", "item_pants", "item_boots", "item_ring", "item_necklace", "item_potion_hp", "item_potion_mp" };

        private string formItemStats = "+25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)";
        private string formItemDesc = "Vật phẩm thần thoại được rèn từ form thiết kế engine.";

        // ================= FORM 2: KỸ NĂNG =================
        private string formSkillName = "Hỏa Long Bộc Phá";
        private string formSkillId = "skill_fire_dragon";
        private int formSkillElemIndex = 0;
        private readonly string[] elemNames = { "Hỏa", "Băng", "Lôi", "Thánh", "Ma Pháp" };

        private int formSkillIndicatorIndex = 0;
        private readonly string[] indicatorNames = { "1. Mũi Tên Thẳng (Line)", "2. Vùng Đất Chỉ Định (AoE)", "3. Hình Nón (Cone Arc)", "4. Nổ Tỏa Quanh Người (Nova)" };

        private string formSkillMana = "25";
        private string formSkillCd = "3.5";
        private string formSkillDmg = "45";
        private string formSkillScaling = "80";

        private int formSkillHotbarIconIndex = 0;
        private readonly string[] skillHotbarIcons = { "skill_fireball", "skill_meteor", "skill_frost", "skill_lightning", "skill_holy", "dash_icon" };

        private int formSkillScrollIconIndex = 0;
        private readonly string[] skillScrollIcons = { "scroll_firebolt", "item_weapon", "item_necklace" };

        private string formSkillDesc = "Triệu hồi sức mạnh rồng lửa tạo đòn tấn công cực mạnh!";

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

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.depth = FantasyUITheme.DepthModal;

            float w = 500f;
            float h = 540f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            Rect modalRect = new Rect(x, y, w, h);

            GUI.color = new Color(0.04f, 0.05f, 0.09f, 0.98f);
            GUI.DrawTexture(modalRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawDoubleGoldFrame(modalRect);

            // Tiêu đề
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };
            GUI.Label(new Rect(x + 12, y + 8, w - 24, 22), "🛠️ ENGINE THIẾT KẾ ASSET TRÒ CHƠI (NO-CODE)", titleStyle);

            if (GUI.Button(new Rect(x + w - 32, y + 8, 24, 22), "✕"))
            {
                IsOpen = false;
            }

            // Mode Tabs (Tạo Vật Phẩm vs Tạo Kỹ Năng)
            studioMode = GUI.Toolbar(new Rect(x + 16, y + 36, w - 32, 26), studioMode, modeNames);

            if (studioMode == 0)
            {
                DrawItemForm(x, y + 70, w, h - 70);
            }
            else
            {
                DrawSkillForm(x, y + 70, w, h - 70);
            }
        }

        private void DrawItemForm(float x, float y, float w, float h)
        {
            float curY = y;
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Tên Vật Phẩm:", lbl);
            formItemName = GUI.TextField(new Rect(x + 130, curY, w - 146, 22), formItemName);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Mã Định Danh (ID):", lbl);
            formItemId = GUI.TextField(new Rect(x + 130, curY, w - 146, 22), formItemId);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Loại Vật Phẩm:", lbl);
            formTypeIndex = GUI.Toolbar(new Rect(x + 130, curY, w - 146, 22), formTypeIndex, typeNames);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Ô Trang Bị:", lbl);
            formSlotIndex = GUI.SelectionGrid(new Rect(x + 130, curY, w - 146, 40), formSlotIndex, slotNames, 4);
            curY += 46f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Phẩm Cấp:", lbl);
            formTierIndex = GUI.Toolbar(new Rect(x + 130, curY, w - 146, 22), formTierIndex, tierNames);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Icon Mẫu:", lbl);
            formItemIconIndex = GUI.SelectionGrid(new Rect(x + 130, curY, w - 146, 40), formItemIconIndex, itemIconOptions, 5);
            curY += 46f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Chỉ Số Thuộc Tính:", lbl);
            formItemStats = GUI.TextField(new Rect(x + 130, curY, w - 146, 22), formItemStats);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Mô Tả Vật Phẩm:", lbl);
            formItemDesc = GUI.TextField(new Rect(x + 130, curY, w - 146, 36), formItemDesc);
            curY += 46f;

            // Nút Tạo & Lưu
            float btnW = (w - 36f) / 2f;
            GUI.color = new Color(0.2f, 0.9f, 0.4f);
            if (GUI.Button(new Rect(x + 14, curY, btnW, 36), "➕ TẠO & ĐƯA VÀO TÚI ĐỒ"))
            {
                CreateAndAddItem();
            }

            GUI.color = new Color(0.2f, 0.8f, 1f);
            if (GUI.Button(new Rect(x + 22 + btnW, curY, btnW, 36), "💾 LƯU DATABASE (JSON)"))
            {
                SaveItemToDatabase();
            }
            GUI.color = Color.white;
        }

        private void DrawSkillForm(float x, float y, float w, float h)
        {
            float curY = y;
            var lbl = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Tên Bí Kíp / Chiêu:", lbl);
            formSkillName = GUI.TextField(new Rect(x + 130, curY, w - 146, 22), formSkillName);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Mã Kỹ Năng (ID):", lbl);
            formSkillId = GUI.TextField(new Rect(x + 130, curY, w - 146, 22), formSkillId);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Nguyên Tố & Phẩm:", lbl);
            formSkillElemIndex = GUI.Toolbar(new Rect(x + 130, curY, (w - 150) / 2f, 22), formSkillElemIndex, elemNames);
            formTierIndex = GUI.Toolbar(new Rect(x + 136 + (w - 150) / 2f, curY, (w - 150) / 2f, 22), formTierIndex, tierNames);
            curY += 26f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Dạng Thi Triển:", lbl);
            formSkillIndicatorIndex = GUI.SelectionGrid(new Rect(x + 130, curY, w - 146, 40), formSkillIndicatorIndex, indicatorNames, 2);
            curY += 44f;

            // Mana, Hồi Chiêu, Sát Thương, Cộng Hưởng
            GUI.Label(new Rect(x + 16, curY, 110, 20), "Mana | Hồi | ST | %INT:", lbl);
            float qW = (w - 158f) / 4f;
            formSkillMana = GUI.TextField(new Rect(x + 130, curY, qW, 22), formSkillMana);
            formSkillCd = GUI.TextField(new Rect(x + 136 + qW, curY, qW, 22), formSkillCd);
            formSkillDmg = GUI.TextField(new Rect(x + 142 + qW * 2, curY, qW, 22), formSkillDmg);
            formSkillScaling = GUI.TextField(new Rect(x + 148 + qW * 3, curY, qW, 22), formSkillScaling);
            curY += 26f;

            // Phân biệt rõ Icon Hotbar vs Icon Cuộn Trục Trong Túi (Theo đúng lời khen của User)
            GUI.Label(new Rect(x + 16, curY, 110, 20), "Icon Phím Nóng [1-4]:", lbl);
            formSkillHotbarIconIndex = GUI.SelectionGrid(new Rect(x + 130, curY, w - 146, 36), formSkillHotbarIconIndex, skillHotbarIcons, 3);
            curY += 40f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Icon Cuộn Trục (Túi):", lbl);
            formSkillScrollIconIndex = GUI.SelectionGrid(new Rect(x + 130, curY, w - 146, 26), formSkillScrollIconIndex, skillScrollIcons, 3);
            curY += 32f;

            GUI.Label(new Rect(x + 16, curY, 110, 20), "Mô Tả Kỹ Năng:", lbl);
            formSkillDesc = GUI.TextField(new Rect(x + 130, curY, w - 146, 34), formSkillDesc);
            curY += 42f;

            // Nút Tạo & Lưu
            float btnW = (w - 36f) / 2f;
            GUI.color = new Color(0.2f, 0.9f, 0.4f);
            if (GUI.Button(new Rect(x + 14, curY, btnW, 36), "➕ TẠO BÍ KÍP & ĐƯA VÀO TÚI"))
            {
                CreateAndAddSkill();
            }

            GUI.color = new Color(0.2f, 0.8f, 1f);
            if (GUI.Button(new Rect(x + 22 + btnW, curY, btnW, 36), "💾 LƯU BÍ KÍP (JSON)"))
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
            Debug.Log($"<color=green>[ASSET STUDIO] Đã tạo thành công vật phẩm '{formItemName}' vào túi đồ!</color>");
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

            string hotbarIcon = skillHotbarIcons[formSkillHotbarIconIndex];
            string scrollIcon = skillScrollIcons[formSkillScrollIconIndex];

            var skillItem = SkillDatabase.Instance.RegisterAndCreateSkillItem(
                formSkillId, formSkillName, elemNames[formSkillElemIndex], tierNames[formTierIndex],
                tierColors[formTierIndex], mana, cd, dmg, scaling, formSkillIndicatorIndex + 1,
                hotbarIcon, scrollIcon, formSkillDesc);

            inv.AddItem(skillItem);
            Debug.Log($"<color=green>[ASSET STUDIO] Đã tạo thành công Cuộn Trục Bí Kíp '{formSkillName}' vào túi đồ!</color>");
        }

        private void SaveSkillToDatabase()
        {
            CreateAndAddSkill();
        }
    }
}
