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
    [BepInPlugin("net.toppe.bepinex.dsp.uilayoutheightfix", "UiLayoutHeightFix Plug-In", VersionInfo.VERSION)]
    public class UiLayoutHeightFix : BaseUnityPlugin
    {
        public static ConfigEntry<int> UiLayoutHeightConfig;
        //public static ConfigEntry<int> VeinAmountThreshold;
        //public static ConfigEntry<bool> ShowMenuButton;
        //public static ConfigEntry<KeyCode> ShowNotificationWindowHotKey;

        int[] ValidUiLayoutHeights = new int[4] { 900, 1080, 1440, 2160 };

    void InitConfig()
        {
            UiLayoutHeightConfig = Config.Bind("General", "UiLayoutHeight", 1080, "What UiLayoutHeight should we enforce [900, 1080, 1440, 2160]");
            //VeinAmountThreshold = Config.Bind("General", "VeinAmountThreshold", 6000, "Threshold of vein amount left to mine for adding the miner to the list");
            //ShowMenuButton = Config.Bind("General.Toggles", "ShowMenuButton", true, "Whether or not to show the menu button lower right");
            //ShowNotificationWindowHotKey = Config.Bind<KeyCode>("config", "ShowInformationWindowHotKey", KeyCode.I, "Key to press for toggling the Miner Information Window");
            if (!ValidUiLayoutHeights.Contains(UiLayoutHeightConfig.Value))
            {
                Debug.LogError("UiLayoutHeight from config file is not from the valid list of resolutions. Resetting to 900");
                UiLayoutHeightConfig.Value = 900;
            }
        }

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            InitConfig();

            UnityEngine.Debug.Log("UiLayoutHeightFix Plugin Loaded!");
            var harmony = new Harmony("net.toppe.bepinex.dsp.uilayoutheightfix");
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

        [HarmonyPatch(typeof(DSPGame), "Awake")]
        class DSPGame_Awake
        {
            public static void Postfix(DSPGame __instance)
            {
                UnityEngine.Debug.Log("UiLayoutHeightFix - DSPGame_Awake: " + DSPGame.globalOption.uiLayoutHeight);
            }
        }

        [HarmonyPatch(typeof(GameOption), "LoadGlobal")]
        class GameOption_LoadGlobal
        {
            public static void Postfix(GameOption __instance)
            {
                UnityEngine.Debug.Log("UiLayoutHeightFix - GameOption.LoadGlobal : " + DSPGame.globalOption.uiLayoutHeight);
            }
        }

        [HarmonyPatch(typeof(GameOption))]

        class GameOption_Import
        {
            public static void OverrideLayoutHeight()
            {
                UnityEngine.Debug.Log("UiLayoutHeightFix - uiLayoutHeight Orig: " + DSPGame.globalOption.uiLayoutHeight + " Overriden to " + UiLayoutHeightConfig.Value);
                DSPGame.globalOption.uiLayoutHeight = UiLayoutHeightConfig.Value;
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


        [HarmonyPatch(typeof(UICanvasScalerHandler))]
        class UICanvasScalerHandler_SetCanvas
        {
            static int lastRes = 0;

            [HarmonyPostfix(), HarmonyPatch("SetCanvas")]
            public static void SetCanvas(UICanvasScalerHandler __instance)
            {
                if (UICanvasScalerHandler.uiLayoutHeight != lastRes)
                {
                    UnityEngine.Debug.Log("UiLayoutHeightFix - scaler: " + UICanvasScalerHandler.uiLayoutHeight);
                    lastRes = UICanvasScalerHandler.uiLayoutHeight;
                }
            }
        }

        [HarmonyPatch(typeof(UIOptionWindow))]
        class UIOptionWindow_ApplyOptions
        {
            [HarmonyPostfix(), HarmonyPatch("ApplyOptions")]
            public static void ApplyOptions(UIOptionWindow __instance)
            {

                UnityEngine.Debug.Log("UiLayoutHeightFix - ApplyOptions: " + DSPGame.globalOption.uiLayoutHeight);

            }
        }

        

        #endregion // Harmony Patch Hooks in DSP
    }
}
