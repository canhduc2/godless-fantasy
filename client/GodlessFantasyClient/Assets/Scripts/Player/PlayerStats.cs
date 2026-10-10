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
        [SerializeField] private int level = 1;
        [SerializeField] private string element = "Hỏa (Fire)";

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

        public string CharacterName => characterName;
        public int Level => level;
        public string Element => element;

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
            health = GetComponent<Health>();
        }

        private void Update()
        {
            // Hồi phục HP & Mana tự nhiên mỗi giây
            regenAccumulator += Time.deltaTime;
            if (regenAccumulator >= 1.0f)
            {
                regenAccumulator -= 1.0f;

                // Hồi Mana
                if (currentMana < maxMana)
                {
                    RestoreMana(Mathf.RoundToInt(manaRegenPerSec));
                }

                // Hồi Máu
                if (health != null && health.CurrentHealth < health.MaxHealth && !health.IsDead)
                {
                    HealPlayer(Mathf.RoundToInt(hpRegenPerSec), false);
                }
            }

            // Phím tắt nhanh: Q dùng bình máu, E dùng bình mana (chỉ dùng nếu không đứng gần nói chuyện với NPC)
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
