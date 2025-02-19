using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using VoxelPaintXML;

public class VoxelPaintXMLParser
{
	private const int vegeCell0 = 6;

	private const int vegeCell1 = 2;

	private const int vegeCell2 = 1;

	private const double noiseScale = 1.0 / 32.0;

	private const float noVegeRad0 = (float)Math.PI / 4f;

	private const float noVegeRad1 = 1.0471965f;

	private const float noVegeRad2 = (float)Math.PI / 10f;

	private const float noVegeRad3 = 0.61086386f;

	private const double _1000ToHalfPi = 0.0015707963267948967;

	private const float fadeInScale = 8f;

	private const float fadeInParm = 0.3f;

	private const float startScale = 4f;

	private const float startParm = 0.7f;

	private static readonly float noVegeTan0 = (float)Math.Tan(0.7853981852531433);

	private static readonly float noVegeTan1 = (float)Math.Tan(1.0471965074539185);

	private static readonly float noVegeTan2 = (float)Math.Tan(0.3141592741012573);

	private static readonly float noVegeTan3 = (float)Math.Tan(0.6108638644218445);

	public rootCLS prms;

	private RegionDescArrayCLS curRegion;

	private HeightDescArrayCLS curHeight;

	private GradientDescArrayCLS curGradient;

	private float reqGradTan;

	private float reqHeight;

	private double reqNoise;

	private int terrainSectionsMapW;

	private int terrainSectionsMapH;

	private Color32[] terrainSectionsCols32;

	private byte defType;

	private SimplexNoise myNoise;

	private System.Random myRand;

	public int RandSeed
	{
		set
		{
			myRand = new System.Random(value);
		}
	}

	public static int MapTypeToRegionId(RandomMapType type)
	{
		return (int)(type - 1);
	}

