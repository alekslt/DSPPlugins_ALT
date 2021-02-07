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
        //public static GUILayoutOption VeinIconColWidth;
        public static GUILayoutOption VeinTypeColWidth;
        public static GUILayoutOption VeinAmountColWidth;

        public static bool HighlightButton = false;
        public static bool ShowButton = true;
        public static bool Show;
        private static Rect winRect = new Rect(0, 0, 460, 300);

        private static Vector2 sv;
        private static GUIStyle textAlignStyle;

        private static GUIStyle menuButton;
        private static GUIStyle menuButtonHighlighted;

        private static bool isInit = false;
        private static Texture2D sign_state_0;

        public static void EatInputInRect(Rect eatRect)
        {
            if (eatRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
            {
                // Ideally I want to only block mouse events from going through.
                var isMouseInput = Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.mouseScrollDelta.y != 0;

                if (!isMouseInput)
                {
                    // UnityEngine.Debug.Log("Canceling capture due to input not being mouse");
                    return;
                } else
                {
                    Input.ResetInputAxes();
                }
                
            }
        }

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

            // sign_state_0 = Resources.Load("sign-state-0") as Texture2D;
            //UIButton[] warningButtons = Traverse.Create(UIRoot.instance.optionWindow).Field("fieldname").GetValue() as UIButton[];
            // warningButtons[0].button.image.mainTexture
            //var rhc = UIRoot.instance.optionWindow.GetFieldValue<UIButton[]>("buildingWarnButtons");

            PlanetColWidth = GUILayout.Width(160);
            LocationColWidth = GUILayout.Width(80);
            AlarmColWidth = GUILayout.Width(90);
            //GUILayoutOption VeinIconColWidth = GUILayout.Width(50);
            VeinTypeColWidth = GUILayout.Width(100);
            VeinAmountColWidth = GUILayout.Width(110);
        }


        public static void OnGUI()
        {
            if (!isInit) { Init(); }

            var uiGame = BGMController.instance.uiGame;
            var shouldShowByGameState = !(uiGame.techTree.active || uiGame.dysonmap.active || uiGame.starmap.active || uiGame.escMenu.active || uiGame.hideAllUI0 || uiGame.hideAllUI1) &&
                DSPGame.GameDesc != null && DSPGame.IsMenuDemo == false && DSPGame.Game.running && (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Sail);

            if (Show && shouldShowByGameState)
            {
                winRect = GUILayout.Window(55416753, winRect, WindowFunc, WindowName);
                EatInputInRect(winRect);
            }

            if (ShowButton && shouldShowByGameState)
            {
                Rect buttonWinRect = new Rect(Screen.width - 120, Screen.height - 46, 45, 45);
                //buttonWinRect = GUILayout.Window(55416752, buttonWinRect, ButtonWindowFunc, "Mineral Vein Miner Exhaustion Button");
                //EatInputInRect(buttonWinRect);
                var activeStyle = HighlightButton ? menuButtonHighlighted : menuButton;
                GUILayout.BeginArea(buttonWinRect);
                if (GUILayout.Button("M", activeStyle, GUILayout.Width(45)))
                {
                    Show = !Show;
                }
                GUILayout.EndArea();
            }
        }

        public static void Draw(PlanetFactory factory)
        {
            var entitySignMat = Configs.builtin.entitySignMat;

            bool showSign = true;
            bool entitySignOn = true;

            Shader.SetGlobalFloat("_Global_ShowEntitySign", (!showSign || !entitySignOn) ? 0f : 1f);
            // Shader.SetGlobalFloat("_Global_ShowEntityIcon", (!showIcon || !entitySignOn) ? 0f : 1f);
            // Shader.SetGlobalInt("_EntitySignMask", buildingWarnMask);
            if (factory.entityCursor > 1)
            {
                entitySignMat.SetBuffer("_SignBuffer", factory.planet.factoryModel.entitySignBuffer);
                entitySignMat.SetPass(0);
                Graphics.DrawProcedural(MeshTopology.Quads, 8 * factory.entityCursor, 1);
            }
        }

        public static void DrawHeader()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            // GUILayout.Label($"<b>Planet</b>", textAlignStyle, PlanetColWidth);
            GUILayout.Label($"<b>Location</b>", textAlignStyle, LocationColWidth);
            GUILayout.Label($"<b>Alarm</b>", textAlignStyle, AlarmColWidth);
            //GUILayout.Label($"<b>Vein</b>", textAlignStyle, VeinIconColWidth);
            GUILayout.Label($"<b>Vein</b>", textAlignStyle, VeinTypeColWidth);
            GUILayout.Label($"<b>Amount Left</b>", textAlignStyle, VeinAmountColWidth);
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

            foreach (var planet in MinerStatistics.notificationList)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"<b>Planet {planet.Key}</b>", textAlignStyle, GUILayout.Width(260));
                GUILayout.EndHorizontal();

                DrawHeader();

                foreach (var item in planet.Value)
                {
                    string latLon = DSPHelper.PositionToLatLon(item.plantPosition);

                    GUILayout.BeginHorizontal(GUI.skin.box);
                    // GUILayout.Label($"{item.planetName}", textAlignStyle, PlanetColWidth);
                    GUILayout.Label($"{latLon}", textAlignStyle, LocationColWidth);
                    GUILayout.Label(DSPHelper.SignNumToText(item.signType), textAlignStyle, AlarmColWidth);
                    //GUILayout.Label($"{item.veinName}", textAlignStyle, VeinIconColWidth);
                    GUILayout.Label($"{item.veinName}", textAlignStyle, VeinTypeColWidth);
                    GUILayout.Label($"{item.veinAmount}", textAlignStyle, VeinAmountColWidth);
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
