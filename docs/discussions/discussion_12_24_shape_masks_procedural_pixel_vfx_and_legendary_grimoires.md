# Thảo Luận Kỹ Thuật 12: Mở Rộng Hệ Thống 24 Phôi Khối Kỹ Năng Pixel VFX, 8 Bí Kíp Huyền Thoại & Đồ Họa Điểm Lọc 32x32

## 1. Bối cảnh & Mục tiêu kỹ thuật
- **Nhu cầu người dùng:** Yêu cầu mở rộng mạnh mẽ sự đa dạng về mặt thị giác của kỹ năng trong Godless Fantasy, đặc biệt là các phôi kỹ năng cho hệ Mộc (cây cỏ trồi đất, xúc tu), hệ Băng (chông băng mọc từ đất), cùng các phôi đặc trưng huyền thoại như Rồng lửa, Phượng hoàng, Đầu lâu tử linh, Chiến chùy sấm sét, Huyết gai, Hố đen hư không, Bảo thuẫn, Phi đao thích khách.
- **Phong cách đồ họa:** Đảm bảo đồ họa pixel art chuẩn mực RPG (32x32 pixel, Point filtering - không làm nhòe mờ, sắc nét từng điểm ảnh, bảng màu 5 tầng theo nguyên tố).
- **Tính năng cần đồng bộ:**
  1. Engine VFX ([ProceduralVFXEngine.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/ProceduralVFXEngine.cs)) & Quản lý VFX ([SkillVFXManager.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillVFXManager.cs)).
  2. Dữ liệu kỹ năng ([SkillDatabase.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/SkillDatabase.cs)): Nâng cấp ma trận 140 kỹ năng và thêm 8 bí kíp huyền thoại.
  3. Giao diện chế tạo kỹ năng ([ItemCreatorModal.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/ItemCreatorModal.cs)).
  4. Bảng kiểm thử ma trận kỹ năng ([SkillTestingMatrixModal.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/SkillTestingMatrixModal.cs)) với live simulation và preview đạn đạo.
  5. Tài nguyên PNG + Meta file trong `Assets/Resources/`.

---

## 2. Bảng phân loại 24 phôi kỹ năng (`BaseShapeMask`)

| Index | Tên Enum | Tên hiển thị (Tiếng Việt) | Loại đạn đạo / Hành vi | Nguyên tố & Archetype đặc trưng |
|:---:|:---|:---|:---|:---|
| 0 | `Orb` | Cầu Năng Lượng | Tuyến tính / Phun tỏa | Hỏa, Quang, Ma pháp cơ bản |
| 1 | `Slash` | Nhát Chém Khí Kình | Vòng cung / Tuyến tính | Chiến binh, Phong kiếm |
| 2 | `Arrow` | Mũi Tên Ma Đạo | Tuyến tính xuyên thấu | Cung thủ, Tiễn đạo |
| 3 | `Wave` | Sóng Xung Kích | Quạt hình nón / Tỏa | Đấu sĩ chấn động, Thủy triều |
| 4 | `Meteor` | Thiên Thạch Khổng Lồ | Cầu vồng / Từ trên trời rơi | Hỏa tinh thạch, Hư không |
| 5 | `NovaRing` | Vòng Sáng Bộc Phá | Tỏa tròn 360 độ | Bộc phá, Sấm sét lan truyền |
| 6 | `Pillar` | Trụ Cột Năng Lượng | Trụ đứng giáng từ trời | Quang phạt, Thiên lôi |
| 7 | `Beam` | Chùm Tia Xuyên Thấu | Chiếu thẳng liên tục | Pháp sư năng lượng, Thần tích |
| 8 | `Tentacle` | Xúc Tu Uốn Lượn | Trồi đất / Vực sâu | Ám ma vương, Vực thẳm |
| 9 | `VineSprout` | Dây Leo Gai Nhọn | Trồi đất / Quấn chân | Mộc hệ thảo mộc, Rừng già |
| 10 | `IceSpikes` | Chông Băng Nhọn | Trồi đất / Đâm xé | Băng hệ hàn băng, Tuyết sơn |
| 11 | `FloraBloom` | Bông Hoa Độc Dược | Trồi đất / Tỏa bào tử | Mộc hệ độc dược, Dược sư |
| 12 | `FlameGeyser` | Cột Lửa Phun Trào | Trồi đất / Địa nhiệt | Hỏa diệm địa ngục, Núi lửa |
| 13 | `ThunderStrike` | Tia Sét Ziczac | Giáng từ trên trời | Lôi đế trảm, Cuồng lôi |
| 14 | `HolyCross` | Thánh Giá Quang Minh | Bay thẳng / Thánh tích | Thần thánh, Trị liệu giáo hội |
| 15 | `TornadoWhirl` | Lốc Xoáy Cuồng Phong | Xoay tròn / Di chuyển | Phong bão, Bão cát sa mạc |
| 16 | `Skull` | Đầu Lâu Tử Linh | Tự tìm mục tiêu (Homing) | Ám linh, Vu sư gọi hồn |
| 17 | `DragonHead` | Đầu Rồng Hỏa Long | Tuyến tính bộc phá lớn | Thượng cổ hỏa long, Chưởng lực |
| 18 | `Dagger` | Phi Đao Thích Khách | Bắn chùm (Shotgun) | Sát thủ ám sát, Phong đao |
| 19 | `Shield` | Kim Cang Bảo Thuẫn | Bảo vệ / Đẩy lùi | Hiệp sĩ phòng ngự, Kim chung tráo |
| 20 | `BloodDrop` | Huyết Ma Địa Ngục | Trồi đất / Huyết gai | Huyết ma giáo, Hút máu |
| 21 | `VoidVortex` | Hố Đen Hư Không | Xoáy tròn / Hút mục tiêu | Ma pháp không gian, Hắc ám |
| 22 | `GiantHammer` | Lôi Thần Trọng Chùy | Bay thẳng / Đập đất | Đấu sĩ thể thuật, Thần búa |
| 23 | `Phoenix` | Phượng Hoàng Lửa | Cầu vồng / Lượn cánh | Hỏa phượng hoàng, Niết bàn |

