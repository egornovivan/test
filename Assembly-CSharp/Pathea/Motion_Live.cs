using SkillSystem;
using UnityEngine;

namespace Pathea;

public class Motion_Live : PeCmpt, IPeMsg
{
	private Action_Sleep m_Sleep;

	private Action_Gather m_Gather;

	private MotionMgrCmpt m_MotionMgr;

	[SerializeField]
	private Action_Hand m_Hand;

	[SerializeField]
	private Action_Handed m_Handed;

	public Action_Gather gather => m_Gather;

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		if (msg == EMsg.View_Prefab_Build)
		{
			BiologyViewCmpt biologyViewCmpt = args[0] as BiologyViewCmpt;
			m_Sleep.m_PhyCtrl = biologyViewCmpt.monoPhyCtrl;
		}
	}

	public override void Start()
	{
		base.Start();
		if (null != base.Entity.aliveEntity)
		{
			base.Entity.aliveEntity.deathEvent += OnDeath;
		}
		m_MotionMgr = base.Entity.motionMgr;
		if (null != m_MotionMgr)
		{
			m_Sleep = new Action_Sleep();
			m_Gather = new Action_Gather();
			m_MotionMgr.AddAction(m_Sleep);
			m_MotionMgr.AddAction(new Action_Eat());
			m_MotionMgr.AddAction(m_Gather);
			m_MotionMgr.AddAction(new Action_PickUpItem());
			m_MotionMgr.AddAction(new Action_Sit());
			m_MotionMgr.AddAction(new Action_Stuned());
			m_MotionMgr.AddAction(new Action_Build());
			m_MotionMgr.AddAction(new Action_Operation());
			m_MotionMgr.AddAction(new Action_Lie());
			m_MotionMgr.AddAction(new Action_Cutscene());
			m_MotionMgr.AddAction(new Action_Cure());
			m_MotionMgr.AddAction(new Action_Leisure());
			m_MotionMgr.AddAction(new Action_Abnormal());
			m_MotionMgr.AddAction(m_Hand);
			m_MotionMgr.AddAction(m_Handed);
		}
	}

	private void OnDeath(SkEntity self, SkEntity caster)
	{
		if (base.Entity.Race == ERace.Paja || base.Entity.Race == ERace.Puja)
		{
			m_MotionMgr.DoAction(PEActionType.AlienDeath);
		}
		else
		{
			m_MotionMgr.DoAction(PEActionType.Death);
		}
	}
}
