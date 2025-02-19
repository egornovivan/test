public class CSMedicalTreatObject : CSCommonObject
{
	public CSMedicalTreat m_Treat => (m_Entity != null) ? (m_Entity as CSMedicalTreat) : null;
}
