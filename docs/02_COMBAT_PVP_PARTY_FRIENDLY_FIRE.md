# ĐẶC TẢ HỆ THỐNG: COMBAT, PVP TỰ DO, FRIENDLY FIRE & TỔ ĐỘI
**Mã tài liệu:** DOC-02  
**Dự án:** Godless Fantasy  
**Studio phát triển:** SquiRRels Studio  

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

### 4.3. Ba Quy Tắc Thiết Kế Tránh Xung Đột Input (Input Conflict Resolution)

Để code và trải nghiệm mượt mà không bị lỗi thao tác, hệ thống khóa chặt 3 quy tắc sau:

1. **Cơ Chế Kiting Song Hành (Run & Aim Concurrently — Chuột Trái Luôn Di Chuyển):**
   - **Chuột Trái KHÔNG DÙNG ĐỂ BẮN**, mà **LUÔN DÀNH CHO DI CHUYỂN (Click-to-Move)** kể cả khi đang ở chế độ ngắm!
   - Khi đang ngắm (`AIMING`): Người chơi click chuột trái xuống đất $\rightarrow$ Nhân vật vẫn chạy né tới điểm đó.
   - **Tâm ngắm (Indicator) vẫn liên tục bám theo con trỏ chuột** theo thời gian thực (vector tính từ vị trí nhân vật đang chạy tới vị trí chuột).
   - **Xác nhận phóng chiêu DUY NHẤT:** Người chơi bấm **ĐÚNG PHÍM SKILL ĐÓ LẦN THỨ 2**!
   - **Xác thực Server tại thời điểm bấm phím lần 2 (Server-Authoritative Timing):** Ngay khi nhận tín hiệu bấm lần 2, Server kiểm tra tọa độ nhân vật tại thời điểm đó, tính toán tầm đánh (Attack Range), kiểm tra Mana và Cooldown. Đủ điều kiện $\rightarrow$ Xuất chiêu ngay từ vị trí hiện tại của nhân vật.
2. **Quy Tắc Ưu Tiên Di Chuyển (WASD Overrides Click-to-Move):**
   - Click-to-Move và WASD hoạt động song song.
   - Nếu nhân vật đang tự động chạy theo Click-to-Move mà người chơi nhấn bất kỳ phím `W`, `A`, `S`, `D` nào $\rightarrow$ **Lệnh WASD ghi đè lập tức 100%**, hủy đường chạy tự động để người chơi né đòn tức thì với độ trễ 0ms!
3. **Phân Biệt Rõ 3 Nhóm Kỹ Năng (Skill Targeting Archetypes):**
   - **Kỹ năng Định Hướng (Directional Skillshots):** Kiếm khí, mũi tên, cầu phép $\rightarrow$ Hiển thị dải mũi tên từ nhân vật hướng về vector con trỏ chuột.
   - **Kỹ năng Chọn Tọa Độ Mặt Đất (Ground-Targeted AoE):** Mưa thiên thạch, vòng cấm chú, bẫy $\rightarrow$ Hiển thị vòng tròn tâm tại con trỏ chuột; nếu trỏ chuột vượt quá tầm tối đa (Max Range), tâm AoE sẽ bị ghim chặt tại ranh giới vòng tròn Max Range.
   - **Kỹ năng Tự Thân / Mục Tiêu Đơn (Self / Single-Target):** Khiên chắn, hồi phục, chém áp sát $\rightarrow$ Hiển thị vòng hào quang quanh bản thân hoặc viền khóa mục tiêu hợp lệ.

---

### 4.4. Hệ Thống Phản Hồi Thị Giác Cảnh Báo Friendly Fire (Visual Feedback)
- **Vòng Phạm Vi Tối Đa (Max Range Border):** Luôn hiển thị bán kính tác dụng lớn nhất của kỹ năng.
- **Màu Sắc Chỉ Thị Trực Quan:**
  - **Màu Xanh Lam / Xanh Lục (Hợp lệ):** Đủ Mana, trong tầm tác dụng, sẵn sàng phóng chiêu.
  - **Màu Đỏ Rực (Cảnh báo):** Không đủ Mana, đang hồi chiêu, hoặc góc ngắm bị vật cản che khuất.
  - **Cảnh Báo Friendly Fire:** Khi vòng AoE hoặc dải đường đạn giao thoa với vùng đứng của **Đồng Đội trong Party**, vòng tròn dưới chân đồng đội sẽ nhấp nháy viền đỏ cảnh báo nguy cơ ăn đòn oan!

