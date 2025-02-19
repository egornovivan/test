using UnityEngine;

public abstract class UIStaticWnd : UIBaseWidget
{
	public GameObject mWndCenter;

	public override bool isShow
	{
		get
		{
			if (mWndCenter != null)
			{
				return mWndCenter.activeInHierarchy;
			}
			return base.gameObject.activeInHierarchy;
		}
		set
		{
			if (base.gameObject != null)
			{
				if (value)
				{
					Show();
				}
				else
				{
					Hide();
				}
			}
		}
	}

	public override void OnCreate()
	{
		base.OnCreate();
	}

	public override void Show()
	{
		if (!CanOpenWnd())
		{
			return;
		}
		if (e_OnShow != null)
		{
			e_OnShow(this);
		}
		PlayShowTween();
		base.PlayOpenSoundEffect();
		if (!mInit)
		{
			InitWindow();
		}
		if (mInit)
		{
			if (mWndCenter != null)
			{
				mWndCenter.SetActive(value: true);
			}
			else
			{
				base.gameObject.SetActive(value: true);
			}
		}
	}

	protected override void OnHide()
	{
		if (mWndCenter != null)
		{
			mWndCenter.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		if (e_OnHide != null)
		{
			e_OnHide(this);
		}
	}

	protected override void OnShowFinish()
	{
		if (e_OnShowFinish != null)
		{
			e_OnShowFinish();
		}
	}
}
