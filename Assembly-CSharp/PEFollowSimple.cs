using UnityEngine;

public class PEFollowSimple : MonoBehaviour
{
	public Transform master;

	private void LateUpdate()
	{
		if (master != null)
		{
			base.transform.position = master.position;
			base.transform.rotation = master.rotation;
		}
	}
}
