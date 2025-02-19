using System;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class CharacterControllerDefault : CharacterControllerBase
{
	[Serializable]
	public class State
	{
		public string clipName;

		public float animationSpeed = 1f;

		public float moveSpeed = 1f;
	}

	[Serializable]
	public enum RotationMode
	{
		Slerp,
		RotateTowards
	}

	public CameraController cam;

	public State[] states;

	public int idleStateIndex;

	public int walkStateIndex = 1;

	public int runStateIndex = 2;

	public float acceleration = 5f;

	public float speedAcceleration = 3f;

	public float angularSpeed = 7f;

	public RotationMode rotationMode;

	protected State state;

	protected Vector3 moveVector;

	protected float speed;

	protected virtual float accelerationMlp => 1f;

	protected virtual void Update()
	{
		if (GetInputDirection() != Vector3.zero)
		{
			state = ((!Input.GetKey(KeyCode.LeftShift)) ? states[walkStateIndex] : states[runStateIndex]);
		}
		else
		{
			state = states[idleStateIndex];
		}
		Vector3 vector = Quaternion.LookRotation(new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z)) * GetInputDirection();
		if (vector != Vector3.zero)
		{
			Vector3 vector2 = Quaternion.FromToRotation(base.transform.forward, vector) * base.transform.forward;
			Vector3 vector3 = base.transform.forward;
			switch (rotationMode)
			{
			case RotationMode.Slerp:
				vector3 = Vector3.Slerp(vector3, vector2, Time.deltaTime * angularSpeed * accelerationMlp);
				break;
			case RotationMode.RotateTowards:
				vector3 = Vector3.RotateTowards(vector3, vector2, Time.deltaTime * angularSpeed * accelerationMlp, 1f);
				break;
			}
			vector3.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(vector3);
		}
		moveVector = Vector3.Lerp(moveVector, vector, Time.deltaTime * acceleration * accelerationMlp);
		speed = Mathf.Lerp(speed, state.moveSpeed, Time.deltaTime * speedAcceleration * accelerationMlp);
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().position += moveVector * Time.deltaTime * speed;
		}
		else
		{
			base.transform.position += moveVector * Time.deltaTime * speed;
		}
	}
}
