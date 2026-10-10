using UnityEngine;
using GodlessFantasy.Core;

namespace GodlessFantasy.Indicator
{
    public enum IndicatorType
    {
        None,
        DirectionalArrow, // Chiêu 1: Mũi tên định hướng (Line Skillshot)
        GroundCircle,     // Chiêu 2: Vòng tròn thả phép tại chuột (Ground AoE)
        ConeArc,          // Chiêu 3: Hình quạt quét lửa 60 độ phía trước (Cone Sweep)
        SelfNova          // Chiêu 4: Hào quang nổ 360 độ quanh người (Self Aura)
    }

    /// <summary>
    /// Bộ Điều Khiển Tâm Ngắm MOBA Đa Dạng (Multi-Archetype MOBA Indicator):
    /// - Hỗ trợ cả 4 loại ngắm: Mũi tên thẳng, Vòng tròn thả đất, Quạt quét lửa, Hào quang nổ quanh thân.
    /// - Tất cả sprite chỉ báo đều có sortingOrder = 0 (NẰM DƯỚI NHÂN VẬT LAYER 10!).
    /// </summary>
    public class IndicatorRenderer : MonoBehaviour
    {
        [Header("Max Ranges")]
        [SerializeField] private float skill1Range = 8.0f;
        [SerializeField] private float skill2MaxCastRange = 7.0f;
        [SerializeField] private float skill3ConeRange = 5.0f;
        [SerializeField] private float skill4NovaRadius = 4.5f;

        private IndicatorType currentType = IndicatorType.None;
        private Camera mainCamera;

        // Visual Transforms
        private GameObject arrowVisual;
        private GameObject groundCircleVisual;
        private GameObject coneVisual;
        private GameObject selfNovaVisual;

        public bool IsAiming => currentType != IndicatorType.None;
        public IndicatorType CurrentType => currentType;

        private void Awake()
        {
            mainCamera = Camera.main;
            BuildIndicators();
            HideIndicator();
        }

        private void BuildIndicators()
        {
            Sprite arrowSp = Resources.Load<Sprite>("indicator_arrow");
            Sprite circleSp = Resources.Load<Sprite>("indicator_ground_circle");
            Sprite coneSp = Resources.Load<Sprite>("indicator_cone");

            // 1. Arrow (Skill 1)
            arrowVisual = new GameObject("Arrow_Visual");
            arrowVisual.transform.parent = transform;
            arrowVisual.transform.localPosition = new Vector3(0.35f, 0, 0);
            var aSr = arrowVisual.AddComponent<SpriteRenderer>();
            aSr.sprite = arrowSp;
            aSr.sortingOrder = 0; // Layer 0: DƯỚI NHÂN VẬT

            // 2. Ground Circle (Skill 2)
            groundCircleVisual = new GameObject("GroundCircle_Visual");
            groundCircleVisual.transform.parent = null; // Tách rời để di chuyển độc lập theo chuột
            var gSr = groundCircleVisual.AddComponent<SpriteRenderer>();
            gSr.sprite = circleSp;
            gSr.sortingOrder = 0; // DƯỚI NHÂN VẬT
            groundCircleVisual.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

            // 3. Cone (Skill 3)
            coneVisual = new GameObject("Cone_Visual");
            coneVisual.transform.parent = transform;
            coneVisual.transform.localPosition = new Vector3(0.2f, 0, 0);
            var cSr = coneVisual.AddComponent<SpriteRenderer>();
            cSr.sprite = coneSp;
            cSr.sortingOrder = 0; // DƯỚI NHÂN VẬT
            coneVisual.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            // 4. Self Nova Circle (Skill 4)
            selfNovaVisual = new GameObject("SelfNova_Visual");
            selfNovaVisual.transform.parent = transform;
            selfNovaVisual.transform.localPosition = Vector3.zero;
            var sSr = selfNovaVisual.AddComponent<SpriteRenderer>();
            sSr.sprite = circleSp;
            sSr.sortingOrder = 0; // DƯỚI NHÂN VẬT
            selfNovaVisual.transform.localScale = new Vector3(2.2f, 2.2f, 1f);
        }

        private void Update()
        {
            if (currentType == IndicatorType.None) return;

            UpdateAim();
        }

        public void ShowIndicator(int skillSlot)
        {
            HideIndicator();

            switch (skillSlot)
            {
                case 1:
                    currentType = IndicatorType.DirectionalArrow;
                    if (arrowVisual != null) arrowVisual.SetActive(true);
                    break;
                case 2:
                    currentType = IndicatorType.GroundCircle;
                    if (groundCircleVisual != null) groundCircleVisual.SetActive(true);
                    break;
                case 3:
                    currentType = IndicatorType.ConeArc;
                    if (coneVisual != null) coneVisual.SetActive(true);
                    break;
                case 4:
                    currentType = IndicatorType.SelfNova;
                    if (selfNovaVisual != null) selfNovaVisual.SetActive(true);
                    break;
            }

            UpdateAim();
        }

        public void HideIndicator()
        {
            currentType = IndicatorType.None;
            if (arrowVisual != null) arrowVisual.SetActive(false);
            if (groundCircleVisual != null) groundCircleVisual.SetActive(false);
            if (coneVisual != null) coneVisual.SetActive(false);
            if (selfNovaVisual != null) selfNovaVisual.SetActive(false);
        }

        private void UpdateAim()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
            mouseWorldPos.z = 0;

            Vector2 toMouse = (Vector2)mouseWorldPos - (Vector2)transform.position;
            Vector2 direction = toMouse.normalized;

            // Xoay thân chỉ báo theo chuột (cho Arrow và Cone)
            if (direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Xử lý Vòng Tròn Thả Đất (Ground AoE Circle Skill 2)
            if (currentType == IndicatorType.GroundCircle && groundCircleVisual != null)
            {
                // Giới hạn bán kính thả phép tối đa 7m
                Vector3 clampedPos = (Vector2)transform.position + Vector2.ClampMagnitude(toMouse, skill2MaxCastRange);
                groundCircleVisual.transform.position = clampedPos;
            }

            // Hào quang nổ quanh thân nhấp nháy (Skill 4)
            if (currentType == IndicatorType.SelfNova && selfNovaVisual != null)
            {
                float pulse = 2.2f + Mathf.PingPong(Time.time * 2f, 0.2f);
                selfNovaVisual.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
        }

        public Vector2 GetAimDirection()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
            return ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        }

        public Vector3 GetGroundTargetPosition()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(GameInput.GetMousePosition());
            mouseWorldPos.z = 0;
            Vector2 toMouse = (Vector2)mouseWorldPos - (Vector2)transform.position;
            return (Vector2)transform.position + Vector2.ClampMagnitude(toMouse, skill2MaxCastRange);
        }

        private void OnDestroy()
        {
            if (groundCircleVisual != null) Destroy(groundCircleVisual);
        }
    }
}
