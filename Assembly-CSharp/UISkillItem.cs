using UnityEngine;

public class UISkillItem : MonoBehaviour
{
	public delegate void DNotify(UISkillItem item);

	[SerializeField]
	private UISprite mSprContent;

	[SerializeField]
	private UISprite mSprLock;

	[SerializeField]
	private UILabel mLbLevel;

	[SerializeField]
	private UIButton mBtnAddSkill;

	[SerializeField]
	private GameObject mLeftLine;

	[SerializeField]
	private UISlicedSprite mSelectSprite;

	public Color enableColor = Color.white;

	public Color disableColor = Color.white;

	private SkillTreeUnit mData;

	private bool isLock;

	public SkillTreeUnit data
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

	public int grade { get; private set; }

	public int index { get; private set; }

	public event DNotify onClickLernBtn;

	public event DNotify onClickItemBtn;

	public void SetCoord(int _grade, int _index)
	{
		grade = _grade;
		index = _index;
		mLeftLine.SetActive(_grade != 1);
	}

	public void SetSelect(bool select)
	{
		mSelectSprite.enabled = select;
	}

	public void Refresh()
	{
		if (mData == null)
		{
			return;
		}
		isLock = data._state == SkillState.Lock;
		mSprLock.enabled = isLock;
		mSprLock.enabled = false;
		mSprContent.spriteName = mData._sprName;
		if (isLock)
		{
			SetColor(disableColor);
		}
		else
		{
			SetColor(enableColor);
		}
		if (data._state == SkillState.learnt)
		{
			if (data._level < data._maxLevel)
			{
				mLbLevel.text = data._level + "/" + data._maxLevel;
				mBtnAddSkill.gameObject.SetActive(value: true);
			}
			else
			{
				mLbLevel.text = " Max" + data._maxLevel;
				mBtnAddSkill.gameObject.SetActive(value: false);
			}
		}
		else
		{
			mLbLevel.text = "-/" + data._maxLevel;
			if (isLock)
			{
				mBtnAddSkill.gameObject.SetActive(value: false);
			}
			else
			{
				mBtnAddSkill.gameObject.SetActive(value: true);
			}
		}
	}

	public void SetColor(Color color)
	{
		mSprContent.color = color;
		mLbLevel.color = color;
	}

	private void OnBtnLern()
	{
		if (this.onClickLernBtn != null)
		{
			this.onClickLernBtn(this);
		}
	}

	private void OnClick()
	{
		if (this.onClickItemBtn != null)
		{
			this.onClickItemBtn(this);
		}
	}

	private void OnTooltip(bool show)
	{
		if (mData != null)
		{
			string content = mData._desc + "\r\n" + PELocalization.GetString(8000160) + "[5CB0FF]" + GameUI.Instance.mSkillWndCtrl._SkillMgr.GetNextExpBySkillType(mData._skillType) + "[-]";
			ToolTipsMgr.ShowText(content);
		}
	}
}
