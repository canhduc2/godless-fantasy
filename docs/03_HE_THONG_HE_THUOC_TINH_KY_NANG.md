# ĐẶC TẢ HỆ THỐNG: 5 HỆ CHIẾN ĐẤU, 6 THUỘC TÍNH, TIẾN TRÌNH & KỸ NĂNG
**Mã tài liệu:** DOC-03  
**Dự án:** Godless Fantasy  
**Studio phát triển:** SquiRRels Studio  

---

## 1. NĂM HỆ CHIẾN ĐẤU (CLASS SYSTEM)

Thế giới game bao gồm 4 Hệ Khởi Đầu (chọn khi tạo nhân vật) và 1 Hệ Ẩn Độc Nhất (chuyển chức đặc biệt):

```
+-----------------------------------------------------------------+
|                   4 HỆ KHỞI ĐẦU (TẠO NHÂN VẬT)                   |
+-------------------+--------------------+------------------------+
| 1. KỊ SĨ (Knight) | 2. XẠ THỦ (Archer) | 3. PHÁP SƯ (Mage)      | 4. VÕ SĨ (Brawler)
| Vũ khí: Kiếm      | Vũ khí: Cung       | Vũ khí: Gậy ma thuật   | Vũ khí: Găng tay
| Phòng ngự, áp sát | Tầm xa, tốc độ cao | Phép diện rộng, khống chế| Đấm liên hoàn, combo
+-------------------+--------------------+------------------------+-------------------+
                                    |
                 (Tìm kiếm bí mật & sự kiện hiếm ở map)
                                    v
+-------------------------------------------------------------------------------------+
|                      5. HỆ ẨN: KẺ NGOẠI ĐẠO (THE HERETIC - CLASS ẨN)                 |
| - Kết hợp: PHÁP SƯ + 1 HỆ NGẪU NHIÊN KHÁC (Kị sĩ / Xạ thủ / Võ sĩ).                 |
| - Dùng song song cả vũ khí & cuộn kỹ năng của 2 hệ.                                |
| - Sở hữu hiệu ứng KHÓA KỸ NĂNG (Mana Silence) của Pháp Sư.                         |
| - Đánh đổi: GIẢM 30% TỔNG LƯỢNG MANA & Kỹ năng Pháp Sư ngốn nhiều Mana hơn!         |
+-------------------------------------------------------------------------------------+
```

### 1.1. Chi Tiết 4 Hệ Cơ Bản
1. **Kị Sĩ (Knight):**
   - **Vũ khí đặc trưng:** Kiếm (Một tay / Trọng kiếm) & Khiên.
   - **Lối chơi:** Chỉ số Máu và Giáp cơ bản cao nhất. Đóng vai trò tiên phong càn quét và che chắn trong giao tranh.
2. **Xạ Thủ (Archer):**
   - **Vũ khí đặc trưng:** Cung tên / Nỏ ma thuật.
   - **Lối chơi:** Tầm bắn xa nhất bản đồ, tốc độ di chuyển và tỉ lệ chí mạng tự nhiên cao. Cần giữ cự ly tốt và tránh bắn trúng đồng đội.
3. **Pháp Sư (Mage):**
   - **Vũ khí đặc trưng:** Gậy ma thuật (Staff / Wand).
   - **Lối chơi:** Sở hữu các chiêu thức diện rộng (AoE) hủy diệt, lượng Mana dồi dào. Cực kỳ nguy hiểm nhưng đòi hỏi cẩn trọng cao độ vì Friendly Fire.
4. **Võ Sĩ (Brawler / Fighter):**
   - **Vũ khí đặc trưng:** Găng tay chiến đấu (Gauntlets).
   - **Lối chơi:** Đánh cận chiến liên hoàn, áp sát chớp nhoáng, tỉ lệ choáng và phá thế cao.

---

### 1.2. Hệ Ẩn Độc Nhất: Kẻ Ngoại Đạo (The Heretic)
- **Phương thức sở hữu:** Không thể chọn lúc tạo tài khoản. Chỉ có thể đạt được thông qua cơ chế kích hoạt bí mật tại vùng map đặc biệt (Xem chi tiết tại DOC-05).
- **Cấu trúc chức nghiệp:** Là sự dung hợp dị biệt giữa **Pháp Sư** và **1 trong 3 hệ còn lại (được gacha ngẫu nhiên khi chuyển chức)**:
  - *Biến thể 1:* Pháp Sư + Kị Sĩ (Hiệp Sĩ Ma Đạo - Cầm Kiếm tung phép).
  - *Biến thể 2:* Pháp Sư + Xạ Thủ (Cung Thủ Ma Đạn - Bắn tên yểm bùa).
  - *Biến thể 3:* Pháp Sư + Võ Sĩ (Quyền Sư Cấm Thuật - Nắm đấm áp bùa cấm chú).

