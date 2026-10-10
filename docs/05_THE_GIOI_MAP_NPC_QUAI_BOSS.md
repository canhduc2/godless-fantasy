# ĐẶC TẢ HỆ THỐNG: THẾ GIỚI MỞ, NPC, QUÁI VẬT, BOSS & BÍ MẬT CHUYỂN CHỨC HỆ ẨN
**Mã tài liệu:** DOC-05  
**Dự án:** Godless Fantasy  
**Studio phát triển:** Squirrels Studio  

---

## 1. KIẾN TRÚC THẾ GIỚI MỞ CỰC LỚN (WORLD -> REGION -> CHUNK)

Để hiện thực hóa yêu cầu "các vùng đất phải cực kỳ rộng lớn" mà vẫn chạy mượt mà trên nền tảng 2D Top-down MMORPG, kiến trúc game được phân tầng làm 3 cấp độ:

```
+-------------------------------------------------------------------------+
|                  TẦNG 1: WORLD MAP (BẢN ĐỒ THẾ GIỚI TỔNG THỂ)           |
|  Bao gồm các Lục Địa Lớn, Biển Sâu, Vương Quốc Cổ, Vùng Hoang Dã        |
+------------------------------------+------------------------------------+
                                     |
                                     v
+-------------------------------------------------------------------------+
|                  TẦNG 2: REGION (VÙNG ĐẤT / KHU VỰC ĐỊA LÝ)             |
|  - Rộng từ 500m x 500m đến 2000m x 2000m.                               |
|  - Có Biome riêng (Rừng Nguyền Rủa, Đầm Lầy Hắc Ám, Núi Lửa Tro Tàn...).|
|  - Chứa Safe Zone, Quái theo cấp độ, Hang động (Dungeon) & World Boss.  |
+------------------------------------+------------------------------------+
                                     |
                                     v
+-------------------------------------------------------------------------+
|                  TẦNG 3: CHUNK (ĐƠN VỊ TẢI DỮ LIỆU ĐỘNG)                |
|  - Kích thước: 32 x 32 ô Tile (khoảng 32m x 32m).                       |
|  - Client: Streaming tải/gỡ Tilemap động bằng Addressables quanh người. |
|  - Server: Mô phỏng AI quái, kiểm soát tầm nhìn (AOI - Area of Interest)|
+-------------------------------------------------------------------------+
```

---

## 2. HỆ THỐNG QUÁI VẬT & BOSS THẾ GIỚI

### 2.1. Phân Loại Quái Vật
1. **Quái Thường (Minions):**
   - Phân bố rải rác theo bãi quái quanh các Region.
   - Cung cấp EXP ổn định, rớt Vàng, rác quái và mảnh cuộn kỹ năng.
2. **Quái Tinh Anh (Elite Mobs):**
   - Kích thước lớn hơn 20%, có vòng hào quang dưới chân.
   - Sở hữu 1 kỹ năng đặc biệt (Lướt áp sát hoặc ném lưới trói). Tỉ lệ rơi đồ phẩm cấp C–B.
3. **Boss Khu Vực / Boss Thế Giới (World Bosses):**
   - Xuất hiện tại các điểm tập kết cố định hoặc đền thờ cổ sau mỗi chu kỳ hồi sinh (Respawn Timer: 4 - 8 tiếng).
   - Lượng máu cực khủng, sở hữu các chiêu thức diện rộng AoE hủy diệt.

### 2.2. Bảng Rơi Đồ Boss & Kịch Bản Friendly Fire Đẫm Máu
- **Rơi Cuộn Trục Kỹ Năng Ngẫu Nhiên (Random Skill Scrolls):**
  - Khi Boss bị hạ gục, rơi ra từ 1 - 3 Cuộn Trục Kỹ Năng ngẫu nhiên phẩm cấp từ **A đến X**.
  - Rơi các trang bị quý hiếm và Ma Thạch.
- **Kịch bản giao tranh thực tế:**
  - Boss tung chiêu AoE diện rộng $\rightarrow$ Các thành viên trong Party phải né tránh.
  - Khi đánh Boss, do có cơ chế **Friendly Fire**, các đòn phép diện rộng của Pháp sư hoặc mũi tên của Xạ thủ nếu bắn bừa bãi sẽ vô tình tiêu diệt chính đồng đội của mình!
  - Khi Boss vừa ngã xuống, đồ rớt tự do ra đất $\rightarrow$ Mọi người (kể cả đồng đội lẫn kẻ thù rình rập ngoài bụi) lập tức lao vào tranh cướp nhặt đồ, tạo nên những màn PvP hỗn chiến đỉnh điểm!

---

## 3. CƠ CHẾ KÍCH HOẠT CHUYỂN CHỨC BÍ MẬT: HỆ ẨN (HERETIC CLASS UNLOCK)

