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
            Vector3 endPos = target.position;

            // Tính sát thương cơ bản: 16 + 40% INT + 30% STR
            int intVal = (playerStats != null) ? playerStats.Intelligence : 12;
            int strVal = (playerStats != null) ? playerStats.Strength : 8;
            int damage = baseDamage + Mathf.RoundToInt(intVal * 0.4f + strVal * 0.3f);

            // Tạo đạn năng lượng ma pháp bay từ người chơi tới mục tiêu
            GameObject bolt = new GameObject("BasicAttack_Missile");
            bolt.transform.position = startPos;

            var sr = bolt.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("magic_bolt") ?? Resources.Load<Sprite>("indicator_ground_circle");
            sr.color = new Color(0.2f, 0.9f, 1f, 0.95f);
            bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            sr.sortingOrder = 22;

            float speed = 18f;
            float totalDist = Vector3.Distance(startPos, endPos);
            float duration = Mathf.Clamp(totalDist / speed, 0.05f, 0.35f);
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bolt.transform.position = Vector3.Lerp(startPos, target.position, t);

                Vector3 dir = (target.position - bolt.transform.position);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                bolt.transform.rotation = Quaternion.Euler(0, 0, angle);

                yield return null;
            }

            Destroy(bolt);

            // Gây sát thương khi đạn chạm trúng mục tiêu!
            if (target != null && h != null && !h.IsDead)
            {
                h.TakeDamage(damage, "Player_Hero");
                FloatingDamageManager.Spawn(target.position + Vector3.up * 0.35f, damage, false);
            }
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
