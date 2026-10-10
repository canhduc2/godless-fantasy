using System.Collections;
using UnityEngine;
using GodlessFantasy.Core;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Bộ điều khiển di chuyển nhân vật:
    /// - Chuột trái: Click-to-Move (tự chạy đến vị trí click).
    /// - WASD: Ghi đè ngay lập tức để né đòn thủ công.
    /// - Shift: Dash nhanh theo hướng con trỏ chuột (Cooldown 3.5s).
    /// - Hỗ trợ cả New Input System và Legacy Input mượt mà!
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float stoppingDistance = 0.15f;

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

        [SerializeField] private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            mainCamera = Camera.main;
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (isDashing) return;

            // Độc lập hướng ngắm: Nhân vật luôn hướng mặt theo con trỏ chuột độc lập với hướng di chuyển
            if (spriteRenderer != null)
            {
                if (mainCamera == null) mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
                    spriteRenderer.flipX = (mouseWorld.x < transform.position.x);
                }
            }

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
            Vector2 inputDir = GameInput.GetMovementVector();

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

        public static bool BlockWorldClick = false;

        public void MoveTo(Vector2 worldPos)
        {
            targetPosition = worldPos;
            isMovingToTarget = true;
        }

        private void HandleMouseMovement()
        {
            if (BlockWorldClick) return;

            // Click chuột trái để di chuyển (kể cả khi đang ngắm skill, chuột trái vẫn ra lệnh chạy)
            if (GameInput.GetLeftMouseButtonDown())
            {
                if (mainCamera == null) mainCamera = Camera.main;
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
                targetPosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
                isMovingToTarget = true;
            }
        }

        private void HandleDashInput()
        {
            if (GameInput.GetDashKeyDown())
            {
                if (Time.time >= lastDashTime + dashCooldown)
                {
                    PerformDash();
                }
            }
        }

        private void PerformDash()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
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
