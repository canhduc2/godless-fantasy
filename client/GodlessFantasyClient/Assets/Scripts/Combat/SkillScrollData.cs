using System;
using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Dữ Liệu Cuộn Trục Kỹ Năng (Skill Scroll Item Data):
    /// - Trong Godless Fantasy, mỗi kỹ năng thực chất là một Cuộn Trục (Bí Kíp) trong Hành Trang/Túi Đồ.
    /// - Người chơi có thể KÉO THẢ (Drag & Drop) cuộn trục từ túi đồ hoặc kho bí kíp vào các ô [1], [2], [3], [4] trên thanh phím nóng.
    /// - Có phân cấp phẩm (E, D, C, B, A, S, X) chuẩn GDD DOC-03 & DOC-04.
    /// </summary>
    [Serializable]
    public class SkillScrollData
    {
        public string id;
        public string scrollName;
        public string requiredClass;     // Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ, Toàn Hệ
        public string tierRank;          // E, D, C, B, A, S, X
        public Color tierColor;
        public string element;           // Ánh Sáng, Bóng Tối, Phong, Mộc, Băng, Hỏa, Lôi
        public int manaCost;
        public float cooldown;
        public int baseDamage;
        public float intelligenceScaling;
        public int indicatorType;        // 1=Line, 2=Ground AoE, 3=Cone Arc, 4=Self Nova, 5=Self Buff
        public int skillCategory;        // 0=Damage, 1=Heal HP, 2=Heal Mana, 3=Buff Stat
        public float castRange;          // Tầm đánh / Cự ly thi triển (mét)
        public string iconName;          // Icon Độc Nhất trên thanh Hotbar [1..4]
        public string scrollIconName;    // Icon Cuộn Trục mang Ấn Ký Hệ trong Túi Đồ
        public string customVfxSprite;   // (Tùy chọn) Đường dẫn sprite đạn bay/hoạt ảnh riêng
        public string description;

        // Thống số Hoạt Ảnh Thủ Tục & Đạn Đạo 140 Biến Thể (Procedural Ballistics - Discussion 10)
        public int baseShapeMask = 0;       // 0=Orb, 1=Slash, 2=Arrow, 3=Wave, 4=Meteor, 5=NovaRing, 6=Pillar, 7=Beam
        public int trajectoryType = 0;      // 0=Linear, 1=SineWave, 2=Homing, 3=LobbedArc, 4=Boomerang, 5=Instant
        public float flySpeed = 16.0f;      // Tốc độ bay (m/s)
        public float explosionRadius = 1.8f;// Bán kính nổ hạt Pixel Impact Burst
        public bool hasTrail = true;        // Bật vệt đuôi năng lượng TrailRenderer
        public bool hasScreenShake = false; // Rung màn hình khi va chạm

        // Tùy Biến Mưa Kỹ Năng AoE (Rain Fall Angle & Style):
        public int rainFallAngle = 0;       // 0=Thẳng Đứng 90°, 1=Chéo Trái 60°, 2=Chéo Phải 60°, 3=Từ Hướng Người Chơi
        public int rainFallStyle = 0;       // 0=Tản Mác (Random), 1=Xoáy Ốc (Spiral), 2=Oanh Tạc Cùng Lúc (Carpet Bomb), 3=Sóng Quét (Sweeping)

        public bool IsSupport => skillCategory != 0;
        public string CategoryName
        {
            get
            {
                switch (skillCategory)
                {
                    case 1: return "Hồi Máu (HP)";
                    case 2: return "Hồi Mana (MP)";
                    case 3: return "Cường Hóa (Buff)";
                    default: return "Tấn Công (ST)";
                }
            }
        }

        public string RangeFormattedText
        {
            get
            {
                switch (indicatorType)
                {
                    case 1: return $"{castRange:0.#}m (Bắn Thẳng)";
                    case 2: return $"{castRange:0.#}m (Vùng Đất / Mưa)";
                    case 3: return $"{castRange:0.#}m (Hình Quạt)";
                    case 4: return $"{castRange:0.#}m (Xung Quanh)";
                    case 5: return "0m (Buff Tự Thân)";
                    case 6: return $"{castRange:0.#}m (Buff Toàn Đội 360°)";
                    default: return $"{castRange:0.#}m";
                }
            }
        }

        public SkillScrollData(string id, string scrollName, string tierRank, Color tierColor, string element, 
                               int manaCost, float cooldown, int baseDamage, float intelligenceScaling, 
                               int indicatorType, string iconName, string scrollIconName, string description,
                               float castRange = 0f, string requiredClass = "Toàn Hệ", string customVfxSprite = "",
                               int skillCategory = 0)
        {
            this.id = id;
            this.scrollName = scrollName;
            this.requiredClass = string.IsNullOrEmpty(requiredClass) ? "Toàn Hệ" : requiredClass;
            this.tierRank = tierRank;
            this.tierColor = tierColor;
            this.element = element;
            this.manaCost = manaCost;
            this.cooldown = cooldown;
            this.baseDamage = baseDamage;
            this.intelligenceScaling = intelligenceScaling;
            this.indicatorType = indicatorType;
            this.skillCategory = skillCategory;
            this.iconName = iconName;
            this.scrollIconName = scrollIconName;
            this.customVfxSprite = customVfxSprite;
            this.description = description;

            if (castRange > 0f)
            {
                this.castRange = castRange;
            }
            else
            {
                // Tự động gán tầm đánh chuẩn theo Archetype
                switch (indicatorType)
                {
                    case 1: this.castRange = 8.0f; break; // Bắn thẳng (Line)
                    case 2: this.castRange = 7.0f; break; // Vùng đất AoE (Ground AoE)
                    case 3: this.castRange = 4.5f; break; // Hình quạt (Cone Sweep)
                    case 4: this.castRange = 2.5f; break; // Hào quang quanh thân (Self Nova)
                    case 5: this.castRange = 0.0f; break; // Buff Tự Thân (Self Buff)
                    default: this.castRange = 6.0f; break;
                }
            }

            // Tự động khớp Phôi Khối (BaseShapeMask) & Quỹ Đạo (Trajectory) chính xác 100% với Tên Chiêu
            AutoAssignShapeMask();
        }

        /// <summary>
        /// Tự động phân tích tên chiêu & cơ chế để gán đúng phôi khối Pixel Art (BaseShapeMask) & Quỹ đạo (Trajectory)
        /// đảm bảo: Tên chiêu Kiếm Khí phải ra Vệt Chém (Slash), Tiễn phải ra Mũi Tên (Arrow), Sóng phải ra Wave, Thiên Thạch phải ra Meteor!
        /// </summary>
        public void AutoAssignShapeMask()
        {
            string n = (scrollName + " " + id).ToLower();

            // 1. Phôi Khối Base Shape Masks (0=Orb, 1=Slash, 2=Arrow, 3=Wave, 4=Meteor, 5=NovaRing, 6=Pillar, 7=Beam)
            if (n.Contains("kiếm") || n.Contains("slash") || n.Contains("trảm") || n.Contains("chém"))
            {
                baseShapeMask = 1; // Slash (Vệt Kiếm Khí)
            }
            else if (n.Contains("tiễn") || n.Contains("arrow") || n.Contains("bắn") || n.Contains("xạ") || n.Contains("snipe"))
            {
                baseShapeMask = 2; // Arrow (Mũi Tên Ma Thuật)
            }
            else if (n.Contains("sóng") || n.Contains("wave") || n.Contains("triều") || n.Contains("toàn phong") || indicatorType == 3)
            {
                baseShapeMask = 3; // Wave (Lưỡi Sóng Xung Kích)
            }
            else if (n.Contains("thiên thạch") || n.Contains("meteor") || n.Contains("oanh kích") || n.Contains("mưa") || n.Contains("trụy") || indicatorType == 2)
            {
                baseShapeMask = 4; // Meteor (Thiên Thạch Bốc Cháy)
            }
            else if (n.Contains("nova") || n.Contains("bá vương") || n.Contains("quanh thân") || n.Contains("bộc phá") || indicatorType == 4)
            {
                baseShapeMask = 5; // NovaRing (Vòng Nổ Năng Lượng 360°)
            }
            else if (n.Contains("trụ") || n.Contains("cột") || n.Contains("pillar") || n.Contains("địa kiếm"))
            {
                baseShapeMask = 6; // Pillar (Cột Năng Lượng Địa Chấn)
            }
            else if (n.Contains("tia") || n.Contains("beam") || n.Contains("laser") || n.Contains("xuyên"))
            {
                baseShapeMask = 7; // Beam (Tia Laser Xuyên Thấu)
            }
            else
            {
                baseShapeMask = 0; // Orb (Cầu Năng Lượng Nguyên Tố)
            }

            // 2. Quỹ Đạo Trajectory (0=Linear, 1=SineWave, 2=Homing, 3=LobbedArc, 4=Boomerang, 5=Instant)
            if (n.Contains("homing") || n.Contains("truy hồn") || n.Contains("đuổi"))
            {
                trajectoryType = 2; // Homing
            }
            else if (n.Contains("sine") || n.Contains("lượn") || n.Contains("rồng"))
            {
                trajectoryType = 1; // SineWave
            }
            else if (n.Contains("arc") || n.Contains("cầu vồng") || n.Contains("dậm"))
            {
                trajectoryType = 3; // LobbedArc
            }
            else if (n.Contains("boomerang") || n.Contains("lộn"))
            {
                trajectoryType = 4; // Boomerang
            }
            else if (n.Contains("instant") || n.Contains("tức thời"))
            {
                trajectoryType = 5; // Instant
            }
            else
            {
                trajectoryType = 0; // Linear
            }
        }

        /// <summary>
        /// Kiểm tra nghiêm ngặt điều kiện trang bị / học cuộn trục:
        /// - KHÔNG PHẢI PHẠT MÀ LÀ KHÔNG DÙNG ĐƯỢC!
        /// - Phải đúng Chức Nghiệp (Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ) hoặc Toàn Hệ.
        /// - Phải đúng Thuộc Tính Nguyên Tố (1 trong 7 hệ: Ánh Sáng, Bóng Tối, Phong, Mộc, Băng, Hỏa, Lôi).
        /// </summary>
        /// <summary>
        /// Kiểm tra nghiêm ngặt điều kiện trang bị / phát huy bí kíp:
        /// 1. HỆ (Class): Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ (hoặc Toàn Hệ). Người thuộc Hệ nào CHỈ DÙNG ĐƯỢC cuộn của Hệ đó!
        /// 2. THUỘC TÍNH (Element): Hỏa, Băng, Lôi, Ánh Sáng, Bóng Tối, Phong, Mộc. Người mang Thuộc Tính nào CHỈ PHÁT HUY ĐƯỢC skill của Thuộc Tính đó!
        /// </summary>
        public bool CanPlayerUse(string playerClass, string playerElement, out string failureReason)
        {
            // 1. Kiểm tra HỆ
            if (!string.IsNullOrEmpty(requiredClass) && requiredClass != "Toàn Hệ")
            {
                if (!string.Equals(requiredClass, playerClass, StringComparison.OrdinalIgnoreCase))
                {
                    failureReason = $"Sai Hệ! Cuộn yêu cầu [{requiredClass}], bạn đang là [{playerClass}]";
                    return false;
                }
            }

            // 2. Kiểm tra THUỘC TÍNH
            if (!string.IsNullOrEmpty(element) && element != "Vô Hệ" && element != "Toàn Hệ")
            {
                bool isMatch = string.Equals(element, playerElement, StringComparison.OrdinalIgnoreCase);
                // Xử lý đồng nghĩa Quang / Ánh Sáng và Ám / Bóng Tối
                if (!isMatch)
                {
                    if ((element == "Ánh Sáng" || element == "Quang") && (playerElement == "Ánh Sáng" || playerElement == "Quang")) isMatch = true;
                    else if ((element == "Bóng Tối" || element == "Ám") && (playerElement == "Bóng Tối" || playerElement == "Ám")) isMatch = true;
                }

                if (!isMatch)
                {
                    failureReason = $"Khác Thuộc Tính! Cuộn yêu cầu [{element}], bạn mang [{playerElement}]";
                    return false;
                }
            }

            failureReason = null;
            return true;
        }

        public SkillScrollData Clone()
        {
            var clone = new SkillScrollData(id, scrollName, tierRank, tierColor, element, manaCost, cooldown, baseDamage, intelligenceScaling, indicatorType, iconName, scrollIconName, description, castRange, requiredClass, customVfxSprite, skillCategory);
            clone.baseShapeMask = this.baseShapeMask;
            clone.trajectoryType = this.trajectoryType;
            clone.flySpeed = this.flySpeed;
            clone.explosionRadius = this.explosionRadius;
            clone.hasTrail = this.hasTrail;
            clone.hasScreenShake = this.hasScreenShake;
            return clone;
        }
    }
}
