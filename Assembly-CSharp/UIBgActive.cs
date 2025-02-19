using UnityEngine;

public class UIBgActive : MonoBehaviour
{
	[SerializeField]
	private UISprite mMainBg_1;

	[SerializeField]
	private UISprite mMainBg_2;

	private bool mActive;

	public bool bActive
	{
		set
		{
			mActive = value;
		}
	}

	private void Update()
	{
		if (mActive)
		{
			mMainBg_1.color = Color.Lerp(mMainBg_1.color, new Color(1f, 1f, 1f, 1f), 2f * Time.deltaTime);
			mMainBg_2.color = Color.Lerp(mMainBg_2.color, new Color(0f, 0.13f, 1f, 0.3f), 2f * Time.deltaTime);
		}
		else
		{
			mMainBg_2.color = Color.Lerp(mMainBg_2.color, new Color(0f, 0.13f, 1f, 0f), 2f * Time.deltaTime);
			mMainBg_1.color = Color.Lerp(mMainBg_1.color, new Color(0.925f, 0.725f, 0.93f, 1f), 2f * Time.deltaTime);
		}
	}
}
