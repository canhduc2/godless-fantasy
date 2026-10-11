# BIÊN BẢN THẢO LUẬN 10: KIẾN TRÚC TRỤC KỸ NĂNG CHUYÊN BIỆT (SKILL AXIS ENGINE), 140 HÌNH THÁI VFX NGUYÊN TỐ & QUY TRÌNH SẢN XUẤT HOẠT ẢNH THỦ TỤC
**Mã tài liệu:** DISC-10  
**Dự án:** Godless Fantasy (2D Top-Down Retro HD MMORPG)  
**Studio phát triển:** SquiRRels Studio  
**Ngày lập:** 2026-10-12  
**Trạng thái:** Kế Hoạch & Thiết Kế Kiến Trúc Nền Tảng (Core Architecture & Engineering Plan)  

---

## 1. TỔNG QUAN & BỐI CẢNH CHIẾN LƯỢC

### 1.1. Bài Toán Quy Mô & Thách Thức Sản Xuất (The Combinatorial Explosion)
Trong *Godless Fantasy*, hệ thống nhân vật và ma pháp được thiết lập dựa trên 2 trục tọa độ then chốt:
* **4 Chức Nghiệp Khởi Đầu (4 Classes):** Kị Sĩ (Knight), Pháp Sư (Mage), Xạ Thủ (Archer), Võ Sĩ (Brawler). *(Cùng 1 Hệ Ẩn - Kẻ Ngoại Đạo Heretic kết hợp)*.
* **7 Thuộc Tính Nguyên Tố (7 Elements):** Hỏa (Fire), Băng (Ice), Lôi (Thunder), Phong (Wind), Mộc (Nature), Quang / Ánh Sáng (Light), Ám / Bóng Tối (Dark).

Nếu mỗi chức nghiệp sở hữu các trục kỹ năng độc quyền cao cấp (phẩm cấp **C, B, A, S, X**):
$$\text{4 Hệ} \times 5 \text{ Trục Phẩm Cấp Độc Quyền} = 20 \text{ Loại Trục Kỹ Năng Độc Quyền}$$

Khi 20 loại trục kỹ năng này được người chơi thuộc 7 thuộc tính nguyên tố khác nhau thi triển, số lượng biến thể hình ảnh và hoạt ảnh chiến đấu bùng nổ lên tới:
$$20 \text{ Trục Kỹ Năng} \times 7 \text{ Thuộc Tính Nguyên Tố} = 140 \text{ Hình Thái Chiêu Thức Hoạt Ảnh Khác Nhau}$$

```
+---------------------------------------------------------------------------------------------------------+
|                                MA TRẬN 140 HÌNH THÁI KỸ NĂNG NGUYÊN TỐ                                  |
+--------------------+---------------------+---------------------+--------------------+-------------------+
| 4 HỆ CHIẾN ĐẤU     | 5 TRỤC ĐỘC QUYỀN    | 7 THUỘC TÍNH        | TỔNG BIẾN THỂ VFX  | CƠ CHẾ SẢN XUẤT   |
+--------------------+---------------------+---------------------+--------------------+-------------------+
| • Kị Sĩ (Knight)   | • Phẩm X (Tối Thượng)| • Hỏa (Đỏ Cam)      |                    |                   |
| • Pháp Sư (Mage)   | • Phẩm S (Sử Thi)   | • Băng (Xanh Tuyết) |                    | PROCEDURAL        |
| • Xạ Thủ (Archer)  | • Phẩm A (Huyền Thoại| • Lôi (Tím Vàng)    |   140 BIẾN THỂ     | MODULAR VFX       |
| • Võ Sĩ (Brawler)  | • Phẩm B (Ưu Tú)    | • Phong (Xanh Lơ)   |   HÌNH THÁI CHUYÊN | ENGINE            |
|                    | • Phẩm C (Hiếm)     | • Mộc (Xanh Lục)    |   BIỆT HÓA HOÀN TOÀN| (Tự động hóa      |
|                    |                     | • Quang (Vàng Kim)  |                    | không làm thủ công|
|                    |                     | • Ám (Tím Đen)      |                    | từng sprite lẻ)   |
+--------------------+---------------------+---------------------+--------------------+-------------------+
| ĐA HỆ / TOÀN HỆ    | • Phẩm D (Khá)      | Dùng chung cho cả   | Hàng chục kỹ năng  | Kỹ năng trung tính|
| (Universal)        | • Phẩm E (Thường)   | 4 hệ (Không khóa hệ)| buff, độc, hồi máu | không chuyên biệt |
+--------------------+---------------------+---------------------+--------------------+-------------------+
```

### 1.2. Mục Tiêu Cốt Lõi Của Báo Cáo
1. **Thiết lập danh mục 20 Trục Kỹ Năng Độc Quyền (X, S, A, B, C):** Đặt tên ngắn gọn (dưới 4 từ), mang đậm bản sắc võ học và ma pháp của từng hệ.
2. **Quy hoạch các Trục Kỹ Năng Đa Hệ (D, E...):** Dành cho cả 4 hệ cùng dùng, tên gọi trung tính, tập trung vào hiệu ứng hỗ trợ, sinh tồn, độc dược, hồi phục và củng cố thể chất.
3. **Thiết kế Procedural Modular VFX & Animation Engine:** Học hỏi giải pháp hiện đại của ngành game (Parametric Projectile / Shader Ramp Tinting / Motion Skeleton / Decoupled Timeline), cho phép họa sĩ chỉ cần vẽ khung hoạt ảnh cơ sở, hệ thống tự động sinh 140 hình thái VFX theo thuộc tính và tham số đường đạn.
4. **Chuẩn hóa công thức sinh chỉ số Cuộn Trục (Stat Generation & Balancing Engine):** Ràng buộc giữa Phẩm cấp, Mana, Cooldown, Scaling đa chỉ số (INT/STR/AGI/VIT) và Tầm đánh.
5. **Đột phá Gameplay 6 Kiểu Tầm Đánh $\times$ 4 Loại Tác Dụng:** Nâng tầm tương tác chiến đấu MOBA-style trên nền tảng cơ chế Đánh Thường Ưu Tiên Chuột đã hoàn thiện.

---

## 2. QUY HOẠCH 20 TRỤC KỸ NĂNG ĐỘC QUYỀN (EXCLUSIVE SKILL AXES)

### 2.1. Quy Tắc Đặt Tên Chuyên Biệt Hóa
* **Độ dài giới hạn:** Tối đa **4 từ** (ngắn gọn, cô đọng, dứt khoát, âm hưởng hào hùng).
* **Đặc trưng hệ:**
  * **Kị Sĩ:** Gắn liền với *Kiếm, Khiên, Trảm, Phá, Giáp, Thánh, Hộ Vệ*.
  * **Pháp Sư:** Gắn liền với *Chú, Pháp, Pháo, Ma, Trụy, Triều, Vực*.
  * **Xạ Thủ:** Gắn liền với *Tiễn, Cung, Vũ, Xuyên, Xạ, Đoạt, Ngân*.
  * **Võ Sĩ:** Gắn liền với *Quyền, Cước, Kình, Chấn, Phá, Bá, Khí*.
* **Cơ chế Khóa Hệ Tuyệt Đối:** Cuộn trục độc quyền ghi rõ chức nghiệp yêu cầu. Hệ khác nhặt được không thể học hoặc trang bị.

---

### 2.2. Chi Tiết 20 Trục Kỹ Năng Độc Quyền Theo 4 Hệ

