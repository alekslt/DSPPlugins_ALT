using System.Collections.Generic;
using UnityEngine;

namespace VeinPlanter
{
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
}
