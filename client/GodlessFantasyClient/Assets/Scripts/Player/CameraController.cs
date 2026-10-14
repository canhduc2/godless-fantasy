using UnityEngine;
using GodlessFantasy.Core;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Điều khiển Camera bám theo Player và Zoom tầm nhìn trong tầm kiểm soát:
    /// - Cuộn chuột để phóng to / thu nhỏ.
    /// - Giới hạn Min Zoom 2.8f và Max Zoom 5.2f (chống hack tầm nhìn).
    /// - Lerp mượt mà.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 14.0f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
        [SerializeField] private float parallaxLookAheadWeight = 0.20f; // Độ ngả parallax theo hướng trỏ chuột

        [Header("Controlled Camera Zoom")]
        [SerializeField] private float minZoom = 2.8f;
        [SerializeField] private float maxZoom = 30.0f; // Hỗ trợ bao quát tầm nhìn 50m mỗi bên (tổng chiều ngang ~100m+)
        [SerializeField] private float zoomSensitivity = 1.3f;
        [SerializeField] private float zoomSmoothSpeed = 10.0f;

        public static CameraController Instance { get; private set; }

        private Camera cam;
        private float targetZoom = 11.25f; // Mặc định tầm nhìn ~20m mỗi bên (tổng chiều ngang ~40m ở tỉ lệ 16:9)

        private void Awake()
        {
            Instance = this;
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                // Tính zoom sao cho tầm nhìn ngang đạt ~20m mỗi bên (halfWidth ≈ 20m)
                float aspect = (cam.aspect > 0.1f) ? cam.aspect : (16f / 9f);
                targetZoom = Mathf.Clamp(20.0f / aspect, minZoom, maxZoom);
                cam.orthographicSize = targetZoom;
                cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);
            }
        }

        private float shakeTimer = 0f;
        private float shakeMagnitude = 0.15f;

        public void Shake(float duration = 0.2f, float magnitude = 0.15f)
        {
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }

        private void LateUpdate()
        {
            HandleTracking();
            HandleZoom();
            HandleShake();
        }

        private void HandleShake()
        {
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                Vector2 rand = Random.insideUnitCircle * shakeMagnitude;
                transform.position += new Vector3(rand.x, rand.y, 0f);
            }
        }

        private void HandleTracking()
        {
            if (target == null)
            {
                var player = FindAnyObjectByType<PlayerMovement>();
                if (player != null) target = player.transform;
                return;
            }

            // Parallax Dynamic Tracking: Camera luôn bám chặt tâm người chơi,
            // đồng thời tạo hiệu ứng thị sai (parallax) trôi nhẹ theo hướng ngắm chuột!
            Vector3 parallaxOffset = Vector3.zero;
            if (cam != null)
            {
                Vector3 mouseWorld = cam.ScreenToWorldPoint(GameInput.GetMousePosition());
                mouseWorld.z = target.position.z;
                Vector3 mouseDelta = Vector3.ClampMagnitude(mouseWorld - target.position, 2.5f);
                parallaxOffset = mouseDelta * parallaxLookAheadWeight;
            }

            Vector3 desiredPosition = target.position + parallaxOffset + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        private void HandleZoom()
        {
            if (cam == null) return;

            // Chống zoom màn hình khi đang rê chuột trên UI, cuộn danh sách menu hoặc đang mở Menu/Drawer/Modal/Popup
            bool isBlockedByUI = GodlessFantasy.UI.CombatHUD.IsMouseOverAnyUI() || GodlessFantasy.UI.CombatHUD.IsAnyModalOpen();
            if (!isBlockedByUI)
            {
                // Phím nóng [Z] hoặc Click chuột giữa (Scroll wheel button): Chuyển đổi nhanh Tầm nhìn Tiêu chuẩn (20m mỗi bên) <-> Toàn cảnh viễn vọng (50m)
                bool isTypingChat = GodlessFantasy.UI.CombatHUD.Instance != null && GodlessFantasy.UI.CombatHUD.Instance.IsChatFocused();
                if (!isTypingChat && (Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(2)))
                {
                    float aspect = (cam != null && cam.aspect > 0.1f) ? cam.aspect : (16f / 9f);
                    float standardZoom20m = Mathf.Clamp(20.0f / aspect, minZoom, maxZoom);
                    if (targetZoom > 18f)
                    {
                        targetZoom = standardZoom20m;
                        GodlessFantasy.Combat.FloatingDamageManager.ShowFloatingText(
                            Vector3.zero, "🔍 Tầm Nhìn Tiêu Chuẩn 20m [Z]", new Color(0.4f, 0.85f, 1f));
                    }
                    else
                    {
                        targetZoom = Mathf.Clamp(50.0f / aspect, minZoom, maxZoom); // Đạt cự ly ~50m sang mỗi bên với màn hình tỉ lệ 16:9
                        GodlessFantasy.Combat.FloatingDamageManager.ShowFloatingText(
                            Vector3.zero, "🔭 Toàn Cảnh Viễn Vọng 50m [Z]", new Color(1f, 0.85f, 0.3f));
                    }
                }

                float scroll = GameInput.GetMouseScroll();
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    // Tăng dần độ nhạy cuộn chuột khi zoom xa để thao tác nhanh mượt, không cần cuộn quá nhiều lần
                    float dynamicSensitivity = zoomSensitivity * (1.0f + (targetZoom - minZoom) * 0.08f);
                    targetZoom -= scroll * dynamicSensitivity;
                    targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                }
            }

            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSmoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
