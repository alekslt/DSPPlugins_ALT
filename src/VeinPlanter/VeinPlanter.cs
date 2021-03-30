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
using VeinPlanter.Service;

namespace VeinPlanter
{
    //[BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "VeinPlanter Plug-In", VersionInfo.VERSION)]
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public class VeinPlanter : BaseUnityPlugin
    {
        Harmony harmony = null;
        public static ConfigEntry<int> UILayoutHeightConfig;
        //public static ConfigEntry<int> VeinAmountThreshold;
        //public static ConfigEntry<bool> ShowMenuButton;
        //public static ConfigEntry<KeyCode> ShowNotificationWindowHotKey;

        public static bool ShowModeMenu = false;
        public static bool ShowPlanetVeinMenu = false;

        void InitConfig()
        {
            UILayoutHeightConfig = Config.Bind("General", "UILayoutHeight", 1080, "What UILayoutHeight should we enforce [900, 1080, 1440, 2160]");
            //VeinAmountThreshold = Config.Bind("General", "VeinAmountThreshold", 6000, "Threshold of vein amount left to mine for adding the miner to the list");
            //ShowMenuButton = Config.Bind("General.Toggles", "ShowMenuButton", true, "Whether or not to show the menu button lower right");
            //ShowNotificationWindowHotKey = Config.Bind<KeyCode>("config", "ShowInformationWindowHotKey", KeyCode.I, "Key to press for toggling the Miner Information Window");
        }

        public static AssetBundle bundleAssets = null;
        public static AssetBundle bundleScenes = null;

        public static GameObject GUIGOModeSelect = null;
        public static GameObject GUIGOVeinGroupModify = null;

        public static UIVeinGardenerMode UIVGMode = null;
        public static UIVeinGardenerGroupEdit UIVGGroupEdit = null;

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
            }
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VeinPlanter.Resources.veinplanterscenes"))
            {
                bundleScenes = AssetBundle.LoadFromStream(stream);
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

            var inGameWindows = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows");
            Debug.Log(inGameWindows);

            GUIGOModeSelect = Instantiate(bundleAssets.LoadAsset<GameObject>("Assets/UI/VeinGardener/UIVeinPlanterMode.prefab"));
            AttachGameObject(GUIGOModeSelect, inGameWindows.transform);

            GUIGOVeinGroupModify = Instantiate(bundleAssets.LoadAsset<GameObject>("Assets/UI/VeinGardener/UIVeinGardenerGroupEdit.prefab"));
            AttachGameObject(GUIGOVeinGroupModify, inGameWindows.transform);

            UIVGMode = GUIGOModeSelect.GetComponent<UIVeinGardenerMode>();
            if (!UIVGMode.IsInit) UIVGMode.Init();
            UIVGGroupEdit = GUIGOVeinGroupModify.GetComponent<UIVeinGardenerGroupEdit>();
            if (!UIVGGroupEdit.IsInit) UIVGGroupEdit.Init();

            InitDialogs();
            UIVGGroupEdit.Hide();
            

            GUIGOVeinGroupModify.SetActive(true);
            UnityEngine.Debug.Log("VeinPlanter Plugin Loaded!");
            harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();
        }
        
        void AttachGameObject(GameObject go, Transform parent)
        {
            DontDestroyOnLoad(go);
            go.transform.SetParent(parent);
            go.transform.localPosition = new Vector3(0, 0, 0);
            go.transform.localScale = new Vector3(1, 1, 1);
            go.SetActive(true);
        }

