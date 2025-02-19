using System.Collections.Generic;
using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcCampsiteSleep), "NpcCampsiteSleep")]
public class BTNpcCampsiteSleep : BTNormal
{
	private class Data
	{
		[Behave]
		public int sleepId;

		[Behave]
		public string sleepAnim;

		[Behave]
		public string sleepTimeSlots;

		private bool m_Init;

		public List<CheckSlot> slots = new List<CheckSlot>();

		public void Init(PeEntity npc)
		{
			if (m_Init)
			{
				return;
			}
			if (npc.NpcCmpt != null)
			{
				npc.NpcCmpt.npcCheck.ClearSleepSlots();
			}
			if (sleepTimeSlots != string.Empty)
			{
				string[] array = PEUtil.ToArrayString(sleepTimeSlots, ',');
				string[] array2 = array;
				foreach (string str in array2)
				{
					float[] array3 = PEUtil.ToArraySingle(str, '_');
					if (array3.Length == 2)
					{
						CheckSlot checkSlot = new CheckSlot(array3[0], array3[1]);
						slots.Add(checkSlot);
						slots.Add(checkSlot);
						if (npc.NpcCmpt != null)
						{
							npc.NpcCmpt.npcCheck.AddSleepSlots(checkSlot.minTime, checkSlot.maxTime);
						}
					}
				}
			}
			m_Init = true;
		}

		public bool IsTimeSlot(float timeSlot)
		{
			return slots.Find((CheckSlot ret) => ret.InSlot(timeSlot)) != null;
		}
	}

	private Data m_Data;

	private int Id = 30200055;

	private bool mArrived = true;

	private bool Test = true;

	private SleepPostion mSleepInfo;

	private Vector3 DirPos;

	private new void Sleep(IOperator oper)
	{
		PEActionParamVQNS param = PEActionParamVQNS.param;
		param.vec = mSleepInfo._Pos;
		param.q = Quaternion.Euler(new Vector3(0f, mSleepInfo._Rate, 0f));
		param.n = m_Data.sleepId;
		param.str = m_Data.sleepAnim;
		DoAction(PEActionType.Sleep, param);
	}

	private Vector3 GetUpPos()
	{
		return PEUtil.GetRandomPositionOnGround(mSleepInfo._Pos, 0f, 3f);
	}

	private void Getup(IOperator oper)
	{
		EndAction(PEActionType.Sleep);
	}

	private bool GetEmptyPos(out SleepPostion _SleepInfo)
	{
		_SleepInfo = new SleepPostion();
		_SleepInfo._Pos = Vector3.zero;
		_SleepInfo._Rate = 0f;
		_SleepInfo.Occpyied = false;
		_SleepInfo._Id = 0;
		for (int i = 0; i < base.Campsite.LayDatas.Count; i++)
		{
			if (!base.Campsite.LayDatas[i].Occpyied)
			{
				base.Campsite.LayDatas[i].Occpyied = true;
				_SleepInfo = base.Campsite.LayDatas[i];
				_SleepInfo._Id = base.entity.Id;
				return true;
			}
		}
		return false;
	}

	private void FreeThePos(SleepPostion _SleepInfo)
	{
		if (base.Campsite == null || base.Campsite.LayDatas == null)
		{
			return;
		}
		for (int i = 0; i < base.Campsite.LayDatas.Count; i++)
		{
			if (base.Campsite.LayDatas[i] == _SleepInfo)
			{
				base.Campsite.LayDatas[i].Occpyied = false;
			}
		}
	}

	private bool InRadiu(Vector3 self, Vector3 target, float radiu)
	{
		float num = PEUtil.SqrMagnitudeH(self, target);
		return num < radiu * radiu;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init(base.entity);
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null && InRadiu(base.position, PeSingleton<PeCreature>.Instance.mainPlayer.position, 5f))
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Sleep))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.Campsite.LayDatas == null || base.Campsite.LayDatas.Count <= 0)
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay))
		{
			return BehaveResult.Failure;
		}
		mArrived = false;
		if (mSleepInfo == null && !GetEmptyPos(out mSleepInfo))
		{
			return BehaveResult.Failure;
		}
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null && InRadiu(mSleepInfo._Pos, PeSingleton<PeCreature>.Instance.mainPlayer.position, 5f))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay))
		{
			return BehaveResult.Success;
		}
		FreeThePos(mSleepInfo);
		DirPos = mSleepInfo._Doorpos;
		return BehaveResult.Failure;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcCampsiteSleep);
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (!IsSelfSleep(base.entity.Id, out mSleepInfo))
		{
			Getup(base.Operator);
			return BehaveResult.Failure;
		}
		if (mSleepInfo == null)
		{
			return BehaveResult.Failure;
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
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Sleep))
		{
			Getup(base.Operator);
			FreeThePos(mSleepInfo);
			SetPosition(PEUtil.GetRandomPositionOnGround(base.Campsite.Pos, 0f, base.Campsite.Radius));
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			if (ContainsRequest(EReqType.Dialogue))
			{
				Getup(base.Operator);
				FreeThePos(mSleepInfo);
				return BehaveResult.Failure;
			}
			if (mArrived)
			{
				Getup(base.Operator);
				SetPosition(mSleepInfo._Doorpos);
			}
			FreeThePos(mSleepInfo);
			return BehaveResult.Failure;
		}
		if (base.attackEnemy != null)
		{
			Getup(base.Operator);
			FreeThePos(mSleepInfo);
			SetPosition(mSleepInfo._Doorpos);
			return BehaveResult.Failure;
		}
		if (!mArrived && IsReached(mSleepInfo._Pos, base.position))
		{
			StopMove();
			SetPosition(mSleepInfo._Pos);
			Sleep(base.Operator);
			mArrived = true;
		}
		if (!mArrived && !IsReached(mSleepInfo._Doorpos, base.position))
		{
			MoveToPosition(mSleepInfo._Doorpos, SpeedState.Run);
			if (Stucking())
			{
				SetPosition(mSleepInfo._Pos);
			}
		}
		if (!mArrived && IsReached(mSleepInfo._Doorpos, base.position))
		{
			SetPosition(mSleepInfo._Pos);
		}
		if (!m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay))
		{
			Getup(base.Operator);
			SetPosition(mSleepInfo._Doorpos);
			FreeThePos(mSleepInfo);
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		FreeThePos(mSleepInfo);
		mArrived = false;
	}
}
