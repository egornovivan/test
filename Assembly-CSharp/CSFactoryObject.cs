public class CSFactoryObject : CSCommonObject
{
	public CSFactory m_Factory => (m_Entity != null) ? (m_Entity as CSFactory) : null;

	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		base.Update();
	}
}
