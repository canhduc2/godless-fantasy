using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Combat;
using GodlessFantasy.Player;
using GodlessFantasy.Core;

namespace GodlessFantasy.UI
{
    /// <summary>
    /// Bảng Điều Khiển Testing Toàn Bộ 140 Kỹ Năng & VFX Đa Lớp (Full Matrix Testing Catalog & Live Arena):
    /// - Cột Trái (Widescreen): Bộ lọc 3 hàng (Phái, Hệ, Phẩm) + Danh sách kỹ năng chọn lựa mượt mà.
    /// - Cột Phải (Live Simulation Arena):
    ///   + Khung đấu trường trực tiếp: Người chơi (Hero) ở trên, Quái vật (Target Dummy) ở dưới.
    ///   + Mô phỏng đạn bay và hiệu ứng bộc phá thời gian thực theo đúng phôi khối, màu hệ, quỹ đạo và nhảy sát thương!
    ///   + Nút [⚡ BẮN THỬ (CAST)] và Checkbox [🔄 TỰ ĐỘNG BẮN LẶP (LOOP)].
    ///   + THANH GẠT TỐC ĐỘ HIỆU ỨNG (SLOW-MOTION SLIDER: 0.1x -> 2.0x) kèm các nút preset [0.2x Soi Chậm], [0.5x], [1.0x Chuẩn]
    ///     cho phép tester soi từng frame chuyển động, phôi hạt và đường lượn xem có đẹp chuẩn xác không!
    ///   + Hộp tóm tắt thông số kỹ năng đang xem (Tầm xa, Bán kính nổ, Hồi chiêu, Mana, ST, Phôi khối).
    ///   + Nút [🔥 BẮN THỬ RA GAME THẬT] và [⚡ GÁN VÀO PHÍM [1]].
    /// - Phím tắt [F3] để Bật/Tắt tức thì trong Game.
    /// </summary>
    public class SkillTestingMatrixModal : MonoBehaviour
    {
        public static SkillTestingMatrixModal Instance { get; private set; }

        public bool IsOpen = false;

        private List<SkillScrollData> allSkills140 = new List<SkillScrollData>();
        private List<SkillScrollData> filteredSkills = new List<SkillScrollData>();
        private SkillScrollData previewSkill = null;

        // Bộ lọc 3 hàng
        private int filterClassIdx = 0; // 0=Tất cả, 1=Kị Sĩ, 2=Pháp Sư, 3=Võ Sĩ, 4=Xạ Thủ
        private readonly string[] classFilterNames = { "TẤT CẢ PHÁI (140)", "⚔️ KỊ SĨ (35)", "🔮 PHÁP SƯ (35)", "🥊 VÕ SĨ (35)", "🏹 XẠ THỦ (35)" };

        private int filterElemIdx = 0; // 0=Tất cả, 1..7
        private readonly string[] elemFilterNames = { "TẤT CẢ (7 HỆ)", "🔥 HỎA", "❄️ BĂNG", "⚡ LÔI", "✨ QUANG", "🔮 ÁM", "🌪️ PHONG", "🌿 MỘC" };
        private readonly string[] rawElemNames = { "", "Hỏa", "Băng", "Lôi", "Quang", "Ám", "Phong", "Mộc" };

        private int filterTierIdx = 0; // 0=Tất cả, 1..5
        private readonly string[] tierFilterNames = { "TẤT CẢ BẬC", "Phẩm C", "Phẩm B", "Phẩm A", "Phẩm S", "Phẩm X" };
        private readonly string[] rawTierNames = { "", "Phẩm C", "Phẩm B", "Phẩm A", "Phẩm S", "Phẩm X" };

        private Vector2 scrollPos;
        private string statusNotification = "";
        private float notifTimer = 0f;

        // Live Simulation Arena States
        private bool isAutoLoopCast = true;
        private float autoLoopTimer = 0f;
        private float dummyHitFlash = 0f;
        private float dummyShakeX = 0f;
        private float dummyHp = 1.0f;

        private class SimParticle
        {
            public Vector2 pos;
            public Vector2 vel;
            public Color color;
            public float life;
            public float maxLife;
            public float size;
            public bool isSpark;
        }

        private class SimDamageText
        {
            public Vector2 pos;
            public string text;
            public Color color;
            public float life;
            public float maxLife;
            public float floatY;
        }

        private class SimSubProjectile
        {
            public Vector2 startPos;
            public Vector2 endPos;
            public Vector2 curPos;
            public Vector2 lastPos;
            public float progress;
            public float speed;
            public float delay;
            public bool hasLaunched;
            public bool hasHit;
            public float angleDeg;
            public float size;
            public int shape;
            public int trajectory;
            public List<Vector2> trail = new List<Vector2>();
            public Color coreCol;
            public Color bodyCol;
            public Color rimCol;
            public int damageToDeal;
            public bool isCritical;
            public float waveWidth;
            public float waveExpandSpeed;
        }

        private class ActiveSimCast
        {
            public SkillScrollData skill;
            public int indicatorType;
            public List<SimSubProjectile> projectiles = new List<SimSubProjectile>();

            public bool hasAoEDecal;
            public Vector2 aoeCenter;
            public float aoeRadius;
            public float aoeRotation;
            public float aoeAlpha;
            public float aoeDuration;
            public Color aoeColor;

            public bool hasShockwave;
            public Vector2 shockwaveCenter;
            public float shockwaveRadius;
            public float shockwaveMaxRadius;
            public float shockwaveAlpha;
            public Color shockwaveColor;

            public bool hasBuffAura;
            public float buffTimer;
            public float buffDuration;
            public Color buffColor;

            public float elapsedTime;
            public bool isFinished;
        }

        private List<ActiveSimCast> activeCasts = new List<ActiveSimCast>();
        private List<SimParticle> activeParticles = new List<SimParticle>();
        private List<SimDamageText> activeDamageTexts = new List<SimDamageText>();

        // Texture cache
        private Dictionary<string, Texture2D> texCache = new Dictionary<string, Texture2D>();
        private Dictionary<BaseShapeMask, Texture2D> shapeTextures = new Dictionary<BaseShapeMask, Texture2D>();
        private Texture2D texAoECircle;
        private Texture2D texSpark;
        private Texture2D texWing;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadCatalog();
        }

        private void Start()
        {
            LoadCatalog();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                IsOpen = !IsOpen;
                if (IsOpen && (allSkills140 == null || allSkills140.Count == 0))
                {
                    LoadCatalog();
                }
            }

            if (!IsOpen) return;

            if (notifTimer > 0f)
            {
                notifTimer -= Time.unscaledDeltaTime;
                if (notifTimer <= 0f) statusNotification = "";
            }

            // Tốc độ giả lập tuân theo thanh gạt SimulationSpeed
            float dt = Time.unscaledDeltaTime * Mathf.Clamp(GameSettings.SimulationSpeed, 0.1f, 2.0f);

            // Auto loop bắn thử
            if (isAutoLoopCast && previewSkill != null)
            {
                autoLoopTimer += dt;
                if (autoLoopTimer >= 1.6f)
                {
                    autoLoopTimer = 0f;
                    TriggerSimCast(previewSkill, false);
                }
            }

            // Hồi phục thanh máu Dummy mượt mà
            dummyHp = Mathf.MoveTowards(dummyHp, 1.0f, dt * 0.45f);

