#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Editor
{
    /// <summary>
    /// Unity Editor Studio Window - Công Cụ Thiết Kế Asset & Kỹ Năng Trực Quan (No-Code Studio):
    /// - Cho phép kéo thả trực tiếp file Sprite từ Project vào ô Icon.
    /// - Tự động tạo file ScriptableObject (.asset) vào thư mục Resources/Data.
    /// - Tự động xuất và đồng bộ vào file JSON Database để game nạp tức thì khi chạy!
    /// </summary>
    public class GodlessStudioWindow : EditorWindow
    {
        private int selectedTab = 0;
        private readonly string[] tabs = { "⚔️ THIẾT KẾ VẬT PHẨM & TRANG BỊ", "📜 THIẾT KẾ KỸ NĂNG & BÍ KÍP" };
        private Vector2 scrollPos;

        // ================= TAB 1: ITEM STUDIO =================
        private string itemId = "custom_sword_gold";
        private string itemName = "Bảo Kiếm Hoàng Kim";
        private ItemType itemType = ItemType.Equipment;
        private EquipSlot equipSlot = EquipSlot.Weapon;
        private string itemTierRank = "Phẩm A";
        private Color itemTierColor = new Color(1.0f, 0.65f, 0.2f);
        private Sprite itemSprite = null;
        private string itemIconName = "item_weapon";
        private string itemStats = "+25 Trí Tuệ (INT) · +15 Sức Mạnh (STR)";
        private string itemDesc = "Bảo kiếm hoàng kim rèn từ thép cổ đại, gia tăng sức mạnh toàn diện.";
        private int itemQuantity = 1;

        // ================= TAB 2: SKILL STUDIO =================
        private string skillId = "custom_fire_dragon";
        private string skillName = "Hỏa Long Bộc Phá";
        private string skillElement = "Hỏa";
        private string skillTierRank = "Phẩm S";
        private Color skillTierColor = new Color(1.0f, 0.85f, 0.25f);
        private int skillIndicatorType = 1; // 1=Line, 2=Ground AoE, 3=Cone, 4=Nova
        private int skillMana = 25;
        private float skillCooldown = 3.5f;
        private int skillBaseDmg = 45;
        private float skillScaling = 0.85f;
        private float skillCastRange = 8.0f;
        private Sprite skillHotbarSprite = null;
        private string skillHotbarIconName = "skill_fireball";
        private Sprite skillScrollSprite = null;
        private string skillScrollIconName = "scroll_firebolt";
        private string skillDesc = "Triệu hồi luồng hỏa long cuồng nộ thiêu rụi mục tiêu trên đường bay!";

        [MenuItem("Tools/Godless Fantasy/Asset & Skill Studio %#s")]
        public static void ShowWindow()
        {
            var window = GetWindow<GodlessStudioWindow>("Godless Asset Studio");
            window.minSize = new Vector2(460, 580);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();

            selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.Height(30));
            EditorGUILayout.Space(8);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (selectedTab == 0)
            {
                DrawItemStudioTab();
            }
            else
            {
                DrawSkillStudioTab();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(4);
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("🎮 GODLESS FANTASY — ASSET DESIGN STUDIO", headerStyle);
            EditorGUILayout.HelpBox("Công cụ thiết kế Asset trực quan cho Game Designer: Kéo thả Sprite, xuất ScriptableObject (.asset) và JSON Database tức thì!", MessageType.Info);
            EditorGUILayout.Space(6);
        }

        private void DrawItemStudioTab()
        {
            EditorGUILayout.LabelField("1. ĐỊNH DANH VẬT PHẨM", EditorStyles.boldLabel);
            itemId = EditorGUILayout.TextField("Mã Định Danh (ID):", itemId);
            itemName = EditorGUILayout.TextField("Tên Vật Phẩm:", itemName);
            itemType = (ItemType)EditorGUILayout.EnumPopup("Loại Vật Phẩm:", itemType);
            equipSlot = (EquipSlot)EditorGUILayout.EnumPopup("Vị Trí Trang Bị:", equipSlot);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("2. PHẨM CẤP & ICON TRỰC QUAN", EditorStyles.boldLabel);
            itemTierRank = EditorGUILayout.TextField("Cấp Phẩm (E, D, C, B, A, S, X):", itemTierRank);
            itemTierColor = EditorGUILayout.ColorField("Màu Phẩm Cấp:", itemTierColor);

            EditorGUILayout.BeginHorizontal();
            itemSprite = (Sprite)EditorGUILayout.ObjectField("Kéo Thả Sprite Icon:", itemSprite, typeof(Sprite), false, GUILayout.Height(40));
            if (itemSprite != null)
            {
                itemIconName = itemSprite.name;
            }
            EditorGUILayout.EndHorizontal();
            itemIconName = EditorGUILayout.TextField("Tên Icon Tài Nguyên:", itemIconName);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("3. CHỈ SỐ & MÔ TẢ", EditorStyles.boldLabel);
            itemStats = EditorGUILayout.TextField("Dòng Chỉ Số Thuộc Tính:", itemStats);
            EditorGUILayout.LabelField("Mô Tả Vật Phẩm:");
            itemDesc = EditorGUILayout.TextArea(itemDesc, GUILayout.Height(50));
            itemQuantity = EditorGUILayout.IntField("Số Lượng Mặc Định:", itemQuantity);

            EditorGUILayout.Space(12);
            DrawActionButtonsForItem();
        }

        private void DrawActionButtonsForItem()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
            if (GUILayout.Button("💾 TẠO SCRIPTABLEOBJECT (.asset)", GUILayout.Height(36)))
            {
                CreateItemScriptableObject();
            }

            GUI.backgroundColor = new Color(0.3f, 0.65f, 1f);
            if (GUILayout.Button("📂 LƯU DATABASE (JSON)", GUILayout.Height(36)))
            {
                SaveItemToJsonDatabase();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void CreateItemScriptableObject()
        {
            string dir = "Assets/Resources/Items";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var so = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            so.itemId = itemId;
            so.itemName = itemName;
            so.itemType = itemType;
            so.equipSlot = equipSlot;
            so.tierRank = itemTierRank;
            so.tierColor = itemTierColor;
            so.iconName = itemIconName;
            so.statsInfo = itemStats;
            so.description = itemDesc;
            so.defaultQuantity = itemQuantity;

            string assetPath = $"{dir}/{itemId}.asset";
            AssetDatabase.CreateAsset(so, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = so;

            EditorUtility.DisplayDialog("Thành Công", $"Đã tạo ScriptableObject tại:\n{assetPath}", "OK");
        }

        private void SaveItemToJsonDatabase()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "custom_item_database.json");
                var itemData = new ItemJsonData
                {
                    id = itemId,
                    name = itemName,
                    itemType = (int)itemType,
                    equipSlot = (int)equipSlot,
                    tierRank = itemTierRank,
                    tierR = itemTierColor.r,
                    tierG = itemTierColor.g,
                    tierB = itemTierColor.b,
                    iconName = itemIconName,
                    statsInfo = itemStats,
                    description = itemDesc,
                    quantity = itemQuantity
                };

                string json = JsonUtility.ToJson(itemData, true);
                File.WriteAllText(path, json);
                EditorUtility.DisplayDialog("Thành Công", $"Đã lưu vật phẩm vào JSON Database tại:\n{path}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Lỗi khi lưu: {ex.Message}", "OK");
            }
        }

        private void DrawSkillStudioTab()
        {
            EditorGUILayout.LabelField("1. ĐỊNH DANH KỸ NĂNG", EditorStyles.boldLabel);
            skillId = EditorGUILayout.TextField("Mã Chiêu Thức (ID):", skillId);
            skillName = EditorGUILayout.TextField("Tên Chiêu Thức:", skillName);
            skillElement = EditorGUILayout.TextField("Hệ Thuộc Tính (Hỏa/Băng/Lôi...):", skillElement);
            skillTierRank = EditorGUILayout.TextField("Cấp Phẩm (E, D, C, B, A, S, X):", skillTierRank);
            skillTierColor = EditorGUILayout.ColorField("Màu Phẩm Cấp:", skillTierColor);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("2. CƠ CHẾ & TẦM ĐÁNH (RANGE)", EditorStyles.boldLabel);
            string[] indNames = { "1. Bắn Thẳng (Line)", "2. Vùng Đất (Ground AoE)", "3. Hình Quạt (Cone)", "4. Nổ Tỏa Quanh (Nova)" };
            skillIndicatorType = EditorGUILayout.Popup("Loại Tâm Ngắm:", skillIndicatorType - 1, indNames) + 1;

            skillCastRange = EditorGUILayout.FloatField("Tầm Đánh / Cự Ly (mét):", skillCastRange);
            skillMana = EditorGUILayout.IntField("Tiêu Hao Mana:", skillMana);
            skillCooldown = EditorGUILayout.FloatField("Thời Gian Hồi Chiêu (s):", skillCooldown);
            skillBaseDmg = EditorGUILayout.IntField("Sát Thương Cơ Bản:", skillBaseDmg);
            skillScaling = EditorGUILayout.Slider("Hệ Số Scale Trí Tuệ (INT):", skillScaling, 0f, 3.0f);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("3. ICON HIỂN THỊ TRỰC QUAN", EditorStyles.boldLabel);

            skillHotbarSprite = (Sprite)EditorGUILayout.ObjectField("Kéo Thả Icon Phím Nóng (Hotbar):", skillHotbarSprite, typeof(Sprite), false, GUILayout.Height(36));
            if (skillHotbarSprite != null) skillHotbarIconName = skillHotbarSprite.name;
            skillHotbarIconName = EditorGUILayout.TextField("Tên Icon Hotbar:", skillHotbarIconName);

            skillScrollSprite = (Sprite)EditorGUILayout.ObjectField("Kéo Thả Icon Cuộn Trục (Túi Đồ):", skillScrollSprite, typeof(Sprite), false, GUILayout.Height(36));
            if (skillScrollSprite != null) skillScrollIconName = skillScrollSprite.name;
            skillScrollIconName = EditorGUILayout.TextField("Tên Icon Cuộn Trục:", skillScrollIconName);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("4. MÔ TẢ CHIÊU THỨC");
            skillDesc = EditorGUILayout.TextArea(skillDesc, GUILayout.Height(50));

            EditorGUILayout.Space(12);
            DrawActionButtonsForSkill();
        }

        private void DrawActionButtonsForSkill()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
            if (GUILayout.Button("💾 TẠO SCRIPTABLEOBJECT (.asset)", GUILayout.Height(36)))
            {
                CreateSkillScriptableObject();
            }

            GUI.backgroundColor = new Color(0.3f, 0.65f, 1f);
            if (GUILayout.Button("📂 LƯU DATABASE (JSON)", GUILayout.Height(36)))
            {
                SaveSkillToJsonDatabase();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void CreateSkillScriptableObject()
        {
            string dir = "Assets/Resources/Skills";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var so = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            so.skillId = skillId;
            so.skillName = skillName;
            so.element = skillElement;
            so.tierRank = skillTierRank;
            so.tierColor = skillTierColor;
            so.indicatorType = skillIndicatorType;
            so.manaCost = skillMana;
            so.cooldown = skillCooldown;
            so.baseDamage = skillBaseDmg;
            so.intelligenceScaling = skillScaling;
            so.castRange = skillCastRange;
            so.hotbarIconName = skillHotbarIconName;
            so.scrollBagIconName = skillScrollIconName;
            so.description = skillDesc;

            string assetPath = $"{dir}/{skillId}.asset";
            AssetDatabase.CreateAsset(so, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = so;

            EditorUtility.DisplayDialog("Thành Công", $"Đã tạo ScriptableObject Kỹ Năng tại:\n{assetPath}", "OK");
        }

        private void SaveSkillToJsonDatabase()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "custom_skill_database.json");
                var skillData = new SkillJsonData
                {
                    id = skillId,
                    name = skillName,
                    requiredClass = "Toàn Hệ",
                    element = skillElement,
                    tierRank = skillTierRank,
                    tierR = skillTierColor.r,
                    tierG = skillTierColor.g,
                    tierB = skillTierColor.b,
                    indicatorType = skillIndicatorType,
                    skillCategory = 0,
                    manaCost = skillMana,
                    cooldown = skillCooldown,
                    baseDamage = skillBaseDmg,
                    intelligenceScaling = skillScaling,
                    castRange = skillCastRange,
                    hotbarIconName = skillHotbarIconName,
                    scrollBagIconName = skillScrollIconName,
                    customVfxSprite = "",
                    description = skillDesc
                };

                string json = JsonUtility.ToJson(skillData, true);
                File.WriteAllText(path, json);
                EditorUtility.DisplayDialog("Thành Công", $"Đã lưu kỹ năng vào JSON Database tại:\n{path}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Lỗi khi lưu database kỹ năng: {ex.Message}", "OK");
            }
        }
    }
}
#endif
