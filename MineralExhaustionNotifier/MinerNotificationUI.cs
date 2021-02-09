using BepInEx;
using System;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;

namespace DSPPlugins_ALT
{
    public static class MinerNotificationUI
    {
        private const string WindowName = "Miner Mineral Vein Exhaustion Information";
        public static GUILayoutOption PlanetColWidth;
        public static GUILayoutOption LocationColWidth;
        public static GUILayoutOption AlarmColWidth;
        public static GUILayoutOption[] VeinIconLayoutOptions;
        //public static GUILayoutOption VeinIconColWidth;
        //public static GUILayoutOption VeinIconColHeight;
        public static GUILayoutOption VeinTypeColWidth;
        public static GUILayoutOption VeinAmountColWidth;
        public static GUILayoutOption VeinRateColWidth;
        public static GUILayoutOption VeinETAColWidth;

        public static bool HighlightButton = false;
        public static bool ShowButton = true;
        public static bool Show;
        private static Rect winRect = new Rect(0, 0, 700, 300); // 540

        private static Vector2 sv;
        private static GUIStyle textAlignStyle;

        private static GUIStyle menuButton;
        private static GUIStyle menuButtonHighlighted;

        private static bool isInit = false;


        private static Texture2D[] sign_state;

        private static void Init()
        {
            isInit = true;

            textAlignStyle = new GUIStyle(GUI.skin.label);
            textAlignStyle.alignment = TextAnchor.MiddleLeft;

            menuButton = new GUIStyle(GUI.skin.button);
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

            sign_state = new Texture2D[10];

            for (int i=0; i<= 8; i++)
            {
                //Debug.Log("Loading sign-state-" + i + " - res :" + sign_state[i]);
                sign_state[i] = Resources.Load<Texture2D>("ui/textures/sprites/icons/sign-state-"+i);
                if (sign_state[i] == null)
                {
                    Debug.LogWarning("Failed Loading sign-state-" + i);
                }
            }
            
        }


        public static void OnGUI()
        {
            if (!isInit && GameMain.isRunning) { Init(); }

            var uiGame = BGMController.instance.uiGame;
            var shouldShowByGameState = !(uiGame.techTree.active || uiGame.dysonmap.active || uiGame.starmap.active || uiGame.escMenu.active || uiGame.hideAllUI0 || uiGame.hideAllUI1) &&
                DSPGame.GameDesc != null && DSPGame.IsMenuDemo == false && DSPGame.Game.running && (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Sail);

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
                if (GUILayout.Button("M", activeStyle, GUILayout.Width(45)))
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

        public static void DrawHeader()
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.MaxHeight(45));
            // GUILayout.Label($"<b>Planet</b>", textAlignStyle, PlanetColWidth);
            GUILayout.Label($"<b>Location</b>", textAlignStyle, LocationColWidth);
            GUILayout.Label($"<b>Alarm</b>", textAlignStyle, AlarmColWidth);
            GUILayout.Label($"<b>Vein</b>", textAlignStyle, VeinTypeColWidth);
            GUILayout.Label($"<b>Amount Left</b>", textAlignStyle, VeinAmountColWidth);
            GUILayout.Label($"<b>Mining Rate/min</b>", textAlignStyle, VeinRateColWidth);
            GUILayout.Label($"<b>~Time to Empty</b>", textAlignStyle, VeinETAColWidth);
            GUILayout.EndHorizontal();
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
            sv = GUILayout.BeginScrollView(sv, GUI.skin.box);

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(5, 0, 0, 0)
            };

            foreach (var planet in MinerStatistics.notificationList)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"<b>Planet {planet.Key}</b>", textAlignStyle, GUILayout.Width(260));
                GUILayout.EndHorizontal();

                DrawHeader();

                foreach (var item in planet.Value)
                {
                    string latLon = DSPHelper.PositionToLatLon(item.plantPosition);

                    var alarmSign = (item.signType != SignData.NONE) ? sign_state[DSPHelper.SignNumToTextureIndex(item.signType)] : Texture2D.blackTexture;
                    var resourceTexture = item.resourceTexture ? item.resourceTexture : Texture2D.blackTexture;

                    // Debug.Log("Drawing sign-state-" + item.signType);

                    GUILayout.BeginHorizontal(boxStyle, GUILayout.MaxHeight(45));
                    // GUILayout.Label($"{item.planetName}", textAlignStyle, PlanetColWidth);
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
                    GUILayout.Label($"{item.miningRatePerMin}", textAlignStyle, VeinRateColWidth);
                    GUILayout.Label($"{item.minutesToEmptyVeinTxt}", textAlignStyle, VeinETAColWidth);

                    GUILayout.EndHorizontal();
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
