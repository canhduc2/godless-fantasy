# ĐẶC TẢ THIẾT KẾ & KIẾN TRÚC KỸ THUẬT: HỆ THỐNG BÍ KÍP CUỘN TRỤC (SKILL SCROLL), 7 THUỘC TÍNH NGUYÊN TỐ & 4 CHỨC NGHIỆP CHIẾN ĐẤU
**Mã tài liệu:** GDD-DOC-03B  
**Dự án:** Godless Fantasy (2D Top-Down Retro HD MMORPG)  
**Studio phát triển:** SquiRRels Studio  
**Trạng thái:** Chính Thức (Approved Specification)  

---

## 1. TỔNG QUAN & BẢN CHẤT CỐT LÕI

Hệ thống Kỹ Năng trong *Godless Fantasy* được xây dựng dựa trên nguyên lý tách rời hoàn toàn giữa:
1. **Vật Phẩm Hữu Hình trong Túi Đồ (Skill Scroll Item)**: Đóng vai trò là vật chứa bí kíp ma pháp cổ xưa.
2. **Kỹ Năng Kích Hoạt Trên Thanh Phím Nóng (Hotbar Active Skill [1..4])**: Thực thể chiêu thức đã được người chơi lĩnh hội và giải phóng trong giao tranh.

```
+----------------------------------------------------------------------------------------------------+
|                                    QUY TRÌNH HỌC VÀ THI TRIỂN BÍ KÍP                                |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  [VẬT PHẨM TRONG TÚI ĐỒ / HÀNH TRANG]                                                              |
|   • Chỉ hiển thị: 1 Template Cuộn Trục Duy Nhất + 1 trong 7 Ấn Ký Nguyên Tố (7 Elemental Runes).   |
|   • Nhìn bên ngoài chỉ biết Hệ: Cuộn Hỏa, Cuộn Băng, Cuộn Lôi, Cuộn Phong...                       |
|   • Xem chi tiết (Tooltip): Mới thấy Tên Chiêu, Phẩm Cấp (E->X), Yêu Cầu Chức Nghiệp, Sát Thương...|
|                                                                                                    |
|                                         |                                                          |
|                                         | (Kéo Thả vào Phím [1], [2], [3], [4])                    |
|                                         v                                                          |
|                                                                                                    |
|  [Ô CHIÊU THỨC TRÊN THANH HOTBAR]                                                                 |
|   • ĐỐI TƯỢNG HOÀN TOÀN KHÁC BIỆT, ĐƯỢC LOAD RIÊNG!                                                |
|   • Icon Chiêu Thức là ĐỘC NHẤT (Unique Skill Icon) - vẽ đặc trưng cho chiêu (Thiên Thạch, Lốc...). |
|   • Lồng vào 1 Khung Viền Vàng Kim Độc Nhất (ui_frame_slot.png).                                   |
|   • Mana Badge hiển thị ở PHÍA TRÊN ô chiêu: -{mana}mn (ví dụ: -32mn, -25mn).                      |
|                                                                                                    |
|                                         |                                                          |
|                                         | (Nhấn Phím 1..4 -> Chọn Tâm Ngắm & Xuất Chiêu)           |
|                                         v                                                          |
|                                                                                                    |
|  [HOẠT ẢNH & HIỆU ỨNG CHIẾN ĐẤU (VFX)]                                                             |
|   • Vận hành trên 4 Cơ Chế Tầm Đánh Chuẩn: Line (Đạn thẳng), AoE (Mưa rơi), Cone (Quạt), Nova.    |
|   • Standard: Tính toán toán học va chạm & phủ màu hạt ánh sáng theo 1 trong 7 Hệ.                 |
|   • Custom: Cho phép tải Sprite Animation riêng biệt từ bên ngoài vào để đổi mới đồ họa!           |
|                                                                                                    |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. HỆ THỐNG ĐIỀU KIỆN KÉP: 4 CHỨC NGHIỆP & 7 THUỘC TÍNH NGUYÊN TỐ

Để học và kích hoạt một Cuộn Trục Kỹ Năng, nhân vật bắt buộc phải thỏa mãn đồng thời **Điều Kiện Kép**:

### 2.1. Điều Kiện 1: Trùng Khớp Chức Nghiệp Chiến Đấu (Class Affinity)
Mỗi cuộn trục ghi rõ chức nghiệp có thể lĩnh hội:
* **Kị Sĩ (Knight):** Chuyên kiếm thuật, khiên chắn, khiêu khích, càn quét cận chiến.
* **Pháp Sư (Mage):** Chuyên ma pháp hủy diệt diện rộng, khống chế, nguyên tố tầm xa.
* **Võ Sĩ (Brawler):** Chuyên quyền cước, áp sát chớp nhoáng, phá giáp, combo choáng.
* **Xạ Thủ (Archer):** Chuyên cung tiễn ma thuật, bẫy tầm xa, bắn tỉa xuyên thấu.
* *(Trường hợp đặc biệt)* **Hệ Ẩn - Kẻ Ngoại Đạo (The Heretic):** Có thể học song song bí kíp của Pháp Sư và 1 hệ phụ ngẫu nhiên đã kết hợp.
* *(Trường hợp cuộn trục phổ thông)* **Toàn Hệ (Universal):** Bất kỳ chức nghiệp nào cũng có thể học (ví dụ: các chiêu bổ trợ, hộ thân cơ bản).

> **Quy tắc:** Nhân vật thuộc Chức Nghiệp nào thì **CHỈ CÓ THỂ HỌC** cuộn trục ghi đúng Chức Nghiệp đó. Kị Sĩ không thể học cuộn trục của Pháp Sư hay Xạ Thủ.

---

### 2.2. Điều Kiện 2: Tương Thích 7 Thuộc Tính Nguyên Tố (7 Elemental Affinities)
Thế giới ma pháp vận hành bởi 7 nguồn năng lượng nguyên tố cổ xưa:

| STT | Thuộc Tính Nguyên Tố | Mã Hệ | Bản Sắc Thị Giác | Ấn Ký Trên Mặt Cuộn Trục (Rune) |
| :---: | :--- | :---: | :--- | :--- |
| **1** | **Ánh Sáng (Quang / Light)** | `Light` | Vàng kim rực rỡ, thái dương, thánh khiết | Ấn Ký Thái Dương Hoàng Kim (`rune_light`) |
| **2** | **Bóng Tối (Ám / Dark)** | `Dark` | Tím thẫm huyền bí, hắc nguyệt, sương đêm | Ấn Ký Hắc Nguyệt Trăng Khuyết (`rune_dark`) |
| **3** | **Phong (Gió / Wind)** | `Wind` | Xanh lơ bạc, gió lốc xoáy cuộn | Ấn Ký Lốc Xoáy Bạc Lơ (`rune_wind`) |
| **4** | **Mộc (Cây Cối / Nature)** | `Wood` | Xanh lục bảo, chồi non, cội nguồn sinh mệnh | Ấn Ký Chồi Non Sinh Mệnh (`rune_wood`) |
| **5** | **Băng (Băng Hàn / Ice)** | `Ice` | Xanh tuyết pha lê, hàn khí lạnh giá | Ấn Ký Hoa Tuyết Pha Lê (`rune_ice`) |
| **6** | **Hỏa (Lửa Đỏ / Fire)** | `Fire` | Đỏ cam rực lửa, bùng cháy, than hồng | Ấn Ký Hỏa Diễm Kim Cương (`rune_fire`) |
| **7** | **Lôi (Sấm Sét / Thunder)** | `Thunder`| Tím điện, vàng sấm chớp giáng lâm | Ấn Ký Thiên Lôi Chớp Giật (`rune_thunder`) |

> [!IMPORTANT]
> **QUY TẮC KHÓA TUYỆT ĐỐI (STRICT LOCKOUT - KHÔNG THỂ SỬ DỤNG):**
> * Tuyệt đối **KHÔNG CÓ CƠ CHẾ PHẠT GIẢM 50% HIỆU QUẢ**. Nếu Nhân vật không đúng Chức Nghiệp (Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ) hoặc không đúng Thuộc Tính Nguyên Tố (1 trong 7 hệ), hệ thống **KHÔNG CHO PHÉP HỌC / KHÔNG CHO PHÉP TRANG BỊ VÀ KHÔNG THỂ THI TRIỂN!**
> * Nhân vật Hệ Hỏa $\rightarrow$ Chỉ có thể học và trang bị cuộn trục mang Ấn Ký Hỏa. Tuyệt đối **KHÔNG DÙNG ĐƯỢC** cuộn trục Băng, Lôi, Quang, Ám, Phong, Mộc.
> * Cùng là một cơ chế xả mưa (Rain Fall Archetype), nếu người chơi hệ Hỏa thì chỉ dùng được cuộn "Mưa Thiên Thạch" (Ấn Ký Hỏa), người hệ Băng chỉ dùng được cuộn "Bão Tuyết Hàn Băng" (Ấn Ký Băng), người hệ Lôi chỉ dùng được cuộn "Thiên Lôi Oanh Kích" (Ấn Ký Lôi). Khác hệ hoàn toàn không thể học!

---

## 3. QUY CHUẨN ĐỒ HỌA TÁCH LỚP: CUỘN TRỤC TÚI ĐỒ VS ICON HOTBAR

Để đảm bảo hiệu năng tối ưu, tính thẩm mỹ cao cấp và khả năng tái sử dụng tài nguyên vô hạn:

### 3.1. Hiển Thị Cuộn Trục Trong Túi Đồ (Item Scroll Presentation)
* **Bản chất:** Trong túi đồ, tất cả cuộn trục đều chia sẻ chung **1 Phôi Cuộn Trục Duy Nhất (`ui_template_scroll.png`)** (kích thước chuẩn 32x32 pixel art):
  - 2 thanh gỗ cuộn với chốt gài trên và dưới.
  - Tờ giấy da cổ (parchment) nhuộm màu thời gian.
  - Con dấu sáp đỏ niêm phong ở góc trên bên phải.
  - Vùng trung tâm cuộn giấy là nơi khắc **1 trong 7 Ấn Ký Thuộc Tính Hệ (7 Runes)**.
* **Quy tắc thị giác:** Nhìn vào túi đồ, người chơi chỉ thấy **7 loại icon cuộn trục cơ bản** tương ứng 7 hệ nguyên tố:
  1. `item_scroll_light` = Template + `rune_light`
  2. `item_scroll_dark` = Template + `rune_dark`
  3. `item_scroll_wind` = Template + `rune_wind`
  4. `item_scroll_wood` = Template + `rune_wood`
  5. `item_scroll_ice` = Template + `rune_ice`
  6. `item_scroll_fire` = Template + `rune_fire`
  7. `item_scroll_thunder` = Template + `rune_thunder`
* **Hiển thị thông tin sâu:** Mọi chi tiết cụ thể sẽ **CHỈ XUẤT HIỆN KHI XEM CHI TIẾT (Hover Tooltip / Modal)**:
  - Tên chiêu thức (ví dụ: *Hỏa Long Bộc Phá*, *Thánh Kiếm Mưa Rơi*).
  - Phẩm cấp (E, D, C, B, A, S, X) với màu sắc viền tương ứng.
  - Chức nghiệp yêu cầu: *[Pháp Sư]* hoặc *[Kị Sĩ]*...
  - Thuộc tính hệ: *[Hệ Hỏa]* hoặc *[Hệ Băng]*...
  - Sát thương cơ bản + Tỷ lệ cộng hưởng Trí Tuệ (% INT).
  - Lượng Mana tiêu hao & Thời gian hồi chiêu (Cooldown).
  - Tầm đánh (mét) và cơ chế ngắm (Bắn thẳng / Mưa vùng đất / Hình quạt / Tỏa quanh thân).

---

### 3.2. Hiển Thị Ô Kỹ Năng Trên Phím Nóng Hotbar [1, 2, 3, 4]
* **ĐỐI TƯỢNG HOÀN TOÀN KHÁC BIỆT & ĐƯỢC LOAD RIÊNG:**
  - Khi cuộn trục được kéo thả vào ô phím [1..4], hệ thống không hiển thị cuộn trục nữa.
  - Thay vào đó, hệ thống load **Icon Kỹ Năng Độc Nhất (Unique Skill Icon)** của chính chiêu thức đó (ví dụ: quả cầu lửa rơi, hoa tuyết lốc xoáy, thanh kiếm sáng, mũi tên phong thần...).
* **Kiến trúc Layer trên Hotbar Slot:**
  - `Layer 0 (Background):` Nền đen sâu thẳm `(0.04, 0.05, 0.09)`.
  - `Layer 1 (Core Icon):` Sprite Icon Độc Nhất của kỹ năng (`icon_skill_*` 32x32).
  - `Layer 2 (Universal Frame):` Khung viền kim loại hoàng kim độc nhất (`ui_frame_slot.png` 32x32) với 4 chốt góc nổi khối che phủ bên ngoài.
  - `Layer 3 (Hotkey Badge):` Chữ số phím nóng `[1]`, `[2]`, `[3]`, `[4]` màu vàng kim ở góc trên bên trái.
  - `Layer 4 (Mana Badge Ở TRÊN KHUNG):` Huy hiệu tiêu hao năng lượng đặt ở **PHÍA TRÊN ĐẦU Ô PHÍM**, định dạng rõ ràng `-32mn`, `-25mn`, `-10mn` với màu xanh Cyan dịu mắt, giữ cho lòng ô kỹ năng luôn thoáng đãng và nổi bật hình ảnh chiêu thức!

---

## 4. NĂM CƠ CHẾ TẦM ĐÁNH CHUẨN (THE 5 COMBAT ARCHETYPES)

Mọi chiêu thức trong trò chơi được quy hoạch dựa trên **5 Cơ Chế Tầm Đánh & Thi Triển Chuẩn**:

```
+-----------------------------------------------------------------------------------------------------------------------+
|                                             5 LOẠI CƠ CHẾ THI TRIỂN CHUẨN                                             |
+--------------------+---------------------+----------------------+-------------------------+---------------------------+
| 1. BẮN THẲNG       | 2. VÙNG ĐẤT / XẢ MƯA | 3. HÌNH QUẠT         | 4. BÙNG NỔ 360° (NOVA)  | 5. BUFF TỰ THÂN (SUPPORT) |
|    (LINE)          |    (GROUND AOE)     |    (CONE ARC)        |    (SELF AOE)           |    (SELF BUFF)            |
+--------------------+---------------------+----------------------+-------------------------+---------------------------+
| Cự ly: 8.0m        | Cự ly: 7.0m         | Cự ly: 4.5m (Góc 60°)| Bán kính: 2.5m - 3.0m   | Cự ly: 0m (Tự Thân)       |
| Đạn bay thẳng      | Chọn tâm đất        | Quét tỏa trước mặt   | Nổ bùng quanh thân      | Hào quang bốc lên         |
| Hitbox đạn / ray   | Mưa rơi từ trên trời| Càn quét nhóm quái   | Giải vây tức thì        | Tăng chỉ số / Hồi phục    |
+--------------------+---------------------+----------------------+-------------------------+---------------------------+
```

### 4.1. Chi Tiết Từng Archetype
1. **Bắn Thẳng (Linear Projectile - Mã 1):**
   - **Tầm xa chuẩn:** 8.0 mét. Bán kính đạn: 0.4m.
   - **Cơ chế thi triển:** Phóng một luồng đạn ma pháp bay thẳng tắp theo hướng con trỏ chuột.
   - **Toán học va chạm:** Tia Raycast / Boxcast liên tục dọc vector vận tốc; gây sát thương khi chạm hitbox mục tiêu đầu tiên (hoặc xuyên thấu toàn bộ nếu có thuộc tính Xuyên Tâm).
2. **Vùng Đất / Xả Mưa Từ Trên Trời (Ground AoE / Rain Fall - Mã 2):**
   - **Cự ly chọn tâm chuẩn:** 7.0 mét. Bán kính vùng nổ: 2.5 mét.
   - **Cơ chế thi triển:** Người chơi di chuyển con trỏ chuột đến một vị trí trên bản đồ (hiển thị vòng tròn chỉ thị decal). Khi bấm xuất chiêu:
     - Hoạt ảnh mưa ma pháp (Mưa Thiên Thạch Hỏa, Mưa Tuyết Băng, Thiên Lôi Lôi, Mưa Kiếm Quang...) rơi từ trên không trung xuống tâm đất.
     - Sau độ trễ rơi (Delay 0.35s), vùng đất phát nổ gây sát thương diện rộng lên mọi kẻ địch trong bán kính 2.5m.
3. **Hình Quạt (Cone Sweep - Mã 3):**
   - **Cự ly quét chuẩn:** 4.5 mét. Góc mở: 60° đến 90°.
   - **Cơ chế thi triển:** Quét một luồng năng lượng hình nón ngay phía trước mặt nhân vật theo hướng chuột.
   - **Toán học va chạm:** Kiểm tra khoảng cách $\le 4.5m$ và góc lệch $\theta \le \pm 30^\circ$ so với vector hướng mặt.
4. **Bùng Nổ 360° Hào Quang (Self Nova - Mã 4):**
   - **Bán kính chuẩn:** 2.5m - 3.0m quanh tâm nhân vật.
   - **Cơ chế thi triển:** Không cần chọn hướng chuột. Nhân vật tụ khí và kích nổ luồng sóng xung kích 360 độ quanh thân.
   - **Tác dụng chiến thuật:** Tuyệt chiêu giải vây tức thì khi bị bầy quái vật vây hãm, thường đi kèm hiệu ứng đẩy lùi hoặc làm chậm.
5. **Buff Tự Thân & Cường Hóa (Self Buff / Support - Mã 5):**
   - **Tầm đánh:** 0 mét (Tác động trực tiếp lên bản thân).
   - **Cơ chế thi triển:** Tự kích hoạt ngay lập tức khi nhấn phím [1..4]. Luồng hào quang nguyên tố bốc lên từ chân lên đầu (màu sắc theo 1 trong 7 Hệ).
   - **Hiệu ứng:** Cường hóa tạm thời chỉ số chiến đấu (Công Phép, Giáp, Tốc Chạy, Tỉ Lệ Bạo Kích...) hoặc hồi phục một lượng tài nguyên.
   - **Giao diện hiển thị trạng thái (Active Buff Widget):**
     - Đặt tại góc bên trái ở giữa màn hình (`x = 16px`, `y = Screen.height * 0.42f`).
     - Hiển thị gồm: **Icon kỹ năng** (lồng khung vàng kim), **Tên hiệu ứng** và **Thanh đếm ngược thời gian còn lại** (`14.2s`).

---

## 4.2. BÀI TOÁN HỒI PHỤC HP & MANA: KHẮC KHỔ & CHỐNG BUFF BẨN

* **Cơ Chế Hồi Máu Khắc Khổ (Harsh Survival):**
  - Tuyệt đối **KHÔNG HỒI MÁU LIÊN TỤC THEO TỪNG GIÂY** (loại bỏ hoàn toàn buff bẩn).
  - Chu kỳ hồi máu tự nhiên: **40 giây / tick** (Kị Sĩ: 30s, Võ Sĩ: 35s, Pháp Sư/Xạ Thủ: 40s).
  - Lượng máu mỗi tick: **Chỉ +1 hoặc +2 HP**. Muốn hồi máu nhanh phải uống bình dược phẩm [Q] hoặc về Vùng An Toàn nghỉ ngơi.
* **Cơ Chế Hồi Mana:**
  - Chu kỳ ổn định: 3.0 giây / tick.
  - Pháp Sư hồi +4.0 Mana / 3s. Các hệ khác hồi +2.0 Mana / 3s.
* **Perk Bẩm Sinh Độc Nhất Theo Hệ (Permanent Inherent Class Perks):**
  - **Kị Sĩ:** [Trái Tim Thép] $\rightarrow$ +50 HP Max, +15 Giáp, Rút ngắn hồi máu xuống 30s (+2 HP).
  - **Pháp Sư:** [Học Giả Chân Kinh] $\rightarrow$ +40 Mana Max, +20 Công Phép, Hồi Mana +4 MP / 3s.
  - **Võ Sĩ:** [Bản Năng Dã Thú] $\rightarrow$ +8% Bạo Kích, +12 ST Đòn Đánh Thường, Hồi máu 35s (+2 HP).
  - **Xạ Thủ:** [Mắt Ưng Thần Tiễn] $\rightarrow$ +1.5m Tầm Bắn Thường (5.0m $\rightarrow$ 6.5m), +0.5m/s Tốc Chạy.

---

## 4.3. PHÂN NHÁNH KỸ NĂNG HỖ TRỢ, HỒI MÁU/MANA & BUFF ĐỒNG ĐỘI (SUPPORT & ALLY HEALING)

### 1. Ý Nghĩa Gameplay & Vị Thế Của Hệ Mộc (Nature / Wood Healer)
* Trong điều kiện hồi phục tự nhiên bị thắt chặt khắc khổ (40s mới hồi 1–2 HP), các kỹ năng chủ động hồi phục và tăng cường của **Hệ Mộc (Tự Nhiên / Wood)** và các chiêu thức hỗ trợ đóng vai trò then chốt trong mọi tổ đội săn quái, đi bí cảnh và công thành:
  - **Mưa Sinh Mệnh / Suối Nguồn Mộc Linh (Archetype 2 Ground AoE):** Khi thả vào một vùng đất (bán kính 2.5m, tầm 7.0m), những giọt mưa năng lượng xanh ngọc và lá rơi tưới mát vùng đất. Bất kỳ đồng đội nào bước vào sẽ được hồi phục liên tục `+HP` (chữ xanh lá) hoặc `+Mana` (chữ xanh dương).
  - **Thánh Quang Hào Quang (Archetype 4 Self Nova):** Bùng nổ hào quang 360 độ quanh người để hồi máu và dựng khiên chắn cho toàn bộ đồng đội xung quanh.
  - **Tia Sáng Trị Liệu (Archetype 1 Line):** Bắn luồng năng lượng thẳng xuyên thấu, đồng minh trên quỹ đạo được hồi máu tức thì.

### 2. 4 Loại Tác Dụng Chiêu Thức (`SkillCategory`)
* **0: Sát Thương (Damage):** Tác động kẻ thù, nảy số đỏ `-45` hoặc vàng `-90` (Crit).
* **1: Hồi Máu (Heal HP):** Tác động đồng đội/bản thân, nảy số xanh lá `+35 HP`.
* **2: Hồi Mana (Restore MP):** Tác động đồng đội/bản thân, nảy số xanh dương `+20 MP`.
* **3: Cường Hóa (Buff Stat):** Tác động đồng đội/bản thân, kích hoạt hiệu ứng chỉ số và HUD đếm ngược.

## 5. HOẠT ẢNH CHIẾN ĐẤU: STANDARD LEVEL VS CUSTOM ANIMATION IMPORT

### 5.1. Mức Chuẩn (Standard Level)
* Engine sử dụng các thuật toán hạt (Particle Systems / Procedural Line Renderers) để tính toán quỹ đạo, vùng nổ và hitbox va chạm theo 4 Archetype chuẩn.
* Tự động nhuộm màu luồng năng lượng theo **1 trong 7 Thuộc Tính Hệ**:
  - Hỏa: Hạt lửa cam đỏ, tia tàn tro bay lên.
  - Băng: Tinh thể băng tuyết xanh nhạt, sương lạnh tỏa ra.
  - Lôi: Tia hồ quang tím vàng giật liên hồi.
  - Quang: Tia sáng hoàng kim rực rỡ, hạt bụi vàng.
  - Ám: Khói đen tím cuộn xoáy, bóng ma chập chờn.
  - Phong: Vệt khí động học trắng bạc lốc xoáy.
  - Mộc: Dây leo gai góc xanh lục, lá rơi bừng nở.

### 5.2. Mức Riêng Biệt (Custom Animation / Sprite Level)
* Khi nhà sáng tạo (Creator) muốn một chiêu thức có nét đồ họa hoàn toàn độc bản:
  - Cho phép gán **Sprite Đạn Bay Riêng (Projectile Sprite)** (ví dụ: hình quả cầu rồng lửa lượn sóng, hình thanh thánh kiếm giáng xuống...).
  - Cho phép gán **Sprite Bùng Nổ Riêng (Impact / Explosion Sprite Sheet)** (chuỗi frame hoạt ảnh nổ bùng).
* Khi kích hoạt, chiêu thức sẽ hiển thị Sprite độc bản này thay thế cho particle mặc định, mang lại cảm giác sống động, mãn nhãn và không hề trùng lặp!

---

## 6. ENGINE XÂY DỰNG CUỘN TRỤC & TÍNH NĂNG NHẬP ẢNH ĐỒ HỌA NGOẠI BỘ (EXTERNAL ASSET IMPORT)

Nhằm mục tiêu biến *Godless Fantasy* thành nền tảng sáng tạo linh hoạt (No-Code & Modder-Friendly):

### 6.1. Giao Diện Thiết Kế Kỹ Năng (In-Game Studio [F1] & Unity Editor Studio)
Studio cung cấp hệ thống 3 cột trực quan:
1. **Cột 1: Phôi Mẫu (Presets):** Danh sách các kỹ năng chuẩn (Mưa Thiên Thạch, Bão Lửa Quạt, Bộc Phá Thánh Điển, Tiễn Băng...).
2. **Cột 2: Bảng Thiết Lập:**
   - Nhập Tên Chiêu, Mã ID.
   - Chọn **Chức Nghiệp Yêu Cầu:** Kị Sĩ / Pháp Sư / Võ Sĩ / Xạ Thủ / Toàn Hệ.
   - Chọn **1 trong 7 Thuộc Tính Hệ:** Ánh Sáng / Bóng Tối / Phong / Mộc / Băng / Hỏa / Lôi.
   - Chọn **1 trong 4 Archetype:** Bắn Thẳng / Vùng Đất / Hình Quạt / Tỏa Quanh Thân.
   - Cài đặt Mana, Hồi chiêu (s), Sát thương gốc, % Cộng hưởng INT, Tầm đánh (m).
   - **Chọn / Tải Icon Hotbar Độc Nhất:**
     - Chọn từ thư viện Icon có sẵn.
     - **Tính Năng Nhập Ảnh Ngoài (External Image Import):** Nhập đường dẫn file ảnh PNG trên máy tính hoặc kéo thả ảnh vào thư mục; Engine tự động nạp thành `Texture2D` và render thành Sprite ô chiêu độc nhất!
   - **Tự Động Tạo Icon Cuộn Trục Túi Đồ:** Tự động ghép Phôi Cuộn Trục + Ấn Ký của Hệ đã chọn!
3. **Cột 3: Live Preview 1:1 & Lưu Database:**
   - Giả lập hiển thị ô hotbar (Khung chuẩn + Icon độc nhất + Mana badge ở trên).
   - Giả lập Tooltip chi tiết (Hiển thị đầy đủ Chức nghiệp, Hệ, Chỉ số).
   - Nút `[TẠO VÀO TÚI ĐỒ]` $\rightarrow$ Xuất hiện ngay lập tức trong hành trang của người chơi để test.
   - Nút `[LƯU DATABASE (JSON)]` $\rightarrow$ Lưu vĩnh viễn vào file JSON cấu hình của game.

---

## 7. CẤU TRÚC DỮ LIỆU CHUẨN (DATA SCHEMA)

### 7.1. C# Class: `SkillScrollData`
```csharp
[Serializable]
public class SkillScrollData
{
    public string id;                      // Mã định danh độc nhất (VD: skill_meteor_flame)
    public string scrollName;              // Tên chiêu thức (VD: Mưa Thiên Thạch Oanh Tạc)
    public string requiredClass;           // Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ, Toàn Hệ
    public string element;                 // Quang, Ám, Phong, Mộc, Băng, Hỏa, Lôi
    public string tierRank;                // E, D, C, B, A, S, X
    public Color tierColor;                // Màu phẩm cấp
    
