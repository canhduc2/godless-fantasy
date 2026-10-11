# BIÊN BẢN THẢO LUẬN 09: HOÀN THIỆN NGHIỆP VỤ BANG HỘI, BẢO VỆ TARGET QUÁI VẬT, VÀ BÁO CÁO KIẾN TRÚC HỆ THỐNG ĐỒ HỌA ĐA LỚP 8 HƯỚNG (MODULAR LAYERED CHARACTER PIPELINE)

> **Dự án:** Godless Fantasy MMORPG (SquiRRels Studio)  
> **Tài liệu tham chiếu:** `DOC-01_KINH_TE_TIEN_TE_SHOP_VAT_PHAM.md`, `DOC-02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md`, `DOC-06_GIAO_DIEN_UI_HUD_VA_CHAT.md`, `DOC-07_MONSTER_AI_BOSS_TELEGRAPH_VA_VFX_STUDIO.md`  
> **Người thực hiện:** Antigravity AI Pair Programmer & Lead Game Designer  
> **Ngày lập:** 11/10/2026  
> **Trạng thái:** Đã triển khai code & Kiểm thử thành công 100% (Build Succeeded - 0 Errors)

---

## PHẦN 1: BÁO CÁO CÁC TÍNH NĂNG VÀ FIX UI/UX ĐÃ HOÀN TẤT TRONG PHIÊN LÀM VIỆC

### 1.1. Nạp Ảnh Icon Ngoài (.PNG) Cho Form Tạo Vật Phẩm Trong Godless Studio
- **Vấn đề:** Ban đầu Studio Form 2 (Kỹ năng) đã có nạp ảnh ngoài, nhưng Form 1 (Vật phẩm) chỉ chọn được icon mẫu có sẵn.
- **Giải pháp:**
  - Thêm ô nhập đường dẫn ảnh `.png` và nút `[Nạp Ảnh]` vào Cột 2 (Bảng thiết kế thông số vật phẩm).
  - Tự động nạp byte data vào `Texture2D` runtime (Point filter sắc nét chuẩn Pixel Art), lưu vào cache nội bộ với ID `ext_item_<filename>`.
  - Đăng ký icon ngoại vi vào `CombatHUD.RegisterExternalIcon` để hiển thị đồng bộ từ Khung Xem Trước (Preview 1:1), Ô Hành Trang (Inventory Grid), Ô Trang Bị (Equipment Slots), cho đến Tooltip chi tiết.
  - Sửa toàn bộ các tham chiếu icon trong Studio sang `GetAllItemIcons()` với cơ chế clamp an toàn, biên dịch 0 lỗi.

### 1.2. Nâng Cấp Toàn Diện Nghiệp Vụ Bang Hội (Guild Operations)
Theo yêu cầu phản hồi từ Lead Game Designer:
1. **Bảo mật Gia Nhập Bằng Mã Code / Pass Bang:**
   - Mỗi Bang Hội sở hữu một mã `guildPassCode` riêng (mặc định Thần Long Hội là `6688`, Phượng Hoàng Cốc là `1234`, Bang mới tạo do Bang Chủ đặt).
   - Khi bấm **"GIA NHẬP"**, hệ thống không tự ý đưa người chơi vào bang ngay mà bật popup xác thực: `🔐 XÁC THỰC GIA NHẬP: [Tên Bang]`.
   - Người chơi phải nhập đúng Pass Code do Bang Chủ cung cấp mới được gia nhập. Nếu nhập sai, hệ thống báo đỏ: `"❌ Sai Mã Code / Pass Bang! (Mật khẩu không đúng)"`.
2. **Khẩu Hiệu Bang Hội (Slogan) Giới Hạn Nghiêm Ngặt Tối Đa 50 Ký Tự:**
   - Ràng buộc trực tiếp trong property `GuildData.slogan` (cắt tối đa 50 ký tự).
   - Ô nhập khẩu hiệu khi tạo bang và trong bảng cài đặt đều hiển thị bộ đếm ký tự thực tế `[X/50 ký tự]` và tự động cắt nếu dài hơn 50 ký tự.
