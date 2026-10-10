# ĐẶC TẢ HỆ THỐNG: GIAO DIỆN NGƯỜI DÙNG (UI/HUD) & HỆ THỐNG TRÒ CHUYỆN (CHAT)
**Mã tài liệu:** DOC-06  
**Dự án:** Godless Fantasy  
**Studio phát triển:** SquiRRels Studio  

---

## 1. TỔNG QUAN BỐ CỤC MÀN HÌNH CHƠI GAME (HUD LAYOUT)

Game hiển thị theo góc nhìn **2D Top-Down**, bố cục HUD được tối ưu để tối đa hóa tầm nhìn chiến đấu:

```
+---------------------------------------------------------------------------------------+
| [TOP-LEFT: THÔNG TIN NHÂN VẬT]                                [TOP-RIGHT: BONG BÓNG]  |
| +---------+  Tên Nhân Vật: ShadowKnight                        +--------------------+ |
| |  AVATAR |  HP   [=====================] 1850/1850            |  [💬 NÚT CHAT]     | |
| | THỰC TẾ |  MANA [=============        ]  420/600             +--------------------+ |
| +---------+  Kị Sĩ · Hỏa · Level 42                                                   |
|                                                                                       |
|                                                                                       |
| [MID-LEFT: MENU TRƯỢT]                                                                |
| +---+                                                                                 |
| | > | -> (Click mở Menu 4 Tab)                                                        |
| +---+                                                                                 |
|                                                                                       |
|                                                                                       |
|                                                                                       |
|                                                                                       |
|                                                                                       |
|                                                                                       |
|                                                                                       |
|                               [BOTTOM-CENTER: THANH KỸ NĂNG]                          |
|                               +----+  +----+  +----+  +----+    +------+              |
|                               | 1  |  | 2  |  | 3  |  | 4  |    | SHIFT|              |
|                               |Chiêu| |Chiêu| |Chiêu| |Khóa|    | DASH |              |
|                               |    |  |    |  |    |  (75) |    | 20 MN|              |
|                               +----+  +----+  +----+  +----+    +------+              |
+---------------------------------------------------------------------------------------+
```

---

## 2. CHI TIẾT CÁC PHÂN VÙNG HUD

### 2.1. Cụm Góc Trái Trên Cùng (Top-Left HUD)
1. **Avatar Thực Tế (Dynamic Real-time Portrait):**
   - Không dùng ảnh tĩnh vẽ sẵn.
   - Hiển thị mô hình/chân dung thực tế của nhân vật phản ánh đúng Trang phục, Mũ, Vũ khí và Thuộc tính đang kích hoạt.
2. **Thanh Máu & Năng Lượng:**
   - **Thanh HP (Đỏ):** Kèm label hiển thị số nguyên cụ thể (Ví dụ: `HP: 1850 / 1850`).
   - **Thanh MANA (Xanh Lam):** Kèm label hiển thị số cụ thể (Ví dụ: `MANA: 420 / 600`).
3. **Dòng Thông Tin Chức Nghiệp:**
   - Cấu trúc: `[Hệ Chiến Đấu] · [Thuộc Tính Ma Lực] · [Cấp Độ]`.
   - Ví dụ: `Pháp Sư · Ánh Sáng · Lv. 56` hoặc `Kẻ Ngoại Đạo · Bóng Tối · Lv. 12`.

---

### 2.2. Nút Menu Trượt & Cửa Sổ Bảng Điều Khiển (Mid-Left Menu)
- **Vị trí:** Chính giữa mép trái màn hình.
- **Biểu tượng:** Nút hình tam giác hướng sang phải `>`.
- **Hành vi:** Nhấn vào sẽ trượt ra một bảng Drawer mượt mà gồm **4 Tab chức năng**:

