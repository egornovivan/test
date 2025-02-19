using UnityEngine;

public class UIBtnTouZi : MonoBehaviour
{
	public delegate void OnClickFunc();

	[SerializeField]
	private UISprite mCotentSpr;

	[SerializeField]
	private Collider mCollider;

	[SerializeField]
	private int mFrameCount;

	private string[] mSprites = new string[4];

	private bool isRun;

	private int tempFrame;

	public event OnClickFunc e_EndRun;

	private void Start()
	{
		mCollider.enabled = !isRun;
		mSprites[0] = "touzi_1";
		mSprites[1] = "touzi_2";
		mSprites[2] = "touzi_3";
		mSprites[3] = "touzi_4";
	}

	private void BtnRandomOnClick()
	{
		isRun = true;
	}

	private void Update()
	{
		if (isRun)
		{
			tempFrame++;
		}
		if (tempFrame > 0)
		{
			if (tempFrame < mFrameCount)
			{
				mCotentSpr.spriteName = mSprites[1];
			}
			else if (tempFrame < 2 * mFrameCount)
			{
				mCotentSpr.spriteName = mSprites[2];
			}
			else if (tempFrame < 3 * mFrameCount)
			{
				mCotentSpr.spriteName = mSprites[3];
			}
			else
			{
				mCotentSpr.spriteName = mSprites[0];
				tempFrame = 0;
				isRun = false;
				mCotentSpr.color = new Color(0.8f, 0.8f, 0.8f, 1f);
				if (this.e_EndRun != null)
				{
					this.e_EndRun();
				}
			}
		}
		mCollider.enabled = !isRun;
	}
}
