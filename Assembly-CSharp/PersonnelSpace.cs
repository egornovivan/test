using Pathea.Operate;

public class PersonnelSpace
{
	public PersonnelBase m_Person;

	public object m_Room;

	private PEMachine m_workmachine;

	private PEDoctor m_hospital;

	private PETrainner m_trainer;

	public PEMachine WorkMachine
	{
		get
		{
			return m_workmachine;
		}
		set
		{
			m_workmachine = value;
			if (m_Person != null)
			{
				m_Person.UpdateWorkMachine(m_workmachine);
			}
		}
	}

	public PEDoctor HospitalMachine
	{
		get
		{
			return m_hospital;
		}
		set
		{
			m_hospital = value;
			if (m_Person != null)
			{
				m_Person.UpdateHospitalMachine(m_hospital);
			}
		}
	}

	public PETrainner TrainerMachine
	{
		get
		{
			return m_trainer;
		}
		set
		{
			m_trainer = value;
			if (m_Person != null)
			{
				m_Person.UpdateTrainerMachine(m_trainer);
			}
		}
	}

	public PersonnelSpace(object room)
	{
		m_Room = room;
	}
}
