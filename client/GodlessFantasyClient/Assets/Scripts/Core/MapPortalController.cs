using System.Collections;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Bộ Điều Khiển Cổng Dịch Chuyển & Chuyển Cảnh Bản Đồ (Map Portal & Zone Transition Engine):
    /// - Kết nối các vùng đất MMORPG: Làng Tân Thủ <-> Rừng Rậm Hắc Ám <-> Hầm Ngục Ma.
    /// - Hiệu ứng hình ảnh Vòng Xoáy Không Gian (Spatial Vortex) xoay nhịp nhàng rực rỡ.
    /// - Khi người chơi bước vào cổng:
    ///   + Tự động phát âm thanh chuyển cảnh không gian (AudioManager.PlayTeleport).
    ///   + Rung chấn camera nhẹ (Camera Shake).
    ///   + Dịch chuyển vị trí tức thời sang tọa độ đích.
    ///   + Đổi nhạc nền BGM phù hợp theo vùng đất (Village Ambient <-> Wilderness Combat Pulse).
    ///   + Cơ chế Teleport Cooldown 2.5s chống dội ngược (ping-pong teleport).
    /// </summary>
    public class MapPortalController : MonoBehaviour
    {
        [Header("Portal Configuration")]
        [SerializeField] private string portalName = "Cổng Dịch Chuyển";
        [SerializeField] private string destinationZoneName = "Rừng Rậm Hắc Ám";
        [SerializeField] private Vector3 destinationCoordinates = new Vector3(12.0f, 0f, 0f);
        [SerializeField] private BgmTrack targetBgm = BgmTrack.Combat;
        [SerializeField] private Color portalGlowColor = new Color(0.6f, 0.2f, 1.0f, 0.75f);
        [SerializeField] private float triggerRadius = 1.35f;

        private static float globalTeleportCooldown = 0f;
        private SpriteRenderer innerVortexSr;
        private SpriteRenderer outerRingSr;
        private GameObject labelObj;

        public string DestinationZoneName => destinationZoneName;

        public void SetupPortal(string name, string destName, Vector3 destPos, BgmTrack bgm, Color glowCol)
        {
            portalName = name;
            destinationZoneName = destName;
            destinationCoordinates = destPos;
            targetBgm = bgm;
            portalGlowColor = glowCol;

            if (innerVortexSr != null) innerVortexSr.color = portalGlowColor;
            if (outerRingSr != null) outerRingSr.color = new Color(portalGlowColor.r, portalGlowColor.g, portalGlowColor.b, 0.45f);
        }

        private void Awake()
        {
            SetupVisuals();
            SetupCollider();
        }

        private void SetupVisuals()
        {
            // 1. Vòng hào quang ngoài
            var outerObj = new GameObject("Vortex_Outer");
            outerObj.transform.parent = transform;
            outerObj.transform.localPosition = Vector3.zero;
            outerObj.transform.localScale = new Vector3(1.7f, 1.7f, 1f);

            outerRingSr = outerObj.AddComponent<SpriteRenderer>();
            outerRingSr.sprite = Resources.Load<Sprite>("indicator_circle");
            outerRingSr.color = new Color(portalGlowColor.r, portalGlowColor.g, portalGlowColor.b, 0.35f);
            outerRingSr.sortingOrder = 6;

            // 2. Lõi xoáy không gian trong
            var innerObj = new GameObject("Vortex_Inner");
            innerObj.transform.parent = transform;
            innerObj.transform.localPosition = Vector3.zero;
            innerObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            innerVortexSr = innerObj.AddComponent<SpriteRenderer>();
            innerVortexSr.sprite = Resources.Load<Sprite>("indicator_cone") ?? Resources.Load<Sprite>("indicator_circle");
            innerVortexSr.color = portalGlowColor;
            innerVortexSr.sortingOrder = 7;
        }

        private void SetupCollider()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = triggerRadius;
        }

        private void Update()
        {
            // Hiệu ứng xoay vòng xoáy không gian
            if (innerVortexSr != null)
            {
                innerVortexSr.transform.Rotate(0, 0, -90f * Time.deltaTime);
                float pulse = 1.0f + Mathf.Sin(Time.time * 4f) * 0.15f;
                innerVortexSr.transform.localScale = new Vector3(pulse * 1.2f, pulse * 1.2f, 1f);
            }

            if (outerRingSr != null)
            {
                outerRingSr.transform.Rotate(0, 0, 45f * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Time.time < globalTeleportCooldown) return;

            if (other.CompareTag("Player") || other.gameObject.name.Contains("Player") || other.GetComponent<PlayerMovement>() != null)
            {
                ExecuteTeleport(other.transform.root);
            }
        }

        public void ExecuteTeleport(Transform playerRoot)
        {
            if (Time.time < globalTeleportCooldown) return;
            globalTeleportCooldown = Time.time + 2.5f; // Khóa 2.5s chống loop teleport

            StartCoroutine(TeleportRoutine(playerRoot));
        }

        private IEnumerator TeleportRoutine(Transform playerRoot)
        {
            Debug.Log($"<color=cyan>[PORTAL DỊCH CHUYỂN]</color> Dịch chuyển nhân vật sang: {destinationZoneName} ({destinationCoordinates})");

            // 1. Âm thanh dịch chuyển không gian
            AudioManager.PlayTeleport();

            // 2. Rung nhẹ camera
            if (CameraController.Instance != null)
            {
                CameraController.Instance.Shake(0.35f, 0.18f);
            }

            // 3. Hiện chữ nổi thông báo chuyển cảnh
            FloatingDamageManager.ShowFloatingText(
                playerRoot.position + Vector3.up * 1.3f,
                $"🌀 Chuyển Cảnh: {destinationZoneName}!",
                new Color(0.75f, 0.45f, 1f)
            );

            // 4. Dịch chuyển tọa độ nhân vật
            var rb = playerRoot.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = destinationCoordinates;
                rb.linearVelocity = Vector2.zero;
            }
            playerRoot.position = destinationCoordinates;

            var movement = playerRoot.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.StopMovement();
            }

            // 5. Chuyển nhạc nền BGM phù hợp
            AudioManager.SwitchBGM(targetBgm);

            yield return new WaitForSeconds(0.15f);

            FloatingDamageManager.ShowFloatingText(
                destinationCoordinates + Vector3.up * 1.1f,
                $"📍 Đã Đến: {destinationZoneName}",
                new Color(0.4f, 0.95f, 0.5f)
            );
        }

        private void OnGUI()
        {
            if (Camera.main == null) return;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.1f);
            if (screenPos.z < 0) return;

            float guiX = screenPos.x;
            float guiY = Screen.height - screenPos.y;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(portalGlowColor.r, portalGlowColor.g, portalGlowColor.b, 1f) }
            };

            GUI.Label(new Rect(guiX - 120, guiY - 12, 240, 24), $"🌀 {portalName}", labelStyle);
        }
    }
}
