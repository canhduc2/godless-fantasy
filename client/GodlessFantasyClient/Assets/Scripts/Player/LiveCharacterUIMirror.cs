using System.Collections.Generic;
using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Hệ thống Live UI Mirror cho Avatar HUD và Menu Xem Trước Nhân Vật (Equipment Preview):
    /// • Tạo bản sao thực thể độc lập (isolated live clone) tại tọa độ thế giới tách biệt (4000, 4000).
    /// • Đồng bộ 100% từng khớp xương (Head, Body, Arms, Legs, Weapon, Costume, VFX) và SpriteRenderer theo thời gian thực (60fps).
    /// • Xoay chuyển động đúng 8 hướng (FacingDir), tương thích mọi trạng thái đi bộ/chạy/thở idle/lướt dash/chém kiếm cùng lúc với nhân vật thật.
    /// • Xuất ra 2 RenderTexture (FilterMode.Point) nền trong suốt tuyệt đối:
    ///   1. rtAvatar (128x128): Cận cảnh gương mặt chibi, tóc, biểu cảm, cổ áo cho HUD Stats.
    ///   2. rtFullBody (256x256): Toàn thân từ chân đến đỉnh đầu, bóng đổ, vũ khí cho Menu Trang Bị Tab 1.
    /// </summary>
    public class LiveCharacterUIMirror : MonoBehaviour
    {
        public static LiveCharacterUIMirror Instance { get; private set; }

        private const float MIRROR_X = 4000f;
        private const float MIRROR_Y = 4000f;
        private static readonly Vector3 MIRROR_ORIGIN = new Vector3(MIRROR_X, MIRROR_Y, 0f);

        [Header("Render Textures (Dynamic Live Mirror)")]
        private RenderTexture rtAvatar;
        private RenderTexture rtFullBody;

        public Texture AvatarTexture => rtAvatar;
        public Texture FullBodyTexture => rtFullBody;

        [Header("Cameras")]
        private Camera camAvatar;
        private Camera camFullBody;

        [Header("Hierarchy Mirror")]
        private GameObject mirrorContainer;
        private GameObject clonedRoot;
        private GameObject clonedShadow;
        private Transform clonedHead;

        private class TransformPair
        {
            public Transform src;
            public Transform dst;
        }

        private class SRPair
        {
            public SpriteRenderer src;
            public SpriteRenderer dst;
        }

        private readonly List<TransformPair> transformPairs = new List<TransformPair>();
        private readonly List<SRPair> srPairs = new List<SRPair>();

        private ProceduralCharacterRig2D boundRig;

        [Header("Camera Tuning")]
        public Vector3 fullBodyCamOffset = new Vector3(0f, 0.18f, -10f);
        public float fullBodyOrthoSize = 0.82f;

        public Vector3 avatarCamOffset = new Vector3(0f, 0.62f, -10f);
        public float avatarOrthoSize = 0.28f;

        public static void EnsureInstance()
        {
            if (Instance == null)
            {
                var existing = FindAnyObjectByType<LiveCharacterUIMirror>();
                if (existing != null)
                {
                    Instance = existing;
                    return;
                }

                var go = new GameObject("_LiveCharacterUIMirror");
                Instance = go.AddComponent<LiveCharacterUIMirror>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            SetupCamerasAndRT();
        }

        private void SetupCamerasAndRT()
        {
            // 1. RenderTexture cho Avatar (128x128 Point filter)
            if (rtAvatar == null)
            {
                rtAvatar = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                rtAvatar.Create();
            }

            // 2. RenderTexture cho FullBody (256x256 Point filter)
            if (rtFullBody == null)
            {
                rtFullBody = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                rtFullBody.Create();
            }

            // 3. Camera cho Avatar
            if (camAvatar == null)
            {
                var camGo = new GameObject("Cam_Avatar");
                camGo.transform.SetParent(transform, false);
                camGo.transform.position = MIRROR_ORIGIN + avatarCamOffset;
                camAvatar = camGo.AddComponent<Camera>();
                camAvatar.orthographic = true;
                camAvatar.orthographicSize = avatarOrthoSize;
                camAvatar.clearFlags = CameraClearFlags.SolidColor;
                camAvatar.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camAvatar.nearClipPlane = 0.1f;
                camAvatar.farClipPlane = 30f;
                camAvatar.targetTexture = rtAvatar;
                camAvatar.enabled = false;
                camAvatar.depth = -100;
            }

            // 4. Camera cho FullBody
            if (camFullBody == null)
            {
                var camGo = new GameObject("Cam_FullBody");
                camGo.transform.SetParent(transform, false);
                camGo.transform.position = MIRROR_ORIGIN + fullBodyCamOffset;
                camFullBody = camGo.AddComponent<Camera>();
                camFullBody.orthographic = true;
                camFullBody.orthographicSize = fullBodyOrthoSize;
                camFullBody.clearFlags = CameraClearFlags.SolidColor;
                camFullBody.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camFullBody.nearClipPlane = 0.1f;
                camFullBody.farClipPlane = 30f;
                camFullBody.targetTexture = rtFullBody;
                camFullBody.enabled = false;
                camFullBody.depth = -100;
            }
        }

        public void SetupMirror(ProceduralCharacterRig2D sourceRig)
        {
            if (sourceRig == null || sourceRig.PartRoot == null) return;

            boundRig = sourceRig;
            transformPairs.Clear();
            srPairs.Clear();

            if (mirrorContainer != null)
            {
                Destroy(mirrorContainer);
            }

            mirrorContainer = new GameObject("MirrorCharacter_Container");
            mirrorContainer.transform.position = MIRROR_ORIGIN;
            mirrorContainer.transform.SetParent(transform, true);

            // Clone RigRoot (chứa toàn bộ xương khớp và sprites của ProceduralCharacterRig2D)
            clonedRoot = Instantiate(sourceRig.PartRoot.gameObject, mirrorContainer.transform);
            clonedRoot.name = "Cloned_RigRoot";
            clonedRoot.transform.localPosition = sourceRig.PartRoot.localPosition;
            clonedRoot.transform.localRotation = sourceRig.PartRoot.localRotation;
            clonedRoot.transform.localScale = sourceRig.PartRoot.localScale;
            clonedHead = clonedRoot.transform.Find("Joint_Head");

            PairHierarchy(sourceRig.PartRoot, clonedRoot.transform);

            // Clone Shadow (nếu có)
            if (sourceRig.PartShadow != null)
            {
                clonedShadow = Instantiate(sourceRig.PartShadow.gameObject, mirrorContainer.transform);
                clonedShadow.name = "Cloned_Shadow";
                clonedShadow.transform.localPosition = sourceRig.PartShadow.localPosition;
                clonedShadow.transform.localRotation = sourceRig.PartShadow.localRotation;
                clonedShadow.transform.localScale = sourceRig.PartShadow.localScale;

                PairHierarchy(sourceRig.PartShadow, clonedShadow.transform);
            }

            // Loại bỏ bất kỳ Collider nào trên mirror (tránh va chạm vật lý)
            var colliders = mirrorContainer.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Destroy(colliders[i]);
            }
        }

        private void PairHierarchy(Transform src, Transform dst)
        {
            if (src == null || dst == null) return;

            transformPairs.Add(new TransformPair { src = src, dst = dst });

            var srcSr = src.GetComponent<SpriteRenderer>();
            var dstSr = dst.GetComponent<SpriteRenderer>();
            if (srcSr != null && dstSr != null)
            {
                srPairs.Add(new SRPair { src = srcSr, dst = dstSr });
            }

            int childCount = Mathf.Min(src.childCount, dst.childCount);
            for (int i = 0; i < childCount; i++)
            {
                PairHierarchy(src.GetChild(i), dst.GetChild(i));
            }
        }

        private void LateUpdate()
        {
            if (boundRig == null || clonedRoot == null || transformPairs.Count == 0)
            {
                if (ProceduralCharacterRig2D.Instance != null && ProceduralCharacterRig2D.Instance.PartRoot != null)
                {
                    SetupMirror(ProceduralCharacterRig2D.Instance);
                }
                else
                {
                    return;
                }
            }

            // Đồng bộ toàn bộ Transforms của các khớp xương
            for (int i = 0; i < transformPairs.Count; i++)
            {
                var p = transformPairs[i];
                if (p.src == null || p.dst == null) continue;
                p.dst.localPosition = p.src.localPosition;
                p.dst.localRotation = p.src.localRotation;
                p.dst.localScale    = p.src.localScale;
            }

            // Đồng bộ trạng thái SpriteRenderer (Sprites, FlipX, FlipY, Tint Color, SortingOrder, Enabled)
            for (int i = 0; i < srPairs.Count; i++)
            {
                var p = srPairs[i];
                if (p.src == null || p.dst == null) continue;
                p.dst.sprite       = p.src.sprite;
                p.dst.flipX        = p.src.flipX;
                p.dst.flipY        = p.src.flipY;
                p.dst.color        = p.src.color;
                p.dst.sortingOrder = p.src.sortingOrder;
                p.dst.enabled      = p.src.enabled;
            }

            if (clonedHead == null && clonedRoot != null)
            {
                clonedHead = clonedRoot.transform.Find("Joint_Head");
            }

            // Render Avatar (Tắt bóng đổ mặt đất để khung chân dung mặt người hoàn toàn sạch sẽ)
            if (camAvatar != null && rtAvatar != null)
            {
                // Căn chỉnh tâm Camera thẳng vào gương mặt Chibi (mắt, mũi, biểu cảm, tóc)
                float headY = (clonedHead != null) ? (clonedHead.position.y + 0.17f) : (MIRROR_Y + 0.62f);
                headY += (avatarCamOffset.y - 0.62f);
                camAvatar.transform.position = new Vector3(MIRROR_X + avatarCamOffset.x, headY, avatarCamOffset.z);
                camAvatar.orthographicSize = avatarOrthoSize;
                if (clonedShadow != null) clonedShadow.SetActive(false);
                camAvatar.Render();
            }

            // Render FullBody (Bật bóng đổ dưới chân nhân vật để hiển thị hoàn chỉnh trong menu Trang Bị)
            if (camFullBody != null && rtFullBody != null)
            {
                // Căn chỉnh tâm Camera giữa đỉnh đầu và bóng đổ chân, hiển thị trọn vẹn 100% nhân vật không bị cắt
                float feetY = (clonedShadow != null) ? clonedShadow.transform.position.y : (MIRROR_Y - 0.48f);
                float topY  = (clonedHead != null) ? (clonedHead.position.y + 0.42f) : (MIRROR_Y + 0.85f);
                float centerY = (feetY + topY) * 0.5f + (fullBodyCamOffset.y - 0.18f);
                camFullBody.transform.position = new Vector3(MIRROR_X + fullBodyCamOffset.x, centerY, fullBodyCamOffset.z);
                camFullBody.orthographicSize = fullBodyOrthoSize;
                if (clonedShadow != null) clonedShadow.SetActive(true);
                camFullBody.Render();
            }
        }

        private void OnDestroy()
        {
            if (rtAvatar != null)
            {
                rtAvatar.Release();
                Destroy(rtAvatar);
                rtAvatar = null;
            }

            if (rtFullBody != null)
            {
                rtFullBody.Release();
                Destroy(rtFullBody);
                rtFullBody = null;
            }
        }
    }
}
