using UnityEngine;

namespace WhiteCat;

public class VCPCarrierLight : VCPart
{
	[SerializeField]
	[Min(0f)]
	private float _energyExpendSpeed;

	[SerializeField]
	private GameObject _effect;

	private CarrierController _controller;

	protected override void Awake()
	{
		base.Awake();
		_effect.SetActive(value: false);
	}

	private void Start()
	{
		_controller = GetComponentInParent<CarrierController>();
	}

	private void Update()
	{
		float expend = _energyExpendSpeed * Time.deltaTime;
		if (_effect.activeSelf)
		{
			if (!_controller.isLightOn || !_controller.isEnergyEnough(expend))
			{
				_effect.SetActive(value: false);
			}
			else if (_controller.isPlayerHost)
			{
				_controller.ExpendEnergy(expend);
			}
		}
		else if (_controller.isLightOn && _controller.isEnergyEnough(expend))
		{
			_effect.SetActive(value: true);
		}
	}
}
