# Thảo Luận Kỹ Thuật 13: Tổng Kiểm Tra Khoảng Trống (Master Gap Audit), Bản Đồ Thế Giới Siêu Mở Rộng Chunk Streaming, Màn Hình Tử Trận & Hệ Sinh Thái NPC Toàn Diện

**Mã tài liệu:** DISC-13  
**Dự án:** Godless Fantasy  
**Studio phát triển:** SquiRRels Studio  
**Trạng thái:** Kế hoạch tổng thể & Hướng dẫn kỹ thuật thực thi  

---

## 1. Bối cảnh & Mục tiêu kỹ thuật

Sau khi hoàn thiện giao diện HUD đáy màn hình (bao gồm Level Badge chạy chữ Marquee, Cánh mũi tên kim loại Pixel art trang trí 2 bên, Thước đo cự ly World Distance Ruler), dự án bước vào giai đoạn mở rộng quy mô gameplay thế giới mở (Open World MMORPG).

Mục tiêu của tài liệu thảo luận này là:
1. **Tổng hợp toàn bộ các khoảng trống (GAP Audit):** Đối chiếu giữa bộ tài liệu đặc tả chuẩn (`DOC-00` đến `DOC-08`, `GDD`) và mã nguồn thực tế trong client để không bỏ sót bất kỳ tính năng cốt lõi nào.
2. **Quy hoạch Bản đồ Thế giới "Cực kỳ rộng lớn (Vast/Infinite World Map)":** Chuyển đổi từ mô hình map phẳng thủ công sang **Kiến trúc Chunk Streaming 2D** (cho phép thế giới rộng từ hàng trăm mét đến hàng ngàn mét mà vẫn đảm bảo 60 FPS).
3. **Hiện thực hóa Màn hình Tử trận (Death Screen) & Cơ chế rơi đồ tự do:** Khi chết, màn hình đen ngòm tối tăm, có nút hồi sinh về làng, rủi ro rơi 50% đồ ngoài 5 ô bảo vệ vĩnh viễn.
4. **Cơ chế Rương Chuyển Sinh (Rebirth Vault):** Lưu giữ vĩnh viễn 2 món đồ trên Server tại vị trí Chuyển Sinh, cùng NPC Sứ Giả Chuyển Sinh đổi vật phẩm bằng điểm RP.
5. **Dựng Hệ sinh thái NPC chức năng:** Thương Nhân Shop, Thợ Rèn đập đồ, Phù Thủy độc dược, Trưởng Làng.
6. **Hệ thống Nhiệm vụ & Cốt truyện (Quest Tracker):** Bảng theo dõi mục tiêu nhiệm vụ thời gian thực.
7. **Hiệu ứng Môi trường (Lighting, Fog & Dungeons):** Ánh sáng đuốc bập bùng, sương mù ma mị, vùng tối và cổng hầm ngục phó bản.

---

## 2. Bảng ma trận tổng hợp các khoảng trống (Master Gap Matrix)

