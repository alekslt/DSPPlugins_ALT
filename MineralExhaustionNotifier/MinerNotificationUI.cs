using BepInEx;
using System;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using DSPPlugins_ALT.Statistics;
using System.Collections;
using UnityEngine.UI;
using static DSPPlugins_ALT.MinerNotificationUI;

namespace DSPPlugins_ALT
{
    public static class MinerNotificationUI
    {
        private const string WindowName = "DSP Information and Alarms";
        public static GUILayoutOption PlanetColWidth;
        public static GUILayoutOption LocationColWidth;
        public static GUILayoutOption AlarmColWidth;
        public static GUILayoutOption[] VeinIconLayoutOptions;
        public static GUILayoutOption[] VeinIconLayoutSmallOptions;
        public static GUILayoutOption[] MenuButtonLayoutOptions;
        
        //public static GUILayoutOption VeinIconColWidth;
        //public static GUILayoutOption VeinIconColHeight;
        public static GUILayoutOption VeinTypeColWidth;
        public static GUILayoutOption VeinAmountColWidth;
        public static GUILayoutOption VeinRateColWidth;
        public static GUILayoutOption VeinETAColWidth;

        public static bool HighlightButton = false;
        public static bool ShowButton = true;
        public static bool Show;
        private static Rect winRect = new Rect(0, 0, 1000, 650); // 680

        private static Vector2 sv;
        private static GUIStyle textAlignStyle;

        private static GUIStyle menuButton;
        private static GUIStyle menuButtonHighlighted;

        private static GUIStyle tabMenuButton;
        private static GUIStyle tabMenuButtonSelected;

        private static GUIStyle demandStyle;
        private static GUIStyle supplyStyle;

        private static bool isInit = false;

        private static Texture2D[] sign_state;
        private static Texture2D menu_button_texture;
        private static Texture2D tab_menu_button_texture;
        private static Texture2D tab_menu_button_texture_selected;

        //private static Slider slider;

        private static void Init()
        {
            isInit = true;

            sign_state = new Texture2D[10];

            for (int i = 0; i <= 8; i++)
            {
                //Debug.Log("Loading sign-state-" + i + " - res :" + sign_state[i]);
                sign_state[i] = Resources.Load<Texture2D>("ui/textures/sprites/icons/sign-state-" + i);
                if (sign_state[i] == null)
                {
                    Debug.LogWarning("Failed Loading sign-state-" + i);
                    sign_state[i] = Texture2D.blackTexture;
                }
            }

            menu_button_texture = Resources.Load<Texture2D>("ui/textures/sprites/round-64px-border-slice");
            if (menu_button_texture == null)
            {
                Debug.LogWarning("Failed Loading menu_button_texture");
                menu_button_texture = Texture2D.blackTexture;
            }

            tab_menu_button_texture = Resources.Load<Texture2D>("ui/textures/sprites/sci-fi/panel-4");
            if (tab_menu_button_texture == null)
            {
                Debug.LogWarning("Failed Loading menu_button_texture");
                tab_menu_button_texture = Texture2D.blackTexture;
            }

            tab_menu_button_texture_selected = Resources.Load<Texture2D>("ui/textures/sprites/sci-fi/panel-3");
            if (tab_menu_button_texture_selected == null)
            {
                Debug.LogWarning("Failed Loading menu_button_texture");
                tab_menu_button_texture_selected = Texture2D.blackTexture;
            }



            textAlignStyle = new GUIStyle(GUI.skin.label);
            textAlignStyle.alignment = TextAnchor.MiddleLeft;

            menuButton = new GUIStyle(GUI.skin.button);
            menuButton.normal.background = menuButton.hover.background = menuButton.active.background = menu_button_texture;
            
            menuButton.normal.textColor = Color.white;
            menuButton.fontSize = 21;

            menuButtonHighlighted = new GUIStyle(menuButton);
            menuButtonHighlighted.normal.textColor = Color.red;

            PlanetColWidth = GUILayout.Width(160);
            LocationColWidth = GUILayout.Width(80);
            AlarmColWidth = GUILayout.Width(140);
            //VeinIconColWidth = GUILayout.Width(45);
            VeinTypeColWidth = GUILayout.Width(135);
            VeinAmountColWidth = GUILayout.Width(80);
            VeinRateColWidth = GUILayout.Width(80);
            VeinETAColWidth = GUILayout.Width(95);

            VeinIconLayoutOptions = new GUILayoutOption[] { GUILayout.Height(35), GUILayout.MaxWidth(35) };
            VeinIconLayoutSmallOptions = new GUILayoutOption[] { GUILayout.Height(30), GUILayout.MaxWidth(30) };

            MenuButtonLayoutOptions = new GUILayoutOption[] { GUILayout.Height(45), GUILayout.MaxWidth(45) };

            tabMenuButton = new GUIStyle(GUI.skin.button);
            tabMenuButton.normal.background = tabMenuButton.hover.background = tabMenuButton.active.background = tab_menu_button_texture;
            tabMenuButtonSelected = new GUIStyle(tabMenuButton);
            tabMenuButtonSelected.normal.background = tabMenuButtonSelected.hover.background = tabMenuButtonSelected.active.background = tab_menu_button_texture_selected;
            tabMenuButtonSelected.normal.textColor = tabMenuButtonSelected.hover.textColor = tabMenuButtonSelected.active.textColor = Color.white;

            demandStyle = new GUIStyle(textAlignStyle);
            demandStyle.normal.textColor = new Color(0.8784f, 0.5450f, 0.3647f);  // 224, 139, 93
            supplyStyle = new GUIStyle(textAlignStyle);
            supplyStyle.normal.textColor = new Color(0.2392f, 0.5450f, 0.6549f); // 61, 139, 167

            // slider = MineralExhaustionNotifier.instance.gameObject.AddComponent<Slider>();
            //sourceCombo = MineralExhaustionNotifier.instance.gameObject.AddComponent<UIComboBox>();
            //sourceCombo.Items = new List<string>() {"A", "B" };
        }

