using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using VeinPlanter.Model;
using VeinPlanter.Presenters;
using VeinPlanter.Service;

namespace VeinPlanter
{


    //[BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "VeinPlanter Plug-In", VersionInfo.VERSION)]
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public partial class VeinPlanter : BaseUnityPlugin
    {    
        public VeinGardenerModel veinGardenerState = new VeinGardenerModel();

        Harmony harmony = null;
        public static ConfigEntry<int> UILayoutHeightConfig;
        //public static ConfigEntry<int> VeinAmountThreshold;
        //public static ConfigEntry<bool> ShowMenuButton;
        //public static ConfigEntry<KeyCode> ShowNotificationWindowHotKey;



        void InitConfig()
        {
            UILayoutHeightConfig = Config.Bind("General", "UILayoutHeight", 1080, "What UILayoutHeight should we enforce [900, 1080, 1440, 2160]");
            //VeinAmountThreshold = Config.Bind("General", "VeinAmountThreshold", 6000, "Threshold of vein amount left to mine for adding the miner to the list");
            //ShowMenuButton = Config.Bind("General.Toggles", "ShowMenuButton", true, "Whether or not to show the menu button lower right");
            //ShowNotificationWindowHotKey = Config.Bind<KeyCode>("config", "ShowInformationWindowHotKey", KeyCode.I, "Key to press for toggling the Miner Information Window");
        }

        public static AssetBundle bundleAssets = null;
        public static AssetBundle bundleScenes = null;

        public readonly VeinGardenerModePresenter modePresenter;
        public readonly VeinGardenerGroupModifyPresenter veinGroupModifyPresenter;

        public static VeinPlanter instance;

        VeinPlanter()
        {
            instance = this;
            modePresenter = new VeinGardenerModePresenter(veinGardenerState);
            veinGroupModifyPresenter = new VeinGardenerGroupModifyPresenter(veinGardenerState);
        }


        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            InitConfig();
            UnityEngine.Debug.Log(Assembly.GetExecutingAssembly().GetName().ToString());
            

#if DEBUG
            bundleAssets = AssetBundle.LoadFromFile(@"C:\Dev\RevEng\DysonSphere\Unity\TestUnityProject\Assets\StreamingAssets\AssetBundles\veinplanter");
#else
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VeinPlanter.Resources.veinplanter"))
            {
                bundleAssets = AssetBundle.LoadFromStream(stream);
            }
#endif


            if (bundleAssets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                //return;
            }

            modePresenter.Init();
            veinGroupModifyPresenter.Init();

            UnityEngine.Debug.Log("VeinPlanter Plugin Loaded!");
            harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();
        }
        
        internal void OnDestroy()
        {
            veinGroupModifyPresenter.OnDestroy();
            modePresenter.OnDestroy();

            // For ScriptEngine hot-reloading
            if (bundleScenes != null) bundleScenes.Unload(true);
            if (bundleAssets != null) bundleAssets.Unload(true);
            if (harmony != null) harmony.UnpatchSelf();
        }
        
        private void Update()
        {
            veinGroupModifyPresenter.Update();
            modePresenter.Update();
            //PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[veinGroupIndex];
            //UIVGGroupEdit.UpdateData((int)veinGroup.type, 1001, veinGroup.count, veinGroup.amount / 10000000000f);
            //UIVGGroupEdit.UpdateData((int)veinGroup.type - 1, UIVGGroupEdit.ProductTypeDropdown.value, veinGroup.count, veinGroup.amount / 100000000f);
            
            if (VFInput.alt && Input.GetKeyDown(KeyCode.V))
            {
                if (VeinGardenerModel.ShowModeMenu)
                {
                    modePresenter.Hide();
                } else
                {
                    modePresenter.Show();
                }
                Input.ResetInputAxes();
            }

            if (!VeinGardenerModel.ShowModeMenu)
            {
                modePresenter.HandleClick();
            }

        }
#endregion // Unity Core Methods

#region Harmony Patch Hooks in DSP



        /*
        [HarmonyPatch(typeof(UIGame))]
        class GameOption_Import
        {
            [HarmonyPostfix(), HarmonyPatch("OnGameBegin")]
            public static void OnGameBegin(UIGame __instance)
            {
                var test = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Key Tips");
                if (test != null)
                {
                    test.SetActive(false);
                }
            }
        }
        */



 
#endregion // Harmony Patch Hooks in DSP


    }

    
}