3. **Bảng Cài Đặt Dành Riêng Cho Bang Chủ (Guild Leader Settings):**
   - Chỉ hiển thị nút vàng kim **`⚙️ CÀI ĐẶT BANG`** khi tài khoản đang đăng nhập là Bang Chủ (`IsPlayerLeader`).
   - Bảng thiết lập cho phép Bang Chủ:
     - Đổi Mã Code / Pass Bang gia nhập.
     - Đổi Khẩu Hiệu Bang Hội (tối đa 50 ký tự).
     - Bấm **`💾 LƯU THAY ĐỔI`** cập nhật tức thì vào dữ liệu bang và thông báo toàn server.
4. **Quyền Hạn Kích Thành Viên (Kick Member):**
   - Trong danh sách thành viên, Bang Chủ được cung cấp nút đỏ **`[KÍCH]`** bên cạnh từng thành viên (không hiển thị cạnh chính mình và không thể kích Bang Chủ).
   - Bấm `[KÍCH]` lập tức loại bỏ người chơi khỏi bang hội, thông báo nổi trên đầu.
5. **Cơ Chế Giải Tán Bang Bảo Vệ Đếm Ngược 60 Giây (1 Phút):**
   - Khi là Bang Chủ, nút đáy cửa sổ chuyển thành **`[GIẢI TÁN BANG]`**.
   - Bấm nút kích hoạt quy trình bảo vệ đếm ngược 60s an toàn (chống bấm nhầm / phá hoại bang): `⏳ BẢO VỆ: CHỜ 60.0s ĐỂ GIẢI TÁN BANG`.
   - Có nút **`[✕ HỦY]`** để hủy lệnh bất cứ lúc nào.
   - Khi đếm ngược về 0s, nút mở khóa chuyển sang màu đỏ: **`[⚠️ GIẢI TÁN]`**. Bấm xác nhận sẽ xóa hoàn toàn bang khỏi danh sách server và giải tán tất cả thành viên.
6. **Thanh Cuộn ScrollView Cho Danh Sách Bang Server:**
   - Danh sách bang hội server khi người chơi chưa có bang được bọc trong `GUI.BeginScrollView`, đảm bảo khi có hàng chục bang hội thì danh sách vẫn cuộn mượt mà, không bị tràn ra ngoài cửa sổ.
7. **Loại Bỏ Thanh Màu Vàng Thừa Thãi & Sửa Triệt Để Lỗi Chồng Chéo UI/UX:**
   - Xóa bỏ hoàn toàn thanh con lăn vàng vẽ đè (`trackRect`, `thumbRect`) trong danh sách thành viên, chuyển sang thanh cuộn chuẩn nguyên bản của Unity với độ rộng vừa vặn (`viewRect.width = scrollArea.width - 18f`), loại bỏ 100% thanh cuộn ngang gây lỗi hiển thị.
   - Sửa thanh đếm ngược bảo vệ ở đáy cửa sổ: mở rộng khung text lên `330px`, đẩy nút `[✕ HỦY]` (75px) và `[XÁC NHẬN]` (106px) về sát mép phải, xóa bỏ hoàn toàn hiện tượng chữ đè lên nút bấm.

