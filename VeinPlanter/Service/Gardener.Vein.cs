using UnityEngine;

namespace VeinPlanter.Service
{
    public partial class Gardener
    {
        public class Vein
        {
            /*
            public static void Add(Vector3 worldPos)
            {

            }*/

            public static void Add(PlanetData localPlanet, Vector3 worldPos, int veinGroupIndex)
            {
                //var planetFactory = GameMain.data.factories[0];
                Debug.Assert(veinGroupIndex >= 0);

                ref PlanetData.VeinGroup veinGroup = ref localPlanet.veinGroups[veinGroupIndex];

                short veinTypeIndex = (int)EVeinType.Iron;

                VeinProto veinProto = PlanetModelingManager.veinProtos[veinTypeIndex];

                var veinCursor = localPlanet.factory.veinCursor + 1;
                VeinData vein = default(VeinData);
                vein.type = EVeinType.Iron;
                vein.groupIndex = (short)veinGroupIndex;
                vein.modelIndex = (short)random.Next(PlanetModelingManager.veinModelIndexs[veinTypeIndex], PlanetModelingManager.veinModelIndexs[veinTypeIndex] + PlanetModelingManager.veinModelCounts[veinTypeIndex]);
                vein.amount = 100;
                vein.productId = PlanetModelingManager.veinProducts[veinTypeIndex];
                vein.pos = worldPos;
                vein.minerCount = 0;
                vein.colliderId = 0;

                vein.amount = Mathf.RoundToInt(vein.amount * DSPGame.GameDesc.resourceMultiplier);

                localPlanet.veinAmounts[veinTypeIndex] += vein.amount;
                int newVeinIndex = localPlanet.factory.AddVeinData(vein);
                localPlanet.factory.veinPool[newVeinIndex].modelId = localPlanet.factoryModel.gpuiManager.AddModel(vein.modelIndex, newVeinIndex, vein.pos, Maths.SphericalRotation(vein.pos, UnityEngine.Random.value * 360f), setBuffer: false);

                ColliderData[] colliders2 = veinProto.prefabDesc.colliders;
                int num2 = 0;
                while (colliders2 != null && num2 < colliders2.Length)
                {
                    localPlanet.factory.veinPool[newVeinIndex].colliderId = localPlanet.physics.AddColliderData(colliders2[num2].BindToObject(newVeinIndex, vein.colliderId, EObjectType.Vein, vein.pos, Quaternion.FromToRotation(Vector3.up, vein.pos.normalized)));
                    num2++;
                }
                localPlanet.factory.RefreshVeinMiningDisplay(newVeinIndex, 0, 0);
                localPlanet.factory.planet.factoryModel.gpuiManager.SyncAllGPUBuffer();

                Gardener.VeinGroup.UpdatePosFromChildren(veinGroupIndex);
            }
        }
        


    }
}