#### 🛡️ NHÓM 1: KIẾM VÀNG & KHIÊN THÉP — HỆ KỊ SĨ (KNIGHT)
*Định vị:* Tiên phong càn quét, hộ vệ đồng đội, áp sát phá trận, kiếm khí dũng mãnh.

| Phẩm cấp | Tên Trục Kỹ Năng | Cự ly & Archetype | Tác dụng cốt lõi | Mô tả gameplay & Bản sắc |
| :---: | :--- | :--- | :---: | :--- |
| **X** | **Thiên Thần Hộ Giới** | 6.5m (Domain Field) | Cường hóa & Bất tử | Triệu hồi kết giới thần thánh 360 độ, miễn nhiễm sát thương 3.0s và phản chấn 50% đòn đánh. |
| **S** | **Liệt Địa Thánh Kiếm** | 7.0m (Ground AoE) | Sát thương nổ | Cắm cự kiếm khổng lồ từ không trung xuống tâm đất, nổ tung địa chấn và hất tung mục tiêu. |
| **A** | **Khiên Thành Xung Kích**| 6.0m (Dash Piercing) | Lướt & Khống chế | Giơ đại khiên lao nhanh về phía trước hất văng mọi kẻ cản đường, làm choáng 1.5s. |
| **B** | **Toàn Phong Trảm Kích**| 3.2m (Self Nova 360°)| Sát thương diện rộng| Xoay kiếm hình lốc xoáy quanh thân 2 vòng, chém quét mọi kẻ thù xung quanh. |
| **C** | **Kiếm Khí Trực Phá** | 8.0m (Linear Line) | Sát thương định hướng| Vung kiếm chém ra luồng kiếm khí bay thẳng xé gió, xuyên qua mục tiêu đầu tiên. |

---

#### 🔮 NHÓM 2: MA PHÁP CỔ XƯA — HỆ PHÁP SƯ (MAGE)
*Định vị:* Phép thuật hủy diệt tầm xa, khống chế diện rộng, biến ảo khôn lường, bão tố nguyên tố.

| Phẩm cấp | Tên Trục Kỹ Năng | Cự ly & Archetype | Tác dụng cốt lõi | Mô tả gameplay & Bản sắc |
| :---: | :--- | :--- | :---: | :--- |
| **X** | **Thế Giới Băng Diệt** | 8.5m (Huge AoE Rain) | Tuyệt kỹ hủy diệt | Ngưng đọng không gian, giáng mưa thiên thạch ma pháp đa tầng tàn phá toàn bộ khu vực. |
| **S** | **Thiên Thạch Ma Trụy** | 7.0m (Ground AoE) | Sát thương nổ lớn | Triệu hồi vẫn thạch khổng lồ từ tầng mây rơi xuống tâm chỉ định, đốt cháy mặt đất 4 giây. |
| **A** | **Bí Pháp Oanh Kích** | 5.0m (Cone Arc 90°) | Sát thương hình quạt | Vận tụ ma lực xả luồng sóng xung kích hồ quang hình nón cực đại thiêu đốt kẻ địch trước mặt. |
| **B** | **Nguyên Tố Triều Tịch**| 8.0m (Linear Wave) | Sát thương & Đẩy lùi | Phóng đợt sóng ma pháp thủy triều cuồn cuộn bay thẳng, đẩy lùi kẻ địch về phía sau. |
| **C** | **Ma Cầu Liên Xạ** | 8.0m (Triple Bolt) | Sát thương liên thanh | Bắn liên tiếp 3 quả cầu ma pháp tốc độ cao bám đuổi nhẹ vào hướng con trỏ chuột. |

---

#### 🏹 NHÓM 3: BÃO TÊN XUYÊN PHÁ — HỆ XẠ THỦ (ARCHER)
*Định vị:* Tầm bắn xa nhất bản đồ, chí mạng sắc lẹm, bẫy ma thuật tầm xa, cơ động né tránh.

| Phẩm cấp | Tên Trục Kỹ Năng | Cự ly & Archetype | Tác dụng cốt lõi | Mô tả gameplay & Bản sắc |
| :---: | :--- | :--- | :---: | :--- |
| **X** | **Vạn Tiễn Quy Tông** | 8.0m (Field Barrage) | Tuyệt kỹ mưa tên | Bắn mũi tên dẫn đạo lên trời, triệu hồi hàng vạn mũi tên nguyên tố xối xả như bão quét sạch màn hình. |
| **S** | **Xuyên Vân Thần Tiễn** | 10.0m (Hyper Line) | Sát thương xuyên thấu | Tụ lực bắn ra mũi đại tiễn ma thuật xuyên thủng mọi hàng phòng ngự, sát thương tăng theo cự ly. |
| **A** | **Bão Tiễn Đoạt Hồn** | 5.5m (Cone Arc 70°) | Sát thương chùm quạt | Bắn một chùm 7 mũi tên xòe hình quạt càn quét diện rộng, gây hiệu ứng xuất huyết. |
| **B** | **Ma Lực Tiễn Vũ** | 7.0m (Ground AoE) | Mưa tên làm chậm | Thả chùm tên ma thuật rơi thẳng xuống vùng đất 2.5m, tạo bãi gai năng lượng làm chậm 60%. |
| **C** | **Truy Hồn Xạ Kích** | 8.5m (Homing Shot) | Đạn tự định hướng | Bắn mũi tên thanh mảnh có khả năng uốn lượn tự tìm đến hitbox mục tiêu gần nhất. |

---

#### 👊 NHÓM 4: QUYỀN CƯỚC BÁ ĐẠO — HỆ VÕ SĨ (BRAWLER)
*Định vị:* Áp sát chớp nhoáng, liên hoàn quyền cước, nội kình bạo liệt, phá giáp đánh choáng.

| Phẩm cấp | Tên Trục Kỹ Năng | Cự ly & Archetype | Tác dụng cốt lõi | Mô tả gameplay & Bản sắc |
| :---: | :--- | :--- | :---: | :--- |
| **X** | **Bá Vương Phá Thiên** | 3.5m (Impact Nova) | Tuyệt kỹ bạo chấn | Dồn toàn bộ chân khí tung một quyền vỡ nát mặt đất, tạo chấn động sóng âm làm choáng toàn bộ 3.0s. |
| **S** | **Chấn Địa Liệt Quyền** | 4.5m (Ground Leap) | Áp sát & Đập đất | Nhảy vọt lên không trung dậm mạnh xuống vị trí chỉ định, tạo hố nứt nguyên tố sát thương cực lớn. |
| **A** | **Bạo Phong Liên Cước** | 4.0m (Cone Sweep) | Cận chiến dồn dập | Tung chuỗi cước ảnh liên hoàn quét hình nón phía trước, đẩy lùi và phá giáp đối thủ 30%. |
| **B** | **Khí Kình Phá Không** | 6.5m (Shockwave Line) | Chưởng lực xuyên thấu| Tung cú đấm thẳng phát ra luồng sóng khí kình bay vụt đi, thổi bay đường đạn của kẻ thù. |
| **C** | **Xung Quyền Thấu Kích** | 2.5m (Point Impact) | Đột kích tức thời | Lướt tới 2 mét đấm thẳng vào mục tiêu, gây sát thương bỏ qua 25% phòng ngự. |

---

## 3. QUY HOẠCH TRỤC KỸ NĂNG ĐA HỆ / PHỔ THÔNG (UNIVERSAL AXES - PHẨM D & E)

