using System.Collections.Generic;
using UnityEngine;

namespace GodlessFantasy.World
{
    /// <summary>
    /// Hiệu ứng ngọn lửa 8-Bit hoạt họa động (Animated 8-bit Pixel Fire):
    /// - Vòng lặp hoạt ảnh ngọn lửa pixel phong cách Fantasy cổ điển (6 frame).
    /// - Tự động tạo ánh sáng bập bùng (flickering light aura) và tàn lửa bay lơ lửng (rising 8-bit embers).
    /// - Tương thích hoàn hảo với mọi trụ đuốc, đống lửa trại, đền thờ và vạc lửa di tích.
    /// </summary>
    public class AnimatedPixelFire : MonoBehaviour
    {
        [Header("Cấu Hình Hoạt Ảnh Lửa 8-Bit")]
        [SerializeField] private float animFps = 11f;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.46f, 0f);
        [SerializeField] private float flameScale = 1.0f;
        [SerializeField] private int sortingOrder = 15;

        [Header("Hiệu Ứng Ánh Sáng & Tàn Lửa")]
        [SerializeField] private bool enableLightPulse = true;
        [SerializeField] private bool enableEmbers = true;
        [SerializeField] private float emberSpawnInterval = 0.22f;

        private static Sprite[] flameFrames;
        private static bool isLoaded = false;

        private GameObject flameObj;
        private SpriteRenderer flameRenderer;
        private GameObject lightAuraObj;
        private SpriteRenderer lightAuraRenderer;

        private float animTimer = 0f;
        private int currentFrame = 0;
        private float emberTimer = 0f;
        private readonly List<EmberParticle> activeEmbers = new List<EmberParticle>();

        private struct EmberParticle
        {
            public GameObject obj;
            public SpriteRenderer sr;
            public Vector3 velocity;
            public float lifetime;
            public float maxLifetime;
        }

        public static void PreloadFrames()
        {
            if (isLoaded) return;
            flameFrames = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                string path = $"VFX/vfx_flame_8bit_{i}";
                Sprite sp = Resources.Load<Sprite>(path);
                if (sp == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>(path);
                    if (tex != null)
                    {
                        sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.15f), 32f);
                    }
                }
                flameFrames[i] = sp;
            }
            isLoaded = true;
        }

        private void Awake()
        {
            PreloadFrames();
            SetupFlameObject();
            SetupLightAura();
        }

        private void SetupFlameObject()
        {
            flameObj = new GameObject("PixelFlame");
            flameObj.transform.SetParent(transform, false);
            flameObj.transform.localPosition = localOffset;
            flameObj.transform.localScale = Vector3.one * flameScale;

            flameRenderer = flameObj.AddComponent<SpriteRenderer>();
            flameRenderer.sortingOrder = sortingOrder;
            if (flameFrames != null && flameFrames.Length > 0 && flameFrames[0] != null)
            {
                flameRenderer.sprite = flameFrames[0];
            }
        }

        private void SetupLightAura()
        {
            if (!enableLightPulse) return;

            lightAuraObj = new GameObject("FlameLightAura");
            lightAuraObj.transform.SetParent(transform, false);
            lightAuraObj.transform.localPosition = localOffset + new Vector3(0f, 0.1f, 0f);
            lightAuraObj.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

            lightAuraRenderer = lightAuraObj.AddComponent<SpriteRenderer>();
            lightAuraRenderer.sortingOrder = sortingOrder - 1;
            lightAuraRenderer.color = new Color(1.0f, 0.72f, 0.2f, 0.28f);

            Sprite circleSp = Resources.Load<Sprite>("indicator_circle");
            if (circleSp != null)
            {
                lightAuraRenderer.sprite = circleSp;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Vòng lặp chuyển frame lửa 8-bit
            animTimer += dt * animFps;
            if (animTimer >= 1.0f)
            {
                animTimer -= 1.0f;
                currentFrame = (currentFrame + 1) % 6;
                if (flameFrames != null && currentFrame < flameFrames.Length && flameFrames[currentFrame] != null)
                {
                    flameRenderer.sprite = flameFrames[currentFrame];
                }
            }

            // 2. Vi rung rinh ngọn lửa tự nhiên (Subtle Organic Flutter)
            if (flameObj != null)
            {
                float flutterX = Mathf.Sin(Time.time * 16f) * 0.02f;
                float flutterScaleY = 1.0f + Mathf.Cos(Time.time * 12f) * 0.06f;
                flameObj.transform.localScale = new Vector3(flameScale + flutterX, flameScale * flutterScaleY, 1f);
            }

            // 3. Xung ánh sáng bập bùng
            if (lightAuraRenderer != null)
            {
                float pulse = 0.24f + Mathf.Sin(Time.time * 8.5f) * 0.06f + (Random.value - 0.5f) * 0.03f;
                lightAuraRenderer.color = new Color(1.0f, 0.70f, 0.18f, pulse);
            }

            // 4. Sinh tàn lửa bay lên (Floating 8-bit Embers)
            if (enableEmbers)
            {
                emberTimer += dt;
                if (emberTimer >= emberSpawnInterval)
                {
                    emberTimer = 0f;
                    SpawnEmber();
                }
                UpdateEmbers(dt);
            }
        }

        private void SpawnEmber()
        {
            if (activeEmbers.Count >= 8) return;

            GameObject eObj = new GameObject("Ember");
            eObj.transform.SetParent(transform, false);
            Vector3 startPos = localOffset + new Vector3(Random.Range(-0.12f, 0.12f), Random.Range(0.15f, 0.35f), 0f);
            eObj.transform.localPosition = startPos;
            eObj.transform.localScale = Vector3.one * Random.Range(0.04f, 0.07f);

            var sr = eObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder + 1;
            sr.color = new Color(1.0f, Random.Range(0.6f, 0.95f), 0.1f, 0.9f);
            sr.sprite = Resources.Load<Sprite>("indicator_circle");

            activeEmbers.Add(new EmberParticle
            {
                obj = eObj,
                sr = sr,
                velocity = new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.45f, 0.85f), 0f),
                lifetime = 0f,
                maxLifetime = Random.Range(0.6f, 1.1f)
            });
        }

        private void UpdateEmbers(float dt)
        {
            for (int i = activeEmbers.Count - 1; i >= 0; i--)
            {
                var ember = activeEmbers[i];
                ember.lifetime += dt;
                if (ember.lifetime >= ember.maxLifetime || ember.obj == null)
                {
                    if (ember.obj != null) Destroy(ember.obj);
                    activeEmbers.RemoveAt(i);
                    continue;
                }

                // Bay lên và uốn lượn nhẹ
                Vector3 pos = ember.obj.transform.localPosition;
                pos += ember.velocity * dt;
                pos.x += Mathf.Sin(Time.time * 10f + ember.lifetime * 5f) * 0.05f * dt;
                ember.obj.transform.localPosition = pos;

                // Mờ dần theo thời gian
                float alpha = 1.0f - (ember.lifetime / ember.maxLifetime);
                if (ember.sr != null)
                {
                    Color c = ember.sr.color;
                    c.a = alpha;
                    ember.sr.color = c;
                }

                activeEmbers[i] = ember;
            }
        }

        private void OnDestroy()
        {
            foreach (var ember in activeEmbers)
            {
                if (ember.obj != null) Destroy(ember.obj);
            }
            activeEmbers.Clear();
        }
    }
}
