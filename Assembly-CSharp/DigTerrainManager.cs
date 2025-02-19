using System.Collections.Generic;
using System.IO;
using ItemAsset;
using NaturalResAsset;
using Pathea;
using PETools;
using SkillAsset;
using SkillSystem;
using UnityEngine;

public class DigTerrainManager
{
	private struct Tuple<T1, T2>
	{
		public T1 v1;

		public T2 v2;
	}

	public delegate void DigTerrainDel(IntVector3 pos);

	public delegate void DirtyVoxelEvent(Vector3 pos, byte terrainType);

	private static Dictionary<IntVector3, float> BlockVolumes = new Dictionary<IntVector3, float>();

	private static IntVector3[] s_vegOfsPos = new IntVector3[5]
	{
		new IntVector3(),
		new IntVector3(-1),
		new IntVector3(1),
		new IntVector3(0, 0, -1),
		new IntVector3(0, 0, 1)
	};

	private static readonly float VoxelDamageMin = 0f;

	private static readonly float VoxelDamageMax = 1275f;

	private static readonly float VoxelDamageDT = VoxelDamageMax - VoxelDamageMin;

	private static readonly float ReduceF = 0.33f;

	private static readonly float MinDamage = 2.5f;

	public static readonly int DonotChangeVoxelNotice = 8000175;

	private static List<DragItemMousePickPlant> mDigPlant = new List<DragItemMousePickPlant>();

	private static List<Tuple<Vector3, int>> trees = new List<Tuple<Vector3, int>>();

	private static List<Vector3> grasses = new List<Vector3>();

	public static event DigTerrainDel onDigTerrain;

	public static event DirtyVoxelEvent onDirtyVoxel;

	private static void DeleteVegetation(IntVector3 treePos)
	{
		for (int i = 0; i < s_vegOfsPos.Length; i++)
		{
			Vector3 pos = treePos + s_vegOfsPos[i];
			DeleteVegetation(pos);
		}
	}

	private static void DeleteVegetation(Vector3 pos)
	{
		if (null != LSubTerrainMgr.Instance)
		{
			LSubTerrainMgr.DeleteTreesAtPos(pos);
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			RSubTerrainMgr.DeleteTreesAtPos(pos);
		}
		PeGrassSystem.DeleteAtPos(pos);
	}

	private static void DigBlocks(IntVector3 blockUnitPos, float durDec, ref List<B45Block> removeList)
	{
		B45Block b45Block = Block45Man.self.DataSource.SafeRead(blockUnitPos.x, blockUnitPos.y, blockUnitPos.z);
		if (!BlockVolumes.ContainsKey(blockUnitPos))
		{
			BlockVolumes[blockUnitPos] = 255f;
		}
		if (b45Block.blockType == 0)
		{
			return;
		}
		int blockItemProtoID = PEBuildingMan.GetBlockItemProtoID(b45Block.materialType);
		ItemProto itemData = ItemProto.GetItemData(blockItemProtoID);
		if (itemData == null)
		{
			string message = "ItemProto not found for material type " + b45Block.materialType;
			Debug.LogError(message);
			return;
		}
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(itemData.setUp);
		if (terrainResData == null)
		{
			string message2 = "NaturalRes not found for item " + itemData.id;
			Debug.LogError(message2);
			return;
		}
		float num = durDec * terrainResData.m_duration;
		Dictionary<IntVector3, float> blockVolumes;
		Dictionary<IntVector3, float> dictionary = (blockVolumes = BlockVolumes);
		IntVector3 key;
		IntVector3 key2 = (key = blockUnitPos);
		float num2 = blockVolumes[key];
		dictionary[key2] = num2 - num;
		if (BlockVolumes[blockUnitPos] <= 0f)
		{
			Block45Man.self.DataSource.SafeWrite(new B45Block(0, 0), blockUnitPos.x, blockUnitPos.y, blockUnitPos.z);
			BlockVolumes.Remove(blockUnitPos);
		}
	}

	public static void ClearBlockInfo()
	{
		BlockVolumes.Clear();
	}

