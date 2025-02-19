using Pathea;
using UnityEngine;

public class PEAH_AreaTimeBetween : PEAH_AreaTime
{
	protected override bool IsInArea
	{
		get
		{
			int num = 0;
			if (PeGameMgr.IsCustom)
			{
				return false;
			}
			if (PeGameMgr.IsStory)
			{
				if (PeSingleton<PeMappingMgr>.Instance == null || SingleGameStory.curType != 0)
				{
					return false;
				}
				num = PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(new Vector2(base.entity.position.x, base.entity.position.z));
			}
			else
			{
				num = AiUtil.GetMapID(base.entity.position);
			}
			for (int i = 2; i < base.values.Length; i++)
			{
				if ((float)num == base.values[i])
				{
					return true;
				}
			}
			return false;
		}
	}

	public override void Update()
	{
		countTime += Time.deltaTime;
		if (Time.time > nextCountTime)
		{
			m_ElapseTime += ((!IsInArea) ? (-1f) : 1f) * countTime;
			m_ElapseTime = Mathf.Clamp(m_ElapseTime, 0f, 2f * base.values[1]);
			countTime = 0f;
			nextCountTime = Time.time + 2f * Random.value;
		}
	}

	public override float HitRate()
	{
		return (!(m_ElapseTime >= base.values[0]) || !(m_ElapseTime <= base.values[1])) ? 0f : 1f;
	}
}
