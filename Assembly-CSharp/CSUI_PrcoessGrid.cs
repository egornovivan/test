using ItemAsset;
using UnityEngine;

public class CSUI_PrcoessGrid : MonoBehaviour
{
	public delegate void OnDelete(object sender, int ItemId, int ProtoID);

	public delegate void OnSelect(object sender);

	[SerializeField]
	private GameObject mDeleteBtn;

	[SerializeField]
	private GameObject mGridSeclect;

	[SerializeField]
	private UISprite mIcon;

	[SerializeField]
	private UILabel mNeedNumLb;

	private bool bSelect;

	private int m_itemID;

	private int m_ProtoId;

	private ItemSample mItemSample;

	private ListItemType mType;

	private ProcessInfo m_ProcessInfo;

	private int mNeedNum;

	public int ItemID
	{
		get
		{
			return m_itemID;
		}
		set
		{
			m_itemID = value;
		}
	}

	public int ProtoID
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

	public ProcessInfo mProcessInfo
	{
		get
		{
			return m_ProcessInfo;
		}
		set
		{
			m_ProcessInfo = value;
			InitProcess();
		}
	}

	public int NeedNum
	{
		get
		{
			return mNeedNum;
		}
		set
		{
			mNeedNum = value;
			if (mNeedNum > 0)
			{
				mNeedNumLb.gameObject.SetActive(value: true);
				mNeedNumLb.text = mNeedNum.ToString();
			}
			else
			{
				mNeedNumLb.gameObject.SetActive(value: false);
			}
		}
	}

	public event OnDelete e_OnDeleteClick;

	public event OnSelect e_OnSelectClick;

	private void Awake()
	{
		InitWnd();
	}

	private void Start()
	{
	}

	private void InitProcess()
	{
		SetIcon(m_ProcessInfo.IconName);
		ItemID = m_ProcessInfo.ItemId;
		NeedNum = m_ProcessInfo.m_NeedNum;
	}

	private void InitWnd()
	{
		GetComponent<UICheckbox>().radioButtonRoot = base.transform.parent;
	}

	public void UpdateGridInfo(ProcessInfo info)
	{
		SetIcon(info.IconName);
		ItemID = info.ItemId;
		NeedNum = info.m_NeedNum;
		ProtoID = info.ProtoId;
	}

	public void ClearInfo()
	{
		SetIcon("Null");
		ItemID = -1;
		NeedNum = 0;
		ProtoID = 0;
	}

	public void SetIcon(string SpritName)
	{
		if (SpritName == string.Empty)
		{
			mIcon.spriteName = "Null";
			return;
		}
		mIcon.spriteName = SpritName;
		mIcon.MakePixelPerfect();
	}

	public void SetGridBox(bool active)
	{
		GetComponent<Collider>().enabled = active;
		GetComponent<UICheckbox>().isChecked = false;
	}

	private void OnDeleteBtn()
	{
		if (this.e_OnDeleteClick != null && ProtoID != 0)
		{
			this.e_OnDeleteClick(this, ItemID, ProtoID);
			GetComponent<UICheckbox>().isChecked = false;
		}
	}

	private void OnSelectBtn()
	{
		bSelect = !bSelect;
		if (this.e_OnSelectClick != null)
		{
			this.e_OnSelectClick(this);
		}
	}

	private void OnActivate(bool active)
	{
		mGridSeclect.gameObject.SetActive(active && ProtoID != 0);
		mDeleteBtn.SetActive(active && ProtoID != 0);
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
				string @string = PELocalization.GetString(mItemSample.protoData.descriptionStringId);
				ToolTipsMgr.ShowText(@string);
			}
			else
			{
				ToolTipsMgr.ShowText(null);
			}
		}
	}

	private void Update()
	{
	}
}
