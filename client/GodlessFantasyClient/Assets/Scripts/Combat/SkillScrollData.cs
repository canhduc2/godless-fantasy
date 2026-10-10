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
        public string iconName;
        public string scrollIconName;
        public string description;

        public SkillScrollData(string id, string scrollName, string tierRank, Color tierColor, string element, 
                               int manaCost, float cooldown, int baseDamage, float intelligenceScaling, 
                               int indicatorType, string iconName, string scrollIconName, string description)
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
        }

        public SkillScrollData Clone()
        {
            return new SkillScrollData(id, scrollName, tierRank, tierColor, element, manaCost, cooldown, baseDamage, intelligenceScaling, indicatorType, iconName, scrollIconName, description);
        }
    }
}
