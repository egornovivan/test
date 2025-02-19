using UnityEngine;

public class UIGraphItemCtrl : MonoBehaviour
{
	public delegate void ClickFunc(int index);

	private const string AtlasType_button = "Button";

	private const string AtlasType_icon = "Icon";

	public UISprite[] mContentSprites;

	public UITexture mContentTexture;

	public UILabel mLbBagCount;

	public UILabel mLbNeedCount;

	public UILabel mLbGetCount;

	public UIAtlas mAtlasButton;

	public UIAtlas mAtlasIcon;

	public GameObject mSelectedSprite;

	public GameObject Content;

	public GameObject child;

	public string atlas = "Icon";

	public UIComWndToolTipCtrl mTipCtrl;

	public int mIndex = -1;

	public event ClickFunc ItemClick;

	public void SetCount(int needCount, int bagCount, int getCount)
	{
		if (needCount == 0)
		{
			if (getCount != 0)
			{
				mLbGetCount.gameObject.transform.localPosition = new Vector3(30f, 0f, -5f);
			}
		}
		else if (getCount != 0)
		{
			mLbNeedCount.gameObject.transform.localPosition = new Vector3(30f, -8f, -5f);
			mLbGetCount.gameObject.transform.localPosition = new Vector3(30f, 8f, -5f);
		}
		else
		{
			mLbNeedCount.gameObject.transform.localPosition = new Vector3(30f, 0f, -5f);
		}
		if (bagCount < needCount)
		{
			mLbNeedCount.color = Color.red;
		}
		else
		{
			mLbNeedCount.color = Color.white;
		}
		SetBagCount(bagCount);
		SetNeedCount(needCount);
		SetGetCount(getCount);
	}

	public void SetIndex(int index)
	{
		mIndex = index;
	}

	public void SetCotent(string[] _sprNames, string strAtlas)
	{
		if (mContentSprites == null || mContentSprites.Length <= 0)
		{
			return;
		}
		if (atlas != strAtlas)
		{
			atlas = strAtlas;
		}
		for (int i = 0; i < _sprNames.Length; i++)
		{
			if (i >= mContentSprites.Length)
			{
				continue;
			}
			if (_sprNames[i] == "0")
			{
				mContentSprites[i].gameObject.SetActive(value: false);
				continue;
			}
			if (atlas == "Icon")
			{
				mContentSprites[i].atlas = mAtlasIcon;
			}
			else
			{
				mContentSprites[i].atlas = mAtlasButton;
			}
			mContentSprites[i].spriteName = _sprNames[i];
			mContentSprites[i].gameObject.SetActive(value: true);
		}
	}

	public void SetCotent(Texture _contentTexture)
	{
		if (mContentTexture == null)
		{
			return;
		}
		mContentTexture.mainTexture = _contentTexture;
		mContentTexture.gameObject.SetActive(value: true);
		if (mContentSprites != null && mContentSprites.Length > 0)
		{
			for (int i = 0; i < mContentSprites.Length; i++)
			{
				mContentSprites[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void SetSelected(bool isSelected)
	{
		mSelectedSprite.SetActive(isSelected);
	}

	private void SetBagCount(int count)
	{
		if (!(mLbBagCount == null))
		{
			mLbBagCount.text = count.ToString();
		}
	}

	private void SetNeedCount(int count)
	{
		if (!(mLbNeedCount == null))
		{
			if (count == 0)
			{
				mLbNeedCount.text = string.Empty;
			}
			else
			{
				mLbNeedCount.text = "- " + count;
			}
		}
	}

	private void SetGetCount(int count)
	{
		if (!(mLbGetCount == null))
		{
			if (count == 0)
			{
				mLbGetCount.text = string.Empty;
			}
			else
			{
				mLbGetCount.text = "+ " + count;
			}
		}
	}

	private void ItemOnClick()
	{
		if (this.ItemClick != null)
		{
			this.ItemClick(mIndex);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