| STT | Phân hệ tính năng | Mã quy chiếu | Hiện trạng trong Code | Yêu cầu chuẩn theo Thiết kế | Giải pháp kỹ thuật triển khai | Ưu tiên |
|:---:|:---|:---:|:---|:---|:---|:---:|
| **1** | **Bản Đồ Thế Giới Mở Rộng** | `DOC-05` | Map tĩnh nhỏ `44x24` ô, chỉ có 3 tile đá/cỏ đơn điệu | Bản đồ cực kỳ rộng lớn (hàng trăm đến hàng ngàn mét), chia nhiều Biome: Làng Tân Thủ, Bình Nguyên, Rừng Rậm, Hẻm Núi | Chuyển sang **Chunk Streaming 2D** (32x32 tiles/chunk), sinh địa hình tự nhiên, nạp/gỡ động 3x3 chunks quanh Hero | **P0 (Cao nhất)** |
| **2** | **Màn Hình Chết & Rơi Đồ** | `DOC-04`, `DOC-02` | Máu về 0 chỉ đứng im hoặc hồi sinh trực tiếp | Màn hình tối đen ngòm (Blackout), thông báo tử trận, rơi tự do 50% đồ ngoài 5 ô bảo vệ, nút "Hồi Sinh Về Làng" | Tạo `DeathScreenModal.cs` + Logic `DropLootOnDeath()` trong `Health.cs` / `InventoryManager.cs` | **P0** |
| **3** | **Rương Chuyển Sinh (Rebirth Vault)** | `DOC-04` | Đã có Rebirth Point & Perks nhưng chưa có kho lưu đồ | Khi Rebirth, chọn đúng 2 món đồ lưu vĩnh viễn vào Rương Server đặt tại Đền Chuyển Sinh | Tạo `RebirthVaultManager.cs` + Đặt Prefab Rương Cổ tại Đền Chuyển Sinh Làng Tân Thủ | **P0** |
| **4** | **NPC Sứ Giả Đổi Điểm RP** | `DOC-04` | Chưa có NPC Shop đổi RP | NPC Sứ Giả Kaelen đứng cạnh Đền Thờ, mở Shop tiêu điểm RP mua Bí kíp ẩn, Perk hiếm, Danh hiệu | Tạo `RebirthEmissaryNPC.cs` + UI Modal Shop RP | **P1** |
| **5** | **Dàn NPC Chức Năng Đầy Đủ** | `DOC-01`, `DOC-05` | Mới có 1 NPC Trưởng làng thoại chữ đơn giản | Cần đủ: Thương Nhân (Shop Vàng), Thợ Rèn (Cường hóa), Phù Thủy (Chế thuốc), Trưởng Làng | Dựng các NPC với Sprite riêng + Modal giao dịch tương ứng | **P1** |
| **6** | **Hệ Thống Nhiệm Vụ (Quest Tracker)** | `DOC-05` | Chưa có hệ thống Quest | Bảng theo dõi Quest dưới Minimap (phím `[L]`), chuỗi 5 nhiệm vụ khởi đầu tân thủ | Xây dựng `QuestManager.cs` + `QuestTrackerHUD.cs` | **P1** |
| **7** | **Ánh Sáng 2D & Sương Mù** | `DOC-05`, `DOC-07` | Màu nền camera phẳng, không có ánh sáng/bóng tối | Đuốc bập bùng, quầng sáng Hero khi vào tối, sương mù lãng đãng trong rừng, chu kỳ ngày/đêm | Xây dựng `EnvironmentAtmosphereManager.cs` (Đã có phôi) | **P1** |
| **8** | **Hệ Thống Phó Bản (Dungeons)** | `DOC-05` | Mới có 2 portal dịch chuyển tọa độ phẳng | Bản đồ Hầm Ngục riêng biệt: 3 phòng ải khép kín, bẫy gai sàn, quái canh cửa, Boss Tàn Tích | Tạo Scene/Zone `DungeonInstanceController.cs` | **P2** |
| **9** | **Tổ Đội (Party HUD)** | `DOC-02` | Logic Friendly Fire đã có trong Combat nhưng thiếu UI | Khung danh sách thành viên Party, thanh máu/mana đồng đội | Dựng `PartyHUD.cs` góc trái màn hình | **P2** |

---

## 3. Kiến trúc chi tiết các trụ cột triển khai

### 3.1. Trụ Cột 1: Bản đồ Thế giới Siêu Rộng (Chunk Streaming Engine)
Để giải quyết bài toán "bản đồ quá nhỏ, phải cực kỳ rộng, rộng điên khùng", game không thể vẽ thủ công từng ô tĩnh. Kiến trúc Chunk Streaming được thiết kế như sau:

