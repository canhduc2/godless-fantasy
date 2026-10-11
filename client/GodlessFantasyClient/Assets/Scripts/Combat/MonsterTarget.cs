using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Mục tiêu hình nộm / Quái luyện công:
    /// - Hiển thị thanh máu mini trên đầu (Floating Overhead HP Bar).
    /// - Khi bị tiêu diệt: Tự động hồi sinh (Respawn) sau 3 giây để người chơi test combo thoải mái!
    /// </summary>
    public class MonsterTarget : MonoBehaviour
    {
        [SerializeField] private string targetName = "Hình Nộm Luyện Công";
        public string TargetName => targetName;
        [SerializeField] private Health health;

        private Camera mainCam;
        private GUIStyle hpTextStyle;
        private Texture2D hpFillTex;
        private Texture2D hpBgTex;
        private float respawnTimer = -1f;
        private Vector3 initialPosition;

        private void Awake()
        {
            if (health == null) health = GetComponent<Health>();
            mainCam = Camera.main;
            initialPosition = transform.position;

            hpFillTex = new Texture2D(1, 1);
            hpFillTex.SetPixel(0, 0, new Color(0.9f, 0.2f, 0.2f, 1f));
            hpFillTex.Apply();

            hpBgTex = new Texture2D(1, 1);
            hpBgTex.SetPixel(0, 0, new Color(0.1f, 0.12f, 0.18f, 0.9f));
            hpBgTex.Apply();

            if (health != null)
            {
                health.OnDeath += HandleDeath;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
            }
        }

        private void HandleDeath()
        {
            respawnTimer = 4.0f;
            // Tạm ẩn hình nộm
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // 1. Phân phối EXP & Vàng (Nếu có bang hội thì chia cho đồng đội trong map)
            int expReward = gameObject.name.Contains("Champion") ? 120 : 45;
            int goldReward = gameObject.name.Contains("Champion") ? 60 : 25;

            var gm = GodlessFantasy.Guild.GuildManager.Instance;
            if (gm != null && gm.IsInGuild)
            {
                gm.DistributeGuildReward(expReward, goldReward, transform.position);
            }
            else
            {
                var player = FindAnyObjectByType<GodlessFantasy.Player.PlayerStats>();
                if (player != null)
                {
                    player.GainExp(expReward);
                    player.AddGold(goldReward);
                    FloatingDamageManager.ShowFloatingText(player.transform.position + Vector3.up * 1f,
                        $"+{expReward} EXP · +{goldReward} Vàng", new Color(1f, 0.88f, 0.25f));
                }
            }

            // 2. Rơi vật phẩm tự do tại vị trí quái chết để người chơi tự do nhặt và tranh nhau đồ
            if (Random.value > 0.4f)
            {
                GroundLootItem.SpawnLoot(transform.position, "Thịt Nướng Lửa Hồng", "Phẩm D", new Color(0.35f, 0.85f, 0.45f), "item_potion_hp", 1);
            }
            else
            {
                GroundLootItem.SpawnLoot(transform.position, "Bình Dược Liệu HP", "Phẩm C", new Color(0.35f, 0.65f, 1.0f), "item_potion_hp", 1);
            }
        }

        private void Update()
        {
            if (respawnTimer > 0f)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
        }

        private void Respawn()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            if (health != null)
            {
                // Reset máu
                typeof(Health).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(health, health.MaxHealth);
            }
            transform.position = initialPosition;
        }
    }
}
