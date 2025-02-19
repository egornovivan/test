using Pathea;
using ScenarioRTL;
using SkillSystem;

namespace PeCustom;

[Statement("DAMAGE")]
public class DamageListener : EventListener
{
	private OBJECT obj;

	private OBJECT atk;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		atk = Utility.ToObject(base.parameters["attacker"]);
	}

	public override void Listen()
	{
		if (PeGameMgr.IsSingle)
		{
			PESkEntity.entityAttackEvent += OnResponse;
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomDamageEventHandler += OnNetResponse;
		}
	}

	public override void Close()
	{
		if (PeGameMgr.IsSingle)
		{
			PESkEntity.entityAttackEvent -= OnResponse;
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.OnCustomDamageEventHandler -= OnNetResponse;
		}
	}

	private void OnResponse(SkEntity self, SkEntity caster, float value)
	{
		if (!(self == null) && !(caster == null))
		{
			PeEntity component = self.GetComponent<PeEntity>();
			PeEntity component2 = caster.GetComponent<PeEntity>();
			if (!(component == null) && !(component2 == null) && PeScenarioUtility.IsObjectContainEntity(obj, component) && PeScenarioUtility.IsObjectContainEntity(atk, component2))
			{
				Post();
			}
		}
	}

	private void OnNetResponse(int scenarioId, int casterScenarioId, float value)
	{
		if (PeScenarioUtility.IsObjectContainEntity(obj, scenarioId) && PeScenarioUtility.IsObjectContainEntity(atk, casterScenarioId))
		{
			Post();
		}
	}
}
