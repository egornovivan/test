namespace ItemAsset;

public class EvaluateInfo
{
	private float _DPS;

	private float _SurplusCnt = 1f;

	private float _IsEquipment;

	private float _InInRange;

	private float _LongRange = 1f;

	private float _rangeFir = 1f;

	private ItemObject _ItemObj;

	private float _EvaluateValue;

	public ItemObject ItemObj => _ItemObj;

	public float EvaluateValue
	{
		get
		{
			_EvaluateValue = _DPS * _IsEquipment * _InInRange * _SurplusCnt * _LongRange * _rangeFir;
			return _EvaluateValue;
		}
	}

	public void SetRangeFir(float value)
	{
		_rangeFir = value;
	}

	public void SetLongRange(float value)
	{
		_LongRange = value;
	}

	public void setEquipment(float value)
	{
		_IsEquipment = value;
	}

	public void SetRangeValue(float value)
	{
		_InInRange = value;
	}

	public void SetDPS(float dps)
	{
		_DPS = dps;
	}

	public void SetSurplusCnt(float value)
	{
		_SurplusCnt = value;
	}

	public void SetObj(ItemObject Obj)
	{
		_ItemObj = Obj;
	}
}
