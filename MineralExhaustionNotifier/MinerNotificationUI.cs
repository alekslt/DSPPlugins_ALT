using BepInEx;
using System;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using DSPPlugins_ALT.Statistics;

namespace DSPPlugins_ALT
{
    public static class MinerNotificationUI
    {
        private const string WindowName = "Miner Mineral Vein Exhaustion Information";
        public static GUILayoutOption PlanetColWidth;
        public static GUILayoutOption LocationColWidth;
        public static GUILayoutOption AlarmColWidth;
        public static GUILayoutOption[] VeinIconLayoutOptions;
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
        private static Rect winRect = new Rect(0, 0, 880, 500); // 680

        private static Vector2 sv;
        private static GUIStyle textAlignStyle;

        private static GUIStyle menuButton;
        private static GUIStyle menuButtonHighlighted;

        private static GUIStyle tabMenuButton;
        private static GUIStyle tabMenuButtonSelected;


        private static bool isInit = false;


        private static Texture2D[] sign_state;
        private static Texture2D menu_button_texture;

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
            VeinETAColWidth = GUILayout.Width(80);

            VeinIconLayoutOptions = new GUILayoutOption[] { GUILayout.Height(35), GUILayout.MaxWidth(35) };
            MenuButtonLayoutOptions = new GUILayoutOption[] { GUILayout.Height(45), GUILayout.MaxWidth(45) };

            tabMenuButton = new GUIStyle(GUI.skin.button);
            tabMenuButtonSelected = new GUIStyle(tabMenuButton);
            tabMenuButtonSelected.normal.background = tabMenuButtonSelected.hover.background = tabMenuButtonSelected.active.background = Texture2D.blackTexture;
 
        }

        public enum eTAB_TYPES { TAB_PLANET, TAB_NETWORK, TAB_RESOURCE };


        public static eTAB_TYPES selectedTab = eTAB_TYPES.TAB_PLANET;

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

            if (!isInit && GameMain.isRunning) { Init(); }

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
            if (GUILayout.Button(eTAB_TYPES.TAB_PLANET.ToString(), selectedTab == eTAB_TYPES.TAB_PLANET ? tabMenuButtonSelected : tabMenuButton))
            {
                selectedTab = eTAB_TYPES.TAB_PLANET;
            }
            if (GUILayout.Button(eTAB_TYPES.TAB_NETWORK.ToString(), selectedTab == eTAB_TYPES.TAB_NETWORK ? tabMenuButtonSelected : tabMenuButton))
            {
                selectedTab = eTAB_TYPES.TAB_NETWORK;
            }
            if (GUILayout.Button(eTAB_TYPES.TAB_RESOURCE.ToString(), selectedTab == eTAB_TYPES.TAB_RESOURCE ? tabMenuButtonSelected : tabMenuButton))
            {
                selectedTab = eTAB_TYPES.TAB_RESOURCE;
            }
            GUILayout.EndHorizontal();

            sv = GUILayout.BeginScrollView(sv, GUI.skin.box);


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
                    DrawMiners(planet.Value);
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
                        GUILayout.Label($"<b>PowerNetwork {netGroup.Key.id} - Health: {Math.Round(netGroup.Key.consumerRatio*100,0) }</b>", textAlignStyle, GUILayout.Width(260));
                        GUILayout.EndHorizontal();

                        DrawHeader();
                        DrawMiners(netGroup);
                    }
                }
            }
            else if (selectedTab == eTAB_TYPES.TAB_RESOURCE)
            {
                var miners = (from miner in minersAll
                              orderby miner.veinAmount
                              group miner by miner.veinName into resourceGroup
                              orderby resourceGroup.Key
                              select new { Name = resourceGroup.Key, Tex=resourceGroup.First().resourceTexture, Miners = resourceGroup.ToList(), SumMiningPerMin = resourceGroup.Sum( m => m.miningRatePerMin) });

                foreach (var resourceGroup in miners)
                {
                    var resourceTexture = resourceGroup.Tex ? resourceGroup.Tex : Texture2D.blackTexture;

                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label($"<b>Resource</b>", textAlignStyle, GUILayout.Width(65));
                    GUILayout.Box(resourceTexture, VeinIconLayoutOptions);
                    GUILayout.Label($"<b>{resourceGroup.Name}</b>", textAlignStyle);
                    GUILayout.Label($"  <b>Sum MiningRate: {resourceGroup.SumMiningPerMin}</b>", textAlignStyle);
                    GUILayout.EndHorizontal();

                    DrawHeader(includePlanet:true);
                    DrawMiners(resourceGroup.Miners, includePlanet: true);
                }
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
