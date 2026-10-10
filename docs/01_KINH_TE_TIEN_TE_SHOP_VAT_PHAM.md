# ĐẶC TẢ HỆ THỐNG: KINH TẾ, TIỀN TỆ, SHOP, HÀNH TRANG & VẬT PHẨM
**Mã tài liệu:** DOC-01  
**Dự án:** Godless Fantasy  
**Studio phát triển:** SquiRRels Studio  

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

### 2.3. Hệ Thống Giao Dịch Người Chơi (P2P Trading) & Lệnh `/trade`
- **Giao dịch nhanh qua lệnh Chat (Slash Command):**
  - Người chơi chỉ cần mở khung chat và gõ lệnh:
    ```
    /trade <Tên_Nhân_Vật>
    ```
    *(Ví dụ: `/trade Knight_01`, `/trade Shadow!`, `/trade Mage#99`)*.
  - **Quy tắc Rate-Limit Giao Dịch (Per-Target & Anti-Spam):**
    - **Cooldown theo mục tiêu (2 phút / 1 người):** Khi người chơi gửi lệnh `/trade <Tên>` đến người chơi B, nếu người B từ chối hoặc lời mời hết hạn (30 giây), người gửi phải đợi **2 PHÚT (120 GIÂY)** mới được phép gửi lại lời mời giao dịch cho **CÙNG NGƯỜI ĐÓ**.
    - **Mục đích:** Chống triệt để hành vi "Spam trade" khủng bố 1 người chơi cụ thể, chống làm phiền đối phương lúc đang ngắm chiêu hoặc né đòn.
    - **Cooldown toàn cục nhẹ (10 giây):** Giữa 2 lần gõ lệnh `/trade` tới 2 người khác nhau phải cách nhau tối thiểu 10 giây (chống bot spam chạy vòng lặp gửi trade hàng loạt cho cả làng).
  - **Tùy chọn Quyền Riêng Tư: Cài Đặt Tự Động Từ Chối Giao Dịch (Trade Privacy Setting):**
    - Trong mục Cài Đặt (Tab 5 Settings), người chơi có thể tích chọn: **`[x] Tự động từ chối mọi yêu cầu giao dịch (Auto-Decline Trade)`**.
    - Khi tính năng này được kích hoạt, bất kỳ người chơi nào gõ lệnh `/trade <Tên>` thì hệ thống sẽ tự động chặn và thông báo lại cho người gửi: *"Mục tiêu đang bật chế độ Từ Chối Giao Dịch!"*.
    - Màn hình của người nhận hoàn toàn không xuất hiện popup hay âm thanh thông báo nào, giúp người chơi yên tâm tập trung vào combat, săn boss hoặc tránh bị quấy rối lúc đang livestream.
  - Hệ thống tự động gửi hộp thoại mời giao dịch (Trade Request Popup) đến mục tiêu. Nếu đối phương bấm Đồng Ý, cửa sổ giao dịch 2 bên (Two-Way Trade Window) lập tức mở ra.

- **Phạm vi vật phẩm được phép đưa lên bàn giao dịch (Scope of Tradable Items):**
  - **CHỈ ĐƯỢC GIAO DỊCH VẬT PHẨM TRONG HÀNH TRANG TÚI ĐỒ (Bag Inventory Only):**
    - ❌ **Tuyệt đối không được giao dịch trang bị đang mặc trên người** (7 ô: Vũ khí, Mũ, Áo, Quần, Giày, Vòng cổ, Nhẫn). Người chơi bắt buộc phải tháo trang bị cất vào túi đồ trước mới có thể giao dịch.
    - ❌ **Tuyệt đối không giao dịch cuộn kỹ năng đang gắn trên 4 ô skill.**
    - ❌ **Tuyệt đối không giao dịch vật phẩm nằm trong các ô đã gắn ấn "Bảo Vệ Vĩnh Viễn"** hoặc vật phẩm đã bị gắn Khóa Linh Hồn (Soulbound).
    - ❌ **Tuyệt đối không giao dịch Thú Cưng (Pet) đang kích hoạt xuất chiến.**

