using Pathea;
using UnityEngine;

public class CSAssemblyObject : CSEntityObject
{
	public PolarShield m_EnergySheildPrfab;

	private PolarShield m_CurEnergySheild;

	public CSAssemblyInfo m_Info;

	private UTimer m_triggerTimer;

	public PolarShield CurEnergySheild => m_CurEnergySheild;

	public CSAssembly m_Assembly => (m_Entity != null) ? (m_Entity as CSAssembly) : null;

	public override int Init(CSBuildingLogic csbul, CSCreator creator, bool bFight = true)
	{
		int num = base.Init(csbul, creator, bFight);
		if (num == 4)
		{
			CreateEnergySheild();
		}
		return num;
	}

	public override int Init(int id, CSCreator creator, bool bFight = true)
	{
		int num = base.Init(id, creator, bFight);
		if (num == 4)
		{
			CreateEnergySheild();
		}
		return num;
	}

	public void CreateEnergySheild()
	{
		if (!(m_CurEnergySheild == null))
		{
			return;
		}
		m_CurEnergySheild = Object.Instantiate(m_EnergySheildPrfab);
		m_CurEnergySheild.transform.parent = base.transform.parent;
		m_CurEnergySheild.transform.localPosition = Vector3.zero;
		if (m_Assembly == null)
		{
			m_CurEnergySheild.SetRadius(m_CurEnergySheild.min_Radius);
			m_CurEnergySheild.SetLevel(0);
		}
		else if (m_Assembly.gameLogic != null)
		{
			if (m_Assembly.gameLogic.GetComponent<CSBuildingLogic>().IsFirstConstruct)
			{
				m_CurEnergySheild.SetLerpRadius(m_Assembly.Radius);
				m_CurEnergySheild.SetLevel(m_Assembly.Level);
			}
			else
			{
				m_CurEnergySheild.SetRadius(m_Assembly.Radius);
				m_CurEnergySheild.SetLevel(m_Assembly.Level);
			}
		}
		else
		{
			m_CurEnergySheild.SetRadius(m_Assembly.Radius);
			m_CurEnergySheild.SetLevel(m_Assembly.Level);
		}
		m_CurEnergySheild.m_Model.SetActive(value: false);
	}

	public void RefreshObject()
	{
		if (m_Assembly == null)
		{
			m_CurEnergySheild.SetLerpRadius(m_CurEnergySheild.min_Radius);
			m_CurEnergySheild.SetLevel(0);
		}
		else
		{
			m_CurEnergySheild.SetLerpRadius(m_Assembly.Radius);
			m_CurEnergySheild.SetLevel(m_Assembly.Level);
			m_CurEnergySheild.AfterUpdate();
		}
	}

	private void OnEnterTrigger(PeEntity monster, int skillId)
	{
		if (m_Assembly != null && m_Assembly.gameLogic != null)
		{
			CSBuildingLogic component = m_Assembly.gameLogic.GetComponent<CSBuildingLogic>();
			component.ShieldOn(monster, skillId);
			PeEntity component2 = m_Assembly.gameLogic.GetComponent<PeEntity>();
			if (component2 != null && PeNpcGroup.Instance != null)
			{
				PeNpcGroup.Instance.OnCSAttackEnmey(component2, monster);
			}
		}
	}

	private void OnExitTrigger(PeEntity monster)
	{
		if (m_Assembly != null && m_Assembly.gameLogic != null)
		{
			CSBuildingLogic component = m_Assembly.gameLogic.GetComponent<CSBuildingLogic>();
			component.ShieldOff(monster);
		}
	}

	private new void Start()
	{
		base.Start();
		m_Info = CSInfoMgr.m_AssemblyInfo;
		CreateEnergySheild();
		m_CurEnergySheild.onEnterTrigger += OnEnterTrigger;
		m_CurEnergySheild.onExitTrigger += OnExitTrigger;
		m_triggerTimer = new UTimer();
		m_triggerTimer.ElapseSpeed = 1f;
	}

	private new void Update()
	{
		base.Update();
		if (m_CurEnergySheild != null)
		{
			if (m_Assembly != null)
			{
				if (m_Assembly.bShowShield)
				{
					m_CurEnergySheild.m_Model.SetActive(value: true);
				}
				else
				{
					m_CurEnergySheild.m_Model.SetActive(value: false);
				}
			}
			else
			{
				m_CurEnergySheild.m_Model.SetActive(value: true);
			}
		}
		if (m_Assembly != null)
		{
			m_Assembly.Position = base.transform.position;
		}
	}

	private void FixedUpdate()
	{
		if (m_Assembly != null)
		{
			if (m_triggerTimer.Second >= (double)m_Assembly.damageCD)
			{
				m_triggerTimer.Second = 0.0;
			}
			m_triggerTimer.Update(Time.deltaTime);
		}
	}
}
