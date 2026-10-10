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
        [SerializeField] private float maxZoom = 5.2f;
        [SerializeField] private float zoomSensitivity = 1.0f;
        [SerializeField] private float zoomSmoothSpeed = 10.0f;

        private Camera cam;
        private float targetZoom = 3.6f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = targetZoom;
                cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);
            }
        }

        private void LateUpdate()
        {
            HandleTracking();
            HandleZoom();
        }

        private void HandleTracking()
        {
            if (target == null)
            {
                var player = FindFirstObjectByType<PlayerMovement>();
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

            float scroll = GameInput.GetMouseScroll();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * zoomSensitivity;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }

            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSmoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
