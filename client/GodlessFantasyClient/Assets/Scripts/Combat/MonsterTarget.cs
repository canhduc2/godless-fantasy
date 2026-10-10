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
            respawnTimer = 3.0f;
            // Tạm ẩn hình nộm
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
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

        private void OnGUI()
        {
            if (health == null || health.IsDead) return;
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            Vector3 worldHeadPos = transform.position + new Vector3(0, 0.65f, 0);
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldHeadPos);
            if (screenPos.z < 0) return;

            float w = 46f;
            float h = 6f;
            float x = screenPos.x - w / 2f;
            float y = Screen.height - screenPos.y;

            // Khung nền
            GUI.DrawTexture(new Rect(x, y, w, h), hpBgTex);

            // Máu đỏ
            float fill = Mathf.Clamp01((float)health.CurrentHealth / health.MaxHealth);
            GUI.DrawTexture(new Rect(x, y, w * fill, h), hpFillTex);

            // Viền vàng kim
            GUI.color = new Color(0.85f, 0.7f, 0.3f, 0.8f);
            GUI.DrawTexture(new Rect(x, y, w, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + h - 1, w, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y, 1, h), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + w - 1, y, 1, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