        //T GetNamedComponentInChildren<T>(GameObject go, string name) where T : MonoBehaviour { return go.GetComponentsInChildren<T>().Where(k => k.gameObject.name == name).FirstOrDefault(); }
        float oreMultiplier = 100000000f;
        void InitDialogs()
        {
            Debug.Log("Registering callbacks for VG.Mode");
            UIVGMode.UIOnCreateNewVeinGroup = () => ChangeMode(eVeinModificationMode.AddVeinGroup);
            UIVGMode.UIOnDeleteVein = () => ChangeMode(eVeinModificationMode.RemoveVein);
            UIVGMode.UIOnDeleteVeinGroup = () => ChangeMode(eVeinModificationMode.RemoveVeinGroup);
            UIVGMode.UIOnExtendVeinGroup = () => ChangeMode(eVeinModificationMode.AddVein);
            UIVGMode.UIOnModifyVeinGroup = () => ChangeMode(eVeinModificationMode.ModifyVeinGroup);
            UIVGMode.UIOnWorldEditShowAll = () => ChangeMode(eVeinModificationMode.PlanetVeins);

            Debug.Log("Registering callbacks for VG.GroupEdit");
            UIVGGroupEdit.UIOnVeinAmountSliderValueChanged = newValue => {
                Debug.Log("VG.GroupEdit : VeinAmountSliderValueChanged : " + newValue);
                Gardener.VeinGroup.UpdateVeinAmount(veinGroupIndex, (long)(newValue / oreMultiplier), localPlanet);
            };

            //UnityEngine.UI.Dropdown VeinTypeDropdown = GetNamedComponentInChildren<UnityEngine.UI.Dropdown>(GUIGOVeinGroupModify, "VeinTypeDropdown");
            //UnityEngine.UI.Dropdown ProductTypeDropdown = GetNamedComponentInChildren<UnityEngine.UI.Dropdown>(GUIGOVeinGroupModify, "ProductTypeDropdown");
            //UnityEngine.UI.Dropdown ModeVeinTypeDropdown = GetNamedComponentInChildren<UnityEngine.UI.Dropdown>(GUIGOModeSelect, "VeinTypeDropdown");


            Debug.Log("Populating UIVGGroupEdit.VeinTypeDropdown");
            UIVGGroupEdit.VeinTypeDropdown.ClearOptions();

            Debug.Log("Populating UIVGGroupEdit.VeinTypeDropdown");
            UIVGMode.VeinTypeDropdown.ClearOptions();
            
            foreach (EVeinType tabType in Enum.GetValues(typeof(EVeinType)))
            {
                if (tabType == EVeinType.Max || tabType == EVeinType.None)
                {
                    continue;
                }
                int veinProduct = PlanetModelingManager.veinProducts[(int)tabType];
                ItemProto itemProto = veinProduct != 0 ? LDB.items.Select(veinProduct) : null;
                if (itemProto != null)
                {
                    UIVGGroupEdit.VeinTypeDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData () { image = itemProto.iconSprite, text = itemProto.name.Translate() });
                    UIVGMode.VeinTypeDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { image = itemProto.iconSprite, text = itemProto.name.Translate() });
                }
            }

            Debug.Log("Populating Group Edit Products");
            UIVGGroupEdit.ProductTypeDropdown.ClearOptions();

            foreach (int prodId in products)
            {
                ItemProto itemProto = prodId != 0 ? LDB.items.Select(prodId) : null;
                if (itemProto != null)
                {
                    UIVGGroupEdit.ProductTypeDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { image = itemProto.iconSprite, text = itemProto.name.Translate() });
                }
            }
        }

        List<int> products = new List<int>() { 1001, 1002, 1003, 1004, 1005, 1006, 1030, 1031, 1011, 1012, 1013, 1014, 1015, 1016, 1101, 1104, 1105, 1106, 1108, 1109, 1103, 1107, 1110, 1119, 1111, 1112, 1113, 1201, 1102, 1202, 1203, 1204, 1205, 1206, 1127, 1301, 1303, 1305, 1302, 1304, 1402, 1401, 1404, 1501, 1000, 1007, 1114, 1116, 1120, 1121, 1122, 1208, 1801, 1802, 1803, 1115, 1123, 1124, 1117, 1118, 1126, 1209, 1210, 1403, 1405, 1406, 5001, 5002, 1125, 1502, 1503, 1131, 1141, 1142, 1143, 2001, 2002, 2003, 2011, 2012, 2013, 2020, 2101, 2102, 2106, 2303, 2304, 2305, 2201, 2202, 2212, 2203, 2204, 2211, 2301, 2302, 2307, 2308, 2306, 2309, 2314, 2313, 2205, 2206, 2207, 2311, 2208, 2312, 2209, 2310, 2210, 2103, 2104, 2105, 2901, 6001, 6002, 6003, 6004, 6005, 6006 };


        internal void OnDestroy()
        {
            if (GUIGOModeSelect != null)
            {
                GUIGOModeSelect.SetActive(false);
                GameObject.Destroy(GUIGOModeSelect);
            }

            if (GUIGOVeinGroupModify != null)
            {
                //TestGUI.SetActive(false);
                GameObject.Destroy(GUIGOVeinGroupModify);
            }

            // For ScriptEngine hot-reloading
            if (bundleScenes != null) bundleScenes.Unload(true);
            if (bundleAssets != null) bundleAssets.Unload(true);
            if (harmony != null) harmony.UnpatchSelf();
        }

        public enum eVeinModificationMode { Deactivated, PlanetVeins, AddVein, MoveVein, RemoveVein, AddVeinGroup, ModifyVeinGroup, MoveVeinGroup, RemoveVeinGroup, TerrainLower };

        eVeinModificationMode modMode = eVeinModificationMode.Deactivated;




        // private UIVeinDetailNode nodePrefab;
        static public UIVeinGroupDialog dialog = null;
        //static public UIPlanetVeins planetVeinsDialog = null;

        bool shouldEatInput = false;

        PlanetData localPlanet;
        int veinGroupIndex;

        private void HandleClick()
        {
            Vector3 worldPos;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, Input.mousePosition, uicam, out var localPoint);
            Ray ray = GameCamera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("Clicking screen pos: " + Input.mousePosition.ToString() + " Ray: " + ray.ToString());

                localPlanet = GameMain.localPlanet;
                if (localPlanet == null)
                {
                    return;
                }

                if (Physics.Raycast(ray, out var hitInfo, 1000f, 15873, QueryTriggerInteraction.Ignore))
                {
                    worldPos = hitInfo.point;
                    // Debug.Log("Clicked on world pos: " + worldPos.ToString());

                    Gardener.VeinGroup.GetClosestIndex(ray, localPlanet, out int closestVeinGroupIndex, out int closestVeinIndex, out float closestVeinDistance, out float closestVeinDistance2D);

                    switch (modMode)
                    {
                        case eVeinModificationMode.AddVein:
                            if (closestVeinGroupIndex < 0)
                            {
                                var veinGroup = Gardener.VeinGroup.New(EVeinType.Iron, worldPos.normalized);
                                closestVeinGroupIndex = localPlanet.AddVeinGroupData(veinGroup);
                                Debug.Log("Adding new veinGroup: " + veinGroup.type.ToString() + " index: " + closestVeinGroupIndex + " Pos: " + veinGroup.pos * localPlanet.radius);
                            }
                            Gardener.Vein.Add(localPlanet, worldPos, closestVeinGroupIndex);
                            break;
                        case eVeinModificationMode.ModifyVeinGroup:
                            if (closestVeinGroupIndex >= 0)
                            {
                                PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[closestVeinGroupIndex];
                                Debug.Log("Clicked on veinGroup: " + veinGroup.ToString() + " index: " + closestVeinGroupIndex + " Type: " + veinGroup.type);
                                Debug.Log("VeinGroup: " + veinGroup.pos.ToString() + " index: " + (veinGroup.pos * (localPlanet.realRadius + 2.5f)));

                                dialog = new UIVeinGroupDialog() {
                                    localPlanet = localPlanet,
                                    veinGroupIndex = closestVeinGroupIndex,
                                    Show = true
                                };
                                veinGroupIndex = closestVeinGroupIndex;
                                var prodId = (localPlanet != null && localPlanet.factory != null) ? (from vein in localPlanet.factory.veinPool where vein.groupIndex == veinGroupIndex select vein.productId).First() : products[0];

                                var prodIndex = products.FindIndex(p => p == prodId);
                                UIVGGroupEdit.UpdateData((int)veinGroup.type-1, prodIndex, veinGroup.count, veinGroup.amount / 100000000f);
                                UIVGGroupEdit.Show();
                            }
                            else
                            {
                                dialog = null;
                            }
                            break;
                        case eVeinModificationMode.RemoveVein:
                            if (closestVeinGroupIndex >= 0 && closestVeinIndex >= 0 && closestVeinDistance2D < 1)
                            {
                                Debug.Log("Removing vein: " + closestVeinIndex + " in group: " + closestVeinGroupIndex);
                                Gardener.Vein.Remove(localPlanet, closestVeinIndex, closestVeinGroupIndex);
                                Gardener.VeinGroup.UpdatePosFromChildren(closestVeinGroupIndex);
                            }
                            break;


                        case eVeinModificationMode.TerrainLower:
                            TerrainLower(worldPos);
                            break;

                        case eVeinModificationMode.Deactivated:
                        default:
                            break;
                    }
                }
            }
        }

        public Vector3[] reformPoints = new Vector3[100];

        
        private void Update()
        {
            
            //PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[veinGroupIndex];
            //UIVGGroupEdit.UpdateData((int)veinGroup.type, 1001, veinGroup.count, veinGroup.amount / 10000000000f);
            //UIVGGroupEdit.UpdateData((int)veinGroup.type - 1, UIVGGroupEdit.ProductTypeDropdown.value, veinGroup.count, veinGroup.amount / 100000000f);
            shouldEatInput = false;
            /*
            if (shift && alt && Math.Abs(Input.mouseScrollDelta.y) > 0)
            {
                int current = (int)modMode;

                if (Input.mouseScrollDelta.y > 0.1)
                {
                    current--;
                }
                if (Input.mouseScrollDelta.y < 0.1)
                {
                    current++;
                }
                if (current < 0)
                {
                    current = 3;
                }
                else if (current > 3)
                {
                    current = 0;
                }

                if (current != (int)modMode)
                {
                    modMode = (eVeinModificationMode)current;
                    shouldEatInput = true;
                    // UIRealtimeTip.Popup("Vein Gardener Mode Changed to: " + modMode + " Input.mouseScrollDelta.y " + Input.mouseScrollDelta.y);
                }
            }
            */

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
                ShowModeMenu = !ShowModeMenu;
                Input.ResetInputAxes();
            }

            /*
            if (modMode == eVeinModificationMode.PlanetVeins && planetVeinsDialog == null)
            {
                planetVeinsDialog = new UIPlanetVeins()
                {
                    localPlanet = GameMain.localPlanet,
                    Show = true
                };
            }
            */

            if (!ShowModeMenu)
            {
                winRect.x = Math.Max(Input.mousePosition.x - winRect.width / 2, 0);
                winRect.y = Math.Max((Screen.height - Input.mousePosition.y) - winRect.height / 2, 0);

                winRect.x = Math.Min(Screen.width - winRect.width, winRect.x);
                winRect.y = Math.Min(Screen.height - winRect.height, winRect.y);

                HandleClick();
            }

        }
        private static Rect winRect = new Rect(Screen.width - 100, Screen.height / 2 - 200, 505, 200);

        public void OnVeinAmountSliderChanged(float newValue) { 

        }

        void OnGUI()
        {
            if (dialog != null)
            {

                dialog.OnGUI();
            }
            /*
            if (planetVeinsDialog != null && planetVeinsDialog.Show)
            {

                planetVeinsDialog.OnGUI();
            }
            */


            if (ShowModeMenu)
            {
                winRect = GUILayout.Window(55416758, winRect, MiniMenu, "");
                UIHelper.EatInputInRect(winRect);
            }
            if (shouldEatInput)
            {
                //Input.ResetInputAxes();
            }
        }

        private void ChangeMode(eVeinModificationMode newmode)
        {
            modMode = newmode;
            ShowModeMenu = false;
            dialog = null;
            UIRealtimeTip.Popup("Vein Gardener Mode Changed to: " + modMode);
        }

        private void DrawButton(eVeinModificationMode mode)
        {
            GUIStyle menuButton = new GUIStyle(GUI.skin.button) {
                wordWrap = true
            };
            GUIStyle selectedMenuButton = new GUIStyle(menuButton);
            selectedMenuButton.normal.background = Texture2D.whiteTexture;
            selectedMenuButton.normal.textColor = Color.black;

            if (GUILayout.Button(mode.ToString(), (mode == modMode) ? selectedMenuButton : menuButton))
            {
                ChangeMode(mode);
            }
        }

        private void MiniMenu(int id)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            DrawButton(eVeinModificationMode.Deactivated);
            //DrawButton(eVeinModificationMode.PlanetVeins);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            //DrawButton(eVeinModificationMode.AddVeinGroup);
            DrawButton(eVeinModificationMode.ModifyVeinGroup);
            //DrawButton(eVeinModificationMode.MoveVeinGroup);
            //DrawButton(eVeinModificationMode.RemoveVeinGroup);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            DrawButton(eVeinModificationMode.AddVein);
            // DrawButton(eVeinModificationMode.ModifyVein);
            //DrawButton(eVeinModificationMode.MoveVein);
            DrawButton(eVeinModificationMode.RemoveVein);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

        public class ResetConfigHook
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(GameMain), "Begin")]
            public static void HookGameStart()
            {
                dialog = null;
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
            int num = localPlanet.aux.mainGrid.ReformSnapTo(pos, reformSize, reformType, reformColor, reformPoints, reformIndices, platform, out reformCenter);
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

                float localPlanetRadius = localPlanet.radius;
                float localPlanetRealRadius = localPlanet.realRadius;

                Debug.Log("TerrainLower Radius: " + localPlanetRadius + " RealRadius: " + localPlanetRealRadius + " Levelized: " + localPlanet.levelized);

                localPlanetRadius -= 5;
                localPlanetRealRadius -= 5;

                if (localPlanet.factory.platformSystem.reformData == null)
                {
                    Debug.Log("InitReformData");
                    localPlanet.factory.platformSystem.InitReformData();
                }
                Debug.Log("ReformSnap: " + localPlanet.aux);
                /*
                playerAction_Build.reformPointsCount = localPlanet.aux.ReformSnap(worldPos, reformSize, reformType, reformColor,
                    reformPoints, playerAction_Build.reformIndices, localPlanet.factory.platformSystem, out reformCenterPoint);*/
                ReformSnap(worldPos, reformSize, reformType, reformColor, reformPoints, playerAction_Build.reformIndices, localPlanet.factory.platformSystem, out reformCenterPoint, localPlanetRadius);

                Debug.Log("reformPointsCount: " + playerAction_Build.reformPointsCount);

                Debug.Log("ComputeFlattenTerrainReform");
                var compFlatten = localPlanet.factory.ComputeFlattenTerrainReformALT(playerAction_Build.reformPoints, worldPos, radius, playerAction_Build.reformPointsCount, localPlanetRealRadius, fade0: 3f, fade1: 1f);

                Debug.Log("FlattenTerrainReform");
                localPlanet.factory.FlattenTerrainReformALT(reformCenterPoint, radius, reformSize, veinBuried, localPlanetRadius, localPlanetRealRadius);
                UIRealtimeTip.Popup("Flatten: " + compFlatten);
            }

        }
    }

    
}
