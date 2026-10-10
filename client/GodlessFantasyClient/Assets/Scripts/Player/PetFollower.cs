using UnityEngine;

namespace GodlessFantasy.Player
{
    /// <summary>
    /// Thú Cưng Mini Đồng Hành (Lively Companion Pet):
    /// - Không bao giờ đứng trước mặt người chơi làm vướng tầm nhìn.
    /// - Luôn bay/chạy ở 1 trong 2 bên hông (Left Wing / Right Wing) cách đều 0.85m chuẩn mực.
    /// - Cơ chế Random Side Switch: Cứ mỗi 4 - 8 giây, thú cưng lại ngẫu nhiên đổi bên (lúc lượn sang trái, lúc chạy sang phải) tạo cảm giác cực kỳ sinh động!
    /// - Tự động quay mặt (FlipX) về phía chủ nhân.
    /// - Nhấp nhô nhịp nhàng bồng bềnh siêu nhẹ tại chỗ.
    /// - Chuẩn 3-KHÔNG: Không đánh, Không cưỡi, Không bị chọn làm mục tiêu.
    /// </summary>
    public class PetFollower : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform owner;
        [SerializeField] private float sideDistance = 0.85f;      // Khoảng cách bên hông chuẩn mực
        [SerializeField] private float followSmoothSpeed = 4.8f;   // Tốc độ di chuyển bám theo mượt mà
        [SerializeField] private float teleportDistance = 8.0f;    // Dịch chuyển tức thời khi quá xa

        [Header("Pet Progression & Evolution (Cấp Độ & Tiến Hóa)")]
        [SerializeField] private int petLevel = 1;
        [SerializeField] private int petExp = 0;
        private const int MaxPetLevel = 50;

        public int PetLevel => petLevel;
        public int PetExp => petExp;
        public int PetMaxExp => petLevel * 100;
        public float ExpProgress => Mathf.Clamp01((float)petExp / Mathf.Max(1, PetMaxExp));

        // 3 Giai Đoạn Tiến Hóa:
        // Giai đoạn 1 (Lv.1 - 9): Sóc Chibi Sơ Cấp (Scale 1.0f)
        // Giai đoạn 2 (Lv.10 - 24): Linh Sóc Trưởng Thành (Scale 1.35f, lông phát sáng)
        // Giai đoạn 3 (Lv.25+): Thần Sóc Hoàng Kim (Scale 1.7f, hào quang rực rỡ)
        public int EvolutionStage => (petLevel >= 25) ? 3 : (petLevel >= 10) ? 2 : 1;
        public string StageName => (EvolutionStage == 3) ? "Thần Sóc Hoàng Kim" : (EvolutionStage == 2) ? "Linh Sóc Trưởng Thành" : "Sóc Chibi Sơ Sinh";
        public float TargetScale => (EvolutionStage == 3) ? 1.7f : (EvolutionStage == 2) ? 1.35f : 1.0f;

        public string BonusStatsDescription =>
            (EvolutionStage == 3) ? "+50 HP Tối Đa · +15 Giáp · +10 Công Phép · +0.5m/s Tốc Chạy" :
            (EvolutionStage == 2) ? "+20 HP Tối Đa · +5 Giáp · +0.2m/s Tốc Chạy" :
            "+5 HP Tối Đa";

        public float GetBonusMoveSpeed() => (EvolutionStage == 3) ? 0.5f : (EvolutionStage == 2) ? 0.2f : 0f;
        public int GetBonusDefense() => (EvolutionStage == 3) ? 15 : (EvolutionStage == 2) ? 5 : 0;

