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

    public class UIPlanetVeins
    {
        private static Rect winRect = new Rect(0, 0, 600, 800);
		public Boolean Show { get; set; } = false;

		public PlanetData localPlanet { get; set; }


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

		public UIPlanetVeins()
        {
			/*
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
			*/

		}

		public void UpdatePos()
        {
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
			windowStyle.padding = new RectOffset(100, 30, 120, 30);
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
		private static Vector2 sv;
		private void WindowFunc(int id)
        {		

			GUILayout.BeginVertical(GUILayout.Width(550), GUILayout.Height(700));
			sv = GUILayout.BeginScrollView(sv, UnityEngine.GUI.skin.box);
			for (int veinGroupIndex = 1; veinGroupIndex < localPlanet.veinGroups.Length; veinGroupIndex++)
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

				GUILayout.BeginHorizontal();
				GUILayout.Box(texture, VeinIconLayoutOptions);
				GUILayout.Box("Type " + veinGroup.type + "\n" + veinName);
				
				GUILayout.Label("Count " + veinGroup.count);
				GUILayout.Label("Amount " + veinGroup.amount);

				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();

			UnityEngine.GUI.DragWindow();
		}
		public System.Random random = new System.Random(0);

    }
}
