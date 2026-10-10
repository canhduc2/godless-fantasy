using System;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.UI;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Công Cụ Vẽ Khối Địa Hình Trực Tiếp (In-Game Terrain Block Painter):
    /// - Đáp ứng đúng mong muốn của User: "xây map theo kiểu vẽ khối địa hình, lần sau k cần code nữa có thể tạo trực tiếp"!
    /// - Bấm phím F2 để Bật / Tắt chế độ vẽ map.
    /// - Chọn loại khối (Đá Làng, Đá Cổ, Cỏ, Nước, Tường Đá, Tẩy) rồi click / rê chuột trên bản đồ để xây dựng thế giới!
    /// </summary>
    public class TerrainPainterTool : MonoBehaviour
    {
        public static TerrainPainterTool Instance { get; private set; }

        public bool IsPaintingMode = false;
        private TerrainBlockType currentBrushType = TerrainBlockType.SafeStone;
        private int brushRadius = 0; // 0 = 1x1, 1 = 3x3

        private Camera mainCam;
        private readonly string[] blockNames = { "Đá Làng", "Đá Cổ", "Cỏ Xanh", "Nước (Cản)", "Tường Đá (Cản)", "Cổng Vàng", "🧹 Tẩy/Xóa" };
        private readonly TerrainBlockType[] blockTypes = {
            TerrainBlockType.SafeStone,
            TerrainBlockType.AncientStone,
            TerrainBlockType.Grass,
            TerrainBlockType.Water,
            TerrainBlockType.Wall,
            TerrainBlockType.Gate,
            TerrainBlockType.Erase
        };

        private void Awake()
        {
            if (Instance == null) Instance = this;
            mainCam = Camera.main;
        }

        private void Update()
        {
            // Phím tắt F2 để bật / tắt nhanh chế độ vẽ địa hình
            if (Input.GetKeyDown(KeyCode.F2))
            {
                IsPaintingMode = !IsPaintingMode;
                Debug.Log($"<color=yellow>[TERRAIN PAINTER]</color> Chế độ vẽ khối địa hình: {(IsPaintingMode ? "ĐANG BẬT [ON]" : "ĐÃ TẮT [OFF]")}");
            }

            if (IsPaintingMode)
            {
                PlayerMovement.BlockWorldClick = true;

                // Chuột trái click hoặc kéo thả để vẽ khối
                if (Input.GetMouseButton(0))
                {
                    Vector2 mousePos = Event.current != null ? Event.current.mousePosition : Input.mousePosition;
                    // Bỏ qua nếu chuột đang nằm trên thanh công cụ UI trên cùng
                    if (Input.mousePosition.y < Screen.height - 75f)
                    {
                        PaintAtMousePosition();
                    }
                }
            }
        }

        private void PaintAtMousePosition()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null || BlockMapManager.Instance == null) return;

            Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            int centerX = Mathf.RoundToInt(worldPos.x);
            int centerY = Mathf.RoundToInt(worldPos.y);

            for (int dx = -brushRadius; dx <= brushRadius; dx++)
            {
                for (int dy = -brushRadius; dy <= brushRadius; dy++)
                {
                    BlockMapManager.Instance.SetBlock(centerX + dx, centerY + dy, currentBrushType, true);
                }
            }
        }

        private void OnGUI()
        {
            if (!IsPaintingMode) return;

            // Lớp vẽ thanh công cụ (Depth 0 - Trên cùng)
            GUI.depth = FantasyUITheme.DepthModal;

            // 1. THANH CÔNG CỤ VẼ ĐỊA HÌNH Ở TRÊN CÙNG MÀN HÌNH
            float barW = Mathf.Min(Screen.width - 32f, 720f);
            float barH = 68f;
            float barX = (Screen.width - barW) / 2f;
            float barY = 12f;
            Rect barRect = new Rect(barX, barY, barW, barH);

            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
            GUI.DrawTexture(barRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawDoubleGoldFrame(barRect);

            // Tiêu đề thanh công cụ
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.88f, 0.45f) }
            };
            GUI.Label(new Rect(barX + 12, barY + 4, 300, 18), "🗺️ CHẾ ĐỘ VẼ KHỐI ĐỊA HÌNH [F2 để Bật/Tắt]", titleStyle);

            // Nút Lưu và Tải Map
            if (GUI.Button(new Rect(barX + barW - 190, barY + 4, 85, 18), "💾 LƯU MAP"))
            {
                BlockMapManager.Instance?.SaveMapToJson();
            }
            if (GUI.Button(new Rect(barX + barW - 100, barY + 4, 60, 18), "📂 TẢI LẠI"))
            {
                BlockMapManager.Instance?.LoadMapFromJson();
            }
            if (GUI.Button(new Rect(barX + barW - 34, barY + 4, 24, 18), "✕"))
            {
                IsPaintingMode = false;
            }

            // Danh sách các loại khối địa hình (Palettes)
            float btnW = (barW - 24f) / blockTypes.Length;
            for (int i = 0; i < blockTypes.Length; i++)
            {
                Rect bRect = new Rect(barX + 12f + i * btnW, barY + 26f, btnW - 4f, 32f);
                bool isSelected = (currentBrushType == blockTypes[i]);

                if (isSelected)
                {
                    GUI.color = new Color(0.2f, 0.8f, 1f, 0.35f);
                    GUI.DrawTexture(bRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    FantasyUITheme.DrawOutline(bRect, new Color(0f, 0.9f, 1f), 2);
                }
                else
                {
                    FantasyUITheme.DrawOutline(bRect, new Color(0.5f, 0.5f, 0.6f, 0.5f), 1);
                }

                var btnStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = isSelected ? Color.yellow : Color.white }
                };

                if (GUI.Button(bRect, blockNames[i], btnStyle))
                {
                    currentBrushType = blockTypes[i];
                }
            }

            // 2. VẼ KHUNG CHỈ BÁO TỌA ĐỘ Ô ĐANG TRỎ TRÊN THẾ GIỚI
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                int gridX = Mathf.RoundToInt(worldPos.x);
                int gridY = Mathf.RoundToInt(worldPos.y);

                Vector3 screenCenter = mainCam.WorldToScreenPoint(new Vector3(gridX, gridY, 0));
                Vector3 screenCorner = mainCam.WorldToScreenPoint(new Vector3(gridX + 0.5f, gridY + 0.5f, 0));
                float tileSize = Mathf.Abs(screenCorner.x - screenCenter.x) * 2f;

                Rect gridRect = new Rect(screenCenter.x - tileSize / 2f, Screen.height - screenCenter.y - tileSize / 2f, tileSize, tileSize);
                FantasyUITheme.DrawOutline(gridRect, new Color(0f, 1f, 0.7f, 0.85f), 2);
            }
        }
    }
}
