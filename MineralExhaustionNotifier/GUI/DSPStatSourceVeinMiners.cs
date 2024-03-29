﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DSPPlugins_ALT.Statistics;

namespace DSPPlugins_ALT.GUI
{
    public class DSPStatSourceVeinMiners : DSPStatSource
    {
        public IEnumerable<MinerNotificationDetail> Source;
        public Dictionary<eTAB_TYPES, IEnumerable<MinerNotificationDetail>> FilteredSource = new Dictionary<eTAB_TYPES, IEnumerable<MinerNotificationDetail>>();
        public Dictionary<eTAB_TYPES, IList<Filter<MinerNotificationDetail>>> TabFilters = new Dictionary<eTAB_TYPES, IList<Filter<MinerNotificationDetail>>>();

        public DSPStatSourceVeinMiners()
        {
            TABPages = new List<eTAB_TYPES>() { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_NETWORK, eTAB_TYPES.TAB_RESOURCE };
            foreach (eTAB_TYPES tabType in TABPages)
            {
                TabFilters[tabType] = new List<Filter<MinerNotificationDetail>>();
                TabFilterInfo[tabType] = new TabFilterInfo();
            }

            InitFilters();
            UpdateSource();

            DefaultCollapsedStateLevel[1] = false;
            DefaultCollapsedStateLevel[2] = true;
            DefaultCollapsedStateLevel[3] = false;

            MaxCollapseLevel[eTAB_TYPES.TAB_PLANET] = 2;
            MaxCollapseLevel[eTAB_TYPES.TAB_NETWORK] = 2;
            MaxCollapseLevel[eTAB_TYPES.TAB_RESOURCE] = 2;
        }
        public override void UpdateSource()
        {
            // Source = DSPStatistics.notificationList.Values.SelectMany(x => x).ToList();
            
            if (ShouldAutoUpdate)
            {
                Source = DSPStatistics.minerStats;
            }
            else
            {
                Source = DSPStatistics.minerStats.ToList();
            }

            foreach (var tbFilterKV in TabFilters)
            {
                var newFilteredSource = Source;
                TabFilterInfo[tbFilterKV.Key].ItemsBefore = newFilteredSource.Count();
                foreach (var filter in tbFilterKV.Value.Where(f => f.enabled == true))
                {
                    // Debug.Log($"{this.GetType().Name} : Filter : {filter.name} : PreCount= {newFilteredSource.Count()}");
                    newFilteredSource = filter.LINQFilter(filter, newFilteredSource);
                    // Debug.Log($"{this.GetType().Name} : Filter : {filter.name} : PostCount= {newFilteredSource.Count()}");
                }
                FilteredSource[tbFilterKV.Key] = newFilteredSource;
                TabFilterInfo[tbFilterKV.Key].ItemsAfter = newFilteredSource.Count();
            }
        }

        public void InitFilters()
        {
            Filter<MinerNotificationDetail> miningRateFilter = new Filter<MinerNotificationDetail>()
            {
                name = "miningRateFilter",
                enabled = false,
                value = 100,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                    var enabled = GUILayout.Toggle(filter.enabled, $"Mining Rate: <{filter.value.ToString("F0")}");
                    var value = GUILayout.HorizontalSlider(filter.value, 0, 1000);
                    var shouldUpdateFiltered = (filter.enabled != enabled || filter.value != value);
                    filter.enabled = enabled;
                    filter.value = value;
                    GUILayout.EndVertical();
                    return shouldUpdateFiltered;
                },
                LINQFilter = (filter, source) =>
                {
                    return source.Where(miner => miner.miningRatePerMin < filter.value);
                }
            };

            Filter<MinerNotificationDetail> miningRateSummedFilter = new Filter<MinerNotificationDetail>()
            {
                name = "miningRateSummedFilter",
                enabled = false,
                value = 3000,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                    var enabled = GUILayout.Toggle(filter.enabled, $"Mining Rate (Sum): <{filter.value.ToString("F0")}");
                    var value = GUILayout.HorizontalSlider(filter.value, 0, 10000);
                    var shouldUpdateFiltered = (filter.enabled != enabled || filter.value != value);
                    filter.enabled = enabled;
                    filter.value = value;
                    GUILayout.EndVertical();
                    return shouldUpdateFiltered;
                },
                LINQFilter = (filter, source) =>
                {
                    var miners = source
                        .GroupBy(x => x.veinName).OrderBy(g => g.Key)
                        .Select(mtg => new { Name = mtg.Key, Miners = mtg.ToList(), SumMiningPerMin = mtg.Sum(m => m.miningRatePerMin) })
                        .Where(mtg => mtg.SumMiningPerMin < filter.value)
                        .SelectMany(mtg => mtg.Miners);

                    return miners;
                }
            };

