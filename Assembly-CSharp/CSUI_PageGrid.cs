using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_PageGrid : MonoBehaviour
{
	public delegate void ChickItem(object sender);

	public UISprite[] mContentSprite;

	[SerializeField]
	private UILabel mMaxNumLb;

	[SerializeField]
	private GameObject mGridSeclect;

	private UICheckbox m_CheckBox;

	private GridInfo m_GridInfo;

	private int m_ProtoId;

	private ItemSample mItemSample;

	private ListItemType mType;

	private int mMaxNum;

	private ItemObject _itemObj;

	public GridInfo mGridInfo
	{
		get
		{
			return m_GridInfo;
		}
		set
		{
			m_GridInfo = value;
			InitGird();
		}
	}

	public int ProtoId
	{
		get
		{
			return m_ProtoId;
		}
		set
		{
			m_ProtoId = value;
		}
	}

	public ListItemType Type
	{
		get
		{
			return mType;
		}
		set
		{
			mType = value;
		}
	}

	public int MaxNum
	{
		get
		{
			return mMaxNum;
		}
		set
		{
			mMaxNum = value;
			mMaxNumLb.text = mMaxNum.ToString();
		}
	}

	public event ChickItem e_ItemClick;

	private void Awake()
	{
		InitWnd();
	}

	private void Start()
	{
	}

	private void InitWnd()
	{
		m_CheckBox = GetComponent<UICheckbox>();
		if (null != m_CheckBox)
		{
			m_CheckBox.radioButtonRoot = base.transform.parent;
		}
	}

	private void InitGird()
	{
		if (m_GridInfo != null)
		{
			setIcon(m_GridInfo.IconName);
			SetMaxNum(m_GridInfo.MaxNum);
			m_ProtoId = m_GridInfo.mProtoId;
		}
	}

	private void SetMaxNum(int max)
	{
		mMaxNum = max;
		mMaxNumLb.text = mMaxNum.ToString();
	}

	public void ShowGridSeclect(bool show)
	{
		if (null == m_CheckBox)
		{
			mGridSeclect.gameObject.SetActive(show);
		}
		else
		{
			m_CheckBox.isChecked = show;
		}
	}

	public void setIcon(string[] icon0)
	{
		if (mContentSprite.Length != 0)
		{
			mContentSprite[1].spriteName = icon0[0];
			mContentSprite[1].MakePixelPerfect();
		}
	}

	public void SetCotent(string[] ico)
	{
		if (mContentSprite.Length == 0)
		{
			return;
		}
		for (int i = 0; i < ico.Length; i++)
		{
			if (mContentSprite[i] != null)
			{
				if (ico[i] == "0")
				{
					mContentSprite[i].gameObject.SetActive(value: false);
					continue;
				}
				mContentSprite[i].spriteName = ico[i];
				mContentSprite[i].gameObject.SetActive(value: true);
			}
		}
	}

	private void Update()
	{
	}

	private void OnClickItem()
	{
		if (this.e_ItemClick != null)
		{
			this.e_ItemClick(this);
		}
	}

	private void OnActivate(bool active)
	{
		mGridSeclect.gameObject.SetActive(active);
	}

	private void OnTooltip(bool show)
	{
		if (mType == ListItemType.mItem)
		{
			if (show && mItemSample == null && m_ProtoId != 0)
			{
				mItemSample = new ItemSample(m_ProtoId);
			}
			else if (!show)
			{
				mItemSample = null;
			}
			if (mItemSample != null)
			{
				_itemObj = PeSingleton<ItemMgr>.Instance.CreateItem(m_ProtoId);
				string tooltip = _itemObj.GetTooltip();
				ToolTipsMgr.ShowText(tooltip);
			}
			else
			{
				PeSingleton<ItemMgr>.Instance.DestroyItem(_itemObj);
				ToolTipsMgr.ShowText(null);
			}
		}
	}
}
