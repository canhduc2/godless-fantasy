using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Engine Quản Lý Vật Phẩm Tập Trung (Data-Driven Item Database):
    /// - Cho phép thêm trang bị / vật phẩm mới bằng cách điền Form hoặc file JSON mà không cần viết thêm dòng code C# nào!
    /// - Tự động đồng bộ và lưu trữ vào thư mục dữ liệu trò chơi.
    /// </summary>
    public class ItemDatabase : MonoBehaviour
    {
        public static ItemDatabase Instance { get; private set; }

        private List<InventoryItem> registeredItemTemplates = new List<InventoryItem>();
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "custom_item_database.json");

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadDatabaseFromJson();
        }

        /// <summary>
        /// Tạo và đăng ký một vật phẩm mới từ Form điền thông số
        /// </summary>
        public InventoryItem RegisterNewItemFromForm(string id, string name, ItemType type, EquipSlot slot,
                                                     string tierRank, Color tierColor, string iconName,
                                                     string stats, string description, int quantity = 1,
                                                     string requiredClass = "Toàn Hệ", string element = "Toàn Thuộc Tính",
                                                     ItemGender requiredGender = ItemGender.All, List<string> traits = null)
        {
            var item = new InventoryItem(id, name, type, slot, tierRank, tierColor, iconName, stats, description, quantity, null, requiredClass, element, requiredGender);
            if (traits != null)
            {
                foreach (var tr in traits) item.WithTrait(tr);
            }
            registeredItemTemplates.Add(item);
            SaveDatabaseToJson();
            Debug.Log($"<color=cyan>[ITEM ENGINE]</color> Đã tạo thành công vật phẩm mới: {name} ({id})!");
            return item;
        }

        public List<InventoryItem> GetAllTemplates() => registeredItemTemplates;

        public void SaveDatabaseToJson()
        {
            try
            {
                var list = new List<ItemJsonData>();
                foreach (var it in registeredItemTemplates)
                {
                    list.Add(ItemJsonData.FromItem(it));
                }
                string json = JsonUtility.ToJson(new ItemListWrapper { items = list }, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"<color=green>[ITEM ENGINE]</color> Đã lưu {list.Count} định nghĩa vật phẩm vào: {SaveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ITEM ENGINE] Lỗi khi lưu database: {ex.Message}");
            }
        }

        public void LoadDatabaseFromJson()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var wrapper = JsonUtility.FromJson<ItemListWrapper>(json);
                    if (wrapper != null && wrapper.items != null)
                    {
                        registeredItemTemplates.Clear();
                        foreach (var data in wrapper.items)
                        {
                            registeredItemTemplates.Add(data.ToItem());
                        }
                        Debug.Log($"<color=green>[ITEM ENGINE]</color> Đã tải {registeredItemTemplates.Count} vật phẩm từ file JSON!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ITEM ENGINE] Chưa có database JSON tùy chỉnh hoặc lỗi tải: {ex.Message}");
            }
        }

        [Serializable]
        private class ItemListWrapper
        {
            public List<ItemJsonData> items;
        }
    }
}
