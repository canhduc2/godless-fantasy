using System;
using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// ScriptableObject & Data Model cho phép thiết kế Kỹ Năng / Bí Kíp trực tiếp trên Inspector hoặc Form:
    /// - Không cần viết code C# khi tạo chiêu thức mới!
    /// - Cho phép cấu hình phân biệt rõ icon cuộn trục trong túi đồ và icon chiêu trên hotbar.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "Godless Fantasy/Skill Definition")]
    public class SkillDefinitionSO : ScriptableObject
    {
        [Header("Thông Tin Chiêu Thức")]
        public string skillId = "skill_custom_1";
        public string skillName = "Hỏa Long Chưởng";
        [Tooltip("Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ, Toàn Hệ")]
        public string requiredClass = "Pháp Sư";
        [Tooltip("Ánh Sáng, Bóng Tối, Phong, Mộc, Băng, Hỏa, Lôi")]
        public string element = "Hỏa";
        public string tierRank = "Phẩm A";
        public Color tierColor = new Color(1.0f, 0.65f, 0.2f);

        [Header("Cơ Chế Thi Triển & Chỉ Số")]
        [Tooltip("1=Bắn thẳng (Line), 2=Vùng đất (Ground AoE), 3=Hình nón (Cone), 4=Nổ xung quanh (Nova), 5=Buff Tự Thân (Buff)")]
        [Range(1, 5)]
        public int indicatorType = 1;
        [Tooltip("0=Tấn Công (Damage), 1=Hồi Máu (Heal HP), 2=Hồi Mana (Restore MP), 3=Cường Hóa (Buff Stat)")]
        [Range(0, 3)]
        public int skillCategory = 0;
        public int manaCost = 25;
        public float cooldown = 4.0f;
        public int baseDamage = 45;
        [Range(0f, 3f)]
        public float intelligenceScaling = 0.85f;
        [Tooltip("Tầm đánh / cự ly thi triển (mét). Nếu để 0 sẽ tự gán theo loại tâm ngắm.")]
        public float castRange = 0f;

        [Header("Icon Hiển Thị Độc Lập (Fallback chuẩn)")]
        [Tooltip("Icon hiển thị khi gắn vào ô phím nóng [1..4]")]
        public string hotbarIconName = "skill_fireball";
        [Tooltip("Icon cuộn trục bí kíp khi nằm trong túi đồ")]
        public string scrollBagIconName = "scroll_firebolt";
        public string customVfxSprite = "";

        [Header("Mô Tả")]
        [TextArea(2, 4)]
        public string description = "Triệu hồi luồng hỏa long lao thẳng phía trước thiêu đốt kẻ địch.";

        public SkillScrollData ToSkillScrollData()
        {
            return new SkillScrollData(skillId, skillName, tierRank, tierColor, element, manaCost, cooldown, baseDamage, intelligenceScaling, indicatorType, hotbarIconName, scrollBagIconName, description, castRange, requiredClass, customVfxSprite, skillCategory);
        }
    }

    [Serializable]
    public class SkillJsonData
    {
        public string id;
        public string name;
        public string requiredClass;
        public string element;
        public string tierRank;
        public float tierR, tierG, tierB;
        public int indicatorType;
        public int skillCategory;
        public int manaCost;
        public float cooldown;
        public int baseDamage;
        public float intelligenceScaling;
        public float castRange;
        public string hotbarIconName;
        public string scrollBagIconName;
        public string customVfxSprite;
        public string description;

        public static SkillJsonData FromSkill(SkillScrollData skill)
        {
            return new SkillJsonData
            {
                id = skill.id,
                name = skill.scrollName,
                requiredClass = skill.requiredClass,
                element = skill.element,
                tierRank = skill.tierRank,
                tierR = skill.tierColor.r,
                tierG = skill.tierColor.g,
                tierB = skill.tierColor.b,
                indicatorType = skill.indicatorType,
                skillCategory = skill.skillCategory,
                manaCost = skill.manaCost,
                cooldown = skill.cooldown,
                baseDamage = skill.baseDamage,
                intelligenceScaling = skill.intelligenceScaling,
                castRange = skill.castRange,
                hotbarIconName = skill.iconName,
                scrollBagIconName = skill.scrollIconName,
                customVfxSprite = skill.customVfxSprite,
                description = skill.description
            };
        }

        public SkillScrollData ToSkill()
        {
            Color c = new Color(tierR, tierG, tierB, 1f);
            return new SkillScrollData(id, name, tierRank, c, element, manaCost, cooldown, baseDamage, intelligenceScaling, indicatorType, hotbarIconName, scrollBagIconName, description, castRange, requiredClass, customVfxSprite, skillCategory);
        }
    }
}
