using Pathea;

public interface IHeavyEquipment
{
	MoveStyle baseMoveStyle { get; }

	void HidEquipmentByUnderWater(bool hide);
}
