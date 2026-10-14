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
        public string guildPassCode = "6688"; // Pass/Code gia nhập bang hội

        [SerializeField] private string _slogan = "Huynh đệ đồng tâm, sinh tử có nhau!";
        public string slogan
        {
            get => _slogan;
            set
            {
                if (string.IsNullOrEmpty(value)) _slogan = "";
                else if (value.Length > 50) _slogan = value.Substring(0, 50);
                else _slogan = value;
            }
        }
        public List<GuildMember> members = new List<GuildMember>();

        public GuildData(string id, string name, string leader, string passCode = "6688")
        {
            guildId = id;
            guildName = name;
            leaderName = leader;
            guildLevel = 1;
            guildGoldFund = 500;
            guildPassCode = string.IsNullOrEmpty(passCode) ? "8888" : passCode;
            slogan = "Huynh đệ đồng tâm, sinh tử có nhau!";
        }
    }

    /// <summary>
    /// Hệ Thống Quản Lý Bang Hội & Đồng Đội (Guild & Teammate System):
    /// - Mở từ Tab Cài Đặt (Settings) -> Pop-up cửa sổ ở giữa màn hình.
    /// - Chưa có bang: Tạo bang (chi phí rẻ 200 vàng ai cũng tạo được) / Gia nhập bang có sẵn bằng Mã Code / Pass Bang.
    /// - Đã có bang: Không thể ở bang khác, muốn sang bang khác phải RỜI BANG.
    /// - Rời Bang / Giải Tán Bang: Cơ chế Double Check bảo vệ đếm ngược 60s (1 phút) mới mở khóa nút Rời/Giải Tán Bang.
    /// - Bang Chủ có bảng Cài Đặt: Đổi Pass Bang, Đổi Khẩu Hiệu (tối đa 50 ký tự), Kích thành viên, Giải tán bang.
    /// - Danh sách thành viên: Xem Profile, Trạng thái Online, Map, Mời Giao Dịch, Kích Thành Viên (Bang Chủ).
    /// </summary>
    public class GuildManager : MonoBehaviour
    {
        public static GuildManager Instance { get; private set; }

        public const int CREATE_GUILD_COST = 200; // 200 Vàng - Giá rẻ ai cũng tạo được

        [SerializeField] private GuildData currentGuild = null;
        [SerializeField] private List<GuildData> publicGuilds = new List<GuildData>();

        // Timer bảo vệ rời / giải tán bang (1 phút = 60s mới mở khóa)
        private float leaveGuildCountdown = 0f;
        private bool isLeaveRequested = false;
        private bool isDisbandRequested = false;

        // Giới hạn nhập Pass Code gia nhập bang: Tối đa 3 lần / ngày
        public const int MAX_DAILY_JOIN_ATTEMPTS = 3;
        private int dailyJoinAttemptsUsed = 0;
        private string lastAttemptDateStr = "";

        public int RemainingDailyJoinAttempts
        {
            get
            {
                CheckResetDailyAttempts();
                return Mathf.Max(0, MAX_DAILY_JOIN_ATTEMPTS - dailyJoinAttemptsUsed);
            }
        }

        private void CheckResetDailyAttempts()
        {
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            if (lastAttemptDateStr != todayStr)
            {
                lastAttemptDateStr = todayStr;
                dailyJoinAttemptsUsed = 0;
            }
        }

        public GuildData CurrentGuild => currentGuild;
        public bool IsInGuild => currentGuild != null;
        public List<GuildData> PublicGuilds => publicGuilds;
        public float LeaveGuildCountdown => leaveGuildCountdown;
        public bool IsLeaveRequested => isLeaveRequested;
        public bool IsDisbandRequested => isDisbandRequested;
        public bool IsLeaveUnlocked => isLeaveRequested && leaveGuildCountdown <= 0f;
        public bool IsDisbandUnlocked => isDisbandRequested && leaveGuildCountdown <= 0f;

        public event Action OnGuildUpdated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            // Dọn sạch dữ liệu giả: Người chơi ban đầu chưa gia nhập bang hội nào
            currentGuild = null;
            publicGuilds.Clear();
        }

        private void Update()
        {
            if ((isLeaveRequested || isDisbandRequested) && leaveGuildCountdown > 0f)
            {
                leaveGuildCountdown -= Time.deltaTime;
                if (leaveGuildCountdown <= 0f)
                {
                    leaveGuildCountdown = 0f;
                }
            }
        }

        public bool IsPlayerLeader(PlayerStats playerStats)
        {
            if (currentGuild == null || playerStats == null) return false;
            if (currentGuild.leaderName == playerStats.CharacterName) return true;
            var m = currentGuild.members.Find(x => x.memberName == playerStats.CharacterName);
            return m != null && m.isLeader;
        }

        public bool CreateGuild(string name, PlayerStats playerStats)
        {
            return CreateGuild(name, "8888", "Huynh đệ đồng tâm, sinh tử có nhau!", playerStats);
        }

        public bool CreateGuild(string name, string passCode, string slogan, PlayerStats playerStats)
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
            var newGuild = new GuildData(gId, name.Trim(), leaderName, string.IsNullOrEmpty(passCode) ? "8888" : passCode.Trim());
            if (!string.IsNullOrEmpty(slogan))
            {
                newGuild.slogan = slogan;
            }
            newGuild.members.Add(new GuildMember(leaderName, playerStats.Level, playerStats.CharacterClass, playerStats.Element, true, "Làng Tân Thủ", true));

            publicGuilds.Add(newGuild);
            currentGuild = newGuild;

            FloatingDamageManager.ShowFloatingText(playerStats.transform.position + Vector3.up * 1.2f,
                $"👑 THÀNH LẬP BANG [{name}] THÀNH CÔNG!", new Color(1f, 0.88f, 0.25f));
            Debug.Log($"<color=yellow>[GUILD]</color> Đã tạo Bang Hội '{name}' thành công! Pass code: {newGuild.guildPassCode}");

            OnGuildUpdated?.Invoke();
            return true;
        }

        public bool JoinGuild(string guildId, PlayerStats playerStats)
        {
            return JoinGuildWithCode(guildId, "", playerStats, out _);
        }

        public bool JoinGuildWithCode(string guildId, string code, PlayerStats playerStats, out string errorMsg)
        {
            errorMsg = "";
            if (IsInGuild)
            {
                errorMsg = "Bạn đã ở trong một bang hội! Cần rời bang trước.";
                return false;
            }

            var target = publicGuilds.Find(g => g.guildId == guildId);
            if (target == null)
            {
                errorMsg = "Không tìm thấy bang hội trên server!";
                return false;
            }

            CheckResetDailyAttempts();

            if (RemainingDailyJoinAttempts <= 0)
            {
                errorMsg = "❌ Đã hết lượt nhập mã hôm nay (Giới hạn: 3 lần/ngày)! Vui lòng thử lại vào ngày mai.";
                return false;
            }

            string requiredCode = string.IsNullOrEmpty(target.guildPassCode) ? "6688" : target.guildPassCode.Trim();
            if (string.IsNullOrEmpty(code) || code.Trim() != requiredCode)
            {
                dailyJoinAttemptsUsed++;
                int left = RemainingDailyJoinAttempts;
                errorMsg = left > 0
                    ? $"❌ Sai Mã Code Bang! (Còn {left}/3 lượt thử hôm nay)"
                    : "❌ Sai Mã Code Bang! Đã hết 3 lượt nhập hôm nay, vui lòng quay lại ngày mai.";
                return false;
            }

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
            isDisbandRequested = false;
            leaveGuildCountdown = 60.0f; // 1 phút an toàn chống bấm nhầm
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

        // Bắt đầu quy trình Double-Check giải tán bang (Dành cho Bang Chủ - Đếm ngược 60s = 1 phút mới mở khóa)
        public void RequestDisbandGuild()
        {
            if (!IsInGuild) return;
            isDisbandRequested = true;
            isLeaveRequested = false;
            leaveGuildCountdown = 60.0f; // 1 phút an toàn bảo vệ
            Debug.Log("<color=red>[GUILD]</color> Bang Chủ đã yêu cầu GIẢI TÁN BANG. Chờ 60 giây để mở khóa xác nhận!");
        }

        public void CancelDisbandGuild()
        {
            isDisbandRequested = false;
            leaveGuildCountdown = 0f;
        }

        public bool ConfirmDisbandGuild(PlayerStats playerStats)
        {
            if (!IsInGuild || !IsDisbandUnlocked || !IsPlayerLeader(playerStats)) return false;

            string oldGuildName = currentGuild.guildName;
            publicGuilds.RemoveAll(g => g.guildId == currentGuild.guildId);
            currentGuild = null;
            isDisbandRequested = false;
            leaveGuildCountdown = 0f;

            FloatingDamageManager.ShowFloatingText(playerStats != null ? playerStats.transform.position + Vector3.up * 1.2f : Vector3.zero,
                $"💥 ĐÃ GIẢI TÁN BANG [{oldGuildName}] HOÀN TOÀN!", new Color(1f, 0.25f, 0.25f));
            Debug.Log($"<color=red>[GUILD]</color> Đã giải tán bang {oldGuildName} thành công!");

            OnGuildUpdated?.Invoke();
            return true;
        }

        // Kích thành viên khỏi bang (Chỉ Bang Chủ mới kích được, không thể kích Bang Chủ)
        public bool KickMember(string memberName, PlayerStats requesterStats)
        {
            if (!IsInGuild || !IsPlayerLeader(requesterStats)) return false;
            if (memberName == requesterStats.CharacterName || memberName == currentGuild.leaderName) return false;

            int removed = currentGuild.members.RemoveAll(m => m.memberName == memberName);
            if (removed > 0)
            {
                FloatingDamageManager.ShowFloatingText(requesterStats != null ? requesterStats.transform.position + Vector3.up * 1f : Vector3.zero,
                    $"⚠️ ĐÃ KÍCH [{memberName}] KHỎI BANG!", new Color(1f, 0.4f, 0.4f));
                Debug.Log($"<color=orange>[GUILD]</color> Bang Chủ đã kích thành viên {memberName} khỏi bang!");
                OnGuildUpdated?.Invoke();
                return true;
            }
            return false;
        }

        // Đổi Pass Bang & Khẩu Hiệu Bang (Dành cho Bang Chủ)
        public bool UpdateGuildSettings(string newSlogan, string newPassCode, PlayerStats requesterStats)
        {
            if (!IsInGuild || !IsPlayerLeader(requesterStats)) return false;

            if (newSlogan != null)
            {
                currentGuild.slogan = newSlogan;
            }
            if (!string.IsNullOrEmpty(newPassCode))
            {
                currentGuild.guildPassCode = newPassCode.Trim();
            }

            Debug.Log($"<color=green>[GUILD]</color> Đã cập nhật cài đặt bang: Slogan='{currentGuild.slogan}', Pass='{currentGuild.guildPassCode}'");
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
