using System;
using UnityEngine;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Player
{
    public enum ItemType
    {
        Equipment,
        SkillScroll,
        Consumable,
        Pet,
        Material
    }

    public enum EquipSlot
    {
        None,
        Helmet,    // Mũ
        Armor,     // Áo
        Pants,     // Quần
        Boots,     // Giày
        Ring,      // Nhẫn
        Necklace,  // Vòng Cổ
        Weapon     // Vũ Khí
    }

    /// <summary>
    /// Đơn vị vật phẩm thống nhất trong Hành Trang Godless Fantasy:
    /// - Cả Trang Bị, Sách Kỹ Năng, Bình Dược, Thú Cưng đều là các ô hành trang bình thường.
    /// - Khi đang được trang bị (mặc trên người / gắn vào ô skill 1-4 / thú cưng đang gọi) -> Được khóa bảo vệ chống xóa/vứt/giao dịch nhầm!
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string id;
        public string itemName;
        public ItemType itemType;
        public EquipSlot equipSlot;
        public string tierRank;          // E, D, C, B, A, S, X
        public Color tierColor;
        public string iconName;          // Tên texture trong Resources
        public int quantity = 1;
        public bool isEquipped = false;
        public int equippedHotbarSlot = -1; // 0..3 nếu là SkillScroll đang gắn vào ô 1..4
        public string description;
        public string statsInfo;
        public SkillScrollData scrollData;

        public InventoryItem(string id, string itemName, ItemType itemType, EquipSlot equipSlot, 
                             string tierRank, Color tierColor, string iconName, string statsInfo, 
                             string description, int quantity = 1, SkillScrollData scrollData = null)
        {
            this.id = id;
            this.itemName = itemName;
            this.itemType = itemType;
            this.equipSlot = equipSlot;
            this.tierRank = tierRank;
            this.tierColor = tierColor;
            this.iconName = iconName;
            this.statsInfo = statsInfo;
            this.description = description;
            this.quantity = quantity;
            this.scrollData = scrollData;
            this.isEquipped = false;
            this.equippedHotbarSlot = -1;
        }

        public InventoryItem Clone()
        {
            var copy = new InventoryItem(id, itemName, itemType, equipSlot, tierRank, tierColor, iconName, statsInfo, description, quantity, scrollData);
            copy.isEquipped = isEquipped;
            copy.equippedHotbarSlot = equippedHotbarSlot;
            return copy;
        }
    }
}
