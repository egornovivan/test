using UnityEngine;

public class UISystemMenu : UIStaticWnd
{
	private static UISystemMenu mInstance;

	public N_ImageButton mSaveBtn;

	public N_ImageButton mLoadBtn;

	public GameObject mMultyWnd;

	public static UISystemMenu Instance => mInstance;

	public override bool isShow
	{
		get
		{
			if (!GameConfig.IsMultiMode)
			{
				if (mWndCenter != null)
				{
					return mWndCenter.activeSelf;
				}
				return base.gameObject.activeSelf;
			}
			if (mMultyWnd != null)
			{
				return mMultyWnd.activeSelf;
			}
			return base.gameObject.activeSelf;
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

	private void Awake()
	{
		mInstance = this;
	}

	public bool isOpen()
	{
		return mWndCenter.activeSelf || mMultyWnd.activeSelf;
	}

	public override void Show()
	{
		if (e_OnShow != null)
		{
			e_OnShow(this);
		}
		PlayShowTween();
		if (GameConfig.IsMultiMode)
		{
			mWndCenter.SetActive(value: false);
			mMultyWnd.SetActive(value: true);
		}
		else
		{
			mWndCenter.SetActive(value: true);
			mMultyWnd.SetActive(value: false);
		}
	}

	protected override void OnHide()
	{
		mWndCenter.SetActive(value: false);
		mMultyWnd.SetActive(value: false);
		if (e_OnHide != null)
		{
			e_OnHide(this);
		}
	}

	public void OnSaveBtn()
	{
		UISaveLoad.Instance.Show();
		UISaveLoad.Instance.ToSaveWnd();
		Hide();
	}

	public void OnLoadBtn()
	{
		UISaveLoad.Instance.Show();
		UISaveLoad.Instance.ToLoadWnd();
		Hide();
	}

	private void OnLobbyBtn()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000078), PeSceneCtrl.Instance.GotoLobbyScene);
	}

	private void OnOptionBtn()
	{
		Hide();
		UIOption.Instance.Show();
		UIOption.Instance.OnVideoBtn();
	}

	private void OnMainMenuBtn()
	{
		Hide();
		if (GameConfig.IsMultiMode)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000079), PeSceneCtrl.Instance.GotoMainMenuScene, Show);
		}
		else
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000080), PeSceneCtrl.Instance.GotoMainMenuScene, Show);
		}
	}

	private void OnQuitGameBtn()
	{
		Hide();
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000081), Application.Quit, Show);
	}

	private void OnResumeBtn()
	{
		Hide();
	}

	public static bool IsSystemOping()
	{
		return (Instance != null && Instance.isOpen()) || (UISaveLoad.Instance != null && UISaveLoad.Instance.isShow) || (UIOption.Instance != null && UIOption.Instance.isShow);
	}
}
