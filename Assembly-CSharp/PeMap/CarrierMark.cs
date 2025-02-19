using UnityEngine;
using WhiteCat;

namespace PeMap;

public class CarrierMark : ILabel
{
	private int icon = 6;

	public CarrierController carrierController;

	ELabelType ILabel.GetType()
	{
		return ELabelType.Vehicle;
	}

	public int GetIcon()
	{
		return icon;
	}

	public Vector3 GetPos()
	{
		if (null != carrierController)
		{
			return carrierController.creationController.boundsCenterInWorld;
		}
		return Vector3.zero;
	}

	public string GetText()
	{
		if (null != carrierController)
		{
			return carrierController.creationController.creationData.m_IsoData.m_HeadInfo.Name;
		}
		return string.Empty;
	}

	public bool FastTravel()
	{
		return false;
	}

	public bool NeedArrow()
	{
		return false;
	}

	public float GetRadius()
	{
		return -1f;
	}

	public EShow GetShow()
	{
		return EShow.All;
	}

	public bool CompareTo(ILabel label)
	{
		return label is CarrierMark carrierMark && carrierMark.carrierController == carrierController;
	}
}