### 3.1. Triết Lý Thiết Kế Đa Hệ
* **Không mang bản sắc chức nghiệp:** Tuyệt đối không dùng chữ *Kiếm, Cung, Gậy, Quyền, Tiễn*.
* **Áp dụng tự nhiên cho cả 4 hệ:** Một Kị Sĩ, một Pháp Sư, một Xạ Thủ hay một Võ Sĩ khi học đều có cảm giác hợp lý và hữu dụng.
* **Định vị chức năng:** Tập trung vào **Hiệu ứng Trạng thái (Status Effects)**, **Bảo hộ thân thể (Defensive Buffs)**, **Khắc phục tài nguyên (Utility / Sustain)**, và **Độc thuật / Khống chế nhẹ**.

```
+---------------------------------------------------------------------------------------------------------+
|                               DANH MỤC TRỤC KỸ NĂNG ĐA HỆ (UNIVERSAL TIERS)                             |
+---------------------+-------------------+---------------------+--------------------+--------------------+
| 1. DẠNG ĐỘC TỐ      | 2. DẠNG HỘ THÂN   | 3. DẠNG HỒI PHỤC    | 4. DẠNG CƠ ĐỘNG    | 5. DẠNG KHỐNG CHẾ  |
+---------------------+-------------------+---------------------+--------------------+--------------------+
| • Nhiễm Độc Trụ     | • Hộ Thể Cương Khí| • Sinh Mệnh Chúc    | • Tốc Bộ Khí       | • Tê Liệt Khí Trận |
| • Thực Hồn Độc Vụ   | • Kim Cang Thuẫn  | • Tĩnh Tâm Quyết    | • Thoát Thân Bộ    | • Triền Nhiên Cực  |
| • Hủ Cốt Ma Khí     | • Hoán Ma Kết Giới| • Thanh Tẩy Chú     | • Thuấn Thân Khí   | • Hàn Khí Tỏa      |
+---------------------+-------------------+---------------------+--------------------+--------------------+
```

### 3.2. Bảng Danh Mục Trục Kỹ Năng Đa Hệ Tiêu Biểu

| Mã Trục | Tên Trục Kỹ Năng | Phẩm | Archetype | Loại tác dụng | Mô tả tính năng đa hệ |
| :---: | :--- | :---: | :---: | :---: | :--- |
| `uni_poison_pillar` | **Nhiễm Độc Trụ** | **D** | 2 (Ground AoE) | Sát thương DoT | Cắm một cột năng lượng độc tại tâm đất; kẻ địch bước vào bị trúng độc rút máu trong 6s. |
| `uni_soul_mist` | **Thực Hồn Độc Vụ** | **D** | 4 (Self Nova) | Hút máu diện rộng | Bốc ra màn sương độc quanh thân; gây sát thương DoT và hút 15% sát thương hồi lại HP. |
| `uni_body_fortify` | **Hộ Thể Cương Khí**| **D** | 5 (Self Buff) | Tăng Giáp & Kháng | Tụ khí quanh người tăng +25 Giáp và giảm 15% sát thương nhận vào trong 10 giây. |
| `uni_diamond_shield`| **Kim Cang Thuẫn** | **D** | 5 (Self Buff) | Khiên Chắn Hấp Thụ | Tạo lớp khiên chắn năng lượng hấp thụ 120 sát thương trước khi vỡ. |
| `uni_life_blessing` | **Sinh Mệnh Chúc** | **D** | 2 (Ground AoE) | Hồi máu đồng minh | Thả vòng hồi sinh xuống đất, đồng đội bước vào hồi +40 HP và phục hồi thể lực. |
| `uni_mind_clarity` | **Tĩnh Tâm Quyết** | **E** | 5 (Self Buff) | Hồi phục Mana | Đứng yên tụ khí 1.5s hồi phục ngay lập tức +30 Mana để tiếp tục giao tranh. |
| `uni_purify_mantra` | **Thanh Tẩy Chú** | **E** | 5 (Self Buff) | Giải trừ trạng thái | Hóa giải ngay lập tức hiệu ứng làm chậm, xuất huyết hoặc trúng độc trên bản thân. |
| `uni_speed_burst` | **Tốc Bộ Khí** | **E** | 5 (Self Buff) | Tăng tốc chạy | Tăng +35% tốc độ di chuyển trong 4.0 giây, hỗ trợ truy đuổi hoặc rút lui nhanh. |
| `uni_paralyze_trap` | **Tê Liệt Khí Trận** | **E** | 2 (Ground AoE) | Bẫy làm chậm | Đặt bẫy năng lượng tại chuột; khi giẫm phải kẻ địch bị trói chân trong 1.8 giây. |
| `uni_energy_spark` | **Nguyên Khí Đạn** | **E** | 1 (Linear Line) | Sát thương cơ bản | Bắn một tia đạn khí sơ cấp thẳng tắp, hồi chiêu nhanh (0.8s), tiêu hao rất ít Mana. |

---

## 4. BÀI TOÁN 140 HÌNH THÁI VFX: PROCEDURAL MODULAR VFX ENGINE

### 4.1. Thách Thức Kỹ Thuật: Tại Sao Không Được Vẽ Thủ Công 140 Bộ Hoạt Ảnh?
Nếu áp dụng phương pháp thủ công cổ điển:
* 140 kỹ năng $\times$ 8 frames hoạt ảnh đạn bay $\times$ 6 frames nổ va chạm = **1.960 frames pixel art thủ công**.
* Bất kỳ khi nào chỉnh sửa cân bằng tốc độ bay, bán kính nổ hay thêm thuộc tính mới, đội ngũ họa sĩ sẽ phải vẽ lại toàn bộ hàng trăm spritesheets.
* Dung lượng bộ nhớ (VRAM / Texture Atlases) sẽ phình to gấp 10 lần không cần thiết.

### 4.2. Giải Pháp Hiện Đại: Kiến Trúc Hoạt Ảnh Thủ Tục Tham Số Hóa (Procedural Parametric Pipeline)
Lấy cảm hứng từ kiến trúc VFX tiên tiến của các siêu phẩm 2D hiện đại (*Vampire Survivors, Noita, Wizard of Legend, Path of Exile*), Godless Fantasy xây dựng cơ chế **Tách Biệt 3 Tầng Độc Lập**:

