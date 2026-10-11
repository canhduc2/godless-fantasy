// =========================================================================
// GODLESS FANTASY - GENERATED SKILLS DATABASE
// Tự động sinh từ Godless Studio Engine (F1)
// Các anh em lập trình viên có thể dùng trực tiếp hoặc gọi hàm nạp vào game!
// =========================================================================
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using System.Collections.Generic;

namespace GodlessFantasy.Database
{
    public static class GeneratedSkillsDatabase
    {
        public static List<InventoryItem> GetGeneratedSkillPresets()
        {
            var list = new List<InventoryItem>();
            var coreAxes = SkillDatabase.GetCoreSkillAxes();
            foreach (var s in coreAxes)
            {
                string effectLabel = s.IsSupport ? s.CategoryName : "ST";
                string stats = $"• [{s.requiredClass} · {s.element}] | {effectLabel}: {s.baseDamage} + {Mathf.RoundToInt(s.intelligenceScaling * 100)}% | CD: {s.cooldown:F1}s | Tầm: {s.RangeFormattedText}";
                string actualIcon = !string.IsNullOrEmpty(s.scrollIconName) ? s.scrollIconName : s.iconName;
                list.Add(new InventoryItem(s.id, s.scrollName, ItemType.SkillScroll, EquipSlot.None, s.tierRank, s.tierColor, actualIcon, stats, s.description, 1, s.Clone()));
            }
            return list;
        }
    }
}
