using UnityEngine;

namespace AnimFollow;

public class Limb_AF : MonoBehaviour
{
	public readonly int version = 4;

	private RagdollControl_AF ragdollControl;

	private string[] ignoreCollidersWithTag;

	private void OnEnable()
	{
		ragdollControl = base.transform.root.GetComponentInChildren<RagdollControl_AF>();
		ignoreCollidersWithTag = ragdollControl.ignoreCollidersWithTag;
	}

	private void OnCollisionEnter(Collision collision)
	{
		bool flag = false;
		if (collision.transform.name == "Terrain" || !(collision.transform.root != base.transform.root))
		{
			return;
		}
		string[] array = ignoreCollidersWithTag;
		foreach (string text in array)
		{
			if (collision.transform.tag == text)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ragdollControl.numberOfCollisions++;
			ragdollControl.collisionSpeed = collision.relativeVelocity.magnitude;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		bool flag = false;
		if (collision.transform.name == "Terrain" || !(collision.transform.root != base.transform.root))
		{
			return;
		}
		string[] array = ignoreCollidersWithTag;
		foreach (string text in array)
		{
			if (collision.transform.tag == text)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ragdollControl.numberOfCollisions--;
		}
	}
}
