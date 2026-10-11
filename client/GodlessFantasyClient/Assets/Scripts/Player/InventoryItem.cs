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
        Material,
        Costume    // Ngoại Trang (Skin)
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
        Weapon,    // Vũ Khí
        Costume    // Ngoại Trang (Skin)
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

        // ĐIỀU KIỆN MẶC TRANG BỊ ĐỒNG BỘ VỚI BÍ KÍP (CLASS & ELEMENT LOCKOUT SYSTEM):
        public string requiredClass = "Toàn Hệ"; // Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ, Toàn Hệ
        public string element = "Vô Hệ";         // Hỏa, Băng, Lôi, Quang / Ánh Sáng, Ám / Bóng Tối, Phong, Mộc, Vô Hệ, Toàn Hệ

        public string RequirementTag
        {
            get
            {
                if (requiredClass == "Toàn Hệ" && (element == "Vô Hệ" || element == "Toàn Hệ")) return "[Toàn Hệ]";
                if (element == "Vô Hệ" || element == "Toàn Hệ") return $"[{requiredClass}]";
                return $"[{requiredClass} · {element}]";
            }
        }

        public InventoryItem(string id, string itemName, ItemType itemType, EquipSlot equipSlot, 
                             string tierRank, Color tierColor, string iconName, string statsInfo, 
                             string description, int quantity = 1, SkillScrollData scrollData = null,
                             string requiredClass = "Toàn Hệ", string element = "Vô Hệ")
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
            this.requiredClass = string.IsNullOrEmpty(requiredClass) ? "Toàn Hệ" : requiredClass;
            this.element = string.IsNullOrEmpty(element) ? "Vô Hệ" : element;
            this.isEquipped = false;
            this.equippedHotbarSlot = -1;
        }

        /// <summary>
        /// Kiểm tra nghiêm ngặt điều kiện trang bị / mặc đồ lên người:
        /// - 100% CÙNG CHUNG LOGIC VỚI CUỘN TRỤC KỸ NĂNG (GDD DOC-03 & THẢO LUẬN 04):
        ///   1. HỆ (Class): Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ (hoặc Toàn Hệ).
        ///   2. THUỘC TÍNH (Element): Hỏa, Băng, Lôi, Quang/Ánh Sáng, Ám/Bóng Tối, Phong, Mộc (hoặc Vô Hệ/Toàn Hệ).
        /// - Nếu không thỏa mãn -> KHÔNG CHO MẶC và báo lý do từ chối rõ ràng!
        /// </summary>
        public bool CanPlayerEquip(string playerClass, string playerElement, out string failureReason)
        {
            // 1. Kiểm tra HỆ (Chức nghiệp)
            if (!string.IsNullOrEmpty(requiredClass) && requiredClass != "Toàn Hệ")
            {
                if (!string.Equals(requiredClass, playerClass, StringComparison.OrdinalIgnoreCase))
                {
                    failureReason = $"Sai Hệ! Trang bị yêu cầu [{requiredClass}], bạn đang là [{playerClass}]";
                    return false;
                }
            }

            // 2. Kiểm tra THUỘC TÍNH NGUYÊN TỐ
            if (!string.IsNullOrEmpty(element) && element != "Vô Hệ" && element != "Toàn Hệ")
            {
                bool isMatch = string.Equals(element, playerElement, StringComparison.OrdinalIgnoreCase);
                // Xử lý đồng nghĩa Quang / Ánh Sáng và Ám / Bóng Tối
                if (!isMatch)
                {
                    if ((element == "Ánh Sáng" || element == "Quang") && (playerElement == "Ánh Sáng" || playerElement == "Quang")) isMatch = true;
                    else if ((element == "Bóng Tối" || element == "Ám") && (playerElement == "Bóng Tối" || playerElement == "Ám")) isMatch = true;
                }

                if (!isMatch)
                {
                    failureReason = $"Khác Thuộc Tính! Trang bị yêu cầu [{element}], bạn mang [{playerElement}]";
                    return false;
                }
            }

            failureReason = null;
            return true;
        }

        public InventoryItem Clone()
        {
            var copy = new InventoryItem(id, itemName, itemType, equipSlot, tierRank, tierColor, iconName, statsInfo, description, quantity, scrollData, requiredClass, element);
            copy.isEquipped = isEquipped;
            copy.equippedHotbarSlot = equippedHotbarSlot;
            return copy;
        }
    }
}
