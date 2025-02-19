namespace ItemAsset;

public class EquipInfo
{
	public EequipEditorType _equipType;

	public EeqSelect _selectType;

	public EquipInfo()
	{
	}

	public EquipInfo(EequipEditorType equipType, EeqSelect select)
	{
		_equipType = equipType;
		_selectType = select;
	}

	public void SetEquipInfo(EequipEditorType equipType, EeqSelect select)
	{
		_equipType = equipType;
		_selectType = select;
	}
}
