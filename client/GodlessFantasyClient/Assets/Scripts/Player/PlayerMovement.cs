using System.Collections;
using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Bộ điều khiển di chuyển nhân vật:
    /// - Chuột trái: Click-to-Move (tự chạy đến vị trí click).
    /// - WASD: Ghi đè ngay lập tức để né đòn thủ công.
    /// - Shift: Dash nhanh theo hướng con trỏ chuột (Cooldown 3.5s).
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float stoppingDistance = 0.1f;

        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 16.0f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 3.5f;
        [SerializeField] private int dashManaCost = 20;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;

        private Vector2 targetPosition;
        private bool isMovingToTarget = false;
        private bool isDashing = false;
        private float lastDashTime = -999f;
        private Camera mainCamera;

        public bool IsDashing => isDashing;
        public float DashCooldownRemaining => Mathf.Max(0, (lastDashTime + dashCooldown) - Time.time);

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (isDashing) return;

            HandleKeyboardMovement();
            HandleMouseMovement();
            HandleDashInput();
        }

        private void FixedUpdate()
        {
            if (isDashing) return;

            if (isMovingToTarget)
            {
                Vector2 currentPos = rb.position;
                Vector2 direction = (targetPosition - currentPos);
                float distance = direction.magnitude;

                if (distance <= stoppingDistance)
                {
                    rb.linearVelocity = Vector2.zero;
                    isMovingToTarget = false;
                }
                else
                {
                    rb.linearVelocity = direction.normalized * moveSpeed;
                }
            }
        }

        private void HandleKeyboardMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal"); // A, D
            float vertical = Input.GetAxisRaw("Vertical");     // W, S

            Vector2 inputDir = new Vector2(horizontal, vertical).normalized;

            if (inputDir.sqrMagnitude > 0.01f)
            {
                // WASD ghi đè Click-to-Move ngay lập tức
                isMovingToTarget = false;
                rb.linearVelocity = inputDir * moveSpeed;
            }
            else if (!isMovingToTarget)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void HandleMouseMovement()
        {
            // Click chuột trái để di chuyển (kể cả khi đang ngắm skill, chuột trái vẫn ra lệnh chạy)
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                targetPosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
                isMovingToTarget = true;
            }
        }

        private void HandleDashInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                if (Time.time >= lastDashTime + dashCooldown)
                {
                    PerformDash();
                }
            }
        }

        private void PerformDash()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dashDirection = ((Vector2)mouseWorldPos - rb.position).normalized;

            if (dashDirection.sqrMagnitude < 0.01f)
            {
                dashDirection = Vector2.right;
            }

            StartCoroutine(DashRoutine(dashDirection));
        }

        private IEnumerator DashRoutine(Vector2 direction)
        {
            isDashing = true;
            isMovingToTarget = false;
            lastDashTime = Time.time;

            rb.linearVelocity = direction * dashSpeed;

            yield return new WaitForSeconds(dashDuration);

            rb.linearVelocity = Vector2.zero;
            isDashing = false;
        }

        public void StopMovement()
        {
            isMovingToTarget = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
}
