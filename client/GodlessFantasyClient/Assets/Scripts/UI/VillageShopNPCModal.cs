using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using GodlessFantasy.Core;

namespace GodlessFantasy.UI
{
    public enum VillageNPCType
    {
        Merchant,   // Thương Nhân Barnaby (Bán Máu, Mana, Cuộn sơ cấp, Bán đồ ve chai)
        Blacksmith, // Thợ Rèn Vulkan (Cường hóa đập đồ +1..+10)
        Alchemist,  // Phù Thủy Morgana (Thuốc lắc Buff, nước thánh giải độc)
        Rebirth     // Sứ Giả Kaelen (Shop điểm RP & Rương Chuyển Sinh)
    }

    /// <summary>
    /// Giao Diện Tương Tác Dàn NPC Làng Tân Thủ (Village NPC Shops & Services):
    /// - Thương Nhân Barnaby: Mua bán dược phẩm, ve chai quái rớt lấy Vàng.
    /// - Thợ Rèn Vulkan: Cường hóa đập đồ (+1 đến +10).
    /// - Phù Thủy Morgana: Pha chế ma dược, thuốc lắc chiến đấu.
    /// - Sứ Giả Kaelen: Tiêu điểm Rebirth Point (RP) mua Perk và mở Rương Chuyển Sinh!
    /// </summary>
    public class VillageShopNPCModal : MonoBehaviour
    {
        public static VillageShopNPCModal Instance { get; private set; }

        public bool IsOpen = false;
        public VillageNPCType currentNpcType = VillageNPCType.Merchant;

        private InventoryManager inventory;
        private PlayerStats playerStats;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public static void OpenNPC(VillageNPCType npcType)
        {
            if (Instance == null)
            {
                var go = new GameObject("[VILLAGE_SHOP_NPC_MODAL]");
                go.AddComponent<VillageShopNPCModal>();
            }
            Instance.currentNpcType = npcType;
            Instance.IsOpen = true;
            Instance.inventory = FindAnyObjectByType<InventoryManager>();
            Instance.playerStats = FindAnyObjectByType<PlayerStats>();
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.depth = -190;

            float boxW = 540f;
            float boxH = 420f;
            float boxX = (Screen.width - boxW) / 2f;
            float boxY = (Screen.height - boxH) / 2f;
            Rect boxRect = new Rect(boxX, boxY, boxW, boxH);

            // Nền Modal
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.96f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(boxRect);

            if (inventory == null) inventory = FindAnyObjectByType<InventoryManager>();
            if (playerStats == null) playerStats = FindAnyObjectByType<PlayerStats>();

            switch (currentNpcType)
            {
                case VillageNPCType.Merchant:
                    DrawMerchantUI(boxX, boxY, boxW, boxH);
                    break;
                case VillageNPCType.Blacksmith:
                    DrawBlacksmithUI(boxX, boxY, boxW, boxH);
                    break;
                case VillageNPCType.Alchemist:
                    DrawAlchemistUI(boxX, boxY, boxW, boxH);
                    break;
                case VillageNPCType.Rebirth:
                    DrawRebirthEmissaryUI(boxX, boxY, boxW, boxH);
                    break;
            }

            // Nút Đóng
            if (GUI.Button(new Rect(boxX + (boxW - 120f) / 2f, boxY + boxH - 42, 120f, 30), "Đóng [ESC]"))
            {
                IsOpen = false;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                IsOpen = false;
            }
        }

        // ================= 1. THƯƠNG NHÂN BARNABY =================
        private void DrawMerchantUI(float x, float y, float w, float h)
        {
            DrawTitle(x, y + 16, w, "🛒 THƯƠNG NHÂN TẠP HÓA BARNABY", "Chào hiệp sĩ! Cửa hàng có đủ thuốc phục hồi và cuộn chiêu cho chuyến viễn chinh.");

            int currentGold = playerStats != null ? playerStats.Gold : 0;
            GUI.Label(new Rect(x + 24, y + 68, w - 48, 22), $"💰 Vàng hiện có: <color=yellow>{currentGold:N0}</color> Vàng");

            float itemY = y + 96;

            DrawShopRow(x + 24, itemY, w - 48, "🧪 Bình Máu HP (Nhỏ)", "Hồi phục 60 HP tức thì (Chiếm 1 ô, dồn tối đa 100)", 10, currentGold, () => {
                BuyItem("item_hp_pot", "Bình Máu HP (+35)", 10, ItemType.Consumable, "item_potion_hp");
            });

            DrawShopRow(x + 24, itemY + 48, w - 48, "💧 Bình Mana MP (Nhỏ)", "Hồi phục 40 MP tức thì (Chiếm 1 ô, dồn tối đa 100)", 10, currentGold, () => {
                BuyItem("item_mp_pot", "Bình Mana (+30)", 10, ItemType.Consumable, "item_potion_mp");
            });

            DrawShopRow(x + 24, itemY + 96, w - 48, "📜 Cuộn Chiêu: Hỏa Cầu", "Kỹ năng nguyên tố Hỏa sơ cấp", 50, currentGold, () => {
                BuyItem("scroll_fireball_c", "Cuộn Chiêu: Hỏa Cầu", 50, ItemType.SkillScroll, "icon_scroll_fire");
            });

            DrawShopRow(x + 24, itemY + 144, w - 48, "📜 Cuộn Chiêu: Băng Tiễn", "Kỹ năng nguyên tố Băng sơ cấp", 50, currentGold, () => {
                BuyItem("scroll_icearrow_c", "Cuộn Chiêu: Băng Tiễn", 50, ItemType.SkillScroll, "icon_scroll_ice");
            });

            // Nút bán ve chai thu hồi vàng
            Rect sellBtnRect = new Rect(x + 24, y + 295, w - 48, 36);
            GUI.color = new Color(0.85f, 0.65f, 0.2f, 1f);
            if (GUI.Button(sellBtnRect, "🗑️ BÁN TOÀN BỘ ĐỒ RÁC QUÁI (THU HỒI VÀNG)"))
            {
                SellTrashLoot();
            }
            GUI.color = Color.white;
        }

