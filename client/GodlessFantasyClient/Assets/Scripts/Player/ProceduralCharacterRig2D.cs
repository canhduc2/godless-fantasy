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
    // ─── CONFIGURATION SCHEMAS (rig_config.json & animation_config.json) ───
    [System.Serializable]
    public class PartConfig
    {
        public string file;
        public int x;
        public int y;
        public int z = 10;
        public bool visible = true;
        public bool flip_x = false;
        public bool flip_y = false;
    }

    [System.Serializable]
    public class DirectionConfig
    {
        public PartConfig idle_head;
        public PartConfig idle_body;
        public PartConfig idle_arm_left;
        public PartConfig idle_arm_right;
        public PartConfig idle_leg_left;
        public PartConfig idle_leg_right;
    }

    [System.Serializable]
    public class GenderConfig
    {
        public DirectionConfig south;
        public DirectionConfig south_east;
        public DirectionConfig east;
        public DirectionConfig north_east;
        public DirectionConfig north;
        public DirectionConfig north_west;
        public DirectionConfig west;
        public DirectionConfig south_west;

        public DirectionConfig GetDir(Direction8 d)
        {
            switch (d)
            {
                case Direction8.North:     return north;
                case Direction8.NorthEast: return north_east;
                case Direction8.East:      return east;
                case Direction8.SouthEast: return south_east;
                case Direction8.South:     return south;
                case Direction8.SouthWest: return south_west;
                case Direction8.West:      return west;
                case Direction8.NorthWest: return north_west;
                default:                   return south;
            }
        }
    }

    [System.Serializable]
    public class FullRigConfig
    {
        public GenderConfig female;
        public GenderConfig male;
    }

    [System.Serializable]
    public class AnimStateData
    {
        public float speed = 1f;
        public float bobAmount = 0.5f;
        public float squashAmount = 0f;
        public float stretchAmount = 0f;
        public float headTiltAngle = 0f;
        public float armSwingAngle = 0f;
        public float bodyLeanAngle = 0f;
        public float legSwingAngle = 0f;
    }

    [System.Serializable]
    public class RigAnimationConfig
    {
        public AnimStateData idle;
        public AnimStateData walk;
        public AnimStateData run;
        public AnimStateData dash;
        public AnimStateData attack;
        public AnimStateData hurt;
    }

    public class ProceduralCharacterRig2D : MonoBehaviour
    {
        public static ProceduralCharacterRig2D Instance { get; private set; }

        // ─── DYNAMIC CONFIGURATIONS LOADED FROM RESOURCES ───────────────
        [Header("Cấu Hình Động (Nạp từ Resources)")]
        public FullRigConfig      fullRigConfig;
        public RigAnimationConfig animConfig;

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

        // Override hỗ trợ Multiplayer Entity từ xa
        public bool? RemoteIsMovingOverride = null;
        public Direction8? RemoteFacingDirOverride = null;

        public void SetGender(bool female) => ForceSetGender(female);

        // ─── REFERENCES ─────────────────────────────────────────────────
        private Rigidbody2D      rb;
        private PlayerMovement   movement;
        private InventoryManager inventoryManager;
        private Camera           mainCam;

        // ─── RIG ROOT & JOINTS ──────────────────────────────────────────
        private Transform partRoot;
        private Transform partShadow;

        public Transform PartRoot => partRoot;
        public Transform PartShadow => partShadow;

        private Transform jointBody;
        private Transform jointHead;
        private Transform jointArmLeft;
        private Transform jointArmRight;
        private Transform jointLegLeft;
        private Transform jointLegRight;
        private Transform jointWeapon;
        private Transform jointSlash;
        private Transform jointCostume;

        // Visual offset transforms (con của từng khớp, bù trừ pivot để ảnh gốc đặt đúng toạ độ pixel)
        private Transform sprBodyObj;
        private Transform sprHeadObj;
        private Transform sprHairFrontObj;
        private Transform sprHairBackObj;
        private Transform sprArmLObj;
        private Transform sprArmRObj;
        private Transform sprLegLObj;
        private Transform sprLegRObj;

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

        // ─── WEARABLE OVERLAYS (Trang Phục & Giáp Độc Lập Bám Khớp Xương) ────
        private SpriteRenderer srWearableHelmet;
        private SpriteRenderer srWearableArmor;
        private SpriteRenderer srWearablePants;
        private SpriteRenderer srWearableBootsL;
        private SpriteRenderer srWearableBootsR;
        private readonly Dictionary<EquipSlot, Sprite> customWearableOverrides = new Dictionary<EquipSlot, Sprite>();

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

        // ─── JOINT PIVOTS (Mặc định Fallback trước khi nạp cấu hình) ───────
        private static readonly Vector3 DEFAULT_PIVOT_HEAD  = new Vector3( 0.00f, 0.48f, 0f);
        private static readonly Vector3 DEFAULT_PIVOT_ARM_L = new Vector3(-0.16f, 0.38f, 0f);
        private static readonly Vector3 DEFAULT_PIVOT_ARM_R = new Vector3( 0.16f, 0.38f, 0f);
        private static readonly Vector3 DEFAULT_PIVOT_LEG_L = new Vector3(-0.08f, 0.18f, 0f);
        private static readonly Vector3 DEFAULT_PIVOT_LEG_R = new Vector3( 0.08f, 0.18f, 0f);
        private static readonly Vector3 DEFAULT_PIVOT_BODY  = new Vector3( 0.00f, 0.28f, 0f);

        // ════════════════════════════════════════════════════════════════
        //  AWAKE & SETUP
        // ════════════════════════════════════════════════════════════════
        private void Awake()
        {
            Instance         = this;
            sprCache.Clear();
            rb               = GetComponent<Rigidbody2D>();
            movement         = GetComponent<PlayerMovement>();
            inventoryManager = GetComponent<InventoryManager>();
            mainCam          = Camera.main;

            if (rb != null)
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Vô hiệu hóa SpriteRenderer cũ trên root (nếu có)
            var oldSr = GetComponent<SpriteRenderer>();
            if (oldSr != null) oldSr.enabled = false;

            LoadConfigs();
            BuildRigHierarchy();
        }

        /// <summary>
        /// Nạp toàn bộ cấu hình toạ độ khớp xương và thông số hoạt ảnh từ Unity Resources (rig_config.json & animation_config.json)
        /// Tuyệt đối tin tưởng cấu hình nạp vào, không hardcode thông số.
        /// </summary>
        public void LoadConfigs()
        {
            var rigText = Resources.Load<TextAsset>("Characters/Rig/rig_config");
            if (rigText != null && !string.IsNullOrEmpty(rigText.text))
            {
                try { fullRigConfig = JsonUtility.FromJson<FullRigConfig>(rigText.text); }
                catch (System.Exception e) { Debug.LogWarning($"[ProceduralCharacterRig2D] Lỗi đọc rig_config: {e.Message}"); }
            }

            var animText = Resources.Load<TextAsset>("Characters/Rig/animation_config");
            if (animText != null && !string.IsNullOrEmpty(animText.text))
            {
                try { animConfig = JsonUtility.FromJson<RigAnimationConfig>(animText.text); }
                catch (System.Exception e) { Debug.LogWarning($"[ProceduralCharacterRig2D] Lỗi đọc animation_config: {e.Message}"); }
            }
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
            jointBody = CreateJoint("Joint_Body", DEFAULT_PIVOT_BODY);
            sprBodyObj = CreateOffsetVisual(jointBody, "Sprite_Body", -DEFAULT_PIVOT_BODY);
            srBody = EnsureSR(sprBodyObj, 11);

            // Khớp Đầu & Tóc (Xoay quanh cổ)
            jointHead = CreateJoint("Joint_Head", DEFAULT_PIVOT_HEAD);
            sprHairBackObj  = CreateOffsetVisual(jointHead, "Sprite_HairBack",  -DEFAULT_PIVOT_HEAD);
            sprHeadObj      = CreateOffsetVisual(jointHead, "Sprite_Head",      -DEFAULT_PIVOT_HEAD);
            sprHairFrontObj = CreateOffsetVisual(jointHead, "Sprite_HairFront", -DEFAULT_PIVOT_HEAD);
            srHairBack  = EnsureSR(sprHairBackObj,  8);
            srHead      = EnsureSR(sprHeadObj,      14);
            srHairFront = EnsureSR(sprHairFrontObj, 15);

            // Khớp Tay Trái & Tay Phải (Xoay quanh vai)
            jointArmLeft  = CreateJoint("Joint_ArmLeft",  DEFAULT_PIVOT_ARM_L);
            jointArmRight = CreateJoint("Joint_ArmRight", DEFAULT_PIVOT_ARM_R);
            sprArmLObj = CreateOffsetVisual(jointArmLeft,  "Sprite_ArmL", -DEFAULT_PIVOT_ARM_L);
            sprArmRObj = CreateOffsetVisual(jointArmRight, "Sprite_ArmR", -DEFAULT_PIVOT_ARM_R);
            srArmL = EnsureSR(sprArmLObj, 12);
            srArmR = EnsureSR(sprArmRObj, 13);

            // Khớp Chân Trái & Chân Phải (Xoay quanh hông)
            jointLegLeft  = CreateJoint("Joint_LegLeft",  DEFAULT_PIVOT_LEG_L);
            jointLegRight = CreateJoint("Joint_LegRight", DEFAULT_PIVOT_LEG_R);
            sprLegLObj = CreateOffsetVisual(jointLegLeft,  "Sprite_LegL", -DEFAULT_PIVOT_LEG_L);
            sprLegRObj = CreateOffsetVisual(jointLegRight, "Sprite_LegR", -DEFAULT_PIVOT_LEG_R);
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

            // Wearable Overlays: Mũ (Head), Áo (Body), Quần (Body/Hông), Giày (Chân)
            var helmetObj = CreateOffsetVisual(jointHead, "Wearable_Helmet", -DEFAULT_PIVOT_HEAD);
            srWearableHelmet = EnsureSR(helmetObj, 16);
            srWearableHelmet.enabled = false;

            var armorObj = CreateOffsetVisual(jointBody, "Wearable_Armor", -DEFAULT_PIVOT_BODY);
            srWearableArmor = EnsureSR(armorObj, 12);
            srWearableArmor.enabled = false;

            var pantsObj = CreateOffsetVisual(jointBody, "Wearable_Pants", -DEFAULT_PIVOT_BODY);
            srWearablePants = EnsureSR(pantsObj, 11);
            srWearablePants.enabled = false;

            var bootLObj = CreateOffsetVisual(jointLegLeft, "Wearable_BootsL", -DEFAULT_PIVOT_LEG_L);
            var bootRObj = CreateOffsetVisual(jointLegRight, "Wearable_BootsR", -DEFAULT_PIVOT_LEG_R);
            srWearableBootsL = EnsureSR(bootLObj, 10);
            srWearableBootsR = EnsureSR(bootRObj, 10);
            srWearableBootsL.enabled = false;
            srWearableBootsR.enabled = false;

            LoadDirectionSprites();
        }

        public void SetDirectWearableOverride(EquipSlot slot, Sprite spr)
        {
            customWearableOverrides[slot] = spr;
            ApplyWearableSprite(slot, spr);
        }

        public void ClearWearableOverrides()
        {
            customWearableOverrides.Clear();
            SyncEquippedWearables();
        }

        public void ApplyWearableSprite(EquipSlot slot, Sprite spr)
        {
            switch (slot)
            {
                case EquipSlot.Helmet:
                    if (srWearableHelmet != null)
                    {
                        srWearableHelmet.sprite = spr;
                        srWearableHelmet.enabled = (spr != null);
                    }
                    break;
                case EquipSlot.Armor:
                    if (srWearableArmor != null)
                    {
                        srWearableArmor.sprite = spr;
                        srWearableArmor.enabled = (spr != null);
                    }
                    break;
                case EquipSlot.Pants:
                    if (srWearablePants != null)
                    {
                        srWearablePants.sprite = spr;
                        srWearablePants.enabled = (spr != null);
                    }
                    break;
                case EquipSlot.Boots:
                    if (srWearableBootsL != null)
                    {
                        srWearableBootsL.sprite = spr;
                        srWearableBootsL.enabled = (spr != null);
                    }
                    if (srWearableBootsR != null)
                    {
                        srWearableBootsR.sprite = spr;
                        srWearableBootsR.enabled = (spr != null);
                    }
                    break;
                case EquipSlot.Costume:
                    if (srCostume != null)
                    {
                        srCostume.sprite = spr;
                        srCostume.enabled = (spr != null);
                    }
                    break;
            }
        }

        public void SyncEquippedWearables()
        {
            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>() ?? FindAnyObjectByType<InventoryManager>();
            if (inventoryManager == null) return;

            EquipSlot[] checkSlots = { EquipSlot.Helmet, EquipSlot.Armor, EquipSlot.Pants, EquipSlot.Boots, EquipSlot.Costume };
            foreach (var slot in checkSlots)
            {
                if (customWearableOverrides.TryGetValue(slot, out Sprite sprOverride) && sprOverride != null)
                {
                    ApplyWearableSprite(slot, sprOverride);
                    continue;
                }

                var item = inventoryManager.GetEquippedItem(slot);
                if (item != null)
                {
                    Sprite spr = item.icon != null ? item.icon : Resources.Load<Sprite>(item.iconName);
                    ApplyWearableSprite(slot, spr);
                }
                else
                {
                    ApplyWearableSprite(slot, null);
                }
            }
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
            UpdateJointPivots(FacingDir);
        }

        /// <summary>
        /// Tính toán toạ độ xoay của các khớp xương hoàn toàn tự động từ toạ độ (x, y) của cấu hình rig_config.json.
        /// Tuyệt đối tin tưởng cấu hình nạp vào, không tự chỉnh sửa hay fix cứng thông số.
        /// </summary>
        public void UpdateJointPivots(Direction8 d)
        {
            if (fullRigConfig == null) return;
            var gConfig = IsFemale ? fullRigConfig.female : fullRigConfig.male;
            if (gConfig == null) return;
            var dConfig = gConfig.GetDir(d);
            if (dConfig == null) return;

            // Texture 64x64, PPU = 32, SpritePivot = (0.5f, 0.1f) => (32px từ trái, 6.4px từ đáy ảnh)
            // Toạ độ Unity tương đối với RigRoot (tâm 0,0 là điểm chân):
            Vector3 PixelToUnity(float px, float py)
            {
                float ux = (px - 32.0f) / 32.0f;
                float uy = ((64.0f - py) - 6.4f) / 32.0f;
                return new Vector3(ux, uy, 0f);
            }

            // 1. Khớp Cổ / Đầu: neo ở điểm nối giữa đầu và thân
            if (dConfig.idle_head != null)
            {
                Vector3 pHead = PixelToUnity(dConfig.idle_head.x + 8f, dConfig.idle_head.y + 19f);
                SetJointPivot(jointHead, sprHeadObj, pHead);
                if (sprHairBackObj != null) sprHairBackObj.localPosition = -pHead;
                if (sprHairFrontObj != null) sprHairFrontObj.localPosition = -pHead;
            }

            // 2. Khớp Vai Trái & Khớp Vai Phải: neo ở đầu khớp cánh tay
            if (dConfig.idle_arm_left != null)
            {
                Vector3 pArmL = PixelToUnity(dConfig.idle_arm_left.x + 3f, dConfig.idle_arm_left.y + 2f);
                SetJointPivot(jointArmLeft, sprArmLObj, pArmL);
            }
            if (dConfig.idle_arm_right != null)
            {
                Vector3 pArmR = PixelToUnity(dConfig.idle_arm_right.x + 3f, dConfig.idle_arm_right.y + 2f);
                SetJointPivot(jointArmRight, sprArmRObj, pArmR);
            }

            // 3. Khớp Hông Trái & Khớp Hông Phải: neo ở đầu khớp đùi
            if (dConfig.idle_leg_left != null)
            {
                Vector3 pLegL = PixelToUnity(dConfig.idle_leg_left.x + 3f, dConfig.idle_leg_left.y + 2f);
                SetJointPivot(jointLegLeft, sprLegLObj, pLegL);
            }
            if (dConfig.idle_leg_right != null)
            {
                Vector3 pLegR = PixelToUnity(dConfig.idle_leg_right.x + 3f, dConfig.idle_leg_right.y + 2f);
                SetJointPivot(jointLegRight, sprLegRObj, pLegR);
            }

            // 4. Khớp Thân: neo ở trọng tâm eo / hông
            if (dConfig.idle_body != null)
            {
                Vector3 pBody = PixelToUnity(dConfig.idle_body.x + 6f, dConfig.idle_body.y + 12f);
                SetJointPivot(jointBody, sprBodyObj, pBody);
            }
        }

        private void SetJointPivot(Transform joint, Transform visual, Vector3 pivotPos)
        {
            if (joint == null || visual == null) return;
            joint.localPosition = pivotPos;
            visual.localPosition = -pivotPos;
        }

        private void UpdateSortingOrders(Direction8 d)
        {
            // Tuyệt đối tin tưởng cấu hình rig_config.json được nạp vào
            if (fullRigConfig != null)
            {
                var gConfig = IsFemale ? fullRigConfig.female : fullRigConfig.male;
                if (gConfig != null)
                {
                    var dConfig = gConfig.GetDir(d);
                    if (dConfig != null)
                    {
                        if (srHead != null && dConfig.idle_head != null)
                        {
                            srHead.sortingOrder = dConfig.idle_head.z;
                            srHead.enabled = dConfig.idle_head.visible;
                        }
                        if (srBody != null && dConfig.idle_body != null)
                        {
                            srBody.sortingOrder = dConfig.idle_body.z;
                            srBody.enabled = dConfig.idle_body.visible;
                        }
                        if (srArmL != null && dConfig.idle_arm_left != null)
                        {
                            srArmL.sortingOrder = dConfig.idle_arm_left.z;
                            srArmL.enabled = dConfig.idle_arm_left.visible;
                        }
                        if (srArmR != null && dConfig.idle_arm_right != null)
                        {
                            srArmR.sortingOrder = dConfig.idle_arm_right.z;
                            srArmR.enabled = dConfig.idle_arm_right.visible;
                        }
                        if (srLegL != null && dConfig.idle_leg_left != null)
                        {
                            srLegL.sortingOrder = dConfig.idle_leg_left.z;
                            srLegL.enabled = dConfig.idle_leg_left.visible;
                        }
                        if (srLegR != null && dConfig.idle_leg_right != null)
                        {
                            srLegR.sortingOrder = dConfig.idle_leg_right.z;
                            srLegR.enabled = dConfig.idle_leg_right.visible;
                        }
                        return;
                    }
                }
            }

            // Fallback nếu chưa nạp được cấu hình
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
            SyncEquippedCostume();

            if (isDying)        { AnimateDeath(); return; }
            if (hurtTimer > 0f) { AnimateHurt(); return; }

            bool isDashing = movement != null && movement.IsDashing;
            bool isMoving  = RemoteIsMovingOverride ?? ((movement != null && movement.IsMoving)
                          || (rb != null && rb.linearVelocity.sqrMagnitude > 0.04f));
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
        //  IDLE — Thở Nhẹ Squash & Stretch Tự Nhiên (Nạp 100% từ animConfig)
        // ════════════════════════════════════════════════════════════════
        private void AnimateIdle()
        {
            cycleTimer = 0f;
            var c = animConfig?.idle;
            float speed      = c != null ? c.speed : 2.2f;
            float squash     = c != null ? c.squashAmount : 0.012f;
            float stretch    = c != null ? c.stretchAmount : 0.018f;
            float bobPixels  = c != null ? c.bobAmount : 0.16f;
            float headAng    = c != null ? c.headTiltAngle : 1.5f;
            float armAng     = c != null ? c.armSwingAngle : 2.5f;
            float leanAng    = c != null ? c.bodyLeanAngle : 0f;
            float legAng     = c != null ? c.legSwingAngle : 0f;

            breathTimer += Time.deltaTime * speed;
            float breath = Mathf.Sin(breathTimer);

            // 1 pixel nhấp nhô = 1/32 đơn vị Unity PPU
            float bobUnity = (bobPixels / 32f) * breath;
            partRoot.localPosition = rootOrigin + new Vector3(0f, bobUnity, 0f);
            partRoot.localScale    = new Vector3(1f - breath * squash, 1f + breath * stretch, 1f);
            partRoot.localRotation = Quaternion.Euler(0f, 0f, breath * leanAng);

            // Đầu và tóc đung đưa theo cấu hình
            jointHead.localRotation = Quaternion.Euler(0f, 0f, breath * headAng);

            // Hai tay đung đưa theo cấu hình
            float armF = Mathf.Sin(breathTimer * 1.1f) * armAng;
            jointArmLeft.localRotation  = Quaternion.Euler(0f, 0f,  armF);
            jointArmRight.localRotation = Quaternion.Euler(0f, 0f, -armF);

            // Khớp chân (nếu có góc cài đặt trong cấu hình)
            if (Mathf.Abs(legAng) > 0.001f)
            {
                jointLegLeft.localRotation  = Quaternion.Euler(0f, 0f,  breath * legAng);
                jointLegRight.localRotation = Quaternion.Euler(0f, 0f, -breath * legAng);
            }
            else
            {
                jointLegLeft.localRotation  = Quaternion.identity;
                jointLegRight.localRotation = Quaternion.identity;
            }
            jointBody.localRotation     = Quaternion.identity;

            ghostTimer = 0f;
        }

        // ════════════════════════════════════════════════════════════════
        //  WALK / RUN — Khớp Xương Xoay Nhịp Nhàng 60fps (Nạp 100% từ animConfig)
        // ════════════════════════════════════════════════════════════════
        private void AnimateWalkRun(bool run)
        {
            var c = animConfig != null ? (run ? animConfig.run : animConfig.walk) : null;
            float speed     = c != null ? c.speed : (run ? 13.5f : 8.5f);
            float legAng    = c != null ? c.legSwingAngle : (run ? 36f : 22f);
            float armAng    = c != null ? c.armSwingAngle : (run ? 28f : 18f);
            float bobPixels = c != null ? c.bobAmount : (run ? 1.1f : 0.6f);
            float leanAmt   = c != null ? c.bodyLeanAngle : (run ? 2.5f : 1.2f);
            float headAng   = c != null ? c.headTiltAngle : (run ? 1.8f : 0.8f);
            float squash    = c != null ? c.squashAmount : 0.02f;
            float stretch   = c != null ? c.stretchAmount : 0.02f;

            cycleTimer  += Time.deltaTime * speed;
            breathTimer += Time.deltaTime * 0.4f;

            float sinT   = Mathf.Sin(cycleTimer);
            float absSin = Mathf.Abs(sinT);

            // Nhấp nhô lên xuống chuẩn theo bobAmount từ cấu hình
            float bobUnity = (bobPixels / 32f) * absSin;
            partRoot.localPosition = rootOrigin + new Vector3(0f, bobUnity, 0f);
            partRoot.localScale    = new Vector3(1f - absSin * squash, 1f + absSin * stretch, 1f);
            partRoot.localRotation = Quaternion.identity;
            jointBody.localRotation= Quaternion.Euler(0f, 0f, sinT * leanAmt);

            // Khớp Chân: xoay so le quanh khớp hông từ cấu hình
            jointLegLeft.localRotation  = Quaternion.Euler(0f, 0f,  sinT * legAng);
            jointLegRight.localRotation = Quaternion.Euler(0f, 0f, -sinT * legAng);

            // Khớp Tay: counter-swing so le đối xứng với bước chân từ cấu hình
            jointArmLeft.localRotation  = Quaternion.Euler(0f, 0f, -sinT * armAng);
            jointArmRight.localRotation = Quaternion.Euler(0f, 0f,  sinT * armAng);

            // Khớp Đầu: gật/ngửa theo cấu hình
            jointHead.localRotation = Quaternion.Euler(0f, 0f, -sinT * headAng);

            // Hiệu ứng bóng mờ (Ghost Trail) khi chạy nhanh (Sprint)
            if (run)
            {
                ghostTimer += Time.deltaTime;
                if (ghostTimer > 0.12f)
                {
                    ghostTimer = 0f;
                    SpawnGhost();
                }
            }
            else ghostTimer = 0f;
        }

        // ════════════════════════════════════════════════════════════════
        //  DASH — Lướt Ép Dẹp Khí Động Học + Ghost Trail (Nạp 100% từ animConfig)
        // ════════════════════════════════════════════════════════════════
        private void AnimateDash()
        {
            var c = animConfig?.dash;
            float squash    = c != null ? c.squashAmount : 0.14f;
            float stretch   = c != null ? c.stretchAmount : -0.18f;
            float lean      = c != null ? c.bodyLeanAngle : -12f;
            float headAng   = c != null ? c.headTiltAngle : -5f;
            float armAng    = c != null ? c.armSwingAngle : -40f;
            float legAng    = c != null ? c.legSwingAngle : 25f;
            float bobPixels = c != null ? c.bobAmount : 0f;

            partRoot.localScale    = new Vector3(1f + squash, 1f + stretch, 1f);
            partRoot.localPosition = rootOrigin + new Vector3(0f, (bobPixels / 32f), 0f);
            partRoot.localRotation = Quaternion.identity;

            jointHead.localRotation     = Quaternion.Euler(0f, 0f, headAng);
            jointArmLeft.localRotation  = Quaternion.Euler(0f, 0f,  armAng);
            jointArmRight.localRotation = Quaternion.Euler(0f, 0f, -armAng);
            jointLegLeft.localRotation  = Quaternion.Euler(0f, 0f,  legAng);
            jointLegRight.localRotation = Quaternion.Euler(0f, 0f, -legAng);
            jointBody.localRotation     = Quaternion.Euler(0f, 0f, lean);

            ghostTimer += Time.deltaTime;
            if (ghostTimer > 0.07f)
            {
                ghostTimer = 0f;
                SpawnGhost();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ATTACK — Vung Vũ Khí & Vệt Đao Khí Nguyên Tố (Nạp 100% từ animConfig)
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

            var c = animConfig?.attack;
            float swingAngle = c != null ? c.armSwingAngle : 45f;
            float bodyLean   = c != null ? c.bodyLeanAngle : 4f;
            float headTilt   = c != null ? c.headTiltAngle : 2f;
            float legSwing   = c != null ? c.legSwingAngle : 10f;

            float swingDir = isFacingLeft ? -swingAngle : swingAngle;

            // 1. Vung khớp tay cầm vũ khí & xoay vũ khí
            Quaternion origArmRot   = jointArmRight.localRotation;
            Quaternion origWepRot   = jointWeapon.localRotation;
            Quaternion origBodyRot  = jointBody.localRotation;
            Quaternion origHeadRot  = jointHead.localRotation;
            Quaternion origLegLRot  = jointLegLeft.localRotation;
            Quaternion origLegRRot  = jointLegRight.localRotation;

            jointArmRight.localRotation = origArmRot * Quaternion.Euler(0, 0, swingDir);
            jointWeapon.localRotation   = origWepRot * Quaternion.Euler(0, 0, swingDir * 1.3f);
            jointBody.localRotation     = Quaternion.Euler(0, 0, isFacingLeft ? -bodyLean : bodyLean);
            jointHead.localRotation     = Quaternion.Euler(0, 0, isFacingLeft ? -headTilt : headTilt);
            jointLegLeft.localRotation  = Quaternion.Euler(0, 0, isFacingLeft ? legSwing : -legSwing);
            jointLegRight.localRotation = Quaternion.Euler(0, 0, isFacingLeft ? -legSwing : legSwing);

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
            jointBody.localRotation     = origBodyRot;
            jointHead.localRotation     = origHeadRot;
            jointLegLeft.localRotation  = origLegLRot;
            jointLegRight.localRotation = origLegRRot;
            isAttacking = false;
        }

        // ════════════════════════════════════════════════════════════════
        //  HURT — Recoil Giật Lùi & Chớp Đỏ (Nạp 100% từ animConfig)
        // ════════════════════════════════════════════════════════════════
        public void TriggerHurt()
        {
            hurtTimer = 0.55f;
        }

        private void AnimateHurt()
        {
            hurtTimer -= Time.deltaTime;
            float flash = Mathf.PingPong(hurtTimer * 14f, 1f);
            Color cCol  = Color.Lerp(Color.white, new Color(1f, 0.25f, 0.25f), flash);

            foreach (var sr in new[] { srHairFront, srHead, srBody, srArmL, srArmR, srLegL, srLegR, srCostume })
                if (sr != null) sr.color = cCol;

            var c = animConfig?.hurt;
            float speed   = c != null ? c.speed : 35f;
            float bobPix  = c != null ? c.bobAmount : 0.5f;
            float leanAng = c != null ? c.bodyLeanAngle : -8f;
            float headAng = c != null ? c.headTiltAngle : 8f;
            float armAng  = c != null ? c.armSwingAngle : -20f;
            float squash  = c != null ? c.squashAmount : 0.08f;
            float stretch = c != null ? c.stretchAmount : -0.05f;

            float jerk = Mathf.Sin(hurtTimer * speed) * (bobPix / 32f);
            partRoot.localPosition = rootOrigin + new Vector3(-jerk, 0f, 0f);
            partRoot.localRotation = Quaternion.Euler(0f, 0f, jerk * (leanAng * 5f));
            partRoot.localScale    = new Vector3(1f + squash, 1f + stretch, 1f);

            jointHead.localRotation     = Quaternion.Euler(0f, 0f, headAng);
            jointArmLeft.localRotation  = Quaternion.Euler(0f, 0f, armAng);
            jointArmRight.localRotation = Quaternion.Euler(0f, 0f, armAng);

            if (hurtTimer <= 0f)
            {
                foreach (var sr in new[] { srHairFront, srHead, srBody, srArmL, srArmR, srLegL, srLegR, srCostume })
                    if (sr != null) sr.color = Color.white;
                partRoot.localRotation = Quaternion.identity;
                partRoot.localPosition = rootOrigin;
                partRoot.localScale    = Vector3.one;
                jointHead.localRotation = Quaternion.identity;
                jointArmLeft.localRotation = Quaternion.identity;
                jointArmRight.localRotation = Quaternion.identity;
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

        private string cachedWepId = null;
        private string cachedWepIcon = null;
        private bool cachedWepActive = false;
        private string cachedCostumeId = null;
        private Direction8 cachedCostumeDir = (Direction8)(-1);

        private void SyncEquippedWeapon()
        {
            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null) return;
            var wep = inventoryManager.GetEquippedGear(EquipSlot.Weapon);
            bool hasWep = (wep != null);
            string wepIcon = (wep != null) ? wep.iconName : null;
            string wepId = (wep != null) ? wep.id : null;

            if (srWeapon != null)
            {
                if (hasWep != cachedWepActive || wepIcon != cachedWepIcon || wepId != cachedWepId)
                {
                    cachedWepActive = hasWep;
                    cachedWepIcon = wepIcon;
                    cachedWepId = wepId;
                    srWeapon.enabled = hasWep;
                    if (hasWep && !string.IsNullOrEmpty(wepIcon))
                    {
                        Sprite s = LoadSpr(wepIcon) ?? LoadSpr("item_weapon");
                        if (s != null) srWeapon.sprite = s;
                    }
                }
            }
        }

        private void SyncEquippedCostume()
        {
            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null) return;
            var cos = inventoryManager.GetEquippedGear(EquipSlot.Costume);
            string cosId = (cos != null) ? cos.id : null;

            if (srCostume != null)
            {
                if (cosId != cachedCostumeId || FacingDir != cachedCostumeDir)
                {
                    cachedCostumeId = cosId;
                    cachedCostumeDir = FacingDir;

                    if (cos != null && !string.IsNullOrEmpty(cos.id) && cos.id.StartsWith("skin_"))
                    {
                        srCostume.enabled = true;
                        string dirStr = DirName(FacingDir);
                        srCostume.sprite = LoadSpr($"Characters/{cos.id}_{dirStr}_0");
                    }
                    else
                    {
                        srCostume.enabled = false;
                        srCostume.sprite = null;
                    }
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  FACING DIRECTION
        // ════════════════════════════════════════════════════════════════
        private void UpdateFacingDirection()
        {
            if (RemoteFacingDirOverride.HasValue)
            {
                FacingDir = RemoteFacingDirOverride.Value;
                return;
            }

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
        //  GHOST AFTERIMAGE — Tin tưởng 100% toạ độ SpriteRenderer cấu hình
        // ════════════════════════════════════════════════════════════════
        private void SpawnGhost()
        {
            if (srBody == null || srBody.sprite == null) return;

            var ghostRoot = new GameObject("RigGhost");

            // Danh sách toàn bộ các bộ phận của nhân vật theo đúng cấu hình rig_config & animation_config
            var renderers = new[] {
                srHairBack,
                srLegL,
                srLegR,
                srBody,
                srArmL,
                srArmR,
                srHead,
                srHairFront,
                srCostume
            };

            var ghostSrs = new List<SpriteRenderer>(9);
            for (int i = 0; i < renderers.Length; i++)
            {
                var sr = renderers[i];
                if (sr == null || !sr.enabled || sr.sprite == null) continue;

                var partGhost = new GameObject(sr.gameObject.name + "_Ghost");
                partGhost.transform.SetParent(ghostRoot.transform, false);
                // Sử dụng chính xác toạ độ thế giới của SpriteRenderer (đã tính toán bù trừ pivot offset theo config)
                partGhost.transform.SetPositionAndRotation(sr.transform.position, sr.transform.rotation);
                partGhost.transform.localScale = sr.transform.lossyScale;

                var gsr = partGhost.AddComponent<SpriteRenderer>();
                gsr.sprite       = sr.sprite;
                gsr.flipX        = sr.flipX;
                gsr.flipY        = sr.flipY;
                gsr.sortingOrder = Mathf.Max(0, sr.sortingOrder - 2); // Nằm ngay sau lưng nhân vật thật
                gsr.color        = new Color(0.20f, 0.85f, 1.0f, 0.35f);
                ghostSrs.Add(gsr);
            }

            StartCoroutine(FadeAndDestroyGhost(ghostRoot, ghostSrs, 0.20f));
        }

        private IEnumerator FadeAndDestroyGhost(GameObject ghostObj, List<SpriteRenderer> srs, float duration)
        {
            if (ghostObj == null) yield break;
            float elapsed = 0f;

            while (elapsed < duration && ghostObj != null)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.35f, 0f, elapsed / duration);
                for (int i = 0; i < srs.Count; i++)
                {
                    if (srs[i] != null)
                    {
                        var c = srs[i].color;
                        c.a = alpha;
                        srs[i].color = c;
                    }
                }
                yield return null;
            }

            if (ghostObj != null) Destroy(ghostObj);
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
        //  AVATAR & FULL BODY TEXTURES — Tĩnh Chính Diện (South / Front) Sắc Nét, Siêu Nhẹ (300+ FPS)
        // ════════════════════════════════════════════════════════════════
        private static Texture2D cachedAvatarMale;
        private static Texture2D cachedAvatarFemale;
        private static Texture2D cachedAvatarSoulKnight;
        private static Texture2D cachedAvatarVoidReaper;
        private static Texture2D cachedFullBodyMale;
        private static Texture2D cachedFullBodyFemale;
        private static Texture2D cachedFullBodySoulKnight;
        private static Texture2D cachedFullBodyVoidReaper;

        private static Texture2D LoadRigBodyTexture(bool isFemale)
        {
            string path = isFemale ? "Characters/Rig/female/south/body" : "Characters/Rig/male/south/body";
            var tex = Resources.Load<Texture2D>(path);
            if (tex != null) return tex;
            var sp = Resources.Load<Sprite>(path);
            if (sp != null) return sp.texture;
            string fbPath = isFemale ? "Characters/fullbody_female" : "Characters/fullbody_male";
            tex = Resources.Load<Texture2D>(fbPath);
            if (tex != null) return tex;
            sp = Resources.Load<Sprite>(fbPath);
            if (sp != null) return sp.texture;
            return isFemale ? Resources.Load<Texture2D>("player_female_base") : Resources.Load<Texture2D>("player_male_base");
        }

        public Texture GetPortraitTexture()
        {
            if (CurrentSkin == SkinType.SoulKnight)
            {
                if (cachedAvatarSoulKnight == null) cachedAvatarSoulKnight = Resources.Load<Texture2D>("Characters/avatar_soul_knight");
                if (cachedAvatarSoulKnight != null) return cachedAvatarSoulKnight;
            }
            if (CurrentSkin == SkinType.VoidReaper)
            {
                if (cachedAvatarVoidReaper == null) cachedAvatarVoidReaper = Resources.Load<Texture2D>("Characters/avatar_void_reaper");
                if (cachedAvatarVoidReaper != null) return cachedAvatarVoidReaper;
            }

            return LoadRigBodyTexture(IsFemale);
        }

        public Texture GetFullBodyTexture()
        {
            if (CurrentSkin == SkinType.SoulKnight)
            {
                if (cachedFullBodySoulKnight == null) cachedFullBodySoulKnight = Resources.Load<Texture2D>("Characters/skin_soul_knight_south_0") ?? Resources.Load<Texture2D>("Characters/avatar_soul_knight");
                if (cachedFullBodySoulKnight != null) return cachedFullBodySoulKnight;
            }
            if (CurrentSkin == SkinType.VoidReaper)
            {
                if (cachedFullBodyVoidReaper == null) cachedFullBodyVoidReaper = Resources.Load<Texture2D>("Characters/skin_void_reaper_south_0") ?? Resources.Load<Texture2D>("Characters/avatar_void_reaper");
                if (cachedFullBodyVoidReaper != null) return cachedFullBodyVoidReaper;
            }

            return LoadRigBodyTexture(IsFemale);
        }

        /// <summary>
        /// Thực hiện chuyển đổi giới tính Nam <-> Nữ:
        /// Yêu cầu bắt buộc: Người chơi đang trang bị Ngoại Trang (Costume) có thuộc tính ẩn 'gender_swap' (Chuyển Giới).
        /// Nếu không thỏa mãn -> Từ chối và thông báo lỗi rõ ràng.
        /// </summary>
        public bool TryToggleGender()
        {
            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            bool canToggle = inventoryManager != null && inventoryManager.CanSwitchGender();

            if (!canToggle)
            {
                string msg = "⛔ Cần mặc Ngoại Trang có thuộc tính Chuyển Giới để đổi giới tính!";
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.4f, msg, new Color(1f, 0.4f, 0.4f));
                AudioManager.PlayHit();
                return false;
            }

            IsFemale = !IsFemale;
            sprCache.Clear();
            LoadDirectionSprites();

            // Kiểm tra và tự động tháo trang bị nếu không còn tương thích với giới tính mới
            if (inventoryManager != null)
            {
                inventoryManager.OnGenderSwapped(IsFemale);
            }

            string label = IsFemale ? "Nữ" : "Nam";
            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.3f,
                $"🚻 {label} [G]", new Color(0.35f, 0.95f, 0.55f));
            AudioManager.PlayPickup();
            return true;
        }

        /// <summary>
        /// Ép đổi giới tính dành cho GM / Studio Testing Playground
        /// </summary>
        public void ForceSetGender(bool female)
        {
            if (IsFemale == female) return;
            IsFemale = female;
            sprCache.Clear();
            LoadDirectionSprites();
            if (inventoryManager == null) inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager != null)
            {
                inventoryManager.OnGenderSwapped(IsFemale);
            }
            string label = IsFemale ? "Nữ" : "Nam";
            FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.3f,
                $"🚻 {label} [Dev Test]", new Color(0.35f, 0.95f, 0.55f));
            AudioManager.PlayPickup();
        }

        // ════════════════════════════════════════════════════════════════
        //  HOTKEYS
        // ════════════════════════════════════════════════════════════════
        private void HandleHotkeys()
        {
            if (GodlessFantasy.UI.CombatHUD.Instance != null && GodlessFantasy.UI.CombatHUD.Instance.IsChatFocused()) return;

            if (Input.GetKeyDown(KeyCode.G))
            {
                TryToggleGender();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                sprCache.Clear();
                LoadConfigs();
                LoadDirectionSprites();
                FloatingDamageManager.ShowFloatingText(transform.position + Vector3.up * 1.5f,
                    "🔄 Đã nạp lại Rig Sprites & Cấu Hình Hoạt Ảnh mới nhất từ ổ đĩa! [F5]", Color.green);
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
            if (sprCache.TryGetValue(path, out var c)) return c;

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

            sprCache[path] = s;
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
