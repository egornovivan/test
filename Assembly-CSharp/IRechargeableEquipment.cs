public interface IRechargeableEquipment
{
	float enMax { get; }

	float enCurrent { get; set; }

	float rechargeSpeed { get; }

	float lastUsedTime { get; set; }

	float rechargeDelay { get; }
}
