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
        SelfNova,         // Chiêu 4: Hào quang nổ 360 độ quanh người (Self Aura)
        SelfBuff,         // Chiêu 5: Buff Tự Thân
        TeamBuffWide      // Chiêu 6: Buff Hào Quang 360 Độ Rộng Lớn Toàn Đội / Bang Hội
    }

    /// <summary>
    /// Bộ Điều Khiển Tâm Ngắm MOBA Đa Dạng (Multi-Archetype MOBA Indicator):
    /// - Hỗ trợ cả 4 loại ngắm + loại 6: Mũi tên thẳng, Vòng tròn thả đất, Quạt quét lửa, Hào quang nổ quanh thân, Buff toàn đội 360 độ.
    /// - Tất cả sprite chỉ báo đều có sortingOrder = 0 (NẰM DƯỚI NHÂN VẬT LAYER 10!).
    /// </summary>
    public class IndicatorRenderer : MonoBehaviour
    {
        [Header("Max Ranges")]
        [SerializeField] private float skill2MaxCastRange = 7.0f;
        public float Skill2MaxCastRange => skill2MaxCastRange;

        private IndicatorType currentType = IndicatorType.None;
        private Camera mainCamera;

        // Visual Transforms
        private GameObject arrowVisual;
        private GameObject groundCircleVisual;
        private GameObject coneVisual;
        private GameObject selfNovaVisual;
        private GameObject teamBuffWideVisual;

        public bool IsAiming => currentType != IndicatorType.None;
        public void SetSprites(Sprite arrow, Sprite circle, Sprite cone)
        {
            if (arrow != null && arrowVisual != null) arrowVisual.GetComponent<SpriteRenderer>().sprite = arrow;
            if (circle != null)
            {
                if (groundCircleVisual != null) groundCircleVisual.GetComponent<SpriteRenderer>().sprite = circle;
                if (selfNovaVisual != null) selfNovaVisual.GetComponent<SpriteRenderer>().sprite = circle;
                if (teamBuffWideVisual != null) teamBuffWideVisual.GetComponent<SpriteRenderer>().sprite = circle;
            }
            if (cone != null && coneVisual != null) coneVisual.GetComponent<SpriteRenderer>().sprite = cone;
        }

        private void Awake()
        {
            mainCamera = Camera.main;
            BuildIndicators();
            HideIndicator();
        }

        private void BuildIndicators()
        {
            Sprite arrowSp = Resources.Load<Sprite>("indicator_arrow") ?? CreateFallbackArrow();
            Sprite circleSp = Resources.Load<Sprite>("indicator_ground_circle") ?? Resources.Load<Sprite>("indicator_circle") ?? CreateFallbackCircle();
            Sprite coneSp = Resources.Load<Sprite>("indicator_cone") ?? CreateFallbackCone();

            // 1. Arrow (Skill 1)
            arrowVisual = new GameObject("Arrow_Visual");
            arrowVisual.transform.parent = transform;
            arrowVisual.transform.localPosition = new Vector3(0.35f, 0, 0);
            var aSr = arrowVisual.AddComponent<SpriteRenderer>();
            aSr.sprite = arrowSp;
            aSr.sortingOrder = 0; // Layer 0: DƯỚI NHÂN VẬT

            // 2. Ground Circle (Skill 2: Mưa Thiên Thạch - Bán kính 1.9m)
            groundCircleVisual = new GameObject("GroundCircle_Visual");
            groundCircleVisual.transform.parent = null; // Tách rời để di chuyển độc lập theo chuột
            var gSr = groundCircleVisual.AddComponent<SpriteRenderer>();
            gSr.sprite = circleSp;
            gSr.sortingOrder = 0; // DƯỚI NHÂN VẬT
            groundCircleVisual.transform.localScale = new Vector3(2.0f, 2.0f, 1f); // Bán kính chuẩn 1.9m

            // 3. Cone (Skill 3: Sóng Lửa - Tầm quét 4.5m, góc 60 độ)
            coneVisual = new GameObject("Cone_Visual");
            coneVisual.transform.parent = transform;
            coneVisual.transform.localPosition = new Vector3(0.2f, 0, 0);
            var cSr = coneVisual.AddComponent<SpriteRenderer>();
            cSr.sprite = coneSp;
            cSr.sortingOrder = 0; // DƯỚI NHÂN VẬT
            coneVisual.transform.localScale = new Vector3(2.4f, 2.4f, 1f); // Tầm chuẩn 4.5m

            // 4. Self Nova Circle (Skill 4: Bộc Phá Thánh Điển - Bán kính chuẩn 2.5m khớp 100% Hitbox)
            selfNovaVisual = new GameObject("SelfNova_Visual");
            selfNovaVisual.transform.parent = transform;
            selfNovaVisual.transform.localPosition = Vector3.zero;
            var sSr = selfNovaVisual.AddComponent<SpriteRenderer>();
            sSr.sprite = circleSp;
            sSr.sortingOrder = 0; // DƯỚI NHÂN VẬT
            selfNovaVisual.transform.localScale = new Vector3(2.66f, 2.66f, 1f); // Bán kính chuẩn 2.5m khớp 100% Hitbox!

            // 6. Team Buff Wide Circle (Skill 6: Buff Hào Quang Đồng Đội 360 Độ - Bán kính rộng 6.5m, màu vàng hoàng kim)
            teamBuffWideVisual = new GameObject("TeamBuffWide_Visual");
            teamBuffWideVisual.transform.parent = transform;
            teamBuffWideVisual.transform.localPosition = Vector3.zero;
            var tbSr = teamBuffWideVisual.AddComponent<SpriteRenderer>();
            tbSr.sprite = circleSp;
            tbSr.color = new Color(1.0f, 0.88f, 0.25f, 0.75f); // Màu vàng hoàng gia nổi bật
            tbSr.sortingOrder = 0; // DƯỚI NHÂN VẬT
            teamBuffWideVisual.transform.localScale = new Vector3(6.8f, 6.8f, 1f); // Bán kính chuẩn 6.5m!
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
                case 6:
                    currentType = IndicatorType.TeamBuffWide;
                    if (teamBuffWideVisual != null) teamBuffWideVisual.SetActive(true);
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
            if (teamBuffWideVisual != null) teamBuffWideVisual.SetActive(false);
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

            // Hào quang nổ quanh thân (Skill 4: Bộc Phá Thánh Điển) giữ đúng bán kính chuẩn 2.5m
            if (currentType == IndicatorType.SelfNova && selfNovaVisual != null)
            {
                selfNovaVisual.transform.localScale = new Vector3(2.66f, 2.66f, 1f);
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

        private Sprite CreateFallbackArrow()
        {
            Texture2D tex = new Texture2D(32, 16, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = new Color(0, 0, 0, 0);
            Color cyan = new Color(0.2f, 0.9f, 1f, 0.9f);
            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 16; y++)
                    tex.SetPixel(x, y, clear);

            for (int x = 0; x < 24; x++)
            {
                tex.SetPixel(x, 7, cyan);
                tex.SetPixel(x, 8, cyan);
            }
            for (int i = 0; i < 8; i++)
            {
                tex.SetPixel(24 + i, 7 - i / 2, cyan);
                tex.SetPixel(24 + i, 8 + i / 2, cyan);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 16), new Vector2(0f, 0.5f), 16f);
        }

        private Sprite CreateFallbackCircle()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            Color cyan = new Color(0.2f, 0.9f, 1f, 0.85f);
            Color fill = new Color(0.2f, 0.9f, 1f, 0.15f);
            Color clear = new Color(0, 0, 0, 0);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= 30f && dist >= 27f) tex.SetPixel(x, y, cyan);
                    else if (dist < 27f) tex.SetPixel(x, y, fill);
                    else tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        private Sprite CreateFallbackCone()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color orange = new Color(1f, 0.55f, 0.15f, 0.85f);
            Color fill = new Color(1f, 0.55f, 0.15f, 0.2f);
            Color clear = new Color(0, 0, 0, 0);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x;
                    float dy = y - size / 2f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Abs(Mathf.Atan2(dy, dx) * Mathf.Rad2Deg);
                    if (angle <= 30f && dist <= 60f)
                    {
                        if (dist >= 56f || angle >= 27f) tex.SetPixel(x, y, orange);
                        else tex.SetPixel(x, y, fill);
                    }
                    else tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0f, 0.5f), 16f);
        }

        private void OnDestroy()
        {
            if (groundCircleVisual != null) Destroy(groundCircleVisual);
        }
    }
}
