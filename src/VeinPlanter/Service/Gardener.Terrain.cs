using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VeinPlanter.Service
{
    static class PlantetFactoryExtensions
    {
        public static int ReformSnap(Vector3 pos, int reformSize, int reformType, int reformColor, Vector3[] reformPoints, int[] reformIndices, PlatformSystem platform, out Vector3 reformCenter, float localPlanetRadius)
        {
            int num = VeinPlanter.instance.veinGardenerState.localPlanet.aux.mainGrid.ReformSnapTo(pos, reformSize, reformType, reformColor, reformPoints, reformIndices, platform, out reformCenter);
            float num2 = localPlanetRadius - 50.2f;
            for (int i = 0; i < num; i++)
            {
                reformPoints[i].x *= num2;
                reformPoints[i].y *= num2;
                reformPoints[i].z *= num2;
            }
            reformCenter *= num2;
            return num;
        }

        private static void TerrainLower(Vector3 worldPos)
        {
            PlayerAction_Build playerAction_Build = GameMain.mainPlayer?.controller.actionBuild;

            if (VFInput.alt && playerAction_Build != null)
            {
                var reformSize = 1;
                var reformType = -1;
                var reformColor = -1;

                bool veinBuried = false;
                float radius = 0.990945935f * (float)reformSize;
                Vector3 reformCenterPoint;

                float localPlanetRadius = VeinPlanter.instance.veinGardenerState.localPlanet.radius;
                float localPlanetRealRadius = VeinPlanter.instance.veinGardenerState.localPlanet.realRadius;

                Debug.Log("TerrainLower Radius: " + localPlanetRadius + " RealRadius: " + localPlanetRealRadius + " Levelized: " + VeinPlanter.instance.veinGardenerState.localPlanet.levelized);

                localPlanetRadius -= 5;
                localPlanetRealRadius -= 5;

                if (VeinPlanter.instance.veinGardenerState.localPlanet.factory.platformSystem.reformData == null)
                {
                    Debug.Log("InitReformData");
                    VeinPlanter.instance.veinGardenerState.localPlanet.factory.platformSystem.InitReformData();
                }
                Debug.Log("ReformSnap: " + VeinPlanter.instance.veinGardenerState.localPlanet.aux);
                /*
                playerAction_Build.reformPointsCount = localPlanet.aux.ReformSnap(worldPos, reformSize, reformType, reformColor,
                    reformPoints, playerAction_Build.reformIndices, localPlanet.factory.platformSystem, out reformCenterPoint);*/
                ReformSnap(worldPos, reformSize, reformType, reformColor, VeinPlanter.instance.veinGardenerState.reformPoints, playerAction_Build.reformIndices, VeinPlanter.instance.veinGardenerState.localPlanet.factory.platformSystem, out reformCenterPoint, localPlanetRadius);

                Debug.Log("reformPointsCount: " + playerAction_Build.reformPointsCount);

                Debug.Log("ComputeFlattenTerrainReform");
                var compFlatten = VeinPlanter.instance.veinGardenerState.localPlanet.factory.ComputeFlattenTerrainReformALT(playerAction_Build.reformPoints, worldPos, radius, playerAction_Build.reformPointsCount, localPlanetRealRadius, fade0: 3f, fade1: 1f);

                Debug.Log("FlattenTerrainReform");
                VeinPlanter.instance.veinGardenerState.localPlanet.factory.FlattenTerrainReformALT(reformCenterPoint, radius, reformSize, veinBuried, localPlanetRadius, localPlanetRealRadius);
                UIRealtimeTip.Popup("Flatten: " + compFlatten);
            }

        }

        public static int ComputeFlattenTerrainReformALT(this PlanetFactory planetFactory, Vector3[] points, Vector3 center, float radius, int pointsCount, float realRadius,
            float fade0 = 3f, float fade1 = 1f)
        {
            PlanetRawData data = planetFactory.planet.data;
            if (planetFactory.tmp_levelChanges == null)
            {
                planetFactory.tmp_levelChanges = new Dictionary<int, int>();
            }
            planetFactory.tmp_levelChanges.Clear();
            Quaternion quaternion = Maths.SphericalRotation(center, 22.5f);
            //float realRadius = planetFactory.planet.realRadius;
            Vector3[] vertices = data.vertices;
            ushort[] heightData = data.heightData;

            float f = ((float)(int)heightData[data.QueryIndex(center)] - realRadius * 100f + 20f) * 0.01f * 2f;
            Debug.Log("ComputeFlattenTerrainReformALT.heightData(center): " + heightData[data.QueryIndex(center)] + " , f: " + f);
            //f = Mathf.Min(9f, Mathf.Abs(f));
            f = Mathf.Min(5f, Mathf.Abs(f));
            fade0 += f;
            float radiusFade = radius + fade0;
            float levelArea = radiusFade * radiusFade;
            Debug.Log("ComputeFlattenTerrainReformALT.levelArea: " + levelArea + " , radius: " + radius + " , fade0: " + fade0);
            //  realRadius * (float)Math.PI / (200 * 2f);
            float num3 = realRadius * (float)Math.PI / ((float)planetFactory.planet.precision * 2f);
            int num4 = Mathf.CeilToInt(radiusFade * 1.414f / num3 + 0.5f);

            Vector3[] array = new Vector3[9]
            {
                center,
                center + quaternion * (new Vector3(0f, 0f, 1.414f) * radiusFade),
                center + quaternion * (new Vector3(0f, 0f, -1.414f) * radiusFade),
                center + quaternion * (new Vector3(1.414f, 0f, 0f) * radiusFade),
                center + quaternion * (new Vector3(-1.414f, 0f, 0f) * radiusFade),
                center + quaternion * (new Vector3(1f, 0f, 1f) * radiusFade),
                center + quaternion * (new Vector3(-1f, 0f, -1f) * radiusFade),
                center + quaternion * (new Vector3(1f, 0f, -1f) * radiusFade),
                center + quaternion * (new Vector3(-1f, 0f, 1f) * radiusFade)
            };

            int stride = data.stride;
            int dataLength = data.dataLength;
            float minHeight = 6f;
            int num6 = 0;
            Vector3[] array2 = array;
            Vector3 vector = default(Vector3);
            Vector3 vector2 = default(Vector3);
            foreach (Vector3 vpos in array2)
            {
                int num7 = data.QueryIndex(vpos);
                for (int j = -num4; j <= num4; j++)
                {
                    int num8 = num7 + j * stride;
                    if (num8 < 0 || num8 >= dataLength)
                    {
                        continue;
                    }
                    for (int k = -num4; k <= num4; k++)
                    {
                        int num9 = num8 + k;
                        if ((uint)num9 >= dataLength)
                        {
                            continue;
                        }
                        vector.x = vertices[num9].x * realRadius;
                        vector.y = vertices[num9].y * realRadius;
                        vector.z = vertices[num9].z * realRadius;
                        vector2.x = vector.x - center.x;
                        vector2.y = vector.y - center.y;
                        vector2.z = vector.z - center.z;

                        float squareSum = vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z;

                        if (squareSum > levelArea /*|| */)
                        {
                            continue;
                        }

                        float num10 = float.PositiveInfinity;
                        for (int l = 0; l < pointsCount; l++)
                        {
                            float num11 = points[l].x - vector.x;
                            float num12 = points[l].y - vector.y;
                            float num13 = points[l].z - vector.z;
                            float num14 = num11 * num11 + num12 * num12 + num13 * num13;
                            num10 = ((!(num10 < num14)) ? num14 : num10);
                        }
                        int num15 = 0;
                        int num16 = 0;
                        int num17 = 0;
                        num16 = data.GetModLevel(num9);


                        if (num10 <= minHeight)
                        {
                            num15 = 3;
                        }
                        else
                        {
                            num10 -= minHeight;
                            if (num10 > fade0 * fade0)
                            {
                                continue;
                            }

                            float num18 = num10 / (fade0 * fade0);

                            if (num18 <= 0.1111111f)
                            {
                                num15 = 2;
                            }
                            else if (num18 <= 0.4444444f)
                            {
                                num15 = 1;
                            }
                            else
                            {
                                if (!(num18 < 1f))
                                {
                                    continue;
                                }
                                num15 = 0;
                            }
                        }
                        num17 = num15 - num16;

                        Debug.Log("CompALT.loop.jk\tnum9: " + num9 + "\t, squareSum: " + squareSum + "\t, contains: " + planetFactory.tmp_levelChanges.ContainsKey(num9)
+ "\t, modLevel: " + data.GetModLevel(num9) + "\t, num15: " + num15 + "\t, num16: " + num16 + "\t, num17: " + num17);
                        if ( /*num15 >= num16 &&*/ num17 != 0)
                        {

                            planetFactory.tmp_levelChanges[num9] = num15;
                            float num19 = (float)(int)heightData[num9] * 0.01f;
                            float num20 = realRadius + 0.2f;
                            float num21 = num20 - num19;
                            if (num21 < 0f)
                            {
                                num21 *= 2f;
                            }
                            float f2 = 100f * (float)num17 * num21 * 0.3333333f;
                            num6 += Mathf.FloorToInt(f2);
                        }
                    }
                }
            }
            return num6;
        }

        public static void FlattenTerrainReformALT(this PlanetFactory planetFactory, Vector3 center, float radius, int reformSize, bool veinBuried, float localRadius, float realRadius, float fade0 = 3f)
        {
            if (planetFactory.tmp_ids == null)
            {
                planetFactory.tmp_ids = new int[1024];
            }
            if (planetFactory.tmp_entity_ids == null)
            {
                planetFactory.tmp_entity_ids = new int[1024];
            }
            Array.Clear(planetFactory.tmp_ids, 0, planetFactory.tmp_ids.Length);
            Array.Clear(planetFactory.tmp_entity_ids, 0, planetFactory.tmp_entity_ids.Length);
            Vector3 zero = Vector3.zero;
            PlanetRawData data = planetFactory.planet.data;
            ushort[] heightData = data.heightData;
            float f = ((float)(int)heightData[data.QueryIndex(center)] - realRadius * 100f + 20f) * 0.01f * 2f;
            //f = Mathf.Min(9f, Mathf.Abs(f));
            f = Mathf.Min(5f, Mathf.Abs(f));
            fade0 += f;
            float areaRadius = radius + fade0;
            short num = (short)(realRadius * 100f + 20f);
            bool levelized = planetFactory.planet.levelized;
            int num2 = Mathf.RoundToInt((center.magnitude - 0.2f - realRadius) / 1.33333325f);
            int num3 = num2 * 133 + num - 60;
            float num4 = localRadius * 100f - 20f;
            foreach (KeyValuePair<int, int> tmp_levelChange in planetFactory.tmp_levelChanges)
            {
                /*
                if (tmp_levelChange.Value <= 0)
                {
                    continue;
                }
                */
                ushort num5 = heightData[tmp_levelChange.Key];
                if (levelized)
                {
                    if (num5 >= num3)
                    {
                        if (data.GetModLevel(tmp_levelChange.Key) < 3)
                        {
                            data.SetModPlane(tmp_levelChange.Key, num2);
                        }
                        planetFactory.planet.AddHeightMapModLevel(tmp_levelChange.Key, tmp_levelChange.Value);
                    }
                }
                else
                {
                    Debug.Log("AddHeightMapModLevel: " + tmp_levelChange.Key + ", " + tmp_levelChange.Value);
                    planetFactory.planet.AddHeightMapModLevelALT(tmp_levelChange.Key, tmp_levelChange.Value);
                }
                if ((float)(int)num5 < num4)
                {
                    planetFactory.planet.landPercentDirty = true;
                }
            }
            if (planetFactory.planet.UpdateDirtyMeshes())
            {
                planetFactory.RenderLocalPlanetHeightmap();
            }
            radius -= (float)reformSize * 0.15f;
            NearColliderLogic nearColliderLogic = planetFactory.planet.physics.nearColliderLogic;
            int vegetablesInAreaNonAlloc = nearColliderLogic.GetVegetablesInAreaNonAlloc(center, areaRadius, planetFactory.tmp_ids);
            float num6 = radius * radius;
            for (int i = 0; i < vegetablesInAreaNonAlloc; i++)
            {
                int num7 = planetFactory.tmp_ids[i];
                zero = planetFactory.vegePool[num7].pos;
                zero -= center;
                if (zero.x * zero.x + zero.y * zero.y + zero.z * zero.z <= num6 + 2.2f)
                {
                    planetFactory.RemoveVegeWithComponents(num7);
                    continue;
                }
                float num8 = data.QueryModifiedHeight(planetFactory.vegePool[num7].pos) - 0.03f;
                planetFactory.vegePool[num7].pos = planetFactory.vegePool[num7].pos.normalized * num8;
                GameMain.gpuiManager.AlterModel(planetFactory.vegePool[num7].modelIndex, planetFactory.vegePool[num7].modelId, num7, planetFactory.vegePool[num7].pos, planetFactory.vegePool[num7].rot, setBuffer: false);
            }
            float num9 = 50f;
            vegetablesInAreaNonAlloc = ((!veinBuried) ? nearColliderLogic.GetVeinsInOceanInAreaNonAlloc(center, areaRadius, planetFactory.tmp_ids) : nearColliderLogic.GetVeinsInAreaNonAlloc(center, areaRadius, planetFactory.tmp_ids));
            for (int j = 0; j < vegetablesInAreaNonAlloc; j++)
            {
                int num10 = planetFactory.tmp_ids[j];
                zero = planetFactory.veinPool[num10].pos;
                float num11 = realRadius + 0.2f;
                Vector3 vector = zero.normalized * num11 - center;
                if (!(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z <= num6 + 2f))
                {
                    continue;
                }
                PlanetPhysics physics = planetFactory.planet.physics;
                int colliderId = planetFactory.veinPool[num10].colliderId;
                ColliderData colliderData = physics.GetColliderData(colliderId);
                if (veinBuried)
                {
                    num11 -= num9;
                }
                else
                {
                    Vector3 center2 = zero.normalized * num11;
                    int entitiesInAreaWhenReformNonAlloc = nearColliderLogic.GetEntitiesInAreaWhenReformNonAlloc(center2, colliderData.radius, planetFactory.tmp_entity_ids);
                    if (entitiesInAreaWhenReformNonAlloc > 0)
                    {
                        num11 = zero.magnitude;
                    }
                }
                Vector3 pos = colliderData.pos.normalized * num11;
                int num12 = colliderId >> 20;
                colliderId &= 0xFFFFF;
                physics.colChunks[num12].colliderPool[colliderId].pos = pos;
                planetFactory.veinPool[num10].pos = zero.normalized * num11;
                physics.RefreshColliders();
                GameMain.gpuiManager.AlterModel(planetFactory.veinPool[num10].modelIndex, planetFactory.veinPool[num10].modelId, num10, planetFactory.veinPool[num10].pos, setBuffer: false);
            }
            planetFactory.tmp_levelChanges.Clear();
            Array.Clear(planetFactory.tmp_ids, 0, planetFactory.tmp_ids.Length);
            Array.Clear(planetFactory.tmp_ids, 0, planetFactory.tmp_entity_ids.Length);
            GameMain.gpuiManager.SyncAllGPUBuffer();
        }

        public static void AddHeightMapModLevelALT(this PlanetData planetData, int index, int level)
        {
            int modLevel = (planetData.data.modData[index >> 1] >> ((index & 1) << 2)) & 3;
            Debug.Log("AddHeightMapModLevelALT. index: " + index + "\t, level: " + level + "\t, modLevel: " + modLevel);
            planetData.data.SetModLevel(index, level);
            //if (planetData.data.AddModLevel(index, level))
            {
                int num = planetData.precision / planetData.segment;
                int num2 = index % planetData.data.stride;
                int num3 = index / planetData.data.stride;
                int num4 = ((num2 >= planetData.data.substride) ? 1 : 0) + ((num3 >= planetData.data.substride) ? 2 : 0);
                int num5 = num2 % planetData.data.substride;
                int num6 = num3 % planetData.data.substride;
                int num7 = (num5 - 1) / num;
                int num8 = (num6 - 1) / num;
                int num9 = num5 / num;
                int num10 = num6 / num;
                if (num9 >= planetData.segment)
                {
                    num9 = planetData.segment - 1;
                }
                if (num10 >= planetData.segment)
                {
                    num10 = planetData.segment - 1;
                }
                int num11 = num4 * planetData.segment * planetData.segment;
                int num12 = num7 + num8 * planetData.segment + num11;
                int num13 = num9 + num8 * planetData.segment + num11;
                int num14 = num7 + num10 * planetData.segment + num11;
                int num15 = num9 + num10 * planetData.segment + num11;
                planetData.dirtyFlags[num12] = true;
                planetData.dirtyFlags[num13] = true;
                planetData.dirtyFlags[num14] = true;
                planetData.dirtyFlags[num15] = true;
            }
        }
    }
}
