public abstract class CSWorkerMachine : CSElectric
{
	public virtual void RecountCounter()
	{
	}

	public override bool AddWorker(CSPersonnel npc)
	{
		bool result = base.AddWorker(npc);
		RecountCounter();
		return result;
	}

	public override void RemoveWorker(CSPersonnel npc)
	{
		base.RemoveWorker(npc);
		RecountCounter();
	}
}