        // ================= 2. THỢ RÈN VULKAN =================
        private void DrawBlacksmithUI(float x, float y, float w, float h)
        {
            DrawTitle(x, y + 16, w, "🔨 THỢ RÈN THIẾT BINH VULKAN", "Ta có thể tôi luyện vũ khí và giáp trụ của ngươi lên tầm cao mới (+1 đến +10).");

            int currentGold = playerStats != null ? playerStats.Gold : 0;
            GUI.Label(new Rect(x + 24, y + 68, w - 48, 22), $"💰 Vàng hiện có: <color=yellow>{currentGold:N0}</color> Vàng");

            float itemY = y + 100;

            GUI.Label(new Rect(x + 24, itemY, w - 48, 40), "Chọn trang bị muốn cường hóa từ danh sách trang bị đang mặc:");

            if (inventory != null && inventory.EquippedGear != null && inventory.EquippedGear.Count > 0)
            {
                int index = 0;
                foreach (var kvp in inventory.EquippedGear)
                {
                    var gear = kvp.Value;
                    if (gear == null) continue;

                    Rect rowRect = new Rect(x + 24, itemY + 40 + index * 44, w - 48, 38);
                    GUI.color = new Color(0.08f, 0.12f, 0.18f, 0.85f);
                    GUI.DrawTexture(rowRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    GUI.Label(new Rect(rowRect.x + 10, rowRect.y + 10, 240, 22), $"{kvp.Key}: {gear.itemName}");

                    int upgradeCost = 50;
                    bool canUpgrade = currentGold >= upgradeCost;

                    GUI.color = canUpgrade ? Color.green : Color.gray;
                    if (GUI.Button(new Rect(rowRect.x + rowRect.width - 150, rowRect.y + 6, 140, 26), $"Cường Hóa (50 Vàng)"))
                    {
                        if (canUpgrade)
                        {
                            playerStats.Gold -= upgradeCost;
                            gear.itemName += " +1";
                            FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up * 1.5f, "✨ Cường Hóa Thành Công! +1", Color.yellow);
                            AudioManager.PlayLevelUp();
                        }
                    }
                    GUI.color = Color.white;

                    index++;
                    if (index >= 4) break;
                }
            }
            else
            {
                GUI.Label(new Rect(x + 24, itemY + 50, w - 48, 30), "<color=gray>(Bạn chưa mặc trang bị nào trên người để cường hóa)</color>");
            }
        }

        // ================= 3. PHÙ THỦY MORGANA =================
        private void DrawAlchemistUI(float x, float y, float w, float h)
        {
            DrawTitle(x, y + 16, w, "🔮 PHÙ THỦY MA DƯỢC MORGANA", "Những bình thuốc ma thuật của ta sẽ tiếp thêm cuồng nộ và tốc độ cho ngươi.");

            int currentGold = playerStats != null ? playerStats.Gold : 0;
            GUI.Label(new Rect(x + 24, y + 68, w - 48, 22), $"💰 Vàng hiện có: <color=yellow>{currentGold:N0}</color> Vàng");

            float itemY = y + 96;

            DrawShopRow(x + 24, itemY, w - 48, "⚡ Thuốc Lắc Cuồng Nộ", "+25% Tốc độ đánh & Chạy trong 60s", 30, currentGold, () => {
                BuyItem("potion_haste", "Thuốc Lắc Cuồng Nộ", 30, ItemType.Consumable, "item_potion_haste");
            });

            DrawShopRow(x + 24, itemY + 48, w - 48, "✨ Nước Thánh Giải Nguyền", "Xóa toàn bộ hiệu ứng bất lợi & Lời nguyền", 25, currentGold, () => {
                BuyItem("potion_cleanse", "Nước Thánh Giải Nguyền", 25, ItemType.Consumable, "item_potion_cleanse");
            });

            DrawShopRow(x + 24, itemY + 96, w - 48, "🛡️ Dược Tê Bì Kim Cang", "+40 Giáp phòng thủ trong 60s", 35, currentGold, () => {
                BuyItem("potion_armor", "Dược Tê Bì Kim Cang", 35, ItemType.Consumable, "item_potion_armor");
            });
        }

