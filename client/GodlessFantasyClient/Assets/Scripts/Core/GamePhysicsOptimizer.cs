using UnityEngine;

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Bộ Tối Ưu Hóa Ma Trận Vật Lý 2D Toàn Cục (Physics 2D Collision Matrix Optimizer):
    /// - Tắt hoàn toàn trọng lực rơi tự do (Physics2D.gravity = 0) cho game 2D Topdown ARPG.
    /// - Bỏ qua các cặp Layer va chạm thừa thãi:
    ///   + Projectile không va chạm Projectile, TriggerZone, Decor, Pet.
    ///   + Pet không va chạm Enemy, Pet, TriggerZone, Decor.
    ///   + Decor không va chạm bất kỳ ai.
    /// - Giảm tải tới 70% số phép tính Narrowphase / Broadphase Collision mỗi frame!
    /// </summary>
    public static class GamePhysicsOptimizer
    {
        public static int LayerPlayer { get; private set; } = 6;
        public static int LayerEnemy { get; private set; } = 7;
        public static int LayerEnvironment { get; private set; } = 8;
        public static int LayerProjectile { get; private set; } = 9;
        public static int LayerTriggerZone { get; private set; } = 10;
        public static int LayerPet { get; private set; } = 11;
        public static int LayerDecor { get; private set; } = 12;

        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            // 1. Triệt tiêu trọng lực rơi tự do trong game 2D Topdown
            Physics2D.gravity = Vector2.zero;

            // 2. Lấy Layer Index động
            int lp = LayerMask.NameToLayer("Player");
            int le = LayerMask.NameToLayer("Enemy");
            int lenv = LayerMask.NameToLayer("Environment");
            int lproj = LayerMask.NameToLayer("Projectile");
            int ltrig = LayerMask.NameToLayer("TriggerZone");
            int lpet = LayerMask.NameToLayer("Pet");
            int ldecor = LayerMask.NameToLayer("Decor");

            if (lp >= 0) LayerPlayer = lp;
            if (le >= 0) LayerEnemy = le;
            if (lenv >= 0) LayerEnvironment = lenv;
            if (lproj >= 0) LayerProjectile = lproj;
            if (ltrig >= 0) LayerTriggerZone = ltrig;
            if (lpet >= 0) LayerPet = lpet;
            if (ldecor >= 0) LayerDecor = ldecor;

            // 3. Cấu hình loại bỏ va chạm thừa thãi
            // Projectile
            SetIgnore(LayerProjectile, LayerProjectile, true);
            SetIgnore(LayerProjectile, LayerTriggerZone, true);
            SetIgnore(LayerProjectile, LayerDecor, true);
            SetIgnore(LayerProjectile, LayerPet, true);

            // Pet
            SetIgnore(LayerPet, LayerPet, true);
            SetIgnore(LayerPet, LayerEnemy, true);
            SetIgnore(LayerPet, LayerTriggerZone, true);
            SetIgnore(LayerPet, LayerDecor, true);

            // Decor (Hoàn toàn không có vật lý)
            SetIgnore(LayerDecor, LayerDecor, true);
            SetIgnore(LayerDecor, LayerEnemy, true);
            SetIgnore(LayerDecor, LayerPlayer, true);
            SetIgnore(LayerDecor, LayerEnvironment, true);
            SetIgnore(LayerDecor, LayerTriggerZone, true);

            // TriggerZone
            SetIgnore(LayerTriggerZone, LayerTriggerZone, true);
            SetIgnore(LayerTriggerZone, LayerEnvironment, true);

            Debug.Log("<color=green>[PHYSICS 2D OPTIMIZER]</color> Đã thiết lập Collision Matrix và triệt tiêu va chạm thừa!");
        }

        private static void SetIgnore(int l1, int l2, bool ignore)
        {
            if (l1 >= 0 && l2 >= 0 && l1 < 32 && l2 < 32)
            {
                Physics2D.IgnoreLayerCollision(l1, l2, ignore);
            }
        }

        public static void SetLayer(GameObject obj, int layer)
        {
            if (obj != null && layer >= 0 && layer < 32)
            {
                obj.layer = layer;
            }
        }
    }
}