### 1.3. Khắc Phục Triệt Để Tranh Chấp Click Chuột Giữa Quái Vật Và Vật Phẩm Rơi Dưới Đất
- **Vấn đề người chơi gặp phải:** Khi quái vật bị hạ gục rơi ra vật phẩm (ví dụ: Thịt Nướng Lửa Hồng) nằm ngay dưới chân quái, người chơi click chuột muốn chọn quái vật khác / hình nộm hồi sinh thì chuột lại kích hoạt nhặt đồ hoặc bị chặn bởi collider của vật phẩm.
- **Giải pháp xử lý:**
  1. **Tách biệt thao tác nhặt:** Loại bỏ `OnMouseDown()` khỏi `GroundLootItem`. Thao tác nhặt đồ được chuyển hoàn toàn sang phím **`[F]`** khi nhân vật đứng trong bán kính gần (`PICKUP_RANGE = 1.8m`), đúng chuẩn thao tác ARPG/MMORPG hiện đại.
  2. **Layer Raycast An Toàn:** Chuyển `layer` của GameObject vật phẩm rơi sang `Ignore Raycast` (Layer 2), chuột xuyên qua hoàn toàn để tới được hitbox quái vật.
  3. **Ưu Tiên Tuyệt Đối Quái Vật Khi Click Chuột:**
     - Trong `BasicAttackController.cs`: Thay thế `Physics2D.OverlapPoint` bằng `Physics2D.OverlapPointAll`. Quét toàn bộ collider dưới con trỏ chuột và ưu tiên chọn `MonsterTarget` / `IDamageable` / `Health`.
     - Trong `PlayerMovement.cs`: Đồng bộ `Physics2D.OverlapPointAll` để nhường quyền ưu tiên cho tấn công khi click trúng quái vật.
  4. **Sửa Lỗi Nhãn Văn Bản Bị Truncate / Xuống Dòng Đè Lên Quái Vật:**
     - Đặt `wordWrap = false` trong GUIStyle của nhãn nhặt đồ.
     - Rút gọn câu chữ khi hành trang đầy thành: `⚠️ ĐẦY TÚI: {itemName}`.
     - Tính toán độ rộng động theo độ dài chuỗi (`CalcSize + 14px`) và vẽ badge nền tối đổ bóng, không bao giờ bị rớt dòng hay che khuất mô hình mục tiêu.

---

## PHẦN 2: BÁO CÁO KIẾN TRÚC & KẾ HOẠCH HỆ THỐNG ĐỒ HỌA ĐA LỚP 8 HƯỚNG (MODULAR LAYERED CHARACTER ANIMATION PIPELINE)

### 2.1. Bản Chất Bài Toán & Vấn Nạn "Bùng Nổ Tổ Hợp" (Combinatorial Explosion)
Trong game 2D MMORPG góc nhìn Top-Down (như *Godless Fantasy*), người chơi có thể thay đổi ngoại trang:
- Giới tính: Nam, Nữ (2 bộ phôi cơ thể gốc).
- Hướng nhìn: 8 hướng (Nam - S, Tây Nam - SW, Tây - W, Tây Bắc - NW, Bắc - N, Đông Bắc - NE, Đông - E, Đông Nam - SE).
- Hoạt ảnh: Idle (đứng yên), Walk/Run (di chuyển), Basic Attack (chém/bắn/đánh), Dash (lướt), Hit (nhận sát thương).
- Trang bị: Áo (Armor), Quần (Pants), Mũ/Tóc (Helmet/Hair), Giày (Boots), Vũ khí (Weapon: Kiếm, Trượng, Cung, Song Đao...).

> **Cảnh báo nếu vẽ thủ công gộp thành 1 ảnh duy nhất:**  
> Nếu vẽ gộp nguyên cả người + giáp + vũ khí thành 1 spritesheet:  
> `2 Giới tính × 10 Bộ giáp × 10 Vũ khí × 8 Hướng × 5 Hoạt ảnh × 4-6 Frame = Hơn 48.000 Frame vẽ tay!`  
> Đây là điều bất khả thi về mặt nhân lực và chi phí sản xuất.

### 2.2. Kiến Trúc Chuẩn: Modular Layered Character Sprites (Ghép Lớp Động)
Thay vì vẽ ảnh gộp, hệ thống chia nhân vật thành **3 Lớp Vẽ Độc Lập (3 Modular Layers)** hoạt động cùng chung hệ trục tọa độ:

```
        +-------------------------------------------------------------+
        |                 UNITY CHARACTER COMPOSITOR                   |
        |              (Đồng bộ Frame, Hướng & Tọa độ)                |
        +-------------------------------------------------------------+
                                       ▲
         +-----------------------------+-----------------------------+
         |                             |                             |
+------------------+         +-------------------+         +-------------------+
| LỚP 1: BODY BASE |         | LỚP 2: OUTFIT/ARM |         | LỚP 3: WEAPON     |
| (Cơ thể gốc)     |         | (Giáp, Quần, Mũ)  |         | (Vũ khí cầm tay)  |
| - Nam / Nữ       |         | - Áo giáp đồng bộ |         | - Sorting động:   |
| - Mặc định:      |         | - Quần, giày      |         |   + Sau lưng khi  |
|   Áo balo/quần đùi|        | - Xuyên thấu màu  |             quay Bắc (N)    |
| - 8 Hướng nhìn   |         |   theo Tier phẩm  |         |   + Trước ngực khi|
| - Full Animation |         | - Khớp Pivot gốc  |             quay Nam (S)    |
+------------------+         +-------------------+         +-------------------+
```

#### Quy Tắc Sắp Xếp Thứ Tự Vẽ (Dynamic Sorting Layer Order):
Thứ tự vẽ các lớp phụ thuộc chặt chẽ vào **Hướng Nhìn (Facing Direction)**:
1. **Khi nhìn về phía Nam / Tây Nam / Đông Nam (Quay mặt xuống người chơi):**
   - Lớp 0: Bóng đổ dưới chân (Shadow).
   - Lớp 1: Thân gốc (Body Base).
   - Lớp 2: Quần + Áo giáp (Outfit/Armor).
   - Lớp 3: Đầu / Mũ (Helmet/Hair).
   - Lớp 4: **Vũ khí (Weapon Front)** — Cầm phía trước ngực và vung về phía trước.
2. **Khi nhìn về phía Bắc / Tây Bắc / Đông Bắc (Quay lưng lại người chơi):**
   - Lớp 0: Bóng đổ dưới chân (Shadow).
   - Lớp 1: **Vũ khí (Weapon Behind)** — Khiên/kiếm giấu phía sau lưng hoặc cán vũ khí khuất.
   - Lớp 2: Thân gốc (Body Base).
   - Lớp 3: Quần + Áo giáp (Outfit/Armor).
   - Lớp 4: Mũ / Tóc phía sau gáy.

---

### 2.3. Quy Chuẩn Kỹ Thuật (Asset Specification) Cho Mọi Lớp Đồ Họa
Để các lớp ghép vào khít 100%, không bị lệch đầu hay trôi vũ khí, toàn bộ asset phải tuân thủ chuẩn sau:
- **Kích thước Frame chuẩn (Grid Cell Size):** `48 × 48 px` hoặc `64 × 64 px`.
- **Điểm đặt chân (Pivot Point):** Cố định tại `(0.5, 0.15)` (tâm bàn chân chạm đất).
- **Quy chuẩn 8 hướng (Direction Naming Convention):**
  - `dir_0` (South - Nam - Quay mặt)
  - `dir_1` (SouthWest - Tây Nam)
  - `dir_2` (West - Tây)
  - `dir_3` (NorthWest - Tây Bắc)
  - `dir_4` (North - Bắc - Quay lưng)
  - `dir_5` (NorthEast - Đông Bắc)
  - `dir_6` (East - Đông)
  - `dir_7` (SouthEast - Đông Nam)
- **Timeline Đồng Bộ Frame (Animation Timing Sync):**
  - `Idle`: 4 frame (chu kỳ 0.8s)
  - `Walk/Run`: 6 frame (chu kỳ 0.6s)
  - `Attack`: 4 frame (chu kỳ 0.35s)
  - `Dash`: 3 frame (chu kỳ 0.25s)

---

### 2.4. Quy Trình Sản Xuất Asset Tự Động Hóa Với PixelLab API + Antigravity
Như tài liệu PixelLab API đã cung cấp:

```
[PixelLab API /create-character-v3]
                │
                ▼
 Sinh Base Body (Nam/Nữ) 8 hướng
                │
                ▼
[PixelLab API /characters/animations]
 Tạo chuỗi frame: Walk, Attack, Dash
                │
                ▼
[Pixelorama / Tinh chỉnh thủ công]
 Kiểm tra palette màu & điểm pixel viền
                │
                ▼
[Godless Character Studio Engine]
 Nạp file .png -> Tự động cắt lát (Auto-Slice) -> Gán Layer -> Chạy Preview 8 hướng
```

