using System.Collections.Generic;
using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBaseTakeFood), "NpcBaseTakeFood")]
public class BTNpcBaseTakeFood : BTNormal
{
	private class Data
	{
		[Behave]
		public string eatAnim;

		[Behave]
		public float minHunger;

		[Behave]
		public float maxHunger;

		[Behave]
		public float minEatTime;

		[Behave]
		public float maxEatTime;

		[Behave]
		public float full;

		[Behave]
		public float interval;

		[Behave]
		public string eatTimeSlots;

		[Behave]
		public string eatIdsStr;

		[Behave]
		public float minBlood;

		[Behave]
		public float maxBlood;

		[Behave]
		public float minComfort;

		[Behave]
		public float maxComfort;

		public List<CheckSlot> slots = new List<CheckSlot>();

		public string[] eatAnims;

		public string CurEatAnim;

		public float m_StartEatTime;

		public float m_CurEatTime;

		public float m_LastEatTime;

		private bool m_Init;

		private CSStorage m_Storage;

		private IOperation m_Table;

		private IOperation m_Tables;

		public CSStorage Storage
		{
			get
			{
				return m_Storage;
			}
			set
			{
				if (m_Storage == value)
				{
					return;
				}
				m_Storage = value;
				if (m_Storage != null && m_Storage.gameLogic != null)
				{
					m_Tables = m_Storage.gameLogic.GetComponent<PETable>();
					m_Table = m_Storage.gameLogic.GetComponent<PETable>().Singles.Find((Operation_Single ret) => ret != null && ret.CanOperateMask(EOperationMask.Eat));
				}
			}
		}

		public IOperation Table => m_Table;

		public IOperation Tables => m_Tables;

		public void Init(PeEntity npc)
		{
			if (m_Init)
			{
				return;
			}
			if (npc.NpcCmpt != null)
			{
				npc.NpcCmpt.npcCheck.ClearEatSlots();
			}
			if (eatTimeSlots != string.Empty)
			{
				string[] array = PEUtil.ToArrayString(eatTimeSlots, ',');
				string[] array2 = array;
				foreach (string str in array2)
				{
					float[] array3 = PEUtil.ToArraySingle(str, '_');
					if (array3.Length == 2)
					{
						CheckSlot checkSlot = new CheckSlot(array3[0], array3[1]);
						slots.Add(checkSlot);
						if (npc.NpcCmpt != null)
						{
							npc.NpcCmpt.npcCheck.AddEatSlots(checkSlot.minTime, checkSlot.maxTime);
						}
					}
				}
			}
			if (eatAnim != string.Empty)
			{
				eatAnims = PEUtil.ToArrayString(eatAnim, ',');
			}
			CurEatAnim = eatAnims[Random.Range(0, eatAnims.Length - 1)];
			m_Init = true;
		}

		public void InitTable()
		{
			if (m_Storage != null && m_Storage.gameLogic != null)
			{
				m_Tables = m_Storage.gameLogic.GetComponent<PETable>();
				m_Table = m_Storage.gameLogic.GetComponent<PETable>().Singles.Find((Operation_Single ret) => ret != null && ret.CanOperateMask(EOperationMask.Eat));
			}
		}

		public bool IsTimeSlot(float timeSlot)
		{
			return slots.Find((CheckSlot ret) => ret.InSlot(timeSlot)) != null;
		}

		private bool IspooolHunger(PeEntity entity)
		{
			float num = entity.GetAttribute(AttribType.Hunger) / entity.GetAttribute(AttribType.HungerMax);
			return num < minHunger;
		}

		private bool IspooolHp(PeEntity entity)
		{
			float num = entity.GetAttribute(AttribType.Hp) / entity.GetAttribute(AttribType.HpMax);
			return num < minBlood;
		}

		private bool IspooolComfort(PeEntity entity)
		{
			float num = entity.GetAttribute(AttribType.Comfort) / entity.GetAttribute(AttribType.ComfortMax);
			return num < minComfort;
		}

		private bool NotHunger(PeEntity entity)
		{
			float num = entity.GetAttribute(AttribType.Hunger) / entity.GetAttribute(AttribType.HungerMax);
			return num > maxHunger;
		}

		private bool NotHp(PeEntity entity)
		{
			float num = entity.GetAttribute(AttribType.Hp) / entity.GetAttribute(AttribType.HpMax);
			return num > maxBlood;
		}

		private bool NotComfort(PeEntity entity)
		{
			float num = entity.GetAttribute(AttribType.Comfort) / entity.GetAttribute(AttribType.ComfortMax);
			return num > maxComfort;
		}

		public bool BeDispatchEat(PeEntity entity)
		{
			return IspooolHp(entity) || IspooolComfort(entity) || IspooolHunger(entity);
		}

		public bool IsEndDispatch(PeEntity entity)
		{
			return NotHp(entity) && NotHunger(entity) && NotComfort(entity);
		}

