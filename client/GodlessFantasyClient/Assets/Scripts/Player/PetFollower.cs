using UnityEngine;
using GodlessFantasy.Core;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Thú Cưng Mini Đồng Hành Tiến Hóa 3 Giai Đoạn (Lively Companion Pet):
    /// - Không bao giờ đứng trước mặt người chơi làm vướng tầm nhìn.
    /// - Luôn bay/chạy ở 1 trong 2 bên hông (Left Wing / Right Wing) cách đều 0.85m chuẩn mực.
    /// - Cơ chế Random Side Switch: Cứ mỗi 4 - 8 giây, thú cưng lại ngẫu nhiên đổi bên (lúc lượn sang trái, lúc chạy sang phải) tạo cảm giác cực kỳ sinh động!
    /// - Tự động quay mặt (FlipX) về phía chủ nhân.
    /// - Nhấp nhô nhịp nhàng bồng bềnh siêu nhẹ tại chỗ.
    /// - Chuẩn 3-KHÔNG: Không đánh, Không cưỡi, Không bị chọn làm mục tiêu.
    /// 
    /// 🌟 HỆ THỐNG TIẾN HÓA 3 GIAI ĐOẠN (EVOLUTION MATRIX):
    /// - Giai Đoạn 1 (Lv.1 - 9): Sóc Chibi Sơ Sinh (Scale 0.95x, lăng xăng đáng yêu).
    /// - Giai Đoạn 2 (Lv.10 - 24): Linh Sóc Trưởng Thành (Scale 1.35x, lông phát sáng, TỰ ĐỘNG NHẶT ĐỒ bán kính 3.5m).
    /// - Giai Đoạn 3 (Lv.25 - 50): Thần Sóc Hoàng Kim (Scale 1.70x, hào quang rực rỡ, TỰ ĐỘNG NHẶT ĐỒ bán kính 5.0m, HỒI PHỤC ĐỊNH KỲ +5 HP / +3 MP mỗi 4s cho chủ nhân!).
    /// - Phím tắt [P]: Cho thú cưng ăn tăng trưởng nhanh!
    /// </summary>
    public class PetFollower : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform owner;
        [SerializeField] private float sideDistance = 0.85f;      // Khoảng cách bên hông chuẩn mực
        [SerializeField] private float followSmoothSpeed = 4.8f;   // Tốc độ di chuyển bám theo mượt mà
        [SerializeField] private float teleportDistance = 8.0f;    // Dịch chuyển tức thời khi quá xa

        [Header("Pet Progression & Evolution (Cấp Độ & Tiến Hóa)")]
        [SerializeField] private int petLevel = 1;
        [SerializeField] private int petExp = 0;
        private const int MaxPetLevel = 50;

        public int PetLevel => petLevel;
        public int PetExp => petExp;
        public int PetMaxExp => petLevel * 100;
        public float ExpProgress => Mathf.Clamp01((float)petExp / Mathf.Max(1, PetMaxExp));

        public int EvolutionStage => (petLevel >= 25) ? 3 : (petLevel >= 10) ? 2 : 1;
        public string StageName => (EvolutionStage == 3) ? "Thần Sóc Hoàng Kim" : (EvolutionStage == 2) ? "Linh Sóc Trưởng Thành" : "Sóc Chibi Sơ Sinh";
        public float TargetScale => (EvolutionStage == 3) ? 1.7f : (EvolutionStage == 2) ? 1.35f : 0.95f;

        public string BonusStatsDescription =>
            (EvolutionStage == 3) ? "+50 HP Tối Đa · +15 Giáp · +10 Công Phép · +0.5m/s Tốc Chạy · Hồi 5 HP/3 MP mỗi 4s · Auto-Loot 5.0m" :
            (EvolutionStage == 2) ? "+20 HP Tối Đa · +5 Giáp · +0.2m/s Tốc Chạy · Auto-Loot 3.5m" :
            "+5 HP Tối Đa";

        public float GetBonusMoveSpeed() => (EvolutionStage == 3) ? 0.5f : (EvolutionStage == 2) ? 0.2f : 0f;
        public int GetBonusDefense() => (EvolutionStage == 3) ? 15 : (EvolutionStage == 2) ? 5 : 0;

        // Timers
        private float autoLootTimer = 0f;
        private float guardianAuraTimer = 0f;

        // Visual Aura GameObject
        private GameObject goldenAuraObj;

        public void FeedPet(int expAmount = 50)
        {
            if (petLevel >= MaxPetLevel)
            {
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 0.9f, "✨ Pet đã đạt cấp độ tối đa (Lv.50)!", new Color(1f, 0.85f, 0.2f));
                Debug.Log("[PET] Thú cưng đã đạt cấp độ tối đa!");
                return;
            }

            int prevStage = EvolutionStage;
            petExp += expAmount;

            FloatingDamageManager.ShowFloatingText(
                transform.position + Vector3.up * 0.9f, $"💖 +{expAmount} EXP (Pet)", new Color(1f, 0.45f, 0.75f));

            while (petExp >= PetMaxExp && petLevel < MaxPetLevel)
            {
                petExp -= PetMaxExp;
                petLevel++;
                FloatingDamageManager.ShowFloatingText(
                    transform.position + Vector3.up * 1.3f, $"🎉 PET LÊN CẤP {petLevel}!", new Color(1f, 0.85f, 0.2f));
                AudioManager.PlayLevelUp();
            }

            // Kiểm tra Tiến Hóa sang hình thái mới
            if (EvolutionStage > prevStage)
            {
                FloatingDamageManager.ShowFloatingText(
                    transform.position + Vector3.up * 1.6f, $"✨ TIẾN HÓA: {StageName}!", new Color(1f, 0.95f, 0.4f));
                Debug.Log($"<color=yellow>[TIẾN HÓA THÚ CƯNG]</color> Thú cưng đã tiến hóa thành: {StageName}!");
                AudioManager.PlayPetEvolve();
            }

            UpdateScaleAndVisuals();
        }

        public void UpdateScaleAndVisuals()
        {
            transform.localScale = Vector3.one * TargetScale;
            if (spriteRenderer != null)
            {
                if (EvolutionStage == 3) spriteRenderer.color = new Color(1f, 0.92f, 0.65f); // Hoàng kim
                else if (EvolutionStage == 2) spriteRenderer.color = new Color(0.9f, 1f, 0.92f); // Linh thú sáng xanh ngọc
                else spriteRenderer.color = Color.white;
            }

            // Quản lý hào quang Hoàng Kim Stage 3
            if (EvolutionStage == 3)
            {
                if (goldenAuraObj == null)
                {
                    goldenAuraObj = new GameObject("Golden_Aura");
                    goldenAuraObj.transform.parent = transform;
                    goldenAuraObj.transform.localPosition = new Vector3(0, -0.05f, 0);
                    goldenAuraObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

                    var auraSr = goldenAuraObj.AddComponent<SpriteRenderer>();
                    auraSr.sprite = Resources.Load<Sprite>("indicator_circle");
                    auraSr.color = new Color(1f, 0.85f, 0.2f, 0.35f);
                    auraSr.sortingOrder = 7;
                }
                goldenAuraObj.SetActive(true);
            }
            else if (goldenAuraObj != null)
            {
                goldenAuraObj.SetActive(false);
            }
        }

        [Header("Lively Flank Switch (Đổi bên ngẫu nhiên)")]
        private float currentSideX = -1.0f;
        private float sideSwitchTimer = 5.0f;

        [Header("Subtle Gentle Floating")]
        [SerializeField] private float bobFrequency = 2.2f;
        [SerializeField] private float bobAmplitude = 0.025f;

        private SpriteRenderer spriteRenderer;
        private Vector3 currentBasePosition;

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
            if (owner != null)
            {
                currentSideX = (Random.value > 0.5f) ? 1.0f : -1.0f;
                currentBasePosition = owner.position + new Vector3(currentSideX * sideDistance, -0.15f, 0);
                transform.position = currentBasePosition;
                ResetSwitchTimer();
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 9; // Hiển thị dưới chân nhân vật (Layer 10)
            }
            currentBasePosition = transform.position;
            currentSideX = (Random.value > 0.5f) ? 1.0f : -1.0f;
            ResetSwitchTimer();
            UpdateScaleAndVisuals();
        }

        private void ResetSwitchTimer()
        {
            sideSwitchTimer = Random.Range(4.5f, 8.0f);
        }

        private void Update()
        {
            // 0. Phím nóng [P]: Cho thú cưng ăn
            if (GameInput.GetPetInteractKeyDown())
            {
                InteractWithPet();
            }

            if (owner == null) return;

            // 1. Logic đổi bên ngẫu nhiên theo thời gian
            sideSwitchTimer -= Time.deltaTime;
            if (sideSwitchTimer <= 0f)
            {
                ResetSwitchTimer();
                if (Random.value < 0.65f)
                {
                    currentSideX = -currentSideX;
                }
            }

            Vector3 ownerPos = owner.position;

            // 2. Vị trí mục tiêu: Luôn ở bên hông, KHÔNG BAO GIỜ đứng trước mặt làm vướng chuột
            Vector3 desiredBasePos = ownerPos + new Vector3(currentSideX * sideDistance, -0.15f, 0);
            float currentDist = Vector2.Distance(currentBasePosition, ownerPos);

            if (currentDist > teleportDistance)
            {
                currentBasePosition = desiredBasePos;
            }
            else
            {
                currentBasePosition = Vector3.Lerp(currentBasePosition, desiredBasePos, followSmoothSpeed * Time.deltaTime);
            }

            // 3. Lật Sprite thú cưng luôn quay mặt nhìn về phía chủ nhân
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (currentBasePosition.x < ownerPos.x);
            }

            // 4. Hiệu ứng nhấp nhô siêu nhẹ tại chỗ
            float bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = currentBasePosition + new Vector3(0, bobY, 0);

            // 5. Hiệu ứng xoay hào quang Stage 3
            if (goldenAuraObj != null && goldenAuraObj.activeSelf)
            {
                goldenAuraObj.transform.Rotate(0, 0, 45f * Time.deltaTime);
            }

            // 6. Tính năng Tiến Hóa: Tự Động Nhặt Đồ (Stage 2: 3.5m, Stage 3: 5.0m)
            HandleAutoLoot();

            // 7. Tính năng Tiến Hóa: Hào Quang Hộ Vệ Thần Hồi Phục (Stage 3)
            HandleGuardianAura();
        }

        private void InteractWithPet()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            InventoryItem food = null;
            if (inv != null)
            {
                food = inv.Items.Find(it => it.itemType == ItemType.Consumable && (it.id.Contains("meat") || it.id.Contains("apple") || it.id.Contains("food") || it.id.Contains("pot")));
            }

            if (food != null && inv.ConsumeItem(food.id, 1))
            {
                FeedPet(120);
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f, $"🍖 Đã cho ăn {food.itemName} (+120 EXP)", new Color(1f, 0.75f, 0.2f));
            }
            else
            {
                FeedPet(45);
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f, $"❤️ Vuốt ve thú cưng (+45 EXP) [P]", new Color(1f, 0.5f, 0.8f));
            }
        }

        private void HandleAutoLoot()
        {
            if (EvolutionStage < 2) return;

            autoLootTimer += Time.deltaTime;
            if (autoLootTimer < 0.75f) return;
            autoLootTimer = 0f;

            float scanRadius = (EvolutionStage == 3) ? 5.0f : 3.5f;
            var allLoot = FindObjectsByType<GroundLootItem>();

            foreach (var loot in allLoot)
            {
                if (loot == null) continue;
                float d = Vector2.Distance(transform.position, loot.transform.position);
                if (d <= scanRadius)
                {
                    string lootName = loot.itemName;
                    loot.TryPickup();
                    FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.1f, $"🐾 {StageName} nhặt: {lootName}!", new Color(0.4f, 0.95f, 0.6f));
                    break;
                }
            }
        }

        private void HandleGuardianAura()
        {
            if (EvolutionStage < 3) return;

            guardianAuraTimer += Time.deltaTime;
            if (guardianAuraTimer < 4.0f) return;
            guardianAuraTimer = 0f;

            if (owner == null) return;
            var ownerHealth = owner.GetComponent<Health>();
            var ownerStats = owner.GetComponent<PlayerStats>();

            bool healed = false;
            if (ownerHealth != null && ownerHealth.CurrentHealth < ownerHealth.MaxHealth)
            {
                ownerHealth.Heal(5);
                healed = true;
            }

            if (ownerStats != null && ownerStats.CurrentMana < ownerStats.MaxMana)
            {
                ownerStats.RestoreMana(3);
                healed = true;
            }

            if (healed)
            {
                FloatingDamageManager.ShowFloatingText(owner.position + Vector3.up * 1.4f, "🛡️ Thần Sóc Bảo Hộ (+5 HP, +3 MP)", new Color(1f, 0.90f, 0.35f));
            }
        }
    }
}
