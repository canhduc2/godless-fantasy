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
        public const int MAX_CONSUMABLE_STACK = 100; // Tất cả vật phẩm tiêu hao chỉ chiếm 1 ô hành trang, dồn tối đa 100 món theo yêu cầu

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
        public InventoryItem GetEquippedItem(EquipSlot slot) => equippedGear.TryGetValue(slot, out var it) ? it : null;
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

        public static string NormalizeItemId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            if (id == "item_potion_hp") return "item_hp_pot";
            if (id == "item_potion_mp") return "item_mp_pot";
            return id;
        }

        /// <summary>
        /// Tự động gom toàn bộ các vật phẩm tiêu hao (Consumable) cùng loại vào 1 ô duy nhất, dồn tối đa 100 món
        /// </summary>
        public void ConsolidateConsumableStacks()
        {
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null || it.itemType != ItemType.Consumable) continue;

                string normId = NormalizeItemId(it.id);
                for (int j = items.Count - 1; j > i; j--)
                {
                    var other = items[j];
                    if (other == null || other.itemType != ItemType.Consumable) continue;

                    if (NormalizeItemId(other.id) == normId || string.Equals(other.itemName, it.itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        it.quantity = Mathf.Min(it.quantity + other.quantity, MAX_CONSUMABLE_STACK);
                        items.RemoveAt(j);
                        Debug.Log($"[HÀNH TRANG] Đã hợp nhất ô tiêu hao trùng lặp '{other.itemName}' vào 1 ô duy nhất (Tổng: {it.quantity}/{MAX_CONSUMABLE_STACK})");
                    }
                }
            }
        }

        public bool CanAddItem(InventoryItem item)
        {
            if (item == null) return false;
            // Nếu là vật phẩm tiêu hao (Consumable) và đã có sẵn trong túi -> Chỉ chiếm 1 ô duy nhất (cộng dồn tới 100)
            if (item.itemType == ItemType.Consumable)
            {
                string normId = NormalizeItemId(item.id);
                var existing = items.Find(x => x != null && x.itemType == ItemType.Consumable && 
                    (NormalizeItemId(x.id) == normId || string.Equals(x.itemName, item.itemName, StringComparison.OrdinalIgnoreCase)));
                if (existing != null) return true;
            }
            return items.Count < currentCapacity;
        }

        public bool AddItem(InventoryItem item)
        {
            if (item == null) return false;
            EnsureSlotIndices();

            // 1. Kiểm tra cộng dồn đối với Dược phẩm / Tiêu hao (Tất cả chỉ chiếm 1 ô hành trang, dồn tối đa 100)
            if (item.itemType == ItemType.Consumable)
            {
                string normId = NormalizeItemId(item.id);
                var existing = items.Find(x => x != null && x.itemType == ItemType.Consumable && 
                    (NormalizeItemId(x.id) == normId || string.Equals(x.itemName, item.itemName, StringComparison.OrdinalIgnoreCase)));
                if (existing != null)
                {
                    existing.quantity = Mathf.Min(existing.quantity + item.quantity, MAX_CONSUMABLE_STACK);
                    Debug.Log($"<color=green>[HÀNH TRANG]</color> Đã cộng dồn {item.quantity} '{item.itemName}' vào 1 ô duy nhất (Hiện có: {existing.quantity}/{MAX_CONSUMABLE_STACK})!");
                    return true;
                }
                item.quantity = Mathf.Min(item.quantity, MAX_CONSUMABLE_STACK);
            }

            // 2. Chặn tuyệt đối nếu đã đạt giới hạn ô hiện tại
            if (items.Count >= currentCapacity)
            {
                Debug.LogWarning($"<color=red>[HÀNH TRANG ĐẦY]</color> Không thể nhận '{item.itemName}' vì hành trang đã đầy ({items.Count}/{currentCapacity})!");
                return false;
            }

            var used = new HashSet<int>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].slotIndex >= 0) used.Add(items[i].slotIndex);
            }
            int assignedSlot = 0;
            while (used.Contains(assignedSlot) && assignedSlot < currentCapacity)
            {
                assignedSlot++;
            }
            item.slotIndex = (assignedSlot < currentCapacity) ? assignedSlot : items.Count;

            items.Add(item);
            ConsolidateConsumableStacks();
            return true;
        }

        private void Awake()
        {
            EnsureReferences();
            InitializeStarterInventory();
            ConsolidateConsumableStacks();
            EnsureSlotIndices();
        }

        private void Start()
        {
            EnsureReferences();
            ConsolidateConsumableStacks();
            EnsureSlotIndices();
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
                if (!IsSlotUnlocked(i))
                {
                    foreach (var it in items)
                    {
                        if (it.itemType == ItemType.SkillScroll && it.isEquipped && it.equippedHotbarSlot == i)
                        {
                            it.isEquipped = false;
                            it.equippedHotbarSlot = -1;
                        }
                    }
                    skillAimingController.UnequipScroll(i);
                    continue;
                }

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
            EnsureSlotIndices();
        }

        // ================= XỬ LÝ TRANG BỊ TAB 1 =================
        public bool EquipGearItem(InventoryItem item, bool bypassLockCheck = false)
        {
            if (item == null || (item.itemType != ItemType.Equipment && item.itemType != ItemType.Costume) || item.equipSlot == EquipSlot.None) return false;

            EnsureReferences();

            // KIỂM TRA ĐIỀU KIỆN KÉP NGHIÊM NGẶT: GIỚI TÍNH, HỆ & THUỘC TÍNH NGUYÊN TỐ (ĐỒNG BỘ 100% VỚI BÍ KÍP)
            if (!bypassLockCheck)
            {
                string playerClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
                string playerElem = playerStats != null ? playerStats.Element : "Hỏa";
                bool isFemale = false;
                if (ProceduralCharacterRig2D.Instance != null) isFemale = ProceduralCharacterRig2D.Instance.IsFemale;
                else if (CharacterVisualController8Dir.Instance != null) isFemale = CharacterVisualController8Dir.Instance.IsFemale;

                if (!item.CanPlayerEquip(isFemale, playerClass, playerElem, out string reason))
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

        /// <summary>
        /// Kiểm tra xem người chơi có đang mặc Trang Phục (hoặc Trang Bị) có thuộc tính ẩn 'gender_swap' (Chuyển Giới) hay không.
        /// </summary>
        public bool CanSwitchGender()
        {
            // Kiểm tra ô Ngoại Trang (Costume)
            var costume = GetEquippedGear(EquipSlot.Costume);
            if (costume != null && costume.HasTrait(ItemTraits.GENDER_SWAP))
                return true;

            // Kiểm tra mở rộng cho bất kỳ trang bị nào đang mặc có trait gender_swap
            foreach (var kvp in equippedGear)
            {
                if (kvp.Value != null && kvp.Value.HasTrait(ItemTraits.GENDER_SWAP))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Được gọi khi người chơi bấm [G] chuyển đổi giới tính:
        /// Tự động tháo an toàn bất kỳ trang bị nào đang mặc mà không tương thích với giới tính mới.
        /// </summary>
        public void OnGenderSwapped(bool newIsFemale)
        {
            var slotsToUnequip = new List<EquipSlot>();
            foreach (var kvp in equippedGear)
            {
                var item = kvp.Value;
                if (item != null && !item.MatchesGender(newIsFemale))
                {
                    slotsToUnequip.Add(kvp.Key);
                }
            }

            for (int i = 0; i < slotsToUnequip.Count; i++)
            {
                var slot = slotsToUnequip[i];
                var item = equippedGear[slot];
                UnequipGearItem(slot);
                Vector3 p = transform.position + Vector3.up * (1.6f + i * 0.25f);
                string reqGenderStr = (item.requiredGender == ItemGender.Male) ? "Nam ♂" : "Nữ ♀";
                FloatingDamageManager.ShowFloatingText(p, $"⚠️ Đã tự tháo '{item.itemName}' (Chỉ dành cho {reqGenderStr})!", new Color(1f, 0.75f, 0.25f));
            }
        }

        // ================= XỬ LÝ SÁCH KỸ NĂNG & ĐIỀU KIỆN MỞ KHÓA Ô =================
        private readonly System.Collections.Generic.HashSet<int> manuallyUnlockedSkillSlots = new System.Collections.Generic.HashSet<int>();

        [SerializeField] private List<int> protectedSlotIndices = new List<int>();
        public const int MAX_PROTECTED_SLOTS = 5;

        public List<int> ProtectedSlotIndices => protectedSlotIndices;
        public int TotalProtectedSlots => Mathf.Clamp(protectedSlotIndices != null ? protectedSlotIndices.Count : 0, 0, MAX_PROTECTED_SLOTS);

        public bool IsInventorySlotProtected(int slotIndex)
        {
            return slotIndex >= 0 && protectedSlotIndices != null && protectedSlotIndices.Contains(slotIndex);
        }

        private bool isTargetingSlotToEnhance = false;
        private InventoryItem currentTargetingRune = null;

        public bool IsTargetingSlotToEnhance => isTargetingSlotToEnhance;
        public InventoryItem CurrentTargetingRune => currentTargetingRune;

        /// <summary>
        /// Bắt đầu chế độ chọn ô cụ thể trong hành trang để cường hóa thành Ô Bảo Vệ Vĩnh Viễn
        /// </summary>
        public void StartTargetingSlotEnhancement(InventoryItem runeItem)
        {
            if (TotalProtectedSlots >= MAX_PROTECTED_SLOTS)
            {
                Vector3 warnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                FloatingDamageManager.ShowFloatingText(warnPos, $"⚠️ Đã đạt tối đa ({MAX_PROTECTED_SLOTS}/{MAX_PROTECTED_SLOTS}) ô bảo vệ vĩnh viễn!", new Color(1f, 0.85f, 0.35f));
                return;
            }
            isTargetingSlotToEnhance = true;
            currentTargetingRune = runeItem;
            Debug.Log("[CƯỜNG HÓA Ô ĐỒ] Đã kích hoạt chế độ chọn ô cường hóa! Hãy click vào 1 ô trong túi đồ để cường hóa.");
        }

        public void CancelTargetingSlotEnhancement()
        {
            isTargetingSlotToEnhance = false;
            currentTargetingRune = null;
        }

        /// <summary>
        /// Cường hóa chính xác ô được người chơi click chọn thành Ô Bảo Vệ Vĩnh Viễn
        /// </summary>
        public bool EnhanceSpecificSlot(int slotIndex)
        {
            if (TotalProtectedSlots >= MAX_PROTECTED_SLOTS)
            {
                CancelTargetingSlotEnhancement();
                Vector3 warnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                FloatingDamageManager.ShowFloatingText(warnPos, $"⚠️ Đã đạt tối đa ({MAX_PROTECTED_SLOTS}/{MAX_PROTECTED_SLOTS}) ô bảo vệ!", new Color(1f, 0.85f, 0.35f));
                return false;
            }

            if (slotIndex < 0 || slotIndex >= currentCapacity)
            {
                return false;
            }

            if (protectedSlotIndices.Contains(slotIndex))
            {
                Vector3 warnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                FloatingDamageManager.ShowFloatingText(warnPos, $"⚠️ Ô #{slotIndex + 1} đã được cường hóa trước đó rồi!", new Color(1f, 0.85f, 0.35f));
                return false;
            }

            protectedSlotIndices.Add(slotIndex);

            // Tiêu hao 1 Bùa Cường Hóa Ô Đồ
            if (currentTargetingRune != null)
            {
                currentTargetingRune.quantity--;
                if (currentTargetingRune.quantity <= 0) items.Remove(currentTargetingRune);
            }
            else
            {
                var rune = items.Find(x => x != null && x.id == "item_slot_protect_rune");
                if (rune != null)
                {
                    rune.quantity--;
                    if (rune.quantity <= 0) items.Remove(rune);
                }
            }

            CancelTargetingSlotEnhancement();
            AudioManager.PlayLevelUp();
            Vector3 spawnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
            FloatingDamageManager.ShowFloatingText(spawnPos, $"✨ ĐÃ CƯỜNG HÓA Ô #{slotIndex + 1}! ({TotalProtectedSlots}/{MAX_PROTECTED_SLOTS})", new Color(0.3f, 1f, 0.6f));
            Debug.Log($"<color=green>[CƯỜNG HÓA Ô ĐỒ]</color> Đã cường hóa thành công ô #{slotIndex + 1}! ({TotalProtectedSlots}/{MAX_PROTECTED_SLOTS} ô bảo vệ vĩnh viễn)");
            return true;
        }

        public bool EnhanceNextSlot()
        {
            if (TotalProtectedSlots >= MAX_PROTECTED_SLOTS)
            {
                Vector3 warnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                FloatingDamageManager.ShowFloatingText(warnPos, $"⚠️ Đã đạt tối đa ({MAX_PROTECTED_SLOTS}/{MAX_PROTECTED_SLOTS}) ô bảo vệ vĩnh viễn!", new Color(1f, 0.85f, 0.35f));
                return false;
            }

            for (int i = 0; i < currentCapacity; i++)
            {
                if (!protectedSlotIndices.Contains(i))
                {
                    return EnhanceSpecificSlot(i);
                }
            }
            return false;
        }

        public bool UseSlotProtectionRune(InventoryItem item)
        {
            if (item == null) return false;
            if (TotalProtectedSlots >= MAX_PROTECTED_SLOTS)
            {
                Vector3 warnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                FloatingDamageManager.ShowFloatingText(warnPos, $"⚠️ Đã đạt tối đa ({MAX_PROTECTED_SLOTS}/{MAX_PROTECTED_SLOTS}) ô bảo vệ vĩnh viễn!", new Color(1f, 0.85f, 0.35f));
                return false;
            }
            StartTargetingSlotEnhancement(item);
            return true;
        }

        // ================= HỆ THỐNG VỊ TRÍ Ô & HOÁN ĐỔI / KÉO THẢ (SLOT POSITIONING & DRAG & DROP) =================
        public void EnsureSlotIndices()
        {
            var used = new HashSet<int>();
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                if (it.slotIndex >= 0 && it.slotIndex < currentCapacity && !used.Contains(it.slotIndex))
                {
                    used.Add(it.slotIndex);
                }
                else
                {
                    it.slotIndex = -1;
                }
            }

            int nextSlot = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                if (it.slotIndex < 0)
                {
                    while (used.Contains(nextSlot) && nextSlot < currentCapacity)
                    {
                        nextSlot++;
                    }
                    if (nextSlot < currentCapacity)
                    {
                        it.slotIndex = nextSlot;
                        used.Add(nextSlot);
                    }
                }
            }
        }

        public InventoryItem GetItemAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= currentCapacity) return null;
            EnsureSlotIndices();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].slotIndex == slotIndex)
                {
                    return items[i];
                }
            }
            return null;
        }

        public bool SwapSlots(int fromSlot, int toSlot)
        {
            if (fromSlot == toSlot) return false;
            if (fromSlot < 0 || fromSlot >= currentCapacity) return false;
            if (toSlot < 0 || toSlot >= currentCapacity) return false;

            EnsureSlotIndices();
            InventoryItem itemFrom = GetItemAtSlot(fromSlot);
            InventoryItem itemTo = GetItemAtSlot(toSlot);

            if (itemFrom == null && itemTo == null) return false;

            // Nếu cả 2 đều là vật phẩm tiêu hao cùng loại -> Hợp nhất stack
            if (itemFrom != null && itemTo != null && itemFrom.itemType == ItemType.Consumable && itemTo.itemType == ItemType.Consumable)
            {
                string idFrom = NormalizeItemId(itemFrom.id);
                string idTo = NormalizeItemId(itemTo.id);
                if (idFrom == idTo || string.Equals(itemFrom.itemName, itemTo.itemName, StringComparison.OrdinalIgnoreCase))
                {
                    int space = MAX_CONSUMABLE_STACK - itemTo.quantity;
                    if (space > 0)
                    {
                        int moveQty = Mathf.Min(space, itemFrom.quantity);
                        itemTo.quantity += moveQty;
                        itemFrom.quantity -= moveQty;
                        if (itemFrom.quantity <= 0)
                        {
                            items.Remove(itemFrom);
                        }
                        AudioManager.PlayPickup();
                        Debug.Log($"[HÀNH TRANG] Đã gộp {moveQty} '{itemTo.itemName}' từ ô #{fromSlot + 1} vào ô #{toSlot + 1}!");
                        return true;
                    }
                }
            }

            // Hoán đổi vị trí bình thường
            if (itemFrom != null) itemFrom.slotIndex = toSlot;
            if (itemTo != null) itemTo.slotIndex = fromSlot;

            AudioManager.PlayPickup();
            Debug.Log($"[HÀNH TRANG] Đã di chuyển/hoán đổi vật phẩm giữa ô #{fromSlot + 1} ⇄ ô #{toSlot + 1}!");
            return true;
        }

        public int GetRequiredLevelForSlot(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0: return 1;   // Ô 1: Lv 1 (Mở sẵn)
                case 1: return 25;  // Ô 2: Lv 25 (Theo yêu cầu)
                case 2: return 50;  // Ô 3: Lv 50 (Theo yêu cầu)
                case 3: return 75;  // Ô 4: Lv 75 (Theo yêu cầu)
                default: return 1;
            }
        }

        public bool IsSlotUnlocked(int slotIndex)
        {
            if (slotIndex == 0) return true;
            int pLv = playerStats != null ? playerStats.Level : 1;
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
                string warn = $"⛔ Ô kỹ năng [{slotIndex + 1}] đang khóa! Cần đạt Cấp độ {reqLv}!";
                Debug.LogWarning($"[KHÓA KỸ NĂNG] {warn}");
                Vector3 spawnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : Vector3.zero;
                FloatingDamageManager.Instance?.SpawnFloatingText(spawnPos, warn, new Color(1f, 0.35f, 0.35f));
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

        public void SwapHotbarSlots(int slotA, int slotB)
        {
            if (slotA < 0 || slotA >= 4 || slotB < 0 || slotB >= 4 || slotA == slotB) return;
            EnsureReferences();

            InventoryItem itemA = null;
            InventoryItem itemB = null;
            foreach (var it in items)
            {
                if (it.itemType == ItemType.SkillScroll && it.isEquipped)
                {
                    if (it.equippedHotbarSlot == slotA) itemA = it;
                    else if (it.equippedHotbarSlot == slotB) itemB = it;
                }
            }

            if (itemA != null) itemA.equippedHotbarSlot = slotB;
            if (itemB != null) itemB.equippedHotbarSlot = slotA;

            if (skillAimingController != null)
            {
                skillAimingController.SwapScrolls(slotA, slotB);
            }
            Debug.Log($"<color=yellow>[HOÁN ĐỔI BÍ KÍP] Đã đổi vị trí Ô [{slotA + 1}] ⇄ Ô [{slotB + 1}]!</color>");
        }

        public InventoryItem FindItemByScroll(Combat.SkillScrollData scroll)
        {
            if (scroll == null) return null;
            return items.Find(it => it != null && it.itemType == ItemType.SkillScroll && (it.scrollData == scroll || (it.scrollData != null && it.scrollData.id == scroll.id)));
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

        // ================= RỦI RO RƠI ĐỒ 5% KHI CHẾT NGOÀI SAFE ZONE =================
        /// <summary>
        /// Khi tử trận: Các ô bảo vệ vĩnh viễn (các ô cụ thể đã được người chơi chọn cường hóa) được niêm phong an toàn 100%.
        /// Các món ngoài vùng bảo vệ chỉ có 5% tỉ lệ rơi tự do ra sàn đất theo đúng yêu cầu user.
        /// Đồ trong Rương Rebirth và đồ đang mặc trên người không bao giờ bị rơi.
        /// </summary>
        public List<string> DropUnprotectedLootOnDeath()
        {
            EnsureSlotIndices();
            List<string> droppedNames = new List<string>();
            List<InventoryItem> toRemove = new List<InventoryItem>();
            Vector3 deathCenter = transform.position;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;

                if (equippedGear != null && equippedGear.ContainsValue(it)) continue;

                // NẾU Ô CỦA MÓN ĐỒ NÀY ĐÃ ĐƯỢC CƯỜNG HÓA BẢO VỆ VĨNH VIỄN -> MIỄN NHIỄM RƠI ĐỒ 100%!
                if (IsInventorySlotProtected(it.slotIndex)) continue;

                // 5% tỉ lệ rơi đồ cho mỗi món không nằm trong ô bảo vệ
                if (UnityEngine.Random.value < 0.05f)
                {
                    toRemove.Add(it);
                    droppedNames.Add(it.itemName);

                    // Bật tung tóe ra sàn map ngay tại vị trí tử trận (vòng cung arc scatter bán kính 1.2m - 2.5m)
                    GroundLootItem.SpawnLoot(deathCenter, it, burstOut: true, burstRadius: 2.5f);
                }
            }

            foreach (var r in toRemove)
            {
                items.Remove(r);
            }

            return droppedNames;
        }

        /// <summary>
        /// Khi Chuyển Sinh (Rebirth): Bay sạch toàn bộ đồ đạc không nằm trong ô bảo vệ vĩnh viễn hoặc Rương Rebirth!
        /// Chỉ bảo lưu an toàn đúng các món nằm trong các ô đã được người chơi chọn cường hóa bảo vệ vĩnh viễn.
        /// </summary>
        public int ClearUnprotectedItemsOnRebirth()
        {
            EnsureSlotIndices();
            int clearedCount = 0;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var it = items[i];
                if (it == null) continue;

                // NẾU MÓN ĐỒ NẰM TRONG Ô CƯỜNG HÓA BẢO VỆ VĨNH VIỄN -> BẢO TOÀN AN TOÀN!
                if (IsInventorySlotProtected(it.slotIndex)) continue;

                // Tháo trang bị hoặc gỡ cuộn bí kíp nếu món này đang được gắn
                if (it.isEquipped && it.equipSlot != EquipSlot.None)
                {
                    UnequipGearItem(it.equipSlot);
                }
                if (it.itemType == ItemType.SkillScroll && it.equippedHotbarSlot >= 0)
                {
                    UnequipSkillScrollFromHotbar(it.equippedHotbarSlot);
                }
                if (it.itemType == ItemType.Pet && it.isEquipped)
                {
                    TogglePetSummon(it);
                }
                clearedCount++;
                items.RemoveAt(i);
            }

            // Đồng bộ lại Hotbar vì cấp độ về 1 (các ô 2, 3, 4 bị khóa)
            SyncEquippedScrollsToCombat();

            Debug.Log($"<color=yellow>[CHUYỂN SINH - REBIRTH]</color> Đã xóa sạch {clearedCount} món đồ không được bảo vệ! Bảo lưu an toàn {items.Count} món trong các ô cường hóa bảo vệ vĩnh viễn.");
            return clearedCount;
        }
    }
}
