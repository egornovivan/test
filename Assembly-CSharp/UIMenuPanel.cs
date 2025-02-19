using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuPanel : MonoBehaviour
{
	public UIMenuListItem parent;

	public UISlicedSprite spBg;

	public GameObject content;

	private UIMenuList list;

	public BoxCollider mBoxCollider;

	[HideInInspector]
	public bool mouseMoveOn;

	public bool isShow => base.gameObject.activeSelf;

	public void Init(UIMenuListItem _parent, UIMenuList _list)
	{
		list = _list;
		parent = _parent;
		content = new GameObject("content");
		content.transform.parent = base.gameObject.transform;
		content.transform.localPosition = Vector3.zero;
		content.transform.localScale = Vector3.one;
		GameObject gameObject = UnityEngine.Object.Instantiate(list.SlicedSpriteBg.gameObject);
		gameObject.transform.parent = base.gameObject.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.SetActive(value: true);
		spBg = gameObject.GetComponent<UISlicedSprite>();
		mBoxCollider = gameObject.GetComponent<BoxCollider>();
		base.gameObject.SetActive(value: true);
		UpdatePosition();
	}

	public void UpdatePosition()
	{
		if (list == null)
		{
			return;
		}
		List<UIMenuListItem> childItems = list.GetChildItems(parent);
		int count = childItems.Count;
		for (int i = 0; i < childItems.Count; i++)
		{
			if (childItems[i] != null)
			{
				GameObject gameObject = childItems[i].gameObject;
				gameObject.transform.localPosition = new Vector3(0f, (float)(-i) * list.ItemSize.y, 0f);
				childItems[i].Box_Collider.center = new Vector3(list.ItemSize.x / 2f - 29f, 0f, 0f);
				childItems[i].Box_Collider.size = new Vector3(list.ItemSize.x, list.ItemSize.y, -2f);
				gameObject.SetActive(value: true);
			}
		}
		int num = Convert.ToInt32(list.ItemSize.x);
		int num2 = Convert.ToInt32(list.Margin.y + list.ItemSize.y * (float)count + list.Margin.w);
		spBg.transform.localScale = new Vector3(num, num2, 1f);
		int num3 = Convert.ToInt32(list.Margin.x);
		int num4 = Convert.ToInt32(0f - list.Margin.y + list.ItemSize.y * (float)count / 2f);
		content.transform.localPosition = new Vector3(num3 - 4, num4, 0f);
		if (parent != null)
		{
			Vector3 localPosition = parent.gameObject.transform.localPosition;
			localPosition += parent.gameObject.transform.parent.localPosition;
			localPosition += parent.gameObject.transform.parent.parent.localPosition;
			int num5 = Convert.ToInt32(localPosition.x + list.PanelMargin.x);
			int num6 = Convert.ToInt32(localPosition.y + list.PanelMargin.y);
			base.gameObject.transform.localPosition = new Vector3(num5 - 5, num6, 0f);
			spBg.pivot = UIWidget.Pivot.Left;
			mBoxCollider.center = new Vector3(0.5f, 0f, 0f);
		}
		else
		{
			spBg.pivot = UIWidget.Pivot.Bottom;
			mBoxCollider.center = new Vector3(0f, 0.5f, 0f);
		}
	}

	private void Update()
	{
		if (parent != null && parent.IsHaveChild && base.gameObject.activeSelf)
		{
			parent.ItemSelectedBg.enabled = true;
		}
		if (mBoxCollider != null && !(UICamera.mainCamera == null))
		{
			Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
			mouseMoveOn = mBoxCollider.Raycast(ray, out var _, 300f);
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		if (parent != null)
		{
			parent.ItemSelectedBg.enabled = false;
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		mouseMoveOn = false;
	}
}