#### Hai Perk Bẩm Sinh Của Hệ Ẩn:
1. **Perk 1 - Kẻ Ngoại Đạo (The Heretic):**
   - Cho phép trang bị và sử dụng toàn bộ Cuộn Trục Kỹ Năng và Vũ Khí của Pháp Sư cùng với Hệ Phụ ngẫu nhiên đã nhận.
2. **Perk 2 - Kẻ Tội Lỗi (The Sinner):**
   - **Hiệu ứng đặc biệt:** Mọi kỹ năng Pháp Sư khi gây sát thương lên kẻ địch sẽ kích hoạt trạng thái **Khóa Ma Lực (Mana Lock / Silence)** trong **2.5 giây**. Kẻ địch bị khóa hoàn toàn khả năng bấm phím `1`, `2`, `3`, `4` và `Shift Dash`.
   - **Đánh đổi cân bằng (Penalties):**
     - **Mana tối đa toàn thân bị giảm cố định 30%** so với công thức chuẩn của nhân vật.
     - **Tiêu hao Mana của tất cả kỹ năng Pháp Sư tăng thêm 45%** (chi phí đắt đỏ để kích hoạt hiệu ứng cấm chiêu).

#### Ưu Thế Độc Quyền: Cơ Chế Tuần Trang Hóa ("Giả Heo Ăn Thịt Hổ") - Tiêu Hao RP:
- **Bản chất chiến thuật:** Nếu để lộ danh xưng "Hệ Ẩn" hay "Kẻ Ngoại Đạo", đối thủ sẽ lập tức cảnh giác đề phòng, kéo đồng đội đến vây bắt hoặc chủ động giữ khoảng cách né chiêu cấm mana. Do đó, Hệ Ẩn sở hữu cơ chế độc quyền **Tuần Trang Hóa (Disguise System)**:
  - Cho phép người chơi Hệ Ẩn **tự do chọn hiển thị công khai 1 hệ cơ bản** (Kị Sĩ, Xạ Thủ, Pháp Sư, hoặc Võ Sĩ) thay vì để lộ chữ "Hệ Ẩn" cho thiên hạ thấy!
  - **Chi phí kích hoạt:** **Tiêu tốn 1 Điểm Rebirth Point (1 RP)** cho mỗi lần thiết lập hoặc đổi lớp ngụy trang sang hệ khác tại giao diện nhân vật.
  - **Phạm vi đánh lừa thị giác:**
    - Thanh thông tin trên đầu nhân vật (Nameplate): Đối thủ nhìn vào chỉ thấy ví dụ `Pháp Sư · Hỏa · Lv. 45` hoặc `Kị Sĩ · Băng · Lv. 45`.
    - Giao diện soi thông tin mục tiêu (Inspect Profile) của người chơi khác.
    - Huy hiệu chức nghiệp hiển thị khi nhắn tin trên kênh Chat (Local/World).
  - **Lối chơi "Giả Heo Ăn Hổ":** Đánh lừa tâm lý, khiến đối phương chủ quan tưởng mình là "mồi ngon" dễ bắt nạt và lao vào áp sát... Đến khi người chơi Hệ Ẩn bất ngờ tung chiêu thức kết hợp của cả 2 hệ và áp hiệu ứng Khóa Mana (Silence) 2.5s thì đối thủ mới nhận ra đã rơi vào bẫy của Kẻ Ngoại Đạo!

#### Khả Năng Rebirth Sau Khi Trở Thành Hệ Ẩn:
- Nhân vật sau khi chuyển chức Hệ Ẩn sẽ bắt đầu hành trình từ Level 1.
- **KHI ĐẠT LEVEL 100, NHÂN VẬT HỆ ẨN VẪN THỰC HIỆN REBIRTH (CHUYỂN SINH) NHƯ BÌNH THƯỜNG!**
  - Giữ nguyên vĩnh viễn chức nghiệp Hệ Ẩn cùng 2 Perk bẩm sinh `[Kẻ Ngoại Đạo]` & `[Kẻ Tội Lỗi]`.
  - Nhận điểm Rebirth Point (RP) bình thường để quay Perk gacha và cường hóa chỉ số.
  - Được chọn cất 2 món vào Rương Chuyển Sinh (Rebirth Vault) tại vị trí tử nạn.
  - Giữ nguyên 5 ô bảo vệ vĩnh viễn.

---

