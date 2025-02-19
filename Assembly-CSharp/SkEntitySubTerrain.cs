using System;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using UnityEngine;

public class SkEntitySubTerrain : SkEntity
{
	private static SkEntitySubTerrain m_Instance;

	private Dictionary<Vector3, float> treeHPInfos = new Dictionary<Vector3, float>();

	public static SkEntitySubTerrain Instance
	{
		get
		{
			if (null == m_Instance)
			{
				m_Instance = new GameObject("SkEntitySubTerrain").AddComponent<SkEntitySubTerrain>();
				m_Instance.InitSkEntity();
				if (GameConfig.IsMultiMode && SubTerrainNetwork.Instance != null)
				{
					SubTerrainNetwork.Instance.Init();
				}
			}
			return m_Instance;
		}
	}

	private event Action<SkEntity, GlobalTreeInfo> onTreeCutDown;

	public void AddListener(Action<SkEntity, GlobalTreeInfo> listener)
	{
		this.onTreeCutDown = (Action<SkEntity, GlobalTreeInfo>)Delegate.Combine(this.onTreeCutDown, listener);
	}

	public void RemoveListener(Action<SkEntity, GlobalTreeInfo> listener)
	{
		this.onTreeCutDown = (Action<SkEntity, GlobalTreeInfo>)Delegate.Remove(this.onTreeCutDown, listener);
	}

	public float GetTreeHP(Vector3 treeInfo)
	{
		if (!treeHPInfos.ContainsKey(treeInfo))
		{
			treeHPInfos[treeInfo] = 255f;
		}
		return treeHPInfos[treeInfo];
	}

	public void SetTreeHp(Vector3 tree, float hp)
	{
		foreach (Vector3 key in treeHPInfos.Keys)
		{
			if (key == tree)
			{
				treeHPInfos[key] = hp;
				break;
			}
		}
	}

	private void InitSkEntity()
	{
		Init(onAlterAttribs, null, 4);
	}

	private void onAlterAttribs(int idx, float oldValue, float newValue)
	{
		if (idx != 2)
		{
			return;
		}
		float damage = _attribs.sums[0];
		float bouns = _attribs.sums[1];
		bool flag = _attribs.sums[2] > 0f;
		SkEntity casterToModAttrib = GetCasterToModAttrib(idx);
		if (!(null != casterToModAttrib) || !(casterToModAttrib is ISkSubTerrain { treeInfo: not null } skSubTerrain))
		{
			return;
		}
		treeHPInfos[skSubTerrain.treeInfo.WorldPos] = DigTerrainManager.Fell(skSubTerrain.treeInfo, damage, GetTreeHP(skSubTerrain.treeInfo.WorldPos));
		if (GameConfig.IsMultiMode)
		{
			casterToModAttrib.SendFellTree(skSubTerrain.treeInfo._treeInfo.m_protoTypeIdx, skSubTerrain.treeInfo.WorldPos, skSubTerrain.treeInfo._treeInfo.m_heightScale, skSubTerrain.treeInfo._treeInfo.m_widthScale);
		}
		else
		{
			if (!(treeHPInfos[skSubTerrain.treeInfo.WorldPos] <= 0f))
			{
				return;
			}
			OnTreeCutDown(casterToModAttrib, skSubTerrain.treeInfo);
			DigTerrainManager.RemoveTree(skSubTerrain.treeInfo);
			if (!flag)
			{
				return;
			}
			bool bGetSpItems = false;
			if (casterToModAttrib is SkAliveEntity)
			{
				SkAliveEntity skAliveEntity = (SkAliveEntity)casterToModAttrib;
				if (skAliveEntity.Entity.proto == EEntityProto.Player)
				{
					SkillTreeUnitMgr cmpt = skAliveEntity.Entity.GetCmpt<SkillTreeUnitMgr>();
					bGetSpItems = cmpt.CheckMinerGetRare();
				}
			}
			Dictionary<int, int> treeResouce = DigTerrainManager.GetTreeResouce(skSubTerrain.treeInfo, bouns, bGetSpItems);
			if (treeResouce.Count <= 0)
			{
				return;
			}
			List<int> plantList = new List<int>(treeResouce.Count * 2);
			foreach (int key in treeResouce.Keys)
			{
				plantList.Add(key);
				plantList.Add(treeResouce[key]);
			}
			GetSpecialItem.PlantItemAdd(ref plantList);
			casterToModAttrib._attribs.pack += plantList.ToArray();
		}
	}

	public void OnTreeCutDown(SkEntity skEntity, GlobalTreeInfo treeInfo)
	{
		if (this.onTreeCutDown != null)
		{
			this.onTreeCutDown(skEntity, treeInfo);
		}
	}
}