            // Cập nhật các phép đang diễn ra trong Arena
            for (int c = activeCasts.Count - 1; c >= 0; c--)
            {
                var cast = activeCasts[c];
                cast.elapsedTime += dt;

                if (cast.hasAoEDecal)
                {
                    cast.aoeRotation += dt * 50f;
                    if (cast.elapsedTime > cast.aoeDuration - 0.4f)
                    {
                        cast.aoeAlpha = Mathf.MoveTowards(cast.aoeAlpha, 0f, dt * 2.5f);
                    }
                }

                if (cast.hasShockwave)
                {
                    float tWave = Mathf.Clamp01(cast.elapsedTime / 0.55f);
                    cast.shockwaveRadius = Mathf.Lerp(10f, cast.shockwaveMaxRadius, tWave);
                    cast.shockwaveAlpha = Mathf.Lerp(1.0f, 0f, tWave);
                }

                if (cast.hasBuffAura)
                {
                    cast.buffTimer += dt;
                }

                bool allProjectilesFinished = true;
                for (int i = 0; i < cast.projectiles.Count; i++)
                {
                    var p = cast.projectiles[i];
                    if (!p.hasLaunched)
                    {
                        allProjectilesFinished = false;
                        p.delay -= dt;
                        if (p.delay <= 0f)
                        {
                            p.hasLaunched = true;
                        }
                    }
                    else if (!p.hasHit)
                    {
                        allProjectilesFinished = false;
                        p.lastPos = p.curPos;
                        p.progress += dt * p.speed;

                        if (p.trajectory == 1) // SineWave
                        {
                            float t = p.progress;
                            Vector2 basePos = Vector2.Lerp(p.startPos, p.endPos, t);
                            float perpOffset = Mathf.Sin(t * Mathf.PI * 6f) * 32f;
                            p.curPos = basePos + new Vector2(perpOffset, 0f);

                            Vector2 diff = p.curPos - p.lastPos;
                            if (diff.sqrMagnitude > 0.01f)
                            {
                                p.angleDeg = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                            }
                        }
                        else
                        {
                            p.curPos = Vector2.Lerp(p.startPos, p.endPos, p.progress);
                        }

                        // Lưu vết đuôi
                        p.trail.Add(p.curPos);
                        if (p.trail.Count > 7) p.trail.RemoveAt(0);

                        // Chạm đích
                        if (p.progress >= 1.0f)
                        {
                            p.progress = 1.0f;
                            p.hasHit = true;
                            p.curPos = p.endPos;

                            // Nổ bộc phá & Tác động lên Dummy
                            if (p.damageToDeal > 0)
                            {
                                dummyHitFlash = 0.32f;
                                dummyShakeX = 8f;
                                dummyHp = Mathf.Max(0.12f, dummyHp - 0.28f);

                                string critStr = p.isCritical ? " CRIT!" : "";
                                SpawnDamageText(p.endPos + Vector2.up * -10f, $"💥 -{p.damageToDeal}{critStr}", p.isCritical ? new Color(1f, 0.85f, 0.2f) : Color.white);
                                SpawnExplosionParticles(p.endPos, p.bodyCol, (cast.skill != null && cast.skill.tierRank == "Phẩm X") ? 26 : 16);
                                AudioManager.PlayExplosion();
                            }
                        }
                    }
                }

                bool decalEnded = !cast.hasAoEDecal || cast.aoeAlpha <= 0.02f;
                bool shockwaveEnded = !cast.hasShockwave || cast.shockwaveAlpha <= 0.02f;
                bool buffEnded = !cast.hasBuffAura || cast.buffTimer >= cast.buffDuration;

                if (allProjectilesFinished && decalEnded && shockwaveEnded && buffEnded)
                {
                    activeCasts.RemoveAt(c);
                }
            }

            // Dummy rung giật khi trúng đòn
            if (dummyHitFlash > 0f)
            {
                dummyHitFlash -= dt;
                dummyShakeX = Mathf.Sin(Time.realtimeSinceStartup * 45f) * (dummyHitFlash * 22f);
            }
            else
            {
                dummyShakeX = 0f;
            }

