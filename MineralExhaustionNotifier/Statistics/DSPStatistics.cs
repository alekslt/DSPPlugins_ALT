using DSPPlugins_ALT.Statistics.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DSPPlugins_ALT.Statistics
{
    class StationItemStat
    {
        internal StationStore item;
        internal ItemProto itemProto;
    }

    public class StationStat
    {
        internal IList<StationItemStat> products;
        internal PlanetData planetData;
        internal string name;
        internal StationComponent stationComponent;
    };

    public class DSPStatistics
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
        static long lastTime = 0;

        public static List<MinerNotificationDetail> minerStats = new List<MinerNotificationDetail>();
        public static List<StationStat> logisticsStationStats = new List<StationStat>();

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
            foreach (var planet in DSPStatistics.notificationList)
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


        public void onGameData_GameTick(long time, GameData gameData)
        {
            if (time - lastTime < (MineralExhaustionNotifier.timeStepsSecond * MineralExhaustionNotifier.CheckPeriodSeconds.Value)) { return; }
            lastTime = time;

            notificationList.Clear();
            minerStats.Clear();
            logisticsStationStats.Clear();

            foreach (var planetFactory in gameData.factories)
            {
                if (planetFactory != null && planetFactory.factorySystem != null)
                {
                    factorySystemStatUpdate(time, planetFactory.factorySystem);
                    for (int i = 1; i < planetFactory.transport.stationCursor; i++)
                    {
                        if (planetFactory.transport.stationPool[i] != null && planetFactory.transport.stationPool[i].id == i)
                        {
                            stationStatUpdate(planetFactory.transport.stationPool[i], planetFactory.planet);
                        }
                    }
                }
            }
            /*
            for (int i = 1; i < gameData.galacticTransport.stationCursor; i++)
            {
                if (gameData.galacticTransport.stationPool[i] != null && gameData.galacticTransport.stationPool[i].gid == i)
                {
                    stationStatUpdate(gameData.galacticTransport.stationPool[i], gameData.galaxy.PlanetById(gameData.galacticTransport.stationPool[i].planetId));
                }
            }*/

            updateNotificationTimes(time);
            prioritizeList();
        }

        public void stationStatUpdate(StationComponent stationComponent, PlanetData planetData)
        {
            var items = from item in stationComponent.storage
                        where item.itemId != 0
                        select new StationItemStat()
                        {
                            item = item,
                            itemProto = LDB.items.Select(item.itemId)
                        };

            string text = ((!string.IsNullOrEmpty(stationComponent.name)) ? stationComponent.name : ((!stationComponent.isStellar) ? ("本地站点号".Translate() + stationComponent.id) : ("星际站点号".Translate() + stationComponent.gid)));
            logisticsStationStats.Add(new StationStat() {
                planetData = planetData,
                stationComponent = stationComponent,
                name = text,
                products = items.ToList()
            });
        }

        public void factorySystemStatUpdate(long time, FactorySystem factorySystem)
        {
            var factory = factorySystem.factory;

            FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[factory.index];
            int[] productRegister = factoryProductionStat.productRegister;

            for (int i = 1; i < factorySystem.minerCursor; i++)
            {
                if (factorySystem.minerPool[i].id == i)
                {
                    var minerComponent = factorySystem.minerPool[i];
                    var networkId = factory.powerSystem.consumerPool[minerComponent.pcId].networkId;

                    if (MinerComponent_InternalUpdate(
                        factory,
                        factory.veinPool,
                        factory.powerSystem.netPool[networkId],
                        factory.powerSystem.networkServes[networkId],
                        GameMain.history.miningCostRate,
                        GameMain.history.miningSpeedScale,
                        productRegister,
                        minerComponent))
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


        public bool MinerComponent_InternalUpdate(PlanetFactory factory, VeinData[] veinPool, PowerNetwork powerNetwork, float power, float miningRate, float miningSpeed, int[] productRegister, MinerComponent minerComponent)
        {
            if (minerComponent.type != EMinerType.Vein)
            {
                return false;
            }

            var plantPosition = factory.entityPool[minerComponent.entityId].pos;

            int vpNum = ((minerComponent.veinCount != 0) ? minerComponent.veins[minerComponent.currentVeinIndex] : 0);
            VeinData veinData = veinPool[vpNum];
            string veinName = veinData.type.ToString();
            int veinAmount = DSPStatisticsHelper.GetTotalVeinAmountForMineComponent(minerComponent, veinPool);

            var signData = factory.entitySignPool[minerComponent.entityId];
            var signType = signData.signType;
            ItemProto itemProto = signData.iconId0 != 0 ? LDB.items.Select((int)signData.iconId0) : null;

            var time = (int)(power * (float)minerComponent.speed * miningSpeed * (float)minerComponent.veinCount);

            float consumerRatio = ((powerNetwork == null || powerNetwork.id <= 0) ? 0f : ((float)powerNetwork.consumerRatio));

            // Debug.Log(factory.planet.displayName + " - " + __instance.entityId + ", " + veinName + ", " + __instance.workstate + ", VeinCount: " + __instance.veinCount + " VeinAmount: " + veinAmount + " | " + latlon);

            //if (veinAmount < MineralExhaustionNotifier.VeinAmountThreshold.Value || signType != SignData.NONE)
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
                    minutesToEmptyVeinTxt = (veinAmount == 0) ? "Empty" : "Infinity";
                }
                else
                {
                    var miningTimePerSec = minerComponent.period / (MineralExhaustionNotifier.timeStepsSecond);
                    var secondsPerMiningOperation = (float)miningTimePerSec / (float)time;
                    miningRatePerMin = 60 / secondsPerMiningOperation;
                    var minutesToEmptyVein = (float)veinAmount / miningRatePerMin;
                    minutesToEmptyVeinTxt = Math.Round(minutesToEmptyVein).ToString();
                    if (minerComponent.workstate == EWorkState.Full)
                    {
                        minutesToEmptyVeinTxt += " to ∞";
                    }
                    minutesToEmptyVeinTxt += " min"; // .ToString("0.0") + "每分钟".Translate();
                }
                var minerStat = new MinerNotificationDetail()
                {
                    minerComponent = minerComponent,
                    entityId = minerComponent.entityId,
                    planetName = factory.planet.displayName,
                    itemProto = itemProto,
                    signType = signData.signType,
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
                    powerNetwork = powerNetwork,
                    consumerRatio = consumerRatio
                };
                minerStats.Add(minerStat);
                notificationList[factory.planet.displayName].Add(minerStat); ;

                return true;
            }
            return false;
        }
    }
}
