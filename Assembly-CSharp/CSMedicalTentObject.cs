public class CSMedicalTentObject : CSCommonObject
{
	public CSMedicalTent m_Tent => (m_Entity != null) ? (m_Entity as CSMedicalTent) : null;
}