Chuyển chức sang Hệ Ẩn là bí mật lớn nhất thế giới Godless Fantasy, được thiết kế theo nguyên tắc: **Hiếm có khó tìm - Cơ chế thời gian ngặt nghèo - Thử thách sống còn.**

```
                                [ ĐIỀU KIỆN TIÊN QUYẾT ]
                   Đạt Level 100 + Không đang mang trọng tội tử hình
                                           |
                                           v
+-------------------------------------------------------------------------------------+
|                  BƯỚC 1: XUẤT HIỆN CỬA SỔ THỜI GIAN HIẾM (TIME WINDOW)              |
|  - Tại Vùng Map bí ẩn: [Thánh Địa Bị Lãng Quên - Forgotten Sanctum].               |
|  - Cổng dịch chuyển vào đền chỉ mở trong 15 PHÚT duy nhất vào thời khắc Nguyệt Thực|
|    (Ví dụ: Xuất hiện ngẫu nhiên vào khung giờ bí mật trong tuần theo giờ Server).  |
+-------------------------------------------------------------------------------------+
                                           |
                                           v
+-------------------------------------------------------------------------------------+
|                  BƯỚC 2: VƯỢT QUA MÊ CUNG CẤM CHÚ & QUÁI CANH CỔ ĐẠI                |
|  - Bản đồ là vùng PvP Tự Do khốc liệt nhất.                                         |
|  - Người chơi phải giải một cơ quan ma trận nguyên tố và đánh bại Hộ Vệ Cấm Pháp.   |
+-------------------------------------------------------------------------------------+
                                           |
                                           v
+-------------------------------------------------------------------------------------+
|                  BƯỚC 3: TƯƠNG TÁC VỚI [BIA ĐÁ NGOẠI ĐẠO] VÀ CHUYỂN CHỨC           |
|  - Khi chấp nhận hiến tế:                                                           |
|      * Nhân vật thực hiện Nghi Thức Chuyển Chức (Biến đổi chức nghiệp sang HỆ ẨN).  |
|      * Server gacha ngẫu nhiên 1 trong 3 hệ còn lại (Kị Sĩ / Xạ Thủ / Võ Sĩ) làm   |
|        Hệ Phụ kết hợp cùng Pháp Sư.                                                 |
|      * Mở khóa vĩnh viễn 2 Perk: [KẺ NGOẠI ĐẠO] & [KẺ TỘI LỖI].                    |
|      * Reset Cấp độ về Lv 1, reset toàn bộ điểm thuộc tính, xóa sạch hành trang     |
|        (y hệt như Chuyển Sinh Rebirth).                                             |
|      * ĐẶC BIỆT: KHÔNG NHẬN ĐƯỢC ĐIỂM REBIRTH POINT (Thứ nhận lại là Sức Mạnh Ẩn!) |
|      * 5 Ô ĐƯỢC BẢO VỆ VĨNH VIỄN (nếu đã có từ trước) VẪN NGUYÊN VẸN KHÔNG MẤT!    |
+-------------------------------------------------------------------------------------+
                                           |
                                           v
+-------------------------------------------------------------------------------------+
|                  BƯỚC 4: TIẾN TRÌNH & VẪN REBIRTH BÌNH THƯỜNG SAU ĐÓ                |
|  - Nhân vật bắt đầu hành trình mới từ Level 1 với tư cách là HỆ ẨN (HERETIC).       |
|  - Cày cuốc thăng cấp từ Lv 1 -> 100 như bình thường.                               |
|  - KHI ĐẠT LEVEL 100: NHÂN VẬT VẪN THỰC HIỆN REBIRTH NHƯ BÌNH THƯỜNG!               |
|      * Nhận điểm Rebirth Point (RP) bình thường (dùng quay Perk, đổi thuộc tính...). |
|      * Được chọn 2 món cất vào Rebirth Vault lưu server bình thường.                |
|      * Giữ nguyên chức nghiệp HỆ ẨN và 2 Perk bẩm sinh [Kẻ Ngoại Đạo], [Kẻ Tội Lỗi] |
|        (không bị mất Hệ Ẩn khi Rebirth).                                            |
|      * 5 Ô bảo vệ vĩnh viễn tiếp tục được giữ nguyên.                               |
+-------------------------------------------------------------------------------------+
```

### 3.1. Quy Tắc Gặp Lại Thánh Địa Lần Nữa (Repeat Encounter Rule)
Nếu một người chơi **đã mang chức nghiệp Hệ Ẩn** mà trong tương lai (sau khi Rebirth hoặc đi khám phá thế giới) may mắn bắt gặp lại cửa sổ thời gian hiếm và mở được Cánh Cổng Thánh Địa Bị Lãng Quên lần thứ hai (hoặc các lần tiếp theo):

