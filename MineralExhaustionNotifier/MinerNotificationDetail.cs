using UnityEngine;

namespace DSPPlugins_ALT
{
    public struct MinerNotificationDetail
    {
        public string planetName;
        public string veinName;
        public int veinAmount;
        public string latLon;
        internal uint signType;
        internal PlanetFactory factory;
        internal Vector3 plantPosition;
        internal int entityId;
    }
}
