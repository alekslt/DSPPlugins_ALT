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

        
        //public static GUILayoutOption VeinIconColWidth;
        //public static GUILayoutOption VeinIconColHeight;


        public static bool HighlightButton = false;
        public static bool ShowButton = true;
        public static bool Show;
        private static Rect winRect = new Rect(0, 0, 1015, 650); // 680

        public static int UILayoutHeight { get; set; } = 1080;

        private static Vector2 sv;


        private static bool isInit = false;



        //private static Slider slider;
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
            /*
            foreach (var source in DSPStatSources.Where(s => s.Value.ShouldAutoUpdate))
            {
                source.Value.UpdateSource();
            }
            */
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

        public static float ScaleRatio { get; set; } = 1.0f;

        const float FixedSizeAdjustOriginal = 0.9f;
        public static float FixedSizeAdjust { get; set; } = FixedSizeAdjustOriginal;

        public static void AutoResize(int designScreenHeight, bool applyCustomScale = true)
        {
            if (applyCustomScale)
            {
                designScreenHeight = (int)Math.Round((float)designScreenHeight / FixedSizeAdjust);
            }
            
            ScaledScreenHeight = designScreenHeight;
            ScaleRatio = (float)Screen.height / designScreenHeight;
            
            // Vector2 resizeRatio = new Vector2((float)Screen.width / screenWidth, (float)Screen.height / screenHeight);
            ScaledScreenWidth = (int)Math.Round(Screen.width / ScaleRatio);
            UnityEngine.GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(ScaleRatio, ScaleRatio, 1.0f));
        }



        public void OnGUI()
        {
            var uiGame = BGMController.instance.uiGame;
            var shouldShowByGameState = DSPGame.GameDesc != null && uiGame != null && uiGame.gameData != null && uiGame.guideComplete && DSPGame.IsMenuDemo == false && DSPGame.Game.running && (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Sail) &&
                !(uiGame.techTree.active || uiGame.dysonmap.active || uiGame.starmap.active || uiGame.escMenu.active || uiGame.hideAllUI0 || uiGame.hideAllUI1) && uiGame.gameMenu.active;

            //Show = shouldShowByGameState = DSPGame.MenuDemoLoaded;

            if (!shouldShowByGameState)
            {
                return;
            }

            if (!isInit && GameMain.isRunning) { UITheme.Init(); InitSources(); isInit = true; }

            AutoResize(DSPGame.globalOption.uiLayoutHeight, applyCustomScale: false);
            if (ShowButton && shouldShowByGameState)
            {
                DrawMenuButton();
            }

            AutoResize(UILayoutHeight);
            if (Show && shouldShowByGameState)
            {
                winRect = GUILayout.Window(55416753, winRect, WindowFunc, WindowName);
                UIHelper.EatInputInRect(winRect);
            }
        }

        // X,Y - 123, 46 = Bottom, 45, 128 = Right under stat
        public int MenuButtonXOffset { get; set; } = 45;
        public int MenuButtonYOffset { get; set; } = 128;

        private void DrawMenuButton()
        {
            // This guy is needs to be locked to the DSPGame.globalOption.uiLayoutHeight ratio.
            Rect buttonWinRect = new Rect(ScaledScreenWidth - (MenuButtonXOffset), DSPGame.globalOption.uiLayoutHeight - (MenuButtonYOffset), 42, 42);
            var activeStyle = HighlightButton ? UITheme.MenuButtonHighlightedStyle : UITheme.MenuButtonStyle;
            GUILayout.BeginArea(buttonWinRect);
            if (GUILayout.Button("M", activeStyle, UITheme.MenuButtonLayoutOptions))
            {
                Show = !Show;
            }
            GUILayout.EndArea();
        }




        private void InitSources()
        {
            DSPStatSources[eTAB_SOURCE_TYPE.VeinMeiners] = new DSPStatSourceVeinMiners();
            DSPStatSources[eTAB_SOURCE_TYPE.LogisticStations] = new DSPStatSourceLogisticStations();
        }


        public void WindowFunc(int id)
        {
            GUILayout.BeginArea(new Rect(winRect.width - 22f, 2f, 20f, 17f));
            if (GUILayout.Button("X"))
            {
                Show = false;
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(winRect.width - 45f, 2f, 20f, 17f));
            if (GUILayout.Button("+"))
            {
                FixedSizeAdjust = Mathf.Min(FixedSizeAdjustOriginal + 0.8f, FixedSizeAdjust + 0.1f);
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(winRect.width - 64f, 2f, 20f, 17f));
            if (GUILayout.Button("1"))
            {
                FixedSizeAdjust = FixedSizeAdjustOriginal;
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(winRect.width - 83f, 2f, 20f, 17f));
            if (GUILayout.Button("-"))
            {
                FixedSizeAdjust = Mathf.Max(FixedSizeAdjustOriginal - 0.5f, FixedSizeAdjust - 0.1f);
            }
            GUILayout.EndArea();

            GUILayout.BeginVertical();
            // Draw Top Menu
            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            GUILayout.Label($"<b>Sources: </b>", UITheme.TextAlignStyle, GUILayout.Width(100));
            foreach (eTAB_SOURCE_TYPE tabType in DSPStatSources.Keys)
            {
                if (GUILayout.Button(UIHelper.GetSourceTabName(tabType), selectedTabSourceType == tabType ? UITheme.TabMenuButtonSelectedStyle : UITheme.TabMenuButtonStyle))
                {
                    selectedTabSourceType = tabType;
                    selectedTab = DSPStatSources[selectedTabSourceType].TABPages.First();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            GUILayout.Label($"<b>Grouped On: </b>", UITheme.TextAlignStyle, GUILayout.Width(100));
            foreach (eTAB_TYPES tabType in DSPStatSources[selectedTabSourceType].TABPages) //Enum.GetValues(typeof(eTAB_TYPES)))
            {
                if (GUILayout.Button(UIHelper.GetTabName(tabType), selectedTab == tabType ? UITheme.TabMenuButtonSelectedStyle : UITheme.TabMenuButtonStyle))
                {
                    selectedTab = tabType;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            DSPStatSources[selectedTabSourceType].DrawFilterGUI(selectedTab);
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            bool shouldUpdate = false;
            var oldAutoUpdateState = DSPStatSources[selectedTabSourceType].ShouldAutoUpdate;
            DSPStatSources[selectedTabSourceType].ShouldAutoUpdate = GUILayout.Toggle(DSPStatSources[selectedTabSourceType].ShouldAutoUpdate, $"AutoRefresh");

            var oldCollapsedState = DSPStatSources[selectedTabSourceType].DefaultIsChildrenCollapsedState;
            DSPStatSources[selectedTabSourceType].DefaultIsChildrenCollapsedState = GUILayout.Toggle(DSPStatSources[selectedTabSourceType].DefaultIsChildrenCollapsedState, $"AutoCollapsed");
            if (oldCollapsedState != DSPStatSources[selectedTabSourceType].DefaultIsChildrenCollapsedState)
            if (oldAutoUpdateState != DSPStatSources[selectedTabSourceType].ShouldAutoUpdate)
            {
                shouldUpdate = true;
            }
            if (shouldUpdate)
            {
                DSPStatSources[selectedTabSourceType].CollapsedState.Clear();
            }
            GUILayout.EndVertical();
 
            GUILayout.EndHorizontal();

            sv = GUILayout.BeginScrollView(sv, UnityEngine.GUI.skin.box);

            DSPStatSources[selectedTabSourceType].DrawTabGUI(selectedTab);
            
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
