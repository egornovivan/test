using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea;

public class AbnormalConditionCmpt : PeCmpt, IPeMsg
{
	private PEAbnormal_N[] m_AbnormalList;

	private EIdentity m_EIdentity;

	private bool m_Inited;

	private NetCmpt m_Net;

	public event Action<PEAbnormalType> evtStart;

	public event Action<PEAbnormalType> evtEnd;

	public event Action<int> evtBuffAdd;

	public event Action<int> evtBuffRemove;

	public event Action<int> evtItemAdd;

	public event Action<float> evtDamage;

	public event Action evtInWater;

	public event Action evtOutWater;

	public event Action<PEAbnormalAttack, Vector3> evtAbnormalAttack;

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.State_Die:
		{
			for (int j = 0; j < m_AbnormalList.Length; j++)
			{
				if (m_AbnormalList[j] != null)
				{
					m_AbnormalList[j].OnDie();
				}
			}
			break;
		}
		case EMsg.State_Revive:
		{
			for (int i = 0; i < m_AbnormalList.Length; i++)
			{
				if (m_AbnormalList[i] != null)
				{
					m_AbnormalList[i].OnRevive();
				}
			}
			break;
		}
		case EMsg.state_Water:
			if ((bool)args[0])
			{
				if (this.evtInWater != null)
				{
					this.evtInWater();
				}
			}
			else if (this.evtOutWater != null)
			{
				this.evtOutWater();
			}
			break;
		case EMsg.Battle_BeAttacked:
			if (this.evtDamage != null)
			{
				this.evtDamage((float)args[0]);
			}
			break;
		case EMsg.Battle_EquipAttack:
		case EMsg.Battle_AttackHit:
		case EMsg.Battle_TargetSkill:
		case EMsg.Camera_ChangeMode:
			break;
		}
	}

	public override void Start()
	{
		base.Start();
		if (!m_Inited)
		{
			m_Net = base.Entity.GetCmpt<NetCmpt>();
			if (null != base.Entity.commonCmpt)
			{
				m_EIdentity = base.Entity.commonCmpt.Identity;
			}
			InitData(m_EIdentity);
		}
	}

	public override void OnUpdate()
	{
		if (m_AbnormalList != null)
		{
			for (int i = 0; i < m_AbnormalList.Length; i++)
			{
				m_AbnormalList[i]?.Update();
			}
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		m_Inited = true;
		m_EIdentity = (EIdentity)r.ReadInt32();
		InitData(m_EIdentity);
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = r.ReadInt32();
			int num3 = r.ReadInt32();
			if (num3 > 0)
			{
				if (m_AbnormalList[num2] != null)
				{
					m_AbnormalList[num2].Deserialize(r.ReadBytes(num3));
				}
				else
				{
					r.ReadBytes(num3);
				}
			}
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		if (!m_Inited)
		{
			m_Net = base.Entity.GetCmpt<NetCmpt>();
			if (null != base.Entity.commonCmpt)
			{
				m_EIdentity = base.Entity.commonCmpt.Identity;
			}
			InitData(m_EIdentity);
		}
		w.Write((int)m_EIdentity);
		List<PEAbnormalType> activeAbnormalList = GetActiveAbnormalList();
		w.Write(activeAbnormalList.Count);
		for (int i = 0; i < activeAbnormalList.Count; i++)
		{
			w.Write((int)m_AbnormalList[(int)activeAbnormalList[i]].type);
			byte[] array = m_AbnormalList[(int)activeAbnormalList[i]].Serialize();
			if (array == null)
			{
				w.Write(0);
				continue;
			}
			w.Write(array.Length);
			w.Write(array);
		}
	}

	private void AddAbnormal(PEAbnormalType type, AbnormalData data)
	{
		PEAbnormal_N pEAbnormal_N = new PEAbnormal_N();
		pEAbnormal_N.Init(type, this, base.Entity, OnStartAbnormal, OnEndAbnormal);
		m_AbnormalList[(int)type] = pEAbnormal_N;
	}

	private void InitData(EIdentity eIdentity)
	{
		m_AbnormalList = new PEAbnormal_N[51];
		if (PeGameMgr.IsBuild || PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
		{
			return;
		}
		int num = 0;
		switch (eIdentity)
		{
		case EIdentity.Player:
			num = 1;
			break;
		case EIdentity.Npc:
			num = 2;
			break;
		case EIdentity.Neutral:
			num = 4;
			break;
		}
		for (int i = 1; i < 51; i++)
		{
			AbnormalData data = AbnormalData.GetData((PEAbnormalType)i);
			if (data != null && (data.target & num) == num)
			{
				AddAbnormal((PEAbnormalType)i, data);
			}
		}
		if (null != base.Entity.aliveEntity)
		{
			base.Entity.aliveEntity.evtOnBuffAdd += OnBuffAdd;
			base.Entity.aliveEntity.evtOnBuffRemove += OnBuffRemove;
		}
		PlayerPackageCmpt playerPackageCmpt = base.Entity.packageCmpt as PlayerPackageCmpt;
		if (null != playerPackageCmpt)
		{
			playerPackageCmpt.getItemEventor.Subscribe(OnItemAdd);
		}
	}

	private void OnBuffAdd(int buffID)
	{
		if (this.evtBuffAdd != null)
		{
			this.evtBuffAdd(buffID);
		}
	}

	private void OnBuffRemove(int buffID)
	{
		if (this.evtBuffRemove != null)
		{
			this.evtBuffRemove(buffID);
		}
	}

	private void OnStartAbnormal(PEAbnormalType type)
	{
		if (this.evtStart != null)
		{
			this.evtStart(type);
		}
		NetSendStartMsg(type);
	}

	private void OnEndAbnormal(PEAbnormalType type)
	{
		if (this.evtEnd != null)
		{
			this.evtEnd(type);
		}
		NetSendEndMsg(type);
	}

	private void NetSendStartMsg(PEAbnormalType type)
	{
		if (PeGameMgr.IsMulti && m_Net.IsController && m_AbnormalList[(int)type] != null)
		{
			PlayerNetwork.SyncAbnormalConditionStart(base.Entity.Id, (int)type, m_AbnormalList[(int)type].Serialize());
		}
	}

	private void NetSendEndMsg(PEAbnormalType type)
	{
		if (PeGameMgr.IsMulti && m_Net.IsController)
		{
			PlayerNetwork.SyncAbnormalConditionEnd(base.Entity.Id, (int)type);
		}
	}

	public void NetApplyState(PEAbnormalType type, byte[] data)
	{
		if (m_AbnormalList == null)
		{
			Debug.LogError("AbnormalConditionCmpt has not been inited.");
		}
		else if (m_AbnormalList[(int)type] != null)
		{
			m_AbnormalList[(int)type].Deserialize(data);
		}
	}

	public void NetEndState(PEAbnormalType type)
	{
		if (m_AbnormalList == null)
		{
			Debug.LogError("AbnormalConditionCmpt has not been inited.");
		}
		else if (m_AbnormalList[(int)type] != null)
		{
			m_AbnormalList[(int)type].EndCondition();
		}
	}

	public void ApplyAbnormalAttack(PEAbnormalAttack attack, Vector3 effectPos)
	{
		if (this.evtAbnormalAttack != null)
		{
			this.evtAbnormalAttack(attack, effectPos);
		}
		switch (attack.type)
		{
		case PEAbnormalAttackType.Dazzling:
			StartAbnormalCondition(PEAbnormalType.Dazzling);
			break;
		case PEAbnormalAttackType.Flashlight:
			StartAbnormalCondition(PEAbnormalType.Flashlight);
			break;
		case PEAbnormalAttackType.Tinnitus:
			StartAbnormalCondition(PEAbnormalType.Tinnitus);
			break;
		case PEAbnormalAttackType.Deafness:
			StartAbnormalCondition(PEAbnormalType.Deafness);
			break;
		case PEAbnormalAttackType.BlurredVision:
			StartAbnormalCondition(PEAbnormalType.BlurredVision);
			break;
		}
	}

	public bool CheckAbnormalCondition(PEAbnormalType type)
	{
		return m_AbnormalList[(int)type] != null && m_AbnormalList[(int)type].hasEffect;
	}

	public List<PEAbnormalType> GetActiveAbnormalList()
	{
		List<PEAbnormalType> list = new List<PEAbnormalType>();
		if (m_AbnormalList != null)
		{
			for (int i = 0; i < m_AbnormalList.Length; i++)
			{
				if (m_AbnormalList[i] != null && m_AbnormalList[i].hasEffect)
				{
					list.Add(m_AbnormalList[i].type);
				}
			}
		}
		return list;
	}

	public void StartAbnormalCondition(PEAbnormalType type)
	{
		if (m_AbnormalList[(int)type] != null)
		{
			m_AbnormalList[(int)type].StartCondition();
		}
	}

	public void EndAbnormalCondition(PEAbnormalType type)
	{
		if (m_AbnormalList[(int)type] != null)
		{
			m_AbnormalList[(int)type].EndCondition();
		}
	}

	private void OnItemAdd(object sender, PlayerPackageCmpt.GetItemEventArg evt)
	{
		if (this.evtItemAdd != null)
		{
			this.evtItemAdd(evt.protoId);
		}
	}
}
