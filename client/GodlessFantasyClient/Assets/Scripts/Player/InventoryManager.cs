using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Combat;

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
        public const int INVENTORY_CAPACITY = 24; // Lưới 4 cột x 6 hàng = 24 ô

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

        public void AddItem(InventoryItem item)
        {
            if (item != null)
            {
                items.Add(item);
            }
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
            EquipGearItem(wep);
            EquipGearItem(helm);
            EquipGearItem(armor);
            EquipGearItem(pants);
            EquipGearItem(boots);
            EquipGearItem(neck);
            EquipGearItem(ring);

            // 2. Sách Kỹ Năng (Cuộn Trục Bí Kíp)
            var s1 = new InventoryItem("scroll_s1", "Bí Kíp: Cầu Ma Thuật", ItemType.SkillScroll, EquipSlot.None, "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_scroll_arcane", "ST: 25 + 100% INT | CD: 1.0s | Tầm: 8.0m", "Bắn cầu ma thuật theo đường thẳng (Line Skillshot).", 1,
                new SkillScrollData("scroll_arcane_bolt", "Cuộn Trục: Cầu Ma Thuật", "Phẩm D", new Color(0.3f, 0.85f, 0.4f), "Ma Pháp", 15, 1.0f, 25, 1.0f, 1, "icon_skill_arcane_bolt", "item_scroll_arcane", "Bắn cầu ma pháp xuyên thấu.", 8.0f));

            var s2 = new InventoryItem("scroll_s2", "Bí Kíp: Mưa Thiên Thạch", ItemType.SkillScroll, EquipSlot.None, "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "item_scroll_meteor", "ST: 40 + 120% INT | CD: 3.0s | Tầm: 7.0m", "Thả khối thiên thạch nổ vùng đất (Ground AoE).", 1,
                new SkillScrollData("scroll_meteor", "Cuộn Trục: Mưa Thiên Thạch", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "Hỏa", 25, 3.0f, 40, 1.2f, 2, "icon_skill_meteor", "item_scroll_meteor", "Thiên thạch giáng xuống diện rộng.", 7.0f));

            var s3 = new InventoryItem("scroll_s3", "Bí Kíp: Sóng Lửa", ItemType.SkillScroll, EquipSlot.None, "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "item_scroll_flame", "ST: 30 + 80% INT | CD: 2.5s | Tầm: 4.5m", "Quét bão lửa hình quạt 60 độ (Cone Sweep).", 1,
                new SkillScrollData("scroll_flame_cone", "Cuộn Trục: Sóng Lửa", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Hỏa", 20, 2.5f, 30, 0.8f, 3, "icon_skill_flame_cone", "item_scroll_flame", "Bão lửa thiêu rụi phía trước.", 4.5f));

            var s4 = new InventoryItem("scroll_s4", "Bí Kíp: Bộc Phá Thánh Điển", ItemType.SkillScroll, EquipSlot.None, "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "item_scroll_nova", "ST: 60 + 150% INT | CD: 5.0s | Tầm: 2.5m", "Nổ hào quang 360 độ cực mạnh (Self Nova).", 1,
                new SkillScrollData("scroll_divine_nova", "Cuộn Trục: Bộc Phá Thánh Điển", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Thánh", 35, 5.0f, 60, 1.5f, 4, "icon_skill_divine_nova", "item_scroll_nova", "Bùng nổ hào quang thần thánh.", 2.5f));

            var s5 = new InventoryItem("scroll_s5", "Bí Kíp: Băng Huyết Tiễn", ItemType.SkillScroll, EquipSlot.None, "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "item_scroll_frost", "ST: 48 + 130% INT | CD: 2.0s | Tầm: 8.0m", "Mũi tên băng giá làm chậm mục tiêu.", 1,
                new SkillScrollData("scroll_frost_arrow", "Cuộn Trục: Băng Huyết Tiễn", "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "Băng", 22, 2.0f, 48, 1.3f, 1, "item_scroll_frost", "item_scroll_frost", "Bắn tên băng sắc lẹm.", 8.0f));

            var s6 = new InventoryItem("scroll_s6", "Bí Kíp: Lôi Thần Nộ", ItemType.SkillScroll, EquipSlot.None, "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "item_scroll_generic", "ST: 55 + 140% INT | CD: 4.0s | Tầm: 7.0m", "Gọi sấm sét oanh tạc vùng đất chỉ định.", 1,
                new SkillScrollData("scroll_thunder_burst", "Cuộn Trục: Lôi Thần Nộ", "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "Lôi", 32, 4.0f, 55, 1.4f, 2, "item_scroll_generic", "item_scroll_generic", "Tia sét thần sấm oanh tạc.", 7.0f));

            items.Add(s1);
            items.Add(s2);
            items.Add(s3);
            items.Add(s4);
            items.Add(s5);
            items.Add(s6);

            // Gắn sẵn vào 4 ô skill hotbar
            EquipSkillScrollToHotbar(s1, 0, true);
            EquipSkillScrollToHotbar(s2, 1, true);
            EquipSkillScrollToHotbar(s3, 2, true);
            EquipSkillScrollToHotbar(s4, 3, true);

            // 3. Bình Thuốc Hồi Phục
            items.Add(new InventoryItem("item_hp_pot", "Bình Máu HP (+35)", ItemType.Consumable, EquipSlot.None, "Phẩm E", Color.white, "item_potion_hp", "Hồi +35 Máu tức thì", "Bình dược liệu màu đỏ hồi phục sinh lực nhanh chóng.", 5));
            items.Add(new InventoryItem("item_mp_pot", "Bình Mana (+30)", ItemType.Consumable, EquipSlot.None, "Phẩm E", Color.white, "item_potion_mana", "Hồi +30 Mana tức thì", "Bình nước thần màu xanh lam bổ sung năng lượng pháp thuật.", 5));

            // 4. Thú Cưng Sóc Chibi
            var pet = new InventoryItem("pet_squirrel_egg", "Sóc Chibi SquiRRels", ItemType.Pet, EquipSlot.None, "Phẩm A", new Color(1f, 0.85f, 0.35f), "pet_squirrel", "Thú Cưng Đồng Hành", "Thú cưng linh vật đáng yêu nhấp nhô nhẹ nhàng đi theo sau lưng.");
            pet.isEquipped = true; // Đang xuất chiến
            items.Add(pet);
        }

        // ================= XỬ LÝ TRANG BỊ TAB 1 =================
        public void EquipGearItem(InventoryItem item)
        {
            if (item == null || item.itemType != ItemType.Equipment || item.equipSlot == EquipSlot.None) return;

            // Nếu ô đó đã có món khác -> Tháo món cũ ra trước
            if (equippedGear.TryGetValue(item.equipSlot, out var oldItem) && oldItem != null)
            {
                oldItem.isEquipped = false;
            }

            equippedGear[item.equipSlot] = item;
            item.isEquipped = true;
            Debug.Log($"<color=green>[TRANG BỊ] Đã mặc '{item.itemName}' vào ô {item.equipSlot}!</color>");
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

        public void RequestDropItem(InventoryItem item)
        {
            if (!CanDropItem(item))
            {
                Debug.LogWarning($"[BẢO VỆ AN TOÀN] Vật phẩm '{item.itemName}' đang được trang bị/sử dụng! Không thể vứt!");
                return;
            }

            pendingDropItem = item;
            dropCountdown = 0f; // Bỏ thời gian chờ 5s theo yêu cầu user
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

        public int GetItemCount(string itemId)
        {
            var item = items.Find(it => it.id == itemId);
            return item != null ? item.quantity : 0;
        }

        // ================= THÚ CƯNG (THÁO RA / THẢ) =================
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
    }
}
