using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GodlessFantasy.Player;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Engine Quản Lý Kỹ Năng / Bí Kíp Tập Trung (Skill Database Engine):
    /// - Không cần code C# để tạo kỹ năng mới!
    /// - Lưu trữ các chiêu thức tùy biến thành file JSON và tự động đồng bộ vào game.
    /// </summary>
    public class SkillDatabase : MonoBehaviour
    {
        public static SkillDatabase Instance { get; private set; }

        private List<SkillScrollData> registeredSkills = new List<SkillScrollData>();
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "custom_skill_database.json");

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadDatabaseFromJson();
        }

        public InventoryItem RegisterAndCreateSkillItem(string id, string name, string element, string tierRank,
                                                        Color tierColor, int manaCost, float cooldown, int baseDamage,
                                                        float intelScaling, int indicatorType, string hotbarIcon,
                                                        string scrollIcon, string desc)
        {
            var scroll = new SkillScrollData(id, name, tierRank, tierColor, element, manaCost, cooldown, baseDamage, intelScaling, indicatorType, hotbarIcon, scrollIcon, desc);
            registeredSkills.Add(scroll);
            SaveDatabaseToJson();

            // Đóng gói thành InventoryItem chuẩn để xuất hiện trong Túi Đồ
            string stats = $"Sát thương: {baseDamage} + {Mathf.RoundToInt(intelScaling * 100)}% Trí Tuệ | Hồi: {cooldown:F1}s | Mana: {manaCost}";
            string actualIcon = !string.IsNullOrEmpty(scrollIcon) ? scrollIcon : hotbarIcon;
            var item = new InventoryItem(id, name, ItemType.SkillScroll, EquipSlot.None, tierRank, tierColor, actualIcon, stats, desc, 1, scroll);
            return item;
        }

        public void SaveDatabaseToJson()
        {
            try
            {
                var list = new List<SkillJsonData>();
                foreach (var s in registeredSkills)
                {
                    list.Add(SkillJsonData.FromSkill(s));
                }
                string json = JsonUtility.ToJson(new SkillListWrapper { skills = list }, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"<color=green>[SKILL ENGINE]</color> Đã lưu {list.Count} kỹ năng vào: {SaveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SKILL ENGINE] Lỗi khi lưu database kỹ năng: {ex.Message}");
            }
        }

        public void LoadDatabaseFromJson()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var wrapper = JsonUtility.FromJson<SkillListWrapper>(json);
                    if (wrapper != null && wrapper.skills != null)
                    {
                        registeredSkills.Clear();
                        foreach (var data in wrapper.skills)
                        {
                            registeredSkills.Add(data.ToSkill());
                        }
                        Debug.Log($"<color=green>[SKILL ENGINE]</color> Đã tải {registeredSkills.Count} kỹ năng từ file JSON!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SKILL ENGINE] Chưa có database kỹ năng JSON tùy chỉnh: {ex.Message}");
            }
        }

        [Serializable]
        private class SkillListWrapper
        {
            public List<SkillJsonData> skills;
        }
    }
}
