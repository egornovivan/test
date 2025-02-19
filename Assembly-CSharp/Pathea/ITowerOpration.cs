namespace Pathea;

public interface ITowerOpration
{
	ECostType ConsumeType { get; }

	int ItemID { get; }

	int ItemCount { get; set; }

	int ItemCountMax { get; }

	int EnergyCount { get; set; }

	int EnergyCountMax { get; }
}
