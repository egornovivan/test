using UnityEngine;

public class TriggerController : MonoBehaviour
{
	public GameObject mTrigerTarget;

	private void Start()
	{
		InitDefault();
	}

	private void Update()
	{
		if (CheckTrigger())
		{
			OnHitTraigger();
		}
	}

	protected virtual void InitDefault()
	{
		if (null == mTrigerTarget)
		{
			mTrigerTarget = base.gameObject;
		}
	}

	protected virtual bool CheckTrigger()
	{
		return false;
	}

	protected virtual void OnHitTraigger()
	{
	}
}
