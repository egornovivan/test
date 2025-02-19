using System.Collections.Generic;
using CustomData;
using ItemAsset;
using NaturalResAsset;
using UnityEngine;

public class VFTerrainTarget
{
	public bool m_bDestroyed;

	public Vector3 m_vPos;

	public IntVector3 m_intPos;

	public VFVoxel m_voxel;

	public List<VFVoxel> mRemoveList = new List<VFVoxel>();

	public VFTerrainTarget(Vector3 vPos, IntVector3 intPos, ref VFVoxel voxel)
	{
		m_vPos = vPos;
		m_intPos = intPos;
		m_voxel = voxel;
	}

	public Vector3 GetPosition()
	{
		return m_vPos;
	}

	public ESkillTargetType GetTargetType()
	{
		if (m_voxel.IsBuilding)
		{
			return ESkillTargetType.TYPE_Building;
		}
		if (m_voxel.Type == 3)
		{
			return ESkillTargetType.TYPE_Mud;
		}
		return ESkillTargetType.TYPE_Mud;
	}

	public int GetDestroyed(SkNetworkInterface caster, float durDec, short weaponBonus, GlobalTreeInfo gTreeInfo = null)
	{
		return 0;
	}

	public List<ItemSample> ReturnItems(float resGotMultiplier, bool bGetSpItems = false)
	{
		int count = mRemoveList.Count;
		List<ItemSample> list = new List<ItemSample>();
		for (int i = 0; i < count; i++)
		{
			NaturalRes terrainResData;
			if ((terrainResData = NaturalRes.GetTerrainResData(mRemoveList[i].Type)) == null || terrainResData.m_itemsGot.Count <= 0)
			{
				continue;
			}
			List<float> list2 = new List<float>();
			ItemSample[] itemGrids = new ItemSample[terrainResData.m_itemsGot.Count];
			for (int j = 0; j < terrainResData.m_itemsGot.Count; j++)
			{
				itemGrids[j] = new ItemSample(terrainResData.m_itemsGot[j].m_id, 0);
			}
			float num = 0f;
			num = ((!(terrainResData.mFixedNum > 0f)) ? (resGotMultiplier + terrainResData.mSelfGetNum) : terrainResData.mFixedNum);
			for (int k = 0; (float)k < num; k++)
			{
				list2.Add(Random.Range(0, 100));
			}
			for (int l = 0; l < list2.Count; l++)
			{
				for (int m = 0; m < terrainResData.m_itemsGot.Count; m++)
				{
					if (list2[l] < terrainResData.m_itemsGot[m].m_probablity)
					{
						itemGrids[m].CountUp(1);
						break;
					}
				}
			}
			List<ItemSample> list3 = new List<ItemSample>();
			if (terrainResData.m_extraGot.extraPercent > 0f && Random.value < num * terrainResData.m_extraGot.extraPercent)
			{
				for (int n = 0; n < terrainResData.m_extraGot.m_extraGot.Count; n++)
				{
					list3.Add(new ItemSample(terrainResData.m_extraGot.m_extraGot[n].m_id, 0));
				}
				num *= terrainResData.m_extraGot.extraPercent;
				for (int num2 = 0; (float)num2 < num; num2++)
				{
					int num3 = Random.Range(0, 100);
					for (int num4 = 0; num4 < terrainResData.m_extraGot.m_extraGot.Count; num4++)
					{
						if ((float)num3 < terrainResData.m_extraGot.m_extraGot[num4].m_probablity)
						{
							list3[num4].CountUp(1);
							break;
						}
					}
				}
			}
			if (bGetSpItems && terrainResData.m_extraSpGot.extraPercent > 0f && Random.value < num * terrainResData.m_extraSpGot.extraPercent)
			{
				for (int num5 = 0; num5 < terrainResData.m_extraSpGot.m_extraGot.Count; num5++)
				{
					list3.Add(new ItemSample(terrainResData.m_extraSpGot.m_extraGot[num5].m_id, 0));
				}
				num *= terrainResData.m_extraSpGot.extraPercent;
				for (int num6 = 0; (float)num6 < num; num6++)
				{
					int num7 = Random.Range(0, 100);
					for (int num8 = 0; num8 < terrainResData.m_extraSpGot.m_extraGot.Count; num8++)
					{
						if ((float)num7 < terrainResData.m_extraSpGot.m_extraGot[num8].m_probablity)
						{
							list3[num8].CountUp(1);
							break;
						}
					}
				}
			}
			for (int num9 = 0; num9 < itemGrids.Length; num9++)
			{
				if (itemGrids[num9].stackCount > 0)
				{
					ItemSample itemSample = list.Find((ItemSample itr) => itr.protoId == itemGrids[num9].protoId);
					if (itemSample != null)
					{
						itemSample.CountUp(itemGrids[num9].stackCount);
					}
					else
					{
						list.Add(itemGrids[num9]);
					}
				}
			}
			foreach (ItemSample item in list3)
			{
				if (item.stackCount > 0)
				{
					list.Add(item);
				}
			}
		}
		mRemoveList.Clear();
		return list;
	}

	public bool IsDestroyed()
	{
		return m_bDestroyed;
	}
}
