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

    public class UIVeinGroupDialog
    {
        private static Rect winRect = new Rect(0, 0, 450, 450);
		public Boolean Show { get; set; } = false;

        public static int UILayoutHeight { get; set; } = 1080;

        public static int ScaledScreenWidth { get; set; } = 1920;
        public static int ScaledScreenHeight { get; set; } = 1080;

        public static float ScaleRatio { get; set; } = 1.0f;

        const float FixedSizeAdjustOriginal = 0.5f;
        public static float FixedSizeAdjust { get; set; } = FixedSizeAdjustOriginal;

        // private static GUISkin customSkin = null;

        public DropDownGUILayout<UIVeinGroupDialogListItem> dropdown = new DropDownGUILayout<UIVeinGroupDialogListItem>();
		public DropDownGUILayout<UIVeinGroupDialogListItem> dropdownItems = new DropDownGUILayout<UIVeinGroupDialogListItem>();

		public PlanetData localPlanet { get; set; }

		public int veinGroupIndex { get; set; }

		private static GUILayoutOption[] VeinIconLayoutOptions;
		private static GUIStyle windowStyle;
		private static GUIStyle buttonStyle;

		private static Texture2D DialogueBackgroundTexture;

		List<int> products = new List<int>() { 1001, 1002, 1003, 1004, 1005, 1006, 1030, 1031, 1011, 1012, 1013, 1014, 1015, 1016, 1101, 1104, 1105, 1106, 1108, 1109, 1103, 1107, 1110, 1119, 1111, 1112, 1113, 1201, 1102, 1202, 1203, 1204, 1205, 1206, 1127, 1301, 1303, 1305, 1302, 1304, 1402, 1401, 1404, 1501, 1000, 1007, 1114, 1116, 1120, 1121, 1122, 1208, 1801, 1802, 1803, 1115, 1123, 1124, 1117, 1118, 1126, 1209, 1210, 1403, 1405, 1406, 5001, 5002, 1125, 1502, 1503, 1131, 1141, 1142, 1143, 2001, 2002, 2003, 2011, 2012, 2013, 2020, 2101, 2102, 2106, 2303, 2304, 2305, 2201, 2202, 2212, 2203, 2204, 2211, 2301, 2302, 2307, 2308, 2306, 2309, 2314, 2313, 2205, 2206, 2207, 2311, 2208, 2312, 2209, 2310, 2210, 2103, 2104, 2105, 2901, 6001, 6002, 6003, 6004, 6005, 6006 };


		public bool DrawItem(UIVeinGroupDialogListItem t)
        {
			bool hit = GUILayout.Button(t.tex, GUI.skin.box, VeinIconLayoutOptions);
			hit |= GUILayout.Button(t.name, buttonStyle, GUILayout.MinWidth(140), GUILayout.MaxWidth(140), GUILayout.MinHeight(40));
			return hit;
		}

		public UIVeinGroupDialog()
        {
			dropdown.DrawItem = DrawItem;

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
					dropdown.list.Add(new UIVeinGroupDialogListItem() { tex= itemProto.iconSprite.texture, name= itemProto.name.Translate() });
				}
			}


			dropdownItems.DrawItem = DrawItem;
			
			foreach (int prodId in products)
			{
				ItemProto itemProto = prodId != 0 ? LDB.items.Select(prodId) : null;
				if (itemProto != null)
				{
					dropdownItems.list.Add(new UIVeinGroupDialogListItem() { tex = itemProto.iconSprite.texture, name = itemProto.name.Translate(), productId= prodId });
				}
			}

		}

        public static void AutoResize(int designScreenHeight, bool applyCustomScale = true)
        {
            if (applyCustomScale)
            {
                designScreenHeight = (int)Math.Round((float)designScreenHeight / FixedSizeAdjust);
            }

            ScaledScreenHeight = designScreenHeight;
            ScaleRatio = (float)Screen.height / designScreenHeight;

            // Vector2 resizeRatio = new Vector2((float)Screen.width / screenWidth, (float)Screen.height / screenHeight);
            ScaledScreenWidth = (int)Math.Round(Screen.width / ScaleRatio);
            UnityEngine.GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(ScaleRatio, ScaleRatio, 1.0f));
        }

        public void UpdatePos()
        {
			float realRadius = localPlanet.realRadius;
			Vector3 localPosition = GameCamera.main.transform.localPosition;
			Vector3 forward = GameCamera.main.transform.forward;

			PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[veinGroupIndex];
			Vector3 veinGroupWorldPos = veinGroup.pos.normalized * (realRadius + 2.5f);
			Vector3 cameraVeinGroupDistance = veinGroupWorldPos - localPosition;
			float magnitude = cameraVeinGroupDistance.magnitude;
			float num = Vector3.Dot(forward, cameraVeinGroupDistance);
			if (magnitude < 1f || num < 1f || magnitude > 150)
			{
				Show = false;
				return;
			}
            
			Vector3 screenPoint = GameCamera.main.WorldToScreenPoint(veinGroupWorldPos);
			// Debug.Log("Screenpoint: " + screenPoint);

			//bool flag = UIRoot.ScreenPointIntoRect(screenPoint, rt, out Vector2 rectPoint);
			//bool flag = DSPExtensions.ScreenPointIntoRectSimple(screenPoint, out Vector2 rectPoint);
			screenPoint.y = Screen.height - screenPoint.y;

			bool flag = true;
			if (Mathf.Abs(screenPoint.x) > Screen.width || Mathf.Abs(screenPoint.y) > Screen.height ||
                screenPoint.x < -100 || screenPoint.y < -100)
			{
				flag = false;
			}

			Show = flag;

			//  Debug.Log("rectPoint: " + rectPoint);
			winRect.x = Mathf.Round(screenPoint.x);
			winRect.y = Mathf.Round(screenPoint.y);
			winRect.x += 10;
			winRect.y += 10;

			if (winRect.xMax > Screen.width)
			{
				winRect.x = Screen.width - winRect.width;
			}

			if (winRect.yMax > Screen.height)
			{
				winRect.y = Screen.height - winRect.height-20;
			}

			winRect.x = Mathf.Max(0, winRect.x);
			winRect.y = Mathf.Max(0, winRect.y);

            winRect.x /= ScaleRatio;
            winRect.y /= ScaleRatio;

        }

		public void Init()
        {
			Texture2D myImage = VeinPlanter.bundleAssets.LoadAsset<Texture2D>("assets/window-dialog.png");

			DialogueBackgroundTexture = Resources.Load<Texture2D>("ui/textures/sprites/sci-fi/window-glass-1");
			if (DialogueBackgroundTexture == null)
			{
				Debug.LogWarning("Failed Loading menu_button_texture");
				DialogueBackgroundTexture = Texture2D.blackTexture;
			}

			// colors used to tint the first 3 mip levels
			// Color[] colors = new Color[3];
			// colors[0] = Color.red;
			// colors[1] = Color.green;
			// colors[2] = Color.blue;

			// Color tintColor = new Color(24/256.0f, 57/256.0f, 83/256.0f, 0.7f);

			//int mipCount = Mathf.Min(3, DialogueBackgroundTextureL2.mipmapCount);

			// tint each mip level
			// for (int mip = 0; mip < mipCount; ++mip)
			//{
				//Color[] cols = DialogueBackgroundTextureL2.GetPixels();
				//for (int i = 0; i < cols.Length; ++i)
				//{
				//	cols[i] = Color.Lerp(cols[i], tintColor, 0.33f);
				//}
				//DialogueBackgroundTextureL2.SetPixels(cols);
			//}
			// actually apply all SetPixels, don't recalculate mip levels
			//DialogueBackgroundTextureL2.Apply(true);

			VeinIconLayoutOptions = new GUILayoutOption[] { GUILayout.MaxWidth(40), GUILayout.MinHeight(40) };
			buttonStyle = new GUIStyle(GUI.skin.box);
			buttonStyle.wordWrap = true;

			windowStyle = new GUIStyle(UnityEngine.GUI.skin.window);
			windowStyle.contentOffset = new Vector2(0,0);
			windowStyle.border = new RectOffset(0, 0, 0, 0);
			windowStyle.padding = new RectOffset(30, 30, 70, 30);
			windowStyle.onActive.background = windowStyle.onFocused.background = windowStyle.onHover.background = windowStyle.onNormal.background = myImage;
			windowStyle.normal.background = windowStyle.active.background = windowStyle.focused.background = windowStyle.hover.background = myImage;

			isInit = true;
		}

		private static bool isInit = false;

		public void OnGUI()
		{
			var uiGame = BGMController.instance.uiGame;
			var shouldShowByGameState = DSPGame.GameDesc != null && uiGame != null && uiGame.gameData != null && uiGame.guideComplete && DSPGame.IsMenuDemo == false && DSPGame.Game.running && (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Sail) &&
				!(uiGame.techTree.active || uiGame.dysonEditor.active || uiGame.starmap.active || uiGame.escMenu.active || uiGame.hideAllUI0 || uiGame.hideAllUI1) && uiGame.gameMenu.active;

			//Show = shouldShowByGameState = DSPGame.MenuDemoLoaded;

			if (!shouldShowByGameState)
			{
				return;
			}

			if (!isInit && GameMain.isRunning) { Init(); }


            AutoResize(DSPGame.globalOption.uiLayoutHeight, applyCustomScale: false);

            //windowStyle.contentOffset = new Vector2(0, 0);

            if (Show)
			{
				winRect = GUILayout.Window(55416755, winRect, WindowFunc, "", windowStyle);
				UIHelper.EatInputInRect(winRect);
			}
		}

		bool largeMax = true;

		private void WindowFunc(int id)
        {
			if (localPlanet == null || localPlanet.factory == null)
            {
				//VeinPlanter.dialog = null;
				return;
            }
 			ref PlanetData.VeinGroup veinGroup = ref localPlanet.veinGroups[veinGroupIndex];
			int veinProduct = PlanetModelingManager.veinProducts[(int)veinGroup.type];
			ItemProto itemProto = veinProduct != 0 ? LDB.items.Select(veinProduct) : null;
			Texture2D texture = null;
			string veinName = null;
			if (itemProto != null)
			{
				texture = itemProto.iconSprite.texture;
				veinName = itemProto.name.Translate();
			}

			

			GUILayout.BeginHorizontal(GUILayout.Width(380));
			GUILayout.BeginVertical();
			dropdown.indexNumber = (int)veinGroup.type - 1;
			if (dropdown.OnGUI(225, GUILayout.MinHeight(42), GUILayout.MinWidth(150), GUILayout.MaxWidth(150)))
			{
				Gardener.VeinGroup.ChangeType(veinGroupIndex, (EVeinType)dropdown.indexNumber + 1, localPlanet);
			}
			var prodId = (localPlanet != null && localPlanet.factory != null) ? (from vein in localPlanet.factory.veinPool where vein.groupIndex == veinGroupIndex select vein.productId).First() : products[0];
			
			if (veinGroup.type == EVeinType.Oil)
            {
				dropdownItems.indexNumber = products.FindIndex(i => i == prodId);
				if (dropdownItems.OnGUI(225, GUILayout.MinHeight(42), GUILayout.MinWidth(150), GUILayout.MaxWidth(150)))
				{
					Gardener.VeinGroup.ChangeType(veinGroupIndex, (EVeinType)dropdown.indexNumber + 1, localPlanet, productId: dropdownItems.list[dropdownItems.indexNumber].productId);
				}

			}


			GUILayout.EndVertical();
			/*
			GUILayout.Box(texture, VeinIconLayoutOptions);
			GUILayout.Box("Type "+ veinGroup.type + "\n" + veinName);
			*/
			GUILayout.BeginVertical(GUILayout.Width(200));
			GUILayout.Label("Count " + veinGroup.count);
			

			float maxAmount;
			float oilMax = 120f;
			float currentValue = veinGroup.amount;
			float oreMultiplier = 0.000001f;

			if (veinGroup.type == EVeinType.Oil)
            {
				maxAmount = 2500000f;
				maxAmount  = oilMax;
				currentValue *= VeinData.oilSpeedMultiplier;
				GUILayout.Label("Amount " + currentValue.ToString("n2"));
			} else
            {
                
				//maxAmount = 1000000f * 100;
				maxAmount = 10;
				largeMax = GUILayout.Toggle(largeMax, "Large Max");
				if (largeMax)
				{
                    //maxAmount = 20000000000f;
                    //maxAmount = 21 000;
                    maxAmount = Math.Min(((long)int.MaxValue - 22845704) * oreMultiplier * veinGroup.count, 25000);

                }
				currentValue *= oreMultiplier;
				GUILayout.Label("Amount " + veinGroup.amount.ToString("n0"));
                //GUILayout.Label("MaxAmount " + maxAmount.ToString("n0"));
            }

			

			float newValue;
			if (veinGroup.type == EVeinType.Oil)
            {
				newValue = GUILayout.HorizontalSlider(currentValue, 0, maxAmount);
				if (newValue != currentValue)
				{
					// Debug.Log("Setting new veinGroup Amount to= CurAmount: " + veinGroup.amount + ",  newValue: " + newValue + " Conv: " + newValue / VeinData.oilSpeedMultiplier);
					Gardener.VeinGroup.UpdateVeinAmount(veinGroupIndex, (long)(newValue / VeinData.oilSpeedMultiplier), localPlanet);
				}
			} else
            {
				newValue = GUILayout.HorizontalSlider(currentValue, 0, maxAmount);
				if (newValue != currentValue)
				{
					Gardener.VeinGroup.UpdateVeinAmount(veinGroupIndex, (long)(newValue / oreMultiplier), localPlanet);
				}
			}
			


			/*
			if (GUILayout.Button("Magic "))
            {
				veinGroup.type = EVeinType.Oil;

				int veinTypeIndex = (int)veinGroup.type;

				for (int i = 1; i < localPlanet.factory.veinCursor; i++)
				{
					ref VeinData vein = ref localPlanet.factory.veinPool[i];
					ref AnimData veinAnim = ref localPlanet.factory.veinAnimPool[i];

					// Skip invalid veins and veins from other groups.
					if (i != vein.id
						|| vein.groupIndex != veinGroupIndex)
					{
						continue;
					}

					// Remove the old vein model instance from the GPU Instancing
					localPlanet.factoryModel.gpuiManager.RemoveModel(vein.modelIndex, vein.modelId, setBuffer: false);

					vein.productId = 6006; //PlanetModelingManager.veinProducts[veinTypeIndex];
					vein.type = veinGroup.type;
					vein.modelIndex = (short)random.Next(PlanetModelingManager.veinModelIndexs[veinTypeIndex],
						PlanetModelingManager.veinModelIndexs[veinTypeIndex] + PlanetModelingManager.veinModelCounts[veinTypeIndex]);
					vein.modelId = localPlanet.factoryModel.gpuiManager.AddModel(
						vein.modelIndex, i, vein.pos, Maths.SphericalRotation(vein.pos,
						UnityEngine.Random.value * 360f), setBuffer: false);

					veinAnim.time = ((vein.amount < 20000) ? (1f - (float)vein.amount * 5E-05f) : 0f);
					veinAnim.prepare_length = 0f;
					veinAnim.working_length = 1f;
					veinAnim.state = (uint)vein.type;
					veinAnim.power = 0f;

					GameMain.localPlanet.factory.RefreshVeinMiningDisplay(i, 0, 0);
				}
				GameMain.localPlanet.factoryModel.gpuiManager.SyncAllGPUBuffer();
			}
			*/

			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}
		public System.Random random = new System.Random(0);

    }
}
