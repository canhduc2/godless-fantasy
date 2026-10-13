# ⚙️ TÀI LIỆU ĐẶC TẢ HỆ THỐNG XƯỞNG SẢN XUẤT RIG 2D (CHIBI RIG WORKSHOP & PIXEL STUDIO)

> **Godless Fantasy** • Hệ thống Công cụ Thiết kế & Lắp ráp Khung xương Nhân vật 2D Đa Hướng (8 Directions)

---

## 1. Giới Thiệu Tổng Quan

**Chibi Rig Workshop & Pixel Studio** là công cụ trực quan (Web-based Local Studio) chuyên dụng cho đồ họa nhân vật pixel Chibi của dự án *Godless Fantasy*. 

Công cụ giải quyết trọn vẹn quy trình tạo mẫu và tinh chỉnh nhân vật:
1. **Lắp ghép từng mảnh bộ phận rời rạc** (`Head`, `Body/Torso`, `Arm L/R`, `Leg L/R`) vào khung chuẩn 64x64 pixel cho cả **2 Giới tính (Nữ & Nam)** trên toàn bộ **8 Hướng di chuyển**.
2. **Phân bổ thứ tự lớp (Z-Order / Sorting Order)** tự do và trực quan để các khớp nối tay, chân, thân, đầu đè lên nhau chuẩn xác, không bị hở khe rỗng hay lộ lỗi khớp tròn.
3. **Chỉnh sửa Pixel Art trực tiếp**: Chuột phải vào bất kỳ mảnh nào trên người để mở ngay tab vẽ pixel chi tiết (bút chì, tẩy, đổ màu, hút màu, hoàn tác, bảng màu chuẩn).
4. **Xuất tự động 1-Click vào Unity**: Nhấn nút `💾 LƯU CẤU HÌNH & XUẤT RA UNITY` sẽ tự động sinh và ghi đè toàn bộ 16 bộ rig (64x64 PNG sprites) trực tiếp vào thư mục Unity Resources để chạy runtime trong game.

---

## 2. Cách Khởi Động Công Cụ

Công cụ hoạt động bằng một máy chủ Python HTTP cục bộ siêu nhẹ, không cần cài đặt thêm thư viện ngoài:

```bash
# Khởi động máy chủ Studio
python scratch/rig_workshop/server.py
```

- **Địa chỉ truy cập**: Mở trình duyệt web tại `http://localhost:8080` (hoặc `http://127.0.0.1:8080`)
- **Tập tin cấu hình**: `scratch/rig_workshop/config.json` (tự động lưu khi nhấn Lưu)
- **Bản sao lưu gốc**: `scratch/rig_workshop/parts_backup/` (cho phép khôi phục bất cứ lúc nào)

---

## 3. Cấu Trúc Các Bộ Phận (Rig Parts Specification)

Mỗi nhân vật được tạo thành từ 6 mảnh xương độc lập được vẽ chuẩn pixel art:

| Tên Bộ Phận | Mã Sprite Unity | Mô Tả & Quy Tắc Lớp |
| :--- | :--- | :--- |
| **Đầu (Head)** | `idle_head.png` | Luôn nằm ở lớp cao nhất ($Z \approx 14$), ôm lấy cổ và thân áo. |
| **Thân / Áo (Body)** | `idle_body.png` | Giữ nguyên thân đầy đủ, áo và quần đè lên phần đùi trên của chân. |
| **Tay Trái (Arm Left)** | `idle_arm_left.png` | Khớp vai xoay tự nhiên, kết nối liền mạch với vai. |
| **Tay Phải (Arm Right)** | `idle_arm_right.png` | Hướng nhìn nghiêng có tay gần ($Z \approx 13$) và tay xa ($Z \approx 9$). |
| **Chân Trái (Leg Left)** | `idle_leg_left.png` | Khớp hông nằm dưới gấu quần ($Z \approx 10$), loại bỏ khớp tròn nhân tạo. |
| **Chân Phải (Leg Right)** | `idle_leg_right.png` | Đối xứng nhịp nhàng với chân trái. |
| **Toàn thân (Composite)**| `body.png` | Ảnh ghép toàn bộ 6 mảnh chuẩn 64x64 cho preview và avatar. |

---

## 4. Các Tính Năng Nổi Bật Trong Giao Diện

### 4.1. Tab 1: Lắp Ghép Xương (Rig Assembler)
- **Kéo thả tự do bằng chuột**: Click vào bộ phận bất kỳ trên canvas và rê chuột để dịch chuyển mượt mà theo lưới 1 pixel.
- **Vi chỉnh phím tắt**:
  - `Mũi tên` / `WASD`: Dịch 1 pixel.
  - `Shift + Mũi tên`: Dịch 5 pixel.
  - `PageUp / PageDown`: Tăng / Giảm Z-Index (thứ tự đè lớp).
  - `Ctrl + S`: Lưu cấu hình và xuất ra Unity ngay lập tức.
