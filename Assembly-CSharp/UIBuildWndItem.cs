using PeUIEffect;
using UnityEngine;

public class UIBuildWndItem : MonoBehaviour
{
	public enum ItemType
	{
		mNull,
		mBlockMat,
		mBlockPattern,
		mVoxelMat,
		mVoxelType,
		mIso,
		mCost,
		mMenu
	}

	public delegate void EventFunc(ItemType _ItemType, int _Index);

	public delegate void ToolToipFunc(bool isShow, ItemType _ItemType, int _Index);

	private const string AtlasType_button = "Button";

	private const string AtlasType_icon = "Icon";

	public int mIndex = -1;

	public ItemType mItemType;

	public int mTargetIndex = -1;

	public int mSubsetIndex = -1;

	public ItemType mTargetItemType;

	public bool mCanDrag;

	public bool mCanGetDrag;

	public UIAtlas mAtlasIcon;

	public UIFilledSprite mSkillCoold;

	public UISprite mSpriteSelect;

	public UILabel mNumber;

	public UILabel mText;

	public UILabel mTextIndex;

	public UISprite mBgSprite;

	public UISprite mIndexSprite;

	public UISprite mDeActiveSprite;

	[SerializeField]
	private UISprite m_QuickBarSprite;

	public UISprite mContentSprite;

	public UITexture mContentTexture;

	[SerializeField]
	private GameObject gridEffectPrefab;

	private int mItemId;

	public string atlas = string.Empty;

	private bool IsOnDrag;

	private bool mActive = true;

	private UIGridEffect effect;

	private bool moveIn;

	private float moveInTime;

	private GameObject _effectGo;

	public int ItemId => mItemId;

	public bool IsActive
	{
		get
		{
			return mActive;
		}
		set
		{
			if (mDeActiveSprite != null)
			{
				mDeActiveSprite.enabled = !value;
			}
			mActive = value;
		}
	}

	public event EventFunc BeginDrag;

	public event EventFunc Drag;

	public event EventFunc Drop;

	public event EventFunc ClickItem;

	public event EventFunc OnGetDrag;

	public event ToolToipFunc ToolTip;

	public void SetItemID(int _ItemID)
	{
		mItemId = _ItemID;
	}

	public void GetDrag(ItemType _targetItemType, int _targetIndex, string _sprName, string strAtlas = "Button")
	{
		if (mCanGetDrag)
		{
			SetCotent(_sprName, strAtlas);
			mTargetItemType = _targetItemType;
			mTargetIndex = _targetIndex;
			if (this.OnGetDrag != null)
			{
				this.OnGetDrag(mItemType, mIndex);
			}
		}
	}

	public void GetDrag(ItemType _targetItemType, int _targetIndex, Texture _contentTexture)
	{
		if (mCanGetDrag)
		{
			SetCotent(_contentTexture);
			mTargetItemType = _targetItemType;
			mTargetIndex = _targetIndex;
			if (this.OnGetDrag != null)
			{
				this.OnGetDrag(mItemType, mIndex);
			}
		}
	}

	public void SetNullContent()
	{
		mTargetIndex = -1;
		mTargetItemType = ItemType.mNull;
		SetCotent("Null", "Button");
	}

	public void SetText(string _text)
	{
		if (mText != null)
		{
			mText.text = _text;
		}
	}

	public void SetNumber(string _number)
	{
		if (mNumber != null)
		{
			mNumber.text = _number;
		}
	}

	public void SetTextIndex(string _index)
	{
		if (mTextIndex != null)
		{
			mTextIndex.text = _index;
		}
	}

	public void SetSpriteIndex(string _index)
	{
		if (mIndexSprite != null)
		{
			mIndexSprite.spriteName = "num_" + _index.ToString();
		}
		if (null != m_QuickBarSprite)
		{
			m_QuickBarSprite.spriteName = "QuickKey_" + _index.ToString();
		}
	}

