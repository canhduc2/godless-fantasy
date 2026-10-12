using System;
using System.Collections.Generic;
using UnityEngine;
using GodlessFantasy.Player;
using GodlessFantasy.Core;
using GodlessFantasy.Combat;

namespace GodlessFantasy.Quest
{
    public enum QuestState
    {
        NotStarted = 0,
        InProgress = 1,
        CanTurnIn = 2,
        Completed = 3
    }

    [Serializable]
    public class QuestData
    {
        public string id;
        public string title;
        public string description;
        public string progressLabel;
        public int currentProgress;
        public int targetProgress;
        public int rewardGold;
        public int rewardExp;
        public int rewardRP;
        public QuestState state;
    }

    /// <summary>
    /// Hệ Thống Quản Lý Nhiệm Vụ & Sổ Tay Nhiệm Vụ Phím [L] (Quest Engine & Quest Log Modal):
    /// - Quản lý chuỗi nhiệm vụ cốt truyện tân thủ:
    ///   + Q1: [Tân Thủ Khởi Hành] - Gặp Trưởng Làng Eldrin.
    ///   + Q2: [Rèn Luyện Kỹ Năng] - Đánh Bù Nhìn Gỗ 5 lần.
    ///   + Q3: [Bảo Vệ Biên Giới] - Tiêu diệt 2 Chiến Binh Quái bìa rừng.
    ///   + Q4: [Thám Hiểm Cổ Tích] - Tiến đến Cổng Hầm Ngục Di Tích.
    /// - Widget theo dõi nhiệm vụ (Quest Tracker Widget) nằm góc phải dưới Minimap & Giờ Thế Giới.
    /// - Nhấn phím [L] hoặc click vào Widget để mở Sổ Tay Nhiệm Vụ cổ điển.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public bool IsQuestLogOpen = false;

        private List<QuestData> questList = new List<QuestData>();
        private int currentActiveIndex = 0;