        // ================= 4. SỨ GIẢ CHUYỂN SINH KAELEN (RP SHOP) =================
        private void DrawRebirthEmissaryUI(float x, float y, float w, float h)
        {
            DrawTitle(x, y + 16, w, "🏛️ SỨ GIẢ CHUYỂN SINH KAELEN", "Đổi điểm Rebirth Point (RP) lấy bí thuật cổ đại, hoặc mở Rương Chuyển Sinh lưu trữ bảo vật.");

            int currentRP = playerStats != null ? playerStats.RebirthPoints : 0;
            GUI.Label(new Rect(x + 24, y + 68, w - 48, 22), $"✨ Điểm Chuyển Sinh hiện có: <color=#00e5ff>{currentRP}</color> RP");

            float itemY = y + 96;

            DrawRPShopRow(x + 24, itemY, w - 48, "🔥 Bí Kíp S: Hỏa Long Bộc Phá", "Chiêu thức huyền thoại phun rồng lửa xuyên thấu", 5, currentRP, () => {
                BuyRPItem("mage_s_draconic_breath", "Bí Kíp S: Hỏa Long Bộc Phá", 5);
            });

            DrawRPShopRow(x + 24, itemY + 46, w - 48, "👑 Bí Kíp X: Phượng Hoàng Niết Bàn", "Chiêu thức Thần Thoại lửa hồi sinh", 8, currentRP, () => {
                BuyRPItem("mage_x_phoenix_rebirth", "Bí Kíp X: Phượng Hoàng Niết Bàn", 8);
            });

            DrawRPShopRow(x + 24, itemY + 92, w - 48, "🎲 Vòng Quay Đặc Tính (Random Perk Thường)", "Tiêu 1 RP quay ngẫu nhiên 1 Đặc Tính / Perk chiến đấu thường", 1, currentRP, () => {
                RollRPPerk(1);
            }, "Quay (1 RP)");

            DrawRPShopRow(x + 24, itemY + 138, w - 48, "🛡️ Bùa Cường Hóa Ô Đồ (Bảo Toàn Vĩnh Viễn)", "Kích hoạt cường hóa +1 ô đồ (Đồ bên trong rơi không mất khi chết)", 2, currentRP, () => {
                BuyRPConsumableItem("item_slot_protect_rune", "Bùa Cường Hóa Ô Đồ", 2, "item_scroll_wood",
                    "✨ Bùa Cường Hóa: Kích hoạt cường hóa vĩnh viễn thêm +1 ô hành trang! Đồ đặt trong ô đã cường hóa sẽ KHÔNG BAO GIỜ BỊ RƠI MẤT KHI CHẾT!");
            });

            // Nút mở Rương Chuyển Sinh
            Rect vaultBtnRect = new Rect(x + 24, y + 284, w - 48, 34);
            GUI.color = new Color(0.2f, 0.85f, 1f, 1f);
            if (GUI.Button(vaultBtnRect, "🏛️ MỞ RƯƠNG CHUYỂN SINH (REBIRTH VAULT - LƯU 2 MÓN)"))
            {
                IsOpen = false;
                if (RebirthVaultManager.Instance != null)
                {
                    RebirthVaultManager.Instance.IsVaultOpen = true;
                }
            }
            GUI.color = Color.white;
        }

        private void DrawShopRow(float rx, float ry, float rw, string title, string sub, int price, int playerGold, System.Action onBuy)
        {
            Rect r = new Rect(rx, ry, rw, 42);
            GUI.color = new Color(0.08f, 0.11f, 0.16f, 0.85f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(rx + 8, ry + 4, rw - 130, 20), title);
            var subStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = Color.gray } };
            GUI.Label(new Rect(rx + 8, ry + 22, rw - 130, 18), sub, subStyle);

