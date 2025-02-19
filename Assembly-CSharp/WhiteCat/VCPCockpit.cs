using UnityEngine;

namespace WhiteCat;

public class VCPCockpit : VCPBaseSeat
{
	[SerializeField]
	private float _maxSteerAngle = 20f;

	[SerializeField]
	private Transform _steerRoot;

	[SerializeField]
	private Transform _leftHandPoint;

	[SerializeField]
	private Transform _rightHandPoint;

	public bool isMotorcycle;

	[SerializeField]
	private MultiSoundController _sound;

	public override void GetOn(IVCPassenger passenger)
	{
		base.GetOn(passenger);
		passenger.SetHands(_leftHandPoint, _rightHandPoint);
	}

	public void UpdateCockpit(float inputX, float inputY, bool hasDriverAndEnergy)
	{
		if ((bool)_steerRoot)
		{
			Vector3 localEulerAngles = _steerRoot.localEulerAngles;
			localEulerAngles.y = inputX * _maxSteerAngle;
			_steerRoot.localEulerAngles = localEulerAngles;
		}
		if ((bool)_sound)
		{
			_sound.UpdateSound(inputY, hasDriverAndEnergy);
		}
	}
}
