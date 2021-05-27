using HarmonyLib;
using UnityEngine;

namespace VeinPlanter
{
    public class Patch_GameMain_Begin
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameMain), "Begin")]
        public static void HookGameStart()
        {
            VeinPlanter.instance.modePresenter.Hide();
            VeinPlanter.instance.veinGroupModifyPresenter.Hide();
            Debug.Log("Resetting dialog");
        }
    }
}
