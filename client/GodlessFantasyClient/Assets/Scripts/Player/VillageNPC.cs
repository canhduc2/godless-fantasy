using UnityEngine;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// NPC Dân Làng / Trưởng Làng Tân Thủ:
    /// - Đứng trong Vùng An Toàn (Safe Zone), miễn nhiễm mọi sát thương.
    /// - Khi người chơi đến gần hoặc click vào: Mở hộp thoại nhiệm vụ hướng dẫn người mới.
    /// </summary>
    public class VillageNPC : MonoBehaviour
    {
        [Header("NPC Specs")]
        [SerializeField] private string npcName = "Trưởng Làng Eldrin";
        [SerializeField] private string dialogue = "Chào mừng dũng sĩ đến với Godless Fantasy! Hãy sử dụng Chiêu 1 [Phím 1] và Lướt [Shift] để luyện tập với các Hình Nộm gỗ ngoài kia trước khi bước vào vùng chiến trường đẫm máu!";

        private bool isPlayerNearby = false;
        private bool showDialogue = false;
        private Transform playerTransform;

        private void Start()
        {
            var health = GetComponent<Health>();
            if (health != null) health.IsInSafeZone = true;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                var player = FindFirstObjectByType<PlayerMovement>();
                if (player != null) playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                isPlayerNearby = (dist <= 2.2f);
                if (!isPlayerNearby) showDialogue = false;
            }
        }

        private void OnMouseDown()
        {
            if (isPlayerNearby)
            {
                showDialogue = !showDialogue;
            }
        }

        private void OnGUI()
        {
            if (isPlayerNearby && !showDialogue)
            {
                Camera cam = Camera.main;
                if (cam == null) return;
                Vector3 screenPos = cam.WorldToScreenPoint(transform.position + new Vector3(0, 1.2f, 0));
                if (screenPos.z > 0)
                {
                    GUI.color = new Color(1f, 0.9f, 0.4f);
                    GUI.Label(new Rect(screenPos.x - 60, Screen.height - screenPos.y - 12, 120, 24), "[BẤM NÓI CHUYỆN 💬]");
                    GUI.color = Color.white;
                }
            }

            if (showDialogue)
            {
                float boxW = 380f;
                float boxH = 110f;
                float boxX = (Screen.width - boxW) / 2f;
                float boxY = Screen.height - boxH - 90f;

                Rect r = new Rect(boxX, boxY, boxW, boxH);
                GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.96f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = new Color(0.85f, 0.70f, 0.28f);
                // Viền vàng
                GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, r.width, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x, r.y, 2, r.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y, 2, r.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 0.85f, 0.35f) }
                };
                var bodyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    wordWrap = true,
                    normal = { textColor = Color.white }
                };

                GUI.Label(new Rect(r.x + 12, r.y + 8, r.width - 40, 20), $"📜 {npcName} (Trưởng Làng Tân Thủ)", titleStyle);
                if (GUI.Button(new Rect(r.x + r.width - 28, r.y + 6, 22, 20), "X"))
                {
                    showDialogue = false;
                }

                GUI.Label(new Rect(r.x + 12, r.y + 30, r.width - 24, 70), dialogue, bodyStyle);
            }
        }
    }
}
