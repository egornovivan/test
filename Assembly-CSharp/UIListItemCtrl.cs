using System.Collections.Generic;
using UnityEngine;

public class UIListItemCtrl : MonoBehaviour
{
	public delegate void OnCheckItem(int index);

	public GameObject mLabelPrefab;

	public GameObject mContent;

	public BoxCollider mCkboxCollider;

	public UISprite mCkSelectedBg;

	public UISprite mItemIco;

	public int mIndex;

	public List<string> mTextList = new List<string>();

	public List<GameObject> mLabelList = new List<GameObject>();

	private int[] mItemWith;

	private bool IsSelected;

	public event OnCheckItem ListItemChecked;

	public event OnCheckItem listItemDoubleClick;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetIconActive(bool isActive)
	{
		if (!(mItemIco == null))
		{
			mItemIco.enabled = isActive;
		}
	}

	public void SetColor(Color _color)
	{
		for (int i = 0; i < mLabelList.Count; i++)
		{
			if (i != 6 || !(GetComponent<UIUpdatePIngTextColor>() != null))
			{
				UILabel component = mLabelList[i].GetComponent<UILabel>();
				component.color = _color;
			}
		}
	}

	public void DeleteItem()
	{
		base.gameObject.transform.parent = null;
		Object.Destroy(base.gameObject);
		mLabelList.Clear();
		mTextList.Clear();
	}

	public void SetActive(bool isActive)
	{
		mCkboxCollider.enabled = isActive;
	}

	public void SetSelected(bool _isSelected)
	{
		IsSelected = _isSelected;
		if (mCkSelectedBg.enabled != _isSelected)
		{
			mCkSelectedBg.enabled = _isSelected;
		}
	}

	public void ClearItemText()
	{
		mTextList.Clear();
		for (int i = 0; i < mLabelList.Count; i++)
		{
			UILabel component = mLabelList[i].GetComponent<UILabel>();
			component.text = string.Empty;
		}
	}

	public void SetItemText(List<string> strText)
	{
		if (mItemWith.Length >= strText.Count)
		{
			mTextList.Clear();
			for (int i = 0; i < strText.Count; i++)
			{
				UILabel component = mLabelList[i].GetComponent<UILabel>();
				component.text = strText[i];
				component.lineWidth = mItemWith[i] - 12;
				mTextList.Add(strText[i]);
			}
		}
	}

	public void InitItem(int[] itemWidth)
	{
		int num = 0;
		for (int i = 0; i < itemWidth.Length; i++)
		{
			GameObject gameObject = CreateLabel();
			gameObject.transform.localPosition = new Vector3(num, 0f, -2f);
			gameObject.transform.localScale = new Vector3(20f, 20f, 1f);
			num += itemWidth[i];
			mLabelList.Add(gameObject);
		}
		mItemWith = itemWidth;
	}

	private GameObject CreateLabel()
	{
		GameObject gameObject = Object.Instantiate(mLabelPrefab);
		gameObject.transform.parent = mContent.transform;
		return gameObject;
	}

	public void OnChecked()
	{
		if (Input.GetMouseButtonUp(0) && !IsSelected && this.ListItemChecked != null)
		{
			this.ListItemChecked(mIndex);
		}
	}

	public void ItemOnDoubleClick()
	{
		if (this.listItemDoubleClick != null && Input.GetMouseButtonUp(0))
		{
			this.listItemDoubleClick(mIndex);
		}
	}
}
