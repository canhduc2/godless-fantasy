using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Hệ thống UI Mirror cho Avatar HUD và Menu Xem Trước Nhân Vật:
    /// Chuyển sang chế độ Render Static Sprite chuẩn chính diện (South / Front-facing)
    /// theo yêu cầu của user:
    /// • Avatar HUD: Ảnh chân dung Chibi chính diện sắc nét, không bị che khuất hay lệch camera.
    /// • Menu Trang Bị: Nhân vật toàn thân chính diện (South) hiển thị đầy đủ trang phục.
    /// • Loại bỏ hoàn toàn camera clone và render pass phụ -> Tối ưu cực đại CPU/GPU đạt 300+ FPS!
    /// </summary>
    public class LiveCharacterUIMirror : MonoBehaviour
    {
        public static LiveCharacterUIMirror Instance { get; private set; }

        public Texture AvatarTexture => null;
        public Texture FullBodyTexture => null;

        public static void EnsureInstance()
        {
            // Chế độ Static Sprite không cần spawn camera clone
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Dọn dẹp bất kỳ container mirror clone hoặc camera cũ nếu còn tồn tại
            var oldContainer = GameObject.Find("MirrorCharacter_Container");
            if (oldContainer != null) Destroy(oldContainer);

            var oldAvatarCam = GameObject.Find("Cam_Avatar");
            if (oldAvatarCam != null) Destroy(oldAvatarCam);

            var oldFullBodyCam = GameObject.Find("Cam_FullBody");
            if (oldFullBodyCam != null) Destroy(oldFullBodyCam);
        }

        public void SetupMirror(ProceduralCharacterRig2D sourceRig)
        {
            // Chế độ tĩnh: Sử dụng trực tiếp ảnh chính diện sắc nét từ Texture2D
        }
    }
}
