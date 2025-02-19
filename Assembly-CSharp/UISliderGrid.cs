using System;
using System.Collections.Generic;
using UnityEngine;

public class UISliderGrid : MonoBehaviour
{
	[SerializeField]
	private UIGrid mGrid;

	[SerializeField]
	private UISprite mGridF_Prefab;

	[SerializeField]
	private Color mGridF_Color;

	[SerializeField]
	private int mItemCount = 12;

	[SerializeField]
	private float mValue;

	private List<UISprite> mItems = new List<UISprite>();

	public float value
	{
		get
		{
			return mValue;
		}
		set
		{
			mValue = value;
		}
	}

	public List<UISprite> Items => mItems;

	private void Awake()
	{
		mItems.Clear();
		for (int i = 0; i < mItemCount; i++)
		{
			UISprite uISprite = UnityEngine.Object.Instantiate(mGridF_Prefab);
			uISprite.transform.parent = mGrid.transform;
			uISprite.transform.localPosition = new Vector3(i, 0f, 0f);
			uISprite.transform.localRotation = Quaternion.identity;
			uISprite.MakePixelPerfect();
			Color color = new Color(mGridF_Color.r, Convert.ToSingle(i) / (float)mItemCount + 0.3f, mGridF_Color.b);
			uISprite.color = color;
			uISprite.gameObject.SetActive(value: true);
			uISprite.enabled = false;
			uISprite.gameObject.transform.localPosition = new Vector3(mGrid.cellWidth * (float)i, 0f, 0f);
			mItems.Add(uISprite);
		}
	}

	public void Repostion()
	{
		mGrid.repositionNow = true;
	}

	private void Update()
	{
		for (int i = 0; i < mItems.Count; i++)
		{
			mItems[i].enabled = ((mValue > Convert.ToSingle(i) / (float)mItemCount) ? true : false);
		}
	}
}
