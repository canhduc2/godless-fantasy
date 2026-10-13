using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Vật Phẩm Rơi Dưới Đất Tự Do Nhặt (Free-for-All Ground Loot Pickup):
    /// - Khi quái chết hoặc người chơi tử trận ngoài SafeZone (rủi ro rơi 5%):
    ///   Vật phẩm bật tung tỏa ra xung quanh sàn map (burst scatter animation) tại đúng vị trí tử trận!
    /// - Ai chạy lại gần bấm [F] thì nhặt được (cơ chế tranh đồ PvP/PvE tự do).
    /// - Bảo toàn 100% dữ liệu gốc của vật phẩm (Trang bị, Bí kíp, Dược phẩm...).
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

        // Lưu trữ bảo toàn nguyên vẹn vật phẩm gốc nếu có (Trang bị, Sách kỹ năng...)
        public InventoryItem originalItem = null;

        private Vector3 basePosition;
        private Transform playerTransform;
        private const float PICKUP_RANGE = 1.8f;

        // Hiệu ứng rơi tung tóe ra sàn map (Death Burst Animation)
        public bool isBursting = false;
        public Vector3 startPos;
        public Vector3 targetPos;
        public float burstDuration = 0.55f;
        public float burstTimer = 0f;

        private void Start()
        {
            if (!isBursting)
            {
                basePosition = transform.position;
            }
            var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
            if (player != null) playerTransform = player.transform;
        }

        private void Update()
        {
            // 1. Nếu đang trong hiệu ứng bật tung ra map khi tử trận (Arc trajectory)
            if (isBursting)
            {
                burstTimer += Time.deltaTime;
                float t = Mathf.Clamp01(burstTimer / burstDuration);
                float easeOut = Mathf.Sin(t * Mathf.PI * 0.5f);
                float arcY = Mathf.Sin(t * Mathf.PI) * 1.35f; // Bật vòng cung lên không trung rồi tiếp đất
                transform.position = Vector3.Lerp(startPos, targetPos, easeOut) + new Vector3(0, arcY, 0);
                basePosition = transform.position;

                if (t >= 1.0f)
                {
                    isBursting = false;
                    basePosition = targetPos;
                    transform.position = targetPos;
                }
                return;
            }

            // 2. Hiệu ứng nhấp nhô lơ lửng nhẹ trên mặt đất
            float yOffset = Mathf.Sin(Time.time * 5f) * 0.08f;
            transform.position = basePosition + new Vector3(0, yOffset, 0);

            // 3. Kiểm tra phím [F] để nhặt đồ khi đứng gần
            if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= PICKUP_RANGE)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    TryPickup();
                }
            }
        }

        public void TryPickup()
        {
            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv != null)
            {
                InventoryItem itemToGive;
                if (originalItem != null)
                {
                    itemToGive = originalItem.Clone();
                    itemToGive.quantity = quantity;
                }
                else
                {
                    itemToGive = new InventoryItem(itemId, itemName, ItemType.Consumable, EquipSlot.None, tierRank, tierColor, iconName,
                        $"Vật phẩm nhặt được từ sàn đất tự do.", "Nhấn chuột phải để sử dụng.", quantity);
                }

                if (!inv.CanAddItem(itemToGive))
                {
                    FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 0.8f, "⚠️ Hành trang đã đầy!", Color.red);
                    Debug.LogWarning($"<color=yellow>[LOOT PICKUP]</color> Không thể nhặt '{itemName}' vì hành trang đã đầy!");
                    return;
                }

                if (inv.AddItem(itemToGive))
                {
                    FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 0.8f, $"🎒 Nhặt được: {itemName} x{quantity}!", tierColor);
                    Debug.Log($"<color=green>[LOOT PICKUP]</color> Đã nhặt thành công {itemName}!");
                    GodlessFantasy.Core.AudioManager.PlayPickup();
                    Destroy(gameObject);
                }
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
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };

            var inv = FindAnyObjectByType<InventoryManager>();
            var testItem = originalItem != null ? originalItem : new InventoryItem(itemId, itemName, ItemType.Consumable, EquipSlot.None, tierRank, tierColor, iconName, "", "", quantity);
            bool isFull = (inv != null && !inv.CanAddItem(testItem));

            string promptText;
            if (dist <= PICKUP_RANGE)
            {
                promptText = isFull ? $"⚠️ ĐẦY TÚI: {itemName}" : $"[F] Nhặt: {itemName}";
            }
            else
            {
                promptText = itemName;
            }

            Vector2 textSize = labelStyle.CalcSize(new GUIContent(promptText));
            float w = textSize.x + 14f;
            Rect r = new Rect(guiX - w / 2f, guiY, w, 20f);

            // Nền tối mờ chống lẫn vào map / quái
            GUI.color = new Color(0.04f, 0.06f, 0.1f, 0.85f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Đổ bóng chữ đen
            labelStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), promptText, labelStyle);

            // Màu hiển thị
            if (dist <= PICKUP_RANGE)
            {
                labelStyle.normal.textColor = isFull ? new Color(1f, 0.35f, 0.35f) : Color.yellow;
            }
            else
            {
                labelStyle.normal.textColor = tierColor;
            }
            GUI.Label(r, promptText, labelStyle);
        }

        public static void SpawnLoot(Vector3 position, string name, string rank, Color col, string icon, int qty = 1, string specificId = null)
        {
            GameObject lootObj = new GameObject($"Loot_{name}");
            lootObj.transform.position = position;

            // Đặt layer thành Ignore Raycast (Layer 2) để không chắn click chuột vào quái vật
            lootObj.layer = LayerMask.NameToLayer("Ignore Raycast");

            var sr = lootObj.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>(icon) ?? Resources.Load<Sprite>("item_potion_hp") ?? Resources.Load<Sprite>("magic_bolt");
            sr.sortingOrder = 9; // Nằm trên sàn, dưới chân nhân vật
            sr.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            var col2d = lootObj.AddComponent<CircleCollider2D>();
            col2d.isTrigger = true;
            col2d.radius = 0.4f;

            var loot = lootObj.AddComponent<GroundLootItem>();
            loot.itemId = !string.IsNullOrEmpty(specificId) ? specificId : ("loot_" + System.Guid.NewGuid().ToString().Substring(0, 6));
            loot.itemName = name;
            loot.tierRank = rank;
            loot.tierColor = col;
            loot.iconName = icon;
            loot.quantity = qty;
        }

        /// <summary>
        /// Rơi vật phẩm đầy đủ từ nhân vật hoặc rương (Hỗ trợ bắn tung ra xung quanh map khi chết)
        /// </summary>
        public static void SpawnLoot(Vector3 originPos, InventoryItem sourceItem, bool burstOut = false, float burstRadius = 2.2f)
        {
            if (sourceItem == null) return;

            GameObject lootObj = new GameObject($"Loot_{sourceItem.itemName}");
            lootObj.transform.position = originPos;
            lootObj.layer = LayerMask.NameToLayer("Ignore Raycast");

            var sr = lootObj.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>(sourceItem.iconName) ?? Resources.Load<Sprite>("item_potion_hp") ?? Resources.Load<Sprite>("magic_bolt");
            sr.sortingOrder = 9;
            sr.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            var col2d = lootObj.AddComponent<CircleCollider2D>();
            col2d.isTrigger = true;
            col2d.radius = 0.45f;

            var loot = lootObj.AddComponent<GroundLootItem>();
            loot.originalItem = sourceItem.Clone();
            loot.itemId = sourceItem.id;
            loot.itemName = sourceItem.itemName;
            loot.tierRank = sourceItem.tierRank;
            loot.tierColor = sourceItem.tierColor;
            loot.iconName = sourceItem.iconName;
            loot.quantity = sourceItem.quantity;

            if (burstOut)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = UnityEngine.Random.Range(1.2f, burstRadius);
                loot.startPos = originPos;
                loot.targetPos = originPos + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0f);
                loot.burstDuration = UnityEngine.Random.Range(0.48f, 0.62f);
                loot.burstTimer = 0f;
                loot.isBursting = true;
                lootObj.transform.position = originPos;
            }
        }
    }
}
