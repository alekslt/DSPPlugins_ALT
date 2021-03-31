using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using VeinPlanter.Presenters;
using VeinPlanter.Service;

namespace VeinPlanter
{
    //[BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "VeinPlanter Plug-In", VersionInfo.VERSION)]
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public class VeinPlanter : BaseUnityPlugin
    {
        public enum eVeinModificationMode { Deactivated, PlanetVeins, ExtendVein, MoveVein, RemoveVein, AddVeinGroup, ModifyVeinGroup, MoveVeinGroup, RemoveVeinGroup, TerrainLower };

        public class VeinGardenerState
        {
            public static bool ShowModeMenu = false;
            public static bool ShowPlanetVeinMenu = false;

            public float oreMultiplier = 100000000f;

            public List<int> products = new List<int>() { 1001, 1002, 1003, 1004, 1005, 1006, 1030, 1031, 1011, 1012, 1013, 1014, 1015, 1016, 1101, 1104, 1105, 1106, 1108, 1109, 1103, 1107, 1110, 1119, 1111, 1112, 1113, 1201, 1102, 1202, 1203, 1204, 1205, 1206, 1127, 1301, 1303, 1305, 1302, 1304, 1402, 1401, 1404, 1501, 1000, 1007, 1114, 1116, 1120, 1121, 1122, 1208, 1801, 1802, 1803, 1115, 1123, 1124, 1117, 1118, 1126, 1209, 1210, 1403, 1405, 1406, 5001, 5002, 1125, 1502, 1503, 1131, 1141, 1142, 1143, 2001, 2002, 2003, 2011, 2012, 2013, 2020, 2101, 2102, 2106, 2303, 2304, 2305, 2201, 2202, 2212, 2203, 2204, 2211, 2301, 2302, 2307, 2308, 2306, 2309, 2314, 2313, 2205, 2206, 2207, 2311, 2208, 2312, 2209, 2310, 2210, 2103, 2104, 2105, 2901, 6001, 6002, 6003, 6004, 6005, 6006 };

            public eVeinModificationMode modMode = eVeinModificationMode.Deactivated;

            public PlanetData localPlanet;
            public int veinGroupIndex;

            public EVeinType newVeinGroupType = EVeinType.Iron;

            public void ChangeMode(eVeinModificationMode newmode)
            {
                modMode = newmode;
                ShowModeMenu = false;
                dialog = null;
                UIRealtimeTip.Popup("Vein Gardener Mode Changed to: " + modMode);
            }
        }

        public VeinGardenerState veinGardenerState = new VeinGardenerState();

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
            //Assembly a = Assembly.Load("VeinPlanterUI");

            //"C:\Dev\RevEng\DysonSphere\Unity\TestUnityProject\Library\ScriptAssemblies\VeinPlanterUI.dll"
            /*
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VeinPlanter.Resources.veinplanter"))
            {
                bundleAssets = AssetBundle.LoadFromStream(stream);
            }*/
            bundleAssets = AssetBundle.LoadFromFile(@"C:\Dev\RevEng\DysonSphere\Unity\TestUnityProject\Assets\StreamingAssets\AssetBundles\veinplanter");
            if (bundleAssets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                //return;
            }

            //TestGUI = GameObject.Instantiate(bundleAssets.LoadAsset<GameObject>("Assets/UI/UITest.prefab"));
            //TestGUI = GameObject.Instantiate(bundleAssets.LoadAsset<GameObject>("Assets/UI/UIMVPTest.prefab"));

            //TestGUI2 = GameObject.Instantiate(bundleAssets.LoadAsset<GameObject>("VeinGroupModification"));

            modePresenter.Init();
            veinGroupModifyPresenter.Init();

            UnityEngine.Debug.Log("VeinPlanter Plugin Loaded!");
            harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();
        }
        
        //

        internal void OnDestroy()
        {
            veinGroupModifyPresenter.OnDestroy();
            modePresenter.OnDestroy();

            // For ScriptEngine hot-reloading
            if (bundleScenes != null) bundleScenes.Unload(true);
            if (bundleAssets != null) bundleAssets.Unload(true);
            if (harmony != null) harmony.UnpatchSelf();
        }

