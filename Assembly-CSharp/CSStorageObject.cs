public class CSStorageObject : CSCommonObject
{
	public CSStorage m_Storage => (m_Entity != null) ? (m_Entity as CSStorage) : null;

	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		base.Update();
	}
}
