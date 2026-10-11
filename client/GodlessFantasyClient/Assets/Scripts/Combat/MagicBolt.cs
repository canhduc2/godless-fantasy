using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Đạn ma thuật (Magic Bolt Projectile):
    /// Bay theo hướng chỉ định. Va chạm vào bất kỳ IDamageable nào
    /// (kể cả đồng đội - Friendly Fire 100%) đều gây sát thương thật!
    /// </summary>
    public class MagicBolt : MonoBehaviour
    {
        [Header("Projectile Specs")]
        [SerializeField] private float speed = 12.0f;
        [SerializeField] private int damage = 25;
        [SerializeField] private float lifetime = 3.0f;

        private Vector2 moveDirection;
        private string shooterId = "Player";
        private Rigidbody2D rb;
        private Vector3 startPosition;
        private float maxRange = 8.0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            // Không hủy ở Awake để tránh làm mất prefab template!
        }

        public void Initialize(Vector2 direction, int boltDamage, string attacker, float range = 8.0f)
        {
            startPosition = transform.position;
            maxRange = Mathf.Max(1.0f, range);
            moveDirection = direction.normalized;
            damage = boltDamage;
            shooterId = attacker;

            // Xoay đầu đạn theo hướng bay
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (rb != null)
            {
                rb.linearVelocity = moveDirection * speed;
            }

            // Tự động hủy đạn theo thời gian tối đa để tới maxRange
            float calculatedLife = Mathf.Min(lifetime, (maxRange / Mathf.Max(1f, speed)) + 0.05f);
            Destroy(gameObject, calculatedLife);
        }

        private void Update()
        {
            // Kiểm tra mất tầm: Khi bay vượt quá maxRange thì biến mất ngay lập tức!
            if (Vector3.Distance(transform.position, startPosition) >= maxRange)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Tránh bắn trúng chính người bắn khi vừa rời nòng
            if (other.CompareTag("Shooter") || other.gameObject.name == shooterId)
            {
                return;
            }

            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage, shooterId);
                FloatingDamageManager.Spawn(other.transform.position, damage);
                Destroy(gameObject);
            }
        }
    }
}
