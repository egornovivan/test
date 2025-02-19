using UnityEngine;

public class GrassAlwaysVisible : MonoBehaviour
{
	private void Update()
	{
		if (Camera.main != null)
		{
			base.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 20f;
		}
	}
}
