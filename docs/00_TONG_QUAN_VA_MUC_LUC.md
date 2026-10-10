# TỔNG QUAN TÀI LIỆU ĐẶC TẢ THIẾT KẾ GAME (GDD & TDD)
## DỰ ÁN: GODLESS FANTASY (2D TOP-DOWN MMORPG)
**Studio phát triển:** Squirrels Studio  
**Phiên bản tài liệu:** v1.0.0  
**Ngày phát hành:** 2026-10-10  

---

## 📌 DANH MỤC TÀI LIỆU CHI TIẾT (DOCUMENT SITEMAP)

| Mã tài liệu | Tên tài liệu đặc tả | Nội dung chính |
| :--- | :--- | :--- |
| **DOC-01** | [01_KINH_TE_TIEN_TE_SHOP_VAT_PHAM.md](file:///D:/godless-fantasy/docs/01_KINH_TE_TIEN_TE_SHOP_VAT_PHAM.md) | Hệ thống 3 loại tiền tệ, NPC Shop, Mở rộng ô hành trang, Rương kho, **🔖 [MỤC THIẾT KẾ MỚI: Danh mục vật phẩm ngoài lề, tiêu hao, nguyên liệu]** |
| **DOC-02** | [02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md](file:///D:/godless-fantasy/docs/02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md) | PvP tự do, Friendly Fire (đánh trúng cả đồng đội), Safe Zone logic, Party chia EXP / Loot tự do, Cơ chế UX ngắm/hủy skill chống lặp |
| **DOC-03** | [03_HE_THONG_HE_THUOC_TINH_KY_NANG.md](file:///D:/godless-fantasy/docs/03_HE_THONG_HE_THUOC_TINH_KY_NANG.md) | 5 Hệ chiến đấu (4 hệ gốc + Hệ Ẩn), 6 Thuộc tính ma lực, Tiến trình Cấp 1–100 mở 4 ô skill, Kỹ năng Shift Dash, Phân cấp Cuộn trục kỹ năng X-E |
| **DOC-04** | [04_REBIRTH_PERK_GACHA_VA_CHET_ROT_DO.md](file:///D:/godless-fantasy/docs/04_REBIRTH_PERK_GACHA_VA_CHET_ROT_DO.md) | Chuyển sinh Lv 100, Rebirth Vault (Rương 2 món lưu server), 5 Ô cường hóa vĩnh viễn không mất, Luật rơi 5% đồ khi chết, Perk Gacha, Title System |
| **DOC-05** | [05_THE_GIOI_MAP_NPC_QUAI_BOSS.md](file:///D:/godless-fantasy/docs/05_THE_GIOI_MAP_NPC_QUAI_BOSS.md) | Kiến trúc Thế giới World → Region → Chunk, Hệ thống NPC đầy đủ, Quái vật, Boss rơi đồ ngẫu nhiên, Cơ chế kích hoạt chuyển chức Hệ Ẩn cực hiếm |
| **DOC-06** | [06_GIAO_DIEN_UI_HUD_VA_CHAT.md](file:///D:/godless-fantasy/docs/06_GIAO_DIEN_UI_HUD_VA_CHAT.md) | Wireframe chi tiết HUD Top/Bottom/Left, Menu trượt 4 Tab, Hệ thống Chat (Khu vực 5s rate-limit, Thế giới 2 tin/ngày & 5 phút cooldown) |

---

## 🎯 CÁC TRỤ CỘT THIẾT KẾ CỐT LÕI (CORE PILLARS)

1. **Chiến Đấu Chân Thật Tuyệt Đối (No Ghost Shields):**
   - Không có cơ chế miễn nhiễm sát thương giữa đồng đội trong Party/Alliance. Chưởng trúng đồng đội là nhận trọn sát thương và hiệu ứng khống chế.
   - Kỹ năng dạng định hướng (Skillshot MOBA style), đòi hỏi kỹ năng ngắm bắn, giữ vị trí và tính toán chiến thuật cao.

2. **Rủi Ro Đi Kèm Cơ Hội (High Risk, Controlled Loss):**
   - Chết ngoài vùng an toàn (Safe Zone) sẽ bị rơi 5% số lượng vật phẩm ngẫu nhiên từ hành trang.
   - Cơ chế bảo vệ tối thượng: Cho phép đầu tư điểm Rebirth Point (RP) để khóa vĩnh viễn tối đa 5 ô (trang bị/kỹ năng/kho đồ) không bao giờ bị rơi khi chết và không bị xóa khi Chuyển Sinh.

3. **Vòng Đời Chuyển Sinh Vĩ Đại (Infinite Progression Loop):**
   - Đạt Cấp 100 mở khóa Rebirth. Nhân vật reset về Cấp 1, toàn bộ hành trang bị thanh trừng, nhưng được quyền cất 2 món đồ/kỹ năng quý nhất vào **Rebirth Vault** nằm tại chính tọa độ hy sinh.
   - Tích lũy Rebirth Points để quay Perk bị động (Gacha), đổi thuộc tính, cường hóa ô vĩnh viễn.

4. **Hệ Ẩn Độc Nhất (Secret Class - Heretic):**
   - Kết hợp giữa Pháp Sư và 1 hệ ngẫu nhiên.
   - Sở hữu hiệu ứng cấm đối thủ dùng chiêu (Mana Silence / Skill Lock) nhưng phải trả giá bằng việc giảm 30% tổng lượng Mana và chiêu thức ngốn nhiều Mana hơn.

5. **Thế Giới Mở Khổng Lồ (Data-Driven World Streaming):**
   - Kiến trúc World Map → Region → Chunk đảm bảo bản đồ cực kỳ rộng lớn, hỗ trợ khám phá không giới hạn mà không gây sập tải server hay client.
