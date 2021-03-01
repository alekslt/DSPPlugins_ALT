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
}
