using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Core;
using GodlessFantasy.Player;

namespace GodlessFantasy.Combat
{
    public enum TrajectoryType
    {
        Linear = 0,     // Bay thẳng tắp
        SineWave = 1,   // Lượn sóng uốn lượn hình sin
        Homing = 2,     // Tự động bẻ lái bám đuổi mục tiêu
        LobbedArc = 3,  // Bay cầu vồng rơi từ trên cao xuống đất
        Boomerang = 4,  // Bay ra xa rồi giật ngược trở về
        Instant = 5     // Bộc phát tức thời (Cột sáng, bốc thiên)
    }

    public enum BaseShapeMask
    {
        Orb = 0,        // Cầu ma pháp
        Slash = 1,      // Vệt kiếm khí trăng khuyết
        Arrow = 2,      // Mũi đại tiễn khí động học
        Wave = 3,       // Sóng xung kích hình quạt
        Meteor = 4,     // Vẫn thạch sao băng
        NovaRing = 5,   // Vòng sóng xung kích 360 độ
        Pillar = 6,     // Cột sáng bốc thiên
        Beam = 7,       // Luồng tia laser kéo dài
        Tentacle = 8,   // Xúc tu ma thuật uốn lượn trồi lên từ đất
        VineSprout = 9, // Dây leo rễ cây gai nhọn mọc trồi lên từ đất
        IceSpikes = 10, // Bụi gai băng tinh thể nhọn hoắt mọc trồi từ lòng đất
        FloraBloom = 11,// Bông hoa ma pháp / mộc nở rộ bộc phát
        FlameGeyser = 12,// Cột lửa địa nhiệt phun trào từ lòng đất
        ThunderStrike = 13,// Tia sét hồ quang giáng thẳng cắm đất
        HolyCross = 14, // Thánh giá hoàng kim phát quang
        TornadoWhirl = 15,// Cơn lốc xoáy rẽ gió cuộn xoay
        Skull = 16,     // Đầu lâu oán linh tử thần
        DragonHead = 17,// Hỏa long thần thú gầm thét phun lửa
        Dagger = 18,    // Phi đao song nhận thích khách
        Shield = 19,    // Kim cang hộ thuẫn bảo giáp
        BloodDrop = 20, // Huyết châu ma đạo / huyết gai trồi đất
        VoidVortex = 21,// Hố đen xoáy hư không nuốt chửng
        GiantHammer = 22,// Chiến chùy lôi đình bạt mạng
        Phoenix = 23    // Phượng hoàng lửa niết bàn sải cánh
    }

    /// <summary>
    /// Bảng Màu & Thông Số 7 Hệ Nguyên Tố Độc Nhất (Elemental Visual Signature Definition):
    /// Cung cấp 5 tầng màu: Core (Lõi), Body (Thân), Rim (Viền), Particle (Hạt tàn vụn), Trail (Vệt đuôi)
    /// cùng thuộc tính vật lý đặc thù cho từng nguyên tố (Lửa bốc lên, Băng tinh thể, Sét ziczac...).
    /// </summary>
    [Serializable]
    public class ElementalPalette
    {
        public string ElementName;
        public Color CoreColor;       // Lõi rực sáng
        public Color BodyColor;       // Thân năng lượng
        public Color RimColor;        // Viền bóng khí
        public Color ParticleColor;   // Vụn nổ văng ra
        public Gradient TrailGradient;

        public static ElementalPalette GetPalette(string element)
        {
            string el = (element ?? "Hỏa").ToLower();

            if (el.Contains("băng") || el.Contains("ice") || el.Contains("frost"))
            {
                return new ElementalPalette
                {
                    ElementName = "Băng",
                    CoreColor = new Color(0.95f, 1.0f, 1.0f, 1.0f),      // Trắng tuyết tinh thể
                    BodyColor = new Color(0.35f, 0.85f, 1.0f, 0.95f),    // Lam tuyết sắc lạnh
                    RimColor = new Color(0.05f, 0.40f, 0.90f, 0.90f),    // Lam đậm sâu thẳm
                    ParticleColor = new Color(0.70f, 0.95f, 1.0f, 1.0f), // Mảnh băng vỡ
                    TrailGradient = CreateGradient(new Color(0.35f, 0.85f, 1f), Color.white)
                };
            }
            if (el.Contains("lôi") || el.Contains("lightning") || el.Contains("thunder") || el.Contains("sấm"))
            {
                return new ElementalPalette
                {
                    ElementName = "Lôi",
                    CoreColor = new Color(1.0f, 1.0f, 0.85f, 1.0f),      // Trắng chớp điện
                    BodyColor = new Color(0.85f, 0.25f, 1.0f, 0.95f),    // Tím hồ quang cao áp
                    RimColor = new Color(0.40f, 0.0f, 0.85f, 0.90f),     // Tím đậm từ trường
                    ParticleColor = new Color(1.0f, 0.95f, 0.25f, 1.0f), // Tia lửa điện vàng chanh
                    TrailGradient = CreateGradient(new Color(0.85f, 0.25f, 1f), new Color(1f, 1f, 0.4f))
                };
            }
            if (el.Contains("phong") || el.Contains("wind") || el.Contains("ngọc"))
            {
                return new ElementalPalette
                {
                    ElementName = "Phong",
                    CoreColor = new Color(0.95f, 1.0f, 0.98f, 1.0f),     // Bạch phong khí
                    BodyColor = new Color(0.25f, 0.90f, 0.70f, 0.95f),   // Lục ngọc bão lốc
                    RimColor = new Color(0.08f, 0.55f, 0.38f, 0.90f),   // Lục phong ba
                    ParticleColor = new Color(0.55f, 1.0f, 0.85f, 1.0f), // Lưỡi gió cắt
                    TrailGradient = CreateGradient(new Color(0.25f, 0.90f, 0.70f), Color.white)
                };
            }
            if (el.Contains("mộc") || el.Contains("wood") || el.Contains("sinh") || el.Contains("nature"))
            {
                return new ElementalPalette
                {
                    ElementName = "Mộc",
                    CoreColor = new Color(0.92f, 1.0f, 0.70f, 1.0f),     // Mầm sống rực rỡ
                    BodyColor = new Color(0.30f, 0.85f, 0.15f, 0.95f),   // Xanh diệp lục tươi
                    RimColor = new Color(0.12f, 0.45f, 0.05f, 0.90f),   // Dây leo cổ thụ
                    ParticleColor = new Color(0.55f, 0.95f, 0.25f, 1.0f),// Cánh hoa / phấn sinh mệnh
                    TrailGradient = CreateGradient(new Color(0.30f, 0.85f, 0.15f), new Color(0.9f, 1f, 0.5f))
                };
            }
            if (el.Contains("quang") || el.Contains("light") || el.Contains("holy") || el.Contains("thánh") || el.Contains("sáng"))
            {
                return new ElementalPalette
                {
                    ElementName = "Quang",
                    CoreColor = new Color(1.0f, 1.0f, 1.0f, 1.0f),       // Thánh quang chói lòa
                    BodyColor = new Color(1.0f, 0.88f, 0.25f, 0.95f),    // Hoàng kim rực rỡ
                    RimColor = new Color(0.85f, 0.60f, 0.05f, 0.90f),    // Hào quang vàng kim
                    ParticleColor = new Color(1.0f, 0.96f, 0.60f, 1.0f), // Bụi sao thiên giới
                    TrailGradient = CreateGradient(new Color(1f, 0.88f, 0.25f), Color.white)
                };
            }
            if (el.Contains("ma") || el.Contains("dark") || el.Contains("hắc") || el.Contains("ám") || el.Contains("tối"))
            {
                return new ElementalPalette
                {
                    ElementName = "Ám",
                    CoreColor = new Color(0.95f, 0.75f, 1.0f, 1.0f),     // Tâm hư không tím
                    BodyColor = new Color(0.50f, 0.0f, 0.75f, 0.95f),     // Hắc ám ma mị
                    RimColor = new Color(0.18f, 0.0f, 0.30f, 0.90f),     // Vực thẳm tử thần
                    ParticleColor = new Color(0.75f, 0.15f, 0.95f, 1.0f),// Khói Miasma
                    TrailGradient = CreateGradient(new Color(0.50f, 0.0f, 0.75f), new Color(0.85f, 0.4f, 1f))
                };
            }

            // Mặc định: Hỏa (Fire)
            return new ElementalPalette
            {
                ElementName = "Hỏa",
                CoreColor = new Color(1.0f, 0.96f, 0.65f, 1.0f),        // Lõi trắng vàng thiêu đốt
                BodyColor = new Color(1.0f, 0.45f, 0.05f, 0.95f),        // Cam lửa hừng hực
                RimColor = new Color(0.85f, 0.12f, 0.05f, 0.90f),        // Đỏ lửa cuồng nộ
                ParticleColor = new Color(1.0f, 0.75f, 0.20f, 1.0f),     // Tàn tro bốc cháy
                TrailGradient = CreateGradient(new Color(1f, 0.45f, 0.05f), new Color(1f, 0.9f, 0.4f))
            };
        }

