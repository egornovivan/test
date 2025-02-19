using System.Collections.Generic;
using UnityEngine;

public class UIMissionNode : MonoBehaviour
{
	public delegate void BaseMsgEvent(object sender);

	public delegate void CheckMsgEvent(object sender, bool isChecked);

	public UICheckbox mCheckBoxTag;

	public UIButton mBtnDelete;

	public UIButton mBtnDetail;

	public UILabel mLbTitle;

	public UISprite mSpSelected;

	public UISprite mSpState;

	public TweenScale mTeewn;

	public UITable mTable;

	[SerializeField]
	private UIButtonTween mBtnTween;

	[HideInInspector]
	public bool mCanSelected;

	[HideInInspector]
	public List<UIMissionNode> mChilds = new List<UIMissionNode>();

	[HideInInspector]
	public UITable mTablePartent;

	[HideInInspector]
	public object mData;

	private UIMissionNode m_Parent;

	public bool enableCkTag
	{
		get
		{
			if (mCheckBoxTag == null)
			{
				return false;
			}
			return mCheckBoxTag.gameObject.activeSelf;
		}
		set
		{
			if (!(mCheckBoxTag == null))
			{
				mCheckBoxTag.gameObject.SetActive(value);
			}
		}
	}

	public bool enableBtnDelete
	{
		get
		{
			if (mBtnDelete == null)
			{
				return false;
			}
			return mBtnDelete.gameObject.activeSelf;
		}
		set
		{
			if (!(mBtnDelete == null))
			{
				mBtnDelete.gameObject.SetActive(value);
			}
		}
	}

	public bool Selected
	{
		get
		{
			return mSpSelected.enabled;
		}
		set
		{
			if (!mCanSelected)
			{
				mSpSelected.enabled = false;
			}
			else
			{
				mSpSelected.enabled = value;
			}
		}
	}

	[HideInInspector]
	public UIMissionNode mParent
	{
		get
		{
			return m_Parent;
		}
		set
		{
			if (null != mBtnDetail)
			{
				mBtnDetail.gameObject.SetActive(value == null);
			}
			m_Parent = value;
		}
	}

	public event BaseMsgEvent e_OnClick;

	public event BaseMsgEvent e_BtnDelete;

	public event CheckMsgEvent e_CheckedTg;

	private void Update()
	{
		mSpState.enabled = mChilds.Count > 0;
		mSpState.spriteName = ((!mTeewn.gameObject.activeSelf) ? "mission_closed" : "mission_open");
	}

	private void ItemOnClick()
	{
		if (mCanSelected && this.e_OnClick != null)
		{
			this.e_OnClick(this);
		}
	}

	private void ItemBtnDeleteOnlick()
	{
		if (this.e_BtnDelete != null)
		{
			this.e_BtnDelete(this);
		}
	}

	private void ItemBtnDetailOnClick()
	{
		if (!GameUI.Instance.mUIMissionWndCtrl.isShow)
		{
			GameUI.Instance.mUIMissionWndCtrl.Show();
		}
		GameUI.Instance.mUIMissionWndCtrl.SelectMissionNodeByData(mData);
	}

	private void ItemCheckedTag(bool isChecked)
	{
		if (this.e_CheckedTg != null)
		{
			this.e_CheckedTg(this, isChecked);
		}
	}

	public void ChangeExpand()
	{
		Invoke("DoChangeExpand", 0.2f);
	}

	private void DoChangeExpand()
	{
		mBtnTween.Play(forward: true);
	}
}
