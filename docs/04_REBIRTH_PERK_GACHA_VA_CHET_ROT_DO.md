# ĐẶC TẢ HỆ THỐNG: CHUYỂN SINH (REBIRTH), RƯƠNG THẾ GIỚI, 5 Ô BẢO VỆ, CHẾT RƠI ĐỒ, PERK GACHA & TITLE
**Mã tài liệu:** DOC-04  
**Dự án:** Godless Fantasy  
**Studio phát triển:** Squirrels Studio  

---

## 1. QUY TRÌNH CHUYỂN SINH (REBIRTH WORKFLOW AT LEVEL 100)

Khi nhân vật đạt Level 100, chức năng **Chuyển Sinh (Rebirth)** sẽ được kích hoạt tại tế đàn thế giới hoặc giao diện nhân vật. Đây là tính năng reset tiến trình để bước vào vòng đời sức mạnh mới.

```
+-------------------------------------------------------------------------------+
|                        QUY TRÌNH CHUYỂN SINH (REBIRTH FLOW)                   |
+-------------------------------------------------------------------------------+
| BƯỚC 1: Đạt Level 100 -> Mở cửa sổ Rebirth.                                   |
| BƯỚC 2: Người chơi chọn tối đa 2 MÓN BẢO LƯU (Trang bị hoặc Cuộn Kỹ Năng).    |
| BƯỚC 3: Xác nhận Chuyển Sinh:                                                 |
|         - Nhân vật hy sinh ngay tại tọa độ hiện tại.                          |
|         - Server tạo ra [REBIRTH VAULT] tại đúng tọa độ đó.                  |
|         - Cất 2 món bảo lưu vào Vault, khóa định danh (OwnerId).              |
| BƯỚC 4: RESET NHÂN VẬT:                                                       |
|         - Cấp độ trở về Level 1.                                              |
|         - Điểm thuộc tính & tiềm năng reset về mốc ban đầu.                   |
|         - Xóa sạch toàn bộ kho đồ & trang bị còn lại (trừ 5 ô bảo vệ vĩnh viễn)|
| BƯỚC 5: NHẬN THƯỞNG:                                                          |
|         - Cộng điểm Chuyển Sinh (Rebirth Points - RP) vĩnh viễn vào tài khoản.|
| BƯỚC 6: Hồi sinh tại Làng Tân thủ -> Tự thân hành hương về tọa độ lấy đồ!   |
+-------------------------------------------------------------------------------+
```

---

## 2. RƯƠNG CHUYỂN SINH (REBIRTH VAULT) - LƯU TRỮ THẾ GIỚI PHÍA SERVER

### 2.1. Bản Chất Kỹ Thuật
- **Không phải rương ảo client:** Rương chuyển sinh là một Entity thực thể độc lập được lưu trữ trực tiếp trong Database máy chủ (Authoritative World DB).
- **Vị trí địa lý:** Nằm tại chính xác `RegionId`, `ChunkId` và tọa độ $(X, Y)$ nơi người chơi bấm nút Chuyển Sinh.

### 2.2. Quy Tắc Bảo Mật & Tương Tác
- **Quyền sở hữu tuyệt đối:** Chỉ duy nhất nhân vật của người chơi (`OwnerCharacterId`) mới có quyền tương tác và mở khóa rương này.
- **Người chơi khác:** Người chơi khác đi ngang qua có thể nhìn thấy hình ảnh chiếc rương cổ tỏa ánh hào quang nhưng **tuyệt đối không thể phá hủy hay cướp đồ bên trong**.
- **Tính lâu dài (Persistence):** Rương sẽ tồn tại vĩnh viễn trên Server cho đến khi người chơi sau khi tái sinh từ cấp 1 tự mình đi bộ băng qua các vùng đất hiểm trở quay lại vị trí đó để mở và lấy lại 2 món bảo vật.
- **Tính toàn vẹn dữ liệu (Atomic Transaction):** Thao tác tạo rương và reset nhân vật được thực thi dưới dạng 1 Transaction cơ sở dữ liệu duy nhất. Nếu server mất điện hoặc đứt mạng đúng lúc bấm nút, hoặc thành công cả hai hoặc hoàn tác (Rollback), không bao giờ có trường hợp bị mất đồ oan.

---

## 3. CƠ CHẾ 5 Ô CƯỜNG HÓA VĨNH VIỄN (PERMANENT PROTECTED SLOTS)

Để cân bằng giữa sự khốc liệt của PvP và bảo vệ nỗ lực cày cuốc của game thủ, Godless Fantasy giới thiệu cơ chế **Khóa Ô Vĩnh Viễn Bằng Điểm Rebirth**:

```
+-------------------------------------------------------------------------+
|                  CƠ CHẾ BẢO VỆ 5 Ô VĨNH VIỄN (MAX 5 SLOTS)               |
+-------------------------------------------------------------------------+
| - Chi phí: 1 Điểm Rebirth Point (RP) mở khóa 1 Ô Bảo Vệ.                |
| - Giới hạn tối đa: Đúng 5 Ô cho toàn bộ nhân vật (Không thể vượt quá). |
| - Phạm vi áp dụng (Người chơi tự do chọn loại ô muốn bảo vệ):            |
|     * Ô Trang Bị (Vũ khí, Mũ, Áo, Quần, Giày)                           |
|     * Ô Kỹ Năng (Ô 1, 2, 3, 4)                                          |
|     * Ô Hành Trang Túi Đồ (Slot túi chỉ định)                           |
| - Hiệu lực bảo vệ:                                                      |
|     [1] KHI CHẾT NGOÀI SAFE ZONE: Đồ trong 5 ô này KHÔNG BAO GIỜ RƠI!   |
|     [2] KHI CHUYỂN SINH (REBIRTH): Đồ trong 5 ô này KHÔNG BỊ XÓA!       |
|     [3] KHI CHUYỂN CHỨC: Đồ trong 5 ô này VẪN ĐƯỢC GIỮ NGUYÊN!          |
+-------------------------------------------------------------------------+
```

