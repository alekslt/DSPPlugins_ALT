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
        internal ItemProto itemProto;
        internal float miningRate;
        internal int time;
        internal int period;
        internal int veinCount;
        internal float miningRatePerMin;
        internal string minutesToEmptyVeinTxt;
        internal Texture2D resourceTexture;
        internal PowerNetwork powerNetwork;
        internal MinerComponent minerComponent;
        internal float consumerRatio;
    }
}
