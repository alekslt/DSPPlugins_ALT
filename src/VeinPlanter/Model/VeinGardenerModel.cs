using System.Collections.Generic;
using UnityEngine;

namespace VeinPlanter.Model
{
    public class VeinGardenerModel
    {
        public static bool ShowModeMenu = false;
        public static bool ShowPlanetVeinMenu = false;

        public float oreMultiplier = 100000000f;

        public List<int> products = new List<int>() { 1001, 1002, 1003, 1004, 1005, 1006, 1030, 1031, 1011, 1012, 1013, 1014, 1015, 1016, 1101, 1104, 1105, 1106, 1108, 1109, 1103, 1107, 1110, 1119, 1111, 1112, 1113, 1201, 1102, 1202, 1203, 1204, 1205, 1206, 1127, 1301, 1303, 1305, 1302, 1304, 1402, 1401, 1404, 1501, 1000, 1007, 1114, 1116, 1120, 1121, 1122, 1208, 1801, 1802, 1803, 1115, 1123, 1124, 1117, 1118, 1126, 1209, 1210, 1403, 1405, 1406, 5001, 5002, 1125, 1502, 1503, 1131, 1141, 1142, 1143, 2001, 2002, 2003, 2011, 2012, 2013, 2020, 2101, 2102, 2106, 2303, 2304, 2305, 2201, 2202, 2212, 2203, 2204, 2211, 2301, 2302, 2307, 2308, 2306, 2309, 2314, 2313, 2205, 2206, 2207, 2311, 2208, 2312, 2209, 2310, 2210, 2103, 2104, 2105, 2901, 6001, 6002, 6003, 6004, 6005, 6006 };

        public EVeinModificationMode modMode = EVeinModificationMode.Deactivated;

        public PlanetData localPlanet;
        public int veinGroupIndex;

        public EVeinType newVeinGroupType = EVeinType.Iron;

        public Vector3[] reformPoints = new Vector3[100];

        public void ChangeMode(EVeinModificationMode newmode)
        {
            modMode = newmode;
            ShowModeMenu = false;
            UIRealtimeTip.Popup("Vein Gardener Mode Changed to: " + modMode);
        }
    }

    
}
