using Steer3D;
using UnityEngine;

public class SteerMotor : MonoBehaviour
{
	private SteerAgent agent;

	private void OnSteer()
	{
		if (agent == null)
		{
			agent = GetComponent<SteerAgent>();
		}
		if (agent != null)
		{
			if (agent.forward.sqrMagnitude > 0.001f)
			{
				base.transform.forward = agent.forward;
			}
			base.transform.position += agent.velocity * agent.maxSpeed * Time.deltaTime;
		}
	}
}
