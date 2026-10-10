# BIÊN BẢN THẢO LUẬN & BÁO CÁO TIẾN ĐỘ DỰ ÁN SỐ 04
**Mã tài liệu:** `discussion_04_class_vs_element_lockout_title_and_pet_evolution.md`  
**Ngày ghi nhận:** 11/10/2026 (05:00 AM)  
**Chủ tọa / Game Director:** SquiRRels Studio  
**Trạng thái phiên họp:** Đã thống nhất & Triển khai toàn diện vào mã nguồn (Fully Implemented)  

---

## 1. MỤC TIÊU CUỘC THẢO LUẬN & ĐẶT VẤN ĐỀ
Trong phiên làm việc chuyên sâu sáng nay, Game Director (SquiRRels Studio) đã đưa ra các chỉ đạo chiến lược nhằm hoàn thiện tính tôn nghiêm của hệ thống Chức Nghiệp, Nguyên Tố và mở rộng các tính năng nhập vai MMORPG:
1. **Phân biệt triệt để "HỆ" (Class) vs "THUỘC TÍNH" (Element):**
   > *"Cuốn trục skill có các vấn đề là (hệ / thuộc tính 2 cái này là khác nhau rồi), thuộc tính là trên icon thể hiện, và kể cả thông tin chi tiết cũng hiện rõ, hệ là bắt buộc hiện :)) hệ nào chỉ dùng được cuộn hệ đó. Tương ứng (hệ -> hệ, thuộc tính -> thuộc tính), người sở hữu thuộc tính gì thì chỉ có thể phát huy skill của thuộc tính đó, hệ cũng như vậy."*
2. **Khắc phục lỗi hiển thị & Tối ưu tương phản giao diện Tab Kỹ Năng:**
   - Sửa triệt để lỗi IMGUI cắt đôi dòng chữ (clipping / auto-wrap ngang) trong bảng chỉ số chiến đấu.
   - Thay đổi huy hiệu Cấp Độ `Lv.1`: Nền đỏ cam rực rỡ, viền vàng hoàng kim, chữ trắng tinh in đậm để đạt độ tương phản 100%.
   - Xóa bỏ hoàn toàn phần ghi chú phụ `(BẨM SINH & TẬP SỰ)`, chỉ giữ lại tiêu đề chuẩn `DANH SÁCH PERK:`.
3. **Triển khai Hệ Thống Danh Hiệu (Title System) tại Tab Kỹ Năng:**
   - Bổ sung danh sách Danh Hiệu có thể mở khóa, cơ chế Đeo / Tháo danh hiệu ngay trên giao diện, cộng thưởng chỉ số trực tiếp và hiển thị tiền tố trước tên nhân vật trên HUD và trong thế giới game.
4. **Hệ Thống Cấp Độ, Tiến Hóa & Nuôi Dưỡng Thú Cưng (Pet Evolution):**
   - Thú cưng có cấp độ (1–50), thanh EXP và 3 giai đoạn tiến hóa trực quan (thay đổi kích thước to hơn, hiệu ứng lông sáng và hào quang hoàng kim).
   - Cơ chế cho thú cưng ăn Thức Ăn Thú Cưng (Hạt Dẻ Linh Hồn) để tăng EXP và kích hoạt tiến hóa.

---

## 2. QUY CHUẨN THIẾT KẾ CHI TIẾT

### 2.1. Phân Biệt Tuyệt Đối: HỆ (Class) vs THUỘC TÍNH (Element)
Game thiết lập 2 trục định danh độc lập và bắt buộc tương thích song song:

```mermaid
graph TD
    subgraph Trục 1: HỆ (Class / Chức Nghiệp)
        C1[Kị Sĩ - Knight]
        C2[Pháp Sư - Mage]
        C3[Võ Sĩ - Brawler]
        C4[Xạ Thủ - Archer]
        C0[Toàn Hệ - All Classes]
    end

    subgraph Trục 2: THUỘC TÍNH (Element / Ma Pháp Nguyên Tố)
        E1[Hỏa - Fire]
        E2[Băng - Ice]
        E3[Lôi - Lightning]
        E4[Quang / Ánh Sáng - Light]
        E5[Ám / Bóng Tối - Dark]
        E6[Phong - Wind]
        E7[Mộc - Wood / Nature]
        E0[Vô Thuộc Tính - None]
    end

    Player[Nhân Vật: Hệ Kị Sĩ · Thuộc Tính Hỏa]
    Player -->|Hệ -> Hệ| C1
    Player -->|Thuộc Tính -> Thuộc Tính| E1
    Player -.->|CẤM SỬ DỤNG| C2
    Player -.->|CẤM SỬ DỤNG| E2
```

