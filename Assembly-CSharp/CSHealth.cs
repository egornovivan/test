using System.Collections.Generic;
using Pathea;

public abstract class CSHealth : CSElectric
{
	public List<PeEntity> allPatients = new List<PeEntity>();

	public virtual void RemoveDeadPatient(int npcId)
	{
	}

	public virtual bool IsDoingYou(PeEntity npc)
	{
		return false;
	}
}
