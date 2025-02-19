using PeUIEffect;
using UnityEngine;

public abstract class UIBaseSighting : MonoBehaviour
{
	[SerializeField]
	private float mValue;

	public bool bShoot;

	protected UISightingOnShootEffect mShootEffect;

	public float Value
	{
		get
		{
			return mValue;
		}
		set
		{
			mValue = value;
		}
	}

	public virtual void OnShoot()
	{
		bShoot = true;
	}

	protected virtual void Start()
	{
		mShootEffect = GetComponent<UISightingOnShootEffect>();
	}

	protected virtual void Update()
	{
		if (bShoot)
		{
			if (mShootEffect != null)
			{
				mShootEffect.Play();
			}
			bShoot = false;
		}
		if (mShootEffect != null)
		{
			mValue += mShootEffect.Value;
		}
	}
}
