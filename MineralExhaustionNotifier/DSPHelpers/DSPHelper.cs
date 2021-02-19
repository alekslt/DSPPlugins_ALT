using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DSPPlugins_ALT
{
    public class DSPHelper
    {
        public static string PositionToLatLon(Vector3 position)
        {
            Maths.GetLatitudeLongitude(position, out var latd, out var latf, out var logd, out var logf, out var north, out var south, out var west, out var east);
            bool flag2 = !north && !south;
            bool flag3 = !east && !west;


            string lat = latd + "° " + latf + "′";
            string lon = logd + "° " + logf + "′";

            lat += (north) ? " N" : " S";
            lon += (west) ? " W" : " E";

            return lat + "\n" + lon;
        }

        public static string SignNumToText(uint signNum)
        {
            switch (signNum)
            {
                case SignData.NONE:
                    return "";
                case SignData.NO_CONNECTION:
                    return "No Connection";
                case SignData.NO_DEMAND:
                    return "No Demand";
                case SignData.CUT_PRODUCTION_SOON:
                    return "Low Resource";
                case SignData.NOT_WORKING:
                    return "Not Working";
                case SignData.NO_FUEL:
                    return "No fuel";
                case SignData.NO_RECIPE:
                    return "No recipie";
                case SignData.LOW_POWER:
                    return "Low Power";
                case SignData.NO_POWER:
                    return "No Power";
                case SignData.NO_POWER_CONN:
                    return "No power connection";
                case SignData.NEED_SETTING:
                    return "Need settings";
                default:
                    return "";
            }
        }

        public static int SignNumToTextureIndex(uint signNum)
        {
            switch (signNum)
            {
                case SignData.NONE:
                    return 7;
                case SignData.NO_CONNECTION:
                    return 8;
                case SignData.NO_DEMAND:
                    return 4;
                case SignData.CUT_PRODUCTION_SOON:
                    return 5;
                case SignData.NOT_WORKING:
                    return 7;
                case SignData.NO_FUEL:
                    return 6;
                case SignData.NO_RECIPE:
                    return 3;
                case SignData.LOW_POWER:
                    return 2;
                case SignData.NO_POWER:
                    return 1;
                case SignData.NO_POWER_CONN:
                    return 0;
                case SignData.NEED_SETTING:
                    return 3;
                default:
                    return 7;
            }
        }


        public static string WorkStateToText(EWorkState workState, float consumerRatio)
        {
            switch (workState)
            {
                case EWorkState.Idle:
                    return "无矿物".Translate();
                case EWorkState.Lack:
                    return "缺少原材料".Translate();
                case EWorkState.Full:
                    return "产物堆积".Translate();
                case EWorkState.Running:
                case EWorkState.Outputing:
                    if (consumerRatio == 1f)
                    {
                        return "正常运转".Translate();
                    }
                    else if (consumerRatio > 0.1f)
                    {
                        return "电力不足".Translate();
                    }
                    else
                    {
                        return "停止运转".Translate();
                    }
            }
            return "Unknown";
        }
    }
}
