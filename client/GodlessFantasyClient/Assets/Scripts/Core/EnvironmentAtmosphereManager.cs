using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Hệ Thống Môi Trường, Ánh Sáng 2D Động, Sương Mù & Bóng Tối (Environment Atmosphere & 2D Lighting Engine):
    /// - Quản lý màu ánh sáng môi trường (Ambient Light Tint) tự động chuyển đổi theo vùng:
    ///   + Làng Tân Thủ: Nắng vàng dịu ấm áp, an toàn.
    ///   + Ranh Giới: Ánh hoàng hôn mờ ảo chuyển tiếp.
    ///   + Rừng Rậm Hoang Dã: Sắc tím sẫm ma mị, âm u, huyền bí.
    ///   + Hầm Ngục: Màn đêm bóng tối bao trùm.
    /// - Quầng sáng đốm lửa bập bùng (Flickering Torches): Tự động tạo hào quang ấm cam nhấp nháy theo thời gian thực.
    /// - Hào quang tầm nhìn người chơi (Player Vision Aura): Quầng sáng di động theo sau nhân vật, xua tan bóng tối khi vào rừng.
    /// - Lớp sương mù lãng đãng (Atmospheric Mist Layer): Dải sương mờ trôi lững lờ ngang qua cánh rừng.
    /// </summary>
    public class EnvironmentAtmosphereManager : MonoBehaviour
    {
        public static EnvironmentAtmosphereManager Instance { get; private set; }

        private Transform playerTransform;
        private Camera mainCamera;

        // Màu môi trường theo vùng
        private readonly Color villageAmbient = new Color(0.09f, 0.12f, 0.18f, 1f);
        private readonly Color borderAmbient = new Color(0.06f, 0.08f, 0.14f, 1f);
        private readonly Color wildernessAmbient = new Color(0.03f, 0.04f, 0.08f, 1f);

        // Hào quang người chơi
        private GameObject playerLightObj;
        private SpriteRenderer playerLightSr;
        private Sprite radialLightSprite;

        // Danh sách đuốc được quản lý
        private List<TorchLightData> activeTorches = new List<TorchLightData>();

        // Lớp sương mù
        private GameObject mistContainer;
        private List<Transform> mistSlices = new List<Transform>();

        private class TorchLightData
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public float baseScale;
            public float noiseSeed;
            public Color lightColor;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            mainCamera = Camera.main;
            radialLightSprite = CreateRadialLightSprite();
        }

        private void Start()
        {
            FindPlayer();
            SetupPlayerVisionAura();
            SetupAtmosphericMist();
        }

        private void FindPlayer()
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else
            {
                var pm = FindAnyObjectByType<PlayerMovement>();
                if (pm != null) playerTransform = pm.transform;
            }
        }

        /// <summary>
        /// Tạo texture quầng sáng tròn chuyển sắc (Radial Gradient Falloff) siêu mượt
        /// </summary>
        private Sprite CreateRadialLightSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] cols = new Color[size * size];
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha); // Smooth falloff curve
                    cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        /// <summary>
        /// Đăng ký một cọc đuốc để kích hoạt quầng sáng bập bùng
        /// </summary>
        public void RegisterTorch(Vector3 position, Color? customColor = null, float scale = 3.2f)
        {
            GameObject lightObj = new GameObject("Torch_Light_Aura");
            lightObj.transform.parent = transform;
            lightObj.transform.position = position + new Vector3(0, 0.45f, 0); // Vị trí ngọn lửa trên đỉnh cọc
            lightObj.transform.localScale = Vector3.one * scale;

            var sr = lightObj.AddComponent<SpriteRenderer>();
            sr.sprite = radialLightSprite;
            Color col = customColor ?? new Color(1.0f, 0.62f, 0.18f, 0.38f);
            sr.color = col;
            sr.sortingOrder = 9; // Chiếu sáng trên mặt đất và dưới nhân vật/đạn

            activeTorches.Add(new TorchLightData
            {
                transform = lightObj.transform,
                renderer = sr,
                baseScale = scale,
                noiseSeed = Random.Range(0f, 100f),
                lightColor = col
            });
        }

        private void SetupPlayerVisionAura()
        {
            if (playerTransform == null) return;

            playerLightObj = new GameObject("Player_Vision_Light");
            playerLightObj.transform.parent = playerTransform;
            playerLightObj.transform.localPosition = Vector3.zero;
            playerLightObj.transform.localScale = Vector3.one * 5.2f;

            playerLightSr = playerLightObj.AddComponent<SpriteRenderer>();
            playerLightSr.sprite = radialLightSprite;
            playerLightSr.color = new Color(0.85f, 0.92f, 1.0f, 0.22f);
            playerLightSr.sortingOrder = 9;
        }

        private void SetupAtmosphericMist()
        {
            mistContainer = new GameObject("Atmospheric_Mist_Container");
            mistContainer.transform.parent = transform;

            // Tạo các dải sương mù mỏng trôi lững lờ tại khu vực rừng rậm hoang dã
            for (int i = 0; i < 4; i++)
            {
                GameObject slice = new GameObject($"Mist_Slice_{i}");
                slice.transform.parent = mistContainer.transform;
                slice.transform.position = new Vector3(10f + i * 16f, (i % 2 == 0 ? 4f : -4f), 0);
                slice.transform.localScale = new Vector3(20f, 10f, 1f);

                var sr = slice.AddComponent<SpriteRenderer>();
                sr.sprite = radialLightSprite;
                sr.color = new Color(0.65f, 0.75f, 0.95f, 0.08f); // Sương mù mỏng nhẹ
                sr.sortingOrder = 15; // Nằm lơ lửng trên đầu quái vật và địa hình

                mistSlices.Add(slice.transform);
            }
        }

        private void Update()
        {
            if (playerTransform == null) FindPlayer();

            UpdateAmbientLighting();
            UpdateTorchFlicker();
            UpdateAtmosphericMist();
        }

        private void UpdateAmbientLighting()
        {
            if (mainCamera == null || playerTransform == null) return;

            float px = playerTransform.position.x;
            Color targetBg;

            if (px <= -8f)
            {
                targetBg = villageAmbient;
            }
            else if (px <= 4f)
            {
                float t = (px - (-8f)) / 12f;
                targetBg = Color.Lerp(villageAmbient, wildernessAmbient, t);
            }
            else
            {
                targetBg = wildernessAmbient;
            }

            mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, targetBg, Time.deltaTime * 2.5f);

            // Tăng nhẹ cường độ ánh sáng của người chơi khi bước vào vùng tối
            if (playerLightSr != null)
            {
                float darkFactor = Mathf.Clamp01((px - (-2f)) / 10f);
                float targetAlpha = Mathf.Lerp(0.12f, 0.32f, darkFactor);
                Color c = playerLightSr.color;
                c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 3f);
                playerLightSr.color = c;
            }
        }

        private void UpdateTorchFlicker()
        {
            float t = Time.time * 6.5f;

            for (int i = 0; i < activeTorches.Count; i++)
            {
                var torch = activeTorches[i];
                if (torch.transform == null) continue;

                // Hiệu ứng ngọn lửa bập bùng Perlin Noise
                float noise = Mathf.PerlinNoise(t, torch.noiseSeed);
                float scalePulse = torch.baseScale * (0.92f + noise * 0.16f);
                torch.transform.localScale = Vector3.one * scalePulse;

                Color c = torch.lightColor;
                c.a = torch.lightColor.a * (0.85f + noise * 0.30f);
                torch.renderer.color = c;
            }
        }

        private void UpdateAtmosphericMist()
        {
            float driftSpeed = 0.65f * Time.deltaTime;

            for (int i = 0; i < mistSlices.Count; i++)
            {
                var slice = mistSlices[i];
                if (slice == null) continue;

                slice.position += new Vector3(driftSpeed, Mathf.Sin(Time.time * 0.5f + i) * 0.08f * Time.deltaTime, 0);

                // Cuộn tuần hoàn khi sương trôi quá xa sang phải
                if (slice.position.x > 46f)
                {
                    slice.position = new Vector3(2f, slice.position.y, 0);
                }
            }
        }
    }
}
