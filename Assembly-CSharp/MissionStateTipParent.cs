using UnityEngine;

public class MissionStateTipParent : MonoBehaviour
{
	[HideInInspector]
	public Transform mParent;

	public bool IsFree()
	{
		return mParent.childCount == 0;
	}

	public void DeleteChild()
	{
		for (int i = 0; i < mParent.childCount; i++)
		{
			Object.Destroy(mParent.GetChild(i));
		}
	}

	private void Awake()
	{
		mParent = base.transform;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
