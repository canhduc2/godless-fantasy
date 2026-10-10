using System;
using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Dữ Liệu Cuộn Trục Kỹ Năng (Skill Scroll Item Data):
    /// - Trong Godless Fantasy, mỗi kỹ năng thực chất là một Cuộn Trục (Bí Kíp) trong Hành Trang/Túi Đồ.
    /// - Người chơi có thể KÉO THẢ (Drag & Drop) cuộn trục từ túi đồ hoặc kho bí kíp vào các ô [1], [2], [3], [4] trên thanh phím nóng.
    /// - Có phân cấp phẩm (E, D, C, B, A, S, X) chuẩn GDD DOC-03 & DOC-04.
    /// </summary>
    [Serializable]
    public class SkillScrollData
    {
        public string id;
        public string scrollName;
        public string requiredClass;     // Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ, Toàn Hệ
        public string tierRank;          // E, D, C, B, A, S, X
        public Color tierColor;
        public string element;           // Ánh Sáng, Bóng Tối, Phong, Mộc, Băng, Hỏa, Lôi
        public int manaCost;
        public float cooldown;
        public int baseDamage;
        public float intelligenceScaling;
        public int indicatorType;        // 1=Line, 2=Ground AoE, 3=Cone Arc, 4=Self Nova, 5=Self Buff
        public int skillCategory;        // 0=Damage, 1=Heal HP, 2=Heal Mana, 3=Buff Stat
        public float castRange;          // Tầm đánh / Cự ly thi triển (mét)
        public string iconName;          // Icon Độc Nhất trên thanh Hotbar [1..4]
        public string scrollIconName;    // Icon Cuộn Trục mang Ấn Ký Hệ trong Túi Đồ
        public string customVfxSprite;   // (Tùy chọn) Đường dẫn sprite đạn bay/hoạt ảnh riêng
        public string description;

        public bool IsSupport => skillCategory != 0;
        public string CategoryName
        {
            get
            {
                switch (skillCategory)
                {
                    case 1: return "Hồi Máu (HP)";
                    case 2: return "Hồi Mana (MP)";
                    case 3: return "Cường Hóa (Buff)";
                    default: return "Tấn Công (ST)";
                }
            }
        }

        public string RangeFormattedText
        {
            get
            {
                switch (indicatorType)
                {
                    case 1: return $"{castRange:0.#}m (Bắn Thẳng)";
                    case 2: return $"{castRange:0.#}m (Vùng Đất / Mưa)";
                    case 3: return $"{castRange:0.#}m (Hình Quạt)";
                    case 4: return $"{castRange:0.#}m (Xung Quanh)";
                    case 5: return "0m (Buff Tự Thân)";
                    default: return $"{castRange:0.#}m";
                }
            }
        }

        public SkillScrollData(string id, string scrollName, string tierRank, Color tierColor, string element, 
                               int manaCost, float cooldown, int baseDamage, float intelligenceScaling, 
                               int indicatorType, string iconName, string scrollIconName, string description,
                               float castRange = 0f, string requiredClass = "Toàn Hệ", string customVfxSprite = "",
                               int skillCategory = 0)
        {
            this.id = id;
            this.scrollName = scrollName;
            this.requiredClass = string.IsNullOrEmpty(requiredClass) ? "Toàn Hệ" : requiredClass;
            this.tierRank = tierRank;
            this.tierColor = tierColor;
            this.element = element;
            this.manaCost = manaCost;
            this.cooldown = cooldown;
            this.baseDamage = baseDamage;
            this.intelligenceScaling = intelligenceScaling;
            this.indicatorType = indicatorType;
            this.skillCategory = skillCategory;
            this.iconName = iconName;
            this.scrollIconName = scrollIconName;
            this.customVfxSprite = customVfxSprite;
            this.description = description;

            if (castRange > 0f)
            {
                this.castRange = castRange;
            }
            else
            {
                // Tự động gán tầm đánh chuẩn theo Archetype
                switch (indicatorType)
                {
                    case 1: this.castRange = 8.0f; break; // Bắn thẳng (Line)
                    case 2: this.castRange = 7.0f; break; // Vùng đất AoE (Ground AoE)
                    case 3: this.castRange = 4.5f; break; // Hình quạt (Cone Sweep)
                    case 4: this.castRange = 2.5f; break; // Hào quang quanh thân (Self Nova)
                    case 5: this.castRange = 0.0f; break; // Buff Tự Thân (Self Buff)
                    default: this.castRange = 6.0f; break;
                }
            }
        }

        /// <summary>
        /// Kiểm tra nghiêm ngặt điều kiện trang bị / học cuộn trục:
        /// - KHÔNG PHẢI PHẠT MÀ LÀ KHÔNG DÙNG ĐƯỢC!
        /// - Phải đúng Chức Nghiệp (Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ) hoặc Toàn Hệ.
        /// - Phải đúng Thuộc Tính Nguyên Tố (1 trong 7 hệ: Ánh Sáng, Bóng Tối, Phong, Mộc, Băng, Hỏa, Lôi).
        /// </summary>
        /// <summary>
        /// Kiểm tra nghiêm ngặt điều kiện trang bị / phát huy bí kíp:
        /// 1. HỆ (Class): Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ (hoặc Toàn Hệ). Người thuộc Hệ nào CHỈ DÙNG ĐƯỢC cuộn của Hệ đó!
        /// 2. THUỘC TÍNH (Element): Hỏa, Băng, Lôi, Ánh Sáng, Bóng Tối, Phong, Mộc. Người mang Thuộc Tính nào CHỈ PHÁT HUY ĐƯỢC skill của Thuộc Tính đó!
        /// </summary>
        public bool CanPlayerUse(string playerClass, string playerElement, out string failureReason)
        {
            // 1. Kiểm tra HỆ
            if (!string.IsNullOrEmpty(requiredClass) && requiredClass != "Toàn Hệ")
            {
                if (!string.Equals(requiredClass, playerClass, StringComparison.OrdinalIgnoreCase))
                {
                    failureReason = $"Sai Hệ! Cuộn yêu cầu [{requiredClass}], bạn đang là [{playerClass}]";
                    return false;
                }
            }

            // 2. Kiểm tra THUỘC TÍNH
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
                    failureReason = $"Khác Thuộc Tính! Cuộn yêu cầu [{element}], bạn mang [{playerElement}]";
                    return false;
                }
            }

            failureReason = null;
            return true;
        }

        public SkillScrollData Clone()
        {
            return new SkillScrollData(id, scrollName, tierRank, tierColor, element, manaCost, cooldown, baseDamage, intelligenceScaling, indicatorType, iconName, scrollIconName, description, castRange, requiredClass, customVfxSprite, skillCategory);
        }
    }
}
