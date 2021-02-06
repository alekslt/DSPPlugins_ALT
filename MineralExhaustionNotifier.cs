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
        static long timeStepsSecond = 60;
        static int checkPeriodSeconds = 5;
        static MinerStatistics minerStatistics = new MinerStatistics();
        public static List<MiningDetail> notificationList = new List<MiningDetail>();

        #region Unity Core Methods
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Mineral Vein Exhaustion Plugin Loaded!");
            var harmony = new Harmony("net.toppe.bepinex.plugins.dsp.veinexhaustion");
            harmony.PatchAll();
        }
        void Update()
        {
            if (minerStatistics.triggerNotification)
            {
                showDialog = true;
                minerStatistics.triggerNotification = false;
                minerStatistics.lastTriggeredNotification = GameMain.instance.timei;
                MinerNotificationUI.Show = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl)) {
                showDialog = true;
                MinerNotificationUI.Show = !MinerNotificationUI.Show;
            }
        }

        void OnGUI()
        {
            if (showDialog && MinerNotificationUI.Show)
            {
                MinerNotificationUI.OnGUI();
            }
        }
        #endregion // Unity Core Methods

        #region Harmony Patch Hooks in DSP

        class MinerStatistics
        {
            class NotificationTiming { public long lastNotification; public long lastUpdated; };

            Dictionary<int, NotificationTiming> notificationTimes = new Dictionary<int, NotificationTiming>();
            public bool triggerNotification = false;

            long notificationWindowLow = timeStepsSecond * 30;
            long notificationWindowHigh = timeStepsSecond * 60;
            long notificationPruneTime = timeStepsSecond * 30;
            public long lastTriggeredNotification = 0;

            public void updateNotificationTimes(long time)
            {
                List<int> deletionList = new List<int>();

                var deltaTriggerNotification = time - lastTriggeredNotification;

                foreach (var miner in notificationTimes)
                {
                    var deltaNotification = time - miner.Value.lastNotification;
                    var deltaUpdated = time - miner.Value.lastUpdated;
                    

                    // Skip if we haven't waited long enough to trigger notification
                    if (deltaNotification > notificationWindowHigh &&
                        deltaTriggerNotification > notificationWindowHigh)
                    {
                        triggerNotification = true;
                    }

                    if (deltaUpdated > notificationPruneTime)
                    {
                        deletionList.Add(miner.Key);
                    }
                }
                foreach (var miner in deletionList)
                {
                    notificationTimes.Remove(miner);
                }
            }

            public void onFactorySystem_GameTick(long time, FactorySystem factorySystem)
            {

                //Debug.Log("Tick");
                notificationList.Clear();
                var factory = factorySystem.factory;
                VeinData[] veinPool = factory.veinPool;

                var minerPool = factorySystem.minerPool;

                PowerSystem powerSystem = factory.powerSystem;
                float[] networkServes = powerSystem.networkServes;
                PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;

                GameHistoryData history = GameMain.history;
                float miningCostRate = history.miningCostRate;
                float miningSpeedScale = history.miningSpeedScale;

                GameStatData statistics = GameMain.statistics;
                FactoryProductionStat factoryProductionStat = statistics.production.factoryStatPool[factory.index];
                int[] productRegister = factoryProductionStat.productRegister;

                for (int i = 1; i < factorySystem.minerCursor; i++)
                {
                    if (factorySystem.minerPool[i].id == i)
                    {
                        int entityId = minerPool[i].entityId;
                        float num2 = networkServes[consumerPool[minerPool[i].pcId].networkId];
                        var minerComponent = factorySystem.minerPool[i];
                        if (MinerComponent_InternalUpdate(factory, veinPool, num2, miningCostRate, miningSpeedScale, productRegister, minerComponent))
                        {
                            // Entry already in list. We need to check if we should retrigger a notification
                            if (notificationTimes.ContainsKey(minerComponent.entityId))
                            {
                                var delta = time - notificationTimes[minerComponent.entityId].lastNotification;
                                notificationTimes[minerComponent.entityId].lastUpdated = time;
                            } else
                            {
                                // We want to trigger almost immediately, but leave notificationWindowLow for cases where you are actively building and haven't wired up things.
                                notificationTimes[minerComponent.entityId] = new NotificationTiming()
                                {
                                    lastNotification = time - notificationWindowHigh + notificationWindowLow,
                                    lastUpdated = time
                                };
                            }
                        }
                    }
                }
            }

            public bool MinerComponent_InternalUpdate(PlanetFactory factory, VeinData[] veinPool, float power, float miningRate, float miningSpeed, int[] productRegister, MinerComponent __instance)
            {
                if (__instance.type != EMinerType.Vein)
                {
                    return false;
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
                    notificationList.Add(new MiningDetail()
                    {
                        entityId = __instance.entityId,
                        planetName = factory.planet.displayName,
                        signType = factory.entitySignPool[__instance.entityId].signType,
                        veinName = veinName,
                        veinAmount = veinAmount,
                        plantPosition = plantPosition,
                        factory = factory,
                    }) ;

                    return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick")]
        class FactorySystem_GameTick
        {
            static long lastTime = 0;

            public static void Postfix(long time, bool isActive, FactorySystem __instance)
            {
                if (time - lastTime < (timeStepsSecond * checkPeriodSeconds)) { return; }
                lastTime = time;

                minerStatistics.onFactorySystem_GameTick(time, __instance);
                minerStatistics.updateNotificationTimes(time);

            }
        }

        #endregion // Harmony Patch Hooks in DSP
    }
}
