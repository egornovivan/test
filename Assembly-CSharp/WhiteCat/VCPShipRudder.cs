using System;
using UnityEngine;

namespace WhiteCat;

public class VCPShipRudder : VCPart
{
	[SerializeField]
	private Transform _pivot;

	[SerializeField]
	private float _steerAngle = 40f;

	[SerializeField]
	private float _steerFactor;

	private BoatController _controller;

	private Vector3 _angles;

	public void Init(BoatController controller)
	{
		_controller = controller;
		if ((_controller.transform.InverseTransformPoint(_pivot.position) - controller.rigidbody.centerOfMass).z < 0f)
		{
			_steerAngle = 0f - _steerAngle;
		}
		_steerFactor = (base.transform.localScale.y + base.transform.localScale.z) * _steerFactor * (float)(Math.Log(_controller.rigidbody.mass / 6666f + 1f) * 6666.0);
		base.enabled = true;
	}

	private void FixedUpdate()
	{
		_angles.y = _controller.inputX * _steerAngle;
		base.transform.localEulerAngles = _angles;
		if (VFVoxelWater.self.IsInWater(_pivot.position))
		{
			float value = Vector3.Dot(_controller.rigidbody.velocity, _controller.transform.forward);
			float num = Mathf.Clamp(value, -4f, 4f) * 0.25f;
			num = Mathf.Sign(num) * num * num * _steerFactor * _controller.inputX;
			_controller.rigidbody.AddRelativeTorque(0f, num * _controller.speedScale, 0f);
		}
	}
}
