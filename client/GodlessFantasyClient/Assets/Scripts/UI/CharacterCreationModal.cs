using System;
using UnityEngine;
using GodlessFantasy.Network;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Giao diện Khởi Tạo Nhân Vật (Character Creation Modal):
    /// - Chuẩn thiết kế GDD DOC-03: Chọn Giới tính (Nam/Nữ), 4 Chức nghiệp khởi đầu.
    /// - Cơ chế "Đổ Xúc Xắc (Roll Stats)": Random 1 trong 7 Hệ Nguyên Tố và 40 điểm thuộc tính (STR, INT, VIT, AGI).
    /// - Preview trực quan và gửi lệnh tạo Server-Authoritative lên Server .NET 8.
    /// </summary>
    public class CharacterCreationModal : MonoBehaviour
    {
        public static CharacterCreationModal Instance { get; private set; }

        public bool IsOpen { get; private set; } = false;

        private int targetUniverseId = 1;
        private string targetUniverseName = "Vũ Trụ 1 - Sơ Khai";

        // Form Fields
        private string inputName = "Lữ Khách";
        private bool isFemale = false; // false = Nam, true = Nữ
        private string selectedClass = "Kị Sĩ";
        private string currentElement = "Hỏa";
        private int currentStr = 11;
        private int currentInt = 8;
        private int currentVit = 12;
        private int currentAgi = 9;

        private string statusMessage = "";
        private Color statusColor = Color.white;
        private bool isRolling = false;

        private readonly string[] Classes = new[] { "Kị Sĩ", "Xạ Thủ", "Pháp Sư", "Võ Sĩ" };
        private readonly string[] ClassDescriptions = new[]
        {
            "🛡️ Tiên phong càn quét, Máu và Giáp cao nhất, sử dụng Kiếm & Khiên.",
            "🏹 Xạ kích tầm xa, Tốc độ di chuyển và Bạo kích cao, sử dụng Cung tên.",
            "🔮 Bậc thầy nguyên tố, Ma lực dồi dào, sát thương diện rộng và khống chế.",
            "🥊 Quyền pháp áp sát, Đòn đánh liên hoàn, tỉ lệ Choáng và Phá thế cao."
        };

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
                net.OnRollStatsResult += HandleRollResult;
                net.OnCharacterCreated += HandleCharacterCreated;
            }
        }

        private void OnDestroy()
        {
            var net = GameNetworkClient.Instance;
            if (net != null)
            {
                net.OnRollStatsResult -= HandleRollResult;
                net.OnCharacterCreated -= HandleCharacterCreated;
            }
        }

        private int freeRerollLeft = 1;

        public void OpenModal(int universeId, string universeName)
        {
            targetUniverseId = universeId;
            targetUniverseName = universeName;
            IsOpen = true;
            freeRerollLeft = 1;
            statusMessage = "Hãy đặt tên và Đổ xúc xắc để nhận Hệ Nguyên Tố & Điểm Thuộc Tính!";
            statusColor = new Color(0.4f, 0.9f, 1f);

            // Tự động roll lần đầu tiên
            RollStats();
        }

        public void CloseModal()
        {
            IsOpen = false;
        }

        private void RollStats()
        {
            isRolling = true;
            var net = GameNetworkClient.Instance;
            if (net != null && net.IsConnected)
            {
                net.SendPacket(OpCodes.CharRollStatsReq, new CharRollStatsReqDto
                {
                    className = selectedClass
                });
            }
        }

        private void HandleRollResult(CharRollStatsRespDto resp)
        {
            isRolling = false;
            currentElement = resp.element;
            currentStr = resp.str;
            currentInt = resp.intStat;
            currentVit = resp.vit;
            currentAgi = resp.agi;

            statusMessage = $"🎲 Đã đổ xúc xắc thành công! Bạn nhận được Hệ {currentElement}!";
            statusColor = GetElementColor(currentElement);
        }

        private void HandleCharacterCreated(CharCreateRespDto resp)
        {
            if (resp.success)
            {
                statusMessage = "🎉 Khởi tạo nhân vật thành công! Đang bước vào thế giới...";
                statusColor = new Color(0.35f, 1f, 0.5f);
                IsOpen = false;

                // Đồng bộ thuộc tính sang PlayerStats local
                ApplyToLocalPlayer(resp.character);

                // Vào thế giới!
                GameNetworkClient.Instance?.SendPacket(OpCodes.WorldEnterReq, new WorldEnterReqDto());
            }
            else
            {
                statusMessage = $"❌ {resp.message}";
                statusColor = new Color(1f, 0.35f, 0.35f);
            }
        }

        private void ApplyToLocalPlayer(CharacterInfoDto c)
        {
            if (c == null) return;
            var ps = Player.PlayerStats.Instance;
            if (ps != null)
            {
                // Áp dụng định danh
                var pObj = ps.gameObject;
                var rig = pObj.GetComponent<Player.ProceduralCharacterRig2D>();
                if (rig != null)
                {
                    rig.SetGender(string.Equals(c.gender, "Nữ", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.depth = FantasyUITheme.DepthModalBase - 25; // Nằm trên toàn bộ màn hình

            // Nền tối đen hoàn toàn phủ kín màn hình (Solid Dark Opaque Backdrop)
            GUI.color = new Color(0.03f, 0.04f, 0.06f, 1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float w = 780f;
            float h = 540f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            Rect modalRect = new Rect(x, y, w, h);

            FantasyUITheme.DrawDoubleGoldFrame(modalRect, 2);

            // 1. Tiêu Đề
            Rect titleRect = new Rect(x + 20, y + 16, w - 40, 30);
            FantasyUITheme.DrawGradientGoldTitle(titleRect, $"⚔️ KHỞI TẠO ANH HÙNG · {targetUniverseName.ToUpper()}", 16);

            // Chia thành 2 cột lớn: Cột Trái (Tên, Giới tính, Chức nghiệp) & Cột Phải (Xúc xắc Hệ & Thuộc tính)
            float colW = (w - 60f) / 2f;
            float col1X = x + 20f;
            float col2X = col1X + colW + 20f;
            float contentY = y + 55f;

            // ================= CỘT 1: THÔNG TIN CƠ BẢN & CHỨC NGHIỆP =================
            DrawBasicInfoColumn(new Rect(col1X, contentY, colW, 400f));

            // ================= CỘT 2: ĐỔ XÚC XẮC NGUYÊN TỐ & THUỘC TÍNH =================
            DrawAttributesColumn(new Rect(col2X, contentY, colW, 400f));

            // ================= DƯỚI CÙNG: DÒNG TRẠNG THÁI & NÚT TẠO =================
            float bottomY = y + h - 68f;

            // Status message
            if (!string.IsNullOrEmpty(statusMessage))
            {
                var msgStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, fontStyle = FontStyle.Bold };
                msgStyle.normal.textColor = statusColor;
                GUI.Label(new Rect(x + 20, bottomY - 24, w - 40, 20), statusMessage, msgStyle);
            }

            // Nút Hủy / Quay lại
            if (FantasyUITheme.DrawFantasyButton(new Rect(x + 30, bottomY, 140, 38), "⬅ QUAY LẠI", 12, false, new Color(0.85f, 0.4f, 0.4f)))
            {
                IsOpen = false;
                AuthUniverseModal.Instance?.OpenModal();
            }

            // Nút Xác Nhận Tạo & Bước Vào Thế Giới
            float createBtnW = 320f;
            float createBtnX = x + w - createBtnW - 30;
            if (FantasyUITheme.DrawFantasyButton(new Rect(createBtnX, bottomY, createBtnW, 38), "⚔️ KHỞI TẠO & BƯỚC VÀO THẾ GIỚI", 13, true, new Color(1f, 0.90f, 0.35f)))
            {
                SubmitCreation();
            }
        }

        private void DrawBasicInfoColumn(Rect r)
        {
            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = FantasyUITheme.ColorTextGold;

            float curY = r.y;

            // 1. Tên Nhân Vật (Giới hạn tối đa 25 ký tự)
            GUI.Label(new Rect(r.x, curY, r.width, 20), $"1. HỌ VÀ TÊN NHÂN VẬT [{inputName.Length}/25]:", headerStyle);
            inputName = GUI.TextField(new Rect(r.x, curY + 22, r.width, 28), inputName, 25);

            // 2. Giới Tính
            curY += 60f;
            GUI.Label(new Rect(r.x, curY, r.width, 20), "2. GIỚI TÍNH CƠ BẢN:", headerStyle);

            float genderBtnW = (r.width - 12f) / 2f;
            bool pickMale = FantasyUITheme.DrawFantasyButton(new Rect(r.x, curY + 22, genderBtnW, 32), "♂ NAM", 12, !isFemale, !isFemale ? new Color(0.4f, 0.85f, 1f) : Color.gray);
            bool pickFemale = FantasyUITheme.DrawFantasyButton(new Rect(r.x + genderBtnW + 12f, curY + 22, genderBtnW, 32), "♀ NỮ", 12, isFemale, isFemale ? new Color(1f, 0.6f, 0.85f) : Color.gray);

            if (pickMale) isFemale = false;
            if (pickFemale) isFemale = true;

            // 3. Chọn 4 Chức Nghiệp Khởi Đầu
            curY += 65f;
            GUI.Label(new Rect(r.x, curY, r.width, 20), "3. CHỌN HỆ CHIẾN ĐẤU (4 HỆ CƠ BẢN):", headerStyle);

            float classBtnW = (r.width - 12f) / 2f;
            for (int i = 0; i < Classes.Length; i++)
            {
                string cls = Classes[i];
                bool isSelected = string.Equals(selectedClass, cls);
                float btnX = (i % 2 == 0) ? r.x : (r.x + classBtnW + 12f);
                float btnY = curY + 22f + (i / 2) * 36f;

                Color col = isSelected ? new Color(1f, 0.88f, 0.35f) : Color.white;
                if (FantasyUITheme.DrawFantasyButton(new Rect(btnX, btnY, classBtnW, 30), cls, 11, isSelected, col))
                {
                    selectedClass = cls;
                }
            }

            // Mô tả chức nghiệp đang chọn
            curY += 105f;
            int classIdx = Array.IndexOf(Classes, selectedClass);
            if (classIdx >= 0 && classIdx < ClassDescriptions.Length)
            {
                Rect descRect = new Rect(r.x, curY, r.width, 70f);
                GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.95f);
                GUI.DrawTexture(descRect, Texture2D.whiteTexture);
                GUI.color = FantasyUITheme.ColorBorderInner;
                GUI.DrawTexture(new Rect(descRect.x, descRect.y, descRect.width, 1), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(descRect.x, descRect.y + descRect.height - 1, descRect.width, 1), Texture2D.whiteTexture);
                GUI.color = Color.white;

                var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
                descStyle.normal.textColor = new Color(0.85f, 0.92f, 1f, 0.9f);
                GUI.Label(new Rect(descRect.x + 8, descRect.y + 6, descRect.width - 16, descRect.height - 12),
                    $"<b>{selectedClass}</b>:\n{ClassDescriptions[classIdx]}", descStyle);
            }
        }

        private void DrawAttributesColumn(Rect r)
        {
            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = FantasyUITheme.ColorTextGold;

            float curY = r.y;

            // 1. Hệ Nguyên Tố Ngẫu Nhiên
            GUI.Label(new Rect(r.x, curY, r.width, 20), "4. HỆ NGUYÊN TỐ (7 THUỘC TÍNH):", headerStyle);

            Rect elemCardRect = new Rect(r.x, curY + 22, r.width, 48);
            Color elemCol = GetElementColor(currentElement);
            GUI.color = new Color(elemCol.r * 0.15f, elemCol.g * 0.15f, elemCol.b * 0.15f, 0.95f);
            GUI.DrawTexture(elemCardRect, Texture2D.whiteTexture);
            GUI.color = elemCol;
            GUI.DrawTexture(new Rect(elemCardRect.x, elemCardRect.y, elemCardRect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(elemCardRect.x, elemCardRect.y + elemCardRect.height - 2, elemCardRect.width, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var elemTitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold };
            elemTitleStyle.normal.textColor = elemCol;
            GUI.Label(elemCardRect, $"✨ HỆ {currentElement.ToUpper()} ({GetElementSubtitle(currentElement)})", elemTitleStyle);

            // 2. Điểm Thuộc Tính Khởi Đầu
            curY += 80f;
            GUI.Label(new Rect(r.x, curY, r.width, 20), "5. ĐIỂM THUỘC TÍNH KHỞI ĐẦU (TỔNG: 40):", headerStyle);

            curY += 24f;
            DrawStatRow(r.x, curY, r.width, "💪 Sức Mạnh (STR)", currentStr, new Color(1f, 0.4f, 0.35f));
            curY += 28f;
            DrawStatRow(r.x, curY, r.width, "🔮 Trí Tuệ (INT)", currentInt, new Color(0.35f, 0.75f, 1f));
            curY += 28f;
            DrawStatRow(r.x, curY, r.width, "❤️ Thể Chất (VIT)", currentVit, new Color(0.4f, 1f, 0.5f));
            curY += 28f;
            DrawStatRow(r.x, curY, r.width, "⚡ Nhanh Nhẹn (AGI)", currentAgi, new Color(1f, 0.9f, 0.3f));

            // 3. NÚT ĐỔ XÚC XẮC LỚN (Giới hạn 1 lần re-roll miễn phí duy nhất)
            curY += 45f;
            string rollBtnText;
            Color rollBtnColor;
            bool canRoll = freeRerollLeft > 0 && !isRolling;

            if (isRolling)
            {
                rollBtnText = "🎲 ĐANG ĐỔ XÚC XẮC...";
                rollBtnColor = new Color(0.4f, 0.9f, 1f);
            }
            else if (freeRerollLeft > 0)
            {
                rollBtnText = $"🎲 ĐỔ XÚC XẮC THUỘC TÍNH (Còn {freeRerollLeft} lượt miễn phí)";
                rollBtnColor = new Color(0.4f, 1f, 0.85f);
            }
            else
            {
                rollBtnText = "🔒 ĐÃ HẾT LƯỢT ĐỔ XÚC XẮC (0/1)";
                rollBtnColor = new Color(0.55f, 0.55f, 0.55f);
            }

            if (FantasyUITheme.DrawFantasyButton(new Rect(r.x, curY, r.width, 42), rollBtnText, 13, canRoll, rollBtnColor))
            {
                if (canRoll)
                {
                    freeRerollLeft--;
                    RollStats();
                }
            }

            // Ghi chú GDD DOC-03
            curY += 50f;
            var noteStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = true, fontStyle = FontStyle.Italic };
            noteStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f, 0.75f);
            GUI.Label(new Rect(r.x, curY, r.width, 40),
                "ℹ️ <i>GDD DOC-03: Sau khi vào game, người chơi có thể dùng Điểm Chuyển Sinh (RP) để tẩy luyện lại Hệ và Bộ Thuộc Tính hoàn mỹ nhất.</i>", noteStyle);
        }

        private void DrawStatRow(float x, float y, float w, string label, int value, Color barColor)
        {
            var lStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            lStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y, 130, 20), label, lStyle);

            float barX = x + 135;
            float barW = w - 180;
            float barH = 12;

            // Nền thanh
            GUI.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);
            GUI.DrawTexture(new Rect(barX, y + 4, barW, barH), Texture2D.whiteTexture);

            // Fill thanh (max scale 20)
            float fillW = barW * Mathf.Clamp01(value / 20f);
            GUI.color = barColor;
            GUI.DrawTexture(new Rect(barX, y + 4, fillW, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Số điểm
            var vStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight, fontSize = 12, fontStyle = FontStyle.Bold };
            vStyle.normal.textColor = barColor;
            GUI.Label(new Rect(x + w - 40, y, 40, 20), value.ToString(), vStyle);
        }

        private void SubmitCreation()
        {
            if (string.IsNullOrWhiteSpace(inputName) || inputName.Trim().Length < 2 || inputName.Trim().Length > 25)
            {
                statusMessage = "Tên nhân vật phải từ 2 đến 25 ký tự!";
                statusColor = new Color(1f, 0.4f, 0.4f);
                return;
            }

            var net = GameNetworkClient.Instance;
            if (net == null || !net.IsConnected)
            {
                statusMessage = "Chưa kết nối đến máy chủ!";
                statusColor = new Color(1f, 0.4f, 0.4f);
                return;
            }

            statusMessage = "Đang gửi thông tin tạo nhân vật lên máy chủ...";
            statusColor = new Color(0.4f, 0.85f, 1f);

            net.SendPacket(OpCodes.CharCreateReq, new CharCreateReqDto
            {
                name = inputName.Trim(),
                gender = isFemale ? "Nữ" : "Nam",
                className = selectedClass,
                element = currentElement,
                str = currentStr,
                intStat = currentInt,
                vit = currentVit,
                agi = currentAgi
            });
        }

        private Color GetElementColor(string el)
        {
            switch (el)
            {
                case "Hỏa": return new Color(1.0f, 0.45f, 0.15f);
                case "Băng": return new Color(0.35f, 0.85f, 1.0f);
                case "Lôi": return new Color(1.0f, 0.88f, 0.25f);
                case "Phong": return new Color(0.35f, 0.95f, 0.80f);
                case "Mộc": return new Color(0.40f, 0.95f, 0.25f);
                case "Quang": return new Color(1.0f, 0.98f, 0.65f);
                case "Ám": return new Color(0.75f, 0.35f, 1.0f);
                default: return Color.white;
            }
        }

        private string GetElementSubtitle(string el)
        {
            switch (el)
            {
                case "Hỏa": return "Hỏa Diệm Rực Cháy 🔥";
                case "Băng": return "Băng Hàn Vĩnh Cửu ❄️";
                case "Lôi": return "Lôi Điện Cuồng Nộ ⚡";
                case "Phong": return "Cuồng Phong Tự Do 🌪️";
                case "Mộc": return "Mầm Sống Sinh Mệnh 🌿";
                case "Quang": return "Thánh Quang Rực Rỡ ☀️";
                case "Ám": return "Hắc Ám Ma Mị 🌑";
                default: return "";
            }
        }
    }
}
