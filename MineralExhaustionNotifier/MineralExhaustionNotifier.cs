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
        public static ConfigEntry<int> VeinAmountThreshold;
        public static ConfigEntry<bool> ShowMenuButton;
        public static ConfigEntry<KeyCode> ShowNotificationWindowHotKey;

        public static MineralExhaustionNotifier instance;

        void InitConfig()
        {
            CheckPeriodSeconds = Config.Bind("General", "CheckPeriodSeconds", 5, "How often to check for miner problems");
            VeinAmountThreshold = Config.Bind("General", "VeinAmountThreshold", 6000, "Threshold of vein amount left to mine for adding the miner to the list");
            ShowMenuButton = Config.Bind("General.Toggles", "ShowMenuButton", true, "Whether or not to show the menu button lower right");
            ShowNotificationWindowHotKey = Config.Bind<KeyCode>("config", "ShowInformationWindowHotKey", KeyCode.I, "Key to press for toggling the Miner Information Window");

            GUI.MinerNotificationUI.ShowButton = ShowMenuButton.Value;
        }

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            instance = this;

            minerNotificationUI = new MinerNotificationUI(minerStatistics);
            InitConfig();

            UnityEngine.Debug.Log("Mineral Vein Exhaustion Plugin Loaded!");
            var harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();
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
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

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
