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
            var existingShadow = transform.Find("GroundShadow") ?? transform.Find("Shadow");
            GameObject shadowObj;
            if (existingShadow != null)
            {
                shadowObj = existingShadow.gameObject;
                shadowTransform = existingShadow;
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();
                if (shadowRenderer == null) shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
            }
            else
            {
                shadowObj = new GameObject("GroundShadow");
                shadowObj.transform.parent = transform;
                shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
                shadowTransform = shadowObj.transform;
            }

            // Căn chỉnh tâm bóng đổ khớp 100% với điểm tiếp đất của bàn chân (Y = -0.19f)
            shadowObj.transform.localPosition = new Vector3(0f, -0.19f, 0f);
            shadowObj.transform.localScale = new Vector3(1.05f, 0.95f, 1f);
            shadowRenderer.sortingOrder = 7;

            Sprite shadowSp = Resources.Load<Sprite>("character_shadow");
            if (shadowSp != null)
            {
                shadowRenderer.sprite = shadowSp;
                shadowRenderer.color = new Color(1f, 1f, 1f, 0.85f);
            }
            else
            {
                shadowRenderer.sprite = Resources.Load<Sprite>("indicator_circle");
                shadowRenderer.color = new Color(0f, 0f, 0f, 0.38f);
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

        public static Sprite LoadMageSprite(string dirName, int frame, bool female = false)
        {
            string gender = female ? "female" : "male";
            string key = $"Characters/Mage/{gender}/mage_{dirName}_{frame}";
            if (spriteCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            Sprite loaded = Resources.Load<Sprite>(key);
            if (loaded == null)
            {
                Texture2D tex = Resources.Load<Texture2D>(key);
                if (tex != null)
                {
                    loaded = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.2f), 100f);
                }
            }

            // Fallback về thư mục Characters/Mage nếu chưa có file
            if (loaded == null)
            {
                string fallbackKey = $"Characters/Mage/mage_{dirName}_{frame}";
                loaded = Resources.Load<Sprite>(fallbackKey);
                if (loaded == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>(fallbackKey);
                    if (tex != null)
                    {
                        loaded = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.2f), 100f);
                    }
                }
            }

            if (loaded != null)
            {
                spriteCache[key] = loaded;
            }
            return loaded;
        }

        private PlayerMovement playerMovement;
        private float idleWindTimer = 0f;
        private float currentTilt = 0f;
        private float ghostTimer = 0f;

        private void Start()
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }

            string dirName = GetDirectionSpriteName(currentDirection);

            bool isSprinting = (playerMovement != null && playerMovement.IsRunning && isMoving);

            if (isMoving)
            {
                // Tốc độ hoạt ảnh bước đi: tự động tăng nhịp khi chạy nước rút (sprint)
                float currentFps = isSprinting ? walkFps * 1.45f : walkFps;
                animTimer += Time.deltaTime * currentFps;
                int frameIdx = ((int)animTimer) % 8;

                Sprite sp = LoadMageSprite(dirName, frameIdx, isFemale) ?? LoadMageSprite(dirName, 0, isFemale);
                if (sp != null) spriteRenderer.sprite = sp;

                // Độ nghiêng tự nhiên khi di chuyển theo chiều gió (Dynamic lean)
                float targetTilt = 0f;
                if (currentDirection == Direction8.West || currentDirection == Direction8.NorthWest || currentDirection == Direction8.SouthWest)
                {
                    targetTilt = 2.2f;
                }
                else if (currentDirection == Direction8.East || currentDirection == Direction8.NorthEast || currentDirection == Direction8.SouthEast)
                {
                    targetTilt = -2.2f;
                }
                currentTilt = Mathf.MoveTowards(currentTilt, targetTilt, Time.deltaTime * 18f);
                transform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
                transform.localScale = initialScale;

                // Dư ảnh bóng mờ khi lướt (Dash ghost afterimage)
                if (playerMovement != null && playerMovement.IsDashing)
                {
                    ghostTimer += Time.deltaTime;
                    if (ghostTimer >= 0.05f)
                    {
                        ghostTimer = 0f;
                        SpawnDashGhost();
                    }
                }
            }
            else
            {
                // Trạng thái nghỉ (Idle): Hoạt họa áo choàng bay nhẹ trong gió (Wind flutter)
                animTimer = 0f;
                currentTilt = Mathf.MoveTowards(currentTilt, 0f, Time.deltaTime * 12f);
                transform.localRotation = Quaternion.identity;

                idleWindTimer += Time.deltaTime;

                // Mỗi chu kỳ 3.6s, cơn gió nhẹ thổi qua làm tà áo choàng lay động (Frame 0 -> Frame 1 -> Frame 0)
                int idleFrame = 0;
                float cycle = idleWindTimer % 3.6f;
                if (cycle > 2.8f && cycle < 3.2f)
                {
                    idleFrame = 1; // Khẽ lay động tà áo choàng
                }

                Sprite sp = LoadMageSprite(dirName, idleFrame, isFemale) ?? LoadMageSprite(dirName, 0, isFemale);
                if (sp != null) spriteRenderer.sprite = sp;

                // Nhịp thở và dao động tà áo choàng bồng bềnh
                if (enableBreathing)
                {
                    float breath = Mathf.Sin(Time.time * breathSpeed) * breathScaleAmount;
                    float flutterX = Mathf.Cos(Time.time * (breathSpeed * 1.2f)) * (breathScaleAmount * 0.75f);
                    transform.localScale = new Vector3(initialScale.x * (1f + flutterX), initialScale.y * (1f + breath), initialScale.z);
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

        /// <summary>
        /// Tạo ảo ảnh bóng mờ áo choàng khi dash lướt nhanh
        /// </summary>
        public void SpawnDashGhost()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;

            GameObject ghost = new GameObject("DashGhost");
            ghost.transform.position = transform.position;
            ghost.transform.rotation = transform.rotation;
            ghost.transform.localScale = transform.localScale;

            var sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = spriteRenderer.sprite;
            sr.sortingOrder = spriteRenderer.sortingOrder - 1;

            // Màu viền áo choàng: Nam = Cyan ngọc, Nữ = Hồng cánh sen
            Color ghostColor = isFemale ? new Color(1.0f, 0.35f, 0.75f, 0.65f) : new Color(0.15f, 0.85f, 1.0f, 0.65f);
            sr.color = ghostColor;

            var fader = ghost.AddComponent<GhostFader>();
            fader.Setup(0.24f);
        }

        private class GhostFader : MonoBehaviour
        {
            private SpriteRenderer sr;
            private float lifetime = 0.24f;
            private float timer = 0f;
            private Color initialCol;

            public void Setup(float time)
            {
                lifetime = time;
                sr = GetComponent<SpriteRenderer>();
                if (sr != null) initialCol = sr.color;
            }

            private void Update()
            {
                timer += Time.deltaTime;
                if (timer >= lifetime || sr == null)
                {
                    Destroy(gameObject);
                    return;
                }
                float alpha = Mathf.Lerp(initialCol.a, 0f, timer / lifetime);
                Color c = initialCol;
                c.a = alpha;
                sr.color = c;
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
        public static Sprite GetSprite(Direction8 dir, int frameIndex, bool female = false)
        {
            string dirName = GetDirectionSpriteName(dir);
            int f = Mathf.Clamp(frameIndex, 0, 7);
            return LoadMageSprite(dirName, f, female) ?? LoadMageSprite(dirName, 0, female);
        }
    }
}
