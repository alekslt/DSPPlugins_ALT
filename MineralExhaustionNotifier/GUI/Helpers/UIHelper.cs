using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DSPPlugins_ALT
{
    public class UIHelper
    {
        public static void EatInputInRect(Rect eatRect)
        {
            var scaledEatRect = new Rect(UnityEngine.GUI.matrix.lossyScale.x * eatRect.x, UnityEngine.GUI.matrix.lossyScale.y * eatRect.y,
                UnityEngine.GUI.matrix.lossyScale.x * eatRect.width, UnityEngine.GUI.matrix.lossyScale.y * eatRect.height);
            if (scaledEatRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
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
