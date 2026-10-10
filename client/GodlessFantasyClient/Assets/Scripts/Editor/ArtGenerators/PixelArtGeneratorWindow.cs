#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace GodlessFantasy.Editor
{
    /// <summary>
    /// Công Cụ Tạo & Quản Lý Sprite Pixel Art (Modular Pixel Art Generator Tool):
    /// - Cho phép tự động vẽ lại hoặc chỉnh sửa Khung Nút Chiêu Thức Độc Nhất (Universal Slot Frame).
    /// - Template Cuộn Trục Bí Kíp (Scroll Template) & Nhân Phù Văn Nguyên Tố (Elemental Runes).
    /// - Nút "Tạo Lại Toàn Bộ Sprite" xuất thẳng ra Assets/Resources không cần dùng phần mềm ngoài!
    /// </summary>
    public class PixelArtGeneratorWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private Texture2D previewFrame;
        private Texture2D previewScrollTemplate;
        private Texture2D previewRuneFire;
        private Texture2D previewEmblemMeteor;

        [MenuItem("Tools/Godless Fantasy/Art Generator/Pixel Art Studio %#a")]
        public static void ShowWindow()
        {
            var win = GetWindow<PixelArtGeneratorWindow>("Pixel Art Studio");
            win.minSize = new Vector2(440, 520);
            win.Show();
        }

        private void OnEnable()
        {
            LoadPreviews();
        }

        private void LoadPreviews()
        {
            previewFrame = Resources.Load<Texture2D>("ui_frame_slot");
            previewScrollTemplate = Resources.Load<Texture2D>("ui_template_scroll_solid");
            previewRuneFire = Resources.Load<Texture2D>("rune_fire");
            previewEmblemMeteor = Resources.Load<Texture2D>("emblem_meteor");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("🎨 GODLESS FANTASY — PIXEL ART STUDIO", titleStyle);
            EditorGUILayout.HelpBox("Kiến trúc Tách Rời Khung & Nhân (Frame + Emblem Separation):\n" +
                                    "• 1 Khung viền duy nhất dùng cho mọi kỹ năng & ô trang bị.\n" +
                                    "• 1 Template cuộn trục duy nhất lồng các ấn chú phù văn vào tâm.\n" +
                                    "• Tái sử dụng tối đa, dễ dàng thêm chiêu thức mới trong vài giây!", MessageType.Info);

            EditorGUILayout.Space(8);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // 1. XEM TRƯỚC HỆ THỐNG MODULAR
            EditorGUILayout.LabelField("1. XEM TRƯỚC (MODULAR PREVIEWS)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            // Preview 1: Khung Slot
            EditorGUILayout.BeginVertical("box", GUILayout.Width(130));
            EditorGUILayout.LabelField("Khung Nút Ô Skill", EditorStyles.miniBoldLabel);
            if (previewFrame != null)
            {
                GUILayout.Box(previewFrame, GUILayout.Width(64), GUILayout.Height(64));
            }
            else
            {
                EditorGUILayout.HelpBox("Chưa nạp", MessageType.None);
            }
            EditorGUILayout.EndVertical();

            // Preview 2: Nhân Meteor
            EditorGUILayout.BeginVertical("box", GUILayout.Width(130));
            EditorGUILayout.LabelField("Nhân Biểu Tượng", EditorStyles.miniBoldLabel);
            if (previewEmblemMeteor != null)
            {
                GUILayout.Box(previewEmblemMeteor, GUILayout.Width(64), GUILayout.Height(64));
            }
            else
            {
                EditorGUILayout.HelpBox("Chưa nạp", MessageType.None);
            }
            EditorGUILayout.EndVertical();

            // Preview 3: Template Cuộn Trục
            EditorGUILayout.BeginVertical("box", GUILayout.Width(130));
            EditorGUILayout.LabelField("Template Cuộn Trục", EditorStyles.miniBoldLabel);
            if (previewScrollTemplate != null)
            {
                GUILayout.Box(previewScrollTemplate, GUILayout.Width(64), GUILayout.Height(64));
            }
            else
            {
                EditorGUILayout.HelpBox("Chưa nạp", MessageType.None);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12);

            // 2. DANH MỤC TÀI NGUYÊN ĐÃ TẠO
            EditorGUILayout.LabelField("2. DANH MỤC TÀI NGUYÊN MODULAR", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("• ui_frame_slot.png: Khung viền vàng chốt góc 4 phía.\n" +
                                    "• ui_frame_slot_white.png: Khung viền trắng để tô màu phẩm cấp (E..X).\n" +
                                    "• ui_template_scroll.png: Khung cuộn trục giấy da hai đầu gỗ cổ điển.\n" +
                                    "• rune_fire / rune_ice / rune_thunder / rune_holy / rune_arcane: Ấn chú phù văn.\n" +
                                    "• emblem_meteor / emblem_flame_cone / emblem_divine_nova / emblem_arcane_bolt / emblem_dash: Nhân chiêu thức.", MessageType.None);

            EditorGUILayout.Space(14);

            // 3. NÚT CHẠY GENERATOR BẰNG PYTHON
            EditorGUILayout.LabelField("3. THAO TÁC XUẤT SPRITE", EditorStyles.boldLabel);
            GUI.backgroundColor = new Color(0.25f, 0.85f, 0.45f);
            if (GUILayout.Button("⚡ CHẠY SCRIPT TẠO LẠI TẤT CẢ SPRITE (PYTHON)", GUILayout.Height(40)))
            {
                RunPythonGenerator();
                LoadPreviews();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(6);
            if (GUILayout.Button("📂 MỞ THƯ MỤC CHỨA CODE GENERATOR (tools/ArtGenerators)", GUILayout.Height(30)))
            {
                string path = Path.GetFullPath("tools/ArtGenerators");
                EditorUtility.RevealInFinder(path);
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunPythonGenerator()
        {
            try
            {
                string scriptPath = Path.GetFullPath("tools/ArtGenerators/generate_pixel_art.py");
                var proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "python";
                proc.StartInfo.Arguments = $"\"{scriptPath}\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Thành Công", "Đã tạo lại toàn bộ Sprite thành công!\n" + output, "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không thể chạy Python script: {ex.Message}", "OK");
            }
        }
    }
}
#endif
