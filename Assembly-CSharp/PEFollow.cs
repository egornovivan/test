using UnityEngine;

public class PEFollow : MonoBehaviour
{
	public Transform master;

	public static void Follow(Transform follower, Transform master)
	{
		if (follower != null && master != null)
		{
			PEFollow pEFollow = follower.gameObject.AddComponent<PEFollow>();
			pEFollow.master = master;
		}
	}

	private void LateUpdate()
	{
		if (master != null)
		{
			base.transform.position = master.position;
			base.transform.rotation = master.rotation;
			if (base.transform.parent == null || base.transform.parent != master.parent)
			{
				base.transform.parent = master.parent;
			}
		}
	}
}
