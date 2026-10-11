using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Vật Phẩm Rơi Dưới Đất Tự Do Nhặt (Free-for-All Ground Loot Pickup):
    /// - Khi quái chết, rơi vật phẩm ngay tại vị trí xác quái.
    /// - Ai chạy lại gần bấm [F] hoặc Click chuột vào vật phẩm thì nhặt được (cơ chế tranh đồ PvP/PvE tự do).
    /// - Có hiệu ứng nhấp nhô lấp lánh (sparkling bounce) trên mặt đất.
    /// </summary>
    public class GroundLootItem : MonoBehaviour
    {
        public string itemId;
        public string itemName;
        public string tierRank = "Phẩm D";
        public Color tierColor = new Color(0.35f, 0.85f, 0.45f);
        public string iconName = "item_potion_hp";
        public int quantity = 1;

        private Vector3 basePosition;
        private Transform playerTransform;
        private const float PICKUP_RANGE = 1.8f;

        private void Start()
        {
            basePosition = transform.position;
            var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
            if (player != null) playerTransform = player.transform;
        }

        private void Update()
        {
            // Hiệu ứng nhấp nhô lơ lửng nhẹ
            float yOffset = Mathf.Sin(Time.time * 5f) * 0.08f;
            transform.position = basePosition + new Vector3(0, yOffset, 0);

            // Kiểm tra phím [F] để nhặt đồ khi đứng gần
            if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= PICKUP_RANGE)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    TryPickup();
                }
            }
        }

        private void OnMouseDown()
        {
            // Cho phép click chuột vào vật phẩm để nhặt khi đứng gần
            if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= PICKUP_RANGE)
            {
                TryPickup();
            }
        }

        public void TryPickup()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv != null)
            {
                var item = new InventoryItem(itemId, itemName, ItemType.Consumable, EquipSlot.None, tierRank, tierColor, iconName,
                    $"Vật phẩm nhặt được từ quái rơi tự do.", "Nhấn chuột phải để sử dụng.", quantity);
                inv.AddItem(item);
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 0.8f, $"🎒 Nhặt được: {itemName} x{quantity}!", tierColor);
                Debug.Log($"<color=green>[LOOT PICKUP]</color> Đã nhặt thành công {itemName}!");
                Destroy(gameObject);
            }
        }

        private void OnGUI()
        {
            if (Camera.main == null || playerTransform == null) return;
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist > 4.5f) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPos.z < 0) return;

            float guiX = screenPos.x;
            float guiY = Screen.height - screenPos.y - 18f;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                fontStyle = FontStyle.Bold
            };

            string promptText = (dist <= PICKUP_RANGE) ? $"[F] Nhặt {itemName}" : itemName;
            float w = 180f;
            Rect r = new Rect(guiX - w / 2f, guiY, w, 18f);

            // Đổ bóng đen
            labelStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), promptText, labelStyle);

            // Màu phẩm cấp
            labelStyle.normal.textColor = (dist <= PICKUP_RANGE) ? Color.yellow : tierColor;
            GUI.Label(r, promptText, labelStyle);
        }

        public static void SpawnLoot(Vector3 position, string name, string rank, Color col, string icon, int qty = 1)
        {
            GameObject lootObj = new GameObject($"Loot_{name}");
            lootObj.transform.position = position;

            var sr = lootObj.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>(icon) ?? Resources.Load<Sprite>("item_potion_hp") ?? Resources.Load<Sprite>("magic_bolt");
            sr.sortingOrder = 9; // Nằm trên sàn, dưới chân nhân vật
            sr.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            var col2d = lootObj.AddComponent<CircleCollider2D>();
            col2d.isTrigger = true;
            col2d.radius = 0.4f;

            var loot = lootObj.AddComponent<GroundLootItem>();
            loot.itemId = "loot_" + System.Guid.NewGuid().ToString().Substring(0, 6);
            loot.itemName = name;
            loot.tierRank = rank;
            loot.tierColor = col;
            loot.iconName = icon;
            loot.quantity = qty;
        }
    }
}
