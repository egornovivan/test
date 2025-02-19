using Pathea;
using ScenarioRTL;
using SkillSystem;

namespace PeCustom;

public class MissionGoal_Kill : MissionGoal
{
	public OBJECT monster;

	public ECompare compare;

	public int target;

	private int _current;

	public int current
	{
		get
		{
			return _current;
		}
		set
		{
			_current = value;
		}
	}

	public override bool achieved
	{
		get
		{
			return Utility.Compare(current, target, compare);
		}
		set
		{
		}
	}

	public override void Init()
	{
		PESkEntity.entityAttackEvent += OnDamage;
	}

	public override void Free()
	{
		PESkEntity.entityAttackEvent -= OnDamage;
	}

	private void OnDamage(SkEntity self, SkEntity caster, float value)
	{
		if (PeSingleton<PeCreature>.Instance == null || !(PeSingleton<PeCreature>.Instance.mainPlayer != null))
		{
			return;
		}
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		PeEntity component = self.GetComponent<PeEntity>();
		PeEntity component2 = caster.GetComponent<PeEntity>();
		if (!(component == null) && !(component2 != mainPlayer))
		{
			float attribute = component.GetAttribute(AttribType.Hp);
			if (attribute <= 0f && attribute + value > 0f && PeScenarioUtility.IsObjectContainEntity(monster, component))
			{
				_current++;
			}
		}
	}
}
