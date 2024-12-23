using Logistics_service.Models;
using System.Text.Json;

namespace Logistics_service.Services
{
    public static class GenerateMap
    {
        public static Point[]? Points { get; private set; }

        static GenerateMap()
        {
            var text = File.ReadAllText("points.json");

            Points = JsonSerializer.Deserialize<Point[]>(text, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static void SaveMap()
        {
            var points = new Point[]
            {
                new Point("0", 68, 33),
                new Point("1", 12, 5),
                new Point("2", 11, 20),
                new Point("3", 4, 17),
                new Point("4", 8, 41),
                new Point("5", 2, 37),
                new Point("6", 6, 50),
                new Point("7", 16, 41),
                new Point("8", 4, 61),
                new Point("9", 3, 88),
                new Point("10", 5, 72),
                new Point("11", 5, 77),
                new Point("12", 9, 80),
                new Point("13", 10, 75),
                new Point("14", 17, 11),
                new Point("15", 25, 16),
                new Point("16", 38, 22),
                new Point("17", 25, 9),
                new Point("18", 39, 12),
                new Point("19", 37, 31),
                new Point("20", 23, 21),
                new Point("21", 21, 33),
                new Point("22", 19, 49),
                new Point("23", 40, 8),
                new Point("24", 57, 12),
                new Point("25", 59, 1),
                new Point("26", 65, 8),
                new Point("27", 65, 12),
                new Point("28", 62, 22),
                new Point("29", 61, 27),
                new Point("30", 74, 23),
                new Point("31", 85, 5),
                new Point("32", 79, 52),
                new Point("33", 71, 55),
                new Point("34", 69, 50),
                new Point("35", 59, 40),
                new Point("36", 70, 35),
                new Point("37", 92, 52),
                new Point("38", 91, 63),
                new Point("39", 97, 19),
                new Point("40", 87, 11),
                new Point("41", 96, 72),
                new Point("42", 92, 90),
                new Point("43", 86, 89),
                new Point("44", 79, 88),
                new Point("45", 75, 97),
                new Point("46", 70, 40),
                new Point("47", 67, 67),
                new Point("48", 62, 59),
                new Point("49", 55, 52),
                new Point("50", 46, 42),
                new Point("51", 49, 31),
                new Point("52", 90, 13),
                new Point("53", 48, 9),
                new Point("54", 66, 85),
                new Point("55", 65, 94),
                new Point("56", 66, 74),
                new Point("57", 61, 72),
                new Point("58", 53, 70),
                new Point("59", 47, 68),
                new Point("60", 41, 66),
                new Point("61", 36, 64),
                new Point("62", 48, 57),
                new Point("63", 36, 70),
                new Point("64", 41, 72),
                new Point("65", 47, 74),
                new Point("66", 36, 75),
                new Point("67", 40, 77),
                new Point("68", 46, 79),
                new Point("69", 52, 80),
                new Point("70", 60, 82),
                new Point("71", 40, 84),
                new Point("72", 46, 87),
                new Point("73", 52, 88),
                new Point("74", 59, 90),
                new Point("75", 42, 86),
                new Point("76", 46, 91),
                new Point("77", 59, 95),
                new Point("78", 42, 90),
                new Point("79", 51, 92),
                new Point("80", 45, 95),
                new Point("81", 58, 99),
                new Point("82", 42, 94),
                new Point("83", 51, 97),
                new Point("84", 37, 87),
                new Point("85", 36, 92),
                new Point("86", 32, 86),
                new Point("87", 33, 91),
                new Point("88", 33, 80),
                new Point("89", 27, 76),
                new Point("90", 25, 88),
                new Point("91", 34, 75),
                new Point("92", 34, 70),
                new Point("93", 31, 28),
                new Point("94", 32, 20),
                new Point("95", 76, 54),
                new Point("96", 74, 41),
                new Point("97", 72, 19),
                new Point("98", 83, 1),
                new Point("99", 93, 57),
                new Point("100", 6, 63),
            };

            for (int i = 0; i < points.Length; i++)
            {
                points[i].Index = i;
            }

            points[0].AddPoint(30, 36);
            points[1].AddPoint(14, 2);
            points[2].AddPoint(3, 1, 4);
            points[3].AddPoint(2);
            points[4].AddPoint(2, 5, 6);
            points[5].AddPoint(4);
            points[6].AddPoint( 4, 8, 7, 100);
            points[7].AddPoint(6);
            points[8].AddPoint(6, 100, 9);
            points[9].AddPoint(11, 8);
            points[10].AddPoint(100, 11, 13);
            points[11].AddPoint(9, 10, 12);
            points[12].AddPoint(11, 13);
            points[13].AddPoint(10, 12);
            points[14].AddPoint(1, 15, 20);
            points[15].AddPoint(17, 14, 94);
            points[16].AddPoint(18, 19, 94);
            points[17].AddPoint(15, 18);
            points[18].AddPoint(16, 17, 23);
            points[19].AddPoint(16, 50, 92, 93);
            points[20].AddPoint(14, 93, 21);
            points[21].AddPoint(20, 22);
            points[22].AddPoint(21, 36);
            points[23].AddPoint(18, 53);
            points[24].AddPoint(25, 53);
            points[25].AddPoint(24, 26);
            points[26].AddPoint(25, 27);
            points[27].AddPoint(26, 28, 97);
            points[28].AddPoint(27);
            points[29].AddPoint(35);
            points[30].AddPoint(0, 31, 32, 97);
            points[31].AddPoint(30, 40, 98);
            points[32].AddPoint(30, 37, 95);
            points[33].AddPoint(34, 95);
            points[34].AddPoint(33, 35, 46);
            points[35].AddPoint(34, 29);
            points[36].AddPoint(0, 46, 96);
            points[37].AddPoint(32, 39, 99);
            points[38].AddPoint(41, 99);
            points[39].AddPoint(37, 52);
            points[40].AddPoint(31, 52);
            points[41].AddPoint(38, 42);
            points[42].AddPoint(41, 43);
            points[43].AddPoint(42, 44, 45);
            points[44].AddPoint(43, 47, 54);
            points[45].AddPoint(43, 56);
            points[46].AddPoint(34, 36, 96);
            points[47].AddPoint(44, 48, 56);
            points[48].AddPoint(47, 49, 57);
            points[49].AddPoint(48, 50, 58);
            points[50].AddPoint(19, 49, 51);
            points[51].AddPoint(50);
            points[52].AddPoint(39, 40);
            points[53].AddPoint(23, 24);
            points[54].AddPoint(44, 55, 56, 70);
            points[55].AddPoint(45, 54, 74, 77);
            points[56].AddPoint(47, 54, 57);
            points[57].AddPoint(48, 56, 58, 70);
            points[58].AddPoint(49, 57, 59, 69);
            points[59].AddPoint(58, 60, 62, 65);
            points[60].AddPoint(59, 61, 64);
            points[61].AddPoint(60, 63);
            points[62].AddPoint(59);
            points[63].AddPoint(61, 64, 66, 92);
            points[64].AddPoint(60, 63, 65, 67);
            points[65].AddPoint(59, 64, 68);
            points[66].AddPoint(63, 67, 91);
            points[67].AddPoint(64, 66, 68, 71);
            points[68].AddPoint(65, 67, 69, 72);
            points[69].AddPoint(58, 68, 70, 73);
            points[70].AddPoint(54, 57, 69, 74);
            points[71].AddPoint(67, 75, 88);
            points[72].AddPoint(68, 73, 75, 76);
            points[73].AddPoint(69, 72, 74);
            points[74].AddPoint(55, 70, 73, 77);
            points[75].AddPoint(71, 72, 78);
            points[76].AddPoint(72, 78, 79, 80);
            points[77].AddPoint(55, 74, 79, 81);
            points[78].AddPoint(75, 76, 82, 84);
            points[79].AddPoint(76, 77, 83);
            points[80].AddPoint(76, 82, 83);
            points[81].AddPoint(77, 83);
            points[82].AddPoint(78, 80, 85);
            points[83].AddPoint(79, 80, 81);
            points[84].AddPoint(78, 85, 86);
            points[85].AddPoint(82, 84, 87);
            points[86].AddPoint(84, 87, 88);
            points[87].AddPoint(85, 86, 90);
            points[88].AddPoint(71, 86, 89, 91);
            points[89].AddPoint(88, 90);
            points[90].AddPoint(87, 89);
            points[91].AddPoint(66, 88, 92);
            points[92].AddPoint(19, 63, 91);
            points[93].AddPoint(19, 20, 94);
            points[94].AddPoint(15, 19, 93);
            points[95].AddPoint(32, 33, 96);
            points[96].AddPoint(46, 95);
            points[97].AddPoint(27, 30, 98);
            points[98].AddPoint(31, 97);
            points[99].AddPoint(37, 38);
            points[100].AddPoint(6, 8, 10);

            string json = JsonSerializer.Serialize(points, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText("points.json", json);
        }
    }
}
