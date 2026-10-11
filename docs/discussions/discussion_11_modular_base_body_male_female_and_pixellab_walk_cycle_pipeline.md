# 📜 BIÊN BẢN THẢO LUẬN & BÁO CÁO KIẾN TRÚC KỸ THUẬT (SỐ 11)
**Dự án:** Godless Fantasy MMORPG | **Studio:** SquiRRels Studio (c) 2026  
**Chủ đề:** QUY HOẠCH PHÔI NHÂN VẬT GỐC (BASE BODY NAM/NỮ ANIME CUTE) VÀ PIPELINE TẠO HOẠT ẢNH BƯỚC ĐI 4-FRAME (PIXELLAB WALKING-4-FRAMES)  
**Ngày lập:** 12/10/2026 | **Phiên bản:** v1.0.0-PROPOSAL  
**Trạng thái:** [APPROVED ARCHITECTURE & IMPLEMENTATION ROADMAP]

---

## 🎯 1. BỐI CẢNH & TÁI ĐỊNH HÌNH BÀI TOÁN PAPERDOLL ENGINE

Theo nguyên lý kiến trúc đã thống nhất tại [Thảo luận 09](file:///d:/godless-fantasy/docs/discussions/discussion_09_modular_layered_character_pipeline_and_guild_operations.md), trò chơi **Godless Fantasy** xây dựng hệ thống đồ họa nhân vật theo cơ chế **Modular Layered Paperdoll (Đồ họa đa tầng tách rời)**:

### ⚠️ Sai lầm của cơ chế Monolithic Sprite:
* Nếu render ra một nhân vật dính cứng giáp sắt (ví dụ: `player_knight`), toàn bộ thân người, áo giáp, vũ khí bị gộp làm một file ảnh duy nhất.
* **Hậu quả:** Khi người chơi tháo giáp, thay đổi áo vải, mặc đồ pháp sư, nhặt được quần áo mới hoặc chuyển đổi ngoại trang thì **không thể tái sử dụng**, buộc phải vẽ lại từ đầu hàng nghìn sprite.

### 🌟 Kiến trúc Chuẩn: Phôi Gốc Tách Rời (Separated Base Body):
Hệ thống đồ họa nhân vật được chia thành 4 tầng hiển thị phân lớp (Sorting Order từ dưới lên trên):
1. **Tầng 0 - Phôi Thân Gốc (Base Body)**:
   * **Nam (`player_base_male`)**: Chàng trai trẻ trung, gương mặt anime sáng sủa, tóc ngắn gọn gàng, thể hình săn chắc, mặc **áo ba lỗ trắng + quần đùi thể thao xám**.
   * **Nữ (`player_base_female`)**: Cô gái trẻ trung, gương mặt anime xinh xắn dễ thương, tóc đuôi ngựa buộc cao năng động, mặc **áo ba lỗ croptop trắng + quần đùi thể thao ngắn**.
2. **Tầng 1 - Quần Dài / Giáp Chân (Pants Layer)**: Đè lên quần đùi của phôi gốc.
3. **Tầng 2 - Giáp Thân / Áo Choàng (Armor Layer)**: Đè lên áo ba lỗ của phôi gốc.
4. **Tầng 3 - Mũ Nón / Tóc (Helmet/Hair Layer)**: Che hoặc đè lên phần đầu.
5. **Tầng 4 - Vũ Khí (Weapon Layer)**: Độc lập xoay theo hướng trỏ chuột.

---

## 🏃 2. GIẢI PHÁP HOẠT ẢNH BƯỚC ĐI 4-FRAME (`walking-4-frames`)

### 2.1. Vấn đề hoạt ảnh bị đơ khi chỉ có 8 ảnh tĩnh
Nếu chỉ có 8 góc nhìn tĩnh của nhân vật (1 frame/hướng):
* Khi **Đứng yên (IDLE)**: Nhân vật bất động hoàn toàn.
* Khi **Di chuyển (WALK/RUN)**: Nhân vật trượt trên mặt cỏ (sliding/t-pose) mà không có nhịp bước chân.

### 2.2. Điểm ngọt PixelLab Template: `walking-4-frames`
Qua rà soát hệ thống API của PixelLab MCP, PixelLab cung cấp sẵn mẫu khung xương Humanoid chuẩn:
* **Template Animation ID:** `walking-4-frames` (Chu kỳ 4 frames bước chân rảo tự nhiên).
* **Cơ chế hoạt động:** Nhận diện phôi 8 hướng của nhân vật và sinh ra bộ 4 frame cho từng hướng di chuyển.
* **Chi phí:** Đúng **1 generation / 1 hướng di chuyển**.

### 2.3. Chiến thuật Đối Xứng Gương (Horizontal Mirroring - Tiết kiệm 40% chi phí)
Trong game 2D Top-Down / Isometric, trục ngang có tính đối xứng hình học hoàn hảo:
* Hướng Tây (`west`) = Lật trục X (`flipX`) của hướng Đông (`east`).
* Hướng Tây Nam (`south_west`) = Lật trục X của Đông Nam (`south_east`).
* Hướng Tây Bắc (`north_west`) = Lật trục X của Đông Bắc (`north_east`).

👉 **Do đó, chỉ cần render AI cho 5 Hướng Cốt Lõi:**
1. **Nam (`south`)** — Chạy thẳng xuống màn hình (chính diện).
2. **Đông Nam (`south_east`)** — Chạy chéo dưới phải.
3. **Đông (`east`)** — Chạy sang phải (ngang).
4. **Đông Bắc (`north_east`)** — Chạy chéo trên phải.
5. **Bắc (`north`)** — Chạy thẳng lên trên (sau lưng).

$\rightarrow$ Toàn bộ 3 hướng bên trái còn lại được Unity tự động xử lý bằng thuộc tính `spriteRenderer.flipX = true`, đảm bảo chuẩn pixel 100% mà không tốn thêm bất kỳ 1 generation nào!

---

## 💰 3. KẾ HOẠCH PHÂN BỔ NGÂN SÁCH QUOTA (12 / 39 GENERATIONS)

Tài khoản PixelLab hiện tại đang có **39 generations** khả dụng. Kế hoạch sử dụng ngân sách vàng được quy hoạch chi tiết:

| Bước thực hiện | Số lượng | Chi phí / Đơn vị | Tổng chi phí | Mô tả chi tiết |
| :--- | :---: | :---: | :---: | :--- |
| **BƯỚC 1: Phôi Nam 8 hướng** | 1 nhân vật | 1 gen | **1 gen** | Tạo `player_base_male` 8 hướng tĩnh (áo ba lỗ + quần đùi) |
| **BƯỚC 2: Phôi Nữ 8 hướng** | 1 nhân vật | 1 gen | **1 gen** | Tạo `player_base_female` 8 hướng tĩnh (croptop + quần đùi) |
| **BƯỚC 3: Walk Cycle Nam 4-frame** | 5 hướng chính | 1 gen / hướng | **5 gen** | 20 frames bước chân cho Nam (`walking-4-frames`) |
| **BƯỚC 4: Walk Cycle Nữ 4-frame** | 5 hướng chính | 1 gen / hướng | **5 gen** | 20 frames bước chân cho Nữ (`walking-4-frames`) |
| **TỔNG CHI PHÍ HOÀN THIỆN** | | | **12 gen** | **Đầy đủ 2 phôi Nam/Nữ + Hoạt ảnh bước đi 4 frames** |
| **SỐ DƯ QUOTA CÒN LẠI** | | | **27 gen** | **Dư dả 27 lượt (+5 lượt refill đêm) cho Quái vật & Map Tileset** |

---

## 📁 4. QUY CHUẨN ĐẶT TÊN & LƯU TRỮ ASSET TRONG UNITY

Toàn bộ file PNG sau khi render và cắt lát (slice) tự động từ tool sẽ được lưu vào:
* Thư mục Runtime: `client/GodlessFantasyClient/Assets/Resources/Characters/`
* Thư mục Prefab: `client/GodlessFantasyClient/Assets/Sprites/Characters/`

### 4.1. Quy tắc đặt tên tệp tin:
```text
player_base_[gender]_[direction]_[frame].png
```
* `[gender]`: `male` hoặc `female`.
* `[direction]`: `south`, `south_east`, `east`, `north_east`, `north`.
* `[frame]`: `0` (Idle/Base), `1` (Bước chân trái), `2` (Chạm đất trung gian), `3` (Bước chân phải).

### 4.2. Thông số Sprite Import chuẩn trong Unity:
* **Texture Type**: `Sprite (2D and UI)`
* **Sprite Mode**: `Single`
* **Pixels Per Unit (PPU)**: `64`
* **Filter Mode**: `Point (no filter)` (Bắt buộc cho Pixel Art giữ độ sắc nét)
* **Compression**: `None`
* **Pivot**: `Bottom-Center` hoặc `Custom (X: 0.5, Y: 0.15)` để chân nhân vật luôn cố định ở mặt đất khi chuyển hướng.

---

## 💻 5. TÍCH HỢP VÀO UNITY ENGINE (`CharacterVisualController8Dir.cs`)

Trong script Unity [CharacterVisualController8Dir.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Player/CharacterVisualController8Dir.cs), cỗ máy hoạt ảnh sẽ vận hành theo 3 chế độ đồng bộ:

```csharp
// Logic chuyển frame đi bộ:
if (isMoving)
{
    walkTimer += Time.deltaTime * walkSpeedMultiplier;
    int currentFrame = ((int)(walkTimer * 8)) % 4; // Lặp 4 frames (0 -> 1 -> 2 -> 3)
    
    // Nạp sprite tương ứng: player_base_male_south_1.png ...
    bodyRenderer.sprite = GetDirectionalFrame(currentGender, currentDirection, currentFrame);
    
    // Kết hợp dao động vật lý vi mô (Vertical Bob + Tilt) tăng độ mượt mà
    ApplyProceduralWalkBob();
}
else
{
    // Đứng yên (IDLE): Giữ frame 0 + chạy nhịp thở Sine wave (Squash & Stretch)
    bodyRenderer.sprite = GetDirectionalFrame(currentGender, currentDirection, 0);
    ApplyProceduralBreathingIdle();
}
```

---

## 🤖 6. PLAYBOOK HƯỚNG DẪN CHO AGENT CLI (`agy`) PHỐI HỢP THỰC THI

Để bất kỳ Agent nào (bao gồm **Antigravity CLI `agy`** chạy trong Terminal của anh) cũng có thể tự động hóa quy trình này mà không cần can thiệp thủ công:

### 6.1. Kiểm tra số dư tài khoản:
```powershell
python tools/ArtGenerators/pixellab_studio.py balance
```

### 6.2. Tạo 2 Phôi Nam & Nữ:
```powershell
# Tạo phôi Nam 8 hướng (tốn 1 gen):
python tools/ArtGenerators/pixellab_studio.py base_male

# Tạo phôi Nữ 8 hướng (tốn 1 gen):
python tools/ArtGenerators/pixellab_studio.py base_female
```

### 6.3. Tạo Hoạt ảnh Bước đi 4-frame:
```powershell
# Tạo Walk cycle 4-frame cho Nam (5 hướng, tốn 5 gen):
python tools/ArtGenerators/pixellab_studio.py walk_male

# Tạo Walk cycle 4-frame cho Nữ (5 hướng, tốn 5 gen):
python tools/ArtGenerators/pixellab_studio.py walk_female
```

### 6.4. Kiểm tra mã nguồn Unity đạt chuẩn:
```powershell
dotnet build client/GodlessFantasyClient/Assembly-CSharp.csproj
```

---

## 📅 7. LỘ TRÌNH TRIỂN KHAI (MILESTONES)

| Cột mốc | Mục tiêu cụ thể | Tiêu chí nghiệm thu (Acceptance Criteria) |
| :--- | :--- | :--- |
| **Milestone 1** (Hiện tại) | - Hoàn tất tài liệu kiến trúc kỹ thuật.<br>- Nâng cấp `pixellab_studio.py` hỗ trợ các lệnh sinh phôi Nam/Nữ và hoạt ảnh `walking-4-frames`. | Báo cáo đầy đủ, công cụ CLI sẵn sàng. |
| **Milestone 2** | - Chạy lệnh sinh phôi Nam & Nữ 8 hướng (`base_male`, `base_female`).<br>- Kiểm tra quota tiêu tốn đúng 2 generations. | 16 file PNG tĩnh đã nạp vào `Assets/Resources/`. |
| **Milestone 3** | - Sinh hoạt ảnh `walking-4-frames` cho 5 hướng chính của Nam & Nữ.<br>- Tải về, tự động cắt frame và đặt tên chuẩn. | 40 file animation frame được nạp vào Unity. |
| **Milestone 4** | - Cập nhật `CharacterVisualController8Dir.cs` để hỗ trợ chuyển đổi frame 4 bước đi + lật gương FlipX hướng Tây.<br>- Build Unity 0 lỗi. | Nhân vật di chuyển mượt mà, chân bước tự nhiên theo 8 hướng chuột! |

---
*Biên bản được lưu trữ vĩnh viễn trong hệ thống tài liệu dự án `docs/discussions/discussion_11_modular_base_body_male_female_and_pixellab_walk_cycle_pipeline.md`.*
