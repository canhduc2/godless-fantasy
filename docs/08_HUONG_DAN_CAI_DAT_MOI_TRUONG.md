# HƯỚNG DẪN CÀI ĐẶT MÔI TRƯỜNG PHÁT TRIỂN (DEVELOPMENT SETUP GUIDE)
## DỰ ÁN: GODLESS FANTASY (2D TOP-DOWN MMORPG)
**Studio:** SquiRRels Studio  
**Mã tài liệu:** DOC-08  
**Biên soạn & Phụ trách:** Lead Dev / SquiRRels Studio  

---

Chào anh em trong team và mọi người tham gia phát triển dự án **Godless Fantasy**!

Để dự án của **SquiRRels Studio** vận hành mượt mà, đồng bộ và tránh tối đa xung đột môi trường hay nặng ổ cứng máy tính, anh viết tài liệu này hướng dẫn chi tiết cho mọi người từng bước chuẩn bị công cụ và cài đặt môi trường làm việc chuẩn chỉ nhất. Mọi người đọc kỹ và làm theo đúng các bước dưới đây nhé!

---

## 1. TỔNG QUAN BỘ CÔNG CỤ & LỘ TRÌNH VERTICAL SLICE 5 BƯỚC

### 1.1. Bộ Công Cụ Tiêu Chuẩn "Nhẹ - Xịn - Mượt"
- **Công cụ vẽ chính thức:** **[Pixelorama](https://github.com/Orama-Interactive/Pixelorama/releases)**  
  *(Mọi người tải bản **Pixelorama-Windows-64bit** về máy nhé, cực kỳ chuẩn bài! Bản này siêu nhẹ ~140MB, giải nén là chạy ngay không cần cài đặt lằng nhằng, hỗ trợ layer, animation và xuất Sprite Sheet 32x32 mượt mà).*
- **Database:** **SQLite 3 (All-In-One)**  
  *Toàn bộ dữ liệu nhân vật, rương thế giới Rebirth Vault, tỷ lệ rơi đồ nằm gọn trong **1 file `.db` duy nhất**, không cần cài đặt SQL Server hay PostgreSQL nặng nề, tốc độ đọc ghi cực nhanh và an toàn tuyệt đối.*
- **Game Engine & Client:** **Unity 2D LTS (C#)** kết hợp Universal Render Pipeline (URP 2D) để xử lý ánh sáng ma thuật và hiệu ứng đỉnh cao.
- **Backend & Logic Testing:** **C# .NET 8.0 SDK (LTS)** bản ổn định dài hạn của Microsoft.

---

### 1.2. Lộ Trình Vertical Slice 5 Bước (Mục Tiêu Giai Đoạn Đầu)
Anh nhắc lại tôn chỉ phát triển của team mình: **Bắt đầu bằng bản demo tinh gọn**, tập trung làm thật mượt vòng lặp điều khiển và chiến đấu cốt lõi trước khi mở rộng ra thế giới MMORPG rộng lớn:

```
[BƯỚC 1] Di chuyển nhân vật mượt mà:
         - Click Chuột Trái để di chuyển tự tìm đường (Click-to-Move).
         - Cụm phím W, A, S, D ghi đè ngay lập tức để né đòn thủ công.

[BƯỚC 2] Kỹ năng Dash cơ động:
         - Nhấn Shift để lướt thẳng theo hướng con trỏ chuột (tiêu hao 20 Mana, cooldown 3.5s).

[BƯỚC 3] Cơ chế ngắm & xả chiêu Two-Tap (Vừa chạy né vừa ngắm):
         - Bấm phím 1 lần đầu: Bật chế độ ngắm (tâm ngắm lia theo chuột).
         - Click Chuột Trái chạy né đòn thoải mái, tâm ngắm vẫn bám chuột liên tục.
         - Bấm phím 1 lần nữa: Xác nhận thi triển đòn đánh.
         - Click Chuột Phải: Hủy ngắm ngay lập tức (không mất Mana, không hồi chiêu).

[BƯỚC 4] Combat cơ bản & Friendly Fire:
         - Đặt 2 nhân vật vào map chiến đấu.
         - Đánh trúng nhau nhận 100% sát thương thật (Friendly Fire không khoan nhượng).
         - Đi vào vùng Safe Zone: Miễn nhiễm sát thương PvP.

[BƯỚC 5] Kết nối Server Authoritative & Multiplayer thử nghiệm:
         - Server đồng bộ vị trí, kiểm tra tầm đánh tại thời điểm xả chiêu, trừ Mana và xác nhận trúng đòn.

=> Khi vòng lặp chiến đấu 5 bước này đã ngon lành, team mình mới mở rộng sang Túi đồ, Boss thế giới, Rebirth Vault và Streaming Map!
```

---

## 2. 🔗 LINK TẢI & HƯỚNG DẪN CÀI ĐẶT CÔNG CỤ (CHỌN CÀI SANG Ổ D:)

### A. C# .NET SDK (Dùng để biên dịch, kiểm thử và chạy Server)
- **Phiên bản:** **.NET 8.0 SDK (LTS)** — Bản ổn định dài hạn của Microsoft.
- **Link tải trực tiếp:** [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **Cách tải:** Ở cột bên trái **".NET SDK 8.0.xxx"**, bấm vào link **Installers: `x64`** (cho Windows 64-bit).
- **Vị trí cài:** File cài đặt chỉ khoảng **~200MB**, anh em cứ để mặc định cài vào `C:\Program Files\dotnet` để các công cụ và terminal tự nhận diện lệnh `dotnet`.
- **Kiểm tra sau khi cài:** Mở Terminal / PowerShell gõ:
  ```powershell
  dotnet --version
  ```
  Nếu xuất hiện phiên bản dạng `8.0.xxx` là chuẩn bài!

---

### B. Unity 2D LTS (Cách cài đặt sang ổ D:\ để không đầy ổ C:)

Dung lượng bộ cài đặt Unity khá lớn (từ 4 đến 6 GB), do đó mọi người làm theo đúng 3 bước dưới đây để điều hướng toàn bộ tải và cài đặt sang ổ **`D:\`**, giữ cho ổ hệ điều hành `C:\` luôn nhẹ và thông thoáng:

#### Bước 1: Tải Unity Hub
- **Link tải:** [https://unity.com/download](https://unity.com/download)
- Tải file `UnityHubSetup.exe` và cài đặt bình thường (Unity Hub chỉ nặng vài chục MB).

#### Bước 2: Đổi vị trí lưu trữ sang ổ D:\ trong Unity Hub
1. Mở phần mềm **Unity Hub**.
2. Bấm vào biểu tượng **Bánh răng (Preferences)** ở góc trên cùng bên trái.
3. Chọn mục **Installs** ở danh sách bên trái:
   - **Installs location:** Bấm nút **Browse** đổi đường dẫn thành `D:\Unity\Editor` *(Tạo thư mục `Unity\Editor` trên ổ D nếu chưa có)*.
   - **Downloads location:** Bấm nút **Browse** đổi đường dẫn thành `D:\Unity\Downloads` *(Tạo thư mục `Unity\Downloads` trên ổ D)*.
4. Bấm **Save**.

#### Bước 3: Cài đặt bản Unity 2D LTS
1. Trong Unity Hub, bấm vào mục **Installs** ở thanh menu chính $\rightarrow$ Bấm nút **Install Editor** (ở góc trên bên phải).
2. Chọn phiên bản có nhãn **LTS (Long Term Support)**:
   - **Khuyên dùng:** **Unity 2022.3 LTS** *(hoặc Unity 6 LTS)*.
   - *(Nếu không thấy trong danh sách Hub, mọi người bấm trực tiếp vào kho lưu trữ: [https://unity.com/releases/editor/archive](https://unity.com/releases/editor/archive) và bấm nút "Unity Hub" màu xanh bên cạnh bản 2022.3 LTS)*.
3. Ở bước chọn **Modules** đi kèm:
   - Tích chọn: **Microsoft Visual Studio Community** *(hoặc Visual Studio Code nếu mọi người đã có sẵn trên máy)*.
   - Tích chọn: **Documentation** *(tùy chọn)*.
4. Bấm **Install** và Unity sẽ tự động tải thẳng vào ổ `D:\` của mọi người!

---

### C. Công Cụ Vẽ Pixel Art Chính Thức: Pixelorama
- **Link tải bản phát hành chính thức:** [https://github.com/Orama-Interactive/Pixelorama/releases](https://github.com/Orama-Interactive/Pixelorama/releases)
- **Tải file:** Tải file `Pixelorama-Windows-64bit.zip` về máy.
- **Cách dùng:** Giải nén vào thư mục làm việc (ví dụ `D:\Tools\Pixelorama`), chạy trực tiếp `Pixelorama.exe` không cần cài đặt.
- **Quy chuẩn đồ họa của SquiRRels Studio:**
  - Kích thước Tile địa hình & Sprite nhân vật chuẩn: **32x32 pixel**.
  - Luôn lưu kèm file gốc project `.pxo` và xuất ra file ảnh `.png` (Sprite Sheet) trong suốt.

---

### D. Hệ Thống Database: SQLite 3 (All-In-One)
- Toàn bộ cơ sở dữ liệu của server và game được đóng gói trong một file dữ liệu duy nhất (`godless_fantasy.db`).
- Không cần cài DBMS service phức tạp. Để xem và debug dữ liệu trực tiếp dưới máy cục bộ, mọi người có thể dùng công cụ trực quan miễn phí:
  - **DB Browser for SQLite:** [https://sqlitebrowser.org/dl/](https://sqlitebrowser.org/dl/) (Tải bản Portable .zip, giải nén là xem/sửa được DB ngay).

---

### E. Tải Mã Nguồn & Bắt Đầu Làm Việc (Git Clone)
Mở PowerShell hoặc Git Bash lên và clone repo chính thức của studio về máy:
```powershell
git clone https://github.com/canhduc2/godless-fantasy.git
```
*(Repo đã được anh cấu hình file `.gitignore` đầy đủ cho cả Unity, SQLite và các thư mục đồ họa riêng tư, mọi người cứ yên tâm làm việc mà không sợ push file rác lên repo).*

---

## 3. THÔNG TIN HỖ TRỢ & LIÊN HỆ

Mọi người gặp bất kỳ vướng mắc nào trong quá trình tải và cài đặt cứ ping trực tiếp cho anh hoặc gửi phản hồi:
- **Studio:** SquiRRels Studio
- **Người phụ trách:** [canhduc2](https://github.com/canhduc2)
- **Email:** `0206canh2@gmail.com`
- **GitHub Repository:** [canhduc2/godless-fantasy](https://github.com/canhduc2/godless-fantasy)

**Chúc anh em cài đặt suôn sẻ, chuẩn bị sẵn sàng để chúng ta cùng nhau bứt phá dự án Godless Fantasy! 🚀**
