using UnityEngine;

public class ColliderClickTrigger : TriggerController
{
	public Collider mCollider;

	public string mFuncName = "OnClick";

	public bool mLeftBtn = true;

	public bool mRightBtn;

	protected override void InitDefault()
	{
		base.InitDefault();
		if (null == mCollider)
		{
			mCollider = GetComponent<Collider>();
		}
	}

	protected override bool CheckTrigger()
	{
		if ((Input.GetMouseButtonDown(0) && mLeftBtn) || (Input.GetMouseButtonDown(1) && mRightBtn))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out var hitInfo, 1000f, -4195329) && hitInfo.collider == mCollider)
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnHitTraigger()
	{
		mTrigerTarget.SendMessage(mFuncName, SendMessageOptions.DontRequireReceiver);
	}
}
