using System;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.UI;

namespace GodlessFantasy.Network
{
    /// <summary>
    /// Thực Thể Người Chơi Từ Xa (Remote Multiplayer Player):
    /// - Nội suy chuyển động mượt mà (Hermite / Lerp Interpolation) chống giật lag khi mạng chập chờn.
    /// - Điều khiển hoạt ảnh 8 hướng bằng ProceduralCharacterRig2D.
    /// - Tích hợp OverheadStatusBar hiển thị Tên, Cấp độ, Hệ, Bang hội và Thanh máu chuẩn GDD.
    /// </summary>
    public class NetworkPlayerEntity : MonoBehaviour
    {
        public int PlayerId { get; private set; }
        public CharacterInfoDto Info { get; private set; }

        private CharacterSpriteAnimator8Dir spriteAnimator;
        private OverheadStatusBar overheadBar;

        // Nội suy chuyển động (Smoothing Interpolation)
        private Vector3 currentVelocity;
        private Vector3 targetPosition;
        private bool isMoving = false;
        private float moveSpeed = 4.2f; // Tốc độ chạy chuẩn
        private Direction8 currentDirection = Direction8.South;

        public void Initialize(CharacterInfoDto info)
        {
            PlayerId = info.id;
            Info = info;
            transform.position = new Vector3(info.posX, info.posY, 0f);
            targetPosition = transform.position;

            // 1. Gán Hoạt Ảnh Frame 8 Hướng (CharacterSpriteAnimator8Dir)
            spriteAnimator = GetComponent<CharacterSpriteAnimator8Dir>();
            if (spriteAnimator == null)
            {
                spriteAnimator = gameObject.AddComponent<CharacterSpriteAnimator8Dir>();
            }

            var oldRig = GetComponent<ProceduralCharacterRig2D>();
            if (oldRig != null) oldRig.enabled = false;

            // Đồng bộ Giới tính
            bool isFemale = string.Equals(info.gender, "Nữ", StringComparison.OrdinalIgnoreCase);
            spriteAnimator.SetGender(isFemale);

            // 2. Gán Nhãn Thông Tin & Thanh Máu Trên Đầu
            overheadBar = GetComponent<OverheadStatusBar>();
            if (overheadBar == null)
            {
                overheadBar = gameObject.AddComponent<OverheadStatusBar>();
            }
            overheadBar.Setup(OverheadType.Player, info.name, $"Lv.{info.level} · {info.className} · {info.element}", 1.05f);

            // Gán Tag và Layer
            gameObject.tag = "Player";
            gameObject.name = $"RemotePlayer_{info.name}";
        }

        public void UpdateMovement(PlayerMoveBroadcastDto moveDto)
        {
            targetPosition = new Vector3(moveDto.targetX, moveDto.targetY, 0f);
            isMoving = moveDto.isMoving;

            // Nếu độ lệch vị trí quá lớn (> 6.0m do lag spike hoặc dịch chuyển tức thời), snap ngay lập tức
            if (Vector3.Distance(transform.position, targetPosition) > 6.0f)
            {
                transform.position = new Vector3(moveDto.posX, moveDto.posY, 0f);
            }

            // Hướng mặt
            if (Enum.TryParse<Direction8>(moveDto.direction, true, out var dir))
            {
                currentDirection = dir;
            }
        }

        private void Update()
        {
            float distToTarget = Vector2.Distance(transform.position, targetPosition);

            if (distToTarget > 0.04f)
            {
                // Nội suy di chuyển mượt mà về đích với tốc độ ổn định
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

                if (spriteAnimator != null)
                {
                    spriteAnimator.IsMoving = true;
                    spriteAnimator.FacingDirection = currentDirection;
                }
            }
            else
            {
                transform.position = targetPosition;
                if (spriteAnimator != null)
                {
                    spriteAnimator.IsMoving = false;
                    spriteAnimator.FacingDirection = currentDirection;
                }
            }
        }
    }
}