#### Quy Tắc Khóa Kỹ Năng Bất Biến (Strict Lockout Law):
1. **Quy tắc Hệ (`Hệ -> Hệ`):**
   - Nhân vật thuộc Hệ nào chỉ được trang bị và thi triển cuộn trục thuộc Hệ đó hoặc cuộn trục `Toàn Hệ`.
   - *Ví dụ:* Kị Sĩ không thể dùng chiêu `[Hệ: Pháp Sư] Mưa Thiên Thạch`. Nếu cố tình lắp hoặc bấm phím, hệ thống sẽ chặn và báo: `⛔ Sai Hệ! Cuộn yêu cầu [Hệ Pháp Sư], bạn đang là [Hệ Kị Sĩ]`.
2. **Quy tắc Thuộc Tính (`Thuộc Tính -> Thuộc Tính`):**
   - Nhân vật mang Thuộc Tính nào chỉ có thể phát huy sức mạnh cuộn trục mang Thuộc Tính đó hoặc `Vô Thuộc Tính`.
   - *Ví dụ:* Kị Sĩ mang thuộc tính Hỏa không thể dùng `[Hệ Kị Sĩ · Băng] Băng Hàn Kiếm`. Hệ thống sẽ chặn và báo: `⛔ Khác Thuộc Tính! Cuộn mang [Thuộc tính Băng], bạn đang là [Thuộc tính Hỏa]`.
3. **Cơ chế chặn 2 lớp an toàn:**
   - **Lớp 1 (Tại Túi Đồ):** Ô cuộn trục không tương thích sẽ hiện viền đỏ cảnh báo và huy hiệu `[⛔]` trên icon. Bấm nút lắp vào ô chiêu sẽ bị chặn và hiện thông báo trên màn hình.
   - **Lớp 2 (Khi Bấm Phím Chiến Đấu 1–4):** Controller kiểm tra thời gian thực. Nếu không hợp lệ sẽ hủy thao tác ngắm, không trừ Mana và bắn chữ `⛔ Sai Hệ / Thuộc Tính!` màu đỏ lên đầu nhân vật.

---

### 2.2. Hệ Thống Danh Hiệu (Character Title System) Trong Tab Kỹ Năng
Hệ thống Danh Hiệu được tích hợp hoàn chỉnh tại Tab 3 [Kỹ Năng]:

| ID Danh Hiệu | Tên Danh Hiệu Hiển Thị | Phẩm Cấp | Màu Sắc | Biểu Tượng | Chỉ Số Thưởng Khi Đeo | Điều Kiện Mở Khóa |
| :--- | :--- | :---: | :---: | :---: | :--- | :--- |
| `title_rookie` | **[Tân Thủ Khai Hoang]** | Phẩm D | Xanh Lá Sáng | 🏆 | • **+30 HP Tối Đa**<br>• **+5 Giáp Vật Lý** | Mặc định khởi tạo |
| `title_knight` | **[Kị Sĩ Tiên Phong]** | Phẩm C | Xanh Lam | 🛡️ | • **+15 Giáp Vật Lý**<br>• **+10 Kháng Phép** | Đạt Cấp 10 Kị Sĩ |
| `title_archmage`| **[Ma Đạo Tôn Sư]** | Phẩm B | Tím Thần Bí | 🔮 | • **+25 Công Phép**<br>• **+20 Mana Tối Đa** | Đạt Cấp 25 Pháp Sư |
| `title_brawler` | **[Bá Vương Quyền Cước]**| Phẩm B | Tím Thần Bí | 🥊 | • **+15 ST Đánh Thường**<br>• **+5% Tỉ Lệ Bạo Kích** | Đạt Cấp 25 Võ Sĩ |
| `title_archer`  | **[Thần Tiễn Bách Phát]** | Phẩm B | Tím Thần Bí | 🏹 | • **+0.5 m/s Tốc Chạy**<br>• **+1.0m Tầm Đánh Thường** | Đạt Cấp 25 Xạ Thủ |
| `title_conqueror`| **[Chiến Thần Vô Địch]** | Phẩm S | Vàng Hoàng Kim | 👑 | • **+50 HP Tối Đa**<br>• **+20 Công Phép**<br>• **+15 Giáp** | Tiêu diệt Boss Thế Giới |

* **Hiển thị trực quan:** Khi đeo danh hiệu, tiền tố sẽ xuất hiện trang trọng trước tên người chơi trên Panel HUD góc trái trên (ví dụ: `<color=#00e5ff>[Tân Thủ Khai Hoang]</color> Dũng Sĩ SquiRRels`).
* **Cộng dồn chỉ số:** Điểm cộng chỉ số của Danh Hiệu được cộng trực tiếp vào bảng Chỉ Số Chiến Đấu (Công, Thủ, Tốc, HP) và cập nhật ngay lập tức.

---

