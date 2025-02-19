using System.Collections.Generic;
using UnityEngine;

public class DestroyAI : MonoBehaviour
{
	public int[] destroyIds;

	private List<Collider> colliders = new List<Collider>();

	public void DestroyMatchAI()
	{
		foreach (Collider collider in colliders)
		{
			DestroyMatch(collider);
		}
	}

	private void DestroyMatch(Collider other)
	{
		if (!(other == null))
		{
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(other == null))
		{
			if (!colliders.Contains(other))
			{
				colliders.Add(other);
			}
			DestroyMatch(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!(other == null) && colliders.Contains(other))
		{
			colliders.Remove(other);
		}
	}
}
