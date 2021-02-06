using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSPHelloWorld
{
    [BepInPlugin("net.toppe.bepinex.plugins.dsp.helloworld", "HelloWorld Plug-In", "1.0.0.0")]
    public class HelloWorldPlugin : BaseUnityPlugin
    {
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Hello, world!");
        }
    }
}
