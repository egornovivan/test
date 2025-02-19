using UnityEngine;

public class PlayerBuildGirdItem : MonoBehaviour
{
	public enum Type
	{
		Type_Null,
		Type_Head,
		Type_Face,
		Type_Hair,
		Type_Save
	}

	public delegate void ClickItemEvent(int index, Type _type);

	[SerializeField]
	private UISprite mIcon;

	[SerializeField]
	private UITexture mTexture;

	[SerializeField]
	private UITexture mTexSelected;

	[SerializeField]
	private UISprite mbg;

	[HideInInspector]
	public int mIndex;

	[HideInInspector]
	public Type mType;

	public bool canSelected;

	private bool mSelected;

	public bool isSelected
	{
		get
		{
			return mSelected;
		}
		set
		{
			mSelected = value;
			if (canSelected)
			{
				mTexSelected.enabled = mSelected;
			}
		}
	}

	public event ClickItemEvent e_ClickItem;

	private void Start()
	{
		Transform transform = base.transform.FindChild("Background");
		if (null != transform)
		{
			UIWidget component = transform.GetComponent<UIWidget>();
			if (null != component)
			{
				component.MakePixelPerfect();
			}
		}
	}

	public void InitItem(int index, Type _type)
	{
		mIndex = index;
		mType = _type;
	}

	public void SetItemInfo(string iconName)
	{
		mIcon.spriteName = iconName;
		mIcon.enabled = true;
		mTexture.enabled = false;
	}

	public void SetItemInfo(Texture2D texture)
	{
		mTexture.mainTexture = texture;
		mTexture.enabled = true;
		mIcon.enabled = false;
	}

	private void OnClick()
	{
		if (Input.GetMouseButtonUp(0) && this.e_ClickItem != null)
		{
			this.e_ClickItem(mIndex, mType);
		}
	}
}
