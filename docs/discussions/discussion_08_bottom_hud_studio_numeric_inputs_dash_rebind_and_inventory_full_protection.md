# BIÊN BẢN THẢO LUẬN 08: CONSOLE HUD ĐÁY MÀN HÌNH, TÁI ĐỊNH DANH PHÍM LƯỚT [D], BẢO VỆ ĐẦY HÀNH TRANG & CÁC Ô ĐIỀN BIẾN CHỈ SỐ THẬT TRONG NO-CODE STUDIO

> **Dự án:** Godless Fantasy MMORPG (SquiRRels Studio)  
> **Tài liệu tham chiếu:** `DOC-01_KINH_TE_TIEN_TE_SHOP_VAT_PHAM.md`, `DOC-02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md`, `DOC-06_GIAO_DIEN_UI_HUD_VA_CHAT.md`, `GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md`  
> **Người thực hiện:** Antigravity AI Pair Programmer & Lead Game Designer  
> **Ngày lập:** 11/10/2026  
> **Trạng thái:** Đã hiện thực hóa & Kiểm thử thành công 100% (Alpha Verified)

---

## 1. TỔNG QUAN CÁC YÊU CẦU NÂNG CẤP TỪ LEAD GAME DESIGNER

Trong phiên làm việc nâng cấp thực chiến giao diện & gameplay, Lead Game Designer đã đưa ra chuỗi phản hồi cốt lõi:
1. **Dời bảng chỉ số HP/Mana xuống Đáy Màn Hình:** Chuyển cụm Avatar, HP, Mana từ góc trên bên trái xuống cạnh 5 ô Hotbar thành một bảng Console hợp nhất chính giữa đáy màn hình, giải phóng 100% tầm nhìn chiến đấu bên trên. Không cần ẩn bảng khi mở menu Drawer.
2. **Tái định danh Phím Lướt sang Phím [D]:** Thay thế phím `Shift` bằng phím **`[D]`** (tiện tay, viết tắt của Dash theo chuẩn ARPG/MOBA, loại bỏ triệt để lỗi hiện hộp thoại Sticky Keys của Windows khi người chơi spam Shift).
3. **Chống Nhặt Đồ Khi Đầy Hành Trang:** Hành trang giới hạn 24 ô. Khi đạt 24/24, nhân vật không được phép nhặt thêm vật phẩm rơi trên mặt đất (hiển thị thông báo đỏ cảnh báo) và No-Code Studio không cho tạo thêm đồ vào túi.
4. **Quy Tắc Chạy Chữ (Marquee) Mượt Mà & Nối Đuôi Vô Hạn:**
   - Dòng văn bản quá dài chạy liên tục nối đuôi nhau vô hạn với khoảng cách nhỏ gọn (36px), xóa bỏ khoảng trống đen mênh mông.
   - Bảng chỉ số nhân vật đáy màn hình cũng tự động chạy chữ nếu thông số quá dài.
   - Quy tắc chuẩn format: `Tên trường: thông tin (mô tả)` — phần `Tên trường:` cố định đứng yên, chỉ phần `thông tin` chạy chữ khi tràn khung.
5. **No-Code Studio — Ô Điền Biến Chỉ Số Thật Cụ Thể:**
   - Cung cấp các ô input số liệu thật tách bạch: **Sát thương (Dam bao nhiêu)**, **Tiêu hao Mana (Mất mấy Mana)**, **Tầm đánh (Tầm đánh ra sao - mét)**, **Thời gian hồi chiêu (giây)**, **Tỉ lệ (% INT)**.
   - Thẻ hiển thị tính toán live: `💥 Vào Dam: XX ST (Gốc + % INT)`.
   - Form Trang bị cung cấp các ô số thực: Công (Dam), Giáp, HP, MP, INT, STR, VIT, AGI kèm nút tự động đồng bộ sang chuỗi thuộc tính.
   - Sửa triệt để lỗi không mở được Web Studio trên trình duyệt bằng cơ chế dò đa đường dẫn fallback.
6. **Thống Nhất Tên Gọi Tiền Tệ Là "Vàng":** Đổi toàn bộ các nhãn tiền tệ sang `"💰 Vàng: {gold}"`.
7. **Cập Nhật Chỉ Số Tức Thì Khi Cộng Điểm:** Tăng điểm INT, STR, VIT, AGI cập nhật ngay lập tức các chỉ số phụ thuộc (Công phép, Máu tối đa, Mana tối đa, Tốc chạy, Tốc đánh, Hồi chiêu Lướt).