- **Cơ Chế Kiểm Soát Lỗi Toàn Diện & Chống Gian Lận / Chống Dupe Đồ (Error Handling & Anti-Duping Matrix):**
  1. **Kiểm tra sức chứa túi đồ (Inventory Space Full Validation):**
     - Trước khi hoàn tất, Server tính toán: `Số ô trống túi người B >= Số vật phẩm người A chuyển giao` (và ngược lại).
     - Nếu túi đồ của bất kỳ bên nào không đủ chỗ chứa $\rightarrow$ Hệ thống lập tức hủy lệnh và hiện thông báo lỗi: *"Giao dịch thất bại: Hành trang của đối phương không đủ ô trống để nhận đồ!"*. Tuyệt đối không để đồ rớt ra sàn.
  2. **Cơ chế Hủy Khóa tự động khi sửa đổi (Auto-Unlock on Modify / Anti-Scam):**
     - Sau khi Người A bấm [Khóa Giao Dịch], nếu Người A tự ý thêm/bớt bất kỳ vật phẩm hoặc số lượng Vàng nào $\rightarrow$ **Trạng thái Khóa của CẢ HAI BÊN lập tức bị hủy bỏ (Reset về Chưa Khóa)**.
     - Điều này ngăn chặn 100% chiêu trò gian lận "tráo đồ rác" hoặc "rút bớt số 0 của vàng" vào giây cuối cùng.
  3. **Giao dịch nguyên tử trên Server (Atomic Database Transaction - Chống Dupe đồ):**
     - Toàn bộ thao tác chuyển giao tài sản được bọc trong một **SQLite Transaction duy nhất (ACID)** trên Server:
       ```sql
       BEGIN TRANSACTION;
       -- 1. Trừ vật phẩm & Vàng của Player A -> Cộng cho Player B
       -- 2. Trừ vật phẩm & Vàng của Player B -> Cộng cho Player A
       COMMIT; -- Hoặc ROLLBACK nếu có bất kỳ lỗi nào!
       ```
     - Nếu có sự cố đứt mạng đột ngột, crash game, hoặc lỗi ghi đĩa giữa chừng $\rightarrow$ Server tự động **Rollback 100%**. Vật phẩm của ai vẫn nằm nguyên vẹn trong túi người đó, triệt tiêu hoàn toàn nguy cơ nhân bản đồ (Dupe Item).
  4. **Kiểm tra trạng thái thời gian thực (Disconnection, Distance & Combat Break):**
     - Nếu một trong hai bên:
       - Di chuyển xa nhau quá 15m.
       - Bị ngắt kết nối mạng (Disconnect / Alt+F4).
       - Bị quái vật hoặc người chơi khác đánh trúng (dính Combat Mode).
       - Bấm nút [Hủy / Đóng Cửa Sổ].
       $\rightarrow$ Giao dịch lập tức tự động **HỦY BỎ AN TOÀN**, mọi vật phẩm trên bàn giao dịch được hoàn trả ngay lập tức về hành trang ban đầu.
  5. **Xác thực quyền sở hữu và số lượng vật phẩm (Item Ownership Verification):**
     - Tại thời điểm bấm Xác Nhận cuối cùng, Server kiểm tra lại ID vật phẩm và số lượng thực tế trong DB xem người chơi có thực sự sở hữu món đồ đó không (chống hack can thiệp client-side).


---

### 2.4. Quy Tắc Đặt Tên Nhân Vật Chuẩn Của Godless Fantasy (Naming Rules)
Để đảm bảo các câu lệnh Chat (như `/trade <Tên>`) hoạt động chính xác tuyệt đối mà không bị lỗi phân tích cú pháp, quy tắc đặt tên được quy định nghiêm ngặt:

1. **Tuyệt đối không chứa dấu cách (No Spaces):** Tên nhân vật phải viết liền (dùng dấu gạch dưới `_` thay thế dấu cách).
2. **Tên độc nhất toàn Server (Unique Name):** Không cho phép đặt trùng tên với bất kỳ người chơi nào khác trên toàn hệ thống database.
3. **Tập ký tự hợp lệ được phép sử dụng:**
   - Chữ cái: `a` – `z`, `A` – `Z`
   - Chữ số: `0` – `9`
   - Ký tự gạch dưới: `_`
   - Ký tự đặc biệt cho phép: `!`, `@`, `#`, `,`
   - *Biểu thức chính quy kiểm tra (Regex):* `^[a-zA-Z0-9_!@#,]{3,16}$`
