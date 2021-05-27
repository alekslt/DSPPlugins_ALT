using System.Collections.Generic;
using UnityEngine;

namespace VeinPlanter
{
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
			GUILayout.BeginHorizontal(GUILayout.MinWidth(containerWidth));
			if (show)
			{
				scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, UnityEngine.GUI.skin.box);

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
			GUILayout.EndHorizontal();

			return oldIndexNumber != indexNumber;
		}
	}
}
