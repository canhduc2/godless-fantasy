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

        public const float ForwardOffset = 0.55f; // Khoảng cách cách đều chuẩn mực ngoài tâm nhân vật (0.55m)
        private float currentCastRange = 8.0f;
        private float currentAreaRadius = 1.9f;

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
            if (arrow != null && arrowVisual != null)
            {
                var sr = arrowVisual.GetComponent<SpriteRenderer>();
                sr.sprite = arrow;
                sr.sortingOrder = 1;
            }
            if (circle != null)
            {
                if (groundCircleVisual != null) { var sr = groundCircleVisual.GetComponent<SpriteRenderer>(); sr.sprite = circle; sr.sortingOrder = 1; }
                if (selfNovaVisual != null) { var sr = selfNovaVisual.GetComponent<SpriteRenderer>(); sr.sprite = circle; sr.sortingOrder = 1; }
                if (teamBuffWideVisual != null) { var sr = teamBuffWideVisual.GetComponent<SpriteRenderer>(); sr.sprite = circle; sr.sortingOrder = 1; }
            }
            if (cone != null && coneVisual != null)
            {
                Sprite customCone = (cone.texture != null)
                    ? Sprite.Create(cone.texture, new Rect(0, 0, cone.texture.width, cone.texture.height), new Vector2(0.125f, 0.5f), 32f)
                    : cone;
                var sr = coneVisual.GetComponent<SpriteRenderer>();
                sr.sprite = customCone;
                sr.sortingOrder = 1;
            }
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
            Sprite rawCone = Resources.Load<Sprite>("indicator_cone");
            Sprite coneSp = (rawCone != null && rawCone.texture != null)
                ? Sprite.Create(rawCone.texture, new Rect(0, 0, rawCone.texture.width, rawCone.texture.height), new Vector2(0.125f, 0.5f), 32f)
                : CreateFallbackCone();

            // 1. Arrow (Skill 1: Bắn Thẳng - Xuất phát cách người 0.55m, nằm layer 1 DƯỚI NHÂN VẬT LAYER 10)
            arrowVisual = new GameObject("Arrow_Visual");
            arrowVisual.transform.parent = transform;
            arrowVisual.transform.localPosition = new Vector3(ForwardOffset, 0, 0);
            var aSr = arrowVisual.AddComponent<SpriteRenderer>();
            aSr.sprite = arrowSp;
            aSr.sortingOrder = 1; // NẰM DƯỚI NHÂN VẬT (Layer 10) VÀ DƯỚI BÓNG (Layer 7)

            // 2. Ground Circle (Skill 2: Mưa Vùng Đất)
            groundCircleVisual = new GameObject("GroundCircle_Visual");
            groundCircleVisual.transform.parent = null;
            var gSr = groundCircleVisual.AddComponent<SpriteRenderer>();
            gSr.sprite = circleSp;
            gSr.sortingOrder = 1;
            groundCircleVisual.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

            // 3. Cone (Skill 3: Hình Quạt - Xuất phát cách người đúng 0.55m như Arrow, nằm layer 1 DƯỚI NHÂN VẬT)
            coneVisual = new GameObject("Cone_Visual");
            coneVisual.transform.parent = transform;
            coneVisual.transform.localPosition = new Vector3(ForwardOffset, 0, 0);
            var cSr = coneVisual.AddComponent<SpriteRenderer>();
            cSr.sprite = coneSp;
            cSr.sortingOrder = 1; // NẰM DƯỚI NHÂN VẬT
            coneVisual.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

            // 4. Self Nova Circle (Skill 4: Bộc Phá 360 Độ)
            selfNovaVisual = new GameObject("SelfNova_Visual");
            selfNovaVisual.transform.parent = transform;
            selfNovaVisual.transform.localPosition = Vector3.zero;
            var sSr = selfNovaVisual.AddComponent<SpriteRenderer>();
            sSr.sprite = circleSp;
            sSr.sortingOrder = 1; // NẰM DƯỚI NHÂN VẬT
            selfNovaVisual.transform.localScale = new Vector3(2.66f, 2.66f, 1f);

            // 6. Team Buff Wide Circle (Skill 6: Buff Hào Quang Đội 360 Độ)
            teamBuffWideVisual = new GameObject("TeamBuffWide_Visual");
            teamBuffWideVisual.transform.parent = transform;
            teamBuffWideVisual.transform.localPosition = Vector3.zero;
            var tbSr = teamBuffWideVisual.AddComponent<SpriteRenderer>();
            tbSr.sprite = circleSp;
            tbSr.color = new Color(1.0f, 0.88f, 0.25f, 0.75f);
            tbSr.sortingOrder = 1; // NẰM DƯỚI NHÂN VẬT
            teamBuffWideVisual.transform.localScale = new Vector3(6.8f, 6.8f, 1f);
        }

        private void Update()
        {
            if (currentType == IndicatorType.None) return;

            UpdateAim();
        }

        public void ShowIndicator(int skillSlot)
        {
            ShowIndicator(skillSlot, 8.0f, 1.9f);
        }

        public void ShowIndicator(int skillSlot, float castRange, float areaRadius = 1.9f)
        {
            HideIndicator();

            currentCastRange = Mathf.Max(1.5f, castRange);
            currentAreaRadius = Mathf.Max(0.5f, areaRadius);
            skill2MaxCastRange = currentCastRange; // Tầm vươn tối đa của chuột khớp 100% tầm chiêu

            switch (skillSlot)
            {
                case 1:
                    currentType = IndicatorType.DirectionalArrow;
                    if (arrowVisual != null)
                    {
                        arrowVisual.SetActive(true);
                        // Scale chiều dài X chính xác từ ForwardOffset đến hết castRange (Base arrow length = 4.5m)
                        float arrowReach = Mathf.Max(1.0f, currentCastRange - ForwardOffset);
                        arrowVisual.transform.localPosition = new Vector3(ForwardOffset, 0, 0);
                        arrowVisual.transform.localScale = new Vector3(arrowReach / 4.5f, 1.0f, 1.0f);
                    }
                    break;

                case 2:
                    currentType = IndicatorType.GroundCircle;
                    if (groundCircleVisual != null)
                    {
                        groundCircleVisual.SetActive(true);
                        // Scale bán kính vòng tròn khớp chuẩn areaRadius
                        groundCircleVisual.transform.localScale = new Vector3(currentAreaRadius, currentAreaRadius, 1.0f);
                    }
                    break;

                case 3:
                    currentType = IndicatorType.ConeArc;
                    if (coneVisual != null)
                    {
                        coneVisual.SetActive(true);
                        // Scale hình quạt chính xác từ ForwardOffset đến hết castRange (Base cone radius = 1.75m)
                        float coneReach = Mathf.Max(1.0f, currentCastRange - ForwardOffset);
                        float coneScale = coneReach / 1.75f;
                        coneVisual.transform.localPosition = new Vector3(ForwardOffset, 0, 0);
                        coneVisual.transform.localScale = new Vector3(coneScale, coneScale, 1.0f);
                    }
                    break;

                case 4:
                    currentType = IndicatorType.SelfNova;
                    if (selfNovaVisual != null)
                    {
                        selfNovaVisual.SetActive(true);
                        float novaR = currentAreaRadius > 0.8f ? currentAreaRadius : 2.5f;
                        selfNovaVisual.transform.localScale = new Vector3(novaR, novaR, 1.0f);
                    }
                    break;

                case 6:
                    currentType = IndicatorType.TeamBuffWide;
                    if (teamBuffWideVisual != null)
                    {
                        teamBuffWideVisual.SetActive(true);
                        float teamR = currentCastRange > 1f ? currentCastRange : 6.5f;
                        teamBuffWideVisual.transform.localScale = new Vector3(teamR, teamR, 1.0f);
                    }
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
            int w = 144, h = 32;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color clear = new Color(0, 0, 0, 0);
            Color border = new Color(0.0f, 0.96f, 1.0f, 0.95f);
            Color borderGlow = new Color(0.0f, 0.85f, 1.0f, 0.75f);
            Color fill = new Color(0.1f, 0.65f, 0.95f, 0.35f);
            Color core = new Color(0.6f, 0.98f, 1.0f, 0.75f);

            float topY = 2f, botY = 29f, midY = 15.5f, halfH = 13.5f;
            float shaftEndX = 116f, capRx = 25f;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (x <= shaftEndX)
                    {
                        if (y < topY || y > botY) { tex.SetPixel(x, y, clear); continue; }
                        float distB = Mathf.Min(Mathf.Abs(y - topY), Mathf.Abs(y - botY));
                        float distC = Mathf.Abs(y - midY);
                        if (distB <= 1f) tex.SetPixel(x, y, border);
                        else if (distB <= 2f) tex.SetPixel(x, y, borderGlow);
                        else if (distC <= 1.5f) tex.SetPixel(x, y, core);
                        else tex.SetPixel(x, y, fill);
                    }
                    else
                    {
                        float nx = (x - shaftEndX) / capRx;
                        float ny = (y - midY) / halfH;
                        float dist = Mathf.Sqrt(nx * nx + ny * ny);
                        if (dist <= 1.02f)
                        {
                            float distC = Mathf.Abs(y - midY);
                            if (dist >= 0.88f) tex.SetPixel(x, y, border);
                            else if (dist >= 0.76f) tex.SetPixel(x, y, borderGlow);
                            else if (distC <= 1.5f && dist < 0.65f) tex.SetPixel(x, y, core);
                            else tex.SetPixel(x, y, fill);
                        }
                        else
                        {
                            tex.SetPixel(x, y, clear);
                        }
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 32f);
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