4. **Độ dài tên:** Tối thiểu **3 ký tự**, tối đa **16 ký tự**.


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
> 1. **Ô Trang Bị (7 Equipment Slots):** **Vũ khí, Mũ, Áo, Quần, Giày, Vòng cổ, Nhẫn** (Ví dụ: Khóa vĩnh viễn ô Vũ khí để cầm vũ khí phẩm X không bao giờ sợ rớt).
> 2. **Ô Kỹ Năng (Active Skill Slots):** Ô 1, Ô 2, Ô 3, Ô 4 (Ví dụ: Khóa vĩnh viễn cuộn tuyệt kỹ phẩm S gắn ở Ô 1).
> 3. **Ô Hành Trang Túi Đồ (Bag Inventory Slots):** Bất kỳ ô nào trong balo (Ví dụ: Ô số 1, số 7, số 12...).
>
> *(Ví dụ cách phân bổ tự do: 1 Ô Vũ khí + 2 Ô Kỹ năng + 2 Ô Túi đồ = Đủ 5 Ô bảo vệ tối đa)*.

```
+-----------------------------------------------------------------------------------------+
|                GIAO DIỆN CHỌN VỊ TRÍ 5 Ô BẢO VỆ VĨNH VIỄN (TỰ DO LỰA CHỌN)              |
+-----------------------------------------------------------------------------------------+
| [PHÂN VÙNG TRANG BỊ (7 Ô)]  [PHÂN VÙNG KỸ NĂNG]            [PHÂN VÙNG HÀNH TRANG TÚI ĐỒ]|
|  [🛡️ Vũ Khí ]*               [✨ Ô Skill 1]*                 | 01 | 02 | 03*| 04 | 05 | 06|
|  [   Mũ     ]                [   Ô Skill 2 ]                 | 07 | 08 | 09 | 10*| 11 | 12|
|  [   Áo     ]                [   Ô Skill 3 ]                 | 13 | 14 | 15 | 16 | 17 | 18|
|  [   Quần   ]                [   Ô Skill 4 ]                 | 19 | 20 | 21 | 22 | 23 | 24|
|  [   Giày   ]                                                                           |
|  [   Vòng Cổ]                                                                           |
|  [   Nhẫn   ]                                                                           |
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

---

### 🔖 [MỤC NGOÀI LỀ 5.5] - HỆ THỐNG THÚ CƯNG (PET CUTE, KHÔNG CƯỠI, TĂNG CHỈ SỐ NHẸ)

#### 1. Triết Lý Thiết Kế Cốt Lõi:
> **PET SIÊU CUTE, NUÔI CHO VUI MẮT & TĂNG CHỈ SỐ HỖ TRỢ:**  
> - **Hình tượng:** Thiết kế theo phong cách Chibi / Cute Pixel Art cực kỳ đáng yêu, lon ton chạy theo sau lưng chủ nhân.
> - **Quy tắc 3 KHÔNG tuyệt đối:**
>   1. **KHÔNG CƯỠI ĐƯỢC (No Mount):** Giữ nguyên tốc độ di chuyển kiting, dash né đòn thuần túy của nhân vật.
>   2. **KHÔNG TẤN CÔNG ĐƯỢC (No Combat Attack):** Pet không cắn xé hay xả chiêu, tránh làm rối mắt màn hình và tránh bị dính sát thương oan từ Friendly Fire của đồng đội!
>   3. **KHÔNG BỊ QUÁI/ĐỐI THỦ ĐÁNH CHẾT (Untargetable):** Pet là thực thể bất tử, đi theo làm cảnh và buff chỉ số.

#### 2. Cơ Chế Ấp Trứng Random (Random Egg Hatching):
- Người chơi nhặt được Trứng Thú Cưng (Pet Egg) từ quái thường hoặc Boss (hoặc nhặt ngẫu nhiên trên bản đồ).
- Đem trứng ấp ngẫu nhiên: Hồi hộp chờ đợi trứng nở ra các loài pet đáng yêu với các bậc độ hiếm:
  - **Thường (Common):** Sóc Nâu Cute, Gà Con Lũn Cũn, Cún Vàng Vẫy Đuôi.
  - **Hiếm (Rare):** Mèo Mắt Hai Màu, Cáo Tuyết Đeo Nơ, Rùa Sỏi Mini.
  - **Sử Thi (Epic):** Cú Mèo Đeo Kính Trí Tuệ, Gấu Trúc Nhỏ Gặm Trúc.
  - **Huyền Thoại (Legendary / Mythic):** Sóc Hoàng Gia SquiRRel Đội Vương Miện, Rồng Con Tí Nị, Tinh Linh Lấp Lánh.

#### 3. Cơ Chế Tăng Chỉ Số Bị Động (Passive Stat Boost) & Perk Vui Vẻ:
- Mặc dù không trực tiếp đánh nhau, mỗi loài Pet sẽ **tăng nhẹ một chỉ số nhất định** cho người chơi khi được trang bị:
  - *Sóc Nâu / Cáo Tuyết:* Tăng +2% ~ +4% Tốc độ di chuyển.
  - *Rùa Sỏi Mini:* Tăng +10 ~ +25 Giáp cơ bản.
  - *Cú Mèo Trí Tuệ:* Tăng +30 ~ +60 Max Mana.
  - *Cún Lửa / Mèo Băng:* Tăng nhẹ +5 Sát thương thuộc tính tương ứng (Hỏa / Băng...).
  - *Perk vui vẻ:* Tự động nhặt tiền lẻ rơi quanh chân, đứng yên 5 giây thì pet nằm ngủ hoặc nhảy múa tạo nét.

#### 4. Bản Chất Của Pet & Ngoại Lệ Rebirth Duy Nhất:
- **Pet là một loại Vật Phẩm đặc biệt (Pet Item):**
  - Có thể trang bị (Summon) để xuất hiện chạy lon ton sau lưng, hoặc tháo ra cất gọn vào hành trang Túi Đồ.
  - **NGOẠI LỆ DUY NHẤT KHÔNG BỊ THANH TRỪNG BỞI REBIRTH:** Pet là loại vật phẩm duy nhất trong toàn bộ thế giới game **KHÔNG BAO GIỜ BỊ XÓA HAY RESET KHI CHUYỂN SINH (REBIRTH LV 100)**! Dù đang mang trên người, trong túi hay trong kho, Pet vẫn vĩnh viễn đồng hành sang kiếp sau.
  - **Bất tử tuyệt đối:** Pet không có thanh máu, không thể bị quái hay người chơi giết chết.
- **Quản lý khi sở hữu nhiều Pet:**
  - Có thể cất vào Rương cá nhân tại Làng.
  - Có thể bán cho NPC hoặc giao dịch cho người chơi khác qua lệnh `/trade` (chỉ khi Pet đã tháo ra cất trong hành trang).

#### 5. Cơ Chế Thả Pet Đi (Phóng Sinh) & Quy Trình Bảo Vệ 1 Phút Chống Bấm Nhầm:
- Người chơi có thể chọn **Thả Pet đi (Release Pet)** để nhận lại một ít Vàng an ủi (Consolation Gold).
- **Quy trình bảo vệ chống bấm nhầm 2 lớp cực kỳ an toàn (Anti-Accidental Security Flow):**
  1. **Bước 1 - Gõ văn bản xác thực:**
     - Hệ thống yêu cầu người chơi phải gõ chính xác từng ký tự:
       - Bản Tiếng Việt: **`TÔI XÁC NHẬN`**
       - Bản English: **`I CONFIRM`**
  2. **Bước 2 - Bắt buộc đợi thời gian đếm ngược ĐỦ 1 PHÚT (60 GIÂY):**
     - Sau khi gõ đúng cụm từ, hệ thống bắt đầu bộ đếm ngược 60 giây.
     - Trong suốt 60 giây này, nút **[HỦY PHÓNG SINH]** luôn hiển thị sáng rõ. Nếu người chơi đổi ý hoặc tiếc nuối, chỉ cần bấm Hủy là Pet lập tức an toàn trở lại túi đồ!
     - Chỉ khi hết trọn vẹn 60 giây mà không bấm Hủy, Pet mới chính thức được thả về tự nhiên và người chơi nhận được số Vàng an ủi tương ứng.