	public void SetSelect(bool _IsSelect)
	{
		if (_IsSelect)
		{
			mSpriteSelect.gameObject.SetActive(value: true);
			mBgSprite.color = new Color(0f, 1f, 0f, 1f);
		}
		else
		{
			mSpriteSelect.gameObject.SetActive(value: false);
			mBgSprite.color = new Color(255f, 255f, 255f, 255f);
		}
	}

	public void InitItem(ItemType _type, int _index)
	{
		mItemType = _type;
		mIndex = _index;
	}

	public void InitItem(ItemType _type, string _sprName, string strAtlas, int _index)
	{
		mIndex = _index;
		mItemType = _type;
		SetCotent(_sprName, strAtlas);
		if (mDeActiveSprite != null)
		{
			mDeActiveSprite.enabled = !mActive;
		}
	}

	public void InitItem(ItemType _type, Texture _contentTexture, int _index)
	{
		mItemType = _type;
		mContentSprite.gameObject.SetActive(value: false);
		mContentTexture.mainTexture = _contentTexture;
		mIndex = _index;
		if (mDeActiveSprite != null)
		{
			mDeActiveSprite.enabled = !mActive;
		}
		if (null != m_QuickBarSprite)
		{
			m_QuickBarSprite.enabled = null != _contentTexture;
		}
	}

	public void SetCotent(string _sprName, string strAtlas)
	{
		SetAtlas(strAtlas);
		mContentSprite.spriteName = _sprName;
		mContentSprite.gameObject.SetActive(value: true);
		mContentTexture.gameObject.SetActive(value: false);
		if (mDeActiveSprite != null)
		{
			if ("Null" == _sprName)
			{
				mDeActiveSprite.enabled = false;
			}
			else
			{
				mDeActiveSprite.enabled = !mActive;
			}
		}
		if (null != m_QuickBarSprite)
		{
			m_QuickBarSprite.enabled = !_sprName.Equals("Null");
		}
	}

	private void SetCotent(Texture _contentTexture)
	{
		mContentTexture.mainTexture = _contentTexture;
		mContentTexture.gameObject.SetActive(value: true);
		mContentSprite.gameObject.SetActive(value: false);
		if (mDeActiveSprite != null)
		{
			mDeActiveSprite.enabled = !mActive;
		}
	}

	private void SetAtlas(string type)
	{
		mContentSprite.atlas = mAtlasIcon;
	}

	public void PlayGridEffect()
	{
		if (!(gridEffectPrefab == null))
		{
			if (_effectGo != null)
			{
				Object.Destroy(_effectGo);
				effect = null;
			}
			_effectGo = Object.Instantiate(gridEffectPrefab);
			_effectGo.transform.parent = base.transform.parent;
			_effectGo.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -5f);
			_effectGo.transform.localScale = new Vector3(48f, 48f, 1f);
			effect = _effectGo.GetComponentInChildren<UIGridEffect>();
			if (effect != null)
			{
				effect.e_OnEnd += EffectEnd;
			}
		}
	}

	private void EffectEnd(UIEffect _effect)
	{
		effect = null;
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (IsOnDrag && Input.GetMouseButtonUp(0))
		{
			OnDrop();
		}
	}

	private void ItemOnClick()
	{
		if (this.ClickItem != null)
		{
			this.ClickItem(mItemType, mIndex);
		}
	}

	private void OnBeginDrag()
	{
		if (this.BeginDrag != null)
		{
			this.BeginDrag(mItemType, mIndex);
		}
		mSkillCoold.fillAmount = 1f;
		IsOnDrag = true;
	}

	private void OnDrag()
	{
		if (Input.GetMouseButton(0) && mCanDrag)
		{
			if (!IsOnDrag)
			{
				OnBeginDrag();
			}
			if (this.Drag != null)
			{
				this.Drag(mItemType, mIndex);
			}
		}
	}

	private void OnDrop()
	{
		mSkillCoold.fillAmount = 0f;
		if (this.Drop != null)
		{
			this.Drop(mItemType, mIndex);
		}
		IsOnDrag = false;
	}

	private void OnTooltip(bool show)
	{
		if (this.ToolTip != null)
		{
			this.ToolTip(show, mItemType, mIndex);
		}
	}
}
