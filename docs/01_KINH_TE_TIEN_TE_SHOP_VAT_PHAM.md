# ĐẶC TẢ HỆ THỐNG: KINH TẾ, TIỀN TỆ, SHOP, HÀNH TRANG & VẬT PHẨM
**Mã tài liệu:** DOC-01  
**Dự án:** Godless Fantasy  
**Studio phát triển:** Squirrels Studio  

---

## 1. HỆ THỐNG TIỀN TỆ (CURRENCIES)

Game vận hành với mô hình kinh tế 3 tầng, phân tách rõ ràng giữa tiền tệ tuần hoàn thông thường và tiền tệ endgame:

| Loại tiền tệ | Ký hiệu | Cách kiếm được | Công dụng chính | Tính chất khi chết / Rebirth |
| :--- | :---: | :--- | :--- | :--- |
| **Vàng (Gold / Coin)** | 🪙 | - Nhặt rơi từ quái, boss<br>- Bán vật phẩm cho NPC<br>- Giao dịch giữa người chơi<br>- Hoàn thành nhiệm vụ | - Mua vật phẩm tiêu hao (máu/mana/cuộn)<br>- Chi phí sửa chữa/chế tác tại Thợ Rèn<br>- Mở rộng ô túi đồ (Inventory Slots)<br>- Mở rộng rương làng (Storage) | - **Rơi 5%** số lượng mang theo nếu chết ngoài Safe Zone.<br>- **Bị xóa 100%** khi thực hiện Rebirth. |
| **Điểm Chuyển Sinh (Rebirth Points - RP)** | 💠 | - Đạt Cấp 100 và thực hiện Chuyển Sinh (Rebirth) thành công.<br>- Vượt qua Thánh Địa Hệ Ẩn lần 2+ (dành cho người đã là Hệ Ẩn: nhận +1 RP). | - Quay Perk Gacha (Kỹ năng bị động)<br>- Cường hóa vĩnh viễn 1 ô đồ (Tối đa 5 ô, tốn 1 RP/ô)<br>- Chuyển đổi 6 Thuộc tính nhân vật<br>- Kích hoạt / Đổi hệ Tuần Trang Hóa (Ngụy Trang Hệ Ẩn - 1 RP/lần) | - **Lưu vĩnh viễn trên Server**, không bao giờ mất khi chết hay Rebirth tiếp theo. |
| **Ma Thạch / Tinh Thể Boss (Boss Shards)** | 💎 | - Đánh bại Boss thế giới, Boss Dungeon | - Đổi các công thức chế tác trang bị hiếm<br>- Ghép vật phẩm đặc biệt tại NPC Ẩn | - Tính là vật phẩm trong túi, có nguy cơ rơi 5% khi chết ngoài Safe Zone nếu không đặt ở ô an toàn. |

---

## 2. HỆ THỐNG CỬA HÀNG (SHOP SYSTEM)

### 2.1. Cửa Hàng NPC Tiêu Chuẩn (Standard NPC Shop)
- Tọa lạc tại các Safe Zone (Làng Tân Thủ, Thành Trì, Trạm Dừng Chân).
- Bán các nhu yếu phẩm cơ bản: Bình máu, bình mana, cuộn về thành, cuộn giải trừ.
- Thu mua lại toàn bộ trang bị thừa, rác quái của người chơi với mức giá tiêu chuẩn (5-15% giá trị gốc).
- Có cơ chế **Buyback** (Mua lại đồ lỡ tay bán nhầm trong vòng 10 phút trước khi rời vùng map).

### 2.2. Rebirth Sanctum (Cửa Hàng / Đền Chuyển Sinh)
- Nơi duy nhất sử dụng điểm **Rebirth Points (RP)**:
  - **Quay Perk Gacha:** Tiêu tốn 1 RP / 1 lần quay (Ra ngẫu nhiên danh hiệu Perk và % chỉ số).
  - **Khóa Ô Bảo Vệ Vĩnh Viễn:** Tiêu tốn 1 RP / 1 ô (Tối đa mở 5 ô toàn tài khoản nhân vật).
  - **Chuyển Đổi Thuộc Tính:** Tiêu tốn 2 RP / 1 lần đổi nguyên tố đại diện.
  - **Tuần Trang Hóa Chức Nghiệp (Dành riêng Hệ Ẩn):** Tiêu tốn 1 RP / 1 lần thiết lập hoặc đổi hệ ngụy trang công khai ("Giả heo ăn hổ").