*Lưu ý kỹ thuật:* Hệ thống bảo vệ theo **Vị Trí Của Ô (Slot ID)** chứ không bảo vệ theo từng ID món đồ cụ thể. Khi người chơi thay một món đồ mới vào ô đã được cường hóa bảo vệ, món đồ mới đó nghiễm nhiên thừa hưởng quyền miễn nhiễm rớt đồ.

---

## 4. HÌNH PHẠT TỬ VONG NGOÀI SAFE ZONE (DEATH PENALTY: 5% LOOT DROP)

Khi một nhân vật bị hạ gục bởi người chơi khác hoặc quái vật ở bên ngoài Safe Zone:

```
[ NHÂN VẬT TỬ VONG ]
       |
       v
[ QUÉT DANH SÁCH VẬT PHẨM ĐỦ ĐIỀU KIỆN RƠI ]
- Loại trừ: 5 Ô được cường hóa vĩnh viễn (Miễn nhiễm 100%)
- Loại trừ: Vật phẩm nhiệm vụ không thể giao dịch
- Lấy tổng số lượng vật phẩm còn lại trong túi & trang bị: N món
       |
       v
[ TÍNH TOÁN RƠI 5% SỐ LƯỢNG NGẪU NHIÊN ]
- Số lượng vật phẩm rơi = Làm tròn lên (N * 5%)
- Thuật toán bốc thăm ngẫu nhiên (Uniform Random) chọn ra các món
       |
       v
[ RƠI XUỐNG ĐẤT THẾ GIỚI ]
- Tạo các gói đồ (Loot Bags) văng xung quanh vị trí xác chết
- Bất kỳ ai (kẻ thủ ác, người qua đường, hoặc đồng đội) đều có thể nhặt tự do!
```

---

## 5. VÒNG QUAY BỊ ĐỘNG PERK GACHA (REBIRTH PERK SYSTEM)

Sử dụng **1 Rebirth Point (RP)** cho mỗi lần quay tại Đền Chuyển Sinh. Mỗi Perk là một kỹ năng bị động độc lập tăng các chỉ số nhân vật theo tỉ lệ phần trăm `%`.

### 5.1. Bảng Tỉ Lệ Độ Hiếm Gacha (Gacha Drop Rate Table)

| Bậc Phẩm Perk | Tên Bậc | Tỉ lệ xuất hiện | Số dòng chỉ số buff | Biên độ tăng % chỉ số |
| :---: | :---: | :---: | :---: | :---: |
| **Common** | Phổ Thông | 65.00% | 1 dòng chỉ số | +3% đến +6% |
| **Rare** | Hiếm | 23.50% | 1 - 2 dòng chỉ số | +7% đến +12% |
| **Epic** | Ưu Tú | 8.50% | 2 dòng chỉ số | +13% đến +20% |
| **Legendary** | Huyền Thoại | 2.50% | 2 - 3 dòng chỉ số | +21% đến +32% |
| **Mythic / Godless** | Thần Thoại | 0.50% | 3 - 4 dòng chỉ số + 1 Hiệu ứng đặc biệt | +33% đến +50% |

### 5.2. Danh Mục Các Perk Tiêu Biểu (Perk Pools)
1. **Trái Tim Sắt Thép (Iron Heart):** Tăng `%` HP tối đa.
2. **Hải Ma Vô Tận (Arcane Reservoir):** Tăng `%` Mana tối đa.
3. **Thần Hành Bộ Pháp (Windstrider):** Tăng `%` Tốc độ di chuyển cơ bản và giảm thời gian hồi Dash.
4. **Hộ Giáp Bất Diệt (Unbroken Aegis):** Tăng `%` Giáp vật lý và kháng ma lực.
5. **Tử Thần Nhãn (Reaper's Eye):** Tăng `%` Tỉ lệ bạo kích và sát thương chí mạng.
6. **Thuộc Tính Cuồng Bạo (Elemental Overload):** Tăng `%` Sức mạnh phát ra (Attribute Power) của hệ nhân vật.

---

## 6. HỆ THỐNG DANH HIỆU (TITLE SYSTEM)

Người chơi có thể sở hữu Danh hiệu thông qua các thành tựu lớn trong game. Danh hiệu hiển thị phía trên đầu nhân vật và mang lại chỉ số cộng dồn nhỏ:

| Tên Danh Hiệu (Title) | Điều kiện mở khóa | Chỉ số thưởng thêm |
| :--- | :--- | :--- |
| **Sứ Giả Khởi Nguyên (The Pioneer)** | Đạt Cấp độ 25 | +20 HP, +10 Mana |
| **Bậc Thầy Chuyển Sinh (Once Reborn)**| Hoàn thành Rebirth lần đầu tiên | +5% HP, +5% Mana vĩnh viễn |
| **Kẻ Săn Boss Cổ Đại (Ancient Slayer)**| Độc hành hạ gục Boss Thế Giới | +3% Tỉ lệ chí mạng, +15 Giáp |
| **Kẻ Tội Lỗi Tái Thế (Godless Outcast)**| Chuyển chức thành công Hệ Ẩn | +5% Tốc độ di chuyển, +25 Sức mạnh thuộc tính |
| **Bất Tử Chiến Thần (Undying)** | Sống sót qua 50 cuộc giao tranh PvP ngoài Safe Zone | +40 Giáp phòng ngự |