---

## 3. Danh sách 8 Bí Kíp Huyền Thoại Mới (`SkillDatabase.cs`)

1. **Hỏa Long Bộc Phá (`mage_s_draconic_breath`)**
   - **Phẩm cấp:** Phẩm S | **Hệ:** Hỏa | **Môn phái:** Pháp Sư.
   - **Phôi:** `DragonHead` (17) | **Đạn đạo:** Tuyến tính xuyên thấu.
   - **Mô tả:** Triệu hoán uy linh rồng lửa ngậm hỏa cầu lao thẳng về phía trước, thiêu rụi toàn bộ kẻ thù trên đường bay và phát nổ khi chạm đích.

2. **Phượng Hoàng Niết Bàn (`mage_x_phoenix_rebirth`)**
   - **Phẩm cấp:** Phẩm X (Thần Thoại) | **Hệ:** Hỏa | **Môn phái:** Pháp Sư.
   - **Phôi:** `Phoenix` (23) | **Đạn đạo:** Quỹ đạo cầu vồng (Lobbed Arc).
   - **Mô tả:** Phượng Hoàng lửa sải cánh bay vút lên rồi đáp xuống tạo vùng lửa niết bàn, thiêu rụi kẻ địch và tạo hào quang sinh mệnh.

3. **Tử Linh Oán Hồn (`mage_s_death_skull`)**
   - **Phẩm cấp:** Phẩm S | **Hệ:** Ám | **Môn phái:** Pháp Sư.
   - **Phôi:** `Skull` (16) | **Đạn đạo:** Tự tìm mục tiêu (Homing).
   - **Mô tả:** Bắn ra oán linh đầu lâu hắc ám lượn vòng truy đuổi kẻ địch gần nhất, gây sát thương ăn mòn linh hồn và làm chậm tốc chạy.

4. **Lôi Thần Trọng Chùy (`brawler_s_thunder_hammer`)**
   - **Phẩm cấp:** Phẩm S | **Hệ:** Lôi | **Môn phái:** Đấu Sĩ.
   - **Phôi:** `GiantHammer` (22) | **Đạn đạo:** Ném thẳng kết hợp choáng.
   - **Mô tả:** Ném chiến chùy sấm sét mang theo vạn cân lực đạo nện thẳng vào mục tiêu, gây choáng diện rộng và giật điện lan truyền.