        // private UIVeinDetailNode nodePrefab;
        static public UIVeinGroupDialog dialog = null;
        //static public UIPlanetVeins planetVeinsDialog = null;

        bool shouldEatInput = false;



        public Vector3[] reformPoints = new Vector3[100];

        
        private void Update()
        {
            veinGroupModifyPresenter.Update();
            modePresenter.Update();
            //PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[veinGroupIndex];
            //UIVGGroupEdit.UpdateData((int)veinGroup.type, 1001, veinGroup.count, veinGroup.amount / 10000000000f);
            //UIVGGroupEdit.UpdateData((int)veinGroup.type - 1, UIVGGroupEdit.ProductTypeDropdown.value, veinGroup.count, veinGroup.amount / 100000000f);
            shouldEatInput = false;
 
            if (dialog != null)
            {
                dialog.UpdatePos();
            }
            if (shouldEatInput)
            {
                Input.ResetInputAxes();
            }

            if (VFInput.alt && Input.GetKeyDown(KeyCode.V))
            {
                if (VeinGardenerState.ShowModeMenu)
                {
                    modePresenter.Hide();
                } else
                {
                    modePresenter.Show();
                }
                Input.ResetInputAxes();
            }

            if (!VeinGardenerState.ShowModeMenu)
            {
                modePresenter.HandleClick();
            }

        }
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

