using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
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
        //public static ConfigEntry<int> UILayoutHeightConfig;
        //int[] ValidUiLayoutHeights = new int[4] { 900, 1080, 1440, 2160 };

        //Harmony harmony;

        private static ILHook _changeFixUILayoutHeightILHook = new ILHook(typeof(GameOption).GetMethod("ImportXML"), new ILContext.Manipulator(FixUILayoutHeightIL), new ILHookConfig {
            ManualApply = true
        });

        void InitConfig()
        {
            /*
            UILayoutHeightConfig = Config.Bind("General", "UILayoutHeight", 1080, "What UILayoutHeight should we enforce [900, 1080, 1440, 2160]");
            if (!ValidUiLayoutHeights.Contains(UILayoutHeightConfig.Value))
            {
                Debug.LogError("UILayoutHeight from config file is not from the valid list of resolutions. Resetting to 900");
                UILayoutHeightConfig.Value = 900;
            }
            */
        }

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            InitConfig();

            UnityEngine.Debug.Log("UILayoutHeightFix Plugin Loaded!");

            _changeFixUILayoutHeightILHook.Apply();

            //harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            //harmony.PatchAll();

            //GameOption_Import.OverrideLayoutHeight();
        }

        /*
        internal void OnDestroy()
        {
            if (harmony != null) harmony.UnpatchSelf();
        }

        void Update()
        {
        }

        void OnGUI()
        {
        }
        */
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

        public static void FixUILayoutHeightIL(ILContext il)
        {
            //Console.WriteLine(il.ToString());

            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchStfld(typeof(GameOption).GetField("uiLayoutHeight")),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(GameOption).GetField("uiLayoutHeight")),
                x => x.MatchLdcI4(0x438)
            );
            c.Goto(c.Prev);
            //Console.WriteLine(c.ToString());
            c.Remove();
            c.Emit(OpCodes.Ldc_I4, 4320);

            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(0x438)
            );
            c.Goto(c.Prev);
            //Console.WriteLine(c.ToString());
            c.Remove();
            c.Emit(OpCodes.Ldc_I4, 4320);


            //Console.WriteLine(il.ToString());
        }

        /*
        [HarmonyPatch(typeof(GameOption))]
        class GameOption_Import
        {
            public static void OverrideLayoutHeight()
            {
                UnityEngine.Debug.Log("UILayoutHeightFix - uiLayoutHeight Orig: " + DSPGame.globalOption.uiLayoutHeight + " Overriden to " + UILayoutHeightConfig.Value);
                DSPGame.globalOption.uiLayoutHeight = UILayoutHeightConfig.Value;
                UICanvasScalerHandler.uiLayoutHeight = UILayoutHeightConfig.Value;
            }


            [HarmonyPostfix(), HarmonyPatch("ImportXML")]
            public static void ImportXML(GameOption __instance)
            {
                OverrideLayoutHeight();
            }
        }    
        */

        #endregion // Harmony Patch Hooks in DSP
    }
}
