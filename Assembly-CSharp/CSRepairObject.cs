public class CSRepairObject : CSCommonObject
{
	public CSRepair m_Repair => (m_Entity != null) ? (m_Entity as CSRepair) : null;

	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		base.Update();
	}
}
