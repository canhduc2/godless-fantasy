using System.Collections.Generic;
using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Hiệu ứng bụi bước chân 8-Bit hoạt họa động (Animated 8-bit Pixel Footstep Dust VFX):
    /// - Tự động tạo đám mây bụi pixel hoạt họa khi nhân vật di chuyển, chạy nhanh hoặc lướt (dash).
    /// - Sử dụng 4 frame chuỗi hoạt ảnh pixel art sắc nét tại Assets/Resources/VFX/vfx_dust_puff_{0..3}.
    /// - Quản lý Object Pool siêu nhẹ, không rác bộ nhớ (Zero Garbage Collection).
    /// </summary>
    public class PixelMovementVFX : MonoBehaviour
    {
        [Header("Cấu Hình Bụi 8-Bit")]
        [SerializeField] private float stepDustInterval = 0.28f;
        [SerializeField] private float sprintDustInterval = 0.14f;
        [SerializeField] private Vector3 footOffset = new Vector3(0f, -0.19f, 0f);
        [SerializeField] private float dustFps = 16f;
        [SerializeField] private int sortingOrder = 8; // Nằm sau nhân vật (order 10) và trên mặt đất

        private static Sprite[] dustSprites;
        private static bool isSpritesLoaded = false;

        private CharacterSpriteAnimator8Dir animator;
        private PlayerMovement playerMovement;

        private float dustTimer = 0f;
        private Vector3 lastPosition;
        private bool wasMoving = false;

        private readonly List<ActiveDustPuff> activePuffs = new List<ActiveDustPuff>();
        private readonly Queue<GameObject> puffPool = new Queue<GameObject>();

        private struct ActiveDustPuff
        {
            public GameObject obj;
            public SpriteRenderer sr;
            public float timer;
            public int currentFrame;
        }

        public static void PreloadSprites()
        {
            if (isSpritesLoaded) return;
            dustSprites = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                string path = $"VFX/vfx_dust_puff_{i}";
                Sprite sp = Resources.Load<Sprite>(path);
                if (sp == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>(path);
                    if (tex != null)
                    {
                        sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.2f), 32f);
                    }
                }
                dustSprites[i] = sp;
            }
            isSpritesLoaded = true;
        }

        private void Awake()
        {
            PreloadSprites();
            animator = GetComponent<CharacterSpriteAnimator8Dir>();
            playerMovement = GetComponent<PlayerMovement>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            bool isMoving = false;
            bool isSprinting = false;

            if (animator != null)
            {
                isMoving = animator.IsMoving;
            }
            if (playerMovement != null)
            {
                isMoving = isMoving || playerMovement.IsMoving;
                isSprinting = playerMovement.IsRunning;
            }
            else
            {
                // Fallback tính theo delta position
                float distMoved = Vector3.Distance(transform.position, lastPosition);
                isMoving = isMoving || (distMoved > 0.01f);
                isSprinting = distMoved > 0.12f;
            }

            // Đột ngột dừng lại (Skid dust stop)
            if (wasMoving && !isMoving)
            {
                SpawnDustPuff(1.15f, new Vector3(Random.Range(-0.08f, 0.08f), 0f, 0f));
            }
            wasMoving = isMoving;
            lastPosition = transform.position;

            // Di chuyển sinh bụi đều đặn
            if (isMoving)
            {
                dustTimer += dt;
                float interval = isSprinting ? sprintDustInterval : stepDustInterval;
                if (dustTimer >= interval)
                {
                    dustTimer = 0f;
                    float scale = isSprinting ? 1.25f : 0.95f;
                    Vector3 offset = footOffset + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.04f, 0.04f), 0f);
                    SpawnDustPuff(scale, offset);
                }
            }
            else
            {
                dustTimer = 0f;
            }

            // Cập nhật các hạt bụi đang hiển thị
            UpdateActivePuffs(dt);
        }

        /// <summary>
        /// Tạo vụ nổ bụi lướt (Dash burst) nhiều cụm bụi kéo dài theo vệt
        /// </summary>
        public void TriggerDashBurst(Vector2 direction)
        {
            for (int i = 0; i < 4; i++)
            {
                float t = i * 0.18f;
                Vector3 burstPos = footOffset - (Vector3)direction.normalized * t + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.05f, 0.05f), 0f);
                SpawnDustPuff(1.3f - i * 0.15f, burstPos);
            }
        }

        private void SpawnDustPuff(float scale, Vector3 offset)
        {
            GameObject puff;
            if (puffPool.Count > 0)
            {
                puff = puffPool.Dequeue();
                puff.SetActive(true);
            }
            else
            {
                puff = new GameObject("PixelDustPuff");
                var sr = puff.AddComponent<SpriteRenderer>();
                sr.sortingOrder = sortingOrder;
            }

            puff.transform.position = transform.position + offset;
            puff.transform.localScale = Vector3.one * scale * (Random.value > 0.5f ? 1f : -1f); // Ngẫu nhiên lật hướng cho sinh động

            var renderer = puff.GetComponent<SpriteRenderer>();
            if (dustSprites != null && dustSprites.Length > 0 && dustSprites[0] != null)
            {
                renderer.sprite = dustSprites[0];
            }

            activePuffs.Add(new ActiveDustPuff
            {
                obj = puff,
                sr = renderer,
                timer = 0f,
                currentFrame = 0
            });
        }

        private void UpdateActivePuffs(float dt)
        {
            for (int i = activePuffs.Count - 1; i >= 0; i--)
            {
                var puff = activePuffs[i];
                puff.timer += dt * dustFps;
                int frame = (int)puff.timer;

                if (frame >= 4 || dustSprites == null)
                {
                    puff.obj.SetActive(false);
                    puffPool.Enqueue(puff.obj);
                    activePuffs.RemoveAt(i);
                    continue;
                }

                if (frame != puff.currentFrame)
                {
                    puff.currentFrame = frame;
                    if (frame < dustSprites.Length && dustSprites[frame] != null)
                    {
                        puff.sr.sprite = dustSprites[frame];
                    }
                }

                activePuffs[i] = puff;
            }
        }

        private void OnDestroy()
        {
            foreach (var puff in activePuffs)
            {
                if (puff.obj != null) Destroy(puff.obj);
            }
            activePuffs.Clear();

            while (puffPool.Count > 0)
            {
                var p = puffPool.Dequeue();
                if (p != null) Destroy(p);
            }
        }
    }
}
