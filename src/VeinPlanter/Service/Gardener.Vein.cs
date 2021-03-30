using System.Linq;
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

                short veinTypeIndex = (short)veinGroup.type;

                VeinProto veinProto = PlanetModelingManager.veinProtos[veinTypeIndex];

                var veinCursor = localPlanet.factory.veinCursor + 1;
                VeinData vein = default(VeinData);
                vein.type = veinGroup.type;
                vein.groupIndex = (short)veinGroupIndex;
                vein.modelIndex = (short)random.Next(PlanetModelingManager.veinModelIndexs[veinTypeIndex], PlanetModelingManager.veinModelIndexs[veinTypeIndex] + PlanetModelingManager.veinModelCounts[veinTypeIndex]);
                
                vein.pos = worldPos;
                vein.minerCount = 0;
                vein.colliderId = 0;

                if (veinGroup.count > 0)
                {
                    VeinData siblingvein = (from pVein in localPlanet.factory.veinPool where pVein.groupIndex == veinGroupIndex select pVein).First();
                    vein.productId = siblingvein.productId;
                    vein.amount = siblingvein.amount;
                    Debug.Log("Using sibling vein #" + siblingvein.id + " product id " + siblingvein.productId);
                } else
                {
                    vein.productId = PlanetModelingManager.veinProducts[veinTypeIndex];
                    vein.amount = Mathf.RoundToInt(1000000 * DSPGame.GameDesc.resourceMultiplier);
                }               

                veinGroup.count++;
                veinGroup.amount += vein.amount;

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

                Debug.Log("Adding new vein: " + vein.type.ToString() + " index: " + newVeinIndex + " Pos: " + vein.pos);
                Gardener.VeinGroup.UpdatePosFromChildren(veinGroupIndex);
            }

            public static void Remove(PlanetData localPlanet, int veinIndexIndex, int veinGroupIndex)
            {
                //var planetFactory = GameMain.data.factories[0];
                Debug.Assert(veinGroupIndex >= 0);
                Debug.Assert(veinIndexIndex >= 0);

                ref PlanetData.VeinGroup veinGroup = ref localPlanet.veinGroups[veinGroupIndex];
                ref VeinData vein = ref localPlanet.factory.veinPool[veinIndexIndex];

                Debug.Assert(vein.id != 0);
                Debug.Assert(vein.groupIndex == veinGroupIndex);

                veinGroup.count--;
                veinGroup.amount -= vein.amount;
                localPlanet.factory.RemoveVeinWithComponents(veinIndexIndex);

                if (veinGroup.count <= 0)
                {
                    Debug.Log("Group: " + veinGroupIndex + " has count " + veinGroup.count + ". Removing (set to 0)");
                    veinGroup.type = EVeinType.None;
                    veinGroup.amount = veinGroup.count = 0;
                }
            }
        }
    }
}
