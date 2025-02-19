using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTurnOnSpot), "TurnOnSpot")]
public class BTTurnOnSpot : BTNormal
{
	private class Data
	{
		[Behave]
		public string turn = string.Empty;

		[Behave]
		public int skillID;

		public float m_StartTime;

		public Vector3 m_Direction;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_Direction = base.attackEnemy.position - base.position;
		float angle = PEUtil.GetAngle(m_Data.m_Direction, base.transform.forward, Vector3.up);
		m_Data.m_StartTime = Time.time;
		FaceDirection(m_Data.m_Direction);
		SetBool(m_Data.turn, value: true);
		SetFloat("Angle", angle);
		StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (PEUtil.Angle(m_Data.m_Direction, base.transform.forward, Vector3.up) > 5f && Time.time - m_Data.m_StartTime <= 3f)
		{
			FaceDirection(m_Data.m_Direction);
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
