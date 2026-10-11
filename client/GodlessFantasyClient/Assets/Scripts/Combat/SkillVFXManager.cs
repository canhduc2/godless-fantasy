using System.Collections;
using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Hệ Thống Quản Lý Template Hiệu Ứng Chiêu Thức Chuẩn Xác 100% Theo Tầm Ngắm (Skill VFX Engine):
    /// - Chiêu 1: Bắn Đạn Tuyến Tính (Line Projectile với TrailRenderer bay thẳng)
    /// - Chiêu 2: Mưa Kỹ Năng / Thiên Thạch Đổ Xuống (Sky Rain / Meteor Shower rơi từ trời xuống điểm ngắm indicator_circle)
    /// - Chiêu 3: Sóng Lửa Quét Hình Nón Liền Mạch (Cohesive Cone Flame Wave khớp 100% hình quạt indicator_cone)
    /// - Chiêu 4: Hào Quang Nổ Xung Kích 360 Độ (Self Nova Shockwave tỏa đều quanh người)
    /// </summary>
    public class SkillVFXManager : MonoBehaviour
    {
        public static SkillVFXManager Instance { get; private set; }

        private Sprite boltSprite;
        private Sprite coneSprite;
        private Sprite groundCircleSprite;
        private Material spriteMaterial;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            boltSprite = Resources.Load<Sprite>("magic_bolt");
            Sprite rawCone = Resources.Load<Sprite>("indicator_cone");
            coneSprite = (rawCone != null && rawCone.texture != null)
                ? Sprite.Create(rawCone.texture, new Rect(0, 0, rawCone.texture.width, rawCone.texture.height), new Vector2(0.125f, 0.5f), 32f)
                : CreateFallbackCone();
            groundCircleSprite = Resources.Load<Sprite>("indicator_ground_circle") ?? Resources.Load<Sprite>("indicator_circle") ?? CreateFallbackCircle();
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        public Color GetElementColor(string element, Color fallback)
        {
            if (string.IsNullOrEmpty(element)) return fallback;
            string elemLower = element.ToLower();
            if (elemLower.Contains("hỏa") || elemLower.Contains("fire")) return new Color(1.0f, 0.32f, 0.05f); // 🔥 Cam đỏ rực
            if (elemLower.Contains("băng") || elemLower.Contains("ice") || elemLower.Contains("frost")) return new Color(0.20f, 0.85f, 1.0f); // 🧊 Xanh băng
            if (elemLower.Contains("lôi") || elemLower.Contains("lightning") || elemLower.Contains("sấm")) return new Color(1.0f, 0.95f, 0.15f); // ⚡ Vàng sấm
            if (elemLower.Contains("mộc") || elemLower.Contains("wood") || elemLower.Contains("sinh") || elemLower.Contains("nature")) return new Color(0.15f, 0.92f, 0.30f); // 🌿 Xanh sinh mệnh
            if (elemLower.Contains("phong") || elemLower.Contains("wind") || elemLower.Contains("ngọc")) return new Color(0.20f, 0.92f, 0.85f); // 💨 Xanh ngọc
            if (elemLower.Contains("quang") || elemLower.Contains("holy") || elemLower.Contains("thánh") || elemLower.Contains("light") || elemLower.Contains("sáng") || elemLower.Contains("bạch kim")) return new Color(0.96f, 0.98f, 1.0f); // ✨ TRẮNG BẠCH KIM
            if (elemLower.Contains("ma") || elemLower.Contains("dark") || elemLower.Contains("hắc") || elemLower.Contains("ám") || elemLower.Contains("tối")) return new Color(0.72f, 0.22f, 1.0f); // 🌑 Tím ma pháp
            return fallback;
        }

        // ================= 1. TEMPLATE CHIÊU 1: BẮN ĐẠN ĐƯỜNG THẲNG (LINE PROJECTILE) =================
        public void SpawnLineProjectile(Vector3 startPos, Vector2 direction, int damage, SkillScrollData scroll, string shooterId)
        {
            float range = scroll != null && scroll.castRange > 0 ? scroll.castRange : 8.0f;

            // Tận dụng ProceduralVFXEngine (140 biến thể nguyên tố & 8 phôi khối Pixel Art)
            if (ProceduralVFXEngine.Instance != null && scroll != null)
            {
                BaseShapeMask shape = (BaseShapeMask)Mathf.Clamp(scroll.baseShapeMask, 0, 7);
                TrajectoryType traj = (TrajectoryType)Mathf.Clamp(scroll.trajectoryType, 0, 5);
                float speed = scroll.flySpeed > 0 ? scroll.flySpeed : 16.0f;
                float radius = scroll.explosionRadius > 0 ? scroll.explosionRadius : 1.8f;
                ProceduralVFXEngine.Instance.FireProceduralSkill(startPos, direction, shape, scroll.element, traj, damage, speed, radius, scroll.hasTrail, scroll.hasScreenShake, shooterId, null, range);
                return;
            }

            Color elemCol = GetElementColor(scroll?.element, scroll?.tierColor ?? new Color(0.2f, 0.8f, 1f));

            GameObject boltObj = new GameObject($"Projectile_{scroll?.scrollName ?? "Bolt"}");
            boltObj.transform.position = startPos;

            var sr = boltObj.AddComponent<SpriteRenderer>();
            sr.material = spriteMaterial;
            sr.sortingOrder = 20;

            Sprite customSprite = !string.IsNullOrEmpty(scroll?.iconName) ? Resources.Load<Sprite>(scroll.iconName) : null;
            sr.sprite = customSprite != null ? customSprite : boltSprite;
            sr.color = elemCol;

            var col = boltObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            var rb = boltObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

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
            bolt.Initialize(direction, damage, shooterId, range);
        }

        // ================= 2. TEMPLATE CHIÊU 2: MƯA SKILL ĐỔ XUỐNG (SKY RAIN / METEOR SHOWER) =================
        // Khớp 100% vòng tròn ngắm indicator_circle: Thiên thạch / mưa phép đổ từ trời xuống điểm chỉ định!
        public void SpawnGroundAoEImpact(Vector3 targetPos, float radius, int damage, SkillScrollData scroll, string attackerId)
        {
            if (ProceduralVFXEngine.Instance != null && scroll != null)
            {
                bool isSprout = (scroll.baseShapeMask == 8 || scroll.baseShapeMask == 9 || scroll.baseShapeMask == 10 || scroll.baseShapeMask == 11 || scroll.baseShapeMask == 12 || scroll.baseShapeMask == 20);
                if (isSprout)
                {
                    ProceduralVFXEngine.Instance.SpawnProceduralGroundEruption(targetPos, scroll.element, (BaseShapeMask)scroll.baseShapeMask, radius, damage, attackerId);
                    return;
                }
                ProceduralVFXEngine.Instance.SpawnProceduralAoERain(targetPos, scroll.element, radius, damage, attackerId, scroll.rainFallAngle, scroll.rainFallStyle);
                return;
            }

            Color elemCol = GetElementColor(scroll?.element, scroll?.tierColor ?? new Color(1f, 0.45f, 0.1f));
            StartCoroutine(MeteorRainShowerRoutine(targetPos, radius, damage, elemCol, scroll, attackerId));
        }

        private IEnumerator MeteorRainShowerRoutine(Vector3 center, float radius, int totalDamage, Color color, SkillScrollData scroll, string attackerId)
        {
            // 1. Decal ma pháp hiển thị ngay trên mặt đất tại điểm ngắm (khớp 100% indicator_circle)
            GameObject decalObj = new GameObject($"AoEDecal_{scroll?.scrollName ?? "Rain"}");
            decalObj.transform.position = center;
            var decalSr = decalObj.AddComponent<SpriteRenderer>();
            decalSr.material = spriteMaterial;
            decalSr.sprite = groundCircleSprite;
            decalSr.color = new Color(color.r, color.g, color.b, 0.45f);
            decalSr.sortingOrder = 5;
            decalObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f); // Bán kính chuẩn 1.9m

            // 2. MƯA SKILL RƠI TỪ BẦU TRỜI: 6 đợt thiên thạch / tia phép dội xé gió xuống vòng tròn
            int totalMeteors = 6;
            int damagePerHit = Mathf.Max(1, totalDamage / 3);
            float rainDuration = 0.60f;
            float interval = rainDuration / totalMeteors;

            for (int i = 0; i < totalMeteors; i++)
            {
                // Điểm rơi ngẫu nhiên bên trong vòng tròn bán kính
                Vector2 randomOffset = Random.insideUnitCircle * (radius * 0.75f);
                Vector3 impactPoint = center + new Vector3(randomOffset.x, randomOffset.y, 0);

                // Rơi từ góc trên cao (Bầu trời đổ xuống góc nghiêng 65 độ)
                Vector3 skyStart = impactPoint + new Vector3(-1.5f, 6.5f, 0);

                StartCoroutine(SingleFallingMeteor(skyStart, impactPoint, damagePerHit, color, attackerId));

                yield return new WaitForSeconds(interval);
            }

            // Decal mờ dần và giải phóng
            float fadeTime = 0.35f;
            float elapsed = 0f;
            Color initC = decalSr.color;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                if (decalSr != null)
                {
                    decalSr.color = new Color(initC.r, initC.g, initC.b, Mathf.Lerp(initC.a, 0f, elapsed / fadeTime));
                }
                yield return null;
            }
            if (decalObj != null) Destroy(decalObj);
        }

        private IEnumerator SingleFallingMeteor(Vector3 start, Vector3 end, int damage, Color color, string attackerId)
        {
            GameObject meteorObj = new GameObject("Falling_Meteor");
            meteorObj.transform.position = start;

            var sr = meteorObj.AddComponent<SpriteRenderer>();
            sr.material = spriteMaterial;
            sr.sprite = boltSprite;
            sr.color = color;
            sr.sortingOrder = 25;
            meteorObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

            var trail = meteorObj.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.42f;
            trail.endWidth = 0.05f;
            trail.sortingOrder = 24;
            trail.material = spriteMaterial;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.95f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trail.colorGradient = grad;

            Vector3 dir = (end - start).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            meteorObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            float travelTime = 0.17f; // Rơi cực nhanh
            float elapsed = 0f;

            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / travelTime;
                meteorObj.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            Destroy(meteorObj);

            // Nổ tung bộc phá tại điểm chạm đất (Cực đẹp)
            SpawnGroundImpactBlast(end, 0.95f, damage, color, attackerId);
        }

        private void SpawnGroundImpactBlast(Vector3 pos, float blastRadius, int damage, Color color, string attackerId)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, blastRadius);
            foreach (var col in hits)
            {
                if (col.gameObject.name == attackerId) continue;
                if (col.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(damage, attackerId);
                    FloatingDamageManager.Spawn(col.transform.position, damage, true);
                }
            }

            GameObject blastObj = new GameObject("Meteor_Blast");
            blastObj.transform.position = pos;
            var sr = blastObj.AddComponent<SpriteRenderer>();
            sr.material = spriteMaterial;
            sr.sprite = groundCircleSprite;
            sr.color = color;
            sr.sortingOrder = 16;
            GodlessFantasy.Core.AudioManager.PlayExplosion();
            StartCoroutine(ImpactBlastExpand(blastObj, sr, blastRadius));
        }

        private IEnumerator ImpactBlastExpand(GameObject obj, SpriteRenderer sr, float maxRadius)
        {
            float dur = 0.22f;
            float elapsed = 0f;
            Color initC = sr.color;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float s = Mathf.Lerp(0.3f, maxRadius * 1.5f, t);
                obj.transform.localScale = new Vector3(s, s, 1f);
                sr.color = new Color(initC.r, initC.g, initC.b, Mathf.Lerp(initC.a, 0f, t));
                yield return null;
            }
            Destroy(obj);
        }

        // ================= 3. TEMPLATE CHIÊU 3: SÓNG LỬA HÌNH QUẠT (COHESIVE CONE FLAME WAVE) =================
        // Khớp 100% hình quạt indicator_cone: Không còn bị bắn đạn tõe góc nữa, mà là một làn sóng lửa cuồn cuộn quét tới!
        public void SpawnConeSweep(Vector3 startPos, Vector2 direction, float range, float halfAngle, int damage, SkillScrollData scroll, string attackerId)
        {
            if (ProceduralVFXEngine.Instance != null && scroll != null)
            {
                ProceduralVFXEngine.Instance.SpawnProceduralConeWave(startPos, direction, scroll.element, range, damage, attackerId);
                return;
            }

            Color elemCol = GetElementColor(scroll?.element, scroll?.tierColor ?? new Color(1f, 0.45f, 0.1f));
            StartCoroutine(ConeFlameWaveRoutine(startPos, direction, range, halfAngle, damage, elemCol, scroll, attackerId));
        }

        private IEnumerator ConeFlameWaveRoutine(Vector3 startPos, Vector2 direction, float range, float halfAngle, int damage, Color color, SkillScrollData scroll, string attackerId)
        {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 1. Đối tượng sóng lửa chính mang hình dáng hình nón quạt khớp 100% indicator_cone
            GameObject coneWaveObj = new GameObject($"ConeWave_{scroll?.scrollName ?? "Flame"}");
            coneWaveObj.transform.position = startPos;
            coneWaveObj.transform.rotation = Quaternion.Euler(0, 0, baseAngle);

            var sr = coneWaveObj.AddComponent<SpriteRenderer>();
            sr.material = spriteMaterial;
            sr.sprite = coneSprite; // Sử dụng chính sprite indicator_cone
            sr.sortingOrder = 22;   // Nổi bật trên nhân vật và mặt đất
            sr.color = new Color(1f, 0.95f, 0.6f, 0.95f); // Lõi lửa trắng vàng rực rỡ lúc khai hỏa

            // 2. Gây sát thương quét trong phạm vi hình nón khớp 100% tầm ngắm
            Collider2D[] hits = Physics2D.OverlapCircleAll(startPos, range);
            foreach (var col in hits)
            {
                if (col.gameObject.name == attackerId) continue;
                Vector2 toTarget = (col.transform.position - startPos);
                float angle = Vector2.Angle(direction, toTarget.normalized);

                if (angle <= halfAngle)
                {
                    if (col.TryGetComponent<IDamageable>(out var target))
                    {
                        target.TakeDamage(damage, attackerId);
                        FloatingDamageManager.Spawn(col.transform.position, damage, false);
                    }
                }
            }

            // 3. Hoạt ảnh sóng lửa dâng trào từ người ra ngoài theo hình quạt tầm 4.5m
            float duration = 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Scale mở rộng theo hình nón từ 0.3 lên 2.4 (Khớp 100% IndicatorRenderer)
                float currentScale = Mathf.Lerp(0.35f, 2.4f, t);
                coneWaveObj.transform.localScale = new Vector3(currentScale, currentScale, 1f);

                // Màu chuyển từ trắng vàng sang cam đỏ rực rồi tan biến
                Color curCol = Color.Lerp(new Color(1f, 0.95f, 0.6f), color, t * 1.2f);
                sr.color = new Color(curCol.r, curCol.g, curCol.b, Mathf.Lerp(0.95f, 0f, t * t));

                yield return null;
            }

            Destroy(coneWaveObj);
        }

        // ================= 4. TEMPLATE CHIÊU 4: NỔ TỎA QUANH NGƯỜI (SELF NOVA 360 ĐỘ) =================
        public void SpawnSelfNova(Vector3 centerPos, float radius, int damage, SkillScrollData scroll, string attackerId)
        {
            if (ProceduralVFXEngine.Instance != null && scroll != null)
            {
                ProceduralVFXEngine.Instance.SpawnProceduralRadialNova(centerPos, scroll.element, radius, damage, attackerId);
                return;
            }

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

        // ================= 5. HIỆU ỨNG BUFF, HỒI PHỤC & MƯA HỆ MỘC =================
        public void SpawnSelfNovaVFX(Vector3 centerPos, string element)
        {
            Color elemCol = GetElementColor(element, new Color(0.2f, 0.95f, 0.4f));
            GameObject novaObj = new GameObject($"NovaVFX_{element}");
            novaObj.transform.position = centerPos;
            StartCoroutine(NovaVisualOnlyRoutine(novaObj, centerPos, 2.5f, elemCol));
        }

        private IEnumerator NovaVisualOnlyRoutine(GameObject novaObj, Vector3 center, float radius, Color color)
        {
            var lr = novaObj.AddComponent<LineRenderer>();
            lr.material = spriteMaterial;
            lr.sortingOrder = 23;
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 32;

            float elapsed = 0f;
            float dur = 0.35f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float currentR = Mathf.Lerp(0.3f, radius * 1.1f, t);
                lr.startWidth = Mathf.Lerp(0.25f, 0.02f, t);
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

        public void SpawnSelfBuffVFX(Vector3 centerPos, string element)
        {
            Color elemCol = GetElementColor(element, Color.cyan);
            GameObject buffObj = new GameObject($"BuffAura_{element}");
            buffObj.transform.position = centerPos;
            StartCoroutine(SelfBuffAuraRoutine(buffObj, centerPos, elemCol));
        }

        private IEnumerator SelfBuffAuraRoutine(GameObject buffObj, Vector3 center, Color color)
        {
            var lr = buffObj.AddComponent<LineRenderer>();
            lr.material = spriteMaterial;
            lr.sortingOrder = 24;
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 24;

            float elapsed = 0f;
            float dur = 0.65f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float currentR = Mathf.Lerp(0.2f, 1.1f, t);
                float yOffset = Mathf.Lerp(0f, 1.2f, t);

                lr.startWidth = Mathf.Lerp(0.18f, 0.02f, t);
                lr.endWidth = lr.startWidth;

                Color c = new Color(color.r, color.g, color.b, (1f - t) * 0.9f);
                lr.startColor = c;
                lr.endColor = c;

                for (int i = 0; i < 24; i++)
                {
                    float angle = i * (Mathf.PI * 2f / 24f);
                    lr.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * currentR, Mathf.Sin(angle) * currentR * 0.5f + yOffset, 0));
                }
                yield return null;
            }
            Destroy(buffObj);
        }

        public void SpawnHealingRain(Vector3 targetPos, float radius, float duration, string element)
        {
            Color elemCol = GetElementColor(element, new Color(0.25f, 0.95f, 0.45f));
            GameObject rainObj = new GameObject($"HealingRain_{element}");
            rainObj.transform.position = targetPos;
            StartCoroutine(HealingRainRoutine(rainObj, targetPos, radius, duration, elemCol));
        }

        private IEnumerator HealingRainRoutine(GameObject rainObj, Vector3 targetPos, float radius, float duration, Color color)
        {
            // Vẽ vòng tròn ngọc bích phát sáng trên mặt đất
            var lr = rainObj.AddComponent<LineRenderer>();
            lr.material = spriteMaterial;
            lr.sortingOrder = 15;
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 32;
            lr.startWidth = 0.12f;
            lr.endWidth = 0.12f;
            lr.startColor = new Color(color.r, color.g, color.b, 0.75f);
            lr.endColor = lr.startColor;

            for (int i = 0; i < 32; i++)
            {
                float angle = i * (Mathf.PI * 2f / 32f);
                lr.SetPosition(i, targetPos + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * 8f);
                lr.startColor = new Color(color.r, color.g, color.b, Mathf.Lerp(0.4f, 0.85f, pulse));
                lr.endColor = lr.startColor;
                yield return null;
            }
            Destroy(rainObj);
        }

        // ================= FALLBACK SPRITE GENERATORS =================
        // ================= 6. TEMPLATE CHIÊU 6: BUFF DIỆN RỘNG TOÀN ĐỘI / BANG HỘI (AOE TEAM BUFF 360 ĐỘ) =================
        public void SpawnTeamBuffAoEVFX(Vector3 centerPos, float radius = 6.5f, string element = "Hỏa")
        {
            Color goldBuffColor = new Color(1.0f, 0.88f, 0.25f); // Màu vàng hoàng kim rực rỡ đặc trưng chiêu 6
            GameObject teamBuffObj = new GameObject($"TeamBuffAoE_{element}");
            teamBuffObj.transform.position = centerPos;
            StartCoroutine(TeamBuffAoERoutine(teamBuffObj, centerPos, radius, goldBuffColor));
        }

        private IEnumerator TeamBuffAoERoutine(GameObject obj, Vector3 center, float radius, Color color)
        {
            var lr = obj.AddComponent<LineRenderer>();
            lr.material = spriteMaterial;
            lr.sortingOrder = 24;
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 36;

            float dur = 0.65f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float curR = Mathf.Lerp(0.5f, radius, Mathf.SmoothStep(0f, 1f, t));
                lr.startWidth = Mathf.Lerp(0.28f, 0.03f, t);
                lr.endWidth = lr.startWidth;

                Color c = new Color(color.r, color.g, color.b, (1f - t) * 0.95f);
                lr.startColor = c;
                lr.endColor = c;

                for (int i = 0; i < 36; i++)
                {
                    float angle = i * (Mathf.PI * 2f / 36f);
                    lr.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * curR, Mathf.Sin(angle) * curR, 0));
                }
                yield return null;
            }

            Destroy(obj);
        }

        private Sprite CreateFallbackCircle()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Vector2 c = new Vector2(size / 2f, size / 2f);
            Color cyan = new Color(0.2f, 0.9f, 1f, 0.85f);
            Color fill = new Color(0.2f, 0.9f, 1f, 0.15f);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), c);
                    if (dist <= 30f && dist >= 27f) tex.SetPixel(x, y, cyan);
                    else if (dist < 27f) tex.SetPixel(x, y, fill);
                    else tex.SetPixel(x, y, Color.clear);
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
            Color fill = new Color(1f, 0.55f, 0.15f, 0.25f);
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
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0f, 0.5f), 16f);
        }
    }
}
