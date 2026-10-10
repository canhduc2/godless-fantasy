using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Thú Cưng Mini (Cute Mascot Pet):
    /// - Bay theo chủ nhân mượt mà bằng Lerp.
    /// - Nhấp nhô lên xuống siêu nhẹ nhàng (chỉ 0.02f) cạnh hông/chân người chơi, không bay quá cao!
    /// - Chuẩn 3-KHÔNG: Không đánh, Không cưỡi, Không bị chọn làm mục tiêu.
    /// </summary>
    public class PetFollower : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform owner;
        [SerializeField] private float followDistance = 0.9f;
        [SerializeField] private float followSmoothSpeed = 6.0f;
        [SerializeField] private float teleportDistance = 10.0f;

        [Header("Subtle Gentle Floating")]
        [SerializeField] private float bobFrequency = 2.2f;
        [SerializeField] private float bobAmplitude = 0.025f; // Rất nhẹ nhàng, chỉ 1 xíu thôi theo đúng yêu cầu!
        [SerializeField] private Vector3 followOffset = new Vector3(-0.70f, -0.22f, 0); // Đặt cạnh chân/hông

        private SpriteRenderer spriteRenderer;
        private Vector3 currentBasePosition;

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
            if (owner != null)
            {
                currentBasePosition = owner.position + followOffset;
                transform.position = currentBasePosition;
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 9; // Dưới chân người chơi layer 10
            }
            currentBasePosition = transform.position;
        }

        private void Update()
        {
            if (owner == null) return;

            // Vị trí mục tiêu cạnh chân chủ nhân
            Vector3 desiredBasePos = owner.position + followOffset;
            float dist = Vector2.Distance(currentBasePosition, desiredBasePos);

            if (dist > teleportDistance)
            {
                currentBasePosition = desiredBasePos;
            }
            else if (dist > followDistance)
            {
                // Bám theo mượt mà
                currentBasePosition = Vector3.Lerp(currentBasePosition, desiredBasePos, followSmoothSpeed * Time.deltaTime);

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (desiredBasePos.x < currentBasePosition.x);
                }
            }

            // Nhấp nhô tại chỗ cực kỳ nhẹ nhàng (chỉ 0.025 đơn vị)
            float bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = currentBasePosition + new Vector3(0, bobY, 0);
        }
    }
}
