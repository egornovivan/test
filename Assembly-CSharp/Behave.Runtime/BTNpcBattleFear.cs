using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBattleFear), "NpcBattleFear")]
public class BTNpcBattleFear : BTNormal
{
	private class Data
	{
		[Behave]
		public string PlayState;

		[Behave]
		public string PlayAnim;

		[Behave]
		public float IntervalTime;

		public string[] mAnims;

		public float mStartIntervalTime;

		public bool mRest;

		public void Init()
		{
			mAnims = PEUtil.ToArrayString(PlayAnim, ',');
		}
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (base.entity != null && base.entity.NpcCmpt != null && !base.entity.NpcCmpt.NpcInAlert && Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init();
		SetBool(m_Data.PlayState, value: true);
		SetBool(m_Data.PlayAnim, value: true);
		m_Data.mStartIntervalTime = Time.time;
		m_Data.mRest = true;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.entity.NpcCmpt.NpcInAlert && Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			StopMove();
			for (int i = 0; i < m_Data.mAnims.Length; i++)
			{
				if (GetBool(m_Data.mAnims[i]))
				{
					SetBool(m_Data.mAnims[i], value: false);
					return BehaveResult.Running;
				}
			}
			if (GetBool(m_Data.PlayState))
			{
				SetBool(m_Data.PlayAnim, value: false);
				return BehaveResult.Running;
			}
			return BehaveResult.Success;
		}
		if (base.hasAnyRequest)
		{
			for (int j = 0; j < m_Data.mAnims.Length; j++)
			{
				if (GetBool(m_Data.mAnims[j]))
				{
					SetBool(m_Data.mAnims[j], value: false);
					return BehaveResult.Running;
				}
			}
			if (GetBool(m_Data.PlayState))
			{
				SetBool(m_Data.PlayAnim, value: false);
				return BehaveResult.Running;
			}
			return BehaveResult.Success;
		}
		if (base.NpcOccpyBuild != null)
		{
			Transform transform = base.NpcOccpyBuild.Occupy(base.entity.Id);
			if (transform != null)
			{
				SetRotation(transform.rotation);
			}
		}
		if (Time.time - m_Data.mStartIntervalTime >= m_Data.IntervalTime)
		{
			SetBool(m_Data.mAnims[Random.Range(0, m_Data.mAnims.Length)], value: true);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Data == null || !m_Data.mRest)
		{
			return;
		}
		SetBool(m_Data.PlayState, value: false);
		for (int i = 0; i < m_Data.mAnims.Length; i++)
		{
			if (GetBool(m_Data.mAnims[i]))
			{
				SetBool(m_Data.PlayAnim, value: false);
			}
		}
		if (base.NpcOccpyBuild != null)
		{
			base.NpcOccpyBuild.Release(base.entity.Id);
			SetOccpyBuild(null);
		}
		m_Data.mRest = false;
	}
}
