using UnityEngine;

namespace WhiteCat;

public class VCPSubmarineBallastTank : VCPart
{
	[SerializeField]
	private float _weightBase = 10000000f;

	[SerializeField]
	private float _waterSpeed = 0.25f;

	[SerializeField]
	private float _energyExpendSpeed = 10f;

	private BoatController _controller;

	private float _maxWeight;

	private float _fillAmount;

	public void Init(BoatController controller)
	{
		_controller = controller;
		Vector3 localScale = base.transform.localScale;
		_maxWeight = (0f - localScale.x) * localScale.y * localScale.z * _weightBase;
		base.enabled = true;
	}

	private void FixedUpdate()
	{
		int num = 0;
		if (_controller.hasDriver && _controller.isEnergyEnough(0.01f))
		{
			float num2 = 0f;
			if (_controller.inputVertical > 0.01f)
			{
				num2 = _controller.inputVertical * PEVCConfig.instance.submarineMaxUpSpeed;
			}
			else if (_controller.inputVertical < -0.01f)
			{
				num2 = _controller.inputVertical * PEVCConfig.instance.submarineMaxDownSpeed;
			}
			float y = _controller.rigidbody.velocity.y;
			if (y < num2 - 0.3f)
			{
				num = 1;
			}
			else if (y > num2 + 0.3f)
			{
				num = -1;
			}
		}
		if (num > 0)
		{
			_fillAmount = Mathf.Clamp01(_fillAmount - _waterSpeed * Time.deltaTime);
		}
		else if (num < 0)
		{
			_fillAmount = Mathf.Clamp01(_fillAmount + _waterSpeed * Time.deltaTime);
		}
		if (_fillAmount > 0f)
		{
			_controller.rigidbody.AddForce(0f, _fillAmount * _maxWeight, 0f);
		}
		if (_controller.isPlayerHost && Mathf.Abs(_controller.inputVertical) > 0.01f)
		{
			_controller.ExpendEnergy(Time.deltaTime * _energyExpendSpeed);
		}
	}
}
