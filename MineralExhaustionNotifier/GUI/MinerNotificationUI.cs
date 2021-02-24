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

namespace DSPPlugins_ALT.GUI
{
    public class MinerNotificationUI
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
        private static Rect winRect = new Rect(0, 0, 1015, 650); // 680

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

        public enum eTAB_SOURCE_TYPE { VeinMeiners, LogisticStations };
        public enum eTAB_TYPES { TAB_PLANET, TAB_NETWORK, TAB_RESOURCE, TAB_LOGISTICS };
        public static eTAB_TYPES selectedTab = eTAB_TYPES.TAB_PLANET;
        public static eTAB_SOURCE_TYPE selectedTabSourceType = eTAB_SOURCE_TYPE.VeinMeiners;



        public static Dictionary<eTAB_SOURCE_TYPE, DSPStatSource> DSPStatSources = new Dictionary<eTAB_SOURCE_TYPE, DSPStatSource>();
        private DSPStatistics minerStatistics;

        public MinerNotificationUI(DSPStatistics minerStatistics)
        {
            this.minerStatistics = minerStatistics;
            minerStatistics.onStatSourcesUpdated += MinerStatistics_onStatSourcesUpdated;
        }

        private void MinerStatistics_onStatSourcesUpdated(long obj)
        {
            foreach (var source in DSPStatSources.Where(s => s.Value.ShouldAutoUpdate))
            {
                source.Value.UpdateSource();
            }
        }

        /*
        public static Dictionary<eTAB_SOURCE_TYPE, IList<eTAB_TYPES>> TABSourceGroups = new Dictionary<eTAB_SOURCE_TYPE, IList<eTAB_TYPES>>()
        {
            { eTAB_SOURCE_TYPE.VeinMeiners, new List<eTAB_TYPES>() {eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_NETWORK, eTAB_TYPES.TAB_RESOURCE } },
            { eTAB_SOURCE_TYPE.LogisticStations, new List<eTAB_TYPES>() {eTAB_TYPES.TAB_PLANET, eTAB_TYPES.TAB_RESOURCE } }
        };
        */

        //public static Dictionary<eTAB_SOURCE_TYPE, Dictionary<eTAB_TYPES, IList<Filter>>> TabFilters = new Dictionary<eTAB_SOURCE_TYPE, Dictionary<eTAB_TYPES, IList<Filter>>>();
        public static int ScaledScreenWidth { get; set; } = 1920;
        public static int ScaledScreenHeight { get; set; } = 1080;

