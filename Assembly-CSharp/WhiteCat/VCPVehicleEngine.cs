using UnityEngine;

namespace WhiteCat;

public class VCPVehicleEngine : VCPart
{
	[SerializeField]
	private float _motorTorque = 100000f;

	[SerializeField]
	private float _maxRPM = 600f;

	[SerializeField]
	[Min(0f)]
	private float _energyExpendSpeed;

	[SerializeField]
	private float _powerIncreaseSpeed = 0.4f;

	[SerializeField]
	private float _powerDecreaseSpeed = 0.4f;

	private float _sign = 1f;

	private float _power;

	private float _pitchAudioTime;

	private bool _lastDirection;

	private float _runningAudioVolume;

	private float _standbyAudioVolume;

	public float UpdateEngine(VehicleController controller, float rpm)
	{
		float inputY = controller.inputY;
		float num = Mathf.Sign(inputY);
		inputY = Mathf.Abs(inputY);
		if (_sign == num)
		{
			_power = Mathf.MoveTowards(_power, inputY, ((!(inputY > _power)) ? _powerDecreaseSpeed : _powerIncreaseSpeed) * Time.deltaTime);
		}
		else
		{
			_power = Mathf.MoveTowards(_power, 0f, _powerDecreaseSpeed * Time.deltaTime);
			if (_power < 0.01f)
			{
				_sign = num;
			}
		}
		float expend = _power * Time.deltaTime * _energyExpendSpeed;
		float result = 0f;
		if (controller.isEnergyEnough(expend))
		{
			result = _power * _sign * _motorTorque * PEVCConfig.instance.motorForce.Evaluate(Mathf.Clamp01(Mathf.Abs(rpm) / _maxRPM));
		}
		if (controller.isPlayerDriver)
		{
			controller.ExpendEnergy(expend);
		}
		return result;
	}
}