        public void FeedPet(int expAmount = 50)
        {
            if (petLevel >= MaxPetLevel)
            {
                Debug.Log("[PET] Thú cưng đã đạt cấp độ tối đa!");
                return;
            }

            int prevStage = EvolutionStage;
            petExp += expAmount;

            GodlessFantasy.Combat.FloatingDamageManager.Instance?.SpawnFloatingText(
                transform.position + Vector3.up * 0.9f, $"💖 +{expAmount} EXP (Pet)", new Color(1f, 0.45f, 0.75f));

            while (petExp >= PetMaxExp && petLevel < MaxPetLevel)
            {
                petExp -= PetMaxExp;
                petLevel++;
                GodlessFantasy.Combat.FloatingDamageManager.Instance?.SpawnFloatingText(
                    transform.position + Vector3.up * 1.3f, $"🎉 PET LÊN CẤP {petLevel}!", new Color(1f, 0.85f, 0.2f));
            }

            // Kiểm tra Tiến Hóa
            if (EvolutionStage > prevStage)
            {
                GodlessFantasy.Combat.FloatingDamageManager.Instance?.SpawnFloatingText(
                    transform.position + Vector3.up * 1.6f, $"✨ TIẾN HÓA: {StageName}!", new Color(1f, 0.95f, 0.4f));
                Debug.Log($"<color=yellow>[TIẾN HÓA THÚ CƯNG]</color> Thú cưng đã tiến hóa thành: {StageName} (To hơn, đẹp hơn!)");
            }

            UpdateScaleAndVisuals();
        }

        public void UpdateScaleAndVisuals()
        {
            transform.localScale = Vector3.one * TargetScale;
            if (spriteRenderer != null)
            {
                if (EvolutionStage == 3) spriteRenderer.color = new Color(1f, 0.92f, 0.65f); // Hoàng kim
                else if (EvolutionStage == 2) spriteRenderer.color = new Color(0.9f, 1f, 0.92f); // Linh thú sáng
                else spriteRenderer.color = Color.white;
            }
        }

        [Header("Lively Flank Switch (Đổi bên ngẫu nhiên)")]
        private float currentSideX = -1.0f;                        // -1: Bên trái, +1: Bên phải
        private float sideSwitchTimer = 5.0f;                      // Bộ đếm thời gian đổi bên

        [Header("Subtle Gentle Floating")]
        [SerializeField] private float bobFrequency = 2.2f;
        [SerializeField] private float bobAmplitude = 0.025f;

        private SpriteRenderer spriteRenderer;
        private Vector3 currentBasePosition;

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
            if (owner != null)
            {
                currentSideX = (Random.value > 0.5f) ? 1.0f : -1.0f;
                currentBasePosition = owner.position + new Vector3(currentSideX * sideDistance, -0.15f, 0);
                transform.position = currentBasePosition;
                ResetSwitchTimer();
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 9; // Hiển thị dưới chân nhân vật (Layer 10)
            }
            currentBasePosition = transform.position;
            currentSideX = (Random.value > 0.5f) ? 1.0f : -1.0f;
            ResetSwitchTimer();
            UpdateScaleAndVisuals();
        }

        private void ResetSwitchTimer()
        {
            // Random khoảng 4.5 đến 8.0 giây một lần
            sideSwitchTimer = Random.Range(4.5f, 8.0f);
        }

        private void Update()
        {
            if (owner == null) return;

            // 1. Logic đổi bên ngẫu nhiên theo thời gian giúp thú cưng luôn sinh động
            sideSwitchTimer -= Time.deltaTime;
            if (sideSwitchTimer <= 0f)
            {
                ResetSwitchTimer();
                // 50% cơ hội đổi bên sang hông đối diện, 50% giữ nguyên phía
                if (Random.value < 0.65f)
                {
                    currentSideX = -currentSideX;
                }
            }

            Vector3 ownerPos = owner.position;

            // 2. Vị trí mục tiêu: Luôn ở bên hông (trái hoặc phải), ngang tầm chân/hông, KHÔNG BAO GIỜ đứng trước mặt
            Vector3 desiredBasePos = ownerPos + new Vector3(currentSideX * sideDistance, -0.15f, 0);
            float currentDist = Vector2.Distance(currentBasePosition, ownerPos);

            if (currentDist > teleportDistance)
            {
                currentBasePosition = desiredBasePos;
            }
            else
            {
                // Bám theo mượt mà
                currentBasePosition = Vector3.Lerp(currentBasePosition, desiredBasePos, followSmoothSpeed * Time.deltaTime);
            }

            // 3. Lật Sprite thú cưng luôn quay mặt nhìn về phía chủ nhân
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (currentBasePosition.x < ownerPos.x);
            }

            // 4. Hiệu ứng nhấp nhô siêu nhẹ tại chỗ
            float bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = currentBasePosition + new Vector3(0, bobY, 0);
        }
    }
}
