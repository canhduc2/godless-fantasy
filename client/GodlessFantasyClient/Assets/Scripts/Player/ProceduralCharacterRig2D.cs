using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Core;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// ╔══════════════════════════════════════════════════════════════════════════════╗
    /// ║          PROCEDURAL RIG 2D — Hệ Thống Xương Chibi Thuần Code               ║
    /// ╠══════════════════════════════════════════════════════════════════════════════╣
    /// ║  • Không cần Spine, không cần Animator Controller, thuần C# 60fps mượt mà  ║
    /// ║  • 8 Hướng x 6 Trạng thái: Idle / Walk / Run / Dash / Attack / Hurt / Die   ║
    /// ║  • Mỗi bộ phận (Đầu, tóc, thân, 2 tay, 2 chân, vũ khí) gắn khớp xương riêng ║
    /// ║  • Khớp vai, hông, cổ xoay tự nhiên quanh Joint Pivots chính xác           ║
    /// ║  • Hỗ trợ Sprite Swap (Paperdoll) + Ghost Sprint Afterimage + Slash Arc VFX║
    /// ║  • Đồng bộ 100% với CombatHUD, BasicAttack, SkillAiming, Inventory         ║
    /// ╚══════════════════════════════════════════════════════════════════════════════╝
    /// </summary>
    public class ProceduralCharacterRig2D : MonoBehaviour
    {
        public static ProceduralCharacterRig2D Instance { get; private set; }

        // ─── PUBLIC STATE ───────────────────────────────────────────────
        public bool       IsFemale   = false;
        public SkinType   CurrentSkin = SkinType.None;
        public Direction8 FacingDir  = Direction8.South;

        // Alias để tương thích ngược hoàn toàn với code cũ
        public Direction8 CurrentDirection
        {
            get => FacingDir;
            set => FacingDir = value;
        }

        // ─── REFERENCES ─────────────────────────────────────────────────
        private Rigidbody2D      rb;
        private PlayerMovement   movement;
        private InventoryManager inventoryManager;
        private Camera           mainCam;

        // ─── RIG ROOT & JOINTS ──────────────────────────────────────────
        private Transform partRoot;
        private Transform partShadow;

        private Transform jointBody;
        private Transform jointHead;
        private Transform jointArmLeft;
        private Transform jointArmRight;
        private Transform jointLegLeft;
        private Transform jointLegRight;
        private Transform jointWeapon;
        private Transform jointSlash;
        private Transform jointCostume;

        // ─── SPRITE RENDERERS ────────────────────────────────────────────
        private SpriteRenderer srHairBack;
        private SpriteRenderer srLegL;
        private SpriteRenderer srLegR;
        private SpriteRenderer srBody;
        private SpriteRenderer srArmL;
        private SpriteRenderer srArmR;
        private SpriteRenderer srHead;
        private SpriteRenderer srHairFront;
        private SpriteRenderer srWeapon;
        private SpriteRenderer srSlash;
        private SpriteRenderer srShadow;
        private SpriteRenderer srCostume;

        // ─── SPRITE CACHE ────────────────────────────────────────────────
        private static readonly Dictionary<string, Sprite> sprCache = new Dictionary<string, Sprite>();

        // ─── ANIMATION TIMERS ────────────────────────────────────────────
        private float cycleTimer  = 0f;
        private float breathTimer = 0f;
        private float ghostTimer  = 0f;
        private float hurtTimer   = 0f;
        private bool  isDying     = false;
        private float dieTimer    = 0f;
        private bool  isAttacking = false;

        private Vector3 rootOrigin;

        // ─── ANATOMICAL JOINT PIVOTS (Local-space relative to RigRoot) ───
        // Khi rotation = 0, sprite offset = -jointPivot -> vị trí sprite bằng (0,0,0) tuyệt đối
        private static readonly Vector3 PIVOT_HEAD     = new Vector3( 0.00f, 0.48f, 0f);
        private static readonly Vector3 PIVOT_ARM_L    = new Vector3(-0.16f, 0.38f, 0f);
        private static readonly Vector3 PIVOT_ARM_R    = new Vector3( 0.16f, 0.38f, 0f);
        private static readonly Vector3 PIVOT_LEG_L    = new Vector3(-0.08f, 0.18f, 0f);
        private static readonly Vector3 PIVOT_LEG_R    = new Vector3( 0.08f, 0.18f, 0f);
        private static readonly Vector3 PIVOT_BODY     = new Vector3( 0.00f, 0.28f, 0f);

        // ════════════════════════════════════════════════════════════════
        //  AWAKE & SETUP
        // ════════════════════════════════════════════════════════════════
        private void Awake()
        {
            Instance         = this;
            rb               = GetComponent<Rigidbody2D>();
            movement         = GetComponent<PlayerMovement>();
            inventoryManager = GetComponent<InventoryManager>();
            mainCam          = Camera.main;

            if (rb != null)
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Vô hiệu hóa SpriteRenderer cũ trên root (nếu có)
            var oldSr = GetComponent<SpriteRenderer>();
            if (oldSr != null) oldSr.enabled = false;

            BuildRigHierarchy();
        }

        // ════════════════════════════════════════════════════════════════
        //  BUILD RIG HIERARCHY — Khớp Xương 2D Độc Lập
        // ════════════════════════════════════════════════════════════════
        private void BuildRigHierarchy()
        {
            // 1. partRoot: neo tại bàn chân (Feet Pivot)
            partRoot = GetOrCreate(transform, "RigRoot");
            partRoot.localPosition = new Vector3(0f, -0.48f, 0f);
            rootOrigin = partRoot.localPosition;

            // 2. Bóng Đổ Mặt Đất (Ground Shadow)
            partShadow = GetOrCreate(transform, "Rig_Shadow");
            partShadow.localPosition = new Vector3(0f, -0.50f, 0f);
            partShadow.localScale    = new Vector3(0.55f, 0.28f, 1f);
            srShadow = EnsureSR(partShadow, 7);
            srShadow.color  = new Color(0f, 0f, 0f, 0.35f);
            srShadow.sprite = LoadSpr("indicator_circle");

            // 3. Khớp Xương (Joints) & Sprites theo thứ tự SortingOrder:
            // 8=HairBack, 9=LegL, 10=LegR, 11=Body, 12=ArmL, 13=ArmR, 14=Head, 15=HairFront, 16=Weapon, 17=Slash

            // Khớp Thân
            jointBody = CreateJoint("Joint_Body", PIVOT_BODY);
            var sprBodyObj = CreateOffsetVisual(jointBody, "Sprite_Body", -PIVOT_BODY);
            srBody = EnsureSR(sprBodyObj, 11);

            // Khớp Đầu & Tóc (Xoay quanh cổ)
            jointHead = CreateJoint("Joint_Head", PIVOT_HEAD);
            var sprHairBackObj  = CreateOffsetVisual(jointHead, "Sprite_HairBack",  -PIVOT_HEAD);
            var sprHeadObj      = CreateOffsetVisual(jointHead, "Sprite_Head",      -PIVOT_HEAD);
            var sprHairFrontObj = CreateOffsetVisual(jointHead, "Sprite_HairFront", -PIVOT_HEAD);
            srHairBack  = EnsureSR(sprHairBackObj,  8);
            srHead      = EnsureSR(sprHeadObj,      14);
            srHairFront = EnsureSR(sprHairFrontObj, 15);

            // Khớp Tay Trái & Tay Phải (Xoay quanh vai)
            jointArmLeft  = CreateJoint("Joint_ArmLeft",  PIVOT_ARM_L);
            jointArmRight = CreateJoint("Joint_ArmRight", PIVOT_ARM_R);
            var sprArmLObj = CreateOffsetVisual(jointArmLeft,  "Sprite_ArmL", -PIVOT_ARM_L);
            var sprArmRObj = CreateOffsetVisual(jointArmRight, "Sprite_ArmR", -PIVOT_ARM_R);
            srArmL = EnsureSR(sprArmLObj, 12);
            srArmR = EnsureSR(sprArmRObj, 13);

            // Khớp Chân Trái & Chân Phải (Xoay quanh hông)
            jointLegLeft  = CreateJoint("Joint_LegLeft",  PIVOT_LEG_L);
            jointLegRight = CreateJoint("Joint_LegRight", PIVOT_LEG_R);
            var sprLegLObj = CreateOffsetVisual(jointLegLeft,  "Sprite_LegL", -PIVOT_LEG_L);
            var sprLegRObj = CreateOffsetVisual(jointLegRight, "Sprite_LegR", -PIVOT_LEG_R);
            srLegL = EnsureSR(sprLegLObj, 9);
            srLegR = EnsureSR(sprLegRObj, 10);

            // Khớp Vũ Khí (Weapon Orbit & Dynamic Aim)
            jointWeapon = GetOrCreate(partRoot, "Joint_Weapon");
            jointWeapon.localPosition = new Vector3(0.30f, 0.25f, 0f);
            srWeapon = EnsureSR(jointWeapon, 16);
            srWeapon.sprite = LoadSpr("item_weapon");
            jointWeapon.localScale = new Vector3(0.85f, 0.85f, 1f);

            // Khớp Vệt Chém Nguyên Tố (Elemental Slash VFX)
            jointSlash = GetOrCreate(partRoot, "Joint_Slash");
            jointSlash.localPosition = new Vector3(0.40f, 0.25f, 0f);
            srSlash = EnsureSR(jointSlash, 17);
            srSlash.enabled = false;
            jointSlash.localScale = new Vector3(1.1f, 1.1f, 1f);

            // Lớp Ngoại Trang Toàn Thân (Costume / Skin)
            jointCostume = GetOrCreate(partRoot, "Layer_Costume");
            jointCostume.localPosition = Vector3.zero;
            srCostume = EnsureSR(jointCostume, 14);
            srCostume.enabled = false;

            LoadDirectionSprites();
        }

        private Transform CreateJoint(string name, Vector3 localPos)
        {
            var j = GetOrCreate(partRoot, name);
            j.localPosition = localPos;
            j.localRotation = Quaternion.identity;
            return j;
        }

        private Transform CreateOffsetVisual(Transform parent, string name, Vector3 localOffset)
        {
            var v = GetOrCreate(parent, name);
            v.localPosition = localOffset;
            v.localRotation = Quaternion.identity;
            return v;
        }

        // ════════════════════════════════════════════════════════════════
        //  SPRITE LOADING — Tải Sprites theo Hướng, Giới Tính, Ngoại Trang
        // ════════════════════════════════════════════════════════════════
        private void LoadDirectionSprites()
        {
            // 1. Trường hợp Ngoại Trang Toàn Thần (Costumes)
            if (CurrentSkin == SkinType.SoulKnight)
            {
                SetRigPartsVisible(false);
                srCostume.enabled = true;
                string dirStr = DirName(FacingDir);
                srCostume.sprite = LoadSpr($"Characters/skin_soul_knight_{dirStr}_0");
                return;
            }
            if (CurrentSkin == SkinType.VoidReaper)
            {
                SetRigPartsVisible(false);
                srCostume.enabled = true;
                string dirStr = DirName(FacingDir);
                srCostume.sprite = LoadSpr($"Characters/skin_void_reaper_{dirStr}_0");
                return;
            }

            // 2. Phôi Gốc Xương 2D (Default Rig Mode)
            srCostume.enabled = false;
            SetRigPartsVisible(true);

            string g   = IsFemale ? "female" : "male";
            string dir = DirName(FacingDir);

            // Nạp Sprite từng bộ phận của Xương 2D Chibi:
            srHead.sprite = LoadRigSpr(g, dir, "idle_head") ?? LoadRigSpr(g, dir, "body");
            srBody.sprite = LoadRigSpr(g, dir, "idle_body") ?? LoadRigSpr(g, dir, "body");
            srArmL.sprite = LoadRigSpr(g, dir, "idle_arm_left");
            srArmR.sprite = LoadRigSpr(g, dir, "idle_arm_right");
            srLegL.sprite = LoadRigSpr(g, dir, "idle_leg_left");
            srLegR.sprite = LoadRigSpr(g, dir, "idle_leg_right");

            // Mọi hướng đều có file ảnh chuẩn vẽ riêng biệt trong folder, không bị lật ngược
            srHead.flipX = false;
            srBody.flipX = false;
            srArmL.flipX = false;
            srArmR.flipX = false;
            srLegL.flipX = false;
            srLegR.flipX = false;

            UpdateSortingOrders(FacingDir);
        }

        private void UpdateSortingOrders(Direction8 d)
        {
            switch (d)
            {
                case Direction8.East:
                case Direction8.SouthEast:
                case Direction8.NorthEast:
                    // Nhìn sang Phải: Tay Trái (far) & Chân Trái (far) ở sau, Tay Phải (near) & Chân Phải (near) ở trước
                    if (srArmL != null) srArmL.sortingOrder = 9;
                    if (srLegL != null) srLegL.sortingOrder = 9;
                    if (srBody != null) srBody.sortingOrder = 11;
                    if (srLegR != null) srLegR.sortingOrder = 12;
                    if (srArmR != null) srArmR.sortingOrder = 13;
                    if (srHead != null) srHead.sortingOrder = 14;
                    break;

                case Direction8.West:
                case Direction8.SouthWest:
                case Direction8.NorthWest:
                    // Nhìn sang Trái: Tay Phải (far) & Chân Phải (far) ở sau, Tay Trái (near) & Chân Trái (near) ở trước
                    if (srArmR != null) srArmR.sortingOrder = 9;
                    if (srLegR != null) srLegR.sortingOrder = 9;
                    if (srBody != null) srBody.sortingOrder = 11;
                    if (srLegL != null) srLegL.sortingOrder = 12;
                    if (srArmL != null) srArmL.sortingOrder = 13;
                    if (srHead != null) srHead.sortingOrder = 14;
                    break;

                case Direction8.North:
                    // Nhìn Lên (Lưng): Tay & Chân ở sau thân (dưới), Lưng ở trước (trên), Đầu ở trên cùng
                    if (srArmL != null) srArmL.sortingOrder = 9;
                    if (srArmR != null) srArmR.sortingOrder = 9;
                    if (srLegL != null) srLegL.sortingOrder = 9;
                    if (srLegR != null) srLegR.sortingOrder = 9;
                    if (srBody != null) srBody.sortingOrder = 12;
                    if (srHead != null) srHead.sortingOrder = 14;
                    break;

                case Direction8.South:
                default:
                    // Nhìn Xuống (Chính diện): Thân ở dưới (10), Tay Chân ở trên Thân (11, 12), Đầu ở trên cùng (14)
                    if (srBody != null) srBody.sortingOrder = 10;
                    if (srLegL != null) srLegL.sortingOrder = 11;
                    if (srLegR != null) srLegR.sortingOrder = 11;
                    if (srArmL != null) srArmL.sortingOrder = 12;
                    if (srArmR != null) srArmR.sortingOrder = 12;
                    if (srHead != null) srHead.sortingOrder = 14;
                    break;
            }
        }

        private void SetRigPartsVisible(bool visible)
        {
            if (srHairFront != null) srHairFront.enabled = visible;
            if (srHairBack  != null) srHairBack.enabled  = visible;
            if (srHead      != null) srHead.enabled      = visible;
            if (srArmL      != null) srArmL.enabled      = visible;
            if (srArmR      != null) srArmR.enabled      = visible;
            if (srLegL      != null) srLegL.enabled      = visible;
            if (srLegR      != null) srLegR.enabled      = visible;
        }

        // ════════════════════════════════════════════════════════════════
        //  UPDATE
        // ════════════════════════════════════════════════════════════════
        private Direction8 prevDir    = Direction8.South;
        private bool       prevFemale = false;
        private SkinType   prevSkin   = SkinType.None;

        private void Update()
        {
            HandleHotkeys();
            UpdateFacingDirection();

            if (FacingDir != prevDir || IsFemale != prevFemale || CurrentSkin != prevSkin)
            {
                prevDir    = FacingDir;
                prevFemale = IsFemale;
                prevSkin   = CurrentSkin;
                LoadDirectionSprites();
            }

            SyncEquippedWeapon();

            if (isDying)        { AnimateDeath(); return; }
            if (hurtTimer > 0f) { AnimateHurt(); return; }

            bool isDashing = movement != null && movement.IsDashing;
            bool isMoving  = (movement != null && movement.IsMoving)
                          || (rb != null && rb.linearVelocity.sqrMagnitude > 0.04f);
            bool isRunning = movement != null && movement.IsRunning;

            if (isDashing)       AnimateDash();
            else if (isMoving)   AnimateWalkRun(isRunning);
            else                 AnimateIdle();

            if (!isAttacking)
            {
                UpdateWeaponOrbit(isMoving);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  IDLE — Thở Nhẹ Squash & Stretch Tự Nhiên
        // ════════════════════════════════════════════════════════════════
        private void AnimateIdle()
        {
            cycleTimer   = 0f;
            breathTimer += Time.deltaTime * 2.2f;
            float breath = Mathf.Sin(breathTimer);
            float squash = breath * 0.012f;
            float stretch= breath * 0.018f;

            partRoot.localPosition = rootOrigin + new Vector3(0f, breath * 0.005f, 0f);
            partRoot.localScale    = new Vector3(1f - squash, 1f + stretch, 1f);
            partRoot.localRotation = Quaternion.identity;

            // Đầu và tóc đung đưa nhẹ theo nhịp thở
            jointHead.localRotation = Quaternion.Euler(0f, 0f, breath * 1.5f);

            // Hai tay đung đưa nhẹ nhàng
            float armF = Mathf.Sin(breathTimer * 1.1f) * 2.5f;
            jointArmLeft.localRotation  = Quaternion.Euler(0f, 0f,  armF);
            jointArmRight.localRotation = Quaternion.Euler(0f, 0f, -armF);

            // Hai chân đứng yên
            jointLegLeft.localRotation  = Quaternion.identity;
            jointLegRight.localRotation = Quaternion.identity;
            jointBody.localRotation     = Quaternion.identity;

            ghostTimer = 0f;
        }

        // ════════════════════════════════════════════════════════════════
        //  WALK / RUN — Khớp Xương Xoay Nhịp Nhàng 60fps
        // ════════════════════════════════════════════════════════════════
        private void AnimateWalkRun(bool run)
        {
            float speed   = run ? 13.5f : 8.5f;
            float legAng  = run ? 36f   : 22f;
            float armAng  = run ? 28f   : 18f;
            float bobAmt  = run ? 0.035f: 0.018f;
            float leanAmt = run ? 2.5f  : 1.2f;

            cycleTimer  += Time.deltaTime * speed;
            breathTimer += Time.deltaTime * 0.4f;

            float sinT   = Mathf.Sin(cycleTimer);
            float absSin = Mathf.Abs(sinT);

            // Thân: nhấp nhô theo bước chân + nghiêng theo gia tốc
            partRoot.localPosition = rootOrigin + new Vector3(0f, absSin * bobAmt, 0f);
            partRoot.localScale    = Vector3.one;
            partRoot.localRotation = Quaternion.identity;
            jointBody.localRotation= Quaternion.Euler(0f, 0f, sinT * leanAmt);

            // Khớp Chân: xoay so le quanh khớp hông Hip Joint
            jointLegLeft.localRotation  = Quaternion.Euler(0f, 0f,  sinT * legAng);
            jointLegRight.localRotation = Quaternion.Euler(0f, 0f, -sinT * legAng);

            // Khớp Tay: counter-swing so le đối xứng với bước chân
            jointArmLeft.localRotation  = Quaternion.Euler(0f, 0f, -sinT * armAng);
            jointArmRight.localRotation = Quaternion.Euler(0f, 0f,  sinT * armAng);

            // Khớp Đầu: chống rung nhẹ (Anti-bobbing counter-tilt)
            jointHead.localRotation = Quaternion.Euler(0f, 0f, -sinT * (run ? 1.8f : 0.8f));

            // Hiệu ứng bóng mờ (Ghost Trail) khi chạy nhanh (Sprint)
            if (run)
            {
                ghostTimer += Time.deltaTime;
                if (ghostTimer > 0.10f)
                {
                    ghostTimer = 0f;
                    SpawnGhost();
                }
            }
            else ghostTimer = 0f;
        }

        // ════════════════════════════════════════════════════════════════
        //  DASH — Lướt Ép Dẹp Khí Động Học + Ghost Trail
        // ════════════════════════════════════════════════════════════════
        private void AnimateDash()
        {
            partRoot.localScale    = new Vector3(1.18f, 0.86f, 1f);
            partRoot.localPosition = rootOrigin;
            partRoot.localRotation = Quaternion.identity;

            jointHead.localRotation     = Quaternion.identity;
            jointArmLeft.localRotation  = Quaternion.identity;
            jointArmRight.localRotation = Quaternion.identity;
            jointLegLeft.localRotation  = Quaternion.identity;
            jointLegRight.localRotation = Quaternion.identity;
            jointBody.localRotation     = Quaternion.identity;

            ghostTimer += Time.deltaTime;
            if (ghostTimer > 0.04f)
            {
                ghostTimer = 0f;
                SpawnGhost();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ATTACK — Vung Vũ Khí & Vệt Đao Khí Nguyên Tố (Elemental Slash)
        // ════════════════════════════════════════════════════════════════
        public void TriggerAttack(string element = "Hỏa", bool isCast = false)
        {
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(AttackRoutine(element, isCast));
        }

        private IEnumerator AttackRoutine(string element, bool isCast)
        {
            isAttacking = true;

            string cleanElem = element?.ToLower().Trim() ?? "fire";
            if      (cleanElem.Contains("hỏa") || cleanElem.Contains("fire"))                          cleanElem = "fire";
            else if (cleanElem.Contains("băng") || cleanElem.Contains("ice"))                           cleanElem = "ice";
            else if (cleanElem.Contains("lôi") || cleanElem.Contains("thunder") || cleanElem.Contains("lightning")) cleanElem = "thunder";
            else if (cleanElem.Contains("phong") || cleanElem.Contains("wind"))                         cleanElem = "wind";
            else if (cleanElem.Contains("quang") || cleanElem.Contains("light") || cleanElem.Contains("holy")) cleanElem = "holy";
            else if (cleanElem.Contains("ám")   || cleanElem.Contains("dark"))                          cleanElem = "dark";
            else if (cleanElem.Contains("mộc") || cleanElem.Contains("nature") || cleanElem.Contains("wood")) cleanElem = "nature";
            else cleanElem = "fire";

            bool isFacingLeft = (FacingDir == Direction8.West || FacingDir == Direction8.NorthWest || FacingDir == Direction8.SouthWest);
            float swingDir = isFacingLeft ? -75f : 75f;

            // 1. Vung khớp tay cầm vũ khí & xoay vũ khí
            Quaternion origArmRot = jointArmRight.localRotation;
            Quaternion origWepRot = jointWeapon.localRotation;

            jointArmRight.localRotation = origArmRot * Quaternion.Euler(0, 0, swingDir * 0.7f);
            jointWeapon.localRotation   = origWepRot * Quaternion.Euler(0, 0, swingDir);

            // 2. Định vị Vệt Chém Nguyên Tố theo đúng 8 hướng
            if (srSlash != null)
            {
                Vector3 slashPos = new Vector3(0.42f, 0.25f, 0f);
                float slashRot   = 0f;
                bool slashFlip   = false;

                switch (FacingDir)
                {
                    case Direction8.West:
                    case Direction8.SouthWest:
                    case Direction8.NorthWest:
                        slashPos = new Vector3(-0.45f, 0.25f, 0f);
                        slashFlip = true;
                        break;
                    case Direction8.South:
                        slashPos = new Vector3(0.10f, -0.15f, 0f);
                        slashRot = -90f;
                        break;
                    case Direction8.North:
                        slashPos = new Vector3(0.10f, 0.55f, 0f);
                        slashRot = 90f;
                        break;
                    case Direction8.SouthEast:
                        slashPos = new Vector3(0.40f, 0.05f, 0f);
                        slashRot = -35f;
                        break;
                    case Direction8.NorthEast:
                        slashPos = new Vector3(0.40f, 0.45f, 0f);
                        slashRot = 35f;
                        break;
                    default:
                        slashPos = new Vector3(0.45f, 0.25f, 0f);
                        break;
                }

                jointSlash.localPosition = slashPos;
                jointSlash.localRotation = Quaternion.Euler(0, 0, slashRot);
                srSlash.flipX            = slashFlip;
                srSlash.sortingOrder     = 18;
                srSlash.enabled          = true;

                for (int f = 0; f < 3; f++)
                {
                    srSlash.sprite = LoadSpr($"Characters/vfx_slash_{cleanElem}_{f}");
                    yield return new WaitForSeconds(0.045f);
                }
                srSlash.enabled = false;
            }
            else
            {
                yield return new WaitForSeconds(0.12f);
            }

            jointArmRight.localRotation = origArmRot;
            jointWeapon.localRotation   = origWepRot;
            isAttacking = false;
        }

        // ════════════════════════════════════════════════════════════════
        //  HURT — Recoil Giật Lùi & Chớp Đỏ
        // ════════════════════════════════════════════════════════════════
        public void TriggerHurt()
        {
            hurtTimer = 0.55f;
        }

        private void AnimateHurt()
        {
            hurtTimer -= Time.deltaTime;
            float flash = Mathf.PingPong(hurtTimer * 14f, 1f);
            Color c     = Color.Lerp(Color.white, new Color(1f, 0.25f, 0.25f), flash);

            foreach (var sr in new[] { srHairFront, srHead, srBody, srArmL, srArmR, srLegL, srLegR, srCostume })
                if (sr != null) sr.color = c;

            float jerk = Mathf.Sin(hurtTimer * 20f) * 0.04f;
            partRoot.localPosition = rootOrigin + new Vector3(-jerk, 0f, 0f);
            partRoot.localRotation = Quaternion.Euler(0f, 0f, jerk * 80f);

            if (hurtTimer <= 0f)
            {
                foreach (var sr in new[] { srHairFront, srHead, srBody, srArmL, srArmR, srLegL, srLegR, srCostume })
                    if (sr != null) sr.color = Color.white;
                partRoot.localRotation = Quaternion.identity;
                partRoot.localPosition = rootOrigin;
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  DIE — Ngã Lăn 90° & Tan Biến Dần
        // ════════════════════════════════════════════════════════════════
        public void TriggerDie()
        {
            isDying  = true;
            dieTimer = 0f;
        }

        private void AnimateDeath()
        {
            dieTimer += Time.deltaTime;
            float t = Mathf.Clamp01(dieTimer / 0.8f);
            float fallAngle = Mathf.Lerp(0f, -90f, Mathf.SmoothStep(0f, 1f, t));
            partRoot.localRotation = Quaternion.Euler(0f, 0f, fallAngle);
            partRoot.localPosition = rootOrigin + new Vector3(0f, Mathf.Lerp(0f, -0.12f, t), 0f);

            float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01((dieTimer - 0.6f) * 3f));
            Color c = new Color(1f, 1f, 1f, alpha);
            foreach (var sr in new[] { srHairFront, srHead, srBody, srArmL, srArmR, srLegL, srLegR, srHairBack, srWeapon, srCostume })
                if (sr != null) sr.color = c;
        }

        // ════════════════════════════════════════════════════════════════
        //  WEAPON TRACKING & ORBIT — Bám 8 Hướng & Chuột
        // ════════════════════════════════════════════════════════════════
        private void UpdateWeaponOrbit(bool isMoving)
        {
            if (srWeapon == null) return;

            Vector3 tOffset; float tAngle; bool flipX; int order;
            switch (FacingDir)
            {
                case Direction8.East:      tOffset=new Vector3( 0.40f, 0.22f,0); tAngle=-15f; flipX=false; order=16; break;
                case Direction8.West:      tOffset=new Vector3(-0.40f, 0.22f,0); tAngle= 15f; flipX=true;  order=16; break;
                case Direction8.South:     tOffset=new Vector3( 0.32f, 0.12f,0); tAngle=-22f; flipX=false; order=17; break;
                case Direction8.SouthEast: tOffset=new Vector3( 0.42f, 0.14f,0); tAngle=-32f; flipX=false; order=17; break;
                case Direction8.SouthWest: tOffset=new Vector3(-0.42f, 0.14f,0); tAngle= 32f; flipX=true;  order=17; break;
                case Direction8.North:     tOffset=new Vector3( 0.28f, 0.42f,0); tAngle= 18f; flipX=false; order=8;  break;
                case Direction8.NorthEast: tOffset=new Vector3( 0.38f, 0.40f,0); tAngle= 12f; flipX=false; order=8;  break;
                case Direction8.NorthWest: tOffset=new Vector3(-0.38f, 0.40f,0); tAngle=-12f; flipX=true;  order=8;  break;
                default:                   tOffset=new Vector3( 0.35f, 0.22f,0); tAngle=  0f; flipX=false; order=16; break;
            }

            if (mainCam != null)
            {
                Vector3 mw  = mainCam.ScreenToWorldPoint(GameInput.GetMousePosition());
                Vector2 aim = ((Vector2)mw - (Vector2)transform.position).normalized;
                float microtilt = Mathf.Sin(Mathf.Atan2(aim.y, aim.x)) * 10f;
                if (flipX) microtilt = -microtilt;
                tAngle += microtilt;
            }

            jointWeapon.localPosition  = Vector3.Lerp(jointWeapon.localPosition, tOffset, Time.deltaTime * 16f);
            jointWeapon.localRotation  = Quaternion.Slerp(jointWeapon.localRotation, Quaternion.Euler(0f, 0f, tAngle), Time.deltaTime * 16f);
            srWeapon.flipX            = flipX;
            srWeapon.sortingOrder     = order;
        }

        private void SyncEquippedWeapon()
        {
            if (inventoryManager == null) return;
            var wep = inventoryManager.GetEquippedGear(EquipSlot.Weapon);
            if (srWeapon != null)
            {
                srWeapon.enabled = (wep != null);
                if (wep != null && !string.IsNullOrEmpty(wep.iconName))
                {
                    Sprite s = LoadSpr(wep.iconName) ?? LoadSpr("item_weapon");
                    if (s != null) srWeapon.sprite = s;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  FACING DIRECTION
        // ════════════════════════════════════════════════════════════════
        private void UpdateFacingDirection()
        {
            if (mainCam == null) mainCam = Camera.main;
            Vector2 aimDir;

            if (movement != null && movement.IsMoving && rb != null && rb.linearVelocity.sqrMagnitude > 0.04f)
                aimDir = rb.linearVelocity.normalized;
            else if (mainCam != null)
            {
                Vector3 mw = mainCam.ScreenToWorldPoint(GameInput.GetMousePosition());
                aimDir = ((Vector2)mw - (Vector2)transform.position).normalized;
            }
            else return;

            FacingDir = Direction8Utils.VectorToDirection8(aimDir);
        }

        // ════════════════════════════════════════════════════════════════
        //  GHOST AFTERIMAGE
        // ════════════════════════════════════════════════════════════════
        private void SpawnGhost()
        {
            if (srBody == null || srBody.sprite == null) return;

            foreach (var (sr, t) in new[] {
                (srBody, jointBody),
                (srHead, jointHead)
            })
            {
                if (sr == null || sr.sprite == null || t == null) continue;
                var ghost = new GameObject("RigGhost");
                ghost.transform.SetPositionAndRotation(t.position, t.rotation);
                ghost.transform.localScale = t.lossyScale;
                var gsr = ghost.AddComponent<SpriteRenderer>();
                gsr.sprite       = sr.sprite;
                gsr.sortingOrder = 9;
                gsr.flipX        = sr.flipX;
                gsr.color        = new Color(0.25f, 0.85f, 1.0f, 0.35f);
                Destroy(ghost, 0.15f);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  STEALTH FADE (Alpha 0.35f)
        // ════════════════════════════════════════════════════════════════
        public void SetStealth(bool inStealth)
        {
            float a = inStealth ? 0.35f : 1.0f;
            foreach (var sr in new[] { srHairFront, srHairBack, srHead, srBody, srArmL, srArmR, srLegL, srLegR, srWeapon, srCostume })
            {
                if (sr == null) continue;
                var c = sr.color; c.a = a; sr.color = c;
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  AVATAR & FULL BODY TEXTURES
        // ════════════════════════════════════════════════════════════════
        public Texture2D GetPortraitTexture()
        {
            if (CurrentSkin == SkinType.SoulKnight)
                return Resources.Load<Texture2D>("Characters/avatar_soul_knight");
            if (CurrentSkin == SkinType.VoidReaper)
                return Resources.Load<Texture2D>("Characters/avatar_void_reaper");
            return Resources.Load<Texture2D>(IsFemale ? "Characters/avatar_female" : "Characters/avatar_male");
        }

        public Texture2D GetFullBodyTexture()
        {
            string genderStr = IsFemale ? "female" : "male";
            string dirName   = DirName(FacingDir);
            return Resources.Load<Texture2D>($"Characters/fullbody_{genderStr}")
                ?? Resources.Load<Texture2D>($"Characters/Rig/{genderStr}/{dirName}/body")
                ?? GetPortraitTexture();
        }

        // ════════════════════════════════════════════════════════════════
        //  HOTKEYS
        // ════════════════════════════════════════════════════════════════
        private void HandleHotkeys()
        {
            if (GodlessFantasy.UI.CombatHUD.Instance != null && GodlessFantasy.UI.CombatHUD.Instance.IsChatFocused()) return;

            if (Input.GetKeyDown(KeyCode.G))
            {
                IsFemale = !IsFemale;
                LoadDirectionSprites();
                string label = IsFemale ? "Nữ Anime (Croptop)" : "Nam Anime (Áo Ba Lỗ)";
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.3f,
                    $"🚻 {label} [G]", new Color(0.3f, 0.9f, 1f));
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                int nextSkin = ((int)CurrentSkin + 1) % 3;
                CurrentSkin = (SkinType)nextSkin;
                LoadDirectionSprites();
                string skinName = CurrentSkin == SkinType.SoulKnight ? "Hiệp Sĩ Soul Knight" :
                                 (CurrentSkin == SkinType.VoidReaper ? "Vu Sư Tử Linh (Void Reaper)" : "Phôi Gốc Xương 2D");
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f,
                    $"🎭 Ngoại Trang: {skinName} [K]", new Color(1f, 0.85f, 0.2f));
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════════
        private Transform GetOrCreate(Transform parent, string name)
        {
            var ex = parent.Find(name);
            if (ex != null) return ex;
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            obj.transform.localScale = Vector3.one;
            return obj.transform;
        }

        private SpriteRenderer EnsureSR(Transform t, int order)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        private Sprite LoadSpr(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (sprCache.TryGetValue(path, out var c) && c != null) return c;

            // 1. Thử Resources.Load<Sprite>
            var s = Resources.Load<Sprite>(path);

            // 2. Thử Resources.LoadAll<Sprite> (nếu texture Multiple)
            if (s == null)
            {
                var all = Resources.LoadAll<Sprite>(path);
                if (all != null && all.Length > 0) s = all[0];
            }

            // 3. Fallback Texture2D runtime Sprite.Create (PPU 32)
            if (s == null)
            {
                var tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                {
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.1f), 32f);
                }
            }

            if (s != null) sprCache[path] = s;
            return s;
        }

        private Sprite LoadRigSpr(string gender, string dir, string part)
        {
            return LoadSpr($"Characters/Rig/{gender}/{dir}/{part}");
        }

        private string DirName(Direction8 d)
        {
            switch (d)
            {
                case Direction8.South:     return "south";
                case Direction8.SouthEast: return "south_east";
                case Direction8.East:      return "east";
                case Direction8.NorthEast: return "north_east";
                case Direction8.North:     return "north";
                case Direction8.NorthWest: return "north_west";
                case Direction8.West:      return "west";
                case Direction8.SouthWest: return "south_west";
                default:                   return "south";
            }
        }

        private string FlipDir(Direction8 d)
        {
            switch (d)
            {
                case Direction8.West:      return "east";
                case Direction8.NorthWest: return "north_east";
                case Direction8.SouthWest: return "south_east";
                default:                   return DirName(d);
            }
        }
    }
}