    // Chỉ số chiến đấu
    public int manaCost;                   // Mana tiêu hao
    public float cooldown;                 // Thời gian hồi chiêu (giây)
    public int baseDamage;                 // Sát thương gốc
    public float intelligenceScaling;      // Hệ số cộng hưởng INT
    
    // Cơ chế tầm đánh & cự ly
    public int indicatorType;              // 1=Line, 2=Ground AoE, 3=Cone, 4=Self Nova
    public float castRange;                // Tầm đánh (mét)
    
    // Đồ họa tách biệt
    public string uniqueHotbarIcon;        // Icon Độc Nhất trên thanh phím [1..4]
    public string scrollRuneIcon;          // Ấn ký của 1 trong 7 hệ trên mặt cuộn trục túi đồ
    public string customVfxSprite;         // (Tùy chọn) Đường dẫn sprite đạn bay/hoạt ảnh riêng
    
    public string description;             // Mô tả cốt truyện và hiệu ứng chiêu
}
```

### 7.2. Định Dạng Lưu Trữ JSON
```json
{
  "id": "skill_meteor_shower",
  "name": "Mưa Thiên Thạch Oanh Tạc",
  "requiredClass": "Pháp Sư",
  "element": "Hỏa",
  "tierRank": "Phẩm A",
  "indicatorType": 2,
  "castRange": 7.0,
  "manaCost": 25,
  "cooldown": 3.0,
  "baseDamage": 40,
  "intelligenceScaling": 1.2,
  "uniqueHotbarIcon": "icon_skill_meteor",
  "scrollRuneIcon": "rune_fire",
  "customVfxSprite": "vfx_meteor_rock",
  "description": "Triệu hồi loạt thiên thạch lửa giáng xuống vùng đất chỉ định gây nổ liên hoàn."
}
```

---

## 8. KẾT LUẬN & CAM KẾT KIẾN TRÚC
Tài liệu này xác lập quy chuẩn kỹ thuật chính thức và bất biến cho toàn bộ dự án *Godless Fantasy*. Mọi công cụ vẽ sprite, engine tạo chiêu và logic chiến đấu trong game đều phải tuân thủ nghiêm ngặt theo các nguyên tắc trên:
* **Túi đồ:** 1 Template cuộn trục + 7 Ấn ký hệ.
* **Hotbar:** 1 Khung viền kim loại chuẩn + Icon chiêu thức độc nhất được load riêng biệt.
* **Quy chuẩn điều kiện:** Kiểm tra chặt chẽ cả Chức Nghiệp (Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ) và 7 Thuộc Tính Nguyên Tố (Quang, Ám, Phong, Mộc, Băng, Hỏa, Lôi).
