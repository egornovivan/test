using UnityEngine;

namespace PeUIEffect;

public class UISpriteScaleEffect : UIEffect
{
	[SerializeField]
	private Vector3 mMaxScale;

	[SerializeField]
	private float mSpeed = 80f;

	private UISprite mSpr;

	public override void Play()
	{
		base.Play();
		if (mSpr != null)
		{
			Vector3 localScale = base.gameObject.transform.localScale;
			localScale.x = 0f;
			mSpr.transform.localScale = localScale;
		}
	}

	public override void End()
	{
		base.End();
	}

	private void Start()
	{
		mSpr = GetComponent<UISprite>();
		Play();
	}

	private void Update()
	{
		if (m_Runing && mSpr != null)
		{
			if (base.gameObject.transform.localScale.x < mMaxScale.x)
			{
				Vector3 localScale = base.gameObject.transform.localScale;
				localScale.x += (int)(mSpeed * Time.deltaTime);
				base.gameObject.transform.localScale = localScale;
			}
			else
			{
				End();
			}
		}
	}
}
