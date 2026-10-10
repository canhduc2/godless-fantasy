# ĐẶC TẢ HỆ THỐNG: COMBAT, PVP TỰ DO, FRIENDLY FIRE & TỔ ĐỘI
**Mã tài liệu:** DOC-02  
**Dự án:** Godless Fantasy  
**Studio phát triển:** Squirrels Studio  

---

## 1. TRIẾT LÝ CHIẾN ĐẤU & LUẬT THẾ GIỚI

### 1.1. PvP Là Tất Yếu & Quy Tắc Vùng An Toàn (Safe Zone)
- Trong Godless Fantasy, **không có hệ thống bật/tắt cờ PK (No PK Flag)**.
- Toàn bộ thế giới bên ngoài là chiến trường PvP mở (Open PvP). Bất kỳ ai cũng có thể tấn công bất kỳ ai.
- **Quy tắc Safe Zone:**
  - Được thiết lập tại các làng mạc khởi đầu, đô thành chính, hoặc trạm nghỉ chân có cắm cờ bảo hộ.
  - Khi nhân vật đứng trong ranh giới Safe Zone:
    - Vô hiệu hóa toàn bộ tương tác sát thương giữa Người chơi với Người chơi (Player vs Player).
    - Người chơi đứng ngoài Safe Zone không thể bắn tên/phóng phép vào người chơi đang ở trong Safe Zone.
    - Người chơi đứng trong Safe Zone không thể tung chiêu gây sát thương ra ngoài Safe Zone.
  - **Lưu ý:** Quái vật nếu bị dụ (pull) vào Safe Zone vẫn có thể tấn công người chơi nếu lính gác làng chưa kịp tiêu diệt chúng.

---

## 2. CƠ CHẾ SÁT THƯƠNG ĐỒNG ĐỘI (FRIENDLY FIRE)

Đây là cơ chế làm nên bản sắc chiến thuật đỉnh cao của Godless Fantasy: **Không có tấm khiên ảo ảnh giữa các đồng minh!**

```
                  [ PHÁP SƯ TUNG CẦU LỬA AOE ]
                               |
                               v
               +-------------------------------+
               |       VÙNG NỔ SÁT THƯƠNG       |
               |                               |
               |  [ QUÁI VẬT ]    [ ĐỒNG ĐỘI ] |
               |     -1.000 HP       -1.000 HP |
               +-------------------------------+
                               |
             Đồng đội chịu 100% sát thương thật
               và bị dính kèm hiệu ứng thiêu đốt!
```

### 2.1. Ma Trận Tác Động Sát Thương (Damage Interaction Matrix)

| Nguồn tấn công | Mục tiêu bị trúng đòn | Trong Safe Zone? | Kết quả tác động |
| :--- | :--- | :---: | :--- |
| **Người chơi** | Quái vật / Boss | Bất kỳ | Gây sát thương & hiệu ứng bình thường. |
| **Quái vật** | Người chơi | Bất kỳ | Gây sát thương bình thường. |
| **Người chơi A** | Người chơi B (Người lạ) | Ngoài Safe Zone | **Gây sát thương PvP thật** (Có thể hạ gục cướp đồ). |
| **Người chơi A** | Người chơi B (Trong Party/Alliance) | Ngoài Safe Zone | **GÂY SÁT THƯƠNG THẬT 100% (FRIENDLY FIRE)!** |
| **Người chơi A** | Người chơi B (Bất kỳ) | Trong Safe Zone | **Bị Server triệt tiêu hoàn toàn** (Damage = 0). |

### 2.2. Ý Nghĩa Chiến Thuật
- Kị sĩ đỡ đòn phía trước phải chú ý đường đạn của Xạ thủ phía sau để không che chắn cản tên hoặc ăn trúng tên của đồng đội.
- Pháp sư tung chiêu diện rộng (AoE) phải tính toán khoảng cách và hô hào vị trí để không quét sạch cả đội hình của mình.
- Đòi hỏi sự giao tiếp bằng giọng nói hoặc ping tín hiệu chuẩn xác thay vì cắm đầu xả skill bừa bãi.

---

## 3. HỆ THỐNG TỔ ĐỘI (PARTY) & LIÊN MINH (ALLIANCE)

### 3.1. Chia Sẻ Điểm Kinh Nghiệm (Party EXP Sharing)
- Người chơi có thể lập Party tối đa **4 thành viên** (hoặc Alliance quy mô lớn hơn cho bang hội).
- **Quy tắc chia EXP:**
  - Thành viên trong Party chỉ được nhận chia sẻ EXP khi đứng trong **Bán kính chia sẻ (Shared Radius: 35 mét)** quanh quái vật bị hạ gục.
  - Thành viên đang chết (Dead state) hoặc đang đứng trong Safe Zone không được chia EXP.
  - Công thức chia EXP: Có thưởng % kinh nghiệm nhóm để khuyến khích đi chung:
    $$\text{EXP mỗi người} = \frac{\text{EXP Quái} \times (1 + 0.1 \times (\text{Số người} - 1))}{\text{Số người}}$$

