using UnityEngine;
using GodlessFantasy.Combat;

namespace GodlessFantasy.AI
{
    public enum MonsterState
    {
        Idle,
        Wander,
        Chase,
        AttackTelegraph,
        Attacking,
        LeashReturn
    }

    /// <summary>
    /// AI Điều Khiển Quái Vật:
    /// - Di chuyển tuần tra ngẫu nhiên (Wander) trong bán kính giới hạn quanh điểm hồi sinh (Spawn Point).
    /// - Phát hiện mục tiêu người chơi (Chase) khi bước vào phạm vi dò tìm.
    /// - Báo hiệu chiêu thức (Telegraph đỏ dưới chân) trước khi tung đòn để người chơi kịp né (Dash [Shift]).
    /// - Cơ chế Buộc Dây (Leash): Vượt quá giới hạn di chuyển sẽ quay về Spawn Point và hồi đầy máu.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class MonsterAIController : MonoBehaviour
    {
        [Header("Patrol & Leash Boundaries")]
        public Vector2 spawnPoint;
        public float wanderRadius = 4.5f;
        public float leashRadius = 11.0f;
        public float wanderSpeed = 1.4f;
        public float chaseSpeed = 2.2f;

        [Header("Combat & Detection")]
        public float detectionRadius = 5.5f;
        public float attackRange = 1.8f;
        public float attackCooldown = 3.5f;
        public float telegraphDuration = 1.2f; // Thời gian nạp chiêu báo trước (Telegraph)
        public int attackDamage = 18;
        public bool isBoss = false;

        [Header("State")]
        public MonsterState currentState = MonsterState.Idle;

        private Health health;
        private Transform playerTransform;
        private Health playerHealth;
        private Vector2 wanderTarget;
        private float stateTimer = 0f;
        private float nextAttackTime = 0f;
        private SpriteRenderer spriteRenderer;

        // Telegraph Visual (Vòng tròn đỏ cảnh báo chiêu thức dưới chân)
        private GameObject telegraphObj;
        private SpriteRenderer telegraphRenderer;
        private float telegraphTimer = 0f;

        private void Awake()
        {
            health = GetComponent<Health>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            spawnPoint = transform.position;
            CreateTelegraphVisual();
        }

        private void Start()
        {
            FindPlayer();
            SetState(MonsterState.Idle);
            stateTimer = Random.Range(1.5f, 3.5f);
        }

        private void FindPlayer()
        {
            var p = GameObject.FindWithTag("Player");
            if (p == null)
            {
                var pm = FindAnyObjectByType<GodlessFantasy.Player.PlayerMovement>();
                if (pm != null) p = pm.gameObject;
            }
            if (p != null)
            {
                playerTransform = p.transform;
                playerHealth = p.GetComponent<Health>();
            }
        }

        private void CreateTelegraphVisual()
        {
            telegraphObj = new GameObject("AI_Telegraph_DangerCircle");
            telegraphObj.transform.parent = transform;
            telegraphObj.transform.localPosition = Vector3.zero;

            telegraphRenderer = telegraphObj.AddComponent<SpriteRenderer>();
            telegraphRenderer.sprite = CreateTelegraphDiskSprite();
            telegraphRenderer.sortingOrder = 1; // Dưới quái và player, trên mặt đất
            telegraphRenderer.color = new Color(1f, 0.15f, 0.15f, 0f);
            telegraphObj.SetActive(false);
        }

        private Sprite CreateTelegraphDiskSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 c = new Vector2(size / 2f, size / 2f);
            float r = size / 2f - 2f;

            Color edgeRed = new Color(1f, 0.1f, 0.1f, 0.95f);
            Color fillRed = new Color(1f, 0.2f, 0.2f, 0.35f);
            Color clear = new Color(0, 0, 0, 0);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (d <= r)
                    {
                        if (d >= r - 3.5f) tex.SetPixel(x, y, edgeRed);
                        else tex.SetPixel(x, y, fillRed);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        private void OnEnable()
        {
            MonsterAIManager.Register(this);
        }

        private void OnDisable()
        {
            MonsterAIManager.Unregister(this);
        }

        /// <summary>
        /// Được gọi tập trung từ MonsterAIManager (Zero C++ Bridge Overhead)
        /// </summary>
        public void TickAI(float dt, Vector3 playerPos, bool isPlayerStealth)
        {
            if (health != null && health.IsDead)
            {
                HideTelegraph();
                return;
            }

            float distFromSpawn = Vector2.Distance(transform.position, spawnPoint);
            float distToPlayer = Vector2.Distance(transform.position, playerPos);

            // Kiểm tra trạng thái ẩn nấp của người chơi
            if (isPlayerStealth && (currentState == MonsterState.Chase || currentState == MonsterState.AttackTelegraph))
            {
                SetState(MonsterState.Wander);
            }

            // 1. Kiểm tra Giới Hạn Leash: Vượt quá bán kính -> Quay đầu hồi máu
            if (currentState != MonsterState.LeashReturn && distFromSpawn > leashRadius)
            {
                SetState(MonsterState.LeashReturn);
            }

            switch (currentState)
            {
                case MonsterState.Idle:
                    UpdateIdle(distToPlayer);
                    break;

                case MonsterState.Wander:
                    UpdateWander(distToPlayer);
                    break;

                case MonsterState.Chase:
                    UpdateChase(distToPlayer);
                    break;

                case MonsterState.AttackTelegraph:
                    UpdateTelegraph();
                    break;

                case MonsterState.LeashReturn:
                    UpdateLeashReturn();
                    break;
            }
        }

        private void SetState(MonsterState newState)
        {
            currentState = newState;
            switch (newState)
            {
                case MonsterState.Idle:
                    stateTimer = Random.Range(1.8f, 3.8f);
                    HideTelegraph();
                    break;

                case MonsterState.Wander:
                    Vector2 randDir = Random.insideUnitCircle * wanderRadius;
                    wanderTarget = spawnPoint + randDir;
                    stateTimer = 5.0f; // Timeout nếu bị kẹt
                    HideTelegraph();
                    break;

                case MonsterState.Chase:
                    HideTelegraph();
                    break;

                case MonsterState.AttackTelegraph:
                    telegraphTimer = 0f;
                    ShowTelegraph();
                    GodlessFantasy.Core.AudioManager.PlayTelegraphCharge();
                    break;

                case MonsterState.LeashReturn:
                    HideTelegraph();
                    // Miễn nhiễm và phục hồi máu khi đang Leash
                    if (health != null) health.IsInSafeZone = true;
                    break;
            }
        }

        private void UpdateIdle(float distToPlayer)
        {
            // Kiểm tra thấy Player (chỉ khi không ẩn nấp)
            if (!GodlessFantasy.Player.PlayerMovement.IsPlayerInStealth && distToPlayer <= detectionRadius && playerHealth != null && !playerHealth.IsDead && !playerHealth.IsInSafeZone)
            {
                SetState(MonsterState.Chase);
                return;
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                SetState(MonsterState.Wander);
            }
        }

        private void UpdateWander(float distToPlayer)
        {
            // Kiểm tra thấy Player (chỉ khi không ẩn nấp)
            if (!GodlessFantasy.Player.PlayerMovement.IsPlayerInStealth && distToPlayer <= detectionRadius && playerHealth != null && !playerHealth.IsDead && !playerHealth.IsInSafeZone)
            {
                SetState(MonsterState.Chase);
                return;
            }

            // Di chuyển tới điểm Wander
            Vector2 curPos = transform.position;
            Vector2 toTarget = wanderTarget - curPos;
            if (toTarget.magnitude <= 0.2f)
            {
                SetState(MonsterState.Idle);
                return;
            }

            Vector2 moveStep = toTarget.normalized * (wanderSpeed * Time.deltaTime);
            transform.position = curPos + moveStep;
            FlipSprite(moveStep.x);

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f) SetState(MonsterState.Idle);
        }

        private void UpdateChase(float distToPlayer)
        {
            if (playerTransform == null || playerHealth == null || playerHealth.IsDead || playerHealth.IsInSafeZone)
            {
                SetState(MonsterState.Idle);
                return;
            }

            // Nếu người chơi chạy quá xa khỏi bán kính dò tìm
            if (distToPlayer > detectionRadius * 1.4f)
            {
                SetState(MonsterState.Idle);
                return;
            }

            // Nếu đã vào tầm đánh
            if (distToPlayer <= attackRange)
            {
                if (Time.time >= nextAttackTime)
                {
                    SetState(MonsterState.AttackTelegraph);
                    return;
                }
            }

            // Đuổi theo người chơi
            Vector2 curPos = transform.position;
            Vector2 toPlayer = (Vector2)playerTransform.position - curPos;
            Vector2 moveStep = toPlayer.normalized * (chaseSpeed * Time.deltaTime);
            transform.position = curPos + moveStep;
            FlipSprite(moveStep.x);
        }

        private void UpdateTelegraph()
        {
            telegraphTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(telegraphTimer / telegraphDuration);

            // Hiệu ứng vòng tròn nguy hiểm sạc dần (Telegraph Warning)
            if (telegraphRenderer != null && telegraphObj != null)
            {
                float pulse = 0.45f + Mathf.PingPong(telegraphTimer * 7f, 0.45f);
                telegraphRenderer.color = isBoss 
                    ? new Color(1f, 0.05f, 0.05f, pulse) 
                    : new Color(1f, 0.2f, 0.2f, pulse * 0.85f);

                // Đường kính sprite là 4.0m (bán kính 2.0m khi scale 1.0)
                // Đảm bảo bán kính vòng đỏ khớp chính xác 100% với attackRange
                float parentScale = transform.localScale.x > 0.01f ? transform.localScale.x : 1.0f;
                float targetScale = (attackRange / 2.0f) / parentScale;
                float sc = targetScale * (0.85f + progress * 0.25f);
                telegraphObj.transform.localScale = new Vector3(sc, sc, 1f);
            }

            if (telegraphTimer >= telegraphDuration)
            {
                // Tung đòn sát thương
                ExecuteAttack();
                nextAttackTime = Time.time + attackCooldown;
                SetState(MonsterState.Chase);
            }
        }

        private void ExecuteAttack()
        {
            HideTelegraph();

            // Hiệu ứng nổ va đập mặt đất
            SpawnAttackImpactVfx(transform.position, attackRange, isBoss);

            if (isBoss)
            {
                GodlessFantasy.Core.AudioManager.PlayBossSlam();
            }
            else
            {
                GodlessFantasy.Core.AudioManager.PlaySlash();
            }

            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist <= attackRange * 1.15f)
                {
                    // Người chơi không né ra khỏi vùng -> Nhận sát thương
                    var targetHealth = playerTransform.GetComponent<Health>();
                    if (targetHealth != null && !targetHealth.IsDead)
                    {
                        string attackerName = isBoss ? "Thủ Lĩnh Ma Thú" : "Quái Vật";
                        targetHealth.TakeDamage(attackDamage, attackerName);
                        FloatingDamageManager.Spawn(playerTransform.position, attackDamage, isBoss);

                        if (isBoss)
                        {
                            var cam = Camera.main != null ? Camera.main.GetComponent<GodlessFantasy.Player.CameraController>() : null;
                            cam?.Shake(0.22f, 0.18f);
                        }
                    }
                }
                else
                {
                    // Người chơi đã né thành công (Dodge / Dash)
                    FloatingDamageManager.ShowFloatingText(playerTransform.position + Vector3.up * 1f, "NÉ ĐÒN!", Color.cyan);
                }
            }
        }

        private void SpawnAttackImpactVfx(Vector3 pos, float range, bool boss)
        {
            GameObject slamObj = new GameObject(boss ? "VFX_Boss_Slam" : "VFX_Monster_Swipe");
            slamObj.transform.position = pos;
            var sr = slamObj.AddComponent<SpriteRenderer>();
            sr.sprite = telegraphRenderer != null ? telegraphRenderer.sprite : null;
            sr.color = boss ? new Color(1f, 0.2f, 0.1f, 0.8f) : new Color(1f, 0.4f, 0.2f, 0.6f);
            sr.sortingOrder = 4;
            float parentScale = transform.localScale.x > 0.01f ? transform.localScale.x : 1.0f;
            float baseScale = (range / 2.0f) / parentScale;
            slamObj.transform.localScale = new Vector3(baseScale, baseScale, 1f);

            // Tự tiêu hủy sau 0.22s hiệu ứng mờ dần
            StartCoroutine(FadeOutAndDestroyVfx(slamObj, sr, 0.22f));
        }

        private System.Collections.IEnumerator FadeOutAndDestroyVfx(GameObject obj, SpriteRenderer sr, float duration)
        {
            float elapsed = 0f;
            Color initC = sr != null ? sr.color : Color.red;
            Vector3 initSc = obj != null ? obj.transform.localScale : Vector3.one;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (sr != null) sr.color = new Color(initC.r, initC.g, initC.b, Mathf.Lerp(initC.a, 0f, t));
                if (obj != null) obj.transform.localScale = initSc * (1f + t * 0.25f);
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }

        private void UpdateLeashReturn()
        {
            Vector2 curPos = transform.position;
            Vector2 toSpawn = spawnPoint - curPos;

            if (toSpawn.magnitude <= 0.3f)
            {
                transform.position = spawnPoint;
                if (health != null)
                {
                    health.IsInSafeZone = false;
                    health.Revive();
                }
                SetState(MonsterState.Idle);
                return;
            }

            Vector2 moveStep = toSpawn.normalized * (chaseSpeed * 1.3f * Time.deltaTime);
            transform.position = curPos + moveStep;
            FlipSprite(moveStep.x);
        }

        public void ResetToSpawn()
        {
            transform.position = spawnPoint;
            HideTelegraph();
            if (health != null)
            {
                health.IsInSafeZone = false;
            }
            SetState(MonsterState.Idle);
        }

        private void ShowTelegraph()
        {
            if (telegraphObj != null)
            {
                telegraphObj.SetActive(true);
                telegraphObj.transform.position = transform.position;
            }
        }

        public void HideTelegraph()
        {
            if (telegraphObj != null)
            {
                telegraphObj.SetActive(false);
            }
        }

        private void FlipSprite(float moveX)
        {
            if (spriteRenderer == null) return;
            if (moveX > 0.01f) spriteRenderer.flipX = false;
            else if (moveX < -0.01f) spriteRenderer.flipX = true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint != Vector2.zero ? (Vector3)spawnPoint : transform.position, wanderRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint != Vector2.zero ? (Vector3)spawnPoint : transform.position, leashRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
