# BIÊN BẢN THẢO LUẬN & BÁO CÁO TIẾN ĐỘ DỰ ÁN SỐ 03
**Mã tài liệu:** `discussion_03_support_skills_aoe_healing_and_ally_buffs.md`  
**Ngày ghi nhận:** 11/10/2026 (04:40 AM)  
**Chủ tọa / Game Director:** SquiRRels Studio  
**Trạng thái phiên họp:** Đã thống nhất & Triển khai vào code (Implemented)  

---

## 1. MỤC TIÊU CUỘC THẢO LUẬN & ĐẶT VẤN ĐỀ
Trong buổi làm việc sáng nay, Game Director (SquiRRels Studio) đã đưa ra định hướng thiết kế mới mang tính đột phá về hệ thống kỹ năng:
> *"Một số skill lại có chức năng ngược lại, ví dụ hệ Mộc hay hệ Tự Nhiên ấy: có chiêu tầm là tầm thả mưa nhưng thả để hồi máu, hồi mana thay vì gây sát thương :))... hoặc tạo buff cho người khác..."*

Điều này mở ra một phân nhánh lối chơi cực kỳ quan trọng:
1. **Không giới hạn chiêu thức chỉ là sát thương đối kháng:** Cùng là cơ chế tầm ngắm (ví dụ Archetype 2: Thả mưa / AoE Vùng Đất), chiêu thức có thể đảo ngược vai trò:
   - Thay vì rơi thiên thạch lửa gây sát thương kẻ thù $\rightarrow$ Rơi **Mưa Sinh Mệnh Hệ Mộc (Healing Rain / Spring of Life)** để hồi HP, hồi Mana hoặc ban phát Buff cho đồng đội và bản thân đứng trong vùng.
2. **Giá trị sinh tồn độc tôn của Hệ Mộc / Hệ Tự Nhiên (Wood / Nature Element):**
   - Do game áp dụng cơ chế **Hồi Máu Khắc Khổ (Harsh HP Regen)** (30s – 40s mới hồi +1 đến +2 HP tự nhiên), bình máu lại tốn kém kinh tế, nên lớp nhân vật mang **Hệ Mộc (Mộc Sư / Druid / Healer)** sẽ trở thành "linh hồn" không thể thiếu trong tổ đội đi Dungeon, săn Boss hay Công Thành Chiến.
3. **Mở rộng toàn diện 5 Combat Archetypes sang chế độ Hỗ Trợ (Support & Ally Buff):**
   - Đảm bảo hệ thống tâm ngắm, hiệu ứng hình ảnh (VFX), cơ chế tìm mục tiêu (Targeting Logic) và chữ số nảy lên (Floating Text) phản ánh đúng tính chất hỗ trợ.

---

## 2. NỘI DUNG THIẾT KẾ CHI TIẾT

### 2.1. Phân Loại Tác Dụng Chiêu Thức (`SkillCategory`)
Mỗi cuộn trục kỹ năng sẽ được định danh 1 trong 4 loại hình tác dụng:

| Mã Loại | Tên Phân Loại | Mục Tiêu Tác Động | Hiệu Quả Chiến Đấu | Màu Sắc Phản Hồi |
| :---: | :--- | :--- | :--- | :---: |
| **0** | **Tấn Công (Damage)** | Kẻ địch / Quái vật | Gây sát thương vật lý / phép thuật | Chữ Đỏ `-45` / Vàng `-90` (Crit) |
| **1** | **Hồi Máu (Heal HP)** | Đồng đội / Bản thân | Phục hồi một lượng sinh lực tức thì hoặc theo đợt | Chữ Xanh Lá Sáng `+35 HP` |
| **2** | **Hồi Mana (Restore MP)**| Đồng đội / Bản thân | Tiếp tế năng lượng ma pháp cho đồng minh | Chữ Xanh Dương `+20 MP` |
| **3** | **Cường Hóa (Buff Stat)**| Đồng đội / Bản thân | Gia tăng chỉ số (Giáp, Tốc chạy, Công phép) | Chữ Cyan `BUFF: Tên Chiêu` |

---

### 2.2. Sự Biến Hóa Của 5 Dạng Tâm Ngắm Sang Chiêu Hỗ Trợ

#### 🌿 Archetype 2: Mưa Rơi Vùng Đất (Ground AoE / Rain Effect) — Điển Hình Của Hệ Mộc
* **Cơ chế ngắm:** Vòng tròn ngắm `indicator_ground_circle` bán kính $R = 2.0m$, tầm xa tối đa $7.0m$.
* **Khi triển khai dạng Hỗ Trợ (Mưa Sinh Mệnh / Suối Nguồn Mộc Linh):**
  - Mặt đất xuất hiện vòng tròn ngọc bích phát sáng.
  - Từ trên cao, những giọt mưa năng lượng xanh ngọc rơi xuống kèm theo cánh hoa / lá cây bay lượn.
  - Mọi thực thể đồng minh (Người chơi, Đồng đội trong Party, Thú cưng) bước vào vùng sẽ được hồi máu `+HP` hoặc hồi `+Mana`.
  - **Kẻ địch bước vào KHÔNG bị thương và KHÔNG được hưởng hồi phục** (loại trừ theo Team / Layer).

