using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GodlessFantasy.Core;

namespace GodlessFantasy.Player
{
    [Serializable]
    public class RebirthVaultData
    {
        public List<InventoryItem> savedItems = new List<InventoryItem>();
    }

    /// <summary>
    /// Rương Chuyển Sinh (Rebirth Vault Engine):
    /// - Khi người chơi Chuyển Sinh (Rebirth):
    ///   + Cho phép chọn tối đa 2 món đồ quý giá bất kỳ để cất vào Rương Server phong ấn.
    ///   + Dữ liệu được lưu vĩnh viễn không bị xóa khi nhân vật reset về Lv 1.
    ///   + Vị trí Rương đặt cố định tại Đền Chuyển Sinh Làng Tân Thủ.
    ///   + Sau khi tái sinh, người chơi chạy lại Đền mở Rương lấy lại 2 món bảo vật vào túi đồ.
    /// </summary>
    public class RebirthVaultManager : MonoBehaviour
    {
        public static RebirthVaultManager Instance { get; private set; }

        public const int MAX_VAULT_SLOTS = 2; // Tối đa 2 món bảo vật được bảo lưu
        private List<InventoryItem> vaultItems = new List<InventoryItem>();

        public bool IsVaultOpen = false;

        private string SavePath => Path.Combine(Application.persistentDataPath, "rebirth_vault.json");

        public List<InventoryItem> VaultItems => vaultItems;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadVaultData();
        }

        public void LoadVaultData()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    var data = JsonUtility.FromJson<RebirthVaultData>(json);
                    if (data != null && data.savedItems != null)
                    {
                        vaultItems = data.savedItems;
                        Debug.Log($"<color=green>[REBIRTH VAULT]</color> Đã nạp {vaultItems.Count} bảo vật từ Rương Server!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[REBIRTH VAULT] Lỗi khi nạp dữ liệu: {ex.Message}");
            }
        }

        public void SaveVaultData()
        {
            try
            {
                var data = new RebirthVaultData { savedItems = vaultItems };
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"<color=green>[REBIRTH VAULT]</color> Đã lưu {vaultItems.Count} bảo vật vào Rương Server vĩnh viễn!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[REBIRTH VAULT] Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Cất 1 món đồ từ túi vào Rương Chuyển Sinh
        /// </summary>
        public bool DepositItem(InventoryItem item, InventoryManager inv)
        {
            if (vaultItems.Count >= MAX_VAULT_SLOTS)
            {
                Debug.LogWarning("[REBIRTH VAULT] Rương chỉ được lưu tối đa 2 món bảo vật chuyển sinh!");
                return false;
            }

            if (item == null || inv == null) return false;

            inv.Items.Remove(item);
            vaultItems.Add(item);
            SaveVaultData();
            Debug.Log($"<color=gold>[REBIRTH VAULT]</color> Đã cất bảo vật '{item.itemName}' vào Rương Chuyển Sinh!");
            return true;
        }

        /// <summary>
        /// Rút 1 món đồ từ Rương Chuyển Sinh về túi đồ
        /// </summary>
        public bool WithdrawItem(InventoryItem item, InventoryManager inv)
        {
            if (item == null || inv == null) return false;

            if (!inv.CanAddItem(item))
            {
                Debug.LogWarning("[REBIRTH VAULT] Hành trang của bạn đã đầy, không thể rút bảo vật!");
                return false;
            }

            vaultItems.Remove(item);
            inv.AddItem(item);
            SaveVaultData();
            Debug.Log($"<color=green>[REBIRTH VAULT]</color> Đã rút bảo vật '{item.itemName}' về hành trang!");
            return true;
        }

        private void OnGUI()
        {
            if (!IsVaultOpen) return;

            GUI.depth = -180;

            float boxW = 460f;
            float boxH = 340f;
            float boxX = (Screen.width - boxW) / 2f;
            float boxY = (Screen.height - boxH) / 2f;
            Rect boxRect = new Rect(boxX, boxY, boxW, boxH);

            // Nền hộp rương
            GUI.color = new Color(0.05f, 0.07f, 0.12f, 0.96f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(boxRect);

            // Tiêu đề
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.25f, 1f) }
            };
            GUI.Label(new Rect(boxX, boxY + 16, boxW, 28), "🏛️ RƯƠNG CHUYỂN SINH (REBIRTH VAULT)", titleStyle);

            var descStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.88f, 0.95f, 0.85f) }
            };
            string desc = "Bảo lưu đúng 2 bảo vật bất kỳ trên Server. Khi bạn Chuyển Sinh về Cấp 1, 2 món đồ này vẫn nguyên vẹn trong rương tại Đền!";
            GUI.Label(new Rect(boxX + 20, boxY + 48, boxW - 40, 38), desc, descStyle);

            var inv = FindAnyObjectByType<InventoryManager>();

            // Hiển thị 2 ô bảo vật trong rương
            float slotSize = 72f;
            float startSlotX = boxX + (boxW - (slotSize * 2 + 30f)) / 2f;
            float slotY = boxY + 100f;

            for (int i = 0; i < MAX_VAULT_SLOTS; i++)
            {
                Rect slotRect = new Rect(startSlotX + i * (slotSize + 30f), slotY, slotSize, slotSize);
                GUI.color = new Color(0.08f, 0.12f, 0.18f, 0.9f);
                GUI.DrawTexture(slotRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawDoubleGoldFrame(slotRect);

                if (i < vaultItems.Count)
                {
                    var it = vaultItems[i];
                    GUI.Label(new Rect(slotRect.x + 4, slotRect.y + 6, slotRect.width - 8, 36), it.itemName, descStyle);

                    if (GUI.Button(new Rect(slotRect.x + 6, slotRect.y + slotRect.height - 24, slotRect.width - 12, 20), "Rút Đồ"))
                    {
                        WithdrawItem(it, inv);
                    }
                }
                else
                {
                    var emptyStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 11,
                        normal = { textColor = Color.gray }
                    };
                    GUI.Label(slotRect, $"[Ô Trống {i + 1}]", emptyStyle);
                }
            }

            // Gợi ý cất đồ từ túi
            var guideStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.4f, 0.9f, 1f, 0.9f) }
            };
            GUI.Label(new Rect(boxX + 20, boxY + 195, boxW - 40, 24), "💡 Để cất đồ: Mở Túi đồ (Phím B) -> Click chuột phải vào món đồ -> Chọn 'Cất vào Rương'.", guideStyle);

            // Nút Đóng
            if (GUI.Button(new Rect(boxX + (boxW - 140f) / 2f, boxY + boxH - 45, 140f, 32), "Đóng Rương"))
            {
                IsVaultOpen = false;
            }
        }

        private void DrawDoubleGoldFrame(Rect r)
        {
            GUI.color = new Color(0.85f, 0.72f, 0.28f, 0.95f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, 2, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y, 2, r.height), Texture2D.whiteTexture);

            GUI.color = new Color(0.45f, 0.35f, 0.15f, 0.85f);
            GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, r.width - 4, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + 2, r.y + r.height - 3, r.width - 4, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, 1, r.height - 4), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 3, r.y + 2, 1, r.height - 4), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
