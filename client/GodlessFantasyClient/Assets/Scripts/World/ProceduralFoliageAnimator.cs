using UnityEngine;

namespace GodlessFantasy.World
{
    /// <summary>
    /// Bộ Điều Khiển Hoạt Ảnh Thủ Tục Cho Khối Địa Hình (Procedural Foliage & Block Animator):
    /// - Tạo chuyển động sống động tự nhiên cho cỏ, cây, bụi rậm, hoa, mặt nước, ngọn lửa mà KHÔNG tốn tài nguyên spritesheet!
    /// - Hỗ trợ các kiểu hoạt ảnh:
    ///   + WindSway: Cỏ dại, hoa, cành lá, bụi rậm đung đưa mềm mại theo làn gió thảo nguyên.
    ///   + WaterWave: Gợn sóng lấp lánh phản chiếu ánh mặt trời trên sông hồ.
    ///   + FlamePulse: Ngọn lửa đuốc bập bùng, co giãn ngẫu nhiên theo sóng Perlin.
    ///   + CrystalPulse: Viên pha lê / ma pháp trận lơ lửng bồng bềnh và phát quang.
    /// </summary>
    public class ProceduralFoliageAnimator : MonoBehaviour
    {
        [SerializeField] private BlockAnimType animType = BlockAnimType.None;
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float intensity = 0.1f;

        private float phaseOffset;
        private Vector3 initialPosition;
        private Vector3 initialScale;
        private SpriteRenderer spriteRenderer;
        private Color initialColor;

        public void Setup(BlockAnimType type, float animSpeed, float animIntensity)
        {
            animType = type;
            speed = animSpeed;
            intensity = animIntensity;
        }

        private void Start()
        {
            initialPosition = transform.localPosition;
            initialScale = transform.localScale;
            phaseOffset = (transform.position.x * 0.7f + transform.position.y * 1.3f) % 6.28f;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) initialColor = spriteRenderer.color;

            // Nếu không có hoạt ảnh, tự hủy component để tối ưu tối đa hiệu năng
            if (animType == BlockAnimType.None)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            float t = Time.time * speed + phaseOffset;

            switch (animType)
            {
                case BlockAnimType.WindSway:
                    // Đung đưa cành lá và ngọn cỏ theo gió
                    float angle = Mathf.Sin(t) * (intensity * 18f);
                    transform.localRotation = Quaternion.Euler(0, 0, angle);
                    float stretch = 1.0f + Mathf.Cos(t * 1.5f) * (intensity * 0.4f);
                    transform.localScale = new Vector3(initialScale.x, initialScale.y * stretch, initialScale.z);
                    break;

                case BlockAnimType.WaterWave:
                    // Gợn sóng mặt nước co giãn nhẹ
                    if (spriteRenderer != null)
                    {
                        float alphaWave = 0.90f + Mathf.Sin(t) * (intensity * 0.5f);
                        Color c = initialColor;
                        c.a = initialColor.a * alphaWave;
                        spriteRenderer.color = c;
                    }
                    break;

                case BlockAnimType.FlamePulse:
                    // Lửa đuốc chập chờn
                    float noise = Mathf.PerlinNoise(Time.time * speed, phaseOffset);
                    float scaleMod = 1.0f + (noise - 0.5f) * intensity * 2.0f;
                    transform.localScale = initialScale * scaleMod;
                    break;

                case BlockAnimType.CrystalPulse:
                    // Tinh thể ma thuật lơ lửng bồng bềnh
                    float yBob = Mathf.Sin(t) * (intensity * 0.15f);
                    transform.localPosition = initialPosition + new Vector3(0, yBob, 0);
                    break;
            }
        }
    }
}