        public class ResetConfigHook
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(GameMain), "Begin")]
            public static void HookGameStart()
            {
                VeinPlanter.instance.modePresenter.Hide();
                VeinPlanter.instance.veinGroupModifyPresenter.Hide();
                Debug.Log("Resetting dialog");
            }
        }

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



        [HarmonyPatch(typeof(UIVeinDetailNode))]
        public static class UIVeinDetailNode_Patch
        {

            static bool ShouldAbortSanityChecks(UIVeinDetailNode __instance, out PlanetData.VeinGroup veinGroup, out ItemProto itemProto)
            {
                veinGroup = default(PlanetData.VeinGroup);
                itemProto = null;

                if (__instance.inspectPlanet == null)
                {
                    return true;
                }
                veinGroup = __instance.inspectPlanet.veinGroups[__instance.veinGroupIndex];
                if (veinGroup.count == 0 || veinGroup.type != EVeinType.Oil)
                {
                    return true;
                }

                var prodId = (from vein in __instance.inspectPlanet.factory.veinPool where vein.groupIndex == __instance.veinGroupIndex select vein.productId).First();
                itemProto = LDB.items.Select(prodId);

                return false;
            }

            [HarmonyPostfix(), HarmonyPatch("Refresh")]
            public static void Refresh(UIVeinDetailNode __instance)
            {
                if (ShouldAbortSanityChecks(__instance, out PlanetData.VeinGroup veinGroup, out ItemProto itemProto)) { return; }

                __instance.veinIcon.sprite = itemProto.iconSprite;
                __instance.infoText.text = string.Concat(new string[]
                {
                    veinGroup.count.ToString(), "空格个".Translate(), itemProto.name, "产量".Translate(),
                    ((float)veinGroup.amount * VeinData.oilSpeedMultiplier).ToString("0.00"), "/s"
                });
            }

            [HarmonyPostfix(), HarmonyPatch("_OnUpdate")]
            public static void _OnUpdate(UIVeinDetailNode __instance)
            {
                if (ShouldAbortSanityChecks(__instance, out PlanetData.VeinGroup veinGroup, out ItemProto itemProto)) { return; }
                __instance.infoText.text = string.Concat(new string[]
                {
                    veinGroup.count.ToString(), "空格个".Translate(), itemProto.name, "产量".Translate(),
                    ((float)veinGroup.amount * VeinData.oilSpeedMultiplier).ToString("0.00"), "/s"
                });
            }
            /*

            [HarmonyPatch("_OnUpdate")]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                
                return new CodeMatcher(instructions)
                    .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "oilSpeedMultiplier"))
                    .MatchBack(false, // false = move at the start of the match, true = move at the end of the match
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "veinProto"))
                    .SetAndAdvance(OpCodes.Ldstr, "Test")
                    .SetOpcodeAndAdvance(OpCodes.Nop)
                    .SetOpcodeAndAdvance(OpCodes.Nop)
                    .InstructionEnumeration();*/

            /*
            return new CodeMatcher(instructions)
                .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "veinIcon"))
                .MatchBack(false, // false = move at the start of the match, true = move at the end of the match
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "veinProto"))
                .SetAndAdvance(OpCodes.Ldstr, "Test")
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .InstructionEnumeration();


            return new CodeMatcher(instructions)
             .InstructionEnumeration();

        }
            */
        }

        #endregion // Harmony Patch Hooks in DSP

        public int ReformSnap(Vector3 pos, int reformSize, int reformType, int reformColor, Vector3[] reformPoints, int[] reformIndices, PlatformSystem platform, out Vector3 reformCenter, float localPlanetRadius)
        {
            int num = veinGardenerState.localPlanet.aux.mainGrid.ReformSnapTo(pos, reformSize, reformType, reformColor, reformPoints, reformIndices, platform, out reformCenter);
            float num2 = localPlanetRadius - 50.2f;
            for (int i = 0; i < num; i++)
            {
                reformPoints[i].x *= num2;
                reformPoints[i].y *= num2;
                reformPoints[i].z *= num2;
            }
            reformCenter *= num2;
            return num;
        }

        private void TerrainLower(Vector3 worldPos)
        {
            PlayerAction_Build playerAction_Build = GameMain.mainPlayer?.controller.actionBuild;

            if (VFInput.alt && playerAction_Build != null)
            {
                var reformSize = 1;
                var reformType = -1;
                var reformColor = -1;

                bool veinBuried = false;
                float radius = 0.990945935f * (float)reformSize;
                Vector3 reformCenterPoint;

                float localPlanetRadius = veinGardenerState.localPlanet.radius;
                float localPlanetRealRadius = veinGardenerState.localPlanet.realRadius;

                Debug.Log("TerrainLower Radius: " + localPlanetRadius + " RealRadius: " + localPlanetRealRadius + " Levelized: " + veinGardenerState.localPlanet.levelized);

                localPlanetRadius -= 5;
                localPlanetRealRadius -= 5;

                if (veinGardenerState.localPlanet.factory.platformSystem.reformData == null)
                {
                    Debug.Log("InitReformData");
                    veinGardenerState.localPlanet.factory.platformSystem.InitReformData();
                }
                Debug.Log("ReformSnap: " + veinGardenerState.localPlanet.aux);
                /*
                playerAction_Build.reformPointsCount = localPlanet.aux.ReformSnap(worldPos, reformSize, reformType, reformColor,
                    reformPoints, playerAction_Build.reformIndices, localPlanet.factory.platformSystem, out reformCenterPoint);*/
                ReformSnap(worldPos, reformSize, reformType, reformColor, reformPoints, playerAction_Build.reformIndices, veinGardenerState.localPlanet.factory.platformSystem, out reformCenterPoint, localPlanetRadius);

                Debug.Log("reformPointsCount: " + playerAction_Build.reformPointsCount);

                Debug.Log("ComputeFlattenTerrainReform");
                var compFlatten = veinGardenerState.localPlanet.factory.ComputeFlattenTerrainReformALT(playerAction_Build.reformPoints, worldPos, radius, playerAction_Build.reformPointsCount, localPlanetRealRadius, fade0: 3f, fade1: 1f);

                Debug.Log("FlattenTerrainReform");
                veinGardenerState.localPlanet.factory.FlattenTerrainReformALT(reformCenterPoint, radius, reformSize, veinBuried, localPlanetRadius, localPlanetRealRadius);
                UIRealtimeTip.Popup("Flatten: " + compFlatten);
            }

        }
    }

    
}