- **PixelLab API tích hợp an toàn:**
  - Token API lưu tại file cấu hình cục bộ không commit git (`config/pixellab_credentials.json`).
  - Hỗ trợ tạo prototype 1 nhân vật chuẩn trước khi tạo hàng loạt để tối ưu chi phí token API ($0.0133 - $0.041 / nhân vật).

---

### 2.5. Thiết Kế Công Cụ "Character & Outfit Studio Engine" Ngay Trong Game
Để biến công việc trở nên nhẹ nhàng: **"Chỉ cần vẽ và nhập liệu, không cần code tay"**, chúng tôi lên kế hoạch xây dựng **Godless Studio Tab 3: Thiết Kế Ngoại Trang & Nhân Vật Đa Lớp (Modular Outfit Studio)**:

#### Giao diện Studio bao gồm:
1. **Cột 1: Quản lý Thư viện Lớp (Layer Library):**
   - Danh mục: Thân gốc (Body), Mũ/Tóc (Head), Áo giáp (Chest), Quần (Pants), Vũ khí (Weapon).
   - Nút `[📂 Nạp Spritesheet .PNG]`: Chọn file spritesheet từ máy tính.
2. **Cột 2: Cấu hình Khung hình & Cắt lát tự động (Auto Slice & Pivot Tool):**
   - Chọn kích thước cell (`32x32`, `48x48`, `64x64`).
   - Tự động nhận diện 8 hướng và frame theo quy tắc đặt tên.
   - Thanh trượt điều chỉnh Pivot X/Y với chế độ xem lưới thời gian thực.
3. **Cột 3: Khung Giả Lập Ghép Lớp Trực Quan (Live 1:1 Layered Preview):**
   - Chọn Giới tính: Nam / Nữ.
   - Thử khoác đồ: Mặc thử Áo A + Cầm Kiếm B + Đội Mũ C.
   - Nút xoay 8 hướng: Bấm xoay xem nhân vật ở mọi góc 360 độ.
   - Nút đổi Hoạt ảnh: Xem thử dáng đứng (Idle), chạy (Walk), vung kiếm (Attack), lướt (Dash).
   - Nút `[💾 XUẤT DATABASE NGOẠI TRANG]`: Lưu cấu hình JSON để áp dụng ngay vào game runtime mà không cần build lại code C#!

---

## PHẦN 3: KẾ HOẠCH TRIỂN KHAI HOÀN THIỆN (ROADMAP & MILESTONES)

| Giai đoạn | Nội dung công việc | Đầu ra mong đợi |
| :--- | :--- | :--- |
| **Milestone 1** (Hiện tại) | - Hoàn tất toàn bộ bug fix UI/UX, nghiệp vụ Bang hội, nhặt đồ và target quái vật.<br>- Lập báo cáo kiến trúc chi tiết. | Build 0 lỗi, tài liệu hoàn chỉnh. |
| **Milestone 2** | - Xây dựng bộ Renderer `ModularCharacterRenderer.cs` (3 Sorting Layers).<br>- Đồng bộ controller 8 hướng di chuyển và animation state machine. | Nhân vật render đa lớp mượt mà trong Scene game. |
| **Milestone 3** | - Prototype đồ họa đầu tiên: 1 Body Nam, 1 Body Nữ, 1 Bộ Giáp Chiến Binh, 1 Bảo Kiếm.<br>- Tạo đủ 8 hướng Idle + Walk + Attack + Dash. | Nhân vật mặc giáp, cầm vũ khí chạy và đánh chuẩn xác không lệch frame. |
| **Milestone 4** | - Tích hợp Tab 3 vào `ItemCreatorModal` (No-Code Modular Character Studio).<br>- Hỗ trợ nạp PNG, auto-slice, preview 8 hướng, lưu JSON Database. | Hệ thống Engine nạp ngoại trang trực quan cho Designer. |

---
*Biên bản được lưu trữ vĩnh viễn trong hệ thống tài liệu dự án `docs/discussions/` của Godless Fantasy.*
