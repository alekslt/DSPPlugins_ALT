using HarmonyLib;
using System;
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
        private static Rect winRect = new Rect(0, 0, 450, 200);
		public Boolean Show { get; set; } = false;

		// private static GUISkin customSkin = null;

		public DropDownGUILayout<UIVeinGroupDialogListItem> dropdown = new DropDownGUILayout<UIVeinGroupDialogListItem>();

		public PlanetData localPlanet { get; set; }

		public int veinGroupIndex { get; set; }

		private static GUILayoutOption[] VeinIconLayoutOptions;
		private static GUIStyle windowStyle;
		private static GUIStyle buttonStyle;

		private static Texture2D DialogueBackgroundTexture;

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
			if (magnitude < 1f || num < 1f)
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
			if (Mathf.Abs(screenPoint.x) > 8000f || Mathf.Abs(screenPoint.y) > 8000f)
			{
				flag = false;
			}

			Show = flag;

			//  Debug.Log("rectPoint: " + rectPoint);
			winRect.x = Mathf.Round(screenPoint.x);
			winRect.y = Mathf.Round(screenPoint.y);


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
		}

		public void Init()
        {
			Texture2D myImage = VeinPlanter.bundle.LoadAsset<Texture2D>("assets/window-dialog.png");

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
			windowStyle.padding = new RectOffset(30, 30, 30, 30);
			windowStyle.onActive.background = windowStyle.onFocused.background = windowStyle.onHover.background = windowStyle.onNormal.background = myImage;
			windowStyle.normal.background = windowStyle.active.background = windowStyle.focused.background = windowStyle.hover.background = myImage;

			isInit = true;
		}

		private static bool isInit = false;

		public void OnGUI()
		{
			var uiGame = BGMController.instance.uiGame;
			var shouldShowByGameState = DSPGame.GameDesc != null && uiGame != null && uiGame.gameData != null && uiGame.guideComplete && DSPGame.IsMenuDemo == false && DSPGame.Game.running && (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Sail) &&
				!(uiGame.techTree.active || uiGame.dysonmap.active || uiGame.starmap.active || uiGame.escMenu.active || uiGame.hideAllUI0 || uiGame.hideAllUI1) && uiGame.gameMenu.active;

			//Show = shouldShowByGameState = DSPGame.MenuDemoLoaded;

			if (!shouldShowByGameState)
			{
				return;
			}

			if (!isInit && GameMain.isRunning) { Init(); }


			//windowStyle.contentOffset = new Vector2(0, 0);

			if (Show)
			{
				winRect = GUILayout.Window(55416755, winRect, WindowFunc, "", windowStyle);
				UIHelper.EatInputInRect(winRect);
			}
		}

        private void WindowFunc(int id)
        {
			PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[veinGroupIndex];
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

			dropdown.indexNumber = (int)veinGroup.type - 1;
			if (dropdown.OnGUI(225, GUILayout.MinHeight(42), GUILayout.MinWidth(150), GUILayout.MaxWidth(150)))
			{
				Gardener.VeinGroup.ChangeType(veinGroupIndex, (EVeinType)dropdown.indexNumber + 1, localPlanet);
			}
			/*
			GUILayout.Box(texture, VeinIconLayoutOptions);
			GUILayout.Box("Type "+ veinGroup.type + "\n" + veinName);
			*/
			GUILayout.BeginVertical(GUILayout.Width(150));
			GUILayout.Label("Count " + veinGroup.count);
			GUILayout.Label("Amount " + veinGroup.amount);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}
		public System.Random random = new System.Random(0);

    }
}
