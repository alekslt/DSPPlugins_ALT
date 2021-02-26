using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using VeinPlanter.Service;

namespace VeinPlanter
{
    [BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "VeinPlanter Plug-In", VersionInfo.VERSION)]
    public class VeinPlanter : BaseUnityPlugin
    {
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

        public static AssetBundle bundle;

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            InitConfig();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VeinPlanter.Resources.veinplanter"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
            }
             
            UnityEngine.Debug.Log("VeinPlanter Plugin Loaded!");
            var harmony = new Harmony(VersionInfo.BEPINEX_FQDN_ID);
            harmony.PatchAll();
        }

        public bool shift;
		public bool ctrl;
		public bool alt;

        public enum eVeinModificationMode { Deactivated, Add, Modify, Move, Remove};

        eVeinModificationMode modMode = eVeinModificationMode.Add;




        // private UIVeinDetailNode nodePrefab;
        UIVeinGroupDialog dialog = null;

        private void UpdateKeys()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                alt = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
            {
                alt = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                shift = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            {
                shift = false;
            }
        }

        bool shouldEatInput = false;

        private void Update()
		{
            UpdateKeys();
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

            Vector3 worldPos;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, Input.mousePosition, uicam, out var localPoint);
            Ray ray = GameCamera.main.ScreenPointToRay(Input.mousePosition);
			if (Input.GetMouseButtonDown(0))
			{
                Debug.Log("Clicking screen pos: " + Input.mousePosition.ToString() + " Ray: " + ray.ToString());

                PlanetData localPlanet = GameMain.localPlanet;
                if (localPlanet == null)
                {
                    return;
                }
                
                if (Physics.Raycast(ray, out var hitInfo, 1000f, 15873, QueryTriggerInteraction.Ignore))
				{
                    worldPos = hitInfo.point;
                    Debug.Log("Clicked on world pos: " + worldPos.ToString());

                    int closestVeinGroupIndex = Gardener.VeinGroup.GetClosestIndex(ray, localPlanet);

                    switch (modMode)
                    {
                        case eVeinModificationMode.Add:
                            if (closestVeinGroupIndex < 0)
                            {
                                var veinGroup = Gardener.VeinGroup.New(EVeinType.Iron, worldPos.normalized);
                                closestVeinGroupIndex = localPlanet.AddVeinGroupData(veinGroup);
                                Debug.Log("Adding new veinGroup: " + veinGroup.type.ToString() + " index: " + closestVeinGroupIndex + " Pos: " + veinGroup.pos * localPlanet.radius);
                            }
                            Gardener.Vein.Add(localPlanet, worldPos, closestVeinGroupIndex);
                            break;
                        case eVeinModificationMode.Modify:
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
                        case eVeinModificationMode.Remove: break;

                        case eVeinModificationMode.Deactivated:
                        default:
                            break;
                    }
                }
            }
            if (dialog != null)
            {
                dialog.UpdatePos();
            }
            if (shouldEatInput)
            {
                Input.ResetInputAxes();
            }

            if (!(shift && alt))
            {
                winRect.x = Math.Max(Input.mousePosition.x-winRect.width/2, 0);
                winRect.y = Math.Max((Screen.height-Input.mousePosition.y) - winRect.height/2,0);

                winRect.x = Math.Min(Screen.width - winRect.width, winRect.x);
                winRect.y = Math.Min(Screen.height - winRect.height, winRect.y);
            }
            
        }
        private static Rect winRect = new Rect(Screen.width-100, Screen.height/2-200, 105, 200);

        void OnGUI()
        {
            if (dialog != null)
            {

                dialog.OnGUI();
            }

            if (shift && alt)
            {
                winRect = GUILayout.Window(55416758, winRect, MiniMenu, "");
                UIHelper.EatInputInRect(winRect);
            }
            if (shouldEatInput)
            {
                Input.ResetInputAxes();
            }
        }

        private void MiniMenu(int id)
        {
            GUILayout.BeginVertical();
            foreach (eVeinModificationMode mode in Enum.GetValues(typeof(eVeinModificationMode)))
            {
                GUIStyle menuButton = new GUIStyle(GUI.skin.button);
                menuButton.wordWrap = true;
                GUIStyle selectedMenuButton = new GUIStyle(menuButton);
                selectedMenuButton.normal.background = Texture2D.whiteTexture;

                if (GUILayout.Button(mode.ToString(), (mode == modMode) ? selectedMenuButton : menuButton)){
                    modMode = mode;
                }
            }
            
            GUILayout.EndVertical();
        }
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP
        /*
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
        */

        #endregion // Harmony Patch Hooks in DSP
    }
}
