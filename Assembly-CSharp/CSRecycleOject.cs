public class CSRecycleOject : CSCommonObject
{
	public CSRecycle m_Recycle => (m_Entity != null) ? (m_Entity as CSRecycle) : null;

	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		base.Update();
	}
}