        public List<QuestData> QuestList => questList;
        public QuestData ActiveQuest => (currentActiveIndex >= 0 && currentActiveIndex < questList.Count) ? questList[currentActiveIndex] : null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            InitializeDefaultQuests();
        }

        private void InitializeDefaultQuests()
        {
            questList.Clear();

            // Q1: Gặp Trưởng Làng Eldrin
            questList.Add(new QuestData
            {
                id = "q_welcome",
                title = "Tân Thủ Khởi Hành",
                description = "Hãy đến Quảng Trường Trung Tâm Làng (-72, 2) gặp Trưởng Làng Eldrin để nhận chỉ dẫn cơ bản.",
                progressLabel = "Gặp Trưởng Làng Eldrin",
                currentProgress = 0,
                targetProgress = 1,
                rewardGold = 50,
                rewardExp = 100,
                rewardRP = 0,
                state = QuestState.InProgress
            });

            // Q2: Đánh Bù Nhìn Gỗ
            questList.Add(new QuestData
            {
                id = "q_dummy_training",
                title = "Rèn Luyện Kỹ Năng",
                description = "Hãy đến bãi tập võ Làng (-76, -12) và tung đòn đánh/kỹ năng vào Bù Nhìn Gỗ 5 lần để làm quen chiến đấu.",
                progressLabel = "Tấn công Bù Nhìn Gỗ",
                currentProgress = 0,
                targetProgress = 5,
                rewardGold = 100,
                rewardExp = 200,
                rewardRP = 0,
                state = QuestState.NotStarted
            });

            // Q3: Tiêu diệt Chiến Binh Quái
            questList.Add(new QuestData
            {
                id = "q_slay_warrior",
                title = "Bảo Vệ Biên Giới",
                description = "Rời khỏi Cổng Làng ra bìa rừng (18, 4) và tiêu diệt 2 Chiến Binh Quái đang tuần tra đe dọa người dân.",
                progressLabel = "Hạ Chiến Binh Quái",
                currentProgress = 0,
                targetProgress = 2,
                rewardGold = 250,
                rewardExp = 450,
                rewardRP = 0,
                state = QuestState.NotStarted
            });

            // Q4: Khám phá Cổng Hầm Ngục Di Tích
            questList.Add(new QuestData
            {
                id = "q_explore_dungeon",
                title = "Thám Hiểm Cổ Tích",
                description = "Vượt qua dòng sông ma thuật đến Thung Lũng Tàn Tích và tìm kiếm Cổng Hầm Ngục Di Tích tại (120, 25).",
                progressLabel = "Tiến đến Cổng Hầm Ngục",
                currentProgress = 0,
                targetProgress = 1,
                rewardGold = 500,
                rewardExp = 1000,
                rewardRP = 1,
                state = QuestState.NotStarted
            });

            currentActiveIndex = 0;
        }

        private void Update()
        {
            // Phím [L] mở / đóng Sổ Tay Nhiệm Vụ
            if (Input.GetKeyDown(KeyCode.L))
            {
                IsQuestLogOpen = !IsQuestLogOpen;
                if (IsQuestLogOpen) AudioManager.PlayPickup();
            }

            // Kiểm tra nhiệm vụ thám hiểm Cổng Hầm Ngục khi người chơi đến gần
            var cur = ActiveQuest;
            if (cur != null && cur.id == "q_explore_dungeon" && cur.state == QuestState.InProgress)
            {
                var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hero");
                if (player != null && Vector2.Distance(player.transform.position, new Vector2(120f, 25f)) <= 4.5f)
                {
                    ReportProgress("reach_dungeon", 1);
                }
            }
        }

        public void ReportProgress(string actionType, int amount = 1)
        {
            var cur = ActiveQuest;
            if (cur == null || cur.state != QuestState.InProgress) return;

            bool isUpdated = false;

            if (cur.id == "q_welcome" && actionType == "talk_elder")
            {
                cur.currentProgress = Mathf.Min(cur.targetProgress, cur.currentProgress + amount);
                isUpdated = true;
            }
            else if (cur.id == "q_dummy_training" && actionType == "dummy_hit")
            {
                cur.currentProgress = Mathf.Min(cur.targetProgress, cur.currentProgress + amount);
                isUpdated = true;
            }
            else if (cur.id == "q_slay_warrior" && actionType == "monster_kill")
            {
                cur.currentProgress = Mathf.Min(cur.targetProgress, cur.currentProgress + amount);
                isUpdated = true;
            }
            else if (cur.id == "q_explore_dungeon" && actionType == "reach_dungeon")
            {
                cur.currentProgress = Mathf.Min(cur.targetProgress, cur.currentProgress + amount);
                isUpdated = true;
            }

            if (isUpdated)
            {
                if (cur.currentProgress >= cur.targetProgress)
                {
                    cur.state = QuestState.CanTurnIn;
                    AudioManager.PlayLevelUp();
                    FloatingDamageManager.ShowFloatingText(Vector3.up * 1.5f, $"✨ Hoàn thành mục tiêu: {cur.title}!", Color.green);
                }
                else
                {
                    AudioManager.PlayHit();
                }
            }
        }

        public void CompleteActiveQuest()
        {
            var cur = ActiveQuest;
            if (cur == null || cur.state != QuestState.CanTurnIn) return;

            cur.state = QuestState.Completed;

            var stats = FindAnyObjectByType<PlayerStats>();
            if (stats != null)
            {
                stats.Gold += cur.rewardGold;
                stats.GainExp(cur.rewardExp);
                if (cur.rewardRP > 0) stats.RebirthPoints += cur.rewardRP;

                FloatingDamageManager.ShowFloatingText(stats.transform.position + Vector3.up * 1.5f,
                    $"🏆 Hoàn thành: {cur.title}! (+{cur.rewardGold} Vàng, +{cur.rewardExp} EXP)", Color.yellow);
            }

            AudioManager.PlayLevelUp();

            // Tự động nhận nhiệm vụ kế tiếp
            if (currentActiveIndex + 1 < questList.Count)
            {
                currentActiveIndex++;
                questList[currentActiveIndex].state = QuestState.InProgress;
                Debug.Log($"<color=cyan>[QUEST]</color> Đã mở khóa nhiệm vụ kế tiếp: {questList[currentActiveIndex].title}");
            }
        }

        private void OnGUI()
        {
            DrawQuestTrackerHUD();

            if (IsQuestLogOpen)
            {
                DrawQuestLogModal();
            }
        }

        /// <summary>
        /// Widget nhỏ gọn góc phải theo dõi nhiệm vụ (Dưới Giờ Thế Giới & Minimap)
        /// </summary>
        private void DrawQuestTrackerHUD()
        {
            float w = 175f;
            float h = 64f;
            float x = Screen.width - w - 12f;
            float y = 158f; // Nằm dưới Giờ Thế Giới

            Rect r = new Rect(x, y, w, h);

            // Nền tối viền vàng
            GUI.color = new Color(0.04f, 0.07f, 0.12f, 0.88f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(r);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.25f) }
            };
            GUI.Label(new Rect(x + 8, y + 4, w - 16, 18), "📜 NHIỆM VỤ [L]", titleStyle);

            var cur = ActiveQuest;
            if (cur != null)
            {
                var qNameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = cur.state == QuestState.CanTurnIn ? Color.green : Color.white }
                };
                GUI.Label(new Rect(x + 8, y + 22, w - 16, 18), $"• {cur.title}", qNameStyle);

                var progStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    normal = { textColor = new Color(0.75f, 0.85f, 0.95f) }
                };
                string progText = (cur.state == QuestState.CanTurnIn) ? "✨ Bấm trả nhiệm vụ!" : $"{cur.progressLabel}: {cur.currentProgress}/{cur.targetProgress}";
                GUI.Label(new Rect(x + 8, y + 40, w - 16, 18), progText, progStyle);

                // Click vào widget để mở Sổ tay
                if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                {
                    if (cur.state == QuestState.CanTurnIn) CompleteActiveQuest();
                    else IsQuestLogOpen = !IsQuestLogOpen;
                }
            }
            else
            {
                var doneStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } };
                GUI.Label(new Rect(x + 8, y + 28, w - 16, 18), "Đã hoàn thành toàn bộ!", doneStyle);
            }
        }

        /// <summary>
        /// Sổ Tay Nhiệm Vụ (Quest Log Modal Phím L)
        /// </summary>
        private void DrawQuestLogModal()
        {
            GUI.depth = -220;

            float boxW = 500f;
            float boxH = 380f;
            float boxX = (Screen.width - boxW) / 2f;
            float boxY = (Screen.height - boxH) / 2f;
            Rect boxRect = new Rect(boxX, boxY, boxW, boxH);

            // Nền hộp
            GUI.color = new Color(0.05f, 0.07f, 0.12f, 0.96f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            DrawDoubleGoldFrame(boxRect);

            // Tiêu đề
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.25f) }
            };
            GUI.Label(new Rect(boxX, boxY + 16, boxW, 28), "📜 SỔ TAY NHIỆM VỤ THẾ GIỚI [L]", titleStyle);

            float rowY = boxY + 54;
            for (int i = 0; i < questList.Count; i++)
            {
                var q = questList[i];
                Rect rowRect = new Rect(boxX + 20, rowY + i * 68, boxW - 40, 60);

                GUI.color = (i == currentActiveIndex) ? new Color(0.10f, 0.16f, 0.24f, 0.9f) : new Color(0.06f, 0.09f, 0.14f, 0.75f);
                GUI.DrawTexture(rowRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                DrawDoubleGoldFrame(rowRect);

                Color stateCol = (q.state == QuestState.Completed) ? Color.gray :
                                 (q.state == QuestState.CanTurnIn) ? Color.green :
                                 (q.state == QuestState.InProgress) ? new Color(1f, 0.85f, 0.35f) : Color.white;

                var qTitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = stateCol } };
                string stateIcon = (q.state == QuestState.Completed) ? "✅ Đã xong" :
                                   (q.state == QuestState.CanTurnIn) ? "✨ Hoàn tất" :
                                   (q.state == QuestState.InProgress) ? "⏳ Đang làm" : "🔒 Chưa mở";

                GUI.Label(new Rect(rowRect.x + 10, rowRect.y + 6, rowRect.width - 120, 20), $"{i + 1}. {q.title} ({stateIcon})", qTitleStyle);

                var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = new Color(0.8f, 0.85f, 0.9f) } };
                GUI.Label(new Rect(rowRect.x + 10, rowRect.y + 24, rowRect.width - 120, 18), q.description, descStyle);

                string rewardStr = $"Thưởng: {q.rewardGold}V · {q.rewardExp}EXP" + (q.rewardRP > 0 ? $" · {q.rewardRP}RP" : "");
                var rewardStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.yellow } };
                GUI.Label(new Rect(rowRect.x + 10, rowRect.y + 40, rowRect.width - 120, 16), rewardStr, rewardStyle);

                // Nút Nhận Thưởng
                if (q.state == QuestState.CanTurnIn)
                {
                    GUI.color = Color.green;
                    if (GUI.Button(new Rect(rowRect.x + rowRect.width - 105, rowRect.y + 16, 95, 28), "Nhận Thưởng"))
                    {
                        CompleteActiveQuest();
                    }
                    GUI.color = Color.white;
                }
            }

            // Nút Đóng
            if (GUI.Button(new Rect(boxX + (boxW - 130f) / 2f, boxY + boxH - 42, 130f, 30), "Đóng [L]"))
            {
                IsQuestLogOpen = false;
            }
        }

        private void DrawDoubleGoldFrame(Rect r)
        {
            GUI.color = new Color(0.85f, 0.72f, 0.28f, 0.95f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, 2, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 2, r.y, 2, r.height), Texture2D.whiteTexture);

            GUI.color = new Color(0.45f, 0.35f, 0.15f, 0.85f);
            GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, r.width - 4, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + 2, r.y + r.height - 3, r.width - 4, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, 1, r.height - 4), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - 3, r.y + 2, 1, r.height - 4), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
