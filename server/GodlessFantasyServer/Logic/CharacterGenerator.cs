using System;
using GodlessFantasyServer.Network;

namespace GodlessFantasyServer.Logic
{
    public static class CharacterGenerator
    {
        private static readonly string[] Elements = new[]
        {
            "Hỏa",      // Lửa rực cháy
            "Băng",     // Băng giá vĩnh cửu
            "Lôi",      // Sấm sét cuồng nộ
            "Phong",    // Gió lốc tự do
            "Mộc",      // Mầm sống sinh mệnh
            "Quang",    // Thánh quang rực rỡ
            "Ám"        // Hắc ám ma mị
        };

        private static readonly Random Rand = new();

        /// <summary>
        /// Đổ xúc xắc ngẫu nhiên Hệ Nguyên Tố & Bộ Chỉ Số Khởi Đầu (40 điểm tiềm năng)
        /// Tuân thủ nghiêm ngặt GDD DOC-03
        /// </summary>
        public static CharRollStatsRespDto RollStats(string className)
        {
            // 1. Random 1 trong 7 Hệ Nguyên Tố
            string elem = Elements[Rand.Next(Elements.Length)];

            // 2. Base 40 điểm thuộc tính, tùy biến nhẹ theo Chức Nghiệp
            int str = 8;
            int intStat = 8;
            int vit = 8;
            int agi = 8;
            int remaining = 8; // Tổng khởi điểm = 32 + 8 = 40 điểm

            switch (className)
            {
                case "Kị Sĩ":
                    str += 3;
                    vit += 3;
                    remaining -= 6;
                    break;
                case "Pháp Sư":
                    intStat += 4;
                    vit += 1;
                    remaining -= 5;
                    break;
                case "Xạ Thủ":
                    agi += 4;
                    str += 1;
                    remaining -= 5;
                    break;
                case "Võ Sĩ":
                    str += 2;
                    agi += 2;
                    vit += 2;
                    remaining -= 6;
                    break;
            }

            // Phân bổ ngẫu nhiên điểm còn lại (xúc xắc may mắn)
            for (int i = 0; i < remaining; i++)
            {
                int pick = Rand.Next(4);
                if (pick == 0) str++;
                else if (pick == 1) intStat++;
                else if (pick == 2) vit++;
                else agi++;
            }

            return new CharRollStatsRespDto
            {
                element = elem,
                str = str,
                intStat = intStat,
                vit = vit,
                agi = agi,
                totalPoints = str + intStat + vit + agi
            };
        }
    }
}