	public void LoadXMLInResources(string xmlPath, string regionMapFolder, SimplexNoise noise, System.Random rand)
	{
		TextAsset textAsset = Resources.Load(xmlPath) as TextAsset;
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(rootCLS));
		StringReader stringReader = new StringReader(textAsset.text);
		if (stringReader != null)
		{
			prms = (rootCLS)xmlSerializer.Deserialize(stringReader);
			stringReader.Close();
			Grad1000ToTanAndSetHeight();
			Texture2D texture2D = Resources.Load(regionMapFolder + prms.regionsMap) as Texture2D;
			if (texture2D != null)
			{
				terrainSectionsMapW = texture2D.width;
				terrainSectionsMapH = texture2D.height;
				terrainSectionsCols32 = texture2D.GetPixels32();
			}
			else
			{
				terrainSectionsCols32 = null;
				Debug.LogWarning("VoxelPaintXMLParser: No sectionmap found.");
			}
			curRegion = prms.RegionDescArrayValues[0];
			curHeight = curRegion.HeightDescArrayValues[0];
			curGradient = curHeight.GradientDescArrayValues[0];
			defType = 3;
			myNoise = noise;
			myRand = rand;
		}
	}

	public void LoadXMLInResources(string xmlPath, string regionMapFolder, int randSeed)
	{
		LoadXMLInResources(xmlPath, regionMapFolder, new SimplexNoise(randSeed), new System.Random(randSeed));
	}

	private void Grad1000ToTan()
	{
		RegionDescArrayCLS[] regionDescArrayValues = prms.RegionDescArrayValues;
		foreach (RegionDescArrayCLS regionDescArrayCLS in regionDescArrayValues)
		{
			HeightDescArrayCLS[] heightDescArrayValues = regionDescArrayCLS.HeightDescArrayValues;
			foreach (HeightDescArrayCLS heightDescArrayCLS in heightDescArrayValues)
			{
				GradientDescArrayCLS[] gradientDescArrayValues = heightDescArrayCLS.GradientDescArrayValues;
				foreach (GradientDescArrayCLS gradientDescArrayCLS in gradientDescArrayValues)
				{
					gradientDescArrayCLS.startTan = (float)Math.Tan((double)gradientDescArrayCLS.start * 0.0015707963267948967);
					gradientDescArrayCLS.endTan = (float)Math.Tan((double)gradientDescArrayCLS.end * 0.0015707963267948967);
				}
			}
			if (regionDescArrayCLS.trees != null)
			{
				PlantHeightDesc[] plantHeightDescValues = regionDescArrayCLS.trees.PlantHeightDescValues;
				foreach (PlantHeightDesc plantHeightDesc in plantHeightDescValues)
				{
					PlantGradientDesc[] plantGradientDescValues = plantHeightDesc.PlantGradientDescValues;
					foreach (PlantGradientDesc plantGradientDesc in plantGradientDescValues)
					{
						plantGradientDesc.startTan = (float)Math.Tan((double)plantGradientDesc.start * 0.0015707963267948967);
						plantGradientDesc.endTan = (float)Math.Tan((double)plantGradientDesc.end * 0.0015707963267948967);
					}
				}
			}
			if (regionDescArrayCLS.grasses != null)
			{
				PlantHeightDesc[] plantHeightDescValues2 = regionDescArrayCLS.grasses.PlantHeightDescValues;
				foreach (PlantHeightDesc plantHeightDesc2 in plantHeightDescValues2)
				{
					PlantGradientDesc[] plantGradientDescValues2 = plantHeightDesc2.PlantGradientDescValues;
					foreach (PlantGradientDesc plantGradientDesc2 in plantGradientDescValues2)
					{
						plantGradientDesc2.startTan = (float)Math.Tan((double)plantGradientDesc2.start * 0.0015707963267948967);
						plantGradientDesc2.endTan = (float)Math.Tan((double)plantGradientDesc2.end * 0.0015707963267948967);
					}
				}
			}
			if (regionDescArrayCLS.newgrasses == null)
			{
				continue;
			}
			PlantHeightDesc[] plantHeightDescValues3 = regionDescArrayCLS.newgrasses.PlantHeightDescValues;
			foreach (PlantHeightDesc plantHeightDesc3 in plantHeightDescValues3)
			{
				PlantGradientDesc[] plantGradientDescValues3 = plantHeightDesc3.PlantGradientDescValues;
				foreach (PlantGradientDesc plantGradientDesc3 in plantGradientDescValues3)
				{
					plantGradientDesc3.startTan = (float)Math.Tan((double)plantGradientDesc3.start * 0.0015707963267948967);
					plantGradientDesc3.endTan = (float)Math.Tan((double)plantGradientDesc3.end * 0.0015707963267948967);
				}
			}
		}
	}

	private void Grad1000ToTanAndSetHeight()
	{
		RegionDescArrayCLS[] regionDescArrayValues = prms.RegionDescArrayValues;
		foreach (RegionDescArrayCLS regionDescArrayCLS in regionDescArrayValues)
		{
			HeightDescArrayCLS[] heightDescArrayValues = regionDescArrayCLS.HeightDescArrayValues;
			foreach (HeightDescArrayCLS heightDescArrayCLS in heightDescArrayValues)
			{
				if (heightDescArrayCLS.start == 0)
				{
					heightDescArrayCLS.end += (int)(VFDataRTGen.waterHeight - 3.9f);
				}
				else if (heightDescArrayCLS.start == 2 && VFDataRTGen.waterHeight > 7f)
				{
					heightDescArrayCLS.start += (int)(VFDataRTGen.waterHeight - 3.9f);
					heightDescArrayCLS.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
				}
				else if (heightDescArrayCLS.start != 2 && VFDataRTGen.waterHeight > 7f)
				{
					heightDescArrayCLS.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
					heightDescArrayCLS.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
				}
				else
				{
					heightDescArrayCLS.start += (int)(VFDataRTGen.waterHeight - 3.9f);
					heightDescArrayCLS.end += (int)(VFDataRTGen.waterHeight - 3.9f);
				}
				GradientDescArrayCLS[] gradientDescArrayValues = heightDescArrayCLS.GradientDescArrayValues;
				foreach (GradientDescArrayCLS gradientDescArrayCLS in gradientDescArrayValues)
				{
					gradientDescArrayCLS.startTan = (float)Math.Tan((double)gradientDescArrayCLS.start * 0.0015707963267948967);
					gradientDescArrayCLS.endTan = (float)Math.Tan((double)gradientDescArrayCLS.end * 0.0015707963267948967);
				}
			}
			if (regionDescArrayCLS.trees != null)
			{
				PlantHeightDesc[] plantHeightDescValues = regionDescArrayCLS.trees.PlantHeightDescValues;
				foreach (PlantHeightDesc plantHeightDesc in plantHeightDescValues)
				{
					if (VFDataRTGen.waterHeight > 7f)
					{
						if (plantHeightDesc.start == 3)
						{
							plantHeightDesc.start += (int)(VFDataRTGen.waterHeight - 3.9f);
							plantHeightDesc.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
						}
						else
						{
							plantHeightDesc.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
							plantHeightDesc.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
						}
					}
					else
					{
						plantHeightDesc.start += (int)(VFDataRTGen.waterHeight - 3.9f);
						plantHeightDesc.end += (int)(VFDataRTGen.waterHeight - 3.9f);
					}
					PlantGradientDesc[] plantGradientDescValues = plantHeightDesc.PlantGradientDescValues;
					foreach (PlantGradientDesc plantGradientDesc in plantGradientDescValues)
					{
						plantGradientDesc.startTan = (float)Math.Tan((double)plantGradientDesc.start * 0.0015707963267948967);
						plantGradientDesc.endTan = (float)Math.Tan((double)plantGradientDesc.end * 0.0015707963267948967);
					}
				}
			}
			if (regionDescArrayCLS.grasses != null)
			{
				PlantHeightDesc[] plantHeightDescValues2 = regionDescArrayCLS.grasses.PlantHeightDescValues;
				foreach (PlantHeightDesc plantHeightDesc2 in plantHeightDescValues2)
				{
					if (VFDataRTGen.waterHeight > 7f)
					{
						if (plantHeightDesc2.start == 3)
						{
							plantHeightDesc2.start += (int)(VFDataRTGen.waterHeight - 3.9f);
							plantHeightDesc2.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
						}
						else
						{
							plantHeightDesc2.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
							plantHeightDesc2.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
						}
					}
					else
					{
						plantHeightDesc2.start += (int)(VFDataRTGen.waterHeight - 3.9f);
						plantHeightDesc2.end += (int)(VFDataRTGen.waterHeight - 3.9f);
					}
					PlantGradientDesc[] plantGradientDescValues2 = plantHeightDesc2.PlantGradientDescValues;
					foreach (PlantGradientDesc plantGradientDesc2 in plantGradientDescValues2)
					{
						plantGradientDesc2.startTan = (float)Math.Tan((double)plantGradientDesc2.start * 0.0015707963267948967);
						plantGradientDesc2.endTan = (float)Math.Tan((double)plantGradientDesc2.end * 0.0015707963267948967);
					}
				}
			}
			if (regionDescArrayCLS.newgrasses == null)
			{
				continue;
			}
			PlantHeightDesc[] plantHeightDescValues3 = regionDescArrayCLS.newgrasses.PlantHeightDescValues;
			foreach (PlantHeightDesc plantHeightDesc3 in plantHeightDescValues3)
			{
				if (VFDataRTGen.waterHeight > 7f)
				{
					if (plantHeightDesc3.start == 3)
					{
						plantHeightDesc3.start += (int)(VFDataRTGen.waterHeight - 3.9f);
						plantHeightDesc3.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
					}
					else
					{
						plantHeightDesc3.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
						plantHeightDesc3.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
					}
				}
				else
				{
					plantHeightDesc3.start += (int)(VFDataRTGen.waterHeight - 3.9f);
					plantHeightDesc3.end += (int)(VFDataRTGen.waterHeight - 3.9f);
				}
				PlantGradientDesc[] plantGradientDescValues3 = plantHeightDesc3.PlantGradientDescValues;
				foreach (PlantGradientDesc plantGradientDesc3 in plantGradientDescValues3)
				{
					plantGradientDesc3.startTan = (float)Math.Tan((double)plantGradientDesc3.start * 0.0015707963267948967);
					plantGradientDesc3.endTan = (float)Math.Tan((double)plantGradientDesc3.end * 0.0015707963267948967);
				}
			}
		}
	}

	public double GetNoise(int noisePosX, int noisePosZ)
	{
		return myNoise.Noise((double)noisePosX * (1.0 / 32.0), (double)noisePosZ * (1.0 / 32.0));
	}

	private byte GetVoxelTypeFromCurGradient()
	{
		int num = (int)((reqNoise + 1.0) * 50.0);
		for (int i = 0; i < curGradient.VoxelRateArrayValues.Length; i++)
		{
			VoxelRateArrayCLS voxelRateArrayCLS = curGradient.VoxelRateArrayValues[i];
			if (num <= voxelRateArrayCLS.perc)
			{
				return (byte)voxelRateArrayCLS.type;
			}
		}
		return defType;
	}

	private byte GetVoxelTypeFromCurHeight()
	{
		for (int i = 0; i < curHeight.GradientDescArrayValues.Length; i++)
		{
			GradientDescArrayCLS gradientDescArrayCLS = curHeight.GradientDescArrayValues[i];
			if (gradientDescArrayCLS.startTan <= reqGradTan && gradientDescArrayCLS.endTan > reqGradTan)
			{
				curGradient = gradientDescArrayCLS;
				return GetVoxelTypeFromCurGradient();
			}
		}
		return defType;
	}

	private byte GetVoxelTypeFromCurRegion()
	{
		for (int i = 0; i < curRegion.HeightDescArrayValues.Length; i++)
		{
			HeightDescArrayCLS heightDescArrayCLS = curRegion.HeightDescArrayValues[i];
			if ((float)heightDescArrayCLS.start <= reqHeight && (float)heightDescArrayCLS.end > reqHeight)
			{
				curHeight = heightDescArrayCLS;
				return GetVoxelTypeFromCurHeight();
			}
		}
		return defType;
	}

	public byte GetVoxelType(float gradTan, float height, double noise, int idxRegion)
	{
		if (gradTan > 999f)
		{
			Debug.LogError("gradTan:" + gradTan);
			gradTan = 999f;
		}
		if (gradTan < 0f)
		{
			Debug.LogError("gradTan:" + gradTan);
			gradTan = 0f;
		}
		reqGradTan = gradTan;
		reqHeight = height;
		reqNoise = noise;
		curRegion = prms.RegionDescArrayValues[idxRegion];
		return GetVoxelTypeFromCurRegion();
	}

	public byte GetVoxelType(float gradTan, float height, int posX, int posZ)
	{
		int num = 0;
		bool flag = true;
		reqGradTan = gradTan;
		reqHeight = height;
		reqNoise = GetNoise(posX, posZ);
		if (curRegion.color != null)
		{
			int num2 = (int)((float)(posX * terrainSectionsMapW) / 18432f);
			int num3 = (int)((float)(posZ * terrainSectionsMapW) / 18432f);
			Color32 color = terrainSectionsCols32[num3 * terrainSectionsMapW + num2];
			num = (color.a << 24) | (color.r << 16) | (color.g << 8) | color.b;
			flag = Convert.ToInt32(curRegion.color, 16) == num;
		}
		if (flag)
		{
			if ((float)curHeight.start <= reqHeight && (float)curHeight.end > reqHeight)
			{
				if (curGradient.startTan <= reqGradTan && curGradient.endTan > reqGradTan)
				{
					return GetVoxelTypeFromCurGradient();
				}
				return GetVoxelTypeFromCurHeight();
			}
			return GetVoxelTypeFromCurRegion();
		}
		RegionDescArrayCLS[] regionDescArrayValues = prms.RegionDescArrayValues;
		foreach (RegionDescArrayCLS regionDescArrayCLS in regionDescArrayValues)
		{
			if (Convert.ToInt32(regionDescArrayCLS.color, 16) == num)
			{
				curRegion = regionDescArrayCLS;
				return GetVoxelTypeFromCurRegion();
			}
		}
		Debug.Log("Unrecognized region color------");
		return defType;
	}

	public byte GetVxMatByGradient(float h, float hXL, float hXR, float hZL, float hZR, int posX, int posZ)
	{
		float num = hXR - hXL;
		float num2 = hZR - hZL;
		double num3 = Math.Sqrt(num * num + num2 * num2) * 0.5;
		return GetVoxelType((float)num3, h, posX, posZ);
	}

	private void PlantAVegetation(int startX, int startZ, int iX, int iZ, int iScl, PlantDescCLS[] pVeges, float[][] tileHeightBuf, float posVar, List<TreeInfo> outlstTreeInfo)
	{
		float num = (float)iX + ((float)myRand.NextDouble() - 0.5f) * posVar;
		if (num < 0f)
		{
			return;
		}
		float num2 = (float)iZ + ((float)myRand.NextDouble() - 0.5f) * posVar;
		if (num2 < 0f)
		{
			return;
		}
		int num3 = (int)(num + 0.5f) + 1;
		int num4 = (int)(num2 + 0.5f) + 1;
		float num5 = tileHeightBuf[num4][num3];
		if (!(num5 < 4f))
		{
			int num6 = pVeges.Length;
			int i = 0;
			for (int num7 = myRand.Next(100); i < num6 && pVeges[i].pct < (float)num7; i++)
			{
			}
			if (i < num6)
			{
				TreeInfo tI = TreeInfo.GetTI();
				tI.m_clr = Color.white;
				tI.m_lightMapClr = Color.white;
				tI.m_protoTypeIdx = pVeges[i].idx;
				tI.m_pos = new Vector3(num * (float)iScl + (float)startX, num5, num2 * (float)iScl + (float)startZ);
				tI.m_heightScale = pVeges[i].hScaleMin + (float)myRand.NextDouble() * (pVeges[i].hScaleMax - pVeges[i].hScaleMin);
				tI.m_widthScale = pVeges[i].wScaleMin + (float)myRand.NextDouble() * (pVeges[i].wScaleMax - pVeges[i].wScaleMin);
				outlstTreeInfo.Add(tI);
			}
		}
	}

	private void PlantANewGrass(int startX, int startZ, int iX, int iZ, float fDensity, PlantDescCLS[] pVeges, float[][] tileHeightBuf, List<VoxelGrassInstance> outlstGrassInst)
	{
		int num = iX + 1;
		int num2 = iZ + 1;
		float num3 = tileHeightBuf[num2][num];
		if (!(num3 < 4f))
		{
			float num4 = tileHeightBuf[num2 - 1][num];
			float num5 = tileHeightBuf[num2 + 1][num];
			float num6 = tileHeightBuf[num2][num - 1];
			float num7 = tileHeightBuf[num2][num + 1];
			float num8 = num7 - num6;
			float z = num5 - num4;
			int num9 = pVeges.Length;
			int i = 0;
			for (int num10 = myRand.Next(100); i < num9 && pVeges[i].pct < (float)num10; i++)
			{
			}
			if (i < num9)
			{
				VoxelGrassInstance item = default(VoxelGrassInstance);
				item.Position = new Vector3(startX + iX, num3, startZ + iZ);
				item.Density = fDensity;
				item.Normal = new Vector3(0f - num8, 2f, z);
				item.ColorDw = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				item.Prototype = pVeges[i].idx;
				outlstGrassInst.Add(item);
			}
		}
	}

	public void PlantTrees(VFTile terTile, double[][] tileNoiseBuf, float[][] tileHeightBuf, float[][] tileGradTanBuf, List<TreeInfo> outlstTreeInfo, RandomMapType[][] tileMapType, int szCell)
	{
		int startX = terTile.tileX << 5;
		int startZ = terTile.tileZ << 5;
		int num = 1 << terTile.tileL;
		for (int i = 0; i < 32; i += szCell)
		{
			int num2 = i + 1;
			double[] array = tileNoiseBuf[num2];
			float[] array2 = tileHeightBuf[num2];
			float[] array3 = tileGradTanBuf[num2];
			byte[][] array4 = terTile.terraVoxels[num2];
			for (int j = 0; j < 32; j += szCell)
			{
				int num3 = j + 1;
				float num4 = array2[num3];
				byte[] array5 = array4[num3];
				byte vType = array5[(int)num4 * 2 + 1];
				if (VFDataRTGen.IsNoPlantType(vType))
				{
					continue;
				}
				float num5 = array3[num3];
				if (num5 >= noVegeTan1)
				{
					continue;
				}
				double num6 = (array[num3] + 1.0) * 128.0;
				curRegion = prms.RegionDescArrayValues[MapTypeToRegionId(tileMapType[i][j])];
				if (num5 < noVegeTan0)
				{
					PlantDescArrayCLS trees = curRegion.trees;
					if (trees != null && (float)(j * num) % trees.cellSize == 0f && (float)(i * num) % trees.cellSize == 0f && (num6 >= (double)trees.start || (num6 > (double)trees.startFadeIn && myRand.NextDouble() <= (num6 - (double)trees.startFadeIn) / (double)(trees.start - trees.startFadeIn))))
					{
						PlantHeightDesc plantHeightDesc = null;
						for (int k = 0; k < trees.PlantHeightDescValues.Length; k++)
						{
							PlantHeightDesc plantHeightDesc2 = trees.PlantHeightDescValues[k];
							if ((float)plantHeightDesc2.start <= num4 && (float)plantHeightDesc2.end > num4)
							{
								plantHeightDesc = plantHeightDesc2;
							}
						}
						if (plantHeightDesc != null)
						{
							PlantGradientDesc plantGradientDesc = null;
							for (int l = 0; l < plantHeightDesc.PlantGradientDescValues.Length; l++)
							{
								PlantGradientDesc plantGradientDesc2 = plantHeightDesc.PlantGradientDescValues[l];
								if ((float)plantGradientDesc2.start <= num5 && (float)plantGradientDesc2.end > num5)
								{
									plantGradientDesc = plantGradientDesc2;
								}
							}
							PlantAVegetation(startX, startZ, j, i, num, plantGradientDesc.PlantDescArrayValues, tileHeightBuf, 5f, outlstTreeInfo);
						}
					}
				}
				if (!(num5 < noVegeTan3))
				{
					continue;
				}
				PlantDescArrayCLS grasses = curRegion.grasses;
				if (grasses == null || (float)(j * num) % grasses.cellSize != 0f || (float)(i * num) % grasses.cellSize != 0f || (!(num6 >= (double)grasses.start) && (!(num6 > (double)grasses.startFadeIn) || !(myRand.NextDouble() <= (num6 - (double)grasses.startFadeIn) / (double)(grasses.start - grasses.startFadeIn)))))
				{
					continue;
				}
				PlantHeightDesc plantHeightDesc3 = null;
				PlantHeightDesc[] plantHeightDescValues = grasses.PlantHeightDescValues;
				foreach (PlantHeightDesc plantHeightDesc4 in plantHeightDescValues)
				{
					if ((float)plantHeightDesc4.start <= num4 && (float)plantHeightDesc4.end > num4)
					{
						plantHeightDesc3 = plantHeightDesc4;
					}
				}
				if (plantHeightDesc3 == null)
				{
					continue;
				}
				PlantGradientDesc plantGradientDesc3 = null;
				PlantGradientDesc[] plantGradientDescValues = plantHeightDesc3.PlantGradientDescValues;
				foreach (PlantGradientDesc plantGradientDesc4 in plantGradientDescValues)
				{
					if ((float)plantGradientDesc4.start <= num5 && (float)plantGradientDesc4.end > num5)
					{
						plantGradientDesc3 = plantGradientDesc4;
					}
				}
				if (plantGradientDesc3 != null)
				{
					PlantAVegetation(startX, startZ, j, i, num, plantGradientDesc3.PlantDescArrayValues, tileHeightBuf, 1.7f, outlstTreeInfo);
				}
			}
		}
	}

	public void PlantGrass(VFTile terTile, double[][] tileNoiseBuf, float[][] tileHeightBuf, float[][] tileGradTanBuf, List<VoxelGrassInstance> outlstGrassInst, RandomMapType[][] tileMapType, int szCell)
	{
		int startX = terTile.tileX << 5;
		int startZ = terTile.tileZ << 5;
		int num = 1 << terTile.tileL;
		for (int i = 0; i < 32; i += szCell)
		{
			int num2 = i + 1;
			double[] array = tileNoiseBuf[num2];
			float[] array2 = tileHeightBuf[num2];
			float[] array3 = tileGradTanBuf[num2];
			byte[][] array4 = terTile.terraVoxels[num2];
			for (int j = 0; j < 32; j += szCell)
			{
				int num3 = j + 1;
				float num4 = array2[num3];
				byte[] array5 = array4[num3];
				byte vType = array5[(int)num4 * 2 + 1];
				if (VFDataRTGen.IsNoPlantType(vType))
				{
					continue;
				}
				float num5 = array3[num3];
				if (num5 >= noVegeTan1 || num5 >= noVegeTan2)
				{
					continue;
				}
				curRegion = prms.RegionDescArrayValues[MapTypeToRegionId(tileMapType[i][j])];
				PlantDescArrayCLS newgrasses = curRegion.newgrasses;
				if (newgrasses == null || (float)j % newgrasses.cellSize != 0f || (float)i % newgrasses.cellSize != 0f)
				{
					continue;
				}
				double num6 = (array[num3] + 1.0) * 128.0;
				if (!(num6 >= (double)newgrasses.start) && (!(num6 > (double)newgrasses.startFadeIn) || !(myRand.NextDouble() <= (num6 - (double)newgrasses.startFadeIn) / (double)(newgrasses.start - newgrasses.startFadeIn))))
				{
					continue;
				}
				PlantHeightDesc plantHeightDesc = null;
				PlantGradientDesc plantGradientDesc = null;
				PlantHeightDesc[] plantHeightDescValues = newgrasses.PlantHeightDescValues;
				foreach (PlantHeightDesc plantHeightDesc2 in plantHeightDescValues)
				{
					if ((float)plantHeightDesc2.start <= num4 && (float)plantHeightDesc2.end > num4)
					{
						plantHeightDesc = plantHeightDesc2;
					}
				}
				if (plantHeightDesc == null)
				{
					continue;
				}
				PlantGradientDesc[] plantGradientDescValues = plantHeightDesc.PlantGradientDescValues;
				foreach (PlantGradientDesc plantGradientDesc2 in plantGradientDescValues)
				{
					if ((float)plantGradientDesc2.start <= num5 && (float)plantGradientDesc2.end > num5)
					{
						plantGradientDesc = plantGradientDesc2;
					}
				}
				if (plantGradientDesc != null)
				{
					float num7 = ((float)num6 - newgrasses.startFadeIn) / (newgrasses.start - newgrasses.startFadeIn);
					if (num7 > 1f)
					{
						num7 = 1f;
					}
					PlantANewGrass(startX, startZ, j, i, num7, plantGradientDesc.PlantDescArrayValues, tileHeightBuf, outlstGrassInst);
				}
			}
		}
	}

	public void PlantVegetation(VFTile terTile, double[][] tileNoiseBuf, float[][] tileHeightBuf, float[][] tileGradTanBuf, List<TreeInfo> outlstTreeInfo, List<VoxelGrassInstance> outlstGrassInst, RandomMapType[][] tileMapType, int szCell)
	{
		int startX = terTile.tileX << 5;
		int startZ = terTile.tileZ << 5;
		int iScl = 1 << terTile.tileL;
		for (int i = 0; i < 32; i += szCell)
		{
			int num = i + 1;
			double[] array = tileNoiseBuf[num];
			float[] array2 = tileHeightBuf[num];
			float[] array3 = tileGradTanBuf[num];
			byte[][] array4 = terTile.terraVoxels[num];
			for (int j = 0; j < 32; j += szCell)
			{
				int num2 = j + 1;
				float num3 = array2[num2];
				byte[] array5 = array4[num2];
				byte vType = array5[(int)num3 * 2 + 1];
				if (VFDataRTGen.IsNoPlantType(vType))
				{
					continue;
				}
				curRegion = prms.RegionDescArrayValues[MapTypeToRegionId(tileMapType[i][j])];
				PlantDescArrayCLS trees = curRegion.trees;
				PlantDescArrayCLS grasses = curRegion.grasses;
				PlantDescArrayCLS newgrasses = curRegion.newgrasses;
				PlantHeightDesc plantHeightDesc = null;
				PlantHeightDesc plantHeightDesc2 = null;
				PlantHeightDesc plantHeightDesc3 = null;
				PlantGradientDesc plantGradientDesc = null;
				PlantGradientDesc plantGradientDesc2 = null;
				PlantGradientDesc plantGradientDesc3 = null;
				double num4 = (array[num2] + 1.0) * 128.0;
				float num5 = array3[num2];
				if (num5 >= noVegeTan1)
				{
					continue;
				}
				if (num5 < noVegeTan0 && trees != null && (float)j % trees.cellSize == 0f && (float)i % trees.cellSize == 0f)
				{
					bool flag = false;
					if (num4 >= (double)trees.start || (num4 > (double)trees.startFadeIn && myRand.NextDouble() <= (num4 - (double)trees.startFadeIn) / (double)(trees.start - trees.startFadeIn)))
					{
						flag = true;
					}
					if (flag)
					{
						for (int k = 0; k < trees.PlantHeightDescValues.Length; k++)
						{
							PlantHeightDesc plantHeightDesc4 = trees.PlantHeightDescValues[k];
							if ((float)plantHeightDesc4.start <= num3 && (float)plantHeightDesc4.end > num3)
							{
								plantHeightDesc = plantHeightDesc4;
							}
						}
						if (plantHeightDesc != null)
						{
							for (int l = 0; l < plantHeightDesc.PlantGradientDescValues.Length; l++)
							{
								PlantGradientDesc plantGradientDesc4 = plantHeightDesc.PlantGradientDescValues[l];
								if ((float)plantGradientDesc4.start <= num5 && (float)plantGradientDesc4.end > num5)
								{
									plantGradientDesc = plantGradientDesc4;
								}
							}
							PlantAVegetation(startX, startZ, j, i, iScl, plantGradientDesc.PlantDescArrayValues, tileHeightBuf, 5f, outlstTreeInfo);
						}
					}
				}
				if (num5 < noVegeTan3 && grasses != null && (float)j % grasses.cellSize == 0f && (float)i % grasses.cellSize == 0f && (num4 >= (double)grasses.start || (num4 > (double)grasses.startFadeIn && myRand.NextDouble() <= (num4 - (double)grasses.startFadeIn) / (double)(grasses.start - grasses.startFadeIn))))
				{
					PlantHeightDesc[] plantHeightDescValues = grasses.PlantHeightDescValues;
					foreach (PlantHeightDesc plantHeightDesc5 in plantHeightDescValues)
					{
						if ((float)plantHeightDesc5.start <= num3 && (float)plantHeightDesc5.end > num3)
						{
							plantHeightDesc2 = plantHeightDesc5;
						}
					}
					if (plantHeightDesc2 != null)
					{
						PlantGradientDesc[] plantGradientDescValues = plantHeightDesc2.PlantGradientDescValues;
						foreach (PlantGradientDesc plantGradientDesc5 in plantGradientDescValues)
						{
							if ((float)plantGradientDesc5.start <= num5 && (float)plantGradientDesc5.end > num5)
							{
								plantGradientDesc2 = plantGradientDesc5;
							}
						}
						if (plantGradientDesc2 != null)
						{
							PlantAVegetation(startX, startZ, j, i, iScl, plantGradientDesc2.PlantDescArrayValues, tileHeightBuf, 1.7f, outlstTreeInfo);
						}
					}
				}
				if (num5 >= noVegeTan2 || newgrasses == null || (float)j % newgrasses.cellSize != 0f || (float)i % newgrasses.cellSize != 0f || (!(num4 >= (double)newgrasses.start) && (!(num4 > (double)newgrasses.startFadeIn) || !(myRand.NextDouble() <= (num4 - (double)newgrasses.startFadeIn) / (double)(newgrasses.start - newgrasses.startFadeIn)))))
				{
					continue;
				}
				PlantHeightDesc[] plantHeightDescValues2 = newgrasses.PlantHeightDescValues;
				foreach (PlantHeightDesc plantHeightDesc6 in plantHeightDescValues2)
				{
					if ((float)plantHeightDesc6.start <= num3 && (float)plantHeightDesc6.end > num3)
					{
						plantHeightDesc3 = plantHeightDesc6;
					}
				}
				if (plantHeightDesc3 == null)
				{
					continue;
				}
				PlantGradientDesc[] plantGradientDescValues2 = plantHeightDesc3.PlantGradientDescValues;
				foreach (PlantGradientDesc plantGradientDesc6 in plantGradientDescValues2)
				{
					if ((float)plantGradientDesc6.start <= num5 && (float)plantGradientDesc6.end > num5)
					{
						plantGradientDesc3 = plantGradientDesc6;
					}
				}
				if (plantGradientDesc3 != null)
				{
					float num8 = ((float)num4 - newgrasses.startFadeIn) / (trees.start - trees.startFadeIn);
					if (num8 > 1f)
					{
						num8 = 1f;
					}
					PlantANewGrass(startX, startZ, j, i, num8, plantGradientDesc3.PlantDescArrayValues, tileHeightBuf, outlstGrassInst);
				}
			}
		}
	}
}
