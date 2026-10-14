using System;
using System.Collections.Generic;
using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Bộ Điều Khiển Hoạt Ảnh Frame-by-Frame 8 Hướng (8-Directional Traditional Sprite Animator):
    /// - Quản lý hoạt ảnh pixel art 8 hướng x 8 Frame bước đi (Walk Cycle) nhịp nhàng, sắc nét.
    /// - Hai trạng thái cốt lõi:
    ///   + Idle (Đứng yên): Hiển thị Frame 0 thế đứng tự nhiên theo hướng ngắm.
    ///   + Walking (Di chuyển): Vòng lặp 8 frame bước đi mượt mà không trượt chân.
    /// - Không có animation chạy (run) riêng, không có animation dash riêng.
    /// - Hỗ trợ cả nhân vật người chơi cục bộ (Local Player) lẫn người chơi từ xa (Remote Multiplayer Player).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterSpriteAnimator8Dir : MonoBehaviour
    {
        [Header("Cấu Hình Hoạt Ảnh")]
        [SerializeField] private float walkFps = 8.5f; // Tốc độ hoạt ảnh bước đi (8 frame/chu kỳ)
        [SerializeField] private Direction8 currentDirection = Direction8.South;
        [SerializeField] private bool isMoving = false;
        [SerializeField] private bool isFemale = false;

        [Header("Hiệu Ứng Phụ Trợ")]
        [SerializeField] private bool enableBreathing = true; // Nhịp thở vi mô khi đứng yên
        [SerializeField] private float breathSpeed = 2.4f;
        [SerializeField] private float breathScaleAmount = 0.015f;

        // Ma trận 64 frame sprite: [8 hướng, 8 frame]
        private static readonly Sprite[,] mageSprites = new Sprite[8, 8];
        private static bool isSpritesLoaded = false;

        private SpriteRenderer spriteRenderer;
        private Transform shadowTransform;
        private SpriteRenderer shadowRenderer;

        private float animTimer = 0f;
        private Vector3 initialScale = Vector3.one;
        private bool inStealth = false;

        public Direction8 FacingDirection
        {
            get => currentDirection;
            set => currentDirection = value;
        }

        public bool IsMoving
        {
            get => isMoving;
            set => isMoving = value;
        }

        public bool IsFemale
        {
            get => isFemale;
            set => isFemale = value;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = 10;
            initialScale = transform.localScale;

            SetupGroundShadow();
        }

        private void SetupGroundShadow()
        {
            var existingShadow = transform.Find("GroundShadow");
            if (existingShadow != null)
            {
                shadowTransform = existingShadow;
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();
            }
            else
            {
                var shadowObj = new GameObject("GroundShadow");
                shadowObj.transform.parent = transform;
                shadowObj.transform.localPosition = new Vector3(0f, -0.42f, 0f);
                shadowObj.transform.localScale = new Vector3(0.6f, 0.28f, 1f);
                shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
                shadowRenderer.sortingOrder = 7;
                shadowRenderer.color = new Color(0f, 0f, 0f, 0.38f);
                shadowRenderer.sprite = Resources.Load<Sprite>("indicator_circle");
                shadowTransform = shadowObj.transform;
            }
        }

        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static string GetDirectionSpriteName(Direction8 dir)
        {
            switch (dir)
            {
                case Direction8.South: return "south";
                case Direction8.SouthEast: return "south_east";
                case Direction8.East: return "east";
                case Direction8.NorthEast: return "north_east";
                case Direction8.North: return "north";
                case Direction8.NorthWest: return "north_west";
                case Direction8.West: return "west";
                case Direction8.SouthWest: return "south_west";
                default: return "south";
            }
        }

        public static Sprite LoadMageSprite(string dirName, int frame)
        {
            string key = $"Characters/Mage/mage_{dirName}_{frame}";
            if (spriteCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            Sprite loaded = Resources.Load<Sprite>(key);
            if (loaded == null)
            {
                // Fallback thử load Texture2D rồi tạo Sprite nếu Unity chưa import dạng Sprite
                Texture2D tex = Resources.Load<Texture2D>(key);
                if (tex != null)
                {
                    loaded = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.2f), 100f);
                }
            }

            if (loaded != null)
            {
                spriteCache[key] = loaded;
            }
            return loaded;
        }

        private void Update()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }

            string dirName = GetDirectionSpriteName(currentDirection);

            if (isMoving)
            {
                // Chu kỳ hoạt ảnh bước đi 8 frame
                animTimer += Time.deltaTime * walkFps;
                int frameIdx = ((int)animTimer) % 8;

                Sprite sp = LoadMageSprite(dirName, frameIdx) ?? LoadMageSprite(dirName, 0);
                if (sp != null) spriteRenderer.sprite = sp;

                // Reset scale bình thường khi bước đi
                transform.localScale = initialScale;
            }
            else
            {
                // Trạng thái nghỉ (Idle): Cố định Frame 0 (Thế đứng chuẩn y hệt Preview)
                animTimer = 0f;
                Sprite sp = LoadMageSprite(dirName, 0) ?? LoadMageSprite("south", 0);
                if (sp != null) spriteRenderer.sprite = sp;

                // Nhịp thở bồng bềnh nhẹ nhàng
                if (enableBreathing)
                {
                    float breath = Mathf.Sin(Time.time * breathSpeed) * breathScaleAmount;
                    transform.localScale = new Vector3(initialScale.x * (1f - breath * 0.5f), initialScale.y * (1f + breath), initialScale.z);
                }
            }

            // Đồng bộ màu trong suốt ẩn thân
            float targetAlpha = inStealth ? 0.35f : 1.0f;
            Color c = spriteRenderer.color;
            if (Mathf.Abs(c.a - targetAlpha) > 0.01f)
            {
                c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * 6f);
                spriteRenderer.color = c;
            }
        }

        public void SetStealth(bool stealth)
        {
            inStealth = stealth;
        }

        public void SetDirection(Direction8 dir)
        {
            currentDirection = dir;
        }

        public void SetMoving(bool moving)
        {
            isMoving = moving;
        }

        public void SetGender(bool female)
        {
            isFemale = female;
        }

        /// <summary>
        /// Lấy Sprite cụ thể theo hướng và số thứ tự frame (0..7)
        /// </summary>
        public static Sprite GetSprite(Direction8 dir, int frameIndex)
        {
            string dirName = GetDirectionSpriteName(dir);
            int f = Mathf.Clamp(frameIndex, 0, 7);
            return LoadMageSprite(dirName, f) ?? LoadMageSprite(dirName, 0);
        }
    }
}
