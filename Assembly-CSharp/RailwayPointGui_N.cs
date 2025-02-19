using System;
using System.Collections.Generic;
using UnityEngine;

public class RailwayPointGui_N : UIBaseWnd
{
	public UILabel mNameLable;

	public UIInput mNameInput;

	public UIPopupList mPreMenu;

	public UIPopupList mNextMenu;

	public N_ImageButton mRecycleBtn;

	public N_ImageButton mCloseBtn;

	public string PointName
	{
		get
		{
			return mNameInput.text;
		}
		set
		{
			UILabel uILabel = mNameLable;
			mNameInput.text = value;
			uILabel.text = value;
			if (mPreMenu.isOpen)
			{
				UnityEngine.Object.Destroy(mPreMenu.ChildPopupMenu);
			}
			if (mNextMenu.isOpen)
			{
				UnityEngine.Object.Destroy(mNextMenu.ChildPopupMenu);
			}
		}
	}

	public event OnGuiBtnClicked RunEvent;

	public event OnGuiBtnClicked RecycleEvent;

	public event OnMenuSelectionChange e_PreMenuChange;

	public event OnMenuSelectionChange e_NextPointChange;

	private void OnRunBtn()
	{
		if (this.RunEvent != null)
		{
			this.RunEvent();
		}
		Hide();
	}

	private void OnRecycleBtn()
	{
		if (this.RecycleEvent != null)
		{
			this.RecycleEvent();
		}
		Hide();
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	public void SetPrePoint(List<string> menuList, int index)
	{
		mPreMenu.items.Clear();
		foreach (string menu in menuList)
		{
			mPreMenu.items.Add(menu);
		}
		if (index >= 0 && index < menuList.Count)
		{
			mPreMenu.selection = menuList[index];
		}
		else
		{
			mPreMenu.selection = string.Empty;
		}
	}

	public void SetNextPoint(List<string> menuList, int index)
	{
		mNextMenu.items.Clear();
		foreach (string menu in menuList)
		{
			mNextMenu.items.Add(menu);
		}
		if (index >= 0 && index < menuList.Count)
		{
			mNextMenu.selection = menuList[index];
		}
		else
		{
			mNextMenu.selection = string.Empty;
		}
	}

	private void OnPreMenuChange(string str)
	{
		int index = mPreMenu.items.FindIndex((string itr) => itr == str);
		if (this.e_PreMenuChange != null)
		{
			this.e_PreMenuChange(index, str);
		}
	}

	private void OnNextMenuChange(string str)
	{
		int index = mPreMenu.items.FindIndex((string itr) => itr == str);
		if (this.e_NextPointChange != null)
		{
			this.e_NextPointChange(index, str);
		}
	}

	private void Start()
	{
		UIPopupList uIPopupList = mPreMenu;
		uIPopupList.onSelectionChange = (UIPopupList.OnSelectionChange)Delegate.Combine(uIPopupList.onSelectionChange, new UIPopupList.OnSelectionChange(OnPreMenuChange));
		UIPopupList uIPopupList2 = mNextMenu;
		uIPopupList2.onSelectionChange = (UIPopupList.OnSelectionChange)Delegate.Combine(uIPopupList2.onSelectionChange, new UIPopupList.OnSelectionChange(OnNextMenuChange));
	}
}