        public static void OnGUI()
        {
            var uiGame = BGMController.instance.uiGame;
            var shouldShowByGameState = DSPGame.GameDesc != null && uiGame != null && uiGame.gameData != null && uiGame.guideComplete && DSPGame.IsMenuDemo == false && DSPGame.Game.running && (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Sail) &&
                !(uiGame.techTree.active || uiGame.dysonmap.active || uiGame.starmap.active || uiGame.escMenu.active || uiGame.hideAllUI0 || uiGame.hideAllUI1) && uiGame.gameMenu.active;

            //Show = shouldShowByGameState = DSPGame.MenuDemoLoaded;

            if (!shouldShowByGameState)
            {
                return;
            }

            if (!isInit && GameMain.isRunning) { Init(); InitFilters(); }

            if (Show && shouldShowByGameState)
            {
                winRect = GUILayout.Window(55416753, winRect, WindowFunc, WindowName);
                UIHelper.EatInputInRect(winRect);
            }

            if (ShowButton && shouldShowByGameState)
            {
                Rect buttonWinRect = new Rect(Screen.width - 120, Screen.height - 46, 45, 45);
                var activeStyle = HighlightButton ? menuButtonHighlighted : menuButton;
                GUILayout.BeginArea(buttonWinRect);
                if (GUILayout.Button("M", activeStyle, MenuButtonLayoutOptions))
                {
                    Show = !Show;
                }
                GUILayout.EndArea();
            }
            // GUILayout.Window(55416753, new Rect(Screen.width/2, Screen.height/2, 200, 200), TestWindowFunc, "Test");
        }

        public static void TestWindowFunc(int id)
        {
            // iconId0 = 1006
            var iconId0 = 1006;
            ItemProto itemProto2 = LDB.items.Select(iconId0);
            Debug.Log("Name: " + itemProto2.name.Translate());
            Debug.Log("IconSprite: " + itemProto2.iconSprite);
            Debug.Log("Text : " + itemProto2.iconSprite.texture);

            GUI.Label(new Rect(0, 0, 10, 10), itemProto2.name.Translate());
            GUI.Box(new Rect(0, 0, 90f, 90f), string.Empty);
            GUI.DrawTexture(new Rect(0, 0, 80f, 80f), itemProto2.iconSprite.texture);

            if (itemProto2 == null)
            {
                return;
            }
        }

