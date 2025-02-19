using UnityEngine;

public class UIWorkShopGridItem : MonoBehaviour
{
	public delegate void ClickFunc(WorkGridItemType mType, int index);

	public UITexture mTextureContent;

	public UISprite mContentSprite;

	public UISprite mSeletedSprite;

	public UILabel mIsoName;

	public UILabel mIsoCreateName;

	public UILabel mLbDingText;

	public UILabel mLbCaiText;

	public UILabel mLbUpDownText;

	public UISprite mSpUpDown;

	public GameObject mUpDown;

	public Texture2D mUnkonwTexture;

	public GameObject[] mBtns;

	public WorkGridItemType mType = WorkGridItemType.mNull;

	public int index = -1;

	private bool m_DownLoad;

	private bool m_IsActiveLoading;

	private bool m_ActiveUpDown;

	private string m_IsoFileName = string.Empty;

	public string IsoFileName => m_IsoFileName;

	public event ClickFunc mClickItem;

	public event ClickFunc mDoubleClickItem;

	public event ClickFunc mBtnReloadOnClick;

	public event ClickFunc mBtnCaiOnClick;

	public event ClickFunc mBtnDingOnClick;

	public void ActiveUpDown(bool isActive)
	{
		m_ActiveUpDown = isActive;
		mUpDown.SetActive(isActive);
	}

	public void UpdteUpDownInfo(string infoText)
	{
		mLbUpDownText.text = infoText;
	}

	public void InitItem(WorkGridItemType ItemItye, string _isoName)
	{
		mType = ItemItye;
		mIsoName.text = _isoName;
		UpdateIsoFileName(_isoName);
		SetUIState();
	}

	public void SetDingText(string strText)
	{
		mLbDingText.text = strText;
	}

	public void SetCaiText(string strText)
	{
		mLbCaiText.text = strText;
	}

	public void ActiveVoteUI(bool isActive)
	{
		for (int i = 0; i < mBtns.Length - 1; i++)
		{
			mBtns[i].SetActive(isActive);
		}
	}

	public void SetIsoName(string _isoName)
	{
		mIsoName.text = _isoName;
		UpdateIsoFileName(_isoName);
	}

	public void SetAuthor(string _CreatorName)
	{
		if (mType == WorkGridItemType.mWorkShop)
		{
			mIsoCreateName.text = PELocalization.GetString(8000692) + ":" + _CreatorName;
		}
		else if (mType == WorkGridItemType.mUpLoad)
		{
			mIsoCreateName.text = PELocalization.GetString(8000692) + ":" + _CreatorName;
		}
		else if (mType == WorkGridItemType.mLocalIcon)
		{
			mIsoCreateName.text = PELocalization.GetString(8000693) + ":" + _CreatorName;
		}
		else
		{
			mIsoCreateName.enabled = false;
		}
	}

	public void SetIco(Texture2D texture)
	{
		if (texture == null)
		{
			texture = mUnkonwTexture;
		}
		if (mType == WorkGridItemType.mLocalIcon)
		{
			mTextureContent.enabled = true;
			mTextureContent.mainTexture = texture;
			mContentSprite.enabled = false;
		}
		else if (mType == WorkGridItemType.mLocalFloder)
		{
			mContentSprite.spriteName = "folder_icon";
			mContentSprite.enabled = true;
			mTextureContent.enabled = false;
		}
		else if (mType == WorkGridItemType.mWorkShop)
		{
			mTextureContent.enabled = true;
			mTextureContent.mainTexture = texture;
			mContentSprite.enabled = false;
		}
		else if (mType == WorkGridItemType.mUpLoad)
		{
			mTextureContent.enabled = true;
			mTextureContent.mainTexture = texture;
			mContentSprite.enabled = false;
		}
	}

	public void ActiveLoadingItem(bool isStar)
	{
		if (isStar)
		{
			mContentSprite.spriteName = "icoloading";
			mContentSprite.enabled = true;
			mTextureContent.enabled = false;
			SetDownloaded(download: false);
		}
		else
		{
			mContentSprite.enabled = false;
			mTextureContent.enabled = true;
		}
		m_IsActiveLoading = isStar;
	}

	public void SetSelected(bool Selected)
	{
		if (mSeletedSprite.enabled != Selected)
		{
			mSeletedSprite.enabled = Selected;
		}
	}

	public void SetDownloaded(bool download)
	{
		if (download)
		{
			ActiveUpDown(isActive: true);
			mSpUpDown.spriteName = "clouddown";
			mLbUpDownText.text = PELocalization.GetString(8000694);
		}
		else
		{
			mLbUpDownText.text = string.Empty;
			ActiveUpDown(isActive: false);
		}
	}

	private void SetUIState()
	{
		if (mType == WorkGridItemType.mWorkShop)
		{
			mIsoCreateName.enabled = true;
			ActiveVoteUI(isActive: false);
			mSpUpDown.spriteName = "clouddown";
		}
		else if (mType == WorkGridItemType.mLocalIcon)
		{
			mIsoCreateName.enabled = true;
			for (int i = 0; i < mBtns.Length; i++)
			{
				mBtns[i].SetActive(value: false);
			}
			mSpUpDown.spriteName = "cloudup";
		}
		else if (mType == WorkGridItemType.mUpLoad)
		{
			mIsoCreateName.enabled = true;
			ActiveVoteUI(isActive: false);
			mBtns[2].SetActive(value: true);
			mSpUpDown.spriteName = "clouddown";
		}
		else if (mType != WorkGridItemType.mUpDown && mType == WorkGridItemType.mLocalFloder)
		{
			mIsoCreateName.enabled = false;
			for (int j = 0; j < mBtns.Length; j++)
			{
				mBtns[j].SetActive(value: false);
			}
			ActiveUpDown(isActive: false);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_IsActiveLoading)
		{
			Vector3 localEulerAngles = mContentSprite.transform.localEulerAngles;
			float z = localEulerAngles.z - Time.deltaTime * 200f;
			mContentSprite.transform.localEulerAngles = new Vector3(localEulerAngles.x, localEulerAngles.y, z);
		}
	}

	private void GridItemOnClick()
	{
		if (Input.GetMouseButtonUp(0) && this.mClickItem != null)
		{
			this.mClickItem(mType, index);
		}
	}

	private void GridItemDoubleClick()
	{
		if (Input.GetMouseButtonUp(0) && this.mDoubleClickItem != null)
		{
			this.mDoubleClickItem(mType, index);
		}
	}

	private void BtnReloadOnClick()
	{
		if (!m_IsActiveLoading && Input.GetMouseButtonUp(0) && this.mBtnReloadOnClick != null)
		{
			this.mBtnReloadOnClick(mType, index);
		}
	}

	private void BtnCaiOnClick()
	{
		if (Input.GetMouseButtonUp(0) && this.mBtnCaiOnClick != null)
		{
			this.mBtnCaiOnClick(mType, index);
		}
	}

	private void BtnDingOnClick()
	{
		if (Input.GetMouseButtonUp(0) && this.mBtnDingOnClick != null)
		{
			this.mBtnDingOnClick(mType, index);
		}
	}

	private void UpdateIsoFileName(string isoName)
	{
		m_IsoFileName = isoName + VCConfig.s_IsoFileExt;
	}
}
