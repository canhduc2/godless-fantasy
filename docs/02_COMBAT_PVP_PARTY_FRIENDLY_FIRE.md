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

## 4. CƠ CHẾ ĐIỀU KHIỂN CÔNG THÁI HỌC (ERGONOMIC CONTROL & TARGETING UX)

### 4.1. Giải Pháp Công Thái Học: 2 Tay Chuẩn Mực (Keyboard Left - Mouse Right)
Tránh xung đột thao tác "thiếu tay cầm chuột", game tối ưu trải nghiệm chiến đấu MOBA/ARPG chuẩn mực:
- **Tay Phải (Chuột):** 
  - **Click Chuột để di chuyển (Click-to-Move):** Click vào bản đồ để nhân vật tự tìm đường chạy tới (Phong cách LMHT / Dota / Diablo).
  - **Rê chuột:** Định hướng vùng chiêu thức (Skill Indicator).
  - **Click Chuột Phải:** Hủy ngay chế độ ngắm chiêu.
- **Tay Trái (Bàn Phím):** 
  - Đặt sẵn trên cụm phím kỹ năng `1`, `2`, `3`, `4` và ngón út trên `Shift` (Dash).
  - (Cụm phím `W`, `A`, `S`, `D` hoặc Arrow Keys vẫn hoạt động song song như phương án di chuyển hỗ trợ phụ cho người thích bấm phím).

---

### 4.2. Cơ Chế Dùng Chiêu Nhấp Kép (Press-to-Aim $\rightarrow$ Press-Again-to-Cast)
Thay vì bắt người chơi phải đè giữ nút kỹ năng gây mỏi ngón tay và dễ kích hoạt nhầm, game áp dụng cơ chế **Nhấp Lần 1 để Ngắm $\rightarrow$ Nhấp Lần 2 để Phóng Chiêu**:

```
+-----------------------------------------------------------------------------------------+
|                  LUỒNG THAO TÁC KỸ NĂNG NHẤP KÉP (TWO-TAP CASTING)                      |
+-----------------------------------------------------------------------------------------+
|                                                                                         |
|       [ BẤM PHÍM SKILL LẦN 1 ]  (Ví dụ: Bấm phím '1')                                   |
|                  |                                                                      |
|                  v                                                                      |
|       [ VÀO CHẾ ĐỘ NGẮM CHIÊU (AIMING MODE) ]                                           |
|       - Xuất hiện Vùng chỉ thị (Indicator) xoay và liệng theo hướng con trỏ chuột.     |
|       - Người chơi KHÔNG CẦN GIỮ PHÍM, tay hoàn toàn thả lỏng thoải mái!                |
|                  |                                                                      |
|         +--------+------------------------------------+                                 |
|         |                                             |                                 |
|         v (Muốn Phóng Chiêu)                          v (Đổi ý muốn Hủy Chiêu)          |
|  [ BẤM TIẾP PHÍM '1' LẦN 2 ]                  [ CLICK CHUỘT PHẢI ]                      |
|         |                                             |                                 |
|         v                                             v                                 |
|  [ KÍCH HOẠT PHÓNG CHIÊU THÀNH CÔNG ]         [ HỦY NGẮM NGAY LẬP TỨC ]                 |
|  - Nhân vật xả kỹ năng theo hướng chuột       - Tắt Indicator, không mất Mana,          |
|  - Trở về trạng thái sẵn sàng bình thường.     - Trở về trạng thái sẵn sàng bình thường. |
+-----------------------------------------------------------------------------------------+
```

### 4.3. Bảng Máy Trạng Thái Điều Khiển (Input FSM Table)

| Trạng thái | Điều kiện kích hoạt | Hành vi hệ thống | Chuyển trạng thái tiếp theo |
| :--- | :--- | :--- | :--- |
| **IDLE** | Đang đứng chờ / di chuyển | Chưa có indicator, nhận lệnh Click-to-Move hoặc phím bình thường. | Bấm phím `1`, `2`, `3`, hoặc `4` $\rightarrow$ **AIMING(Skill_X)** |
| **AIMING(X)** | Vừa bấm phím Skill X lần 1 | Hiển thị indicator của Skill X xoay theo tọa độ con trỏ chuột. | - Bấm lại đúng phím **Skill X lần 2** $\rightarrow$ **CAST_SUCCESS**<br>- Click **Chuột Phải** $\rightarrow$ **CANCEL_SUCCESS**<br>- Bấm sang phím **Skill Y khác** $\rightarrow$ Chuyển sang **AIMING(Skill_Y)** |
| **CANCEL_SUCCESS** | Vừa Click Chuột Phải khi đang ngắm | Tắt ngay indicator, xóa bộ nhớ ngắm. Không tiêu hao Mana, không tính cooldown. | Trở về ngay **IDLE** |
| **CAST_SUCCESS** | Bấm đúng phím Skill X lần 2 | Server xác nhận vị trí con trỏ chuột $\rightarrow$ Trừ Mana $\rightarrow$ Bắt đầu tính Cooldown $\rightarrow$ Phóng chiêu. | Trở về **IDLE** sau khi hoạt ảnh thi triển hoàn tất. |

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