            // Cập nhật hạt bộc phá
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                var pt = activeParticles[i];
                pt.pos += pt.vel * dt;
                pt.vel *= Mathf.Exp(-dt * 3.5f);
                pt.life -= dt;
                if (pt.life <= 0f)
                {
                    activeParticles.RemoveAt(i);
                }
            }

            // Cập nhật chữ số sát thương bay lên
            for (int i = activeDamageTexts.Count - 1; i >= 0; i--)
            {
                var dtItem = activeDamageTexts[i];
                dtItem.life -= dt;
                dtItem.floatY += dt * 38f;
                if (dtItem.life <= 0f)
                {
                    activeDamageTexts.RemoveAt(i);
                }
            }
        }

        public void LoadCatalog()
        {
            allSkills140 = SkillDatabase.GetFull140SkillMatrix();
            ApplyFilter();
            if (previewSkill == null && allSkills140.Count > 0)
            {
                previewSkill = allSkills140[0];
            }
        }

        private void ApplyFilter()
        {
            filteredSkills.Clear();
            string selectedClass = (filterClassIdx == 1) ? "Kị Sĩ" :
                                   (filterClassIdx == 2) ? "Pháp Sư" :
                                   (filterClassIdx == 3) ? "Võ Sĩ" :
                                   (filterClassIdx == 4) ? "Xạ Thủ" : "";

            string selectedElem = (filterElemIdx > 0 && filterElemIdx < rawElemNames.Length) ? rawElemNames[filterElemIdx] : "";
            string selectedTier = (filterTierIdx > 0 && filterTierIdx < rawTierNames.Length) ? rawTierNames[filterTierIdx] : "";

            foreach (var s in allSkills140)
            {
                if (!string.IsNullOrEmpty(selectedClass) && s.requiredClass != selectedClass) continue;
                if (!string.IsNullOrEmpty(selectedElem) && s.element != selectedElem) continue;
                if (!string.IsNullOrEmpty(selectedTier) && s.tierRank != selectedTier) continue;
                filteredSkills.Add(s);
            }

            if (filteredSkills.Count > 0 && (previewSkill == null || !filteredSkills.Contains(previewSkill)))
            {
                previewSkill = filteredSkills[0];
            }
        }

        private Texture2D GetTexture(string name)
        {
            if (string.IsNullOrEmpty(name)) return Texture2D.whiteTexture;
            if (!texCache.TryGetValue(name, out var t) || t == null)
            {
                t = Resources.Load<Texture2D>(name);
                if (t == null) t = Texture2D.whiteTexture;
                texCache[name] = t;
            }
            return t;
        }

        private Texture2D GetMaskTexture(BaseShapeMask shape)
        {
            if (shapeTextures.TryGetValue(shape, out var t) && t != null) return t;
            var created = CreateMaskTexture(shape);
            shapeTextures[shape] = created;
            return created;
        }

        private Texture2D CreateMaskTexture(BaseShapeMask shape)
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
                            // Crescent moon blade curving towards +X
                            float r = Mathf.Sqrt(dx * dx + dy * dy);
                            filled = (r >= 5f && r <= 13f && dx >= -3f && Mathf.Abs(dy) <= 12f);
                            break;
                        case BaseShapeMask.Arrow:
                            // Aerodynamic arrowhead pointing towards +X
                            filled = (Mathf.Abs(dy) <= (12f - dx) * 0.65f && dx >= -10f && dx <= 11f);
                            break;
                        case BaseShapeMask.Wave:
                            // Sweeping crescent shockwave arc curved towards +X
                            float arcX = 8f - (dy * dy) * 0.08f;
                            filled = (dx >= arcX - 4f && dx <= arcX && Mathf.Abs(dy) <= 12f);
                            break;
                        case BaseShapeMask.Meteor:
                            // Asteroid rock with blazing tail trailing to -X
                            filled = ((dx * dx + dy * dy) <= (8f * 8f)) || (dx <= 0 && Mathf.Abs(dy) <= (5f + dx * 0.4f) && dx >= -12f);
                            break;
                        case BaseShapeMask.NovaRing:
                            float rNova = Mathf.Sqrt(dx * dx + dy * dy);
                            filled = (rNova >= 6f && rNova <= 12f);
                            break;
                        case BaseShapeMask.Pillar:
                            filled = (Mathf.Abs(dy) <= 4f && Mathf.Abs(dx) <= 14f);
                            break;
                        case BaseShapeMask.Beam:
                            filled = (Mathf.Abs(dy) <= 3f && Mathf.Abs(dx) <= 15f);
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
                            // Kim cang hộ thuẫn bảo giáp
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
            return tex;
        }

        public static string GetShapeDisplayName(int mask)
        {
            switch (mask)
            {
                case 0: return "Ma Cầu (Orb)";
                case 1: return "Kiếm Khí (Slash)";
                case 2: return "Đại Tiễn (Arrow)";
                case 3: return "Sóng Quạt (Wave)";
                case 4: return "Thiên Thạch (Meteor)";
                case 5: return "Nova 360°";
                case 6: return "Cột Sáng (Pillar)";
                case 7: return "Tia Pháo (Beam)";
                case 8: return "Xúc Tu (Tentacle)";
                case 9: return "Rễ Gai Dây Leo (VineSprout)";
                case 10: return "Bụi Gai Băng (IceSpikes)";
                case 11: return "Hoa Ma Pháp (FloraBloom)";
                case 12: return "Cột Lửa Phun (FlameGeyser)";
                case 13: return "Sấm Sét Giáng (ThunderStrike)";
                case 14: return "Thánh Giá (HolyCross)";
                case 15: return "Cơn Lốc Xoáy (TornadoWhirl)";
                case 16: return "Đầu Lâu Tử Linh (Skull)";
                case 17: return "Hỏa Long Thần Thú (DragonHead)";
                case 18: return "Phi Đao Song Nhận (Dagger)";
                case 19: return "Kim Cang Hộ Thuẫn (Shield)";
                case 20: return "Huyết Châu Gai (BloodDrop)";
                case 21: return "Hố Đen Hư Không (VoidVortex)";
                case 22: return "Chiến Chùy Lôi Đình (GiantHammer)";
                case 23: return "Phượng Hoàng Lửa (Phoenix)";
                default: return $"Phôi Khối #{mask}";
            }
        }

        private Texture2D GetAoECircleTexture()
        {
            if (texAoECircle != null) return texAoECircle;
            int size = 64;
            texAoECircle = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texAoECircle.filterMode = FilterMode.Point;
            float center = (size - 1) / 2f;
            Color clear = Color.clear;
            Color white = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    bool filled = false;

                    if (r >= 26f && r <= 29f)
                    {
                        float aNorm = (angle + Mathf.PI) / (Mathf.PI * 2f) * 16f;
                        if ((aNorm % 1f) < 0.70f) filled = true;
                    }
                    else if (r >= 18f && r <= 20f)
                    {
                        filled = true;
                    }
                    else if (r <= 10f)
                    {
                        if (Mathf.Abs(dx) <= 1.5f || Mathf.Abs(dy) <= 1.5f) filled = true;
                        if (Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dy)) <= 1.2f && r <= 8f) filled = true;
                    }

                    texAoECircle.SetPixel(x, y, filled ? white : clear);
                }
            }
            texAoECircle.Apply();
            return texAoECircle;
        }

        private Texture2D GetSparkTexture()
        {
            if (texSpark != null) return texSpark;
            int size = 16;
            texSpark = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texSpark.filterMode = FilterMode.Point;
            float c = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - c);
                    float dy = Mathf.Abs(y - c);
                    bool filled = (dx + dy <= 5.5f) || (dx <= 1f && dy <= 7f) || (dy <= 1f && dx <= 7f);
                    texSpark.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            texSpark.Apply();
            return texSpark;
        }

        private Texture2D GetWingTexture()
        {
            if (texWing != null) return texWing;
            int size = 32;
            texWing = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texWing.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = x / 31f;
                    float ny = y / 31f;
                    float spine = Mathf.Sin(nx * Mathf.PI * 0.8f) * 0.6f + 0.2f;
                    bool filled = (ny >= spine - 0.22f && ny <= spine + 0.22f && nx >= 0.1f && nx <= 0.95f);
                    texWing.SetPixel(x, y, filled ? Color.white : Color.clear);
                }
            }
            texWing.Apply();
            return texWing;
        }

        private Vector2 GetArenaHeroPos()
        {
            float w = Mathf.Clamp(Screen.width * 0.95f, 1020f, 1220f);
            float h = Mathf.Clamp(Screen.height * 0.94f, 600f, 720f);
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            float colLeftW = w * 0.58f;
            float colRightX = x + 16f + colLeftW;
            float colRightW = w - colLeftW - 28f;
            float arenaY = y + 40f + 26f;
            return new Vector2(colRightX + colRightW / 2f, arenaY + 32f);
        }

        private Vector2 GetArenaDummyPos()
        {
            float w = Mathf.Clamp(Screen.width * 0.95f, 1020f, 1220f);
            float h = Mathf.Clamp(Screen.height * 0.94f, 600f, 720f);
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            float colLeftW = w * 0.58f;
            float colRightX = x + 16f + colLeftW;
            float colRightW = w - colLeftW - 28f;
            float arenaY = y + 40f + 26f;
            float arenaH = 240f;
            return new Vector2(colRightX + colRightW / 2f, arenaY + arenaH - 42f);
        }

        private void TriggerSimCast(SkillScrollData s, bool playAudio = true)
        {
            if (s == null) return;
            Vector2 heroPos = GetArenaHeroPos();
            Vector2 dummyPos = GetArenaDummyPos();
            var palette = ElementalPalette.GetPalette(s.element);

            var cast = new ActiveSimCast
            {
                skill = s,
                indicatorType = s.indicatorType,
                elapsedTime = 0f,
                isFinished = false
            };

            int ind = s.indicatorType;
            if (ind == 1) // 1. BẮN ĐẠN TUYẾN TÍNH / QUỸ ĐẠO
            {
                float spd = Mathf.Clamp(s.flySpeed * 0.10f, 1.0f, 2.8f);
                var proj = new SimSubProjectile
                {
                    startPos = heroPos + Vector2.up * 8f,
                    endPos = dummyPos,
                    curPos = heroPos + Vector2.up * 8f,
                    lastPos = heroPos + Vector2.up * 8f,
                    progress = 0f,
                    speed = spd,
                    delay = 0f,
                    hasLaunched = true,
                    hasHit = false,
                    angleDeg = 90f,
                    size = (s.tierRank == "Phẩm X") ? 38f : (s.tierRank == "Phẩm S") ? 32f : 26f,
                    shape = s.baseShapeMask,
                    trajectory = s.trajectoryType,
                    coreCol = palette.CoreColor,
                    bodyCol = palette.BodyColor,
                    rimCol = palette.RimColor,
                    damageToDeal = s.baseDamage,
                    isCritical = (s.tierRank == "Phẩm S" || s.tierRank == "Phẩm X")
                };
                cast.projectiles.Add(proj);
            }
            else if (ind == 2) // 2. MƯA KỸ NĂNG / THIÊN THẠCH / CỰ KIẾM TỪ TRỜI RƠI XUỐNG
            {
                cast.hasAoEDecal = true;
                cast.aoeCenter = dummyPos;
                cast.aoeRadius = Mathf.Clamp(s.explosionRadius * 22f, 42f, 72f);
                cast.aoeAlpha = 0.85f;
                cast.aoeDuration = 1.3f;
                cast.aoeColor = palette.BodyColor;

                bool isGroundSprout = (s.baseShapeMask == 8 || s.baseShapeMask == 9 || s.baseShapeMask == 10 || s.baseShapeMask == 11 || s.baseShapeMask == 12 || s.baseShapeMask == 20);
                if (isGroundSprout)
                {
                    // 4 cụm gai/rễ/xúc tu/cột lửa trồi vọt từ lòng đất lên đâm xiên vào mục tiêu!
                    int sproutCount = 4;
                    int dmgPerSprout = Mathf.Max(1, s.baseDamage / 2);
                    for (int i = 0; i < sproutCount; i++)
                    {
                        float ang = i * (Mathf.PI * 2f / sproutCount) + UnityEngine.Random.Range(-0.25f, 0.25f);
                        float dist = (i == 0) ? 0f : UnityEngine.Random.Range(8f, cast.aoeRadius * 0.55f);
                        Vector2 targetPoint = dummyPos + new Vector2(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist * 0.45f);
                        Vector2 underGroundStart = targetPoint + Vector2.up * 16f; // bắt đầu lặn dưới đất trồi lên

                        var sprout = new SimSubProjectile
                        {
                            startPos = underGroundStart,
                            endPos = targetPoint,
                            curPos = underGroundStart,
                            lastPos = underGroundStart,
                            progress = 0f,
                            speed = 2.8f, // vọt lên chớp nhoáng
                            delay = i * 0.08f,
                            hasLaunched = false,
                            hasHit = false,
                            angleDeg = -90f + UnityEngine.Random.Range(-15f, 15f), // hướng thẳng đứng lên trời!
                            size = (s.tierRank == "Phẩm X") ? 38f : 30f,
                            shape = s.baseShapeMask,
                            trajectory = 0,
                            coreCol = palette.CoreColor,
                            bodyCol = palette.BodyColor,
                            rimCol = palette.RimColor,
                            damageToDeal = dmgPerSprout,
                            isCritical = (i == 0)
                        };
                        cast.projectiles.Add(sprout);
                    }
                }
                else
                {
                    int strikes = 6;
                    int dmgPerStrike = Mathf.Max(1, s.baseDamage / 3);

                    // Góc rơi fallAngle: 0=90° Thẳng đứng, 1=-30° Chéo trái, 2=+30° Chéo phải
                    float xOffset = (s.rainFallAngle == 1) ? -42f : (s.rainFallAngle == 2) ? 42f : 0f;

                    for (int i = 0; i < strikes; i++)
                    {
                        float delay = (s.rainFallStyle == 2) ? (i * 0.035f) : (i * 0.12f); // 2 = Carpet bombing oanh tạc đồng loạt
                        
                        // Điểm chạm đất ngẫu nhiên trong vòng tròn AoE Decal
                        float landAng = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                        float landDist = UnityEngine.Random.Range(0f, cast.aoeRadius * 0.75f);
                        Vector2 targetPoint = dummyPos + new Vector2(Mathf.Cos(landAng) * landDist, Mathf.Sin(landAng) * landDist * 0.55f);

                        // Điểm xuất phát từ bầu trời cao
                        Vector2 skyStart = new Vector2(targetPoint.x + xOffset, heroPos.y - 18f);

                        Vector2 dir = (targetPoint - skyStart).normalized;
                        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                        var meteor = new SimSubProjectile
                        {
                            startPos = skyStart,
                            endPos = targetPoint,
                            curPos = skyStart,
                            lastPos = skyStart,
                            progress = 0f,
                            speed = 2.4f,
                            delay = delay,
                            hasLaunched = false,
                            hasHit = false,
                            angleDeg = angleDeg,
                            size = (s.tierRank == "Phẩm X") ? 34f : 26f,
                            shape = (s.baseShapeMask == 1) ? 1 : 4, // 1 = Cự kiếm cắm đất, 4 = Thiên thạch
                            trajectory = 0,
                            coreCol = palette.CoreColor,
                            bodyCol = palette.BodyColor,
                            rimCol = palette.RimColor,
                            damageToDeal = dmgPerStrike,
                            isCritical = (i == 0 || i == strikes - 1)
                        };
                        cast.projectiles.Add(meteor);
                    }
                }
            }
            else if (ind == 3) // 3. SÓNG QUẠT QUÉT 60° (CONE WAVE SWEEP)
            {
                int waveLayers = 3;
                for (int w = 0; w < waveLayers; w++)
                {
                    float delay = w * 0.08f;
                    var wave = new SimSubProjectile
                    {
                        startPos = heroPos + Vector2.up * 12f,
                        endPos = dummyPos,
                        curPos = heroPos + Vector2.up * 12f,
                        lastPos = heroPos + Vector2.up * 12f,
                        progress = 0f,
                        speed = 1.35f,
                        delay = delay,
                        hasLaunched = false,
                        hasHit = false,
                        angleDeg = 90f,
                        size = 28f,
                        shape = 3, // BaseShapeMask.Wave
                        trajectory = 0,
                        coreCol = palette.CoreColor,
                        bodyCol = palette.BodyColor,
                        rimCol = palette.RimColor,
                        damageToDeal = (w == 0) ? s.baseDamage : 0, // Gây sát thương khi đợt sóng đầu tiên quét qua
                        isCritical = (s.tierRank == "Phẩm A" || s.tierRank == "Phẩm S" || s.tierRank == "Phẩm X"),
                        waveWidth = 24f,
                        waveExpandSpeed = 90f
                    };
                    cast.projectiles.Add(wave);
                }
            }
            else if (ind == 4) // 4. BỘC PHÁ HÀO QUANG 360° (RADIAL NOVA SHOCKWAVE)
            {
                cast.hasShockwave = true;
                cast.shockwaveCenter = heroPos;
                cast.shockwaveRadius = 10f;
                cast.shockwaveMaxRadius = 145f;
                cast.shockwaveAlpha = 1.0f;
                cast.shockwaveColor = palette.BodyColor;

                int shardCount = 12;
                for (int i = 0; i < shardCount; i++)
                {
                    float rad = i * (Mathf.PI * 2f / shardCount);
                    Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    Vector2 endPt = heroPos + dir * 140f;
                    float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                    var shard = new SimSubProjectile
                    {
                        startPos = heroPos,
                        endPos = endPt,
                        curPos = heroPos,
                        lastPos = heroPos,
                        progress = 0f,
                        speed = 1.6f,
                        delay = 0f,
                        hasLaunched = true,
                        hasHit = false,
                        angleDeg = angleDeg,
                        size = 22f,
                        shape = (s.baseShapeMask == 1) ? 1 : 5, // Slash or NovaRing
                        trajectory = 0,
                        coreCol = palette.CoreColor,
                        bodyCol = palette.BodyColor,
                        rimCol = palette.RimColor,
                        damageToDeal = (i == 3) ? s.baseDamage : 0, // Tia bắn hướng xuống chạm trúng dummy
                        isCritical = true
                    };
                    cast.projectiles.Add(shard);
                }
            }
            else if (ind == 5) // 5. HÀO QUANG TỰ THÂN (SELF BUFF / HEAL / SERAPH WINGS)
            {
                cast.hasBuffAura = true;
                cast.buffTimer = 0f;
                cast.buffDuration = 1.9f;
                cast.buffColor = palette.CoreColor;

                // Spawn floating buff text over hero
                SpawnDamageText(heroPos + Vector2.up * -22f, s.IsSupport ? $"💚 HỒI PHỤC +{s.baseDamage}" : $"✨ HỘ THỂ (+25% ST)", palette.CoreColor);
            }
            else // 6. KẾT GIỚI / THẦN THOẠI (TEAM DOMAIN)
            {
                cast.hasAoEDecal = true;
                cast.aoeCenter = dummyPos;
                cast.aoeRadius = 76f;
                cast.aoeAlpha = 0.95f;
                cast.aoeDuration = 1.6f;
                cast.aoeColor = palette.BodyColor;

                // 3 cự kiếm thần thoại cắm xuyên tâm địa
                for (int k = 0; k < 3; k++)
                {
                    Vector2 landPos = dummyPos + new Vector2((k - 1) * 32f, 0f);
                    Vector2 skyPos = new Vector2(landPos.x, heroPos.y - 20f);
                    var sword = new SimSubProjectile
                    {
                        startPos = skyPos,
                        endPos = landPos,
                        curPos = skyPos,
                        lastPos = skyPos,
                        progress = 0f,
                        speed = 2.6f,
                        delay = k * 0.16f,
                        hasLaunched = false,
                        hasHit = false,
                        angleDeg = 90f,
                        size = 36f,
                        shape = 1,
                        trajectory = 0,
                        coreCol = palette.CoreColor,
                        bodyCol = palette.BodyColor,
                        rimCol = palette.RimColor,
                        damageToDeal = Mathf.Max(1, s.baseDamage / 2),
                        isCritical = true
                    };
                    cast.projectiles.Add(sword);
                }
            }

            activeCasts.Add(cast);
            if (playAudio) AudioManager.PlayCast();
        }

        private void SpawnExplosionParticles(Vector2 centerPos, Color elemCol, int count = 20)
        {
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.25f, 0.25f);
                float spd = UnityEngine.Random.Range(45f, 160f);
                var p = new SimParticle
                {
                    pos = centerPos,
                    vel = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * spd,
                    color = Color.Lerp(elemCol, Color.white, UnityEngine.Random.Range(0.2f, 0.85f)),
                    life = UnityEngine.Random.Range(0.35f, 0.65f),
                    size = UnityEngine.Random.Range(5f, 10f),
                    isSpark = true
                };
                p.maxLife = p.life;
                activeParticles.Add(p);
            }
        }

        private void SpawnDamageText(Vector2 pos, string text, Color color)
        {
            activeDamageTexts.Add(new SimDamageText
            {
                pos = pos,
                text = text,
                color = color,
                life = 0.75f,
                maxLife = 0.75f,
                floatY = 0f
            });
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.depth = FantasyUITheme.DepthModal - 2;

            // 1. Phủ mờ màn hình
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. Khung Window Chính Widescreen Chia Đôi
            float w = Mathf.Clamp(Screen.width * 0.95f, 1020f, 1220f);
            float h = Mathf.Clamp(Screen.height * 0.94f, 600f, 720f);
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            Rect winRect = new Rect(x, y, w, h);

            GUI.color = new Color(0.04f, 0.05f, 0.09f, 0.98f);
            GUI.DrawTexture(winRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawDoubleGoldFrame(winRect, 2);

            // 3. Header
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };
            GUI.Label(new Rect(x + 16, y + 10, w - 260, 24), "⚡ TESTING MATRIX · 140 KỸ NĂNG & LIVE ARENA PREVIEW (SLOW-MOTION TESTING)", headerStyle);

            // Nút Thử Ngẫu Nhiên
            if (GUI.Button(new Rect(x + w - 245, y + 10, 115, 24), "🎲 NGẪU NHIÊN"))
            {
                PickRandomSkill();
            }

            // Nút Hồi Đầy Mana / HP
            if (GUI.Button(new Rect(x + w - 124, y + 10, 80, 24), "🧪 ĐẦY MANA"))
            {
                ResetCooldownAndMana();
            }

            // Nút Đóng [✕]
            var closeBtnStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold };
            if (GUI.Button(new Rect(x + w - 38, y + 10, 26, 24), "✕", closeBtnStyle))
            {
                IsOpen = false;
            }

            // Chia 2 cột: Cột Trái 58% (Filters & List), Cột Phải 42% (Live Arena Preview)
            float colLeftW = w * 0.58f;
            float colRightW = w - colLeftW - 28f;
            float contentY = y + 40f;
            float contentH = h - 52f;

            // ─────────────────────────────────────────────────────────────
            // CỘT TRÁI: BỘ LỌC 3 HÀNG & DANH SÁCH 140 KỸ NĂNG
            // ─────────────────────────────────────────────────────────────
            DrawLeftPanel(new Rect(x + 12f, contentY, colLeftW, contentH));

            // ─────────────────────────────────────────────────────────────
            // CỘT PHẢI: KHUNG LIVE SIMULATION ARENA & THANH GẠT TỐC ĐỘ (SLOW-MO)
            // ─────────────────────────────────────────────────────────────
            DrawRightArenaPanel(new Rect(x + 16f + colLeftW, contentY, colRightW, contentH));
        }

        private void DrawLeftPanel(Rect leftRect)
        {
            float curY = leftRect.y;

            // Subheader
            var subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.4f, 0.9f, 1f) }
            };
            GUI.Label(new Rect(leftRect.x, curY, leftRect.width, 18), "💡 Chọn chiêu để xem Preview động bên phải! Bấm [GÁN Ô 1] để tự động chuyển Hệ & Phẩm!", subStyle);
            curY += 20f;

            // Bộ lọc 3 hàng
            int oldClass = filterClassIdx;
            filterClassIdx = GUI.Toolbar(new Rect(leftRect.x, curY, leftRect.width, 22), filterClassIdx, classFilterNames);
            curY += 25f;

            int oldElem = filterElemIdx;
            filterElemIdx = GUI.Toolbar(new Rect(leftRect.x, curY, leftRect.width, 22), filterElemIdx, elemFilterNames);
            curY += 25f;

            int oldTier = filterTierIdx;
            filterTierIdx = GUI.Toolbar(new Rect(leftRect.x, curY, leftRect.width, 22), filterTierIdx, tierFilterNames);
            curY += 27f;

            if (oldClass != filterClassIdx || oldElem != filterElemIdx || oldTier != filterTierIdx)
            {
                ApplyFilter();
            }

            // Thống kê
            var countStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow }
            };
            GUI.Label(new Rect(leftRect.x, curY, 240, 18), $"📊 Danh sách: {filteredSkills.Count} / {allSkills140.Count} kỹ năng", countStyle);

            if (!string.IsNullOrEmpty(statusNotification))
            {
                var notifStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(0.2f, 1f, 0.4f) }
                };
                GUI.Label(new Rect(leftRect.x + 240, curY, leftRect.width - 240, 18), statusNotification, notifStyle);
            }
            curY += 20f;

            // Danh sách cuộn
            Rect listRect = new Rect(leftRect.x, curY, leftRect.width, leftRect.height - (curY - leftRect.y));
            FantasyUITheme.DrawOutline(listRect, new Color(0.25f, 0.35f, 0.5f, 0.6f), 1);

            float rowH = 62f;
            float totalH = filteredSkills.Count * (rowH + 5f) + 8f;
            Rect viewRect = new Rect(0, 0, listRect.width - 16f, Mathf.Max(listRect.height, totalH));

            scrollPos = GUI.BeginScrollView(listRect, scrollPos, viewRect, false, true);

            for (int i = 0; i < filteredSkills.Count; i++)
            {
                var s = filteredSkills[i];
                Rect rowRect = new Rect(4f, 4f + i * (rowH + 5f), viewRect.width - 8f, rowH);

                bool isSelected = (previewSkill == s);

                // Nền hàng
                if (isSelected)
                {
                    GUI.color = new Color(0.18f, 0.28f, 0.45f, 0.98f);
                }
                else
                {
                    GUI.color = (i % 2 == 0) ? new Color(0.08f, 0.11f, 0.18f, 0.95f) : new Color(0.06f, 0.08f, 0.14f, 0.95f);
                }
                GUI.DrawTexture(rowRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                FantasyUITheme.DrawOutline(rowRect, isSelected ? new Color(1f, 0.85f, 0.2f) : s.tierColor, isSelected ? 2 : 1);

                // Icon chiêu
                Rect iconBox = new Rect(rowRect.x + 6f, rowRect.y + (rowH - 48f) / 2f, 48f, 48f);
                Texture2D iconTex = GetTexture(s.iconName);
                if (iconTex == Texture2D.whiteTexture && !string.IsNullOrEmpty(s.scrollIconName))
                    iconTex = GetTexture(s.scrollIconName);

                GUI.DrawTexture(iconBox, iconTex);
                FantasyUITheme.DrawOutline(iconBox, s.tierColor, 1);

                // Thông tin tóm tắt
                float textX = iconBox.x + iconBox.width + 8f;
                float textW = rowRect.width - 165f - textX;

                var nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = s.tierColor }
                };
                GUI.Label(new Rect(textX, rowRect.y + 4f, textW, 18), $"{s.scrollName}  <color=white>({s.tierRank})</color>", nameStyle);

                string shapeName = GetShapeDisplayName(s.baseShapeMask);

                var tagStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    normal = { textColor = new Color(0.85f, 0.9f, 1f) }
                };
                GUI.Label(new Rect(textX, rowRect.y + 22f, textW, 16), $"[{s.requiredClass} · {s.element}] · {shapeName} · Tầm {s.castRange:0.#}m · CD {s.cooldown:0.#}s", tagStyle);

                var descStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 9,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(0.65f, 0.75f, 0.85f) }
                };
                GUI.Label(new Rect(textX, rowRect.y + 38f, textW, 16), s.description, descStyle);

                // Nút Chọn Preview & Nút Gán Ô 1
                float btnW = 75f;
                float btnH = 36f;
                float btnY = rowRect.y + (rowH - btnH) / 2f;

                // Nút Preview
                GUI.color = isSelected ? new Color(1f, 0.85f, 0.25f) : new Color(0.35f, 0.8f, 1f);
                if (GUI.Button(new Rect(rowRect.x + rowRect.width - btnW * 2f - 8f, btnY, btnW, btnH), isSelected ? "🔥 BẮN THẬT" : "👁️ BẮN THỬ"))
                {
                    previewSkill = s;
                    TriggerSimCast(s, false);
                    TestCastSpecificSkill(s); // Bắn trực tiếp ra Game Thật bằng hệ thống skill thực tế
                }

                // Nút Gán Ô 1
                GUI.color = new Color(0.25f, 0.95f, 0.45f);
                if (GUI.Button(new Rect(rowRect.x + rowRect.width - btnW - 4f, btnY, btnW, btnH), "⚡ GÁN Ô 1"))
                {
                    previewSkill = s;
                    EquipAndAutoTunePlayer(s);
                    TriggerSimCast(s, false);
                    TestCastSpecificSkill(s);
                }
                GUI.color = Color.white;
            }

            GUI.EndScrollView();
        }

        private void DrawRightArenaPanel(Rect rightRect)
        {
            float curY = rightRect.y;

            // Header Khung Preview
            var arenaHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.88f, 0.35f) }
            };
            GUI.Label(new Rect(rightRect.x, curY, rightRect.width - 120, 22), "🎯 ĐẤU TRƯỜNG PREVIEW TRỰC TIẾP", arenaHeaderStyle);

            // Nút Bật/Tắt Auto loop
            GUI.color = isAutoLoopCast ? new Color(0.3f, 0.95f, 0.5f) : new Color(0.7f, 0.7f, 0.7f);
            if (GUI.Button(new Rect(rightRect.x + rightRect.width - 110, curY, 110, 22), isAutoLoopCast ? "🔄 LẶP: BẬT" : "⏸️ LẶP: TẮT"))
            {
                isAutoLoopCast = !isAutoLoopCast;
            }
            GUI.color = Color.white;
            curY += 26f;

            // ─────────────────────────────────────────────────────────────
            // VIEWPORT ARENA: Người ở trên, Quái ở dưới, Đạn bắn từ trên xuống
            // ─────────────────────────────────────────────────────────────
            float arenaH = 240f;
            Rect arenaRect = new Rect(rightRect.x, curY, rightRect.width, arenaH);

            // Nền sàn đấu kẻ lưới pixel retro
            GUI.color = new Color(0.03f, 0.05f, 0.09f, 0.98f);
            GUI.DrawTexture(arenaRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawDoubleGoldFrame(arenaRect, 1);

            // Vẽ lưới kẻ ô sàn đấu mờ
            GUI.color = new Color(0.15f, 0.22f, 0.35f, 0.25f);
            for (float gx = arenaRect.x + 24; gx < arenaRect.x + arenaRect.width; gx += 24)
            {
                GUI.DrawTexture(new Rect(gx, arenaRect.y + 4, 1, arenaH - 8), Texture2D.whiteTexture);
            }
            for (float gy = arenaRect.y + 24; gy < arenaRect.y + arenaH; gy += 24)
            {
                GUI.DrawTexture(new Rect(arenaRect.x + 4, gy, arenaRect.width - 8, 1), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // Vị trí Người chơi (Trên) và Quái vật (Dưới)
            float cx = arenaRect.x + arenaRect.width / 2f;
            float topY = arenaRect.y + 28f;
            float botY = arenaRect.y + arenaH - 42f;

            // 1. VẼ NGƯỜI CHƠI CHIBI (Hero) Ở TRÊN
            Rect heroBox = new Rect(cx - 20, topY, 40, 40);
            Texture2D heroTex = GetTexture("player_male_base");
            if (heroTex == Texture2D.whiteTexture) heroTex = GetTexture("icon_skill_dash");
            GUI.DrawTexture(heroBox, heroTex);

            // Nhãn tên người chơi
            var heroLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.35f, 0.85f, 1f) }
            };
            GUI.Label(new Rect(cx - 60, topY - 16, 120, 16), "🧙 BẠN (HERO)", heroLabelStyle);

            // 2. VẼ QUÁI VẬT (Target Dummy) Ở DƯỚI
            Rect dummyBox = new Rect(cx - 22 + dummyShakeX, botY - 20, 44, 44);
            Texture2D dummyT = GetTexture("pet_squirrel");
            if (dummyHitFlash > 0f)
            {
                GUI.color = Color.Lerp(Color.red, Color.white, 0.6f);
            }
            GUI.DrawTexture(dummyBox, dummyT);
            GUI.color = Color.white;

            // Nhãn quái vật + Thanh máu Dummy
            var dummyLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.4f, 0.4f) }
            };
            GUI.Label(new Rect(cx - 60, botY + 24, 120, 16), "🎯 BÙ NHÌN (DUMMY)", dummyLabelStyle);

            // Thanh máu dummy (co giãn mượt mà theo sát thương thật)
            Rect dHpRect = new Rect(cx - 30, botY + 20, 60, 4);
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            GUI.DrawTexture(dHpRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            GUI.DrawTexture(new Rect(dHpRect.x, dHpRect.y, dHpRect.width * dummyHp, 4), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // ─────────────────────────────────────────────────────────────
            // 3. VẼ ĐẠN BAY & HIỆU ỨNG CHIÊU THỜI GIAN THỰC (ĐỒ HỌA PIXEL CHUẨN XÁC)
            // ─────────────────────────────────────────────────────────────
            Vector2 heroCenter = new Vector2(cx, topY + 20f);
            Vector2 dummyCenter = new Vector2(cx, botY);

            // 3.1. Vẽ Vòng Tròn Ma Pháp Dưới Đất (AoE Ground Decals)
            Texture2D aoeTex = GetAoECircleTexture();
            foreach (var cast in activeCasts)
            {
                if (cast.hasAoEDecal && cast.aoeAlpha > 0f)
                {
                    float r = cast.aoeRadius;
                    Vector2 piv = cast.aoeCenter;
                    GUIUtility.RotateAroundPivot(cast.aoeRotation, piv);
                    GUI.color = new Color(cast.aoeColor.r, cast.aoeColor.g, cast.aoeColor.b, cast.aoeAlpha * 0.65f);
                    GUI.DrawTexture(new Rect(cast.aoeCenter.x - r, cast.aoeCenter.y - r * 0.65f, r * 2f, r * 1.3f), aoeTex);
                    GUIUtility.RotateAroundPivot(-cast.aoeRotation, piv);
                    GUI.color = Color.white;
                }
            }

            // 3.2. Vẽ Nova Shockwave Ring 360° Nở To Từ Tâm Hero
            Texture2D novaTex = GetMaskTexture(BaseShapeMask.NovaRing);
            foreach (var cast in activeCasts)
            {
                if (cast.hasShockwave && cast.shockwaveAlpha > 0f)
                {
                    float r = cast.shockwaveRadius;
                    GUI.color = new Color(cast.shockwaveColor.r, cast.shockwaveColor.g, cast.shockwaveColor.b, cast.shockwaveAlpha * 0.75f);
                    GUI.DrawTexture(new Rect(cast.shockwaveCenter.x - r, cast.shockwaveCenter.y - r, r * 2f, r * 2f), novaTex);
                    GUI.color = Color.white;
                }
            }

            // 3.3. Vẽ Cặp Cánh Seraph Wings & Hào Quang Khiên Hộ Thể (Nếu Buff Active)
            Texture2D wingTex = GetWingTexture();
            foreach (var cast in activeCasts)
            {
                if (cast.hasBuffAura && cast.buffTimer < cast.buffDuration)
                {
                    float pulse = Mathf.Sin(Time.realtimeSinceStartup * 14f) * 3f;
                    float wW = 32f + pulse;
                    float wH = 24f;
                    GUI.color = new Color(cast.buffColor.r, cast.buffColor.g, cast.buffColor.b, 0.9f);

                    Vector2 pL = new Vector2(heroCenter.x - 22f, heroCenter.y);
                    GUIUtility.RotateAroundPivot(-20f + pulse, pL);
                    GUI.DrawTexture(new Rect(pL.x - wW / 2f, pL.y - wH / 2f, wW, wH), wingTex);
                    GUIUtility.RotateAroundPivot(20f - pulse, pL);

                    Vector2 pR = new Vector2(heroCenter.x + 22f, heroCenter.y);
                    GUIUtility.RotateAroundPivot(20f - pulse, pR);
                    GUI.DrawTexture(new Rect(pR.x - wW / 2f, pR.y - wH / 2f, wW, wH), wingTex);
                    GUIUtility.RotateAroundPivot(-20f + pulse, pR);

                    GUI.color = Color.white;
                }
            }

            // 3.4. Vẽ Sub-Projectiles & Phôi Khối Thủ Tục (Đạn, Kiếm Khí, Mưa Gai Băng, Xúc Tu, Rễ Gai, Cột Lửa...)
            Texture2D sparkTex = GetSparkTexture();
            foreach (var cast in activeCasts)
            {
                foreach (var p in cast.projectiles)
                {
                    if (!p.hasLaunched || p.hasHit) continue;

                    // Vẽ vệt đuôi Trail chuyển sắc gradient mờ dần
                    for (int t = 0; t < p.trail.Count; t++)
                    {
                        float frac = (float)(t + 1) / p.trail.Count;
                        float trSz = p.size * (0.25f + frac * 0.45f);
                        GUI.color = new Color(p.bodyCol.r, p.bodyCol.g, p.bodyCol.b, frac * 0.50f);
                        GUI.DrawTexture(new Rect(p.trail[t].x - trSz / 2f, p.trail[t].y - trSz / 2f, trSz, trSz), sparkTex);
                    }

                    // Vẽ đầu đạn / phôi khối xoay đúng góc bay
                    Texture2D maskTex = GetMaskTexture((BaseShapeMask)p.shape);
                    Vector2 piv = p.curPos;
                    GUIUtility.RotateAroundPivot(p.angleDeg, piv);

                    if (p.shape == (int)BaseShapeMask.Wave) // Sóng quạt tỏa rộng
                    {
                        float waveW = Mathf.Lerp(p.waveWidth, p.waveWidth + p.waveExpandSpeed, p.progress);
                        GUI.color = p.bodyCol;
                        GUI.DrawTexture(new Rect(p.curPos.x - waveW / 2f, p.curPos.y - p.size / 2f, waveW, p.size), maskTex);

                        GUI.color = p.coreCol;
                        float cW = waveW * 0.65f;
                        GUI.DrawTexture(new Rect(p.curPos.x - cW / 2f, p.curPos.y - (p.size * 0.6f) / 2f, cW, p.size * 0.6f), maskTex);
                    }
                    else
                    {
                        // Thân đạn nguyên tố
                        GUI.color = p.bodyCol;
                        GUI.DrawTexture(new Rect(p.curPos.x - p.size / 2f, p.curPos.y - p.size / 2f, p.size, p.size), maskTex);

                        // Lõi sáng rực nóng cháy (Hot core 2-tone)
                        GUI.color = p.coreCol;
                        float coreSz = p.size * 0.6f;
                        GUI.DrawTexture(new Rect(p.curPos.x - coreSz / 2f, p.curPos.y - coreSz / 2f, coreSz, coreSz), maskTex);
                    }

                    GUIUtility.RotateAroundPivot(-p.angleDeg, piv);
                    GUI.color = Color.white;
                }
            }

            // 3.5. Vẽ hạt nổ bộc phá bung tóe
            foreach (var pt in activeParticles)
            {
                float alpha = Mathf.Clamp01(pt.life / pt.maxLife);
                GUI.color = new Color(pt.color.r, pt.color.g, pt.color.b, alpha);
                GUI.DrawTexture(new Rect(pt.pos.x - pt.size / 2f, pt.pos.y - pt.size / 2f, pt.size, pt.size), sparkTex);
            }
            GUI.color = Color.white;

            // 3.6. Vẽ chữ số sát thương bay lên sinh động
            foreach (var dtItem in activeDamageTexts)
            {
                float alpha = Mathf.Clamp01(dtItem.life / dtItem.maxLife);
                var dmgStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(dtItem.color.r, dtItem.color.g, dtItem.color.b, alpha) }
                };
                GUI.Label(new Rect(dtItem.pos.x - 70f, dtItem.pos.y - dtItem.floatY, 140f, 22f), dtItem.text, dmgStyle);
            }
            GUI.color = Color.white;

            curY += arenaH + 10f;

            // ─────────────────────────────────────────────────────────────
            // THANH GẠT TỐC ĐỘ HIỆU ỨNG (SLOW-MOTION TESTING SLIDER: 0.1x -> 2.0x)
            // ─────────────────────────────────────────────────────────────
            Rect speedBox = new Rect(rightRect.x, curY, rightRect.width, 68f);
            GUI.color = new Color(0.06f, 0.09f, 0.14f, 0.95f);
            GUI.DrawTexture(speedBox, Texture2D.whiteTexture);
            GUI.color = Color.white;
            FantasyUITheme.DrawOutline(speedBox, new Color(0.3f, 0.45f, 0.7f, 0.6f), 1);

            float curSpeed = GameSettings.SimulationSpeed;
            string slowSpeedStr = (1f / curSpeed).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            string fastSpeedStr = curSpeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            string speedLabel = (curSpeed < 0.95f) ? $" (Chậm {slowSpeedStr}x - Soi từng frame)" :
                                (curSpeed > 1.05f) ? $" (Tua nhanh {fastSpeedStr}x)" : " (Chuẩn 100%)";

            var spdHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };
            GUI.Label(new Rect(speedBox.x + 8f, speedBox.y + 4f, speedBox.width - 16f, 18f),
                $"⏱️ TỐC ĐỘ HIỆU ỨNG: <color=#00e5ff>{curSpeed:0.00}x</color>{speedLabel}", spdHeaderStyle);

            // Thanh gạt slider
            float newSpeed = GUI.HorizontalSlider(new Rect(speedBox.x + 8f, speedBox.y + 24f, speedBox.width - 16f, 18f), curSpeed, 0.1f, 2.0f);
            if (Mathf.Abs(newSpeed - curSpeed) > 0.005f)
            {
                GameSettings.SetSimulationSpeed(newSpeed);
            }

            // Các nút preset tốc độ nhanh
            float pBtnW = (speedBox.width - 24f) / 4f;
            float pBtnX = speedBox.x + 8f;
            float pBtnY = speedBox.y + 42f;

            if (DrawPresetBtn(new Rect(pBtnX, pBtnY, pBtnW, 20f), "🐢 0.2x Chậm", curSpeed, 0.2f))
                GameSettings.SetSimulationSpeed(0.2f);
            pBtnX += pBtnW + 3f;

            if (DrawPresetBtn(new Rect(pBtnX, pBtnY, pBtnW, 20f), "0.5x Nửa Tốc", curSpeed, 0.5f))
                GameSettings.SetSimulationSpeed(0.5f);
            pBtnX += pBtnW + 3f;

            if (DrawPresetBtn(new Rect(pBtnX, pBtnY, pBtnW, 20f), "▶️ 1.0x Chuẩn", curSpeed, 1.0f))
                GameSettings.SetSimulationSpeed(1.0f);
            pBtnX += pBtnW + 3f;

            if (DrawPresetBtn(new Rect(pBtnX, pBtnY, pBtnW, 20f), "🚀 1.5x Nhanh", curSpeed, 1.5f))
                GameSettings.SetSimulationSpeed(1.5f);

            curY += 76f;

            // ─────────────────────────────────────────────────────────────
            // HỘP THÔNG SỐ CHI TIẾT KỸ NĂNG ĐANG PREVIEW
            // ─────────────────────────────────────────────────────────────
            if (previewSkill != null)
            {
                float infoBoxH = 100f;
                Rect infoBox = new Rect(rightRect.x, curY, rightRect.width, infoBoxH);
                GUI.color = new Color(0.05f, 0.07f, 0.11f, 0.95f);
                GUI.DrawTexture(infoBox, Texture2D.whiteTexture);
                GUI.color = Color.white;
                FantasyUITheme.DrawOutline(infoBox, previewSkill.tierColor, 1);

                var titleS = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = previewSkill.tierColor }
                };
                GUI.Label(new Rect(infoBox.x + 8f, infoBox.y + 4f, infoBox.width - 16f, 18f),
                    $"📜 {previewSkill.scrollName} [{previewSkill.tierRank}] · {previewSkill.requiredClass} ({previewSkill.element})", titleS);

                string shapeName = GetShapeDisplayName(previewSkill.baseShapeMask);

                var statS = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    normal = { textColor = new Color(0.9f, 0.95f, 1f) }
                };
                GUI.Label(new Rect(infoBox.x + 8f, infoBox.y + 24f, infoBox.width - 16f, 16f),
                    $"• Phôi Khối: <color=#ffaa00>{shapeName}</color> | Tầm Xa: <color=#00e5ff>{previewSkill.castRange:0.#}m</color> | Bán Kính: <color=#ff66cc>{previewSkill.explosionRadius:0.#}m</color>", statS);

                GUI.Label(new Rect(infoBox.x + 8f, infoBox.y + 42f, infoBox.width - 16f, 16f),
                    $"• Sát Thương: <color=#ff4444>{previewSkill.baseDamage}</color> | Hồi Chiêu: <color=#ffd700>{previewSkill.cooldown:0.#}s</color> | Mana: <color=#33ccff>{previewSkill.manaCost}</color>", statS);

                var descS = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 9,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(0.7f, 0.8f, 0.9f) }
                };
                GUI.Label(new Rect(infoBox.x + 8f, infoBox.y + 60f, infoBox.width - 16f, 36f),
                    $"Mô tả: \"{previewSkill.description}\"", descS);

                curY += infoBoxH + 8f;
            }

            // ─────────────────────────────────────────────────────────────
            // HÀNG NÚT THAO TÁC ARENA
            // ─────────────────────────────────────────────────────────────
            float actionBtnH = 34f;
            float aBtnW = (rightRect.width - 8f) / 3f;
            float aBtnX = rightRect.x;

            // Nút 1: Bắn thử ra game thật
            GUI.color = new Color(1.0f, 0.45f, 0.15f);
            if (GUI.Button(new Rect(aBtnX, curY, aBtnW, actionBtnH), "🔥 BẮN THẬT (IN-GAME)"))
            {
                TestCastSpecificSkill(previewSkill);
                TriggerSimCast(previewSkill, true);
            }
            aBtnX += aBtnW + 4f;

            // Nút 2: Tự động đổi Phái/Hệ & Bắn Thử
            GUI.color = new Color(0.35f, 0.85f, 1f);
            if (GUI.Button(new Rect(aBtnX, curY, aBtnW, actionBtnH), "⚡ TEST NHANH"))
            {
                EquipAndAutoTunePlayer(previewSkill);
                TestCastSpecificSkill(previewSkill);
            }
            aBtnX += aBtnW + 4f;

            // Nút 3: Gán vào phím [1]
            GUI.color = new Color(0.25f, 0.95f, 0.45f);
            if (GUI.Button(new Rect(aBtnX, curY, aBtnW, actionBtnH), "⚡ GÁN PHÍM [1]"))
            {
                EquipAndAutoTunePlayer(previewSkill);
            }
            GUI.color = Color.white;
        }

        private bool DrawPresetBtn(Rect r, string label, float curSpeed, float targetSpeed)
        {
            bool isCurrent = Mathf.Abs(curSpeed - targetSpeed) < 0.04f;
            GUI.color = isCurrent ? new Color(1f, 0.85f, 0.25f) : new Color(0.85f, 0.90f, 1f);
            bool clicked = GUI.Button(r, label);
            GUI.color = Color.white;
            return clicked;
        }

        private void EquipAndAutoTunePlayer(SkillScrollData s)
        {
            if (s == null) return;

            var pStats = FindAnyObjectByType<PlayerStats>();
            if (pStats != null)
            {
                pStats.SetCharacterClass(s.requiredClass);
                pStats.SetElement(s.element);
            }

            var aiming = FindAnyObjectByType<SkillAimingController>();
            if (aiming != null)
            {
                aiming.EquipScroll(0, s);
            }

            var inv = FindAnyObjectByType<InventoryManager>();
            if (inv != null)
            {
                bool exists = inv.Items.Exists(x => x.id == s.id);
                if (!exists && !inv.IsFull)
                {
                    var item = new InventoryItem(
                        s.id, s.scrollName, ItemType.SkillScroll, EquipSlot.None,
                        s.tierRank, s.tierColor, s.scrollIconName, s.description, s.description,
                        1, s.Clone(), s.requiredClass, s.element);
                    inv.AddItem(item);
                }
            }

            if (pStats != null)
            {
                FloatingDamageManager.ShowFloatingText(
                    pStats.transform.position + Vector3.up * 1.5f,
                    $"⚡ AUTO ĐỔI: [{s.requiredClass} · {s.element}] & GÁN Ô [1]!",
                    new Color(0.3f, 1f, 0.5f));
            }

            statusNotification = $"✅ Đã đổi sang [{s.requiredClass} · {s.element}] & gán '{s.scrollName}' vào Ô [1]!";
            notifTimer = 4.0f;
            Debug.Log($"<color=green>[TESTING MATRIX]</color> Đã tự động đổi Phái sang '{s.requiredClass}', Thuộc tính sang '{s.element}' và gán '{s.scrollName}' vào Ô [1]!");
        }

        private void TestCastSpecificSkill(SkillScrollData s)
        {
            if (s == null) return;

            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                statusNotification = "⚠️ Không tìm thấy Player_Hero trong Scene!";
                notifTimer = 3.0f;
                return;
            }

            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld = (Camera.main != null)
                ? Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 10f))
                : (player.transform.position + Vector3.right * 3f);
            mouseWorld.z = 0f;

            Vector2 aimDir = ((Vector2)(mouseWorld - player.transform.position)).normalized;
            if (aimDir.sqrMagnitude < 0.001f) aimDir = Vector2.right;

            Vector3 spawnPos = player.transform.position + (Vector3)(aimDir * 0.5f);
            BaseShapeMask shape = (BaseShapeMask)s.baseShapeMask;
            TrajectoryType traj = (TrajectoryType)s.trajectoryType;

            if (ProceduralVFXEngine.Instance == null)
            {
                statusNotification = "⚠️ ProceduralVFXEngine chưa khởi tạo!";
                notifTimer = 3.0f;
                return;
            }

            switch (s.indicatorType)
            {
                case 2: // Vùng Đất / Mưa (AoE Rain)
                    Vector3 rainTarget = mouseWorld;
                    if (Vector3.Distance(player.transform.position, rainTarget) > s.castRange)
                    {
                        rainTarget = player.transform.position + (Vector3)(aimDir * s.castRange);
                    }
                    ProceduralVFXEngine.Instance.SpawnProceduralAoERain(
                        rainTarget, s.element, s.explosionRadius, s.baseDamage, player.name,
                        s.rainFallAngle, s.rainFallStyle, player.transform.position);
                    statusNotification = $"🌧️ ĐÃ BẮN THỬ: Mưa [{s.scrollName}] Hướng: {s.rainFallAngle}!";
                    break;

                case 3: // Sóng Quạt (Cone)
                    ProceduralVFXEngine.Instance.SpawnProceduralConeWave(
                        player.transform.position, aimDir, s.element, s.castRange, s.baseDamage, player.name);
                    statusNotification = $"🔥 ĐÃ BẮN THỬ: Sóng Quạt [{s.scrollName}] Tầm {s.castRange}m!";
                    break;

                case 4: // Nổ 360° (Nova Ring)
                    ProceduralVFXEngine.Instance.SpawnProceduralRadialNova(
                        player.transform.position, s.element, s.explosionRadius * 2f, s.baseDamage, player.name);
                    statusNotification = $"💥 ĐÃ BẮN THỬ: Nổ 360° [{s.scrollName}] Bán kính {s.explosionRadius * 2f:0.#}m!";
                    break;

                case 5: // Hào Quang Tự Thân (Self Aura)
                    ProceduralVFXEngine.Instance.SpawnProceduralSelfAura(
                        player.transform, s.element, 3.5f);
                    statusNotification = $"✨ ĐÃ BẮN THỬ: Hào Quang Tự Thân [{s.scrollName}]!";
                    break;

                case 6: // Kết Giới Đội (Team Domain)
                    Vector3 zoneTarget = mouseWorld;
                    if (Vector3.Distance(player.transform.position, zoneTarget) > s.castRange)
                    {
                        zoneTarget = player.transform.position + (Vector3)(aimDir * s.castRange);
                    }
                    ProceduralVFXEngine.Instance.SpawnProceduralTeamBarrier(
                        zoneTarget, s.element, s.explosionRadius * 2.5f, 4.0f);
                    statusNotification = $"🛡️ ĐÃ BẮN THỬ: Kết Giới Đội [{s.scrollName}]!";
                    break;

                case 1: // Bắn Thẳng
                default:
                    ProceduralVFXEngine.Instance.FireProceduralSkill(
                        spawnPos, aimDir, shape, s.element, traj, s.baseDamage, s.flySpeed, s.explosionRadius,
                        s.hasTrail, s.hasScreenShake, player.name, null, s.castRange);
                    statusNotification = $"🔥 ĐÃ BẮN THỬ: [{s.scrollName}] Tốc độ {s.flySpeed}m/s Tầm {s.castRange}m!";
                    break;
            }

            AudioManager.PlayCast();
            notifTimer = 4.0f;
        }

        private void PickRandomSkill()
        {
            if (allSkills140.Count == 0) return;
            int rIdx = UnityEngine.Random.Range(0, allSkills140.Count);
            previewSkill = allSkills140[rIdx];
            EquipAndAutoTunePlayer(previewSkill);
            TriggerSimCast(previewSkill, true);
        }

        private void ResetCooldownAndMana()
        {
            var pStats = FindAnyObjectByType<PlayerStats>();
            if (pStats != null)
            {
                pStats.RestoreMana(999);
            }
            statusNotification = "💧 Đã hồi phục đầy 100% Mana!";
            notifTimer = 3.0f;
        }
    }
}
