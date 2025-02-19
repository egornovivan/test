public class CSMedicalCheckObject : CSCommonObject
{
	public CSMedicalCheck m_Check => (m_Entity != null) ? (m_Entity as CSMedicalCheck) : null;
}
