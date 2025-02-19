using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTPlayLoopAnimation), "PlayLoopAnimation")]
public class BTPlayLoopAnimation : BTNormal
{
	private class Data
	{
		[Behave]
		public string PlayState;

		[Behave]
		public string PlayAnim;

		[Behave]
		public string PlayTimes;

		[Behave]
		public float IntervalTime;

		[Behave]
		public float MinTime;

		[Behave]
		public float MaxTime;

		public int AnimPlayTimes;

		public float mStartPlayTime;

		public float m_starIntervlTime;

		public bool InRadius(Vector3 self, Vector3 buidPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(self, buidPos);
			return num < radiu * radiu;
		}
	}

	private Data m_Data;

	private float actionTime;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Work))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		m_Data.mStartPlayTime = Time.time;
		m_Data.m_starIntervlTime = Time.time;
		SetBool(m_Data.PlayState, value: true);
		SetBool(m_Data.PlayAnim, value: true);
		actionTime = Random.Range(m_Data.MinTime, m_Data.MaxTime);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || base.hasAnyRequest || base.entity.IsNpcInSleepTime || base.entity.IsNpcInDinnerTime || !NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Work))
		{
			StopMove();
			if (GetBool(m_Data.PlayAnim))
			{
				SetBool(m_Data.PlayAnim, value: false);
				return BehaveResult.Running;
			}
			if (GetBool(m_Data.PlayState))
			{
				SetBool(m_Data.PlayState, value: false);
				return BehaveResult.Running;
			}
			return BehaveResult.Failure;
		}
		if (base.NpcOccpyBuild != null)
		{
			Transform transform = base.NpcOccpyBuild.Occupy(base.entity.Id);
			if (transform != null)
			{
				SetRotation(transform.rotation);
			}
		}
		if (Time.time - m_Data.mStartPlayTime >= actionTime)
		{
			StopMove();
			if (GetBool(m_Data.PlayAnim))
			{
				SetBool(m_Data.PlayAnim, value: false);
				return BehaveResult.Running;
			}
			if (GetBool(m_Data.PlayState))
			{
				SetBool(m_Data.PlayState, value: false);
				return BehaveResult.Running;
			}
			return BehaveResult.Success;
		}
		if (Time.time - m_Data.m_starIntervlTime >= m_Data.IntervalTime)
		{
			if (base.NpcOccpyBuild != null && base.NpcOccpyBuild.Occupy(base.entity.Id) != null && !m_Data.InRadius(base.position, base.NpcOccpyBuild.Occupy(base.entity.Id).transform.position))
			{
				StopMove();
				if (GetBool(m_Data.PlayAnim))
				{
					SetBool(m_Data.PlayAnim, value: false);
					return BehaveResult.Running;
				}
				if (GetBool(m_Data.PlayState))
				{
					SetBool(m_Data.PlayState, value: false);
					return BehaveResult.Running;
				}
				return BehaveResult.Failure;
			}
			m_Data.m_starIntervlTime = Time.time;
			SetBool(m_Data.PlayAnim, value: true);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		SetBool(m_Data.PlayAnim, value: false);
		SetBool(m_Data.PlayState, value: false);
		if (base.NpcOccpyBuild != null)
		{
			base.NpcOccpyBuild.Release(base.entity.Id);
			SetOccpyBuild(null);
		}
	}
}
