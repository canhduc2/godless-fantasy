# BIÊN BẢN THẢO LUẬN 07: KẾ HOẠCH BỔ SUNG GAP AI QUÁI VẬT & BOSS (WANDERING / LEASH), HỆ THỐNG TELEGRAPH (VÙNG ĐỎ BÁO ĐÒN NÉ CHIÊU), COMBAT SIGNALS HAI CHIỀU & CÔNG CỤ SÁNG TẠO HOẠT ẢNH VFX

> **Dự án:** Godless Fantasy MMORPG (SquiRRels Studio)  
> **Tài liệu tham chiếu:** `DOC-02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md`, `DOC-05_THE_GIOI_MAP_NPC_QUAI_BOSS.md`, `DOC-06_GIAO_DIEN_UI_HUD_VA_CHAT.md`, `GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md`  
> **Người thực hiện:** Antigravity AI Pair Programmer & Lead Game Designer  
> **Ngày lập:** 11/10/2026  
> **Trạng thái:** Sẵn sàng triển khai (Actionable Roadmap & Technical Specifications)

---

## 1. TỔNG KẾT TIẾN ĐỘ & CÁC HẠNG MỤC ĐÃ HOÀN TẤT (ALPHA RECAP)

Trước khi bước vào giai đoạn mở rộng cơ chế AI Quái, Telegraph và Studio Animation, toàn bộ các phản hồi giao diện thực chiến từ Lead Designer đã được hiện thực hóa và kiểm thử đạt chuẩn 100% không lỗi biên dịch:

1. **Khung Chỉ Số Nhân Vật Góc Trái (Top-Left Player Panel):**
   - Thu gọn chiều rộng `panelW` từ 270px xuống **225px** thanh lịch, ôm sát thanh máu và mana (160px).
   - Avatar 44x44, badge Cấp độ Lv 36x16 đặt gọn gàng, dòng chỉ số đáy (Công, Giáp, Tốc, Vàng) hiển thị sắc nét không tràn viền.
2. **Cơ Chế Khóa Hủy Vật Phẩm Bậc Cao (Inventory Safety):**
   - Vật phẩm từ bậc B trở lên (B, A, S, X) bắt buộc chờ đếm ngược **30 giây (Double-Check Protection)** mới cho phép xóa vĩnh viễn, chống thao tác nhầm trang bị quý giá. Bậc C, D, E hủy tức thì.
3. **Đánh Thường Có Điều Kiện & Chuột Phải Công Thái Học (Conditional Basic Attack):**
   - Chỉ cho phép ngắm và đánh thường khi mục tiêu nằm trong cự ly hữu hiệu 3.2m.
   - Nhấp chuột phải mới xuất chiêu (không tự động xả đòn khi đứng yên). Vùng đệm 3.2m - 4.8m giữ focus, ra khỏi 4.8m hủy dấu sau 0.6s ân hạn.
4. **Tối Ưu Hóa Giao Diện Bang Hội (Guild Modal Streamlining):**
   - **Loại bỏ hoàn toàn** đoạn văn bản mô tả dài dòng ("Bạn chỉ được tham gia 1 Bang...", "Cơ chế bảo vệ 1 phút...").
   - Giữ lại **Nút Rời Bang** gọn gàng, chuẩn gameplay đặt tại góc đáy cửa sổ. Khi bấm rời bang, thanh đếm ngược 60s hiển thị inline nhỏ gọn kèm nút Hủy và Xác Nhận.
   - **Thẻ Thông Tin Bang Hội (Info Card):** Tách thành 4 Label con riêng biệt với kích thước chữ lớn (13pt - 16pt bold):
     - Label 1: Tên Bang (`16pt`) & Cấp Bang (`14pt`).
     - Label 2: `Bang chủ: <Tên>` (`13pt`).
     - Label 3: `Quỹ bang: <Vàng>` (`13pt`).
     - Label 4: `Thành viên: <X/50>` (`13pt`).
     - Label 5: `Khẩu hiệu: "<Slogan>"` (`12pt`).