| Tab | Tên Tab | Nội dung hiển thị & Chức năng |
| :---: | :--- | :--- |
| **Tab 1** | **Trang Bị (Equipment)** | - Hiển thị 5 vị trí mặc đồ: **Vũ Khí, Mũ, Áo, Quần, Giày**.<br>- Xem chỉ số chi tiết của trang bị đang mang.<br>- Tháo/mặc đồ nhanh. |
| **Tab 2** | **Kỹ Năng & Thuộc Tính** | - Hiển thị 4 ô cuộn trục kỹ năng đã gán và kho cuộn kỹ năng đã học.<br>- Bảng tăng điểm 3 chỉ số chính: **HP, Mana, Attribute Power**.<br>- Bảng tra cứu 3 chỉ số phụ: **Tốc độ, Giáp, Tỉ lệ chí mạng**.<br>- Quản lý Danh Hiệu (Titles) và Perk đang sở hữu. |
| **Tab 3** | **Kho Đồ (Inventory)** | - Lưới túi đồ cá nhân (Mặc định 24 ô, hiển thị nút mở rộng thêm hàng).<br>- Đánh dấu đặc biệt **5 Ô Bảo Vệ Vĩnh Viễn** (có biểu tượng vương miện/hào quang khóa an toàn). |
| **Tab 4** | **Cài Đặt (Settings)** | - **Cấu hình Ngôn Ngữ (Language):** Mặc định **Tiếng Việt**, có thể chuyển sang **English (Tiếng Anh)**.<br>- Tùy chỉnh Âm thanh, BGM, SFX.<br>- Tùy chỉnh Đồ họa, hiển thị tên, thanh máu đồng đội.<br>- Phím tắt điều khiển.<br>- Nút Đăng Xuất (Logout) / Thoát Game. |

---

### 2.3. Cụm Thanh Kỹ Năng Trung Tâm Phía Dưới (Bottom-Center Skill Bar)
1. **Bốn Ô Kỹ Năng Chủ Động (Phím `1`, `2`, `3`, `4`):**
   - Ô 1: Mở mặc định từ Lv 1.
   - Ô 2: Mở khóa khi đạt Lv 25 (chưa đạt hiện ổ khóa mờ kèm chữ "Lv 25").
   - Ô 3: Mở khóa khi đạt Lv 50.
   - Ô 4: Mở khóa khi đạt Lv 75.
   - Hiển thị biểu tượng kỹ năng, số Mana tiêu hao ở góc dưới, và hiệu ứng đồng hồ xoay tròn đếm ngược Cooldown khi đang hồi chiêu.
2. **Ô Kỹ Năng Bẩm Sinh: DASH (Phím `Shift`):**
   - Nằm tách biệt sang bên phải của 4 ô kỹ năng.
   - Luôn sáng rõ, hiển thị tiêu hao 20 Mana và hồi chiêu 3.5 giây.

---

## 3. HỆ THỐNG TRÒ CHUYỆN (CHAT SYSTEM) VÀ CƠ CHẾ CHỐNG SPAM

### 3.1. Nút Chat & Giao Diện Pop-up
- **Vị trí:** Nằm ở góc trên cùng bên phải màn hình (Biểu tượng bong bóng chat `💬`).
- **Hành vi:** Nhấp chuột vào nút sẽ xổ ra cửa sổ Pop-up gồm **2 Tab**:
  1. **Tab "Vị Trí Hiện Tại" (Local / Map Chat):** **[MẶC ĐỊNH FOCUS KHI MỞ POP-UP]**.
  2. **Tab "Thế Giới" (World / Server Chat).**

```
+-------------------------------------------------------------+
| [TAB: VỊ TRÍ HIỆN TẠI (Mặc định)]   |   [TAB: THẾ GIỚI]     |
+-------------------------------------------------------------+
| [Bìa Rừng] DarkMage: Coi chừng skill lửa của tao nhé ae!    |
| [Bìa Rừng] Knight01: Mày chưởng trúng tao là tao chém đấy :D|
| [Bìa Rừng] ArcherX: Boss ra rồi anh em ơi tập trung!       |
|                                                             |
|                                                             |
+-------------------------------------------------------------+
| [Nhập tin nhắn...]                             [ GỬI ]      |
+-------------------------------------------------------------+
```