        public static void AutoResize(int designScreenHeight)
        {
            float ratio = (float)Screen.height / designScreenHeight;
            // Vector2 resizeRatio = new Vector2((float)Screen.width / screenWidth, (float)Screen.height / screenHeight);
            ScaledScreenWidth = (int)Math.Round(Screen.width / ratio);
            UnityEngine.GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(ratio, ratio, 1.0f));
        }

        private void Init()
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



            textAlignStyle = new GUIStyle(UnityEngine.GUI.skin.label);
            textAlignStyle.alignment = TextAnchor.MiddleLeft;

            menuButton = new GUIStyle(UnityEngine.GUI.skin.button);
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

            tabMenuButton = new GUIStyle(UnityEngine.GUI.skin.button);
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

        public void OnGUI()
        {
            AutoResize(1080);
            var uiGame = BGMController.instance.uiGame;
            var shouldShowByGameState = DSPGame.GameDesc != null && uiGame != null && uiGame.gameData != null && uiGame.guideComplete && DSPGame.IsMenuDemo == false && DSPGame.Game.running && (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Sail) &&
                !(uiGame.techTree.active || uiGame.dysonmap.active || uiGame.starmap.active || uiGame.escMenu.active || uiGame.hideAllUI0 || uiGame.hideAllUI1) && uiGame.gameMenu.active;

            //Show = shouldShowByGameState = DSPGame.MenuDemoLoaded;

            if (!shouldShowByGameState)
            {
                return;
            }

            if (!isInit && GameMain.isRunning) { Init(); InitSources(); }

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

        #region Filter Setup

        public abstract class DSPStatSource
        {
            public IList<eTAB_TYPES> TABPages;
            public Dictionary<eTAB_TYPES, TabFilterInfo> TabFilterInfo = new Dictionary<eTAB_TYPES, TabFilterInfo>();

            public abstract void UpdateSource();
            public abstract void DrawFilterGUI();
            public abstract void DrawTabGUI();

            public bool ShouldAutoUpdate { get; set; } = false;

        }
        public class TabFilterInfo
        {
            public int ItemsBefore;
            public int ItemsAfter;
        }

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
            }
            public override void UpdateSource()
            {
                // Source = DSPStatistics.notificationList.Values.SelectMany(x => x).ToList();
                Source = DSPStatistics.minerStats;
                
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

            public override void DrawFilterGUI()
            {
                bool shouldUpdateFiltered = false;
                GUILayout.BeginVertical(GUILayout.MaxWidth(80));
                GUILayout.Label($"<b>Filters</b>", textAlignStyle);
                GUILayout.Label($"({TabFilterInfo[selectedTab].ItemsAfter}/{TabFilterInfo[selectedTab].ItemsBefore})", textAlignStyle);
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

            void DrawVeinMiners(IEnumerable<MinerNotificationDetail> miners, bool includePlanet = false)
            {
                GUIStyle boxStyle = new GUIStyle(UnityEngine.GUI.skin.box)
                {
                    margin = new RectOffset(5, 0, 0, 0)
                };

                foreach (var item in miners)
                {
                    var alarmSign = (item.signType != SignData.NONE) ? sign_state[DSPHelper.SignNumToTextureIndex(item.signType)] : Texture2D.blackTexture;
                    var resourceTexture = item.resourceTexture ? item.resourceTexture : Texture2D.blackTexture;

                    // Debug.Log("Drawing sign-state-" + item.signType);

                    GUILayout.BeginHorizontal(boxStyle, GUILayout.MaxHeight(45));
                    if (includePlanet)
                    {
                        GUILayout.Label($"{item.planetName}", textAlignStyle, PlanetColWidth);
                    }
                    GUILayout.Label($"{DSPHelper.PositionToLatLon(item.plantPosition)}", textAlignStyle, LocationColWidth);
                    GUILayout.Label($"{DSPHelper.WorkStateToText(item.minerComponent.workstate, item.consumerRatio)}", textAlignStyle, LocationColWidth);

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

            public override void DrawTabGUI()
            {
                var minersByPlanet = FilteredSource[selectedTab]
                        .OrderBy(m => m.minutesToEmptyVein)
                        .GroupBy(m => m.planetName).OrderBy(g => g.Key);

                if (selectedTab == eTAB_TYPES.TAB_PLANET)
                {
                    foreach (var planet in minersByPlanet)
                    {
                        GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                        GUILayout.Label($"<b>Planet {planet.Key}</b>", textAlignStyle);
                        GUILayout.EndHorizontal();

                        DrawVeinMinersHeader();

                        DrawVeinMiners(planet);
                    }
                }
                else if (selectedTab == eTAB_TYPES.TAB_NETWORK)
                {
                    foreach (var planet in minersByPlanet)
                    {
                        GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                        GUILayout.Label($"<b>Planet {planet.Key}</b>", textAlignStyle);
                        GUILayout.EndHorizontal();

                        var planetNetGroups = planet
                            .OrderBy(m => m.veinAmount)
                            .GroupBy(m => m.powerNetwork)
                            .OrderBy(ng => ng.Key.id);   

                        foreach (var netGroup in planetNetGroups)
                        {
                            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                            GUILayout.Label($"<b>PowerNetwork {netGroup.Key.id} - Health: {Math.Round(netGroup.Key.consumerRatio * 100, 0) }</b>", textAlignStyle, GUILayout.Width(260));
                            GUILayout.EndHorizontal();

                            DrawVeinMinersHeader();
                            DrawVeinMiners(netGroup);
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
                        var resourceTexture = resourceGroup.Tex ? resourceGroup.Tex : Texture2D.blackTexture;

                        GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                        GUILayout.Label($"<b>Resource</b>", textAlignStyle, GUILayout.Width(65));
                        GUILayout.Box(resourceTexture, VeinIconLayoutOptions);
                        GUILayout.Label($"<b>{resourceGroup.Name}</b>", textAlignStyle);
                        GUILayout.Label($"  <b>Sum MiningRate: {resourceGroup.SumMiningPerMin}</b>", textAlignStyle);
                        GUILayout.EndHorizontal();

                        DrawVeinMinersHeader(includePlanet: true);
                        DrawVeinMiners(resourceGroup.Miners, includePlanet: true);
                    }
                }
            }

        }

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
            }

            public override void UpdateSource()
            {
                Source = DSPStatistics.logisticsStationStats.SelectMany(station => station.products, (station, product) => new ResStationGroup() { station = station, product = product });
                
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

                        var shouldUpdateFiltered = ( filter.value != value1 || filter.value2 != value2 || filter.value3 != value3);
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
                                                || (sg.station.stationComponent.isStellar && filter.value2 > 0.5)
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
                        GUILayout.Label("Supply", textAlignStyle, GUILayout.Width(60));
                        var value1 = GUILayout.Toggle(filter.value > 0.5, $"L") ? 1 : 0;
                        var value2 = GUILayout.Toggle(filter.value2 > 0.5, $"R") ? 1 : 0;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Demand", textAlignStyle, GUILayout.Width(60));
                        var value3 = GUILayout.Toggle(filter.value3 > 0.5, $"L") ? 1 : 0;
                        var value4 = GUILayout.Toggle(filter.value4 > 0.5, $"R") ? 1 : 0;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Storage", textAlignStyle, GUILayout.Width(60));
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
                        return source.Where(sg =>  (sg.product.item.localLogic  == ELogisticStorage.Supply  && filter.value > 0.5)
                                                || (sg.station.stationComponent.isStellar && sg.product.item.remoteLogic == ELogisticStorage.Supply  && filter.value2 > 0.5)
                                                || (sg.product.item.localLogic  == ELogisticStorage.Demand  && filter.value3 > 0.5)
                                                || (sg.station.stationComponent.isStellar && sg.product.item.remoteLogic == ELogisticStorage.Demand  && filter.value4 > 0.5)
                                                || (sg.product.item.localLogic  == ELogisticStorage.None    && filter.value5 > 0.5)
                                                || (sg.station.stationComponent.isStellar && sg.product.item.remoteLogic == ELogisticStorage.None    && filter.value6 > 0.5)
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

            public override void DrawFilterGUI()
            {
                bool shouldUpdateFiltered = false;

                GUILayout.BeginVertical(GUILayout.MaxWidth(80));
                GUILayout.Label($"<b>Filters</b>", textAlignStyle);
                GUILayout.Label($"({TabFilterInfo[selectedTab].ItemsAfter}/{TabFilterInfo[selectedTab].ItemsBefore})", textAlignStyle);
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
                    GUILayout.Label($"{station.station.name}", textAlignStyle, GUILayout.Width(0.98f * MaxWidth));
                    if (station.station.stationComponent.isCollector)
                    {
                        GUILayout.Label($"Collector", textAlignStyle);
                    }
                    else if (station.station.stationComponent.isStellar)
                    {
                        GUILayout.Label($"Warpers: {station.station.stationComponent.warperCount}", textAlignStyle);
                    }
                    GUILayout.Label($"{DSPHelper.PositionToLatLon(station.station.stationPosition)}", textAlignStyle, LocationColWidth);
                    GUILayout.EndVertical();



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

            public override void DrawTabGUI()
            {
                if (selectedTab == eTAB_TYPES.TAB_PLANET)
                {
                    var productsPerStation = FilteredSource[selectedTab].GroupBy(pair => pair.station, (group, pairList) => new { station = group, products = pairList });
                    var stationsPerPlanet = productsPerStation.GroupBy(pair => pair.station.planetData, (group, pairList) => new { planet = group, stations = pairList });

                    foreach (var stationsPlanetGroup in stationsPerPlanet)
                    {
                        GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                        GUILayout.Label($"<b>Planet {stationsPlanetGroup.planet.name.Translate() }</b>", textAlignStyle, GUILayout.Width(170));
                        GUILayout.EndHorizontal();

                        foreach (var statProdGroup in stationsPlanetGroup.stations)
                        {
                            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                            GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(75), GUILayout.MaxWidth(75));
                            GUILayout.Label($"{statProdGroup.station.name}", textAlignStyle);
                            if (statProdGroup.station.stationComponent.isCollector)
                            {
                                GUILayout.Label($"Collector", textAlignStyle);
                            }
                            else if (statProdGroup.station.stationComponent.isStellar)
                            {
                                GUILayout.Label($"Warpers: {statProdGroup.station.stationComponent.warperCount}", textAlignStyle);
                            }
                            GUILayout.Label($"{DSPHelper.PositionToLatLon(statProdGroup.station.stationPosition)}", textAlignStyle, LocationColWidth);
                            GUILayout.EndVertical();
                            foreach (var product in statProdGroup.products)
                            {
                                GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(150), GUILayout.MaxWidth(150));
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
                    }
                }
                else if (selectedTab == eTAB_TYPES.TAB_RESOURCE)
                {
                    var stationsPerResource = FilteredSource[selectedTab].GroupBy(pair => pair.product.item.itemId, pair => new ResStationGroup() { station = pair.station, product = pair.product });

                    foreach (var resource in stationsPerResource)
                    {
                        ItemProto itemProto = LDB.items.Select(resource.Key);

                        var resourceTexture = itemProto.iconSprite.texture;

                        GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                        GUILayout.Label($"<b>Resource</b>", textAlignStyle, GUILayout.Width(65));
                        GUILayout.Box(resourceTexture, VeinIconLayoutOptions);
                        GUILayout.Label($"<b>{itemProto.Name.Translate()}</b>", textAlignStyle);
                        GUILayout.EndHorizontal();

                        var interstellarDemand = resource.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.Demand);
                        var interstellarSupply = resource.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.Supply);
                        var interstellarNone = resource.Where(s => s.station.stationComponent.isStellar && s.product.item.remoteLogic == ELogisticStorage.None);

                        var localDemand = resource.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.Demand);
                        var localSupply = resource.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.Supply);
                        var localNone = resource.Where(s => !s.station.stationComponent.isStellar && s.product.item.localLogic == ELogisticStorage.None);

                        List<PresOrderTuple> presOrder = new List<PresOrderTuple>()
                    {
                        { new PresOrderTuple() { name= "Interstellar Supply", stations=interstellarSupply, style=supplyStyle}},
                        { new PresOrderTuple() { name= "Interstellar Demand", stations=interstellarDemand, style=demandStyle}},
                        { new PresOrderTuple() { name= "Interstellar Storage", stations=interstellarNone, style=textAlignStyle}},
                        { new PresOrderTuple() { name= "Planetary Supply", stations=localSupply, style=supplyStyle}},
                        { new PresOrderTuple() { name= "Planetary Demand", stations=localDemand, style=demandStyle}},
                        { new PresOrderTuple() { name= "Planetary Storage", stations=localNone, style=textAlignStyle}}
                    };

                        foreach (var pres in presOrder)
                        {
                            if (pres.stations.Count() > 0)
                            {
                                GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                                GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(75), GUILayout.MaxWidth(75));
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
        }


        private void InitSources()
        {
            DSPStatSources[eTAB_SOURCE_TYPE.VeinMeiners] = new DSPStatSourceVeinMiners();
            DSPStatSources[eTAB_SOURCE_TYPE.LogisticStations] = new DSPStatSourceLogisticStations();
        }

        #endregion


        public void WindowFunc(int id)
        {
            GUILayout.BeginArea(new Rect(winRect.width - 22f, 2f, 20f, 17f));
            if (GUILayout.Button("X"))
            {
                Show = false;
            }
            GUILayout.EndArea();

            GUILayout.BeginVertical();
            // Draw Top Menu
            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            GUILayout.Label($"<b>Sources: </b>", textAlignStyle, GUILayout.Width(100));
            foreach (eTAB_SOURCE_TYPE tabType in DSPStatSources.Keys)
            {
                if (GUILayout.Button(GetSourceTabName(tabType), selectedTabSourceType == tabType ? tabMenuButtonSelected : tabMenuButton))
                {
                    selectedTabSourceType = tabType;
                    selectedTab = DSPStatSources[selectedTabSourceType].TABPages.First();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            GUILayout.Label($"<b>Grouped On: </b>", textAlignStyle, GUILayout.Width(100));
            foreach (eTAB_TYPES tabType in DSPStatSources[selectedTabSourceType].TABPages) //Enum.GetValues(typeof(eTAB_TYPES)))
            {
                if (GUILayout.Button(GetTabName(tabType), selectedTab == tabType ? tabMenuButtonSelected : tabMenuButton))
                {
                    selectedTab = tabType;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            DSPStatSources[selectedTabSourceType].DrawFilterGUI();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            DSPStatSources[selectedTabSourceType].ShouldAutoUpdate = GUILayout.Toggle(DSPStatSources[selectedTabSourceType].ShouldAutoUpdate, $"AutoRefresh");
            GUILayout.EndVertical();

            
            GUILayout.EndHorizontal();

            sv = GUILayout.BeginScrollView(sv, UnityEngine.GUI.skin.box);

            DSPStatSources[selectedTabSourceType].DrawTabGUI();
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            UnityEngine.GUI.DragWindow();

            // Always close window on Escape for now
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            {
                Show = false;
            }
        }
    }
}
