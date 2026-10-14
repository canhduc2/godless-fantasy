using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;

namespace GodlessFantasy.AI
{
    /// <summary>
    /// Bộ Điều Phối & Quản Lý Vòng Lặp Tập Trung Cho Toàn Bộ AI Quái Vật (Centralized Monster AI Manager):
    /// - Gom toàn bộ hàng chục - hàng trăm con quái vật vào 1 hàm Update() duy nhất.
    /// - Triệt tiêu chi phí C++ to C# bridge overhead của hàng trăm MonoBehaviour Update().
    /// - Tích hợp Distance Culling & Tick LOD:
    ///   + Quái vật gần (< 30m): Cập nhật mượt mà full 300+ FPS mỗi frame.
    ///   + Quái vật cự ly trung bình (30m - 45m): Cập nhật mỗi 3 frame một lần (vẫn tự nhiên).
    ///   + Quái vật ở xa (> 45m): Bỏ qua tính toán, chỉ tick 1.5s/lần để tiết kiệm 98% CPU!
    /// </summary>
    public class MonsterAIManager : MonoBehaviour
    {
        public static MonsterAIManager Instance { get; private set; }

        private static List<MonsterAIController> monsters = new List<MonsterAIController>();
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public static void Register(MonsterAIController ai)
        {
            if (ai != null && !monsters.Contains(ai))
            {
                monsters.Add(ai);
            }
        }

        public static void Unregister(MonsterAIController ai)
        {
            if (ai != null)
            {
                monsters.Remove(ai);
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
                if (player != null) playerTransform = player.transform;
            }

            Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
            bool isPlayerStealth = PlayerMovement.IsPlayerInStealth;
            float dt = Time.deltaTime;

            for (int i = monsters.Count - 1; i >= 0; i--)
            {
                var m = monsters[i];
                if (m == null)
                {
                    monsters.RemoveAt(i);
                    continue;
                }

                if (!m.gameObject.activeInHierarchy || !m.enabled) continue;

                // TÍNH TOÁN KHOẢNG CÁCH LOD / DISTANCE CULLING
                float dx = m.transform.position.x - playerPos.x;
                float dy = m.transform.position.y - playerPos.y;
                float distSq = dx * dx + dy * dy;

                if (distSq > 2025f) // > 45 mét
                {
                    // Quá xa player: Bỏ qua 98% số frame, chỉ kiểm tra 1.5s/lần
                    if (Time.frameCount % 90 != (i % 90)) continue;
                }
                else if (distSq > 900f) // 30m - 45m
                {
                    // Cự ly trung bình: Tick mỗi 3 frame
                    if (Time.frameCount % 3 != (i % 3)) continue;
                }

                m.TickAI(dt, playerPos, isPlayerStealth);
            }
        }
    }
}
