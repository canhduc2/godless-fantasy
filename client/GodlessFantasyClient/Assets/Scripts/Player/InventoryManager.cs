using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Combat;
using GodlessFantasy.Core;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Bộ Quản Lý Hành Trang Thống Nhất (Unified Inventory Manager):
    /// - Quản lý 24 ô hành trang hiển thị dạng lưới icon ô vuông chuẩn theo bản vẽ của User.
    /// - Mọi vật phẩm (Trang Bị, Sách Kỹ Năng, Bình Dược, Thú Cưng) đều nằm chung trong hành trang.
    /// - Trang Bị ở Tab 1 lấy trực tiếp từ hành trang khi được mặc lên người.
    /// - Sách Kỹ Năng có thể ấn Sử Dụng để lắp vào ô [1], [2], [3], [4] (có kiểm tra điều kiện cấp độ mở khóa ô).
    /// - Cơ chế BẢO VỆ: Nếu vật phẩm đang được trang bị / đang gán ô skill / thú cưng đang gọi -> Không thể xóa, vứt, giao dịch nhầm!
    /// - Cơ chế DOUBLE-CHECK: Khi bấm Vứt, bắt buộc đếm ngược 5 giây mới cho bấm nút xác nhận xóa!
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public const int DEFAULT_CAPACITY = 24; // Ban đầu 24 ô (Lưới 4 cột x 6 hàng)
        public const int MAX_CAPACITY = 96;     // Giới hạn mở khóa tối đa 96 ô (24 hàng x 4 cột)
        public const int INVENTORY_CAPACITY = 24; // Giữ hằng số để tương thích ngược

        [SerializeField] private int currentCapacity = DEFAULT_CAPACITY;

        [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();

        // 7 Vị Trí Trang Bị Đang Mặc (Tab 1)
        private Dictionary<EquipSlot, InventoryItem> equippedGear = new Dictionary<EquipSlot, InventoryItem>();

        // Trạng thái Double Check 5s khi Vứt đồ
        private InventoryItem pendingDropItem = null;
        private float dropCountdown = 0f;
        private bool isDropModalOpen = false;

        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private SkillAimingController skillAimingController;
        [SerializeField] private PetFollower petFollower;

        public List<InventoryItem> Items => items;
        public Dictionary<EquipSlot, InventoryItem> EquippedGear => equippedGear;
        public bool IsDropModalOpen => isDropModalOpen;
        public InventoryItem PendingDropItem => pendingDropItem;
        public float DropCountdown => dropCountdown;

        public int CurrentCapacity => currentCapacity;
        public int MaxCapacity => MAX_CAPACITY;
        public bool IsFull => items.Count >= currentCapacity;

        /// <summary>
        /// Tính giá mở khóa ô tiếp theo bằng Vàng.
        /// Mỗi một ô thì tốn nhiều tiền hơn theo công thức bậc 2 lũy tiến:
        /// - Ô 25: 100 Vàng
        /// - Ô 26: 168 Vàng
        /// - Ô 27: 244 Vàng
        /// - Ô 28: 328 Vàng
        /// - Ô 29: 420 Vàng
        /// - Ô 30: 520 Vàng...
        /// </summary>
        public int GetNextSlotUnlockCost()
        {
            if (currentCapacity >= MAX_CAPACITY) return -1;
            int extraSlots = currentCapacity - DEFAULT_CAPACITY;
            return 100 + (extraSlots * 60) + (extraSlots * extraSlots * 8);
        }

        public bool CanUnlockNextSlot(out string failReason)
        {
            if (currentCapacity >= MAX_CAPACITY)
            {
                failReason = $"Hành trang đã đạt giới hạn tối đa {MAX_CAPACITY} ô!";
                return false;
            }

            int cost = GetNextSlotUnlockCost();
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                failReason = "Không tìm thấy dữ liệu thuộc tính người chơi!";
                return false;
            }

            if (playerStats.Gold < cost)
            {
                failReason = $"Không đủ vàng! Cần {cost:N0} Vàng (Hiện có: {playerStats.Gold:N0} Vàng).";
                return false;
            }

            failReason = null;
            return true;
        }

        /// <summary>
        /// Mở khóa thêm 1 ô hành trang bằng Vàng
        /// </summary>
        public bool UnlockNextSlot()
        {
            if (!CanUnlockNextSlot(out string failReason))
            {
                if (!string.IsNullOrEmpty(failReason))
                {
                    FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f, $"❌ {failReason}", new Color(1f, 0.35f, 0.35f));
                }
                return false;
            }

            int cost = GetNextSlotUnlockCost();
            playerStats.Gold -= cost;
            currentCapacity++;

            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f, $"🔓 MỞ KHÓA Ô {currentCapacity}! (-{cost:N0} Vàng)", new Color(1f, 0.88f, 0.25f));
            Debug.Log($"<color=yellow>[HÀNH TRANG]</color> Đã mở khóa thêm ô thứ {currentCapacity}! Chi phí: {cost:N0} Vàng. Vàng còn lại: {playerStats.Gold:N0}");
            return true;
        }

        public bool CanAddItem(InventoryItem item)
        {
            if (item == null) return false;
            // Nếu là vật phẩm tiêu hao (Consumable) và đã có sẵn trong túi -> Cho phép cộng dồn số lượng
            if (item.itemType == ItemType.Consumable)
            {
                var existing = items.Find(x => x.id == item.id && x.itemType == ItemType.Consumable);
                if (existing != null) return true;
            }
            return items.Count < currentCapacity;
        }

        public bool AddItem(InventoryItem item)
        {
            if (item == null) return false;

            // 1. Kiểm tra cộng dồn đối với Dược phẩm / Tiêu hao
            if (item.itemType == ItemType.Consumable)
            {
                var existing = items.Find(x => x.id == item.id && x.itemType == ItemType.Consumable);
                if (existing != null)
                {
                    existing.quantity += item.quantity;
                    Debug.Log($"<color=green>[HÀNH TRANG]</color> Đã cộng dồn {item.quantity} '{item.itemName}' (Hiện có: {existing.quantity})!");
                    return true;
                }
            }

            // 2. Chặn tuyệt đối nếu đã đạt giới hạn ô hiện tại
            if (items.Count >= currentCapacity)
            {
                Debug.LogWarning($"<color=red>[HÀNH TRANG ĐẦY]</color> Không thể nhận '{item.itemName}' vì hành trang đã đầy ({items.Count}/{currentCapacity})!");
                return false;
            }

            items.Add(item);
            return true;
        }

        private void Awake()
        {
            EnsureReferences();
            InitializeStarterInventory();
        }

        private void Start()
        {
            EnsureReferences();
            SyncEquippedScrollsToCombat();
        }

        public void EnsureReferences()
        {
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (skillAimingController == null) skillAimingController = GetComponent<SkillAimingController>();
            if (petFollower == null) petFollower = FindAnyObjectByType<PetFollower>();
        }

        public void SyncEquippedScrollsToCombat()
        {
            EnsureReferences();
            if (skillAimingController == null) return;
            for (int i = 0; i < 4; i++)
            {
                InventoryItem equippedScroll = null;
                foreach (var it in items)
                {
                    if (it.itemType == ItemType.SkillScroll && it.isEquipped && it.equippedHotbarSlot == i)
                    {
                        equippedScroll = it;
                        break;
                    }
                }
                if (equippedScroll != null)
                {
                    skillAimingController.EquipScroll(i, equippedScroll.scrollData);
                }
                else
                {
                    skillAimingController.UnequipScroll(i);
                }
            }
        }

        private void Update()
        {
            if (isDropModalOpen && dropCountdown > 0f)
            {
                dropCountdown -= Time.deltaTime;
                if (dropCountdown < 0f) dropCountdown = 0f;
            }
        }

        private void InitializeStarterInventory()
        {
            items.Clear();
            equippedGear.Clear();

            // 1. Bộ 7 Món Trang Bị Khởi Đầu
            var wep = new InventoryItem("gear_weapon_1", "Gậy Tập Sự", ItemType.Equipment, EquipSlot.Weapon, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_weapon", "+8 Trí Tuệ (INT)", "Gậy gỗ sồi yểm ma pháp sơ cấp cho người mới bắt đầu.");
            var helm = new InventoryItem("gear_helm_1", "Mũ Vải Tân Thủ", ItemType.Equipment, EquipSlot.Helmet, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_helmet", "+4 Thể Lực (VIT)", "Mũ vải nhẹ nhàng bảo vệ vùng đầu.");
            var armor = new InventoryItem("gear_armor_1", "Áo Vải Thô", ItemType.Equipment, EquipSlot.Armor, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_armor", "+5 Giáp Vật Lý", "Áo dệt từ sợi gai thô mộc, thoáng khí và bền chắc.");
            var pants = new InventoryItem("gear_pants_1", "Quần Vải Thô", ItemType.Equipment, EquipSlot.Pants, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_pants", "+4 Giáp Vật Lý", "Quần chiến đấu cơ động giúp di chuyển linh hoạt.");
            var boots = new InventoryItem("gear_boots_1", "Giày Da Mềm", ItemType.Equipment, EquipSlot.Boots, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_boots", "+0.4 Tốc Chạy", "Đôi giày làm từ da hươu mềm mại, êm chân.");
            var neck = new InventoryItem("gear_neck_1", "Dây Chuyền Bạc", ItemType.Equipment, EquipSlot.Necklace, "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "item_necklace", "+3 Trí Tuệ (INT)", "Dây chuyền gắn đá hộ mệnh tích tụ chút năng lượng tâm linh.");
            var ring = new InventoryItem("gear_ring_1", "Nhẫn Đồng Cổ", ItemType.Equipment, EquipSlot.Ring, "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "item_ring", "+2 Trí Tuệ (INT)", "Chiếc nhẫn đồng chạm khắc cổ xưa.");

            items.Add(wep);
            items.Add(helm);
            items.Add(armor);
            items.Add(pants);
            items.Add(boots);
            items.Add(neck);
            items.Add(ring);

            // Mặc sẵn 7 món này lên người nhân vật
            EquipGearItem(wep, true);
            EquipGearItem(helm, true);
            EquipGearItem(armor, true);
            EquipGearItem(pants, true);
            EquipGearItem(boots, true);
            EquipGearItem(neck, true);
            EquipGearItem(ring, true);

            // 2. Sách Kỹ Năng (Cuộn Trục Bí Kíp) - Tên chuyên nghiệp, ngắn gọn & chuẩn hệ
            var s1 = new InventoryItem("scroll_s1", "Hỏa Kiếm Khí", ItemType.SkillScroll, EquipSlot.None, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_scroll_fire", "• [Kị Sĩ · Hỏa] | ST: 25 + 100% INT | CD: 1.0s | Tầm: 8.0m", "Phóng kiếm khí hỏa diễm theo đường thẳng (Line Skillshot).", 1,
                new SkillScrollData("scroll_fire_slash", "Hỏa Kiếm Khí", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "Hỏa", 15, 1.0f, 25, 1.0f, 1, "icon_skill_flame_cone", "item_scroll_fire", "Phóng kiếm khí hỏa diễm theo đường thẳng.", 8.0f, "Kị Sĩ"));

            var s2 = new InventoryItem("scroll_s2", "Liệt Hỏa Oanh Kích", ItemType.SkillScroll, EquipSlot.None, "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "item_scroll_fire", "• [Kị Sĩ · Hỏa] | ST: 40 + 120% INT | CD: 3.0s | Tầm: 7.0m", "Giáng luồng lửa thiêu rụi vùng đất (Ground AoE).", 1,
                new SkillScrollData("scroll_fire_ground", "Liệt Hỏa Oanh Kích", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "Hỏa", 25, 3.0f, 40, 1.2f, 2, "icon_skill_meteor", "item_scroll_fire", "Giáng luồng lửa thiêu rụi vùng đất.", 7.0f, "Kị Sĩ"));

            var s3 = new InventoryItem("scroll_s3", "Hỏa Long Toàn Phong", ItemType.SkillScroll, EquipSlot.None, "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "item_scroll_fire", "• [Kị Sĩ · Hỏa] | ST: 30 + 80% INT | CD: 2.5s | Tầm: 4.5m", "Quét bão lửa hình quạt 60 độ phía trước (Cone Sweep).", 1,
                new SkillScrollData("scroll_fire_sweep", "Hỏa Long Toàn Phong", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Hỏa", 20, 2.5f, 30, 0.8f, 3, "icon_skill_flame_cone", "item_scroll_fire", "Quét bão lửa hình quạt 60 độ phía trước.", 4.5f, "Kị Sĩ"));

            var s4 = new InventoryItem("scroll_s4", "Hỏa Long Kim Cang", ItemType.SkillScroll, EquipSlot.None, "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "item_scroll_fire", "• [Kị Sĩ · Hỏa] | Buff: +25% Công/Giáp (15s) | CD: 5.0s", "Kích hoạt hào quang rồng lửa hộ thể (Self Buff).", 1,
                new SkillScrollData("scroll_fire_buff", "Hỏa Long Kim Cang", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Hỏa", 30, 5.0f, 50, 1.5f, 5, "icon_skill_divine_nova", "item_scroll_fire", "Kích hoạt hào quang rồng lửa hộ thể.", 0.0f, "Kị Sĩ", "", 3));

            // Các cuộn trục mẫu của Hệ khác / Thuộc Tính khác:
            var s5 = new InventoryItem("scroll_s5", "Mưa Thiên Thạch", ItemType.SkillScroll, EquipSlot.None, "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "item_scroll_fire", "• [Pháp Sư · Hỏa] | ST: 55 + 140% INT | CD: 4.0s | Tầm: 7.0m", "Thả thiên thạch ma đạo (Kị Sĩ không thể dùng vì sai hệ).", 1,
                new SkillScrollData("scroll_mage_meteor", "Mưa Thiên Thạch", "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "Hỏa", 32, 4.0f, 55, 1.4f, 2, "icon_skill_meteor", "item_scroll_fire", "Bí kíp độc quyền của Pháp Sư.", 7.0f, "Pháp Sư"));

            var s6 = new InventoryItem("scroll_s6", "Băng Hàn Kiếm", ItemType.SkillScroll, EquipSlot.None, "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "item_scroll_ice", "• [Kị Sĩ · Băng] | ST: 48 + 130% INT | CD: 2.0s | Tầm: 8.0m", "Kiếm khí băng tuyết (Người chơi Hỏa không thể dùng vì khác thuộc tính).", 1,
                new SkillScrollData("scroll_knight_frost", "Băng Hàn Kiếm", "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "Băng", 22, 2.0f, 48, 1.3f, 1, "item_scroll_ice", "item_scroll_ice", "Bí kíp thuộc tính Băng.", 8.0f, "Kị Sĩ"));

            var s7 = new InventoryItem("scroll_s7", "Mưa Sinh Mệnh", ItemType.SkillScroll, EquipSlot.None, "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "item_scroll_wood", "• [Pháp Sư · Mộc] | Hồi HP: 35 | CD: 6.0s | Tầm: 7.0m", "Mưa xanh hồi phục sinh lực (Chỉ Pháp Sư mới dùng được).", 1,
                new SkillScrollData("scroll_mage_life_rain", "Mưa Sinh Mệnh", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Mộc", 28, 6.0f, 35, 1.0f, 2, "icon_skill_arcane_bolt", "item_scroll_wood", "Mưa sinh mệnh phục hồi sinh lực.", 7.0f, "Pháp Sư", "", 1));

            var s8 = new InventoryItem("scroll_s8", "Lôi Thần Nộ", ItemType.SkillScroll, EquipSlot.None, "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "item_scroll_thunder", "• [Toàn Hệ · Lôi] | ST: 45 + 110% INT | CD: 3.5s | Tầm: 7.0m", "Sấm sét giáng từ trời cao (Bí kíp Toàn Hệ).", 1,
                new SkillScrollData("scroll_all_thunder", "Lôi Thần Nộ", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Lôi", 25, 3.5f, 45, 1.1f, 2, "icon_skill_meteor", "item_scroll_thunder", "Sấm sét giáng diện rộng.", 7.0f, "Toàn Hệ"));

            var s9 = new InventoryItem("scroll_s9", "Hào Quang Bang Hội", ItemType.SkillScroll, EquipSlot.None, "Phẩm S", new Color(1.0f, 0.88f, 0.25f), "item_scroll_fire", "• [Kị Sĩ · Hỏa] | Buff Đồng Đội 360° (+20% ST) | CD: 6.0s | Tầm: 6.5m", "Kích hoạt hào quang vàng hoàng gia buff toàn đội trong bán kính 6.5m!", 1,
                new SkillScrollData("scroll_guild_buff", "Hào Quang Bang Hội", "Phẩm S", new Color(1.0f, 0.88f, 0.25f), "Hỏa", 30, 6.0f, 40, 1.2f, 6, "icon_skill_divine_nova", "item_scroll_fire", "Kích hoạt hào quang vàng hoàng kim 360 độ ban phước +20% sát thương cho bản thân và toàn bộ đồng đội cùng bang!", 6.5f, "Kị Sĩ", "", 3));

            items.Add(s1);
            items.Add(s2);
            items.Add(s3);
            items.Add(s4);
            items.Add(s5);
            items.Add(s6);
            items.Add(s7);
            items.Add(s8);
            items.Add(s9);

            // Gắn sẵn vào 4 ô skill hotbar (đều thuộc Hệ Kị Sĩ, Thuộc Tính Hỏa)
            EquipSkillScrollToHotbar(s1, 0, true);
            EquipSkillScrollToHotbar(s2, 1, true);
            EquipSkillScrollToHotbar(s3, 2, true);
            EquipSkillScrollToHotbar(s4, 3, true);

            // 3. Vật Phẩm Hồi Phục: Thức Ăn (Phím Q) & Nước Uống (Phím E)
            items.Add(new InventoryItem("item_hp_pot", "Bình Máu HP (+35)", ItemType.Consumable, EquipSlot.None, "Phẩm E", Color.white, "item_potion_hp", "❤️ Thuốc: Hồi +35 Máu tức thì (Phím Q)", "Bình dược liệu màu đỏ hồi phục sinh lực nhanh chóng.", 5));
            items.Add(new InventoryItem("item_mp_pot", "Bình Mana (+30)", ItemType.Consumable, EquipSlot.None, "Phẩm E", Color.white, "item_potion_mana", "💧 Nước: Hồi +30 Mana tức thì (Phím E)", "Bình nước thần màu xanh lam bổ sung năng lượng pháp thuật.", 5));
            items.Add(new InventoryItem("item_roast_meat", "Thịt Nướng Lửa Hồng", ItemType.Consumable, EquipSlot.None, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_potion_hp", "🍗 Thức Ăn: Hồi +30 HP & +20 MP (Phím Q)", "Miếng thịt nướng thơm lừng nóng hổi, phục hồi cả sinh lực và ma lực.", 5));
            items.Add(new InventoryItem("item_water", "Nước Suối Thanh Khiết", ItemType.Consumable, EquipSlot.None, "Phẩm D", new Color(0.35f, 0.65f, 1.0f), "item_potion_mana", "💧 Nước Uống: Hồi +35 Mana (Phím E)", "Nước nguồn tinh khiết trong lành giúp thanh lọc tâm trí và hồi phục năng lượng.", 5));
            items.Add(new InventoryItem("item_pet_food", "Hạt Dẻ Linh Hồn", ItemType.Consumable, EquipSlot.None, "Phẩm C", new Color(1.0f, 0.85f, 0.35f), "item_potion_hp", "🌰 Thức Ăn Pet: Tăng +60 EXP Thú Cưng", "Hạt dẻ chứa linh khí đất trời, món ăn ưa thích giúp Pet tăng cấp và tiến hóa to hơn.", 10));

            // 4. Thú Cưng Sóc Chibi
            var pet = new InventoryItem("pet_squirrel_egg", "Sóc Chibi SquiRRels", ItemType.Pet, EquipSlot.None, "Phẩm A", new Color(1f, 0.85f, 0.35f), "pet_squirrel", "Thú Cưng Đồng Hành", "Thú cưng linh vật đáng yêu, có thể tiến hóa to hơn và mạnh hơn khi được cho ăn hạt dẻ.");
            pet.isEquipped = true; // Đang xuất chiến
            items.Add(pet);

            // 5. Ngoại Trang (Skin Trang Phục)
            var skin = new InventoryItem("gear_costume_champion", "Ngoại Trang Thiết Giáp Chiến Thần", ItemType.Costume, EquipSlot.Costume, "Phẩm S", new Color(1f, 0.85f, 0.25f), "item_costume", "+15 Giáp • Che Lấp Toàn Thân (Skin)", "Ngoại trang hoàng gia tuyệt mỹ, biến đổi hoàn toàn diện mạo thành Chiến Binh Hoàng Kim!");
            items.Add(skin);

            // 6. KHO TRANG BỊ MẪU KIỂM THỬ KHÓA HỆ & THUỘC TÍNH (TEST GEAR FOR ALL CLASSES & ELEMENTS):
            // - Món 1: Chiến Giáp Hỏa Long (Kị Sĩ · Hỏa) -> Người chơi Kị Sĩ Hỏa MẶC ĐƯỢC!
            items.Add(new InventoryItem("gear_knight_flame_armor", "Chiến Giáp Hỏa Long", ItemType.Equipment, EquipSlot.Armor, "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "item_armor", "+28 Giáp · +45 Thể Lực (VIT)", "Chiến giáp đúc từ vảy Hỏa Long cổ đại, uy lực ngút trời.", 1, null, "Kị Sĩ", "Hỏa"));

            // - Món 2: Pháp Bào Băng Sương (Pháp Sư · Băng) -> Người chơi Kị Sĩ Hỏa BỊ CHẶN: Sai Hệ & Khác Thuộc Tính!
            items.Add(new InventoryItem("gear_mage_frost_robe", "Pháp Bào Băng Sương", ItemType.Equipment, EquipSlot.Armor, "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "item_armor", "+12 Giáp · +40 Trí Tuệ (INT)", "Áo dệt từ tơ tuyết vĩnh cửu, tỏa hàn khí buốt giá.", 1, null, "Pháp Sư", "Băng"));

            // - Món 3: Hàn Băng Thánh Khiên (Kị Sĩ · Băng) -> Cùng hệ Kị Sĩ nhưng Khác Thuộc Tính Băng -> BỊ CHẶN!
            items.Add(new InventoryItem("gear_knight_ice_shield", "Hàn Băng Thánh Khiên", ItemType.Equipment, EquipSlot.Helmet, "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "item_helmet", "+20 Giáp · +25 Thể Lực (VIT)", "Khiên băng nghìn năm hộ thể, chỉ người mang nguyên tố Băng mới phát huy được.", 1, null, "Kị Sĩ", "Băng"));

            // - Món 4: Áo Da Phong Linh (Xạ Thủ · Phong) -> Sai Hệ!
            items.Add(new InventoryItem("gear_archer_wind_vest", "Áo Da Phong Linh", ItemType.Equipment, EquipSlot.Armor, "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "item_armor", "+16 Giáp · +38 Nhanh Nhẹn (AGI)", "Áo da nhẹ bẫng tựa gió thoảng, hỗ trợ thần tiễn thủ xuất chiêu chớp nhoáng.", 1, null, "Xạ Thủ", "Phong"));

            // - Món 5: Võ Khố Lôi Đình (Võ Sĩ · Lôi) -> Sai Hệ!
            items.Add(new InventoryItem("gear_brawler_thunder_pants", "Võ Khố Lôi Đình", ItemType.Equipment, EquipSlot.Pants, "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "item_pants", "+18 Giáp · +30 Sức Mạnh (STR)", "Quần võ gia tăng điện tích cuồng nộ trong từng đòn thế.", 1, null, "Võ Sĩ", "Lôi"));

            // - Món 6: Thánh Quang Hộ Phù (Toàn Hệ · Quang) -> Toàn Hệ nhưng khác nguyên tố (Quang vs Hỏa) -> BỊ CHẶN!
            items.Add(new InventoryItem("gear_all_holy_amulet", "Thánh Quang Hộ Phù", ItemType.Equipment, EquipSlot.Necklace, "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "item_necklace", "+15 Giáp · +30 INT · +30 VIT", "Bùa hộ mệnh thần thánh ban phước, chỉ phát huy với dũng sĩ mang Thuộc Tính Ánh Sáng.", 1, null, "Toàn Hệ", "Ánh Sáng"));

            // - Món 7: Nhẫn Liệt Hỏa (Toàn Hệ · Hỏa) -> Toàn Hệ và cùng nguyên tố Hỏa -> Người chơi MẶC ĐƯỢC!
            items.Add(new InventoryItem("gear_all_flame_ring", "Nhẫn Liệt Hỏa", ItemType.Equipment, EquipSlot.Ring, "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "item_ring", "+25 Công · +15 INT · +15 STR", "Nhẫn rực lửa tăng mạnh lực chiến, tương thích cho bất kỳ ai mang nguyên tố Hỏa.", 1, null, "Toàn Hệ", "Hỏa"));
        }

        // ================= XỬ LÝ TRANG BỊ TAB 1 =================
        public bool EquipGearItem(InventoryItem item, bool bypassLockCheck = false)
        {
            if (item == null || (item.itemType != ItemType.Equipment && item.itemType != ItemType.Costume) || item.equipSlot == EquipSlot.None) return false;

            EnsureReferences();

            // KIỂM TRA ĐIỀU KIỆN KÉP NGHIÊM NGẶT: HỆ & THUỘC TÍNH NGUYÊN TỐ (ĐỒNG BỘ 100% VỚI BÍ KÍP)
            if (!bypassLockCheck)
            {
                string playerClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
                string playerElem = playerStats != null ? playerStats.Element : "Hỏa";

                if (!item.CanPlayerEquip(playerClass, playerElem, out string reason))
                {
                    Debug.LogWarning($"[KHÓA TRANG BỊ] {reason}");
                    Vector3 spawnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                    FloatingDamageManager.ShowFloatingText(spawnPos, $"⛔ {reason}", new Color(1f, 0.35f, 0.35f));
                    AudioManager.PlayHit();
                    return false;
                }
            }

            // Nếu ô đó đã có món khác -> Tháo món cũ ra trước
            if (equippedGear.TryGetValue(item.equipSlot, out var oldItem) && oldItem != null)
            {
                oldItem.isEquipped = false;
            }

            equippedGear[item.equipSlot] = item;
            item.isEquipped = true;

            Vector3 okPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.4f : Vector3.zero;
            FloatingDamageManager.ShowFloatingText(okPos, $"🛡️ Mặc {item.itemName}", new Color(0.35f, 0.95f, 0.55f));
            AudioManager.PlayPickup();
            Debug.Log($"<color=green>[TRANG BỊ] Đã mặc '{item.itemName}' vào ô {item.equipSlot}!</color>");
            return true;
        }

        public void UnequipGearItem(EquipSlot slot)
        {
            if (equippedGear.TryGetValue(slot, out var item) && item != null)
            {
                item.isEquipped = false;
                equippedGear.Remove(slot);
                Debug.Log($"<color=yellow>[THÁO TRANG BỊ] Đã tháo '{item.itemName}' khỏi ô {slot}!</color>");
            }
        }

        public InventoryItem GetEquippedGear(EquipSlot slot)
        {
            equippedGear.TryGetValue(slot, out var item);
            return item;
        }

        // ================= XỬ LÝ SÁCH KỸ NĂNG & ĐIỀU KIỆN MỞ KHÓA Ô =================
        public int GetRequiredLevelForSlot(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0: return 1;   // Ô 1: Lv 1
                case 1: return 25;  // Ô 2: Lv 25
                case 2: return 50;  // Ô 3: Lv 50
                case 3: return 75;  // Ô 4: Lv 75
                default: return 1;
            }
        }

        public bool IsSlotUnlocked(int slotIndex)
        {
            int pLv = playerStats != null ? playerStats.Level : 1;
            // Cho phép ô 1 luôn mở, và các ô khác kiểm tra level
            // Lưu ý: Đối với bản demo, nếu người chơi mở test ta có thể linh hoạt, nhưng tuân thủ đúng yêu cầu:
            return pLv >= GetRequiredLevelForSlot(slotIndex);
        }

        public bool EquipSkillScrollToHotbar(InventoryItem scrollItem, int slotIndex, bool bypassLevelCheck = false)
        {
            if (scrollItem == null || scrollItem.itemType != ItemType.SkillScroll || scrollItem.scrollData == null) return false;
            if (slotIndex < 0 || slotIndex >= 4) return false;

            EnsureReferences();

            if (!bypassLevelCheck && !IsSlotUnlocked(slotIndex))
            {
                int reqLv = GetRequiredLevelForSlot(slotIndex);
                Debug.LogWarning($"[KHÓA KỸ NĂNG] Ô [{slotIndex + 1}] yêu cầu Cấp {reqLv} mới mở khóa!");
                return false;
            }

            // KIỂM TRA ĐIỀU KIỆN KÉP NGHIÊM NGẶT: HỆ -> HỆ, THUỘC TÍNH -> THUỘC TÍNH
            if (!bypassLevelCheck && scrollItem.scrollData != null)
            {
                string playerClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
                string playerElem = playerStats != null ? playerStats.Element : "Hỏa";
                if (!scrollItem.scrollData.CanPlayerUse(playerClass, playerElem, out string reason))
                {
                    Debug.LogWarning($"[KHÓA BÍ KÍP] {reason}");
                    Vector3 spawnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                    FloatingDamageManager.Instance?.SpawnFloatingText(spawnPos, $"⛔ {reason}", new Color(1f, 0.35f, 0.35f));
                    return false;
                }
            }

            // Tháo cuộn cũ đang ở ô này ra
            foreach (var it in items)
            {
                if (it.itemType == ItemType.SkillScroll && it.isEquipped && it.equippedHotbarSlot == slotIndex)
                {
                    it.isEquipped = false;
                    it.equippedHotbarSlot = -1;
                }
            }

            scrollItem.isEquipped = true;
            scrollItem.equippedHotbarSlot = slotIndex;

            if (skillAimingController != null)
            {
                skillAimingController.EquipScroll(slotIndex, scrollItem.scrollData);
            }

            Debug.Log($"<color=cyan>[BÍ KÍP] Đã lắp '{scrollItem.itemName}' vào Ô [{slotIndex + 1}]!</color>");
            return true;
        }

        public void UnequipSkillScrollFromHotbar(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 4) return;
            EnsureReferences();

            foreach (var it in items)
            {
                if (it.itemType == ItemType.SkillScroll && it.isEquipped && it.equippedHotbarSlot == slotIndex)
                {
                    it.isEquipped = false;
                    it.equippedHotbarSlot = -1;
                    break;
                }
            }

            if (skillAimingController != null)
            {
                skillAimingController.UnequipScroll(slotIndex);
            }
            Debug.Log($"<color=yellow>[THÁO BÍ KÍP] Đã tháo cuộn trục khỏi Ô [{slotIndex + 1}]!</color>");
        }

        // ================= XỬ LÝ VỨT ĐỒ (DOUBLE CHECK 5S) =================
        public bool CanDropItem(InventoryItem item)
        {
            if (item == null) return false;
            // BẢO VỆ: Đang dùng không được vứt
            if (item.isEquipped) return false;
            return true;
        }

        public static bool IsTierBOrHigher(string tierRank)
        {
            if (string.IsNullOrEmpty(tierRank)) return false;
            string t = tierRank.Trim().ToUpper();
            // Bậc B trở lên bao gồm: B, A, S, X (hoặc Phẩm B, Phẩm A, Phẩm S, Phẩm X)
            // Bậc thấp: C, D, E
            return t.Contains("B") || t.Contains("A") || t.Contains("S") || t.Contains("X");
        }

        public void RequestDropItem(InventoryItem item)
        {
            if (!CanDropItem(item))
            {
                Debug.LogWarning($"[BẢO VỆ AN TOÀN] Vật phẩm '{item.itemName}' đang được trang bị/sử dụng! Không thể vứt!");
                return;
            }

            pendingDropItem = item;
            // Khóa 30 giây bảo hộ an toàn đối với vật phẩm từ bậc B trở lên, bậc thấp (C, D, E) thì không cần chờ
            dropCountdown = IsTierBOrHigher(item.tierRank) ? 30f : 0f;
            isDropModalOpen = true;
        }

        public void ConfirmDropItem()
        {
            if (!isDropModalOpen || pendingDropItem == null || dropCountdown > 0f) return;

            items.Remove(pendingDropItem);
            Debug.Log($"<color=red>[ĐÃ XÓA] Đã vứt bỏ vĩnh viễn '{pendingDropItem.itemName}' khỏi hành trang!</color>");
            pendingDropItem = null;
            isDropModalOpen = false;
        }

        public void CancelDropItem()
        {
            pendingDropItem = null;
            isDropModalOpen = false;
            dropCountdown = 0f;
        }

        // ================= TIÊU DÙNG VẬT PHẨM (BÌNH DƯỢC, VẬT PHẨM TIÊU HAO) =================
        public bool ConsumeItem(string itemId, int count = 1)
        {
            var item = items.Find(it => it.id == itemId);
            if (item != null && item.quantity >= count)
            {
                item.quantity -= count;
                Debug.Log($"<color=orange>[TIÊU HAO] Đã dùng {count}x '{item.itemName}'. Còn lại trong túi: {item.quantity}</color>");
                if (item.quantity <= 0)
                {
                    items.Remove(item);
                    Debug.Log($"<color=red>[HẾT VẬT PHẨM] '{item.itemName}' đã hết và được dọn khỏi ô hành trang!</color>");
                }
                return true;
            }
            return false;
        }

        public bool RemoveItem(string itemId, int count = 1) => ConsumeItem(itemId, count);

        public int GetItemCount(string itemId)
        {
            var item = items.Find(it => it.id == itemId);
            return item != null ? item.quantity : 0;
        }

        public void TogglePetSummon(InventoryItem petItem)
        {
            if (petItem == null || petItem.itemType != ItemType.Pet) return;
            petItem.isEquipped = !petItem.isEquipped;

            if (petFollower == null) petFollower = FindAnyObjectByType<PetFollower>();
            if (petFollower != null)
            {
                petFollower.gameObject.SetActive(petItem.isEquipped);
            }
            Debug.Log(petItem.isEquipped ? "[PET] Đã gọi Thú Cưng xuất chiến!" : "[PET] Đã thu hồi Thú Cưng vào lồng!");
        }

        // ================= RỦI RO RƠI ĐỒ 50% KHI CHẾT NGOÀI SAFE ZONE =================
        /// <summary>
        /// Khi tử trận: 5 ô đầu tiên được bảo vệ vĩnh viễn (Permanent Protected Slots).
        /// Các món từ ô thứ 6 trở đi có 50% tỉ lệ rơi tự do ra sàn đất theo đúng luật Hardcore MMORPG.
        /// </summary>
        public List<string> DropUnprotectedLootOnDeath()
        {
            List<string> droppedNames = new List<string>();
            List<InventoryItem> toRemove = new List<InventoryItem>();

            for (int i = 5; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;

                if (equippedGear != null && equippedGear.ContainsValue(it)) continue;

                if (UnityEngine.Random.value < 0.50f)
                {
                    toRemove.Add(it);
                    droppedNames.Add(it.itemName);

                    Vector3 dropPos = transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.85f);
                    GroundLootItem.SpawnLoot(dropPos, it.itemName, it.tierRank, it.tierColor, it.iconName, it.quantity);
                }
            }

            foreach (var r in toRemove)
            {
                items.Remove(r);
            }

            return droppedNames;
        }
    }
}
