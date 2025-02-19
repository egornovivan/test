using System;
using AnimationOrTween;
using UnityEngine;

public class UICustomGameSelectWnd : MonoBehaviour, IListReceiver
{
	[Serializable]
	public class CMapInfo
	{
		public UITexture texture;

		public UILabel name;

		public UILabel size;

		public GameObject root;
	}

	[Serializable]
	public class CPlayerInfo
	{
		public UIPopupList playerList;

		public GameObject root;
	}

	[SerializeField]
	private GameObject content;

	[SerializeField]
	private UIEfficientGrid mapItemGrid;

	[SerializeField]
	private UILabel pathLb;

	[SerializeField]
	private Transform mask;

	private UICustomMapItem mSelectedItem;

	[SerializeField]
	private CMapInfo mapInfo;

	[SerializeField]
	private CPlayerInfo playerInfo;

	[SerializeField]
	private UITweenBufferAlpha alphaTweener;

	[SerializeField]
	private TweenScale scaleTweener;

	[SerializeField]
	private UIHintBox hintBox;

	public string Path
	{
		get
		{
			return pathLb.text;
		}
		set
		{
			pathLb.text = value;
		}
	}

	public UICustomMapItem selectedItem => mSelectedItem;

	public UIHintBox HintBox => hintBox;

	public event Action<CMapInfo, CPlayerInfo, UICustomMapItem> onMapItemClick;

	public event Action<UICustomMapItem> onMapItemDoubleClick;

	public event Func<bool> onStartBtnClick;

	public event Action<int> onPlayerSelectedChanged;

	public event Action onInit;

	public event Action onOpen;

	public event Action onClose;

	public event Func<bool> onBack;

	public event Action<int, UICustomMapItem> onMapItemSetContent;

	public event Action<UICustomMapItem> onMapItemClearContent;

	void IListReceiver.SetContent(int index, GameObject go)
	{
		UICustomMapItem component = go.GetComponent<UICustomMapItem>();
		component.index = index;
		component.onClick -= OnMapItemClick;
		component.onDoubleClick -= OnMapItemDbClick;
		component.onClick += OnMapItemClick;
		component.onDoubleClick += OnMapItemDbClick;
		if (this.onMapItemSetContent != null)
		{
			this.onMapItemSetContent(index, component);
		}
	}

	void IListReceiver.ClearContent(GameObject go)
	{
		UICustomMapItem component = go.GetComponent<UICustomMapItem>();
		component.index = -1;
		component.IsSelected = false;
		component.onClick -= OnMapItemClick;
		component.onDoubleClick -= OnMapItemDbClick;
		if (this.onMapItemClearContent != null)
		{
			this.onMapItemClearContent(component);
		}
	}

	public void ClearMapItem()
	{
		mapItemGrid.UpdateList(0, this);
	}

	public void CreateMapItem(int count)
	{
		mapItemGrid.UpdateList(count, this);
	}

	public void Init()
	{
		mapItemGrid.itemGoPool.Init();
		if (this.onInit != null)
		{
			this.onInit();
		}
		hintBox.onOpen += OnHintBoxOpen;
		hintBox.onClose += OnHintBoxClose;
	}

	public void Open()
	{
		if (this.onOpen != null)
		{
			this.onOpen();
		}
		scaleTweener.Play(forward: true);
		alphaTweener.Play(forward: true);
		content.SetActive(value: true);
	}

	public void Close()
	{
		alphaTweener.Play(forward: false);
		scaleTweener.Play(forward: false);
	}

	private void OnAlphaTweenFinished(UITweener tween)
	{
		if (tween.direction == Direction.Reverse)
		{
			content.SetActive(value: false);
			if (this.onClose != null)
			{
				this.onClose();
			}
		}
	}

	private void OnMapItemClick(UICustomMapItem item)
	{
		for (int i = 0; i < mapItemGrid.Gos.Count; i++)
		{
			UICustomMapItem component = mapItemGrid.Gos[i].GetComponent<UICustomMapItem>();
			component.IsSelected = false;
		}
		item.IsSelected = true;
		mSelectedItem = item;
		if (item.IsFile)
		{
			if (mapInfo.root.activeSelf)
			{
				mapInfo.root.SetActive(value: false);
			}
			if (playerInfo.root != null && playerInfo.root.activeSelf)
			{
				playerInfo.root.SetActive(value: false);
			}
			return;
		}
		if (!mapInfo.root.activeSelf)
		{
			mapInfo.root.SetActive(value: true);
		}
		if (playerInfo.root != null && !playerInfo.root.activeSelf)
		{
			playerInfo.root.SetActive(value: true);
		}
		if (this.onMapItemClick != null)
		{
			this.onMapItemClick(mapInfo, playerInfo, item);
		}
	}

	private void OnMapItemDbClick(UICustomMapItem item)
	{
		if (this.onMapItemDoubleClick != null)
		{
			this.onMapItemDoubleClick(item);
		}
		if (item.IsFile)
		{
			mSelectedItem = null;
		}
		else
		{
			OnMapItemClick(item);
		}
	}

	private void OnStartBtnClick()
	{
		if (this.onStartBtnClick == null || !this.onStartBtnClick())
		{
			Close();
		}
	}

	private void OnCancelBtnClick()
	{
		Close();
	}

	private void OnClose()
	{
		Close();
	}

	private void OnBackClick()
	{
		if (this.onBack != null && this.onBack())
		{
			if (mSelectedItem != null)
			{
				mSelectedItem.IsSelected = false;
				mSelectedItem = null;
			}
			mapInfo.root.SetActive(value: false);
			if (playerInfo.root != null)
			{
				playerInfo.root.SetActive(value: false);
			}
		}
	}

	private void OnPlayerSelectChanged(string select)
	{
		if (this.onPlayerSelectedChanged != null)
		{
			int obj = playerInfo.playerList.items.FindIndex((string item0) => item0 == select);
			this.onPlayerSelectedChanged(obj);
		}
	}

	private void OnHintBoxOpen()
	{
		Vector3 localPosition = mask.localPosition;
		localPosition.z = (base.gameObject.transform.localPosition.z + hintBox.transform.localPosition.z) * 0.5f;
		mask.localPosition = localPosition;
	}

	private void OnHintBoxClose()
	{
		Vector3 localPosition = mask.localPosition;
		localPosition.z = 0f;
		mask.localPosition = localPosition;
	}

	private void Awake()
	{
		Init();
	}
}
