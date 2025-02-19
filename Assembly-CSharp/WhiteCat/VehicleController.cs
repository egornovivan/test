using System.Collections.Generic;
using UnityEngine;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat;

public class VehicleController : CarrierController
{
	private VCPVehicleEngine _engine;

	private VCPVehicleWheel[] _wheels;

	private VCPVehicleWheel[] _motorWheels;

	private float _inputBrake = 1f;

	private float _wheelRangeWidth;

	public float inputBrake => _inputBrake;

	protected override float mass => Mathf.Clamp(base.creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.vehicleMassScale, PEVCConfig.instance.vehicleMinMass, PEVCConfig.instance.vehicleMaxMass);

	protected override Vector3 centerOfMass => new Vector3(0f, base.creationController.creationData.m_Attribute.m_CenterOfMass.y * 0.7f, base.creationController.bounds.size.z * 0f);

	protected override Vector3 inertiaTensorScale => PEVCConfig.instance.vehicleInertiaTensorScale;

	protected override void InitDrags(out float standardDrag, out float underwaterDrag, out float standardAngularDrag, out float underwaterAngularDrag)
	{
		standardDrag = PEVCConfig.instance.vehicleStandardDrag;
		underwaterDrag = PEVCConfig.instance.vehicleUnderwaterDrag;
		standardAngularDrag = PEVCConfig.instance.vehicleStandardAngularDrag;
		underwaterAngularDrag = PEVCConfig.instance.vehicleUnderwaterAngularDrag;
	}

	protected override void InitOtherThings()
	{
		base.InitOtherThings();
		LoadPart(ref _engine);
		LoadParts(ref _wheels);
		List<VCPVehicleWheel> list = new List<VCPVehicleWheel>(_wheels.Length);
		List<VCPVehicleWheel> list2 = new List<VCPVehicleWheel>(_wheels.Length);
		VCPVehicleWheel[] wheels = _wheels;
		foreach (VCPVehicleWheel vCPVehicleWheel in wheels)
		{
			if (vCPVehicleWheel.isSteerWheel)
			{
				list.Add(vCPVehicleWheel);
			}
			if (vCPVehicleWheel.isMotorWheel)
			{
				list2.Add(vCPVehicleWheel);
			}
		}
		_motorWheels = list2.ToArray();
		Vector3 localPosition = _wheels[0].transform.localPosition;
		float z = localPosition.z;
		float z2 = localPosition.z;
		float x = localPosition.x;
		float x2 = localPosition.x;
		VCPVehicleWheel[] wheels2 = _wheels;
		foreach (VCPVehicleWheel vCPVehicleWheel2 in wheels2)
		{
			localPosition = vCPVehicleWheel2.transform.localPosition;
			if (localPosition.z > z)
			{
				z = localPosition.z;
			}
			if (localPosition.z < z2)
			{
				z2 = localPosition.z;
			}
			if (localPosition.x > x)
			{
				x = localPosition.x;
			}
			if (localPosition.x < x2)
			{
				x2 = localPosition.x;
			}
		}
		_wheelRangeWidth = x - x2;
		float maxWheelSteerAngle = PEVCConfig.instance.maxWheelSteerAngle;
		VCPVehicleWheel[] wheels3 = _wheels;
		foreach (VCPVehicleWheel vCPVehicleWheel3 in wheels3)
		{
			Vector3 localPosition2 = vCPVehicleWheel3.transform.localPosition;
			float num = ((!(z - z2 < 0.1f)) ? ((localPosition2.z - (z + z2) * 0.5f) / ((z - z2) * 0.5f)) : 1f);
			float num2 = Interpolation.EaseOut(Mathf.Clamp01(_wheelRangeWidth * 0.125f)) * PEVCConfig.instance.vehicleSteerRadiusExtend + PEVCConfig.instance.vehicleSteerRadiusBase;
			float num3 = maxWheelSteerAngle + Mathf.Asin((x - localPosition2.x) * Mathf.Sin(maxWheelSteerAngle) / num2);
			float num4 = maxWheelSteerAngle + Mathf.Asin((localPosition2.x - x2) * Mathf.Sin(maxWheelSteerAngle) / num2);
			vCPVehicleWheel3.Init(this, num3 * num, num4 * num);
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.rigidbody.isKinematic)
		{
			return;
		}
		if (_wheelRangeWidth < 0.5f)
		{
			Vector3 forward = base.transform.forward;
			Vector3 rhs = Vector3.Cross(Vector3.up, forward);
			if (rhs.sqrMagnitude > 0.01f && Vector3.Dot(base.transform.up, Vector3.up) > 0.25f)
			{
				rhs = Vector3.Cross(forward, rhs);
				rhs = base.transform.InverseTransformDirection(rhs);
				float num = (0f - Mathf.Atan2(rhs.x, rhs.y)) * 57.29578f;
				num -= base.inputX * PEVCConfig.instance.motorcycleBiasAngle * Mathf.Clamp01(Mathf.Abs(Vector3.Dot(base.rigidbody.velocity, forward)) * 0.1f);
				rhs = base.rigidbody.angularVelocity;
				base.rigidbody.angularVelocity = rhs + (1f - _wheelRangeWidth - _wheelRangeWidth) * (num * PEVCConfig.instance.motorcycleBalanceHelp * forward - Vector3.Project(rhs, forward));
			}
		}
		for (int i = 0; i < _wheels.Length; i++)
		{
			_wheels[i].OnFixedUpdate(base.rigidbody.mass / (float)_wheels.Length);
		}
	}

	protected override void Update()
	{
		base.Update();
		float num = 0f;
		for (int i = 0; i < _motorWheels.Length; i++)
		{
			num += _motorWheels[i].rpm;
		}
		float num2 = _engine.UpdateEngine(this, num / (float)_motorWheels.Length);
		num2 /= (float)_motorWheels.Length;
		float rotateFactor = PEVCConfig.instance.speedToRotateFactor.Evaluate(Vector3.Project(base.rigidbody.velocity, base.transform.forward).magnitude);
		for (int j = 0; j < _wheels.Length; j++)
		{
			_wheels[j].OnUpdate(num2, rotateFactor);
		}
	}

	protected override uint EncodeInput(uint inputState)
	{
		inputState = base.EncodeInput(inputState);
		bool flag = PeInput.Get(PeInput.LogicFunction.Vehicle_Brake);
		inputState = inputState.SetBit(7, !flag);
		return inputState;
	}

	protected override void DecodeInput(uint inputState)
	{
		base.DecodeInput(inputState);
		float num = Vector3.Dot(base.rigidbody.velocity, base.transform.forward);
		float num2 = 0f;
		if ((base.inputY < 0f && num > 1f) || (base.inputY > 0f && num < -1f))
		{
			num2 = 1f;
		}
		else
		{
			bool flag = !inputState.GetBit(7);
			if (!flag && !base.isJetting && Mathf.Abs(base.inputY) < 0.01f && base.rigidbody.velocity.sqrMagnitude < 0.05f)
			{
				flag = true;
			}
			num2 = ((!flag) ? 0f : 1f);
		}
		_inputBrake = Mathf.MoveTowards(_inputBrake, num2, 4f * Time.deltaTime);
	}

	protected override uint OnDriverGetOff(uint inputState)
	{
		inputState = base.OnDriverGetOff(inputState);
		inputState = inputState.SetBit0(7);
		return inputState;
	}
}
