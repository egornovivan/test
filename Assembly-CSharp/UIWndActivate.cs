using UnityEngine;
using UnityEngine.Events;

public class UIWndActivate : MonoBehaviour
{
	public BoxCollider mCollider;

	[SerializeField]
	private Vector3 mBgLocalPos;

	[SerializeField]
	private UnityEvent OnActive;

	[SerializeField]
	private UnityEvent OnDeactive;

	public Vector3 BgLocalPos => mBgLocalPos;

	private void Start()
	{
		if (!(mCollider == null))
		{
			mCollider.center = Vector3.zero;
			mCollider.size = Vector3.one;
			mBgLocalPos = mCollider.transform.localPosition;
			Transform parent = mCollider.transform.parent;
			do
			{
				mBgLocalPos += parent.localPosition;
				parent = parent.parent;
			}
			while (parent != base.transform && parent != null);
		}
	}

	public void Activate()
	{
		OnActive.Invoke();
		if (mCollider != null)
		{
			mCollider.center = new Vector3(0f, 0f, 0f);
		}
	}

	public void Deactivate()
	{
		OnDeactive.Invoke();
		if (mCollider != null)
		{
			mCollider.center = new Vector3(0f, 0f, -13.5f);
		}
	}
}