### 3.2. Quy Tắc Giới Hạn Tần Suất Gửi Tin (Chat Rate Limits & Rules)

Cơ chế được kiểm duyệt và thực thi **100% tại World Server** (Ngăn chặn bypass bằng công cụ thứ ba):

| Kênh Chat | Hạn mức số tin trong ngày | Thời gian chờ giữa 2 tin (Cooldown) | Mục đích thiết kế |
| :--- | :--- | :--- | :--- |
| **Vị Trí Hiện Tại (Local Map)** | **Không giới hạn** số tin | **5 giây / 1 tin** (Rate-limit) | Phục vụ giao tiếp chiến thuật thời gian thực, cảnh báo né chưởng đồng đội trong bán kính khu vực. |
| **Thế Giới (World / Server)** | **Tối đa 2 tin / 1 ngày** | **Tối thiểu 5 phút** giữa 2 tin | Chống hoàn toàn tình trạng spam rao vặt, chửi bới vô bổ; biến tin nhắn toàn server thành thông điệp đắt giá (rao boss, chiêu mộ bang chiến). |

---

## 4. CƠ CHẾ ĐIỀU KHIỂN CÔNG THÁI HỌC (ERGONOMIC CONTROLS & HOTKEYS)

Tránh mâu thuẫn thao tác "vừa bấm mũi tên vừa cầm chuột ngắm chiêu", game áp dụng giải pháp công thái học chuẩn MOBA/ARPG:

### 4.1. Phân Bổ 2 Tay Chuẩn Mực
- **Tay Phải (Chuột) [KHUYẾN NGHỊ MẶC ĐỊNH]:**
  - **Chuột Trái:** **LUÔN LUÔN DÀNH CHO DI CHUYỂN (CLICK-TO-MOVE)** — Click trên địa hình để nhân vật tự tìm đường di chuyển tới đó. Kể cả khi đang trong chế độ ngắm skill, Click Chuột Trái vẫn giúp nhân vật chạy né/kite quái bình thường mà không bị ngắt ngắm hay phóng chiêu nhầm!
  - **Rê Chuột:** Điều chỉnh hướng, góc bắn hoặc vùng kỹ năng (Indicator luôn bám theo chuột, ngay cả trong lúc nhân vật đang chạy).
  - **Chuột Phải:** **HỦY CHẾ ĐỘ NGẮM** (không mất Mana, không tính cooldown).
- **Tay Trái (Bàn Phím):**
  - `1`, `2`, `3`, `4`: **Bấm lần 1** để BẬT chế độ ngắm $\rightarrow$ Di chuyển né đòn / căn góc $\rightarrow$ **Bấm LẠI ĐÚNG PHÍM ĐÓ LẦN 2** để XÁC NHẬN PHÓNG CHIÊU! *(Chuột Trái tuyệt đối không dùng để phóng chiêu nhằm bảo toàn 100% khả năng di chuyển linh hoạt)*.
  - `Shift`: Dash (Lướt thẳng theo **hướng con trỏ chuột**, tiêu hao 20 Mana, cooldown 3.5s).
  - `W`, `A`, `S`, `D` (hoặc Arrow Keys): Di chuyển thủ công phụ (ghi đè Click-to-Move ngay khi bấm để né đòn tức thì).
  - `I`: Phím tắt mở nhanh Tab Kho Đồ.
  - `K`: Phím tắt mở nhanh Tab Kỹ Năng & Thuộc Tính.
  - `C`: Phím tắt mở nhanh Tab Trang Bị.
  - `Enter`: Mở nhanh khung gõ Chat.

---

## 5. HỆ THỐNG ĐA NGÔN NGỮ (LOCALIZATION - SONG NGỮ ANH / VIỆT)

Game hỗ trợ chính thức kiến trúc **Song Ngữ Toàn Diện (Bilingual Support)**:

