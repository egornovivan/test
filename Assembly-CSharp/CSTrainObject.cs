public class CSTrainObject : CSCommonObject
{
	public CSTraining m_Train => (m_Entity != null) ? (m_Entity as CSTraining) : null;

	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		base.Update();
	}
}
