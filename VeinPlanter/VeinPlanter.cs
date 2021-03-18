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
    [BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "VeinPlanter Plug-In", VersionInfo.VERSION)]
    public class VeinPlanter : BaseUnityPlugin
    {
        Harmony harmony;
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

        public static AssetBundle bundle;

        public static GameObject TestGUI = null;

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            InitConfig();
            UnityEngine.Debug.Log(Assembly.GetExecutingAssembly().GetName().ToString());
            

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VeinPlanter.Resources.veinplanter"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
            }
            /*
            TestGUI = GameObject.Instantiate(bundle.LoadAsset<GameObject>("Assets/UI/UITest.prefab"));
            GameObject.DontDestroyOnLoad(TestGUI);
            TestGUI.transform.SetParent(UIRoot.instance.transform);
            TestGUI.SetActive(true);
            */


            UnityEngine.Debug.Log("VeinPlanter Plugin Loaded!");
            harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();            
        }

        internal void OnDestroy()
        {
            if (TestGUI != null)
            {
                TestGUI.SetActive(false);
                GameObject.Destroy(TestGUI);
            }

            // For ScriptEngine hot-reloading
            bundle.Unload(true);
            harmony.UnpatchSelf();
        }

        public bool shift;
		public bool ctrl;
		public bool alt;

        public enum eVeinModificationMode { Deactivated, PlanetVeins, AddVein, MoveVein, RemoveVein, AddVeinGroup, ModifyVeinGroup, MoveVeinGroup, RemoveVeinGroup };

        eVeinModificationMode modMode = eVeinModificationMode.Deactivated;




        // private UIVeinDetailNode nodePrefab;
        static public UIVeinGroupDialog dialog = null;
        static public UIPlanetVeins planetVeinsDialog = null;

        bool shouldEatInput = false;


        private void HandleClick()
        {
            Vector3 worldPos;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, Input.mousePosition, uicam, out var localPoint);
            Ray ray = GameCamera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("Clicking screen pos: " + Input.mousePosition.ToString() + " Ray: " + ray.ToString());

                PlanetData localPlanet = GameMain.localPlanet;
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

                                dialog = new UIVeinGroupDialog()
                                {
                                    localPlanet = localPlanet,
                                    veinGroupIndex = closestVeinGroupIndex,
                                    Show = true
                                };
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

                        case eVeinModificationMode.Deactivated:
                        default:
                            break;
                    }
                }
            }
        }
        private void Update()
		{
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
            
            if (modMode == eVeinModificationMode.PlanetVeins && planetVeinsDialog == null)
            {
                planetVeinsDialog = new UIPlanetVeins()
                {
                    localPlanet = GameMain.localPlanet,
                    Show = true
                };
            }


            if (!ShowModeMenu)
            {
                winRect.x = Math.Max(Input.mousePosition.x-winRect.width/2, 0);
                winRect.y = Math.Max((Screen.height-Input.mousePosition.y) - winRect.height/2,0);

                winRect.x = Math.Min(Screen.width - winRect.width, winRect.x);
                winRect.y = Math.Min(Screen.height - winRect.height, winRect.y);

                HandleClick();
            }
            
        }
        private static Rect winRect = new Rect(Screen.width-100, Screen.height/2-200, 505, 200);

        void OnGUI()
        {
            if (dialog != null)
            {

                dialog.OnGUI();
            }
            if (planetVeinsDialog != null && planetVeinsDialog.Show)
            {

                planetVeinsDialog.OnGUI();
            }
            

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
            GUIStyle menuButton = new GUIStyle(GUI.skin.button);
            menuButton.wordWrap = true;
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
    }
}
