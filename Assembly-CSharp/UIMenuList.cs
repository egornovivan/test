using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UIMenuList : UIComponent
{
	[HideInInspector]
	private const int MouseMoveInAudioID = 2494;

	private static UIMenuList minstance;

	public GameObject UIMeunItemPrefab;

	public UISlicedSprite SlicedSpriteBg;

	public GameObject ItemsContent;

	public Vector4 Margin = new Vector4(12f, 15f, 5f, 15f);

	public Vector2 ItemSize = new Vector2(188f, 50f);

	public List<UIMenuListItem> Items = new List<UIMenuListItem>();

	[HideInInspector]
	public bool IsShow = true;

	public Vector2 PanelMargin = new Vector2(200f, 0f);

	public List<UIMenuPanel> panels = new List<UIMenuPanel>();

	public UIMenuPanel rootPanel;

	public UIMenuListItem mMouseMoveInItem;

	public UIConpomentEvent e_ItemOnClick = new UIConpomentEvent();

	public UIConpomentEvent e_ItemOnMouseMoveIn = new UIConpomentEvent();

	public UIConpomentEvent e_ItemOnMouseMoveOut = new UIConpomentEvent();

	public static UIMenuList Instance => minstance;

	public bool mouseMoveOn
	{
		get
		{
			bool result = false;
			for (int i = 0; i < panels.Count; i++)
			{
				if (panels[i].mouseMoveOn)
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}

	private void Awake()
	{
		InitializeComponent();
		minstance = this;
	}

	public void InitializeComponent()
	{
		for (int i = 0; i < Items.Count; i++)
		{
			Items[i].e_OnClick += ItemOnClick;
			Items[i].e_OnMouseMoveIn += ItemOnMouseMoveIn;
			Items[i].e_OnMouseMoveOut += ItemOnMouseMoveOut;
		}
		for (int j = 0; j < panels.Count; j++)
		{
			if (panels[j].gameObject.name == "root")
			{
				rootPanel = panels[j];
			}
			panels[j].Hide();
		}
		if (rootPanel != null)
		{
			rootPanel.Show();
		}
	}

	public void Hide()
	{
		Hide_ChildPanel(null);
		rootPanel.Hide();
		IsShow = false;
	}

	public void Show()
	{
		rootPanel.Show();
		IsShow = true;
	}

	public UIMenuListItem AddItem(int parentIndex, string text, UIGameMenuCtrl.MenuItemFlag flag, string icoName = "")
	{
		return AddItem(Items[parentIndex], text, flag, icoName);
	}

	public UIMenuListItem AddItem(UIMenuListItem parent, string text, UIGameMenuCtrl.MenuItemFlag flag, string icoName = "")
	{
		GameObject gameObject = AddObj();
		UIMenuPanel uIMenuPanel = FindMenuPanel(parent);
		if (uIMenuPanel == null)
		{
			uIMenuPanel = CreatePanel(parent, this);
		}
		UIMenuListItem component = gameObject.GetComponent<UIMenuListItem>();
		component.Text = text;
		component.Parent = parent;
		component.icoName = icoName;
		component.mMenuItemFlag = flag;
		component.e_OnClick += ItemOnClick;
		component.e_OnMouseMoveIn += ItemOnMouseMoveIn;
		component.e_OnMouseMoveOut += ItemOnMouseMoveOut;
		gameObject.transform.parent = uIMenuPanel.content.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		Items.Insert(Items.Count, component);
		UpdateIndex();
		uIMenuPanel.UpdatePosition();
		if (parent != null)
		{
			parent.IsHaveChild = true;
		}
		return component;
	}

	public UIMenuPanel FindMenuPanel(UIMenuListItem parent)
	{
		return panels.Find((UIMenuPanel mp) => mp.parent == parent);
	}

	public bool DeleteItem(int index)
	{
		if (index >= Items.Count || index < 0)
		{
			return false;
		}
		return DeleteItem(Items[index]);
	}

	public bool DeleteItem(UIMenuListItem item)
	{
		DeleteChildItem(item);
		Debug.Log(item.gameObject.name);
		if (item.gameObject != null)
		{
			item.gameObject.transform.parent = null;
			if (Application.isEditor)
			{
				Object.DestroyImmediate(item.gameObject);
			}
			else
			{
				Object.Destroy(item.gameObject);
			}
		}
		Items.Remove(item);
		UpdateIndex();
		UIMenuPanel uIMenuPanel = FindMenuPanel(item.Parent);
		if (uIMenuPanel != null)
		{
			uIMenuPanel.UpdatePosition();
		}
		if (GetChildItems(item.Parent).Count == 0)
		{
			DeletePanel(item.Parent);
			if (item.Parent != null)
			{
				item.Parent.IsHaveChild = false;
			}
		}
		return true;
	}

	public void UpdatePanelPositon()
	{
		foreach (UIMenuPanel panel in panels)
		{
			panel.UpdatePosition();
		}
	}

	public List<UIMenuListItem> GetChildItems(UIMenuListItem parent)
	{
		List<UIMenuListItem> list = new List<UIMenuListItem>();
		for (int i = 0; i < Items.Count; i++)
		{
			if (Items[i].Parent == parent)
			{
				list.Add(Items[i]);
			}
		}
		return list;
	}

	private GameObject AddObj()
	{
		if (UIMeunItemPrefab == null)
		{
			Debug.LogError("Error: 'UIMenuList.UIMeunItemPrefab = null' !");
			return null;
		}
		if (ItemsContent == null)
		{
			Debug.LogError("Error: 'UIMenuList.SlicedSpriteBg = null' !");
			return null;
		}
		GameObject gameObject = Object.Instantiate(UIMeunItemPrefab);
		gameObject.SetActive(value: true);
		return gameObject;
	}

	private void UpdateIndex()
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (Items[i] != null)
			{
				Items[i].Index = i;
			}
		}
	}

	private void DeleteChildItem(UIMenuListItem parent)
	{
		List<UIMenuListItem> childItems = GetChildItems(parent);
		foreach (UIMenuListItem item in childItems)
		{
			DeleteItem(item);
		}
		DeletePanel(parent);
	}

	private void DeletePanel(UIMenuListItem parent)
	{
		UIMenuPanel uIMenuPanel = FindMenuPanel(parent);
		if (uIMenuPanel == null)
		{
			return;
		}
		if (uIMenuPanel.gameObject != null)
		{
			uIMenuPanel.gameObject.transform.parent = null;
			if (Application.isEditor)
			{
				Object.DestroyImmediate(uIMenuPanel.gameObject);
			}
			else
			{
				Object.Destroy(uIMenuPanel.gameObject);
			}
		}
		panels.Remove(uIMenuPanel);
	}

	private UIMenuPanel CreatePanel(UIMenuListItem _parent, UIMenuList _list)
	{
		string text = ((!(_parent == null)) ? ("panel_" + _parent.Text) : "root");
		GameObject gameObject = new GameObject(text);
		gameObject.transform.parent = _list.ItemsContent.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		UIMenuPanel uIMenuPanel = gameObject.AddComponent<UIMenuPanel>();
		uIMenuPanel.Init(_parent, _list);
		panels.Add(uIMenuPanel);
		if (uIMenuPanel.parent == null)
		{
			rootPanel = uIMenuPanel;
		}
		return uIMenuPanel;
	}

	private void Hide_BotherPanels(UIMenuListItem item)
	{
		List<UIMenuListItem> childItems = GetChildItems(item.Parent);
		for (int i = 0; i < childItems.Count; i++)
		{
			if (childItems[i] != item)
			{
				UIMenuPanel uIMenuPanel = FindMenuPanel(childItems[i]);
				if (uIMenuPanel != null && uIMenuPanel.isShow)
				{
					Hide_ChildPanel(childItems[i]);
					uIMenuPanel.Hide();
				}
			}
		}
	}

	private void Hide_ChildPanel(UIMenuListItem item)
	{
		List<UIMenuListItem> childItems = GetChildItems(item);
		for (int i = 0; i < childItems.Count; i++)
		{
			if (!childItems[i].IsHaveChild)
			{
				continue;
			}
			UIMenuPanel uIMenuPanel = FindMenuPanel(childItems[i]);
			if (uIMenuPanel != null && uIMenuPanel.isShow)
			{
				if (childItems[i].IsHaveChild)
				{
					Hide_ChildPanel(childItems[i]);
				}
				uIMenuPanel.Hide();
			}
		}
	}

	private void ItemOnMouseMoveIn(object sender)
	{
		UIMenuListItem uIMenuListItem = sender as UIMenuListItem;
		if (uIMenuListItem != null)
		{
			if (uIMenuListItem.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_Phone)
			{
				UIMenuListItem uIMenuListItem2 = Items.Find((UIMenuListItem a) => a.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_Diplomacy);
				if (PeGameMgr.IsAdventure || PeSingleton<ReputationSystem>.Instance.GetActiveState((int)PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID)))
				{
					if (null == uIMenuListItem2)
					{
						AddItem(uIMenuListItem, NewUIText.mMenuDiplomacy.GetString(), UIGameMenuCtrl.MenuItemFlag.Flag_Diplomacy, "listico_22_1");
					}
				}
				else if (uIMenuListItem2 != null)
				{
					DeleteItem(uIMenuListItem2);
				}
				UIMenuListItem uIMenuListItem3 = Items.Find((UIMenuListItem a) => a.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_SpeciesWiki);
				if (PeGameMgr.IsAdventure || (PeGameMgr.IsStory && null != StroyManager.Instance && StroyManager.Instance.enableBook))
				{
					if (null == uIMenuListItem3)
					{
						AddItem(uIMenuListItem, NewUIText.mMenuSpeciesWiki.GetString(), UIGameMenuCtrl.MenuItemFlag.Flag_SpeciesWiki, "listico_24_1");
					}
				}
				else if (uIMenuListItem3 != null)
				{
					DeleteItem(uIMenuListItem3);
				}
				UIMenuListItem uIMenuListItem4 = Items.Find((UIMenuListItem a) => a.mMenuItemFlag == UIGameMenuCtrl.MenuItemFlag.Flag_Radio);
				if (null != GameUI.Instance)
				{
					if (!GameUI.Instance.mPhoneWnd.InitRadio)
					{
						GameUI.Instance.mPhoneWnd.InitRadioData();
					}
					if (GameUI.Instance.mPhoneWnd.CheckOpenRadio())
					{
						if (null == uIMenuListItem4)
						{
							AddItem(uIMenuListItem, NewUIText.mMenuRadio.GetString(), UIGameMenuCtrl.MenuItemFlag.Flag_Radio, "listico_25_1");
						}
					}
					else if (uIMenuListItem4 != null)
					{
						DeleteItem(uIMenuListItem4);
					}
				}
			}
			uIMenuListItem.ItemSelectedBg.enabled = true;
			AudioManager.instance.Create(Vector3.zero, 2494);
			if (uIMenuListItem != mMouseMoveInItem)
			{
				Hide_BotherPanels(uIMenuListItem);
				if (uIMenuListItem.IsHaveChild)
				{
					UIMenuPanel uIMenuPanel = FindMenuPanel(uIMenuListItem);
					if (uIMenuPanel != null)
					{
						uIMenuPanel.Show();
					}
				}
			}
		}
		mMouseMoveInItem = uIMenuListItem;
		e_ItemOnMouseMoveIn.Send(eventReceiver, sender);
	}

	private void ItemOnMouseMoveOut(object sender)
	{
		UIMenuListItem uIMenuListItem = sender as UIMenuListItem;
		if (uIMenuListItem != null)
		{
			bool flag = true;
			UIMenuPanel uIMenuPanel = FindMenuPanel(uIMenuListItem);
			if (uIMenuPanel != null && uIMenuPanel.isShow)
			{
				flag = false;
			}
			if (flag)
			{
				uIMenuListItem.ItemSelectedBg.enabled = false;
			}
		}
		e_ItemOnMouseMoveOut.Send(eventReceiver, sender);
	}

	private void ItemOnClick(object sender)
	{
		e_ItemOnClick.Send(eventReceiver, sender);
	}

	public void RefreshHotKeyName()
	{
		if (Items.Count == 0)
		{
			return;
		}
		for (int i = 0; i < Items.Count; i++)
		{
			if (Items[i].KeyId != -1)
			{
				switch (Items[i].mCategory)
				{
				case UIOption.KeyCategory.Common:
				{
					string hotKeyContent = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[0][Items[i].KeyId])._key.ToStr();
					Items[i].SetHotKeyContent(hotKeyContent);
					break;
				}
				case UIOption.KeyCategory.Character:
				{
					string hotKeyContent = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[1][Items[i].KeyId])._key.ToStr();
					Items[i].SetHotKeyContent(hotKeyContent);
					break;
				}
				case UIOption.KeyCategory.Construct:
				{
					string hotKeyContent = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[2][Items[i].KeyId])._key.ToStr();
					Items[i].SetHotKeyContent(hotKeyContent);
					break;
				}
				case UIOption.KeyCategory.Carrier:
				{
					string hotKeyContent = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[3][Items[i].KeyId])._key.ToStr();
					Items[i].SetHotKeyContent(hotKeyContent);
					break;
				}
				}
			}
		}
	}
}
