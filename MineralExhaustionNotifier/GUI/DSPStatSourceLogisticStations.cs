using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DSPPlugins_ALT.Statistics;

namespace DSPPlugins_ALT.GUI
{
    public class DSPStatSourceLogisticStations : DSPStatSource
    {
        public IEnumerable<ResStationGroup> Source;
        public Dictionary<eTAB_TYPES, IEnumerable<ResStationGroup>> FilteredSource = new Dictionary<eTAB_TYPES, IEnumerable<ResStationGroup>>();
        public Dictionary<eTAB_TYPES, IList<Filter<ResStationGroup>>> TabFilters = new Dictionary<eTAB_TYPES, IList<Filter<ResStationGroup>>>();

        public DSPStatSourceLogisticStations()
        {
            foreach (eTAB_TYPES tabType in Enum.GetValues(typeof(eTAB_TYPES)))
            {
                TabFilters[tabType] = new List<Filter<ResStationGroup>>();
                TabFilterInfo[tabType] = new TabFilterInfo();
            }
            TABPages = new List<eTAB_TYPES>() { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_RESOURCE };
            InitFilters();
            UpdateSource();

            DefaultCollapsedStateLevel[1] = false;
            DefaultCollapsedStateLevel[2] = false;
            DefaultCollapsedStateLevel[3] = true;
        }

        public override void UpdateSource()
        {
            if (ShouldAutoUpdate)
            {
                Source = DSPStatistics.logisticsStationStats.SelectMany(station => station.products, (station, product) => new ResStationGroup() { station = station, product = product });
            } else
            {
                Source = DSPStatistics.logisticsStationStats.ToList().SelectMany(station => station.products, (station, product) => new ResStationGroup() { station = station, product = product });
            }
            

            foreach (var tbFilterKV in TabFilters)
            {
                var newFilteredSource = Source;
                TabFilterInfo[tbFilterKV.Key].ItemsBefore = newFilteredSource.Count();
                foreach (var filter in tbFilterKV.Value.Where(f => f.enabled == true))
                {
                    newFilteredSource = filter.LINQFilter(filter, newFilteredSource);
                }
                FilteredSource[tbFilterKV.Key] = newFilteredSource;
                TabFilterInfo[tbFilterKV.Key].ItemsAfter = newFilteredSource.Count();
            }
        }

        public void InitFilters()
        {
            Filter<ResStationGroup> stationItemAmountFilter = new Filter<ResStationGroup>()
            {
                enabled = false,
                value = 50,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                    var enabled = GUILayout.Toggle(filter.enabled, $"Item Amount %: <{filter.value.ToString("F0")}");
                    var value = GUILayout.HorizontalSlider(filter.value, 0, 100);
                    var shouldUpdateFiltered = (filter.enabled != enabled || filter.value != value);
                    filter.enabled = enabled;
                    filter.value = value;
                    GUILayout.EndVertical();
                    return shouldUpdateFiltered;
                },
                LINQFilter = (filter, source) =>
                {
                    return source.Where(sg => Math.Min((float)sg.product.item.count / sg.product.item.max, 1) <= (filter.value / 100));
                }
            };

            Filter<ResStationGroup> stationWarpAmountFilter = new Filter<ResStationGroup>()
            {
                enabled = false,
                value = 50,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                    var enabled = GUILayout.Toggle(filter.enabled, $"Warper Amount %: <{filter.value.ToString("F0")}");
                    var value = GUILayout.HorizontalSlider(filter.value, 0, 100);
                    var shouldUpdateFiltered = (filter.enabled != enabled || filter.value != value);
                    filter.enabled = enabled;
                    filter.value = value;
                    GUILayout.EndVertical();
                    return shouldUpdateFiltered;
                },
                LINQFilter = (filter, source) =>
                {
                    return source.Where(sg => Math.Min((float)sg.station.stationComponent.warperCount / sg.station.stationComponent.warperMaxCount, 1) <= (filter.value / 100));
                }
            };

