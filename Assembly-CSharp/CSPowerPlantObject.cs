using UnityEngine;

public class CSPowerPlantObject : CSEntityObject
{
	[SerializeField]
	private EnergyArea m_EnergyArea;

	public CSPowerPlantInfo m_Info;

	public CSPowerPlant m_PowerPlant => (m_Entity != null) ? (m_Entity as CSPowerPlant) : null;

	public override int Init(CSBuildingLogic csbl, CSCreator creator, bool bFight)
	{
		int num = 0;
		num++;
		return base.Init(csbl, creator, bFight);
	}

	public override int Init(int id, CSCreator creator, bool bFight)
	{
		int num = 0;
		num++;
		return base.Init(id, creator, bFight);
	}

	private new void Start()
	{
		base.Start();
		if (m_ItemID == 1558)
		{
			m_Info = CSInfoMgr.m_ppFusion;
		}
		else
		{
			m_Info = CSInfoMgr.m_ppCoal;
		}
	}

	private new void Update()
	{
		base.Update();
		if (m_EnergyArea == null)
		{
			return;
		}
		if (m_PowerPlant == null)
		{
			m_EnergyArea.radius = m_Info.m_Radius;
			return;
		}
		if (!m_PowerPlant.bShowElectric)
		{
			m_EnergyArea.gameObject.SetActive(value: false);
		}
		else
		{
			m_EnergyArea.gameObject.SetActive(value: true);
		}
		m_EnergyArea.radius = m_PowerPlant.Radius;
		if (m_Type == CSConst.ObjectType.PowerPlant_Coal)
		{
			CSPPCoal cSPPCoal = m_PowerPlant as CSPPCoal;
			if (cSPPCoal.isWorking() && cSPPCoal.IsRunning)
			{
				m_EnergyArea.energyScale = (1f - cSPPCoal.Data.m_CurWorkedTime / cSPPCoal.Data.m_WorkedTime) * 0.5f;
			}
			else
			{
				m_EnergyArea.energyScale = 0f;
			}
		}
		else if (m_Type == CSConst.ObjectType.PowerPlant_Fusion)
		{
			CSPPFusion cSPPFusion = m_PowerPlant as CSPPFusion;
			if (cSPPFusion.isWorking() && cSPPFusion.IsRunning)
			{
				m_EnergyArea.energyScale = (1f - cSPPFusion.Data.m_CurWorkedTime / cSPPFusion.Data.m_WorkedTime) * 0.5f;
			}
			else
			{
				m_EnergyArea.energyScale = 0f;
			}
		}
		m_Power = m_PowerPlant.m_RestPower;
	}
}
