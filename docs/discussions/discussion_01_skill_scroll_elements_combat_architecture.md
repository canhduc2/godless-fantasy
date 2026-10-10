# BIÊN BẢN THẢO LUẬN & BÁO CÁO TIẾN ĐỘ DỰ ÁN (PROJECT PROGRESS & TECHNICAL DISCUSSION)
**Mã tài liệu:** `discussion_01_skill_scroll_elements_combat_architecture.md`  
**Ngày họp / ghi nhận:** 11/10/2026 (04:05 AM)  
**Chủ tọa / Game Director:** SquiRRels Studio  
**Trạng thái phiên họp:** Đã thống nhất & Đang triển khai (In Progress / Actionable)  

---

## 1. MỤC TIÊU CUỘC THẢO LUẬN
Phiên thảo luận nhằm giải quyết dứt điểm các hiểu lầm về kiến trúc hệ thống kỹ năng, thống nhất định nghĩa giữa **Vật phẩm Cuộn Trục (Skill Scroll Item)** và **Kỹ năng Kích hoạt trên Hotbar (Active Skill)**, quy chuẩn hóa **4 Chức Nghiệp Chiến Đấu** và **7 Thuộc Tính Nguyên Tố**, cùng cơ chế nhập ảnh đồ họa từ bên ngoài (External Asset Import) phục vụ thiết kế không cần code (No-Code Game Studio).

---

## 2. NỘI DUNG THỐNG NHẤT & CÁC NGUYÊN TẮC CỐT LÕI

### 2.1. Phân Định Giữa Cuộn Trục Túi Đồ và Icon Ô Chiêu Hotbar
1. **Trong Túi Đồ (Inventory / Bag):**
   - Không vẽ hình thù chiêu thức phức tạp lên mặt cuộn trục.
   - Bản chất hiển thị chỉ là **1 Phôi Cuộn Trục Chuẩn (`ui_template_scroll.png`)** kết hợp **Ấn Ký Thuộc Tính của Hệ (`rune_*`)** trong 7 hệ nguyên tố.
   - Nhìn lướt qua trong túi chỉ phân biệt: *Cuộn Hỏa*, *Cuộn Băng*, *Cuộn Lôi*, *Cuộn Quang*, *Cuộn Ám*, *Cuộn Phong*, *Cuộn Mộc*.
   - Mọi thông số (Tên chiêu, Phẩm cấp E->X, Chức nghiệp, Sát thương, Mana, Hồi chiêu, Tầm đánh) được xem chi tiết qua Tooltip / Modal kiểm tra.
2. **Trên Thanh Phím Nóng Hotbar [1], [2], [3], [4]:**
   - **LÀ MỘT ĐỐI TƯỢNG HOÀN TOÀN KHÁC, ĐƯỢC LOAD RIÊNG!**
   - Icon chiêu thức là **ĐỘC NHẤT (Unique Skill Icon)** cho từng chiêu (Thiên Thạch, Rồng Lửa, Hoa Tuyết, Sấm Sét...).
   - Đặt lọt lòng trong **Khung Viền Vàng Kim Độc Nhất (`ui_frame_slot.png`)**.
   - Mana Badge hiển thị **Ở PHÍA TRÊN ĐẦU Ô PHÍM** (`-{mana}mn`) để giữ lòng ô chiêu sạch sẽ, sang trọng.

### 2.2. Hệ Thống Điều Kiện Kép: 4 Chức Nghiệp & 7 Thuộc Tính
* **4 Chức Nghiệp (Classes):**
  - Kị Sĩ (Knight)
  - Pháp Sư (Mage)
  - Võ Sĩ (Brawler)
  - Xạ Thủ (Archer)
  *(Và Hệ Ẩn: Kẻ Ngoại Đạo - The Heretic kết hợp Pháp Sư + 1 hệ phụ).*
* **7 Thuộc Tính Nguyên Tố (Elements):**
  1. Ánh Sáng (Quang / Light)
  2. Bóng Tối (Ám / Dark)
  3. Phong (Gió / Wind)
  4. Mộc (Cây Cối / Nature)
  5. Băng (Băng Hàn / Ice)
  6. Hỏa (Lửa / Fire)
  7. Lôi (Sấm Sét / Thunder)
* **Quy tắc:** Nhân vật thuộc Chức Nghiệp nào & Thuộc Tính nào thì **CHỈ CÓ THỂ HỌC & DÙNG CUỘN TRỤC TƯƠNG ỨNG**!

### 2.3. Bốn Cơ Chế Tầm Đánh Chuẩn (Standard Archetypes)
1. **Line (Bắn thẳng):** 8.0m, đạn bay raycast/hitbox.
2. **Ground AoE (Vùng đất / Xả mưa):** 7.0m, bán kính 2.5m, hiệu ứng mưa rơi từ trời xuống đất.
3. **Cone (Hình quạt):** 4.5m, quét góc 60°-90° trước mặt.
4. **Self Nova (Tỏa tròn 360°):** 2.5m - 3.0m quanh bản thân người chơi.

### 2.4. Công Cụ Thiết Kế (Studio) & Tải Ảnh Đồ Họa Ngoại Bộ
- Cung cấp studio trong game (`ItemCreatorModal`) và Unity Editor (`GodlessStudioWindow`).
- Cho phép chọn 4 Chức Nghiệp và 7 Thuộc Tính Hệ.
- Hỗ trợ nhập trực tiếp đường dẫn file ảnh ngoài (PNG/JPG) để nạp Sprite chiêu thức độc nhất mà không cần mở Unity Editor hay viết code C#.

