using UnityEngine;

namespace EVP;

public class VehicleStandardInput : MonoBehaviour
{
	public VehicleController target;

	public bool continuousForwardAndReverse = true;

	public string steerAxis = "Horizontal";

	public string throttleAndBrakeAxis = "Vertical";

	public string handbrakeAxis = "Jump";

	public KeyCode resetVehicleKey = KeyCode.Return;

	private bool m_doReset;

	private void OnEnable()
	{
		if (target == null)
		{
			target = GetComponent<VehicleController>();
		}
	}

	private void Update()
	{
		if (!(target == null) && Input.GetKeyDown(resetVehicleKey))
		{
			m_doReset = true;
		}
	}

	private void FixedUpdate()
	{
		if (target == null)
		{
			return;
		}
		float steerInput = Mathf.Clamp(Input.GetAxis(steerAxis), -1f, 1f);
		float num = Mathf.Clamp01(Input.GetAxis(throttleAndBrakeAxis));
		float num2 = Mathf.Clamp01(0f - Input.GetAxis(throttleAndBrakeAxis));
		float handbrakeInput = Mathf.Clamp01(Input.GetAxis(handbrakeAxis));
		float throttleInput = 0f;
		float brakeInput = 0f;
		if (continuousForwardAndReverse)
		{
			float num3 = 0.1f;
			float num4 = 0.1f;
			if (target.speed > num3)
			{
				throttleInput = num;
				brakeInput = num2;
			}
			else if (num2 > num4)
			{
				throttleInput = 0f - num2;
				brakeInput = 0f;
			}
			else if (num > num4)
			{
				if (target.speed < 0f - num3)
				{
					throttleInput = 0f;
					brakeInput = num;
				}
				else
				{
					throttleInput = num;
					brakeInput = 0f;
				}
			}
		}
		else if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
		{
			throttleInput = num;
			brakeInput = num2;
		}
		else
		{
			throttleInput = 0f - num2;
			brakeInput = 0f;
		}
		target.steerInput = steerInput;
		target.throttleInput = throttleInput;
		target.brakeInput = brakeInput;
		target.handbrakeInput = handbrakeInput;
		if (m_doReset)
		{
			target.ResetVehicle();
			m_doReset = false;
		}
	}
}
