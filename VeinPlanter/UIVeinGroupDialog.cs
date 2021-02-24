using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VeinPlanter
{
	public class UIHelper
	{
		public static void EatInputInRect(Rect eatRect)
		{
			if (eatRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
			{
				// Ideally I want to only block mouse events from going through.
				var isMouseInput = Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.mouseScrollDelta.y != 0;

				if (!isMouseInput)
				{
					// UnityEngine.Debug.Log("Canceling capture due to input not being mouse");
					return;
				}
				else
				{
					Input.ResetInputAxes();
				}

			}
		}
	}
	public class DropDownGUI
	{
		private Vector2 scrollViewVector = Vector2.zero;
		public Rect dropDownRect = new Rect(125, 50, 125, 300);
		public List<string> list = new List<string>() { "Drop_Down_Menu" };

		int indexNumber;
		bool show = false;

		public void OnGUI()
		{
			if (GUI.Button(new Rect((dropDownRect.x - 100), dropDownRect.y, dropDownRect.width, 25), ""))
			{
				show = !show;
			}

			if (show)
			{
				scrollViewVector = GUI.BeginScrollView(new Rect((dropDownRect.x - 100), (dropDownRect.y + 25), dropDownRect.width, dropDownRect.height), scrollViewVector, new Rect(0, 0, dropDownRect.width, Mathf.Max(dropDownRect.height, (list.Count * 25))));

				GUI.Box(new Rect(0, 0, dropDownRect.width, Mathf.Max(dropDownRect.height, (list.Count * 25))), "");

				for (int index = 0; index < list.Count; index++)
				{

					if (GUI.Button(new Rect(0, (index * 25), dropDownRect.height, 25), ""))
					{
						show = false;
						indexNumber = index;
					}

					GUI.Label(new Rect(5, (index * 25), dropDownRect.height, 25), list[index]);

				}
				GUI.EndScrollView();
			}
			else
			{
				GUI.Label(new Rect((dropDownRect.x - 95), dropDownRect.y, 300, 25), list[indexNumber]);
			}

		}
	}
	public class DropDownGUILayout<T>
	{
		private Vector2 scrollViewVector = Vector2.zero;
		public Rect dropDownRect = new Rect(125, 50, 125, 300);
		public List<T> list = new List<T>();

		public int indexNumber = 0;
		bool show = false;

		public delegate bool DrawItemDelegate(T item);

		public DrawItemDelegate DrawItem;

		public bool OnGUI(float containerWidth, params GUILayoutOption[] options)
		{
			int oldIndexNumber = indexNumber;
			if (show)
			{
				scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, UnityEngine.GUI.skin.box, GUILayout.Width(containerWidth));

				//scrollViewVector = GUI.BeginScrollView(new Rect((dropDownRect.x - 100), (dropDownRect.y + 25), dropDownRect.width, dropDownRect.height), scrollViewVector, new Rect(0, 0, dropDownRect.width, Mathf.Max(dropDownRect.height, (list.Length * 25))));

				//GUILayout.Box(new Rect(0, 0, dropDownRect.width, Mathf.Max(dropDownRect.height, (list.Length * 25))), "");
				GUILayout.BeginVertical();
				for (int index = 0; index < list.Count; index++)
				{
					GUILayout.BeginHorizontal(GUI.skin.button, options);
					if (DrawItem(list[index]))
					{
						show = false;
						indexNumber = index;
					}
					GUILayout.EndHorizontal();

				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
			}
			else
			{
				GUILayout.BeginHorizontal(GUI.skin.button, options);
				if (DrawItem(list[indexNumber]))
				{
					show = !show;
				}
				GUILayout.EndHorizontal();
			}

			return oldIndexNumber != indexNumber;
		}
	}

	public class ListItem
    {
		public Texture2D tex;
		public string name;
    }

	public class UIVeinGroupDialog
    {
        private static Rect winRect = new Rect(0, 0, 400, 150);
		public Boolean Show { get; set; } = false;
		
		public GUISkin customSkin = null;

		public DropDownGUILayout<ListItem> dropdown = new DropDownGUILayout<ListItem>();

		public PlanetData localPlanet { get; set; }

		public int veinGroupIndex { get; set; }

		GUILayoutOption[] VeinIconLayoutOptions;
		GUIStyle windowStyle;
		GUIStyle buttonStyle;

		public bool DrawItem(ListItem t)
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
					dropdown.list.Add(new ListItem() { tex= itemProto.iconSprite.texture, name= itemProto.name.Translate() });
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
				winRect.y = Screen.height - winRect.height;
			}

			winRect.x = Mathf.Max(0, winRect.x);
			winRect.y = Mathf.Max(0, winRect.y);			
		}

		public void OnGUI()
        {
			VeinIconLayoutOptions = new GUILayoutOption[] { GUILayout.MaxWidth(40),GUILayout.MinHeight(40)  };
			buttonStyle = new GUIStyle(GUI.skin.box);
			buttonStyle.wordWrap = true;

			windowStyle = new GUIStyle(UnityEngine.GUI.skin.window);
			windowStyle.border = new RectOffset(8,8,8,8);
			windowStyle.padding.top = 10;
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
				ChangeVeinGroupType(veinGroupIndex, (EVeinType)dropdown.indexNumber + 1);
			}
			/*
			GUILayout.Box(texture, VeinIconLayoutOptions);
			GUILayout.Box("Type "+ veinGroup.type + "\n" + veinName);
			*/
			GUILayout.Label("Count " + veinGroup.count);
			GUILayout.Label("Amount " + veinGroup.amount);


			GUILayout.EndHorizontal();
		}
		public System.Random random = new System.Random(0);
		public void ChangeVeinGroupType(int veinGroupIndex, EVeinType newType)
        {
			ref PlanetData.VeinGroup veinGroup = ref localPlanet.veinGroups[veinGroupIndex];
			veinGroup.type = newType;

			int veinTypeIndex = (int)newType;

			for (int i = 1; i < localPlanet.factory.veinCursor; i++)
            {
				ref VeinData vein = ref localPlanet.factory.veinPool[i];
				ref AnimData veinAnim = ref localPlanet.factory.veinAnimPool[i];
				if (i != vein.id
					|| vein.groupIndex != veinGroupIndex)
                {
					continue;
                }
				
				vein.productId = PlanetModelingManager.veinProducts[veinTypeIndex];
				vein.type = newType;

				localPlanet.factoryModel.gpuiManager.RemoveModel(vein.modelIndex, vein.modelId, setBuffer: false);
				vein.modelIndex = (short)random.Next(PlanetModelingManager.veinModelIndexs[veinTypeIndex], PlanetModelingManager.veinModelIndexs[veinTypeIndex] + PlanetModelingManager.veinModelCounts[veinTypeIndex]);

				veinAnim.time = ((vein.amount < 20000) ? (1f - (float)vein.amount * 5E-05f) : 0f);
				veinAnim.prepare_length = 0f;
				veinAnim.working_length = 1f;
				veinAnim.state = (uint)vein.type;
				veinAnim.power = 0f;

				vein.modelId = localPlanet.factoryModel.gpuiManager.AddModel(
					vein.modelIndex, i, vein.pos, Maths.SphericalRotation(vein.pos,
					UnityEngine.Random.value * 360f), setBuffer: false);

				/*
				GameMain.localPlanet.factoryModel.gpuiManager.AlterModel(vein.modelIndex, vein.modelId,
					i, vein.pos, Maths.SphericalRotation(vein.pos, UnityEngine.Random.value * 360f));*/

				GameMain.localPlanet.factory.RefreshVeinMiningDisplay(i, 0, 0);
			}
			GameMain.localPlanet.factoryModel.gpuiManager.SyncAllGPUBuffer();
			/*
			var veinPositions = (from vein in GameMain.localPlanet.factory.veinPool
								 where vein.groupIndex == veinGroupIndex

								 select vein.pos).ToList();
			*/
		}
    }
}
