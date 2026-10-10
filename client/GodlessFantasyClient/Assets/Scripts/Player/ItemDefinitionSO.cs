using System;
using UnityEngine;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// ScriptableObject & Data Model cho phép thiết kế vật phẩm trực tiếp trên Inspector hoặc form điền thông số:
    /// - Không cần viết code C# khi tạo trang bị, bí kíp, bình dược, thú cưng mới!
    /// - Chuẩn hóa thông số, phẩm cấp (tier), icon, loại ô trang bị.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Godless Fantasy/Item Definition")]
    public class ItemDefinitionSO : ScriptableObject
    {
        [Header("Thông Tin Cơ Bản")]
        public string itemId = "item_custom_1";
        public string itemName = "Trang Bị Mới";
        public ItemType itemType = ItemType.Equipment;
        public EquipSlot equipSlot = EquipSlot.Weapon;

        [Header("Phẩm Cấp & Hiển Thị")]
        public string tierRank = "Phẩm B";
        public Color tierColor = new Color(0.3f, 0.65f, 1f);
        public string iconName = "item_weapon";

        [Header("Chỉ Số & Mô Tả")]
        public string statsInfo = "+10 Trí Tuệ (INT)";
        [TextArea(2, 4)]
        public string description = "Vật phẩm thần bí được tạo từ bảng thiết kế engine.";

        public int defaultQuantity = 1;

        /// <summary>
        /// Chuyển đổi định nghĩa thành InventoryItem trong game
        /// </summary>
        public InventoryItem ToInventoryItem()
        {
            return new InventoryItem(itemId, itemName, itemType, equipSlot, tierRank, tierColor, iconName, statsInfo, description, defaultQuantity);
        }
    }

    /// <summary>
    /// Model lưu trữ dạng JSON nhẹ để lưu/tải danh mục vật phẩm
    /// </summary>
    [Serializable]
    public class ItemJsonData
    {
        public string id;
        public string name;
        public int itemType;
        public int equipSlot;
        public string tierRank;
        public float tierR, tierG, tierB;
        public string iconName;
        public string statsInfo;
        public string description;
        public int quantity = 1;

        public static ItemJsonData FromItem(InventoryItem item)
        {
            return new ItemJsonData
            {
                id = item.id,
                name = item.itemName,
                itemType = (int)item.itemType,
                equipSlot = (int)item.equipSlot,
                tierRank = item.tierRank,
                tierR = item.tierColor.r,
                tierG = item.tierColor.g,
                tierB = item.tierColor.b,
                iconName = item.iconName,
                statsInfo = item.statsInfo,
                description = item.description,
                quantity = item.quantity
            };
        }

        public InventoryItem ToItem()
        {
            Color c = new Color(tierR, tierG, tierB, 1f);
            return new InventoryItem(id, name, (ItemType)itemType, (EquipSlot)equipSlot, tierRank, c, iconName, statsInfo, description, quantity);
        }
    }
}
