using System.Collections.Generic;

public class AiCommonTD : GroupNetInterface
{
	protected double startTime;

	protected float preTime;

	protected float coolTime;

	protected int waveIndex;

	protected int totalWave;

	protected int totalCount;

	protected int deathCount;

	protected List<AiObject> aiList = new List<AiObject>();

	public int authId { get; protected set; }

	protected override void OnPEStart()
	{
		Add(this);
	}

	protected override void OnPEDestroy()
	{
		foreach (AiObject ai in aiList)
		{
			ai.PEInstantiateEvent -= OnMonsterInstantiate;
			ai.DeathEventHandler -= OnMonsterDeath;
			NetInterface.NetDestroy(ai);
		}
	}

	public virtual void OnMonsterDeath(SkNetworkInterface net)
	{
		AiObject aiObject = net as AiObject;
		if (null != aiObject)
		{
			deathCount++;
			aiObject.DeathEventHandler -= OnMonsterDeath;
			aiObject.PEInstantiateEvent -= OnMonsterInstantiate;
			aiList.Remove(aiObject);
			RPCOthers(EPacketType.PT_InGame_TDMonsterDeath, deathCount);
		}
	}

	public void OnMonsterInstantiate(NetInterface sender)
	{
		AiObject aiObject = sender as AiObject;
		if (null != aiObject && !aiList.Contains(aiObject))
		{
			aiList.Add(aiObject);
		}
	}
}
