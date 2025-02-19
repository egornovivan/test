using UnityEngine;

public class UIOnScrollMouse : MonoBehaviour
{
	private BoxCollider mBoxClollider;

	private void Start()
	{
		mBoxClollider = base.gameObject.GetComponent<BoxCollider>();
	}
}
