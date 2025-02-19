using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcCampsiteEat), "NpcCampsiteEat")]
public class BTNpcCampsiteEat : BTNormal
{
	private class Data
	{
		public class AnimSlot
		{
			public bool bplay;

			public string playState = string.Empty;

			public string animName = string.Empty;

			public AnimSlot(string state, string anim)
			{
				if (state != "0")
				{
					playState = state;
					bplay = true;
				}
				else
				{
					bplay = false;
				}
				animName = anim;
			}
		}

		[Behave]
		public string EatHour;

		[Behave]
		public string AnimName;

		[Behave]
		public float PlayTime;

		public List<CheckSlot> slots;

		private List<AnimSlot> AnimSlots;

		public PEBuilding mBuild;

		public float mStartEatTime;

		public AnimSlot CurSlot;

		private bool mInit;

		public void Init()
		{
			if (mInit)
			{
				return;
			}
			slots = new List<CheckSlot>();
			string[] array = PEUtil.ToArrayString(EatHour, ',');
			for (int i = 0; i < array.Length; i++)
			{
				float[] array2 = PEUtil.ToArraySingle(array[i], '_');
				if (array2.Length == 2)
				{
					slots.Add(new CheckSlot(array2[0], array2[1]));
				}
			}
			AnimSlots = new List<AnimSlot>();
			string[] array3 = PEUtil.ToArrayString(AnimName, ',');
			for (int j = 0; j < array3.Length; j++)
			{
				string[] array4 = PEUtil.ToArrayString(array3[j], '_');
				if (array4.Length == 2)
				{
					AnimSlots.Add(new AnimSlot(array4[0], array4[1]));
				}
			}
			mInit = true;
		}

		public bool GetEatPostions(int AssetId)
		{
			PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(AssetId);
			if (doodadEntities == null || doodadEntities.Length <= 0)
			{
				return false;
			}
			mBuild = doodadEntities[0].Doodabuid;
			if (mBuild == null)
			{
				return false;
			}
			mBuild.SetFoodShowSlots(slots);
			return true;
		}

		public bool IsReached(Vector3 pos, Vector3 targetPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(pos, targetPos);
			return num < radiu * radiu;
		}

		public bool RangeAnim()
		{
			if (AnimSlots.Count == 0)
			{
				return false;
			}
			CurSlot = AnimSlots[Random.Range(0, AnimSlots.Count)];
			return true;
		}

		public bool InTimeSlot(float curTime)
		{
			return slots.Find((CheckSlot ret) => ret.InSlot(curTime)) != null;
		}
	}

	private Data m_Data;

	private Vector3 eatpostion;

	private Transform mTrans;

	private BehaveResult End()
	{
		if (m_Data.CurSlot.bplay)
		{
			if (GetBool(m_Data.CurSlot.playState))
			{
				SetBool(m_Data.CurSlot.playState, value: false);
				return BehaveResult.Running;
			}
			if (GetBool(m_Data.CurSlot.animName))
			{
				SetBool(m_Data.CurSlot.animName, value: false);
				return BehaveResult.Running;
			}
		}
		else if (IsMotionRunning(PEActionType.Eat))
		{
			EndAction(PEActionType.Eat);
			return BehaveResult.Running;
		}
		return BehaveResult.Failure;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init();
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		m_Data.RangeAnim();
		if (base.hasAttackEnemy)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Dining))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (base.Campsite == null || base.Campsite.mEatInfo == null)
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.InTimeSlot((float)GameTime.Timer.HourInDay))
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.GetEatPostions(base.Campsite.mEatInfo.assesID))
		{
			return BehaveResult.Failure;
		}
		mTrans = m_Data.mBuild.Occupy(base.entity.Id);
		if (mTrans == null)
		{
			return BehaveResult.Failure;
		}
		eatpostion = base.Campsite.GetObjectPostion(base.Campsite.mEatInfo.assesID);
		if (eatpostion == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		m_Data.mStartEatTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcCampsiteEat);
		if (!base.IsNpcCampsite || base.hasAttackEnemy || base.hasAnyRequest || base.entity.NpcCmpt.NpcInAlert || !NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Dining) || mTrans == null || !m_Data.InTimeSlot((float)GameTime.Timer.HourInDay))
		{
			StopMove();
			return End();
		}
		if (GetBool("OperateMed"))
		{
			SetBool("OperateMed", value: false);
		}
		if (GetBool("OperateCom"))
		{
			SetBool("OperateCom", value: false);
		}
		if (GetBool("FixLifeboat"))
		{
			SetBool("FixLifeboat", value: false);
		}
		if (m_Data.IsReached(base.position, mTrans.position))
		{
			if (Time.time - m_Data.mStartEatTime >= m_Data.PlayTime)
			{
				if (m_Data.CurSlot.bplay)
				{
					if (!GetBool(m_Data.CurSlot.playState))
					{
						SetBool(m_Data.CurSlot.playState, value: true);
						return BehaveResult.Running;
					}
					if (!GetBool(m_Data.CurSlot.animName))
					{
						SetBool(m_Data.CurSlot.animName, value: true);
						return BehaveResult.Running;
					}
				}
				else
				{
					m_Data.mStartEatTime = Time.time;
					PEActionParamS param = PEActionParamS.param;
					param.str = m_Data.CurSlot.animName;
					DoAction(PEActionType.Eat, param);
				}
				FaceDirection(eatpostion - base.position);
			}
		}
		else
		{
			if (Stucking())
			{
				SetPosition(mTrans.position);
			}
			MoveToPosition(mTrans.position, SpeedState.Run, avoid: false);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (IsMotionRunning(PEActionType.Eat))
		{
			EndAction(PEActionType.Eat);
		}
		if (m_Data != null && m_Data.CurSlot != null)
		{
			SetBool(m_Data.CurSlot.playState, value: false);
			SetBool(m_Data.CurSlot.animName, value: false);
		}
		if (m_Data != null && m_Data.mBuild != null)
		{
			m_Data.mBuild.Release(base.entity.Id);
		}
	}
}