### 2.3. Hệ Thống Thú Cưng: Cấp Độ & Tiến Hóa 3 Giai Đoạn (Pet Evolution)
Thú cưng đồng hành không chỉ là bạn đường mà còn là nguồn gia tăng sức mạnh sinh tồn quan trọng:

```
[Giai Đoạn 1: Sóc Bé Nhỏ (Baby)]  (Lv. 1 - 9)
  │  • Kích thước: Scale 1.0f
  │  • Thưởng chỉ số: +5% Tốc chạy, +2 Giáp Hộ Thể
  ▼  (Cho ăn Hạt Dẻ Linh Hồn đạt Lv. 10)
[Giai Đoạn 2: Linh Sóc Trưởng Thành (Adult)]  (Lv. 10 - 24)
  │  • Kích thước: Scale 1.35f (To hơn rõ rệt)
  │  • Ngoại quan: Bộ lông phát sáng ánh linh khí
  │  • Thưởng chỉ số: +20 HP, +5 Giáp, +0.2 m/s Tốc chạy
  ▼  (Cho ăn Hạt Dẻ Linh Hồn đạt Lv. 25)
[Giai Đoạn 3: Thần Sóc Hoàng Kim (Divine)]  (Lv. 25 - 50)
     • Kích thước: Scale 1.7f (Oai vệ, dũng mãnh)
     • Ngoại quan: Hào quang hoàng kim rực rỡ
     • Thưởng chỉ số: +50 HP, +15 Giáp, +10 Công Phép, +0.5 m/s Tốc chạy
```

* **Giao diện Tab 4 [Thú Cưng]:**
  - Hiển thị Cấp Độ hiện tại (`Lv.X / 50`) và Giai Đoạn tiến hóa.
  - Thanh tiến trình EXP thú cưng với tỷ lệ phần trăm trực quan.
  - Nút bấm `[CHO ĂN HẠT DẺ (Số lượng)]`: Tiêu tốn 1 Hạt Dẻ Linh Hồn trong túi đồ, tăng `+60 EXP` cho thú cưng và bắn hiệu ứng nổi bọt `🐿️ Pet +60 EXP!`.

---

## 3. DANH SÁCH FILE ĐÃ TRIỂN KHAI & CẬP NHẬT

1. **`Assets/Scripts/Player/PlayerStats.cs`**:
   - Bổ sung `characterClass` và các hàm `SetCharacterClass(string)`, `SetElement(string)` phục vụ kiểm tra tương thích.
   - Triển khai toàn bộ cấu trúc `CharacterTitle`, danh sách danh hiệu, logic Đeo/Tháo và các getter cộng chỉ số chiến đấu.
2. **`Assets/Scripts/Combat/SkillScrollData.cs`**:
   - Viết lại hàm `CanPlayerUse(...)` kiểm tra tương thích song song cả Hệ Phái lẫn Thuộc Tính Nguyên Tố.
3. **`Assets/Scripts/Player/InventoryManager.cs`**:
   - Tích hợp kiểm tra `CanPlayerUse` trong `EquipSkillScrollToHotbar` (chặn lắp cuộn không hợp lệ).
   - Khởi tạo đầy đủ các cuộn trục mẫu kiểm tra khóa kỹ năng (sai Hệ, sai Thuộc tính).
   - Cung cấp hàm `RemoveItem` / `ConsumeItem`.
4. **`Assets/Scripts/Combat/SkillAimingController.cs`**:
   - Thêm bước chặn khóa trong `HandleSkillPress`: nếu người chơi bấm phím chiêu không tương thích sẽ từ chối ngắm chiêu và phát cảnh báo.
5. **`Assets/Scripts/Player/PetFollower.cs`**:
   - Bổ sung hệ thống cấp độ (Level 1..50), EXP, cơ chế tiến hóa 3 giai đoạn (Scale 1.0 $\rightarrow$ 1.35 $\rightarrow$ 1.7) và hàm `FeedPet(exp)`.
6. **`Assets/Scripts/UI/CombatHUD.cs`**:
   - Nâng cấp Tab 3 [Kỹ Năng]: Bổ sung khu vực Danh Hiệu, sửa tiêu đề Perk thành `DANH SÁCH PERK:`, sửa lỗi text wrapping.
   - Sửa badge `Lv.1` sang nền đỏ cam, viền vàng kim, chữ trắng in đậm chuẩn tương phản cao.
   - Nâng cấp Tab 4 [Thú Cưng]: Thanh EXP, trạng thái tiến hóa và nút cho ăn hạt dẻ.
   - Nâng cấp Tab 5 [Cài Đặt]: Thêm thanh công cụ Test nhanh Hệ & Thuộc tính cho Game Director thử nghiệm.

---
**Ký duyệt:** SquiRRels Studio  
*Godless Fantasy — Tự hào kiến tạo thế giới MMORPG đỉnh cao.*
