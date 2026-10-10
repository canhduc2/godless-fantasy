using System.Collections;
using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Hệ Thống Quản Lý Template Hiệu Ứng Chiêu Thức (Skill VFX & Casting Engine):
    /// - Tạo sẵn 4 Template hoàn chỉnh cho 4 loại chưởng skill:
    ///   1. Bắn Đạn Đường Thẳng (Line Projectile với TrailRenderer)
    ///   2. Vùng Đất Chỉ Định (Ground AoE với Cột Năng Lượng / Nổ Vùng Đất)
    ///   3. Hình Nón Quét (Cone Sweep với Chùm Sóng Năng Lượng Hình Quạt)
    ///   4. Nổ Tỏa Quanh Người (Self Nova với Vòng Sóng Xung Kích 360 Độ)
    /// - Hỗ trợ nạp sprite/icon tùy biến hoặc tự động đổi màu theo Hệ/Thuộc Tính: Hỏa, Băng, Lôi, Thánh, Ma Pháp!
    /// </summary>
    public class SkillVFXManager : MonoBehaviour
    {
        public static SkillVFXManager Instance { get; private set; }

        private Sprite boltSprite;
        private Material spriteMaterial;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            boltSprite = Resources.Load<Sprite>("magic_bolt");
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        public Color GetElementColor(string element, Color fallback)
        {
            if (string.IsNullOrEmpty(element)) return fallback;
            string elemLower = element.ToLower();
            if (elemLower.Contains("hỏa") || elemLower.Contains("fire")) return new Color(1.0f, 0.40f, 0.10f); // Cam đỏ rực
            if (elemLower.Contains("băng") || elemLower.Contains("ice") || elemLower.Contains("frost")) return new Color(0.20f, 0.85f, 1.0f); // Xanh băng
            if (elemLower.Contains("lôi") || elemLower.Contains("lightning") || elemLower.Contains("sấm")) return new Color(1.0f, 0.95f, 0.20f); // Vàng điện
            if (elemLower.Contains("thánh") || elemLower.Contains("holy") || elemLower.Contains("quang")) return new Color(1.0f, 0.90f, 0.45f); // Vàng kim thánh
            if (elemLower.Contains("ma") || elemLower.Contains("dark") || elemLower.Contains("hắc")) return new Color(0.75f, 0.25f, 1.0f); // Tím ma pháp
            return fallback;
        }

        // ================= 1. TEMPLATE BẮN ĐẠN ĐƯỜNG THẲNG (LINE PROJECTILE) =================
        public void SpawnLineProjectile(Vector3 startPos, Vector2 direction, int damage, SkillScrollData scroll, string shooterId)
        {
            Color elemCol = GetElementColor(scroll?.element, scroll?.tierColor ?? new Color(0.2f, 0.8f, 1f));

            GameObject boltObj = new GameObject($"Projectile_{scroll?.scrollName ?? "Bolt"}");
            boltObj.transform.position = startPos;

            var sr = boltObj.AddComponent<SpriteRenderer>();
            sr.material = spriteMaterial;
            sr.sortingOrder = 20;

            // Nạp Sprite tùy biến từ icon chiêu nếu có, ngược lại dùng boltSprite
            Sprite customSprite = !string.IsNullOrEmpty(scroll?.iconName) ? Resources.Load<Sprite>(scroll.iconName) : null;
            sr.sprite = customSprite != null ? customSprite : boltSprite;
            sr.color = elemCol;

            var col = boltObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            var rb = boltObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            // Vệt năng lượng TrailRenderer rực rỡ
            var trail = boltObj.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.35f;
            trail.endWidth = 0.02f;
            trail.sortingOrder = 20;
            trail.material = spriteMaterial;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(elemCol, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trail.colorGradient = grad;

            var bolt = boltObj.AddComponent<MagicBolt>();
            bolt.Initialize(direction, damage, shooterId);
        }

        // ================= 2. TEMPLATE VÙNG ĐẤT CHỈ ĐỊNH (GROUND AOE IMPACT) =================
        public void SpawnGroundAoEImpact(Vector3 targetPos, float radius, int damage, SkillScrollData scroll, string attackerId)
        {
            Color elemCol = GetElementColor(scroll?.element, scroll?.tierColor ?? new Color(1f, 0.5f, 0.1f));

            // Hiệu ứng nổ vùng đất
            GameObject fxObj = new GameObject($"AoE_{scroll?.scrollName ?? "Impact"}");
            fxObj.transform.position = targetPos;
            StartCoroutine(GroundAoERoutine(fxObj, targetPos, radius, damage, elemCol, attackerId));
        }

        private IEnumerator GroundAoERoutine(GameObject fxObj, Vector3 center, float radius, int damage, Color color, string attackerId)
        {
            // Vòng tròn decal mặt đất
            var lr = fxObj.AddComponent<LineRenderer>();
            lr.material = spriteMaterial;
            lr.sortingOrder = 15;
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 32;
            lr.startWidth = 0.08f;
            lr.endWidth = 0.08f;

            for (int i = 0; i < 32; i++)
            {
                float angle = i * (Mathf.PI * 2f / 32f);
                lr.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
            }

            // Gây sát thương thực tế lên mọi quái vật trong vòng tròn
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var col in hits)
            {
                if (col.gameObject.name == attackerId) continue;
                if (col.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(damage, attackerId);
                    FloatingDamageManager.Spawn(col.transform.position, damage, true);
                }
            }

            // Hoạt ảnh nổ bung tỏa ra từ tâm
            float elapsed = 0f;
            float dur = 0.45f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float currentR = Mathf.Lerp(radius * 0.4f, radius * 1.15f, t);
                lr.startWidth = Mathf.Lerp(0.18f, 0.01f, t);
                lr.endWidth = lr.startWidth;

                Color c = new Color(color.r, color.g, color.b, 1f - t);
                lr.startColor = c;
                lr.endColor = c;

                for (int i = 0; i < 32; i++)
                {
                    float angle = i * (Mathf.PI * 2f / 32f);
                    lr.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * currentR, Mathf.Sin(angle) * currentR, 0));
                }
                yield return null;
            }

            Destroy(fxObj);
        }

        // ================= 3. TEMPLATE HÌNH NÓN QUÉT (CONE SWEEP FAN) =================
        public void SpawnConeSweep(Vector3 startPos, Vector2 direction, float range, float halfAngle, int damage, SkillScrollData scroll, string attackerId)
        {
            Color elemCol = GetElementColor(scroll?.element, scroll?.tierColor ?? new Color(1f, 0.7f, 0.2f));

            // Bắn một chùm 5 tia năng lượng quét hình nón
            int pellets = 5;
            float angleStep = (halfAngle * 2f) / (pellets - 1);
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            for (int i = 0; i < pellets; i++)
            {
                float currentAngle = baseAngle - halfAngle + (i * angleStep);
                Vector2 pelletDir = new Vector2(Mathf.Cos(currentAngle * Mathf.Rad2Deg), Mathf.Sin(currentAngle * Mathf.Rad2Deg)).normalized;

                GameObject boltObj = new GameObject($"ConePellet_{i}");
                boltObj.transform.position = startPos;

                var sr = boltObj.AddComponent<SpriteRenderer>();
                sr.material = spriteMaterial;
                sr.sortingOrder = 20;
                sr.sprite = boltSprite;
                sr.color = elemCol;
                boltObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

                var col = boltObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.25f;

                var rb = boltObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;

                var trail = boltObj.AddComponent<TrailRenderer>();
                trail.time = 0.18f;
                trail.startWidth = 0.25f;
                trail.endWidth = 0.02f;
                trail.sortingOrder = 20;
                trail.material = spriteMaterial;
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(elemCol, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trail.colorGradient = grad;

                var bolt = boltObj.AddComponent<MagicBolt>();
                bolt.Initialize(pelletDir, damage, attackerId);
            }
        }

        // ================= 4. TEMPLATE NỔ TỎA QUANH NGƯỜI (SELF NOVA 360 ĐỘ) =================
        public void SpawnSelfNova(Vector3 centerPos, float radius, int damage, SkillScrollData scroll, string attackerId)
        {
            Color elemCol = GetElementColor(scroll?.element, scroll?.tierColor ?? new Color(0.9f, 0.3f, 1f));

            GameObject novaObj = new GameObject($"Nova_{scroll?.scrollName ?? "Blast"}");
            novaObj.transform.position = centerPos;
            StartCoroutine(SelfNovaRoutine(novaObj, centerPos, radius, damage, elemCol, attackerId));
        }

        private IEnumerator SelfNovaRoutine(GameObject novaObj, Vector3 center, float radius, int damage, Color color, string attackerId)
        {
            var lr = novaObj.AddComponent<LineRenderer>();
            lr.material = spriteMaterial;
            lr.sortingOrder = 22;
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 32;

            // Gây sát thương nổ quét toàn bộ quái vật quanh người
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var col in hits)
            {
                if (col.gameObject.name == attackerId) continue;
                if (col.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(damage, attackerId);
                    FloatingDamageManager.Spawn(col.transform.position, damage, true);
                }
            }

            // Hoạt ảnh sóng xung kích dâng trào ra ngoài 360 độ
            float elapsed = 0f;
            float dur = 0.35f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float currentR = Mathf.Lerp(0.3f, radius * 1.1f, t);
                lr.startWidth = Mathf.Lerp(0.28f, 0.02f, t);
                lr.endWidth = lr.startWidth;

                Color c = new Color(color.r, color.g, color.b, 1f - t);
                lr.startColor = c;
                lr.endColor = c;

                for (int i = 0; i < 32; i++)
                {
                    float angle = i * (Mathf.PI * 2f / 32f);
                    lr.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * currentR, Mathf.Sin(angle) * currentR, 0));
                }
                yield return null;
            }

            Destroy(novaObj);
        }
    }
}
