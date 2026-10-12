using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Hệ Thống Môi Trường, Chu Kỳ Ngày & Đêm, Ánh Sáng 2D Động, Sương Mù & Đom Đóm (Environment Atmosphere & Day-Night Engine):
    /// - Chu kỳ 24h thế giới (Day-Night Cycle) chuyển đổi siêu mượt mà:
    ///   + 🌅 Bình Minh (05:00 - 07:00): Ánh nắng mai hồng cam dịu mát.
    ///   + ☀️ Ban Ngày (07:00 - 17:00): Nắng vàng ấm áp, trong trẻo.
    ///   + 🌇 Hoàng Hôn (17:00 - 19:30): Rực rỡ sắc hổ phách, đỏ tía huyền ảo.
    ///   + 🌙 Ban Đêm (19:30 - 05:00): Màn đêm ánh trăng xanh thẳm huyền bí bao trùm.
    /// - Lớp phủ ánh sáng toàn màn hình (Full-screen Atmospheric Ambient Tint Overlay) bám theo Camera.
    /// - Đom đóm dạ quang (Bioluminescent Fireflies) tự động xuất hiện và bay lượn lấp lánh khi trời tối.
    /// - Ngọn đuốc (Torches) & Hào quang nhân vật (Player Light) tự động bùng sáng rực rỡ khi màn đêm buông xuống.
    /// - Lớp sương mù lãng đãng (Atmospheric Mist) trôi bồng bềnh qua cánh rừng.
    /// </summary>
    public class EnvironmentAtmosphereManager : MonoBehaviour
    {
        public static EnvironmentAtmosphereManager Instance { get; private set; }

        [Header("Day-Night Cycle Settings")]
        [Range(0f, 24f)]
        public float currentHour = 10.5f; // Bắt đầu lúc 10:30 sáng
        public float dayDurationMinutes = 12f; // 12 phút thực = 24 giờ game
        public bool isTimePaused = false;

        private Transform playerTransform;
        private Camera mainCamera;

        // Lớp phủ ánh sáng toàn màn hình bám theo Camera
        private GameObject cameraTintOverlayObj;
        private SpriteRenderer cameraTintSr;

        // Hào quang người chơi
        private GameObject playerLightObj;
        private SpriteRenderer playerLightSr;
        private Sprite radialLightSprite;

        // Danh sách đuốc được quản lý
        private List<TorchLightData> activeTorches = new List<TorchLightData>();

        // Lớp sương mù
        private GameObject mistContainer;
        private List<Transform> mistSlices = new List<Transform>();

        // Đom đóm đêm
        private GameObject fireflyContainer;
        private List<FireflyData> fireflies = new List<FireflyData>();

        public float CurrentHour => currentHour;
        public bool IsNight => currentHour >= 19.5f || currentHour < 5.0f;
        private string lastPhase = "";

        private class TorchLightData
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public float baseScale;
            public float noiseSeed;
            public Color lightColor;
        }

        private class FireflyData
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public Vector3 anchorOffset;
            public float seed;
            public float speed;
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
            SetupCameraTintOverlay();
            SetupPlayerVisionAura();
            SetupAtmosphericMist();
            SetupNightFireflies();
        }

        private void FindPlayer()
        {
            var p = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
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
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        /// <summary>
        /// Tạo tấm lọc ánh sáng toàn cảnh (Full-Screen Ambient Tint Overlay) bám chặt Camera
        /// </summary>
        private void SetupCameraTintOverlay()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            cameraTintOverlayObj = new GameObject("Camera_Ambient_Tint_Overlay");
            cameraTintOverlayObj.transform.parent = mainCamera.transform;
            cameraTintOverlayObj.transform.localPosition = new Vector3(0, 0, 5f); // Nằm trước Camera

            cameraTintSr = cameraTintOverlayObj.AddComponent<SpriteRenderer>();

            // Tạo texture vuông 1x1 trắng
            Texture2D whiteTex = Texture2D.whiteTexture;
            cameraTintSr.sprite = Sprite.Create(whiteTex, new Rect(0, 0, whiteTex.width, whiteTex.height), new Vector2(0.5f, 0.5f), 1);
            cameraTintSr.sortingOrder = 18; // Nằm trên địa hình và nhân vật, dưới HUD
            cameraTintSr.color = new Color(0, 0, 0, 0);
        }

        public void RegisterTorch(Vector3 position, Color? customColor = null, float scale = 3.6f)
        {
            GameObject lightObj = new GameObject("Torch_Light_Aura");
            lightObj.transform.parent = transform;
            lightObj.transform.position = position + new Vector3(0, 0.45f, 0);
            lightObj.transform.localScale = Vector3.one * scale;

            var sr = lightObj.AddComponent<SpriteRenderer>();
            sr.sprite = radialLightSprite;
            Color col = customColor ?? new Color(1.0f, 0.62f, 0.18f, 0.42f);
            sr.color = col;
            sr.sortingOrder = 9;

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
            playerLightObj.transform.localScale = Vector3.one * 5.6f;

            playerLightSr = playerLightObj.AddComponent<SpriteRenderer>();
            playerLightSr.sprite = radialLightSprite;
            playerLightSr.color = new Color(0.85f, 0.92f, 1.0f, 0.22f);
            playerLightSr.sortingOrder = 9;
        }

        private void SetupAtmosphericMist()
        {
            mistContainer = new GameObject("Atmospheric_Mist_Container");
            mistContainer.transform.parent = transform;

            for (int i = 0; i < 6; i++)
            {
                GameObject slice = new GameObject($"Mist_Slice_{i}");
                slice.transform.parent = mistContainer.transform;
                slice.transform.position = new Vector3(15f + i * 20f, (i % 2 == 0 ? 8f : -8f), 0);
                slice.transform.localScale = new Vector3(28f, 14f, 1f);

                var sr = slice.AddComponent<SpriteRenderer>();
                sr.sprite = radialLightSprite;
                sr.color = new Color(0.65f, 0.75f, 0.95f, 0.08f);
                sr.sortingOrder = 15;

                mistSlices.Add(slice.transform);
            }
        }

        private void SetupNightFireflies()
        {
            fireflyContainer = new GameObject("Night_Fireflies_Container");
            fireflyContainer.transform.parent = transform;

            for (int i = 0; i < 16; i++)
            {
                GameObject fly = new GameObject($"Firefly_{i}");
                fly.transform.parent = fireflyContainer.transform;

                var sr = fly.AddComponent<SpriteRenderer>();
                sr.sprite = radialLightSprite;
                sr.color = new Color(0.45f, 1.0f, 0.55f, 0f);
                sr.sortingOrder = 16;
                fly.transform.localScale = Vector3.one * 0.45f;

                fireflies.Add(new FireflyData
                {
                    transform = fly.transform,
                    renderer = sr,
                    anchorOffset = new Vector3(Random.Range(-15f, 15f), Random.Range(-10f, 10f), 0),
                    seed = Random.Range(0f, 100f),
                    speed = Random.Range(0.6f, 1.4f)
                });
            }
        }

        private void Update()
        {
            if (playerTransform == null) FindPlayer();

            UpdateDayNightTime();
            UpdateCameraTintOverlay();
            UpdateTorchFlicker();
            UpdateAtmosphericMist();
            UpdateNightFireflies();
        }

        private void UpdateDayNightTime()
        {
            if (!isTimePaused)
            {
                // 1 ngày game (24h) = dayDurationMinutes phút thực tế
                float hoursPerSecond = 24f / (dayDurationMinutes * 60f);
                currentHour = (currentHour + Time.deltaTime * hoursPerSecond) % 24f;

                string curPhase = GetTimePhaseName();
                if (!string.IsNullOrEmpty(lastPhase) && curPhase != lastPhase)
                {
                    AudioManager.PlayLevelUp();
                    Combat.FloatingDamageManager.ShowFloatingText(
                        playerTransform != null ? playerTransform.position + Vector3.up * 1.8f : Vector3.zero,
                        $"⏰ Chuyển Canh: {GetTimePhaseIcon()} {curPhase} ({GetFormattedTime()})",
                        new Color(1f, 0.85f, 0.35f)
                    );
                }
                lastPhase = curPhase;
            }
        }

        private void UpdateCameraTintOverlay()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || cameraTintSr == null) return;

            // Co giãn kích thước tấm phủ theo orthographicSize của Camera
            float camH = mainCamera.orthographicSize * 2.5f;
            float camW = camH * mainCamera.aspect * 1.3f;
            cameraTintOverlayObj.transform.localScale = new Vector3(camW, camH, 1f);

            // Tính màu sắc khí quyển theo giờ (Day-Night Color Curve)
            Color tintColor = CalculateAtmosphericTint(currentHour);

            // Điều chỉnh theo vị trí (Vùng Làng Tân Thủ sáng hơn, Rừng rậm tối hơn)
            if (playerTransform != null)
            {
                float px = playerTransform.position.x;
                if (px <= -20f)
                {
                    // Làng Tân Thủ: Giảm 30% độ tối để người chơi thấy rõ nhà cửa/shop
                    tintColor.a *= 0.70f;
                }
                else if (px > 20f)
                {
                    // Rừng Rậm: Tăng 15% độ sâu của màu màn đêm
                    tintColor.a = Mathf.Min(0.65f, tintColor.a * 1.15f);
                }
            }

            cameraTintSr.color = Color.Lerp(cameraTintSr.color, tintColor, Time.deltaTime * 3f);

            // Đổi màu nền Camera Background
            Color bgSky = CalculateSkyBackground(currentHour);
            mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, bgSky, Time.deltaTime * 2.5f);

            // Tăng cường Hào Quang Người Chơi khi trời tối
            if (playerLightSr != null)
            {
                float nightFactor = Mathf.Clamp01(cameraTintSr.color.a / 0.45f);
                float targetLightAlpha = Mathf.Lerp(0.12f, 0.45f, nightFactor);
                Color lc = playerLightSr.color;
                lc.a = Mathf.Lerp(lc.a, targetLightAlpha, Time.deltaTime * 3f);
                playerLightSr.color = lc;
            }
        }

        private Color CalculateAtmosphericTint(float hour)
        {
            // 05:00 - 07:00: Bình Minh (Hồng cam dịu)
            if (hour >= 5.0f && hour < 7.0f)
            {
                float t = (hour - 5.0f) / 2.0f;
                Color dawn = new Color(1.0f, 0.65f, 0.45f, 0.20f);
                Color day = new Color(1.0f, 0.95f, 0.85f, 0.02f);
                return Color.Lerp(dawn, day, t);
            }
            // 07:00 - 17:00: Ban Ngày (Nắng vàng trong trẻo, mỏng nhẹ)
            else if (hour >= 7.0f && hour < 17.0f)
            {
                return new Color(1.0f, 0.95f, 0.85f, 0.02f);
            }
            // 17:00 - 19.5f: Hoàng Hôn (Cam hổ phách, đỏ tía rực rỡ)
            else if (hour >= 17.0f && hour < 19.5f)
            {
                float t = (hour - 17.0f) / 2.5f;
                Color day = new Color(1.0f, 0.95f, 0.85f, 0.02f);
                Color dusk = new Color(0.95f, 0.38f, 0.15f, 0.32f);
                return Color.Lerp(day, dusk, t);
            }
            // 19:30 - 05:00: Ban Đêm (Màn đêm huyền bí ánh trăng xanh biếc)
            else
            {
                float nightProgression = (hour >= 19.5f) ? (hour - 19.5f) / 4.5f : (hour + 4.5f) / 9.5f;
                Color nightColor = new Color(0.04f, 0.08f, 0.26f, 0.52f);
                return nightColor;
            }
        }

        private Color CalculateSkyBackground(float hour)
        {
            if (hour >= 7.0f && hour < 17.0f) return new Color(0.10f, 0.14f, 0.22f); // Trời ngày
            if (hour >= 17.0f && hour < 19.5f) return new Color(0.18f, 0.08f, 0.14f); // Trời hoàng hôn
            return new Color(0.02f, 0.03f, 0.06f); // Trời đêm
        }

        private void UpdateTorchFlicker()
        {
            float t = Time.time * 6.5f;
            float nightBoost = IsNight ? 1.45f : 1.0f; // Ban đêm đuốc bùng sáng mạnh hơn

            for (int i = 0; i < activeTorches.Count; i++)
            {
                var torch = activeTorches[i];
                if (torch.transform == null) continue;

                float noise = Mathf.PerlinNoise(t, torch.noiseSeed);
                float scalePulse = torch.baseScale * (0.92f + noise * 0.16f) * (IsNight ? 1.18f : 1.0f);
                torch.transform.localScale = Vector3.one * scalePulse;

                Color c = torch.lightColor;
                c.a = torch.lightColor.a * (0.85f + noise * 0.30f) * nightBoost;
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

                if (slice.position.x > 140f)
                {
                    slice.position = new Vector3(10f, slice.position.y, 0);
                }
            }
        }

        private void UpdateNightFireflies()
        {
            float targetAlpha = IsNight ? 0.85f : 0.0f;

            for (int i = 0; i < fireflies.Count; i++)
            {
                var fly = fireflies[i];
                if (fly == null || fly.transform == null) continue;

                Color col = fly.renderer.color;
                col.a = Mathf.Lerp(col.a, targetAlpha, Time.deltaTime * 2f);
                fly.renderer.color = col;

                if (col.a > 0.01f && playerTransform != null)
                {
                    float waveX = Mathf.Sin(Time.time * fly.speed + fly.seed) * 3.5f;
                    float waveY = Mathf.Cos(Time.time * (fly.speed * 1.2f) + fly.seed) * 2.2f;
                    fly.transform.position = playerTransform.position + fly.anchorOffset + new Vector3(waveX, waveY, 0);
                }
            }
        }

        // ================= HELPER CHO UI COMBAT HUD =================
        public string GetFormattedTime()
        {
            int h = Mathf.FloorToInt(currentHour);
            int m = Mathf.FloorToInt((currentHour - h) * 60f);
            return $"{h:D2}:{m:D2}";
        }

        public string GetTimePhaseName()
        {
            if (currentHour >= 5.0f && currentHour < 7.0f) return "Bình Minh";
            if (currentHour >= 7.0f && currentHour < 17.0f) return "Ban Ngày";
            if (currentHour >= 17.0f && currentHour < 19.5f) return "Hoàng Hôn";
            return "Đêm Trăng";
        }

        public string GetTimePhaseIcon()
        {
            if (currentHour >= 5.0f && currentHour < 7.0f) return "🌅";
            if (currentHour >= 7.0f && currentHour < 17.0f) return "☀️";
            if (currentHour >= 17.0f && currentHour < 19.5f) return "🌇";
            return "🌙";
        }
    }
}
