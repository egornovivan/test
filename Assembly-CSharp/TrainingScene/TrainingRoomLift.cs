using Pathea;
using UnityEngine;

namespace TrainingScene;

public class TrainingRoomLift : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		IKCmpt componentInParent = other.transform.GetComponentInParent<IKCmpt>();
		if (null != componentInParent)
		{
			componentInParent.ikEnable = false;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		IKCmpt componentInParent = other.transform.GetComponentInParent<IKCmpt>();
		if (null != componentInParent)
		{
			componentInParent.ikEnable = true;
		}
	}
}
