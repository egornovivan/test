using UnityEngine;

public class UIFlyHeight : MonoBehaviour
{
	[SerializeField]
	private UILabel mLbValue;

	[SerializeField]
	private UISprite mLbUp;

	[SerializeField]
	private UISprite mLbDn;

	[SerializeField]
	private Color mSpColor;

	[SerializeField]
	private float mUpdateTime;

	private int mValue;

	private float time;

	public int Value
	{
		get
		{
			return mValue;
		}
		set
		{
			mValue = value;
			mLbValue.text = mValue.ToString();
		}
	}

	private void Update()
	{
		if (mLbValue == null || mLbUp == null || mLbDn == null)
		{
			return;
		}
		if (time > mUpdateTime)
		{
			if (mValue > 0)
			{
				mLbUp.color = ((!(mLbUp.color == mSpColor)) ? mSpColor : Color.white);
				mLbDn.color = Color.white;
			}
			else if (mValue == 0)
			{
				mLbUp.color = Color.white;
				mLbDn.color = Color.white;
			}
			else
			{
				mLbUp.color = Color.white;
				mLbDn.color = ((!(mLbDn.color == mSpColor)) ? mSpColor : Color.white);
			}
			time = 0f;
		}
		time += Time.deltaTime;
	}
}
