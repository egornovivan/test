using Pathea;
using PeUIMap;
using UnityEngine;
using WhiteCat;

public class UIWorldMapCtrl : UIStaticWnd
{
	[SerializeField]
	private UIMap mStroyMap;

	[SerializeField]
	private UIMap mRandomMap;

	private UIMap mMap;

	public UIMap CurMap
	{
		get
		{
			if (null == mMap)
			{
				mMap = ((!bRandomMap) ? mStroyMap : mRandomMap);
			}
			return mMap;
		}
	}

	private bool bRandomMap => PeGameMgr.IsAdventure || PeGameMgr.IsBuild;

	public override void Show()
	{
		if (!PeGameMgr.IsCustom)
		{
			base.Show();
			CurMap.Show();
			if (null != LockUI.instance)
			{
				LockUI.instance.HideWhenUIPopup();
			}
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		CurMap.Hide();
		if (null != LockUI.instance)
		{
			LockUI.instance.ShowWhenUIDisappear();
		}
	}

	private void MaskYes()
	{
		CurMap.MaskYes();
	}

	private void WarpYes()
	{
		CurMap.OnWarpYes();
	}

	private void OnSelfMask0()
	{
		CurMap.ChangeMaskIcon(0);
	}

	private void OnSelfMask1()
	{
		CurMap.ChangeMaskIcon(1);
	}

	private void OnSelfMask2()
	{
		CurMap.ChangeMaskIcon(2);
	}

	private void OnSelfMask3()
	{
		CurMap.ChangeMaskIcon(3);
	}
}