> **1. KHÔNG ĐƯỢC CHUYỂN CHỨC NỮA:**  
> Hệ thống sẽ từ chối nghi thức chuyển chức mới. Người chơi không thể đổi lại hệ ban đầu, không được re-roll lại hệ phụ thứ hai. Chức nghiệp Hệ Ẩn là bất biến.
> 
> **2. PHẦN THƯỞNG: CỘNG +1 REBIRTH POINT & NÂNG CẤP BẬC DANH HIỆU (PROGRESSIVE TITLES)**  
> Mỗi lần vượt qua thử thách Thánh Địa thành công từ lần 2 trở đi, người chơi sẽ nhận **+1 Điểm Rebirth Point (1 RP)** và danh hiệu được **tiến hóa theo từng bậc (Lần 2 -> 3 -> 4 -> 5)**.
> 
> *Nguyên tắc cân bằng chỉ số (Tránh buff quá bẩn / Anti-Power Creep):*  
> - Người chơi chỉ được kích hoạt (đeo) **1 Danh Hiệu duy nhất** tại một thời điểm (Bậc mới thay thế bậc cũ, không cộng dồn chồng chéo).  
> - Các chỉ số tăng có mức độ vừa phải, hỗ trợ bù trừ nhẹ cho điểm yếu của Hệ Ẩn chứ không tạo ra chênh lệch một chiều phá vỡ cân bằng PvP.  
> - **Chạm trần ở Lần 5 (Max Cap):** Từ lần thứ 6 trở đi vẫn nhận +1 RP nhưng danh hiệu không tăng thêm chỉ số để giữ vững môi trường thi đấu lành mạnh.

#### Bảng Tiến Hóa Danh Hiệu Tái Chinh Phục Thánh Địa:

| Lần chinh phục | Danh Hiệu Tiến Hóa (Title) | Chỉ Số Thưởng Kèm (Cân Bằng) | Thưởng Điểm RP |
| :---: | :--- | :--- | :---: |
| **Lần 1** | *Nghi thức Chuyển Chức: Hệ Ẩn* | Nhận 2 Perk: Kẻ Ngoại Đạo & Kẻ Tội Lỗi (-30% Max Mana) | 0 RP |
| **Lần 2** | `[Chân Ngoại Đạo I]` *(True Heretic I)* | +30 Sức mạnh thuộc tính, +3% Tốc độ chạy, +30 Mana tối đa | **+1 RP** |
| **Lần 3** | `[Kẻ Nghịch Mệnh II]` *(Defier of Fate II)* | +45 Sức mạnh thuộc tính, +4% Tốc độ chạy, +50 Mana tối đa, +10 Giáp | **+1 RP** |
| **Lần 4** | `[Kẻ Vượt Cấm Địa III]` *(Sanctum Breaker III)* | +60 Sức mạnh thuộc tính, +5% Tốc độ chạy, +70 Mana tối đa, +20 Giáp | **+1 RP** |
| **Lần 5 (Trần Tối Đa)** | `[Chúa Tể Ngoại Đạo IV]` *(Sovereign Heretic IV)* | +75 Sức mạnh thuộc tính, +6% Tốc độ chạy, +90 Mana tối đa, +30 Giáp, +2% Chí mạng | **+1 RP** |
| **Lần 6 trở đi** | *Giữ nguyên danh hiệu Bậc 5* | Giữ nguyên chỉ số Bậc 5 (Khóa trần chỉ số chống lạm phát) | **+1 RP / lần** |

---

## 4. TỔNG QUAN HỆ THỐNG NPC THẾ GIỚI (MASTER NPC LIST)

```
[ THÀNH PHỐ / LÀNG AN TOÀN (SAFE ZONE) ]
|-- NPC Trưởng Làng: Giao nhiệm vụ cốt truyện sơ khởi
|-- NPC Dược Sĩ: Bán bình máu, mana, thuốc giải độc
|-- NPC Thợ Rèn: Rèn, sửa trang bị, phân tách quặng
|-- NPC Quản Kho: Cất giữ đồ đạc an toàn tại làng
|-- NPC Đấu Giá Sư: Sàn giao dịch giữa người chơi
|-- NPC Sứ Giả Chuyển Sinh (Rebirth Emissary): Hướng dẫn Rebirth tại mốc Lv 100

[ NGOÀI VÙNG HOANG DÃ (WILDERNESS / PVP DANGER ZONE) ]
|-- NPC Thương Nhân Lang Thang: Bán đồ hiếm theo giờ ngẫu nhiên
|-- NPC Ẩn Sĩ Tàn Phế: Gợi ý các manh mối về Thánh Địa Hệ Ẩn
|-- NPC Bia Đá Ngoại Đạo (Khu vực bí mật): Nơi thực hiện nghi thức chuyển chức Hệ Ẩn
```
