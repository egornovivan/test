using UnityEngine;

public class UIActiveChange : MonoBehaviour
{
	[SerializeField]
	private GameObject mTarget;

	private void OnEnable()
	{
		if (mTarget != null)
		{
			mTarget.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if (mTarget != null)
		{
			mTarget.SetActive(value: false);
		}
	}
}
