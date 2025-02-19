using System.Collections.Generic;
using HelpExtension;
using UnityEngine;

public class UISkillGrade : MonoBehaviour
{
	public delegate void DNotify(UISkillItem item);

	public Transform mContent;

	public UISprite mIcon;

	[SerializeField]
	private UISprite mLeftLine_h;

	[SerializeField]
	private UISprite mLeftLine_v;

	[SerializeField]
	private UISprite mRightLine_h;

	[SerializeField]
	private UISprite mRightLine_v;

	public Color enableColor = Color.white;

	public Color disableColor = Color.white;

	[SerializeField]
	private GameObject mSkillItemPrefab;

	private List<UISkillItem> mSkillItems = new List<UISkillItem>(10);

	private List<GameObject> mSkillGos = new List<GameObject>(10);

	public static int c_SkillItemSpace = 30;

	public List<UISkillItem> skillItems => mSkillItems;

	public event DNotify onSkillItemLernBtn;

	public event DNotify onSkillItemClick;

	public void SetContent(int grade, List<SkillTreeUnit> skills)
	{
		mSkillGos.RefreshItem(skills.Count, mSkillItemPrefab, mContent);
		mSkillItems.Clear();
		for (int i = 0; i < mSkillGos.Count; i++)
		{
			UISkillItem component = mSkillGos[i].GetComponent<UISkillItem>();
			int num = i * -c_SkillItemSpace * 2 + (mSkillGos.Count - 1) * c_SkillItemSpace;
			component.gameObject.transform.localPosition = new Vector3(0f, num, 0f);
			component.SetCoord(grade, i);
			component.onClickLernBtn -= OnSkillItemLernBtn;
			component.onClickItemBtn -= OnSkillItemClick;
			component.onClickLernBtn += OnSkillItemLernBtn;
			component.onClickItemBtn += OnSkillItemClick;
			mSkillItems.Add(component);
		}
		SetV_LineSize((mSkillGos.Count - 1) * c_SkillItemSpace * 2);
	}

	public void RefreshData(UISkillType.SkillTypeData data)
	{
		if (data == null || data.data.Count > 3 || data.data.Count == 0)
		{
			return;
		}
		foreach (UISkillItem mSkillItem in mSkillItems)
		{
			if (data.data.ContainsKey(mSkillItem.grade) && mSkillItem.index < data.data[mSkillItem.grade].Count)
			{
				mSkillItem.data = data.data[mSkillItem.grade][mSkillItem.index];
			}
		}
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
		if (this.onSkillItemClick != null)
		{
			this.onSkillItemClick(item);
		}
	}

	public void SetV_LineSize(float size)
	{
		Vector3 localScale = mLeftLine_v.transform.localScale;
		localScale.y = size;
		mLeftLine_v.transform.localScale = localScale;
		localScale = mRightLine_v.transform.localScale;
		localScale.y = size;
		mRightLine_v.transform.localScale = localScale;
	}

	private void Update()
	{
		bool flag = true;
		foreach (UISkillItem mSkillItem in mSkillItems)
		{
			if (mSkillItem.data._state != 0)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			mIcon.color = enableColor;
		}
		else
		{
			mIcon.color = disableColor;
		}
	}
}