```
+---------------------------------------------------------------------------------------------------------+
|                              KIẾN TRÚC PROCEDURAL MODULAR VFX ENGINE 3 TẦNG                             |
+---------------------------------------------------------------------------------------------------------+
|                                                                                                         |
|  [TẦNG 1: KHUNG XƯƠNG CHUYỂN ĐỘNG & ĐƯỜNG ĐẠN] (Motion & Ballistics Skeleton)                           |
|   • Trajectory Curve: Tuyến tính (Linear), Bán cầu (Lobbed Arc), Lượn sóng (Sine), Đạn đuổi (Homing)... |
|   • Hitbox & Physics: Raycast 2D, Boxcast, Circle Overlap, Velocity Vector, Acceleration, Lifespan.     |
|   • Event Timeline: OnSpawn -> OnFlight -> OnImpact -> OnAreaTick -> OnExpire.                          |
|                                                                                                         |
|                                         |                                                               |
|                                         v                                                               |
|                                                                                                         |
|  [TẦNG 2: PHÔI KHỐI ĐỒ HỌA GỐC (Greyscale Base Shape Masks)]                                            |
|   • Thư viện 8 Phôi Khối Chuẩn (32x32 / 64x64 Greyscale Normal):                                        |
|     - Mask_Orb (Cầu năng lượng tròn)       - Mask_Beam (Tia sáng laser kéo dài)                        |
|     - Mask_Slash (Vệt kiếm khí trăng khuyết) - Mask_Arrow (Mũi tên khí động học)                        |
|     - Mask_Wave (Sóng xung kích cung tròn)  - Mask_RainDrop (Giọt mưa sao băng rơi)                     |
|     - Mask_NovaRing (Vòng sóng xung kích)   - Mask_Pillar (Cột sáng bốc thiên)                          |
|                                                                                                         |
|                                         |                                                               |
|                                         v                                                               |
|                                                                                                         |
|  [TẦNG 3: BỘ ÁO NGUYÊN TỐ & SHADER RAMP TINTING] (Elemental Skin & Colorization)                        |
|   • Shader Ramp Color / Palette Swapping tự động nhuộm màu theo 7 Hệ:                                    |
|     - Hỏa: Core Trắng -> Viền Đỏ Cam -> Khói Tro Đen -> Hạt tàn lửa bay lượn (Fire Sparkles).          |
|     - Băng: Core Xanh Nhạt -> Viền Lam Tuyết -> Bụi sương lạnh -> Vỡ vụn pha lê (Ice Crystals).        |
|     - Lôi: Core Vàng Chanh -> Viền Tím Điện -> Tia hồ quang zigzag ngẫu nhiên (Lightning Arcs).        |
|     - Phong: Core Trắng Xám -> Viền Xanh Bạc -> Vệt khí cuộn lốc xoáy (Wind Trails).                   |
|     - Mộc: Core Xanh Lá -> Viền Lục Bảo -> Lá rơi xoay tít + Mầm chồi (Leaves & Petals).                |
|     - Quang: Core Vàng Rực -> Viền Kim Cương -> Hào quang thập tự phát sáng (Holy Cross Rays).          |
|     - Ám: Core Tím Đen -> Viền Hắc Ám -> Màn sương quỷ dị nuốt chửng (Dark Miasma).                     |
|                                                                                                         |
+---------------------------------------------------------------------------------------------------------+
```

---

### 4.3. Bảng Tham Số Hóa Đường Đạn & Quy Mô Hiệu Ứng (Ballistics Spec Sheet)
Trong Studio Tạo Kỹ Năng, người thiết kế chỉ cần kéo thanh trượt (No-Code Parameters):

```csharp
[System.Serializable]
public class SkillMotionProfile
{
    [Header("1. Ballistics & Trajectory (Kiểu Đường Đạn)")]
    public TrajectoryType trajectory;        // Linear, SineWave, Homing, LobbedArc, Boomerang, Orbital
    public float flySpeed = 16.0f;           // Tốc độ bay (m/s)
    public float acceleration = 0.0f;        // Gia tốc tăng/giảm tốc
    public float turnRate = 0.0f;            // Tốc độ bẻ lái bám đuôi (Homing deg/s)
    public float waveAmplitude = 0.0f;       // Biên độ lắc sóng (cho đạn uốn lượn)
    public float waveFrequency = 0.0f;       // Tần số dao động sóng
    public int maxPiercingTargets = 1;       // Số mục tiêu xuyên thấu (1 = nổ khi chạm đầu tiên)

    [Header("2. Dimensions & Scale (Kích Thước & Quy Mô)")]
    public Vector2 projectileSize = new Vector2(0.5f, 0.5f); // Kích thước đạn (m)
    public float explosionRadius = 2.0f;     // Bán kính vùng nổ va chạm (m)
    public float lifetime = 1.5f;            // Thời gian tồn tại tối đa (s)

    [Header("3. Visual Rendering & Shapes (Kiểu Dáng Khối)")]
    public BaseShapeMask shapeMask;          // Orb, Slash, Arrow, Wave, Meteor, NovaRing
    public bool enableTrailRenderer = true;  // Vệt đuôi kéo dài
    public float trailDuration = 0.2f;       // Độ dài thời gian vệt đuôi
    public float trailStartWidth = 0.4f;     // Độ dày đầu đuôi
    public float trailEndWidth = 0.02f;      // Độ nhọn vuốt đuôi
    public bool cameraShakeOnImpact = false; // Rung màn hình khi nổ
    public float screenShakeIntensity = 0.15f;// Cường độ rung chấn
}
```

---

## 5. HỆ THỐNG XÂY DỰNG CHỈ SỐ CUỘN TRỤC (STAT GENERATION & BALANCE ENGINE)

### 5.1. Công Thức Cân Bằng Toán Học Theo Phẩm Cấp (E $\rightarrow$ X)
Để đảm bảo trò chơi không bị lạm phát chỉ số hay mất cân bằng giữa các phẩm cấp, mọi cuộn trục được tính toán dựa trên **Ngân Sách Sức Mạnh (Power Budget Formula)**:

$$\text{Tổng Sức Mạnh (Score)} = \text{BaseDamage} \times 1.0 + (\text{Scaling} \times 100) \times 0.6 + (\text{AoE Radius} \times 15) - (\text{Cooldown} \times 8) - (\text{ManaCost} \times 1.2)$$

| Phẩm Cấp | Bậc Phẩm | Ngân Sách Điểm (Budget) | Sát Thương Cơ Bản | % Scaling Thuộc Tính | Mana Tiêu Hao | Hồi Chiêu Chuẩn | Số Dòng Phụ (Affixes) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **E (Thường)** | Trắng | 30 – 50 pts | 15 – 25 ST | 40% – 60% | 8 – 14 MP | 0.8s – 1.8s | 0 dòng |
| **D (Khá)** | Lục | 55 – 85 pts | 26 – 40 ST | 65% – 85% | 15 – 22 MP | 1.8s – 3.0s | 1 dòng ngẫu nhiên |
| **C (Hiếm)** | Lam | 90 – 130 pts | 42 – 65 ST | 90% – 115% | 23 – 32 MP | 2.5s – 4.2s | 2 dòng ngẫu nhiên |
| **B (Ưu tú)** | Tím | 135 – 190 pts | 70 – 100 ST | 120% – 150%| 33 – 45 MP | 3.5s – 6.0s | 3 dòng ngẫu nhiên |
| **A (Huyền thoại)**| Cam | 195 – 270 pts | 105 – 150 ST| 155% – 190%| 46 – 60 MP | 5.0s – 8.5s | 4 dòng cao cấp |
| **S (Sử thi)** | Đỏ | 275 – 380 pts | 160 – 230 ST| 200% – 250%| 65 – 85 MP | 7.0s – 12.0s| 5 dòng + 1 Dòng Độc Bản |
| **X (Tối Thượng)** | Vàng Kim | 400 – 550 pts | 250 – 380 ST| 260% – 350%| 90 – 130 MP| 12.0s – 25.0s| Chỉ số Max + Hào Quang Vực Thần |

---

### 5.2. Đa Dạng Hóa Thuộc Tính Cộng Hưởng (Multi-Attribute Scaling)
Thay vì chỉ scale theo Trí Tuệ (INT) của Pháp Sư, hệ thống cuộn trục mở rộng công thức sát thương theo từng chức nghiệp:
* **Kị Sĩ (Knight):** $\text{Sát Thương} = \text{BaseDamage} + (0.7 \times \text{STR}) + (0.5 \times \text{VIT})$.
* **Pháp Sư (Mage):** $\text{Sát Thương} = \text{BaseDamage} + (1.2 \times \text{INT}) + (0.2 \times \text{AGI})$.
* **Xạ Thủ (Archer):** $\text{Sát Thương} = \text{BaseDamage} + (0.9 \times \text{AGI}) + (0.4 \times \text{STR})$.
* **Võ Sĩ (Brawler):** $\text{Sát Thương} = \text{BaseDamage} + (0.8 \times \text{STR}) + (0.6 \times \text{AGI})$.

