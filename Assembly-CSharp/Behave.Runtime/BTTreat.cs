using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTTreat), "Treat")]
public class BTTreat : BTNormal
{
	private class Data
	{
		[Behave]
		public int skillId;

		[Behave]
		public string anim = string.Empty;
	}

	private Data m_Data;

	private bool m_Treat;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Treat == null || base.entity.Treat.IsDeath() || !base.entity.Treat.hasView)
		{
			return BehaveResult.Failure;
		}
		m_Treat = false;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Treat == null || base.entity.Treat.IsDeath() || !base.entity.Treat.hasView)
		{
			return BehaveResult.Failure;
		}
		if (!base.entity.Treat.IsSeriousInjury)
		{
			return BehaveResult.Success;
		}
		Vector3 vector = base.entity.Treat.position;
		if (PEUtil.SqrMagnitudeH(base.position, vector) > 4f)
		{
			MoveToPosition(vector, SpeedState.Run);
		}
		else
		{
			MoveToPosition(Vector3.zero);
			Vector3 vector2 = Vector3.ProjectOnPlane(base.entity.Treat.position - base.position, Vector3.up);
			Vector3 to = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up);
			if (Vector3.Angle(vector2, to) > 5f)
			{
				FaceDirection(vector2);
			}
			else
			{
				FaceDirection(Vector3.zero);
				if (!m_Treat)
				{
					m_Treat = true;
					base.entity.Treat.StopSkill(30100326);
					SetBool(m_Data.anim, value: true);
					StartSkill(base.entity.Treat, m_Data.skillId);
				}
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && base.entity.Treat != null)
		{
			SetBool(m_Data.anim, value: false);
			StopSkill(m_Data.skillId);
			base.entity.Treat = null;
		}
	}
}
