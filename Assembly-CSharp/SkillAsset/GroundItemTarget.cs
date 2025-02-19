using System.Collections.Generic;
using ItemAsset;
using NaturalResAsset;
using UnityEngine;

namespace SkillAsset;

public class GroundItemTarget : ISkillTarget, INaturalResTarget
{
	public float m_Hp = 255f;

	public float mMaxHp = 255f;

	public Vector3 mPos;

	public GlobalTreeInfo mGroundItem;

	public GroundItemTarget(Vector3 pos, GlobalTreeInfo item)
	{
		mPos = pos;
		mGroundItem = item;
		mMaxHp = (m_Hp *= item._treeInfo.m_widthScale * item._treeInfo.m_widthScale);
	}

	public ESkillTargetType GetTargetType()
	{
		return NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000).m_type switch
		{
			9 => ESkillTargetType.TYPE_Wood, 
			10 => ESkillTargetType.TYPE_Herb, 
			_ => ESkillTargetType.TYPE_Herb, 
		};
	}

	public Vector3 GetPosition()
	{
		return mPos;
	}

	public int GetDestroyed(SkillRunner caster, float durDec, float radius)
	{
		if (!GameConfig.IsMultiMode)
		{
			NaturalRes terrainResData = NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000);
			if (m_Hp > 0f)
			{
				m_Hp -= durDec * terrainResData.m_duration;
				UITreeCut.Instance.SetSliderValue(mGroundItem._treeInfo, m_Hp / mMaxHp);
			}
			return 0;
		}
		return 0;
	}

	public List<ItemSample> ReturnItems(short resGotMultiplier, int num)
	{
		List<ItemSample> list = new List<ItemSample>();
		NaturalRes terrainResData = NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000);
		ItemSample[] array = new ItemSample[terrainResData.m_itemsGot.Count];
		for (int i = 0; i < terrainResData.m_itemsGot.Count; i++)
		{
			array[i] = new ItemSample(terrainResData.m_itemsGot[i].m_id, 0);
		}
		float num2 = 0f;
		num2 = ((!(terrainResData.mFixedNum > 0f)) ? ((float)resGotMultiplier + terrainResData.mSelfGetNum * mGroundItem._treeInfo.m_widthScale * mGroundItem._treeInfo.m_widthScale * mGroundItem._treeInfo.m_heightScale) : terrainResData.mFixedNum);
		for (int j = 0; j < (int)num2; j++)
		{
			int num3 = Random.Range(0, 100);
			for (int k = 0; k < terrainResData.m_itemsGot.Count; k++)
			{
				if ((float)num3 < terrainResData.m_itemsGot[k].m_probablity)
				{
					array[k].IncreaseStackCount(1);
					break;
				}
			}
		}
		List<ItemSample> list2 = new List<ItemSample>();
		if (terrainResData.m_extraGot.extraPercent > 0f && Random.value < num2 * terrainResData.m_extraGot.extraPercent)
		{
			for (int l = 0; l < terrainResData.m_extraGot.m_extraGot.Count; l++)
			{
				list2.Add(new ItemSample(terrainResData.m_extraGot.m_extraGot[l].m_id, 0));
			}
			num2 *= terrainResData.m_extraGot.extraPercent;
			for (int m = 0; (float)m < num2; m++)
			{
				int num4 = Random.Range(0, 100);
				for (int n = 0; n < terrainResData.m_extraGot.m_extraGot.Count; n++)
				{
					if ((float)num4 < terrainResData.m_extraGot.m_extraGot[n].m_probablity)
					{
						list2[n].IncreaseStackCount(1);
						break;
					}
				}
			}
		}
		for (int num5 = 0; num5 < terrainResData.m_itemsGot.Count; num5++)
		{
			if (array[num5].GetCount() > 0)
			{
				list.Add(array[num5]);
			}
		}
		foreach (ItemSample item in list2)
		{
			if (item.GetCount() > 0)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool IsDestroyed()
	{
		return m_Hp <= 0f;
	}
}
