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
        [SerializeField] private Color hitColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float flashDuration = 0.12f;

        private Color originalColor = Color.white;
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
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
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
                    if (spriteRenderer != null) spriteRenderer.color = originalColor;
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

            // Chớp đỏ phản hồi trúng đòn
            TriggerHitFlash();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void TriggerHitFlash()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = hitColor;
                flashTimer = flashDuration;
                isFlashing = true;
            }
        }

        private void Die()
        {
            Debug.Log($"[DEATH] {gameObject.name} đã hy sinh!");
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

        public void Revive()
        {
            currentHealth = maxHealth;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
