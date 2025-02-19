using System;
using UnityEngine;

public class UICustomMapItem : MonoBehaviour
{
	[SerializeField]
	private UISlicedSprite fileDirIcon;

	[SerializeField]
	private UISlicedSprite mapIcon;

	[SerializeField]
	private UILabel nameLb;

	[SerializeField]
	private UISlicedSprite seletedSprite;

	private bool mIsSelected;

	public int index = -1;

	public bool IsSelected
	{
		get
		{
			return mIsSelected;
		}
		set
		{
			mIsSelected = value;
			if (!mIsSelected)
			{
				seletedSprite.enabled = false;
				return;
			}
			seletedSprite.enabled = true;
			seletedSprite.alpha = 0.75f;
		}
	}

	public bool IsFile
	{
		get
		{
			return fileDirIcon.enabled;
		}
		set
		{
			if (value)
			{
				SetFileIcon();
			}
			else
			{
				SetMapIcon();
			}
		}
	}

	public bool IsMap
	{
		get
		{
			return mapIcon.enabled;
		}
		set
		{
			if (value)
			{
				SetMapIcon();
			}
			else
			{
				SetFileIcon();
			}
		}
	}

	public string nameStr
	{
		get
		{
			return nameLb.text;
		}
		set
		{
			nameLb.text = value;
		}
	}

	public event Action<UICustomMapItem> onClick;

	public event Action<UICustomMapItem> onDoubleClick;

	public void SetFileIcon()
	{
		fileDirIcon.enabled = true;
		mapIcon.enabled = false;
	}

	public void SetMapIcon()
	{
		fileDirIcon.enabled = false;
		mapIcon.enabled = true;
	}

	private void OnMouseOver()
	{
		if (IsSelected)
		{
			seletedSprite.alpha = 1f;
		}
		else
		{
			seletedSprite.alpha = 0.25f;
		}
		seletedSprite.enabled = true;
	}

	private void OnMouseOut()
	{
		if (IsSelected)
		{
			seletedSprite.alpha = 0.75f;
			seletedSprite.enabled = true;
		}
		else
		{
			seletedSprite.enabled = false;
			seletedSprite.alpha = 0f;
		}
	}

	private void OnMouseDbClick()
	{
		if (this.onDoubleClick != null)
		{
			this.onDoubleClick(this);
		}
	}

	private void OnMouseClick()
	{
		if (this.onClick != null)
		{
			this.onClick(this);
		}
	}

	private void Awake()
	{
		if (IsSelected)
		{
			seletedSprite.alpha = 0.75f;
			seletedSprite.enabled = true;
		}
		else
		{
			seletedSprite.enabled = false;
		}
	}
}
