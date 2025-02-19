using System.Collections.Generic;

namespace SkillAsset;

public class EffSkillBuffManager
{
	internal List<EffSkillBuffInst> m_effBuffInstList = new List<EffSkillBuffInst>();

	private Dictionary<int, List<EffSkillBuffInst>> mTypeManagedMap = new Dictionary<int, List<EffSkillBuffInst>>();

	internal bool m_bEffBuffDirty = true;

	public bool Add(EffSkillBuffInst buffInst)
	{
		if (!mTypeManagedMap.ContainsKey(buffInst.m_buff.m_buffType))
		{
			mTypeManagedMap[buffInst.m_buff.m_buffType] = new List<EffSkillBuffInst>();
		}
		if (mTypeManagedMap[buffInst.m_buff.m_buffType].Count > 0)
		{
			if (mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_buff.m_id == buffInst.m_buff.m_id)
			{
				if (mTypeManagedMap[buffInst.m_buff.m_buffType].Count == buffInst.m_buff.m_StackLimit)
				{
					m_effBuffInstList.Remove(mTypeManagedMap[buffInst.m_buff.m_buffType][0]);
					if (mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_runner != null)
					{
						mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_runner.stop = true;
					}
					mTypeManagedMap[buffInst.m_buff.m_buffType].RemoveAt(0);
				}
			}
			else
			{
				if (mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_buff.m_Priority > buffInst.m_buff.m_Priority)
				{
					return false;
				}
				foreach (EffSkillBuffInst item in mTypeManagedMap[buffInst.m_buff.m_buffType])
				{
					if (item.m_runner != null)
					{
						item.m_runner.stop = true;
					}
					m_effBuffInstList.Remove(item);
				}
				mTypeManagedMap[buffInst.m_buff.m_buffType].Clear();
			}
		}
		mTypeManagedMap[buffInst.m_buff.m_buffType].Add(buffInst);
		m_effBuffInstList.Add(buffInst);
		m_bEffBuffDirty = true;
		return true;
	}

	public void Remove(EffSkillBuffInst buffInst)
	{
		if (buffInst.m_runner != null)
		{
			buffInst.m_runner.stop = true;
		}
		if (mTypeManagedMap.ContainsKey(buffInst.m_buff.m_buffType))
		{
			mTypeManagedMap[buffInst.m_buff.m_buffType].Remove(buffInst);
			m_effBuffInstList.Remove(buffInst);
			m_bEffBuffDirty = true;
		}
	}

	public void Remove(int skillId)
	{
		if (skillId == 0)
		{
			return;
		}
		for (int i = 0; i < m_effBuffInstList.Count; i++)
		{
			if (m_effBuffInstList[i].m_skillId != skillId)
			{
				continue;
			}
			int buffType = m_effBuffInstList[i].m_buff.m_buffType;
			if (!mTypeManagedMap.ContainsKey(buffType))
			{
				break;
			}
			foreach (EffSkillBuffInst item in mTypeManagedMap[buffType])
			{
				if (item.m_runner != null)
				{
					item.m_runner.stop = true;
				}
				m_effBuffInstList.Remove(item);
			}
			mTypeManagedMap[buffType].Clear();
			m_bEffBuffDirty = true;
			break;
		}
	}

	public void AddBuff(EffSkillBuff addBuff)
	{
		EffSkillBuffInst effSkillBuffInst = new EffSkillBuffInst();
		effSkillBuffInst.m_buff = addBuff;
		Add(effSkillBuffInst);
	}

	public void RemoveBuff(EffSkillBuff removeBuff)
	{
		int buffType = removeBuff.m_buffType;
		if (!mTypeManagedMap.ContainsKey(buffType))
		{
			return;
		}
		foreach (EffSkillBuffInst item in mTypeManagedMap[buffType])
		{
			if (item.m_runner != null)
			{
				item.m_runner.stop = true;
			}
			m_effBuffInstList.Remove(item);
		}
		mTypeManagedMap[buffType].Clear();
		m_bEffBuffDirty = true;
	}

	public EffSkillBuff GetBuff(int ID)
	{
		EffSkillBuff result = null;
		foreach (EffSkillBuffInst effBuffInst in m_effBuffInstList)
		{
			if (effBuffInst.m_buff.m_id == ID)
			{
				result = effBuffInst.m_buff;
				break;
			}
		}
		return result;
	}

	public bool IsAppendBuff(int buffId)
	{
		for (int i = 0; i < m_effBuffInstList.Count; i++)
		{
			if (m_effBuffInstList[i].m_buff.m_id == buffId)
			{
				return true;
			}
		}
		return false;
	}
}
