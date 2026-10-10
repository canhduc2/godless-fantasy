using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Thú Cưng Mini (Cute Pet Companion):
    /// - Linh vật Sóc Chibi của SquiRRels Studio chạy lon ton theo sau người chơi.
    /// - Tiêu chuẩn 3-KHÔNG: Không cưỡi, Không tấn công, Không thể bị chọn làm mục tiêu (Untargetable).
    /// - Miễn trừ xóa / reset khi Chuyển Sinh (Rebirth).
    /// </summary>
    public class PetFollower : MonoBehaviour
    {
        [Header("Target & Follow Settings")]
        [SerializeField] private Transform owner;
        [SerializeField] private float followDistance = 1.2f;
        [SerializeField] private float moveSpeed = 4.8f;
        [SerializeField] private float maxDistanceTeleport = 12.0f;

        private SpriteRenderer spriteRenderer;
        private Vector3 targetOffset = new Vector3(-0.8f, -0.2f, 0);
        private float bobTimer = 0f;

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Pet nằm ở layer 9 (ngay dưới chân người chơi layer 10)
                spriteRenderer.sortingOrder = 9;
            }
        }

        private void Update()
        {
            if (owner == null) return;

            Vector3 targetPos = owner.position + targetOffset;
            float dist = Vector2.Distance(transform.position, targetPos);

            // Nếu khoảng cách quá xa -> Dịch chuyển tức thời về cạnh chủ nhân
            if (dist > maxDistanceTeleport)
            {
                transform.position = targetPos;
                return;
            }

            // Chạy theo chủ nhân nếu khoảng cách > followDistance
            if (dist > followDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

                // Lật sprite theo hướng chạy
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (targetPos.x < transform.position.x);
                }

                // Nhún nhảy cute khi chạy
                bobTimer += Time.deltaTime * 12f;
                float bobY = Mathf.Sin(bobTimer) * 0.08f;
                transform.position += new Vector3(0, bobY, 0);
            }
            else
            {
                // Đứng im lắc lư nhè nhẹ
                bobTimer += Time.deltaTime * 3f;
                float breatheY = Mathf.Sin(bobTimer) * 0.02f;
                transform.position += new Vector3(0, breatheY, 0);
            }
        }
    }
}