### 2.3. Hệ Thống Chợ / Giao Dịch Người Chơi (P2P Trading)
- Cho phép 2 người chơi đứng cạnh nhau mở cửa sổ trao đổi trực tiếp (Direct Trade).
- Có tính năng ký gửi hàng hóa tại NPC Đấu Giá Sư ở các Đại Đô Thành (thu phí sàn 5% Vàng).
- Quy tắc an toàn: Không cho phép giao dịch khi đang trong trạng thái Chiến Đấu (Combat Mode) hoặc ngoài Safe Zone nếu bị đánh ngắt quãng.

---

## 3. HỆ THỐNG NPC CHỨC NĂNG (NPC SYSTEM)

1. **NPC Trưởng Làng / Sứ Giả Chỉ Dẫn (Village Elder / Guide):**
   - Hướng dẫn tân thủ, giao chuỗi nhiệm vụ cốt truyện sơ khai, mở khóa bản đồ cơ sở.
2. **NPC Dược Sĩ / Nhà Giả Kim (Alchemist / Apothecary):**
   - Bán bình dược hồi phục HP/Mana các cấp.
   - Chế thuốc cao cấp từ thảo dược và nội tạng quái vật thu thập được.
3. **NPC Thợ Rèn (Blacksmith):**
   - Sửa chữa độ bền trang bị (nếu có áp dụng độ bền).
   - Tách trang bị thừa thành quặng ma thuật.
   - Nâng cấp chỉ số cơ bản của trang bị bằng vàng và quặng.
4. **NPC Thủ Kho (Storage Keeper / Banker):**
   - Quản lý Rương Đồ Cá Nhân đặt tại làng an toàn.
   - Cung cấp dịch vụ mở rộng ô rương làng.
5. **NPC Thương Nhân Lưu Động (Wandering Merchant):**
   - Xuất hiện ngẫu nhiên theo khung giờ tại các bìa rừng hoặc phế tích nguy hiểm ngoài Safe Zone.
   - Bán các nguyên liệu hiếm, cuộn trục kỹ năng ngẫu nhiên phẩm cấp C–B. Người chơi đến mua phải đối mặt nguy cơ bị PvP cướp đồ.

---

## 4. HỆ THỐNG HÀNH TRANG & CƠ CHẾ TỰ DO CHỌN 5 Ô BẢO VỆ VĨNH VIỄN

### 4.1. Cơ Chế Tự Do Chọn 5 Ô Cường Hóa Bảo Vệ Vĩnh Viễn
> **QUY TẮC CỐT LÕI:** Người chơi dùng **1 Rebirth Point (RP)** để cường hóa vĩnh viễn 1 ô bất kỳ. **Tổng cộng tối đa được chọn 5 ô trên toàn bộ nhân vật**.
> 
> Người chơi **HOÀN TOÀN TỰ DO LỰA CHỌN** vị trí gắn ấn bảo vệ trong cả 3 phân vùng:
> 1. **Ô Trang Bị (Equipment Slots):** Vũ khí, Mũ, Áo, Quần, Giày (Ví dụ: Khóa vĩnh viễn ô Vũ khí để cầm vũ khí phẩm X không bao giờ sợ rớt).
> 2. **Ô Kỹ Năng (Active Skill Slots):** Ô 1, Ô 2, Ô 3, Ô 4 (Ví dụ: Khóa vĩnh viễn cuộn tuyệt kỹ phẩm S gắn ở Ô 1).
> 3. **Ô Hành Trang Túi Đồ (Bag Inventory Slots):** Bất kỳ ô nào trong balo (Ví dụ: Ô số 1, số 7, số 12...).
>
> *(Ví dụ cách phân bổ tự do: 1 Ô Vũ khí + 2 Ô Kỹ năng + 2 Ô Túi đồ = Đủ 5 Ô bảo vệ tối đa)*.

