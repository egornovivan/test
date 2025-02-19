using UnityEngine;

namespace EpsilonIndi;

public class PlanetAppearance : MonoBehaviour
{
	public float diameter;

	private float c_diameter = 1f;

	private void Start()
	{
		base.transform.localScale = Vector3.one * c_diameter;
	}

	private void Update()
	{
		if (diameter != c_diameter)
		{
			c_diameter = diameter;
			base.transform.localScale = Vector3.one * c_diameter;
		}
	}
}
