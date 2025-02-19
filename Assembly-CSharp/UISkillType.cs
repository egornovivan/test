using System.Collections.Generic;
using UnityEngine;

public class UISkillType : MonoBehaviour
{
	public class SkillTypeData
	{
		public Dictionary<int, List<SkillTreeUnit>> data { get; set; }

		public SkillMainType info { get; set; }

		public SkillTypeData(Dictionary<int, List<SkillTreeUnit>> _data, SkillMainType _info)
		{
			data = _data;
			info = _info;
		}
	}

	public delegate void DNotify(UISkillItem item);

	private SkillTypeData mData;

	[SerializeField]
	private GameObject mSkillItemPrefab;

	[SerializeField]
	private UISkillGrade mGrade_1;

	[SerializeField]
	private UISkillGrade mGrade_2;

	[SerializeField]
	private UISkillGrade mGrade_3;

	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private UISprite mSprInfo;

	[SerializeField]
	private UISprite mSprLine;

	private UISkillItem mSelectItem;

	private bool bCreate;

	public SkillTypeData data
	{
		get
		{
			return mData;
		}
		set
		{
			mData = value;
			Refresh();
		}
	}

	public int mainType { get; set; }

	public UISkillItem selectItem
	{
		get
		{
			return mSelectItem;
		}
		set
		{
			if (value != mSelectItem)
			{
				if (mSelectItem != null)
				{
					mSelectItem.SetSelect(select: false);
				}
				mSelectItem = value;
				if (mSelectItem != null)
				{
					mSelectItem.SetSelect(select: true);
				}
				if (this.onSelectItemChanged != null)
				{
					this.onSelectItemChanged(mSelectItem);
				}
			}
		}
	}

	public float height
	{
		get
		{
			if (mData == null)
			{
				return 362f;
			}
			int num = 0;
			foreach (List<SkillTreeUnit> value in mData.data.Values)
			{
				if (num < value.Count)
				{
					num = value.Count;
				}
			}
			return Mathf.Max(num * UISkillGrade.c_SkillItemSpace * 2, 362f);
		}
	}

	public event DNotify onSkillItemLernBtn;

	public event DNotify onSelectItemChanged;

	private void Awake()
	{
		mGrade_1.onSkillItemClick += OnSkillItemClick;
		mGrade_1.onSkillItemLernBtn += OnSkillItemLernBtn;
		mGrade_2.onSkillItemClick += OnSkillItemClick;
		mGrade_2.onSkillItemLernBtn += OnSkillItemLernBtn;
		mGrade_3.onSkillItemClick += OnSkillItemClick;
		mGrade_3.onSkillItemLernBtn += OnSkillItemLernBtn;
	}

	private void Refresh()
	{
		if (mData != null)
		{
			Create();
			RefreshSkillType();
			RefreshData();
			mSprLine.transform.localPosition = new Vector3(0f, (0f - height) / 2f, 0f);
		}
	}

	public void RefreshData()
	{
		mGrade_1.RefreshData(mData);
		mGrade_2.RefreshData(mData);
		mGrade_3.RefreshData(mData);
	}

	private void RefreshSkillType()
	{
		if (mData.info != null)
		{
			mLbName.text = mData.info._desc;
			int count = mData.info._icon.Count;
			if (count > 0)
			{
				mSprInfo.spriteName = mData.info._icon[0];
			}
			else
			{
				mSprInfo.enabled = false;
			}
			if (count > 1)
			{
				mGrade_1.mIcon.spriteName = mData.info._icon[1];
			}
			else
			{
				mGrade_1.mIcon.enabled = false;
			}
			if (count > 2)
			{
				mGrade_2.mIcon.spriteName = mData.info._icon[2];
			}
			else
			{
				mGrade_2.mIcon.enabled = false;
			}
			if (count > 3)
			{
				mGrade_3.mIcon.spriteName = mData.info._icon[3];
			}
			else
			{
				mGrade_3.mIcon.enabled = false;
			}
			mSprInfo.MakePixelPerfect();
			mGrade_1.mIcon.MakePixelPerfect();
			mGrade_2.mIcon.MakePixelPerfect();
			mGrade_3.mIcon.MakePixelPerfect();
		}
	}

	private bool Create()
	{
		if (mData.data == null)
		{
			return false;
		}
		if (mData.data.ContainsKey(1))
		{
			mGrade_1.SetContent(1, mData.data[1]);
			if (mData.data.ContainsKey(2))
			{
				mGrade_2.SetContent(2, mData.data[2]);
				if (mData.data.ContainsKey(3))
				{
					mGrade_3.SetContent(3, mData.data[3]);
					return true;
				}
				mGrade_3.transform.gameObject.SetActive(value: false);
				return true;
			}
			mGrade_2.transform.gameObject.SetActive(value: false);
			mGrade_3.transform.gameObject.SetActive(value: false);
			return true;
		}
		mGrade_1.transform.gameObject.SetActive(value: false);
		mGrade_2.transform.gameObject.SetActive(value: false);
		mGrade_3.transform.gameObject.SetActive(value: false);
		return true;
	}

	private void OnSkillItemLernBtn(UISkillItem item)
	{
		if (this.onSkillItemLernBtn != null)
		{
			this.onSkillItemLernBtn(item);
		}
	}

	private void OnSkillItemClick(UISkillItem item)
	{
		selectItem = item;
	}
}
