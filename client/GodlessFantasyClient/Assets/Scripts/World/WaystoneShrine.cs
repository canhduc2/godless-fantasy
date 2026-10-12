using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Core;
using GodlessFantasy.Combat;
using GodlessFantasy.UI;

namespace GodlessFantasy.World
{
    /// <summary>
    /// Trụ Hồi Sinh Đá Cổ (Waystone Shrine / Respawn Obelisk):
    /// - Đặt rải rác trên thế giới (Làng, Bìa Rừng, Cầu Đá, Thung Lũng, Cửa Hầm Ngục).
    /// - Khi người chơi đến gần bấm phím [E]: Kích hoạt và lưu trụ này làm Điểm Hồi Sinh mặc định.
    /// - Khi tử trận trên chiến trường, người chơi sẽ hồi sinh ngay tại Trụ Hồi Sinh gần nhất đã lưu.
    /// - Pha lê trên đỉnh trụ có hiệu ứng lơ lửng và phát sáng hào quang ma thuật.
    /// </summary>
    public class WaystoneShrine : MonoBehaviour
    {
        public static WaystoneShrine ActiveRespawnPoint { get; private set; }
        public static List<WaystoneShrine> AllShrines { get; private set; } = new List<WaystoneShrine>();

        [Header("Shrine Info")]
        [SerializeField] private string shrineName = "Trụ Làng Tân Thủ";
        [SerializeField] private bool isDefaultActive = false;

        private bool isPlayerNearby = false;
        private Transform playerTransform;
        private SpriteRenderer spriteRenderer;
        private Sprite activeSprite;
        private Sprite inactiveSprite;

        public string ShrineName => shrineName;
        public bool IsCurrentActive => ActiveRespawnPoint == this;

        public static Vector3 GetRespawnPosition()
        {
            if (ActiveRespawnPoint != null)
            {
                return ActiveRespawnPoint.transform.position + new Vector3(0, -0.6f, 0);
            }
            return new Vector3(-70.0f, 0, 0); // Vị trí mặc định ở quảng trường Làng
        }

        public static string GetRespawnLocationName()
        {
            return (ActiveRespawnPoint != null) ? ActiveRespawnPoint.ShrineName : "Quảng Trường Làng Tân Thủ";
        }

        public void Setup(string name, bool defaultActive = false)
        {
            shrineName = name;
            isDefaultActive = defaultActive;
            if (defaultActive && ActiveRespawnPoint == null)
            {
                ActiveRespawnPoint = this;
            }
        }

        private void Awake()
        {
            if (!AllShrines.Contains(this)) AllShrines.Add(this);
            spriteRenderer = GetComponent<SpriteRenderer>();

            activeSprite = Resources.Load<Sprite>("prop_waystone_shrine");
            inactiveSprite = Resources.Load<Sprite>("prop_waystone_shrine");

            if (isDefaultActive && ActiveRespawnPoint == null)
            {
                ActiveRespawnPoint = this;
            }
        }

        private void OnDestroy()
        {
            AllShrines.Remove(this);
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
                if (player != null) playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                isPlayerNearby = (dist <= 2.8f);

                // Bấm phím E để kích hoạt điểm hồi sinh
                if (isPlayerNearby && Core.GameInput.GetInteractKeyDown())
                {
                    ActivateShrine();
                }
            }
        }

        private void OnMouseDown()
        {
            if (isPlayerNearby)
            {
                ActivateShrine();
            }
        }

        public void ActivateShrine()
        {
            if (ActiveRespawnPoint == this)
            {
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.6f,
                    $"🏛️ Điểm hồi sinh hiện tại: {shrineName}", new Color(0.4f, 0.9f, 1f));
                return;
            }

            ActiveRespawnPoint = this;
            AudioManager.PlayLevelUp();

            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.8f,
                $"✨ ĐÃ LƯU ĐIỂM HỒI SINH TẠI: {shrineName}!", new Color(1f, 0.85f, 0.25f));
            Debug.Log($"<color=cyan>[WAYSTONE]</color> Điểm hồi sinh đã chuyển về: {shrineName} ({transform.position})");
        }

        private void OnGUI()
        {
            if (!isPlayerNearby) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 screenPos = cam.WorldToScreenPoint(transform.position + new Vector3(0, 1.45f, 0));
            if (screenPos.z <= 0) return;

            string promptText = IsCurrentActive
                ? $"🏛️ [ {shrineName.ToUpper()} · ĐIỂM HỒI SINH HIỆN TẠI ✨ ]"
                : $"[ BẤM E ĐỂ LƯU ĐIỂM HỒI SINH: {shrineName.ToUpper()} 🏛️ ]";

            Color textColor = IsCurrentActive ? new Color(0.4f, 1.0f, 0.6f) : new Color(1f, 0.88f, 0.35f);

            var promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                clipping = TextClipping.Overflow,
                normal = { textColor = textColor }
            };

            Vector2 promptSize = promptStyle.CalcSize(new GUIContent(promptText));
            float promptW = Mathf.Max(220f, promptSize.x + 24f);
            float promptH = 22f;
            Rect promptRect = new Rect(screenPos.x - promptW / 2f, Screen.height - screenPos.y - 14, promptW, promptH);

            if (CombatHUD.IsAnyModalBlockingScreen(promptRect.x + promptRect.width / 2f, promptRect.y + promptRect.height / 2f)) return;

            GUI.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);
            GUI.DrawTexture(promptRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(promptRect);

            GUI.Label(promptRect, promptText, promptStyle);
        }

        private void DrawDoubleGoldFrame(Rect r)
        {
            GUI.color = IsCurrentActive ? new Color(0.2f, 0.9f, 0.5f, 0.95f) : new Color(0.85f, 0.72f, 0.28f, 0.95f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, 2, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y, 2, r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
