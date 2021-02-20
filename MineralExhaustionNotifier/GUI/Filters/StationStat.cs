using System.Collections.Generic;
using UnityEngine;

namespace DSPPlugins_ALT.Statistics
{
    public class StationStat
    {
        internal IList<StationItemStat> products;
        internal PlanetData planetData;
        internal string name;
        internal StationComponent stationComponent;
        internal Vector3 stationPosition;
    };
}