```
                                [ THẾ GIỚI MỞ TỔNG THỂ (600m x 400m HOẶC VÔ HẠN) ]
+---------------------------------------------------------------------------------------------------+
|  [ VÙNG 1: LÀNG TÂN THỦ ]     |  [ VÙNG 2: BÌNH NGUYÊN ]   |  [ VÙNG 3: RỪNG RẬM ]   | [ VÙNG 4: HẺM NÚI ]|
|  - Tọa độ X: -300 đến -100    |  - Tọa độ X: -100 đến 0    |  - Tọa độ X: 0 đến 200  | - Tọa độ X: 200+   |
|  - Safe Zone an toàn 100%     |  - Quái Lv.1 - 3 tuần tra  |  - Sương mù, quái Elite | - PvP Đẫm máu      |
|  - Dàn NPC Shop, Thợ Rèn, Đền |  - Bù nhìn, bãi cỏ hoa     |  - Đầm lầy, cây cổ thụ  | - CỔNG PHÓ BẢN     |
+-------------------------------+----------------------------+-------------------------+--------------------+
                                               ^
                                               |  (Chỉ nạp 3x3 Chunk quanh Player)
                                               v
                                   [ CHUNK STREAMING 2D ]
                                   +-----+-----+-----+
                                   | C0  | C1  | C2  |
                                   +-----+-----+-----+
                                   | C3  |HERO | C5  |  <- Chỉ tốn cực ít bộ nhớ!
                                   +-----+-----+-----+     Đi tới đâu sinh tới đó!
                                   | C6  | C7  | C8  |
                                   +-----+-----+-----+
```

- **Đơn vị Chunk:** Mỗi Chunk gồm `32 x 32 tiles` (kích thước 32m x 32m).
- **Quy tắc Streaming:** Bán kính tầm nhìn 1 Chunk xung quanh Hero (vùng 3x3 chunks = 9 chunks kích hoạt cùng lúc). Khi người chơi bước qua ranh giới chunk, chunk phía sau tự động cất vào Object Pool, chunk phía trước được sinh ra tức thời.
- **Tích hợp Minimap:** Minimap tự động tính toán tọa độ tương đối theo vị trí nhân vật trong thế giới mở rộng, hỗ trợ click-to-move tầm xa.

---

### 3.2. Trụ Cột 2: Màn hình Tử trận (Death Screen) & Cơ chế Rơi Đồ Tự Do

```
+-----------------------------------------------------------------------------------------+
|                               [ MÀN HÌNH TỐI ĐEN KỊT - BLACKOUT ]                       |
|                                                                                         |
|                         ☠️ BẠN ĐÃ TỬ TRẬN TRÊN CHIẾN TRƯỜNG! ☠️                        |
|                                                                                         |
|   [!] RỦI RO CHIẾN TRANH: 50% Vật phẩm không được bảo vệ đã rơi tự do ra sàn đất!      |
|                                                                                         |
|   🛡️ 5 Ô BẢO VỆ VĨNH VIỄN: An toàn tuyệt đối (Không bị rơi).                           |
|   📦 CÁC TRANG BỊ ĐÃ RƠI RA SÀN:                                                       |
|      - [Kiếm Sắt Rỉ Sét] x1                                                             |
|      - [Cuộn Trục Cầu Lửa Sơ Cấp] x1                                                    |
|                                                                                         |
|                   +-----------------------------------------------+                     |
|                   |    [ HỒI SINH VỀ LÀNG AN TOÀN ] (Phím SPACE)  |                     |
|                   +-----------------------------------------------+                     |
|                                                                                         |
+-----------------------------------------------------------------------------------------+
```

1. **Điều kiện kích hoạt:** Khi `Health.CurrentHealth <= 0` trên nhân vật người chơi.
2. **Xử lý đồ đạc (PvP Loot Drop):**
   - Duyệt qua toàn bộ vật phẩm trong túi đồ (`InventoryManager`).
   - Bỏ qua các trang bị nằm trong **5 Ô Được Bảo Vệ Vĩnh Viễn (Permanent Protected Slots)**.
   - Các vật phẩm còn lại có tỉ lệ 50% bị văng ra đất (`DropScatterLoot()`) tạo thành bọc đồ nhặt tự do dưới chân vị trí tử trận.
