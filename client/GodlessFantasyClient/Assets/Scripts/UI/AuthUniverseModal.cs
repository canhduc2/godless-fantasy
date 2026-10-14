using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Network;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Giao diện Đăng Nhập / Đăng Ký & Chọn Vũ Trụ (Auth & Universe Selection Modal):
    /// - Chuẩn phong cách Fantasy UI viền vàng navy hoàng kim.
    /// - Quản lý đăng ký tài khoản, đăng nhập an toàn và chọn cụm máy chủ / Vũ trụ.
    /// </summary>
    public class AuthUniverseModal : MonoBehaviour
    {
        public static AuthUniverseModal Instance { get; private set; }

        public bool IsOpen { get; private set; } = true;

        public enum ModalStep
        {
            LoginRegister,
            UniverseSelect
        }

        public ModalStep CurrentStep { get; private set; } = ModalStep.LoginRegister;

        // Login / Register Form
        private string inputUsername = "";
        private string inputPassword = "";
        private string statusMessage = "";
        private Color statusColor = Color.white;
        private bool isRegisterMode = false;
        private bool isConnecting = false;

        // Universe Selection
        private List<UniverseInfoDto> universeList = new List<UniverseInfoDto>();
        private int selectedUniverseId = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            var net = GameNetworkClient.Instance;
            if (net != null)
            {
                net.OnConnected += HandleConnected;
                net.OnDisconnected += HandleDisconnected;
                net.OnRegisterResult += HandleRegisterResult;
                net.OnLoginResult += HandleLoginResult;
                net.OnUniverseListReceived += HandleUniverseList;
                net.OnUniverseSelected += HandleUniverseSelected;
            }

            // Tự động kết nối đến server
            ConnectToServer();
        }

        private void OnDestroy()
        {
            var net = GameNetworkClient.Instance;
            if (net != null)
            {
                net.OnConnected -= HandleConnected;
                net.OnDisconnected -= HandleDisconnected;
                net.OnRegisterResult -= HandleRegisterResult;
                net.OnLoginResult -= HandleLoginResult;
                net.OnUniverseListReceived -= HandleUniverseList;
                net.OnUniverseSelected -= HandleUniverseSelected;
            }
        }

        public async void ConnectToServer()
        {
            isConnecting = true;
            statusMessage = "Đang kết nối đến máy chủ trung tâm...";
            statusColor = new Color(0.4f, 0.85f, 1f);

            var net = GameNetworkClient.Instance;
            if (net != null)
            {
                bool success = await net.ConnectAsync();
                isConnecting = false;
                if (!success)
                {
                    statusMessage = "Không thể kết nối đến máy chủ! Vui lòng khởi động server .NET 8.";
                    statusColor = new Color(1f, 0.4f, 0.4f);
                }
            }
        }

        private void HandleConnected()
        {
            statusMessage = "🟢 Đã kết nối đến máy chủ! Vui lòng đăng nhập hoặc tạo tài khoản.";
            statusColor = new Color(0.4f, 1f, 0.5f);
        }

        private void HandleDisconnected(string reason)
        {
            statusMessage = $"🔴 {reason}";
            statusColor = new Color(1f, 0.45f, 0.45f);
            CurrentStep = ModalStep.LoginRegister;
        }

        private void HandleRegisterResult(AuthRegisterRespDto resp)
        {
            if (resp.success)
            {
                statusMessage = $"🎉 {resp.message} Vui lòng bấm Đăng Nhập!";
                statusColor = new Color(0.35f, 1f, 0.5f);
                isRegisterMode = false;
            }
            else
            {
                statusMessage = $"❌ {resp.message}";
                statusColor = new Color(1f, 0.35f, 0.35f);
            }
        }

        private void HandleLoginResult(AuthLoginRespDto resp)
        {
            if (resp.success)
            {
                statusMessage = $"Chào mừng {resp.username}!";
                statusColor = new Color(1f, 0.9f, 0.3f);
                CurrentStep = ModalStep.UniverseSelect;

                // Yêu cầu danh sách Vũ Trụ
                GameNetworkClient.Instance?.SendPacket(OpCodes.UniverseListReq, new { });
            }
            else
            {
                statusMessage = $"❌ {resp.message}";
                statusColor = new Color(1f, 0.35f, 0.35f);
            }
        }

        private void HandleUniverseList(UniverseListRespDto resp)
        {
            if (resp != null && resp.universes != null)
            {
                universeList = resp.universes;
            }
        }

        private void HandleUniverseSelected(UniverseSelectRespDto resp)
        {
            if (resp.success)
            {
                if (resp.hasCharacter && resp.character != null)
                {
                    // Đã có nhân vật -> Vào thế giới ngay!
                    IsOpen = false;
                    GameNetworkClient.Instance?.SendPacket(OpCodes.WorldEnterReq, new WorldEnterReqDto());
                    Debug.Log($"<color=green>[AUTH]</color> Đã vào vũ trụ với nhân vật {resp.character.name}!");
                }
                else
                {
                    // Chưa có nhân vật -> Mở cửa sổ Khởi tạo Nhân vật!
                    IsOpen = false;
                    CharacterCreationModal.Instance?.OpenModal(resp.universeId, resp.universeName);
                }
            }
            else
            {
                statusMessage = $"❌ {resp.message}";
                statusColor = new Color(1f, 0.35f, 0.35f);
            }
        }

        public void OpenModal()
        {
            IsOpen = true;
        }

        public void CloseModal()
        {
            IsOpen = false;
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.depth = FantasyUITheme.DepthModalBase - 20; // Nằm trên toàn bộ HUD khi chưa vào game

            // Nền tối đen hoàn toàn phủ kín màn hình (Solid Dark Opaque Backdrop)
            GUI.color = new Color(0.03f, 0.04f, 0.06f, 1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (CurrentStep == ModalStep.LoginRegister)
            {
                DrawLoginRegisterWindow();
            }
            else if (CurrentStep == ModalStep.UniverseSelect)
            {
                DrawUniverseSelectWindow();
            }
        }

        private void DrawLoginRegisterWindow()
        {
            float w = 460f;
            float h = 390f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            Rect modalRect = new Rect(x, y, w, h);

            FantasyUITheme.DrawDoubleGoldFrame(modalRect, 2);

            // 1. Tiêu Đề
            string title = isRegisterMode ? "✨ TẠO TÀI KHOẢN MỚI · GODLESS FANTASY" : "⚔️ ĐĂNG NHẬP · THẾ GIỚI KHÔNG THẦN";
            Rect titleRect = new Rect(x + 20, y + 20, w - 40, 30);
            FantasyUITheme.DrawGradientGoldTitle(titleRect, title, 16);

            // Subtitle
            Rect subRect = new Rect(x + 20, y + 54, w - 40, 20);
            var subStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, fontStyle = FontStyle.Italic };
            subStyle.normal.textColor = new Color(0.75f, 0.85f, 0.95f, 0.8f);
            GUI.Label(subRect, "Hệ Thống Đa Vũ Trụ · Đồng Bộ Thời Gian Thực", subStyle);

            // 2. Form Nhập Liệu
            float fieldY = y + 95f;
            float fieldW = w - 80f;
            float fieldX = x + 40f;

            var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = FantasyUITheme.ColorTextGold;

            // Username
            GUI.Label(new Rect(fieldX, fieldY, fieldW, 20), "TÊN ĐĂNG NHẬP / TÀI KHOẢN:", labelStyle);
            inputUsername = GUI.TextField(new Rect(fieldX, fieldY + 24, fieldW, 28), inputUsername, 24);

            // Password
            fieldY += 65f;
            GUI.Label(new Rect(fieldX, fieldY, fieldW, 20), "MẬT KHẨU BẢO MẬT:", labelStyle);
            inputPassword = GUI.PasswordField(new Rect(fieldX, fieldY + 24, fieldW, 28), inputPassword, '*', 32);

            // 3. Thông Báo Trạng Thái
            fieldY += 62f;
            if (!string.IsNullOrEmpty(statusMessage))
            {
                var msgStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, wordWrap = true, fontStyle = FontStyle.Bold };
                msgStyle.normal.textColor = statusColor;
                GUI.Label(new Rect(fieldX, fieldY, fieldW, 36), statusMessage, msgStyle);
            }

            // 4. Các Nút Thao Tác
            float btnY = y + h - 70f;
            float btnW = (fieldW - 16f) / 2f;

            if (isRegisterMode)
            {
                // Nút Đăng Ký
                if (FantasyUITheme.DrawFantasyButton(new Rect(fieldX, btnY, btnW, 36), "XÁC NHẬN ĐĂNG KÝ", 12, true, new Color(0.4f, 1f, 0.5f)))
                {
                    TryRegister();
                }

                // Nút Quay lại Đăng Nhập
                if (FantasyUITheme.DrawFantasyButton(new Rect(fieldX + btnW + 16f, btnY, btnW, 36), "QUAY LẠI ĐĂNG NHẬP", 12, false, Color.white))
                {
                    isRegisterMode = false;
                    statusMessage = "";
                }
            }
            else
            {
                // Nút Đăng Nhập
                if (FantasyUITheme.DrawFantasyButton(new Rect(fieldX, btnY, btnW, 36), "ĐĂNG NHẬP NGAY", 12, true, FantasyUITheme.ColorTextGold))
                {
                    TryLogin();
                }

                // Nút Chuyển sang Đăng Ký
                if (FantasyUITheme.DrawFantasyButton(new Rect(fieldX + btnW + 16f, btnY, btnW, 36), "TẠO TÀI KHOẢN MỚI", 12, false, new Color(0.6f, 0.9f, 1f)))
                {
                    isRegisterMode = true;
                    statusMessage = "";
                }
            }

            // Phím tắt Enter để submit
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                if (isRegisterMode) TryRegister();
                else TryLogin();
                Event.current.Use();
            }
        }

        private void TryLogin()
        {
            if (string.IsNullOrWhiteSpace(inputUsername) || string.IsNullOrWhiteSpace(inputPassword))
            {
                statusMessage = "Vui lòng nhập đầy đủ tên tài khoản và mật khẩu!";
                statusColor = new Color(1f, 0.4f, 0.4f);
                return;
            }

            var net = GameNetworkClient.Instance;
            if (net == null || !net.IsConnected)
            {
                statusMessage = "Chưa kết nối đến máy chủ! Đang thử kết nối lại...";
                statusColor = new Color(1f, 0.6f, 0.3f);
                ConnectToServer();
                return;
            }

            statusMessage = "Đang xác thực thông tin đăng nhập...";
            statusColor = new Color(0.4f, 0.85f, 1f);

            net.SendPacket(OpCodes.AuthLoginReq, new AuthLoginReqDto
            {
                username = inputUsername.Trim(),
                password = inputPassword
            });
        }

        private void TryRegister()
        {
            if (string.IsNullOrWhiteSpace(inputUsername) || inputUsername.Length < 3)
            {
                statusMessage = "Tên tài khoản phải từ 3 đến 20 ký tự!";
                statusColor = new Color(1f, 0.4f, 0.4f);
                return;
            }
            if (string.IsNullOrWhiteSpace(inputPassword) || inputPassword.Length < 4)
            {
                statusMessage = "Mật khẩu phải từ 4 ký tự trở lên!";
                statusColor = new Color(1f, 0.4f, 0.4f);
                return;
            }

            var net = GameNetworkClient.Instance;
            if (net == null || !net.IsConnected)
            {
                statusMessage = "Chưa kết nối đến máy chủ! Đang thử kết nối lại...";
                statusColor = new Color(1f, 0.6f, 0.3f);
                ConnectToServer();
                return;
            }

            statusMessage = "Đang khởi tạo tài khoản mới trên máy chủ...";
            statusColor = new Color(0.4f, 0.85f, 1f);

            net.SendPacket(OpCodes.AuthRegisterReq, new AuthRegisterReqDto
            {
                username = inputUsername.Trim(),
                password = inputPassword
            });
        }

        private void DrawUniverseSelectWindow()
        {
            float w = 620f;
            float h = 480f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            Rect modalRect = new Rect(x, y, w, h);

            FantasyUITheme.DrawDoubleGoldFrame(modalRect, 2);

            // 1. Tiêu Đề
            string username = GameNetworkClient.Instance?.LoggedInUsername ?? "Hiệp Sĩ";
            Rect titleRect = new Rect(x + 20, y + 18, w - 40, 30);
            FantasyUITheme.DrawGradientGoldTitle(titleRect, $"🌌 CHỌN VŨ TRỤ · TÀI KHOẢN: {username.ToUpper()}", 16);

            // Nút Refresh
            if (FantasyUITheme.DrawSquareIconButton(new Rect(x + w - 48, y + 16, 30, 30), "🔄", 14))
            {
                GameNetworkClient.Instance?.SendPacket(OpCodes.UniverseListReq, new { });
            }

            // 2. Danh Sách Vũ Trụ
            float listY = y + 60f;
            float listW = w - 40f;
            float listX = x + 20f;

            if (universeList.Count == 0)
            {
                var emptyStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
                emptyStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(listX, listY + 80, listW, 40), "Đang tải danh sách Vũ Trụ từ máy chủ...", emptyStyle);
            }
            else
            {
                for (int i = 0; i < universeList.Count; i++)
                {
                    var u = universeList[i];
                    bool isSelected = (selectedUniverseId == u.id);
                    float cardH = 92f;
                    float cardY = listY + i * (cardH + 12f);
                    Rect cardRect = new Rect(listX, cardY, listW, cardH);

                    // Nền card
                    Color bg = isSelected ? new Color(0.10f, 0.16f, 0.28f, 0.95f) : new Color(0.05f, 0.08f, 0.14f, 0.90f);
                    GUI.color = bg;
                    GUI.DrawTexture(cardRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    // Viền
                    Color borderCol = isSelected ? FantasyUITheme.ColorBorderHighlight : FantasyUITheme.ColorBorderGold;
                    GUI.color = borderCol;
                    GUI.DrawTexture(new Rect(cardRect.x, cardRect.y, cardRect.width, 1), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(cardRect.x, cardRect.y + cardRect.height - 1, cardRect.width, 1), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(cardRect.x, cardRect.y, 1, cardRect.height), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(cardRect.x + cardRect.width - 1, cardRect.y, 1, cardRect.height), Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    // Tên Vũ Trụ & Trạng thái
                    var nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
                    nameStyle.normal.textColor = isSelected ? new Color(1f, 0.92f, 0.4f) : Color.white;
                    GUI.Label(new Rect(cardRect.x + 14, cardRect.y + 10, 320, 22), u.name, nameStyle);

                    // Badge Online & Tổng Cư Dân Đã Định Cư
                    var badgeStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
                    badgeStyle.normal.textColor = new Color(0.35f, 1f, 0.5f);
                    GUI.Label(new Rect(cardRect.x + 14, cardRect.y + 34, 420, 18), $"🟢 Trực Tuyến: {u.onlinePlayers}/{u.maxPlayers}   👥 Cư Dân: {u.totalCharacters} nhân vật", badgeStyle);

                    // Mô tả
                    var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
                    descStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f, 0.8f);
                    GUI.Label(new Rect(cardRect.x + 14, cardRect.y + 54, 380, 30), u.description, descStyle);

                    // Nút Hành Động Bên Phải
                    float actionBtnW = 145f;
                    float actionBtnH = 38f;
                    Rect actionBtnRect = new Rect(cardRect.x + cardRect.width - actionBtnW - 14, cardRect.y + (cardH - actionBtnH) / 2f, actionBtnW, actionBtnH);

                    string btnText = u.hasCharacter ? "⚔️ VÀO GAME" : "✨ TẠO NHÂN VẬT";
                    Color btnCol = u.hasCharacter ? new Color(1f, 0.85f, 0.3f) : new Color(0.4f, 1f, 0.6f);

                    if (FantasyUITheme.DrawFantasyButton(actionBtnRect, btnText, 12, isSelected, btnCol))
                    {
                        selectedUniverseId = u.id;
                        SelectUniverse(u.id);
                    }
                }
            }

            // Nút Đăng Xuất ở dưới cùng
            float logoutBtnW = 140f;
            float logoutBtnY = y + h - 50f;
            if (FantasyUITheme.DrawFantasyButton(new Rect(x + 20, logoutBtnY, logoutBtnW, 32), "⬅ ĐĂNG XUẤT", 11, false, new Color(0.85f, 0.4f, 0.4f)))
            {
                CurrentStep = ModalStep.LoginRegister;
                inputPassword = "";
                statusMessage = "Đã đăng xuất tài khoản.";
                statusColor = Color.white;
            }
        }

        private void SelectUniverse(int universeId)
        {
            var net = GameNetworkClient.Instance;
            if (net == null || !net.IsConnected) return;

            net.SendPacket(OpCodes.UniverseSelectReq, new UniverseSelectReqDto
            {
                universeId = universeId
            });
        }
    }
}
