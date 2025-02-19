using UnityEngine;

public class UIFollow : MonoBehaviour
{
	public Transform Target;

	private void Update()
	{
		base.transform.position = Target.position;
	}
}
