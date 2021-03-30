

veinSpotArr



themeProto.RareVeins



Vector3 vector = default(Vector3);

vector.x = (float)random2.NextDouble() * 2f - 1f;
vector.y = (float)random2.NextDouble() - 0.5f;
vector.z = (float)random2.NextDouble() * 2f - 1f;
vector.Normalize();
vector *= (float)(random2.NextDouble() * 0.4 + 0.2);


Vector3 zero = Vector3.zero;

zero.x = (float)random2.NextDouble() * 2f - 1f;
zero.y = (float)random2.NextDouble() * 2f - 1f;
zero.z = (float)random2.NextDouble() * 2f - 1f;
if (eVeinType != EVeinType.Oil)
{
	zero += vector;
}
zero.Normalize();
float num15 = data.QueryHeight(zero);


veinVectors[veinVectorCount] = zero;
veinVectorTypes[veinVectorCount] = eVeinType;

Array.Clear(planet.veinAmounts, 0, planet.veinAmounts.Length);
data.veinCursor = 1;
planet.veinGroups = new PlanetData.VeinGroup[veinVectorCount];

VeinData vein = default(VeinData);
vein.type = EVeinType.Iron;
vein.groupIndex = (short)num18;
vein.modelIndex = (short)random2.Next(veinModelIndexs[num19], veinModelIndexs[num19] + veinModelCounts[num19]);
vein.amount = Mathf.RoundToInt((float)random2.Next(minValue, maxValue) * num10);
vein.amount = 1000000000;
vein.productId = veinProducts[num19];
vein.pos = normalized + vector6;
vein.minerCount = 0;

float num28 = data.QueryHeight(vein.pos);
data.EraseVegetableAtPoint(vein.pos);
vein.pos = vein.pos.normalized * num28;

planet.veinAmounts[(uint)eVeinType2] += vein.amount;
planet.veinGroups[num18].count++;
planet.veinGroups[num18].amount += vein.amount;
data.AddVeinData(vein);