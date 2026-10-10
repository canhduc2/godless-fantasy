using System.Collections.Generic;
using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Hiển thị chữ số sát thương bay lên (Floating Damage Text Popup):
    /// Khi quái hoặc người chơi trúng đòn, số sát thương (-35) sẽ nảy lên,
    /// bay nhẹ lên trên và mờ dần (Fade Out) cực kỳ đã mắt!
    /// </summary>
    public class FloatingDamageManager : MonoBehaviour
    {
        public static FloatingDamageManager Instance { get; private set; }

        private class DamagePopup
        {
            public Vector3 worldPos;
            public string text;
            public Color color;
            public float lifetime;
            public float maxLifetime;
            public float velocityY;
        }

        private readonly List<DamagePopup> activePopups = new List<DamagePopup>();
        private GUIStyle damageStyle;
        private Camera mainCam;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
            mainCam = Camera.main;
        }

        public static void Spawn(Vector3 worldPos, int damage, bool isCrit = false)
        {
            if (Instance == null)
            {
                var go = new GameObject("[FLOATING_DAMAGE_MANAGER]");
                Instance = go.AddComponent<FloatingDamageManager>();
            }

            Color col = isCrit ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.25f, 0.25f);
            Instance.activePopups.Add(new DamagePopup
            {
                worldPos = worldPos + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0),
                text = $"-{damage}",
                color = col,
                lifetime = 0.9f,
                maxLifetime = 0.9f,
                velocityY = 1.2f
            });
        }

        private void Update()
        {
            for (int i = activePopups.Count - 1; i >= 0; i--)
            {
                var p = activePopups[i];
                p.lifetime -= Time.deltaTime;
                p.worldPos.y += p.velocityY * Time.deltaTime;
                p.velocityY = Mathf.Max(0.2f, p.velocityY - Time.deltaTime * 1.5f);

                if (p.lifetime <= 0)
                {
                    activePopups.RemoveAt(i);
                }
            }
        }

        private void OnGUI()
        {
            if (activePopups.Count == 0) return;
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            if (damageStyle == null)
            {
                damageStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            foreach (var p in activePopups)
            {
                Vector3 screenPos = mainCam.WorldToScreenPoint(p.worldPos);
                if (screenPos.z < 0) continue; // Behind camera

                float alpha = Mathf.Clamp01(p.lifetime / (p.maxLifetime * 0.6f));
                Color c = p.color;
                c.a = alpha;

                // Tọa độ GUI (Y đảo ngược)
                float guiY = Screen.height - screenPos.y;
                Rect r = new Rect(screenPos.x - 50, guiY - 15, 100, 30);

                // Vẽ viền đen bóng đổ
                damageStyle.normal.textColor = new Color(0, 0, 0, alpha * 0.9f);
                GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), p.text, damageStyle);
                GUI.Label(new Rect(r.x - 1, r.y - 1, r.width, r.height), p.text, damageStyle);

                // Vẽ chữ màu chính (Đỏ rực / Vàng kim)
                damageStyle.normal.textColor = c;
                GUI.Label(r, p.text, damageStyle);
            }
        }
    }
}
