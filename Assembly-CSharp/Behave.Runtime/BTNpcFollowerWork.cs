using System;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcFollowerWork), "NpcFollowerWork")]
public class BTNpcFollowerWork : BTNormal
{
	private static Vector3 s_Position = new Vector3(0f, -10000f, 0f);

	private double m_StartTime;

	private Vector3 m_Moveposition;

	private bool m_Transparent;

	private bool m_SetPos;

	private float percent;

	private float WORKTIME = 300f;

	private Vector3 GetMovePosition()
	{
		return PEUtil.GetRandomPosition(base.position, 1024f, 2048f);
	}

	private Vector3 GetPosition()
	{
		return PEUtil.GetRandomPosition(GetMasterPosition(base.NpcMaster), 3f, 5f) + Vector3.up * 2f;
	}

	private Vector3 GetPutItemPostion()
	{
		return PEUtil.GetRandomPosition(GetMasterPosition(base.NpcMaster), 3f, 5f);
	}

	private Vector3 GetMasterPosition(ServantLeaderCmpt leader)
	{
		return leader.GetComponent<PeTrans>().position;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcFollower || !base.IsNpcFollowerWork)
		{
			return BehaveResult.Failure;
		}
		m_SetPos = false;
		m_Transparent = false;
		m_StartTime = GameTime.Timer.Minute;
		m_Moveposition = GetMovePosition();
		percent = 0f;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcFollowerWork);
		if (!base.IsNpcFollower)
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcFollowerWork)
		{
			Fadein(3f);
			GetItemsSkill(GetPutItemPostion(), percent);
			return BehaveResult.Success;
		}
		double num = GameTime.Timer.Minute - m_StartTime;
		if (num < 3.0)
		{
			MoveToPosition(m_Moveposition, SpeedState.Run);
		}
		else if (num < 4.0)
		{
			if (!m_Transparent)
			{
				Fadeout(3f);
				m_Transparent = true;
			}
		}
		else if (!m_SetPos)
		{
			SetPosition(s_Position);
			m_SetPos = true;
		}
		if (num < 60.0)
		{
			percent = Convert.ToSingle(num / 60.0);
		}
		else
		{
			percent = 1f;
			CallBackFollower();
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
	}
}