### 5.3. Bảng Dòng Thuộc Tính Phụ Ngẫu Nhiên (Random Scroll Affixes)
Mỗi cuộn trục từ phẩm D đến S khi rơi ra từ Boss hoặc rương báu sẽ mang các dòng phụ bất ngờ:
1. `[Tật Phong]`: Rút ngắn thời gian hồi chiêu từ 5% – 18%.
2. `[Tiết Khí]`: Giảm lượng Mana tiêu hao từ 10% – 25%.
3. `[Bạo Liệt]`: Tăng tỉ lệ chí mạng riêng cho kỹ năng này +8% – 20%.
4. `[Xuyên Giáp]`: Bỏ qua 15% – 35% giáp phòng ngự của mục tiêu.
5. `[Hút Hồn]`: Chuyển hóa 6% – 12% sát thương gây ra thành máu hồi phục.
6. `[Khuếch Tán]`: Tăng bán kính vùng ảnh hưởng AoE thêm +0.5m – 1.2m.

---

## 6. MA TRẬN 6 KIỂU TẦM ĐÁNH $\times$ 4 LOẠI TÁC DỤNG: ĐỘT PHÁ GAMEPLAY

Hệ thống kế thừa trọn vẹn sự độc đáo của cơ chế **Đánh Thường Thông Minh (Left-Click Focus / Right-Click Auto-Hit / 3 Vùng Hysteresis Buffer / Ưu Tiên 100% Target Chuột Giữa Quái & Người Chơi)** đã phát triển thành công.

### 6.1. Chi Tiết 6 Kiểu Tầm Đánh (The 6 Range Archetypes)

```
+---------------------------------------------------------------------------------------------------------------------------------------+
|                                                  6 KIỂU TẦM ĐÁNH & CƠ CHẾ THI TRIỂN CHUẨN                                             |
+-------------------+--------------------+--------------------+--------------------+-----------------------+----------------------------+
| 1. BẮN THẲNG      | 2. THẢ ĐẤT / XẢ MƯA| 3. HÌNH QUẠT       | 4. TỎA QUANH THÂN  | 5. BUFF TỰ THÂN       | 6. HÀO QUANG TOÀN ĐỘI      |
|    (LINEAR LINE)  |    (GROUND AOE)    |    (CONE SWEEP)    |    (SELF NOVA 360°)|    (SELF STANCE BUFF) |    (TEAM WIDE DOMAIN)      |
+-------------------+--------------------+--------------------+--------------------+-----------------------+----------------------------+
| Cự ly: 8.0 - 10.0m| Cự ly tâm: 7.0m    | Cự ly: 4.5 - 5.5m  | Bán kính: 2.5 - 3m | Cự ly: 0m (Bản thân)  | Bán kính: 6.5 - 8.5m       |
| Mũi tên chỉ hướng | Vòng tròn chọn đất | Vòng quạt góc 60-90| Vòng tròn bao quanh| Hào quang chân lên đầu| Vòng kết giới hoàng kim    |
| Xuyên / Nổ đầu    | Delay mưa rơi 0.35s| Quét cận chiến rộng| Giải vây tức thì   | Cường hóa chỉ số HUD  | Chia sẻ buff cho cả Party  |
+-------------------+--------------------+--------------------+--------------------+-----------------------+----------------------------+
```

1. **Archetype 1: Bắn Thẳng (Linear Skillshot / Piercing Beam):**
   * Chuột nhắm hướng nào, đạn/kiếm khí/mũi tên phóng thẳng hướng đó.
   * Đòi hỏi ngắm chuẩn, đọc hướng di chuyển (Lead target) của đối thủ trong PvP.
2. **Archetype 2: Thả Đất / Xả Mưa (Ground AoE / Rain Fall):**
   * Di chuột chọn tọa độ tâm đất trong phạm vi 7.0m.
   * Có độ trễ 0.35s trước khi thiên thạch/bão tuyết/mưa tên ập xuống. Buộc đối phương phải phản xạ dùng phím `Shift Dash` để né thoát vùng đỏ.
3. **Archetype 3: Hình Quạt (Cone Sweep):**
   * Quét luồng sát thương hình nón 60° đến 90° ngay trước mặt. Cực kỳ hiệu quả khi dọn bầy quái áp sát hoặc ép góc đối thủ vào bờ tường.
4. **Archetype 4: Tỏa Quanh Thân (Self Nova 360°):**
   * Nổ bùng năng lượng 360 độ quanh bản thân bán kính 2.5m - 3.0m. Chiêu thức hộ thân giải vây khi bị nhiều người chơi bao vây.
5. **Archetype 5: Buff Tự Thân (Self Stance Buff):**
   * Kích hoạt tức thì không cần ngắm. Nhân vật bốc luồng khí nguyên tố, kích hoạt icon trên thanh Buff HUD đếm ngược thời gian.
6. **Archetype 6: Hào Quang Toàn Đội / Kết Giới Lãnh Địa (Team Wide Domain Aura):**
   * Tỏa vòng hào quang khổng lồ 6.5m – 8.5m. Toàn bộ thành viên trong Party / Bang hội đứng trong vòng đều nhận được bùa lợi (Hồi máu, Tăng công, Giảm sát thương nhận vào).

---

### 6.2. Chi Tiết 4 Loại Tác Dụng Chiêu Thức (`SkillCategory`)
1. **0: Sát Thương (Damage):** Trừ máu kẻ địch, phân định sát thương Đơn Mục Tiêu, Sát thương Diện Rộng, Sát thương Duy Trì (DoT) và Sát thương Chí Mạng.
2. **1: Hồi Máu (Heal HP):** Nảy số xanh lá `+HP`, cấp thiết cho sinh tồn khi hệ thống hồi máu tự nhiên bị siết chặt 40 giây/lần.
3. **2: Hồi Mana (Restore MP):** Nảy số xanh dương `+MP`, tiếp tế nhiên liệu chiến đấu cho đồng minh tung chiêu.
4. **3: Cường Hóa / Khống chế (Buff / CC):** Tăng tốc, tăng giáp, tạo khiên chắn; hoặc gây hiệu ứng Làm Chậm (Slow), Làm Choáng (Stun), Khóa Chiêu (Silence), Đẩy Lùi (Knockback).

---

### 6.3. Bảng Ma Trận Tương Tác Sáng Tạo ($6 \times 4$)

