using System;
using System.Collections.Generic;
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

        [Header("Potion Bag (Bình Dược Liệu)")]
        [SerializeField] private int hpPotionCount = 5;
        [SerializeField] private int manaPotionCount = 5;

        private Health health;
        private InventoryManager inventoryManager;

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

        public void SetCharacterClass(string newClass)
        {
            if (!string.IsNullOrEmpty(newClass))
            {
                characterClass = newClass;
                ApplyInherentClassPerkStats();
            }
        }

        public void SetElement(string newElement)
        {
            if (!string.IsNullOrEmpty(newElement))
            {
                element = newElement;
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

            // Khởi tạo danh sách Danh Hiệu (Titles)
            InitTitles();
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
            Debug.Log($"<color=cyan>[BUFF SYSTEM]</color> Đã kích hoạt hiệu ứng '{buffName}' ({duration:F1}s) - Trị giá: +{statValue}%!");
        }

        public float GetDamageBuffMultiplier()
        {
            float mult = 0f;
            if (activeBuffs != null)
            {
                for (int i = 0; i < activeBuffs.Count; i++)
                {
                    var b = activeBuffs[i];
                    if (b != null && b.remainingTime > 0f)
                    {
                        if (b.statType == 1) mult += (b.statValue / 100f);
                    }
                }
            }
            return mult;
        }

        public float GetDefenseBuffMultiplier()
        {
            float mult = 0f;
            if (activeBuffs != null)
            {
                for (int i = 0; i < activeBuffs.Count; i++)
                {
                    var b = activeBuffs[i];
                    if (b != null && b.remainingTime > 0f)
                    {
                        if (b.statType == 2) mult += (b.statValue / 100f);
                    }
                }
            }
            return mult;
        }

        /// <summary>
        /// Tính sát thương đòn đánh thường chuẩn xác (STR, INT, Perk, Title và nhân hệ số BUFF).
        /// </summary>
        public int CalculateBasicAttackDamage()
        {
            int baseDmg = 16 + Mathf.RoundToInt(intelligence * 0.4f + strength * 0.3f);
            baseDmg += InherentBonusAttackDmg;
            baseDmg += TitleBonusPhysAtk;
            float buffMult = GetDamageBuffMultiplier();
            int totalDmg = Mathf.RoundToInt(baseDmg * (1f + buffMult));
            return Mathf.Max(1, totalDmg);
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

        /// <summary>
        /// Phím [Q]: Auto sử dụng Thức Ăn hoặc Bình Máu (Không phân biệt thuốc hay đồ ăn, cứ là món hồi máu / thức ăn là auto ăn)
        /// </summary>
        public void UseFoodOrHpRecovery()
        {
            if (health == null || health.IsDead) return;
            if (health.CurrentHealth >= health.MaxHealth)
            {
                Debug.Log("[HỒI MÁU] Máu đã đầy, không cần dùng thức ăn hay bình máu!");
                return;
            }

            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();

            // Tìm trong túi món hồi máu/thức ăn: item_hp_pot, item_roast_meat, item_bread...
            InventoryItem foodItem = null;
            if (inventoryManager != null)
            {
                foodItem = inventoryManager.Items.Find(it => it.itemType == ItemType.Consumable && 
                    (it.id.Contains("hp") || it.id.Contains("food") || it.id.Contains("meat") || it.id.Contains("bread")));
            }

            if (foodItem != null && inventoryManager.ConsumeItem(foodItem.id, 1))
            {
                int healHp = 35;
                int bonusMana = 0;
                if (foodItem.id.Contains("meat") || foodItem.id.Contains("food"))
                {
                    healHp = 30;
                    bonusMana = 20;
                }

                HealPlayer(healHp, true);
                if (bonusMana > 0) RestoreMana(bonusMana);

                string iconMsg = foodItem.id.Contains("meat") ? "🍗" : "❤️";
                FloatingDamageManager.Instance?.SpawnFloatingText(transform.position + Vector3.up * 1.2f, $"{iconMsg} Đã dùng {foodItem.itemName} (+{healHp} HP)", new Color(0.2f, 0.95f, 0.35f));
                Debug.Log($"<color=green>[THỨC ĂN/THUỐC]</color> Đã dùng '{foodItem.itemName}' (+{healHp} HP, +{bonusMana} MP).");
            }
            else
            {
                // Fallback nếu không có item trong bag nhưng còn biến đếm
                if (hpPotionCount > 0)
                {
                    hpPotionCount--;
                    HealPlayer(35, true);
                    Debug.Log($"[THUỐC] Đã uống Bình Máu cơ bản! Còn {hpPotionCount} bình.");
                }
                else
                {
                    FloatingDamageManager.Instance?.SpawnFloatingText(transform.position + Vector3.up * 1.2f, "❌ Đã hết Thức Ăn & Bình Máu!", new Color(1f, 0.35f, 0.35f));
                    Debug.LogWarning("[HỒI MÁU] Đã hết thức ăn và bình máu trong hành trang!");
                }
            }
        }

        public void UseHpPotion() => UseFoodOrHpRecovery();

        /// <summary>
        /// Phím [E]: Auto sử dụng Nước Uống hoặc Bình Mana (Không phân biệt thuốc hay nước uống, cứ là món hồi mana / nước uống là auto uống)
        /// </summary>
        public void UseDrinkOrManaRecovery()
        {
            if (currentMana >= maxMana)
            {
                Debug.Log("[HỒI MANA] Mana đã đầy, không cần dùng nước uống hay bình mana!");
                return;
            }

            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();

            // Tìm trong túi món hồi mana/nước uống: item_mp_pot, item_water, item_drink, item_tea...
            InventoryItem drinkItem = null;
            if (inventoryManager != null)
            {
                drinkItem = inventoryManager.Items.Find(it => it.itemType == ItemType.Consumable && 
                    (it.id.Contains("mp") || it.id.Contains("mana") || it.id.Contains("water") || it.id.Contains("drink") || it.id.Contains("tea")));
            }

            if (drinkItem != null && inventoryManager.ConsumeItem(drinkItem.id, 1))
            {
                int manaRestore = 35;
                RestoreMana(manaRestore);
                FloatingDamageManager.Instance?.SpawnFloatingText(transform.position + Vector3.up * 1.2f, $"💧 Đã uống {drinkItem.itemName} (+{manaRestore} MP)", new Color(0.2f, 0.75f, 1f));
                Debug.Log($"<color=cyan>[NƯỚC UỐNG/MANA]</color> Đã uống '{drinkItem.itemName}' (+{manaRestore} MP).");
            }
            else
            {
                if (manaPotionCount > 0)
                {
                    manaPotionCount--;
                    RestoreMana(30);
                    FloatingDamageManager.Instance?.SpawnFloatingText(transform.position + Vector3.up * 1.2f, "💧 +30 MP", new Color(0.2f, 0.75f, 1f));
                    Debug.Log($"[MANA] Đã uống Bình Mana cơ bản! Còn {manaPotionCount} bình.");
                }
                else
                {
                    FloatingDamageManager.Instance?.SpawnFloatingText(transform.position + Vector3.up * 1.2f, "❌ Đã hết Nước Uống & Bình Mana!", new Color(1f, 0.35f, 0.35f));
                    Debug.LogWarning("[HỒI MANA] Đã hết nước uống và bình mana trong hành trang!");
                }
            }
        }

        public void UseManaPotion() => UseDrinkOrManaRecovery();

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

        // ================= HỆ THỐNG DANH HIỆU (CHARACTER TITLE SYSTEM) =================
        [SerializeField] private string equippedTitleId = "title_rookie";
        private List<CharacterTitle> titleList = new List<CharacterTitle>();

        public CharacterTitle EquippedTitle => TitleList.Find(t => t.id == equippedTitleId);
        public List<CharacterTitle> TitleList
        {
            get
            {
                if (titleList == null || titleList.Count == 0) InitTitles();
                return titleList;
            }
        }

        public int TitleBonusHp => EquippedTitle != null ? EquippedTitle.bonusHp : 0;
        public int TitleBonusArmor => EquippedTitle != null ? EquippedTitle.bonusArmor : 0;
        public int TitleBonusMagicAtk => EquippedTitle != null ? EquippedTitle.bonusMagicAtk : 0;
        public int TitleBonusPhysAtk => EquippedTitle != null ? EquippedTitle.bonusPhysAtk : 0;
        public float TitleBonusSpeed => EquippedTitle != null ? EquippedTitle.bonusSpeed : 0f;

        public void EquipTitle(string titleId)
        {
            var t = TitleList.Find(x => x.id == titleId);
            if (t != null && t.isUnlocked)
            {
                equippedTitleId = titleId;
                Debug.Log($"<color=yellow>[DANH HIỆU]</color> Đã đeo danh hiệu: {t.name}");
                FloatingDamageManager.Instance?.SpawnFloatingText(transform.position + Vector3.up * 1.6f, $"✨ Đã Đeo: {t.name}", t.titleColor);
            }
        }

        public void UnequipTitle()
        {
            equippedTitleId = "";
            Debug.Log("<color=yellow>[DANH HIỆU]</color> Đã tháo danh hiệu!");
        }

        public void InitTitles()
        {
            if (titleList.Count > 0) return;
            titleList.Add(new CharacterTitle("title_rookie", "[Tân Thủ Khai Hoang]", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "🏆", "+30 Máu Tối Đa · +5 Giáp", 30, 5, 0, 0, 0f, true));
            titleList.Add(new CharacterTitle("title_knight", "[Kị Sĩ Tiên Phong]", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "🛡️", "+15 Giáp Vật Lý · +10 Kháng Phép", 0, 15, 0, 0, 0f, true));
            titleList.Add(new CharacterTitle("title_archmage", "[Ma Đạo Tôn Sư]", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "🔮", "+25 Công Phép · +20 Mana Tối Đa", 0, 0, 25, 0, 0f, true));
            titleList.Add(new CharacterTitle("title_brawler", "[Bá Vương Quyền Cước]", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "🥊", "+15 ST Đánh Thường · +5% Bạo Kích", 0, 0, 0, 15, 0f, true));
            titleList.Add(new CharacterTitle("title_archer", "[Thần Tiễn Bách Phát]", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "🏹", "+0.5 m/s Tốc Chạy · +1.0m Tầm Đánh", 0, 0, 0, 0, 0.5f, true));
            titleList.Add(new CharacterTitle("title_conqueror", "[Chiến Thần Vô Địch]", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "👑", "+50 Máu · +20 Công Phép · +15 Giáp", 50, 15, 20, 0, 0f, true));
        }
    }

    [Serializable]
    public class CharacterTitle
    {
        public string id;
        public string name;
        public string rarity;
        public Color titleColor;
        public string iconEmoji;
        public string statDesc;
        public int bonusHp;
        public int bonusArmor;
        public int bonusMagicAtk;
        public int bonusPhysAtk;
        public float bonusSpeed;
        public bool isUnlocked;

        public CharacterTitle(string id, string name, string rarity, Color titleColor, string iconEmoji, string statDesc,
                              int bonusHp = 0, int bonusArmor = 0, int bonusMagicAtk = 0, int bonusPhysAtk = 0, float bonusSpeed = 0f, bool isUnlocked = true)
        {
            this.id = id;
            this.name = name;
            this.rarity = rarity;
            this.titleColor = titleColor;
            this.iconEmoji = iconEmoji;
            this.statDesc = statDesc;
            this.bonusHp = bonusHp;
            this.bonusArmor = bonusArmor;
            this.bonusMagicAtk = bonusMagicAtk;
            this.bonusPhysAtk = bonusPhysAtk;
            this.bonusSpeed = bonusSpeed;
            this.isUnlocked = isUnlocked;
        }
    }
}
