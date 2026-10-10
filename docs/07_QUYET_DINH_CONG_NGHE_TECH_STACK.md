# TÀI LIỆU ĐẶC TẢ CÔNG NGHỆ (TECH STACK & ARCHITECTURE DECISION RECORD - ADR)
## DỰ ÁN: GODLESS FANTASY (2D TOP-DOWN MMORPG)
**Mã tài liệu:** DOC-07  
**Studio phát triển:** SquiRRels Studio  
**Phiên bản:** v1.1.0  

---

## 1. MỤC TIÊU CÔNG NGHỆ CỐT LÕI (TECH GOALS)
1. **Nhẹ & Mượt:** Chạy ổn định 60–120 FPS trên hầu hết các cấu hình máy tính phổ thông, dung lượng bộ cài gọn gàng, tải map không bị giật lag (Zero Hitch Streaming).
2. **Xịn & Đẹp (Modern Visuals):** Phong cách đồ họa Pixel Art 2D hiện đại kết hợp ánh sáng động (Dynamic 2D Lighting, Normal Maps, Bloom, Particles) tạo chiều sâu không gian thần thoại u tối.
3. **Multiplayer Chuẩn Server-Authoritative:** Server làm trọng tài tuyệt đối, chống gian lận (Anti-cheat), xử lý va chạm Friendly Fire và giao dịch nguyên tử (Atomic Transactions cho Rebirth Vault và Rơi đồ).
4. **AI Pair-Programming Thân Thiện:** Kiến trúc mã nguồn được module hóa rõ ràng để Antigravity AI có thể trực tiếp viết code, gỡ lỗi, kiểm thử và cùng anh xây dựng game từ A đến Z.

---

## 2. QUYẾT ĐỊNH CÔNG NGHỆ ĐỒ HỌA & ART PIPELINE ("NHẸ - XỊN - MƯỢT")

### 2.1. Phong Cách Đồ Họa: Modern HD Pixel Art + 2D Lighting
- **Quy chuẩn kích thước Tile:** **32x32 pixel** (Độ phân giải tiêu chuẩn vàng cho 2D Top-Down RPG: đủ chi tiết cho vũ khí, trang bị, biểu cảm nhân vật mà vẫn giữ được độ nhẹ của asset).
- **Công nghệ ánh sáng động (Dynamic 2D Lighting):**
  - Không dùng đồ họa pixel phẳng đơn điệu.
  - Sử dụng **2D Normal Mapping** và **2D Lights** (Ánh sáng từ ngọn đuốc, ma pháp nguyên tố Lửa đỏ rực, Băng xanh biếc, Bóng tối tím đen) tạo đổ bóng chân thực theo thời gian ngày/đêm.
- **Tối ưu hiệu năng hiển thị:**
  - **Sprite Atlas:** Gom toàn bộ icon trang bị, skill cuộn trục vào các tấm Atlas 2048x2048 để giảm số lượng Draw Call xuống tối thiểu (< 30 Draw Calls trên toàn màn hình).
  - **GPU Instancing:** Cho phép render hàng nghìn quái vật và hiệu ứng cây cối cùng lúc mà không tụt FPS.

