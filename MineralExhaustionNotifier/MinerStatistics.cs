using System;
using System.Collections.Generic;
using UnityEngine;

namespace DSPPlugins_ALT
{
    public class MinerStatistics
    {
        class NotificationTiming { public long lastNotification; public long lastUpdated; };

        public static Dictionary<string,List<MinerNotificationDetail>> notificationList = new Dictionary<string,List<MinerNotificationDetail>>();
        Dictionary<int, NotificationTiming> notificationTimes = new Dictionary<int, NotificationTiming>();
        public bool triggerNotification = false;

        long notificationWindowLow = MineralExhaustionNotifier.timeStepsSecond * 30;
        long notificationWindowHigh = MineralExhaustionNotifier.timeStepsSecond * 60;
        long notificationPruneTime = MineralExhaustionNotifier.timeStepsSecond * 30;
        public long lastTriggeredNotification = 0;
        public bool firstTimeNotification = true;

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

            if (notificationTimes.Count == 0)
            {
                firstTimeNotification = true;
                MinerNotificationUI.HighlightButton = false;
            } else
            {
                MinerNotificationUI.HighlightButton = true;
            }
        }

        public void prioritizeList()
        {
            foreach (var planet in MinerStatistics.notificationList)
            {
                planet.Value.Sort(delegate (MinerNotificationDetail x, MinerNotificationDetail y)
                {
                    if (x.veinAmount == y.veinAmount) return 0;
                    else if (x.veinAmount < y.veinAmount) return -1;
                    else if (x.veinAmount > y.veinAmount) return 1;
                    return 0;
                });
            }
        }

        public void onFactorySystem_GameTick(long time, FactorySystem factorySystem)
        {
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
                    var minerComponent = factorySystem.minerPool[i];
                    var network = networkServes[consumerPool[minerPool[i].pcId].networkId];

                    if (MinerComponent_InternalUpdate(factory, veinPool, network, miningCostRate, miningSpeedScale, productRegister, minerComponent))
                    {
                        // Update notificationTimes used to trigger notifications
                        if (notificationTimes.ContainsKey(minerComponent.entityId))
                        {
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

        public bool MinerComponent_InternalUpdate(PlanetFactory factory, VeinData[] veinPool, float power, float miningRate, float miningSpeed, int[] productRegister, MinerComponent minerComponent)
        {
            if (minerComponent.type != EMinerType.Vein)
            {
                return false;
            }

            var plantPosition = factory.entityPool[minerComponent.entityId].pos;

            int vpNum = ((minerComponent.veinCount != 0) ? minerComponent.veins[minerComponent.currentVeinIndex] : 0);
            VeinData veinData = veinPool[vpNum];
            string veinName = veinData.type.ToString();

            int veinAmount = 0;
            if (minerComponent.veinCount > 0)
            {
                for (int i = 0; i < minerComponent.veinCount; i++)
                {
                    int num = minerComponent.veins[i];
                    if (num > 0 && veinPool[num].id == num && veinPool[num].amount > 0)
                    {
                        veinAmount += veinPool[num].amount;
                    }
                }
            }

            var signData = factory.entitySignPool[minerComponent.entityId];
            var signType = signData.signType;

            var time = (int)(power * (float)minerComponent.speed * miningSpeed * (float)minerComponent.veinCount);

            ItemProto itemProto = signData.iconId0 != 0 ? LDB.items.Select((int)signData.iconId0) : null;

            // Debug.Log(factory.planet.displayName + " - " + __instance.entityId + ", " + veinName + ", " + __instance.workstate + ", VeinCount: " + __instance.veinCount + " VeinAmount: " + veinAmount + " | " + latlon);

            if (veinAmount < MineralExhaustionNotifier.VeinAmountThreshold.Value || signType != SignData.NONE)
            {
                if (!notificationList.ContainsKey(factory.planet.displayName))
                {
                    notificationList.Add(factory.planet.displayName, new List<MinerNotificationDetail>());
                }

                Texture2D texture = null;
                
                if (itemProto != null)
                {
                    texture = itemProto.iconSprite.texture;
                    veinName = itemProto.name.Translate();
                }

                string minutesToEmptyVeinTxt;
                var miningRatePerMin = 0f;
                if (time == 0 || veinAmount == 0 || minerComponent.period == 0)
                {
                    if (veinAmount == 0)
                    {
                        minutesToEmptyVeinTxt = "Empty";
                    }
                    else
                    {
                        minutesToEmptyVeinTxt = "Infinity";
                    }

                }
                else
                {
                    var miningTimePerSec = minerComponent.period / (MineralExhaustionNotifier.timeStepsSecond);
                    var secondsPerMiningOperation = (float)miningTimePerSec / (float)time;
                    miningRatePerMin = 60 / secondsPerMiningOperation;
                    var minutesToEmptyVein = (float)veinAmount / miningRatePerMin;
                    minutesToEmptyVeinTxt = Math.Round(minutesToEmptyVein).ToString() + " min"; // .ToString("0.0") + "每分钟".Translate();
                }

                notificationList[factory.planet.displayName].Add(new MinerNotificationDetail()
                {
                    entityId = minerComponent.entityId,
                    planetName = factory.planet.displayName,
                    itemProto = itemProto,
                    signType = factory.entitySignPool[minerComponent.entityId].signType,
                    veinName = veinName,
                    veinAmount = veinAmount,
                    plantPosition = plantPosition,
                    factory = factory,
                    miningRate = miningRate,
                    time = time,
                    period = minerComponent.period,
                    veinCount = minerComponent.veinCount,
                    miningRatePerMin = miningRatePerMin,
                    minutesToEmptyVeinTxt = minutesToEmptyVeinTxt,
                    resourceTexture = texture,
                }); ;

                return true;
            }
            return false;
        }
    }
}
