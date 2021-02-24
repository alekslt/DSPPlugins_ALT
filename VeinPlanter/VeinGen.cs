using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VeinPlanter
{
    public class VeinGen
    {
		protected PlanetData planet;
		private Vector3[] veinVectors = new Vector3[512];

		private EVeinType[] veinVectorTypes = new EVeinType[512];

		private int veinVectorCount;

		private List<Vector2> tmp_vecs = new List<Vector2>(100);

		public virtual void GenerateVeins(bool sketchOnly)
		{
			lock (planet)
			{
				ThemeProto themeProto = LDB.themes.Select(planet.theme);
				if (themeProto == null)
				{
					return;
				}
				System.Random random = new System.Random(planet.seed);
				random.Next();
				random.Next();
				random.Next();
				random.Next();
				random.Next();
				int num = random.Next();
				System.Random random2 = new System.Random(num);
				PlanetRawData data = planet.data;
				float num2 = 2.1f / planet.radius;
				VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
				int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
				int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
				int[] veinProducts = PlanetModelingManager.veinProducts;
				int[] veinSpotArr = new int[veinProtos.Length];
				float[] veinCountArr = new float[veinProtos.Length];
				float[] veinOpacityArr = new float[veinProtos.Length];
				if (themeProto.VeinSpot != null)
				{
					Array.Copy(themeProto.VeinSpot, 0, veinSpotArr, 1, Math.Min(themeProto.VeinSpot.Length, veinSpotArr.Length - 1));
				}
				if (themeProto.VeinCount != null)
				{
					Array.Copy(themeProto.VeinCount, 0, veinCountArr, 1, Math.Min(themeProto.VeinCount.Length, veinCountArr.Length - 1));
				}
				if (themeProto.VeinOpacity != null)
				{
					Array.Copy(themeProto.VeinOpacity, 0, veinOpacityArr, 1, Math.Min(themeProto.VeinOpacity.Length, veinOpacityArr.Length - 1));
				}
				float p = 1f;
				ESpectrType spectr = planet.star.spectr;
				switch (planet.star.type)
				{
					case EStarType.MainSeqStar:
						switch (spectr)
						{
							case ESpectrType.M:
								p = 2.5f;
								break;
							case ESpectrType.K:
								p = 1f;
								break;
							case ESpectrType.G:
								p = 0.7f;
								break;
							case ESpectrType.F:
								p = 0.6f;
								break;
							case ESpectrType.A:
								p = 1f;
								break;
							case ESpectrType.B:
								p = 0.4f;
								break;
							case ESpectrType.O:
								p = 1.6f;
								break;
						}
						break;
					case EStarType.GiantStar:
						p = 2.5f;
						break;
					case EStarType.WhiteDwarf:
						{
							p = 3.5f;
							veinSpotArr[9]++;
							veinSpotArr[9]++;
							for (int j = 1; j < 12; j++)
							{
								if (random.NextDouble() >= 0.44999998807907104)
								{
									break;
								}
								veinSpotArr[9]++;
							}
							veinCountArr[9] = 0.7f;
							veinOpacityArr[9] = 1f;
							veinSpotArr[10]++;
							veinSpotArr[10]++;
							for (int k = 1; k < 12; k++)
							{
								if (random.NextDouble() >= 0.44999998807907104)
								{
									break;
								}
								veinSpotArr[10]++;
							}
							veinCountArr[10] = 0.7f;
							veinOpacityArr[10] = 1f;
							veinSpotArr[12]++;
							for (int l = 1; l < 12; l++)
							{
								if (random.NextDouble() >= 0.5)
								{
									break;
								}
								veinSpotArr[12]++;
							}
							veinCountArr[12] = 0.7f;
							veinOpacityArr[12] = 0.3f;
							break;
						}
					case EStarType.NeutronStar:
						{
							p = 4.5f;
							veinSpotArr[14]++;
							for (int m = 1; m < 12; m++)
							{
								if (random.NextDouble() >= 0.64999997615814209)
								{
									break;
								}
								veinSpotArr[14]++;
							}
							veinCountArr[14] = 0.7f;
							veinOpacityArr[14] = 0.3f;
							break;
						}
					case EStarType.BlackHole:
						{
							p = 5f;
							veinSpotArr[14]++;
							for (int i = 1; i < 12; i++)
							{
								if (random.NextDouble() >= 0.64999997615814209)
								{
									break;
								}
								veinSpotArr[14]++;
							}
							veinCountArr[14] = 0.7f;
							veinOpacityArr[14] = 0.3f;
							break;
						}
				}
				for (int n = 0; n < themeProto.RareVeins.Length; n++)
				{
					int num3 = themeProto.RareVeins[n];
					float num4 = ((planet.star.index != 0) ? themeProto.RareSettings[n * 4 + 1] : themeProto.RareSettings[n * 4]);
					float num5 = themeProto.RareSettings[n * 4 + 2];
					float num6 = themeProto.RareSettings[n * 4 + 3];
					float num7 = num6;
					num4 = 1f - Mathf.Pow(1f - num4, p);
					num6 = 1f - Mathf.Pow(1f - num6, p);
					num7 = 1f - Mathf.Pow(1f - num7, p);
					if (!(random.NextDouble() < (double)num4))
					{
						continue;
					}
					veinSpotArr[num3]++;
					veinCountArr[num3] = num6;
					veinOpacityArr[num3] = num6;
					for (int num8 = 1; num8 < 12; num8++)
					{
						if (random.NextDouble() >= (double)num5)
						{
							break;
						}
						veinSpotArr[num3]++;
					}
				}
				float num9 = planet.star.resourceCoef;
				bool flag = DSPGame.GameDesc.resourceMultiplier >= 99.5f;
				if (planet.galaxy.birthPlanetId == planet.id)
				{
					num9 *= 2f / 3f;
				}
				float num10 = 1f;
				num10 *= 1.1f;
				Array.Clear(veinVectors, 0, veinVectors.Length);
				Array.Clear(veinVectorTypes, 0, veinVectorTypes.Length);
				veinVectorCount = 0;
				Vector3 vector = default(Vector3);
				if (planet.galaxy.birthPlanetId == planet.id)
				{
					Pose pose = planet.PredictPose(120.0);
					vector = Maths.QInvRotateLF(pose.rotation, planet.star.uPosition - (VectorLF3)pose.position * 40000.0);
					vector.Normalize();
					vector *= 0.75f;
				}
				else
				{
					vector.x = (float)random2.NextDouble() * 2f - 1f;
					vector.y = (float)random2.NextDouble() - 0.5f;
					vector.z = (float)random2.NextDouble() * 2f - 1f;
					vector.Normalize();
					vector *= (float)(random2.NextDouble() * 0.4 + 0.2);
				}
				planet.veinSpotsSketch = veinSpotArr;
				if (sketchOnly)
				{
					return;
				}
				for (int num11 = 1; num11 < 15; num11++)
				{
					if (veinVectorCount >= veinVectors.Length)
					{
						break;
					}
					EVeinType eVeinType = (EVeinType)num11;
					int num12 = veinSpotArr[num11];
					if (num12 > 1)
					{
						num12 += random2.Next(-1, 2);
					}
					for (int num13 = 0; num13 < num12; num13++)
					{
						int num14 = 0;
						Vector3 zero = Vector3.zero;
						bool flag2 = false;
						while (num14++ < 200)
						{
							zero.x = (float)random2.NextDouble() * 2f - 1f;
							zero.y = (float)random2.NextDouble() * 2f - 1f;
							zero.z = (float)random2.NextDouble() * 2f - 1f;
							if (eVeinType != EVeinType.Oil)
							{
								zero += vector;
							}
							zero.Normalize();
							float num15 = data.QueryHeight(zero);
							if (num15 < planet.radius || (eVeinType == EVeinType.Oil && num15 < planet.radius + 0.5f))
							{
								continue;
							}
							bool flag3 = false;
							float num16 = ((eVeinType != EVeinType.Oil) ? 196f : 100f);
							for (int num17 = 0; num17 < veinVectorCount; num17++)
							{
								if ((veinVectors[num17] - zero).sqrMagnitude < num2 * num2 * num16)
								{
									flag3 = true;
									break;
								}
							}
							if (flag3)
							{
								continue;
							}
							flag2 = true;
							break;
						}
						if (flag2)
						{
							veinVectors[veinVectorCount] = zero;
							veinVectorTypes[veinVectorCount] = eVeinType;
							veinVectorCount++;
							if (veinVectorCount == veinVectors.Length)
							{
								break;
							}
						}
					}
				}
				Array.Clear(planet.veinAmounts, 0, planet.veinAmounts.Length);
				data.veinCursor = 1;
				planet.veinGroups = new PlanetData.VeinGroup[veinVectorCount];
				tmp_vecs.Clear();
				VeinData vein = default(VeinData);
				for (int num18 = 0; num18 < veinVectorCount; num18++)
				{
					tmp_vecs.Clear();
					Vector3 normalized = veinVectors[num18].normalized;
					EVeinType eVeinType2 = veinVectorTypes[num18];
					int num19 = (int)eVeinType2;
					Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, normalized);
					Vector3 vector2 = quaternion * Vector3.right;
					Vector3 vector3 = quaternion * Vector3.forward;
					planet.veinGroups[num18].type = eVeinType2;
					planet.veinGroups[num18].pos = normalized;
					planet.veinGroups[num18].count = 0;
					planet.veinGroups[num18].amount = 0L;
					tmp_vecs.Add(Vector2.zero);
					int num20 = Mathf.RoundToInt(veinCountArr[num19] * (float)random2.Next(20, 25));
					if (eVeinType2 == EVeinType.Oil)
					{
						num20 = 1;
					}
					int num21 = 0;
					while (num21++ < 20)
					{
						int count = tmp_vecs.Count;
						for (int num22 = 0; num22 < count; num22++)
						{
							if (tmp_vecs.Count >= num20)
							{
								break;
							}
							if (tmp_vecs[num22].sqrMagnitude > 36f)
							{
								continue;
							}
							double num23 = random2.NextDouble() * Math.PI * 2.0;
							Vector2 vector4 = new Vector2((float)Math.Cos(num23), (float)Math.Sin(num23));
							vector4 += tmp_vecs[num22] * 0.2f;
							vector4.Normalize();
							Vector2 vector5 = tmp_vecs[num22] + vector4;
							bool flag4 = false;
							for (int num24 = 0; num24 < tmp_vecs.Count; num24++)
							{
								if ((tmp_vecs[num24] - vector5).sqrMagnitude < 0.85f)
								{
									flag4 = true;
									break;
								}
							}
							if (!flag4)
							{
								tmp_vecs.Add(vector5);
							}
						}
						if (tmp_vecs.Count >= num20)
						{
							break;
						}
					}
					int num25 = Mathf.RoundToInt(veinOpacityArr[num19] * 100000f * num9);
					if (num25 < 20)
					{
						num25 = 20;
					}
					int num26 = ((num25 >= 16000) ? 15000 : Mathf.FloorToInt((float)num25 * 0.9375f));
					int minValue = num25 - num26;
					int maxValue = num25 + num26 + 1;
					for (int num27 = 0; num27 < tmp_vecs.Count; num27++)
					{
						Vector3 vector6 = (tmp_vecs[num27].x * vector2 + tmp_vecs[num27].y * vector3) * num2;
						vein.type = eVeinType2;
						vein.groupIndex = (short)num18;
						vein.modelIndex = (short)random2.Next(veinModelIndexs[num19], veinModelIndexs[num19] + veinModelCounts[num19]);
						vein.amount = Mathf.RoundToInt((float)random2.Next(minValue, maxValue) * num10);
						if (planet.veinGroups[num18].type != EVeinType.Oil)
						{
							vein.amount = Mathf.RoundToInt((float)vein.amount * DSPGame.GameDesc.resourceMultiplier);
						}
						if (vein.amount < 1)
						{
							vein.amount = 1;
						}
						if (flag && vein.type != EVeinType.Oil)
						{
							vein.amount = 1000000000;
						}
						vein.productId = veinProducts[num19];
						vein.pos = normalized + vector6;
						if (vein.type == EVeinType.Oil)
						{
							vein.pos = planet.aux.RawSnap(vein.pos);
						}
						vein.minerCount = 0;
						float num28 = data.QueryHeight(vein.pos);
						data.EraseVegetableAtPoint(vein.pos);
						vein.pos = vein.pos.normalized * num28;
						if (planet.waterItemId == 0 || !(num28 < planet.radius))
						{
							planet.veinAmounts[(uint)eVeinType2] += vein.amount;
							planet.veinGroups[num18].count++;
							planet.veinGroups[num18].amount += vein.amount;
							data.AddVeinData(vein);
						}
					}
				}
				tmp_vecs.Clear();
			}
		}
		public VeinData[] veinPool;
		public int AddVeinData(VeinData vein)
		{
			vein.id = veinCursor++;
			if (vein.id == veinCapacity)
			{
				SetVeinCapacity(veinCapacity * 2);
			}
			veinPool[vein.id] = vein;
			return vein.id;
		}
		private void SetVeinCapacity(int newCapacity)
		{
			VeinData[] array = veinPool;
			veinPool = new VeinData[newCapacity];
			if (array != null)
			{
				Array.Copy(array, veinPool, (newCapacity <= veinCapacity) ? newCapacity : veinCapacity);
			}
			veinCapacity = newCapacity;
		}

		/*
		public void Init(GameData _gameData, PlanetData _planet, int _index)
		{
			index = _index;
			gameData = _gameData;
			planet = _planet;
			planet.factory = this;
			SetEntityCapacity(1024);
			SetPrebuildCapacity(256);

			PlanetRawData data = planet.data;
			SetVeinCapacity(data.veinCursor + 2);
			veinCursor = data.veinCursor;
			Array.Copy(data.veinPool, veinPool, veinCursor);
		}
		*/
}