| Tầm Đánh (Archetype) | Sát Thương (Damage) | Hồi Máu (Heal HP) | Hồi Mana (Restore MP) | Cường Hóa / Khống Chế (Buff/CC) |
| :--- | :--- | :--- | :--- | :--- |
| **1. Bắn Thẳng** | *Hỏa Kiếm Khí* (Phóng tia xuyên thấu) | *Tia Sáng Trị Liệu* (Bắn tia hồi máu đồng đội trên đường) | *Tia Năng Lượng* (Bắn sạc Mana cho đồng minh từ xa) | *Băng Tiễn Tê Liệt* (Bắn trúng làm choáng kẻ thù 1.2s) |
| **2. Thả Đất / Xả Mưa** | *Mưa Thiên Thạch* (Nổ thiêu rụi vùng đất) | *Mưa Sinh Mệnh* (Vùng lá rơi tưới mát hồi máu liên tục) | *Suối Nguồn Ma Lực* (Khu vực hồi phục MP cho đồng đội) | *Đầm Lầy Trọng Lực* (Vùng bùn lầy giảm 70% tốc chạy địch) |
| **3. Hình Quạt** | *Bão Lửa Quạt* (Quét sạch kẻ thù trước mặt) | *Gió Xuân Thanh Mát* (Quạt làn gió hồi phục nhóm đồng minh) | *Luồng Khí Tụ Linh* (Quét hồi 1 lượng MP cho đội hình) | *Sóng Âm Đẩy Lùi* (Thổi văng toàn bộ kẻ địch về phía sau) |
| **4. Tỏa Quanh Thân** | *Bộc Phá Thánh Điển* (Nổ 360° tiêu diệt quái vây quanh)| *Thánh Quang Tỏa* (Hồi phục cấp cứu cho mọi người quanh mình)| *Ma Lực Trào Dâng* (Bùng nổ nạp đầy năng lượng cho tổ đội)| *Tiếng Gầm Khiêu Khích* (Ép toàn bộ quái quanh mình tấn công) |
| **5. Buff Tự Thân** | *Liệt Hỏa Kiếm Thể* (Cường hóa đòn đánh nảy thêm lửa) | *Dưỡng Mệnh Quyết* (Hồi phục 5% HP mỗi giây trong 6s) | *Hóa Linh Chú* (Giảm 50% tiêu hao Mana trong 12s) | *Hộ Thể Kim Cang* (Kháng toàn bộ hiệu ứng khống chế 4s) |
| **6. Hào Quang Đội** | *Hỏa Vực Diệt Thế* (Đốt cháy liên tục mọi kẻ địch trong 8m) | *Thánh Vực Chữa Lành* (Cả Bang hội trong 8m được hồi máu) | *Lãnh Địa Ma Pháp* (Cả Bang hội hồi Mana gấp 3 lần) | *Chiến Ca Hộ Quốc* (Toàn đội tăng 25% Giáp và 20% Công) |

---

## 7. QUY TRÌNH KỸ THUẬT & CÔNG CỤ SÁNG TẠO (IN-GAME STUDIO & TOOLING ROADMAP)

### 7.1. Cấu Trúc Dữ Liệu C# Mở Rộng (`SkillScrollData.cs`)
Hệ thống lưu trữ JSON được cập nhật trường dữ liệu chuẩn hóa:

```csharp
namespace GodlessFantasy.Combat
{
    [System.Serializable]
    public class SkillScrollData
    {
        public string id;                      // Định danh duy nhất (VD: "knight_x_angel_domain")
        public string scrollName;              // Tên chiêu (<= 4 từ, VD: "Thiên Thần Hộ Giới")
        public string requiredClass;           // "Kị Sĩ", "Pháp Sư", "Xạ Thủ", "Võ Sĩ", "Toàn Hệ"
        public string element;                 // "Hỏa", "Băng", "Lôi", "Phong", "Mộc", "Quang", "Ám"
        public string tierRank;                // "Phẩm X", "Phẩm S", "Phẩm A", "Phẩm B", "Phẩm C", "Phẩm D", "Phẩm E"
        public Color tierColor;                // Màu đại diện phẩm cấp

        // Chỉ số chiến đấu & Cân bằng
        public int manaCost;                   // Mana tiêu hao
        public float cooldown;                 // Thời gian hồi chiêu (giây)
        public int baseDamage;                 // Sát thương / Trị liệu gốc
        public float primaryScaling;           // Hệ số cộng hưởng chỉ số chính (% STR / INT / AGI)
        public float secondaryScaling;         // Hệ số cộng hưởng chỉ số phụ (% VIT / AGI)

        // Phân loại tầm đánh & tác dụng
        public int indicatorType;              // 1=Line, 2=Ground, 3=Cone, 4=Nova, 5=SelfBuff, 6=TeamDomain
        public int skillCategory;              // 0=Damage, 1=HealHP, 2=RestoreMP, 3=BuffCC
        public float castRange;                // Tầm thi triển (mét)
        public float areaRadius;               // Bán kính ảnh hưởng (mét)
        public float effectDuration;           // Thời gian hiệu lực (giây, cho Buff/DoT)

        // Bộ tham số hoạt ảnh thủ tục (Procedural Motion)
        public string baseShapeMask;           // "Orb", "Slash", "Arrow", "Wave", "Meteor", "Nova"
        public int trajectoryType;             // 0=Linear, 1=Sine, 2=Homing, 3=Lobbed, 4=Instant
        public float flySpeed;                 // Tốc độ bay (m/s)
        public bool hasTrail;                  // Có vệt sáng kéo dài không
        public bool hasScreenShake;            // Có rung chấn màn hình không

        // Nhận diện đồ họa tách lớp
        public string uniqueHotbarIcon;        // Icon độc nhất trên ô [1..4]
        public string scrollRuneIcon;          // 1 trong 7 ấn ký hệ trên cuộn giấy da túi đồ
        public string description;             // Mô tả chiêu thức
    }
}
```

---

### 7.2. Lộ Trình Triển Khai 4 Giai Đoạn (Execution Roadmap)

```
+---------------------------------------------------------------------------------------------------------+
|                                    LỘ TRÌNH TRIỂN KHAI HỆ THỐNG KỸ NĂNG                                 |
+--------------------+--------------------+---------------------+--------------------+--------------------+
| GIAI ĐOẠN 1        | GIAI ĐOẠN 2        | GIAI ĐOẠN 3         | GIAI ĐOẠN 4        | KẾT QUẢ ĐẦU RA     |
| (Database & Schema)| (VFX Shader Engine)| (Studio Tool Web/F1)| (Tích Hợp Gameplay)|                    |
+--------------------+--------------------+---------------------+--------------------+--------------------+
| • Mở rộng class    | • Xây dựng Shader  | • Nâng cấp          | • Tích hợp vào     | • Hệ thống 140     |
|   SkillScrollData. |   Ramp Tinting màu |   GodlessSkillStudio|   Player Combat HUD|   chiêu thức mượt  |
| • Nạp sẵn 20 Trục  |   theo 7 Hệ.       |   hỗ trợ thanh trượt| • Đồng bộ hotbar   |   mà không giật lag|
|   độc quyền +      | • Tạo 8 Prefabs    |   tham số đạn bay.  |   kéo thả túi đồ.  | • No-code Creator  |
|   10 Trục đa hệ.   |   Phôi Khối gốc.   | • Export JSON 1-click| • Cân bằng số liệu |   tùy biến vô hạn  |
+--------------------+--------------------+---------------------+--------------------+--------------------+
```

1. **Giai đoạn 1 — Database & Quy Hoạch Dữ Liệu (Hoàn thành ngay):**
   * Đưa toàn bộ cấu trúc 20 Trục Độc Quyền (X, S, A, B, C) và Trục Đa Hệ (D, E) vào `SkillDatabase.cs` và file JSON cấu hình.
   * Áp dụng quy tắc kiểm tra điều kiện kép (Chức nghiệp $\times$ Thuộc tính).

