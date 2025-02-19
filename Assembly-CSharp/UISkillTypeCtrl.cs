using System.Collections.Generic;
using HelpExtension;
using UnityEngine;

public class UISkillTypeCtrl : MonoBehaviour
{
	public delegate void DSkillBtnNotify(UISkillTypeBtn btn);

	[SerializeField]
	private UIGrid btnGrid;

	[SerializeField]
	private GameObject btnPrefab;

	[SerializeField]
	private GameObject bgTracer;

	[SerializeField]
	private GameObject bg;

	[SerializeField]
	private UILabel infoContent;

	[SerializeField]
	private UIPanel panel;

	private List<GameObject> mSkillBtnGos = new List<GameObject>();

	private List<UISkillTypeBtn> mSkillBtnItems = new List<UISkillTypeBtn>();

	public string desc
	{
		get
		{
			return infoContent.text;
		}
		set
		{
			infoContent.text = value;
		}
	}

	public event DSkillBtnNotify onSetBtnActive;

	public void SetContent(int count, DSkillBtnNotify setContent)
	{
		mSkillBtnGos.RefreshItem(count, btnPrefab, btnGrid.transform);
		mSkillBtnItems.Clear();
		if (setContent != null)
		{
			for (int i = 0; i < count; i++)
			{
				UISkillTypeBtn component = mSkillBtnGos[i].GetComponent<UISkillTypeBtn>();
				mSkillBtnItems.Add(component);
				component.index = i;
				component.onBtnClick -= OnSkillBtnClick;
				component.onBtnClick += OnSkillBtnClick;
				setContent?.Invoke(component);
			}
		}
		btnGrid.repositionNow = true;
		float num = (float)mSkillBtnGos.Count * btnGrid.cellWidth + btnGrid.transform.position.x;
		float num2 = num + 20f;
		float num3 = panel.clipRange.z - num2;
		Vector3 localPosition = bgTracer.transform.localPosition;
		Vector3 localScale = bgTracer.transform.localScale;
		localPosition.x = num2;
		localScale.x = num3;
		bgTracer.transform.localPosition = localPosition;
		bgTracer.transform.localScale = localScale;
		bg.transform.position = bgTracer.transform.position;
		bg.transform.localScale = bgTracer.transform.localScale;
		infoContent.lineWidth = (int)(num3 - 31f);
	}

	public void SetActiveBtn(int index)
	{
		if (index >= mSkillBtnItems.Count || index < 0)
		{
			Debug.LogError("The giving index is error");
			return;
		}
		mSkillBtnItems[index].SetEnable(enable: true);
		for (int i = 0; i < mSkillBtnItems.Count; i++)
		{
			if (i != index)
			{
				mSkillBtnItems[i].SetEnable(enable: false);
			}
		}
		if (this.onSetBtnActive != null)
		{
			this.onSetBtnActive(mSkillBtnItems[index]);
		}
	}

	private void OnSkillBtnClick(UISkillTypeBtn btn)
	{
		SetActiveBtn(btn.index);
	}
}