        private static Gradient CreateGradient(Color startCol, Color endCol)
        {
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(startCol, 0.0f), new GradientColorKey(endCol, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            return grad;
        }
    }

    /// <summary>
    /// Bộ Điều Khiển Hoạt Ảnh Thủ Tục Đặc Trưng Nguyên Tố (Elemental Dynamic VFX Animator):
    /// Liên tục sinh hoạt ảnh sống động theo thời gian thực cho từng hệ nguyên tố:
    /// - Hỏa: Ngọn lửa bập bùng, tàn tro bốc lên, rung rinh nhiệt lượng
    /// - Băng: Tinh thể băng xoay đa giác, gai băng nhọn, sương lạnh
    /// - Lôi: Tia sét ziczac rẽ nhánh ngẫu nhiên, hồ quang chớp giật
    /// - Quang: Thánh giá xoay, hào quang tỏa sáng, bụi sao lấp lánh
    /// - Ám: Xoáy hư không hút vật chất, xúc tu bóng tối uốn lượn
    /// - Phong: Lốc xoáy ốc hai dải, lưỡi dao gió cắt ngang, lá bay
    /// - Mộc: Dây leo gai uốn lượn, cánh hoa nở rộ, phấn hoa phục hồi
    /// </summary>
    public class ElementalDynamicVFXAnimator : MonoBehaviour
    {
        public ElementalPalette Palette;
        public BaseShapeMask Shape = BaseShapeMask.Orb;

        private float emitTimer = 0f;
        private SpriteRenderer sr;
        private LineRenderer lightningLine;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (Palette != null && Palette.ElementName == "Lôi")
            {
                SetupLightningRenderer();
            }
        }

        private void SetupLightningRenderer()
        {
            lightningLine = gameObject.AddComponent<LineRenderer>();
            lightningLine.material = new Material(Shader.Find("Sprites/Default"));
            lightningLine.startWidth = 0.12f;
            lightningLine.endWidth = 0.03f;
            lightningLine.positionCount = 5;
            lightningLine.useWorldSpace = false;
            lightningLine.startColor = Palette.CoreColor;
            lightningLine.endColor = Palette.BodyColor;
            lightningLine.sortingOrder = 24;
        }

        private void Update()
        {
            if (Palette == null) return;
            emitTimer += Time.deltaTime;

            // 1. Cập nhật đặc trưng hình ảnh trực tiếp trên lõi
            AnimateCoreVisuals();

            // 2. Bắn hạt năng lượng động đặc trưng (Emission rate: 15-20 hạt/giây)
            if (emitTimer >= 0.06f)
            {
                emitTimer = 0f;
                EmitElementalParticle();
            }
        }

        private void AnimateCoreVisuals()
        {
            float t = Time.time;
            switch (Palette.ElementName)
            {
                case "Hỏa":
                    // Ngọn lửa bập bùng (Flickering flame pulse)
                    float flameScale = 1.0f + Mathf.Sin(t * 18f) * 0.12f + UnityEngine.Random.Range(-0.05f, 0.05f);
                    transform.localScale = new Vector3(flameScale, flameScale * 1.15f, 1f);
                    break;

                case "Băng":
                    // Tinh thể băng xoay đều góc sắc nhọn
                    transform.Rotate(0, 0, 180f * Time.deltaTime);
                    break;

                case "Lôi":
                    // Cập nhật ziczac tia sét hồ quang (Arc crackle)
                    if (lightningLine != null)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            float xOffset = (i == 0 || i == 4) ? 0f : UnityEngine.Random.Range(-0.35f, 0.35f);
                            float yOffset = (i - 2) * 0.25f + UnityEngine.Random.Range(-0.1f, 0.1f);
                            lightningLine.SetPosition(i, new Vector3(xOffset, yOffset, 0));
                        }
                    }
                    break;

                case "Quang":
                    // Thánh giá phát quang xoay chậm 45 độ/s
                    transform.Rotate(0, 0, 45f * Time.deltaTime);
                    break;

                case "Ám":
                    // Xoáy hư không xoay ngược chiều kim đồng hồ
                    transform.Rotate(0, 0, -220f * Time.deltaTime);
                    break;

                case "Phong":
                    // Lốc xoáy rung lắc nhẹ
                    float windWobble = Mathf.Sin(t * 24f) * 0.15f;
                    transform.localScale = new Vector3(1.0f + windWobble, 1.0f - windWobble, 1f);
                    break;

