using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DSPPlugins_ALT
{
    

    [BepInPlugin("net.toppe.bepinex.dsp.veinexhaustion", "Mineral Vein Exhaustion Plug-In", "0.5.0.0")]
    public class MineralExhaustionNotifier : BaseUnityPlugin
    {
        public static bool showDialog = true;
        public static long timeStepsSecond = 60;
        public static int checkPeriodSeconds = 5;
        public static MinerStatistics minerStatistics = new MinerStatistics();
        
        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Mineral Vein Exhaustion Plugin Loaded!");
            var harmony = new Harmony("net.toppe.bepinex.dsp.veinexhaustion");
            harmony.PatchAll();
        }
        void Update()
        {
            if (minerStatistics.triggerNotification)
            {
                MinerNotificationUI.HighlightButton = true;

                if (minerStatistics.firstTimeNotification)
                {
                    minerStatistics.triggerNotification = false;
                    minerStatistics.lastTriggeredNotification = GameMain.instance.timei;
                    MinerNotificationUI.Show = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftControl)) {
                MinerNotificationUI.Show = !MinerNotificationUI.Show;
            }
        }

        void OnGUI()
        {
            if (showDialog)
            {
                MinerNotificationUI.OnGUI();
            }
        }
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

        [HarmonyPatch(typeof(GameData), "GameTick")]
        class GameData_GameTick
        {
            static long lastTime = 0;

            public static void Postfix(long time, GameData __instance)
            {
                if (time - lastTime < (timeStepsSecond * checkPeriodSeconds)) { return; }
                lastTime = time;

                MinerStatistics.notificationList.Clear();

                foreach (var planetFactory in __instance.factories)
                {
                    if (planetFactory!= null && planetFactory.factorySystem != null)
                    {
                        minerStatistics.onFactorySystem_GameTick(time, planetFactory.factorySystem);
                    }
                }

                minerStatistics.updateNotificationTimes(time);
                minerStatistics.prioritizeList();
            }
        }

        #endregion // Harmony Patch Hooks in DSP
    }
}
