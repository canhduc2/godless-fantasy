using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Thú Cưng Mini Đồng Hành (Lively Companion Pet):
    /// - Không bao giờ đứng trước mặt người chơi làm vướng tầm nhìn.
    /// - Luôn bay/chạy ở 1 trong 2 bên hông (Left Wing / Right Wing) cách đều 0.85m chuẩn mực.
    /// - Cơ chế Random Side Switch: Cứ mỗi 4 - 8 giây, thú cưng lại ngẫu nhiên đổi bên (lúc lượn sang trái, lúc chạy sang phải) tạo cảm giác cực kỳ sinh động!
    /// - Tự động quay mặt (FlipX) về phía chủ nhân.
    /// - Nhấp nhô nhịp nhàng bồng bềnh siêu nhẹ tại chỗ.
    /// - Chuẩn 3-KHÔNG: Không đánh, Không cưỡi, Không bị chọn làm mục tiêu.
    /// </summary>
    public class PetFollower : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform owner;
        [SerializeField] private float sideDistance = 0.85f;      // Khoảng cách bên hông chuẩn mực
        [SerializeField] private float followSmoothSpeed = 4.8f;   // Tốc độ di chuyển bám theo mượt mà
        [SerializeField] private float teleportDistance = 8.0f;    // Dịch chuyển tức thời khi quá xa

        [Header("Lively Flank Switch (Đổi bên ngẫu nhiên)")]
        private float currentSideX = -1.0f;                        // -1: Bên trái, +1: Bên phải
        private float sideSwitchTimer = 5.0f;                      // Bộ đếm thời gian đổi bên

        [Header("Subtle Gentle Floating")]
        [SerializeField] private float bobFrequency = 2.2f;
        [SerializeField] private float bobAmplitude = 0.025f;

        private SpriteRenderer spriteRenderer;
        private Vector3 currentBasePosition;

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
            if (owner != null)
            {
                currentSideX = (Random.value > 0.5f) ? 1.0f : -1.0f;
                currentBasePosition = owner.position + new Vector3(currentSideX * sideDistance, -0.15f, 0);
                transform.position = currentBasePosition;
                ResetSwitchTimer();
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 9; // Hiển thị dưới chân nhân vật (Layer 10)
            }
            currentBasePosition = transform.position;
            currentSideX = (Random.value > 0.5f) ? 1.0f : -1.0f;
            ResetSwitchTimer();
        }

        private void ResetSwitchTimer()
        {
            // Random khoảng 4.5 đến 8.0 giây một lần
            sideSwitchTimer = Random.Range(4.5f, 8.0f);
        }

        private void Update()
        {
            if (owner == null) return;

            // 1. Logic đổi bên ngẫu nhiên theo thời gian giúp thú cưng luôn sinh động
            sideSwitchTimer -= Time.deltaTime;
            if (sideSwitchTimer <= 0f)
            {
                ResetSwitchTimer();
                // 50% cơ hội đổi bên sang hông đối diện, 50% giữ nguyên phía
                if (Random.value < 0.65f)
                {
                    currentSideX = -currentSideX;
                }
            }

            Vector3 ownerPos = owner.position;

            // 2. Vị trí mục tiêu: Luôn ở bên hông (trái hoặc phải), ngang tầm chân/hông, KHÔNG BAO GIỜ đứng trước mặt
            Vector3 desiredBasePos = ownerPos + new Vector3(currentSideX * sideDistance, -0.15f, 0);
            float currentDist = Vector2.Distance(currentBasePosition, ownerPos);

            if (currentDist > teleportDistance)
            {
                currentBasePosition = desiredBasePos;
            }
            else
            {
                // Bám theo mượt mà
                currentBasePosition = Vector3.Lerp(currentBasePosition, desiredBasePos, followSmoothSpeed * Time.deltaTime);
            }

            // 3. Lật Sprite thú cưng luôn quay mặt nhìn về phía chủ nhân
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (currentBasePosition.x < ownerPos.x);
            }

            // 4. Hiệu ứng nhấp nhô siêu nhẹ tại chỗ
            float bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = currentBasePosition + new Vector3(0, bobY, 0);
        }
    }
}