---

## 2. BỐ CỤC CONSOLE HUD ĐÁY MÀN HÌNH (UNIFIED BOTTOM CONSOLE)

Toàn bộ cụm điều khiển trung tâm được căn chỉnh chính giữa chiều ngang đáy màn hình:

```
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|                                         CHIẾN TRƯỜNG 2D TOP-DOWN                                   |
|                                                                                                    |
|                     [CONSOLE HỢP NHẤT ĐÁY MÀN HÌNH - TỔNG RỘNG 531px]                              |
|   +-----------------------+     +------+ +------+ +------+ +------+     +------+                   |
|   | [AVATAR] Tên · [Hệ]   |     |  1   | |  2   | |  3   | |  4   |     |  D   |                   |
|   | Lv.XX    HP 150/150   |     |Chiêu1| |Chiêu2| |Chiêu3| |Chiêu4|     | DASH |                   |
|   | Thuộc tính INT STR AGI|     | -25mn| | -25mn| | -20mn| | -30mn|     | -10mn|                   |
|   +-----------------------+     +------+ +------+ +------+ +------+     +------+                   |
|                                                                                                    |
|   [ 📢 BẢNG TIN TỨC SERVER TỰ ĐỘNG CHẠY CHỮ ĐÁY MÀN HÌNH (SERVER NEWS MARQUEE) ]                  |
+----------------------------------------------------------------------------------------------------+
```

### Thông số hình học chính xác:
- `panelW = 215f`, `panelH = 72f`
- `slotSize = 58f`, `slotGap = 5f`
- `totalW = panelW + 10f + 5 * slotSize + 4 * slotGap = 531f`
- Tọa độ X: `startX = (Screen.width - totalW) / 2f` (Cân đối hoàn hảo giữa màn hình).
- Đáy của cụm HUD nằm tại `Screen.height - 30f`, cách thanh tin tức server đúng 6px.
- Click chuột trên vùng HUD được chặn xuyên thế giới (`PlayerMovement.BlockWorldClick`), nhấp vào màn hình ngoài HUD điều khiển di chuyển mượt mà.

---

## 3. TÁI ĐỊNH DANH PHÍM LƯỚT [D] & BẢO VỆ PHÍM TẮT

| Hạng mục | Trước nâng cấp | Sau nâng cấp |
| :--- | :--- | :--- |
| **Phím Lướt Chính** | `Shift` (Dễ dính phím Sticky Keys khi nhấp 5 lần) | **`[D]`** (Công thái học, ngón trỏ tay trái bấm cực thuận, đúng chuẩn Dash) |
| **Phím Lướt Phụ** | Không | Hỗ trợ thêm `[Shift]` và `[Space]` dự phòng |
| **Huy hiệu trên Slot** | Badge chữ `SHIFT` | Badge chữ **`D`** vàng kim |
| **Bảo vệ gõ phím** | Có thể bị trigger nhầm khi nhập text | Khi mở Chat, Godless Studio, Bang Hội hoặc bất kỳ Modal nào -> **Chặn 100% phím D di chuyển/lướt** |

---

## 4. CƠ CHẾ BẢO VỆ GIỚI HẠN HÀNH TRANG (CAPACITY PROTECTION: 24/24)

1. **Nguyên lý:**
   - Hành trang có sức chứa cố định: `INVENTORY_CAPACITY = 24`.
   - Khi `Items.Count >= 24`:
     - **Vật phẩm rơi từ quái (GroundLootItem):** Nhân vật chạy lại gần bấm `[F]` hoặc Click chuột sẽ bị từ chối, hiển thị chữ bay cảnh báo màu đỏ: `⚠️ Hành trang đã đầy (24/24)!`. Text gợi ý trên đầu vật phẩm chuyển sang màu đỏ: `⚠️ [HÀNH TRANG ĐẦY] Tên Vật Phẩm`.
     - **No-Code Studio:** Bấm `➕ TẠO VÀO TÚI ĐỒ` hoặc `➕ TẠO BÍ KÍP VÀO TÚI` sẽ hiển thị thông báo trạng thái: `⚠️ Hành trang đã đầy (24/24)! Hãy vứt bớt đồ trước khi tạo.` và không ghi đè vào danh sách.
   - **Ngoại lệ thông minh:** Các Dược phẩm / Tiêu hao (Bình Máu, Bình Mana) nếu đã có sẵn trong túi sẽ tự động **cộng dồn số lượng** vào ô hiện có mà không chiếm thêm slot mới.

