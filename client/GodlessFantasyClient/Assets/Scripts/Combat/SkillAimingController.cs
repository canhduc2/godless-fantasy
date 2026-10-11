using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Indicator;
using GodlessFantasy.Core;
using GodlessFantasy.Player;

namespace GodlessFantasy.Combat
{
    public enum AimState
    {
        Idle,
        Aiming
    }

    /// <summary>
    /// Bộ Điều Khiển Kỹ Năng & Cuộn Trục (Skill Scroll & Aiming Controller):
    /// - Mỗi kỹ năng là một Cuộn Trục Kỹ Năng (Bí Kíp) thực thụ thuộc hành trang.
    /// - Người chơi có thể KÉO THẢ (Drag & Drop) cuộn trục giữa túi đồ và 4 ô phím tắt [1], [2], [3], [4]!
    /// - Khi đổi cuộn trục, biểu tượng, sát thương, tầm ngắm và cooldown của ô đó tự động cập nhật.
    /// 
    /// </summary>
    public class SkillAimingController : MonoBehaviour
    {
        [Header("Skill Specs")]
        [SerializeField] private GameObject magicBoltPrefab;
        [SerializeField] private Transform firePoint;

        [Header("References")]
        [SerializeField] private IndicatorRenderer indicatorRenderer;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Health playerHealth;

        // 4 Ô Cuộn Trục Kỹ Năng Đang Trang Bị (Hotbar Slots 1..4)
        [SerializeField] private SkillScrollData[] equippedSlots = new SkillScrollData[4];

        // Kho Bí Kíp Kỹ Năng trong Hành Trang
        public List<SkillScrollData> scrollInventory = new List<SkillScrollData>();

        // Thời gian thi triển gần nhất của 4 ô
        private float[] slotLastCastTime = new float[4] { -999f, -999f, -999f, -999f };

        private AimState currentState = AimState.Idle;
        private int currentAimingSlot = 0; // 1..4, 0=None

        public bool IsAiming => currentState == AimState.Aiming;
        public int CurrentAimingSkill => currentAimingSlot;

        // Backward Compatibility Getters
        public float Skill1CooldownRemaining => GetCooldownRemaining(0);
        public float Skill2CooldownRemaining => GetCooldownRemaining(1);
        public float Skill3CooldownRemaining => GetCooldownRemaining(2);
        public float Skill4CooldownRemaining => GetCooldownRemaining(3);

        public int Skill1ManaCost => equippedSlots[0] != null ? equippedSlots[0].manaCost : 15;
        public int Skill2ManaCost => equippedSlots[1] != null ? equippedSlots[1].manaCost : 25;
        public int Skill3ManaCost => equippedSlots[2] != null ? equippedSlots[2].manaCost : 20;
        public int Skill4ManaCost => equippedSlots[3] != null ? equippedSlots[3].manaCost : 35;

        private bool isDefaultScrollsInitialized = false;

        private void Awake()
        {
            if (indicatorRenderer == null) indicatorRenderer = GetComponentInChildren<IndicatorRenderer>();
            if (firePoint == null) firePoint = transform;
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (playerHealth == null) playerHealth = GetComponent<Health>();

            InitializeDefaultScrolls();
        }

