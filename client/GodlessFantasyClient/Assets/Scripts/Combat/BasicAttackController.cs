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
        [SerializeField] private float basicAttackRange = 3.2f;  // Tầm đánh thường chuẩn (phải lại gần mới focus & đánh được)
        [SerializeField] private float loseFocusRange = 4.8f;    // Tầm mất Focus (khoảng đệm hysteresis chống giật)
        [SerializeField] private float loseFocusGracePeriod = 0.6f; // Thời gian đệm ra ngoài tầm trước khi mất dấu hẳn
        [SerializeField] private float attackCooldown = 0.65f;   // 0.65s mỗi đòn đánh (~1.5 đòn/s)
        [SerializeField] private int baseDamage = 16;

        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SkillAimingController skillAimingController;

        private Transform currentTarget = null;
        private Health targetHealth = null;
        private string targetDisplayName = "Mục Tiêu";
        private float lastAttackTime = -999f;
        private float outOfRangeTimer = 0f;
        private float lastOutRangeWarnTime = -999f;

        private Camera mainCam;
        private Texture2D reticleTex;
        private GUIStyle targetHeaderStyle;
        private GUIStyle targetHpStyle;
        private GUIStyle targetRangeStyle;

        public Transform CurrentTarget => currentTarget;
        public float BasicAttackRange => (playerStats != null) ? playerStats.DynamicBasicAttackRange : basicAttackRange;
        public float LoseFocusRange => BasicAttackRange * 1.35f;
        public float AttackCooldown
        {
            get
            {
                // Tốc đánh đặc trưng: Võ Sĩ đấm quyền liên hoàn 0.50s, Xạ Thủ 0.58s, Kị Sĩ 0.65s, Pháp Sư 0.75s
                string cls = (playerStats != null) ? playerStats.CharacterClass : "Kị Sĩ";
                float baseCd = attackCooldown;
                if (cls.Contains("Võ") || cls.Contains("Brawler")) baseCd = 0.50f;
                else if (cls.Contains("Xạ") || cls.Contains("Archer")) baseCd = 0.58f;
                else if (cls.Contains("Kị") || cls.Contains("Knight")) baseCd = 0.65f;
                else if (cls.Contains("Pháp") || cls.Contains("Mage")) baseCd = 0.75f;

                float agiReduction = (playerStats != null) ? playerStats.DynamicAttackSpeedBonus : 0f;
                return Mathf.Max(0.30f, baseCd - agiReduction);
            }
        }

        private void Awake()
        {
            if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (skillAimingController == null) skillAimingController = GetComponent<SkillAimingController>();
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
            if (GodlessFantasy.Core.GameInput.IsAuthOrCreationActive()) return;

            if (mainCam == null) mainCam = Camera.main;

            HandleTargetSelectionInput();
            HandleRightClickAttack();
            HandleTargetHysteresis();
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

        /// <summary>
        /// 1. CHỌN MỤC TIÊU (FOCUS CÓ ĐIỀU KIỆN):
        /// - Người chơi click chuột trái vào quái / mục tiêu.
        /// - Chỉ được phép Focus khi mục tiêu ở trong tầm đánh thường (<= basicAttackRange)!
        /// - Không cho phép focus/aim từ xa, không tự kéo nhân vật chạy đi!
        /// </summary>
        private void HandleTargetSelectionInput()
        {
            // Bỏ qua nếu đang hover trên UI modal
            if (PlayerMovement.BlockWorldClick || GodlessFantasy.UI.CombatHUD.IsMouseOverAnyUI()) return;

            // Click chuột trái vào quái hoặc người chơi đối thủ để chọn mục tiêu
            if (GameInput.GetLeftMouseButtonDown())
            {
                if (mainCam == null) mainCam = Camera.main;
                if (mainCam == null) return;
                Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(GameInput.GetMousePosition());

                if (TryFindCombatTargetAt(mouseWorldPos, out Transform targetTf, out Health targetH))
                {
                    float dist = Vector2.Distance(transform.position, targetTf.position);

                    // ĐIỀU KIỆN TIÊN QUYẾT: Đủ tầm đánh thường mới được Focus!
                    if (dist <= BasicAttackRange)
                    {
                        SetTarget(targetTf, targetH);
                        FloatingDamageManager.ShowFloatingText(targetTf.position + Vector3.up * 1f,
                            $"🎯 Đã khóa: {targetDisplayName}", new Color(0.4f, 1f, 0.4f));
                    }
                    else
                    {
                        // Ở ngoài tầm: Không cho phép focus từ xa, thông báo người chơi phải lại gần
                        FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.2f,
                            $"⚠️ Quá xa ({dist:F1}m > {BasicAttackRange:F1}m)! Hãy lại gần để khóa mục tiêu.", new Color(1f, 0.7f, 0.2f));
                    }
                    return;
                }
            }

            // Phím ESC: Hủy chọn mục tiêu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearTarget();
            }
        }

        /// <summary>
        /// Ưu tiên tuyệt đối mục tiêu MonsterTarget / PlayerStats (đối thủ) / IDamageable bằng Physics2D.OverlapPointAll:
        /// Chuột click vào quái hoặc người chơi đối thủ luôn chọn trúng 100%, bỏ qua các vật cản địa hình / loot / trigger.
        /// </summary>
        public bool TryFindCombatTargetAt(Vector2 point, out Transform targetTransform, out Health targetH)
        {
            targetTransform = null;
            targetH = null;

            Collider2D[] allHits = Physics2D.OverlapPointAll(point);
            if (allHits == null || allHits.Length == 0) return false;

            Collider2D bestCol = null;
            Health bestHealth = null;
            int bestScore = -1;

            foreach (var col in allHits)
            {
                if (col == null) continue;
                // Loại trừ bản thân người chơi đang điều khiển (cả root lẫn child hitbox)
                if (col.gameObject == gameObject || col.transform.root == transform.root) continue;
                // Loại trừ NPC dân làng thân thiện
                if (col.GetComponentInParent<VillageNPC>() != null) continue;

                var monster = col.GetComponentInParent<MonsterTarget>();
                var otherPlayer = col.GetComponentInParent<PlayerStats>();
                var dmg = col.GetComponentInParent<IDamageable>();
                var h = col.GetComponentInParent<Health>();

                if (monster == null && otherPlayer == null && dmg == null && h == null) continue;

                bool isAlive = (h != null && !h.IsDead) || (dmg != null && !dmg.IsDead);
                int score = 0;

                // Thang điểm ưu tiên tuyệt đối:
                // 1. Quái vật (MonsterTarget) hoặc Người chơi đối thủ (PlayerStats) còn sống: 100 điểm
                // 2. IDamageable / Health khác còn sống: 90 điểm
                // 3. Quái vật / Đối thủ đang chờ hồi sinh: 80 điểm
                // 4. IDamageable khác: 50 điểm
                if (monster != null || otherPlayer != null)
                {
                    score = isAlive ? 100 : 80;
                }
                else
                {
                    score = isAlive ? 90 : 50;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCol = col;
                    bestHealth = h;
                }
            }

            if (bestCol != null)
            {
                var parentTarget = bestCol.GetComponentInParent<MonsterTarget>()?.transform
                                ?? bestCol.GetComponentInParent<PlayerStats>()?.transform
                                ?? bestCol.GetComponentInParent<Health>()?.transform
                                ?? bestCol.transform;

                targetTransform = parentTarget;
                targetH = bestHealth ?? targetTransform.GetComponentInParent<Health>();
                return true;
            }

            return false;
        }

        private void SetTarget(Transform target, Health h)
        {
            currentTarget = target;
            targetHealth = h ?? target.GetComponentInParent<Health>();
            outOfRangeTimer = 0f;

            // Lấy tên hiển thị chuẩn (người chơi đối thủ, quái vật, bù nhìn)
            var otherPlayer = target.GetComponentInParent<PlayerStats>();
            var monster = target.GetComponentInParent<MonsterTarget>();
            var overhead = target.GetComponentInParent<OverheadStatusBar>();

            if (otherPlayer != null && !string.IsNullOrEmpty(otherPlayer.CharacterName))
            {
                targetDisplayName = otherPlayer.CharacterName;
            }
            else if (monster != null && !string.IsNullOrEmpty(monster.TargetName))
            {
                targetDisplayName = monster.TargetName;
            }
            else if (overhead != null)
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
            outOfRangeTimer = 0f;
        }

        /// <summary>
        /// 2. ĐÁNH THƯỜNG BẰNG CHUỘT PHẢI (TRÚNG ĐÍCH TỰ ĐỘNG KHI CLICK CHUỘT PHẢI):
        /// - Nhân vật KHÔNG tự động xả đòn khi đứng im.
        /// - Khi người chơi CLICK CHUỘT PHẢI: Nếu mục tiêu trong tầm thì chưởng trúng đích!
        /// - Skill vẫn độc lập: Nếu đang ngắm skill (SkillAimingController) thì nhường chuột phải cho tung skill.
        /// </summary>
        private void HandleRightClickAttack()
        {
            if (PlayerMovement.BlockWorldClick || GodlessFantasy.UI.CombatHUD.IsMouseOverAnyUI()) return;

            // Nếu đang trong trạng thái ngắm chiêu (SkillAimingController) thì chuột phải là tung skill, không can thiệp
            if (skillAimingController != null && skillAimingController.IsAiming) return;

            // Kiểm tra click chuột phải hoặc giữ chuột phải để gõ đòn đánh thường
            if (GameInput.GetRightMouseButtonDown() || Input.GetMouseButton(1))
            {
                // Nếu click chuột phải trực tiếp vào một mục tiêu cụ thể (quái vật hoặc người chơi đối thủ)
                if (mainCam != null && GameInput.GetRightMouseButtonDown())
                {
                    Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(GameInput.GetMousePosition());
                    if (TryFindCombatTargetAt(mouseWorldPos, out Transform targetTf, out Health targetH))
                    {
                        float distHit = Vector2.Distance(transform.position, targetTf.position);
                        if (distHit <= BasicAttackRange)
                        {
                            SetTarget(targetTf, targetH);
                            TryPerformAttack();
                            return;
                        }
                        else
                        {
                            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.2f,
                                $"⚠️ Mục tiêu ngoài tầm ({distHit:F1}m > {BasicAttackRange:F1}m)!", new Color(1f, 0.65f, 0.2f));
                            return;
                        }
                    }
                }

                // Nếu đã có mục tiêu Focus trước đó
                if (currentTarget != null)
                {
                    float dist = Vector2.Distance(transform.position, currentTarget.position);
                    if (dist <= BasicAttackRange)
                    {
                        TryPerformAttack();
                    }
                    else
                    {
                        // Ngoài tầm đánh thường nhưng còn trong tầm đệm giữ Focus
                        if (Time.time >= lastOutRangeWarnTime + 0.8f)
                        {
                            lastOutRangeWarnTime = Time.time;
                            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.2f,
                                $"⚠️ Mục tiêu ngoài tầm đánh ({dist:F1}m > {BasicAttackRange:F1}m)!", new Color(1f, 0.65f, 0.2f));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 3. CƠ CHẾ KHOẢNG ĐỆM & MẤT DẤU (HYSTERESIS BUFFER):
        /// - dist <= BasicAttackRange: Sẵn sàng đánh thường bằng chuột phải.
        /// - BasicAttackRange < dist <= LoseFocusRange: Ngừng đánh thường nhưng TẠM GIỮ FOCUS.
        /// - dist > LoseFocusRange: Ra khỏi tầm đệm -> Sau 0.6s mất dấu Focus hoàn toàn!
        /// - Khi mất Focus: Không tự động chuyển quái, người chơi phải áp sát lấy lại dấu.
        /// </summary>
        private void HandleTargetHysteresis()
        {
            if (currentTarget == null) return;

            // Mục tiêu đã chết hoặc bị tắt
            if (targetHealth != null && targetHealth.IsDead || !currentTarget.gameObject.activeInHierarchy)
            {
                ClearTarget();
                return;
            }

            float dist = Vector2.Distance(transform.position, currentTarget.position);

            if (dist <= BasicAttackRange)
            {
                // Ở trong tầm đánh thường: Reset timer
                outOfRangeTimer = 0f;
            }
            else if (dist <= LoseFocusRange)
            {
                // Khoảng đệm: Ngừng đánh thường nhưng TẠM GIỮ FOCUS!
                outOfRangeTimer = 0f;
            }
            else
            {
                // Vượt ra ngoài tầm mất Focus: Đếm thời gian đệm ngắn trước khi mất dấu hẳn
                outOfRangeTimer += Time.deltaTime;
                if (outOfRangeTimer >= loseFocusGracePeriod)
                {
                    Debug.Log($"<color=orange>[MẤT DẤU] Mục tiêu '{targetDisplayName}' ra khỏi tầm ({dist:F1}m > {LoseFocusRange:F1}m)!</color>");
                    FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.2f,
                        $"💨 Mất dấu mục tiêu (Cự ly {dist:F1}m)", new Color(0.85f, 0.85f, 0.85f));
                    ClearTarget();
                }
            }
        }

        private void TryPerformAttack()
        {
            if (currentTarget == null) return;
            if (Time.time < lastAttackTime + AttackCooldown) return;

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

            // Phát âm thanh phù hợp với chức nghiệp
            bool isCastClass = (charClass == "Pháp Sư" || charClass == "Xạ Thủ");
            if (isCastClass)
            {
                AudioManager.PlayCast();
            }
            else
            {
                AudioManager.PlaySlash();
            }

            // Kích hoạt vung vũ khí và bung vệt kiếm khí / đao khí nguyên tố của nhân vật
            GodlessFantasy.Player.CharacterVisualController8Dir.Instance?.TriggerAttack(element, isCastClass);

            // Chạy VFX phù hợp từng class
            yield return StartCoroutine(SpawnClassBasicAttackVFX(charClass, startPos, target, elemColor, isBuffed));

            // Gây sát thương khi đạn chạm trúng (hỗ trợ cả Quái vật lẫn Đối thủ Người chơi)
            if (target != null)
            {
                var dmgTarget = (h != null) ? (IDamageable)h : target.GetComponentInParent<IDamageable>();
                if (dmgTarget != null && !dmgTarget.IsDead)
                {
                    dmgTarget.TakeDamage(damage, "Player_Hero");
                    FloatingDamageManager.Spawn(target.position + Vector3.up * 0.35f, damage, isCrit: isBuffed);
                    AudioManager.PlayHit();
                }
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

            // Giữ lại duy nhất hình dạng vòng cung bán nguyệt sắc nét, bỏ vệt chưởng dài (No TrailRenderer)

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
        // VÕ SĨ: Chưởng quyền — Chùm nắm đấm Pixel Art sắc nét có ngón tay & vệt khí kình (Pixel Fist Strike)
        // ─────────────────────────────────────────────────────────
        private static Sprite fistShockwaveSprite = null;
        private Sprite GetOrCreateFistSprite()
        {
            if (fistShockwaveSprite != null) return fistShockwaveSprite;
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // Pixel art sắc nét 100%

            // Ma trận Pixel Art Nắm Đấm Chiến Binh (Brawler Knuckle Fist):
            // - Hướng đấm quay sang phải (+X), khớp 100% với góc xoay tiếp tuyến Atan2(dy, dx).
            // - Cổ tay ở bên trái (X: 3..11), Mu bàn tay ở giữa (X: 12..15).
            // - 4 Đốt ngón tay (Knuckles) gập vuông góc ở mặt trước (X: 16..23), ngón giữa nhô cao nhất.
            // - Rãnh chia ngón tay '#' tạo độ tương phản khối retro rõ nét.
            // - Đỉnh khớp ngón '*' sáng chấn quang.
            // - Vệt sóng kình khí '^' xé gió phía trước.
            string[] fistMap = new string[]
            {
                "................................", // Y=0
                "................................", // Y=1
                "................................", // Y=2
                "................................", // Y=3
                "................................", // Y=4
                "...........######...............", // Y=5
                ".........##oooooo##.............", // Y=6  Ngón cái cuộn gập ở dưới
                "........#oooooooooo#............", // Y=7
                ".......#oooooooooooo#....^......", // Y=8
                "......#ooooooooooooo*#..^^......", // Y=9  Ngón út
                "....##oooooooooooooo*#...^......", // Y=10
                "...#oooooooo#oooooooo#..........", // Y=11 Rãnh ngón út - áp út
                "..#ooooooooo#oooooooo*#..^^.....", // Y=12 Ngón áp út
                ".#oooooooooo#ooooooooo*#.^^^....", // Y=13
                "#ooooooooooo#ooooooooo*#..^^....", // Y=14
                "#ooooooooooo#oooooooooo*#..^....", // Y=15 Ngón giữa (đấm mạnh nhất, vươn xa nhất)
                "#ooooooooooo#oooooooooo*#.^^....", // Y=16
                ".#oooooooooo#ooooooooo*#.^^^....", // Y=17
                "..#ooooooooo#oooooooo*#..^^.....", // Y=18 Rãnh ngón giữa - trỏ
                "...#oooooooo#oooooooo#..........", // Y=19
                "....##oooooo#ooooooo*#...^......", // Y=20 Ngón trỏ
                "......#ooooooooooooo*#..^^......", // Y=21
                ".......#oooooooooooo#....^......", // Y=22
                "........#oooooooooo#............", // Y=23
                ".........##oooooo##.............", // Y=24
                "...........######...............", // Y=25
                "................................", // Y=26
                "................................", // Y=27
                "................................", // Y=28
                "................................", // Y=29
                "................................", // Y=30
                "................................"  // Y=31
            };

            for (int y = 0; y < size; y++)
            {
                string row = fistMap[y];
                for (int x = 0; x < size; x++)
                {
                    char c = (x < row.Length) ? row[x] : '.';
                    Color col;
                    switch (c)
                    {
                        case '#': col = new Color(0.28f, 0.30f, 0.42f, 0.95f); break; // Viền bóng / Rãnh ngón
                        case 'o': col = new Color(0.92f, 0.94f, 1.0f, 1.0f); break;  // Thân nắm đấm
                        case '*': col = new Color(1.0f, 1.0f, 1.0f, 1.0f); break;    // Đỉnh đốt ngón tay phát quang
                        case '^': col = new Color(0.70f, 0.88f, 1.0f, 0.75f); break; // Vệt kình khí rẽ gió
                        default:  col = Color.clear; break;
                    }
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            fistShockwaveSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.45f, 0.5f), 32f);
            return fistShockwaveSprite;
        }

        private IEnumerator VFX_WarriorPalm(Vector3 startPos, Transform target, Color elemColor, bool isBuffed)
        {
            if (target == null) yield break;

            // Bắn ra chính xác 2 chưởng nắm đấm đối xứng uốn lượn chéo nhau (Double Symmetrical Crossed Fist Strike)
            // Đúng theo hình vẽ tay của User: 2 quyền trái & phải tách biệt, cắt chéo nhau đối xứng, khoảng cách rõ ràng, không rối mắt!
            StartCoroutine(FistSymmetricalProjectile(startPos, target, elemColor, isBuffed, 1));  // Chưởng quyền trái (+1)
            StartCoroutine(FistSymmetricalProjectile(startPos, target, elemColor, isBuffed, -1)); // Chưởng quyền phải (-1)

            float duration = 0.28f;
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator FistSymmetricalProjectile(Vector3 startPos, Transform target, Color elemColor, bool isBuffed, int side)
        {
            if (target == null) yield break;

            GameObject fist = new GameObject($"BA_WarriorFist_{(side > 0 ? "Left" : "Right")}");
            fist.transform.position = startPos;

            var sr = fist.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateFistSprite();
            sr.color = elemColor;
            float sc = isBuffed ? 0.38f : 0.30f; // Kích thước pixel gọn gàng, sắc nét, chuẩn võ sĩ chibi
            fist.transform.localScale = new Vector3(sc, sc, 1f);
            sr.sortingOrder = 22;

            var trail = fist.AddComponent<TrailRenderer>();
            trail.time = 0.14f;
            trail.startWidth = sc * 0.55f;
            trail.endWidth = 0.02f;
            trail.sortingOrder = 21;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(elemColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = g;

            Vector3 baseDir = (target.position - startPos).normalized;
            Vector3 perpDir = new Vector3(-baseDir.y, baseDir.x, 0f); // Trục vuông góc uốn lượn

            float totalDist = Vector3.Distance(startPos, target.position);
            float speed = 16f;
            float duration = Mathf.Clamp(totalDist / speed, 0.15f, 0.38f);
            float elapsed = 0f;

            // Biên độ cong đối xứng hình chữ X/thấu kính theo đúng hình vẽ tay
            float maxAmplitude = Mathf.Clamp(totalDist * 0.25f, 0.40f, 0.75f);
            Vector3 lastPos = startPos;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Đường bay thẳng tiến tới mục tiêu
                Vector3 directPos = Vector3.Lerp(startPos, target.position, t);

                // Quỹ đạo uốn cong đối xứng chuẩn xác theo hình vẽ tay của User:
                // Tại t=0: dãn cách hai bên vai (wave = -1)
                // Tại t=0.25: cắt chéo nhau lần 1 (wave = 0)
                // Tại t=0.5: đổi bên dãn cách rộng hình thấu kính (wave = +1) -> khoảng cách rộng rãi không rối mắt
                // Tại t=0.75: cắt chéo nhau lần 2 (wave = 0)
                // Tại t=0.75..1.0: hội tụ mượt mà vào mục tiêu
                float wave = -Mathf.Cos(t * Mathf.PI * 2f);
                float envelope = t < 0.75f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.75f) / 0.25f);
                float lateralOffset = side * maxAmplitude * wave * envelope;
                Vector3 currentPos = directPos + perpDir * lateralOffset;
                fist.transform.position = currentPos;

                // Xoay nắm đấm theo tiếp tuyến quỹ đạo chuyển động thực tế
                Vector3 moveDelta = currentPos - lastPos;
                if (moveDelta.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(moveDelta.y, moveDelta.x) * Mathf.Rad2Deg;
                    fist.transform.rotation = Quaternion.Euler(0, 0, angle);
                }
                lastPos = currentPos;

                yield return null;
            }

            // Hiệu ứng nổ quyền khi chạm mục tiêu
            if (target != null)
            {
                FloatingDamageManager.ShowFloatingText(target.position, "💥", elemColor);
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

            // Màu sắc ngắm phản hồi trực quan theo 3 vùng cự ly:
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            Color pulseColor;
            if (dist <= basicAttackRange)
            {
                // Vùng 1: Trong tầm đánh (Xanh lục / Vàng)
                pulseColor = Color.Lerp(new Color(0.3f, 1f, 0.4f), new Color(1f, 0.88f, 0.2f), Mathf.PingPong(Time.time * 4f, 1f));
            }
            else if (dist <= loseFocusRange)
            {
                // Vùng 2: Khoảng đệm (Vàng cam tạm giữ Focus)
                pulseColor = Color.Lerp(new Color(1f, 0.8f, 0.2f), new Color(1f, 0.5f, 0.1f), Mathf.PingPong(Time.time * 4f, 1f));
            }
            else
            {
                // Vùng 3: Vượt ra ngoài tầm (Đỏ - sắp mất dấu)
                pulseColor = new Color(1f, 0.25f, 0.25f, 0.95f);
            }

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
            float bannerW = 285f;
            float bannerH = 46f;
            float bannerX = (Screen.width - bannerW) / 2f;
            float bannerY = 48f; // Hạ xuống dưới thước đo khoảng cách (tránh đè lên ruler đỉnh màn hình)
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

            // Thông tin cự ly & trạng thái theo 3 vùng hysteresis
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            string rangeText;
            if (dist <= basicAttackRange)
            {
                rangeText = $"<color=#55ff55>Cự ly: {dist:F1}m • [TRONG TẦM ĐÁNH {basicAttackRange:F1}m - CHUỘT PHẢI ĐÁNH]</color>";
            }
            else if (dist <= loseFocusRange)
            {
                rangeText = $"<color=#ffbb33>Cự ly: {dist:F1}m • [KHOẢNG ĐỆM {loseFocusRange:F1}m - TẠM GIỮ FOCUS]</color>";
            }
            else
            {
                float remain = Mathf.Max(0f, loseFocusGracePeriod - outOfRangeTimer);
                rangeText = $"<color=#ff4444>Cự ly: {dist:F1}m • [NGOÀI TẦM - MẤT DẤU TRONG {remain:F1}s]</color>";
            }

            GUI.Label(new Rect(r.x + 10, r.y + 32, r.width - 20, 13), rangeText, targetRangeStyle);
        }
    }
}
