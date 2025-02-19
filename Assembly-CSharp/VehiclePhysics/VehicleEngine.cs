using UnityEngine;

namespace VehiclePhysics;

[RequireComponent(typeof(Rigidbody))]
public class VehicleEngine : MonoBehaviour
{
	public VehicleWheel[] wheels = new VehicleWheel[0];

	public float realMass = 10000f;

	public float maxPower;

	public float maxMotorTorque;

	public bool showInfo;

	private Rigidbody rigid;

	private float currPower;

	private float currMotorTorque;

	private void Awake()
	{
		rigid = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		for (int i = 0; i < wheels.Length; i++)
		{
			wheels[i].Init(this);
		}
	}

	public void Drive(Vector3 inputAxis, bool handBrake)
	{
		for (int i = 0; i < wheels.Length; i++)
		{
			wheels[i].motorTorque = wheels[i].maxMotorTorque * inputAxis.z;
			wheels[i].brakeTorque = wheels[i].dynamicBrakeTorque;
			wheels[i].SetWheelTorques();
		}
	}

	private void FixedUpdate()
	{
		rigid.centerOfMass = Vector3.up * 0.5f;
	}

	private void LateUpdate()
	{
		for (int i = 0; i < wheels.Length; i++)
		{
			wheels[i].SyncModel();
		}
	}
}
