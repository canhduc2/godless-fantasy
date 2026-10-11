using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Guild
{
    [Serializable]
    public class GuildMember
    {
        public string memberName;
        public int level;
        public string characterClass;
        public string element;
        public bool isOnline;
        public string currentMap;
        public bool isLeader;
        public Vector2 mapPosition;

        public GuildMember(string name, int lv, string cls, string elem, bool online, string map, bool leader = false, Vector2 pos = default)
        {
            memberName = name;
            level = lv;
            characterClass = cls;
            element = elem;
            isOnline = online;
            currentMap = map;
            isLeader = leader;
            mapPosition = pos;
        }
    }

    [Serializable]
    public class GuildData
    {
        public string guildId;
        public string guildName;
        public string leaderName;
        public int guildLevel = 1;
        public int guildGoldFund = 500;
        public string slogan = "Huynh đệ đồng tâm, sinh tử có nhau!";
        public List<GuildMember> members = new List<GuildMember>();

        public GuildData(string id, string name, string leader)
        {
            guildId = id;
            guildName = name;
            leaderName = leader;
            guildLevel = 1;
            guildGoldFund = 500;
        }
    }

    /// <summary>
    /// Hệ Thống Quản Lý Bang Hội & Đồng Đội (Guild & Teammate System):
    /// - Mở từ Tab Cài Đặt (Settings) -> Pop-up cửa sổ ở giữa màn hình.
    /// - Chưa có bang: Tạo bang (chi phí rẻ 200 vàng ai cũng tạo được) / Gia nhập bang có sẵn.
    /// - Đã có bang: Không thể ở bang khác, muốn sang bang khác phải RỜI BANG.
    /// - Rời Bang: Cơ chế Double Check bảo vệ đếm ngược 60s (1 phút) mới mở khóa nút Rời Bang.
    /// - Danh sách thành viên: Xem Profile, Trạng thái Online, Map, Mời Giao Dịch.
    /// - Hiển thị Tên Bang trên cùng đỉnh đầu nhân vật (trên danh hiệu).
    /// - Hiển thị chấm màu đồng minh trên Minimap khi ở cùng Map.
    /// - Chia sẻ EXP & Vàng khi đánh boss/quái cùng nhau.
    /// - Hưởng Buff Chiêu 6 (AoE Team Buff) khi đứng gần nhau trong cùng Map.
    /// - Title chung phó bản bang: « Huynh Đệ Sát Cánh ».
    /// </summary>
    public class GuildManager : MonoBehaviour
    {
        public static GuildManager Instance { get; private set; }

        public const int CREATE_GUILD_COST = 200; // 200 Vàng - Giá rẻ ai cũng tạo được

        [SerializeField] private GuildData currentGuild = null;
        [SerializeField] private List<GuildData> publicGuilds = new List<GuildData>();

        // Timer bảo vệ rời bang (1 phút = 60s mới mở khóa)
        private float leaveGuildCountdown = 0f;
        private bool isLeaveRequested = false;

        public GuildData CurrentGuild => currentGuild;
        public bool IsInGuild => currentGuild != null;
        public List<GuildData> PublicGuilds => publicGuilds;
        public float LeaveGuildCountdown => leaveGuildCountdown;
        public bool IsLeaveRequested => isLeaveRequested;
        public bool IsLeaveUnlocked => isLeaveRequested && leaveGuildCountdown <= 0f;

        public event Action OnGuildUpdated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            InitializeDefaultGuilds();
        }

        private void InitializeDefaultGuilds()
        {
            // Danh sách các bang hội server mẫu để người chơi có thể gia nhập hoặc xem
            var g1 = new GuildData("guild_dragon", "Thần Long Hội", "Chiến Thần Long");
            g1.slogan = "Chấn hưng bang phái, bình định tứ hải!";
            g1.members.Add(new GuildMember("Chiến Thần Long", 15, "Kị Sĩ", "Hỏa", true, "Rừng Rậm PvP", true, new Vector2(8f, 2f)));
            g1.members.Add(new GuildMember("Băng Tâm Tiên Tử", 14, "Pháp Sư", "Băng", true, "Rừng Rậm PvP", false, new Vector2(9.5f, 3f)));
            g1.members.Add(new GuildMember("Lôi Đình Quyền Vương", 12, "Võ Sĩ", "Lôi", true, "Làng Tân Thủ", false, new Vector2(-12f, 0f)));
            publicGuilds.Add(g1);

            var g2 = new GuildData("guild_phoenix", "Phượng Hoàng Cốc", "Hỏa Phượng Tiên");
            g2.slogan = "Tự do khám phá, tương trợ anh em!";
            g2.members.Add(new GuildMember("Hỏa Phượng Tiên", 16, "Pháp Sư", "Hỏa", true, "Làng Tân Thủ", true));
            g2.members.Add(new GuildMember("Phong Ảnh Xạ Thủ", 11, "Xạ Thủ", "Phong", false, "Rừng Rậm PvP", false));
            publicGuilds.Add(g2);

            // Mặc định tạo sẵn bang hội "SquiRRels Studio" người chơi đang tham gia để trải nghiệm ngay
            JoinGuildInternal("guild_dragon", "Dũng Sĩ SquiRRels");
        }

        private void Update()
        {
            if (isLeaveRequested && leaveGuildCountdown > 0f)
            {
                leaveGuildCountdown -= Time.deltaTime;
                if (leaveGuildCountdown <= 0f)
                {
                    leaveGuildCountdown = 0f;
                }
            }
        }

        public bool CreateGuild(string name, PlayerStats playerStats)
        {
            if (IsInGuild)
            {
                Debug.LogWarning("[GUILD] Bạn đang ở trong bang hội khác! Phải rời bang trước.");
                return false;
            }

            if (string.IsNullOrEmpty(name) || name.Trim().Length < 2)
            {
                Debug.LogWarning("[GUILD] Tên bang hội không hợp lệ!");
                return false;
            }

            if (playerStats == null || !playerStats.ConsumeGold(CREATE_GUILD_COST))
            {
                Debug.LogWarning($"[GUILD] Không đủ Vàng tạo bang! Cần {CREATE_GUILD_COST} Vàng.");
                FloatingDamageManager.ShowFloatingText(playerStats != null ? playerStats.transform.position + Vector3.up * 1f : Vector3.zero,
                    $"❌ Không đủ {CREATE_GUILD_COST} Vàng tạo Bang!", new Color(1f, 0.4f, 0.4f));
                return false;
            }

            string leaderName = playerStats.CharacterName;
            string gId = "guild_" + Guid.NewGuid().ToString().Substring(0, 8);
            var newGuild = new GuildData(gId, name.Trim(), leaderName);
            newGuild.members.Add(new GuildMember(leaderName, playerStats.Level, playerStats.CharacterClass, playerStats.Element, true, "Làng Tân Thủ", true));

            publicGuilds.Add(newGuild);
            currentGuild = newGuild;

            FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up * 1.2f,
                $"👑 THÀNH LẬP BANG [{name}] THÀNH CÔNG!", new Color(1f, 0.88f, 0.25f));
            Debug.Log($"<color=yellow>[GUILD]</color> Đã tạo Bang Hội '{name}' thành công!");

            OnGuildUpdated?.Invoke();
            return true;
        }

        public bool JoinGuild(string guildId, PlayerStats playerStats)
        {
            if (IsInGuild)
            {
                Debug.LogWarning("[GUILD] Bạn đã ở trong một bang hội!");
                return false;
            }

            var target = publicGuilds.Find(g => g.guildId == guildId);
            if (target == null) return false;

            string myName = playerStats != null ? playerStats.CharacterName : "Dũng Sĩ";
            target.members.Add(new GuildMember(myName, playerStats != null ? playerStats.Level : 1,
                playerStats != null ? playerStats.CharacterClass : "Kị Sĩ",
                playerStats != null ? playerStats.Element : "Hỏa", true, "Làng Tân Thủ", false));

            currentGuild = target;
            FloatingDamageManager.ShowFloatingText(playerStats != null ? playerStats.transform.position + Vector3.up * 1.2f : Vector3.zero,
                $"🛡️ GIA NHẬP BANG [{target.guildName}] THÀNH CÔNG!", new Color(0.4f, 0.95f, 0.5f));

            OnGuildUpdated?.Invoke();
            return true;
        }

        private void JoinGuildInternal(string guildId, string memberName)
        {
            var target = publicGuilds.Find(g => g.guildId == guildId);
            if (target != null)
            {
                if (!target.members.Exists(m => m.memberName == memberName))
                {
                    target.members.Add(new GuildMember(memberName, 1, "Kị Sĩ", "Hỏa", true, "Làng Tân Thủ", false));
                }
                currentGuild = target;
            }
        }

        // Bắt đầu quy trình Double-Check rời bang (Đếm ngược 60s = 1 phút mới mở khóa)
        public void RequestLeaveGuild()
        {
            if (!IsInGuild) return;
            isLeaveRequested = true;
            leaveGuildCountdown = 60.0f; // 1 phút an toàn chống bấm nhầm / phá bang
            Debug.Log("<color=yellow>[GUILD]</color> Đã yêu cầu rời bang. Chờ 60 giây để xác nhận!");
        }

        public void CancelLeaveGuild()
        {
            isLeaveRequested = false;
            leaveGuildCountdown = 0f;
        }

        public bool ConfirmLeaveGuild(PlayerStats playerStats)
        {
            if (!IsInGuild || !IsLeaveUnlocked) return false;

            string myName = playerStats != null ? playerStats.CharacterName : "Dũng Sĩ";
            currentGuild.members.RemoveAll(m => m.memberName == myName);
            string oldGuildName = currentGuild.guildName;
            currentGuild = null;
            isLeaveRequested = false;
            leaveGuildCountdown = 0f;

            FloatingDamageManager.ShowFloatingText(playerStats != null ? playerStats.transform.position + Vector3.up * 1.2f : Vector3.zero,
                $"👋 ĐÃ RỜI BANG [{oldGuildName}]!", new Color(1f, 0.6f, 0.2f));

            OnGuildUpdated?.Invoke();
            return true;
        }

        // Chia sẻ EXP & Vàng cho thành viên cùng Bang trong map
        public void DistributeGuildReward(int totalExp, int totalGold, Vector3 sourcePos)
        {
            if (!IsInGuild) return;

            var player = FindAnyObjectByType<PlayerStats>();
            if (player != null)
            {
                player.GainExp(totalExp);
                player.AddGold(totalGold);
                FloatingDamageManager.ShowFloatingText(player.transform.position + Vector3.up * 1f,
                    $"🤝 Bang Hội: +{totalExp} EXP · +{totalGold} Vàng", new Color(1f, 0.88f, 0.25f));
            }
        }

        // Kiểm tra xem vị trí có đồng minh cùng bang không
        public bool HasGuildAllyNear(Vector3 position, float radius)
        {
            if (!IsInGuild) return false;
            foreach (var m in currentGuild.members)
            {
                if (m.isOnline && !m.memberName.Contains("SquiRRels"))
                {
                    if (Vector2.Distance(position, m.mapPosition) <= radius) return true;
                }
            }
            return false;
        }
    }
}