### 2.2. Công Cụ Vẽ Chính Thức: Pixelorama
- **Công cụ được chọn:** **Pixelorama** (Phần mềm vẽ Pixel Art chuyên nghiệp, mã nguồn mở, miễn phí 100%).
- **Link tải phiên bản mới nhất:** [https://github.com/Orama-Interactive/Pixelorama/releases](https://github.com/Orama-Interactive/Pixelorama/releases)
- **Ưu điểm vượt trội:**
  - Dung lượng siêu nhẹ (~140MB), chạy ngay không cần cài đặt.
  - Hỗ trợ timeline animation, xuất Sprite Sheet tự động, bảng màu palette và layer phong phú, tương thích xuất thẳng sang Unity 2D.

---

## 3. QUYẾT ĐỊNH GAME ENGINE & HỆ THỐNG DATABASE

### 3.1. Game Engine: Unity 2D LTS (C#)
- **Engine chính:** **Unity 2D LTS (Long Term Support)** với **Universal Render Pipeline (URP 2D)**.
- **Lý do lựa chọn:**
  - Hệ sinh thái 2D mạnh mẽ nhất ngành công nghiệp: Tilemap Palette, 2D Lights, Sprite Shape, 2D Animation.
  - Ngôn ngữ C# hướng đối tượng mạnh mẽ, dễ viết code sạch và tối ưu hiệu năng.
  - Antigravity AI hỗ trợ 10/10: Có thể tự động viết toàn bộ script C#, PlayerController, StateMachine, UI HUD và Network Sync.

### 3.2. Database: SQLite 3 (All-In-One Architecture)
- **Database chính:** **SQLite 3**.
- **Lý do lựa chọn:**
  - **All-in-one & Serverless:** Không cần cài đặt các dịch vụ cơ sở dữ liệu cồng kềnh (như MySQL hay PostgreSQL), toàn bộ Database nằm gọn trong 1 file `.db` duy nhất.
  - **Tốc độ cực nhanh & An toàn tuyệt đối:** Hỗ trợ ACID Transactions, đảm bảo an toàn tuyệt đối cho cơ chế Rebirth Vault (lưu rương 2 món) và trừ đồ khi chết 5%.
  - **Dễ dàng sao lưu (Backup) & Di động:** Chỉ cần copy 1 file `.db` là có thể mang sang bất kỳ máy chủ nào.

---

## 4. KIẾN TRÚC MÃ NGUỒN CLIENT & SERVER (CODE ARCHITECTURE)

```
[ GODLESS FANTASY ARCHITECTURE ]
|
|-- CLIENT (Unity 2D / C#)
|   |-- Core/
|   |   |-- StateMachine/       -> FSM: Idle, Aiming, Casting, Dead
|   |   |-- Input/              -> Mouse Click-to-Move, WASD Override, Shift Dash (Cursor Vector)
|   |   |-- Movement/           -> Pathfinding A* (NavMesh2D / Grid)
|   |-- Combat/
|   |   |-- Targeting/          -> IndicatorRenderer (Directional Arrow, Ground AoE Circle)
|   |   |-- SkillManager/       -> Two-Tap Casting Controller (Press 1 Aim -> Press 1 Cast)
|   |-- UI/
|   |   |-- HUD/                -> Realtime Portrait, HP/Mana Bars, Level Label
|   |   |-- SlideMenu/          -> Equipment, Skills, Inventory (5 Protected Slots), Settings
|   |   |-- Chat/               -> Local (5s limit), World (2 msgs/day)
|   |-- Localization/           -> String Table: Tiếng Việt (Default), English
|
|-- WORLD SERVER (C# .NET Core / Headless Server)
|   |-- Simulation/             -> Tick-based Game Loop (20 - 30 ticks/sec)
|   |-- Validation/             -> Movement anti-speedhack, Combat Range Validator (At Key-Press 2)
|   |-- CombatEngine/           -> Friendly Fire Resolver, Damage Matrix, Safe Zone Guard
|   |-- Persistence/
|   |   |-- RebirthVaultManager -> Server-stored Vault at Death Coordinates
|   |   |-- InventoryService    -> 5 Protected Slots, 5% Loot Drop Calculator
|   |   |-- GachaEngine/        -> Perk Gacha & Random Seed Verification
|   |-- Database/               -> SQLite 3 (Lưu trữ SQLite All-in-One: Character & Vaults)
```

---

## 5. LỘ TRÌNH TRIỂN KHAI VERTICAL SLICE (ZERO TO PLAYABLE)

Không vội vàng làm ngay thế giới MMORPG khổng lồ, chiến lược phát triển bắt đầu bằng một **Bản Demo Lõi (Vertical Slice)** tinh gọn để hoàn thiện cảm giác chơi (Game Feel):

```
+-----------------------------------------------------------------------------------------+
|                  LỘ TRÌNH 5 BƯỚC VERTICAL SLICE (CORE COMBAT LOOP)                      |
+-----------------------------------------------------------------------------------------+
| [BƯỚC 1] Nhân vật di chuyển mượt mà:                                                    |
|          - Click Chuột Trái để di chuyển (Click-to-Move).                               |
|          - Phím W, A, S, D ghi đè ngay lập tức để né đòn thủ công.                      |
|                                                                                         |
| [BƯỚC 2] Kỹ năng Dash cơ động:                                                          |
|          - Nhấn Shift để lướt thẳng theo hướng con trỏ chuột (tiêu hao 20 Mana, 3.5s CD)|
|                                                                                         |
| [BƯỚC 3] Cơ chế ngắm & xả chiêu Two-Tap:                                                |
|          - Bấm phím 1 lần đầu: Bật chế độ ngắm (tâm ngắm lia theo chuột).               |
|          - Vừa chạy né đòn (Click Trái), tâm ngắm vẫn bám chuột liên tục.               |
|          - Bấm phím 1 lần nữa: Xác nhận thi triển đòn đánh.                             |
|          - Click Chuột Phải: Hủy ngắm ngay lập tức.                                     |
|                                                                                         |
| [BƯỚC 4] Combat cơ bản & Friendly Fire:                                                 |
|          - Đặt 2 nhân vật vào map (cùng nhóm hoặc khác nhóm).                           |
|          - Chưởng trúng nhau nhận 100% sát thương thật (Friendly Fire).                 |
|          - Đi vào vùng Safe Zone: Miễn nhiễm sát thương PvP.                            |
|                                                                                         |
| [BƯỚC 5] Kết nối Server Authoritative & Multiplayer thử nghiệm:                         |
|          - Server đồng bộ vị trí, kiểm tra tầm đánh, trừ mana và xác nhận trúng đòn.    |
|                                                                                         |
| => KHI VÒNG LẶP CHIẾN ĐẤU ĐÃ VUI & ỔN ĐỊNH: Mới mở rộng sang Balo, Boss, Rebirth & Map!  |
+-----------------------------------------------------------------------------------------+
```

---

## 6. HƯỚNG DẪN TẢI & CHỌN VỊ TRÍ CÀI ĐẶT CÔNG CỤ (DOWNLOAD & SETUP GUIDE)

### 6.1. Tải & Cài Đặt C# .NET SDK
- **Mục đích:** Biên dịch và chạy mã nguồn C#, Unit Test và Server Core.
- **Phiên bản khuyến nghị:** **.NET 8.0 SDK (LTS)**.
- **Link tải chính thức:** [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **Gói tải:** Chọn mục **"SDK x64"** cho Windows.
- **Vị trí cài đặt:** Bộ cài .NET rất nhẹ (~200MB), anh cứ bấm cài đặt theo vị trí mặc định (`C:\Program Files\dotnet`) để các công cụ và terminal tự động nhận diện PATH.

---

### 6.2. Tải & Cài Đặt Unity 2D LTS (Cách Chuyển Vị Trí Cài Đặt Sang Ổ D:\)

Để tránh làm đầy ổ `C:\`, anh thực hiện cài đặt Unity sang ổ `D:\` như sau:

#### Bước 1: Tải Unity Hub
- Link tải chính thức: [https://unity.com/download](https://unity.com/download)
- Cài đặt file `UnityHubSetup.exe` vào máy.

#### Bước 2: Cấu hình đổi ổ đĩa cài đặt sang ổ D:\ trong Unity Hub
1. Mở **Unity Hub**.
2. Bấm vào biểu tượng **Bánh răng (Preferences)** ở góc trên bên trái.
3. Chọn mục **Installs** trong thanh menu bên trái:
   - **Installs location:** Đổi đường dẫn thành: `D:\Unity\Editor`
   - **Downloads location:** Đổi đường dẫn thành: `D:\Unity\Downloads`
4. Bấm **Save**.

#### Bước 3: Tải phiên bản Unity 2D LTS
1. Trong Unity Hub, vào tab **Installs** $\rightarrow$ Bấm nút **Install Editor**.
2. Chọn phiên bản có nhãn **LTS (Long Term Support)**:
   - Khuyến nghị: **Unity 2022.3 LTS** (hoặc Unity 6 LTS).
   - *(Hoặc truy cập trang lưu trữ chính thức: [https://unity.com/releases/editor/archive](https://unity.com/releases/editor/archive) và bấm nút "Install with Unity Hub")*.
3. Ở bước chọn Modules đi kèm:
   - Tích chọn: **Microsoft Visual Studio Community** (hoặc tích Visual Studio Code).
   - Tích chọn: **Documentation** (tùy chọn).
4. Bấm **Install** và chờ Unity tải trực tiếp vào ổ `D:\`!

---

## 7. MÔI TRƯỜNG TRIỂN KHAI SERVER CLOUD (PRODUCTION HOSTING & PM2)

### 7.1. Cấu Hình Phần Cứng & Khả Năng Chịu Tải
- **Hạ tầng máy chủ:** Cloud VPS kiến trúc ARM64 hiện đại (ví dụ: Oracle Cloud Ampere A1 / AWS Graviton):
  - **Kiến trúc CPU:** **ARM64 (aarch64)** — Tiết kiệm điện, đa luồng hiệu năng cao, tối ưu hóa cực đỉnh trên .NET 8.
  - **Bộ nhớ RAM:** **12 GB** (Cực kỳ rộng rãi, thoải mái chạy game server và bộ đệm SQLite).
  - **Vi xử lý (CPU):** **2 Cores ARM64**.
- **Đánh giá hiệu năng:** .NET 8 hỗ trợ **native ARM64** với tập lệnh SIMD/NEON cực nhanh:
  - 1 Core phụ trách Simulation Loop & Combat Validation (20–30 ticks/s).
  - 1 Core phụ trách Network I/O (gói tin mạng) & SQLite Async Write.
  - Hạ tầng 2 Cores ARM64 / 12GB RAM gánh cực kỳ mượt mà từ **500 đến 1.000 người chơi đồng thời (CCU)** — hoàn hảo cho mọi đợt thử nghiệm Alpha/Beta của SquiRRels Studio!

### 7.2. Cấu Hình Cổng Mạng (Network Port)
- **Cổng dịch vụ game chính thức:** **Port 5454** (hỗ trợ cả TCP và UDP).
- **Cấu hình Bind IP:** `0.0.0.0:5454` (Lắng nghe toàn cục trên mọi card mạng của máy chủ Cloud).
- **Quy tắc Firewall (Inbound Rule):**
  - Mở port `5454` trên Cloud Provider Security Groups (Source: `0.0.0.0/0`).
  - Mở port qua UFW: `sudo ufw allow 5454/tcp && sudo ufw allow 5454/udp`.

### 7.3. Bộ Quản Lý Tiến Trình Tự Động Hồi Sinh (Process Manager): PM2
SquiRRels Studio chính thức lựa chọn **PM2** làm công cụ quản lý và duy trì hoạt động liên tục (24/7) cho Server CLI:

```bash
# 1. Biên dịch Server .NET 8 ra 1 file binary NATIVE cho chip ARM64 (linux-arm64)
dotnet publish -c Release -r linux-arm64 --self-contained -p:PublishSingleFile=true -o ./publish

# 2. Khởi chạy Server qua PM2 với cơ chế tự khởi động lại khi crash (delay 3s)
pm2 start ./publish/GodlessServer --name "godless-server" --restart-delay 3000

# 3. Thiết lập tự khởi động cùng hệ điều hành khi máy Cloud bị reboot
pm2 save
pm2 startup
```

**Các lệnh giám sát vận hành Server hằng ngày:**
- `pm2 status`: Xem trạng thái online, uptime, lượng RAM/CPU đang sử dụng.
- `pm2 logs godless-server`: Xem log console và sự kiện chiến đấu theo thời gian thực.
- `pm2 monit`: Mở dashboard giám sát trực quan tài nguyên CPU/RAM.
- `pm2 restart godless-server`: Khởi động lại server khi cập nhật bản vá mới.

