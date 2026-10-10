using System.Collections;
using UnityEngine;
using GodlessFantasy.Core;
using GodlessFantasy.UI;

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
        [SerializeField] private int dashManaCost = 10;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PlayerStats playerStats;

        private Vector2 targetPosition;
        private bool isMovingToTarget = false;
        private bool isDashing = false;
        private float lastDashTime = -999f;
        private Camera mainCamera;

        // HIỆU ỨNG RIPPLE VỊ TRÍ CLICK-TO-MOVE
        private Vector2 clickMarkerWorldPos;
        private float clickMarkerTimer = 0f;
        private const float CLICK_MARKER_DURATION = 0.45f;

        public float CurrentDashCooldown
        {
            get
            {
                int agi = (playerStats != null) ? playerStats.Agility : 9;
                // Mỗi điểm AGI giảm 0.1s; giảm tối đa 1.0s (không bao giờ tụt quá 1s so với 3.5s gốc, không tụt về 0.9s, 0.8s, 0.7s...)
                float reduction = Mathf.Min(1.0f, Mathf.Max(0, agi - 9) * 0.1f);
                return dashCooldown - reduction;
            }
        }

        public bool IsDashing => isDashing;
        public float DashCooldownRemaining => Mathf.Max(0, (lastDashTime + CurrentDashCooldown) - Time.time);

        [SerializeField] private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            mainCamera = Camera.main;
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (clickMarkerTimer > 0f) clickMarkerTimer -= Time.deltaTime;
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
            clickMarkerWorldPos = worldPos;
            clickMarkerTimer = CLICK_MARKER_DURATION;
        }

        public void SetTargetPosition(Vector2 worldPos)
        {
            targetPosition = worldPos;
            isMovingToTarget = true;
        }

        public void StopMoving()
        {
            isMovingToTarget = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        private void HandleMouseMovement()
        {
            if (BlockWorldClick) return;

            // Click chuột trái để di chuyển
            if (GameInput.GetLeftMouseButtonDown())
            {
                if (mainCamera == null) mainCamera = Camera.main;
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());

                // Nếu click trúng quái vật / mục tiêu tấn công thì nhường quyền xử lý cho BasicAttack
                Collider2D hit = Physics2D.OverlapPoint(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
                if (hit != null && hit.gameObject != gameObject && (hit.GetComponent<Combat.IDamageable>() != null || hit.GetComponent<Combat.MonsterTarget>() != null))
                {
                    return;
                }

                targetPosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
                isMovingToTarget = true;
                clickMarkerWorldPos = targetPosition;
                clickMarkerTimer = CLICK_MARKER_DURATION;
            }
        }

        private void HandleDashInput()
        {
            if (GameInput.GetDashKeyDown())
            {
                if (Time.time >= lastDashTime + CurrentDashCooldown)
                {
                    if (playerStats == null) playerStats = GetComponent<PlayerStats>();
                    if (playerStats != null && !playerStats.ConsumeMana(dashManaCost))
                    {
                        Debug.Log($"<color=yellow>[DASH]</color> Không đủ {dashManaCost} Mana để lướt!");
                        return;
                    }
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

        // ================= VẼ HIỆU ỨNG RIPPLE TẠI ĐIỂM CLICK ĐẾN =================
        private void OnGUI()
        {
            if (clickMarkerTimer > 0f)
            {
                if (mainCamera == null) mainCamera = Camera.main;
                if (mainCamera == null) return;

                Vector3 sp = mainCamera.WorldToScreenPoint(clickMarkerWorldPos);
                if (sp.z > 0)
                {
                    GUI.depth = FantasyUITheme.DepthOverhead;
                    float progress = 1f - (clickMarkerTimer / CLICK_MARKER_DURATION);
                    float alpha = Mathf.Clamp01(clickMarkerTimer / CLICK_MARKER_DURATION);
                    float curY = Screen.height - sp.y;

                    // Vòng sóng lan tỏa mở rộng (Cyan ngọc)
                    float ring1Radius = 8f + progress * 22f;
                    Color ringColor = new Color(0.1f, 0.9f, 1f, alpha * 0.9f);
                    DrawMarkerCircle(sp.x, curY, ring1Radius, ringColor);

                    // Vòng tròn trung tâm co lại (Vàng hoàng kim)
                    float ring2Radius = Mathf.Max(3f, 10f * (1f - progress * 0.5f));
                    Color centerColor = new Color(1f, 0.92f, 0.4f, alpha * 0.95f);
                    DrawMarkerCircle(sp.x, curY, ring2Radius, centerColor);

                    // 4 Vạch tiêu điểm thập tự (Crosshair ticks)
                    float tickLen = 5f;
                    float tickDist = ring1Radius + 2f;
                    DrawMarkerLine(sp.x - tickDist - tickLen, curY, sp.x - tickDist, curY, ringColor);
                    DrawMarkerLine(sp.x + tickDist, curY, sp.x + tickDist + tickLen, curY, ringColor);
                    DrawMarkerLine(sp.x, curY - tickDist - tickLen, sp.x, curY - tickDist, ringColor);
                    DrawMarkerLine(sp.x, curY + tickDist, sp.x, curY + tickDist + tickLen, ringColor);
                }
            }
        }

        private void DrawMarkerCircle(float cx, float cy, float radius, Color color)
        {
            int segments = 16;
            float step = (Mathf.PI * 2f) / segments;
            for (int i = 0; i < segments; i++)
            {
                float a1 = i * step;
                float a2 = (i + 1) * step;
                float x1 = cx + Mathf.Cos(a1) * radius;
                float y1 = cy + Mathf.Sin(a1) * radius;
                float x2 = cx + Mathf.Cos(a2) * radius;
                float y2 = cy + Mathf.Sin(a2) * radius;
                DrawMarkerLine(x1, y1, x2, y2, color);
            }
        }

        private void DrawMarkerLine(float x1, float y1, float x2, float y2, Color color)
        {
            GUI.color = color;
            float dx = x2 - x1;
            float dy = y2 - y1;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 0.1f) return;
            float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
            Matrix4x4 matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, new Vector2(x1, y1));
            GUI.DrawTexture(new Rect(x1, y1 - 1f, len, 2f), Texture2D.whiteTexture);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }
    }
}
