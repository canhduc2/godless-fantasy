using UnityEngine;

namespace GodlessFantasy.Combat
{
    /// <summary>
    /// Vùng An Toàn (Safe Zone):
    /// Bất kỳ nhân vật hoặc thực thể nào bước vào vùng Trigger này
    /// sẽ được kích hoạt cờ IsInSafeZone = true (miễn nhiễm sát thương PvP).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SafeZone : MonoBehaviour
    {
        [SerializeField] private string safeZoneName = "Tân Thủ Thôn";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.IsInSafeZone = true;
                Debug.Log($"[SAFE ZONE] {other.gameObject.name} đã bước vào {safeZoneName} (Vùng An Toàn)!");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.IsInSafeZone = false;
                Debug.Log($"[PVP DANGER] {other.gameObject.name} đã rời khỏi {safeZoneName}. Nguy cơ Friendly Fire & PvP kích hoạt!");
            }
        }
    }
}