```
+-----------------------------------------------------------------------------------------+
|                GIAO DIỆN CHỌN VỊ TRÍ 5 Ô BẢO VỆ VĨNH VIỄN (TỰ DO LỰA CHỌN)              |
+-----------------------------------------------------------------------------------------+
| [PHÂN VÙNG TRANG BỊ]        [PHÂN VÙNG KỸ NĂNG]            [PHÂN VÙNG HÀNH TRANG TÚI ĐỒ]|
|  [🛡️ Vũ Khí]*                [✨ Ô Skill 1]*                 | 01 | 02 | 03*| 04 | 05 | 06|
|  [  Mũ     ]                 [   Ô Skill 2 ]                 | 07 | 08 | 09 | 10*| 11 | 12|
|  [  Áo     ]                 [   Ô Skill 3 ]                 | 13 | 14 | 15 | 16 | 17 | 18|
|  [  Quần   ]                 [   Ô Skill 4 ]                 | 19 | 20 | 21 | 22 | 23 | 24|
|  [  Giày   ]                                                                            |
|                                                                                         |
|  * Ký hiệu [*]: Đã được người chơi CHỌN gắn ấn BẢO VỆ VĨNH VIỄN (Tổng: 5/5 Ô)           |
+-----------------------------------------------------------------------------------------+
```

### 4.2. Quy Cách Túi Đồ Cá Nhân (Bag Slots)
- **Số ô khởi đầu:** 24 ô (Lưới 4 hàng x 6 cột).
- **Giới hạn mở rộng tối đa:** 60 ô (10 hàng x 6 cột).
- **Cơ chế mở rộng ô chứa:**
  - Mở rộng theo từng hàng (6 ô/lần).
  - Điều kiện: Đạt Level yêu cầu + Tiêu tốn Vàng + Vật phẩm Túi da / Túi ma thuật.
- **Quy tắc xếp chồng:** Slot-based. Dược phẩm stack 20, Thảo dược/Rác stack 99, Trang bị/Cuộn trục chiếm 1 ô/món.

### 4.2. Rương Đồ Cá Nhân Tại Làng (Bank Storage)
- Nằm cố định tại NPC Thủ Kho ở các Safe Zone.
- Khi người chơi chết ngoài map, vật phẩm trong Rương Đồ Tại Làng **hoàn toàn an toàn 100%**, không bị rơi rụng.
- Số ô khởi đầu: 30 ô. Có thể mở rộng bằng Vàng tối đa lên tới 120 ô.
- Khi Rebirth (Chuyển Sinh Lv 100): Toàn bộ kho đồ tại làng cũng bị thanh trừng (ngoại trừ 2 món mang vào Rebirth Vault và các ô được bảo vệ).

---

## 5. 🔖 [MỤC THIẾT KẾ MỚI / NGOÀI LỀ]: DANH MỤC VẬT PHẨM BỔ TRỢ & TIÊU HAO

> **LƯU Ý:** Các mục dưới đây là phần thiết kế mở rộng được đánh dấu rõ ràng với ký hiệu **🔖 [MỤC NGOÀI LỀ]** để đội ngũ phát triển dễ dàng tra cứu, kiểm tra cân bằng và lập trình.

---

### 🔖 [MỤC NGOÀI LỀ 5.1] - NHÓM DƯỢC PHẨM HỒI PHỤC (POTIONS)
Do game có cơ chế đánh nhau khốc liệt và Mana là tài nguyên sống còn để dùng skill / dash, các bình hồi phục được chia thành:

| Tên vật phẩm | Phẩm cấp | Hiệu ứng | Thời gian hồi (Cooldown) | Cách sở hữu |
| :--- | :---: | :--- | :--- | :--- |
| **Bình Máu Nhỏ (Lesser HP Potion)** | E | Hồi ngay 150 HP | 10 giây | Mua NPC (50 Vàng) / Quái sơ cấp |
| **Bình Máu Vừa (Medium HP Potion)** | C | Hồi ngay 500 HP | 10 giây | Mua NPC (250 Vàng) / Quái Trung cấp |
| **Bình Máu Lớn (Greater HP Potion)** | A | Hồi 35% HP tối đa trong 5s | 15 giây | Chế tác Dược Sĩ / Quái Tinh Anh |
| **Bình Mana Nhỏ (Lesser Mana Potion)**| E | Hồi ngay 100 Mana | 8 giây | Mua NPC (60 Vàng) / Quái sơ cấp |
| **Bình Mana Vừa (Medium Mana Potion)**| C | Hồi ngay 350 Mana | 8 giây | Mua NPC (300 Vàng) / Quái Trung cấp |
| **Bình Mana Lớn (Greater Mana Potion)**| A | Hồi 40% Mana tối đa trong 5s | 12 giây | Chế tác Dược Sĩ / Boss Mini |
| **Dược Tinh Linh (Elixir of Balance)** | S | Hồi phục đồng thời 30% HP & 30% Mana | 25 giây | Rơi từ Boss Thế Giới / Hòm Hiếm |

