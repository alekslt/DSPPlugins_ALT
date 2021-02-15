using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSPPlugins_ALT.Statistics.Helpers
{
    public static class DSPStatisticsHelper
    {
        public static int GetTotalVeinAmountForMineComponent(MinerComponent minerComponent, VeinData[] veinPool)
        {
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
            return veinAmount;
        }
    }
}
