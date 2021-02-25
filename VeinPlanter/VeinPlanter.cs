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

namespace VeinPlanter
{
    [BepInPlugin(VersionInfo.BEPINEX_FQDN_ID, "VeinPlanter Plug-In", VersionInfo.VERSION)]
    public class VeinPlanter : BaseUnityPlugin
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

        public System.Random random = new System.Random(0);



        public static PlanetData.VeinGroup NewVeinGroup(EVeinType vType, Vector3 worldPos)
        {
            PlanetData.VeinGroup veinGroup = new PlanetData.VeinGroup();
            veinGroup.count = 1;
            veinGroup.pos = worldPos;
            veinGroup.type = vType;
            veinGroup.amount = 0;

            return veinGroup;
        }


        public enum eVeinModificationMode { Deactivated, Add, Modify, Remove};

        eVeinModificationMode modMode = eVeinModificationMode.Modify;


        private void Vein_Add(Vector3 worldPos, int closestVeinGroupIndex)
        {
            var planetFactory = GameMain.data.factories[0];

            PlanetData.VeinGroup veinGroup;
            if (closestVeinGroupIndex >= 0)
            {
                veinGroup = GameMain.localPlanet.veinGroups[closestVeinGroupIndex];
            } else
            {
                veinGroup = NewVeinGroup(EVeinType.Iron, worldPos.normalized);
                closestVeinGroupIndex = planetFactory.planet.AddVeinGroupData(veinGroup);
            }

            short veinTypeIndex = (int)EVeinType.Iron;

            VeinProto veinProto = PlanetModelingManager.veinProtos[veinTypeIndex];

            var veinCursor = planetFactory.veinCursor + 1;
            VeinData vein = default(VeinData);
            vein.type = EVeinType.Iron;
            vein.groupIndex = (short)closestVeinGroupIndex;
            vein.modelIndex = (short)random.Next(PlanetModelingManager.veinModelIndexs[veinTypeIndex], PlanetModelingManager.veinModelIndexs[veinTypeIndex] + PlanetModelingManager.veinModelCounts[veinTypeIndex]);
            vein.amount = 100;
            vein.productId = PlanetModelingManager.veinProducts[veinTypeIndex];
            vein.pos = worldPos;
            vein.minerCount = 0;
            vein.colliderId = 0;

            vein.amount = Mathf.RoundToInt(vein.amount * DSPGame.GameDesc.resourceMultiplier);

            planetFactory.planet.veinAmounts[veinTypeIndex] += vein.amount;
            int newVeinIndex = planetFactory.AddVeinData(vein);
            planetFactory.veinPool[newVeinIndex].modelId = planetFactory.planet.factoryModel.gpuiManager.AddModel(vein.modelIndex, newVeinIndex, vein.pos, Maths.SphericalRotation(vein.pos, UnityEngine.Random.value * 360f), setBuffer: false);

            ColliderData[] colliders2 = veinProto.prefabDesc.colliders;
            int num2 = 0;
            while (colliders2 != null && num2 < colliders2.Length)
            {
                planetFactory.veinPool[newVeinIndex].colliderId = planetFactory.planet.physics.AddColliderData(colliders2[num2].BindToObject(newVeinIndex, vein.colliderId, EObjectType.Vein, vein.pos, Quaternion.FromToRotation(Vector3.up, vein.pos.normalized)));
                num2++;
            }
            planetFactory.RefreshVeinMiningDisplay(newVeinIndex, 0, 0);

            planetFactory.planet.factoryModel.gpuiManager.SyncAllGPUBuffer();
        }


