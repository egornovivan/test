using UnityEngine;

namespace PeUIEffect;

public class MenuParticleEffect : UIEffect
{
	[SerializeField]
	private UISprite mSpr1;

	[SerializeField]
	private UISprite mSpr2;

	[SerializeField]
	private float mSpeed = 1f;

	[SerializeField]
	private float mEndPos_y;

	[SerializeField]
	private Vector3 SprStartPos = new Vector3(78f, 55f, 0f);

	private UISprite mMoveSpr;

	public override void Play()
	{
		base.Play();
		mSpr1.transform.localPosition = SprStartPos;
		mSpr2.transform.localPosition = SprStartPos;
		mMoveSpr = mSpr1;
	}

	public override void End()
	{
		base.End();
	}

	private void Start()
	{
		Play();
	}

	private void Update()
	{
		if (!m_Runing || !(mMoveSpr != null))
		{
			return;
		}
		Vector3 localPosition = mMoveSpr.transform.localPosition;
		localPosition.y -= mSpeed * Time.deltaTime;
		mMoveSpr.transform.localPosition = localPosition;
		if (localPosition.y < mEndPos_y)
		{
			mMoveSpr.transform.localPosition = SprStartPos;
			if (mMoveSpr == mSpr1)
			{
				mMoveSpr = mSpr2;
			}
			else
			{
				mMoveSpr = mSpr1;
			}
		}
	}
}
