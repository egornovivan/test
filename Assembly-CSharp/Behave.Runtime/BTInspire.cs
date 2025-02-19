using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTInspire), "Inspire")]
public class BTInspire : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float radius;

		[Behave]
		public int skillId;

		[Behave]
		public string anim = string.Empty;
	}

	private Data m_Data;

	private float m_LastTime = -1000f;

	private void Inspire()
	{
		SetBool(m_Data.anim, value: true);
		List<PeEntity> entitiesFriendly = PeSingleton<EntityMgr>.Instance.GetEntitiesFriendly(base.position, m_Data.radius, (int)GetAttribute(AttribType.DefaultPlayerID), base.entity.ProtoID, isDeath: false, base.entity);
		for (int i = 0; i < entitiesFriendly.Count; i++)
		{
			StartSkill(entitiesFriendly[i].GetComponent<PeEntity>(), m_Data.skillId);
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_LastTime < m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		Inspire();
		m_LastTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (GetBool("Inspire"))
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
