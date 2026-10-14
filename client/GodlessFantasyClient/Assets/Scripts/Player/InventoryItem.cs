using System;
using UnityEngine;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Player
{
    public enum ItemType
    {
        Equipment,
        SkillScroll,
        Consumable,
        Pet,
        Material,
        Costume    // Ngoại Trang (Skin)
    }

    public enum EquipSlot
    {
        None,
        Helmet,    // Mũ
        Armor,     // Áo
        Pants,     // Quần
        Boots,     // Giày
        Ring,      // Nhẫn
        Necklace,  // Vòng Cổ
        Weapon,    // Vũ Khí
        Costume    // Ngoại Trang (Skin)
    }

    public enum ItemGender
    {
        All,    // Chung (Dành cho cả Nam & Nữ / Unisex)
        Male,   // Dành riêng cho Nam ♂
        Female  // Dành riêng cho Nữ ♀
    }

    /// <summary>
    /// Đơn vị vật phẩm thống nhất trong Hành Trang Godless Fantasy:
    /// - Cả Trang Bị, Sách Kỹ Năng, Bình Dược, Thú Cưng đều là các ô hành trang bình thường.
    /// - Khi đang được trang bị (mặc trên người / gắn vào ô skill 1-4 / thú cưng đang gọi) -> Được khóa bảo vệ chống xóa/vứt/giao dịch nhầm!
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string id;
        public string itemName;
        public ItemType itemType;
        public EquipSlot equipSlot;
        public string tierRank;          // E, D, C, B, A, S, X
        public Color tierColor;
        public string iconName;          // Tên texture trong Resources
        public Sprite customSprite;      // Sprite tùy chỉnh trực tiếp (dành cho Costume Workshop / Modding)
        public Sprite icon => (customSprite != null) ? customSprite : Resources.Load<Sprite>(iconName);
        public int quantity = 1;
        public bool isEquipped = false;
        public int equippedHotbarSlot = -1; // 0..3 nếu là SkillScroll đang gắn vào ô 1..4
        public int slotIndex = -1;          // Vị trí ô cụ thể trong hành trang (0..currentCapacity - 1)
        public string description;
        public string statsInfo;
        public SkillScrollData scrollData;

        // ĐIỀU KIỆN MẶC TRANG BỊ & NGOẠI TRANG: GIỚI TÍNH, HỆ (CHỨC NGHIỆP) & THUỘC TÍNH (NGUYÊN TỐ)
        public ItemGender requiredGender = ItemGender.All;  // All (Chung / Cả Nam & Nữ), Male (Chỉ Nam ♂), Female (Chỉ Nữ ♀)
        public string requiredClass = "Toàn Hệ";            // Kị Sĩ, Pháp Sư, Võ Sĩ, Xạ Thủ hoặc đa hệ "Kị Sĩ, Võ Sĩ", hoặc "Toàn Hệ"
        public string element = "Toàn Thuộc Tính";          // Đơn thuộc tính "Hỏa", hoặc đa thuộc tính 2, 3, 4 hệ "Hỏa, Băng", "Hỏa, Băng, Lôi", hoặc "Toàn Thuộc Tính"

        // ─── THUỘC TÍNH ẨN / TÍNH NĂNG ĐẶC BIỆT (HIDDEN TRAITS & SPECIAL PROPERTIES) ───
        // Tái sử dụng linh hoạt cho nhiều loại Trang Phục (Costume) và Trang Bị:
        // Ví dụ: ItemTraits.GENDER_SWAP ("gender_swap"), "speed_boost", "stealth_aura", v.v.
        public System.Collections.Generic.List<string> specialTraits = new System.Collections.Generic.List<string>();

        public bool HasTrait(string traitId)
        {
            if (specialTraits == null || specialTraits.Count == 0 || string.IsNullOrEmpty(traitId)) return false;
            for (int i = 0; i < specialTraits.Count; i++)
            {
                if (string.Equals(specialTraits[i], traitId, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public void AddTrait(string traitId)
        {
            if (specialTraits == null) specialTraits = new System.Collections.Generic.List<string>();
            if (!HasTrait(traitId)) specialTraits.Add(traitId);
        }

        public void RemoveTrait(string traitId)
        {
            if (specialTraits == null || string.IsNullOrEmpty(traitId)) return;
            for (int i = specialTraits.Count - 1; i >= 0; i--)
            {
                if (string.Equals(specialTraits[i], traitId, System.StringComparison.OrdinalIgnoreCase))
                    specialTraits.RemoveAt(i);
            }
        }

        public InventoryItem WithTrait(string traitId)
        {
            AddTrait(traitId);
            return this;
        }

        public InventoryItem WithTraits(params string[] traits)
        {
            if (traits != null)
            {
                for (int i = 0; i < traits.Length; i++) AddTrait(traits[i]);
            }
            return this;
        }

        public InventoryItem WithGender(ItemGender gender)
        {
            this.requiredGender = gender;
            return this;
        }

        public InventoryItem WithClass(string allowedClasses)
        {
            this.requiredClass = allowedClasses;
            return this;
        }

        public InventoryItem WithElement(string allowedElements)
        {
            this.element = allowedElements;
            return this;
        }

        /// <summary>
        /// Thuộc tính ẩn cho phép đổi giới tính Nam <-> Nữ [G]
        /// </summary>
        public bool CanSwitchGender => HasTrait(ItemTraits.GENDER_SWAP);

        public bool MatchesGender(bool playerIsFemale)
        {
            if (requiredGender == ItemGender.All) return true;
            if (requiredGender == ItemGender.Female) return playerIsFemale;
            if (requiredGender == ItemGender.Male) return !playerIsFemale;
            return true;
        }

        public string GenderDisplayString
        {
            get
            {
                switch (requiredGender)
                {
                    case ItemGender.Male: return "Nam ♂";
                    case ItemGender.Female: return "Nữ ♀";
                    default: return "Chung (Cả Nam & Nữ)";
                }
            }
        }

        /// <summary>
        /// Kiểm tra khớp Hệ (Chức Nghiệp): Hỗ trợ đơn hệ ("Kị Sĩ"), đa hệ ("Kị Sĩ, Võ Sĩ") hoặc toàn hệ ("Toàn Hệ").
        /// </summary>
        public static bool CheckClassMatch(string allowedClassesStr, string playerClass)
        {
            if (string.IsNullOrEmpty(allowedClassesStr)) return true;
            string clean = allowedClassesStr.Trim();
            if (string.Equals(clean, "Toàn Hệ", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clean, "Tất Cả", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clean, "All", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.IsNullOrEmpty(playerClass)) return true;

            char[] sep = new char[] { ',', '/', '|', ';' };
            string[] parts = clean.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string c = parts[i].Trim();
                if (string.Equals(c, "Toàn Hệ", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c, "Tất Cả", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c, "All", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (string.Equals(c, playerClass.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Chuẩn hóa tên thuộc tính nguyên tố và xử lý từ đồng nghĩa (Ánh Sáng/Quang, Bóng Tối/Ám...)
        /// </summary>
        public static string NormalizeElementName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "Toàn Thuộc Tính";
            string s = raw.Trim().ToLowerInvariant();
            if (s.Contains("hỏa") || s.Contains("fire")) return "Hỏa";
            if (s.Contains("băng") || s.Contains("ice") || s.Contains("hàn")) return "Băng";
            if (s.Contains("lôi") || s.Contains("thunder") || s.Contains("lightning") || s.Contains("sét")) return "Lôi";
            if (s.Contains("phong") || s.Contains("wind") || s.Contains("gió")) return "Phong";
            if (s.Contains("mộc") || s.Contains("wood") || s.Contains("nature") || s.Contains("cây")) return "Mộc";
            if (s.Contains("quang") || s.Contains("sáng") || s.Contains("light") || s.Contains("holy")) return "Quang";
            if (s.Contains("ám") || s.Contains("tối") || s.Contains("dark") || s.Contains("shadow")) return "Ám";
            if (s.Contains("toàn") || s.Contains("vô") || s.Contains("all") || s.Contains("none")) return "Toàn Thuộc Tính";
            return raw.Trim();
        }

        /// <summary>
        /// Kiểm tra khớp Thuộc Tính (Nguyên Tố): Hỗ trợ đơn thuộc tính ("Hỏa"), đa thuộc tính 2, 3, 4 hệ ("Hỏa, Băng", "Hỏa, Băng, Lôi") hoặc toàn bộ ("Toàn Thuộc Tính").
        /// </summary>
        public static bool CheckElementMatch(string allowedElementsStr, string playerElement)
        {
            if (string.IsNullOrEmpty(allowedElementsStr)) return true;
            string clean = allowedElementsStr.Trim();
            if (string.Equals(clean, "Toàn Hệ", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clean, "Toàn Thuộc Tính", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clean, "Vô Hệ", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clean, "Tất Cả", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clean, "All", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.IsNullOrEmpty(playerElement)) return true;

            string normPlayer = NormalizeElementName(playerElement);
            if (normPlayer == "Toàn Thuộc Tính") return true;

            char[] sep = new char[] { ',', '/', '|', ';' };
            string[] parts = clean.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string normItem = NormalizeElementName(parts[i]);
                if (normItem == "Toàn Thuộc Tính") return true;
                if (string.Equals(normItem, normPlayer, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public string RequirementTag
        {
            get
            {
                var tags = new System.Collections.Generic.List<string>();
                if (requiredGender == ItemGender.Male) tags.Add("Nam ♂");
                else if (requiredGender == ItemGender.Female) tags.Add("Nữ ♀");

                if (!string.IsNullOrEmpty(requiredClass) && requiredClass != "Toàn Hệ" && requiredClass != "Tất Cả")
                    tags.Add(requiredClass);

                string normElem = NormalizeElementName(element);
                if (!string.IsNullOrEmpty(element) && normElem != "Toàn Thuộc Tính" && element != "Vô Hệ")
                    tags.Add(element);

                if (tags.Count == 0) return "[Toàn Hệ]";
                return $"[{string.Join(" · ", tags)}]";
            }
        }

        public InventoryItem(string id, string itemName, ItemType itemType, EquipSlot equipSlot, 
                             string tierRank, Color tierColor, string iconName, string statsInfo, 
                             string description, int quantity = 1, SkillScrollData scrollData = null,
                             string requiredClass = "Toàn Hệ", string element = "Toàn Thuộc Tính",
                             ItemGender requiredGender = ItemGender.All)
        {
            this.id = id;
            this.itemName = itemName;
            this.itemType = itemType;
            this.equipSlot = equipSlot;
            this.tierRank = tierRank;
            this.tierColor = tierColor;
            this.iconName = iconName;
            this.statsInfo = statsInfo;
            this.description = description;
            this.quantity = quantity;
            this.scrollData = scrollData;
            this.requiredClass = string.IsNullOrEmpty(requiredClass) ? "Toàn Hệ" : requiredClass;
            this.element = string.IsNullOrEmpty(element) ? "Toàn Thuộc Tính" : element;
            this.requiredGender = requiredGender;
            this.isEquipped = false;
            this.equippedHotbarSlot = -1;
        }

        public InventoryItem WithCustomSprite(Sprite spr)
        {
            this.customSprite = spr;
            return this;
        }

        /// <summary>
        /// Kiểm tra nghiêm ngặt cả 3 điều kiện:
        /// 1. Giới Tính (Nam ♂ / Nữ ♀ / Chung)
        /// 2. Hệ / Chức Nghiệp (Đơn hệ hoặc đa hệ 2, 3 hệ)
        /// 3. Thuộc Tính / Nguyên Tố (Đơn thuộc tính hoặc đa thuộc tính 2, 3, 4 hệ)
        /// </summary>
        public bool CanPlayerEquip(bool playerIsFemale, string playerClass, string playerElement, out string failureReason)
        {
            // 1. Kiểm tra GIỚI TÍNH
            if (!MatchesGender(playerIsFemale))
            {
                string reqG = (requiredGender == ItemGender.Male) ? "Nam ♂" : "Nữ ♀";
                string curG = playerIsFemale ? "Nữ ♀" : "Nam ♂";
                failureReason = $"Sai Giới Tính! Trang bị chỉ dành cho [{reqG}], bạn đang là [{curG}]!";
                return false;
            }

            // 2. Kiểm tra HỆ
            if (!CheckClassMatch(requiredClass, playerClass))
            {
                failureReason = $"Sai Hệ! Yêu cầu [{requiredClass}], bạn đang là [{playerClass}]!";
                return false;
            }

            // 3. Kiểm tra THUỘC TÍNH NGUYÊN TỐ
            if (!CheckElementMatch(element, playerElement))
            {
                failureReason = $"Khác Thuộc Tính! Yêu cầu [{element}], bạn mang [{playerElement}]!";
                return false;
            }

            failureReason = null;
            return true;
        }

        public bool CanPlayerEquip(string playerClass, string playerElement, out string failureReason)
        {
            bool isFemale = false;
            if (ProceduralCharacterRig2D.Instance != null) isFemale = ProceduralCharacterRig2D.Instance.IsFemale;
            else if (CharacterVisualController8Dir.Instance != null) isFemale = CharacterVisualController8Dir.Instance.IsFemale;
            return CanPlayerEquip(isFemale, playerClass, playerElement, out failureReason);
        }

        public InventoryItem Clone()
        {
            var copy = new InventoryItem(id, itemName, itemType, equipSlot, tierRank, tierColor, iconName, statsInfo, description, quantity, scrollData, requiredClass, element, requiredGender);
            copy.isEquipped = isEquipped;
            copy.equippedHotbarSlot = equippedHotbarSlot;
            copy.slotIndex = slotIndex;
            if (specialTraits != null)
                copy.specialTraits = new System.Collections.Generic.List<string>(specialTraits);
            return copy;
        }
    }

    /// <summary>
    /// Các định danh Thuộc Tính Ẩn & Tính Năng Đặc Biệt của Trang Bị / Ngoại Trang
    /// </summary>
    public static class ItemTraits
    {
        public const string GENDER_SWAP = "gender_swap"; // Cho phép chuyển đổi giới tính Nam <-> Nữ [G]
        public const string SPEED_BOOST = "speed_boost"; // Tăng tốc độ di chuyển
        public const string STEALTH     = "stealth";     // Ẩn thân
        public const string FLIGHT      = "flight";      // Bay lượn
    }
}
