public class CSFarmObject : CSCommonObject
{
	public CSFarm m_Farm => (m_Entity != null) ? (m_Entity as CSFarm) : null;

	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		base.Update();
	}
}