### 1.3. Giới Tính (Nam / Nữ) & Cơ Chế Ngoại Trang Cơ Bản (Base Body)
- **Tùy chọn giới tính:** Cả 4 Hệ Khởi Đầu và Hệ Ẩn đều có tùy chọn giới tính **Nam (Male)** hoặc **Nữ (Female)**.
- **Trạng thái "Không Mặc Gì" (Base Unarmored State):**
  - Khi nhân vật mới tạo hoặc khi tháo toàn bộ giáp/ngoại trang, nhân vật **KHÔNG** ở trạng thái khỏa thân mà luôn hiển thị mặc định:
    - **Áo ba lỗ + Quần cộc (Tank Top & Shorts)** áp dụng đồng nhất cho cả nhân vật Nam và Nữ.
  - **Kiến trúc Layer Ngoại Trang (Cosmetic Layering Architecture):**
    - `Layer 0 (Base Body):` Cơ thể nhân vật mặc áo ba lỗ + quần cộc.
    - `Layer 1 (Equipment / Armor):` Trang bị giáp, áo choàng, nón, găng tay phủ lên trên.
    - `Layer 2 (Cosmetics / Fashion):` Ngoại trang thời trang đè lên trang bị thật (nếu có).

---

## 2. SÁU THUỘC TÍNH NGUYÊN TỐ (ELEMENTS) & CƠ CHẾ RANDOM KHỞI ĐẦU

Mọi nguồn ma lực và kỹ năng trong thế giới đều mang 1 trong 6 thuộc tính:

```
[ ÁNH SÁNG ]    [ BÓNG TỐI ]    [ PHONG ]    [ MỘC ]    [ BĂNG ]    [ HỎA ]
  (Light)         (Dark)        (Wind)      (Nature)     (Ice)       (Fire)
   Vàng Kim        Tím Đen       Xanh Lơ     Xanh Lục   Xanh Băng    Đỏ Rực
```

### 2.1. Triết Lý Thiết Kế Đồng Đẳng (No Rock-Paper-Scissors):
- **Tuyệt đối không có bảng khắc chế hơn thua** (Không có Hỏa khắc Mộc hay Ánh Sáng khắc Bóng Tối làm tăng sát thương) để tránh tình trạng lệch meta và người chơi bị ép buộc đổi hệ.
- **Sự khác biệt cốt lõi:**
  - **Màu sắc và hiệu ứng thị giác (VFX / Animation):** Mỗi hệ tỏa ra khí sắc đặc trưng.
  - **Danh xưng và phong cách âm thanh:** Tạo bản sắc nhập vai riêng cho nhân vật.

### 2.2. Cơ Chế Thuộc Tính Khởi Đầu Ngẫu Nhiên & Đổi Thuộc Tính Bằng RP:
- **Thuộc tính khởi đầu hoàn toàn ngẫu nhiên (Random Starting Attributes):**
  - Khi nhân vật mới được tạo và giáng sinh vào thế giới, các thuộc tính nguyên tố và chỉ số tiềm năng cơ bản sẽ được **Random ngẫu nhiên**.
  - Không nhân vật nào sinh ra có xuất phát điểm rập khuôn giống hệt nhau, tạo nên tính độc bản cho mỗi cá nhân.
- **Cơ chế Tẩy / Đổi Thuộc Tính Bằng Rebirth Points (RP):**
  - Người chơi hoàn toàn có thể **thay đổi lại thuộc tính ngẫu nhiên này bằng cách tiêu hao điểm Rebirth Point (RP)**.
  - Đây là cơ chế cốt lõi để người chơi cày cuốc vòng lặp Chuyển Sinh, tích lũy RP để "tẩy luyện" (Re-roll) bộ thuộc tính và chỉ số hoàn mỹ nhất (Min-Max Build) theo phong cách chiến đấu của mình!


---

## 3. TIẾN TRÌNH CẤP ĐỘ 1–100 & 4 Ô KỸ NĂNG (PROGRESSION & SKILL SLOTS)

| Cấp độ | Cột mốc mở khóa | Trạng thái ô kỹ năng | Yêu cầu điều kiện |
| :---: | :--- | :--- | :--- |
| **Lv 1** | Khởi đầu game | **Mở khóa Ô Kỹ Năng 1** | Có sẵn phím `1` + Phím `Shift` (Dash) |
| **Lv 25** | Giai đoạn Sơ cấp | **Mở khóa Ô Kỹ Năng 2** | Mở thêm phím `2` |
| **Lv 50** | Giai đoạn Trung cấp | **Mở khóa Ô Kỹ Năng 3** | Mở thêm phím `3` |
| **Lv 75** | Giai đoạn Cao cấp | **Mở khóa Ô Kỹ Năng 4** | Mở thêm phím `4` (Đạt tối đa 4 ô kỹ năng chủ động) |
| **Lv 100** | Đỉnh cao tu luyện | **MỞ KHÓA CHUYỂN SINH (REBIRTH)** | Đạt điều kiện Chuyển Sinh về Cấp 1, nhận Rebirth Point |

