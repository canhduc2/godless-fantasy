using System;
using UnityEngine;

namespace GodlessFantasy.Combat
{
    public interface IDamageable
    {
        void TakeDamage(int damage, string attackerId);
        bool IsInSafeZone { get; set; }
        bool IsDead { get; }
    }

    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health Configuration")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;

        [Header("Hit Flash Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float flashDuration = 0.22f;

        private SpriteRenderer[] allRenderers;
        private Color[] originalColors;
        private float flashTimer = 0f;
        private bool isFlashing = false;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsInSafeZone { get; set; } = false;

        public event Action<int, int> OnHealthChanged; // current, max
        public event Action OnDeath;

        private void Awake()
        {
            allRenderers = GetComponentsInChildren<SpriteRenderer>();
            if (allRenderers != null && allRenderers.Length > 0)
            {
                originalColors = new Color[allRenderers.Length];
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    originalColors[i] = allRenderers[i].color;
                }
            }
            if (spriteRenderer == null && allRenderers != null && allRenderers.Length > 0)
            {
                spriteRenderer = allRenderers[0];
            }
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (isFlashing)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    isFlashing = false;
                    RestoreColors();
                }
            }
        }

        private void RestoreColors()
        {
            if (allRenderers != null && originalColors != null)
            {
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    if (allRenderers[i] != null && i < originalColors.Length)
                    {
                        allRenderers[i].color = originalColors[i];
                    }
                }
            }
        }

        public void TakeDamage(int damage, string attackerId)
        {
            if (IsDead) return;

            // Safe Zone Check: Miễn nhiễm sát thương trong Vùng An Toàn
            if (IsInSafeZone)
            {
                Debug.Log($"[SAFE ZONE] Mục tiêu {gameObject.name} được bảo vệ an toàn, miễn nhiễm {damage} sát thương từ {attackerId}!");
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage);
            Debug.Log($"[COMBAT / FRIENDLY FIRE] {gameObject.name} nhận {damage} sát thương từ {attackerId}! HP còn: {currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Chớp đỏ phản hồi trúng đòn cho cả Người Chơi lẫn Quái Vật
            TriggerHitFlash();
            GodlessFantasy.Core.AudioManager.PlayHit();

            // Nếu người chơi bị đánh -> Màn hình chớp đỏ cảnh báo
            if (gameObject.name.Contains("Player") || CompareTag("Player"))
            {
                UI.CombatHUD.TriggerScreenDamageFlash();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void TriggerHitFlash()
        {
            if (allRenderers == null || allRenderers.Length == 0)
            {
                allRenderers = GetComponentsInChildren<SpriteRenderer>();
            }

            if (allRenderers != null)
            {
                foreach (var sr in allRenderers)
                {
                    if (sr != null) sr.color = hitColor;
                }
                flashTimer = flashDuration;
                isFlashing = true;
            }
        }

        private void Die()
        {
            Debug.Log($"[DEATH] {gameObject.name} đã hy sinh!");
            GodlessFantasy.Core.AudioManager.PlayExplosion();
            OnDeath?.Invoke();
            // Đổi màu thành xám tối
            if (spriteRenderer != null) spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetMaxHealth(int newMax)
        {
            maxHealth = newMax;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void IncreaseMaxHealth(int amount)
        {
            maxHealth += amount;
            currentHealth += amount;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Revive()
        {
            currentHealth = maxHealth;
            RestoreColors();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
