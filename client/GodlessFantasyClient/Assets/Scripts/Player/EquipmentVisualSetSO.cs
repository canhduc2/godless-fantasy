using System;
using UnityEngine;

namespace GodlessFantasy.Player
{
    public enum Direction8
    {
        South = 0,      // Xuống (Nam)
        SouthEast = 1,  // Xuống Phải (Đông Nam)
        East = 2,       // Phải (Đông)
        NorthEast = 3,  // Lên Phải (Đông Bắc)
        North = 4,      // Lên (Bắc)
        NorthWest = 5,  // Lên Trái (Tây Bắc)
        West = 6,       // Trái (Tây)
        SouthWest = 7   // Xuống Trái (Tây Nam)
    }

    public static class Direction8Utils
    {
        public static Direction8 VectorToDirection8(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.001f) return Direction8.South;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // -180 đến 180
            if (angle < 0) angle += 360f; // 0 đến 360

            // Chia 8 góc (mỗi góc 45 độ, offset 22.5 độ)
            if (angle >= 337.5f || angle < 22.5f) return Direction8.East;
            if (angle >= 22.5f && angle < 67.5f) return Direction8.NorthEast;
            if (angle >= 67.5f && angle < 112.5f) return Direction8.North;
            if (angle >= 112.5f && angle < 157.5f) return Direction8.NorthWest;
            if (angle >= 157.5f && angle < 202.5f) return Direction8.West;
            if (angle >= 202.5f && angle < 247.5f) return Direction8.SouthWest;
            if (angle >= 247.5f && angle < 292.5f) return Direction8.South;
            return Direction8.SouthEast;
        }

        public static bool IsFacingLeft(Direction8 dir)
        {
            return dir == Direction8.West || dir == Direction8.NorthWest || dir == Direction8.SouthWest;
        }
    }

    /// <summary>
    /// Bộ Sprite 8 Hướng (Template Cho Phép Upload Thiết Kế Sprite Theo Hướng):
    /// - Áp dụng cho: Ngoại hình nhân vật, Ngoại hình giáp, Ngoại hình vũ khí theo từng hướng.
    /// - Tự động lật đối xứng (Mirror Flip) giữa Trái và Phải nếu không có sprite riêng biệt.
    /// </summary>
    [Serializable]
    public class DirectionalSpriteSet
    {
        [Header("5 Hướng Chuẩn (Tự động lật cho 3 hướng còn lại)")]
        public Sprite south;     // Hướng Xuống (Nam)
        public Sprite southEast; // Hướng Xuống Phải (Đông Nam)
        public Sprite east;      // Hướng Phải (Đông)
        public Sprite northEast; // Hướng Lên Phải (Đông Bắc)
        public Sprite north;     // Hướng Lên (Bắc)

        [Header("Sprite Tùy Chọn Riêng Cho Hướng Trái (Nếu không muốn lật gương)")]
        public Sprite west;
        public Sprite northWest;
        public Sprite southWest;

        public Sprite GetSprite(Direction8 dir, out bool shouldFlipX)
        {
            shouldFlipX = false;
            switch (dir)
            {
                case Direction8.South:
                    return south;
                case Direction8.North:
                    return north;
                case Direction8.East:
                    return east;
                case Direction8.SouthEast:
                    return southEast;
                case Direction8.NorthEast:
                    return northEast;

                case Direction8.West:
                    if (west != null) return west;
                    shouldFlipX = true;
                    return east;

                case Direction8.SouthWest:
                    if (southWest != null) return southWest;
                    shouldFlipX = true;
                    return southEast;

                case Direction8.NorthWest:
                    if (northWest != null) return northWest;
                    shouldFlipX = true;
                    return northEast;

                default:
                    return south;
            }
        }
    }

    /// <summary>
    /// ScriptableObject Template Cho Thiết Kế Ngoại Hình Trang Bị & Nhân Vật 8 Hướng:
    /// - Không cần code! Chỉ cần tạo file asset, điền tên và kéo thả sprite các hướng vào ô!
    /// </summary>
    [CreateAssetMenu(fileName = "NewEquipmentVisualSet", menuName = "Godless Fantasy/Equipment Visual Set (8-Dir)")]
    public class EquipmentVisualSetSO : ScriptableObject
    {
        public string visualSetId = "visual_iron_armor";
        public string visualSetName = "Bộ Ngoại Hình Giáp Sắt";
        public EquipSlot targetSlot = EquipSlot.Armor;

        [Header("Bộ Sprite 8 Hướng")]
        public DirectionalSpriteSet spriteSet = new DirectionalSpriteSet();

        [Header("Offset Hiển Thị (Điều Chỉnh Vị Trí Ghép Đồ)")]
        public Vector2 offset = Vector2.zero;
        public int sortingOrderOffset = 0;
    }
}
