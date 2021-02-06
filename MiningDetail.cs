using UnityEngine;

namespace DSPHelloWorld
{
    public partial class MineralExhaustionNotifier
    {
        public struct MiningDetail
        {
            public string planetName;
            public string veinName;
            public int veinAmount;
            public string latLon;
            internal uint signType;
            internal PlanetFactory factory;
            internal Vector3 plantPosition;
        }
    }
}
