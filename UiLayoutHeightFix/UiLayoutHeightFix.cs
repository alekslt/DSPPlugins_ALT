using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DSPPlugins_ALT
{
    [BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "UiLayoutHeightFix Plug-In", VersionInfo.VERSION)]
    public class UILayoutHeightFix : BaseUnityPlugin
    {
        public static ConfigEntry<int> UILayoutHeightConfig;
        //public static ConfigEntry<int> VeinAmountThreshold;
        //public static ConfigEntry<bool> ShowMenuButton;
        //public static ConfigEntry<KeyCode> ShowNotificationWindowHotKey;

        int[] ValidUiLayoutHeights = new int[4] { 900, 1080, 1440, 2160 };

    void InitConfig()
        {
            UILayoutHeightConfig = Config.Bind("General", "UILayoutHeight", 1080, "What UILayoutHeight should we enforce [900, 1080, 1440, 2160]");
            //VeinAmountThreshold = Config.Bind("General", "VeinAmountThreshold", 6000, "Threshold of vein amount left to mine for adding the miner to the list");
            //ShowMenuButton = Config.Bind("General.Toggles", "ShowMenuButton", true, "Whether or not to show the menu button lower right");
            //ShowNotificationWindowHotKey = Config.Bind<KeyCode>("config", "ShowInformationWindowHotKey", KeyCode.I, "Key to press for toggling the Miner Information Window");
            if (!ValidUiLayoutHeights.Contains(UILayoutHeightConfig.Value))
            {
                Debug.LogError("UILayoutHeight from config file is not from the valid list of resolutions. Resetting to 900");
                UILayoutHeightConfig.Value = 900;
            }
        }

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            InitConfig();

            UnityEngine.Debug.Log("UILayoutHeightFix Plugin Loaded!");
            var harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();
        }
        void Update()
        {
        }

        void OnGUI()
        {
        }
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

        [HarmonyPatch(typeof(GameOption))]
        class GameOption_Import
        {
            public static void OverrideLayoutHeight()
            {
                UnityEngine.Debug.Log("UILayoutHeightFix - uiLayoutHeight Orig: " + DSPGame.globalOption.uiLayoutHeight + " Overriden to " + UILayoutHeightConfig.Value);
                DSPGame.globalOption.uiLayoutHeight = UILayoutHeightConfig.Value;
            }

            [HarmonyPostfix(), HarmonyPatch("Import")]
            public static void Import(GameOption __instance)
            {
                OverrideLayoutHeight();
            }

            [HarmonyPostfix(), HarmonyPatch("ImportXML")]
            public static void ImportXML(GameOption __instance)
            {
                OverrideLayoutHeight();
            }
        }      

        #endregion // Harmony Patch Hooks in DSP
    }
}