3. **Hiệu ứng thị giác:** Màn hình chuyển sang đen tuyền phủ mờ (`alpha = 0.95`), khóa di chuyển và tương tác chiến đấu, chỉ để lại duy nhất nút Hồi Sinh.
4. **Hồi sinh:** Người chơi bấm nút hồi sinh -> Vị trí nhân vật được dịch chuyển về Đài Tế Làng Tân Thủ (`x = -35, y = 0`), hồi phục 100% HP và 100% Mana, gỡ bỏ màn hình đen.

---

### 3.3. Trụ Cột 3: Rương Chuyển Sinh (Rebirth Vault) & NPC Sứ Giả RP

```
+-----------------------------------------------------------------------------------------+
|                        🏛️ RƯƠNG CHUYỂN SINH (REBIRTH VAULT)                            |
|                                                                                         |
|  "Trước khi buông bỏ thân xác để luân hồi, hãy chọn đúng 2 bảo vật cất vào rương này.   |
|   Server sẽ bảo lưu vĩnh viễn để ngươi lấy lại sau khi tái sinh!"                       |
|                                                                                         |
|                 +-----------------------+     +-----------------------+                 |
|                 |    [ Ô BẢO VẬT 1 ]    |     |    [ Ô BẢO VẬT 2 ]    |                 |
|                 |  «Trượng Băng Cổ Đại» |     |   «Giáp Hộ Mệnh Cấp S»|                 |
|                 +-----------------------+     +-----------------------+                 |
|                                                                                         |
|                   +-----------------------------------------------+                     |
|                   |       [ XÁC NHẬN CHUYỂN SINH VÀ NIÊM PHONG ]  |                     |
|                   +-----------------------------------------------+                     |
+-----------------------------------------------------------------------------------------+
```

- **Vị trí Rương trong Thế giới:** Đặt cố định tại Đài Tế Đền Chuyển Sinh Làng Tân Thủ (`x = -30, y = 8`).
- **Lưu trữ Server/Local:** Dữ liệu 2 món bảo vật được lưu độc lập với tiến trình nhân vật (`RebirthVaultData.json`), đảm bảo khi cấp độ bị reset về Lv 1, người chơi vẫn có thể chạy lại mở rương và nhận lại 2 món này.
- **NPC Sứ Giả Chuyển Sinh Kaelen:** Đứng ngay bên cạnh Rương, cung cấp Shop đổi điểm **Rebirth Point (RP)**:
  - *Perk [Bất Tử Tâm Can]:* Giá 3 RP.
  - *Cuộn Chiêu Huyền Thoại [Hỏa Long Bộc Phá]:* Giá 5 RP.
  - *Đá Tẩy Điểm Thuộc Tính Vô Hạn:* Giá 1 RP.

---

### 3.4. Trụ Cột 4: Hệ sinh thái Dàn NPC Làng Tân Thủ (Village NPC Ecosystem)

Dựng đầy đủ 5 NPC cốt lõi của Làng Tân Thủ, mỗi NPC có chức năng riêng:

```
[ LÀNG TÂN THỦ (SAFE ZONE) ]
├── 1. Trưởng Làng Eldrin (-35, 2): Hướng dẫn khởi đầu, giao/trả nhiệm vụ cốt truyện chính.
├── 2. Thương Nhân Barnaby (-28, 4): Cửa hàng bán Thuốc Hồi Máu (Q), Hồi Mana (E), Cuộn sơ cấp, Mua ve chai quái rớt lấy Vàng.
├── 3. Thợ Rèn Thiết Binh Vulkan (-28, -4): Lò rèn cường hóa trang bị (+1 đến +10), đúc kiếm giáp, sửa độ bền.
├── 4. Phù Thủy Ma Dược Morgana (-20, 6): Nấu thuốc lắc tăng tốc độ, nước thánh giải nguyền, bán bùa dịch chuyển.
└── 5. Sứ Giả Chuyển Sinh Kaelen (-30, 8): Quản lý Chuyển sinh, Rương Rebirth Vault và Shop Rebirth Points.
```

