using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using UnityEngine;

public class NpcAbilityCmpt : DataCmpt
{
	protected int mAttributeUpTimes;

	protected List<int> mSkillAbilityIds = new List<int>();

	protected List<NpcAbility> m_curNpcAblitys = new List<NpcAbility>();

	protected Dictionary<int, AblityInfo> AblityInfos = new Dictionary<int, AblityInfo>();

	public List<int> SkillAbilityIds
	{
		get
		{
			return mSkillAbilityIds;
		}
		set
		{
			mSkillAbilityIds.Clear();
			mSkillAbilityIds.AddRange(value);
			foreach (int mSkillAbilityId in mSkillAbilityIds)
			{
				AddNpcSkillAbility(mSkillAbilityId);
			}
		}
	}

	public int curAttributeUpTimes => mAttributeUpTimes;

	public NpcAbilityCmpt()
	{
		mType = ECmptType.NpcSkillAbility;
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, mSkillAbilityIds.Count);
		foreach (int mSkillAbilityId in mSkillAbilityIds)
		{
			BufferHelper.Serialize(w, mSkillAbilityId);
		}
		BufferHelper.Serialize(w, mAttributeUpTimes);
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		int num = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num; i++)
		{
			int id = BufferHelper.ReadInt32(r);
			AddNpcSkillAbility(id);
		}
		mAttributeUpTimes = BufferHelper.ReadInt32(r);
	}

	public void AddNpcSkillAbility(int id)
	{
		if (!mSkillAbilityIds.Contains(id))
		{
			mSkillAbilityIds.Add(id);
		}
		if (!m_curNpcAblitys.Exists((NpcAbility iter) => iter.id == id))
		{
			NpcAbility npcAbility = NpcAbility.FindNpcAbility(id);
			if (npcAbility != null)
			{
				m_curNpcAblitys.Add(npcAbility);
				UpdateAbilityInfo(npcAbility);
			}
		}
	}

	public void RemoveNpcSkillAbiliy(int id)
	{
		if (mSkillAbilityIds.Contains(id))
		{
			mSkillAbilityIds.Remove(id);
		}
		NpcAbility npcAbility = m_curNpcAblitys.Find((NpcAbility iter) => iter.id == id);
		if (npcAbility != null)
		{
			m_curNpcAblitys.Remove(npcAbility);
		}
		if (AblityInfos.ContainsKey(id))
		{
			AblityInfos.Remove(id);
		}
	}

	private float GetValueById(int Id)
	{
		return NpcAbility.FindNpcAbility(Id)?.Percent ?? 0f;
	}

	private List<int> GetProtoIDs(int Id)
	{
		return NpcAbility.FindNpcAbility(Id)?.ProtoIds;
	}

	private void UpdateAbilityInfo(NpcAbility ability)
	{
		if (ability != null)
		{
			if (ability.IsTalent())
			{
				AblityInfo ablityInfo = new AblityInfo();
				ability.CalculateCondtion();
				ablityInfo._Percent = GetValueById(ability.id);
				ablityInfo._ProtoIds = GetProtoIDs(ability.id);
				ablityInfo.IsTalent = true;
				ablityInfo._type = ability.Type;
				ablityInfo._Correctrate = ability.Correctrate;
				ablityInfo.DecsId = ability.desc;
				ablityInfo._icon = ability.icon;
				ablityInfo._level = ability.level;
				AblityInfos[ability.id] = ablityInfo;
			}
			else if (ability.Isskill())
			{
				AblityInfo ablityInfo2 = new AblityInfo();
				ablityInfo2.IsSkill = true;
				ability.CalculateCondtion();
				ablityInfo2.SkillId = ability.skillId;
				ablityInfo2._Skill_R = ability.SkillRange;
				ablityInfo2._Percent = ability.SkillPerCent;
				ablityInfo2._type = ability.Type;
				ablityInfo2.DecsId = ability.desc;
				ablityInfo2._icon = ability.icon;
				AblityInfos[ability.id] = ablityInfo2;
			}
			else if (ability.IsBuff())
			{
				AblityInfo ablityInfo3 = new AblityInfo();
				ablityInfo3.IsBuff = true;
				ablityInfo3.BuffId = ability.buffId;
				ablityInfo3._type = ability.Type;
				ablityInfo3.DecsId = ability.desc;
				ablityInfo3._icon = ability.icon;
				AblityInfos[ability.id] = ablityInfo3;
			}
			if (ability.IsGetItem())
			{
				AblityInfo ablityInfo4 = new AblityInfo();
				ablityInfo4.IsGetItem = true;
				ablityInfo4._Items = ability.GetItem(1f);
				ablityInfo4._type = ability.Type;
				ablityInfo4.DecsId = ability.desc;
				AblityInfos[ability.id] = ablityInfo4;
			}
		}
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
		List<MaterialItem> list = new List<MaterialItem>();
		if (m_curNpcAblitys == null)
		{
			return null;
		}
		foreach (NpcAbility curNpcAblity in m_curNpcAblitys)
		{
			if (curNpcAblity.IsGetItem())
			{
				list = curNpcAblity.GetItem(percent);
				break;
			}
		}
		if (list == null)
		{
			return null;
		}
		List<int> list2 = new List<int>();
		foreach (MaterialItem item in list)
		{
			if (item.count > 0)
			{
				list2.Add(item.protoId);
				list2.Add(item.count);
			}
		}
		if (list2 == null || list2.Count <= 0)
		{
			return null;
		}
		Quaternion rot = Quaternion.Euler(0f, random.Next(360), 0f);
		return new RandomItemObj(pos + new Vector3((float)random.NextDouble() * 0.15f, 0f, (float)random.NextDouble() * 0.15f), rot, list2.ToArray());
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

	public void AttributeUpgrade(AttribType type)
	{
		if (CanAttributeUp())
		{
			AttPlusBuffDb.Item item = AttPlusBuffDb.Get(type);
			if (item != null)
			{
				mAttributeUpTimes++;
			}
		}
	}

	public bool CanAttributeUp()
	{
		AiAdNpcNetwork aiAdNpcNetwork = base.Net as AiAdNpcNetwork;
		if (null == aiAdNpcNetwork)
		{
			return false;
		}
		return AttPlusNPCData.ComparePlusCout(aiAdNpcNetwork.ProtoId, mAttributeUpTimes);
	}
}
