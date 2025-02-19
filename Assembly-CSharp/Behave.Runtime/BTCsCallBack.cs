using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCsCallBack), "CsCallBack")]
public class BTCsCallBack : BTNormal
{
	private class Data
	{
		[Behave]
		public float walkTime;

		public float startBackTime;

		public Vector3 mDirPos;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.NpcJob == ENpcJob.Processor)
		{
			return BehaveResult.Failure;
		}
		float num = PEUtil.Magnitude(base.position, base.Creater.Assembly.Position);
		if (num <= base.Creater.Assembly.Radius)
		{
			return BehaveResult.Failure;
		}
		if (!NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mDirPos))
		{
			m_Data.mDirPos = base.Creater.Assembly.Position;
		}
		m_Data.startBackTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		float num = PEUtil.Magnitude(base.position, base.Creater.Assembly.Position);
		if (num <= base.Creater.Assembly.Radius)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt != null)
		{
			base.entity.NpcCmpt.SetCsBacking(value: true);
		}
		if (Stucking() || Time.time - m_Data.startBackTime > m_Data.walkTime)
		{
			m_Data.startBackTime = Time.time;
			if (NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mDirPos))
			{
				SetPosition(m_Data.mDirPos);
				return BehaveResult.Failure;
			}
			float num2 = PEUtil.Magnitude(base.position, base.Creater.Assembly.Position);
			if (num2 > base.Creater.Assembly.Radius)
			{
				SetPosition(base.Creater.Assembly.Position);
			}
			return BehaveResult.Running;
		}
		MoveToPosition(m_Data.mDirPos, SpeedState.Run);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (base.entity.NpcCmpt != null)
		{
			base.entity.NpcCmpt.SetCsBacking(value: false);
		}
	}
}
