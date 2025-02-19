using Pathea;
using UnityEngine;

public class PEAH_AreaTime : PEAbnormalHit
{
	protected float m_ElapseTime;

	protected float countTime;

	protected float nextCountTime;

	public PeEntity entity { get; set; }

	public float[] values { get; set; }

	protected virtual bool IsInArea
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
				num = PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(new Vector2(entity.position.x, entity.position.z));
			}
			else
			{
				num = AiUtil.GetMapID(entity.position);
			}
			for (int i = 1; i < values.Length; i++)
			{
				if ((float)num == values[i])
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
			m_ElapseTime = Mathf.Clamp(m_ElapseTime, 0f, 2f * values[0]);
			countTime = 0f;
			nextCountTime = Time.time + 2f * Random.value;
		}
	}

	public override void Clear()
	{
		m_ElapseTime = 0f;
		base.Clear();
	}

	public override float HitRate()
	{
		return (!(m_ElapseTime >= values[0])) ? 0f : 1f;
	}
}
