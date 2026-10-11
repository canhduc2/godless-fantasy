# TỔNG QUAN TÀI LIỆU ĐẶC TẢ THIẾT KẾ GAME (GDD & TDD)
## DỰ ÁN: GODLESS FANTASY (2D TOP-DOWN MMORPG)
**Studio phát triển:** SquiRRels Studio  
**Phiên bản tài liệu:** v1.0.0  
**Ngày phát hành:** 2026-10-10  

---

## 📌 DANH MỤC TÀI LIỆU CHI TIẾT (DOCUMENT SITEMAP)

| Mã tài liệu | Tên tài liệu đặc tả | Nội dung chính |
| :--- | :--- | :--- |
| **DOC-01** | [01_KINH_TE_TIEN_TE_SHOP_VAT_PHAM.md](file:///D:/godless-fantasy/docs/01_KINH_TE_TIEN_TE_SHOP_VAT_PHAM.md) | Hệ thống 3 loại tiền tệ, NPC Shop, Mở rộng ô hành trang, Rương kho, **🔖 [MỤC THIẾT KẾ MỚI: Danh mục vật phẩm ngoài lề, tiêu hao, nguyên liệu]** |
| **DOC-02** | [02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md](file:///D:/godless-fantasy/docs/02_COMBAT_PVP_PARTY_FRIENDLY_FIRE.md) | PvP tự do, Friendly Fire (đánh trúng cả đồng đội), Safe Zone logic, Party chia EXP / Loot tự do, Cơ chế UX ngắm/hủy skill chống lặp |
| **DOC-03** | [03_HE_THONG_HE_THUOC_TINH_KY_NANG.md](file:///D:/godless-fantasy/docs/03_HE_THONG_HE_THUOC_TINH_KY_NANG.md) | 5 Hệ chiến đấu (4 hệ gốc + Hệ Ẩn), 7 Thuộc tính ma lực, Tiến trình Cấp 1–100 mở 4 ô skill, Kỹ năng Shift Dash, Phân cấp Cuộn trục kỹ năng X-E |
| **DOC-03B**| [GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md](file:///D:/godless-fantasy/docs/GDD_SKILL_SCROLL_ELEMENT_SYSTEM.md) | **[MỚI]** Kiến trúc Bí Kíp Cuộn Trục Túi Đồ (7 Ấn Ký) vs Icon Hotbar Độc Nhất, 4 Archetypes Tầm Đánh & Cơ Chế Import Ảnh Đồ Họa Ngoại Bộ |
| **DOC-04** | [04_REBIRTH_PERK_GACHA_VA_CHET_ROT_DO.md](file:///D:/godless-fantasy/docs/04_REBIRTH_PERK_GACHA_VA_CHET_ROT_DO.md) | Chuyển sinh Lv 100, Rebirth Vault (Rương 2 món lưu server), 5 Ô cường hóa vĩnh viễn không mất, Luật rơi 5% đồ khi chết, Perk Gacha, Title System |
| **DOC-05** | [05_THE_GIOI_MAP_NPC_QUAI_BOSS.md](file:///D:/godless-fantasy/docs/05_THE_GIOI_MAP_NPC_QUAI_BOSS.md) | Kiến trúc Thế giới World → Region → Chunk, Hệ thống NPC đầy đủ, Quái vật, Boss rơi đồ ngẫu nhiên, Cơ chế kích hoạt chuyển chức Hệ Ẩn cực hiếm |
| **DOC-06** | [06_GIAO_DIEN_UI_HUD_VA_CHAT.md](file:///D:/godless-fantasy/docs/06_GIAO_DIEN_UI_HUD_VA_CHAT.md) | Wireframe chi tiết HUD Top/Bottom/Left, Menu trượt 4 Tab, Hệ thống Chat (Khu vực 5s rate-limit, Thế giới 2 tin/ngày & 5 phút cooldown) |
| **DOC-07** | [07_QUYET_DINH_CONG_NGHE_TECH_STACK.md](file:///D:/godless-fantasy/docs/07_QUYET_DINH_CONG_NGHE_TECH_STACK.md) | Quyết định công nghệ đồ họa (HD Pixel Art + 2D Lighting), Game Engine (Unity vs Godot), Kiến trúc Server-Authoritative & MCP Code Toolings |
| **DOC-08** | [08_HUONG_DAN_CAI_DAT_MOI_TRUONG.md](file:///D:/godless-fantasy/docs/08_HUONG_DAN_CAI_DAT_MOI_TRUONG.md) | Hướng dẫn cài đặt môi trường phát triển (Pixelorama, SQLite 3, .NET 8 SDK, Unity 2D LTS/Unity 6 sang ổ D:\, Lộ trình Vertical Slice 5 bước) |

---

## 📂 BIÊN BẢN THẢO LUẬN & TIẾN ĐỘ ĐỊNH KỲ (PROJECT DISCUSSIONS)
*Cập nhật định kỳ theo định dạng `discussion_sothutu_noidung.md`:*

* 📄 **Thảo luận 01:** [discussion_01_skill_scroll_elements_combat_architecture.md](file:///D:/godless-fantasy/docs/discussions/discussion_01_skill_scroll_elements_combat_architecture.md) — *Kiến trúc Bí Kíp Cuộn Trục, 7 Hệ Thuộc Tính, 4 Chức Nghiệp & Xử lý tồn đọng kỹ thuật.*
* 📄 **Thảo luận 02:** [discussion_02_hp_mp_regeneration_buff_system_and_class_perks.md](file:///D:/godless-fantasy/docs/discussions/discussion_02_hp_mp_regeneration_buff_system_and_class_perks.md) — *Hồi Máu Khắc Khổ 40s (chống buff bẩn), Hồi Mana 3s, Perk 4 Hệ, Archetype Buff số 5 & UI Buff Widget.*
* 📄 **Thảo luận 03:** [discussion_03_support_skills_aoe_healing_and_ally_buffs.md](file:///D:/godless-fantasy/docs/discussions/discussion_03_support_skills_aoe_healing_and_ally_buffs.md) — *Kỹ Năng Hỗ Trợ, Mưa Hồi Máu/Mana Hệ Mộc Vùng Đất & Buff Đồng Đội.*
* 📄 **Thảo luận 04:** [discussion_04_class_vs_element_lockout_title_and_pet_evolution.md](file:///D:/godless-fantasy/docs/discussions/discussion_04_class_vs_element_lockout_title_and_pet_evolution.md) — *Phân biệt tuyệt đối Hệ (Class) vs Thuộc Tính (Element), Khóa kỹ năng 2 lớp, Hệ Thống Danh Hiệu (Title System) & Thú Cưng Tiến Hóa 3 Giai Đoạn.*
* 📄 **Thảo luận 05:** [discussion_05_food_drink_potions_and_consumables_system.md](file:///d:/godless-fantasy/docs/discussions/discussion_05_food_drink_potions_and_consumables_system.md) — *Hệ Thống Thức Ăn, Đồ Uống, Dược Phẩm (Potions HP/Mana) & Cơ Chế Phím Tắt Tự Động Thông Minh [Q] Ăn / [E] Uống.*
* 📄 **Thảo luận 06:** [discussion_06_guild_system_exp_skill6_team_buff_and_combat_vfx.md](file:///d:/godless-fantasy/docs/discussions/discussion_06_guild_system_exp_skill6_team_buff_and_combat_vfx.md) — *Hệ Thống Bang Hội (Guild), EXP & Cấp Độ, Kỹ Năng Số 6 (Team Buff 360°) & Nâng Cấp Combat VFX.*
* 📄 **Thảo luận 07:** [discussion_07_monster_ai_telegraph_boss_skills_and_vfx_anim_studio.md](file:///d:/godless-fantasy/docs/discussions/discussion_07_monster_ai_telegraph_boss_skills_and_vfx_anim_studio.md) — *Kế Hoạch Bổ Sung Gap: AI Quái Vật Tuần Tra (Wander/Leash), Boss Skills & Telegraph Báo Đòn Né Chiêu, Combat Signals Hai Chiều & VFX Animation Studio.*
* 📄 **Thảo luận 08:** [discussion_08_bottom_hud_studio_numeric_inputs_dash_rebind_and_inventory_full_protection.md](file:///d:/godless-fantasy/docs/discussions/discussion_08_bottom_hud_studio_numeric_inputs_dash_rebind_and_inventory_full_protection.md) — *Console HUD Đáy Màn Hình, Tái Định Danh Phím Lướt [D], Chống Tràn Hành Trang (24/24), Marquee Nối Đuôi Vô Hạn & Ô Điền Biến Chỉ Số Thật Trong No-Code Studio.*
* 📄 **Thảo luận 09:** [discussion_09_modular_layered_character_pipeline_and_guild_operations.md](file:///d:/godless-fantasy/docs/discussions/discussion_09_modular_layered_character_pipeline_and_guild_operations.md) — *Nâng Cấp Nghiệp Vụ Bang Hội (Pass Code, Slogan 50 ký tự, Cài Đặt Bang Chủ, Giải Tán 60s, Kích Người), Bảo Vệ Target Quái Vật Khi Rơi Đồ, & Báo Cáo Kiến Trúc Hệ Thống Đồ Họa Đa Lớp 8 Hướng (Modular Character Pipeline).*
* 📄 **Thảo luận 10:** [discussion_10_skill_axis_engine_140_vfx_variants_and_procedural_combat_pipeline.md](file:///d:/godless-fantasy/docs/discussions/discussion_10_skill_axis_engine_140_vfx_variants_and_procedural_combat_pipeline.md) — *Kiến Trúc Trục Kỹ Năng Chuyên Biệt (20 Trục Độc Quyền X/S/A/B/C + Trục Đa Hệ D/E), 140 Hình Thái VFX Nguyên Tố, Procedural Modular VFX Engine, Ma Trận Testing [F3] Tự Đổi Hệ, Căn Giữa Phôi Mask 32x32 & Live Inventory Inspector.*
* 📄 **Thảo luận 11:** [discussion_11_modular_base_body_male_female_and_pixellab_walk_cycle_pipeline.md](file:///d:/godless-fantasy/docs/discussions/discussion_11_modular_base_body_male_female_and_pixellab_walk_cycle_pipeline.md) — *Quy Hoạch Phôi Nhân Vật Gốc (Base Body Nam/Nữ Anime Cute 16-bit), Pipeline Tạo Hoạt Ảnh Bước Đi 4-Frame (PixelLab `walking-4-frames`), Chiến Thuật Đối Xứng Gương Tiết Kiệm Quota (12/39 gen) & Playbook Cho Agent CLI.*



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

6. **Hỗ Trợ Song Ngữ Toàn Diện (Bilingual Support - Anh / Việt):**
   - Mặc định là **Tiếng Việt**, có thể chuyển sang **English (Tiếng Anh)** trong Tab Cài Đặt (Settings).
   - Cơ chế đổi ngôn ngữ tức thì (Hot-swap), không cần khởi động lại game.

