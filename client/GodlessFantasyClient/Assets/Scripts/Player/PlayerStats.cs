using System;
using UnityEngine;
using GodlessFantasy.Combat;
using GodlessFantasy.Core;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Hệ Thống Thuộc Tính & Chỉ Số Nhân Vật (Chuẩn GDD 03):
    /// - Level & Hệ Nguyên Tố (Hệ Hỏa / Hệ Lôi).
    /// - STR, INT, VIT, AGI.
    /// - Quản lý Máu & Mana (Hồi tự nhiên + Bình Thuốc Q / E).
    /// - Tự động scale sát thương kỹ năng dựa trên INT!
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Character Identity (Định Danh & Cấp Độ)")]
        [SerializeField] private string characterName = "Dũng Sĩ SquiRRels";
        [SerializeField] private string characterClass = "Kị Sĩ";
        [SerializeField] private int level = 1;
        [SerializeField] private string element = "Hỏa";

        [Header("Core Attributes (Thuộc Tính Cơ Bản)")]
        [SerializeField] private int strength = 8;
        [SerializeField] private int intelligence = 12;
        [SerializeField] private int vitality = 10;
        [SerializeField] private int agility = 9;

        [Header("Mana Pool")]
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int currentMana = 80;

        [Header("Regeneration Rates (Hồi Phục Tự Nhiên)")]
        [SerializeField] private float hpRegenPerSec = 2.0f;
        [SerializeField] private float manaRegenPerSec = 4.0f;

        [Header("Potion Bag (Bình Dược Liệu)")]
        [SerializeField] private int hpPotionCount = 5;
        [SerializeField] private int manaPotionCount = 5;

        private Health health;
        private InventoryManager inventoryManager;
        private float regenAccumulator = 0f;

        public const int MaxNameLength = 20;

        public string CharacterName
        {
            get => characterName;
            set
            {
                if (string.IsNullOrEmpty(value)) characterName = "Dũng Sĩ";
                else if (value.Length > MaxNameLength) characterName = value.Substring(0, MaxNameLength);
                else characterName = value;
            }
        }
        public string CharacterClass => string.IsNullOrEmpty(characterClass) ? "Kị Sĩ" : characterClass;
        public int Level => level;
        public string Element
        {
            get
            {
                if (string.IsNullOrEmpty(element)) return "Hỏa";
                if (element.Contains("(")) return element.Substring(0, element.IndexOf('(')).Trim();
                return element;
            }
        }

        public int Strength => strength;
        public int Intelligence => intelligence;
        public int Vitality => vitality;
        [Header("Stat Points & Currencies (Điểm Tiềm Năng & Tiền Tệ)")]
        [SerializeField] private int availableStatPoints = 5;
        [SerializeField] private int gold = 1500;
        [SerializeField] private int magicCrystals = 25;
        [SerializeField] private int rebirthPoints = 2;

        public int AvailableStatPoints => availableStatPoints;
        public int Gold => gold;
        public int MagicCrystals => magicCrystals;
        public int RebirthPoints => rebirthPoints;

        public void AddStrength()
        {
            if (availableStatPoints > 0) { strength++; availableStatPoints--; }
        }

        public void AddIntelligence()
        {
            if (availableStatPoints > 0) { intelligence++; availableStatPoints--; }
        }

        public void AddVitality()
        {
            if (availableStatPoints > 0)
            {
                vitality++;
                availableStatPoints--;
                if (health != null) health.SetMaxHealth(health.MaxHealth + 10);
            }
        }

        public void AddAgility()
        {
            if (availableStatPoints > 0) { agility++; availableStatPoints--; }
        }

        public void AddGold(int amount) => gold += amount;
        public bool ConsumeGold(int amount)
        {
            if (gold >= amount) { gold -= amount; return true; }
            return false;
        }

        public int Agility => agility;
        public int CurrentMana => currentMana;
        public int MaxMana => maxMana;
        public int HpPotionCount => hpPotionCount;
        public int ManaPotionCount => manaPotionCount;

        public event Action<int, int> OnManaChanged;

        private void Awake()
        {
            if (!string.IsNullOrEmpty(characterName) && characterName.Length > MaxNameLength)
            {
                characterName = characterName.Substring(0, MaxNameLength);
            }
            health = GetComponent<Health>();

            // Áp dụng thưởng chỉ số vĩnh viễn từ Perk Bẩm Sinh của Hệ
            ApplyInherentClassPerkStats();
        }

        private float hpRegenTimer = 0f;
        private float manaRegenTimer = 0f;

        // Chu kỳ hồi máu khắc khổ: 40s (Kị sĩ: 30s, Võ sĩ: 35s, Pháp sư/Xạ thủ: 40s)
        public float HpRegenInterval => (characterClass == "Kị Sĩ") ? 30.0f : (characterClass == "Võ Sĩ") ? 35.0f : 40.0f;
        public int HpRegenAmount => (characterClass == "Kị Sĩ" || characterClass == "Võ Sĩ") ? 2 : 1;
        public float HpRegenTimerProgress => Mathf.Clamp01(hpRegenTimer / HpRegenInterval);

        // Chu kỳ hồi Mana: 3.0s (Pháp sư: +4, Các hệ khác: +2)
        public float ManaRegenInterval => 3.0f;
        public int ManaRegenAmount => (characterClass == "Pháp Sư") ? (4 + Mathf.FloorToInt(intelligence / 12f)) : 2;

        // ================= HỆ THỐNG PERK BẨM SINH ĐỘC NHẤT THEO HỆ =================
        public string InherentPerkName =>
            (characterClass == "Kị Sĩ") ? "Trái Tim Thép" :
            (characterClass == "Pháp Sư") ? "Học Giả Chân Kinh" :
            (characterClass == "Võ Sĩ") ? "Bản Năng Dã Thú" : "Mắt Ưng Thần Tiễn";

        public string InherentPerkDesc =>
            (characterClass == "Kị Sĩ") ? "+50 HP Tối Đa · +15 Giáp · Hồi HP 30s (+2 HP)" :
            (characterClass == "Pháp Sư") ? "+40 Mana Tối Đa · +20 Công Phép · Hồi Mana +4/3s" :
            (characterClass == "Võ Sĩ") ? "+8% Bạo Kích · +12 ST Đòn Đánh Thường · Hồi HP 35s (+2 HP)" :
            "+1.5m Tầm Đánh Thường (6.5m) · +0.5m/s Tốc Chạy";

        public int InherentBonusMaxHp => (characterClass == "Kị Sĩ") ? 50 : 0;
        public int InherentBonusMaxMana => (characterClass == "Pháp Sư") ? 40 : 0;
        public int InherentBonusArmor => (characterClass == "Kị Sĩ") ? 15 : 0;
        public int InherentBonusMagicAtk => (characterClass == "Pháp Sư") ? 20 : 0;
        public float InherentBonusCritRate => (characterClass == "Võ Sĩ") ? 8.0f : 0f;
        public int InherentBonusAttackDmg => (characterClass == "Võ Sĩ") ? 12 : 0;
        public float InherentBonusBasicRange => (characterClass == "Xạ Thủ") ? 1.5f : 0f;
        public float InherentBonusMoveSpeed => (characterClass == "Xạ Thủ") ? 0.5f : 0f;

        private void ApplyInherentClassPerkStats()
        {
            if (InherentBonusMaxMana > 0)
            {
                maxMana += InherentBonusMaxMana;
                currentMana = maxMana;
            }
            if (health != null && InherentBonusMaxHp > 0)
            {
                health.IncreaseMaxHealth(InherentBonusMaxHp);
            }
        }

        // ================= HỆ THỐNG ACTIVE BUFF (ARCHETYPE SỐ 5) =================
        [Serializable]
        public class ActiveBuff
        {
            public string id;
            public string name;
            public string iconName;
            public float duration;
            public float remainingTime;
            public int statType; // 1=Công Phép, 2=Giáp, 3=Tốc Chạy, 4=Bạo Kích
            public float statValue;
        }

        private System.Collections.Generic.List<ActiveBuff> activeBuffs = new System.Collections.Generic.List<ActiveBuff>();
        public System.Collections.Generic.List<ActiveBuff> ActiveBuffs => activeBuffs;

        public void ApplyBuff(string id, string buffName, string iconName, float duration, int statType = 1, float statValue = 20f)
        {
            var existing = activeBuffs.Find(b => b.id == id);
            if (existing != null)
            {
                existing.remainingTime = duration;
            }
            else
            {
                activeBuffs.Add(new ActiveBuff
                {
                    id = id,
                    name = buffName,
                    iconName = iconName,
                    duration = duration,
                    remainingTime = duration,
                    statType = statType,
                    statValue = statValue
                });
            }
            Debug.Log($"<color=cyan>[BUFF SYSTEM]</color> Đã kích hoạt hiệu ứng '{buffName}' ({duration:F1}s)!");
        }

        private void Update()
        {
            // 1. CƠ CHẾ HỒI MÁU KHẮC KHỔ (40 GIÂY / TICK, CHỐNG BUFF BẨN)
            hpRegenTimer += Time.deltaTime;
            if (hpRegenTimer >= HpRegenInterval)
            {
                hpRegenTimer -= HpRegenInterval;
                if (health != null && health.CurrentHealth < health.MaxHealth && !health.IsDead)
                {
                    HealPlayer(HpRegenAmount, false);
                }
            }

            // 2. CƠ CHẾ HỒI MANA (3 GIÂY / TICK)
            manaRegenTimer += Time.deltaTime;
            if (manaRegenTimer >= ManaRegenInterval)
            {
                manaRegenTimer -= ManaRegenInterval;
                if (currentMana < maxMana)
                {
                    RestoreMana(ManaRegenAmount);
                }
            }

            // 3. ĐẾM NGƯỢC THỜI GIAN ACTIVE BUFF
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                activeBuffs[i].remainingTime -= Time.deltaTime;
                if (activeBuffs[i].remainingTime <= 0f)
                {
                    Debug.Log($"<color=yellow>[BUFF SYSTEM]</color> Hiệu ứng '{activeBuffs[i].name}' đã hết tác dụng!");
                    activeBuffs.RemoveAt(i);
                }
            }

            // Phím tắt nhanh: Q dùng bình máu, E dùng bình mana
            if (GameInput.GetHpPotionKeyDown())
            {
                UseHpPotion();
            }
            if (GameInput.GetManaPotionKeyDown() && !VillageNPC.IsPlayerNearAnyNPC && !VillageNPC.IsAnyNPCDialogueOpen)
            {
                UseManaPotion();
            }
        }

        public bool ConsumeMana(int amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                OnManaChanged?.Invoke(currentMana, maxMana);
                return true;
            }
            return false;
        }

        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        public void UseHpPotion()
        {
            if (health == null || health.IsDead) return;
            if (health.CurrentHealth >= health.MaxHealth)
            {
                Debug.Log("[POTION] Máu đã đầy, không cần dùng Bình Máu!");
                return;
            }

            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();

            int countInBag = inventoryManager != null ? inventoryManager.GetItemCount("item_hp_pot") : hpPotionCount;
            if (countInBag > 0)
            {
                if (inventoryManager != null) inventoryManager.ConsumeItem("item_hp_pot", 1);
                hpPotionCount = inventoryManager != null ? inventoryManager.GetItemCount("item_hp_pot") : hpPotionCount - 1;
                int healAmount = 35;
                HealPlayer(healAmount, true);
                Debug.Log($"[POTION] Đã uống Bình Máu! Hồi {healAmount} HP. Còn lại trong túi: {hpPotionCount} bình.");
            }
            else
            {
                Debug.LogWarning("[POTION] Đã hết Bình Máu trong hành trang!");
            }
        }

        public void UseManaPotion()
        {
            if (currentMana >= maxMana)
            {
                Debug.Log("[POTION] Mana đã đầy, không cần dùng Bình Mana!");
                return;
            }

            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();

            int countInBag = inventoryManager != null ? inventoryManager.GetItemCount("item_mp_pot") : manaPotionCount;
            if (countInBag > 0)
            {
                if (inventoryManager != null) inventoryManager.ConsumeItem("item_mp_pot", 1);
                manaPotionCount = inventoryManager != null ? inventoryManager.GetItemCount("item_mp_pot") : manaPotionCount - 1;
                int manaAmount = 30;
                RestoreMana(manaAmount);
                FloatingDamageManager.Spawn(transform.position, manaAmount, false);
                Debug.Log($"[POTION] Đã uống Bình Mana! Hồi {manaAmount} MP. Còn lại trong túi: {manaPotionCount} bình.");
            }
            else
            {
                Debug.LogWarning("[POTION] Đã hết Bình Mana trong hành trang!");
            }
        }

        private void HealPlayer(int amount, bool showPopup)
        {
            if (health == null) return;
            // Gọi hàm hồi máu Health
            int newHp = Mathf.Min(health.MaxHealth, health.CurrentHealth + amount);
            typeof(Health).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(health, newHp);

            if (showPopup)
            {
                FloatingDamageManager.Spawn(transform.position + new Vector3(0, 0.4f, 0), amount, true);
            }
        }

        /// <summary>
        /// Sát thương Chiêu 1 = Cơ bản (25) + 100% Trí Tuệ (INT = 12) = 37 Sát Thương Phép!
        /// </summary>
        public int GetSkill1TotalDamage(int baseDamage = 25)
        {
            return baseDamage + intelligence;
        }
    }
}
