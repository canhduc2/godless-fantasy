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
    /// Bộ Điều Khiển Kỹ Năng MOBA Đa Dạng (Two-Tap Multi-Skill Controller):
    /// - Chiêu 1 [Phím 1]: Cầu Ma Thuật (Line Skillshot bắn thẳng).
    /// - Chiêu 2 [Phím 2]: Mưa Thiên Thạch (Ground AoE thả vòng tròn tại chuột).
    /// - Chiêu 3 [Phím 3]: Sóng Lửa (Cone Sweep quét hình quạt 60 độ).
    /// - Chiêu 4 [Phím 4]: Bộc Phá Thần Thánh (Self Nova nổ hào quang 360 độ).
    /// - Lướt [Shift / Space]: Lướt 360 độ né chiêu.
    /// Chuột Trái luôn dùng để Di chuyển né đòn (Kiting). Chuột Phải hủy ngắm lập tức!
    /// </summary>
    public class SkillAimingController : MonoBehaviour
    {
        [Header("Skill Specs")]
        [SerializeField] private GameObject magicBoltPrefab;
        [SerializeField] private Transform firePoint;

        [Header("Cooldowns")]
        [SerializeField] private float skill1Cd = 1.0f;
        [SerializeField] private float skill2Cd = 3.0f;
        [SerializeField] private float skill3Cd = 2.5f;
        [SerializeField] private float skill4Cd = 5.0f;

        [Header("Mana Costs")]
        [SerializeField] private int skill1Mana = 15;
        [SerializeField] private int skill2Mana = 25;
        [SerializeField] private int skill3Mana = 20;
        [SerializeField] private int skill4Mana = 35;

        [Header("Base Damages")]
        [SerializeField] private int skill1BaseDmg = 25;
        [SerializeField] private int skill2BaseDmg = 40;
        [SerializeField] private int skill3BaseDmg = 30;
        [SerializeField] private int skill4BaseDmg = 60;

        [Header("References")]
        [SerializeField] private IndicatorRenderer indicatorRenderer;
        [SerializeField] private PlayerStats playerStats;

        private AimState currentState = AimState.Idle;
        private int currentAimingSkill = 0; // 0=None, 1..4

        private float lastS1CastTime = -999f;
        private float lastS2CastTime = -999f;
        private float lastS3CastTime = -999f;
        private float lastS4CastTime = -999f;

        public bool IsAiming => currentState == AimState.Aiming;
        public int CurrentAimingSkill => currentAimingSkill;

        public float Skill1CooldownRemaining => Mathf.Max(0, (lastS1CastTime + skill1Cd) - Time.time);
        public float Skill2CooldownRemaining => Mathf.Max(0, (lastS2CastTime + skill2Cd) - Time.time);
        public float Skill3CooldownRemaining => Mathf.Max(0, (lastS3CastTime + skill3Cd) - Time.time);
        public float Skill4CooldownRemaining => Mathf.Max(0, (lastS4CastTime + skill4Cd) - Time.time);

        public int Skill1ManaCost => skill1Mana;
        public int Skill2ManaCost => skill2Mana;
        public int Skill3ManaCost => skill3Mana;
        public int Skill4ManaCost => skill4Mana;

        private void Awake()
        {
            if (indicatorRenderer == null) indicatorRenderer = GetComponentInChildren<IndicatorRenderer>();
            if (firePoint == null) firePoint = transform;
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            HandleSkillInput();
            HandleCancelInput();
        }

        private void HandleSkillInput()
        {
            // Phím 1
            if (GameInput.GetSkill1KeyDown()) HandleSkillPress(1, Skill1CooldownRemaining, skill1Mana);
            // Phím 2
            if (GameInput.GetSkill2KeyDown()) HandleSkillPress(2, Skill2CooldownRemaining, skill2Mana);
            // Phím 3
            if (GameInput.GetSkill3KeyDown()) HandleSkillPress(3, Skill3CooldownRemaining, skill3Mana);
            // Phím 4
            if (GameInput.GetSkill4KeyDown()) HandleSkillPress(4, Skill4CooldownRemaining, skill4Mana);
        }

        private void HandleSkillPress(int slot, float cdRemaining, int manaCost)
        {
            if (cdRemaining > 0.05f)
            {
                Debug.Log($"[SKILL {slot}] Đang hồi chiêu! Còn {cdRemaining:F1}s");
                return;
            }

            if (playerStats != null && playerStats.CurrentMana < manaCost)
            {
                Debug.LogWarning($"[SKILL {slot}] Không đủ Mana! (Cần {manaCost} Mana)");
                return;
            }

            if (currentState == AimState.Idle)
            {
                // BẤM LẦN 1: Bật chế độ ngắm
                EnterAimingState(slot);
            }
            else if (currentState == AimState.Aiming)
            {
                if (currentAimingSkill == slot)
                {
                    // BẤM LẦN 2 CÙNG PHÍM: XÁC NHẬN PHÓNG CHIÊU!
                    ConfirmAndCastSkill(slot);
                }
                else
                {
                    // Bấm phím chiêu khác -> Chuyển sang ngắm chiêu mới!
                    EnterAimingState(slot);
                }
            }
        }

        private void HandleCancelInput()
        {
            // Click Chuột Phải: Hủy ngắm ngay lập tức
            if (currentState == AimState.Aiming && GameInput.GetRightMouseButtonDown())
            {
                CancelAiming();
            }
        }

        private void EnterAimingState(int slot)
        {
            currentState = AimState.Aiming;
            currentAimingSkill = slot;

            if (indicatorRenderer != null)
            {
                indicatorRenderer.ShowIndicator(slot);
            }

            Debug.Log($"[AIMING] Đang ngắm Chiêu {slot}. Chuột trái né đòn, Bấm lại [{slot}] để xả chiêu, Chuột phải hủy!");
        }

        private void ConfirmAndCastSkill(int slot)
        {
            int manaCost = (slot == 1) ? skill1Mana : (slot == 2) ? skill2Mana : (slot == 3) ? skill3Mana : skill4Mana;

            if (playerStats != null && !playerStats.ConsumeMana(manaCost))
            {
                Debug.LogWarning($"[SKILL {slot}] Không đủ Mana để thi triển!");
                ExitAimingState();
                return;
            }

            int intel = playerStats != null ? playerStats.Intelligence : 12;

            switch (slot)
            {
                case 1:
                    CastSkill1_LineSkillshot(intel);
                    lastS1CastTime = Time.time;
                    break;
                case 2:
                    CastSkill2_GroundMeteor(intel);
                    lastS2CastTime = Time.time;
                    break;
                case 3:
                    CastSkill3_FlameCone(intel);
                    lastS3CastTime = Time.time;
                    break;
                case 4:
                    CastSkill4_DivineNova(intel);
                    lastS4CastTime = Time.time;
                    break;
            }

            ExitAimingState();
        }

        // ================= CHIÊU 1: CẦU MA THUẬT (LINE SKILLSHOT) =================
        private void CastSkill1_LineSkillshot(int intel)
        {
            Vector2 aimDir = indicatorRenderer != null ? indicatorRenderer.GetAimDirection() : Vector2.right;
            int dmg = skill1BaseDmg + intel; // 25 + 12 = 37

            Vector3 spawnPos = firePoint.position + (Vector3)(aimDir * 0.45f);
            if (magicBoltPrefab != null)
            {
                GameObject boltObj = Instantiate(magicBoltPrefab, spawnPos, Quaternion.identity);
                boltObj.SetActive(true);
                if (boltObj.TryGetComponent<MagicBolt>(out var bolt))
                {
                    bolt.Initialize(aimDir, dmg, gameObject.name);
                }
            }
            Debug.Log($"[CAST CHIÊU 1] Phóng Cầu Ma Thuật! Sát thương: {dmg}");
        }

        // ================= CHIÊU 2: MƯA THIÊN THẠCH (GROUND AOE CIRCLE) =================
        private void CastSkill2_GroundMeteor(int intel)
        {
            Vector3 targetPos = indicatorRenderer != null ? indicatorRenderer.GetGroundTargetPosition() : transform.position;
            int dmg = skill2BaseDmg + Mathf.RoundToInt(intel * 1.2f); // 40 + 14 = 54
            float aoeRadius = 2.2f;

            // Gây sát thương AoE lên mọi mục tiêu trong vòng tròn
            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, aoeRadius);
            int hitCount = 0;
            foreach (var col in hits)
            {
                if (col.gameObject == gameObject) continue;
                if (col.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(dmg, gameObject.name);
                    FloatingDamageManager.Spawn(col.transform.position, dmg, true);
                    hitCount++;
                }
            }

            Debug.Log($"[CAST CHIÊU 2] Thiên Thạch Ma Pháp nổ tại ({targetPos.x:F1}, {targetPos.y:F1})! Trúng {hitCount} mục tiêu. Sát thương: {dmg}");
        }

        // ================= CHIÊU 3: SÓNG LỬA HÌNH QUẠT (CONE SWEEP) =================
        private void CastSkill3_FlameCone(int intel)
        {
            Vector2 aimDir = indicatorRenderer != null ? indicatorRenderer.GetAimDirection() : Vector2.right;
            int dmg = skill3BaseDmg + Mathf.RoundToInt(intel * 0.8f); // 30 + 10 = 40
            float coneRange = 5.0f;
            float halfAngle = 35.0f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, coneRange);
            int hitCount = 0;
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
                        hitCount++;
                    }
                }
            }

            Debug.Log($"[CAST CHIÊU 3] Quét Sóng Lửa hình quạt! Trúng {hitCount} mục tiêu. Sát thương: {dmg}");
        }

        // ================= CHIÊU 4: BỘC PHÁ THẦN THÁNH (SELF NOVA AOE) =================
        private void CastSkill4_DivineNova(int intel)
        {
            int dmg = skill4BaseDmg + Mathf.RoundToInt(intel * 1.5f); // 60 + 18 = 78
            float novaRadius = 4.5f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, novaRadius);
            int hitCount = 0;
            foreach (var col in hits)
            {
                if (col.gameObject == gameObject) continue;
                if (col.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(dmg, gameObject.name);
                    FloatingDamageManager.Spawn(col.transform.position, dmg, true);
                    hitCount++;
                }
            }

            Debug.Log($"[CAST CHIÊU 4 - ULTIMATE] Bộc Phá Thần Thánh 360 độ! Trúng {hitCount} mục tiêu. Sát thương: {dmg}");
        }

        private void CancelAiming()
        {
            Debug.Log("[CANCEL] Đã hủy ngắm bằng chuột phải! Không mất Mana.");
            ExitAimingState();
        }

        private void ExitAimingState()
        {
            currentState = AimState.Idle;
            currentAimingSkill = 0;

            if (indicatorRenderer != null)
            {
                indicatorRenderer.HideIndicator();
            }
        }
    }
}