            bool canBuy = playerGold >= price;
            GUI.color = canBuy ? Color.green : Color.gray;
            if (GUI.Button(new Rect(rx + rw - 110, ry + 8, 100, 26), $"Mua ({price} V)") && canBuy)
            {
                onBuy?.Invoke();
            }
            GUI.color = Color.white;
        }

        private void DrawRPShopRow(float rx, float ry, float rw, string title, string sub, int rpCost, int playerRP, System.Action onBuy, string btnText = null)
        {
            Rect r = new Rect(rx, ry, rw, 42);
            GUI.color = new Color(0.08f, 0.11f, 0.16f, 0.85f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(rx + 8, ry + 4, rw - 130, 20), title);
            var subStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = Color.gray } };
            GUI.Label(new Rect(rx + 8, ry + 22, rw - 130, 18), sub, subStyle);

            bool canBuy = playerRP >= rpCost;
            GUI.color = canBuy ? new Color(0f, 0.9f, 1f) : Color.gray;
            string displayBtn = !string.IsNullOrEmpty(btnText) ? btnText : $"Đổi ({rpCost} RP)";
            if (GUI.Button(new Rect(rx + rw - 110, ry + 8, 100, 26), displayBtn) && canBuy)
            {
                onBuy?.Invoke();
            }
            GUI.color = Color.white;
        }

        private void BuyItem(string id, string name, int cost, ItemType type, string icon)
        {
            if (playerStats == null || inventory == null) return;
            if (playerStats.Gold < cost) return;

            var it = new InventoryItem(id, name, type, EquipSlot.None, "Phẩm C", Color.white, icon, "Vật phẩm mua từ NPC", "", 1);
            if (inventory.CanAddItem(it))
            {
                playerStats.Gold -= cost;
                inventory.AddItem(it);
                AudioManager.PlayPickup();
                FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up, $"Mua thành công {name}!", Color.green);
            }
        }

        private void BuyRPItem(string id, string name, int rpCost)
        {
            if (playerStats == null || inventory == null) return;
            if (playerStats.RebirthPoints < rpCost) return;

            var it = new InventoryItem(id, name, ItemType.SkillScroll, EquipSlot.None, "Phẩm S", new Color(1f, 0.4f, 0.1f), "icon_scroll_fire", "Bí kíp đổi từ Sứ Giả", "", 1);
            if (inventory.CanAddItem(it))
            {
                playerStats.RebirthPoints -= rpCost;
                inventory.AddItem(it);
                AudioManager.PlayLevelUp();
                FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up, $"Đã đổi {name}!", Color.cyan);
            }
        }

        private void BuyRPConsumableItem(string id, string name, int rpCost, string icon, string desc)
        {
            if (playerStats == null || inventory == null) return;
            if (playerStats.RebirthPoints < rpCost) return;

            var it = new InventoryItem(id, name, ItemType.Consumable, EquipSlot.None, "Phẩm S", new Color(0.3f, 0.9f, 1f), icon, desc, desc, 1);
            if (inventory.CanAddItem(it))
            {
                playerStats.RebirthPoints -= rpCost;
                inventory.AddItem(it);
                AudioManager.PlayLevelUp();
                FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up, $"Đã đổi {name}!", Color.cyan);
            }
        }

        private void RollRPPerk(int rpCost)
        {
            if (playerStats == null) return;
            if (playerStats.RebirthPoints < rpCost)
            {
                FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up, "❌ Không đủ điểm RP!", Color.red);
                return;
            }

            if (!playerStats.ConsumeRebirthPoints(rpCost)) return;

            var rolled = playerStats.RollRandomRegularPerk();
            if (rolled != null)
            {
                FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up * 1.3f, $"🎲 QUAY TRÚNG: {rolled.emoji} [{rolled.name}]!", new Color(1f, 0.85f, 0.25f));
                Debug.Log($"<color=yellow>[VÒNG QUAY PERK]</color> Chúc mừng! Đã mở khóa Perk '{rolled.name}': {rolled.desc}");
                AudioManager.PlayLevelUp();
            }
        }

        private void SellTrashLoot()
        {
            if (inventory == null || playerStats == null) return;

            int earned = 0;
            List<InventoryItem> toSell = new List<InventoryItem>();
            foreach (var it in inventory.Items)
            {
                if (it != null && (it.itemName.Contains("Mảnh") || it.itemName.Contains("Da") || it.itemName.Contains("Rác")))
                {
                    earned += 15 * it.quantity;
                    toSell.Add(it);
                }
            }

            foreach (var s in toSell) inventory.Items.Remove(s);

            if (earned > 0)
            {
                playerStats.Gold += earned;
                AudioManager.PlayPickup();
                FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up, $"+{earned} Vàng từ ve chai!", Color.yellow);
            }
            else
            {
                FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up, "Không có đồ rác để bán!", Color.gray);
            }
        }

        private void DrawTitle(float x, float y, float w, string title, string sub)
        {
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.25f, 1f) }
            };
            GUI.Label(new Rect(x, y, w, 26), title, titleStyle);

            var subStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.8f, 0.85f, 0.9f, 0.8f) }
            };
            GUI.Label(new Rect(x + 16, y + 26, w - 32, 22), sub, subStyle);
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
