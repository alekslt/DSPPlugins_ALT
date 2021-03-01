﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VeinPlanter.Service
{
    public partial class Gardener
    {
        public class VeinGroup
        {
            public static PlanetData.VeinGroup New(EVeinType vType, Vector3 worldPos)
            {
                PlanetData.VeinGroup veinGroup = new PlanetData.VeinGroup();
                veinGroup.count = 0;
                veinGroup.pos = worldPos;
                veinGroup.type = vType;
                veinGroup.amount = 0;

                return veinGroup;
            }

            public static bool GetClosestIndex(Ray ray, PlanetData localPlanet, out int closestVeinGroupIndex, out int closestVeinIndex, out float closestVeinDistance, out float closestVeinDistance2D)
            {
                float realRadius = localPlanet.realRadius;

                closestVeinGroupIndex = -1;
                closestVeinIndex = -1;
                closestVeinDistance = -1;
                closestVeinDistance2D = -1;
                float closestVeinGroupDistance = 100f;
                Vector3 vector = Vector3.zero;
                
                if (Phys.RayCastSphere(ray.origin, ray.direction, 600f, Vector3.zero, realRadius + 1f, out var rch))
                {
                    Dictionary<int, float> distMap = new Dictionary<int, float>();

                    // First pass check for vein Group. Uses veinGroups (cheaper)
                    for (int i = 0; i < localPlanet.veinGroups.Length; i++)
                    {
                        PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[i];
                        if (veinGroup.type == EVeinType.None)
                        {
                            continue;
                        }

                        float currentveinGroupDistance = Vector3.Distance(rch.point, veinGroup.pos * realRadius);
                        //Debug.Log("Comp: veinGroup: " + veinGroup.ToString() + " index: " + i + " Pos: " + veinGroup.pos.ToString() + " dist: " + currentveinGroupDistance);
                        distMap[i] = currentveinGroupDistance;
                        if (currentveinGroupDistance < closestVeinGroupDistance)
                        {
                            closestVeinGroupDistance = currentveinGroupDistance;
                            closestVeinGroupIndex = i;
                            vector = veinGroup.pos * (realRadius + 2.5f);
                        }
                    }

                    // Second Pass. Looks up distance to specific vein nodes.
                    var limitedCandidates = distMap.OrderBy(key => key.Value).Take(Math.Min(distMap.Count(), 5));

                    //var veinCandidates = from vg in limitedCandidates select from vp in localPlanet.factory.veinPool where vp;
                    closestVeinDistance = closestVeinGroupDistance;
                    closestVeinDistance2D = closestVeinGroupDistance;
                    foreach (var candkv in limitedCandidates)
                    {
                        Debug.Log("Cand: VeinGroup Idx=" + candkv.Key + "\t Dist: " + candkv.Value);

                        for (int i = 1; i < localPlanet.factory.veinCursor; i++)
                        {
                            var vein = localPlanet.factory.veinPool[i];
                            if (vein.id != i || vein.groupIndex != candkv.Key) { continue; }

                            float veinDistance = Vector3.Distance(rch.point, vein.pos);
                            if (veinDistance < closestVeinDistance)
                            {
                                closestVeinDistance = veinDistance;
                                closestVeinGroupIndex = candkv.Key;
                                closestVeinIndex = vein.id;
                            }

                            float veinDistance2D = Vector2.Distance(rch.point, vein.pos);
                            if (veinDistance2D < closestVeinDistance2D)
                            {
                                closestVeinDistance2D = veinDistance2D;
                                closestVeinGroupIndex = candkv.Key;
                                closestVeinIndex = vein.id;
                            }
                        }
                    }
                    Debug.Log("Closest: VgIdx=" + closestVeinGroupIndex + " Vein Idx=" + closestVeinIndex + "\t Dist: " + closestVeinDistance + ", Dist2D: " + closestVeinDistance2D);



                    // Cheat for now
                    //Assert.Equals(closestVeinGroupIndex, limitedCandidates.First().Key);
                    //closestVeinGroupIndex = candidates.First().Key;
                }

                // Check if there are any vein colliders inside a radius 5f sphere. 
                if (closestVeinGroupIndex >= 0)
                {
                    /*
                    if (!Phys.RayCastSphere(ray.origin, ray.direction, 600f, vector, 5f, out rch))
                    {
                        Debug.Log("Resetting to -1! ");
                        closestVeinGroupIndex = -1;
                        closestVeinIndex = -1;
                        return false;
                    }
                    */
                    if (closestVeinDistance2D < 5)
                    {
                        return true;
                    } else
                    {
                        Debug.Log("Resetting to -1! Distance is above: 5");
                        closestVeinGroupIndex = -1;
                        closestVeinIndex = -1;
                        closestVeinDistance2D = -1;
                        return false;
                    }
                    
                }
                return false;
            }

            public static void UpdatePosFromChildren(int veinGroupIndex)
            {
                ref PlanetData.VeinGroup veinGroup = ref GameMain.localPlanet.veinGroups[veinGroupIndex];
                var veinPositions = (from vein in GameMain.localPlanet.factory.veinPool
                                     where vein.groupIndex == veinGroupIndex
                                     select vein.pos).ToList();

                var averagePosition = veinPositions.Aggregate(Vector3.zero, (acc, v) => acc + v) / veinPositions.Count;
                Debug.Log("VeinGroup Pos for index=" + veinGroupIndex + " was: " + veinGroup.pos.ToString() + " is now: " + averagePosition.normalized);
                veinGroup.pos = averagePosition.normalized;
            }

            public static void ChangeType(int veinGroupIndex, EVeinType newType, PlanetData localPlanet)
            {
                ref PlanetData.VeinGroup veinGroup = ref localPlanet.veinGroups[veinGroupIndex];
                veinGroup.type = newType;

                int veinTypeIndex = (int)newType;

                for (int i = 1; i < localPlanet.factory.veinCursor; i++)
                {
                    ref VeinData vein = ref localPlanet.factory.veinPool[i];
                    ref AnimData veinAnim = ref localPlanet.factory.veinAnimPool[i];

                    // Skip invalid veins and veins from other groups.
                    if (i != vein.id
                        || vein.groupIndex != veinGroupIndex)
                    {
                        continue;
                    }

                    // Remove the old vein model instance from the GPU Instancing
                    localPlanet.factoryModel.gpuiManager.RemoveModel(vein.modelIndex, vein.modelId, setBuffer: false);

                    vein.productId = PlanetModelingManager.veinProducts[veinTypeIndex];
                    vein.type = newType;
                    vein.modelIndex = (short)random.Next(PlanetModelingManager.veinModelIndexs[veinTypeIndex],
                        PlanetModelingManager.veinModelIndexs[veinTypeIndex] + PlanetModelingManager.veinModelCounts[veinTypeIndex]);
                    vein.modelId = localPlanet.factoryModel.gpuiManager.AddModel(
                        vein.modelIndex, i, vein.pos, Maths.SphericalRotation(vein.pos,
                        UnityEngine.Random.value * 360f), setBuffer: false);

                    veinAnim.time = ((vein.amount < 20000) ? (1f - (float)vein.amount * 5E-05f) : 0f);
                    veinAnim.prepare_length = 0f;
                    veinAnim.working_length = 1f;
                    veinAnim.state = (uint)vein.type;
                    veinAnim.power = 0f;

                    GameMain.localPlanet.factory.RefreshVeinMiningDisplay(i, 0, 0);
                }
                GameMain.localPlanet.factoryModel.gpuiManager.SyncAllGPUBuffer();
                /*
                var veinPositions = (from vein in GameMain.localPlanet.factory.veinPool
                                     where vein.groupIndex == veinGroupIndex

                                     select vein.pos).ToList();
                */
            }
        }
        


    }
}