5. **Huyết Ma Địa Ngục (`knight_s_crimson_blood_spikes`)**
   - **Phẩm cấp:** Phẩm S | **Hệ:** Ám | **Môn phái:** Hiệp Sĩ.
   - **Phôi:** `BloodDrop` (20) | **Đạn đạo:** Trồi đất (Ground Eruption).
   - **Mô tả:** Đập mạnh vũ khí xuống đất, triệu hồi hàng loạt chông huyết ma đâm thốc từ dưới lòng đất lên xé rách kẻ địch và hồi phục sinh lực.

6. **Hư Không Thôn Tính (`mage_x_void_blackhole`)**
   - **Phẩm cấp:** Phẩm X (Thần Thoại) | **Hệ:** Ám | **Môn phái:** Pháp Sư.
   - **Phôi:** `VoidVortex` (21) | **Đạn đạo:** Quỹ đạo xoáy / Hút mục tiêu.
   - **Mô tả:** Mở ra hố đen hư không tại vị trí chỉ định, liên tục hút kẻ thù xung quanh vào tâm và phát nổ nghiền nát vật chất.

7. **Kim Cang Hộ Thuẫn (`knight_a_aegis_bulwark`)**
   - **Phẩm cấp:** Phẩm A | **Hệ:** Quang | **Môn phái:** Hiệp Sĩ.
   - **Phôi:** `Shield` (19) | **Đạn đạo:** Hộ thể / Đẩy lùi.
   - **Mô tả:** Khởi động thần thuẫn ánh sáng bao bọc bản thân, tăng mạnh giáp phòng ngự và phản chấn kẻ địch lao tới.

8. **Ám Ảnh Phi Đao (`archer_s_flying_daggers`)**
   - **Phẩm cấp:** Phẩm S | **Hệ:** Phong | **Môn phái:** Cung Thủ.
   - **Phôi:** `Dagger` (18) | **Đạn đạo:** Chùm nón (Shotgun Spread 5 tia).
   - **Mô tả:** Phóng ra 5 phi đao sắc bén tỏa hình quạt với tốc độ cực nhanh, gây sát thương chí mạng và hiệu ứng chảy máu.

---

## 4. Pipeline Đồ Họa Pixel Art & Tối Ưu Hóa Asset Unity

### 4.1. Quy chuẩn xuất Texture 32x32 Point-Filtered
- Tất cả các file sprite được lưu trữ tại `client/GodlessFantasyClient/Assets/Resources/`:
  - 24 file phôi: `vfx_mask_orb.png`, `vfx_mask_slash.png`, ..., `vfx_mask_phoenix.png`.
  - 8 file icon kỹ năng: `skill_dragon_breath.png`, ..., `skill_flying_daggers.png`.
- Từng file đều được đính kèm file cấu hình `.meta` chuẩn Unity Sprite:
  ```yaml
  TextureImporter:
    spritePixelsToUnits: 32
    filterMode: 0 # Point (no filter)
    textureCompression: 0 # Uncompressed
  ```
- **Lợi ích:** Đảm bảo khi Unity render trong Canvas UI hoặc SpriteRenderer 2D, các cạnh pixel không bị mờ nhòe (anti-aliasing mờ), giữ trọn vẹn nét sắc sảo của phong cách pixel 16-bit / 32-bit.

### 4.2. Khả năng Fallback Procedural Math độc lập
- Trong [ProceduralVFXEngine.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/Combat/ProceduralVFXEngine.cs) và [SkillTestingMatrixModal.cs](file:///d:/godless-fantasy/client/GodlessFantasyClient/Assets/Scripts/UI/SkillTestingMatrixModal.cs), hàm `CreateProceduralMask(shape)` đã cài đặt thuật toán giải tích hình học điểm ảnh (Coordinate-based pixel math) cho trọn bộ 24 phôi:
  - Tự động fallback sinh `Texture2D` runtime nếu thiếu file ảnh tài nguyên tĩnh.
  - Phù hợp tuyệt đối cho các hệ thống dynamic loading và modding.

---

## 5. Kiểm thử & Kết quả
- **Kiểm tra biên dịch:** `dotnet build` đạt **0 Errors** trên toàn bộ Client.
- **Tương thích UI:** Modal phím tắt `[F3]` hiển thị đầy đủ tên tiếng Việt, icon, và hiệu ứng của 24 phôi và 140+ kỹ năng; modal chế tạo trang bị `[C]` cho phép thợ rèn chọn đủ 24 loại phôi hạt nhân khi chế tạo bí kíp.