---

## 3. TIẾN ĐỘ THỰC TẾ & KẾT QUẢ ĐÃ ĐẠT ĐƯỢC

| Hạng mục công việc | Trạng thái | Chi tiết kỹ thuật |
| :--- | :---: | :--- |
| **GDD Đặc Tả Kỹ Năng Cuộn Trục** | HOÀN TẤT | Tạo file [GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md](file:///d:/godless-fantasy/docs/GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md) |
| **Đồng Bộ GDD DOC-03** | HOÀN TẤT | Cập nhật 7 hệ và điều kiện kép trong [03_HE_THONG_HE_THUOC_TINH_KY_NANG.md](file:///d:/godless-fantasy/docs/03_HE_THONG_HE_THUOC_TINH_KY_NANG.md) |
| **Hit Flash & Combat Feedback** | HOÀN TẤT | Flash đỏ mục tiêu 0.22s, viền màn hình đỏ khi dính đòn, phím `[T]` test sát thương |
| **Đánh Thường Chuột Trái (Basic Attack)** | HOÀN TẤT | Tầm đánh 5.0m, tự động tiếp cận nếu ngoài tầm, reticle ngắm & banner thông tin mục tiêu |
| **Thanh Hotbar & Mana Badge** | HOÀN TẤT | Mana badge đặt TRÊN ô phím (`-32mn`, `-10mn`), giới hạn giảm hồi chiêu Dash tối đa 1.0s |
| **Bảng Chỉ Số Đầy Đủ Trong Tab Kỹ Năng** | HOÀN TẤT | Hiển thị Công Phép, Vật Lý, Đánh Thường, Tốc Đánh, Tầm Đánh 5.0m, Giáp, Kháng, Tốc Chạy |

---

## 4. CÁC VẤN ĐỀ TỒN ĐỌNG & HƯỚNG GIẢI QUYẾT (ACTION ITEMS)

### Vấn đề 1: Đồng bộ hóa 7 Ấn Ký Hệ chuẩn 32x32 và Khung Viền Hoàng Kim
* **Thực trạng:** Cần có đầy đủ 7 Sprite Ấn Ký Hệ (`rune_light`, `rune_dark`, `rune_wind`, `rune_wood`, `rune_ice`, `rune_fire`, `rune_thunder`) chuẩn kích thước 32x32 pixel art.
* **Hướng giải quyết:** Cập nhật script `tools/ArtGenerators/generate_pixel_art.py` để kết xuất chính xác 7 ấn ký và các cuộn trục mẫu 32x32, bảo lưu tính thẩm mỹ retro nguyên bản.

### Vấn đề 2: Cập nhật Data Model `SkillScrollData` với `requiredClass` và 7 Hệ
* **Thực trạng:** Class `SkillScrollData` hiện tại mới có `element` dạng string 5 hệ, chưa có `requiredClass` (Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ, Toàn Hệ).
* **Hướng giải quyết:** Thêm trường `requiredClass`, cập nhật constructor, phương thức `Clone()` và bộ phân tích JSON `SkillJsonData`.

### Vấn đề 3: Nâng cấp Game Studio (`ItemCreatorModal` & `GodlessStudioWindow`)
* **Thực trạng:** Form thiết kế kỹ năng cần có dropdown/toolbar chọn 4 Chức Nghiệp, 7 Hệ, tự động hiển thị Ấn Ký Cuộn Trục tương ứng, và hỗ trợ nút Browse/Input file ảnh ngoài (PNG).
* **Hướng giải quyết:** Bổ sung giao diện chọn Chức nghiệp, 7 Hệ, và cơ chế `ImageLoader.LoadTextureFromFile()` vào `ItemCreatorModal.cs`.

---

## 5. BẢNG THAM CHIẾU TÀI LIỆU & FILE MÃ NGUỒN LIÊN QUAN

* **Tài liệu GDD Đặc tả:**
  - [GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md](file:///d:/godless-fantasy/docs/GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md) — Đặc tả chi tiết Cuộn Trục, 7 Hệ và 4 Chức Nghiệp.
  - [03_HE_THONG_HE_THUOC_TINH_KY_NANG.md](file:///d:/godless-fantasy/docs/03_HE_THONG_HE_THUOC_TINH_KY_NANG.md) — Tài liệu GDD hệ thống gốc.
* **Mã nguồn Logic Chiến Đấu & Kỹ Năng:**
  - [SkillScrollData.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillScrollData.cs) — Cấu trúc dữ liệu cuộn trục kỹ năng.
  - [SkillDefinitionSO.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillDefinitionSO.cs) — ScriptableObject & Data Model JSON.
  - [SkillDatabase.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillDatabase.cs) — Engine quản lý database bí kíp.
  - [BasicAttackController.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/BasicAttackController.cs) — Bộ điều khiển đòn đánh thường chuột trái.
  - [Health.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/Health.cs) — Hệ thống máu và hiệu ứng hit-flash đỏ.
* **Giao diện & Công cụ Thiết kế:**
  - [ItemCreatorModal.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/ItemCreatorModal.cs) — Studio thiết kế asset & kỹ năng [F1].
  - [CombatHUD.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/CombatHUD.cs) — HUD thanh kỹ năng và chỉ số Tab 3.
  - [generate_pixel_art.py](file:///d:/godless-fantasy/tools/ArtGenerators/generate_pixel_art.py) — Công cụ tự động tạo Pixel Art.
