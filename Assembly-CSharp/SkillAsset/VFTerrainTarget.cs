using System.Collections.Generic;
using ItemAsset;
using NaturalResAsset;
using UnityEngine;

namespace SkillAsset;

public class VFTerrainTarget : ISkillTarget, INaturalResTarget
{
	public bool m_bDestroyed;

	public Vector3 m_vPos;

	public IntVector3 m_intPos;

	public VFVoxel m_voxel;

	private List<VFVoxel> mRemoveList = new List<VFVoxel>();

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

	public int GetDestroyed(SkillRunner caster, float durDec, float radius)
	{
		return 0;
	}

	public List<ItemSample> ReturnItems(short resGotMultiplier, int num)
	{
		num = mRemoveList.Count;
		List<ItemSample> list = new List<ItemSample>();
		for (int i = 0; i < num; i++)
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
			float num2 = 0f;
			num2 = ((!(terrainResData.mFixedNum > 0f)) ? ((float)resGotMultiplier + terrainResData.mSelfGetNum) : terrainResData.mFixedNum);
			for (int k = 0; (float)k < num2; k++)
			{
				list2.Add(Random.Range(0, 100));
			}
			for (int l = 0; l < list2.Count; l++)
			{
				for (int m = 0; m < terrainResData.m_itemsGot.Count; m++)
				{
					if (list2[l] < terrainResData.m_itemsGot[m].m_probablity)
					{
						itemGrids[m].IncreaseStackCount(1);
						break;
					}
				}
			}
			List<ItemSample> list3 = new List<ItemSample>();
			if (terrainResData.m_extraGot.extraPercent > 0f && Random.value < num2 * terrainResData.m_extraGot.extraPercent)
			{
				for (int n = 0; n < terrainResData.m_extraGot.m_extraGot.Count; n++)
				{
					list3.Add(new ItemSample(terrainResData.m_extraGot.m_extraGot[n].m_id, 0));
				}
				num2 *= terrainResData.m_extraGot.extraPercent;
				for (int num3 = 0; (float)num3 < num2; num3++)
				{
					int num4 = Random.Range(0, 100);
					for (int num5 = 0; num5 < terrainResData.m_extraGot.m_extraGot.Count; num5++)
					{
						if ((float)num4 < terrainResData.m_extraGot.m_extraGot[num5].m_probablity)
						{
							list3[num5].IncreaseStackCount(1);
							break;
						}
					}
				}
			}
			for (int num6 = 0; num6 < itemGrids.Length; num6++)
			{
				if (itemGrids[num6].GetCount() > 0)
				{
					ItemSample itemSample = list.Find((ItemSample itr) => itr.protoId == itemGrids[num6].protoId);
					if (itemSample != null)
					{
						itemSample.IncreaseStackCount(itemGrids[num6].GetCount());
					}
					else
					{
						list.Add(itemGrids[num6]);
					}
				}
			}
			ItemSample data;
			foreach (ItemSample item in list3)
			{
				data = item;
				if (data.GetCount() > 0)
				{
					ItemSample itemSample2 = list.Find((ItemSample itr) => itr.protoId == data.protoId);
					if (itemSample2 != null)
					{
						itemSample2.IncreaseStackCount(data.GetCount());
					}
					else
					{
						list.Add(data);
					}
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