Mỗi NPC đều có:
- **Tên & Danh hiệu trên đầu:** Overhead Status Bar (Ví dụ: `Thợ Rèn Vulkan [NPC 🔨]`).
- **Giao diện Modal riêng biệt:** Mở ra khi người chơi click chuột trái hoặc bấm phím `[F]` tương tác ở cự ly gần (< 2.5m).

---

### 3.5. Trụ Cột 5: Hệ thống Cốt truyện & Nhiệm vụ (Quest Tracker)

- **Vị trí hiển thị:** Widget Nhiệm Vụ nằm ở góc phải màn hình, ngay phía dưới Active Buffs.
- **Phím nóng:** Bấm `[L]` để mở Sổ Tay Nhiệm Vụ (Quest Log Modal) xem chi tiết phần thưởng EXP, Vàng và trang bị.
- **Chuỗi Nhiệm Vụ Tân Thủ Mở Đầu:**
  1. *[Nhiệm vụ 1: Lời Hiệu Triệu Tân Thủ]*: Gặp Trưởng Làng Eldrin -> Nhận 1 Cuộn Chiêu Cầu Lửa & 100 Vàng.
  2. *[Nhiệm vụ 2: Luyện Thao Võ Nghệ]*: Tấn công Bù Nhìn Gỗ trong làng 5 lần -> Thưởng 50 EXP.
  3. *[Nhiệm vụ 3: Mối Đe Dọa Cửa Làng]*: Tiêu diệt 3 Chiến Binh Quái tại cổng ranh giới -> Thưởng Kiếm Sắt Cấp 1.
  4. *[Nhiệm vụ 4: Thám Hiểm Hắc Ám]*: Vượt sông sang Rừng Rậm Hắc Ám tìm kiếm Cổng Phó Bản Cổ -> Thưởng Danh hiệu «Kẻ Thám Hiểm».

---

## 4. Lộ trình triển khai (Execution Phases)

```
[ GIAI ĐOẠN 1: BẢN ĐỒ KHỔNG LỒ & HIỆU ỨNG ] -> [ GIAI ĐOẠN 2: CHẾT RƠI ĐỒ & REBIRTH VAULT ] -> [ GIAI ĐOẠN 3: DÀN NPC & NHIỆM VỤ ]
```

### Giai đoạn 1: Bản đồ Siêu Rộng & Hiệu ứng Môi trường (P0)
- Mở rộng bản đồ bằng Engine Chunk Streaming / Grid quy mô lớn (90m x 44m đến 600m x 400m).
- Đặt các Props cảnh quan: Giếng làng, Cây xanh làng, Cây cổ thụ rừng, Hàng rào, Cầu đá qua sông, Cổng Hầm ngục.
- Tích hợp `EnvironmentAtmosphereManager.cs`: Đuốc bập bùng, Hào quang người chơi, Sương mù rừng rậm.
- Đồng bộ Minimap theo kích thước thế giới mới.

### Giai đoạn 2: Màn hình Tử trận & Rương Chuyển Sinh (P0)
- Xây dựng `DeathScreenModal.cs`: Màn hình đen tối tăm, thống kê vật phẩm rơi 50%, nút hồi sinh về làng.
- Xây dựng `RebirthVaultManager.cs`: Rương Server lưu 2 món bảo vật tại Đền Chuyển Sinh.

### Giai đoạn 3: Dàn NPC Shop, Thợ Rèn, Phù Thủy & Quest Tracker (P1)
- Tạo các Model NPC: Thương Nhân Barnaby, Thợ Rèn Vulkan, Phù Thủy Morgana, Sứ Giả Kaelen.
- Dựng UI Modal Shop mua bán bằng Vàng & Shop tiêu điểm RP.
- Tạo `QuestManager.cs` và widget `QuestTrackerHUD.cs` với chuỗi 4 nhiệm vụ tân thủ.
