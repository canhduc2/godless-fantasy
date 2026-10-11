# BIÊN BẢN THẢO LUẬN 06: HỆ THỐNG BANG HỘI (GUILD), EXP & LEVEL, KỸ NĂNG SỐ 6 (TEAM BUFF AOE) & NÂNG CẤP COMBAT VFX

> **Dự án:** Godless Fantasy MMORPG (SquiRRels Studio)  
> **Tài liệu liên quan:** `02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md`, `03_HE_THONG_HE_THUOC_TINH_KY_NANG.md`, `06_GIAO_DIEN_UI_HUD_VA_CHAT.md`  
> **Người thực hiện:** Antigravity AI Pair Programmer & Lead Designer  
> **Ngày cập nhật:** 11/10/2026  

---

## 1. TỔNG QUAN YÊU CẦU & BỐI CẢNH THIẾT KẾ

Từ phiên thảo luận và phản hồi thực chiến của Lead Game Designer, phiên bản Alpha Slice cần chuẩn hóa và bổ sung 5 trụ cột then chốt:

1. **Chuẩn hóa Màu Sắc Thuộc Tính (Element Palette) & Basic Attack VFX Chức Nghiệp:**
   - Hệ Quang (Ánh Sáng) chuẩn xác là **TRẮNG BẠCH KIM** (`#f5f8ff` lấp lánh), không phải vàng kim (tránh nhầm với Lôi/Hào Quang Buff).
   - Kị Sĩ (Knight): Kiếm khí hình **vòng cung bán nguyệt (Crescent Arc)** sắc bén bay quét tới trước.
   - Võ Sĩ (Warrior): Chưởng xuất ra **chùm nắm đấm mở rộng hình vòng cung (Expanding Fist Arc)** bay dồn dập, diện tích lớn, rõ nét.
2. **Tối Ưu Giao Diện Active Buffs Widget:**
   - Đưa toàn bộ khung thẻ Buff (Active Buffs Card) sang **sát mép phải màn hình** (dưới Minimap), không che khuất khu vực di chuyển/chiến đấu bên trái.
3. **Kỹ Năng Số 6: Buff Diện Rộng Toàn Đội (AoE Team Buff 360°):**
   - Indicator 360 độ bán kính rộng (6.0m - 7.5m), hào quang màu vàng hoàng kim.
   - Áp dụng hiệu ứng buff cho bản thân và toàn bộ thành viên cùng Bang Hội (Guild) trong cùng bản đồ khi đứng trong phạm vi.
4. **Hệ Thống EXP & Cấp Độ (Leveling System) & UI Thanh Tiến Trình:**
   - Triển khai chỉ số EXP, MaxEXP, cơ chế nhận EXP khi diệt quái/boss.
   - Lên cấp: Hồi đầy HP/MP, cộng 5 điểm tiềm năng, hiệu ứng chữ nổi hào quang.
   - Bổ sung thanh tiến trình EXP ngay dưới dòng `HỆ / THUỘC TÍNH / LEVEL` trong Tab Trang Bị với chỉ số rõ ràng.
5. **Hệ Thống Bang Hội (Guild System) & Tính Năng Đồng Đội:**
   - Mở giao diện Quản Lý Bang Hội từ Tab Cài Đặt (hoặc phím tắt).
   - Tạo bang với chi phí vừa phải (ai cũng tạo được), danh sách bang và gia nhập bang.
   - Thành viên hiển thị nhãn Tên Bang `< Tên Bang >` trên cùng ở trên đầu nhân vật (trên cả Danh hiệu).
   - Hiển thị chấm màu đồng minh trên Minimap cho thành viên cùng bang trong cùng Map.
   - Vẫn cho phép PvP/Friendly-fire bình thường ngoài vùng an toàn.
   - Cơ chế Rời Bang có Double Check bảo vệ đếm ngược 60 giây (1 phút) mới mở khóa nút Rời Bang.
   - Xem Profile thành viên, trạng thái Online, Giao dịch (Trade).
   - Đánh phó bản/Boss chung: Chia tiền, chia EXP, cơ chế rơi vật phẩm tự do nhặt tại chỗ (tranh đồ).
   - Nhận Danh Hiệu (Title) chung khi cùng nhau vượt ải phó bản.
   - Bổ sung Tab Chat Bang Hội `[Bang]` vào khung Chat UI.

---

## 2. BẢNG MÀU CHUẨN 7 THUỘC TÍNH NGUYÊN TỐ (ELEMENT COLOR MATRIX)

| Nguyên Tố | Biểu Tượng | Tên Mã Màu | Giá Trị RGB | Mô Tả & Hiệu Ứng |
|---|---|---|---|---|
| **Hỏa** | 🔥 | Cam Đỏ Rực | `(1.00f, 0.32f, 0.05f)` | Lửa đỏ bốc khói, nhiệt lượng cao |
| **Băng** | 🧊 | Xanh Băng | `(0.20f, 0.85f, 1.00f)` | Băng tuyết hàn băng buốt giá |
| **Lôi** | ⚡ | Vàng Sấm | `(1.00f, 0.95f, 0.15f)` | Tia sét vàng chói gắt giật liên hồi |
| **Mộc** | 🌿 | Xanh Sinh Mệnh | `(0.15f, 0.92f, 0.30f)` | Xanh lục tươi hồi phục sinh lực tự nhiên |
| **Phong** | 💨 | Xanh Ngọc | `(0.20f, 0.92f, 0.85f)` | Gió lốc ngọc bích lướt nhanh thanh thoát |
| **Quang** | ✨ | **Trắng Bạch Kim** | `(0.96f, 0.98f, 1.00f)` | Ánh sáng thần thánh bạch kim rực rỡ, viền lấp lánh |
| **Ám** | 🌑 | Tím Ma Pháp | `(0.72f, 0.22f, 1.00f)` | Ma thuật bóng tối bí ẩn tà ác |