2. **Giai đoạn 2 — Xây Dựng Procedural VFX & Shader Engine:**
   * Viết Shader `Sprites-ElementalRamp` hỗ trợ thay đổi Color Palette theo 7 Thuộc tính tức thì (không tốn chi phí draw call).
   * Chuẩn hóa 8 Prefab đạn bay và hiệu ứng nổ mẫu trong thư mục `Resources/VFX/`.

3. **Giai đoạn 3 — Nâng Cấp Web & In-Game Skill Studio [F1]:**
   * Bổ sung mục chọn **Tham số đường đạn** (Tốc độ bay, Bán kính, Độ rung màn hình, Trail).
   * Hỗ trợ xem trước 140 hình thái nguyên tố theo thời gian thực (Live Preview).

4. **Giai đoạn 4 — Kiểm Thử Đấu Trường & Tối Ưu Hiệu Năng:**
   * Triển khai hệ thống Object Pooling cho đạn bay và hạt hiệu ứng, duy trì 60 FPS mượt mà ngay cả khi 10 người chơi cùng xả chiêu trong trận chiến Bang Hội.

---

---

## 9. CẬP NHẬT KỸ THUẬT & TRIỂN KHAI THỰC TẾ CHI TIẾT (ENGINE IMPLEMENTATION & COMBAT EXPANSION)

Nhằm hiện thực hóa 100% tầm nhìn và giải quyết triệt để các khúc mắc phát sinh trong quá trình thử nghiệm thực chiến, hệ thống đã được triển khai và hoàn thiện với các trụ cột kỹ thuật sau:

### 9.1. Quy Hoạch Tầm Đánh Thường & Cân Bằng Perk Bẩm Sinh 4 Hệ Phái

Trong các phiên bản ban đầu, tầm đánh thường của các hệ phái bị đồng nhất khiến đặc trưng chiến thuật bị mờ nhạt. Hệ thống đã tiến hành quy hoạch lại thông số cơ bản và perk bẩm sinh cho 4 hệ phái:

```
+---------------------------------------------------------------------------------------------------------+
+ CHỨC NGHIỆP      | TẦM ĐÁNH CƠ BẢN | TỐC ĐỘ ĐÁNH (ASPD) | PERK BẨM SINH ĐẶC TRƯNG     | ĐỊNH VỊ CHIẾN ĐẤU       +
+------------------+-----------------+--------------------+-----------------------------+-------------------------+
+ Kị Sĩ (Knight)   | 1.85m           | 0.75 giây / nhịp   | Giáp Thép Kiên Cố (+20 Giáp)| Đấu sĩ tiên phong       +
+ Võ Sĩ (Brawler)  | 1.25m           | 0.50 giây / nhịp   | Thể Phách Bạo Liệt (+15% STR| Đấm cận chiến siêu tốc, +
+                  | (ngắn nhất)     | (nhanh nhất game)  | dồn dập, chịu đòn bền bỉ)   | khống chế phá giáp      +
+ Pháp Sư (Mage)   | 5.50m           | 0.85 giây / nhịp   | Ma Lực Dồi Dào (+20% INT,   | Cấu rỉa tầm xa ma pháp, +
+                  |                 |                    | +50 Mana tối đa)            | sát thương diện rộng    +
+ Xạ Thủ (Archer)  | 7.50m           | 0.65 giây / nhịp   | Thiện Xạ Ưng Nhãn (Tầm xa   | Thả diều xa nhất bản đồ,+
+                  | (xa nhất game)  |                    | nhất, +15% Tốc chạy & Né)   | chí mạng sắc bén        +
+---------------------------------------------------------------------------------------------------------+
```

*Code tham chiếu:* [PlayerStats.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Player/PlayerStats.cs) và [BasicAttackController.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/BasicAttackController.cs).

---

### 9.2. Chuẩn Hóa Căn Giữa Phôi Bằng Procedural Pixel Masks (Loại Bỏ Lệch Pivot)

* **Hiện tượng lỗi cũ:** Khi sử dụng các sprite phôi tải từ bên ngoài hoặc cắt tự do, tâm xoay (pivot) không nằm ở trung tâm hình học `(0.5, 0.5)` làm cho các chiêu vệt chém (Slash) hoặc mũi tên (Arrow) bay lệch khỏi tâm con trỏ ngắm.
* **Giải pháp khắc phục:** 
  1. Xây dựng hàm tạo phôi toán học nội tại `ProceduralVFXEngine.CreateProceduralMask(shape)`.
  2. Toàn bộ 8 dạng phôi gốc (Orb, Slash, Arrow, Wave, Meteor, NovaRing, Pillar, Beam) được vẽ trên lưới 32x32 pixel với pivot cố định chính xác tuyệt đối ở `Vector2(0.5f, 0.5f)`.
  3. Mọi phép xoay hướng góc bay `Quaternion.Euler(0, 0, angle)` đều xoay quanh đúng tâm đạn, đảm bảo đường đạn luôn xuất phát và tiếp đất chuẩn xác 100%.

---

### 9.3. Cơ Chế Khống Chế Điểm Rơi AoE Rain (Chặn Văng Đạn Xuyên Màn Hình)

* **Hiện tượng lỗi cũ:** Đạn mưa thiên thạch (AoE Rain) rơi qua vòng tròn chỉ định mà không có điểm dừng, tiếp tục bay xuyên màn hình xuống tận thanh Hotbar UI gây lỗi thị giác nghiêm trọng.
* **Giải pháp khắc phục:**
  1. Bổ sung trường `TargetStopPosition` và `MaxDistance` vào `ProceduralProjectile`.
  2. Khi khoảng cách từ vị trí đạn tới điểm rơi mục tiêu $\le \text{Threshold}$ hoặc đã đi hết quãng đường giới hạn, đạn **lập tức dừng lại và phát nổ ngay tại vị trí chỉ định**.
  3. Kích hoạt hiệu ứng bộc phá hạt 2 tầng (`ProceduralBurstParticle`) và sóng xung kích mặt đất (`ImpactRing`).

#### Mở Rộng 4 Hướng Rơi (`rainFallAngle`) & 4 Kiểu Rơi (`rainFallStyle`):
* **Hướng Rơi (Fall Angle):**
  * `0`: Thẳng đứng 90° (Vertical Down).
  * `1`: Chéo Trái -30° (Slanted Leftward).
  * `2`: Chéo Phải +30° (Slanted Rightward).
  * `3`: Từ Người Chơi Vút Lên (Caster Stream — phóng từ nhân vật vút lên trời rồi cắm xuống tâm AoE).
* **Kiểu Rơi (Fall Style):**
  * `0`: Tản Mác (Random Scatter ngẫu nhiên trong bán kính).
  * `1`: Xoáy Ốc (Spiral Barrage cuốn quanh tâm).
  * `2`: Oanh Tạc Dồn Dập (Bombardment tập trung vào lõi tâm).
  * `3`: Sóng Quét (Sweeping Waves quét thành từng đợt ngang qua vùng đất).

*Code tham chiếu:* [ProceduralVFXEngine.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/ProceduralVFXEngine.cs).

---

### 9.4. Thư Viện Hoạt Ảnh Động 44 Frame Pixel Art & PixelVFXFrameAnimator