        private int GetClosestVeinGroupIndex(Ray ray, PlanetData localPlanet)
        {
            float realRadius = localPlanet.realRadius;

            int closestVeinGroupIndex = -1;
            Vector3 vector = Vector3.zero;
            if (Phys.RayCastSphere(ray.origin, ray.direction, 600f, Vector3.zero, realRadius + 1f, out var rch))
            {
                float clostestVeinGroupDistance = 100f;
                Dictionary<int, float> distMap = new Dictionary<int, float>();

                // First pass check for vein Group. Uses veinGroups (cheaper)
                for (int i = 0; i < localPlanet.veinGroups.Length; i++)
                {
                    PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[i];
                    float currentveinGroupDistance = Vector3.Distance(rch.point, veinGroup.pos * realRadius);
                    //Debug.Log("Comp: veinGroup: " + veinGroup.ToString() + " index: " + i + " Pos: " + veinGroup.pos.ToString() + " dist: " + currentveinGroupDistance);
                    distMap[i] = currentveinGroupDistance;
                    if (currentveinGroupDistance < clostestVeinGroupDistance)
                    {
                        clostestVeinGroupDistance = currentveinGroupDistance;
                        closestVeinGroupIndex = i;
                        vector = veinGroup.pos * (realRadius + 2.5f);
                    }
                }

                // Second Pass. Looks up distance to specific vein nodes.
                var candidates = distMap.OrderBy(key => key.Value);

                foreach (var candkv in candidates)
                {
                    // Debug.Log("VeinGroup Idx=" + candkv.Key + "\t Dist: " + candkv.Value);
                }

                var limitedCandidates = candidates.Take(Math.Min(candidates.Count(), 3));

                foreach (var candkv in limitedCandidates)
                {
                    Debug.Log("Cand: VeinGroup Idx=" + candkv.Key + "\t Dist: " + candkv.Value);
                }
                
                // Cheat for now
                Assert.Equals(closestVeinGroupIndex, limitedCandidates.First().Key);
                closestVeinGroupIndex = candidates.First().Key;
            }
            if (closestVeinGroupIndex >= 0 && !Phys.RayCastSphere(ray.origin, ray.direction, 600f, vector, 3.5f, out rch))
            {
                closestVeinGroupIndex = -1;
            }

            return closestVeinGroupIndex;
        }

        public void UpdateVeinGroupPos(int veinGroupIndex)
        {
            var veinGroup = GameMain.localPlanet.veinGroups[veinGroupIndex];
            var veinPositions = (from vein in GameMain.localPlanet.factory.veinPool
                                 where vein.groupIndex == veinGroupIndex
                                 select vein.pos).ToList();

            var averagePosition = veinPositions.Aggregate(Vector3.zero, (acc, v) => acc + v) / veinPositions.Count;
            Debug.Log("VeinGroup Pos for index="+veinGroupIndex + " was: " + veinGroup.pos.ToString() + " is now: " + averagePosition.normalized);
            veinGroup.pos = averagePosition.normalized;
        }

        // private UIVeinDetailNode nodePrefab;
        UIVeinGroupDialog dialog = null;

        private void Update()
		{
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

                    int closestVeinGroupIndex = GetClosestVeinGroupIndex(ray, localPlanet);
                    if (closestVeinGroupIndex >= 0)
                    {
                        PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[closestVeinGroupIndex];
                        UpdateVeinGroupPos(closestVeinGroupIndex);
                        Debug.Log("Clicked on veinGroup: " + veinGroup.ToString() + " index: " + closestVeinGroupIndex + " Type: " + veinGroup.type);
                        Debug.Log("VeinGroup: " + veinGroup.pos.ToString() + " index: " + (veinGroup.pos*(localPlanet.realRadius + 2.5f)));

                        /*
                        UIVeinDetailNode uIVeinDetailNode = UnityEngine.Object.Instantiate(nodePrefab, nodePrefab.transform.parent);
                        uIVeinDetailNode._Create();
                        uIVeinDetailNode.inspectPlanet = localPlanet;
                        uIVeinDetailNode.veinGroupIndex = closestVeinGroupIndex;
                        uIVeinDetailNode._Init(BGMController.instance.uiGame.gameData); // BGMController.instance.uiGame.veinDetail.data
                        */
                        dialog = new UIVeinGroupDialog()
                        {
                            localPlanet = localPlanet,
                            veinGroupIndex = closestVeinGroupIndex,
                            Show = true
                        };
                    } else
                    {
                        dialog = null;
                    }

                    switch (modMode)
                    {
                        
                        case eVeinModificationMode.Add: Vein_Add(worldPos, closestVeinGroupIndex); break;
                        case eVeinModificationMode.Modify: break;
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
        }

		void OnGUI()
        {
            if (dialog != null)
            {

                dialog.OnGUI();
            }
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
