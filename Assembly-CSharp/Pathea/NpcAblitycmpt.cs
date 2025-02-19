using System;
using System.Collections.Generic;
using ItemAsset;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class NpcAblitycmpt
{
	private Dictionary<int, AblityInfo> AblityInfos = new Dictionary<int, AblityInfo>();

	private SkEntity _SkEnity;

	private Ablities m_AblityId;

	private List<NpcAbility> m_curNpcAblitys;

	private static List<NpcAbility> mNpcSkills => PeSingleton<NpcAbility.Mgr>.Instance.mList;

	public Ablities AblityId => m_AblityId;

	public List<NpcAbility> CurNpcAblitys
	{
		get
		{
			UpdateCurablity();
			return m_curNpcAblitys;
		}
	}

	public NpcAblitycmpt(SkEntity target)
	{
		_SkEnity = target;
		m_AblityId = new Ablities(5);
	}

	public void SetAblitiyIDs(Ablities abl)
	{
		m_AblityId.Clear();
		m_AblityId.AddRange(abl);
		ReflashBuffbyIds(m_AblityId, _SkEnity);
		UpdateCurablity();
		GetAblityInfos();
	}

	private void UpdateCurablity()
	{
		if (m_AblityId != null)
		{
			if (m_curNpcAblitys == null)
			{
				m_curNpcAblitys = new List<NpcAbility>();
			}
			m_curNpcAblitys.Clear();
			m_curNpcAblitys = FindAblitysById(m_AblityId);
		}
	}

	private float GetValueById(int Id)
	{
		return FindNpcAblityById(Id)?.Percent ?? 0f;
	}

	private List<int> GetProtoIDs(int Id)
	{
		return FindNpcAblityById(Id)?.ProtoIds;
	}

	public NpcAbility GetAblity(AblityType type, SkillLevel level)
	{
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.Type == type && mNpcSkill.Level == level)
			{
				return mNpcSkill;
			}
		}
		return null;
	}

	private void ClearAbliy()
	{
		if (m_AblityId != null)
		{
			for (int i = 0; i < m_AblityId.Count; i++)
			{
				m_AblityId[i] = 0;
			}
		}
	}

	private void GetAblityInfos()
	{
		AblityInfos.Clear();
		foreach (NpcAbility curNpcAblity in m_curNpcAblitys)
		{
			if (curNpcAblity == null)
			{
				break;
			}
			if (curNpcAblity.IsTalent())
			{
				AblityInfo ablityInfo = new AblityInfo();
				curNpcAblity.CalculateCondtion();
				ablityInfo._Percent = GetValueById(curNpcAblity.id);
				ablityInfo._ProtoIds = GetProtoIDs(curNpcAblity.id);
				ablityInfo.IsTalent = true;
				ablityInfo._type = curNpcAblity.Type;
				ablityInfo._Correctrate = curNpcAblity.Correctrate;
				ablityInfo.DecsId = curNpcAblity.desc;
				ablityInfo._icon = curNpcAblity.icon;
				ablityInfo._level = curNpcAblity.level;
				AblityInfos[curNpcAblity.id] = ablityInfo;
			}
			else if (curNpcAblity.Isskill())
			{
				AblityInfo ablityInfo2 = new AblityInfo();
				ablityInfo2.IsSkill = true;
				curNpcAblity.CalculateCondtion();
				ablityInfo2.SkillId = curNpcAblity.skillId;
				ablityInfo2._Skill_R = curNpcAblity.SkillRange;
				ablityInfo2._Percent = curNpcAblity.SkillPerCent;
				ablityInfo2._type = curNpcAblity.Type;
				ablityInfo2.DecsId = curNpcAblity.desc;
				ablityInfo2._icon = curNpcAblity.icon;
				ablityInfo2._level = curNpcAblity.level;
				AblityInfos[curNpcAblity.id] = ablityInfo2;
			}
			else if (curNpcAblity.IsBuff())
			{
				AblityInfo ablityInfo3 = new AblityInfo();
				ablityInfo3.IsBuff = true;
				ablityInfo3.BuffId = curNpcAblity.buffId;
				ablityInfo3._type = curNpcAblity.Type;
				ablityInfo3.DecsId = curNpcAblity.desc;
				ablityInfo3._icon = curNpcAblity.icon;
				ablityInfo3._level = curNpcAblity.level;
				AblityInfos[curNpcAblity.id] = ablityInfo3;
			}
			if (curNpcAblity.IsGetItem())
			{
				AblityInfo ablityInfo4 = new AblityInfo();
				ablityInfo4.IsGetItem = true;
				ablityInfo4._Items = curNpcAblity.GetItem(1f);
				ablityInfo4._type = curNpcAblity.Type;
				ablityInfo4.DecsId = curNpcAblity.desc;
				ablityInfo4._level = curNpcAblity.level;
				AblityInfos[curNpcAblity.id] = ablityInfo4;
			}
		}
	}

	public bool Cur_ContainsId(int id)
	{
		for (int i = 0; i < m_AblityId.Count; i++)
		{
			if (m_AblityId[i] == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool Cur_ContainsType(AblityType type)
	{
		if (AblityInfos == null)
		{
			return false;
		}
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key]._type == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCollectSkill()
	{
		for (int i = 5; i < 9; i++)
		{
			if (Cur_ContainsType((AblityType)i))
			{
				return true;
			}
		}
		return false;
	}

	public AblityInfo Cur_GetAblityInfoById(int Id)
	{
		return (!AblityInfos.ContainsKey(Id)) ? null : AblityInfos[Id];
	}

	public AblityInfo Cur_GetAblityInfoByType(AblityType type)
	{
		if (AblityInfos == null)
		{
			return null;
		}
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key]._type == type)
			{
				return AblityInfos[key];
			}
		}
		return null;
	}

	public List<AblityInfo> Cur_GetAblityByType(AblityType type)
	{
		List<AblityInfo> list = new List<AblityInfo>();
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key]._type == type)
			{
				list.Add(AblityInfos[key]);
			}
		}
		return list;
	}

	public bool ReflashBuffById(int Id, SkEntity target)
	{
		NpcAbility npcAbility = CanAddBuff(Id);
		if (npcAbility == null)
		{
			return false;
		}
		if (_SkEnity == null)
		{
			return false;
		}
		return npcAbility.RefreshBuff(target);
	}

	public void ReflashBuffbyIds(Ablities ids, SkEntity target)
	{
		if (!(target == null) && ids != null)
		{
			for (int i = 0; i < ids.Count; i++)
			{
				ReflashBuffById(ids[i], target);
			}
		}
	}

	public float GetTalentPercent(AblityType type)
	{
		if (AblityInfos.Count == 0)
		{
			return 0f;
		}
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key]._type == type)
			{
				return AblityInfos[key]._Percent;
			}
		}
		return 0f;
	}

	public float GetCorrectRate(AblityType type)
	{
		if (AblityInfos.Count == 0)
		{
			return 0f;
		}
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key]._type == type)
			{
				return AblityInfos[key]._Correctrate;
			}
		}
		return 0f;
	}

	public RandomItemObj TryGetItemskill(Vector3 pos, float percent = 1f)
	{
		System.Random random = new System.Random();
		List<List<MaterialItem>> list = new List<List<MaterialItem>>();
		if (m_curNpcAblitys == null)
		{
			return null;
		}
		foreach (NpcAbility curNpcAblity in m_curNpcAblitys)
		{
			if (curNpcAblity.IsGetItem())
			{
				list.Add(curNpcAblity.GetItem(percent));
			}
		}
		if (list == null)
		{
			return null;
		}
		List<int> list2 = new List<int>();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == null || list[i].Count <= 0)
			{
				continue;
			}
			foreach (MaterialItem item in list[i])
			{
				if (item.count > 0)
				{
					list2.Add(item.protoId);
					list2.Add(item.count);
				}
			}
		}
		if (list2 == null || list2.Count <= 0)
		{
			return null;
		}
		return new RandomItemObj(pos + new Vector3((float)random.NextDouble() * 0.15f, 0f, (float)random.NextDouble() * 0.15f), list2.ToArray());
	}

	public List<int> GetProtoIds(AblityType type)
	{
		if (AblityInfos.Count == 0)
		{
			return null;
		}
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key]._type == type)
			{
				return AblityInfos[key]._ProtoIds;
			}
		}
		return null;
	}

	public List<int> GetSkillIDs()
	{
		List<int> list = new List<int>();
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key].IsSkill)
			{
				list.Add(AblityInfos[key].SkillId);
			}
		}
		return list;
	}

	public float GetCmptSkillRange(int SkillId)
	{
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key].IsSkill && AblityInfos[key].SkillId == SkillId)
			{
				return AblityInfos[key]._Skill_R;
			}
		}
		return 0f;
	}

	public AblityType GetSkillType(int skillid)
	{
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key].IsSkill && AblityInfos[key].SkillId == skillid)
			{
				return AblityInfos[key]._type;
			}
		}
		return AblityType.Max;
	}

	public float GetChangeHpPer(int SkillId)
	{
		foreach (int key in AblityInfos.Keys)
		{
			if (AblityInfos[key].IsSkill && AblityInfos[key].SkillId == SkillId)
			{
				return AblityInfos[key]._Percent;
			}
		}
		return 0f;
	}

	public int GetCanLearnId(int learnId)
	{
		return CanLearnAblity(learnId)?.id ?? 0;
	}

	private NpcAbility CanLearnAblity(int learnId)
	{
		NpcAbility npcAbility = FindNpcAblityById(learnId);
		if (npcAbility == null || npcAbility.Type == AblityType.Max)
		{
			return null;
		}
		if (Cur_ContainsType(npcAbility.Type))
		{
			AblityInfo ablityInfo = Cur_GetAblityInfoByType(npcAbility.Type);
			if (ablityInfo._level >= 4)
			{
				return null;
			}
			if (npcAbility.level == ablityInfo._level)
			{
				return null;
			}
			if (npcAbility.level > ablityInfo._level)
			{
				return FindNpcAblity(npcAbility.Type, ablityInfo._level + 1);
			}
		}
		return FindNpcAblity(npcAbility.Type, 1);
	}

	private static NpcAbility CanAddBuff(int Id)
	{
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.id == Id)
			{
				if (mNpcSkill.IsBuff())
				{
					return mNpcSkill;
				}
				return null;
			}
		}
		return null;
	}

	public static NpcAbility FindNpcAblityById(int Id)
	{
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.id == Id)
			{
				return mNpcSkill;
			}
		}
		return null;
	}

	public static NpcAbility FindNpcAblity(AblityType type, int level)
	{
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.Type == type && mNpcSkill.level == level)
			{
				return mNpcSkill;
			}
		}
		return null;
	}

	public static List<NpcAbility> FindAblitysById(Ablities Ids)
	{
		if (Ids == null)
		{
			return null;
		}
		List<NpcAbility> list = new List<NpcAbility>();
		for (int i = 0; i < Ids.Count; i++)
		{
			list.Add(FindNpcAblityById(Ids[i]));
		}
		return list;
	}

	public static NpcAbility FindNpcAblityBySkillId(int SkillId)
	{
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.skillId == SkillId)
			{
				return mNpcSkill;
			}
		}
		return null;
	}

	public static List<NpcAbility> GetAbilityByType(AblityType type)
	{
		List<NpcAbility> list = new List<NpcAbility>();
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.Type == type)
			{
				list.Add(mNpcSkill);
			}
		}
		return list;
	}

	private static AblityType GetAblityType(int Id)
	{
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.id == Id)
			{
				return mNpcSkill.Type;
			}
		}
		return AblityType.Max;
	}

	private static List<int> GetTheSameTypeAbliy(int id)
	{
		List<int> list = new List<int>();
		foreach (NpcAbility mNpcSkill in mNpcSkills)
		{
			if (mNpcSkill.id == id)
			{
				list.Add(id);
			}
		}
		return list;
	}

	public static List<int> GetCoverAbilityId(int cur_ablityId)
	{
		List<int> list = new List<int>();
		List<NpcAbility> list2 = CoverAblity(cur_ablityId);
		if (list2 == null || list2.Count <= 0)
		{
			return list;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			list.Add(list2[i].id);
		}
		return list;
	}

	public static float GetLearnTime(int abilityid)
	{
		return FindNpcAblityById(abilityid)?.learnTime ?? 0f;
	}

	private static List<NpcAbility> CoverAblity(int abliyId)
	{
		List<NpcAbility> list = new List<NpcAbility>();
		NpcAbility npcAbility = FindNpcAblityById(abliyId);
		if (npcAbility == null)
		{
			return list;
		}
		List<NpcAbility> abilityByType = GetAbilityByType(npcAbility.Type);
		if (abilityByType == null)
		{
			return list;
		}
		foreach (NpcAbility item in abilityByType)
		{
			if (npcAbility.level > item.level)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static SkillLevel GetSkilllevel(int Id)
	{
		return FindNpcAblityById(Id)?.Level ?? SkillLevel.none;
	}

	public static Ablities CompareSkillType(List<int> _ablityIds)
	{
		Ablities ablities = new Ablities(_ablityIds.Count);
		ablities.AddRange(_ablityIds);
		if (ablities.Count <= 1)
		{
			return ablities;
		}
		if (ablities.Count == 2)
		{
			int num = ablities[0];
			int num2 = ablities[1];
			NpcAbility npcAbility = FindNpcAblityById(num);
			if (npcAbility == null)
			{
				Debug.LogError("Random Ability error. no Ability!!!   " + num);
				return ablities;
			}
			NpcAbility npcAbility2 = FindNpcAblityById(num2);
			if (npcAbility == null)
			{
				Debug.LogError("Random Ability error. no Ability!!!   " + num2);
				return ablities;
			}
			if (npcAbility.Type == npcAbility2.Type)
			{
				if (npcAbility.level - npcAbility2.level > 0)
				{
					ablities.Remove(num2);
				}
				else
				{
					ablities.Remove(num);
				}
			}
		}
		return ablities;
	}
}
