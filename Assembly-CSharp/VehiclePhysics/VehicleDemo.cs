using System;
using System.Reflection;
using UnityEngine;

namespace VehiclePhysics;

public class VehicleDemo : MonoBehaviour
{
	public VehicleEngine engine;

	private void Awake()
	{
		Type typeFromHandle = typeof(WheelCollider);
		FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			Debug.Log(fieldInfo.Name);
		}
	}

	private void FixedUpdate()
	{
		Vector3 zero = Vector3.zero;
		bool handBrake = false;
		if (Input.GetKey(KeyCode.W))
		{
			zero.z += 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero.z += -1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			zero.x += -1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			zero.x += 1f;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			zero.y += -1f;
		}
		if (Input.GetKey(KeyCode.LeftControl))
		{
			zero.y += 1f;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			handBrake = true;
		}
		engine.Drive(zero, handBrake);
	}
}