        public void InitializeDefaultScrolls()
        {
            if (isDefaultScrollsInitialized) return;
            isDefaultScrollsInitialized = true;

            // Khởi tạo 4 Bí Kíp mặc định gắn vào 4 ô
            if (equippedSlots[0] == null)
            {
                equippedSlots[0] = new SkillScrollData(
                    "scroll_fire_slash", "Bí Kíp [Hệ Kị Sĩ]: Hỏa Kiếm Khí", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "Hỏa",
                    15, 1.0f, 25, 1.0f, 1, "icon_skill_flame_cone", "item_scroll_fire",
                    "Phóng kiếm khí hỏa diễm theo đường thẳng. Tiêu hao ít mana, hồi nhanh.", 8.0f, "Kị Sĩ");
            }

            if (equippedSlots[1] == null)
            {
                equippedSlots[1] = new SkillScrollData(
                    "scroll_fire_ground", "Bí Kíp [Hệ Kị Sĩ]: Liệt Hỏa Oanh Kích", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "Hỏa",
                    25, 3.0f, 40, 1.2f, 2, "icon_skill_meteor", "item_scroll_fire",
                    "Giáng luồng bão lửa thiêu rụi vùng đất chỉ định gây nổ diện rộng.", 7.0f, "Kị Sĩ");
            }

            if (equippedSlots[2] == null)
            {
                equippedSlots[2] = new SkillScrollData(
                    "scroll_fire_sweep", "Bí Kíp [Hệ Kị Sĩ]: Hỏa Long Toàn Phong", "Phẩm B", new Color(0.85f, 0.45f, 1.0f), "Hỏa",
                    20, 2.5f, 30, 0.8f, 3, "icon_skill_flame_cone", "item_scroll_fire",
                    "Quét bão lửa hình quạt 60 độ về phía trước thiêu rụi nhiều mục tiêu.", 4.5f, "Kị Sĩ");
            }

            if (equippedSlots[3] == null)
            {
                equippedSlots[3] = new SkillScrollData(
                    "scroll_fire_buff", "Bí Kíp [Hệ Kị Sĩ]: Hỏa Long Kim Cang", "Phẩm A", new Color(1.0f, 0.65f, 0.2f), "Hỏa",
                    30, 5.0f, 50, 1.5f, 5, "icon_skill_divine_nova", "item_scroll_fire",
                    "Kích hoạt hào quang rồng lửa hộ thể tăng giáp và công phép.", 0.0f, "Kị Sĩ", "", 3);
            }

            // Kho bí kíp trong Túi Đồ để người chơi trải nghiệm Kéo Thả (Drag & Drop)
            if (scrollInventory.Count == 0)
            {
                scrollInventory.Add(new SkillScrollData(
                    "scroll_frost_arrow", "Cuộn Trục: Băng Huyết Tiễn", "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "Băng",
                    22, 2.0f, 48, 1.3f, 1, "item_scroll_frost", "item_scroll_frost",
                    "Bắn mũi tên băng giá cực nhanh, gây sát thương băng và làm tê liệt kẻ địch."));

                scrollInventory.Add(new SkillScrollData(
                    "scroll_thunder_burst", "Cuộn Trục: Lôi Thần Nộ", "Phẩm S", new Color(1.0f, 0.25f, 0.25f), "Lôi",
                    32, 4.0f, 55, 1.4f, 2, "item_scroll_generic", "item_scroll_generic",
                    "Gọi tia sét thần sấm oanh tạc vùng đất chỉ định, giáng đòn chí mạng uy lực."));
            }
        }

        public SkillScrollData GetEquippedScroll(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 4) return null;
            if (!isDefaultScrollsInitialized) InitializeDefaultScrolls();
            return equippedSlots[slotIndex];
        }

        public void EquipScroll(int slotIndex, SkillScrollData scroll)
        {
            if (slotIndex < 0 || slotIndex >= 4) return;
            equippedSlots[slotIndex] = scroll;
            slotLastCastTime[slotIndex] = -999f; // Reset CD khi lắp mới
            Debug.Log($"<color=yellow>[KÉO THẢ KỸ NĂNG] Đã gắn '{scroll?.scrollName}' vào Ô [{slotIndex + 1}]!</color>");
        }

        public void SwapScrolls(int slotA, int slotB)
        {
            if (slotA < 0 || slotA >= 4 || slotB < 0 || slotB >= 4) return;
            var temp = equippedSlots[slotA];
            equippedSlots[slotA] = equippedSlots[slotB];
            equippedSlots[slotB] = temp;
            Debug.Log($"<color=yellow>[HOÁN ĐỔI KỸ NĂNG] Đã đổi vị trí Ô [{slotA + 1}] và Ô [{slotB + 1}]!</color>");
        }

