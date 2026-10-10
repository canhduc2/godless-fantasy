# BIÊN BẢN THẢO LUẬN & BÁO CÁO TIẾN ĐỘ DỰ ÁN SỐ 02
**Mã tài liệu:** `discussion_02_hp_mp_regeneration_buff_system_and_class_perks.md`  
**Ngày ghi nhận:** 11/10/2026 (04:25 AM)  
**Chủ tọa / Game Director:** SquiRRels Studio  
**Trạng thái phiên họp:** Đã thống nhất & Triển khai vào code (Implemented)  

---

## 1. MỤC TIÊU CUỘC THẢO LUẬN
Phiên thảo luận tập trung giải quyết 3 bài toán game design cốt lõi nhằm nâng cao chiều sâu nhập vai, cân bằng sinh tồn và đa dạng hóa cơ chế chiến đấu của *Godless Fantasy*:
1. **Bài toán hồi phục chỉ số HP & Mana:** Thiết lập cơ chế Hồi Máu Khắc Khổ (chống spam / buff bẩn), hồi Mana ổn định và chuyên biệt hóa theo 4 Chức Nghiệp.
2. **Cơ chế Kỹ Năng Số 5: Buff Tự Thân (Self-Buff / Support) & UI Buff Widget:** Bổ sung cơ chế thi triển không cần tầm đánh và giao diện hiển thị trạng thái hiệu ứng buff ở góc trái giữa màn hình.
3. **Phần thưởng Perk Bẩm Sinh Độc Nhất Theo Hệ:** Thiết kế bộ 4 Perk độc bản cộng vĩnh viễn chỉ số theo Chức Nghiệp.
4. **Khôi phục Khung Viền Chuẩn Hoàng Kim 32x32:** Trích xuất và khóa bất biến cấu trúc pixel-art khung viền kỹ năng nguyên bản theo hình ảnh chỉ định của studio.

---

## 2. NỘI DUNG THỐNG NHẤT & THIẾT KẾ CHI TIẾT

### 2.1. Cơ Chế Hồi Máu Khắc Khổ (Harsh HP Regeneration - No Cheap Buffs)
* **Triết lý thiết kế:** Trong thế giới vô thần khắc nghiệt của *Godless Fantasy*, sinh mạng nhân vật là tài nguyên cực kỳ quý giá. Tuyệt đối không cho phép hồi máu spam liên tục theo từng giây (chống hiện tượng "buff bẩn" làm mất tính thử thách sinh tồn).
* **Quy chuẩn thời gian & lượng hồi:**
  - Chu kỳ hồi máu tự nhiên: **30s – 40s mới kích hoạt 1 lần tick**.
  - Lượng máu mỗi lần tick: **Chỉ +1 đến +2 HP**.
  - Phân hóa theo Chức Nghiệp:
    - **Kị Sĩ (Knight):** Thể chất vượt trội $\rightarrow$ Chu kỳ **30 giây** / tick, hồi **+2 HP**.
    - **Võ Sĩ (Brawler):** Khí huyết dồi dào $\rightarrow$ Chu kỳ **35 giây** / tick, hồi **+2 HP**.
    - **Pháp Sư (Mage) & Xạ Thủ (Archer):** Thể lực mỏng $\rightarrow$ Chu kỳ **40 giây** / tick, hồi **+1 HP**.
* **Định hướng hành vi người chơi:** Khi bị trọng thương, người chơi bắt buộc phải:
  1. Sử dụng bình dược liệu hồi máu [Q] (tiêu tốn tài nguyên kinh tế).
  2. Rút lui về Vùng An Toàn (Safe Zone / Làng Tân Thủ) để tịnh dưỡng hoặc xin hỗ trợ từ đồng đội.

---

### 2.2. Cơ Chế Hồi Mana & Chuyên Biệt Hóa 4 Chức Nghiệp
* **Mana (Năng lượng thi triển):** Là "xăng" vận hành chiêu thức và động tác Lướt (Dash).
* **Quy chuẩn hồi phục:** Chu kỳ tick 3.0 giây / lần.
  - **Pháp Sư (Mage):** Bậc thầy ma đạo $\rightarrow$ **+4.0 Mana / 3s** (+1.33 MP/s), tăng thêm theo chỉ số Trí Tuệ INT.
  - **Kị Sĩ, Võ Sĩ, Xạ Thủ:** **+2.0 Mana / 3s** (+0.67 MP/s).
* **Bảng chỉ số thông tin:** Hiển thị công khai trong Tab 1 (Trang Bị) và Tab 3 (Kỹ Năng) của Combat HUD:
  - *Tốc độ hồi Máu: +1 HP mỗi 40s (Khắc khổ)*
  - *Tốc độ hồi Mana: +4.0 MP mỗi 3s (Tự nhiên)*

---

### 2.3. Hệ Thống Perk Bẩm Sinh Độc Nhất Theo Hệ (Permanent Inherent Class Perks)
Mỗi Chức Nghiệp khi khởi tạo được ban tặng **1 Perk Bẩm Sinh Độc Bản**, cộng vĩnh viễn các chỉ số đặc thù tạo nên phong cách chiến đấu hoàn toàn tách biệt:

| Chức Nghiệp | Tên Perk Bẩm Sinh | Biểu Tượng | Chỉ Số Thưởng Vĩnh Viễn | Bản Sắc Lối Chơi |
| :--- | :--- | :---: | :--- | :--- |
| **Kị Sĩ (Knight)** | **Trái Tim Thép** *(Iron Heart)* | 🛡️ | • **+50 Máu Tối Đa** vĩnh viễn<br>• **+15 Giáp Vật Lý**<br>• Rút ngắn hồi HP xuống 30s (+2 HP) | Bức tường thép tiên phong, chống chịu đòn đánh và che chắn đồng đội. |
| **Pháp Sư (Mage)** | **Học Giả Chân Kinh** *(Archmage Mind)* | 🔮 | • **+40 Mana Tối Đa** vĩnh viễn<br>• **+20 Công Phép**<br>• Tốc độ hồi Mana siêu tốc (+4 MP / 3s) | Cỗ pháo ma thuật tầm xa, xả chiêu diện rộng liên tục. |
| **Võ Sĩ (Brawler)** | **Bản Năng Dã Thú** *(Primal Instinct)* | 🥊 | • **+8% Tỉ Lệ Chí Mạng** vĩnh viễn<br>• **+12 Sát Thương Đòn Đánh Thường**<br>• Rút ngắn hồi HP xuống 35s (+2 HP) | Quyền cước bạo liệt, áp sát dồn combo sốc sát thương bạo kích. |
| **Xạ Thủ (Archer)** | **Mắt Ưng Thần Tiễn** *(Eagle Eye)* | 🏹 | • **+1.5m Tầm Đánh Thường** (5.0m $\rightarrow$ 6.5m)<br>• **+0.5 m/s Tốc Độ Chạy** vĩnh viễn | Bắn tỉa tầm siêu xa, thả diều cơ động né tránh truy đuổi. |

---

### 2.4. Cơ Chế Tầm Kỹ Năng Số 5: BUFF TỰ THÂN & GIAO DIỆN HIỂN THỊ TRẠNG THÁI (BUFF HUD)
* **Định nghĩa Archetype Số 5 (Self Buff):**
  - Không có cự ly tầm ngắm ($Range = 0m$).
  - Khi bấm phím nóng [1..4], kỹ năng tự động kích hoạt lên chính bản thân người chơi.
  - Tỏa luồng hào quang ánh sáng bốc lên quanh thân (Particle bốc từ chân lên đầu theo màu của 1 trong 7 Hệ).
* **Loại hình hiệu ứng:**
  - Hồi phục một lượng sinh lực hoặc ma lực tức thì / theo đợt.
  - Cường hóa tạm thời chỉ số (ví dụ: *+25% Công Phép trong 15 giây*, *+40 Giáp trong 10 giây*, *+30% Tốc Chạy trong 8 giây*).
* **Giao Diện Active Buff Widget (Góc Trái Giữa Màn Hình):**
  - Vị trí: `x = 16px`, `y = Screen.height * 0.42f`.
  - Khung thẻ hiển thị sang trọng gồm:
    1. **Icon kỹ năng** (Icon độc nhất lồng trong khung vàng kim nhỏ).
    2. **Tên hiệu ứng** (ví dụ: *Cuồng Bạo Hỏa Long*, *Khiên Thánh Quang*).
    3. **Thanh tiến trình & Thời gian còn lại** (ví dụ: `14.2s` đếm ngược thời gian thực).

---

### 2.5. Khôi Phục Khung Viền Ô Chiêu Chuẩn Hoàng Kim 32x32
* **Trích xuất nguyên bản từ ảnh mẫu của studio:**
  - Viền ngoài: 1 pixel Vàng Kim `(218, 165, 32)`.
  - Viền trong: 1 pixel Bevel Xanh Đen Navy `(25, 38, 58)`.
  - Chốt góc: Khối góc chữ L màu Vàng Sáng Hoàng Kim `(235, 195, 75)` tại 4 góc `(1,1)`, `(30,1)`, `(1,30)`, `(30,30)`.
  - Toàn bộ lòng ô từ `x=2..29`, `y=2..29` là **100% trong suốt**, giúp icon độc nhất của kỹ năng hiển thị trọn vẹn, sắc nét và đẳng cấp retro HD.

---

## 3. DANH MỤC FILE THAY ĐỔI & THAM CHIẾU

* **Tài liệu GDD & Báo cáo:**
  - [GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md](file:///d:/godless-fantasy/docs/GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md) — Cập nhật Archetype 5 (Buff) và cơ chế hồi HP khắc khổ.
  - [discussion_01_skill_scroll_elements_combat_architecture.md](file:///d:/godless-fantasy/docs/discussions/discussion_01_skill_scroll_elements_combat_architecture.md) — Thảo luận số 01.
  - [discussion_02_hp_mp_regeneration_buff_system_and_class_perks.md](file:///d:/godless-fantasy/docs/discussions/discussion_02_hp_mp_regeneration_buff_system_and_class_perks.md) — Thảo luận số 02 (tài liệu này).
* **Mã nguồn thực thi:**
  - [PlayerStats.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Player/PlayerStats.cs) — Logic hồi máu 40s, hồi mana 3s, perk 4 hệ, buff manager.
  - [SkillScrollData.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillScrollData.cs) — Bổ sung Indicator Type 5 (Buff) và buff duration.
  - [CombatHUD.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/CombatHUD.cs) — Widget Buff góc trái giữa màn hình, hiển thị tốc độ hồi máu/mana trong Tab Kỹ Năng.
  - [generate_pixel_art.py](file:///d:/godless-fantasy/tools/ArtGenerators/generate_pixel_art.py) — Khóa thuật toán vẽ khung vàng kim chuẩn 32x32.