                case "Mộc":
                    // Dây leo nhấp nhô nhịp thở
                    float vinePulse = 1.0f + Mathf.PingPong(t * 3f, 0.18f);
                    transform.localScale = new Vector3(vinePulse, vinePulse, 1f);
                    break;
            }
        }

        private void EmitElementalParticle()
        {
            if (ProceduralVFXEngine.Instance == null) return;
            ProceduralVFXEngine.Instance.SpawnSingleDynamicParticle(transform.position, Palette);
        }
    }

    /// <summary>
    /// Bộ phát hoạt ảnh Frame-by-Frame Pixel Art (Pixel VFX Frame Animator):
    /// Điều khiển chuỗi 4 frame sprite liên tục ở tốc độ 12-16 FPS cho 7 hệ và các kỹ năng đặc biệt
    /// (Lửa bập bùng, Băng trồi, Sét giật, Hào quang, Lốc xoáy, Mộc nở hoa, Đại kiếm cắm đất, Cánh thiên thần).
    /// </summary>
    public class PixelVFXFrameAnimator : MonoBehaviour
    {
        public Sprite[] Frames;
        public float FrameRate = 12.0f;
        public bool Loop = true;
        public bool DestroyOnEnd = false;

        private SpriteRenderer sr;
        private float timer = 0f;
        private int currentIdx = 0;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (Frames != null && Frames.Length > 0 && sr != null)
            {
                sr.sprite = Frames[0];
            }
        }

        private void Update()
        {
            if (Frames == null || Frames.Length <= 1 || sr == null) return;

            timer += Time.deltaTime;
            float frameDuration = 1.0f / Mathf.Max(1f, FrameRate);

            if (timer >= frameDuration)
            {
                timer -= frameDuration;
                currentIdx++;
                if (currentIdx >= Frames.Length)
                {
                    if (Loop)
                    {
                        currentIdx = 0;
                    }
                    else
                    {
                        currentIdx = Frames.Length - 1;
                        if (DestroyOnEnd) Destroy(gameObject);
                    }
                }
                sr.sprite = Frames[currentIdx];
            }
        }
    }

    /// <summary>
    /// Bộ phát vệt đuôi hạt Pixel Sparks (Pixel Spark Trail Emitter):
    /// Liên tục thả các mảnh vụn pixel art li ti lấp lánh dọc theo đường đạn bay.
    /// </summary>
    public class PixelSparkTrailEmitter : MonoBehaviour
    {
        public ElementalPalette Palette;
        private float emitTimer = 0f;

        private void Update()
        {
            if (Palette == null) return;
            emitTimer += Time.deltaTime;
            if (emitTimer >= 0.035f)
            {
                emitTimer = 0f;
                if (ProceduralVFXEngine.Instance != null)
                {
                    ProceduralVFXEngine.Instance.SpawnSingleSparkParticle(
                        transform.position,
                        (Vector2)UnityEngine.Random.insideUnitCircle * 0.35f,
                        Palette.BodyColor,
                        0.14f,
                        0.22f
                    );
                }
            }
        }
    }

    /// <summary>
    /// Đạn Phép Thuật Thủ Tục Đa Quỹ Đạo Pixel Art (Procedural Pixel Projectile Controller):
    /// Điều khiển toàn diện các quỹ đạo bay (Linear, Sine Wave, Homing, Lobbed Arc, Boomerang)
    /// kèm co giãn Squash & Stretch theo vận tốc, vệt đuôi Trail, và bung nổ hạt va chạm!
    /// </summary>
    public class ProceduralProjectile : MonoBehaviour
    {
        public TrajectoryType Trajectory = TrajectoryType.Linear;
        public float FlySpeed = 16.0f;
        public float WaveAmplitude = 0.65f;
        public float WaveFrequency = 12.0f;
        public float MaxLifespan = 1.8f;
        public float ExplosionRadius = 1.8f;
        public int Damage = 25;
        public int PiercingCount = 1;
        public bool HasCameraShake = false;
        public string ShooterId = "Player";
        public ElementalPalette Palette;

        // Cơ chế sóng quạt quét nở rộng bề ngang theo tiến trình bay
        public bool IsExpandingWave = false;
        public float WaveBaseWidth = 0.85f;
        public float WaveExpandSpeed = 1.8f;

        // Cơ chế khống chế cự ly bay & điểm dừng chính xác theo vùng kỹ năng
        [System.NonSerialized] public Vector3? TargetStopPosition = null;
        public float MaxDistance = 14.0f;

        private Vector2 initialDirection;
        private Vector3 startPosition;
        private float lifeTimer = 0f;
        private Transform targetTransform;
        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private int hitsRemaining;

        public void Launch(Vector2 direction, Transform target = null)
        {
            initialDirection = direction.normalized;
            startPosition = transform.position;
            targetTransform = target;
            hitsRemaining = PiercingCount;
            lifeTimer = 0f;

            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            sr = GetComponent<SpriteRenderer>();

            // Gắn bộ sinh hoạt ảnh động nguyên tố
            var anim = gameObject.AddComponent<ElementalDynamicVFXAnimator>();
            anim.Palette = Palette;

            // Xoay hướng mặt đạn theo hướng bắn
            float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= MaxLifespan)
            {
                TriggerImpactAndDestroy(transform.position);
                return;
            }

            // 1. Kiểm tra điểm dừng chính xác (Dành cho AoE Rain hoặc kỹ năng có điểm đáp định trước)
            if (TargetStopPosition.HasValue)
            {
                float distToTarget = Vector3.Distance(transform.position, TargetStopPosition.Value);
                Vector3 toTarget = TargetStopPosition.Value - transform.position;
                if (distToTarget <= 0.35f || Vector3.Dot(initialDirection, toTarget) <= 0f)
                {
                    TriggerImpactAndDestroy(TargetStopPosition.Value);
                    return;
                }
            }
            else
            {
                // 2. Kiểm tra cự ly bay tối đa: Đạn chỉ bay tối đa trong phạm vi castRange rồi tự nổ tung!
                if (Vector3.Distance(transform.position, startPosition) >= MaxDistance)
                {
                    TriggerImpactAndDestroy(transform.position);
                    return;
                }
            }

            Vector3 nextPos = transform.position;
            Vector2 currentVelocity = initialDirection * FlySpeed;

            switch (Trajectory)
            {
                case TrajectoryType.Linear:
                    nextPos += (Vector3)(initialDirection * (FlySpeed * Time.deltaTime));
                    break;

                case TrajectoryType.SineWave:
                    Vector2 perp = new Vector2(-initialDirection.y, initialDirection.x);
                    float forwardDist = FlySpeed * lifeTimer;
                    float waveOffset = Mathf.Sin(lifeTimer * WaveFrequency) * WaveAmplitude;
                    nextPos = startPosition + (Vector3)(initialDirection * forwardDist + perp * waveOffset);
                    currentVelocity = (nextPos - transform.position) / Mathf.Max(0.001f, Time.deltaTime);
                    break;

                case TrajectoryType.Homing:
                    if (targetTransform == null) FindNearestTarget();
                    if (targetTransform != null)
                    {
                        Vector2 toTarget = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
                        initialDirection = Vector2.MoveTowards(initialDirection, toTarget, 4.5f * Time.deltaTime).normalized;
                    }
                    nextPos += (Vector3)(initialDirection * (FlySpeed * Time.deltaTime));
                    currentVelocity = initialDirection * FlySpeed;
                    break;

                case TrajectoryType.LobbedArc:
                    float tNorm = Mathf.Clamp01(lifeTimer / MaxLifespan);
                    float totalDistance = FlySpeed * (MaxLifespan * 0.75f);
                    float arcY = 4.0f * 1.8f * tNorm * (1.0f - tNorm);
                    Vector3 flatStep = startPosition + (Vector3)(initialDirection * (totalDistance * tNorm));
                    nextPos = flatStep + new Vector3(0, arcY, 0);
                    currentVelocity = (nextPos - transform.position) / Mathf.Max(0.001f, Time.deltaTime);
                    break;

                case TrajectoryType.Boomerang:
                    float halfLife = MaxLifespan * 0.5f;
                    float progress = (lifeTimer < halfLife)
                        ? (lifeTimer / halfLife)
                        : (2.0f - lifeTimer / halfLife);
                    nextPos = startPosition + (Vector3)(initialDirection * (FlySpeed * halfLife * Mathf.Sin(progress * Mathf.PI * 0.5f)));
                    currentVelocity = (nextPos - transform.position) / Mathf.Max(0.001f, Time.deltaTime);
                    break;
            }

            if (IsExpandingWave)
            {
                float prog = Mathf.Clamp01(lifeTimer / Mathf.Max(0.1f, MaxLifespan));
                float curW = WaveBaseWidth + WaveExpandSpeed * prog;
                transform.localScale = new Vector3(curW, 0.85f, 1f);
            }

            transform.position = nextPos;
        }

        private void FindNearestTarget()
        {
            var allTargets = FindObjectsByType<MonsterTarget>(FindObjectsInactive.Exclude);
            float nearestDist = 12.0f;
            foreach (var mt in allTargets)
            {
                if (mt == null) continue;
                var h = mt.GetComponent<Health>();
                if (h != null && (h.IsDead || h.IsInSafeZone)) continue;

                float d = Vector2.Distance(transform.position, mt.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    targetTransform = mt.transform;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || other.gameObject.name == ShooterId) return;
            if (other.isTrigger && other.GetComponent<IDamageable>() == null) return;

            var targetDmg = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
            if (targetDmg != null && !targetDmg.IsDead)
            {
                targetDmg.TakeDamage(Damage, ShooterId);
                FloatingDamageManager.Spawn(other.transform.position + Vector3.up * 0.4f, Damage, isCrit: (Trajectory == TrajectoryType.Homing));
                AudioManager.PlayHit();

                hitsRemaining--;
                if (hitsRemaining <= 0)
                {
                    TriggerImpactAndDestroy(other.transform.position);
                }
            }
        }

        public void TriggerImpactAndDestroy(Vector3 impactPos)
        {
            if (ExplosionRadius > 0.5f)
            {
                Collider2D[] splashHits = Physics2D.OverlapCircleAll(impactPos, ExplosionRadius);
                foreach (var col in splashHits)
                {
                    if (col == null || col.gameObject.name == ShooterId) continue;
                    var dmg = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                    if (dmg != null && !dmg.IsDead)
                    {
                        int splashDmg = Mathf.Max(1, Damage / 2);
                        dmg.TakeDamage(splashDmg, ShooterId);
                        FloatingDamageManager.Spawn(col.transform.position + Vector3.up * 0.3f, splashDmg, false);
                    }
                }
            }

            if (ProceduralVFXEngine.Instance != null && Palette != null)
            {
                ProceduralVFXEngine.Instance.SpawnPixelImpactBurst(impactPos, Palette, ExplosionRadius);
            }

            if (HasCameraShake && CameraController.Instance != null)
            {
                CameraController.Instance.Shake(0.25f, 0.20f);
            }

            AudioManager.PlayExplosion();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Engine Quản Lý Toàn Bộ Hoạt Ảnh Thủ Tục 140 Biến Thể Nguyên Tố (Procedural VFX Engine):
    /// - Quản lý 8 Phôi Khối Base Shape Masks (Orb, Slash, Arrow, Wave, Meteor, NovaRing, Pillar, Beam)
    /// - Quản lý Bảng Màu 7 Hệ Nguyên Tố (Hỏa, Băng, Lôi, Phong, Mộc, Quang, Ám)
    /// - Hỗ trợ đầy đủ CẢ 6 DẠNG TẦM ĐÁNH (Bắn Thẳng, Vùng Đất AoE, Hình Quạt, Nổ 360, Tự Thân, Kết Giới)
    /// </summary>
    public class ProceduralVFXEngine : MonoBehaviour
    {
        public static ProceduralVFXEngine Instance { get; private set; }

        private Dictionary<BaseShapeMask, Sprite> maskSprites = new Dictionary<BaseShapeMask, Sprite>();
        private Dictionary<string, Sprite[]> animSequences = new Dictionary<string, Sprite[]>();
        private Material pixelSpriteMaterial;
        private Sprite sparkSprite;
        private Sprite novaRingSprite;

        public Sprite GetSparkSprite()
        {
            if (sparkSprite != null) return sparkSprite;
            int size = 8;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = Color.clear;
            Color white = Color.white;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - 3);
                    int dy = Mathf.Abs(y - 3);
                    bool filled = (dx + dy <= 3);
                    tex.SetPixel(x, y, filled ? white : clear);
                }
            }
            tex.Apply();
            sparkSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return sparkSprite;
        }

        public Sprite GetNovaRingSprite()
        {
            if (novaRingSprite != null) return novaRingSprite;
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = Color.clear;
            Color white = Color.white;
            float c = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    bool filled = (dist >= 8.5f && dist <= 12.5f);
                    tex.SetPixel(x, y, filled ? white : clear);
                }
            }
            tex.Apply();
            novaRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return novaRingSprite;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeEngine();
            }
            else
            {
                Destroy(this);
            }
        }

        private void InitializeEngine()
        {
            pixelSpriteMaterial = new Material(Shader.Find("Sprites/Default"));

            // Nạp 11 bộ Hoạt ảnh Frame-by-Frame Pixel Art đích thực
            LoadAnimSequence("vfx_fire", 4);
            LoadAnimSequence("vfx_ice", 4);
            LoadAnimSequence("vfx_lightning", 4);
            LoadAnimSequence("vfx_holy", 4);
            LoadAnimSequence("vfx_dark", 4);
            LoadAnimSequence("vfx_wind", 4);
            LoadAnimSequence("vfx_nature", 4);
            LoadAnimSequence("vfx_sword_impale", 4);
            LoadAnimSequence("vfx_seraph_wings", 4);
            LoadAnimSequence("vfx_crescent_slash", 4);
            LoadAnimSequence("vfx_arrow_rain", 4);

            maskSprites[BaseShapeMask.Orb] = LoadMask("vfx_mask_orb", BaseShapeMask.Orb);
            maskSprites[BaseShapeMask.Slash] = LoadMask("vfx_mask_slash", BaseShapeMask.Slash);
            maskSprites[BaseShapeMask.Arrow] = LoadMask("vfx_mask_arrow", BaseShapeMask.Arrow);
            maskSprites[BaseShapeMask.Wave] = LoadMask("vfx_mask_wave", BaseShapeMask.Wave);
            maskSprites[BaseShapeMask.Meteor] = LoadMask("vfx_mask_meteor", BaseShapeMask.Meteor);
            maskSprites[BaseShapeMask.NovaRing] = LoadMask("vfx_mask_novaring", BaseShapeMask.NovaRing);
            maskSprites[BaseShapeMask.Pillar] = LoadMask("vfx_mask_pillar", BaseShapeMask.Pillar);
            maskSprites[BaseShapeMask.Beam] = LoadMask("vfx_mask_beam", BaseShapeMask.Beam);
            maskSprites[BaseShapeMask.Tentacle] = LoadMask("vfx_mask_tentacle", BaseShapeMask.Tentacle);
            maskSprites[BaseShapeMask.VineSprout] = LoadMask("vfx_mask_vinesprout", BaseShapeMask.VineSprout);
            maskSprites[BaseShapeMask.IceSpikes] = LoadMask("vfx_mask_icespikes", BaseShapeMask.IceSpikes);
            maskSprites[BaseShapeMask.FloraBloom] = LoadMask("vfx_mask_florabloom", BaseShapeMask.FloraBloom);
            maskSprites[BaseShapeMask.FlameGeyser] = LoadMask("vfx_mask_flamegeyser", BaseShapeMask.FlameGeyser);
            maskSprites[BaseShapeMask.ThunderStrike] = LoadMask("vfx_mask_thunderstrike", BaseShapeMask.ThunderStrike);
            maskSprites[BaseShapeMask.HolyCross] = LoadMask("vfx_mask_holycross", BaseShapeMask.HolyCross);
            maskSprites[BaseShapeMask.TornadoWhirl] = LoadMask("vfx_mask_tornadowhirl", BaseShapeMask.TornadoWhirl);
            maskSprites[BaseShapeMask.Skull] = LoadMask("vfx_mask_skull", BaseShapeMask.Skull);
            maskSprites[BaseShapeMask.DragonHead] = LoadMask("vfx_mask_dragonhead", BaseShapeMask.DragonHead);
            maskSprites[BaseShapeMask.Dagger] = LoadMask("vfx_mask_dagger", BaseShapeMask.Dagger);
            maskSprites[BaseShapeMask.Shield] = LoadMask("vfx_mask_shield", BaseShapeMask.Shield);
            maskSprites[BaseShapeMask.BloodDrop] = LoadMask("vfx_mask_blooddrop", BaseShapeMask.BloodDrop);
            maskSprites[BaseShapeMask.VoidVortex] = LoadMask("vfx_mask_voidvortex", BaseShapeMask.VoidVortex);
            maskSprites[BaseShapeMask.GiantHammer] = LoadMask("vfx_mask_gianthammer", BaseShapeMask.GiantHammer);
            maskSprites[BaseShapeMask.Phoenix] = LoadMask("vfx_mask_phoenix", BaseShapeMask.Phoenix);
        }

        private void LoadAnimSequence(string prefix, int count)
        {
            List<Sprite> list = new List<Sprite>();
            for (int i = 0; i < count; i++)
            {
                Sprite sp = Resources.Load<Sprite>($"VFX/{prefix}_{i}");
                if (sp != null) list.Add(sp);
            }
            if (list.Count > 0) animSequences[prefix] = list.ToArray();
        }

        public Sprite[] GetAnimSequence(string key)
        {
            if (animSequences.TryGetValue(key, out var arr)) return arr;
            return null;
        }

        private Sprite LoadMask(string maskName, BaseShapeMask shape)
        {
            return CreateProceduralMask(shape);
        }

        private Sprite CreateProceduralMask(BaseShapeMask shape)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = Color.clear;
            Color white = Color.white;
            float center = (size - 1) / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    bool filled = false;

                    switch (shape)
                    {
                        case BaseShapeMask.Orb:
                            filled = (dx * dx + dy * dy) <= (10f * 10f);
                            break;
                        case BaseShapeMask.Slash:
                            // Crescent slash arc centered
                            float r = Mathf.Sqrt(dx * dx + dy * dy);
                            filled = (r >= 5f && r <= 13f && dx >= -3f && Mathf.Abs(dy) <= 12f);
                            break;
                        case BaseShapeMask.Arrow:
                            // Arrowhead pointing right
                            filled = (Mathf.Abs(dy) <= (12f - dx) * 0.65f && dx >= -10f && dx <= 11f);
                            break;
                        case BaseShapeMask.Wave:
                            // Broad sweeping wave
                            filled = (Mathf.Abs(dx - dy * 0.35f) <= 4f && Mathf.Abs(dy) <= 11f);
                            break;
                        case BaseShapeMask.Meteor:
                            // Round rock with tail
                            filled = ((dx * dx + dy * dy) <= (8f * 8f)) || (dx <= 0 && Mathf.Abs(dy) <= (5f + dx * 0.4f) && dx >= -12f);
                            break;
                        case BaseShapeMask.NovaRing:
                            // Concentric hollow ring
                            float rNova = Mathf.Sqrt(dx * dx + dy * dy);
                            filled = (rNova >= 6f && rNova <= 12f);
                            break;
                        case BaseShapeMask.Pillar:
                            filled = (Mathf.Abs(dx) <= 5f && Mathf.Abs(dy) <= 13f);
                            break;
                        case BaseShapeMask.Beam:
                            filled = (Mathf.Abs(dy) <= 4f && Mathf.Abs(dx) <= 14f);
                            break;
                        case BaseShapeMask.Tentacle:
                            // Xúc tu ma thuật uốn lượn hình chữ S
                            float spineX = Mathf.Sin((dy + 12f) / 24f * Mathf.PI * 1.5f) * 6f;
                            float tentacleThick = Mathf.Lerp(5.5f, 1.2f, (dy + 12f) / 24f);
                            filled = (Mathf.Abs(dx - spineX) <= tentacleThick && dy >= -13f && dy <= 13f);
                            if (!filled && dy >= -8f && dy <= 6f)
                            {
                                float suckerDist = Mathf.Abs(dx - (spineX - tentacleThick - 1.5f));
                                if (suckerDist <= 1.5f && (Mathf.Abs(dy % 4f) <= 1.2f)) filled = true;
                            }
                            break;
                        case BaseShapeMask.VineSprout:
                            // Dây leo rễ cây gai nhọn mọc trồi lên
                            float rootSpine = Mathf.Sin((dy + 12f) * 0.25f) * 3f;
                            float rootThick = Mathf.Lerp(5f, 1f, (dy + 12f) / 24f);
                            filled = (Mathf.Abs(dx - rootSpine) <= rootThick && dy >= -13f && dy <= 13f);
                            if (!filled && dy >= -6f && dy <= 4f)
                            {
                                if (dx < rootSpine && dx >= rootSpine - 6f && Mathf.Abs(dy - (-2f) - (rootSpine - dx) * 0.6f) <= 1.2f) filled = true;
                                if (dx > rootSpine && dx <= rootSpine + 6f && Mathf.Abs(dy - (3f) - (dx - rootSpine) * 0.6f) <= 1.2f) filled = true;
                            }
                            break;
                        case BaseShapeMask.IceSpikes:
                            // Bụi gai băng tinh thể 3 mũi nhọn hoắt
                            bool mainSpike = (dy >= -13f && dy <= 13f && Mathf.Abs(dx) <= (13f - dy) * 0.42f);
                            bool leftSpike = (dy >= -13f && dy <= 7f && Mathf.Abs(dx - (-7f + (7f - dy) * 0.25f)) <= (7f - dy) * 0.35f);
                            bool rightSpike = (dy >= -13f && dy <= 9f && Mathf.Abs(dx - (7f - (9f - dy) * 0.25f)) <= (9f - dy) * 0.35f);
                            filled = mainSpike || leftSpike || rightSpike;
                            break;
                        case BaseShapeMask.FloraBloom:
                            // Bông hoa ma pháp 5 cánh nở rộ
                            float rFlower = Mathf.Sqrt(dx * dx + dy * dy);
                            float aFlower = Mathf.Atan2(dy, dx);
                            float petalDist = 11f + Mathf.Sin(aFlower * 5f) * 3.5f;
                            filled = (rFlower <= petalDist) && !(rFlower >= 3.5f && rFlower <= 5f && Mathf.Sin(aFlower * 5f) < -0.3f);
                            break;
                        case BaseShapeMask.FlameGeyser:
                            // Cột lửa địa nhiệt phun trào từ lòng đất
                            float flameBaseW = Mathf.Lerp(4f, 11f, (dy + 12f) / 24f);
                            float jagged = Mathf.Sin(dy * 1.8f) * 2.5f;
                            filled = (dy >= -13f && dy <= 13f && Mathf.Abs(dx) <= (flameBaseW + jagged) * (1f - (dy - 5f) / 18f));
                            break;
                        case BaseShapeMask.ThunderStrike:
                            // Tia sét hồ quang giáng thẳng cắm đất
                            float boltX = (dy < -4f) ? (dy * 0.4f) : (dy < 4f) ? (-2f + dy * 0.8f) : (1f - dy * 0.5f);
                            filled = (dy >= -14f && dy <= 14f && Mathf.Abs(dx - boltX) <= (dy <= -10f ? 1.5f : 3.2f));
                            if (!filled && dy >= -3f && dy <= 6f && dx >= boltX && dx <= boltX + 7f)
                            {
                                if (Mathf.Abs((dy - 2f) - (dx - boltX) * 0.5f) <= 1.2f) filled = true;
                            }
                            break;
                        case BaseShapeMask.HolyCross:
                            // Thánh giá hoàng kim phát quang
                            bool vertB = (Mathf.Abs(dx) <= 3.5f && Mathf.Abs(dy) <= 13f);
                            bool horizB = (Mathf.Abs(dy) <= 3.5f && Mathf.Abs(dx) <= 10f);
                            bool haloB = (Mathf.Abs(dx) + Mathf.Abs(dy) <= 8f && Mathf.Abs(dx) + Mathf.Abs(dy) >= 5f);
                            filled = vertB || horizB || haloB;
                            break;
                        case BaseShapeMask.TornadoWhirl:
                            // Cơn lốc xoáy rẽ gió
                            float funnelW = Mathf.Lerp(2.5f, 13f, (dy + 12f) / 24f);
                            float spiralRidge = Mathf.Sin(dy * 1.2f) * 2f;
                            filled = (dy >= -13f && dy <= 13f && Mathf.Abs(dx + spiralRidge) <= funnelW);
                            break;
                        case BaseShapeMask.Skull:
                            // Đầu lâu oán linh tử thần
                            bool cranium = ((dx * dx + (dy - 2f) * (dy - 2f) <= 9.5f * 9.5f) && dy >= -2f);
                            bool jaw = (dy >= -10f && dy < -2f && Mathf.Abs(dx) <= 6.2f);
                            bool eyeSockets = ((Mathf.Abs(Mathf.Abs(dx) - 3.5f) <= 1.8f) && Mathf.Abs(dy - 1.2f) <= 2.0f);
                            bool noseHole = (Mathf.Abs(dx) <= 1.2f && dy >= -3.5f && dy <= -1.2f);
                            bool teethSlots = (dy <= -6f && dy >= -8.5f && (Mathf.Abs(dx) == 2f || dx == 0f));
                            filled = (cranium || jaw) && !eyeSockets && !noseHole && !teethSlots;
                            break;
                        case BaseShapeMask.DragonHead:
                            // Hỏa long thần thú gầm thét phun lửa
                            bool dragonSnout = (dx >= -4f && dx <= 13f && dy >= -6f && dy <= 6f);
                            bool jawMouth = (dx >= 1f && dx <= 12f && Mathf.Abs(dy) <= (dx - 1f) * 0.42f);
                            bool horns = (dx >= -12f && dx <= -2f && dy >= 2f && Mathf.Abs(dy - (4f - dx * 0.7f)) <= 2.2f);
                            bool dragonEye = (dx >= -2f && dx <= 1f && dy >= 2.5f && dy <= 4.5f);
                            bool mane = (dx >= -11f && dx <= -4f && dy >= -5f && dy <= 2f);
                            filled = (dragonSnout || horns || mane) && !jawMouth && !dragonEye;
                            break;
                        case BaseShapeMask.Dagger:
                            // Phi đao song nhận thích khách
                            bool blade = (dx >= -3f && dx <= 14f && Mathf.Abs(dy) <= (14f - dx) * 0.35f);
                            bool crossguard = (Mathf.Abs(dx - (-4f)) <= 1.5f && Mathf.Abs(dy) <= 7f);
                            bool hilt = (dx >= -11f && dx <= -4f && Mathf.Abs(dy) <= 2f);
                            float pommelR = (dx - (-12f)) * (dx - (-12f)) + dy * dy;
                            bool pommel = (pommelR <= 2.8f * 2.8f && pommelR >= 1.2f * 1.2f);
                            bool fullerGroove = (dx >= -2f && dx <= 9f && Mathf.Abs(dy) <= 0.6f);
                            filled = (blade || crossguard || hilt || pommel) && !fullerGroove;
                            break;
                        case BaseShapeMask.Shield:
                            // Kim cang hộ thuẫn bảo giáp (Kite Shield)
                            bool topShield = (dy >= 0f && dy <= 12f && Mathf.Abs(dx) <= 10f);
                            bool botShield = (dy >= -13f && dy < 0f && Mathf.Abs(dx) <= 10f * (1f + dy / 13f));
                            filled = (topShield || botShield);
                            break;
                        case BaseShapeMask.BloodDrop:
                            // Huyết châu ma đạo / gai huyết ngưng kết
                            float rDrop = Mathf.Sqrt(dx * dx + (dy + 2f) * (dy + 2f));
                            bool dropBody = (dy <= -2f && rDrop <= 9f) || (dy > -2f && dy <= 13f && Mathf.Abs(dx) <= (13f - dy) * 0.65f);
                            bool bloodSpikes = (dy >= -5f && dy <= 1f) && Mathf.Abs(Mathf.Abs(dx) - (8f + (dy + 2f) * 0.8f)) <= 1.5f;
                            filled = dropBody || bloodSpikes;
                            break;
                        case BaseShapeMask.VoidVortex:
                            // Hố đen xoáy hư không nuốt chửng
                            float rV = Mathf.Sqrt(dx * dx + dy * dy);
                            float aV = Mathf.Atan2(dy, dx);
                            filled = (rV >= 2.5f && rV <= 13f && Mathf.Sin(rV * 0.8f - aV * 3f) > 0.05f) || (rV <= 1.8f);
                            break;
                        case BaseShapeMask.GiantHammer:
                            // Chiến chùy lôi đình bạt mạng
                            bool handle = (dx >= -12f && dx <= 3f && Mathf.Abs(dy - (-dx * 0.6f)) <= 1.5f);
                            bool hammerHead = (dx >= 2f && dx <= 12f && dy >= -8f && dy <= 6f);
                            bool spikeTip = (dx >= 12f && dx <= 15f && Mathf.Abs(dy - (-1f)) <= (15f - dx) * 0.9f);
                            filled = handle || hammerHead || spikeTip;
                            break;
                        case BaseShapeMask.Phoenix:
                            // Phượng hoàng lửa niết bàn sải cánh
                            bool birdBody = (Mathf.Abs(dy) <= 3f && dx >= -6f && dx <= 10f);
                            bool fireWings = (Mathf.Abs(dy) >= 2f && Mathf.Abs(dy) <= 13f && dx >= -5f - Mathf.Abs(dy) * 0.4f && dx <= 6f + Mathf.Abs(dy) * 0.35f);
                            bool beak = (dx >= 9f && dx <= 14f && Mathf.Abs(dy) <= (14f - dx) * 0.45f);
                            bool featherTails = (dx <= -6f && dx >= -14f && (Mathf.Abs(dy) <= 1.5f || Mathf.Abs(dy - 4f) <= 1.2f || Mathf.Abs(dy + 4f) <= 1.2f));
                            filled = birdBody || fireWings || beak || featherTails;
                            break;
                    }
                    tex.SetPixel(x, y, filled ? white : clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        public Sprite GetMaskSprite(BaseShapeMask shape)
        {
            if (maskSprites.TryGetValue(shape, out var sp) && sp != null) return sp;
            var created = CreateProceduralMask(shape);
            maskSprites[shape] = created;
            return created;
        }

        // ================= 1. BẮN ĐẠN TUYẾN TÍNH / QUỸ ĐẠO (INDICATOR 1) =================
        public GameObject FireProceduralSkill(
            Vector3 spawnPos,
            Vector2 aimDirection,
            BaseShapeMask shape,
            string element,
            TrajectoryType trajectory,
            int damage,
            float speed = 16.0f,
            float explosionRadius = 1.8f,
            bool hasTrail = true,
            bool hasScreenShake = false,
            string shooterId = "Player",
            Vector3? targetStopPosition = null,
            float maxDistance = 14.0f)
        {
            var palette = ElementalPalette.GetPalette(element);
            Sprite maskSp = GetMaskSprite(shape);

            GameObject projObj = new GameObject($"VFX_{shape}_{palette.ElementName}_{trajectory}");
            projObj.transform.position = spawnPos;
            projObj.transform.localScale = Vector3.one * 0.85f; // Chuẩn pixel art sắc nét

            // 1. Tầng Thân Ngoại Vi (Body Color)
            var sr = projObj.AddComponent<SpriteRenderer>();
            sr.material = pixelSpriteMaterial;
            sr.sprite = maskSp;
            sr.color = palette.BodyColor;
            sr.sortingOrder = 22;

            // 2. Tầng Lõi Nóng 2-Tone Bên Trong (Hot Core Color - Thu nhỏ 0.60x như Preview Đấu Trường)
            GameObject coreObj = new GameObject("HotCore");
            coreObj.transform.parent = projObj.transform;
            coreObj.transform.localPosition = Vector3.zero;
            coreObj.transform.localRotation = Quaternion.identity;
            coreObj.transform.localScale = Vector3.one * 0.60f;

            var srCore = coreObj.AddComponent<SpriteRenderer>();
            srCore.material = pixelSpriteMaterial;
            srCore.sprite = maskSp;
            srCore.color = palette.CoreColor;
            srCore.sortingOrder = 23;

            var col = projObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            // 3. Vệt Đuôi Hạt Pixel Sparks Tinh Thể & Trail Gradient
            if (hasTrail)
            {
                var trailComp = projObj.AddComponent<PixelSparkTrailEmitter>();
                trailComp.Palette = palette;

                var trail = projObj.AddComponent<TrailRenderer>();
                trail.time = 0.18f;
                trail.startWidth = 0.30f;
                trail.endWidth = 0.02f;
                trail.sortingOrder = 21;
                trail.material = pixelSpriteMaterial;
                trail.colorGradient = palette.TrailGradient;
            }

            var controller = projObj.AddComponent<ProceduralProjectile>();
            controller.Trajectory = trajectory;
            controller.FlySpeed = speed;
            controller.ExplosionRadius = explosionRadius;
            controller.Damage = damage;
            controller.ShooterId = shooterId;
            controller.Palette = palette;
            controller.HasCameraShake = hasScreenShake;
            controller.TargetStopPosition = targetStopPosition;
            controller.MaxDistance = maxDistance;

            if (shape == BaseShapeMask.Wave)
            {
                controller.IsExpandingWave = true;
                controller.WaveBaseWidth = 0.85f;
                controller.WaveExpandSpeed = 1.8f;
            }

            if (trajectory == TrajectoryType.SineWave)
            {
                controller.WaveAmplitude = 0.75f;
                controller.WaveFrequency = 14.0f;
            }

            controller.Launch(aimDirection);
            return projObj;
        }

        // ================= 2. MƯA KỸ NĂNG VÙNG ĐẤT (INDICATOR 2 - AOE RAIN) =================
        public void SpawnProceduralAoERain(
            Vector3 targetPos,
            string element,
            float radius,
            int damage,
            string shooterId,
            int fallAngle = 0,
            int fallStyle = 0,
            Vector3? casterPos = null)
        {
            var palette = ElementalPalette.GetPalette(element);
            StartCoroutine(AoERainRoutine(targetPos, palette, radius, damage, shooterId, fallAngle, fallStyle, casterPos));
        }

        private IEnumerator AoERainRoutine(
            Vector3 center,
            ElementalPalette palette,
            float radius,
            int totalDamage,
            string shooterId,
            int fallAngle,
            int fallStyle,
            Vector3? casterPos)
        {
            // Vòng tròn ma pháp dưới đất khớp chuẩn với tầm ngắm
            GameObject circleObj = new GameObject($"AoECircle_{palette.ElementName}");
            circleObj.transform.position = center;
            var sr = circleObj.AddComponent<SpriteRenderer>();
            sr.material = pixelSpriteMaterial;
            sr.sprite = Resources.Load<Sprite>("indicator_ground_circle") ?? GetMaskSprite(BaseShapeMask.NovaRing);
            sr.color = new Color(palette.BodyColor.r, palette.BodyColor.g, palette.BodyColor.b, 0.45f);
            sr.sortingOrder = 5;
            circleObj.transform.localScale = Vector3.one * (radius * 1.5f);

            int strikes = 7;
            int dmgPerStrike = Mathf.Max(1, totalDamage / 3);

            // 1. KIỂU RƠI: Carpet Bombing (Đồng loạt oanh tạc cùng một lượt)
            if (fallStyle == 2)
            {
                for (int i = 0; i < strikes; i++)
                {
                    Vector2 rOffset = UnityEngine.Random.insideUnitCircle * (radius * 0.70f);
                    Vector3 impactPoint = center + new Vector3(rOffset.x, rOffset.y, 0);
                    Vector3 skyPoint = CalculateSkyStartPoint(impactPoint, center, fallAngle, casterPos);
                    float fallDist = Vector3.Distance(skyPoint, impactPoint);

                    FireProceduralSkill(skyPoint, (impactPoint - skyPoint).normalized, BaseShapeMask.Meteor, palette.ElementName, TrajectoryType.Linear, dmgPerStrike, 24.0f, 1.2f, true, (i == 0), shooterId, impactPoint, fallDist + 0.1f);
                }
                yield return new WaitForSeconds(0.40f);
                Destroy(circleObj);
                yield break;
            }

            // 2. CÁC KIỂU RƠI TUẦN TỰ (Random Scatter, Spiral Swirl, Sweeping Cascade)
            for (int i = 0; i < strikes; i++)
            {
                Vector2 rOffset = Vector2.zero;
                switch (fallStyle)
                {
                    case 1: // Xoáy ốc đồng tâm (Spiral Swirl)
                        float angle = i * (Mathf.PI * 2f / strikes) * 1.6f;
                        float r = Mathf.Lerp(radius * 0.75f, radius * 0.15f, (float)i / strikes);
                        rOffset = new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
                        break;

                    case 3: // Đợt sóng quét từ trái sang phải (Sweeping Cascade)
                        float t = (float)i / Mathf.Max(1, strikes - 1);
                        float xOff = Mathf.Lerp(-radius * 0.70f, radius * 0.70f, t);
                        float yOff = UnityEngine.Random.Range(-0.30f, 0.30f) * radius;
                        rOffset = new Vector2(xOff, yOff);
                        break;

                    default: // Tản mác ngẫu nhiên (Random Scatter)
                        rOffset = UnityEngine.Random.insideUnitCircle * (radius * 0.70f);
                        break;
                }

                Vector3 impactPoint = center + new Vector3(rOffset.x, rOffset.y, 0);
                Vector3 skyPoint = CalculateSkyStartPoint(impactPoint, center, fallAngle, casterPos);
                float fallDist = Vector3.Distance(skyPoint, impactPoint);

                // Khống chế điểm dừng: TargetStopPosition = impactPoint! Chạm đất nổ tung ngay!
                FireProceduralSkill(skyPoint, (impactPoint - skyPoint).normalized, BaseShapeMask.Meteor, palette.ElementName, TrajectoryType.Linear, dmgPerStrike, 22.0f, 1.2f, true, false, shooterId, impactPoint, fallDist + 0.1f);

                yield return new WaitForSeconds(0.08f);
            }

            yield return new WaitForSeconds(0.35f);
            Destroy(circleObj);
        }

        private Vector3 CalculateSkyStartPoint(Vector3 impactPoint, Vector3 center, int fallAngle, Vector3? casterPos)
        {
            switch (fallAngle)
            {
                case 0: // Rơi Thẳng Đứng 90° (Cắm thẳng từ đỉnh đầu xuống đất)
                    return impactPoint + new Vector3(0, 4.2f, 0);

                case 1: // Rơi Chéo Trái sang Phải (-30°)
                    return impactPoint + new Vector3(-2.2f, 4.0f, 0);

                case 2: // Rơi Chéo Phải sang Trái (+30°)
                    return impactPoint + new Vector3(2.2f, 4.0f, 0);

                case 3: // Rơi theo hướng người chơi đang hướng tới
                    if (casterPos.HasValue)
                    {
                        Vector3 dir = (center - casterPos.Value).normalized;
                        return impactPoint - dir * 2.5f + Vector3.up * 3.8f;
                    }
                    return impactPoint + new Vector3(-1.2f, 4.0f, 0);

                default:
                    return impactPoint + new Vector3(-0.9f, 4.0f, 0);
            }
        }

        // ================= 3. SÓNG QUÉT HÌNH QUẠT 60° (INDICATOR 3 - CONE WAVE) =================
        public void SpawnProceduralConeWave(Vector3 origin, Vector2 direction, string element, float range, int damage, string shooterId)
        {
            var palette = ElementalPalette.GetPalette(element);
            StartCoroutine(ConeWaveRoutine(origin, direction, palette, range, damage, shooterId));
        }

        private IEnumerator ConeWaveRoutine(Vector3 origin, Vector2 direction, ElementalPalette palette, float range, int damage, string shooterId)
        {
            int waveLayers = 3;
            for (int i = 0; i < waveLayers; i++)
            {
                int dmg = (i == 0) ? damage : 0;
                FireProceduralSkill(origin, direction, BaseShapeMask.Wave, palette.ElementName, TrajectoryType.Linear, dmg, 14.0f + i * 2.5f, 1.2f, true, (i == 0), shooterId, null, range);
                yield return new WaitForSeconds(0.08f);
            }
        }

        // ================= 4. BỘC PHÁ NỔ 360° (INDICATOR 4 - NOVA SHOCKWAVE) =================
        public void SpawnProceduralRadialNova(Vector3 origin, string element, float radius, int damage, string shooterId)
        {
            var palette = ElementalPalette.GetPalette(element);

            // 1. Sóng xung kích 360 độ mở rộng từ tâm (Hollow NovaRing)
            GameObject centralRing = new GameObject("NovaShockwave_Center");
            centralRing.transform.position = origin;
            var ringSr = centralRing.AddComponent<SpriteRenderer>();
            ringSr.material = pixelSpriteMaterial;
            ringSr.sprite = GetNovaRingSprite();
            ringSr.color = palette.CoreColor;
            ringSr.sortingOrder = 24;
            StartCoroutine(AnimateRingExpand(centralRing, ringSr, radius, 0.42f));

            // Gây sát thương nổ 360 độ 1 lần chuẩn xác
            Collider2D[] novaHits = Physics2D.OverlapCircleAll(origin, radius);
            foreach (var col in novaHits)
            {
                if (col == null || col.gameObject.name == shooterId) continue;
                var dmg = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                if (dmg != null && !dmg.IsDead)
                {
                    dmg.TakeDamage(damage, shooterId);
                    FloatingDamageManager.Spawn(col.transform.position + Vector3.up * 0.35f, damage, true);
                }
            }

            // 2. 12 tia kiếm khí / mảnh ma pháp tỏa đều 360 độ (bán kính nhỏ nổ nhẹ tinh tế, không sinh 12 quả bom lổm chổm)
            int count = 12;
            for (int i = 0; i < count; i++)
            {
                float ang = (360f / count) * i;
                float rad = ang * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                FireProceduralSkill(origin, dir, BaseShapeMask.Slash, element, TrajectoryType.Linear, 0, 13.0f, 0.35f, true, false, shooterId, null, radius);
            }
        }

        // ================= 5. HÀO QUANG TỰ THÂN (INDICATOR 5 - SELF BUFF) =================
        public void SpawnProceduralSelfAura(Transform target, string element, float duration)
        {
            if (target == null) return;
            var palette = ElementalPalette.GetPalette(element);
            StartCoroutine(SelfAuraRoutine(target, palette, duration));
        }

        private IEnumerator SelfAuraRoutine(Transform target, ElementalPalette palette, float duration)
        {
            GameObject auraObj = new GameObject($"SelfAura_{palette.ElementName}");
            auraObj.transform.parent = target;
            auraObj.transform.localPosition = Vector3.zero;

            var sr = auraObj.AddComponent<SpriteRenderer>();
            sr.material = pixelSpriteMaterial;
            sr.sortingOrder = 18;

            if (animSequences.TryGetValue("vfx_seraph_wings", out var wings) && wings.Length > 0)
            {
                var wingAnim = auraObj.AddComponent<PixelVFXFrameAnimator>();
                wingAnim.Frames = wings;
                wingAnim.FrameRate = 8.0f;
                wingAnim.Loop = true;
                sr.sprite = wings[0];
                sr.color = Color.white;
                auraObj.transform.localPosition = new Vector3(0, 0.45f, 0);
            }
            else
            {
                sr.sprite = GetMaskSprite(BaseShapeMask.NovaRing);
                sr.color = palette.CoreColor;
            }

            float elapsed = 0f;
            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float pulse = 1.0f + Mathf.Sin(Time.time * 6f) * 0.2f;
                auraObj.transform.localScale = Vector3.one * (0.8f * pulse);
                auraObj.transform.Rotate(0, 0, 180f * Time.deltaTime);

                if (UnityEngine.Random.value < 0.3f)
                {
                    SpawnSingleDynamicParticle(target.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.4f, palette);
                }

                yield return null;
            }

            if (auraObj != null) Destroy(auraObj);
        }

        // ================= 6. TRỤ KẾT GIỚI TOÀN ĐỘI (INDICATOR 6 - TEAM BARRIER) =================
        public void SpawnProceduralTeamBarrier(Vector3 center, string element, float radius, float duration)
        {
            var palette = ElementalPalette.GetPalette(element);
            StartCoroutine(TeamBarrierRoutine(center, palette, radius, duration));
        }

        private IEnumerator TeamBarrierRoutine(Vector3 center, ElementalPalette palette, float radius, float duration)
        {
            GameObject domeObj = new GameObject($"TeamBarrier_{palette.ElementName}");
            domeObj.transform.position = center;

            var sr = domeObj.AddComponent<SpriteRenderer>();
            sr.material = pixelSpriteMaterial;
            sr.sprite = Resources.Load<Sprite>("indicator_circle") ?? GetMaskSprite(BaseShapeMask.NovaRing);
            sr.color = new Color(palette.BodyColor.r, palette.BodyColor.g, palette.BodyColor.b, 0.35f);
            sr.sortingOrder = 6;
            domeObj.transform.localScale = Vector3.one * (radius * 2.0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                sr.color = new Color(palette.BodyColor.r, palette.BodyColor.g, palette.BodyColor.b, 0.25f + Mathf.PingPong(Time.time * 2f, 0.15f));
                yield return null;
            }

            if (domeObj != null) Destroy(domeObj);
        }

        // ================= 7. KỸ NĂNG MỌC TRỒI LÊN TỪ DƯỚI ĐẤT (GROUND ERUPTION / EARTH SPROUT) =================
        // Hỗ trợ mọc rễ gai trồi lên, bụi gai băng đâm vút lên, xúc tu bóng tối quẫy trồi, cột lửa phun trào...
        public void SpawnProceduralGroundEruption(Vector3 center, string element, BaseShapeMask shape, float radius, int damage, string shooterId, int clusterCount = 3)
        {
            var palette = ElementalPalette.GetPalette(element);
            StartCoroutine(GroundEruptionRoutine(center, palette, shape, radius, damage, shooterId, clusterCount));
        }

        private IEnumerator GroundEruptionRoutine(Vector3 center, ElementalPalette palette, BaseShapeMask shape, float radius, int damage, string shooterId, int clusterCount)
        {
            // Vòng tròn chấn động dưới mặt đất
            GameObject decal = new GameObject($"EruptionGround_{palette.ElementName}");
            decal.transform.position = center;
            var srDecal = decal.AddComponent<SpriteRenderer>();
            srDecal.material = pixelSpriteMaterial;
            srDecal.sprite = Resources.Load<Sprite>("indicator_circle") ?? GetMaskSprite(BaseShapeMask.NovaRing);
            srDecal.color = new Color(palette.BodyColor.r, palette.BodyColor.g, palette.BodyColor.b, 0.40f);
            srDecal.sortingOrder = 5;
            decal.transform.localScale = Vector3.one * (radius * 1.6f);

            AudioManager.PlayCast();

            // Mọc trồi từ đất lên: các cụm gai/rễ/xúc tu/cột lửa
            List<GameObject> sprouts = new List<GameObject>();
            for (int i = 0; i < clusterCount; i++)
            {
                Vector2 rOff = (i == 0) ? Vector2.zero : (Vector2)UnityEngine.Random.insideUnitCircle * (radius * 0.65f);
                Vector3 sproutPos = center + new Vector3(rOff.x, rOff.y, 0);

                GameObject spObj = new GameObject($"Sprout_{shape}_{i}");
                spObj.transform.position = sproutPos;

                var sr = spObj.AddComponent<SpriteRenderer>();
                sr.material = pixelSpriteMaterial;
                sr.sprite = GetMaskSprite(shape);
                sr.color = palette.BodyColor;
                sr.sortingOrder = 22;

                // Góc nghiêng tự nhiên
                float tilt = (i == 0) ? 0f : UnityEngine.Random.Range(-25f, 25f);
                spObj.transform.rotation = Quaternion.Euler(0, 0, tilt);
                spObj.transform.localScale = new Vector3(1.2f, 0.05f, 1f); // Bắt đầu dẹp sát đất

                sprouts.Add(spObj);
            }

            // Giai đoạn 1: Vọt trồi lên từ lòng đất (Eruption surge: 0.12s)
            float surgeTime = 0.14f;
            float elapsed = 0f;
            while (elapsed < surgeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / surgeTime;
                float scaleY = Mathf.Lerp(0.05f, 1.45f, t);
                foreach (var sp in sprouts)
                {
                    if (sp != null) sp.transform.localScale = new Vector3(1.25f, scaleY, 1f);
                }
                yield return null;
            }

            // Gây sát thương nổ bộc phá diện rộng tại thời điểm trồi đỉnh điểm
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var col in hits)
            {
                if (col.gameObject.name == shooterId) continue;
                if (col.TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(damage, shooterId);
                    FloatingDamageManager.Spawn(col.transform.position, damage, true);
                }
            }
            SpawnPixelImpactBurst(center, palette, radius);
            AudioManager.PlayExplosion();

            // Giai đoạn 2: Quẫy lắc / rung rinh duy trì trên mặt đất (0.35s)
            float holdTime = 0.38f;
            elapsed = 0f;
            while (elapsed < holdTime)
            {
                elapsed += Time.deltaTime;
                float wobble = Mathf.Sin(Time.time * 24f) * 0.10f;
                foreach (var sp in sprouts)
                {
                    if (sp != null) sp.transform.localScale = new Vector3(1.25f + wobble, 1.45f - wobble, 1f);
                }
                yield return null;
            }

            // Giai đoạn 3: Mờ dần và thụt lùi xuống lòng đất (Fade out: 0.20s)
            float fadeTime = 0.20f;
            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                foreach (var sp in sprouts)
                {
                    if (sp != null)
                    {
                        var sr = sp.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = new Color(palette.BodyColor.r, palette.BodyColor.g, palette.BodyColor.b, 1f - t);
                        sp.transform.localScale = new Vector3(1.25f * (1f - t), 1.45f * (1f - t), 1f);
                    }
                }
                if (srDecal != null) srDecal.color = new Color(palette.BodyColor.r, palette.BodyColor.g, palette.BodyColor.b, 0.40f * (1f - t));
                yield return null;
            }

            foreach (var sp in sprouts) if (sp != null) Destroy(sp);
            if (decal != null) Destroy(decal);
        }

        // ================= HẠT ĐỘNG THEO NGUYÊN TỐ =================
        public void SpawnSingleDynamicParticle(Vector3 pos, ElementalPalette palette)
        {
            GameObject p = new GameObject("DynamicParticle");
            p.transform.position = pos;

            var sr = p.AddComponent<SpriteRenderer>();
            sr.material = pixelSpriteMaterial;
            sr.sprite = GetMaskSprite(BaseShapeMask.Orb);
            sr.color = palette.ParticleColor;
            sr.sortingOrder = 23;
            p.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.12f, 0.24f);

            Vector2 vel = Vector2.zero;
            switch (palette.ElementName)
            {
                case "Hỏa":
                    // Lửa bốc lên trên kèm gió tản
                    vel = new Vector2(UnityEngine.Random.Range(-0.8f, 0.8f), UnityEngine.Random.Range(1.5f, 3.2f));
                    break;
                case "Băng":
                    // Mảnh băng trôi chậm rơi xuống
                    vel = new Vector2(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(-0.4f, -1.2f));
                    break;
                case "Lôi":
                    // Tia điện giật ngẫu nhiên tốc độ cao
                    vel = UnityEngine.Random.insideUnitCircle * 4.5f;
                    break;
                case "Phong":
                    // Xoáy hình tròn
                    vel = new Vector2(UnityEngine.Random.Range(1.2f, 2.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
                    break;
                case "Mộc":
                    // Hoa lá bồng bềnh
                    vel = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0.2f, 0.8f));
                    break;
                case "Quang":
                    // Tia hào quang tỏa ra
                    vel = UnityEngine.Random.insideUnitCircle * 1.8f;
                    break;
                case "Ám":
                    // Khói đen xoáy cuộn
                    vel = new Vector2(UnityEngine.Random.Range(-0.4f, 0.4f), UnityEngine.Random.Range(-0.8f, 0.8f));
                    break;
            }

            StartCoroutine(AnimateSingleParticle(p, sr, vel, 0.35f));
        }

        private IEnumerator AnimateSingleParticle(GameObject p, SpriteRenderer sr, Vector2 velocity, float duration)
        {
            float elapsed = 0f;
            Color initCol = sr.color;
            Vector3 cur = p.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                cur += (Vector3)(velocity * Time.deltaTime);
                p.transform.position = cur;
                sr.color = new Color(initCol.r, initCol.g, initCol.b, Mathf.Lerp(initCol.a, 0f, t));
                yield return null;
            }

            Destroy(p);
        }

        public void SpawnPixelImpactBurst(Vector3 center, ElementalPalette palette, float radius)
        {
            // Tối ưu hóa hiệu năng & Draw Calls: Bỏ qua sinh hiệu ứng nổ nếu nằm ngoài camera / xa hơn 28m
            if (Camera.main != null)
            {
                Vector3 camPos = Camera.main.transform.position;
                camPos.z = center.z;
                if ((camPos - center).sqrMagnitude > 28f * 28f) return;
            }

            // 1. Chớp sáng tâm va chạm (Gọn gàng 0.55m, không làm nổ bung bắp cải 10 mét)
            GameObject flash = new GameObject("ImpactFlash");
            flash.transform.position = center;
            var flashSr = flash.AddComponent<SpriteRenderer>();
            flashSr.material = pixelSpriteMaterial;
            flashSr.sprite = GetSparkSprite();
            flashSr.color = palette.CoreColor;
            flashSr.sortingOrder = 28;
            flash.transform.localScale = Vector3.one * 0.55f;
            StartCoroutine(AnimateFlashFade(flash, flashSr, 0.12f));

            // 2. Chùm hạt bộc phá pixel sparks (18 hạt nổ 360 độ với ma sát suy giảm tự nhiên)
            int particleCount = 18;
            for (int i = 0; i < particleCount; i++)
            {
                float angleDeg = (360f / particleCount) * i + UnityEngine.Random.Range(-10f, 10f);
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                float speed = (i % 2 == 0) ? UnityEngine.Random.Range(4.5f, 7.5f) : UnityEngine.Random.Range(2.2f, 4.0f);
                Color pCol = (i % 3 == 0) ? palette.CoreColor : (i % 3 == 1) ? palette.BodyColor : palette.ParticleColor;

                Vector2 extraVel = Vector2.zero;
                if (palette.ElementName == "Hỏa") extraVel = new Vector2(0, UnityEngine.Random.Range(1.2f, 2.5f));
                else if (palette.ElementName == "Lôi") extraVel = UnityEngine.Random.insideUnitCircle * 2.0f;

                SpawnSingleSparkParticle(
                    center,
                    (dir * speed) + extraVel,
                    pCol,
                    UnityEngine.Random.Range(0.16f, 0.26f),
                    UnityEngine.Random.Range(0.28f, 0.45f)
                );
            }

            // 3. Vòng sóng xung kích giãn nở rỗng (Crisp Hollow NovaRing)
            GameObject ring = new GameObject("ImpactRing");
            ring.transform.position = center;
            var ringSr = ring.AddComponent<SpriteRenderer>();
            ringSr.material = pixelSpriteMaterial;
            ringSr.sprite = GetNovaRingSprite();
            ringSr.color = palette.CoreColor;
            ringSr.sortingOrder = 25;
            StartCoroutine(AnimateRingExpand(ring, ringSr, Mathf.Max(0.75f, radius), 0.24f));
        }

        public void SpawnSingleSparkParticle(Vector3 pos, Vector2 vel, Color color, float size, float duration)
        {
            GameObject sp = new GameObject("SparkParticle");
            sp.transform.position = pos;

            var sr = sp.AddComponent<SpriteRenderer>();
            sr.material = pixelSpriteMaterial;
            sr.sprite = GetSparkSprite();
            sr.color = color;
            sr.sortingOrder = 26;
            sp.transform.localScale = Vector3.one * size;

            StartCoroutine(AnimateSparkParticle(sp, sr, vel, duration));
        }

        private IEnumerator AnimateSparkParticle(GameObject p, SpriteRenderer sr, Vector2 velocity, float duration)
        {
            float elapsed = 0f;
            Color initCol = sr.color;
            Vector3 cur = p.transform.position;
            Vector2 curVel = velocity;

            while (elapsed < duration && p != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                curVel *= Mathf.Exp(-Time.deltaTime * 4.5f);
                cur += (Vector3)(curVel * Time.deltaTime);
                p.transform.position = cur;
                sr.color = new Color(initCol.r, initCol.g, initCol.b, Mathf.Lerp(initCol.a, 0f, t));
                yield return null;
            }

            if (p != null) Destroy(p);
        }

        private IEnumerator AnimateFlashFade(GameObject flash, SpriteRenderer sr, float duration)
        {
            float elapsed = 0f;
            Color initCol = sr.color;
            while (elapsed < duration && flash != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                sr.color = new Color(initCol.r, initCol.g, initCol.b, Mathf.Lerp(1.0f, 0f, t));
                yield return null;
            }
            if (flash != null) Destroy(flash);
        }

        private IEnumerator AnimateRingExpand(GameObject ring, SpriteRenderer sr, float maxRadius, float duration)
        {
            float elapsed = 0f;
            Color initCol = sr.color;

            while (elapsed < duration && ring != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float sc = Mathf.Lerp(0.2f, maxRadius * 1.4f, Mathf.Sqrt(t));
                ring.transform.localScale = new Vector3(sc, sc, 1f);
                sr.color = new Color(initCol.r, initCol.g, initCol.b, Mathf.Lerp(initCol.a, 0f, t * t));
                yield return null;
            }

            if (ring != null) Destroy(ring);
        }
    }
}
