using System.Collections.Generic;

namespace DSPPlugins_ALT.GUI
{
    public enum eTAB_SOURCE_TYPE { VeinMeiners, LogisticStations };
    public enum eTAB_TYPES { TAB_PLANET, TAB_NETWORK, TAB_RESOURCE, TAB_LOGISTICS };

    public abstract class DSPStatSource
    {
        public IList<eTAB_TYPES> TABPages;
        public Dictionary<eTAB_TYPES, TabFilterInfo> TabFilterInfo = new Dictionary<eTAB_TYPES, TabFilterInfo>();

        public abstract void UpdateSource();
        public abstract void DrawFilterGUI(eTAB_TYPES selectedTab);
        public abstract void DrawTabGUI(eTAB_TYPES selectedTab);

        public bool ShouldAutoUpdate { get; set; } = false;

    }
}