            Filter<ResStationGroup> stationTypeFilter = new Filter<ResStationGroup>()
            {
                enabled = true,
                value = 1,
                value2 = 1,
                value3 = 1,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                        // var enabled = GUILayout.Toggle(filter.enabled, $"Station Types");
                        var value1 = GUILayout.Toggle(filter.value > 0.5, $"Planetary") ? 1 : 0;
                    var value2 = GUILayout.Toggle(filter.value2 > 0.5, $"Stellar") ? 1 : 0;
                    var value3 = GUILayout.Toggle(filter.value3 > 0.5, $"Collector") ? 1 : 0;

                    var shouldUpdateFiltered = (filter.value != value1 || filter.value2 != value2 || filter.value3 != value3);
                        // filter.enabled = enabled;
                        filter.value = value1;
                    filter.value2 = value2;
                    filter.value3 = value3;
                    GUILayout.EndVertical();
                    return shouldUpdateFiltered;
                },
                LINQFilter = (filter, source) =>
                {
                    return source.Where(sg => (!sg.station.stationComponent.isStellar && filter.value > 0.5)
                                            || (sg.station.stationComponent.isStellar && !sg.station.stationComponent.isCollector && filter.value2 > 0.5)
                                            || (sg.station.stationComponent.isCollector && filter.value3 > 0.5)
                                            );
                }
            };

            Filter<ResStationGroup> itemLogTypeFilter = new Filter<ResStationGroup>()
            {
                enabled = true,
                value = 1,
                value2 = 1,
                value3 = 1,
                value4 = 1,
                value5 = 1,
                value6 = 1,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                        // var enabled = GUILayout.Toggle(filter.enabled, $"Station Types");

                        GUILayout.BeginHorizontal();
                    GUILayout.Label("Supply", UITheme.TextAlignStyle, GUILayout.Width(60));
                    var value1 = GUILayout.Toggle(filter.value > 0.5, $"L") ? 1 : 0;
                    var value2 = GUILayout.Toggle(filter.value2 > 0.5, $"R") ? 1 : 0;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Demand", UITheme.TextAlignStyle, GUILayout.Width(60));
                    var value3 = GUILayout.Toggle(filter.value3 > 0.5, $"L") ? 1 : 0;
                    var value4 = GUILayout.Toggle(filter.value4 > 0.5, $"R") ? 1 : 0;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Storage", UITheme.TextAlignStyle, GUILayout.Width(60));
                    var value5 = GUILayout.Toggle(filter.value5 > 0.5, $"L") ? 1 : 0;
                    var value6 = GUILayout.Toggle(filter.value6 > 0.5, $"R") ? 1 : 0;
                    GUILayout.EndHorizontal();


                        /*
                        var value1 = GUILayout.Toggle(filter.value > 0.5, $"Supply") ? 1 : 0;
                        var value2 = GUILayout.Toggle(filter.value2 > 0.5, $"Demand") ? 1 : 0;
                        var value3 = GUILayout.Toggle(filter.value3 > 0.5, $"Storage") ? 1 : 0;
                        */

                    var shouldUpdateFiltered = (filter.value != value1 || filter.value2 != value2 || filter.value3 != value3
                                             || filter.value4 != value4 || filter.value5 != value5 || filter.value6 != value6);

                        // filter.enabled = enabled;
                        filter.value = value1;
                    filter.value2 = value2;
                    filter.value3 = value3;
                    filter.value4 = value4;
                    filter.value5 = value5;
                    filter.value6 = value6;
                    GUILayout.EndVertical();
                    return shouldUpdateFiltered;
                },
                LINQFilter = (filter, source) =>
                {
                    return source.Where(sg => (sg.product.item.localLogic == ELogisticStorage.Supply && filter.value > 0.5)
                                            || (sg.station.stationComponent.isStellar && sg.product.item.remoteLogic == ELogisticStorage.Supply && filter.value2 > 0.5)
                                            || (sg.product.item.localLogic == ELogisticStorage.Demand && filter.value3 > 0.5)
                                            || (sg.station.stationComponent.isStellar && sg.product.item.remoteLogic == ELogisticStorage.Demand && filter.value4 > 0.5)
                                            || (sg.product.item.localLogic == ELogisticStorage.None && filter.value5 > 0.5)
                                            || (sg.station.stationComponent.isStellar && sg.product.item.remoteLogic == ELogisticStorage.None && filter.value6 > 0.5)
                                            );
                }
            };

            AddFilterToTabs(stationTypeFilter, new List<eTAB_TYPES> { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_LOGISTICS, eTAB_TYPES.TAB_RESOURCE });
            AddFilterToTabs(itemLogTypeFilter, new List<eTAB_TYPES> { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_LOGISTICS, eTAB_TYPES.TAB_RESOURCE });

            AddFilterToTabs(stationItemAmountFilter, new List<eTAB_TYPES> { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_LOGISTICS, eTAB_TYPES.TAB_RESOURCE });
            AddFilterToTabs(stationWarpAmountFilter, new List<eTAB_TYPES> { eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_LOGISTICS, eTAB_TYPES.TAB_RESOURCE });
        }

        public void AddFiltersToTab(eTAB_TYPES tab, IEnumerable<Filter<ResStationGroup>> filters)
        {
            foreach (var filter in filters)
            {
                TabFilters[tab].Add(filter);
            }
        }
        public void AddFilterToTabs(Filter<ResStationGroup> filter, IEnumerable<eTAB_TYPES> tabs)
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
            GUILayout.Label($"({TabFilterInfo[selectedTab].ItemsAfter}/{TabFilterInfo[selectedTab].ItemsBefore})", UITheme.TextAlignStyle);
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

        void DrawStationResourceGUI(IEnumerable<ResStationGroup> stations, float MaxWidth = 150, int MaxStationsPerLine = 5)
        {
            int stationsNum = 0;

            foreach (var station in stations)
            {
                if (stationsNum % MaxStationsPerLine == 0)
                {
                    GUILayout.BeginHorizontal();
                }
                GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(1.0f * MaxWidth), GUILayout.MaxWidth(1.0f * MaxWidth));
                GUILayout.BeginVertical(GUILayout.Width(1.0f * MaxWidth), GUILayout.MaxWidth(1.0f * MaxWidth), GUILayout.MinHeight(50));
                GUILayout.Label($"{station.station.name}", UITheme.TextAlignStyle, GUILayout.Width(0.98f * MaxWidth));
                if (station.station.stationComponent.isCollector)
                {
                    GUILayout.Label($"Collector", UITheme.TextAlignStyle);
                }
                else if (station.station.stationComponent.isStellar)
                {
                    GUILayout.Label($"Warpers: {station.station.stationComponent.warperCount}", UITheme.TextAlignStyle);
                }
                GUILayout.Label($"{DSPHelper.PositionToLatLon(station.station.stationPosition)}", UITheme.TextAlignStyle, UITheme.LocationColWidth);
                GUILayout.EndVertical();



                GUILayout.Label($"Planet: {station.station.planetData.name.Translate()}", UITheme.TextAlignStyle, GUILayout.Width(0.96f * MaxWidth));
                GUILayout.Label($"Amount: {station.product.item.count}/{station.product.item.max}", UITheme.TextAlignStyle, GUILayout.Width(0.86f * MaxWidth));

                GUILayout.BeginHorizontal(GUILayout.Width(1.0f * MaxWidth), GUILayout.MaxWidth(1.0f * MaxWidth));
                GUILayout.Label($"L: {station.product.item.localLogic}", station.product.item.localLogic == ELogisticStorage.Demand ? UITheme.DemandStyle : UITheme.SupplyStyle, GUILayout.Width(0.49f * MaxWidth), GUILayout.MinWidth(0.49f * MaxWidth));
                GUILayout.Label($"R: {station.product.item.remoteLogic}", station.product.item.remoteLogic == ELogisticStorage.Demand ? UITheme.DemandStyle : UITheme.SupplyStyle, GUILayout.Width(0.49f * MaxWidth), GUILayout.MinWidth(0.49f * MaxWidth));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                if (++stationsNum % MaxStationsPerLine == 0 || stationsNum == stations.Count())
                {
                    GUILayout.EndHorizontal();
                }
            }
        }

        public override void DrawTabGUI(eTAB_TYPES selectedTab)
        {
            var parentId = eTAB_SOURCE_TYPE.LogisticStations + "." + selectedTab;
            if (selectedTab == eTAB_TYPES.TAB_PLANET)
            {
                var productsPerStation = FilteredSource[selectedTab].GroupBy(pair => pair.station, (group, pairList) => new { station = group, products = pairList });
                var stationsPerPlanet = productsPerStation.GroupBy(pair => pair.station.planetData, (group, pairList) => new { planet = group, stations = pairList });

                foreach (var stationsPlanetGroup in stationsPerPlanet)
                {
                    var myPlanetId = parentId + "." + stationsPlanetGroup.planet.id;

                    GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                    DrawCollapsedChildrenChevron(myPlanetId, out bool childrenCollapsed);
                    GUILayout.Label($"<b>Planet {stationsPlanetGroup.planet.name.Translate() }</b>", UITheme.TextAlignStyle, GUILayout.Width(170));
                    GUILayout.EndHorizontal();

                    if (!childrenCollapsed)
                    {
                        foreach (var statProdGroup in stationsPlanetGroup.stations)
                        {
                            var myStationId = myPlanetId + "." + statProdGroup.station.stationComponent.id;
                            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);

                            GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(75), GUILayout.MaxWidth(75));
                            DrawCollapsedChildrenChevron(myStationId, out bool stationCildrenCollapsed);
                            GUILayout.Label($"{statProdGroup.station.name}", UITheme.TextAlignStyle);
                            if (statProdGroup.station.stationComponent.isCollector)
                            {
                                GUILayout.Label($"Collector", UITheme.TextAlignStyle);
                            }
                            else if (statProdGroup.station.stationComponent.isStellar)
                            {
                                GUILayout.Label($"Warpers: {statProdGroup.station.stationComponent.warperCount}", UITheme.TextAlignStyle);
                            }
                            GUILayout.Label($"{DSPHelper.PositionToLatLon(statProdGroup.station.stationPosition)}", UITheme.TextAlignStyle, UITheme.LocationColWidth);
                            GUILayout.EndVertical();

                            if (!stationCildrenCollapsed)
                            {
                                foreach (var product in statProdGroup.products)
                                {
                                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(150), GUILayout.MaxWidth(150));
                                    GUILayout.BeginHorizontal(GUILayout.Width(150), GUILayout.MaxWidth(150), GUILayout.MinHeight(50));
                                    GUILayout.Box(product.product.itemProto.iconSprite.texture, UITheme.VeinIconLayoutSmallOptions);
                                    GUILayout.Label($"{product.product.itemProto.name}", UITheme.TextAlignStyle, GUILayout.Width(120));
                                    GUILayout.EndHorizontal();
                                    GUILayout.Label($"Amount: {product.product.item.count}/{product.product.item.max}", UITheme.TextAlignStyle, GUILayout.Width(130));
                                    GUILayout.BeginHorizontal(GUILayout.Width(150), GUILayout.MaxWidth(150));
                                    GUILayout.Label($"L: {product.product.item.localLogic}", product.product.item.localLogic == ELogisticStorage.Demand ? UITheme.DemandStyle : UITheme.SupplyStyle, GUILayout.Width(73), GUILayout.MinWidth(73));
                                    GUILayout.Label($"R: {product.product.item.remoteLogic}", product.product.item.remoteLogic == ELogisticStorage.Demand ? UITheme.DemandStyle : UITheme.SupplyStyle, GUILayout.Width(73), GUILayout.MinWidth(73));
                                    GUILayout.EndHorizontal();
                                    GUILayout.EndVertical();
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
            else if (selectedTab == eTAB_TYPES.TAB_RESOURCE)
            {
                var stationsPerResource = FilteredSource[selectedTab].GroupBy(pair => pair.product.item.itemId, pair => new ResStationGroup() { station = pair.station, product = pair.product })
                            .Select(grp => new { itemId = grp.Key, itemProto = LDB.items.Select(grp.Key), stations = grp }).OrderBy(grp => grp.itemProto.name.Translate());

                foreach (var resource in stationsPerResource)
                {
                    var myId = parentId + "." + resource.itemId;

                    var resourceTexture = resource.itemProto.iconSprite.texture;

                    GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                    DrawCollapsedChildrenChevron(myId, out bool resChildrenCollapsed);
                    GUILayout.Label($"<b>Resource</b>", UITheme.TextAlignStyle, GUILayout.Width(65));
                    GUILayout.Box(resourceTexture, UITheme.VeinIconLayoutOptions);
                    GUILayout.Label($"<b>{resource.itemProto.Name.Translate()}</b>", UITheme.TextAlignStyle);
                    GUILayout.EndHorizontal();

                    var interstellarDemand = resource.stations.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.Demand);
                    var interstellarSupply = resource.stations.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.Supply);
                    var interstellarNone = resource.stations.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.None);

                    var localDemand = resource.stations.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.Demand);
                    var localSupply = resource.stations.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.Supply);
                    var localNone = resource.stations.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.None);

                    List<PresOrderTuple> presOrder = new List<PresOrderTuple>()
                    {
                        { new PresOrderTuple() { name= "Interstellar Supply", stations=interstellarSupply, style=UITheme.SupplyStyle}},
                        { new PresOrderTuple() { name= "Interstellar Demand", stations=interstellarDemand, style=UITheme.DemandStyle}},
                        { new PresOrderTuple() { name= "Interstellar Storage", stations=interstellarNone, style=UITheme.TextAlignStyle}},
                        { new PresOrderTuple() { name= "Planetary Supply", stations=localSupply, style=UITheme.SupplyStyle}},
                        { new PresOrderTuple() { name= "Planetary Demand", stations=localDemand, style=UITheme.DemandStyle}},
                        { new PresOrderTuple() { name= "Planetary Storage", stations=localNone, style=UITheme.TextAlignStyle}}
                    };

                    if (!resChildrenCollapsed)
                    {
                        foreach (var pres in presOrder)
                        {
                            if (pres.stations.Count() > 0)
                            {
                                var logTypeByResourceId = myId + "." + pres.name;

                                var stationsByPlanetByResource = pres.stations
                                    .GroupBy(m => m.station.planetData.name.Translate()).OrderBy(g => g.Key)
                                    .Select(mtg => new { Name = mtg.Key, Stations = mtg.ToList()});

                                GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                                GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(75), GUILayout.MaxWidth(75));
                                DrawCollapsedChildrenChevron(logTypeByResourceId, out bool logTypeByResourceChildrenCollapsed);
                                GUILayout.Label($"{pres.name}", pres.style);
                                GUILayout.EndVertical();

                                if (!logTypeByResourceChildrenCollapsed)
                                {
                                    GUILayout.BeginVertical();
                                    foreach (var stationsPlanet in stationsByPlanetByResource)
                                    {
                                        var mStationsByPlanetId = logTypeByResourceId + "." + stationsPlanet.Name;
                                        GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                                        DrawCollapsedChildrenChevron(mStationsByPlanetId, out bool stationsByPlanetChildrenCollapsed);
                                        GUILayout.Label($"<b>Planet {stationsPlanet.Name}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                                        GUILayout.Label($"  <b># P.L.S: {stationsPlanet.Stations.Where(s => !s.station.stationComponent.isStellar).Count()}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                                        GUILayout.Label($"  <b># I.L.S: {stationsPlanet.Stations.Where(s => s.station.stationComponent.isStellar && !s.station.stationComponent.isCollector).Count()}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                                        GUILayout.Label($"  <b># Collectors: {stationsPlanet.Stations.Where(s => s.station.stationComponent.isCollector).Count()}</b>", UITheme.TextAlignStyle, GUILayout.MinWidth(65));
                                        GUILayout.EndHorizontal();

                                        if (!stationsByPlanetChildrenCollapsed)
                                        {
                                            GUILayout.BeginVertical();
                                            DrawStationResourceGUI(stationsPlanet.Stations, MaxWidth: 165, MaxStationsPerLine: 4);
                                            GUILayout.EndVertical();
                                        }
                                    }
                                    GUILayout.EndVertical();
                                }

                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                }
            }
        }
    }
}