### Quy Tắc Trang Bị Cuộn Trục Kỹ Năng (Skill Scroll Equip Rules):
- Game **không giới hạn cấp độ** để học cuộn kỹ năng. Nhân vật cấp 1 vẫn có thể nhặt được cuộn kỹ năng phẩm S nếu may mắn.
- **Tuy nhiên, điều kiện sử dụng cực kỳ nghiêm ngặt:**
  1. Cuộn trục phải trùng khớp với **Hệ** của nhân vật (Ví dụ: Kị sĩ chỉ học cuộn Kiếm thuật; trừ Hệ Ẩn được học 2 hệ).
  2. Cuộn trục phải tương thích với **Thuộc tính** của nhân vật.
  3. Nhân vật phải có **Đủ lượng Mana tối đa và HP tối thiểu** đáp ứng yêu cầu kích hoạt của cuộn trục. (Nếu cuộn trục đòi hỏi 600 Mana mà nhân vật mới có 200 Mana thì không thể kích hoạt xuất chiêu).

---

## 4. PHÂN CẤP PHẨM VẬT PHẨM & CUỘN KỸ NĂNG (TIER RANKS: X -> E)

Cả **Trang Bị** (Vũ khí, Mũ, Áo, Quần, Giày) và **Cuộn Trục Kỹ Năng** đều tuân theo hệ thống 7 phẩm cấp chuẩn:

```
[ E ] ---> [ D ] ---> [ C ] ---> [ B ] ---> [ A ] ---> [ S ] ---> [ X ]
Trắng     Lục       Lam      Tím        Cam       Đỏ      Hoàng Kim (Tối Thượng)
```

| Phẩm cấp | Màu sắc đại diện | Tỉ lệ nhặt cơ bản | Đặc điểm Trang Bị | Đặc điểm Cuộn Kỹ Năng |
| :---: | :---: | :---: | :--- | :--- |
| **E (Thường)** | Trắng (Common) | 60.0% | 0 dòng thuộc tính phụ, độ bền thấp. | Kỹ năng cơ bản, thời gian hồi ngắn, tiêu hao ít mana. |
| **D (Khá)** | Xanh Lục (Uncommon)| 25.0% | 1 dòng thuộc tính ngẫu nhiên. | Tầm tác dụng tăng nhẹ, sát thương trung bình. |
| **C (Hiếm)** | Xanh Lam (Rare) | 10.0% | 2 dòng thuộc tính ngẫu nhiên. | Có hiệu ứng làm chậm / đẩy lùi nhỏ. |
| **B (Ưu tú)** | Tím (Epic) | 4.0% | 3 dòng thuộc tính ngẫu nhiên. | Vùng ảnh hưởng AoE lớn, sát thương cao. |
| **A (Huyền thoại)**| Cam (Legendary)| 0.8% | 4 dòng thuộc tính cao cấp. | Kỹ năng có hiệu ứng choáng hoặc buff mạnh. |
| **S (Sử thi)** | Đỏ (Mythic) | 0.18% | 5 dòng thuộc tính + 1 hiệu ứng độc bản. | Chiêu thức diện rộng hủy diệt, tiêu hao mana lớn. |
| **X (Tối Thượng)** | Hoàng Kim (Godless)| 0.02% | Chỉ số tối đa tuyệt đối, hiệu ứng hào quang. | Tuyệt kỹ tối thượng, có thể xoay chuyển cục diện trận chiến. |

---

## 5. HỆ THỐNG CHỈ SỐ NHÂN VẬT (CHARACTER STATS)

Mỗi khi thăng cấp (Level Up), người chơi nhận được điểm tiềm năng để phân bổ vào **3 Chỉ Số Chính**:

### 5.1. Ba Chỉ Số Chính (Core Attributes)
1. **HP (Máu):** Thể lực và sức sống của nhân vật. Cạn HP = Tử vong $\rightarrow$ Kích hoạt hình phạt rơi đồ.
2. **Mana (Năng lượng / Xăng):** Nhiên liệu vận hành toàn bộ hành vi thi triển kỹ năng và phím Shift Dash. Hết Mana = Mất khả năng chiến đấu và trốn chạy.
3. **Attribute Power (Sức Mạnh Thuộc Tính):** Hệ số khuếch đại toàn bộ sát thương phép thuật và vật lý phát ra từ hệ và thuộc tính của nhân vật.

### 5.2. Ba Chỉ Số Phụ (Derived / Secondary Stats)
Được cộng thêm từ Trang bị, Perk, Danh hiệu (Title) hoặc Phẩm cấp vật phẩm:
1. **Tốc Độ Di Chuyển (Movement Speed):** Quyết định tốc độ chạy 4 hướng.
2. **Giáp (Armor / Defense):** Giảm trừ % sát thương vật lý và kháng ma lực nhận vào.
3. **Tỉ Lệ Chí Mạng (Critical Strike Chance):** Xác suất gây đòn đánh bạo kích (x150% - x200% sát thương).
