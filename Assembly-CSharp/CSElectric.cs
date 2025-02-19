using System.Collections.Generic;

public abstract class CSElectric : CSCommon
{
	public CSPowerPlant m_PowerPlant;

	public List<CSPowerPlant> m_PowerPlants = new List<CSPowerPlant>();

	public override void ChangeState()
	{
		bool isRunning = m_IsRunning;
		if (base.Assembly != null && base.Assembly.IsRunning && m_PowerPlant != null && m_PowerPlant.IsRunning)
		{
			m_IsRunning = true;
		}
		else
		{
			m_IsRunning = false;
		}
		if (isRunning && !m_IsRunning)
		{
			DestroySomeData();
		}
		else if (!isRunning && m_IsRunning)
		{
			UpdateDataToUI();
		}
	}
}
