using Pathea;
using ScenarioRTL;
using SkillSystem;

namespace PeCustom;

[Statement("DEATH")]
public class DeathListener : EventListener
{
	private OBJECT obj;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
	}

	public override void Listen()
	{
		if (PeGameMgr.IsSingle)
		{
			PESkEntity.entityDeadEvent += OnResponse;
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomDeathEventHandler += OnNetResponse;
		}
	}

	public override void Close()
	{
		if (PeGameMgr.IsSingle)
		{
			PESkEntity.entityDeadEvent -= OnResponse;
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomDeathEventHandler -= OnNetResponse;
		}
	}

	private void OnResponse(SkEntity sk)
	{
		PeEntity component = sk.GetComponent<PeEntity>();
		if (!(component == null) && PeScenarioUtility.IsObjectContainEntity(obj, component))
		{
			Post();
		}
	}

	private void OnNetResponse(int scenarioId)
	{
		if (PeScenarioUtility.IsObjectContainEntity(obj, scenarioId))
		{
			Post();
		}
	}
}