		public void NpcTalkSth(PeEntity npc)
		{
			List<NpcRandomTalkDb.Item> talkItems = NpcRandomTalkDb.GetTalkItems(npc);
			for (int i = 0; i < talkItems.Count; i++)
			{
				if (talkItems[i] != null && talkItems[i].Type != AttribType.Max && talkItems[i].Level != ETalkLevel.Max)
				{
					npc.NpcCmpt.AddTalkInfo(talkItems[i].TalkType, ENpcSpeakType.Both, canLoop: true);
				}
			}
		}
	}

	private Data m_Data;

	private EThinkingType mDiningTh = EThinkingType.Dining;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		m_Data.Init(base.entity);
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Dining))
		{
			return BehaveResult.Failure;
		}
		if (!NpcThinkDb.CanDo(base.entity, mDiningTh))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.lineType != ELineType.TeamEat)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.Storage == null || m_Data.Storage.gameLogic == null)
		{
			m_Data.Storage = CSUtils.GetTargetStorage(NpcEatDb.GetWantEatIds(base.entity), base.entity);
		}
		if (m_Data.Table == null || m_Data.Tables == null || m_Data.Table.Equals(null) || m_Data.Tables.Equals(null))
		{
			m_Data.InitTable();
		}
		if (m_Data.Storage == null || m_Data.Table == null || m_Data.Tables == null || m_Data.Table.Equals(null) || m_Data.Tables.Equals(null))
		{
			if (NpcEatDb.IsNeedEatsth(base.entity) && Enemy.IsNullOrInvalid(base.attackEnemy))
			{
				m_Data.NpcTalkSth(base.entity);
			}
			base.entity.NpcCmpt.ThinkAgent.RemoveThink(mDiningTh);
			return BehaveResult.Failure;
		}
		if (!NpcEatDb.CanEatFromStorage(base.entity, m_Data.Storage) || !m_Data.Tables.CanOperateMask(EOperationMask.Eat))
		{
			if (NpcEatDb.IsNeedEatsth(base.entity) && Enemy.IsNullOrInvalid(base.attackEnemy))
			{
				m_Data.NpcTalkSth(base.entity);
			}
			base.entity.NpcCmpt.ThinkAgent.RemoveThink(mDiningTh);
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init(base.entity);
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Dining))
		{
			m_Data.Table.StopOperate(base.Operator, EOperationMask.Eat);
			return BehaveResult.Failure;
		}
		if (!NpcThinkDb.CanDoing(base.entity, mDiningTh))
		{
			m_Data.Table.StopOperate(base.Operator, EOperationMask.Eat);
			return BehaveResult.Failure;
		}
		if (m_Data.Storage == null || m_Data.Table == null || m_Data.Tables == null || m_Data.Table.Equals(null) || m_Data.Tables.Equals(null))
		{
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.NpcBaseTakeFood);
		if (base.entity.hasView && m_Data.Tables.CanOperateMask(EOperationMask.Eat) && !m_Data.Tables.ContainsOperator(base.Operator))
		{
			if (!m_Data.Tables.CanOperate(base.transform))
			{
				bool flag = PEUtil.IsUnderBlock(base.entity);
				bool flag2 = PEUtil.IsForwardBlock(base.entity, base.entity.peTrans.forward, 2f);
				if (flag)
				{
					SetPosition(m_Data.Table.Trans.position);
				}
				else
				{
					MoveToPosition(m_Data.Table.Trans.position, SpeedState.Run);
				}
				if (flag2)
				{
					SetPosition(m_Data.Table.Trans.position, neeedrepair: false);
				}
				if (Stucking())
				{
					SetPosition(m_Data.Table.Trans.position, neeedrepair: false);
				}
				if (IsReached(base.position, m_Data.Table.Trans.position))
				{
					SetPosition(m_Data.Table.Trans.position, neeedrepair: false);
				}
			}
			else
			{
				MoveToPosition(Vector3.zero);
				m_Data.m_StartEatTime = Time.time;
				m_Data.m_CurEatTime = Random.Range(m_Data.minEatTime, m_Data.maxEatTime);
				m_Data.Tables.StartOperate(base.Operator, EOperationMask.Eat);
			}
		}
		else
		{
			if (!NpcEatDb.CanEatSthFromStorages(base.entity, base.Creater.Assembly.Storages, out var item, out var storage))
			{
				m_Data.Tables.StopOperate(base.Operator, EOperationMask.Eat);
				base.entity.NpcCmpt.ThinkAgent.RemoveThink(mDiningTh);
				return BehaveResult.Success;
			}
			m_Data.Storage = storage;
			if (base.entity.UseItem.GetCdByItemProtoId(item.protoId) < float.Epsilon)
			{
				UseItem(item);
				PEActionParamS param = PEActionParamS.param;
				param.str = m_Data.CurEatAnim;
				DoAction(PEActionType.Eat, param);
				CSUtils.DeleteItemInStorage(m_Data.Storage, item.protoId);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Data != null && m_Data.Table != null && !m_Data.Table.Equals(null) && base.Operator != null && !base.Operator.Equals(null) && m_Data.Tables.ContainsOperator(base.Operator))
		{
			m_Data.Tables.StopOperate(base.Operator, EOperationMask.Eat);
		}
	}
}