#### 🛡️ Archetype 4: Hào Quang Tỏa 360 Độ (Self Nova)
* **Offensive:** Sóng xung kích giật nổ kẻ địch xung quanh.
* **Support (Thánh Quang Lan Tỏa / Vòng Tròn Hộ Vệ):**
  - Tỏa ra luồng sóng ánh sáng ngọc bích / thánh quang bán kính $2.5m$ quanh người thi triển.
  - Toàn bộ đồng đội đứng gần người chơi được hưởng chung hiệu ứng: Hồi phục tức thì hoặc nhận Khiên Giáp Bảo Vệ.

#### 🏹 Archetype 1: Bắn Đạn Tuyến Tính (Line Skillshot)
* **Offensive:** Bắn mũi tên lửa / đạn sét công phá đối phương.
* **Support (Tia Sáng Trị Liệu / Luồng Sinh Khí):**
  - Phóng luồng khí sinh mệnh màu xanh lá bay thẳng xuyên qua đồng đội.
  - Đồng đội nào chạm vào viên đạn sẽ được hồi máu và nhận thêm tốc độ di chuyển trong 3 giây.

#### 🪓 Archetype 3: Sóng Quét Hình Nón (Cone Wave)
* **Offensive:** Quét luồng sóng lửa thiêu rụi kẻ địch phía trước.
* **Support (Gió Thần Ban Phước):**
  - Quét một làn gió mát lành màu xanh ngọc về phía trước hình quạt $60^\circ$, tầm $4.5m$, thanh tẩy hiệu ứng xấu cho tiền tuyến.

#### 💎 Archetype 5: Buff Tự Thân (Self Buff)
* Kích hoạt tức thì lên chính bản thân người dùng (không cần ngắm, cự ly 0m), hiển thị trên HUD Buff Widget.

---

### 2.3. Hệ Thống Chữ Số Nảy Lên (Floating Text) Đa Sắc Màu
* **Sát thương thường:** Màu Đỏ `(1.0, 0.25, 0.25)` $\rightarrow$ `-45`
* **Sát thương bạo kích:** Màu Vàng Hoàng Kim `(1.0, 0.85, 0.20)` $\rightarrow$ `-90 CRIT!`
* **Hồi phục Sinh Lực:** Màu Xanh Lá Sinh Mệnh `(0.20, 0.95, 0.35)` $\rightarrow$ `+35 HP`
* **Hồi phục Ma Lực:** Màu Xanh Dương Năng Lượng `(0.20, 0.75, 1.0)` $\rightarrow$ `+20 MP`
* **Hiệu ứng Buff:** Màu Cyan Thần Thoại `(0.0, 0.90, 1.0)` $\rightarrow$ `BUFF: Giáp Thần Mộc (15s)`

---

## 3. CÁC FILE THỰC THI & LIÊN KẾT HỆ THỐNG

1. **[SkillScrollData.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillScrollData.cs):**
   - Bổ sung trường `skillCategory` (0=Damage, 1=Heal HP, 2=Heal Mana, 3=Buff Stat).
   - Thêm thuộc tính kiểm tra `IsSupport`, `CategoryName`.
2. **[SkillAimingController.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillAimingController.cs):**
   - Tách luồng xử lý: Nếu `scroll.IsSupport`, chuyển sang logic quét đồng minh / bản thân để hồi HP, hồi Mana hoặc kích hoạt Buff thay vì trừ máu kẻ địch.
3. **[FloatingDamageManager.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/FloatingDamageManager.cs):**
   - Bổ sung hàm `SpawnFloatingText(...)` tổng quát và `SpawnHeal(...)` để vẽ chữ số xanh lá `+HP` và xanh dương `+MP`.
4. **[SkillVFXManager.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillVFXManager.cs):**
   - Bổ sung hiệu ứng hạt màu Xanh Lá Ngọc Bích cho Hệ Mộc `(0.25f, 0.90f, 0.35f)`.
   - Bổ sung `SpawnSelfBuffVFX(...)` và `SpawnHealingRain(...)`.
5. **[CombatHUD.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/CombatHUD.cs):**
   - Triển khai phương thức `DrawActiveBuffsWidget()` hiển thị danh sách buff kèm thời gian đếm ngược thời gian thực ở góc trái giữa màn hình.
6. **[ItemCreatorModal.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/ItemCreatorModal.cs):**
   - Tích hợp lựa chọn "Tác Dụng Kỹ Năng" (Tấn Công vs Hồi Máu vs Hồi Mana vs Cường Hóa).
   - Tích hợp công cụ nạp ảnh ngoài từ ổ cứng máy tính (External PNG Import).