```
+-------------------------------------------------------------------------+
|                  CẤU HÌNH NGÔN NGỮ (LANGUAGE SETTINGS)                  |
+-------------------------------------------------------------------------+
| [●] TIẾNG VIỆT (MẶC ĐỊNH - DEFAULT)                                     |
| [ ] ENGLISH (TIẾNG ANH - TÙY CHỌN TRONG CÀI ĐẶT)                        |
|                                                                         |
| -> Chuyển đổi tức thì trong Tab 4 (Settings), không cần khởi động lại!  |
+-------------------------------------------------------------------------+
```

### 5.1. Quy Chuẩn Bản Địa Hóa (Localization Rules)
1. **Ngôn ngữ mặc định:** **Tiếng Việt (`vi-VN`)** — Toàn bộ tân thủ khi cài đặt hoặc lần đầu vào game sẽ hiển thị giao diện tiếng Việt.
2. **Tùy chọn chuyển đổi:** **English (`en-US`)** — Người chơi có thể tự do chuyển sang tiếng Anh bất kỳ lúc nào trong **Tab 4 (Cài đặt / Settings)**.
3. **Cơ chế Hot-Reload (Chuyển đổi tức thì):**
   - Khi chọn ngôn ngữ mới trong Settings, hệ thống Client tự động phát sự kiện `OnLanguageChanged` để cập nhật lại toàn bộ Text trên UI, mô tả trang bị, tooltip kỹ năng ngay lập tức mà **không yêu cầu người chơi phải thoát game hay khởi động lại client**.

### 5.2. Bảng Đối Chiếu Thuật Ngữ Song Ngữ Chuẩn (Glossary Matrix)

| Thuật ngữ Tiếng Việt (Default) | English Term (In Settings) | Phân loại |
| :--- | :--- | :--- |
| **Kị Sĩ** | Knight | Hệ chiến đấu |
| **Xạ Thủ** | Archer | Hệ chiến đấu |
| **Pháp Sư** | Mage | Hệ chiến đấu |
| **Võ Sĩ** | Brawler | Hệ chiến đấu |
| **Hệ Ẩn / Kẻ Ngoại Đạo** | The Heretic | Chức nghiệp ẩn |
| **Kẻ Tội Lỗi** | The Sinner | Perk bẩm sinh hệ ẩn |
| **Chuyển Sinh** | Rebirth | Cơ chế tiến trình |
| **Rương Chuyển Sinh** | Rebirth Vault | Rương thế giới |
| **Điểm Chuyển Sinh** | Rebirth Points (RP) | Tiền tệ cao cấp |
| **Ô Bảo Vệ Vĩnh Viễn** | Permanent Protected Slot | Cơ chế an toàn |
| **Vùng An Toàn** | Safe Zone | Khu vực bản đồ |
| **Sát Thương Đồng Minh** | Friendly Fire | Cơ chế chiến đấu |
| **Lướt Nhanh** | Dash (Shift) | Kỹ năng cơ động |
| **Ánh Sáng / Bóng Tối / Phong / Mộc / Băng / Hỏa** | Light / Dark / Wind / Nature / Ice / Fire | 6 Thuộc tính ma lực |
| **Phẩm cấp: E $\rightarrow$ X** | Common $\rightarrow$ Godless | Phân cấp trang bị & cuộn trục |

### 5.3. Yêu Cầu Kỹ Thuật Phông Chữ (Typography / Unicode)
- Sử dụng bảng mã **UTF-8 tiêu chuẩn**.
- Toàn bộ Text Asset và Font Asset trong Unity (Text Mesh Pro) phải được nướng sẵn (bake SDF) đầy đủ bảng ký tự tiếng Việt có dấu (bao gồm cả hoa và thường: `ă, â, đ, ê, ô, ơ, ư...`) nhằm triệt tiêu hoàn toàn lỗi hiển thị ô vuông (missing glyph) khi hiển thị tiếng Việt.
