using UnityEngine;

public class UIBuildSaveWndCtrl : UIBaseWnd
{
	public delegate void ClickCancelFunc();

	public delegate bool ClickSaveFunc(string isoName);

	[SerializeField]
	private UILabel mInputIsoNmae;

	public UIBuildWndItem mSaveIsoItem;

	public string IsoName => mInputIsoNmae.text;

	public event ClickCancelFunc btnCanel;

	public event ClickCancelFunc OnWndClosed;

	public event ClickSaveFunc btnSave;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		if (this.OnWndClosed != null)
		{
			this.OnWndClosed();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetIsoItemContent(Texture contentTexture)
	{
		mSaveIsoItem.InitItem(UIBuildWndItem.ItemType.mNull, contentTexture, -1);
	}

	public void SetIsoItemContent(string contentSprName, string atlas)
	{
		mSaveIsoItem.InitItem(UIBuildWndItem.ItemType.mNull, contentSprName, atlas, -1);
	}

	public string GetIsoName()
	{
		string text = mInputIsoNmae.text;
		if (text == "need iso name")
		{
			text = string.Empty;
		}
		return text;
	}

	private void BtnSaveOnClick()
	{
		string text = mInputIsoNmae.text;
		if (text == "need iso name")
		{
			text = string.Empty;
		}
		if (text.Length != 0 && (this.btnSave == null || this.btnSave(text)))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void BtnCancelOnClick()
	{
		if (this.btnCanel != null)
		{
			this.btnCanel();
		}
		base.gameObject.SetActive(value: false);
	}

	private void BtnCloseOnClick()
	{
		BtnCancelOnClick();
	}
}
