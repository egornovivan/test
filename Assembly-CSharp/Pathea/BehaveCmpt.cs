using System;
using System.IO;
using Behave.Runtime;
using SkillSystem;

namespace Pathea;

public class BehaveCmpt : PeCmpt, IAgent, IBehave, IPeMsg
{
	private string assetPath = string.Empty;

	private int m_BehaveID;

	private SkAliveEntity m_SkEntity;

	private RequestCmpt m_Request;

	private NpcCmpt m_Npc;

	private bool m_Start;

	private float m_minPatrolRadius;

	private float m_maxPatrolRadius;

	private BHPatrolMode m_PatrolMode = BHPatrolMode.CurrentCenter;

	public float MinPatrolRadius
	{
		get
		{
			return m_minPatrolRadius;
		}
		set
		{
			m_minPatrolRadius = value;
		}
	}

	public float MaxPatrolRadius
	{
		get
		{
			return m_maxPatrolRadius;
		}
		set
		{
			m_maxPatrolRadius = value;
		}
	}

	public BHPatrolMode PatrolMode
	{
		get
		{
			return m_PatrolMode;
		}
		set
		{
			m_PatrolMode = value;
		}
	}

	public bool BehaveActive => true;

	public event Action<int> OnBehaveStop;

	public void Excute()
	{
		m_Start = true;
		if (Behave.Runtime.Singleton<BTLauncher>.Instance != null)
		{
			Behave.Runtime.Singleton<BTLauncher>.Instance.Excute(m_BehaveID);
		}
	}

	public void Stop()
	{
		m_Start = false;
		if (Behave.Runtime.Singleton<BTLauncher>.Instance != null)
		{
			Behave.Runtime.Singleton<BTLauncher>.Instance.Stop(m_BehaveID);
		}
	}

	public void Reset()
	{
		if (Behave.Runtime.Singleton<BTLauncher>.Instance != null)
		{
			Behave.Runtime.Singleton<BTLauncher>.Instance.Reset(m_BehaveID);
		}
	}

	public void Pause(bool value)
	{
		if (Behave.Runtime.Singleton<BTLauncher>.Instance != null)
		{
			Behave.Runtime.Singleton<BTLauncher>.Instance.Pause(m_BehaveID, value);
		}
	}

	public void SetAssetPath(string path)
	{
		assetPath = path;
	}

	public void Stopbehave()
	{
		if ((!(m_Request != null) || !m_Request.HasAnyRequest()) && (!(m_Npc != null) || (m_Npc.Type != ENpcType.Follower && m_Npc.Type != ENpcType.Base)))
		{
			DispatchBehaveStopEvent();
			Stop();
		}
	}

	private void DispatchBehaveStopEvent()
	{
		if (this.OnBehaveStop != null)
		{
			this.OnBehaveStop(m_BehaveID);
		}
	}

	private void OnDeath(SkEntity self, SkEntity injurer)
	{
		Behave.Runtime.Singleton<BTLauncher>.Instance.Reset(m_BehaveID);
		Behave.Runtime.Singleton<BTLauncher>.Instance.Pause(m_BehaveID, value: true);
	}

	private void OnRevive(SkEntity entity)
	{
		Behave.Runtime.Singleton<BTLauncher>.Instance.Pause(m_BehaveID, value: false);
	}

	private void InitAttacks()
	{
		if (m_BehaveID > 0)
		{
			TargetCmpt component = GetComponent<TargetCmpt>();
			if (component != null)
			{
				component.SetActions(Behave.Runtime.Singleton<BTLauncher>.Instance.GetAgent(m_BehaveID).GetActions());
			}
		}
	}

	public override void Start()
	{
		base.Start();
		m_SkEntity = GetComponent<SkAliveEntity>();
		m_Request = GetComponent<RequestCmpt>();
		m_Npc = GetComponent<NpcCmpt>();
		if (m_SkEntity != null)
		{
			m_SkEntity.deathEvent += OnDeath;
			m_SkEntity.reviveEvent += OnRevive;
		}
		m_BehaveID = Behave.Runtime.Singleton<BTLauncher>.Instance.Instantiate(assetPath, this, isLaunch: false);
		InitAttacks();
		if (PeGameMgr.IsSingle && m_Npc != null && (m_Npc.Type == ENpcType.Follower || m_Npc.Type == ENpcType.Base || m_Npc.hasAnyRequest))
		{
			Excute();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!(Behave.Runtime.Singleton<BTLauncher>.Instance == null))
		{
			if (m_BehaveID > 0 && m_Start && !Behave.Runtime.Singleton<BTLauncher>.Instance.IsStart(m_BehaveID))
			{
				Behave.Runtime.Singleton<BTLauncher>.Instance.Excute(m_BehaveID);
			}
			if (m_BehaveID > 0 && !m_Start && Behave.Runtime.Singleton<BTLauncher>.Instance.IsStart(m_BehaveID))
			{
				Behave.Runtime.Singleton<BTLauncher>.Instance.Stop(m_BehaveID);
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (Behave.Runtime.Singleton<BTLauncher>.Instance != null)
		{
			Behave.Runtime.Singleton<BTLauncher>.Instance.Remove(m_BehaveID);
		}
		if (m_SkEntity != null)
		{
			m_SkEntity.deathEvent -= OnDeath;
			m_SkEntity.reviveEvent -= OnRevive;
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(assetPath);
	}

	public override void Deserialize(BinaryReader r)
	{
		assetPath = r.ReadString();
	}

	public void Reset(Tree sender)
	{
	}

	public int SelectTopPriority(Tree sender, params int[] IDs)
	{
		return IDs[0];
	}

	public BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Success;
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Model_Build:
			if (!PeGameMgr.IsMulti)
			{
				Excute();
			}
			break;
		case EMsg.View_Model_Destroy:
			if (!PeGameMgr.IsMulti)
			{
				Pause(value: false);
				Stopbehave();
			}
			break;
		case EMsg.State_Die:
			Reset();
			break;
		case EMsg.Net_Controller:
			if (!(base.Entity.monstermountCtrl != null) || base.Entity.monstermountCtrl.ctrlType != ECtrlType.Mount)
			{
				Excute();
			}
			break;
		case EMsg.Net_Proxy:
			Stop();
			break;
		case EMsg.View_Ragdoll_Fall_Begin:
			Pause(value: true);
			break;
		case EMsg.View_Ragdoll_Getup_Finished:
			Pause(value: false);
			break;
		}
	}
}
