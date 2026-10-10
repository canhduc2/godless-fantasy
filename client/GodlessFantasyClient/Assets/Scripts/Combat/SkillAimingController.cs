using UnityEngine;
using GodlessFantasy.Indicator;

namespace GodlessFantasy.Combat
{
    public enum AimState
    {
        Idle,
        Aiming
    }

    /// <summary>
    /// Bộ điều khiển kỹ năng cơ chế Two-Tap (Vừa chạy né vừa ngắm):
    /// - Bấm 1 lần đầu: Vào trạng thái ngắm (hiện Indicator bám chuột).
    /// - Chuột trái: VẪN DI CHUYỂN BÌNH THƯỜNG (Kiting), không xác nhận skill và không tự hủy.
    /// - Chuột phải: Hủy chế độ ngắm lập tức (không mất Mana, không tính cooldown).
    /// - Bấm 1 lần nữa: Xác nhận phóng chiêu! Bắn đạn ma thuật bay theo hướng ngắm.
    /// </summary>
    public class SkillAimingController : MonoBehaviour
    {
        [Header("Skill 1 Configuration")]
        [SerializeField] private GameObject magicBoltPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float skill1Cooldown = 2.0f;
        [SerializeField] private int skill1Damage = 35;
        [SerializeField] private int skill1ManaCost = 15;

        [Header("References")]
        [SerializeField] private IndicatorRenderer indicatorRenderer;

        private AimState currentState = AimState.Idle;
        private int currentAimingSkill = 0; // 0 = none, 1 = skill 1
        private float lastSkill1CastTime = -999f;

        public AimState CurrentState => currentState;
        public bool IsAiming => currentState == AimState.Aiming;
        public float Skill1CooldownRemaining => Mathf.Max(0, (lastSkill1CastTime + skill1Cooldown) - Time.time);

        private void Awake()
        {
            if (indicatorRenderer == null) indicatorRenderer = GetComponentInChildren<IndicatorRenderer>();
            if (firePoint == null) firePoint = transform;
        }

        private void Update()
        {
            HandleSkillInput();
            HandleCancelInput();
        }

        private void HandleSkillInput()
        {
            // Bấm phím 1
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                if (currentState == AimState.Idle)
                {
                    // LẦN 1: Bật chế độ ngắm (nếu hết cooldown)
                    if (Time.time >= lastSkill1CastTime + skill1Cooldown)
                    {
                        EnterAimingState(1);
                    }
                    else
                    {
                        Debug.Log($"[SKILL 1] Đang hồi chiêu! Còn {Skill1CooldownRemaining:F1}s");
                    }
                }
                else if (currentState == AimState.Aiming && currentAimingSkill == 1)
                {
                    // LẦN 2: Bấm lại đúng phím 1 để XÁC NHẬN PHÓNG CHIÊU
                    ConfirmAndCastSkill1();
                }
            }
        }

        private void HandleCancelInput()
        {
            // Click chuột phải: Hủy ngắm ngay lập tức
            if (currentState == AimState.Aiming && Input.GetMouseButtonDown(1))
            {
                CancelAiming();
            }
        }

        private void EnterAimingState(int skillSlot)
        {
            currentState = AimState.Aiming;
            currentAimingSkill = skillSlot;

            if (indicatorRenderer != null)
            {
                indicatorRenderer.ShowIndicator();
            }

            Debug.Log($"[AIMING] Đã bật chế độ ngắm Skill {skillSlot}. Click chuột trái di chuyển né đòn thoải mái, bấm {skillSlot} lần nữa để xả chiêu, Chuột phải để hủy!");
        }

        private void ConfirmAndCastSkill1()
        {
            Vector2 aimDirection = indicatorRenderer != null ? indicatorRenderer.GetAimDirection() : Vector2.right;

            // Bắn đạn ma thuật
            if (magicBoltPrefab != null)
            {
                GameObject boltObj = Instantiate(magicBoltPrefab, firePoint.position, Quaternion.identity);
                if (boltObj.TryGetComponent<MagicBolt>(out var bolt))
                {
                    bolt.Initialize(aimDirection, skill1Damage, gameObject.name);
                }
            }
            else
            {
                Debug.LogWarning("[COMBAT] magicBoltPrefab chưa được gán vào Inspector!");
            }

            lastSkill1CastTime = Time.time;
            Debug.Log($"[CAST SUCCESS] Đã phóng chiêu Skill 1! Hướng: {aimDirection}, Sát thương: {skill1Damage}");

            ExitAimingState();
        }

        private void CancelAiming()
        {
            Debug.Log("[CANCEL] Đã hủy ngắm skill bằng chuột phải! Không mất Mana, không tính cooldown.");
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