---

### 4.5. Bảng Máy Trạng Thái Điều Khiển Hoàn Chỉnh (Input FSM Table)

| Trạng thái | Điều kiện kích hoạt | Hành vi hệ thống | Chuyển trạng thái tiếp theo |
| :--- | :--- | :--- | :--- |
| **IDLE** | Đang đứng chờ / di chuyển | Nhận Click Chuột Trái (di chuyển tìm đường), nhận WASD (chạy thủ công). | Bấm phím `1`, `2`, `3`, hoặc `4` $\rightarrow$ **AIMING(Skill_X)** |
| **AIMING(X)** | Bấm phím Skill X lần 1 | Hiện Indicator xoay theo chuột. Click Chuột Trái **vẫn di chuyển né đòn bình thường**, tâm ngắm tiếp tục bám theo chuột. | - Bấm lại **Đúng Phím Skill X lần 2** $\rightarrow$ **CAST_SUCCESS**<br>- Click **Chuột Phải** $\rightarrow$ **CANCEL_SUCCESS**<br>- Bấm **Phím Y khác** $\rightarrow$ Đổi sang **AIMING(Skill_Y)** |
| **CANCEL_SUCCESS** | Click Chuột Phải khi đang ngắm | Tắt ngay indicator, xóa bộ nhớ ngắm. Không mất Mana, không tính cooldown. Giữ nguyên di chuyển. | Trở về ngay **IDLE** |
| **CAST_SUCCESS** | Bấm đúng Phím Skill X lần 2 | Server kiểm tra Tầm đánh & Mana tại thời điểm bấm $\rightarrow$ Trừ Mana $\rightarrow$ Thi triển đòn đánh. | Trở về **IDLE** sau khi hoạt ảnh thi triển hoàn tất. |

---

## 5. KỸ NĂNG CƠ ĐỘNG MẶC ĐỊNH: DASH (LƯỚT THEO HƯỚNG CHUỘT)

- **Phím kích hoạt:** Phím `Shift` (Ngón út tay trái).
- **Cơ chế lướt đỉnh cao:**
  - **Lướt theo hướng con trỏ chuột (Cursor Vector):** Nhân vật lướt thẳng về phía con trỏ chuột đang chỉ (thay vì chỉ lướt theo hướng mặt), cho phép thực hiện kỹ thuật vừa chạy lùi vừa lướt né đòn hoặc lướt xuyên góc bất ngờ.
  - **Thông số kỹ thuật:**
    - Cự ly lướt: Cố định 4.5 mét.
    - Tiêu hao: **20 Mana** mỗi lần lướt.
    - Thời gian hồi (Cooldown): **3.5 giây**.
  - Không chiếm bất kỳ ô nào trong 4 ô kỹ năng cuộn trục. Trong suốt 0.25s lướt, nhân vật không bị khựng hành động, nhưng vẫn nhận sát thương nếu va phải vùng sát thương AoE đang tồn tại.

---

## 6. QUY CHUẨN DI CHUYỂN 8 HƯỚNG & CHIẾN ĐẤU TỰ DO 360° (8-DIRECTIONAL MOVEMENT & 360° COMBAT)

Để tối ưu hóa trải nghiệm chiến đấu thời gian thực (Real-time PvP), kiting né chiêu và dash trong không gian 2D Top-Down MMORPG, Studio thống nhất quy chuẩn kiến trúc điều khiển:

