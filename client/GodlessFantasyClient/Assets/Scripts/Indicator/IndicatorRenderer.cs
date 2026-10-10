using UnityEngine;

namespace GodlessFantasy.Indicator
{
    /// <summary>
    /// Hiển thị tâm ngắm kỹ năng theo chuột (Directional Skillshot Indicator):
    /// Luôn bám theo vị trí nhân vật và xoay đầu mũi tên hướng về con trỏ chuột.
    /// Cho phép nhân vật vừa chạy né đòn (kiting) mà tâm ngắm vẫn bám chuột liên tục!
    /// </summary>
    public class IndicatorRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform indicatorVisual;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isAiming = false;
        private Camera mainCamera;

        public bool IsAiming => isAiming;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (indicatorVisual == null) indicatorVisual = transform;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            HideIndicator();
        }

        private void Update()
        {
            if (!isAiming) return;

            UpdateAimDirection();
        }

        public void ShowIndicator()
        {
            isAiming = true;
            if (indicatorVisual != null) indicatorVisual.gameObject.SetActive(true);
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            UpdateAimDirection();
        }

        public void HideIndicator()
        {
            isAiming = false;
            if (indicatorVisual != null) indicatorVisual.gameObject.SetActive(false);
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }

        private void UpdateAimDirection()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

            if (direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        public Vector2 GetAimDirection()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            return ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        }
    }
}