Nhằm mang lại sức sống và sự sinh động cho 140 biến thể kỹ năng, hệ thống đã sản xuất và cắt ghép tự động 44 Frame Sprite Pixel Art (11 chuỗi x 4 frames, chuẩn 16x16 pixel):
* **7 Hệ Nguyên Tố Cơ Bản:**
  1. Hỏa (Fire Flame Pulse): Ngọn lửa bập bùng chuyển động 4 nhịp.
  2. Băng (Ice Shard Drift): Tinh thể băng trôi dạt và lấp lánh.
  3. Lôi (Thunder Arc): Tia sét tích điện giật phóng liên hoàn.
  4. Quang (Light Radiance): Hào quang thánh thần mở rộng và tỏa sáng.
  5. Ám (Dark Void Swirl): Lốc xoáy hư vô u tối hút năng lượng.
  6. Phong (Wind Tornado): Lốc gió xoáy lộng thổi cuộn tròn.
  7. Mộc (Nature Bloom / Vine): Chồi non hoa lá vươn mình sinh sôi.
* **4 Chiêu Thức Hoạt Ảnh Đặc Biệt:**
  8. Cự Kiếm Cắm Đất (Holy Sword Impale): Kiếm thánh giáng thẳng xuống đất phát sáng.
  9. Nữ Thần Thăng Hoa (Goddess Ascension): Hình bóng nữ thần bay lên ban phát bảo hộ.
  10. Trận Pháp Mộc Nở Hoa (Flora Bloom Barrier): Vòng hoa cỏ đan kết kết giới phục hồi.
  11. Khiên Ma Trận Phản Chấn (Matrix Shield Reflection): Vòng lục giác xoay tròn kích hoạt phòng ngự.
* Bộ điều khiển `PixelVFXFrameAnimator` chạy độc lập với tốc độ 12 FPS, tự động tráo SpriteRenderer theo thời gian thực và tự hủy khi hết vòng đời.

---

### 9.5. Menu Testing 140 Kỹ Năng Độc Bản [F3] & Cài Đặt (Auto Chuyển Phái & Hệ)

Để hỗ trợ đội ngũ lập trình và kiểm thử (Dev & QA) trải nghiệm nhanh toàn bộ 140 hình thái VFX mà không cần phải thực hiện quy trình mở túi, roll ngẫu nhiên hay bị khóa bởi cơ chế hạn chế chức nghiệp:
* **Phương thức mở:**
  * Nhấn phím nóng **`[F3]`** bất kỳ lúc nào trong màn hình chơi.
  * Mở Menu **Cài Đặt** (`CombatHUD.cs` Drawer Tab 5) $\rightarrow$ Nhấn nút **⚡ TESTING 140 KỸ NĂNG & VFX [F3]**.
* **Bộ Lọc Đa Tầng:**
  * Hàng 1: Chọn Chức Nghiệp (Kị Sĩ / Pháp Sư / Xạ Thủ / Võ Sĩ).
  * Hàng 2: Chọn Hệ Nguyên Tố (Hỏa / Băng / Lôi / Quang / Ám / Phong / Mộc).
  * Hàng 3: Chọn Phẩm Cấp (C, B, A, S, X).
* **Cơ chế Auto Class & Element Tuning:**
  * Khi tester click vào nút **`[⚡ CHỌN & ĐỔI HỆ]`** của bất kỳ kỹ năng nào:
    1. Nhân vật của người chơi **tự động chuyển ngay sang Chức Nghiệp** của chiêu thức (`playerStats.SetCharacterClass()`).
    2. Nhân vật **tự động chuyển ngay sang Hệ Nguyên Tố** của chiêu thức (`playerStats.SetElement()`).
    3. Kỹ năng được **gán trực tiếp vào Hotbar Ô [1]** (`skillAimingController.EquipScroll(0, skill)`).
    4. Thêm 1 bản sao cuộn trục vào Túi đồ người chơi.
* **Nút `[🔥 TEST CAST]` Bắn Thử Trực Tiếp:**
  * Cho phép tester thi triển ngay lập tức kỹ năng ra chiến trường mà không cần căn chỉnh, tự động kích hoạt đúng dạng tâm ngắm (Line, AoE Rain, Cone, Nova, Self Aura, Domain Barrier).
* **Chiến lược Icon Fallback:**
  * 140 kỹ năng sử dụng icon cuộn trục theo 7 hệ nguyên tố làm fallback (`item_scroll_fire..wood`), đảm bảo tính mỹ thuật đồng bộ mà không cần phải vẽ thủ công 140 icon riêng biệt.

*Code tham chiếu:* [SkillTestingMatrixModal.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/SkillTestingMatrixModal.cs).

---

### 9.6. Tab 3 Live Inventory Inspector Trong `ItemCreatorModal.cs`

Bên cạnh việc tạo mới trang bị và kỹ năng, `ItemCreatorModal.cs` được mở rộng thêm Tab 3 cho phép can thiệp trực tiếp vào 24 ô hành trang thực tế của nhân vật đang chơi:
* **Cột 1 — Danh Sách 24 Ô Túi Đồ:** Hiển thị trực quan icon, tên, số lượng và phẩm cấp của từng ô đồ.
* **Cột 2 — Bảng Chỉnh Sửa Trực Tiếp (Live Editor):**
  * Cho phép sửa đổi Tên vật phẩm, Phẩm cấp (E..X), Sát thương (Damage), Giáp (Armor), HP, MP, các chỉ số thuộc tính (STR, INT, VIT, AGI).
  * Chỉnh sửa thông số đường đạn: Phôi khối (Base Shape), Quỹ đạo (Trajectory), Tốc độ bay (Fly Speed), Bán kính nổ (Explosion Radius), Hướng rơi (Fall Angle), Kiểu rơi (Fall Style).
  * Nút **`[💾 LƯU THAY ĐỔI VÀO Ô ĐỒ]`**: Cập nhật tức thì vào bộ nhớ game mà không cần reload scene.
* **Cột 3 — Live Preview & Action Buttons:**
  * Xem trước mô tả và chỉ số theo thời gian thực.
  * Nút **`[⚡ BẮN THỬ CHIÊU TRONG Ô]`**: Test cast trực tiếp chiêu thức từ ô đồ ra màn hình.
  * Nút **`[📋 SAO CHÉP Ô]`** và **`[🗑️ XÓA Ô]`**.

*Code tham chiếu:* [ItemCreatorModal.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/ItemCreatorModal.cs).

---

## 10. TỔNG KẾT & KẾT LUẬN KIẾN TRÚC

1. **Hoàn Thành Toàn Diện 140 Biến Thể:** Hệ thống Procedural Modular VFX Engine đã giải quyết triệt để bài toán kết hợp giữa 4 Chức Nghiệp $\times$ 7 Nguyên Tố $\times$ 5 Bậc Kỹ Năng Độc Quyền mà không tốn công sức vẽ tay hàng nghìn khung sprite rời rạc.
2. **Khắc Phục Hoàn Toàn Lỗi Đạn & Pivot:** Đạn mưa AoE Rain dừng chính xác bên trong vòng tròn chỉ định, phôi đạn 32x32 được căn giữa 100% loại bỏ hiện tượng lệch tâm ngắm.
3. **Tiện Ích Kiểm Thử Tối Đa:** Menu testing `[F3]` với tính năng tự động chuyển phái và thuộc tính giúp toàn bộ đội ngũ lập trình và thiết kế kiểm tra toàn diện hoạt ảnh và cân bằng thông số chỉ trong tích tắc.
4. **Hệ Thống Sẵn Sàng Vận Hành:** Toàn bộ mã nguồn C# và công cụ Web Studio HTML đã được đồng bộ hóa hoàn chỉnh, biên dịch thành công 0 lỗi, đặt nền móng vững chắc cho các bản cập nhật nội dung chiến đấu tiếp theo!