- **Quản lý danh sách lớp (Layers Panel)**:
  - Hiển thị từ trên xuống dưới theo thứ tự Z-Index.
  - Nút `▲` và `▼` để đổi thứ tự đè.
  - Nút `👁️` bật/tắt hiển thị.
  - Nút `🎨` mở trực tiếp mảnh đó trong Xưởng Vẽ Pixel.
- **Menu Chuột Phải (Context Menu)**:
  - Chuột phải vào bất kỳ mảnh nào trên nhân vật để hiển thị menu thao tác nhanh: *Chỉnh sửa chi tiết trong Xưởng Vẽ*, *Đưa lên/xuống 1 lớp*, *Lật ngang*, *Căn giữa*, *Đặt lại vị trí*.
- **Công cụ Đối Xứng & Đồng Bộ (Symmetry Tools)**:
  - `🎯 Căn giữa X (Trục 32)`: Căn tâm mảnh ngay trục giữa $X=32$.
  - `🪞 Đối xứng qua tâm`: Lật vị trí mảnh sang phía đối diện ($X = 64 - X - W$).
  - `👥 Đồng bộ 2 Tay`: Tự động đặt Tay Phải đối xứng hoàn hảo với Tay Trái.
  - `👥 Đồng bộ 2 Chân`: Tự động đặt Chân Phải đối xứng hoàn hảo với Chân Trái.
  - `🌐 Sao chép sang hướng đối diện`: Tự động lật và đồng bộ nguyên cả bộ dáng từ hướng Đông sang Tây (East ➔ West, South-East ➔ South-West, North-East ➔ North-West).
- **Dải Toàn Cảnh 8 Hướng Thời Gian Thực (Gallery Strip)**:
  - Cập nhật tức thì cả 8 hướng ở đáy màn hình khi bạn vừa dịch chuyển bất kỳ mảnh nào.
  - Click vào ô bất kỳ để nhảy thẳng đến hướng đó.

### 4.2. Tab 2: Xưởng Vẽ Pixel (Pixel Art Studio)
- Chuyển tab nhanh chóng khi nhấn chuột phải vào mảnh hoặc chọn nút `🎨 Sửa Mảnh Này`.
- **Bộ công cụ vẽ chuẩn**:
  - `✏️ Bút chì (Pencil)` [Phím B]
  - `🧽 Tẩy (Eraser)` [Phím E]
  - `🪣 Đổ màu (Bucket Fill)` [Phím G]
  - `👁️‍🗨️ Hút màu (Eyedropper)` [Phím I]
  - `↩️ Hoàn tác (Undo)` [Ctrl+Z] & `↪️ Làm lại (Redo)` [Ctrl+Y]
- **Bảng màu Chibi tuyển chọn**: Màu da sáng/tối, màu áo cam gradient, màu quần thô đen, màu tóc nâu hạt dẻ, viền tối và màu mắt anime.
- **Lưu 1-Click**: Nút `💾 LƯU MẢNH & QUAY LẠI GHÉP XƯƠNG` ghi đè file ảnh mảnh, tự động làm tươi bộ nhớ đệm và chuyển về tab Lắp Ghép Xương mà không làm mất trạng thái.

---

## 5. Tích Hợp Vào Mã Nguồn Unity Client

Tất cả các tệp xuất ra được đặt tại đường dẫn:
`client/GodlessFantasyClient/Assets/Resources/Characters/Rig/{gender}/{direction}/`

Mã nguồn điều khiển chuyển động thủ tục (`ProceduralCharacterRig2D.cs`) nạp và gán các sprite này vào các GameObject xương tương ứng:

```csharp
// Nạp sprite từng bộ phận từ thư mục Resources:
srHead.sprite = LoadRigSpr(gender, dir, "idle_head") ?? LoadRigSpr(gender, dir, "body");
srBody.sprite = LoadRigSpr(gender, dir, "idle_body") ?? LoadRigSpr(gender, dir, "body");
srArmL.sprite = LoadRigSpr(gender, dir, "idle_arm_left");
srArmR.sprite = LoadRigSpr(gender, dir, "idle_arm_right");
srLegL.sprite = LoadRigSpr(gender, dir, "idle_leg_left");
srLegR.sprite = LoadRigSpr(gender, dir, "idle_leg_right");
```

---

## 6. Danh Mục Tệp Tin Dự Án

```
godless-fantasy/
├── docs/
│   └── RIG_WORKSHOP_SPECIFICATION.md   # Tài liệu đặc tả hệ thống này
└── scratch/
    └── rig_workshop/
        ├── server.py                   # Máy chủ HTTP cục bộ & REST API (Port 8080)
        ├── export_rig.py               # Module dựng và xuất Sprite vào Unity
        ├── default_config.json         # Cấu hình vị trí mặc định ban đầu
        ├── config.json                 # Cấu hình đang lưu của người dùng
        ├── parts/                      # Thư viện ảnh sprite các mảnh rời rạc (.png)
        ├── parts_backup/               # Bản sao lưu gốc của các mảnh
        └── static/
            ├── index.html              # Giao diện chính 2 Tab
            ├── style.css               # Phong cách Dark Studio & Pixel Grid
            └── app.js                  # Engine xử lý tương tác, kéo thả, đối xứng, vẽ pixel
```
