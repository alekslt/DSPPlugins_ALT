using System;
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
                veinGroup.count = 1;
                veinGroup.pos = worldPos;
                veinGroup.type = vType;
                veinGroup.amount = 0;

                return veinGroup;
            }

            public static int GetClosestIndex(Ray ray, PlanetData localPlanet)
            {
                float realRadius = localPlanet.realRadius;

                int closestVeinGroupIndex = -1, clostestVeinId = -1;
                float clostestVeinGroupDistance = 100f;
                Vector3 vector = Vector3.zero;
                
                if (Phys.RayCastSphere(ray.origin, ray.direction, 600f, Vector3.zero, realRadius + 1f, out var rch))
                {
                    Dictionary<int, float> distMap = new Dictionary<int, float>();

                    // First pass check for vein Group. Uses veinGroups (cheaper)
                    for (int i = 0; i < localPlanet.veinGroups.Length; i++)
                    {
                        PlanetData.VeinGroup veinGroup = localPlanet.veinGroups[i];
                        float currentveinGroupDistance = Vector3.Distance(rch.point, veinGroup.pos * realRadius);
                        //Debug.Log("Comp: veinGroup: " + veinGroup.ToString() + " index: " + i + " Pos: " + veinGroup.pos.ToString() + " dist: " + currentveinGroupDistance);
                        distMap[i] = currentveinGroupDistance;
                        if (currentveinGroupDistance < clostestVeinGroupDistance)
                        {
                            clostestVeinGroupDistance = currentveinGroupDistance;
                            closestVeinGroupIndex = i;
                            vector = veinGroup.pos * (realRadius + 2.5f);
                        }
                    }

                    // Second Pass. Looks up distance to specific vein nodes.
                    var limitedCandidates = distMap.OrderBy(key => key.Value).Take(Math.Min(distMap.Count(), 5));

                    //var veinCandidates = from vg in limitedCandidates select from vp in localPlanet.factory.veinPool where vp;
                    float currentClosest = clostestVeinGroupDistance;
                    foreach (var candkv in limitedCandidates)
                    {
                        Debug.Log("Cand: VeinGroup Idx=" + candkv.Key + "\t Dist: " + candkv.Value);

                        for (int i = 1; i < localPlanet.factory.veinCursor; i++)
                        {
                            var vein = localPlanet.factory.veinPool[i];
                            if (vein.id != i || vein.groupIndex != candkv.Key) { continue; }

                            float veinDistance = Vector3.Distance(rch.point, vein.pos);
                            if (veinDistance < currentClosest)
                            {
                                currentClosest = veinDistance;
                                closestVeinGroupIndex = candkv.Key;
                                clostestVeinId = vein.id;
                            }
                        }
                    }
                    Debug.Log("Closest: VgIdx=" + closestVeinGroupIndex + " Vein Idx=" + clostestVeinId + "\t Dist: " + currentClosest);



                    // Cheat for now
                    //Assert.Equals(closestVeinGroupIndex, limitedCandidates.First().Key);
                    //closestVeinGroupIndex = candidates.First().Key;
                }
                if (closestVeinGroupIndex >= 0 && !Phys.RayCastSphere(ray.origin, ray.direction, 600f, vector, 3.5f, out rch))
                {
                    Debug.Log("Resetting to -1! ");
                    closestVeinGroupIndex = -1;
                }

                return closestVeinGroupIndex;
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
