using UnityEngine;

namespace WhiteCat;

public class VCPFuelCell : VCPart
{
	[SerializeField]
	private float _energyCapacity = 20000f;

	public float energyCapacity => _energyCapacity;
}