---

### 🔖 [MỤC NGOÀI LỀ 5.2] - NHÓM CUỘN TRỤC TIỆN ÍCH (UTILITY SCROLLS)

| Tên vật phẩm | Phẩm cấp | Hiệu ứng chi tiết | Ghi chú an toàn PvP |
| :--- | :---: | :--- | :--- |
| **Cuộn Về Thành (Town Return Scroll)** | D | Dẫn kênh (Channeling) 4 giây để dịch chuyển về Safe Zone gần nhất. | Nếu nhận bất kỳ sát thương nào trong 4 giây dẫn kênh sẽ bị ngắt lập tức! Chống lạm dụng trốn PvP. |
| **Cuộn Thanh Tẩy (Cleanse Scroll)** | C | Hóa giải ngay lập tức các hiệu ứng khống chế cơ bản (Làm chậm, Choáng, Đốt cháy). | **Không thể giải trừ** hiệu ứng "Khóa Kỹ Năng / Cấm Mana" của Hệ Ẩn! |
| **Cuộn Giám Định (Identify Scroll)** | E | Dùng để giám định các trang bị chưa rõ nguồn gốc nhặt từ Boss/Dungeon để mở khóa dòng chỉ số ẩn ngẫu nhiên. | Bán đại trà tại NPC Cửa Hàng (100 Vàng). |
| **Đá Thu Hút Chiến Lợi Phẩm (Magnet Stone)** | B | Tự động hút các vật phẩm rớt dưới đất trong phạm vi 3m về túi trong vòng 3 phút. | Cực kỳ lợi hại trong các pha tranh cướp đồ boss khi party đánh quái! |

---

### 🔖 [MỤC NGOÀI LỀ 5.3] - NHÓM NGUYÊN LIỆU CHẾ TÁC & RÁC QUÁI (CRAFTING MATERIALS)
Dùng để bán lấy vàng nuôi nhân vật hoặc giao nộp cho Thợ Rèn để nâng cấp đồ:

1. **Rác Phổ Thông (Monster Drops - Bán Vàng):**
   - *Da Chó Sói, Nanh Lợn Rừng, Xương Khô Mục, Lông Quạ Đen...* (Giá trị từ 5 - 40 Vàng/món, xếp chồng tối đa 99).
2. **Quặng Khoáng Sản (Minerals - Chế Trang Bị):**
   - *Quặng Sắt (Iron Ore):* Chế tạo vũ khí phẩm cấp E–D.
   - *Hắc Thiết (Dark Iron):* Chế tạo trang bị phẩm cấp C–B.
   - *Tinh Thể Mithril:* Chế tạo trang bị phẩm cấp A–S.
   - *Kim Cương Hư Không (Void Diamond):* Nguyên liệu cực hiếm để đúc trang bị phẩm X.
3. **Mảnh Cuộn Kỹ Năng (Skill Scroll Fragments):**
   - Đánh quái thường có tỉ lệ rớt mảnh cuộn kỹ năng chưa hoàn chỉnh.
   - Gom đủ 10 mảnh cùng Hệ + Phẩm cấp đem tới NPC Đạo Sĩ để ghép thành 1 Cuộn Trục Kỹ Năng hoàn chỉnh ngẫu nhiên.

---

### 🔖 [MỤC NGOÀI LỀ 5.4] - VẬT PHẨM ĐẶC BIỆT CHUYỂN SINH & THUỘC TÍNH

1. **Đá Tinh Chất Nguyên Tố (Elemental Prism):**
   - Cho phép người chơi đổi màu hạt hiệu ứng kỹ năng sang 1 trong 6 thuộc tính khác mà không cần tẩy lại điểm gốc.
2. **Ngọc Khóa Linh Hồn (Soulbind Talisman):**
   - Vật phẩm tiêu hao một lần: Khi gắn vào 1 món đồ trong túi, món đồ đó sẽ được bảo vệ 1 lần duy nhất khỏi luật rơi đồ 5% khi tử vong. Sau khi kích hoạt bảo vệ thành công, Ngọc sẽ vỡ vụn.
