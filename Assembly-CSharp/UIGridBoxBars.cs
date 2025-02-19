using System.Collections.Generic;
using UnityEngine;

public class UIGridBoxBars : MonoBehaviour
{
	public delegate void PageIndexChange(int _PageIndex);

	public int mMaxPageCount = 3;

	[SerializeField]
	private UIGrid mGrid;

	[SerializeField]
	private UIGrid mGrid_2;

	[SerializeField]
	private UILabel mLbPageText;

	private List<GameObject> mItems = new List<GameObject>();

	public List<GameObject> Items => mItems;

	public int PageIndex { get; private set; }

	public int ItemCount { get; private set; }

	public event PageIndexChange e_PageIndexChange;

	public void Init(GameObject itemPrefab, int itemCount)
	{
		ItemCount = itemCount;
		for (int i = 0; i < itemCount; i++)
		{
			GameObject gameObject = Object.Instantiate(itemPrefab);
			if (mGrid_2 != null && i >= itemCount / 2)
			{
				gameObject.transform.parent = mGrid_2.gameObject.transform;
			}
			else
			{
				gameObject.transform.parent = mGrid.gameObject.transform;
			}
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: true);
			gameObject.layer = 30;
			mItems.Add(gameObject);
		}
		PageIndex = 1;
	}

	public void Reposition()
	{
		mGrid.repositionNow = true;
		if (mGrid_2 != null)
		{
			mGrid_2.repositionNow = true;
		}
	}

	public void BtnPageUpOnClick()
	{
		PageIndex = ((PageIndex <= 1) ? mMaxPageCount : (PageIndex - 1));
		mLbPageText.text = PageIndex.ToString();
		if (this.e_PageIndexChange != null)
		{
			this.e_PageIndexChange(PageIndex);
		}
	}

	public void BtnPageDnOnClick()
	{
		PageIndex = ((PageIndex >= mMaxPageCount) ? 1 : (PageIndex + 1));
		mLbPageText.text = PageIndex.ToString();
		if (this.e_PageIndexChange != null)
		{
			this.e_PageIndexChange(PageIndex);
		}
	}
}