---

## 3. THIẾT KẾ CHI TIẾT CÁC TÍNH NĂNG MỚI

### 3.1. Basic Attack VFX Theo Chức Nghiệp
- **Kị Sĩ (Knight):** 
  - Khí tức vung kiếm phóng ra dạng **Vòng Cung Bán Nguyệt (Crescent Blade Aura)**.
  - Vòng cung có độ dày vuốt nhọn 2 đầu, xoay theo góc chém và hướng tới mục tiêu với vận tốc cao (22m/s).
- **Võ Sĩ (Warrior):**
  - Chưởng quyền phóng ra **Chùm Nắm Đấm (Fist Volley)** bung mở rộng theo vòng cung (Arc Fan spread: 35 độ).
  - 3 đến 4 nắm đấm quyền kình bay đồng loạt, vệt sóng chấn động rộng rãi, dễ quan sát từ xa.
- **Pháp Sư (Mage):** Cầu phép ma thuật tròn xoay, quỹ đạo parabol nhẹ có trail dài.
- **Xạ Thủ (Archer):** Mũi tên thanh mảnh, tốc độ 28m/s xuyên phá.

### 3.2. Kỹ Năng Số 6: Buff Diện Rộng Đồng Đội (Indicator Type 6)
- **Cơ chế:** Kích hoạt hào quang bảo hộ 360 độ quanh người caster với bán kính lớn (6.5m - 7.5m).
- **Màu sắc:** Vàng hoàng kim (`Color(1.0f, 0.85f, 0.2f)`).
- **Hiệu ứng:**
  - Nếu đi một mình: Tự kích hoạt buff cho bản thân.
  - Nếu có đồng đội cùng Bang Hội đứng trong phạm vi: Tất cả thành viên trong tầm đều được nhận hiệu ứng buff tương ứng (+20% Công/Thủ hoặc Hồi phục).
  - Hiển thị card đếm ngược trên widget Active Buffs của mỗi nhân vật được áp dụng.

### 3.3. Hệ Thống EXP & Cấp Độ (Level System)
- **Công thức EXP:** `MaxExp(Lv) = 100 * Lv^1.5` (Làm tròn hàng chục: Lv1 = 100, Lv2 = 280, Lv3 = 520, ...).
- **Thưởng khi diệt quái/boss:**
  - Quái thường: +35 EXP, 15 Vàng.
  - Quái Tinh Anh / Boss: +150 EXP, 80 Vàng.
  - Khi có đồng đội cùng bang trong map: EXP và Vàng được chia sẻ công bằng cho mọi thành viên gần đó.
- **Thanh EXP trên HUD:**
  - Vị trí: Đặt ngay bên dưới dòng `HỆ / THUỘC TÍNH / LEVEL` trong Tab Trang Bị.
  - Chiều cao 14px, nền xanh đen tối, fill màu xanh ngọc bích / tím hoàng gia, chữ hiển thị rõ ràng: `EXP: 45 / 100 (45.0%)`.

### 3.4. Hệ Thống Bang Hội (Guild System)
- **Tạo Bang:** Chi phí rẻ (200 Vàng) để bất kỳ người chơi nào cũng có thể lập bang.
- **Quản lý Bang:**
  - Danh sách thành viên: Tên, Cấp, Chức nghiệp, Trạng thái (Online/Offline), Map hiện tại.
  - Xem hồ sơ (Profile) của thành viên khác trong bang.
  - Nút Mời Giao Dịch (Trade).
  - Cơ chế Rời Bang an toàn: Đếm ngược bảo vệ 60s (1 phút) mới mở khóa nút Rời Bang.
- **Hiển thị In-Game:**
  - Label Tên Bang `< Tên Bang >` màu vàng kim trên đỉnh đầu nhân vật (trên danh hiệu).
  - Icon/Chấm màu đồng minh riêng biệt trên Minimap cho thành viên cùng bang.
  - Giữ nguyên Friendly-fire/PvP ngoài Safe Zone: Các thành viên vẫn có thể giao tranh so tài tự do.
  - Danh hiệu chung: Vượt ải / săn Boss cùng bang mở khóa danh hiệu `« Huynh Đệ Sát Cánh »`.
  - Cơ chế Rơi Đồ: Vật phẩm rơi tự do tại chỗ quái chết, người chơi tự do nhặt đồ và tranh đồ (Free-for-all Loot).
- **Kênh Chat Bang Hội:**
  - Bổ sung Tab `[🛡️ BANG HỘI]` trong hộp thoại Chat.
  - Tin nhắn gửi kèm tiền tố `[Bang] {Tên}: {Nội dung}` màu vàng kim/xanh lá.