### 3.2. Cơ Chế Nhặt Đồ Tự Do (Free-For-All Loot)
- Đồ rơi từ quái, boss hay người chơi chết **hoàn toàn tự do (Free Loot)**.
- **Ai chạy đến nhặt trước thì người đó sở hữu!**
- Không có hệ thống tự động chia đồ vào hòm đồ từng người, không có cơ chế tung xúc xắc (Roll) bắt buộc. Điều này tạo nên sự cạnh tranh thực tế, tin tưởng nội bộ hoặc những pha tranh cướp kịch tính ngay giữa các thành viên.

---

## 4. CƠ CHẾ ĐIỀU KHIỂN & SỬ DỤNG KỸ NĂNG (TARGETING UX & INPUT)

### 4.1. Phong Cách Định Hướng MOBA (Skillshot Targeting)
- Thao tác:
  1. Người chơi nhấn giữ phím kỹ năng (`1`, `2`, `3`, `4`).
  2. Xuất hiện **Vùng chỉ thị kỹ năng (Indicator)**: Vòng tròn bán kính nổ, mũi tên định hướng, hoặc hình nón (cone).
  3. Di chuyển chuột tới vị trí mục tiêu mong muốn.
  4. **Thả phím (Key Release):** Xác nhận phóng kỹ năng tới tọa độ con trỏ chuột.

### 4.2. Cơ Chế Hủy Chiêu Bằng Chuột Phải & Máy Trạng Thái Chống Lặp Lệnh (Input State Machine)

> **Vấn đề cốt lõi cần giải quyết:** Khi đang giữ phím số `1` để ngắm chiêu, người chơi đổi ý muốn hủy nên click Chuột Phải. Theo thói quen ngón tay, phím `1` lúc đó vẫn chưa kịp thả ra. Nếu code đọc input theo dạng `GetKeyDown` hoặc polling phím đang giữ, hệ thống sẽ nhầm là người chơi vừa muốn hủy rồi lại ngay lập tức ngắm chiêu mới!

Để giải quyết triệt để, hệ thống sử dụng Finite State Machine (FSM) gồm 4 trạng thái:

```
    [ IDLE ]  ---(Nhấn giữ Phím 1/2/3/4)---> [ AIMING ]
       ^                                         |
       |                               (Click Chuột Phải để Hủy)
       |                                         |
       |                                         v
       +---(Nhận sự kiện THẢ PHÍM)--- [ CANCELLED_AWAIT_RELEASE ]
                                      (Khóa ngắm, phớt lờ trạng thái giữ!)
```

#### Bảng Mô Tả Trạng Thái Kỹ Thuật (Input Logic Table)

| Trạng thái | Điều kiện kích hoạt | Hành vi hệ thống | Chuyển trạng thái tiếp theo |
| :--- | :--- | :--- | :--- |
| **IDLE** | Đang đứng chờ | Chưa có indicator, nhận phím bấm bình thường. | Nhấn phím kỹ năng $\rightarrow$ **AIMING** |
| **AIMING** | Phím kỹ năng đang được giữ | Hiển thị indicator theo vị trí con trỏ chuột. | - Thả phím kỹ năng $\rightarrow$ **CAST_SUCCESS**<br>- Click Chuột Phải $\rightarrow$ **CANCELLED_AWAIT_RELEASE** |
| **CANCELLED_AWAIT_RELEASE** | Chuột phải vừa click khi phím kỹ năng chưa nhả | **Tắt ngay indicator**. Đặt cờ khóa `IsSkillLocked = true`. Hoàn toàn bỏ qua việc phím skill vẫn đang bị đè! | Nhận sự kiện phím skill **ĐƯỢC NHẢ RA HOÀN TOÀN (Key Up)** $\rightarrow$ Mở khóa, trở về **IDLE**. |
| **CAST_SUCCESS** | Phím skill được thả tại tọa độ hợp lệ | Gửi gói tin lên Server: $\{SkillId, TargetVector, Timestamp\}$. Thực hiện hoạt ảnh thi triển. | Trở về **IDLE** sau khi hoạt ảnh kết thúc. |

---

## 5. KỸ NĂNG CƠ ĐỘNG MẶC ĐỊNH: DASH (LƯỚT NHANH)

- **Phím kích hoạt:** Phím `Shift` (hoặc hướng di chuyển kết hợp).
- **Đặc tính:**
  - Là kỹ năng bẩm sinh có sẵn ngay từ cấp 1 của mọi hệ, **không chiếm bất kỳ ô nào trong 4 ô kỹ năng**.
  - Lướt nhanh một khoảng cách cố định (khoảng 4.5 mét) theo hướng di chuyển hiện tại của nhân vật.
  - **Tiêu hao:** Tốn **20 Mana** mỗi lần lướt.
  - **Thời gian hồi (Cooldown):** 3.5 giây.
  - Cho phép người chơi né nhanh các đường đạn nguy hiểm, lướt qua chưởng của đồng đội hoặc trốn chạy khỏi kẻ địch.
  - Trong thời gian lướt không bị khựng di chuyển, nhưng vẫn nhận sát thương nếu va chạm với vùng nổ diện rộng đang tồn tại.