            Filter<MinerNotificationDetail> hasAlarmFilter = new Filter<MinerNotificationDetail>()
            {
                name = "hasAlarmFilter",
                enabled = false,
                value = 0,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                    var enabled = GUILayout.Toggle(filter.enabled, $"Has Alarm State");
                        //var value = GUILayout.HorizontalSlider(filter.value, 0, 1000);
                        var shouldUpdateFiltered = (filter.enabled != enabled);
                    filter.enabled = enabled;
                    GUILayout.EndVertical();
                    return shouldUpdateFiltered;
                },
                LINQFilter = (filter, source) =>
                {
                    var miners = source
                        .Where(m => m.signType != SignData.NONE);

                    return miners;
                }
            };
            AddFilterToTabs(miningRateFilter, new List<eTAB_TYPES> { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_NETWORK, eTAB_TYPES.TAB_RESOURCE });
            AddFilterToTabs(miningRateSummedFilter, new List<eTAB_TYPES> { eTAB_TYPES.TAB_RESOURCE });
            AddFilterToTabs(hasAlarmFilter, new List<eTAB_TYPES> { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_NETWORK, eTAB_TYPES.TAB_RESOURCE });
        }

        public void AddFiltersToTab(eTAB_TYPES tab, IEnumerable<Filter<MinerNotificationDetail>> filters)
        {
            foreach (var filter in filters)
            {
                TabFilters[tab].Add(filter);
            }
        }
        public void AddFilterToTabs(Filter<MinerNotificationDetail> filter, IEnumerable<eTAB_TYPES> tabs)
        {
            foreach (var tab in tabs)
            {
                TabFilters[tab].Add(filter);
            }
        }

        public override void DrawFilterGUI(eTAB_TYPES selectedTab)
        {
            bool shouldUpdateFiltered = false;
            GUILayout.BeginVertical(GUILayout.MaxWidth(80));
            GUILayout.Label($"<b>Filters</b>", UITheme.TextAlignStyle);
            GUILayout.Label($"Showing: ({TabFilterInfo[selectedTab].ItemsAfter}/{TabFilterInfo[selectedTab].ItemsBefore})", UITheme.TextAlignStyle);
            GUILayout.EndVertical();

            foreach (var filter in TabFilters[selectedTab])
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(150));
                shouldUpdateFiltered |= filter.onGUI(filter);
                GUILayout.EndHorizontal();
            }
            if (shouldUpdateFiltered)
            {
                UpdateSource();
            }
        }

        public void DrawVeinMinersHeader(bool includePlanet = false)
        {
            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box, GUILayout.MaxHeight(45));
            if (includePlanet)
            {
                GUILayout.Label($"<b>Planet</b>", UITheme.TextAlignStyle, UITheme.PlanetColWidth);
            }
            GUILayout.Label($"<b>Location</b>", UITheme.TextAlignStyle, UITheme.LocationColWidth);
            GUILayout.Label($"<b>W-State</b>", UITheme.TextAlignStyle, UITheme.LocationColWidth);
            GUILayout.Label($"<b>Alarm</b>", UITheme.TextAlignStyle, UITheme.AlarmColWidth);
            GUILayout.Label($"<b>Vein</b>", UITheme.TextAlignStyle, UITheme.VeinTypeColWidth);
            GUILayout.Label($"<b>Amount \nLeft</b>", UITheme.TextAlignStyle, UITheme.VeinAmountColWidth);
            GUILayout.Label($"<b>Mining Rate/min</b>", UITheme.TextAlignStyle, UITheme.VeinRateColWidth);
            GUILayout.Label($"<b>~Time to Empty</b>", UITheme.TextAlignStyle, UITheme.VeinETAColWidth);
            GUILayout.EndHorizontal();
        }

        void DrawVeinMiners(IEnumerable<MinerNotificationDetail> miners, string parentId, bool includePlanet = false)
        {
            GUIStyle boxStyle = new GUIStyle(UnityEngine.GUI.skin.box)
            {
                margin = new RectOffset(5, 0, 0, 0)
            };

            foreach (var item in miners)
            {
                var myId = parentId + "." + item.minerComponent.id;
                var alarmSign = (item.signType != SignData.NONE) ? UITheme.Sign_state[DSPHelper.SignNumToTextureIndex(item.signType)] : Texture2D.blackTexture;
                var resourceTexture = item.resourceTexture ? item.resourceTexture : Texture2D.blackTexture;

                // Debug.Log("Drawing sign-state-" + item.signType);

                GUILayout.BeginHorizontal(boxStyle, GUILayout.MaxHeight(45));
                if (includePlanet)
                {
                    GUILayout.Label($"{item.planetName}", UITheme.TextAlignStyle, UITheme.PlanetColWidth);
                }
                GUILayout.Label($"{DSPHelper.PositionToLatLon(item.plantPosition)}", UITheme.TextAlignStyle, UITheme.LocationColWidth);
                GUILayout.Label($"{DSPHelper.WorkStateToText(item.minerComponent.workstate, item.consumerRatio)}", UITheme.TextAlignStyle, UITheme.LocationColWidth);

                GUILayout.BeginHorizontal(UITheme.VeinTypeColWidth);
                GUILayout.Box(alarmSign, UITheme.VeinIconLayoutOptions);
                GUILayout.Label(DSPHelper.SignNumToText(item.signType), UITheme.TextAlignStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(UITheme.VeinTypeColWidth);
                GUILayout.Box(resourceTexture, UITheme.VeinIconLayoutOptions);
                GUILayout.Label($"{item.veinName}", UITheme.TextAlignStyle);
                GUILayout.EndHorizontal();

                GUILayout.Label($"{item.veinAmount}", UITheme.TextAlignStyle, UITheme.VeinAmountColWidth);
                GUILayout.Label($"{Math.Round(item.miningRatePerMin, 0)}", UITheme.TextAlignStyle, UITheme.VeinRateColWidth);
                GUILayout.Label($"{item.minutesToEmptyVeinTxt}", UITheme.TextAlignStyle, UITheme.VeinETAColWidth);

                GUILayout.EndHorizontal();
            }
        }


        public override void DrawTabGUI(eTAB_TYPES selectedTab)
        {
            var parentId = eTAB_SOURCE_TYPE.VeinMeiners + "." + selectedTab;

            var minersByPlanet = FilteredSource[selectedTab]
                    .OrderBy(m => m.minutesToEmptyVein)
                    .GroupBy(m => m.planetName).OrderBy(g => g.Key);

            if (selectedTab == eTAB_TYPES.TAB_PLANET)
            {
                foreach (var planet in minersByPlanet)
                {
                    var myPlanetId = parentId + "." + planet.Key;
                    var resourcesByPlanet = planet
                        .GroupBy(miner => miner.itemProto).OrderBy(g => g.Key.name.Translate())
                        .Select(mtg => new { Name = mtg.Key.name.Translate(), Tex = mtg.First().resourceTexture, Miners = mtg.ToList(), SumMiningPerMin = mtg.Sum(m => m.miningRatePerMin) });


                    GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                    DrawCollapsedChildrenChevron(myPlanetId, out bool childrenCollapsed);
                    GUILayout.Label($"<b>Planet {planet.Key}</b>", UITheme.TextAlignStyle);
                    GUILayout.Label($"  <b>Number of resource types: {resourcesByPlanet.Count()}</b>", UITheme.TextAlignStyle);
                    GUILayout.EndHorizontal();

                    if (!childrenCollapsed)
                    {
                        foreach (var resource in resourcesByPlanet)
                        {
                            var myResId = myPlanetId + "." + resource.Name;
                            var resourceTexture = resource.Tex ? resource.Tex : Texture2D.blackTexture;

                            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                            DrawCollapsedChildrenChevron(myResId, out bool resourceChildrenCollapsed);
                            GUILayout.Label($"<b>Resource</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                            GUILayout.Box(resourceTexture, UITheme.VeinIconLayoutOptions);
                            GUILayout.Label($"<b>{resource.Name}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                            GUILayout.Label($"  <b># Miners: {resource.Miners.Count()}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                            GUILayout.Label($"  <b>Sum Mining Rate: {resource.SumMiningPerMin.ToString("F0")}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                            GUILayout.EndHorizontal();

                            if (!resourceChildrenCollapsed)
                            {
                                DrawVeinMinersHeader();
                                DrawVeinMiners(resource.Miners, parentId: myResId);
                            }
                          
                        }

                    }

                }
            }
            else if (selectedTab == eTAB_TYPES.TAB_NETWORK)
            {
                foreach (var planet in minersByPlanet)
                {
                    var myId = parentId + "." + planet.Key;
                    GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                    DrawCollapsedChildrenChevron(myId, out bool childrenCollapsed);
                    GUILayout.Label($"<b>Planet {planet.Key}</b>", UITheme.TextAlignStyle);
                    GUILayout.EndHorizontal();

                    if (!childrenCollapsed)
                    {
                        var planetNetGroups = planet
                        .OrderBy(m => m.veinAmount)
                        .GroupBy(m => m.powerNetwork)
                        .OrderBy(ng => ng.Key.id);

                        foreach (var netGroup in planetNetGroups)
                        {
                            var myNetId = parentId + "." + netGroup.Key.id;
                            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                            DrawCollapsedChildrenChevron(myNetId, out bool netChildrenCollapsed);
                            GUILayout.Label($"<b>PowerNetwork {netGroup.Key.id} - Health: {Math.Round(netGroup.Key.consumerRatio * 100, 0) }</b>", UITheme.TextAlignStyle, GUILayout.Width(260));
                            GUILayout.EndHorizontal();

                            if (!netChildrenCollapsed)
                            {
                                DrawVeinMinersHeader();
                                DrawVeinMiners(netGroup, parentId: myId);
                            }
                        }
                    }
                }
            }
            else if (selectedTab == eTAB_TYPES.TAB_RESOURCE)
            {
                var miners = FilteredSource[selectedTab]
                                    .OrderBy(miner => miner.veinAmount)
                                    .GroupBy(x => x.veinName).OrderBy(g => g.Key)
                                    .Select(mtg => new { Name = mtg.Key, Tex = mtg.First().resourceTexture, Miners = mtg.ToList(), SumMiningPerMin = mtg.Sum(m => m.miningRatePerMin) });

                foreach (var resourceGroup in miners)
                {
                    var myResId = parentId + "." + resourceGroup.Name;
                    var resourceTexture = resourceGroup.Tex ? resourceGroup.Tex : Texture2D.blackTexture;

                    GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                    DrawCollapsedChildrenChevron(myResId, out bool childrenCollapsed);
                    GUILayout.Label($"<b>Resource</b>", UITheme.TextAlignStyle, GUILayout.Width(65));
                    GUILayout.Box(resourceTexture, UITheme.VeinIconLayoutOptions);
                    GUILayout.Label($"<b>{resourceGroup.Name}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                    GUILayout.Label($"  <b>Sum Mining Rate: {resourceGroup.SumMiningPerMin.ToString("F0")}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                    GUILayout.EndHorizontal();

                    var minersByPlanetByResource = resourceGroup.Miners
                        .OrderBy(m => m.minutesToEmptyVein)
                        .GroupBy(m => m.planetName).OrderBy(g => g.Key)
                        .Select(mtg => new { Name = mtg.Key, Miners = mtg.ToList(), SumMiningPerMin = mtg.Sum(m => m.miningRatePerMin) });

                    if (!childrenCollapsed)
                    {
                        foreach (var minersPlanet in minersByPlanetByResource)
                        {
                            var myMinersByPlanetId = myResId + "." + minersPlanet.Name;

                            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                            DrawCollapsedChildrenChevron(myMinersByPlanetId, out bool minersByPlanetChildrenCollapsed);
                            GUILayout.Label($"<b>Planet {minersPlanet.Name}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                            GUILayout.Label($"  <b># Miners: {minersPlanet.Miners.Count()}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                            GUILayout.Label($"  <b>Sum Mining Rate: {minersPlanet.SumMiningPerMin.ToString("F0")}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                            GUILayout.EndHorizontal();

                            if (!minersByPlanetChildrenCollapsed)
                            {
                                DrawVeinMinersHeader(includePlanet: false);
                                DrawVeinMiners(minersPlanet.Miners, parentId: myMinersByPlanetId, includePlanet: false);
                            }
                        }
                    }
                }
            }
        }

    }
}
