public class CSEnhanceObject : CSCommonObject
{
	public CSEnhance m_Enhance => (m_Entity != null) ? (m_Entity as CSEnhance) : null;

	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		base.Update();
	}
}