	public static void ClearColonyBlockInfo(CSAssembly assembly)
	{
		if (assembly == null)
		{
			return;
		}
		List<IntVector3> list = new List<IntVector3>();
		foreach (IntVector3 key in BlockVolumes.Keys)
		{
			if (assembly.InRange(key.ToVector3()))
			{
				list.Add(key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			BlockVolumes.Remove(list[i]);
		}
		list.Clear();
	}

	public static int DigBlock45(IntVector3 intPos, float durDec, float radius, float height, ref List<B45Block> removeList, bool square = true)
	{
		int num = 0;
		bool flag = true;
		int num2 = intPos.x * 2;
		int num3 = intPos.y * 2;
		int num4 = intPos.z * 2;
		int num5 = (int)(radius * 2f);
		int num6 = (int)(height * 2f);
		float num7 = radius * radius;
		int num8 = (int)(num7 * 2f * 2f);
		for (int i = -num5; i <= num5; i++)
		{
			for (int j = -num5; j <= num5; j++)
			{
				for (int k = -num6; k <= num6; k++)
				{
					int num9 = i * i + k * k + j * j;
					if (square || num9 <= num8)
					{
						IntVector3 blockUnitPos = new IntVector3(num2 + i, num3 + k, num4 + j);
						DigBlocks(blockUnitPos, durDec, ref removeList);
					}
				}
			}
		}
		for (float num10 = 0f - radius; num10 <= radius; num10 += 1f)
		{
			for (float num11 = 0f - radius; num11 <= radius; num11 += 1f)
			{
				for (float num12 = 0f - height; num12 <= height; num12 += 1f)
				{
					if (square || !(num10 * num10 + num12 * num12 + num11 * num11 > num7))
					{
						IntVector3 treePos = new IntVector3(Mathf.FloorToInt((float)intPos.x + num10), Mathf.FloorToInt((float)intPos.y + num12), Mathf.FloorToInt((float)intPos.z + num11));
						DeleteVegetation(treePos);
					}
				}
			}
		}
		if (null != LSubTerrainMgr.Instance)
		{
			LSubTerrainMgr.RefreshAllLayerTerrains();
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			RSubTerrainMgr.RefreshAllLayerTerrains();
		}
		if (flag)
		{
			num += 100;
		}
		return num;
	}

	private static void DigVoxels(IntVector3 intPos, float durDec, ref List<VFVoxel> removeList)
	{
		Vector3 zero = Vector3.zero;
		zero.x = intPos.x;
		zero.y = intPos.y;
		zero.z = intPos.z;
		VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.SafeRead(intPos.x, intPos.y, intPos.z);
		if (vFVoxel.Volume == 0)
		{
			return;
		}
		float num = durDec;
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(vFVoxel.Type);
		if (terrainResData != null)
		{
			float num2 = (num - VoxelDamageMin) / VoxelDamageDT;
			float num3 = 37.14689f * terrainResData.m_duration * terrainResData.m_duration - 14.12429f * terrainResData.m_duration + 1.39f;
			if (num3 - num2 > ReduceF)
			{
				num *= 1f - Mathf.Clamp01((num3 - num2 - ReduceF) / ReduceF);
			}
			num *= terrainResData.m_duration;
			if (num < MinDamage)
			{
				num = MinDamage;
			}
		}
		else
		{
			Debug.LogWarning("VoxelType[" + vFVoxel.Type + "] does't have NaturalRes data.");
		}
		if (num >= 255f)
		{
			vFVoxel.Volume = 0;
		}
		else if ((float)(int)vFVoxel.Volume > num)
		{
			vFVoxel.Volume -= (byte)num;
		}
		else
		{
			vFVoxel.Volume = 0;
		}
		if (vFVoxel.Volume <= 127)
		{
			vFVoxel.Volume = 0;
			VFVoxelTerrain.self.AlterVoxelInBuild(zero, new VFVoxel(0));
			removeList.Add(vFVoxel);
		}
		else
		{
			VFVoxelTerrain.self.AlterVoxelInBuild(zero, vFVoxel);
		}
	}

	public static int DigTerrain(IntVector3 intPos, float durDec, float radius, float height, ref List<VFVoxel> removeList, bool square = true)
	{
		int num = 0;
		bool flag = true;
		for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
		{
			for (float num3 = 0f - radius; num3 <= radius; num3 += 1f)
			{
				for (float num4 = 0f; num4 <= height; num4 += 1f)
				{
					float num5 = num2 * num2 + num4 * num4 + num3 * num3;
					if (square || !(num5 > radius * radius))
					{
						IntVector3 intVector = new IntVector3((float)intPos.x + num2, (float)intPos.y + num4, (float)intPos.z + num3);
						DeleteVegetation(intVector);
						DigVoxels(intVector, durDec, ref removeList);
						if (DigTerrainManager.onDigTerrain != null)
						{
							DigTerrainManager.onDigTerrain(intVector);
						}
					}
				}
			}
		}
		DigPlant(intPos.ToVector3(), radius);
		if (null != LSubTerrainMgr.Instance)
		{
			LSubTerrainMgr.RefreshAllLayerTerrains();
		}
		else if (null != RSubTerrainMgr.Instance)
		{
			RSubTerrainMgr.RefreshAllLayerTerrains();
		}
		if (flag)
		{
			num += 100;
		}
		return num;
	}

	private static void DigPlant(Vector3 position, float radius)
	{
		Collider[] array = Physics.OverlapSphere(position, radius);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].tag == "Plant")
			{
				DragItemMousePickPlant componentInParent = array[i].GetComponentInParent<DragItemMousePickPlant>();
				if (null != componentInParent && !mDigPlant.Contains(componentInParent))
				{
					componentInParent.OnClear();
					mDigPlant.Add(componentInParent);
				}
			}
		}
		mDigPlant.Clear();
	}

	public static void ChangeTerrain(IntVector3 intPos, float radius, byte targetType, SkEntity dig = null)
	{
		if (GameConfig.IsMultiMode && dig != null && dig.IsController())
		{
			List<VFVoxel> changedVoxel = new List<VFVoxel>();
			grasses.Clear();
			for (float num = 0f - radius; num <= radius; num += 1f)
			{
				for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
				{
					for (float num3 = 0f - radius; num3 <= radius; num3 += 1f)
					{
						IntVector3 intVector = new IntVector3((float)intPos.x + num, (float)intPos.y + num3, (float)intPos.z + num2);
						VFVoxel item = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z);
						changedVoxel.Add(item);
						if (PeGrassSystem.DeleteAtPos(intVector, out var pos))
						{
							grasses.Add(pos);
						}
					}
				}
			}
			if (changedVoxel.Count != 0)
			{
				byte[] array = Serialize.Export(delegate(BinaryWriter w)
				{
					w.Write(changedVoxel.Count);
					foreach (VFVoxel item2 in changedVoxel)
					{
						BufferHelper.Serialize(w, item2);
					}
				});
				dig._net.RPCServer(EPacketType.PT_InGame_SKChangeTerrain, intPos, radius, targetType, array);
			}
			if (grasses.Count == 0)
			{
				return;
			}
			byte[] array2 = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, grasses.Count);
				foreach (Vector3 grass in grasses)
				{
					BufferHelper.Serialize(w, grass);
				}
			});
			dig._net.RPCServer(EPacketType.PT_InGame_ClearGrass, array2);
			return;
		}
		Dictionary<IntVector3, VFVoxel> dictionary = new Dictionary<IntVector3, VFVoxel>();
		for (float num4 = 0f - radius; num4 <= radius; num4 += 1f)
		{
			for (float num5 = 0f - radius; num5 <= radius; num5 += 1f)
			{
				for (float num6 = 0f - radius; num6 <= radius; num6 += 1f)
				{
					IntVector3 intVector2 = new IntVector3((float)intPos.x + num4, (float)intPos.y + num6, (float)intPos.z + num5);
					VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector2.x, intVector2.y, intVector2.z);
					if (voxel.Type != 0 && voxel.Type != targetType)
					{
						if (voxel.Volume > 128)
						{
							dictionary[intVector2] = new VFVoxel((byte)((voxel.Volume >= 235) ? 255u : ((uint)(voxel.Volume + 20))), targetType);
						}
						else
						{
							dictionary[intVector2] = new VFVoxel(voxel.Volume, targetType);
						}
						VFVoxelTerrain.self.AlterVoxelInBuild(intVector2, dictionary[intVector2]);
					}
					PeGrassSystem.DeleteAtPos(intVector2);
					DirtyTerrain(intVector2, voxel, targetType);
				}
			}
		}
		if (dictionary.Count == 0)
		{
			new PeTipMsg(PELocalization.GetString(DonotChangeVoxelNotice), string.Empty, PeTipMsg.EMsgLevel.Norm);
		}
	}

	public static void ChangeTerrainNetReturn(IntVector3 intPos, float radius, byte targetType, byte[] data)
	{
		Serialize.Import(data, delegate
		{
			for (float num = 0f - radius; num <= radius; num += 1f)
			{
				for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
				{
					for (float num3 = 0f - radius; num3 <= radius; num3 += 1f)
					{
						IntVector3 intVector = new IntVector3((float)intPos.x + num, (float)intPos.y + num3, (float)intPos.z + num2);
						VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z);
						if (voxel.Type != 0 && voxel.Type != targetType)
						{
							voxel = ((voxel.Volume <= 128) ? new VFVoxel(voxel.Volume, targetType) : new VFVoxel((byte)((voxel.Volume >= 235) ? 255u : ((uint)(voxel.Volume + 20))), targetType));
							ApplyBSDataFromNet(0, intVector, new BSVoxel(voxel));
						}
						DirtyTerrain(intVector, voxel, targetType);
					}
				}
			}
		});
	}

	public static Dictionary<int, int> GetResouce(List<VFVoxel> removeList, float bouns, bool bGetSpItems = false)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		if (removeList.Count == 0)
		{
			return dictionary2;
		}
		foreach (VFVoxel remove in removeList)
		{
			if (dictionary.ContainsKey(remove.Type))
			{
				Dictionary<int, int> dictionary3;
				Dictionary<int, int> dictionary4 = (dictionary3 = dictionary);
				int type;
				int key = (type = remove.Type);
				type = dictionary3[type];
				dictionary4[key] = type + 1;
			}
			else
			{
				dictionary[remove.Type] = 1;
			}
		}
		foreach (int key5 in dictionary.Keys)
		{
			NaturalRes terrainResData;
			if ((terrainResData = NaturalRes.GetTerrainResData(key5)) == null || terrainResData.m_itemsGot.Count <= 0)
			{
				continue;
			}
			float num = 0f;
			num = ((!(terrainResData.mFixedNum > 0f)) ? (bouns + terrainResData.mSelfGetNum) : terrainResData.mFixedNum);
			if (num < 0f)
			{
				num = 0f;
			}
			num *= (float)dictionary[key5];
			num = (int)num + ((Random.value < num - (float)(int)num) ? 1 : 0);
			for (int i = 0; (float)i < num; i++)
			{
				int num2 = Random.Range(0, 100);
				for (int j = 0; j < terrainResData.m_itemsGot.Count; j++)
				{
					if ((float)num2 < terrainResData.m_itemsGot[j].m_probablity)
					{
						if (dictionary2.ContainsKey(terrainResData.m_itemsGot[j].m_id))
						{
							Dictionary<int, int> dictionary5;
							Dictionary<int, int> dictionary6 = (dictionary5 = dictionary2);
							int type;
							int key2 = (type = terrainResData.m_itemsGot[j].m_id);
							type = dictionary5[type];
							dictionary6[key2] = type + 1;
						}
						else
						{
							dictionary2[terrainResData.m_itemsGot[j].m_id] = 1;
						}
						break;
					}
				}
			}
			if (terrainResData.m_extraGot.extraPercent > 0f && Random.value < num * terrainResData.m_extraGot.extraPercent)
			{
				num *= terrainResData.m_extraGot.extraPercent;
				for (int k = 0; (float)k < num; k++)
				{
					int num3 = Random.Range(0, 100);
					for (int l = 0; l < terrainResData.m_extraGot.m_extraGot.Count; l++)
					{
						if ((float)num3 < terrainResData.m_extraGot.m_extraGot[l].m_probablity)
						{
							if (dictionary2.ContainsKey(terrainResData.m_extraGot.m_extraGot[l].m_id))
							{
								Dictionary<int, int> dictionary7;
								Dictionary<int, int> dictionary8 = (dictionary7 = dictionary2);
								int type;
								int key3 = (type = terrainResData.m_extraGot.m_extraGot[l].m_id);
								type = dictionary7[type];
								dictionary8[key3] = type + 1;
							}
							else
							{
								dictionary2[terrainResData.m_extraGot.m_extraGot[l].m_id] = 1;
							}
							break;
						}
					}
				}
			}
			if (!bGetSpItems)
			{
				continue;
			}
			num = ((!(terrainResData.mFixedNum > 0f)) ? (bouns + terrainResData.mSelfGetNum) : terrainResData.mFixedNum);
			if (num < 0f)
			{
				num = 0f;
			}
			num *= (float)dictionary[key5];
			num = (int)num + ((Random.value < num - (float)(int)num) ? 1 : 0);
			if (!(terrainResData.m_extraSpGot.extraPercent > 0f) || !(Random.value < num * terrainResData.m_extraSpGot.extraPercent))
			{
				continue;
			}
			num *= terrainResData.m_extraSpGot.extraPercent;
			for (int m = 0; (float)m < num; m++)
			{
				int num4 = Random.Range(0, 100);
				for (int n = 0; n < terrainResData.m_extraSpGot.m_extraGot.Count; n++)
				{
					if ((float)num4 < terrainResData.m_extraSpGot.m_extraGot[n].m_probablity)
					{
						if (dictionary2.ContainsKey(terrainResData.m_extraSpGot.m_extraGot[n].m_id))
						{
							Dictionary<int, int> dictionary9;
							Dictionary<int, int> dictionary10 = (dictionary9 = dictionary2);
							int type;
							int key4 = (type = terrainResData.m_extraSpGot.m_extraGot[n].m_id);
							type = dictionary9[type];
							dictionary10[key4] = type + 1;
						}
						else
						{
							dictionary2[terrainResData.m_extraSpGot.m_extraGot[n].m_id] = 1;
						}
						break;
					}
				}
			}
		}
		return dictionary2;
	}

	public static void DestroyTerrainInRange(int type, Vector3 pos, float power, float radius)
	{
		bool flag = false;
		if (type == 2)
		{
			IntVector3 intVector = new IntVector3(pos - radius * Vector3.one);
			for (float num = 0f; num < 2f * radius; num += 1f)
			{
				for (float num2 = 0f; num2 < 2f * radius; num2 += 1f)
				{
					for (float num3 = 0f; num3 < 2f * radius; num3 += 1f)
					{
						IntVector3 intVector2 = new IntVector3((float)intVector.x + num, (float)intVector.y + num2, (float)intVector.z + num3);
						if (!(Vector3.Distance(intVector2, pos) <= radius))
						{
							continue;
						}
						VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector2.x, intVector2.y, intVector2.z);
						if (null != LSubTerrainMgr.Instance)
						{
							LSubTerrainMgr.DeleteTreesAtPos(intVector2);
							if (!flag)
							{
								flag = LSubTerrainMgr.Picking(intVector2, includeTrees: true).Count > 0;
							}
						}
						else if (null != RSubTerrainMgr.Instance)
						{
							RSubTerrainMgr.DeleteTreesAtPos(intVector2);
							if (!flag)
							{
								flag = RSubTerrainMgr.Picking(intVector2, includeTrees: true).Count > 0;
							}
						}
						PeGrassSystem.DeleteAtPos(pos);
						if (voxel.Volume > 0)
						{
							NaturalRes terrainResData = NaturalRes.GetTerrainResData(voxel.Type);
							float num4 = power * terrainResData.m_duration * (1f - Mathf.Clamp01(Vector3.Distance(pos, intVector2.ToVector3()) / radius) * 0.25f);
							if ((float)(int)voxel.Volume > num4)
							{
								voxel.Volume -= (byte)num4;
							}
							else
							{
								voxel.Volume = 0;
							}
							if (voxel.Volume < 128)
							{
								voxel.Volume = 0;
								VFVoxelTerrain.self.AlterVoxelInBuild(intVector2, new VFVoxel(0, 0));
							}
							else
							{
								VFVoxelTerrain.self.AlterVoxelInBuild(intVector2, voxel);
							}
							if (DigTerrainManager.onDigTerrain != null)
							{
								DigTerrainManager.onDigTerrain(intVector2);
							}
						}
					}
				}
			}
		}
		if (type == 1 || type == 2)
		{
			for (float num5 = 0f - radius; num5 < radius; num5 += 0.5f)
			{
				for (float num6 = 0f - radius; num6 < radius; num6 += 0.5f)
				{
					for (float num7 = 0f - radius; num7 < radius; num7 += 0.5f)
					{
						Vector3 vector = new Vector3(num5, num6, num7);
						Vector3 vector2 = pos + vector;
						vector2.x = Mathf.Floor(vector2.x * 2f) * 0.5f;
						vector2.y = Mathf.Floor(vector2.y * 2f) * 0.5f;
						vector2.z = Mathf.Floor(vector2.z * 2f) * 0.5f;
						if (!(Vector3.Distance(vector2, pos) < radius))
						{
							continue;
						}
						IntVector3 intVector3 = new IntVector3(Mathf.FloorToInt(vector2.x * 2f), Mathf.FloorToInt(vector2.y * 2f), Mathf.FloorToInt(vector2.z * 2f));
						B45Block b45Block = Block45Man.self.DataSource.SafeRead(intVector3.x, intVector3.y, intVector3.z);
						if (!BlockVolumes.ContainsKey(intVector3))
						{
							BlockVolumes[intVector3] = 255f;
						}
						if (b45Block.blockType >> 2 != 0)
						{
							int blockItemProtoID = PEBuildingMan.GetBlockItemProtoID(b45Block.materialType);
							NaturalRes terrainResData2 = NaturalRes.GetTerrainResData(ItemProto.GetItemData(blockItemProtoID).setUp);
							float num8 = power * terrainResData2.m_duration * (1f - Mathf.Clamp01(vector.magnitude / radius) * 0.25f);
							Dictionary<IntVector3, float> blockVolumes;
							Dictionary<IntVector3, float> dictionary = (blockVolumes = BlockVolumes);
							IntVector3 key;
							IntVector3 key2 = (key = intVector3);
							float num9 = blockVolumes[key];
							dictionary[key2] = num9 - num8;
							if (BlockVolumes[intVector3] <= 0f)
							{
								Block45Man.self.DataSource.SafeWrite(new B45Block(0, 0), intVector3.x, intVector3.y, intVector3.z);
								BlockVolumes.Remove(intVector3);
								EffectManager.Instance.Instantiate(47, vector2, Quaternion.identity);
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			if (null != LSubTerrainMgr.Instance)
			{
				LSubTerrainMgr.RefreshAllLayerTerrains();
			}
			else if (null != RSubTerrainMgr.Instance)
			{
				RSubTerrainMgr.RefreshAllLayerTerrains();
			}
		}
	}

	private static void GetTreeInfo(Vector3 treePos, ref List<Tuple<Vector3, int>> trees)
	{
		if (LSubTerrainMgr.Instance != null)
		{
			List<GlobalTreeInfo> list = LSubTerrainMgr.Picking(treePos, includeTrees: true);
			if (list.Count <= 0)
			{
				return;
			}
			{
				Tuple<Vector3, int> item = default(Tuple<Vector3, int>);
				foreach (GlobalTreeInfo item3 in list)
				{
					item.v1 = item3.WorldPos;
					item.v2 = item3._treeInfo.m_protoTypeIdx;
					trees.Add(item);
				}
				return;
			}
		}
		List<TreeInfo> list2 = RSubTerrainMgr.Instance.TreesAtPos(treePos);
		Tuple<Vector3, int> item2 = default(Tuple<Vector3, int>);
		foreach (TreeInfo item4 in list2)
		{
			item2.v1 = item4.m_pos;
			item2.v2 = item4.m_protoTypeIdx;
			trees.Add(item2);
		}
	}

	private static void GetTreeInfo(IntVector3 idx, ref List<Tuple<Vector3, int>> trees)
	{
		Vector3 treePos = idx;
		GetTreeInfo(treePos, ref trees);
		treePos.x = idx.x - 1;
		GetTreeInfo(treePos, ref trees);
		treePos.x = idx.x + 1;
		GetTreeInfo(treePos, ref trees);
		treePos.x = idx.x;
		treePos.z = idx.z - 1;
		GetTreeInfo(treePos, ref trees);
		treePos.z = idx.z + 1;
		GetTreeInfo(treePos, ref trees);
	}

	private static void GetGrassPos(IntVector3 idx, ref List<Vector3> grasses)
	{
		Vector3 voxelPos = idx;
		if (PeGrassSystem.DeleteAtPos(voxelPos, out var pos))
		{
			grasses.Add(pos);
		}
		voxelPos.x = idx.x - 1;
		if (PeGrassSystem.DeleteAtPos(voxelPos, out pos))
		{
			grasses.Add(pos);
		}
		voxelPos.x = idx.x + 1;
		if (PeGrassSystem.DeleteAtPos(voxelPos, out pos))
		{
			grasses.Add(pos);
		}
		voxelPos.x = idx.x;
		voxelPos.z = idx.z - 1;
		if (PeGrassSystem.DeleteAtPos(voxelPos, out pos))
		{
			grasses.Add(pos);
		}
		voxelPos.z = idx.z + 1;
		if (PeGrassSystem.DeleteAtPos(voxelPos, out pos))
		{
			grasses.Add(pos);
		}
	}

	internal static int DigTerrainNetwork(SkNetworkInterface digger, IntVector3 intPos, float durDec, float radius, float resourceBonus, bool returnResource, bool bGetSpItems, float height)
	{
		if (!digger.hasOwnerAuth)
		{
			return 0;
		}
		if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer._curSceneId != 0)
		{
			return 0;
		}
		trees.Clear();
		grasses.Clear();
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			for (float num = 0f - radius; num <= radius; num += 1f)
			{
				for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
				{
					for (float num3 = 0f - height; num3 <= height; num3 += 1f)
					{
						IntVector3 intVector = new IntVector3((float)intPos.x + num, (float)intPos.y + num3, (float)intPos.z + num2);
						float num4 = num * num + num3 * num3 + num2 * num2;
						if (returnResource || !(num4 > radius * radius))
						{
							VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z);
							BufferHelper.Serialize(w, vFVoxel.Type);
							BufferHelper.Serialize(w, vFVoxel.Volume);
							GetTreeInfo(intVector, ref trees);
							GetGrassPos(intVector, ref grasses);
						}
					}
				}
			}
		});
		digger.RPCServer(EPacketType.PT_InGame_SKDigTerrain, intPos, durDec, radius, resourceBonus, array, returnResource, bGetSpItems, height);
		DigPlant(intPos.ToVector3(), radius);
		if (trees.Count != 0)
		{
			byte[] array2 = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, trees.Count);
				foreach (Tuple<Vector3, int> tree in trees)
				{
					BufferHelper.Serialize(w, tree.v1);
					BufferHelper.Serialize(w, tree.v2);
				}
			});
			digger.RPCServer(EPacketType.PT_InGame_ClearTree, array2);
		}
		if (grasses.Count != 0)
		{
			byte[] array3 = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, grasses.Count);
				foreach (Vector3 grass in grasses)
				{
					BufferHelper.Serialize(w, grass);
				}
			});
			digger.RPCServer(EPacketType.PT_InGame_ClearGrass, array3);
		}
		return 0;
	}

	internal static void DestroyTerrainInRangeNetwork(int type, SkillRunner caster, Vector3 pos, float power, float radius)
	{
		if (type == 2)
		{
			trees.Clear();
			grasses.Clear();
			IntVector3 basePos = new IntVector3(pos - radius * Vector3.one);
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				for (int i = 0; (float)i < 2f * radius; i++)
				{
					for (int j = 0; (float)j < 2f * radius; j++)
					{
						for (int k = 0; (float)k < 2f * radius; k++)
						{
							IntVector3 intVector = new IntVector3(basePos.x + i, basePos.y + j, basePos.z + k);
							if (Vector3.Distance(intVector, pos) <= radius)
							{
								VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z);
								BufferHelper.Serialize(w, vFVoxel.Type);
								BufferHelper.Serialize(w, vFVoxel.Volume);
								GetTreeInfo(intVector.ToVector3(), ref trees);
								GetGrassPos(intVector, ref grasses);
							}
						}
					}
				}
			});
			if (null != caster)
			{
				caster.RPCServer(EPacketType.PT_InGame_SkillVoxelRange, pos, power, radius, array);
			}
			if (trees.Count != 0)
			{
				byte[] array2 = Serialize.Export(delegate(BinaryWriter w)
				{
					BufferHelper.Serialize(w, trees.Count);
					foreach (Tuple<Vector3, int> tree in trees)
					{
						BufferHelper.Serialize(w, tree.v1);
						BufferHelper.Serialize(w, tree.v2);
					}
				});
				caster.RPCServer(EPacketType.PT_InGame_ClearTree, array2);
			}
			if (grasses.Count != 0)
			{
				byte[] array3 = Serialize.Export(delegate(BinaryWriter w)
				{
					BufferHelper.Serialize(w, grasses.Count);
					foreach (Vector3 grass in grasses)
					{
						BufferHelper.Serialize(w, grass);
					}
				});
				caster.RPCServer(EPacketType.PT_InGame_ClearGrass, array3);
			}
		}
		if ((type == 1 || type == 2) && null != caster)
		{
			caster.RPCServer(EPacketType.PT_InGame_SkillBlockRange, pos, power, radius, 0.5f);
		}
	}

	public static void BlockClearGrass(IBSDataSource ds, IEnumerable<IntVector3> indexes)
	{
		grasses.Clear();
		if (ds == BuildingMan.Blocks)
		{
			foreach (IntVector3 index in indexes)
			{
				Vector3 voxelPos = new Vector3((float)index.x * ds.Scale, (float)index.y * ds.Scale, (float)index.z * ds.Scale) - ds.Offset;
				if (PeGrassSystem.DeleteAtPos(voxelPos, out var pos))
				{
					grasses.Add(pos);
				}
				voxelPos.y -= 1f;
				if (PeGrassSystem.DeleteAtPos(voxelPos, out pos))
				{
					grasses.Add(pos);
				}
			}
		}
		else if (ds == BuildingMan.Voxels)
		{
			foreach (IntVector3 index2 in indexes)
			{
				Vector3 voxelPos2 = index2.ToVector3();
				if (PeGrassSystem.DeleteAtPos(voxelPos2, out var pos2))
				{
					grasses.Add(pos2);
				}
				voxelPos2.y -= 1f;
				if (PeGrassSystem.DeleteAtPos(voxelPos2, out pos2))
				{
					grasses.Add(pos2);
				}
			}
		}
		if (grasses.Count == 0)
		{
			return;
		}
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(grasses.Count);
			foreach (Vector3 grass in grasses)
			{
				BufferHelper.Serialize(w, grass);
			}
		});
		PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearGrass, array);
	}

	internal static float Fell(GlobalTreeInfo treeinfo, float damage, float hp)
	{
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(treeinfo._treeInfo.m_protoTypeIdx + 1000);
		if (terrainResData != null)
		{
			hp -= damage * terrainResData.m_duration / (treeinfo._treeInfo.m_heightScale * treeinfo._treeInfo.m_widthScale);
		}
		return hp;
	}

	internal static void RemoveTree(GlobalTreeInfo treeinfo)
	{
		if (treeinfo != null)
		{
			if (null != LSubTerrainMgr.Instance)
			{
				LSubTerrainMgr.DeleteTree(treeinfo);
				LSubTerrainMgr.RefreshAllLayerTerrains();
			}
			else if (null != RSubTerrainMgr.Instance)
			{
				RSubTerrainMgr.DeleteTree(treeinfo._treeInfo);
				RSubTerrainMgr.RefreshAllLayerTerrains();
			}
		}
	}

	internal static Dictionary<int, int> GetTreeResouce(GlobalTreeInfo treeinfo, float bouns, bool bGetSpItems = false)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (treeinfo != null)
		{
			NaturalRes terrainResData = NaturalRes.GetTerrainResData(treeinfo._treeInfo.m_protoTypeIdx + 1000);
			if (terrainResData != null)
			{
				float num = 0f;
				num = ((!(terrainResData.mFixedNum > 0f)) ? (terrainResData.mSelfGetNum * treeinfo._treeInfo.m_widthScale * treeinfo._treeInfo.m_widthScale * treeinfo._treeInfo.m_heightScale * (1f + bouns)) : terrainResData.mFixedNum);
				if (num < 1f)
				{
					num = 1f;
				}
				for (int i = 0; i < (int)num; i++)
				{
					int num2 = Random.Range(0, 100);
					for (int j = 0; j < terrainResData.m_itemsGot.Count; j++)
					{
						if ((float)num2 < terrainResData.m_itemsGot[j].m_probablity)
						{
							if (dictionary.ContainsKey(terrainResData.m_itemsGot[j].m_id))
							{
								Dictionary<int, int> dictionary2;
								Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
								int id;
								int key = (id = terrainResData.m_itemsGot[j].m_id);
								id = dictionary2[id];
								dictionary3[key] = id + 1;
							}
							else
							{
								dictionary[terrainResData.m_itemsGot[j].m_id] = 1;
							}
							break;
						}
					}
				}
				if (terrainResData.m_extraGot.extraPercent > 0f && Random.value < num * terrainResData.m_extraGot.extraPercent)
				{
					num *= terrainResData.m_extraGot.extraPercent;
					for (int k = 0; (float)k < num; k++)
					{
						int num3 = Random.Range(0, 100);
						for (int l = 0; l < terrainResData.m_extraGot.m_extraGot.Count; l++)
						{
							if ((float)num3 < terrainResData.m_extraGot.m_extraGot[l].m_probablity)
							{
								if (dictionary.ContainsKey(terrainResData.m_extraGot.m_extraGot[l].m_id))
								{
									Dictionary<int, int> dictionary4;
									Dictionary<int, int> dictionary5 = (dictionary4 = dictionary);
									int id;
									int key2 = (id = terrainResData.m_extraGot.m_extraGot[l].m_id);
									id = dictionary4[id];
									dictionary5[key2] = id + 1;
								}
								else
								{
									dictionary[terrainResData.m_extraGot.m_extraGot[l].m_id] = 1;
								}
								break;
							}
						}
					}
				}
				if (bGetSpItems)
				{
					num = ((!(terrainResData.mFixedNum > 0f)) ? (terrainResData.mSelfGetNum * treeinfo._treeInfo.m_widthScale * treeinfo._treeInfo.m_widthScale * treeinfo._treeInfo.m_heightScale * (1f + bouns)) : terrainResData.mFixedNum);
					if (num < 1f)
					{
						num = 1f;
					}
					if (terrainResData.m_extraSpGot.extraPercent > 0f && Random.value < num * terrainResData.m_extraSpGot.extraPercent)
					{
						num *= terrainResData.m_extraSpGot.extraPercent;
						for (int m = 0; (float)m < num; m++)
						{
							int num4 = Random.Range(0, 100);
							for (int n = 0; n < terrainResData.m_extraSpGot.m_extraGot.Count; n++)
							{
								if ((float)num4 < terrainResData.m_extraSpGot.m_extraGot[n].m_probablity)
								{
									if (dictionary.ContainsKey(terrainResData.m_extraSpGot.m_extraGot[n].m_id))
									{
										Dictionary<int, int> dictionary6;
										Dictionary<int, int> dictionary7 = (dictionary6 = dictionary);
										int id;
										int key3 = (id = terrainResData.m_extraSpGot.m_extraGot[n].m_id);
										id = dictionary6[id];
										dictionary7[key3] = id + 1;
									}
									else
									{
										dictionary[terrainResData.m_extraSpGot.m_extraGot[n].m_id] = 1;
									}
									break;
								}
							}
						}
					}
				}
			}
		}
		return dictionary;
	}

	public static void DigTerrainNetReturn(IntVector3 pos, float durDec, float radius, float height, bool bReturnItem)
	{
		for (float num = 0f - radius; num <= radius; num += 1f)
		{
			for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
			{
				for (float num3 = 0f - height; num3 <= height; num3 += 1f)
				{
					IntVector3 intVector = new IntVector3((float)pos.x + num, (float)pos.y + num3, (float)pos.z + num2);
					float num4 = num * num + num3 * num3 + num2 * num2;
					if (!bReturnItem && num4 > radius * radius)
					{
						continue;
					}
					VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z);
					if (voxel.Volume != 0)
					{
						float num5 = durDec;
						NaturalRes terrainResData = NaturalRes.GetTerrainResData(voxel.Type);
						if (terrainResData != null)
						{
							num5 *= terrainResData.m_duration;
						}
						if (num5 >= 255f)
						{
							voxel.Volume = 0;
						}
						else if ((float)(int)voxel.Volume > num5)
						{
							voxel.Volume -= (byte)num5;
						}
						else
						{
							voxel.Volume = 0;
						}
						if (voxel.Volume <= 127)
						{
							voxel.Volume = 0;
						}
						ApplyBSDataFromNet(0, intVector, new BSVoxel(voxel));
						DeleteTree(intVector);
						DeleteGrass(intVector);
						if (DigTerrainManager.onDigTerrain != null)
						{
							DigTerrainManager.onDigTerrain(intVector);
						}
					}
				}
			}
		}
		if (LSubTerrainMgr.Instance != null)
		{
			LSubTerrainMgr.RefreshAllLayerTerrains();
		}
		else if (RSubTerrainMgr.Instance != null)
		{
			RSubTerrainMgr.RefreshAllLayerTerrains();
		}
	}

	private static void DeleteTree(IntVector3 idx)
	{
		Vector3 treePos = idx;
		DeleteTree(treePos);
		treePos.x = idx.x - 1;
		DeleteTree(treePos);
		treePos.x = idx.x + 1;
		DeleteTree(treePos);
		treePos.x = idx.x;
		treePos.z = idx.z - 1;
		DeleteTree(treePos);
		treePos.z = idx.z + 1;
		DeleteTree(treePos);
	}

	public static void DeleteTree(Vector3 treePos)
	{
		if (LSubTerrainMgr.Instance != null)
		{
			LSubTerrainMgr.DeleteTreesAtPos(treePos);
			LSubTerrSL.AddDeletedTree(treePos);
		}
		else if (RSubTerrainMgr.Instance != null)
		{
			RSubTerrainMgr.DeleteTreesAtPos(treePos);
			RSubTerrSL.AddDeletedTree(treePos);
		}
	}

	public static void CacheDeleteTree(List<Vector3> positions)
	{
		if (PeGameMgr.IsMultiCustom || PeGameMgr.IsMultiStory)
		{
			foreach (Vector3 position in positions)
			{
				LSubTerrSL.AddDeletedTree(position);
			}
			return;
		}
		foreach (Vector3 position2 in positions)
		{
			RSubTerrSL.AddDeletedTree(position2);
		}
	}

	private static void DeleteGrass(IntVector3 idx)
	{
		Vector3 pos = idx;
		DeleteGrass(pos);
		pos.x = idx.x - 1;
		DeleteGrass(pos);
		pos.x = idx.x + 1;
		DeleteGrass(pos);
		pos.x = idx.x;
		pos.z = idx.z - 1;
		DeleteGrass(pos);
		pos.z = idx.z + 1;
		DeleteGrass(pos);
	}

	public static void DeleteGrass(Vector3 pos)
	{
		if (!PeGrassSystem.DeleteAtPos(pos))
		{
			GrassDataSL.AddDeletedGrass(pos);
		}
	}

	public static void CacheDeleteGrass(List<Vector3> positions)
	{
		foreach (Vector3 position in positions)
		{
			GrassDataSL.AddDeletedGrass(position);
		}
	}

	public static void TerrainDestroyInRangeNetReturn(Vector3 pos, float power, float radius)
	{
		IntVector3 intVector = new IntVector3(pos - radius * Vector3.one);
		for (int i = 0; (float)i < 2f * radius; i++)
		{
			for (int j = 0; (float)j < 2f * radius; j++)
			{
				for (int k = 0; (float)k < 2f * radius; k++)
				{
					IntVector3 intVector2 = new IntVector3(intVector.x + i, intVector.y + j, intVector.z + k);
					if (!(Vector3.Distance(intVector2, pos) < radius))
					{
						continue;
					}
					VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector2.x, intVector2.y, intVector2.z);
					DeleteTree(intVector2.ToVector3());
					DeleteGrass(intVector2.ToVector3());
					if (voxel.Volume <= 0)
					{
						continue;
					}
					NaturalRes terrainResData = NaturalRes.GetTerrainResData(voxel.Type);
					if (terrainResData != null)
					{
						float num = power * terrainResData.m_duration * (1f - Mathf.Clamp01(Vector3.Distance(pos, intVector2.ToVector3()) / radius) * 0.25f);
						if (num >= 255f)
						{
							voxel.Volume = 0;
						}
						else if ((float)(int)voxel.Volume > num)
						{
							voxel.Volume -= (byte)num;
						}
						else
						{
							voxel.Volume = 0;
						}
						if (voxel.Volume <= 127)
						{
							voxel.Volume = 0;
						}
						ApplyBSDataFromNet(0, intVector2, new BSVoxel(voxel));
					}
					if (DigTerrainManager.onDigTerrain != null)
					{
						DigTerrainManager.onDigTerrain(intVector2);
					}
				}
			}
		}
		if (LSubTerrainMgr.Instance != null)
		{
			LSubTerrainMgr.RefreshAllLayerTerrains();
		}
		else if (RSubTerrainMgr.Instance != null)
		{
			RSubTerrainMgr.RefreshAllLayerTerrains();
		}
	}

	public static void ApplyVoxelData(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader br)
		{
			int num = br.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadIntVector3(br, out var v);
				BufferHelper.ReadBSVoxel(br, out var _value);
				ApplyBSDataFromNet(0, v, _value);
				if (_value.volmue < 128)
				{
					if (DigTerrainManager.onDigTerrain != null)
					{
						DigTerrainManager.onDigTerrain(v);
					}
				}
				else
				{
					DirtyTerrain(v, _value.ToVoxel(), _value.type);
				}
			}
		});
		if (RSubTerrainMgr.Instance != null)
		{
			RSubTerrainMgr.RefreshAllLayerTerrains();
		}
		if (LSubTerrainMgr.Instance != null)
		{
			LSubTerrainMgr.RefreshAllLayerTerrains();
		}
	}

	public static void BlockDestroyInRangeNetReturn(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadIntVector3(r, out var v);
				BufferHelper.ReadBSVoxel(r, out var _value);
				ApplyBSDataFromNet(1, v, _value);
			}
		});
	}

	public static void ApplyBlockData(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader br)
		{
			int num = br.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadIntVector3(br, out var v);
				BufferHelper.ReadBSVoxel(br, out var _value);
				ApplyBSDataFromNet(1, v, _value);
			}
		});
	}

	public static void ApplyBSVoxelData(byte[] binData)
	{
		Serialize.Import(binData, delegate(BinaryReader r)
		{
			int dsType = BufferHelper.ReadInt32(r);
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadIntVector3(r, out var v);
				BufferHelper.ReadBSVoxel(r, out var _value);
				ApplyBSDataFromNet(dsType, v, _value);
			}
		});
	}

	public static void DirtyTerrain(IntVector3 pos, VFVoxel voxel, byte targetType)
	{
		if (VFVoxelTerrain.self.Voxels.SafeRead(pos.x, pos.y + 1, pos.z).Volume < 128 && voxel.Volume >= 128 && DigTerrainManager.onDirtyVoxel != null)
		{
			DigTerrainManager.onDirtyVoxel(pos, targetType);
		}
	}

	public static void ApplyBSDataFromNet(int dsType, IntVector3 pos, BSVoxel voxel)
	{
		if (dsType == 0)
		{
			List<IntVector3> dirtyChunkPosListMulti = VFVoxelChunkData.GetDirtyChunkPosListMulti(pos.x, pos.y, pos.z);
			foreach (IntVector3 item in dirtyChunkPosListMulti)
			{
				ChunkManager.ApplyVoxelVolume(pos, item, voxel.ToVoxel());
			}
		}
		BuildingMan.Datas[dsType].Write(voxel, pos.x, pos.y, pos.z);
	}
}