```
                  [ BẮC (UP) ]
           TÂY BẮC \     |     / ĐÔNG BẮC
                    \    |    /
    [ TÂY (LEFT) ] --+-- (HERO) --+-- [ ĐÔNG (RIGHT) ]
                    /    |    \
           TÂY NAM /     |     \ ĐÔNG NAM
                  [ NAM (DOWN) ]
      
     Di chuyển: 8 Hướng (Vector Normalized)
     Ngắm Bắn Skill & Dash: Tự do 360° theo con trỏ chuột
```

### 6.1. Triển Khai Di Chuyển 8 Hướng (8-Directional Movement)
- **8 Hướng di chuyển vật lý:** 
  - 4 hướng chính: Trên (North), Dưới (South), Trái (West), Phải (East).
  - 4 hướng chéo góc $45^\circ$: Đông Bắc (North-East), Tây Bắc (North-West), Đông Nam (South-East), Tây Nam (South-West).
- **Chuẩn hóa Vector Vận Tốc (Normalized Movement Vector):**
  - Bắt buộc thực hiện `direction = direction.normalized` trước khi nhân với `MoveSpeed`.
  - Triệt tiêu hoàn toàn lỗi chạy chéo nhanh hơn chạy thẳng ($\sqrt{2} \approx 1.414\times$), đảm bảo tốc độ di chuyển luôn đồng nhất ở mọi hướng ($5.0\text{ m/s}$).
- **Hỗ trợ Song Hành:** Cả Click-to-Move (tìm đường theo lưới 8 hướng) và phím WASD thủ công đều tuân thủ chặt chẽ 8 hướng này.

### 6.2. Độc Lập Giữa Hướng Di Chuyển & Hướng Ngắm (Kiting / Decoupled Aiming)
- Hướng di chuyển của thân thể và hướng ngắm kỹ năng là **hai hệ trục hoàn toàn độc lập**:
  - Nhân vật có thể chạy lùi về phía Tây Bắc trong khi chuột và ánh mắt vẫn ngắm bắn thẳng về phía Đông Nam để xả chiêu vào kẻ địch truy đuổi (kỹ thuật Kiting đỉnh cao trong PvP).
- **Sprite Hướng Mặt (Facing Direction):**
  - Thân dưới bước đi theo 8 hướng di chuyển.
  - Thân trên và vũ khí hướng về phía con trỏ chuột ($360^\circ$).

### 6.3. Chiến Đấu & Skillshot Tự Do 360° (360° Freelance Aiming)
- **Tuyệt đối KHÔNG ép góc kỹ năng vào 8 hướng:**
  - Tâm ngắm mũi tên (Indicator) và mọi viên đạn ma thuật / kiếm khí bay ra (Projectiles) đều tính toán theo góc thực tế $\theta = \text{atan2}(\Delta y, \Delta x) \in [0^\circ, 360^\circ]$.
  - Độ chính xác tuyệt đối theo con trỏ chuột, giúp người chơi phán đoán bước chạy (leading shot) và bắn tỉa cực kỳ sắc sảo.

### 6.4. Dash Cơ Động Không Giới Hạn (Unrestricted 360° Dash)
- Kỹ năng Lướt (Dash) bằng phím `Shift` hoặc `Space` lướt thẳng theo vector con trỏ chuột ở **góc tự do 360°**.
- Giúp nhân vật bẻ hướng né đòn đột ngột theo bất kỳ góc nào mà không bị khựng lại theo góc lưới $45^\circ$.

### 6.5. Quy Chuẩn Đồ Họa Hoạt Ảnh (Animation Pipeline)
- **Quy chuẩn Sprite:** Khung tranh nhân vật thiết kế dạng Canvas 64x64px hỗ trợ bộ hoạt ảnh 8 hướng (Idle, Walk, Run, Cast, Hurt, Die).
- **Tối ưu hóa giai đoạn đầu:**
  - Tái sử dụng hoạt ảnh lật đối xứng trục X (Flip X) cho các hướng đối xứng (Đông $\leftrightarrow$ Tây, Đông Bắc $\leftrightarrow$ Tây Bắc, Đông Nam $\leftrightarrow$ Tây Nam) nhằm tiết kiệm 40% chi phí vẽ asset nhưng vẫn giữ trọn vẹn độ mượt mà.

