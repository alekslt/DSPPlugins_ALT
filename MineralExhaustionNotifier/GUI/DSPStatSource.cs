using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DSPPlugins_ALT.GUI
{
    public enum eTAB_SOURCE_TYPE { VeinMeiners, LogisticStations };
    public enum eTAB_TYPES { TAB_PLANET, TAB_NETWORK, TAB_RESOURCE, TAB_LOGISTICS };

    public abstract class DSPStatSource
    {
        public IList<eTAB_TYPES> TABPages;
        public Dictionary<eTAB_TYPES, TabFilterInfo> TabFilterInfo = new Dictionary<eTAB_TYPES, TabFilterInfo>();
        public Dictionary<string, bool> CollapsedState = new Dictionary<string, bool>();
        public Dictionary<eTAB_TYPES, int> MaxCollapseLevel = new Dictionary<eTAB_TYPES, int>();

        public abstract void UpdateSource();
        public abstract void DrawFilterGUI(eTAB_TYPES selectedTab);
        public abstract void DrawTabGUI(eTAB_TYPES selectedTab);

        public bool ShouldAutoUpdate { get; set; } = false;

        /*
        public bool DefaultIsChildrenCollapsedStateLevel1 { get; set; } = false;
        public bool DefaultIsChildrenCollapsedStateLevel2 { get; set; } = false;
        public bool DefaultIsChildrenCollapsedStateLevel3 { get; set; } = true;
        */

        public Dictionary<int, bool> DefaultCollapsedStateLevel = new Dictionary<int, bool>();

        public bool IsItemChildrenCollapsed(string id)
        {
            // LogisticStations.TAB_PLANET.planetId.iron;
            if (CollapsedState.ContainsKey(id))
            {
                return CollapsedState[id];
            }
            var displayLevel = id.Count(f => f == '.') - 1;

            if (displayLevel >= 1 && displayLevel <= 3)
            {
                return DefaultCollapsedStateLevel[displayLevel];
            }
                        
            return false;  
        }

        public void DrawCollapsedChildrenChevron(string myId, out bool childrenCollapsed)
        {
            childrenCollapsed = IsItemChildrenCollapsed(myId);
            if (GUILayout.Button(childrenCollapsed ? ">" : "▼", UITheme.ContainerDisclureIconStyle, UITheme.VeinIconLayoutOptions))
            {
                CollapsedState[myId] = !childrenCollapsed;
            }
        }

    }
}
