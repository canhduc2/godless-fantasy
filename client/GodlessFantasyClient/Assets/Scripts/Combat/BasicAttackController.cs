using System.Collections;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Core;
using GodlessFantasy.UI;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Hệ Thống Đánh Thường & Khóa Mục Tiêu (Basic Attack & Target Lock Engine):
    /// - Chuột trái vào quái / mục tiêu: Khóa mục tiêu (Target Reticle) & tự động tấn công.
    /// - Tầm đánh thường (Basic Attack Range): 5.0m (Tầm Xa Phép của Pháp Sư / Hero).
    /// - Nếu ở ngoài tầm: Tự động chạy lại gần cho tới khi vào tầm (<= 5.0m) rồi dừng lại bắn đòn đánh thường.
    /// - Khi bị đánh: Cả Quái lẫn Người Chơi đều chớp đỏ (Hit Flash) rõ nét đặc trưng MMORPG.
    /// - Phím [T]: Thử nhận sát thương lên người chơi để test hiệu ứng chớp đỏ bất cứ lúc nào!
    /// </summary>
    public class BasicAttackController : MonoBehaviour
    {
        [Header("Basic Attack Settings")]
        [SerializeField] private float basicAttackRange = 5.0f; // 5.0 mét chuẩn tầm xa
        [SerializeField] private float attackCooldown = 0.65f;   // 0.65s mỗi đòn đánh (~1.5 đòn/s)
        [SerializeField] private int baseDamage = 16;

        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Transform currentTarget = null;
        private Health targetHealth = null;
        private string targetDisplayName = "Mục Tiêu";
        private float lastAttackTime = -999f;
        private bool isAutoApproaching = false;

        private Camera mainCam;
        private Texture2D reticleTex;
        private GUIStyle targetHeaderStyle;
        private GUIStyle targetHpStyle;
        private GUIStyle targetRangeStyle;

        public Transform CurrentTarget => currentTarget;
        public float BasicAttackRange => basicAttackRange;
        public float AttackCooldown => attackCooldown;

        private void Awake()
        {
            if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            mainCam = Camera.main;

            CreateTextures();
        }

        private void CreateTextures()
        {
            reticleTex = new Texture2D(1, 1);
            reticleTex.SetPixel(0, 0, Color.white);
            reticleTex.Apply();
        }

        private void Update()
        {
            if (mainCam == null) mainCam = Camera.main;

            HandleTargetSelectionInput();
            HandleAutoApproachAndAttack();
            HandleTestPlayerDamage();
        }

        /// <summary>
        /// Phím [T]: Thử nhận 15 sát thương lên Người Chơi để test hiệu ứng chớp đỏ người chơi
        /// </summary>
        private void HandleTestPlayerDamage()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                var pHealth = GetComponent<Health>();
                if (pHealth != null && !pHealth.IsDead)
                {
                    Debug.Log("<color=red>[TEST HIT FLASH] Người chơi nhận 15 sát thương thử nghiệm!</color>");
                    pHealth.TakeDamage(15, "Thử Nghiệm [T]");
                }
            }
        }

        private void HandleTargetSelectionInput()
        {
            // Bỏ qua nếu đang hover trên UI modal
            if (PlayerMovement.BlockWorldClick) return;

            // Click chuột trái vào quái để khóa mục tiêu
            if (GameInput.GetLeftMouseButtonDown())
            {
                if (mainCam == null) return;
                Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(GameInput.GetMousePosition());
                Collider2D hit = Physics2D.OverlapPoint(new Vector2(mouseWorldPos.x, mouseWorldPos.y));

                if (hit != null && hit.gameObject != gameObject)
                {
                    var h = hit.GetComponent<Health>();
                    var dmg = hit.GetComponent<IDamageable>();
                    var monster = hit.GetComponent<MonsterTarget>();

                    if (h != null || dmg != null || monster != null)
                    {
                        // Chọn mục tiêu
                        SetTarget(hit.transform, h);

                        float dist = Vector2.Distance(transform.position, currentTarget.position);
                        if (dist <= basicAttackRange)
                        {
                            // Trong tầm: Dừng lại và đánh ngay
                            if (playerMovement != null) playerMovement.StopMoving();
                            isAutoApproaching = false;
                            TryPerformAttack();
                        }
                        else
                        {
                            // Ngoài tầm: Bắt đầu tự động tiếp cận
                            isAutoApproaching = true;
                            if (playerMovement != null) playerMovement.MoveTo(currentTarget.position);
                        }
                        return;
                    }
                }
                else
                {
                    // Click đất trống: Nếu click xa thì hủy tự động tiếp cận
                    isAutoApproaching = false;
                }
            }

            // Phím ESC: Hủy chọn mục tiêu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearTarget();
            }
        }

        private void SetTarget(Transform target, Health h)
        {
            currentTarget = target;
            targetHealth = h ?? target.GetComponent<Health>();

            // Lấy tên hiển thị
            var overhead = target.GetComponent<OverheadStatusBar>();
            if (overhead != null)
            {
                targetDisplayName = target.name.Replace("Dummy_", "").Replace("_", " ");
            }
            else
            {
                targetDisplayName = target.name;
            }
        }

        public void ClearTarget()
        {
            currentTarget = null;
            targetHealth = null;
            isAutoApproaching = false;
        }

        private void HandleAutoApproachAndAttack()
        {
            if (currentTarget == null) return;

            // Mục tiêu đã chết hoặc bị tắt
            if (targetHealth != null && targetHealth.IsDead || !currentTarget.gameObject.activeInHierarchy)
            {
                ClearTarget();
                return;
            }

            float dist = Vector2.Distance(transform.position, currentTarget.position);

            if (isAutoApproaching)
            {
                // Cập nhật vị trí di chuyển theo mục tiêu
                if (playerMovement != null) playerMovement.SetTargetPosition(currentTarget.position);

                if (dist <= basicAttackRange)
                {
                    // Đã vào tầm đánh: Dừng lại và xả đòn!
                    isAutoApproaching = false;
                    if (playerMovement != null) playerMovement.StopMoving();
                    TryPerformAttack();
                }
            }
            else
            {
                // Nếu đang đứng trong tầm và giữ khoảng cách thì tiếp tục đánh khi hồi chiêu
                if (dist <= basicAttackRange && GameInput.GetLeftMouseButton())
                {
                    TryPerformAttack();
                }
            }
        }

        private void TryPerformAttack()
        {
            if (currentTarget == null) return;
            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            // Xoay mặt về phía mục tiêu
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (currentTarget.position.x < transform.position.x);
            }

            StartCoroutine(ExecuteBasicAttackRoutine(currentTarget, targetHealth));
        }

        private IEnumerator ExecuteBasicAttackRoutine(Transform target, Health h)
        {
            if (target == null) yield break;

            Vector3 startPos = transform.position + Vector3.up * 0.1f;

            bool isBuffed = (playerStats != null && playerStats.GetDamageBuffMultiplier() > 0f);
            int damage = playerStats != null
                ? playerStats.CalculateBasicAttackDamage()
                : baseDamage + Mathf.RoundToInt(12 * 0.4f + 8 * 0.3f);

            string charClass = playerStats != null ? playerStats.CharacterClass : "Kị Sĩ";
            string element   = playerStats != null ? playerStats.Element : "Hỏa";

            // Màu theo thuộc tính (dùng lại GetElementColor từ SkillVFXManager)
            Color elemColor = SkillVFXManager.Instance != null
                ? SkillVFXManager.Instance.GetElementColor(element, new Color(0.25f, 0.85f, 1f))
                : new Color(0.25f, 0.85f, 1f);

            // Buff tăng độ bão hoà màu: khi buff ON thì màu sáng hơn, có thêm glow vàng cam
            if (isBuffed) elemColor = Color.Lerp(elemColor, new Color(1f, 0.75f, 0.1f), 0.35f);

            // Chạy VFX phù hợp từng class
            yield return StartCoroutine(SpawnClassBasicAttackVFX(charClass, startPos, target, elemColor, isBuffed));

            // Gây sát thương khi đạn chạm trúng
            if (target != null && h != null && !h.IsDead)
            {
                h.TakeDamage(damage, "Player_Hero");
                FloatingDamageManager.Spawn(target.position + Vector3.up * 0.35f, damage, isCrit: isBuffed);
            }
        }

        /// <summary>
        /// Dispatch VFX đánh thường theo từng chức nghiệp:
        /// - Pháp Sư  → Cầu phép tròn (magic orb) bay theo đường parabol nhẹ, có trail sáng
        /// - Kị Sĩ    → Kiếm khí (slash streak) — vệt kéo dài, xoay 45° bay thẳng tới mục tiêu
        /// - Võ Sĩ    → Chưởng (palm shockwave) — 2 vòng sóng tỏa ra từ người rồi hội tụ vào mục tiêu
        /// - Xạ Thủ   → Mũi tên (arrow) mảnh, bay thẳng nhanh, trail mảnh như luồng khí
        /// </summary>
        private IEnumerator SpawnClassBasicAttackVFX(string charClass, Vector3 startPos, Transform target, Color elemColor, bool isBuffed)
        {
            string cls = charClass?.ToLower() ?? "";

            if (cls.Contains("pháp") || cls.Contains("mage"))
                yield return StartCoroutine(VFX_MageOrb(startPos, target, elemColor, isBuffed));
            else if (cls.Contains("kị") || cls.Contains("knight"))
                yield return StartCoroutine(VFX_KnightSlash(startPos, target, elemColor, isBuffed));
            else if (cls.Contains("võ") || cls.Contains("warrior"))
                yield return StartCoroutine(VFX_WarriorPalm(startPos, target, elemColor, isBuffed));
            else if (cls.Contains("xạ") || cls.Contains("archer"))
                yield return StartCoroutine(VFX_ArcherArrow(startPos, target, elemColor, isBuffed));
            else
                yield return StartCoroutine(VFX_MageOrb(startPos, target, elemColor, isBuffed)); // fallback
        }

        // ─────────────────────────────────────────────────────────
        // PHÁP SƯ: Cầu phép tròn màu thuộc tính có trail sáng
        // ─────────────────────────────────────────────────────────
        private IEnumerator VFX_MageOrb(Vector3 startPos, Transform target, Color elemColor, bool isBuffed)
        {
            GameObject orb = new GameObject("BA_MageOrb");
            orb.transform.position = startPos;

            var sr = orb.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("magic_bolt") ?? Resources.Load<Sprite>("indicator_ground_circle");
            sr.color = elemColor;
            float scale = isBuffed ? 0.55f : 0.42f;
            orb.transform.localScale = new Vector3(scale, scale, 1f);
            sr.sortingOrder = 22;

            // Trail glow mờ dài, màu thuộc tính
            var trail = orb.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = scale * 0.8f;
            trail.endWidth = 0.02f;
            trail.sortingOrder = 21;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(elemColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = g;

            float speed = 16f;
            float totalDist = Vector3.Distance(startPos, target != null ? target.position : startPos);
            float duration = Mathf.Clamp(totalDist / speed, 0.06f, 0.38f);
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Nhẹ arc parabol theo chiều Y
                float arc = Mathf.Sin(t * Mathf.PI) * 0.25f;
                Vector3 pos = Vector3.Lerp(startPos, target.position, t);
                pos.y += arc;
                orb.transform.position = pos;
                // Quay vòng nhẹ
                orb.transform.Rotate(0, 0, 360f * Time.deltaTime * 3f);
                yield return null;
            }
            Destroy(orb);
        }

        // ─────────────────────────────────────────────────────────
        // KỊ SĨ: Kiếm khí — Hình dạng vòng cung bán nguyệt (Crescent Arc Blade)
        // ─────────────────────────────────────────────────────────
        private static Sprite crescentBladeSprite = null;
        private Sprite GetOrCreateCrescentSprite()
        {
            if (crescentBladeSprite != null) return crescentBladeSprite;
            int w = 64;
            int h = 64;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(20f, 32f);
            float outerR = 30f;
            float innerR = 18f;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;

                    if (dist >= innerR && dist <= outerR && angle >= -65f && angle <= 65f)
                    {
                        // Vuốt nhọn hai đầu cánh cung và sáng ở cạnh ngoài
                        float edgeFactor = 1f - Mathf.Abs(dist - (innerR + outerR) * 0.5f) / ((outerR - innerR) * 0.5f);
                        float angleFactor = 1f - Mathf.Abs(angle) / 65f;
                        float alpha = Mathf.Clamp01(edgeFactor * angleFactor * 1.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            crescentBladeSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
            return crescentBladeSprite;
        }

        private IEnumerator VFX_KnightSlash(Vector3 startPos, Transform target, Color elemColor, bool isBuffed)
        {
            if (target == null) yield break;

            Vector3 dir = (target.position - startPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Kiếm khí hình vòng cung bán nguyệt (Crescent Arc) bay quét tới mục tiêu
            GameObject slash = new GameObject("BA_KnightCrescentSlash");
            slash.transform.position = startPos;
            slash.transform.rotation = Quaternion.Euler(0, 0, angle);

            var sr = slash.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateCrescentSprite();
            sr.color = elemColor;
            float sc = isBuffed ? 1.4f : 1.1f;
            slash.transform.localScale = new Vector3(sc, sc * 1.25f, 1f);
            sr.sortingOrder = 22;

            // Trail kiếm khí vòng cung cong phát sáng
            var trail = slash.AddComponent<TrailRenderer>();
            trail.time = 0.16f;
            trail.startWidth = 0.35f * sc;
            trail.endWidth = 0.02f;
            trail.sortingOrder = 21;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(elemColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = g;

            float speed = 20f;
            float totalDist = Vector3.Distance(startPos, target.position);
            float duration = Mathf.Clamp(totalDist / speed, 0.06f, 0.35f);
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                slash.transform.position = Vector3.Lerp(startPos, target.position, t);
                // Vòng cung nghiêng nhẹ theo quỹ đạo chém
                slash.transform.rotation = Quaternion.Euler(0, 0, angle + Mathf.Sin(t * Mathf.PI * 2f) * 12f);
                yield return null;
            }
            Destroy(slash);
        }

        // ─────────────────────────────────────────────────────────
        // VÕ SĨ: Chưởng quyền — Chùm nhiều nắm đấm mở rộng theo hình vòng cung (Expanding Fist Arc Volley)
        // ─────────────────────────────────────────────────────────
        private static Sprite fistShockwaveSprite = null;
        private Sprite GetOrCreateFistSprite()
        {
            if (fistShockwaveSprite != null) return fistShockwaveSprite;
            int size = 48;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Vector2 c = new Vector2(24f, 24f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), c);
                    // Tạo khối nắm đấm quyền kình (lõi tròn đặc + ngón tay nổi)
                    if (dist <= 18f)
                    {
                        float alpha = (dist <= 14f) ? 1.0f : (18f - dist) / 4f;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            fistShockwaveSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
            return fistShockwaveSprite;
        }

        private IEnumerator VFX_WarriorPalm(Vector3 startPos, Transform target, Color elemColor, bool isBuffed)
        {
            if (target == null) yield break;

            Vector3 baseDir = (target.position - startPos).normalized;
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

            // Bắn ra chùm 4 nắm đấm xòe mở rộng theo hình vòng cung (Arc Spread: -36°, -12°, +12°, +36°)
            float[] arcAngles = new float[] { -36f, -12f, 12f, 36f };
            for (int i = 0; i < arcAngles.Length; i++)
            {
                float angleOffset = arcAngles[i];
                float delay = i * 0.03f;
                StartCoroutine(FistArcProjectile(startPos, target, elemColor, isBuffed, baseAngle + angleOffset, delay, i));
            }

            float duration = 0.32f;
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator FistArcProjectile(Vector3 startPos, Transform target, Color elemColor, bool isBuffed, float launchAngle, float delay, int fistIdx)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            if (target == null) yield break;

            GameObject fist = new GameObject($"BA_WarriorFist_{fistIdx}");
            fist.transform.position = startPos;
            fist.transform.rotation = Quaternion.Euler(0, 0, launchAngle);

            var sr = fist.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateFistSprite();
            sr.color = elemColor;
            float sc = isBuffed ? 0.65f : 0.50f; // Kích thước to rõ, dễ nhìn từ xa
            fist.transform.localScale = new Vector3(sc, sc, 1f);
            sr.sortingOrder = 22;

            var trail = fist.AddComponent<TrailRenderer>();
            trail.time = 0.15f;
            trail.startWidth = sc * 0.7f;
            trail.endWidth = 0.05f;
            trail.sortingOrder = 21;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(elemColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = g;

            // Hướng xòe ban đầu theo vòng cung
            Vector3 spreadDir = new Vector3(Mathf.Cos(launchAngle * Mathf.Deg2Rad), Mathf.Sin(launchAngle * Mathf.Deg2Rad), 0);
            float totalDist = Vector3.Distance(startPos, target.position);
            float speed = 18f;
            float duration = Mathf.Clamp(totalDist / speed, 0.08f, 0.38f);
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Quỹ đạo: mở rộng ra hình vòng cung lúc đầu (t < 0.4), sau đó hội tụ mạnh vào mục tiêu (t >= 0.4)
                Vector3 directPos = Vector3.Lerp(startPos, target.position, t);
                Vector3 arcOffset = spreadDir * (Mathf.Sin(t * Mathf.PI) * 0.45f);
                fist.transform.position = directPos + arcOffset;

                // Quay nhẹ theo xung chấn quyền
                fist.transform.Rotate(0, 0, (fistIdx % 2 == 0 ? 360f : -360f) * Time.deltaTime * 2f);
                yield return null;
            }

            Destroy(fist);
        }

        // ─────────────────────────────────────────────────────────
        // XẠ THỦ: Mũi tên — hình mảnh bay thẳng siêu nhanh
        // ─────────────────────────────────────────────────────────
        private IEnumerator VFX_ArcherArrow(Vector3 startPos, Transform target, Color elemColor, bool isBuffed)
        {
            if (target == null) yield break;

            Vector3 dir = (target.position - startPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            GameObject arrow = new GameObject("BA_ArcherArrow");
            arrow.transform.position = startPos;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

            var sr = arrow.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("magic_bolt") ?? Resources.Load<Sprite>("indicator_ground_circle");
            sr.color = elemColor;
            // Mũi tên: rất hẹp rất dài
            float arrowW = isBuffed ? 1.35f : 1.0f;
            float arrowH = isBuffed ? 0.12f : 0.09f;
            arrow.transform.localScale = new Vector3(arrowW, arrowH, 1f);
            sr.sortingOrder = 22;

            // Trail siêu mảnh như làn khí sau mũi tên
            var trail = arrow.AddComponent<TrailRenderer>();
            trail.time = 0.10f;
            trail.startWidth = arrowH * 0.5f;
            trail.endWidth = 0f;
            trail.sortingOrder = 21;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            Gradient g = new Gradient();
            Color tipColor = Color.Lerp(elemColor, Color.white, 0.5f);
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(tipColor, 0f), new GradientColorKey(elemColor, 0.4f), new GradientColorKey(Color.clear, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 0.4f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = g;

            float speed = 28f; // Xạ thủ bắn nhanh nhất
            float totalDist = Vector3.Distance(startPos, target.position);
            float duration = Mathf.Clamp(totalDist / speed, 0.04f, 0.28f);
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                arrow.transform.position = Vector3.Lerp(startPos, target.position, t);
                // Giữ góc xoay luôn trỏ về phía mục tiêu
                Vector3 d = (target.position - arrow.transform.position);
                if (d.sqrMagnitude > 0.001f)
                    arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                yield return null;
            }
            Destroy(arrow);
        }

        private void InitStyles()
        {
            if (targetHeaderStyle == null)
            {
                targetHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(1f, 0.9f, 0.45f) }
                };

                targetHpStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };

                targetRangeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 9,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.3f, 0.9f, 1f) }
                };
            }
        }

        private void OnGUI()
        {
            if (currentTarget == null) return;
            if (targetHealth != null && targetHealth.IsDead) return;

            InitStyles();

            // 1. VẼ VÒNG KHÓA MỤC TIÊU TRONG THẾ GIỚI (TARGET RETICLE CORNER BRACKETS)
            DrawWorldTargetReticle();

            // 2. VẼ THANH TRẠNG THÁI MỤC TIÊU Ở ĐỈNH MÀN HÌNH (TARGET HUD BANNER)
            DrawTopTargetBanner();
        }

        private void DrawWorldTargetReticle()
        {
            if (mainCam == null) return;
            Vector3 screenPos = mainCam.WorldToScreenPoint(currentTarget.position);
            if (screenPos.z < 0) return; // Nằm sau camera

            float guiY = Screen.height - screenPos.y;
            float boxSize = 42f;
            Rect targetBox = new Rect(screenPos.x - boxSize / 2f, guiY - boxSize / 2f, boxSize, boxSize);

            // Màu nhấp nháy vàng - đỏ
            Color pulseColor = Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(1f, 0.25f, 0.25f), Mathf.PingPong(Time.time * 5f, 1f));

            // Vẽ 4 góc ngắm [  ]
            float armLen = 8f;
            float thick = 2f;
            GUI.color = pulseColor;

            // Góc trên-trái
            GUI.DrawTexture(new Rect(targetBox.x, targetBox.y, armLen, thick), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(targetBox.x, targetBox.y, thick, armLen), Texture2D.whiteTexture);

            // Góc trên-phải
            GUI.DrawTexture(new Rect(targetBox.x + targetBox.width - armLen, targetBox.y, armLen, thick), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(targetBox.x + targetBox.width - thick, targetBox.y, thick, armLen), Texture2D.whiteTexture);

            // Góc dưới-trái
            GUI.DrawTexture(new Rect(targetBox.x, targetBox.y + targetBox.height - thick, armLen, thick), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(targetBox.x, targetBox.y + targetBox.height - armLen, thick, armLen), Texture2D.whiteTexture);

            // Góc dưới-phải
            GUI.DrawTexture(new Rect(targetBox.x + targetBox.width - armLen, targetBox.y + targetBox.height - thick, armLen, thick), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(targetBox.x + targetBox.width - thick, targetBox.y + targetBox.height - armLen, thick, armLen), Texture2D.whiteTexture);

            GUI.color = Color.white;
        }

        private void DrawTopTargetBanner()
        {
            float bannerW = 270f;
            float bannerH = 46f;
            float bannerX = (Screen.width - bannerW) / 2f;
            float bannerY = 12f;
            Rect r = new Rect(bannerX, bannerY, bannerW, bannerH);

            // Nền tối
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Viền vàng kim
            FantasyUITheme.DrawDoubleGoldFrame(r, 1);

            // Tên mục tiêu
            GUI.Label(new Rect(r.x + 8, r.y + 3, r.width - 34, 16), $"🎯 {targetDisplayName}", targetHeaderStyle);

            // Nút [X] hủy khóa
            if (GUI.Button(new Rect(r.x + r.width - 24, r.y + 3, 18, 16), "✕"))
            {
                ClearTarget();
                return;
            }

            // Thanh máu của mục tiêu
            float curHp = (targetHealth != null) ? targetHealth.CurrentHealth : 100;
            float maxHp = (targetHealth != null) ? targetHealth.MaxHealth : 100;
            float hpRatio = Mathf.Clamp01(curHp / maxHp);

            Rect hpBarRect = new Rect(r.x + 10, r.y + 20, r.width - 20, 12);
            GUI.color = new Color(0.15f, 0.18f, 0.22f, 1f);
            GUI.DrawTexture(hpBarRect, Texture2D.whiteTexture);

            GUI.color = new Color(0.95f, 0.25f, 0.25f, 1f);
            GUI.DrawTexture(new Rect(hpBarRect.x, hpBarRect.y, hpBarRect.width * hpRatio, hpBarRect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(hpBarRect, $"{curHp:0}/{maxHp:0} HP", targetHpStyle);

            // Thông tin cự ly & trạng thái trong tầm đánh
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            bool inRange = dist <= basicAttackRange;
            string rangeText = inRange
                ? $"<color=#55ff55>Cự ly: {dist:F1}m • [TRONG TẦM ĐÁNH 5.0m]</color>"
                : $"<color=#ffaa33>Cự ly: {dist:F1}m • [NGOÀI TẦM - ĐANG TIẾP CẬN]</color>";

            GUI.Label(new Rect(r.x + 10, r.y + 32, r.width - 20, 13), rangeText, targetRangeStyle);
        }
    }
}
