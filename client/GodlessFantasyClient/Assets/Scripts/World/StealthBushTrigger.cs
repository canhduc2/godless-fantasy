using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;
using GodlessFantasy.Core;

namespace GodlessFantasy.World
{
    /// <summary>
    /// Bụi Rậm Ẩn Nấp (Stealth Bush Trigger):
    /// - Khi nhân vật bước vào bên trong bụi rậm:
    ///   + Chuyển sang trạng thái tàng hình / ẩn nấp (IsPlayerInStealth = true).
    ///   + Làm mờ hình ảnh nhân vật (Alpha = 0.40f) tạo cảm giác ẩn thân sau tán lá.
    ///   + Quái vật mất dấu mục tiêu, hủy truy đuổi và quay về trạng thái tuần tra.
    /// - Khi bước ra khỏi bụi rậm:
    ///   + Khôi phục độ hiển thị 100% của nhân vật.
    /// </summary>
    public class StealthBushTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.name.Contains("Player"))
            {
                var pMove = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
                if (pMove != null)
                {
                    pMove.SetStealth(true);
                    AudioManager.PlayPickup();
                    FloatingDamageManager.ShowFloatingText(other.transform.position + Vector3.up * 1.2f, "🌿 Đang ẩn nấp trong bụi rậm!", new Color(0.4f, 1.0f, 0.6f));
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.name.Contains("Player"))
            {
                var pMove = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
                if (pMove != null)
                {
                    pMove.SetStealth(false);
                }
            }
        }
    }
}
