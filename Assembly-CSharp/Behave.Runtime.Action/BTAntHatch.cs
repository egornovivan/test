using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAntHatch), "AntHatch")]
public class BTAntHatch : BTNormal
{
	private class Data
	{
		[Behave]
		public float deathTime;

		[Behave]
		public int protoID;
	}

	private Data m_Data;

	private int m_GroupMask = 1073741824;

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
			SceneMan.AddSceneObj(MonsterEntityCreator.CreateAgent(base.position, m_Data.protoID + m_GroupMask));
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
