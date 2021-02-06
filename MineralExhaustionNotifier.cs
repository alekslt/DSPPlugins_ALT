using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DSPHelloWorld
{
    [BepInPlugin("net.toppe.bepinex.plugins.dsp.veinexhaustion", "Mineral Vein Exhaustion Plug-In", "1.0.0.0")]
    public partial class MineralExhaustionNotifier : BaseUnityPlugin
    {
        static bool showDialog = true;
        static bool sendTip = false;
        public static List<MiningDetail> notificationList = new List<MiningDetail>();

        // Give notification on when ore patch is running low or is exhausted.
        // https://github.com/ragzilla/DSP_Mods/blob/main/DSP_MinerOverride/MinerOverride.cs

        public static UINotificationBox notificationBox = new UINotificationBox();

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Mineral Vein Exhaustion Plugin Loaded!");
            var harmony = new Harmony("net.toppe.bepinex.plugins.dsp.veinexhaustion");
            harmony.PatchAll();
        }
        void Update()
        {
            if (showDialog)
            {
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    MinerNotificationUI.Show = !MinerNotificationUI.Show;
                }
            }
        }

        void OnGUI()
        {
            if (showDialog && MinerNotificationUI.Show)
            {
                MinerNotificationUI.OnGUI();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnUpdate")]
        public static class UIGame__OnUpdate
        {
            public static void Postfix(UIGame __instance)
            {
                // notificationBox._Init(null);
                if (sendTip == false)
                {
                    //for (; notificationList.Count > 0;)
                    {
                        //UIRealtimeTip.Popup(notificationList[0]);
                        //notificationList.RemoveAt(0);
                        //sendTip = true;
                    }
                    
                    
                }

            }
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick")]
        class FactorySystem_GameTick
        {
            static long lastTime = 0;
            const long timeStepsSecond = 60;
            public static void Postfix(long time, bool isActive, FactorySystem __instance)
            {
                if (time - lastTime < (timeStepsSecond * 5)) { return; }
                lastTime = time;

                //Debug.Log("Tick");
                notificationList.Clear();
                var factory = __instance.factory;
                VeinData[] veinPool = factory.veinPool;

                var minerPool = __instance.minerPool;

                PowerSystem powerSystem = factory.powerSystem;
                float[] networkServes = powerSystem.networkServes;
                PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;

                GameHistoryData history = GameMain.history;
                float miningCostRate = history.miningCostRate;
                float miningSpeedScale = history.miningSpeedScale;

                GameStatData statistics = GameMain.statistics;
                FactoryProductionStat factoryProductionStat = statistics.production.factoryStatPool[factory.index];
                int[] productRegister = factoryProductionStat.productRegister;

                for (int i = 1; i < __instance.minerCursor; i++)
                {
                    if (__instance.minerPool[i].id == i)
                    {
                        int entityId = minerPool[i].entityId;
                        float num2 = networkServes[consumerPool[minerPool[i].pcId].networkId];
                        MinerComponent_InternalUpdate(factory, veinPool, num2, miningCostRate, miningSpeedScale, productRegister, __instance.minerPool[i]);
                        // entitySignPool[entityId].signType = ((minerPool[i].minimumVeinAmount < 1000) ? 7u : 0u);
                        // entitySignPool[entityId].count0 = minerPool[i].minimumVeinAmount;
                    }
                }
            }
        }


        //[HarmonyPostfix, HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        public static void MinerComponent_InternalUpdate(PlanetFactory factory, VeinData[] veinPool, float power, float miningRate, float miningSpeed, int[] productRegister, MinerComponent __instance)
        {

            //if (!Input.GetKey(KeyCode.LeftShift))
            {
                /*
                if (!VFInput.onGUI)
                {
                    UICursor.SetCursor(ECursor.Delete);
                }
                */
                //return;
            }

            if (__instance.type != EMinerType.Vein)
            {
                return;
            }

            var plantPosition = factory.entityPool[__instance.entityId].pos;
            
            ItemProto itemProto = LDB.items.Select(__instance.productId);
            int vpNum = ((__instance.veinCount != 0) ? __instance.veins[__instance.currentVeinIndex] : 0);
            VeinData veinData = veinPool[vpNum];
            VeinProto veinProto = LDB.veins.Select((int)veinData.type);
            
            //string veinName = (vpNum == 0) ? "Empty" : veinData.type.ToString();
            string veinName = veinData.type.ToString();
            string product = veinData.type.ToString();
            int veinAmount = 0;
            if (__instance.veinCount > 0)
            {
                for (int i = 0; i < __instance.veinCount; i++)
                {
                    int num = __instance.veins[i];
                    if (num > 0 && veinPool[num].id == num && veinPool[num].amount > 0)
                    {
                        veinAmount += veinPool[num].amount;
                    }
                }
            }

            // Debug.Log(factory.planet.displayName + " - " + __instance.entityId + ", " + veinName + ", " + __instance.workstate + ", VeinCount: " + __instance.veinCount + " VeinAmount: " + veinAmount + " | " + latlon);

            if (veinAmount < 6000)
            {
                // notificationList.Add("Mining facility on planet " + factory.planet.displayName + " on a " + veinName + " is running low. Remaining ore: " + veinAmount + "| Product: " + product + ". Move it asap.\nPosition: " + latlon);
                notificationList.Add(new MiningDetail() {
                    planetName=factory.planet.displayName,
                    signType=factory.entitySignPool[__instance.entityId].signType,
                    veinName =veinName,
                    veinAmount=veinAmount,
                    plantPosition=plantPosition,
                    factory=factory,
                });
            }
        }
    }
}
