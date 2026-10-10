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
        public string tierRank;          // E, D, C, B, A, S, X
        public Color tierColor;
        public string element;           // Hỏa, Băng, Lôi, Thánh, Ma Pháp
        public int manaCost;
        public float cooldown;
        public int baseDamage;
        public float intelligenceScaling;
        public int indicatorType;        // 1=Line, 2=Ground AoE, 3=Cone Arc, 4=Self Nova
        public float castRange;          // Tầm đánh / Cự ly thi triển (mét)
        public string iconName;
        public string scrollIconName;
        public string description;

        public string RangeFormattedText
        {
            get
            {
                switch (indicatorType)
                {
                    case 1: return $"{castRange:0.#}m (Bắn Thẳng)";
                    case 2: return $"{castRange:0.#}m (Vùng Đất)";
                    case 3: return $"{castRange:0.#}m (Hình Quạt)";
                    case 4: return $"{castRange:0.#}m (Xung Quanh)";
                    default: return $"{castRange:0.#}m";
                }
            }
        }

        public SkillScrollData(string id, string scrollName, string tierRank, Color tierColor, string element, 
                               int manaCost, float cooldown, int baseDamage, float intelligenceScaling, 
                               int indicatorType, string iconName, string scrollIconName, string description,
                               float castRange = 0f)
        {
            this.id = id;
            this.scrollName = scrollName;
            this.tierRank = tierRank;
            this.tierColor = tierColor;
            this.element = element;
            this.manaCost = manaCost;
            this.cooldown = cooldown;
            this.baseDamage = baseDamage;
            this.intelligenceScaling = intelligenceScaling;
            this.indicatorType = indicatorType;
            this.iconName = iconName;
            this.scrollIconName = scrollIconName;
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
                    default: this.castRange = 6.0f; break;
                }
            }
        }

        public SkillScrollData Clone()
        {
            return new SkillScrollData(id, scrollName, tierRank, tierColor, element, manaCost, cooldown, baseDamage, intelligenceScaling, indicatorType, iconName, scrollIconName, description, castRange);
        }
    }
}
