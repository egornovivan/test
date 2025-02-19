using System.Collections.Generic;
using UnityEngine;

public class UIOptionMenu : UIStaticWnd
{
	[SerializeField]
	private GameObject mOptionPrefab;

	[SerializeField]
	private UIGrid mGrid;

	[SerializeField]
	private UISprite mSprBg;

	public int mItemHeight = 30;

	public int mItemWidth = 140;

	[HideInInspector]
	private List<UIOptionMenuItem> mItems = new List<UIOptionMenuItem>();

	private Camera uiCamera;

	private Collider mBgCollider;

	public int ItemsCount => mItems.Count;

	public void Init(Camera _uiCamera)
	{
		if (!(null == this))
		{
			if ((bool)mSprBg)
			{
				mBgCollider = mSprBg.GetComponent<BoxCollider>();
			}
			if (null != _uiCamera)
			{
				uiCamera = _uiCamera;
			}
		}
	}

	public override void Show()
	{
		Repostion();
		CalculatePos();
		base.Show();
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	public void AddOption(string text, UIOptionMenuItem.BaseMsgEvent clickCallBack)
	{
		GameObject gameObject = Object.Instantiate(mOptionPrefab);
		gameObject.transform.parent = mGrid.transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, -2f);
		gameObject.transform.localScale = Vector3.one;
		UIOptionMenuItem component = gameObject.GetComponent<UIOptionMenuItem>();
		component.e_OnClickItem += clickCallBack;
		component.Init(text, ItemsCount);
		mItems.Add(component);
	}

	public UIOptionMenuItem GetItem(int index)
	{
		return (index >= ItemsCount) ? null : mItems[index];
	}

	public void Repostion()
	{
		mGrid.repositionNow = true;
		mSprBg.transform.localScale = new Vector3(mItemWidth + 16, mItemHeight * ItemsCount + 16, 1f);
	}

	public void Clear()
	{
		for (int i = 0; i < ItemsCount; i++)
		{
			mItems[i].gameObject.transform.parent = null;
			Object.Destroy(mItems[i].gameObject);
		}
		mItems.Clear();
	}

	private void CalculatePos()
	{
		if (!(uiCamera == null))
		{
			Vector3 mousePosition = Input.mousePosition;
			Vector3 localScale = mSprBg.transform.localScale;
			mousePosition.x = Mathf.Clamp01(mousePosition.x / (float)Screen.width);
			mousePosition.y = Mathf.Clamp01(mousePosition.y / (float)Screen.height);
			float num = uiCamera.orthographicSize / base.gameObject.transform.parent.lossyScale.y;
			float num2 = (float)Screen.height * 0.5f / num;
			Vector2 vector = new Vector2(num2 * localScale.x / (float)Screen.width, num2 * localScale.y / (float)Screen.height);
			mousePosition.x = Mathf.Min(mousePosition.x, 1f - vector.x);
			mousePosition.y = Mathf.Max(mousePosition.y, vector.y);
			base.gameObject.transform.position = uiCamera.ViewportToWorldPoint(mousePosition);
			mousePosition = base.gameObject.transform.localPosition;
			mousePosition.x = Mathf.Round(mousePosition.x);
			mousePosition.y = Mathf.Round(mousePosition.y);
			mousePosition.z = -100f;
			base.gameObject.transform.localPosition = mousePosition;
		}
	}

	private bool IsMouseIn()
	{
		Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		return mBgCollider.Raycast(ray, out hitInfo, 100f);
	}

	private void Update()
	{
		if (isShow && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && !IsMouseIn())
		{
			Hide();
		}
	}
}
