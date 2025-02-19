using System;
using UnityEngine;

namespace WhiteCat;

public class SleepingUI : BaseBehaviour
{
	[SerializeField]
	private UIFilledSprite timeSprite;

	private Func<float> sleepTime;

	public void Show(Func<float> sleepTime01)
	{
		timeSprite.fillAmount = 0f;
		base.gameObject.SetActive(value: true);
		sleepTime = sleepTime01;
		base.enabled = true;
		Update();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		base.enabled = false;
	}

	private void Update()
	{
		timeSprite.fillAmount = sleepTime();
		GameUI.Instance.mItemOp.mMainWnd.SetActive(value: false);
	}
}
