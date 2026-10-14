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
        [SerializeField] private int currentExp = 35;
        [SerializeField] private int maxExp = 100;
        [SerializeField] private string element = "Hỏa";

        public const int MAX_LEVEL = 100; // Giới hạn cấp độ tối đa (Maxping 100, không thể vượt quá)

        public static PlayerStats Instance { get; private set; }

        public int CurrentExp => (level >= MAX_LEVEL) ? maxExp : currentExp;
        public int MaxExp => maxExp;
        public float ExpPercentage => (level >= MAX_LEVEL) ? 1.0f : (maxExp > 0 ? Mathf.Clamp01((float)currentExp / maxExp) : 0f);
        public event Action<int, int> OnExpChanged;
        public event Action<int> OnLevelUp;

        public void GainExp(int amount)
        {
            if (amount <= 0) return;
            if (level >= MAX_LEVEL)
            {
                level = MAX_LEVEL;
                currentExp = maxExp;
                OnExpChanged?.Invoke(currentExp, maxExp);
                return;
            }

            currentExp += amount;

            while (currentExp >= maxExp)
            {
                if (level >= MAX_LEVEL)
                {
                    level = MAX_LEVEL;
                    currentExp = maxExp;
                    break;
                }

                currentExp -= maxExp;
                level++;
                maxExp = Mathf.RoundToInt(100f * Mathf.Pow(level, 1.45f));
                availableStatPoints += 5;

                // Hồi đầy máu, mana và thể lực khi lên cấp
                if (health != null) health.Revive();
                currentMana = maxMana;
                currentStamina = MaxStamina;
                OnManaChanged?.Invoke(currentMana, maxMana);
                OnStaminaChanged?.Invoke(currentStamina, MaxStamina);

                string maxNotice = (level >= MAX_LEVEL) ? " 🌟 ĐÃ ĐẠT CẤP TỐI ĐA 100 (HÃY CHUYỂN SINH)!" : "";
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.3f, 
                    $"🎉 LÊN CẤP {level}! (+5 TIỀM NĂNG - Khả Dụng: {availableStatPoints}){maxNotice}", new Color(1f, 0.90f, 0.25f));
                Debug.Log($"<color=yellow>[LEVEL UP]</color> Nhân vật {characterName} đã đạt Cấp {level}! (+5 Điểm Tiềm Năng, Tổng khả dụng: {availableStatPoints})");
                AudioManager.PlayLevelUp();
                OnLevelUp?.Invoke(level);

                if (level >= MAX_LEVEL)
                {
                    level = MAX_LEVEL;
                    currentExp = maxExp;
                    break;
                }
            }

            OnExpChanged?.Invoke(currentExp, maxExp);
        }

        [Header("Core Attributes (Thuộc Tính Cơ Bản)")]
        [SerializeField] private int strength = 8;
        [SerializeField] private int intelligence = 12;
        [SerializeField] private int vitality = 10;
        [SerializeField] private int agility = 9;

        [Header("Mana Pool")]
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int currentMana = 80;

        [Header("Stamina Pool (Thể Lực)")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;

        [Header("Potion Bag (Bình Dược Liệu)")]
        [SerializeField] private int hpPotionCount = 0;
        [SerializeField] private int manaPotionCount = 0;

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
        [SerializeField] private int availableStatPoints = 0;
        [SerializeField] private int gold = 200;
        [SerializeField] private int magicCrystals = 0;
        [SerializeField] private int rebirthPoints = 0;
        [SerializeField] private int rebirthCount = 0;

        public int AvailableStatPoints => availableStatPoints;
        public int Gold { get => gold; set => gold = value; }
        public int MagicCrystals => magicCrystals;
        public int RebirthPoints { get => rebirthPoints; set => rebirthPoints = value; }
        public int RebirthCount => rebirthCount;
        public bool CanRebirth => level >= 100;

        public const int BASE_STRENGTH = 8;
        public const int BASE_INTELLIGENCE = 12;
        public const int BASE_VITALITY = 10;
        public const int BASE_AGILITY = 9;

        public bool PerformRebirth()
        {
            if (level < 100) return false;

            rebirthCount++;
            rebirthPoints++;
            level = 1;
            currentExp = 0;
            maxExp = 100;

            // Xóa sạch toàn bộ điểm tiềm năng đã cộng vào 4 chỉ số và đưa về mốc cơ bản Level 1
            strength = BASE_STRENGTH;
            intelligence = BASE_INTELLIGENCE;
            vitality = BASE_VITALITY;
            agility = BASE_AGILITY;

            // Khôi phục Máu, Mana và Thể Lực tối đa về mốc cơ bản cấp 1
            maxMana = 100 + InherentBonusMaxMana;
            currentMana = maxMana;
            currentStamina = MaxStamina;
            if (health != null)
            {
                health.SetMaxHealth(100 + InherentBonusMaxHp);
                health.Revive();
            }
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);

            // Toàn bộ điểm tiềm năng tích lũy từ cấp 100 bay sạch sẽ, chỉ nhận đúng 10 điểm thưởng Chuyển Sinh khởi đầu mới
            availableStatPoints = 10;

            // 1. Quét sạch toàn bộ đồ đạc ngoài các ô bảo vệ vĩnh viễn (0..TotalProtectedSlots - 1) hoặc Rương Rebirth
            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>() ?? UnityEngine.Object.FindAnyObjectByType<InventoryManager>();
            int wipedCount = 0;
            if (inventoryManager != null)
            {
                wipedCount = inventoryManager.ClearUnprotectedItemsOnRebirth();
            }

            // 2. HIỆU ỨNG RUNG MÀN HÌNH VÀ ÂM THANH TRÙNG SINH HOÀNH TRÁNG
            CameraController.Instance?.Shake(0.85f, 0.40f);
            AudioManager.PlayBossSlam();
            AudioManager.PlayLevelUp();
            AudioManager.PlayTeleport();

            string wipeNotice = wipedCount > 0 
                ? $"\n(Đã quét sạch {wipedCount} món ngoài ô bảo vệ & Đặt lại toàn bộ Tiềm Năng)" 
                : "\n(Toàn bộ điểm Tiềm Năng đã được làm mới sạch sẽ)";
            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f,
                $"✨ TRÙNG SINH THÀNH CÔNG! (+1 RP, +10 STAT MỚI){wipeNotice}", new Color(1f, 0.4f, 0.9f));

            OnLevelUp?.Invoke(level);
            OnExpChanged?.Invoke(currentExp, maxExp);
            return true;
        }

        public void AddStrength()
        {
            if (availableStatPoints > 0)
            {
                strength++;
                availableStatPoints--;
            }
        }

        public void AddIntelligence()
        {
            if (availableStatPoints > 0)
            {
                intelligence++;
                availableStatPoints--;
                maxMana += 8;
                currentMana = Mathf.Min(maxMana, currentMana + 8);
                OnManaChanged?.Invoke(currentMana, maxMana);
            }
        }

        public void AddVitality()
        {
            if (availableStatPoints > 0)
            {
                vitality++;
                availableStatPoints--;
                if (health != null)
                {
                    health.IncreaseMaxHealth(15);
                }
            }
        }

        public void AddAgility()
        {
            if (availableStatPoints > 0)
            {
                agility++;
                availableStatPoints--;
            }
        }

        /// <summary>
        /// Tự động phân bổ toàn bộ điểm tiềm năng theo định hướng hệ phái (Class)
        /// </summary>
        public void AutoAllocateStatPoints()
        {
            while (availableStatPoints > 0)
            {
                string cClass = CharacterClass;
                if (cClass.Contains("Kị") || cClass.Contains("Chiến") || cClass.Contains("Knight"))
                {
                    // Kị Sĩ: Ưu tiên Sinh Lực (VIT) và Sức Mạnh (STR)
                    if (vitality <= strength) AddVitality();
                    else AddStrength();
                }
                else if (cClass.Contains("Pháp") || cClass.Contains("Mage"))
                {
                    // Pháp Sư: Ưu tiên Trí Tuệ (INT) và Sinh Lực (VIT)
                    if (intelligence <= vitality * 2) AddIntelligence();
                    else AddVitality();
                }
                else if (cClass.Contains("Sát") || cClass.Contains("Cung") || cClass.Contains("Assassin"))
                {
                    // Sát Thủ / Xạ Thủ: Ưu tiên Nhanh Nhẹn (AGI) và Sức Mạnh (STR)
                    if (agility <= strength) AddAgility();
                    else AddStrength();
                }
                else
                {
                    // Mặc định: Phân bổ cân bằng theo chu kỳ chỉ số thấp nhất
                    int minVal = Mathf.Min(strength, Mathf.Min(intelligence, Mathf.Min(vitality, agility)));
                    if (strength == minVal) AddStrength();
                    else if (vitality == minVal) AddVitality();
                    else if (intelligence == minVal) AddIntelligence();
                    else AddAgility();
                }
            }
        }

        /// <summary>
        /// Tăng cấp trực tiếp và trao điểm tiềm năng (+5 điểm/cấp) vào kho điểm khả dụng để người chơi tự phân bổ hoặc test tính năng
        /// </summary>
        public void LevelUpCheat(int levelsToAdd = 1, bool autoAllocateStats = false)
        {
            if (level >= MAX_LEVEL)
            {
                level = MAX_LEVEL;
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f,
                    $"⭐ ĐÃ ĐẠT CẤP ĐỘ TỐI ĐA (CẤP {MAX_LEVEL})! ĐỦ ĐIỀU KIỆN CHUYỂN SINH (REBIRTH)", new Color(1f, 0.85f, 0.25f));
                return;
            }

            int actualLevelsAdded = 0;
            for (int i = 0; i < levelsToAdd; i++)
            {
                if (level >= MAX_LEVEL) break;
                level++;
                actualLevelsAdded++;
                maxExp = Mathf.RoundToInt(100f * Mathf.Pow(level, 1.45f));
                availableStatPoints += 5;

                if (autoAllocateStats)
                {
                    AutoAllocateStatPoints();
                }

                if (health != null) health.Revive();
                currentMana = maxMana;
                OnManaChanged?.Invoke(currentMana, maxMana);
            }

            if (actualLevelsAdded == 0) return;

            int addedPts = actualLevelsAdded * 5;
            string statMsg = autoAllocateStats 
                ? $"(Đã tự cộng +{addedPts} Điểm Tiềm Năng)" 
                : $"(Nhận +{addedPts} Điểm Tiềm Năng! Tổng khả dụng: {availableStatPoints})";
            string maxNotice = (level >= MAX_LEVEL) ? " 🌟 ĐÃ ĐẠT CẤP TỐI ĐA 100 (HÃY CHUYỂN SINH)!" : "";
            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f,
                $"🎉 LÊN CẤP {level}! {statMsg}{maxNotice}", new Color(1f, 0.90f, 0.25f));
            Debug.Log($"<color=yellow>[LEVEL UP TEST]</color> Nhân vật {characterName} đã đạt Cấp {level}! (+{addedPts} Tiềm Năng, Tổng khả dụng: {availableStatPoints})");
            AudioManager.PlayLevelUp();
            OnLevelUp?.Invoke(level);
            OnExpChanged?.Invoke(currentExp, maxExp);
        }

        public void AddGold(int amount) => gold += amount;
        public bool ConsumeGold(int amount)
        {
            if (gold >= amount) { gold -= amount; return true; }
            return false;
        }

        public bool ConsumeRebirthPoints(int amount)
        {
            if (rebirthPoints >= amount) { rebirthPoints -= amount; return true; }
            return false;
        }

        public int Agility => agility;
        public int CurrentMana
        {
            get => currentMana;
            set
            {
                currentMana = Mathf.Clamp(value, 0, maxMana);
                OnManaChanged?.Invoke(currentMana, maxMana);
            }
        }
        public int MaxMana => maxMana;
        public float MaxStamina => maxStamina + (vitality * 5f);
        public float CurrentStamina
        {
            get => currentStamina;
            set
            {
                currentStamina = Mathf.Clamp(value, 0f, MaxStamina);
                OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
            }
        }
        public int HpPotionCount => hpPotionCount;
        public int ManaPotionCount => manaPotionCount;

        public event Action<int, int> OnManaChanged;
        public event Action<float, float> OnStaminaChanged;

        public bool ConsumeStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
                return true;
            }
            currentStamina = 0f;
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
            return false;
        }

        public void RestoreStamina(float amount)
        {
            currentStamina = Mathf.Min(MaxStamina, currentStamina + amount);
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
        }

        private void Awake()
        {
            Instance = this;
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

        // Chu kỳ hồi máu khắc khổ: 40s (Kị sĩ: 28s, Võ sĩ: 32s, Pháp sư/Xạ thủ: 40s)
        public float HpRegenInterval => (characterClass == "Kị Sĩ") ? 28.0f : (characterClass == "Võ Sĩ") ? 32.0f : 40.0f;
        public int HpRegenAmount => (characterClass == "Võ Sĩ") ? 3 : (characterClass == "Kị Sĩ") ? 2 : 1;
        public float HpRegenTimerProgress => Mathf.Clamp01(hpRegenTimer / HpRegenInterval);

        // Chu kỳ hồi Mana: 3.0s (Pháp sư: +4, Các hệ khác: +2)
        public float ManaRegenInterval => 3.0f;
        public int ManaRegenAmount => (characterClass == "Pháp Sư") ? (4 + Mathf.FloorToInt(intelligence / 12f)) : 2;

        // ================= HỆ THỐNG TẦM ĐÁNH THƯỜNG ĐẶC TRƯNG TỪNG PHÁI =================
        public float BaseBasicAttackRange
        {
            get
            {
                string cls = CharacterClass;
                if (cls.Contains("Võ") || cls.Contains("Brawler")) return 1.85f;  // Võ Sĩ: Quyền phong kình khí vươn tầm thoải mái
                if (cls.Contains("Kị") || cls.Contains("Knight"))  return 2.35f;  // Kị Sĩ: Cận chiến trường kiếm/thương vung rộng rãi
                if (cls.Contains("Pháp") || cls.Contains("Mage"))  return 5.50f;  // Pháp Sư: Tầm xa ma pháp trung bình
                if (cls.Contains("Xạ") || cls.Contains("Archer"))  return 7.50f;  // Xạ Thủ: Tầm xa cung tiễn cực đại
                return 2.35f;
            }
        }

        public float DynamicBasicAttackRange => BaseBasicAttackRange + InherentBonusBasicRange;

        // Tác dụng Nhanh Nhẹn (AGI): Tăng vừa phải các chỉ số sống còn (tốc chạy +0.04m/s, hồi lướt, tốc đánh, bạo kích, né tránh)
        public float DynamicMoveSpeedBonus => Mathf.Max(0, agility - 9) * 0.04f + InherentBonusMoveSpeed + TitleBonusSpeed;
        public float DynamicDashCooldownReduction => Mathf.Min(1.0f, Mathf.Max(0, agility - 9) * 0.05f);
        public float DynamicAttackSpeedBonus => Mathf.Max(0, agility - 9) * 0.015f;
        public float CriticalRate => 5.0f + Mathf.Max(0, agility - 9) * 0.5f + InherentBonusCritRate;
        public float DodgeRate => 3.0f + Mathf.Max(0, agility - 9) * 0.4f + ((characterClass == "Xạ Thủ") ? 5.0f : 0f);

        // ================= HỆ THỐNG PERK BẨM SINH ĐỘC NHẤT THEO HỆ =================
        [SerializeField] private bool enableInherentPerk = false; // Mặc định tắt (0) theo yêu cầu xóa sạch perk của User
        public bool EnableInherentPerk => enableInherentPerk;

        public string InherentPerkName =>
            !enableInherentPerk ? "Không có" :
            (characterClass == "Kị Sĩ") ? "Thiết Vệ Kiên Cố" :
            (characterClass == "Pháp Sư") ? "Minh Tưởng Pháp Tắc" :
            (characterClass == "Võ Sĩ") ? "Kim Cang Bá Thể" : "Ưng Nhãn Thần Xạ";

        public string InherentPerkDesc =>
            !enableInherentPerk ? "Chưa kích hoạt Đặc Tính (0)" :
            (characterClass == "Kị Sĩ") ? "+50 HP · +15 Giáp · Tầm Cận Chiến 2.35m · Hồi HP 28s (+2)" :
            (characterClass == "Pháp Sư") ? "+50 Mana · +25 Công Phép · Tầm Xa 5.5m · Hồi Mana +4/3s" :
            (characterClass == "Võ Sĩ") ? "+80 HP · +16 Lực Đấm · +10% Bạo Kích · Tầm Đấm 1.85m · Bước Đi Đầm Chắc" :
            "+2.0m Tầm Bắn (Tổng 9.5m!) · +0.5m/s Tốc Chạy · +15% Bạo Kích";

        public int InherentBonusMaxHp => enableInherentPerk ? ((characterClass == "Võ Sĩ") ? 80 : (characterClass == "Kị Sĩ") ? 50 : 0) : 0;
        public int InherentBonusMaxMana => enableInherentPerk ? ((characterClass == "Pháp Sư") ? 50 : 0) : 0;
        public int InherentBonusArmor => enableInherentPerk ? ((characterClass == "Kị Sĩ") ? 15 : 0) : 0;
        public int InherentBonusMagicAtk => enableInherentPerk ? ((characterClass == "Pháp Sư") ? 25 : 0) : 0;
        public float InherentBonusCritRate => enableInherentPerk ? ((characterClass == "Xạ Thủ") ? 15.0f : (characterClass == "Võ Sĩ") ? 10.0f : 0f) : 0f;
        public int InherentBonusAttackDmg => enableInherentPerk ? ((characterClass == "Võ Sĩ") ? 16 : 0) : 0;
        public float InherentBonusBasicRange => enableInherentPerk ? ((characterClass == "Xạ Thủ") ? 2.0f : 0f) : 0f;
        public float InherentBonusMoveSpeed => enableInherentPerk ? ((characterClass == "Xạ Thủ") ? 0.5f : (characterClass == "Võ Sĩ") ? -0.2f : 0f) : 0f;

        private void ApplyInherentClassPerkStats()
        {
            if (enableInherentPerk && InherentBonusMaxMana > 0)
            {
                maxMana += InherentBonusMaxMana;
                currentMana = maxMana;
            }
            if (enableInherentPerk && health != null && InherentBonusMaxHp > 0)
            {
                health.IncreaseMaxHealth(InherentBonusMaxHp);
            }
        }

        // ================= HỆ THỐNG PERK THƯỜNG & VÒNG QUAY ĐẶC TÍNH (1 RP) =================
        [Serializable]
        public class RegularPerkDef
        {
            public string emoji;
            public string name;
            public string desc;

            public RegularPerkDef(string emoji, string name, string desc)
            {
                this.emoji = emoji;
                this.name = name;
                this.desc = desc;
            }
        }

        [SerializeField]
        private List<RegularPerkDef> acquiredPerks = new List<RegularPerkDef>();

        public List<RegularPerkDef> AcquiredPerks => acquiredPerks;

        public static readonly List<RegularPerkDef> AvailableRegularPerkPool = new List<RegularPerkDef>()
        {
            new RegularPerkDef("🗡️", "Cuồng Bạo Chí Mạng", "Tăng +10% Tỉ lệ bạo kích và +25% Sát thương bạo kích."),
            new RegularPerkDef("🏹", "Ưng Nhãn Thiện Xạ", "Tăng +15% Tầm đánh cơ bản và +10 Tốc độ tấn công."),
            new RegularPerkDef("❤️", "Trường Sinh Khí", "Tăng +100 Máu tối đa và hồi phục +2 HP mỗi giây."),
            new RegularPerkDef("💧", "Thiền Định Huyền Môn", "Tăng +80 Mana tối đa và hồi phục +3 MP mỗi giây."),
            new RegularPerkDef("🌪️", "Hấp Huyết Ma Pháp", "Chuyển hóa 5% sát thương gây ra thành Máu hồi phục."),
            new RegularPerkDef("💎", "Tài Thần Ban Phước", "Tăng +20% lượng Vàng và EXP nhận được khi diệt quái."),
            new RegularPerkDef("⚔️", "Thần Lực Bất Bại", "Tăng +15 Công vật lý và +15 Sát thương phép."),
            new RegularPerkDef("🛡️", "Bất Tử Tâm Can", "Giảm 15% toàn bộ sát thương nhận vào khi HP < 30%."),
            new RegularPerkDef("⚡", "Tốc Biến Thần Hành", "Tăng vĩnh viễn +1.0m/s Tốc độ chạy ngoài giao tranh.")
        };

        public RegularPerkDef RollRandomRegularPerk()
        {
            if (AvailableRegularPerkPool == null || AvailableRegularPerkPool.Count == 0) return null;
            // Ưu tiên quay trúng perk mà nhân vật chưa có
            var unlearned = AvailableRegularPerkPool.FindAll(p => !acquiredPerks.Exists(a => a.name == p.name));
            var pool = (unlearned.Count > 0) ? unlearned : AvailableRegularPerkPool;
            var chosen = pool[UnityEngine.Random.Range(0, pool.Count)];

            if (!acquiredPerks.Exists(a => a.name == chosen.name))
            {
                acquiredPerks.Add(chosen);
            }
            return chosen;
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
        [SerializeField] private string equippedTitleId = "";
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

        /// <summary>
        /// Áp dụng dữ liệu nhân vật chính thức từ Server khi đăng nhập hoặc tạo nhân vật thành công.
        /// </summary>
        public void ApplyCharacterData(string charName, string cClass, string elem, int cLevel, int cStr, int cInt, int cVit, int cAgi, int cGold)
        {
            characterName = charName;
            characterClass = cClass;
            element = elem;
            level = Mathf.Max(1, cLevel);
            strength = cStr;
            intelligence = cInt;
            vitality = cVit;
            agility = cAgi;
            gold = cGold;
            magicCrystals = 0;
            rebirthPoints = 0;
            availableStatPoints = 0;
            equippedTitleId = "";
            acquiredPerks.Clear();

            if (health != null)
            {
                health.SetMaxHealth(100 + vitality * 10);
                health.Revive();
            }
            maxMana = 50 + intelligence * 5;
            currentMana = maxMana;

            var overhead = GetComponent<GodlessFantasy.UI.OverheadStatusBar>();
            if (overhead != null)
            {
                overhead.Setup(GodlessFantasy.UI.OverheadType.Player, charName, $"Lv.{level} · {characterClass} · {element}", 1.05f);
            }
            Debug.Log($"<color=green>[PLAYER DATA]</color> Đã cập nhật nhân vật: {charName} (Hệ {characterClass} · {element}, Lv.{level}, {gold} Vàng)");
        }

        public void InitTitles()
        {
            if (titleList.Count > 0) return;
            // Danh sách danh hiệu trong game - ban đầu người chơi chưa mở khóa bất kỳ danh hiệu nào (isUnlocked = false)
            titleList.Add(new CharacterTitle("title_rookie", "[Tân Thủ Khai Hoang]", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "🏆", "+30 Máu Tối Đa · +5 Giáp", 30, 5, 0, 0, 0f, false));
            titleList.Add(new CharacterTitle("title_knight", "[Kị Sĩ Tiên Phong]", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "🛡️", "+15 Giáp Vật Lý · +10 Kháng Phép", 0, 15, 0, 0, 0f, false));
            titleList.Add(new CharacterTitle("title_archmage", "[Ma Đạo Tôn Sư]", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "🔮", "+25 Công Phép · +20 Mana Tối Đa", 0, 0, 25, 0, 0f, false));
            titleList.Add(new CharacterTitle("title_brawler", "[Bá Vương Quyền Cước]", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "🥊", "+15 ST Đánh Thường · +5% Bạo Kích", 0, 0, 0, 15, 0f, false));
            titleList.Add(new CharacterTitle("title_archer", "[Thần Tiễn Bách Phát]", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "🏹", "+0.5 m/s Tốc Chạy · +1.0m Tầm Đánh", 0, 0, 0, 0, 0.5f, false));
            titleList.Add(new CharacterTitle("title_conqueror", "[Chiến Thần Vô Địch]", "Phẩm S", new Color(1.0f, 0.85f, 0.25f), "👑", "+50 Máu · +20 Công Phép · +15 Giáp", 50, 15, 20, 0, 0f, false));
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