5. **Godless Skill & Scroll Web Studio (Tools):**
   - Cập nhật đầy đủ **6 Cơ Chế Tâm Ngắm (Indicator)** (gồm Cơ chế 6: *Buff Đồng Đội Toàn Đội 360°*).
   - Bổ sung nút tích chọn 2 biến thể AoE rõ ràng: *Biến thể 1: Gây Sát Thương Vùng* vs *Biến thể 2: Hồi Máu & Hồi Mana Vùng*.
   - Hệ thống tải file toàn năng (Universal File Upload): Hỗ trợ file `.json` (Presets), file `.cs` (C# database code), ảnh icon `.png/.jpg` cùng chế độ kéo thả Fullscreen Drag & Drop.

---

## 2. PHÂN TÍCH GAP HỆ THỐNG MỚI (DETAILED GAP ANALYSIS)

Dựa trên yêu cầu chiến lược mới từ Lead Designer:
> *"ae nên làm 1 hệ thống.. cho phép vẽ sáng tạo animation hoạt ảnh cho từng skill e ạ :)) ... rồi cũng nên tiến tới làm thử vài con quái biết di chuyển qua lại random ... :)) có điểm spawwn vùng giới hạn di chuyển nhất định . ... cơ chế AI, boss và các chiêu thức của boss cx giống mình mình cx xem đc các điểm chưởng của quái :))... à quên nữa các chiêu thức đánh nhau giữ người hay quái thì ở phía còn lại đều nhận đc các tín hiệu nhất định khi ra chiêu đế biết còn né đòn nhé ... cái này em làm được chứ /???"*

Studio xác định 4 GAP kỹ thuật trọng tâm cần hoàn thiện ngay:

```
+-----------------------------------------------------------------------------------------+
|                       BẢN ĐỒ KHOẢNG TRỐNG TÍNH NĂNG (GAP MAP)                           |
+-----------------------------------------------------------------------------------------+
|                                                                                         |
|  [ GAP 1: AI QUÁI VẬT & BOSS ]                                                          |
|  • Hiện tại: MonsterTarget chỉ là hình nộm tĩnh (Dummy) đứng yên chịu đòn.              |
|  • Mục tiêu: Quái có Spawn Point, tuần tra ngẫu nhiên (Random Wander), cơ chế Leash     |
|              (quay về vị trí cũ nếu bị kéo quá xa), chuyển đổi FSM (Patrol/Chase/Attack)|
|                                                                                         |
|  [ GAP 2: KỸ NĂNG BOSS & HỆ THỐNG TELEGRAPH (VÙNG ĐỎ BÁO ĐÒN) ]                         |
|  • Hiện tại: Quái chưa có bộ kỹ năng chủ động, không có chỉ báo nguy hiểm dưới đất.     |
|  • Mục tiêu: Boss sở hữu kỹ năng giống người chơi (AoE Đập Đất, Tia Bắn, Quạt Lửa).     |
|              Xuất hiện Vùng Cảnh Báo Nguy Hiểm Đỏ Rực (Telegraph) nạp đầy dần (1.2s - 2s)|
|              trước khi chiêu giáng xuống để người chơi kịp dùng Shift Dash né đòn.      |
|                                                                                         |
|  [ GAP 3: TÍN HIỆU RA CHIÊU HAI CHIỀU (BI-DIRECTIONAL COMBAT SIGNALS) ]                 |
|  • Hiện tại: Đòn đánh xuất chiêu tức thì, đối thủ không có cửa sổ phản xạ (Wind-up).   |
|  • Mục tiêu: Cả hai phía (Người - Quái và PvP Người - Người) đều nhận được tín hiệu     |
|              trực quan (Aura nạp phép, vệt hướng, vòng đất) để thực hiện né đòn kiting. |
|                                                                                         |
|  [ GAP 4: CÔNG CỤ SÁNG TẠO HOẠT ẢNH ANIMATION KỸ NĂNG (VFX STUDIO) ]                    |
|  • Hiện tại: Skill Studio chỉ tạo thông số và sinh code text C#.                       |
|  • Mục tiêu: Tích hợp Canvas Vẽ Pixel / Sequence Frame & Particle Motion Simulator     |
|              ngay trên Web Studio, cho phép xem trước animation 60fps và xuất code.     |
+-----------------------------------------------------------------------------------------+
```

---

## 3. THIẾT KẾ CHI TIẾT CÁC TÍNH NĂNG MỚI (TECHNICAL SPECIFICATIONS)

### 3.1. GAP 1: Máy Trạng Thái AI Quái Vật (Monster Roaming & Leash FSM)

Mỗi quái vật và Boss trong thế giới sở hữu một tọa độ sinh bản địa (`spawnPosition`) và một vùng giới hạn hoạt động (Leash Territory):

```
                   [ VÙNG GIỚI HẠN DI CHUYỂN (LEASH RADIUS: 12m) ]
      + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
      '                                                                         '
      '             [ VÙNG TUẦN TRA NGẪU NHIÊN (WANDER: 4.5m) ]                 '
      '        + - - - - - - - - - - - - - - - - - - - - - - - - - +            '
      '        '                                                   '            '
      '        '                 [ SPAWN POINT ]                   '            '
      '        '                        (0, 0)                     '            '
      '        '                     * Quái sinh sống              '            '
      '        '                     * Đứng nghỉ 2s - 4s           '            '
      '        '                     * Chọn điểm bước ngẫu nhiên   '            '
      '        '                                                   '            '
      '        + - - - - - - - - - - - - - - - - - - - - - - - - - +            '
      '                                      |                                  '
      '                 Phát hiện Player     v  (Cự ly < 6.5m)                  '
      '                [ TRUY ĐUỔI & TUNG CHIÊU / CHASE & ATTACK ]              '
      '                                                                         '
      + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
                                             |
                  Player chạy quá xa         v  (Khoảng cách > 12m)
                  [ LEASH RETURN: QUAY VỀ GỐC & MIỄN NHIỄM HỒI ĐẦY MÁU ]
```

#### Các Trạng Thái FSM Chi Tiết:
1. **`IDLE_RESTING` (Nghỉ chân):**
   - Quái đứng lại, thở nhịp nhàng trong 2.0s - 4.5s.
   - Liên tục quét tầm nhìn (Aggro Radius = 6.0m) để tìm người chơi.
2. **`WANDERING` (Tuần tra ngẫu nhiên):**
   - Chọn một điểm ngẫu nhiên trong bán kính `wanderRadius = 4.5m` quanh `spawnPosition`.
   - Di chuyển chậm rãi (Tốc độ tuần tra: 1.6m/s) tới đích.
   - Đến nơi thì chuyển về `IDLE_RESTING`.
3. **`CHASING` (Truy kích mục tiêu):**
   - Khi phát hiện Player trong tầm nhìn, quái phát ra hiệu ứng chấm than đỏ `❗` trên đầu.
   - Tăng tốc chạy (Tốc độ rượt đuổi: 3.2m/s) bám theo người chơi.
4. **`COMBAT_CASTING` (Vận chiêu tấn công):**
   - Khi vào cự ly tấn công (Basic: 2.0m, Skill: 5.0m): Quái dừng bước, bắt đầu quá trình **Wind-up Casting**.
   - Kích hoạt **Telegraph Vùng Nguy Hiểm** xuống mặt đất.
5. **`LEASH_RETURNING` (Rút lui về gốc):**
   - Nếu bị người chơi dụ chạy vượt quá `leashRadius = 12.0m` tính từ `spawnPosition`:
   - Quái lập tức buông mục tiêu (Drop Aggro), quay lưng chạy thẳng về `spawnPosition` với tốc độ cao (5.5m/s).
   - Trong quá trình rút lui: Quái nhận trạng thái **Miễn Nhiễm Sát Thương (Invulnerable)** và hồi phục 100% lượng máu tối đa (ngăn chặn hành vi lạm dụng kéo boss ra xa để khai thác lỗi).

---

### 3.2. GAP 2 & 3: Hệ Thống Telegraph (Vùng Đỏ Báo Đòn Đánh) & Tín Hiệu Né Đòn Hai Chiều

Để tạo nên trải nghiệm chiến đấu ARPG đỉnh cao (như *Diablo*, *Lost Ark*, *Path of Exile*), toàn bộ chiêu thức nguy hiểm của Quái và Boss đều bắt buộc phải có **Cửa sổ Báo Hiệu (Telegraph Wind-up Window)**:

```
+-----------------------------------------------------------------------------------------+
|                  CƠ CHẾ TELEGRAPH VÙNG ĐỎ BÁO ĐÒN (WIND-UP & DODGE SCHEME)              |
+-----------------------------------------------------------------------------------------+
|                                                                                         |
|  [ GIAI ĐOẠN 1: BẮT ĐẦU VẬN CHIÊU ] (Thời gian: 0.0s)                                   |
|  - Boss gầm rú, dừng di chuyển, xuất hiện hào quang tụ năng lượng trên tay.            |
|  - DƯỚI ĐẤT XUẤT HIỆN VÙNG CHỈ THỊ ĐỎ RỰC (Telegraph Area) tại điểm chiêu sẽ nổ:        |
|    * Dạng Vòng Tròn (AoE Đập Đất): Bán kính 3.2m.                                       |
|    * Dạng Dải Băng (Line Beam): Rộng 1.4m, dài 8.0m.                                    |
|    * Dạng Hình Quạt (Cone Flame): Góc 60 độ, vươn xa 5.0m.                              |
|                                                                                         |
|  [ GIAI ĐOẠN 2: NẠP ĐẦY TIẾN TRÌNH (FILL PROGRESS) ] (Thời gian: 0.0s -> 1.5s)          |
|  - Vùng đỏ có viền ngoài cố định, vòng trong màu đỏ đậm nở rộng / nạp đầy dần (0% -> 100%)|
|  - NGƯỜI CHƠI NHÌN THẤY RÕ RÀNG VÙNG NGUY HIỂM:                                         |
|    => Bấm phím [Shift] lướt Dash thoát hiểm!                                            |
|    => Dùng Click-to-Move / WASD kiting chạy ra khỏi ranh giới màu đỏ!                   |
|                                                                                         |
|  [ GIAI ĐOẠN 3: PHÁT NỔ TUNG SÁT THƯƠNG ] (Thời gian: 1.5s)                             |
|  - Vòng nạp chạm 100% -> Chiêu thức giáng xuống! Hiệu ứng nổ lửa / đất đá rung chuyển!  |
|  - Tính sát thương: Chỉ những ai ĐANG ĐỨNG TRONG VÙNG ĐỎ mới chịu sát thương!            |
|  - Người chơi đã kịp lướt ra ngoài: NÉ ĐÒN THÀNH CÔNG (DODGE CLEARED) - Không mất máu!  |
+-----------------------------------------------------------------------------------------+
```

#### Bảng Quy Chuẩn Tín Hiệu Báo Chiêu Hai Chiều (Bi-directional Combat Signals):

| Đối tượng ra chiêu | Loại chiêu thức | Dạng Telegraph Báo Hiệu | Thời gian Wind-up | Phương án né đòn (Counter-play) |
|---|---|---|---|---|
| **Boss / Quái Tinh Anh** | *Đập Đất Cuồng Lộ* (AoE Circle) | Vòng tròn đỏ bán kính 3.2m nạp đầy dần dưới đất | **1.5 giây** | Lướt `Shift` Dash ra ngoài vòng tròn |
| **Boss / Quái Tinh Anh** | *Tia Ma Đạn Xuyên Phá* (Line Beam) | Dải hình chữ nhật đỏ neon dài 8m bám theo hướng nhắm | **1.2 giây** | Chạy vuông góc 90° né sang hai bên |
| **Boss / Quái Tinh Anh** | *Phun Lửa Hỏa Diễm* (Cone Sweep) | Hình quạt đỏ quét 60 độ tỏa ra phía trước | **1.4 giây** | Áp sát vòng ra sau lưng Boss |
| **Người chơi (PvP)** | *Chiêu Cuộn Trục Bậc A-X* | Hào quang nguyên tố bộc phát + Vòng ngắm sáng đèn | **0.8s - 1.2s** | Đối thủ nhìn hướng mặt và vệt tụ khí để Dash né |

---

### 3.3. GAP 4: Bộ Công Cụ Sáng Tạo Hoạt Ảnh Animation Kỹ Năng (VFX Studio Tool)

Tích hợp trực tiếp module **🎨 VFX & Animation Canvas Designer** vào [tools/GodlessSkillStudio/index.html](file:///d:/godless-fantasy/tools/GodlessSkillStudio/index.html):

```
+-----------------------------------------------------------------------------------------+
|                  GIAO DIỆN VFX & SKILL ANIMATION DESIGNER TRÊN WEB STUDIO               |
+-----------------------------------------------------------------------------------------+
|                                                                                         |
|  [ CỘT TRÁI: DỰ THẢO FRAME & TIMELINE ]     [ CỘT GIỮA: LIVE PREVIEW & PARTICLES ]     |
|  • Timeline 4 - 8 Frame hoạt ảnh            • Canvas mô phỏng 60 FPS                    |
|  • Frame 1: Khởi động tụ khí (Charge)        • Hiệu ứng Hạt (Spark / Shockwave / Trail)  |
|  • Frame 2-3: Bay lượn / Phóng đạn          • Mô phỏng di chuyển đạn bay & va chạm      |
|  • Frame 4-5: Bộc phá / Nổ tung (Impact)    • Nút Play / Pause / Replay Animation       |
|                                                                                         |
|  [ CỘT PHẢI: BẢNG THIẾT LẬP THUỘC TÍNH VFX ]                                            |
|  • Màu sắc hạt: Chọn theo 7 Hệ Nguyên Tố (Hỏa, Băng, Lôi, Quang, Ám, Phong, Mộc)        |
|  • Hình dạng chưởng: Tròn (Ball), Cung Kiếm (Crescent), Chùm Đấm (Fist), Tia Sét (Beam) |
|  • Tốc độ bay (Velocity), Độ rung màn hình (Screen Shake), Bán kính nổ (Blast Radius)   |
|  • Nút xuất: 📋 Xuất Code C# VFX Routine & 📥 Tải File Cấu Hình VFX JSON                |
+-----------------------------------------------------------------------------------------+
```

---

## 4. KẾ HOẠCH TRIỂN KHAI THEO CÁC GIAI ĐOẠN (IMPLEMENTATION ROADMAP)

### Giai Đoạn 1: Hiện Thực Hóa AI Quái Vật Tuần Tra & Boss Đập Đất (C# Client)
- **Tập tin:** `Assets/Scripts/Combat/MonsterAIController.cs` (Mới)
  - Triển khai FSM: `Idle`, `Wander` (ngẫu nhiên quanh spawn point), `Chase` (khi cự ly < 6.5m), `Attack/Skill`, `LeashReturn` (hồi máu khi cự ly > 12m).
  - Khởi tạo quái vật tuần tra trong `DemoBootstrapper.cs`.
- **Tập tin:** `Assets/Scripts/Combat/TelegraphIndicator.cs` (Mới)
  - Vẽ vùng cảnh báo nguy hiểm (Màu đỏ trong suốt, vòng viền nạp đầy dần theo thời gian 1.2s - 1.5s).
  - Tự động kích hoạt sát thương và hủy khi chiêu phát nổ.

### Giai Đoạn 2: Thử Nghiệm Thực Tế Boss Có Kỹ Năng Đập Đất & Né Chiêu (Combat Test)
- Đặt 1 Boss Mini hoặc Quái Tinh Anh `Tê Giác Sa Mạc / Thủ Lĩnh Hỏa Long` tại bãi quái.
- Cứ sau 6s - 8s, Boss dừng lại, gồng chiêu đập đất AoE (bán kính 3.2m), hiện Telegraph đỏ nạp trong 1.5s.
- Người chơi nhìn thấy vùng đỏ -> dùng `Shift` Dash lướt né ra ngoài an toàn.

### Giai Đoạn 3: Tích Hợp Module VFX & Skill Animation Designer Vào Web Studio
- Cập nhật `tools/GodlessSkillStudio/index.html`:
  - Thêm khu vực **🎨 Sáng Tạo Animation & Hoạt Ảnh Kỹ Năng**.
  - Tích hợp HTML5 Canvas vẽ chuyển động hoạt ảnh của chiêu thức, mô phỏng vệt bay của chưởng lực theo thời gian thực.
  - Cho phép chọn hiệu ứng hạt, màu sắc phát sáng và kiểm tra 60fps trực quan.

---

## 5. TIÊU CHUẨN ĐÁNH GIÁ & NGHIỆM THU (ACCEPTANCE CRITERIA)

1. **AI Di Chuyển Tuần Tra:** Quái vật tự động đi dạo qua lại trong bán kính 4.5m, không đi xuyên vật cản, đứng nghỉ ngẫu nhiên, bị kéo quá 12m tự quay về chỗ cũ hồi đầy máu.
2. **Cơ Chế Telegraph Báo Hiệu Đòn Đánh:** Khi Boss tung chiêu đập đất AoE, vùng cảnh báo đỏ rực nạp đầy dần trong 1.5s hiển thị rõ ràng dưới chân; người chơi đứng ngoài vùng đỏ không phải nhận sát thương.
3. **Phản Xạ Né Chiêu Mượt Mà:** Phím `Shift` (Dash) và di chuyển giúp nhân vật kịp thời thoát khỏi vùng nguy hiểm trong cửa sổ 1.5s.
4. **Hệ Thống Biên Dịch Unity:** Toàn bộ mã nguồn C# biên dịch thành công (`0 Errors, 0 Warnings`).
5. **Công Cụ Web Studio:** Có canvas xem trước hoạt ảnh bay và bộc phá chiêu thức cho các nhà thiết kế game trong team.
