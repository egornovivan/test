using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTDeath), "Death")]
public class BTDeath : BTNormal
{
	private class Data
	{
		[Behave]
		public float deathTime;
	}

	private Data m_Data;

	private float m_SpawnTime;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_SpawnTime < float.Epsilon)
		{
			m_SpawnTime = Time.time;
		}
		if (Time.time - m_SpawnTime >= m_Data.deathTime)
		{
			base.entity.SetAttribute(AttribType.Hp, 0f, offEvent: false);
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