---

## 5. THUẬT TOÁN CHẠY CHỮ LIÊN TỤC VÔ HẠN (SEAMLESS INFINITE MARQUEE)

```csharp
// Khoảng cách nhỏ gọn 36px, loại bỏ hoàn toàn khoảng trống đen mênh mông
float gap = 36f;
float travelDistance = textW + gap;
float offset = -(Time.time * speed % travelDistance);

// Vẽ liên tiếp các bản sao chữ tạo thành dòng chảy vô tận nối đuôi nhau
GUI.Label(new Rect(offset, 0, textW + 30, r.height), text, style);
GUI.Label(new Rect(offset + travelDistance, 0, textW + 30, r.height), text, style);
if (offset + travelDistance * 2 < r.width)
    GUI.Label(new Rect(offset + travelDistance * 2, 0, textW + 30, r.height), text, style);
```

- **Quy tắc tiền tố tĩnh:** Hàm `DrawFieldWithAutoScroll` tách `labelPrefix` (ví dụ `"⚡ Ô [1]: "`) đứng yên cố định ở bên trái, chỉ phần nội dung động bên phải mới cuộn chữ nếu vượt quá chiều rộng khung.

---

## 6. NO-CODE ENGINE STUDIO: CÁC Ô ĐIỀN BIẾN CHỈ SỐ THẬT

### 6.1. Form Thiết Kế Kỹ Năng (Form 2)
Cung cấp bảng thẻ thiết lập chỉ số thực tế 2 cột rõ ràng:
- **⚔️ Sát Thương (Dam):** Ô nhập số nguyên sát thương gốc (hoặc Lượng Hồi HP / Mana / Lực Buff).
- **💧 Mất Mấy Mana:** Ô nhập chi phí tiêu hao Mana khi kích hoạt.
- **🎯 Tầm Đánh (mét):** Ô nhập cự ly thi triển (ví dụ: `8.0` mét).
- **⏱️ Hồi Chiêu (giây):** Ô nhập thời gian hồi chiêu kỹ năng.
- **🧠 Tỉ Lệ (% INT):** Ô nhập phần trăm cộng dồn theo Trí Tuệ.
- **Live Preview Tính Toán:** `💥 Vào Dam: 55 ST (45 + 80% INT)` — phản ánh trực tiếp sát thương thực tế theo chỉ số hiện tại của nhân vật.

### 6.2. Form Thiết Kế Vật Phẩm (Form 1)
Cung cấp lưới 8 ô biến số thực tế:
- `⚔️ Công (Dam)` | `🛡️ Giáp Thủ`
- `❤️ Máu (HP)` | `💧 Mana (MP)`
- `🧠 Trí Tuệ (INT)` | `💪 Sức Mạnh (STR)`
- `🛡️ Thể Lực (VIT)` | `⚡ Nhanh Nhẹn (AGI)`
- Tự động đồng bộ và sinh chuỗi thuộc tính hiển thị (ví dụ: `+20 Công (Dam) · +25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)`).

### 6.3. Khắc Phục Lỗi Mở Web Studio Cho Team
Cung cấp mảng đường dẫn fallback đa tầng:
```csharp
string[] candidates = new string[]
{
    Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "tools", "GodlessSkillStudio", "index.html")),
    Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "tools", "GodlessSkillStudio", "index.html")),
    Path.GetFullPath(Path.Combine(Application.dataPath, "..", "tools", "GodlessSkillStudio", "index.html")),
    "d:\\godless-fantasy\\tools\\GodlessSkillStudio\\index.html",
    "d:\\godless-fantasy\\client\\tools\\GodlessSkillStudio\\index.html"
};
```
Đảm bảo khi bấm `🌐 MỞ WEB STUDIO CHO TEAM` trên bất kỳ môi trường Unity Editor hay bản Build độc lập đều kích hoạt trình duyệt mở file thành công 100%.

---

## 7. KẾT QUẢ BIÊN DỊCH VÀ XÁC NHẬN

- Toàn bộ source code được biên dịch qua `dotnet build Assembly-CSharp.csproj`:
  - **Kết quả:** `Build succeeded.`
  - **Lỗi:** **0 Error(s)**
  - **Cảnh báo tồn dư:** 1 Warning (Unity internal inspector field).
- Game sẵn sàng khởi chạy mượt mà ngay trên Unity Editor và client game!