        public void UnequipScroll(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 4) return;
            var removed = equippedSlots[slotIndex];
            if (removed != null && !scrollInventory.Contains(removed))
            {
                scrollInventory.Add(removed);
            }
            equippedSlots[slotIndex] = null;
        }

        public float GetCooldownRemaining(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 4) return 0f;
            var scroll = equippedSlots[slotIndex];
            if (scroll == null) return 0f;
            return Mathf.Max(0, (slotLastCastTime[slotIndex] + scroll.cooldown) - Time.time);
        }

        private void Update()
        {
            HandleSkillInput();
            HandleCancelInput();
        }

        private void HandleSkillInput()
        {
            if (GameInput.GetSkill1KeyDown()) HandleSkillPress(1);
            if (GameInput.GetSkill2KeyDown()) HandleSkillPress(2);
            if (GameInput.GetSkill3KeyDown()) HandleSkillPress(3);
            if (GameInput.GetSkill4KeyDown()) HandleSkillPress(4);
        }

        private void HandleSkillPress(int slot)
        {
            int slotIdx = slot - 1;
            SkillScrollData scroll = equippedSlots[slotIdx];

            if (scroll == null)
            {
                Debug.LogWarning($"[Ô {slot} TRỐNG] Hãy mở Menu Túi Đồ/Kỹ Năng để kéo cuộn trục vào ô này!");
                return;
            }

            // KIỂM TRA ĐIỀU KIỆN KÉP NGHIÊM NGẶT: HỆ -> HỆ, THUỘC TÍNH -> THUỘC TÍNH
            string pClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
            string pElem = playerStats != null ? playerStats.Element : "Hỏa";
            if (!scroll.CanPlayerUse(pClass, pElem, out string lockReason))
            {
                Debug.LogWarning($"[KHÓA KỸ NĂNG] Ô [{slot}]: {lockReason}");
                Vector3 spawnPos = playerStats != null ? playerStats.transform.position + Vector3.up * 1.5f : transform.position + Vector3.up * 1.5f;
                FloatingDamageManager.Instance?.SpawnFloatingText(spawnPos, $"⛔ {lockReason}", new Color(1f, 0.35f, 0.35f));
                return;
            }

            float cdRemaining = GetCooldownRemaining(slotIdx);
            if (cdRemaining > 0.05f)
            {
                Debug.Log($"[Ô {slot}] {scroll.scrollName} đang hồi chiêu! Còn {cdRemaining:F1}s");
                return;
            }

            if (playerStats != null && playerStats.CurrentMana < scroll.manaCost)
            {
                Debug.LogWarning($"[Ô {slot}] Không đủ Mana! (Cần {scroll.manaCost} Mana)");
                return;
            }

            // Archetype 5: Buff Tự Thân - Kích hoạt ngay lập tức mà không cần ngắm
            if (scroll.indicatorType == 5)
            {
                ConfirmAndCastSkill(slot);
                return;
            }

            if (currentState == AimState.Idle)
            {
                EnterAimingState(slot);
            }
            else if (currentState == AimState.Aiming)
            {
                if (currentAimingSlot == slot)
                {
                    // Cơ chế đảo ngược theo yêu cầu: Ấn tiếp lần 2 cùng phím là HỦY NGẮM
                    CancelAiming();
                }
                else
                {
                    EnterAimingState(slot);
                }
            }
        }

        private void HandleCancelInput()
        {
            if (currentState == AimState.Aiming)
            {
                // Chuột Phải: TUNG CHIÊU
                if (GameInput.GetRightMouseButtonDown())
                {
                    ConfirmAndCastSkill(currentAimingSlot);
                }
                // ESC: Hủy ngắm
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelAiming();
                }
            }
        }

        private void EnterAimingState(int slot)
        {
            int slotIdx = slot - 1;
            SkillScrollData scroll = equippedSlots[slotIdx];
            if (scroll == null) return;

            currentState = AimState.Aiming;
            currentAimingSlot = slot;

            if (indicatorRenderer != null)
            {
                indicatorRenderer.ShowIndicator(scroll.indicatorType);
            }

            Debug.Log($"[AIMING] Đang ngắm {scroll.scrollName} [Ô {slot}]. Chuột Phải để tung chiêu · Bấm [{slot}] hoặc ESC để hủy!");
        }

        private void ConfirmAndCastSkill(int slot)
        {
            int slotIdx = slot - 1;
            SkillScrollData scroll = equippedSlots[slotIdx];
            if (scroll == null)
            {
                ExitAimingState();
                return;
            }

            if (playerStats != null && !playerStats.ConsumeMana(scroll.manaCost))
            {
                Debug.LogWarning($"[Ô {slot}] Không đủ Mana để thi triển {scroll.scrollName}!");
                ExitAimingState();
                return;
            }

            int intel = playerStats != null ? playerStats.Intelligence : 12;
            float buffMult = (playerStats != null) ? playerStats.GetDamageBuffMultiplier() : 0f;
            int totalDamage = Mathf.RoundToInt((scroll.baseDamage + Mathf.RoundToInt(intel * scroll.intelligenceScaling)) * (1f + buffMult));

            // Thi triển theo Archetype của Cuộn Trục
            switch (scroll.indicatorType)
            {
                case 1:
                    CastLineSkillshot(scroll, totalDamage);
                    break;
                case 2:
                    CastGroundAoE(scroll, totalDamage);
                    break;
                case 3:
                    CastConeSweep(scroll, totalDamage);
                    break;
                case 4:
                    CastSelfNova(scroll, totalDamage);
                    break;
                case 5:
                    CastSelfBuff(scroll);
                    break;
                case 6:
                    CastTeamBuffAoE(scroll);
                    break;
            }

            slotLastCastTime[slotIdx] = Time.time;
            ExitAimingState();
        }

        // 6. Buff Diện Rộng Toàn Đội / Bang Hội 360 Độ (AoE Team Buff - Bán kính 6.5m)
        private void CastTeamBuffAoE(SkillScrollData scroll)
        {
            float radius = 6.5f;

            if (SkillVFXManager.Instance != null)
            {
                SkillVFXManager.Instance.SpawnTeamBuffAoEVFX(transform.position, radius, scroll.element);
            }

            string cleanName = scroll.scrollName;
            if (cleanName.Contains(":")) cleanName = cleanName.Substring(cleanName.IndexOf(':') + 1).Trim();
            else if (cleanName.Contains("]")) cleanName = cleanName.Substring(cleanName.IndexOf(']') + 1).Trim();

            // Buff cho bản thân người dùng
            if (playerStats != null)
            {
                playerStats.ApplyBuff(scroll.id, cleanName, scroll.iconName, 18.0f, 1, 20f);
            }
            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.1f, $"👑 {cleanName} (+20% ST Đội)!", new Color(1f, 0.88f, 0.25f));

            // Quét và áp dụng hiệu ứng cho tất cả đồng đội / thành viên Bang Hội trong phạm vi 6.5m cùng map
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var col in hits)
            {
                if (col.gameObject == gameObject) continue;
                if (col.CompareTag("Player") || col.gameObject.name.Contains("Player") || col.gameObject.name.Contains("Pet"))
                {
                    if (col.TryGetComponent<PlayerStats>(out var allyStats))
                    {
                        allyStats.ApplyBuff(scroll.id, cleanName, scroll.iconName, 18.0f, 1, 20f);
                    }
                    FloatingDamageManager.ShowFloatingText(col.transform.position + Vector3.up * 1.1f, $"🛡️ {cleanName} (+20% ST)!", new Color(1f, 0.88f, 0.25f));
                }
            }

            Debug.Log($"<color=yellow>[TEAM BUFF AOE]</color> Đã kích hoạt Buff Đồng Đội '{scroll.scrollName}' ({scroll.element}) bán kính {radius}m!");
        }

        private void CastSelfBuff(SkillScrollData scroll)
        {
            if (SkillVFXManager.Instance != null)
            {
                SkillVFXManager.Instance.SpawnSelfBuffVFX(transform.position, scroll.element);
            }

            if (scroll.skillCategory == 1) // Hồi Máu
            {
                if (playerHealth != null) playerHealth.Heal(scroll.baseDamage);
                FloatingDamageManager.SpawnHeal(transform.position, scroll.baseDamage, false);
            }
            else if (scroll.skillCategory == 2) // Hồi Mana
            {
                if (playerStats != null) playerStats.RestoreMana(scroll.baseDamage);
                FloatingDamageManager.SpawnHeal(transform.position, scroll.baseDamage, true);
            }
            else // Cường Hóa (Buff Stat: +25% ST Đánh Thường & Kỹ Năng)
            {
                string cleanName = scroll.scrollName;
                if (cleanName.Contains(":")) cleanName = cleanName.Substring(cleanName.IndexOf(':') + 1).Trim();
                else if (cleanName.Contains("]")) cleanName = cleanName.Substring(cleanName.IndexOf(']') + 1).Trim();

                if (playerStats != null)
                {
                    playerStats.ApplyBuff(scroll.id, cleanName, scroll.iconName, 15.0f, 1, 25f);
                }
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 0.9f, $"🔥 {cleanName} (+25% ST)!", new Color(1f, 0.75f, 0.2f));
            }
            Debug.Log($"<color=cyan>[BUFF CAST]</color> Đã kích hoạt Buff '{scroll.scrollName}' ({scroll.element})!");
        }

        // 1. Phóng Đạn Tuyến Tính (Line Skillshot)
        private void CastLineSkillshot(SkillScrollData scroll, int dmg)
        {
            Vector2 aimDir = indicatorRenderer != null ? indicatorRenderer.GetAimDirection() : Vector2.right;
            Vector3 spawnPos = firePoint != null ? firePoint.position + (Vector3)(aimDir * 0.45f) : transform.position + (Vector3)(aimDir * 0.45f);

            if (SkillVFXManager.Instance != null)
            {
                SkillVFXManager.Instance.SpawnLineProjectile(spawnPos, aimDir, dmg, scroll, gameObject.name);
            }
            else if (magicBoltPrefab != null)
            {
                GameObject boltObj = Instantiate(magicBoltPrefab, spawnPos, Quaternion.identity);
                boltObj.SetActive(true);
                if (boltObj.TryGetComponent<MagicBolt>(out var bolt))
                {
                    bolt.Initialize(aimDir, dmg, gameObject.name);
                }
            }
            Debug.Log($"[CAST] Phóng '{scroll.scrollName}'! Sát thương: {dmg}");
        }

        // 2. Thả Vòng Tròn Đất (Ground AoE / Rain Effect - Bán kính 1.9m)
        private void CastGroundAoE(SkillScrollData scroll, int dmg)
        {
            Vector3 targetPos = indicatorRenderer != null ? indicatorRenderer.GetGroundTargetPosition() : transform.position;
            float aoeRadius = 1.9f;

            if (scroll.IsSupport)
            {
                // HỖ TRỢ / HỒI MÁU / HỒI MANA / BUFF ĐỒNG ĐỘI (Ví dụ: Mưa Sinh Mệnh Hệ Mộc)
                if (SkillVFXManager.Instance != null)
                {
                    SkillVFXManager.Instance.SpawnHealingRain(targetPos, aoeRadius, 2.0f, scroll.element);
                }

                // Nếu người chơi đứng trong vùng mưa
                if (Vector2.Distance(transform.position, targetPos) <= aoeRadius)
                {
                    ApplySupportEffect(scroll, dmg, gameObject);
                }

                // Quét đồng minh / đồng đội trong vùng
                Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, aoeRadius);
                foreach (var col in hits)
                {
                    if (col.gameObject == gameObject) continue;
                    if (col.CompareTag("Player") || col.gameObject.name.Contains("Player") || col.gameObject.name.Contains("Pet"))
                    {
                        ApplySupportEffect(scroll, dmg, col.gameObject);
                    }
                }
                Debug.Log($"<color=green>[HEALING RAIN / SUPPORT]</color> Kích hoạt mưa hỗ trợ '{scroll.scrollName}' tại ({targetPos.x:F1}, {targetPos.y:F1})!");
            }
            else
            {
                if (SkillVFXManager.Instance != null)
                {
                    SkillVFXManager.Instance.SpawnGroundAoEImpact(targetPos, aoeRadius, dmg, scroll, gameObject.name);
                }
                else
                {
                    Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, aoeRadius);
                    foreach (var col in hits)
                    {
                        if (col.gameObject == gameObject) continue;
                        if (col.TryGetComponent<IDamageable>(out var target))
                        {
                            target.TakeDamage(dmg, gameObject.name);
                            FloatingDamageManager.Spawn(col.transform.position, dmg, true);
                        }
                    }
                }
                Debug.Log($"[CAST] Thả '{scroll.scrollName}' tại ({targetPos.x:F1}, {targetPos.y:F1})! Bán kính: {aoeRadius}m. Sát thương: {dmg}");
            }
        }

        // 3. Quét Sóng Lửa Hình Quạt (Cone Sweep - Tầm 4.5m, góc 60 độ khớp 100% Visual Indicator)
        private void CastConeSweep(SkillScrollData scroll, int dmg)
        {
            Vector2 aimDir = indicatorRenderer != null ? indicatorRenderer.GetAimDirection() : Vector2.right;
            float coneRange = 4.5f;
            float halfAngle = 30.0f;
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

            if (SkillVFXManager.Instance != null)
            {
                SkillVFXManager.Instance.SpawnConeSweep(spawnPos, aimDir, coneRange, halfAngle, dmg, scroll, gameObject.name);
            }
            else
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, coneRange);
                foreach (var col in hits)
                {
                    if (col.gameObject == gameObject) continue;
                    Vector2 toTarget = (col.transform.position - transform.position);
                    float angle = Vector2.Angle(aimDir, toTarget.normalized);

                    if (angle <= halfAngle)
                    {
                        if (col.TryGetComponent<IDamageable>(out var target))
                        {
                            target.TakeDamage(dmg, gameObject.name);
                            FloatingDamageManager.Spawn(col.transform.position, dmg, false);
                        }
                    }
                }
            }
            Debug.Log($"[CAST] Quét '{scroll.scrollName}' hình quạt! Sát thương: {dmg}");
        }

        // 4. Bộc Phá Hào Quang 360 Độ (Self Nova - Bán kính chuẩn 2.5m)
        private void CastSelfNova(SkillScrollData scroll, int dmg)
        {
            float novaRadius = 2.5f;

            if (scroll.IsSupport)
            {
                if (SkillVFXManager.Instance != null)
                {
                    SkillVFXManager.Instance.SpawnSelfNovaVFX(transform.position, scroll.element);
                }
                ApplySupportEffect(scroll, dmg, gameObject);

                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, novaRadius);
                foreach (var col in hits)
                {
                    if (col.gameObject == gameObject) continue;
                    if (col.CompareTag("Player") || col.gameObject.name.Contains("Player") || col.gameObject.name.Contains("Pet"))
                    {
                        ApplySupportEffect(scroll, dmg, col.gameObject);
                    }
                }
                Debug.Log($"<color=cyan>[SUPPORT NOVA]</color> Hào quang hỗ trợ '{scroll.scrollName}' 360 độ quanh người!");
                return;
            }

            if (SkillVFXManager.Instance != null)
            {
                SkillVFXManager.Instance.SpawnSelfNova(transform.position, novaRadius, dmg, scroll, gameObject.name);
            }
            else
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, novaRadius);
                foreach (var col in hits)
                {
                    if (col.gameObject == gameObject) continue;
                    if (col.TryGetComponent<IDamageable>(out var target))
                    {
                        target.TakeDamage(dmg, gameObject.name);
                        FloatingDamageManager.Spawn(col.transform.position, dmg, true);
                    }
                }
            }
            Debug.Log($"[CAST] Nổ '{scroll.scrollName}' 360 độ quanh người! Sát thương: {dmg}");
        }

        private void ApplySupportEffect(SkillScrollData scroll, int amount, GameObject targetObj)
        {
            if (scroll.skillCategory == 1) // Hồi Máu
            {
                if (targetObj.TryGetComponent<Health>(out var h))
                {
                    h.Heal(amount);
                    FloatingDamageManager.SpawnHeal(targetObj.transform.position, amount, false);
                }
            }
            else if (scroll.skillCategory == 2) // Hồi Mana
            {
                if (targetObj.TryGetComponent<PlayerStats>(out var ps))
                {
                    ps.RestoreMana(amount);
                    FloatingDamageManager.SpawnHeal(targetObj.transform.position, amount, true);
                }
            }
            else if (scroll.skillCategory == 3) // Buff Chỉ Số
            {
                if (targetObj.TryGetComponent<PlayerStats>(out var ps))
                {
                    ps.ApplyBuff(scroll.id, scroll.scrollName, scroll.iconName, 15f, 1, amount);
                    FloatingDamageManager.Instance?.SpawnFloatingText(targetObj.transform.position, $"BUFF: {scroll.scrollName}", Color.cyan);
                }
            }
        }

        private void CancelAiming()
        {
            Debug.Log("[CANCEL] Đã hủy ngắm chiêu! Không mất Mana.");
            ExitAimingState();
        }

        private void ExitAimingState()
        {
            currentState = AimState.Idle;
            currentAimingSlot = 0;

            if (indicatorRenderer != null)
            {
                indicatorRenderer.HideIndicator();
            }
        }
    }
}
