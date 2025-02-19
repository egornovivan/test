using System.Collections.Generic;
using UnityEngine;

public class SimpleMenu_N : MonoBehaviour
{
	public MenuSelection_N mCurSelection;

	private List<MenuSelection_N> mSelections;

	public UITweener mExpansionTrigger;

	public UISlicedSprite mExpansionBg;

	public UIScrollBar mScrollBar;

	public UIPanel mSelPanel;

	public int mSelNumPerPage = 10;

	public float mUpdateStep = 0.2f;

	private bool mExpansion;

	private static SimpleMenu_N mExpansionMenu;

	public string CurSelection
	{
		get
		{
			return mCurSelection.mTextLabel.text;
		}
		set
		{
			mCurSelection.mTextLabel.text = value;
		}
	}

	public event OnMenuSelectionChange SelectedEvent;

	private void Awake()
	{
		mExpansion = false;
		mSelections = new List<MenuSelection_N>();
		mExpansionTrigger.transform.localPosition = (mCurSelection.Size.x + mExpansionBg.sprite.paddingLeft) * Vector3.right;
		mExpansionBg.transform.localPosition = mExpansionBg.sprite.paddingTop * Vector3.up;
	}

	public void PlayExpansion(bool forward)
	{
		if (forward)
		{
			mScrollBar.scrollValue = 0f;
			mExpansionTrigger.transform.localScale = Vector3.one;
			mSelPanel.enabled = false;
			mUpdateStep = 0.1f;
		}
		else
		{
			mScrollBar.scrollValue = 0f;
			mExpansionTrigger.transform.localScale = Vector3.zero;
		}
	}

	public void SetSelections(List<string> selections)
	{
		foreach (MenuSelection_N mSelection in mSelections)
		{
			Object.Destroy(mSelection.gameObject);
		}
		mSelections.Clear();
		mCurSelection.ExpansionEnable = selections.Count > 1;
		if (selections.Count > 0)
		{
			mCurSelection.SetInfo(selections[0], this, -1);
		}
		else
		{
			mCurSelection.SetInfo(string.Empty, this, -1);
		}
		if (selections.Count > 1)
		{
			if (selections.Count > mSelNumPerPage)
			{
				mScrollBar.gameObject.SetActive(value: true);
				mExpansionBg.transform.localScale = new Vector3(mExpansionBg.sprite.paddingLeft + mExpansionBg.sprite.paddingRight + mCurSelection.Size.x, mExpansionBg.sprite.paddingTop + mExpansionBg.sprite.paddingBottom + 10f * mCurSelection.Size.y, 1f);
				mExpansionBg.transform.localScale += 10f * Vector3.right;
				mScrollBar.transform.localPosition = new Vector3(mExpansionBg.transform.localScale.x - 5f, -5f, 0f);
				mScrollBar.background.transform.localScale = new Vector3(4f, mExpansionBg.transform.localScale.y - 10f, 0f);
			}
			else
			{
				mScrollBar.gameObject.SetActive(value: false);
				mExpansionBg.transform.localScale = new Vector3(mExpansionBg.sprite.paddingLeft + mExpansionBg.sprite.paddingRight + mCurSelection.Size.x, mExpansionBg.sprite.paddingTop + mExpansionBg.sprite.paddingBottom + (float)selections.Count * mCurSelection.Size.y, 1f);
			}
			mSelPanel.clipRange = new Vector4(mExpansionBg.transform.localScale.x / 2f, (0f - mExpansionBg.transform.localScale.y) / 2f, mExpansionBg.transform.localScale.x, mExpansionBg.transform.localScale.y);
			for (int i = 0; i < selections.Count; i++)
			{
				MenuSelection_N menuSelection_N = Object.Instantiate(mCurSelection);
				menuSelection_N.transform.parent = mSelPanel.transform;
				menuSelection_N.transform.localPosition = (float)i * mCurSelection.Size.y * Vector3.down;
				menuSelection_N.transform.localScale = Vector3.one;
				menuSelection_N.SetInfo(selections[i], this, i);
				menuSelection_N.ShowExpansion = false;
				mSelections.Add(menuSelection_N);
			}
			mScrollBar.scrollValue = 0f;
		}
	}

	private void Update()
	{
		if (!mSelPanel.enabled)
		{
			mUpdateStep -= Time.deltaTime;
			if (mUpdateStep < 0f)
			{
				mSelPanel.enabled = true;
			}
		}
	}

	public void OnSelectionChange(int index, string text)
	{
		if (null != mExpansionMenu && mExpansionMenu != this)
		{
			mExpansionMenu.PlayExpansion(forward: false);
		}
		if (index == -1)
		{
			if (this == mExpansionMenu)
			{
				PlayExpansion(forward: false);
				mExpansionMenu = null;
			}
			else
			{
				PlayExpansion(forward: true);
				mExpansionMenu = this;
			}
		}
		else
		{
			PlayExpansion(forward: false);
			mExpansionMenu = null;
			if (this.SelectedEvent != null)
			{
				this.SelectedEvent(index, text);
			}
		}
	}
}
