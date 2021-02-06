using BepInEx;
using System;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;

namespace DSPHelloWorld
{
    public static class MinerNotificationUI
    {
        public static bool Show;
        private static Rect winRect = new Rect(0, 0, 700, 300);
        private static Vector2 sv;
        private static GUIStyle buttonWrapperStyle;
        private static GUIStyle textAlignStyle;
        private static bool isInit = false;
        private static Texture2D sign_state_0;

        public static void EatInputInRect(Rect eatRect)
        {
            if (eatRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                Input.ResetInputAxes();
        }

        private static void Init()
        {
            isInit = true;

            textAlignStyle = new GUIStyle(GUI.skin.label);
            textAlignStyle.alignment = TextAnchor.MiddleLeft;


            // sign_state_0 = Resources.Load("sign-state-0") as Texture2D;
            //UIButton[] warningButtons = Traverse.Create(UIRoot.instance.optionWindow).Field("fieldname").GetValue() as UIButton[];
            // warningButtons[0].button.image.mainTexture
            //var rhc = UIRoot.instance.optionWindow.GetFieldValue<UIButton[]>("buildingWarnButtons");
        }


        public static void OnGUI()
        {
            if (!isInit) { Init(); }

            winRect = GUILayout.Window(55416752, winRect, WindowFunc, "Mineral Vein Miner Exhaustion Information");
            EatInputInRect(winRect);
        }

        public static void Draw(PlanetFactory factory)
        {
            var entitySignMat = Configs.builtin.entitySignMat;

            bool showSign = true;
            bool entitySignOn = true;

            Shader.SetGlobalFloat("_Global_ShowEntitySign", (!showSign || !entitySignOn) ? 0f : 1f);
            // Shader.SetGlobalFloat("_Global_ShowEntityIcon", (!showIcon || !entitySignOn) ? 0f : 1f);
            // Shader.SetGlobalInt("_EntitySignMask", buildingWarnMask);
            if (factory.entityCursor > 1)
            {
                entitySignMat.SetBuffer("_SignBuffer", factory.planet.factoryModel.entitySignBuffer);
                entitySignMat.SetPass(0);
                Graphics.DrawProcedural(MeshTopology.Quads, 8 * factory.entityCursor, 1);
            }
        }

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
            switch(signNum)
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


        public static void WindowFunc(int id)
        {
            GUILayout.BeginArea(new Rect(winRect.width - 22f, 2f, 20f, 17f));
            if (GUILayout.Button("X"))
            {
                //Event.current.Use();
                Show = false;
            }
            GUILayout.EndArea();

            // Size budget is 750. Divided equally in 5 = 150.
            // PlanetCol = 350, Sum: 350
            // Location = 200, 450
            // Alarm Symbol = 50, 500
            // Vein Icon = 50, 550
            // Vein Type = 100, 600
            // Vein Amount = 150, 750
            GUILayoutOption PlanetColWidth = GUILayout.Width(160);
            GUILayoutOption LocationColWidth = GUILayout.Width(80);
            GUILayoutOption AlarmColWidth = GUILayout.Width(90);
            //GUILayoutOption VeinIconColWidth = GUILayout.Width(50);
            GUILayoutOption VeinTypeColWidth = GUILayout.Width(100);
            GUILayoutOption VeinAmountColWidth = GUILayout.Width(110);


            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"<b>Planet</b>", textAlignStyle, PlanetColWidth);
            GUILayout.Label($"<b>Location</b>", textAlignStyle, LocationColWidth);
            GUILayout.Label($"<b>Alarm</b>", textAlignStyle, AlarmColWidth);
            //GUILayout.Label($"<b>Vein</b>", textAlignStyle, VeinIconColWidth);
            GUILayout.Label($"<b>Vein</b>", textAlignStyle, VeinTypeColWidth);
            GUILayout.Label($"<b>Amount Left</b>", textAlignStyle, VeinAmountColWidth);

            GUILayout.EndHorizontal();

            sv = GUILayout.BeginScrollView(sv, GUI.skin.box);
            
            foreach (var item in MineralExhaustionNotifier.notificationList)
            {
                string latLon = PositionToLatLon(item.plantPosition);

                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"{item.planetName}", textAlignStyle, PlanetColWidth);
                GUILayout.Label($"{latLon}", textAlignStyle, LocationColWidth);
                GUILayout.Label(SignNumToText(item.signType), textAlignStyle, AlarmColWidth);
                //GUILayout.Label($"{item.veinName}", textAlignStyle, VeinIconColWidth);
                GUILayout.Label($"{item.veinName}", textAlignStyle, VeinTypeColWidth);
                GUILayout.Label($"{item.veinAmount}", textAlignStyle, VeinAmountColWidth);
                GUILayout.EndHorizontal();
            }

            
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
            /*
            var e = Event.current;

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.MouseMove) && winRect.Contains(e.mousePosition))
            {
                Event.current.Use();
            }
            */
        }
    }

    /*
    public static class ProtoSetEx
    {
        private static Vector2 sv;
        private static Dictionary<Type, int> selectPages = new Dictionary<Type, int>()
        {
            {typeof(AdvisorTipProtoSet) , 0 },
            {typeof(AudioProtoSet) , 0 },
            {typeof(EffectEmitterProtoSet) , 0 },
            {typeof(ItemProtoSet) , 0 },
            {typeof(ModelProtoSet) , 0 },
            {typeof(PlayerProtoSet) , 0 },
            {typeof(RecipeProtoSet) , 0 },
            {typeof(StringProtoSet) , 0 },
            {typeof(TechProtoSet) , 0 },
            {typeof(ThemeProtoSet) , 0 },
            {typeof(TutorialProtoSet) , 0 },
            {typeof(VegeProtoSet) , 0 },
            {typeof(VeinProtoSet) , 0 }
        };

        public static void ShowSet<T>(this ProtoSet<T> protoSet) where T : Proto
        {
            if (protoSet.dataArray.Length > 100)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"Page {selectPages[protoSet.GetType()] + 1} / {protoSet.dataArray.Length / 100 + 1}", GUILayout.Width(80));
                if (GUILayout.Button("-", GUILayout.Width(20))) selectPages[protoSet.GetType()]--;
                if (GUILayout.Button("+", GUILayout.Width(20))) selectPages[protoSet.GetType()]++;
                if (selectPages[protoSet.GetType()] < 0) selectPages[protoSet.GetType()] = protoSet.dataArray.Length / 100;
                else if (selectPages[protoSet.GetType()] > protoSet.dataArray.Length / 100) selectPages[protoSet.GetType()] = 0;
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"index", GUILayout.Width(40));
            GUILayout.Label($"ID", GUILayout.Width(40));
            GUILayout.Label($"Name");
            GUILayout.Label($"TranslateName");
            if (SupportsHelper.SupportsRuntimeUnityEditor)
            {
                GUILayout.Label($"Show Data", GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
            sv = GUILayout.BeginScrollView(sv, GUI.skin.box);
            for (int i = selectPages[protoSet.GetType()] * 100; i < Mathf.Min(selectPages[protoSet.GetType()] * 100 + 100, protoSet.dataArray.Length); i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{i}", GUILayout.Width(40));
                if (protoSet.dataArray[i] != null)
                {
                    GUILayout.Label($"{protoSet.dataArray[i].ID}", GUILayout.Width(40));
                    GUILayout.Label($"{protoSet.dataArray[i].Name}");
                    GUILayout.Label($"{protoSet.dataArray[i].name.Translate()}");
                    if (SupportsHelper.SupportsRuntimeUnityEditor)
                    {
                        if (GUILayout.Button($"Show Data", GUILayout.Width(100)))
                        {
                            ShowItem item = new ShowItem(protoSet.dataArray[i], $"{protoSet.dataArray[i].GetType().Name} {protoSet.dataArray[i].name.Translate()}");
                            RUEHelper.ShowData(item);
                        }
                    }
                }
                else
                {
                    GUILayout.Label("null");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
        */
}
