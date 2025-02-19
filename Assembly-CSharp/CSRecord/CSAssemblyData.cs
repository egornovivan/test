namespace CSRecord;

public class CSAssemblyData : CSObjectData
{
	public bool m_ShowShield;

	public int m_Level;

	public float m_CurUpgradeTime;

	public float m_UpgradeTime;

	public long m_TimeTicks;

	public int m_MedicineResearchTimes;

	public double m_MedicineResearchState;

	public CSAssemblyData()
	{
		m_ShowShield = true;
		dType = 1;
	}
}