        public static void DrawHeader(bool includePlanet = false)
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.MaxHeight(45));
            if (includePlanet)
            {
                GUILayout.Label($"<b>Planet</b>", textAlignStyle, PlanetColWidth);
            }
            GUILayout.Label($"<b>Location</b>", textAlignStyle, LocationColWidth);
            GUILayout.Label($"<b>W-State</b>", textAlignStyle, LocationColWidth);
            GUILayout.Label($"<b>Alarm</b>", textAlignStyle, AlarmColWidth);
            GUILayout.Label($"<b>Vein</b>", textAlignStyle, VeinTypeColWidth);
            GUILayout.Label($"<b>Amount \nLeft</b>", textAlignStyle, VeinAmountColWidth);
            GUILayout.Label($"<b>Mining Rate/min</b>", textAlignStyle, VeinRateColWidth);
            GUILayout.Label($"<b>~Time to Empty</b>", textAlignStyle, VeinETAColWidth);
            GUILayout.EndHorizontal();
        }

        static void DrawMiners(IEnumerable<MinerNotificationDetail> miners, bool includePlanet = false)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(5, 0, 0, 0)
            };

            foreach (var item in miners)
            {
                string latLon = DSPHelper.PositionToLatLon(item.plantPosition);

                var alarmSign = (item.signType != SignData.NONE) ? sign_state[DSPHelper.SignNumToTextureIndex(item.signType)] : Texture2D.blackTexture;
                var resourceTexture = item.resourceTexture ? item.resourceTexture : Texture2D.blackTexture;

                // Debug.Log("Drawing sign-state-" + item.signType);

                GUILayout.BeginHorizontal(boxStyle, GUILayout.MaxHeight(45));
                if (includePlanet)
                {
                    GUILayout.Label($"{item.planetName}", textAlignStyle, PlanetColWidth);
                }
                GUILayout.Label($"{latLon}", textAlignStyle, LocationColWidth);
                GUILayout.Label($"{WorkStateToText(item.minerComponent.workstate, item.consumerRatio)}", textAlignStyle, LocationColWidth);

                GUILayout.BeginHorizontal(VeinTypeColWidth);
                GUILayout.Box(alarmSign, VeinIconLayoutOptions);
                GUILayout.Label(DSPHelper.SignNumToText(item.signType), textAlignStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(VeinTypeColWidth);
                GUILayout.Box(resourceTexture, VeinIconLayoutOptions);
                GUILayout.Label($"{item.veinName}", textAlignStyle);
                GUILayout.EndHorizontal();

                GUILayout.Label($"{item.veinAmount}", textAlignStyle, VeinAmountColWidth);
                GUILayout.Label($"{Math.Round(item.miningRatePerMin, 0)}", textAlignStyle, VeinRateColWidth);
                GUILayout.Label($"{item.minutesToEmptyVeinTxt}", textAlignStyle, VeinETAColWidth);

                GUILayout.EndHorizontal();
            }
        }

        private static string GetSourceTabName(eTAB_SOURCE_TYPE tabType)
        {
            switch (tabType)
            {
                case eTAB_SOURCE_TYPE.VeinMeiners: return "Vein Miners";
                case eTAB_SOURCE_TYPE.LogisticStations: return "Logistic Stations";
                default: return "Undefined";
            }
        }

        private static string GetTabName(eTAB_TYPES tabType)
        {
            switch (tabType)
            {
                case eTAB_TYPES.TAB_PLANET: return "Planet";
                case eTAB_TYPES.TAB_NETWORK: return "Power Network";
                case eTAB_TYPES.TAB_RESOURCE: return "Resource";
                case eTAB_TYPES.TAB_LOGISTICS: return "Logistics";
                default: return "Undefined";
            }
        }

        private static string WorkStateToText(EWorkState workState, float consumerRatio)
        {
            switch (workState)
            {
                case EWorkState.Idle:
                    return "无矿物".Translate();
                case EWorkState.Lack:
                    return "缺少原材料".Translate();
                case EWorkState.Full:
                    return "产物堆积".Translate();
                case EWorkState.Running:
                case EWorkState.Outputing:
                    if (consumerRatio == 1f)
                    {
                        return "正常运转".Translate();
                    }
                    else if (consumerRatio > 0.1f)
                    {
                        return "电力不足".Translate();
                    }
                    else
                    {
                        return "停止运转".Translate();
                    }
            }
            return "Unknown";
	    }

        public enum eTAB_SOURCE_TYPE { VeinMeiners, LogisticStations };
        public enum eTAB_TYPES { TAB_PLANET, TAB_NETWORK, TAB_RESOURCE, TAB_LOGISTICS };
        public static eTAB_TYPES selectedTab = eTAB_TYPES.TAB_PLANET;
        public static eTAB_SOURCE_TYPE selectedTabSourceType = eTAB_SOURCE_TYPE.VeinMeiners;


        public static Dictionary<eTAB_SOURCE_TYPE, IList<eTAB_TYPES>> TABSourceGroups = new Dictionary<eTAB_SOURCE_TYPE, IList<eTAB_TYPES>>()
        {
            { eTAB_SOURCE_TYPE.VeinMeiners, new List<eTAB_TYPES>() {eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_NETWORK, eTAB_TYPES.TAB_RESOURCE } },
            { eTAB_SOURCE_TYPE.LogisticStations, new List<eTAB_TYPES>() {eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_RESOURCE } }
        };

        //public delegate T LINQFilterDelegate<T>(Filter<T> filter, T param);

        public class Filter
        {
            public bool enabled;
            public float value;

            public Action<Filter> onGUI;

            //public LINQFilterDelegate<T> LINQFilter;

            public Func<Filter, IEnumerable<object>, IEnumerable<object>> LINQFilter;

        }

        public struct SourceTabTuple
        {
            public eTAB_SOURCE_TYPE source;
            public eTAB_TYPES tab;
        }

        public static void DrawVeinMinersGUI()
        {
            // IOrderedEnumerable<IGrouping<Object, MinerNotificationDetail>> miners;
            var minersAll = DSPStatistics.notificationList.Values.SelectMany(x => x).ToList();
            if (selectedTab == eTAB_TYPES.TAB_PLANET)
            {
                foreach (var planet in DSPStatistics.notificationList)
                {
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>Planet {planet.Key}</b>", textAlignStyle);
                    GUILayout.EndHorizontal();

                    DrawHeader();

                    var filters = TabFilters[selectedTabSourceType][selectedTab];
                    IEnumerable<MinerNotificationDetail> filteredMiners = planet.Value.Select(a => a);
                    foreach (var filter in filters.Where(f => f.enabled == true))
                    {
                        filteredMiners = filter.LINQFilter(filter, filteredMiners.Cast<object>()).Cast<MinerNotificationDetail>();
                    }
                    DrawMiners(filteredMiners);
                }
            }
            else if (selectedTab == eTAB_TYPES.TAB_NETWORK)
            {
                foreach (var planet in DSPStatistics.notificationList)
                {
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>Planet {planet.Key}</b>", textAlignStyle);
                    GUILayout.EndHorizontal();

                    var miners = (from miner in planet.Value
                                  orderby miner.veinAmount
                                  group miner by miner.powerNetwork into netGroup
                                  orderby netGroup.Key.id
                                  select netGroup);

                    foreach (var netGroup in miners)
                    {
                        GUILayout.BeginHorizontal(GUI.skin.box);
                        GUILayout.Label($"<b>PowerNetwork {netGroup.Key.id} - Health: {Math.Round(netGroup.Key.consumerRatio * 100, 0) }</b>", textAlignStyle, GUILayout.Width(260));
                        GUILayout.EndHorizontal();

                        DrawHeader();
                        DrawMiners(netGroup);
                    }
                }
            }
            else if (selectedTab == eTAB_TYPES.TAB_RESOURCE)
            {
                var minersTest = minersAll.OrderByDescending(miner => miner.veinAmount);
                var minersTestGroup = minersTest.GroupBy(x => x.veinName).OrderBy(g => g.Key);
                var miners = minersTestGroup.Select(mtg => new { Name = mtg.Key, Tex = mtg.First().resourceTexture, Miners = mtg.ToList(), SumMiningPerMin = mtg.Sum(m => m.miningRatePerMin) });

                /*
                var miners = (from miner in minersAll
                              orderby miner.veinAmount
                              group miner by miner.veinName into resourceGroup
                              orderby resourceGroup.Key
                              select new { Name = resourceGroup.Key, Tex=resourceGroup.First().resourceTexture, Miners = resourceGroup.ToList(), SumMiningPerMin = resourceGroup.Sum( m => m.miningRatePerMin) });
                */

                foreach (var resourceGroup in miners)
                {
                    var resourceTexture = resourceGroup.Tex ? resourceGroup.Tex : Texture2D.blackTexture;

                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>Resource</b>", textAlignStyle, GUILayout.Width(65));
                    GUILayout.Box(resourceTexture, VeinIconLayoutOptions);
                    GUILayout.Label($"<b>{resourceGroup.Name}</b>", textAlignStyle);
                    GUILayout.Label($"  <b>Sum MiningRate: {resourceGroup.SumMiningPerMin}</b>", textAlignStyle);
                    GUILayout.EndHorizontal();

                    DrawHeader(includePlanet: true);
                    DrawMiners(resourceGroup.Miners, includePlanet: true);
                }
            }
        }

        // static float productionRateFilter = 100;
        // static bool productionRateFilterEnabled = true;

        public static Dictionary<eTAB_SOURCE_TYPE, Dictionary<eTAB_TYPES, IList<Filter>>> TabFilters = new Dictionary<eTAB_SOURCE_TYPE, Dictionary<eTAB_TYPES, IList<Filter>>>();

        public static void InitFilters()
        {
            Filter miningRateFilter = new Filter()
            {
                enabled = true,
                value = 100,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                    filter.enabled = GUILayout.Toggle(filter.enabled, $"Mining Rate: {filter.value.ToString("F0")}");
                    filter.value = GUILayout.HorizontalSlider(filter.value, 0, 1000);
                    GUILayout.EndVertical();
                },
                LINQFilter = (filter, source) =>
                {
                    return source.Cast<MinerNotificationDetail>().Where(miner => miner.miningRatePerMin < filter.value).Cast<object>();
                }
            };

            Filter stationItemAmountFilter = new Filter()
            {
                enabled = true,
                value = 50,
                onGUI = (filter) =>
                {
                    GUILayout.BeginVertical();
                    filter.enabled = GUILayout.Toggle(filter.enabled, $"Amount %: {filter.value.ToString("F0")}");
                    filter.value = GUILayout.HorizontalSlider(filter.value, 0, 100);
                    GUILayout.EndVertical();
                },
                LINQFilter = (filter, source) =>
                {
                    return source.Cast<ResStationGroup>().Where(sg => Math.Min((float)sg.product.item.count/ sg.product.item.max, 1) <= (filter.value/100)).Cast<object>();
                }
            };

            // Need to ensure all combinations of source+tab is added
            foreach (eTAB_SOURCE_TYPE sourceType in Enum.GetValues(typeof(eTAB_SOURCE_TYPE)))
            {
                TabFilters[sourceType] = new Dictionary<eTAB_TYPES, IList<Filter>>();
                foreach (eTAB_TYPES tabType in Enum.GetValues(typeof(eTAB_TYPES)))
                {
                    TabFilters[sourceType][tabType] = new List<Filter>();
                }
            }

            TabFilters[eTAB_SOURCE_TYPE.VeinMeiners][eTAB_TYPES.TAB_PLANET].Add(miningRateFilter);
            TabFilters[eTAB_SOURCE_TYPE.VeinMeiners][eTAB_TYPES.TAB_NETWORK].Add(miningRateFilter);

            TabFilters[eTAB_SOURCE_TYPE.LogisticStations][eTAB_TYPES.TAB_PLANET].Add(stationItemAmountFilter);
            TabFilters[eTAB_SOURCE_TYPE.LogisticStations][eTAB_TYPES.TAB_LOGISTICS].Add(stationItemAmountFilter);
            TabFilters[eTAB_SOURCE_TYPE.LogisticStations][eTAB_TYPES.TAB_RESOURCE].Add(stationItemAmountFilter);
        }


        public static void DrawVeinMinersFilterGUI()
        {
            GUILayout.Label($"<b>Filters</b>", textAlignStyle, GUILayout.Width(100));
            foreach (var filter in TabFilters[selectedTabSourceType][selectedTab])
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(150));
                filter.onGUI(filter);
                GUILayout.EndHorizontal();
            }
        }

        struct ResStationGroup
        {
            public StationStat station;
            public StationItemStat product;
        };

        struct PresOrderTuple
        {
            public string name;
            public GUIStyle style;
            public IEnumerable<ResStationGroup> stations;
        };

        public static void DrawLogisticStationsGUI()
        {
            var logStationProducts = DSPStatistics.logisticsStationStats.SelectMany(station => station.products, (station, product) => new ResStationGroup() { station = station, product = product });

            var filters = TabFilters[selectedTabSourceType][selectedTab];
            IEnumerable<ResStationGroup> filteredLogStationProducts = logStationProducts;
            foreach (var filter in filters.Where(f => f.enabled == true))
            {
                filteredLogStationProducts = filter.LINQFilter(filter, filteredLogStationProducts.Cast<object>()).Cast<ResStationGroup>();
            }

            if (selectedTab == eTAB_TYPES.TAB_PLANET)
            {
                /*
                var stationsPerPlanet = (from station in DSPStatistics.logisticsStationStats
                                         orderby station.name
                                         group station by station.planetData into resourceGroup
                                         orderby resourceGroup.Key.name
                                         select new { Name = resourceGroup.Key.name.Translate(), Stations = resourceGroup.ToList() });*/
                var productsPerStation = filteredLogStationProducts.GroupBy(pair => pair.station, (group, pairList) => new { station = group, products = pairList });
                var stationsPerPlanet = productsPerStation.GroupBy(pair => pair.station.planetData, (group, pairList) => new { planet = group, stations = pairList });
                /*
                var stationsPerPlanet2 = (from statProd in filteredLogStationProducts
                                         orderby statProd.station.name
                                         group statProd.station by statProd.station.planetData into resourceGroup
                                         orderby resourceGroup.Key.name
                                         select new { Name = resourceGroup.Key.name.Translate(), Stations = resourceGroup.ToList() });*/

                foreach (var stationsPlanetGroup in stationsPerPlanet)
                {
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>Planet {stationsPlanetGroup.planet.name.Translate() }</b>", textAlignStyle, GUILayout.Width(170));
                    GUILayout.EndHorizontal();

                    foreach (var statProdGroup in stationsPlanetGroup.stations)
                    {
                        GUILayout.BeginHorizontal(GUI.skin.box);
                        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(75), GUILayout.MaxWidth(75));
                        GUILayout.Label($"{statProdGroup.station.name}", textAlignStyle);
                        if (statProdGroup.station.stationComponent.isCollector)
                        {
                            GUILayout.Label($"Collector", textAlignStyle);
                        }
                        else if (statProdGroup.station.stationComponent.isStellar)
                        {
                            GUILayout.Label($"Warpers: {statProdGroup.station.stationComponent.warperCount}", textAlignStyle);
                        }
                        GUILayout.EndVertical();
                        foreach (var product in statProdGroup.products)
                        {
                            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150), GUILayout.MaxWidth(150));
                            GUILayout.BeginHorizontal(GUILayout.Width(150), GUILayout.MaxWidth(150), GUILayout.MinHeight(50));
                            GUILayout.Box(product.product.itemProto.iconSprite.texture, VeinIconLayoutSmallOptions);
                            GUILayout.Label($"{product.product.itemProto.name}", textAlignStyle, GUILayout.Width(120));
                            GUILayout.EndHorizontal();
                            GUILayout.Label($"Amount: {product.product.item.count}/{product.product.item.max}", textAlignStyle, GUILayout.Width(130));
                            GUILayout.BeginHorizontal(GUILayout.Width(150), GUILayout.MaxWidth(150));
                            GUILayout.Label($"L: {product.product.item.localLogic}", product.product.item.localLogic == ELogisticStorage.Demand ? demandStyle : supplyStyle, GUILayout.Width(73), GUILayout.MinWidth(73));
                            GUILayout.Label($"R: {product.product.item.remoteLogic}", product.product.item.remoteLogic == ELogisticStorage.Demand ? demandStyle : supplyStyle, GUILayout.Width(73), GUILayout.MinWidth(73));
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();
                    }

                    //DrawHeader(includePlanet: true);
                    //DrawMiners(resourceGroup.Miners, includePlanet: true);
                }
            } else if (selectedTab == eTAB_TYPES.TAB_RESOURCE)
            {
                /*
                var test = DSPStatistics.logisticsStationStats
                            .SelectMany(station => station.products, (station, product) => new { station, product })
                            
                */
                var stationsPerResource = filteredLogStationProducts.GroupBy(pair => pair.product.item.itemId, pair => new ResStationGroup() { station = pair.station, product = pair.product });
                
                foreach (var resource in stationsPerResource)
                {
                    ItemProto itemProto = LDB.items.Select(resource.Key);

                    var resourceTexture = itemProto.iconSprite.texture;

                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>Resource</b>", textAlignStyle, GUILayout.Width(65));
                    GUILayout.Box(resourceTexture, VeinIconLayoutOptions);
                    GUILayout.Label($"<b>{itemProto.Name.Translate()}</b>", textAlignStyle);
                    GUILayout.EndHorizontal();


                    var interstellarDemand  = resource.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.Demand);
                    var interstellarSupply  = resource.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.Supply);
                    var interstellarNone    = resource.Where(s => s.station.stationComponent.isStellar &&  s.product.item.remoteLogic == ELogisticStorage.None);

                    var localDemand = resource.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.Demand);
                    var localSupply = resource.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.Supply);
                    var localNone   = resource.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.None);

                    void DrawStationResourceGUI (IEnumerable<ResStationGroup> stations, float MaxWidth = 150, int MaxStationsPerLine = 5)
                    {
                        int stationsNum = 0;

                        foreach (var station in stations)
                        {
                            if (stationsNum % MaxStationsPerLine == 0)
                            {
                                GUILayout.BeginHorizontal();
                            }
                            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(1.0f * MaxWidth), GUILayout.MaxWidth(1.0f * MaxWidth));
                            GUILayout.BeginHorizontal(GUILayout.Width(1.0f * MaxWidth), GUILayout.MaxWidth(1.0f * MaxWidth), GUILayout.MinHeight(50));
                            GUILayout.Label($"{station.station.name}", textAlignStyle, GUILayout.Width(0.98f * MaxWidth));
                            GUILayout.EndHorizontal();

                            GUILayout.Label($"Planet: {station.station.planetData.name.Translate()}", textAlignStyle, GUILayout.Width(0.96f * MaxWidth));
                            GUILayout.Label($"Amount: {station.product.item.count}/{station.product.item.max}", textAlignStyle, GUILayout.Width(0.86f * MaxWidth));

                            GUILayout.BeginHorizontal(GUILayout.Width(1.0f * MaxWidth), GUILayout.MaxWidth(1.0f * MaxWidth));
                            GUILayout.Label($"L: {station.product.item.localLogic}", station.product.item.localLogic == ELogisticStorage.Demand ? demandStyle : supplyStyle, GUILayout.Width(0.49f * MaxWidth), GUILayout.MinWidth(0.49f * MaxWidth));
                            GUILayout.Label($"R: {station.product.item.remoteLogic}", station.product.item.remoteLogic == ELogisticStorage.Demand ? demandStyle : supplyStyle, GUILayout.Width(0.49f * MaxWidth), GUILayout.MinWidth(0.49f * MaxWidth));
                            GUILayout.EndHorizontal();

                            GUILayout.EndVertical();

                            if (++stationsNum % MaxStationsPerLine == 0 || stationsNum == stations.Count())
                            {
                                GUILayout.EndHorizontal();
                            }
                        }
                    }

                    List<PresOrderTuple> presOrder = new List<PresOrderTuple>()
                    {
                        { new PresOrderTuple() { name= "Interstellar Supply", stations=interstellarSupply, style=supplyStyle}},
                        { new PresOrderTuple() { name= "Interstellar Demand", stations=interstellarDemand, style=demandStyle}},
                        { new PresOrderTuple() { name= "Interstellar Storage", stations=interstellarNone, style=textAlignStyle}},
                        { new PresOrderTuple() { name= "Local Supply", stations=localSupply, style=supplyStyle}},
                        { new PresOrderTuple() { name= "Local Demand", stations=localDemand, style=demandStyle}},
                        { new PresOrderTuple() { name= "Local Storage", stations=localNone, style=textAlignStyle}}
                    };

                    foreach (var pres in presOrder)
                    {
                        if (pres.stations.Count() > 0)
                        {
                            GUILayout.BeginHorizontal(GUI.skin.box);
                            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(75), GUILayout.MaxWidth(75));
                            GUILayout.Label($"{pres.name}", pres.style);
                            GUILayout.EndVertical();
                            GUILayout.BeginVertical();
                            DrawStationResourceGUI(pres.stations, MaxWidth: 165, MaxStationsPerLine: 4);
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }


        public static void DrawLogisticStationsFilterGUI()
        {
            GUILayout.Label($"<b>Filters</b>", textAlignStyle, GUILayout.Width(100));
            foreach (var filter in TabFilters[selectedTabSourceType][selectedTab])
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(150));
                filter.onGUI(filter);
                GUILayout.EndHorizontal();
            }
        }

        public static void WindowFunc(int id)
        {
            GUILayout.BeginArea(new Rect(winRect.width - 22f, 2f, 20f, 17f));
            if (GUILayout.Button("X"))
            {
                Show = false;
            }
            GUILayout.EndArea();

            GUILayout.BeginVertical();
            // Draw Top Menu
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"<b>Sources: </b>", textAlignStyle, GUILayout.Width(100));
            foreach (eTAB_SOURCE_TYPE tabType in TABSourceGroups.Keys)
            {
                if (GUILayout.Button(GetSourceTabName(tabType), selectedTabSourceType == tabType ? tabMenuButtonSelected : tabMenuButton))
                {
                    selectedTabSourceType = tabType;
                    selectedTab = TABSourceGroups[selectedTabSourceType].First();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"<b>Grouped On: </b>", textAlignStyle, GUILayout.Width(100));
            foreach (eTAB_TYPES tabType in TABSourceGroups[selectedTabSourceType]) //Enum.GetValues(typeof(eTAB_TYPES)))
            {
                if (GUILayout.Button(GetTabName(tabType), selectedTab == tabType ? tabMenuButtonSelected : tabMenuButton))
                {
                    selectedTab = tabType;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUI.skin.box);
            switch (selectedTabSourceType)
            {
                case eTAB_SOURCE_TYPE.VeinMeiners: DrawVeinMinersFilterGUI(); break;
                case eTAB_SOURCE_TYPE.LogisticStations: DrawLogisticStationsFilterGUI(); break;
            }
            GUILayout.EndHorizontal();


            sv = GUILayout.BeginScrollView(sv, GUI.skin.box);

            switch (selectedTabSourceType)
            {
                case eTAB_SOURCE_TYPE.VeinMeiners: DrawVeinMinersGUI(); break;
                case eTAB_SOURCE_TYPE.LogisticStations: DrawLogisticStationsGUI(); break;
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();

            // Always close window on Escape for now
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            {
                Show = false;
            }
        }
    }
}
