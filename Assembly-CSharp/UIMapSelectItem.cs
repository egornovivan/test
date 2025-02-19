using UnityEngine;

public class UIMapSelectItem : MonoBehaviour
{
	public delegate void ItemOnDbClick(object sender);

	public delegate void ItemOnClick(object sender);

	[SerializeField]
	private UILabel mLbText;

	[SerializeField]
	private UISprite mBg_dir;

	[SerializeField]
	private UISprite mBg_map;

	[SerializeField]
	private GameObject mTexSelected;

	[SerializeField]
	private UILabel mLbPathvalve;

	private UIMapSelectWnd.ItemType mType;

	private Texture _screenshot;

	private Vector3 _size;

	public int index;

	public UIMapSelectWnd.ItemType Type
	{
		get
		{
			return mType;
		}
		set
		{
			mType = value;
			EnableBg();
		}
	}

	public string Text
	{
		get
		{
			return mLbText.text;
		}
		set
		{
			mLbText.text = value;
		}
	}

	public Texture mTexture
	{
		get
		{
			return _screenshot;
		}
		set
		{
			_screenshot = value;
		}
	}

	public Vector3 mSize
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
		}
	}

	public event ItemOnDbClick e_ItemOnDbClick;

	public event ItemOnClick e_ItemOnClick;

	private void MapItemOver()
	{
		mTexSelected.SetActive(value: true);
	}

	private void MapItemOut()
	{
		mTexSelected.SetActive(value: false);
	}

	private void MapItemOnClick()
	{
		if (this.e_ItemOnClick != null)
		{
			this.e_ItemOnClick(this);
		}
	}

	private void MapItemOnDbClick()
	{
		if (this.e_ItemOnDbClick != null)
		{
			this.e_ItemOnDbClick(this);
		}
	}

	private void EnableBg()
	{
		if (mType == UIMapSelectWnd.ItemType.it_dir)
		{
			mBg_dir.enabled = true;
			mBg_map.enabled = false;
		}
		else
		{
			mBg_dir.enabled = false;
			mBg_map.enabled = true;
		}
	}
}
