# TÀI LIỆU ĐẶC TẢ CÔNG NGHỆ (TECH STACK & ARCHITECTURE DECISION RECORD - ADR)
## DỰ ÁN: GODLESS FANTASY (2D TOP-DOWN MMORPG)
**Mã tài liệu:** DOC-07  
**Studio phát triển:** Squirrels Studio  
**Phiên bản:** v1.0.0  

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

### 2.2. Công Cụ Sản Xuất Asset Khuyến Nghị
- **Vẽ Pixel & Animation:** Aseprite (hoặc Photoshop / Krita).
- **Bộ Tileset thế giới:** Hỗ trợ Auto-tile 47-tiles (để vẽ đường đi, vách đá, sông suối tự động bo góc mượt mà).

---

## 3. SO SÁNH & QUYẾT ĐỊNH GAME ENGINE & CODE STACK

Vì anh đã 4 năm chưa dùng lại Unity, chúng ta có 2 phương án hàng đầu được cân nhắc:

| Tiêu chí | Phương án A: UNITY 2D (C#) [KHUYẾN NGHỊ HÀNG ĐẦU] | Phương án B: GODOT 4 (C#) [PHƯƠNG ÁN SIÊU NHẸ] |
| :--- | :--- | :--- |
| **Độ quen thuộc** | Anh đã từng có nền tảng Unity (dù 4 năm trước), em sẽ gánh toàn bộ code C# và hướng dẫn chi tiết từng nút bấm trên Editor mới. | Engine mới toanh, cần làm quen khái niệm Node/Scene của Godot. |
| **Dung lượng & Cài đặt** | Unity Hub + Editor (~4 - 6 GB). | **Siêu nhẹ: File chạy chỉ ~60MB**, giải nén là chạy ngay không cần cài đặt. |
| **Hệ thống 2D & Ánh sáng**| **URP 2D Renderer** cực xịn: ánh sáng 2D, shadow caster, bloom, post-processing có sẵn chuẩn công nghiệp. | 2D Engine thuần túy rất mượt, ánh sáng 2D ổn nhưng ít package hỗ trợ MMORPG hơn. |
| **Giải pháp Multiplayer** | Rất dồi dào: **Mirror Networking**, **Fish-Net**, hoặc **WebSocket/KCP tự viết**. Hàng nghìn tutorial và template MMORPG 2D. | Tự viết WebSocket / ENet hoặc dùng Nakama. |
| **Mức độ hỗ trợ của AI** | **Cực đại 10/10**: AI viết C# Unity siêu chuẩn xác, bao gồm PlayerController, Pathfinding NavMesh 2D, Inventory UI, Animator, Network Manager. | **Rất tốt 9/10**: AI viết C# Godot mượt mà. |

> 💡 **KẾT LUẬN ĐỀ XUẤT:** 
> - **Lựa chọn 1 (Toàn diện nhất):** Dùng **Unity 2D (C#)** phiên bản LTS (2022.3 LTS hoặc 2023 LTS) kết hợp **Universal Render Pipeline (URP 2D)**. Em sẽ viết toàn bộ code C# kiến trúc theo mô hình sạch (Clean Architecture / MVC), anh chỉ việc mở project lên và kéo thả prefab theo hướng dẫn.
> - **Lựa chọn 2 (Nếu máy anh muốn siêu nhẹ, không muốn cài Unity nặng):** Dùng **Godot 4 (C#)** — chạy ngay lập tức trong 30 giây.

---

## 4. KIẾN TRÚC MÃ NGUỒN CLIENT & SERVER (CODE ARCHITECTURE)

```
[ GODLESS FANTASY ARCHITECTURE ]
|
|-- CLIENT (Unity 2D / C#)
|   |-- Core/
|   |   |-- StateMachine/       -> FSM: Idle, Aiming, Casting, Dead
|   |   |-- Input/              -> Mouse Click-to-Move, WASD Override, Shift Dash
|   |   |-- Movement/           -> Pathfinding A* (NavMesh2D / Grid)
|   |-- Combat/
|   |   |-- Targeting/          -> IndicatorRenderer (Arrow, AoE Circle, Arc)
|   |   |-- SkillManager/       -> Two-Tap Casting Controller
|   |-- UI/
|   |   |-- HUD/                -> Realtime Portrait, HP/Mana Bars, Level Label
|   |   |-- SlideMenu/          -> Equipment, Skills, Inventory (5 Protected Slots), Settings
|   |   |-- Chat/               -> Local (5s limit), World (2 msgs/day)
|   |-- Localization/           -> String Table: Tiếng Việt (Default), English
|
|-- WORLD SERVER (C# .NET Core / Headless Server)
|   |-- Simulation/             -> Tick-based Game Loop (20 - 30 ticks/sec)
|   |-- Validation/             -> Movement anti-speedhack, Combat Range Validator
|   |-- CombatEngine/           -> Friendly Fire Resolver, Damage Matrix
|   |-- Persistence/
|   |   |-- RebirthVaultManager -> Server-stored Vault at Death Coordinates
|   |   |-- InventoryService    -> 5 Protected Slots, 5% Loot Drop Calculator
|   |   |-- GachaEngine/        -> Perk Gacha & Random Seed Verification
|   |-- Database/               -> PostgreSQL / SQLite (Lưu trữ nhân vật & rương)
```

---

## 5. HỖ TRỢ MCP (MODEL CONTEXT PROTOCOL) & TOOLING PHÁT TRIỂN

Để biến Antigravity thành trợ lý toàn năng pair-programming cùng anh, hệ thống MCP và công cụ tích hợp bao gồm:

1. **MCP File & Code Generator:**
   - Tự động sinh mã nguồn C# chuẩn theo ScriptableObject, Component, và Clean Architecture.
   - Viết sẵn các class dữ liệu JSON/ScriptableObject cho 7 phẩm cấp (X-E), 6 thuộc tính, 5 hệ.
2. **MCP Database & Server Inspector:**
   - Kiểm tra trực tiếp dữ liệu nhân vật, rương Rebirth Vault, lịch sử rơi đồ và chat log.
3. **Automated Unit Testing & Simulation:**
   - Viết các bài test logic độc lập (Test riêng công thức rơi đồ 5%, test Friendly Fire, test FSM Aiming) mà không cần phải bật giao diện game lên mới kiểm tra được.

---

## 6. LỘ TRÌNH BẮT ĐẦU CODE THỰC TẾ (ROADMAP: ZERO TO PLAYABLE)

- **Giai đoạn 1 (Khởi động dự án):**
  - Cài đặt công cụ nền tảng (Unity Hub / Editor hoặc Godot theo lựa chọn của anh).
  - Khởi tạo repo cấu trúc thư mục chuẩn.
- **Giai đoạn 2 (Vertical Slice Gameplay - Em trực tiếp viết code):**
  - Code Player Controller: Di chuyển Click-to-Move + WASD ghi đè.
  - Code Kỹ Năng: State Machine bấm 1 ngắm $\rightarrow$ di chuột $\rightarrow$ bấm 1 chưởng $\rightarrow$ chuột phải hủy.
  - Code Dash: Phím Shift lướt theo hướng chuột.
- **Giai đoạn 3 (UI & Hệ thống Dữ liệu):**
  - Dựng HUD, Menu 4 Tab, Inventory 24 ô + 5 ô khóa bảo vệ.
- **Giai đoạn 4 (Multiplayer & World Logic):**
  - Đồng bộ 2 người chơi qua Server, kiểm tra Friendly Fire chưởng lẫn nhau.
  - Rebirth Cấp 100 & Rương Rebirth Vault.
