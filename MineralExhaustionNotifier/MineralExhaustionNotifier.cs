using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using DSPPlugins_ALT.Statistics;
using DSPPlugins_ALT.GUI;

namespace DSPPlugins_ALT
{
    [BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "Mineral Vein Exhaustion Plug-In", VersionInfo.VERSION)]
    public class MineralExhaustionNotifier : BaseUnityPlugin
    {
        public static bool showDialog = true;
        public static long timeStepsSecond = 60;

        public static DSPStatistics minerStatistics = new DSPStatistics();
        public static MinerNotificationUI minerNotificationUI;

        public static ConfigEntry<int> CheckPeriodSeconds;
        public static ConfigEntry<bool> ShowPopups;
        public static ConfigEntry<bool> ShowMenuButton;
        public static ConfigEntry<int> MenuButtonPlacementX;
        public static ConfigEntry<int> MenuButtonPlacementY;
        public static ConfigEntry<KeyCode> ShowNotificationWindowHotKey;

        public static ConfigEntry<bool> ScaleGUIWithUILayoutHeight;
        public static ConfigEntry<int> OverriddenUILayoutHeight;

        public static MineralExhaustionNotifier instance;

        private static Harmony harmony;

        void InitConfig()
        {
            CheckPeriodSeconds = Config.Bind("General", "CheckPeriodSeconds", 5, "How often to check for miner problems");
            ShowPopups = Config.Bind("General.Toggles", "ShowPopups", false, "Show popup window (e.g. during start of game) or not");
            MenuButtonPlacementX = Config.Bind("General", "MenuButtonPlacementX", 45, "Menubutton X and Y Offset from bottom right of the screen. X,Y - 123, 46 = Under Tech; 45, 128 = Right under stat");
            MenuButtonPlacementY = Config.Bind("General", "MenuButtonPlacementY", 128, "Menubutton X and Y Offset from bottom right of the screen. X,Y - 123, 46 = Under Tech; 45, 128 = Right under stat");

            ShowMenuButton = Config.Bind("General.Toggles", "ShowMenuButton", true, "Whether or not to show the menu button lower right");
            ShowNotificationWindowHotKey = Config.Bind<KeyCode>("config", "ShowInformationWindowHotKey", KeyCode.I, "Key to press for toggling the Miner Information Window in addition to ALT");

            ScaleGUIWithUILayoutHeight = Config.Bind("General.Toggles", "ScaleGUIWithUILayoutHeight", true, "Use the game setting UI Layout Reference Height to scale GUI instead of the config option");
            OverriddenUILayoutHeight = Config.Bind("General", "OverriddenUILayoutHeight", 1080, "If ScaleGUIWithUILayoutHeight is set to false, then use this value to scale UI. Common values: [900, 1080, 1440, 2160]");

            // X,Y - 123, 46 = Under Tech; 45, 128 = Right under stat
        }

        void ApplyConfig()
        {
            GUI.MinerNotificationUI.ShowButton = ShowMenuButton.Value;
            minerNotificationUI.MenuButtonXOffset = MenuButtonPlacementX.Value;
            minerNotificationUI.MenuButtonYOffset = MenuButtonPlacementY.Value;

            minerStatistics.triggerNotification = ShowPopups.Value;
            minerStatistics.firstTimeNotification = ShowPopups.Value;
        }

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            InitConfig();
            instance = this;
            minerNotificationUI = new MinerNotificationUI(minerStatistics);
            ApplyConfig();

            UnityEngine.Debug.Log("Mineral Vein Exhaustion Plugin Loaded!");
            harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();
        }

        internal void OnDestroy()
        {
            // For ScriptEngine hot-reloading
            //if (bundleScenes != null) bundleScenes.Unload(true);
            //if (bundleAssets != null) bundleAssets.Unload(true);
            if (harmony != null) harmony.UnpatchSelf();
        }


        bool keyModifierAltIsDown = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
                keyModifierAltIsDown = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
            {
                keyModifierAltIsDown = false;
            }


            if (minerStatistics.triggerNotification)
            {
                if (minerStatistics.firstTimeNotification)
                {
                    minerStatistics.triggerNotification = false;
                    minerStatistics.firstTimeNotification = false;
                    minerStatistics.lastTriggeredNotification = GameMain.instance.timei;
                    GUI.MinerNotificationUI.Show = true;
                }
            }

            if (keyModifierAltIsDown && Input.GetKeyDown(ShowNotificationWindowHotKey.Value)) {
                GUI.MinerNotificationUI.Show = !GUI.MinerNotificationUI.Show;
            }
        }

        void OnGUI()
        {
            if (showDialog)
            {
                minerNotificationUI.OnGUI();
            }
        }

        private static void UpdateScaling()
        {
            if (ScaleGUIWithUILayoutHeight.Value == true)
            {
                GUI.MinerNotificationUI.UILayoutHeight = DSPGame.globalOption.uiLayoutHeight;
            }
            else
            {
                GUI.MinerNotificationUI.UILayoutHeight = OverriddenUILayoutHeight.Value;
            }
        }

        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

        [HarmonyPatch(typeof(GameOption))]
        class GameOption_Import
        {
            [HarmonyPostfix(), HarmonyPatch("Apply")]
            public static void Import(GameOption __instance)
            {
                UpdateScaling();
            }
        }

        [HarmonyPatch(typeof(GameData), "GameTick")]
        class GameData_GameTick
        {
            public static void Postfix(long time, GameData __instance)
            {
                minerStatistics.onGameData_GameTick(time, __instance);
            }
        }

        #endregion // Harmony Patch Hooks in DSP
    }
}
