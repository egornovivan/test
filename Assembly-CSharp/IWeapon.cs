using ItemAsset;
using SkillSystem;

public interface IWeapon
{
	string[] leisures { get; }

	ItemObject ItemObj { get; }

	bool HoldReady { get; }

	bool UnHoldReady { get; }

	void HoldWeapon(bool hold);

	AttackMode[] GetAttackMode();

	bool CanAttack(int index = 0);

	void Attack(int index = 0, SkEntity targetEntity = null);

	bool AttackEnd(int index = 0);

	bool IsInCD(int index = 0);
}
