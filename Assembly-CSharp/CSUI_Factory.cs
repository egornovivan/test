using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CustomData;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_Factory : MonoBehaviour
{
	public CSFactory m_Factory;

	public CSEntity m_Entity;

	private ItemLabel.Root m_RootType;

	private int m_ItemType;

	private List<UICompoundWndListItem> m_MenuItems = new List<UICompoundWndListItem>();

	public CSUI_FactoryReplicator FactoryReplicator;

	public CSUI_CompoundItem m_CompoundItemPrefab;

	public UIGrid m_CompoundItemRoot;

	public Transform mScriptListParent;

	public UIScriptItem_N m_UIScriptItemPrefab;

	public int mScriptItemPaddingX = 30;

	private Queue<UIScriptItem_N> mScriptItemPool = new Queue<UIScriptItem_N>();

	private List<UIScriptItem_N> mCurScriptItemList = new List<UIScriptItem_N>();

	private int mListSelectedIndex;

	private string m_AllStr = string.Empty;

	private List<CSUI_CompoundItem> m_CompoudItems = new List<CSUI_CompoundItem>();

	private Dictionary<int, List<Replicator.KnownFormula>> m_Formulas = new Dictionary<int, List<Replicator.KnownFormula>>();

	private List<ItemProto> m_ItemDataList = new List<ItemProto>();

	private UIGridItemCtrl m_RightGridItem;

	private UIScriptItem_N m_BackupScriptItem;

	private int m_CurItemID;

	private Replicator m_Replicator => UIGraphControl.GetReplicator();

	public void SetEntity(CSEntity enti)
	{
		if (enti == null)
		{
			Debug.LogWarning("Reference Entity is null.");
			return;
		}
		m_Factory = enti as CSFactory;
		if (m_Factory == null)
		{
			Debug.LogWarning("Reference Entity is not a Assembly Entity.");
			return;
		}
		m_Entity = enti;
		CSUI_MainWndCtrl.Instance.mSelectedEnntity = m_Entity;
	}

	private CSUI_CompoundItem _createCompoundItem()
	{
		CSUI_CompoundItem cSUI_CompoundItem = Object.Instantiate(m_CompoundItemPrefab);
		cSUI_CompoundItem.transform.parent = m_CompoundItemRoot.transform;
		cSUI_CompoundItem.transform.localPosition = Vector3.zero;
		cSUI_CompoundItem.transform.localRotation = Quaternion.identity;
		cSUI_CompoundItem.transform.localScale = Vector3.one;
		cSUI_CompoundItem.gameObject.GetComponent<UICheckbox>().radioButtonRoot = m_CompoundItemRoot.transform;
		cSUI_CompoundItem.Count = 0;
		cSUI_CompoundItem.SliderValue = 0f;
		cSUI_CompoundItem.RightBtnClickEvent = OnCompoudClick;
		cSUI_CompoundItem.onDeleteBtnClick += OnDeleteBtnClick;
		m_CompoudItems.Add(cSUI_CompoundItem);
		m_CompoundItemRoot.repositionNow = true;
		return cSUI_CompoundItem;
	}

	private void OnCompoudClick(CSUI_CompoundItem ci)
	{
		if (!GameConfig.IsMultiMode)
		{
			int num = m_CompoudItems.FindIndex((CSUI_CompoundItem item0) => item0 == ci);
			if (num == -1 || !CSUI_MainWndCtrl.IsWorking() || m_Factory.CompoudItemsCount <= num)
			{
				return;
			}
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (cmpt == null)
			{
				Debug.LogError("CSUI_Factory.OnCompoundClick: pkg is null!");
				return;
			}
			CompoudItem outCompoudItem = null;
			ItemProto itemData;
			if (!m_Factory.GetTakeAwayCompoundItem(num, out outCompoudItem))
			{
				if (outCompoudItem != null)
				{
					itemData = ItemProto.GetItemData(outCompoudItem.itemID);
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mIsCompounding.GetString(), itemData.GetName()), Color.red);
				}
				return;
			}
			itemData = ItemProto.GetItemData(outCompoudItem.itemID);
			int vacancyCount = cmpt.package.GetSlotList(outCompoudItem.itemID).vacancyCount;
			if (!cmpt.package.CanAdd(m_Factory.Data.m_CompoudItems[num].itemID, ci.Count))
			{
				string text = PELocalization.GetString(8000050).Replace("\\n", " ");
				CSUI_MainWndCtrl.ShowStatusBar(text, Color.red);
				if (itemData.maxStackNum > 1 || vacancyCount == 0)
				{
					return;
				}
			}
			int itemCnt = outCompoudItem.itemCnt;
			int num2 = itemCnt;
			if (itemData.maxStackNum > 1)
			{
				cmpt.Add(outCompoudItem.itemID, outCompoudItem.itemCnt);
			}
			else
			{
				if (itemCnt > vacancyCount)
				{
					num2 = vacancyCount;
				}
				cmpt.Add(outCompoudItem.itemID, num2);
			}
			outCompoudItem.itemCnt = itemCnt - num2;
			if (outCompoudItem.itemCnt == 0)
			{
				m_Factory.TakeAwayCompoudItem(num);
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayCompoundItem.GetString(), itemData.GetName()));
			}
		}
		else
		{
			int num3 = m_CompoudItems.FindIndex((CSUI_CompoundItem item0) => item0 == ci);
			if (num3 != -1 && m_Factory.CompoudItemsCount > num3 && m_Factory.Data.m_CompoudItems[num3] != null && !(m_Factory.Data.m_CompoudItems[num3].curTime < m_Factory.Data.m_CompoudItems[num3].time))
			{
				m_Factory._ColonyObj._Network.FCT_Fetch(num3);
			}
		}
	}

	private void OnDeleteBtnClick(CSUI_CompoundItem ci)
	{
		if (!(ci == null) && m_Factory != null)
		{
			int num = m_CompoudItems.FindIndex((CSUI_CompoundItem item0) => item0 == ci);
			if (num != -1)
			{
				m_Factory.OnCancelCompound(num);
			}
		}
	}

	private void SetPopuplistItem()
	{
		UIPopupList popList = FactoryReplicator.m_MenuContent.popList;
		popList.items.Clear();
		popList.items.Add(m_AllStr);
		if (m_RootType != 0)
		{
			popList.items.AddRange(ItemLabel.GetDirectChildrenName((int)m_RootType));
		}
		if (popList.items.Count > 0)
		{
			popList.selection = popList.items[0];
		}
		UpdateLeftList(useSearch: true);
	}

	private void UpdateLeftListEventHandler(object sender, Replicator.EventArg e)
	{
		UpdateLeftList();
	}

	private void UpdateLeftList(bool useSearch = false)
	{
		if (m_Replicator == null)
		{
			return;
		}
		string queryString = FactoryReplicator.GetQueryString();
		m_Formulas.Clear();
		m_ItemDataList.Clear();
		Dictionary<ItemProto, List<Replicator.KnownFormula>> dictionary = new Dictionary<ItemProto, List<Replicator.KnownFormula>>();
		foreach (Replicator.KnownFormula kf in m_Replicator.knowFormulas)
		{
			if (kf == null)
			{
				continue;
			}
			Replicator.Formula formula = kf.Get();
			if (formula == null)
			{
				continue;
			}
			ItemProto item = ItemProto.GetItemData(formula.productItemId);
			if (item == null)
			{
				continue;
			}
			bool flag = false;
			if (m_RootType != ItemLabel.Root.ISO && (m_RootType == ItemLabel.Root.all || (m_RootType == item.rootItemLabel && (m_ItemType == 0 || m_ItemType == item.itemLabel))))
			{
				if (useSearch)
				{
					if (QueryItem(queryString, item.GetName()))
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				if (!dictionary.Keys.Any((ItemProto a) => a.id == item.id))
				{
					dictionary.Add(item, new List<Replicator.KnownFormula>());
				}
				ItemProto key = dictionary.Keys.First((ItemProto a) => a.id == item.id);
				if (!dictionary[key].Any((Replicator.KnownFormula a) => a.id == kf.id))
				{
					dictionary[key].Add(kf);
				}
			}
		}
		dictionary = dictionary.OrderBy((KeyValuePair<ItemProto, List<Replicator.KnownFormula>> a) => a.Key.sortLabel).ToDictionary((KeyValuePair<ItemProto, List<Replicator.KnownFormula>> k) => k.Key, (KeyValuePair<ItemProto, List<Replicator.KnownFormula>> v) => v.Value);
		m_Formulas = dictionary.ToDictionary((KeyValuePair<ItemProto, List<Replicator.KnownFormula>> k) => k.Key.id, (KeyValuePair<ItemProto, List<Replicator.KnownFormula>> v) => v.Value);
		m_ItemDataList = dictionary.Keys.ToList();
		FactoryReplicator.m_MenuContent.gridList.UpdateList(m_ItemDataList.Count, SetMenuListItemContent, ClearMenuListItemContent);
		FactoryReplicator.m_MenuContent.scrollBox.m_VertScrollBar.scrollValue = 0f;
	}

	private bool QueryItem(string text, string ItemName)
	{
		if (text.Trim().Length == 0)
		{
			return true;
		}
		string text2 = mToLower(text);
		string text3 = mToLower(ItemName);
		return text3.Contains(text2) || text2.Contains(text3);
	}

	private string mToLower(string strs)
	{
		string text = strs;
		char[] array = text.ToCharArray();
		Regex regex = new Regex("[A-Z]");
		text = string.Empty;
		char[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			char c = array2[i];
			text = ((!regex.IsMatch(c.ToString())) ? (text + c) : (text + c.ToString().ToLower()));
		}
		return text;
	}

	private void SetMenuListItemContent(int index, GameObject go)
	{
		UICompoundWndListItem component = go.GetComponent<UICompoundWndListItem>();
		if (component == null || index < 0 || index >= m_ItemDataList.Count)
		{
			return;
		}
		ItemProto itemProto = m_ItemDataList[index];
		if (m_Formulas.ContainsKey(itemProto.id))
		{
			bool bNew = m_Formulas[itemProto.id].Any((Replicator.KnownFormula a) => a.flag);
			component.SetItem(itemProto.name, itemProto.id, bNew, itemProto.icon, itemProto.GetName(), index, ListItemType.mItem);
			component.SetSelectmState(isSelected: false);
			component.mItemClick -= OnMenuListItemClick;
			component.mItemClick += OnMenuListItemClick;
		}
	}

	private void ClearMenuListItemContent(GameObject go)
	{
		UICompoundWndListItem component = go.GetComponent<UICompoundWndListItem>();
		if (!(component == null))
		{
			component.mItemClick -= OnMenuListItemClick;
		}
	}

	private void OnLeftMenuClick(GameObject go, int index)
	{
		if (go == null)
		{
			Debug.LogError("Gamobject is miss for Replicator menu");
		}
		else
		{
			if (!Input.GetMouseButtonUp(0))
			{
				return;
			}
			switch (index)
			{
			case 0:
				if (m_RootType != 0)
				{
					m_RootType = ItemLabel.Root.all;
					SetPopuplistItem();
				}
				break;
			case 1:
				if (m_RootType != ItemLabel.Root.weapon)
				{
					m_RootType = ItemLabel.Root.weapon;
					SetPopuplistItem();
				}
				break;
			case 2:
				if (m_RootType != ItemLabel.Root.equipment)
				{
					m_RootType = ItemLabel.Root.equipment;
					SetPopuplistItem();
				}
				break;
			case 3:
				if (m_RootType != ItemLabel.Root.tool)
				{
					m_RootType = ItemLabel.Root.tool;
					SetPopuplistItem();
				}
				break;
			case 4:
				if (m_RootType != ItemLabel.Root.turret)
				{
					m_RootType = ItemLabel.Root.turret;
					SetPopuplistItem();
				}
				break;
			case 5:
				if (m_RootType != ItemLabel.Root.consumables)
				{
					m_RootType = ItemLabel.Root.consumables;
					SetPopuplistItem();
				}
				break;
			case 6:
				if (m_RootType != ItemLabel.Root.resoure)
				{
					m_RootType = ItemLabel.Root.resoure;
					SetPopuplistItem();
				}
				break;
			case 7:
				if (m_RootType != ItemLabel.Root.part)
				{
					m_RootType = ItemLabel.Root.part;
					SetPopuplistItem();
				}
				break;
			case 8:
				if (m_RootType != ItemLabel.Root.decoration)
				{
					m_RootType = ItemLabel.Root.decoration;
					SetPopuplistItem();
				}
				break;
			case 9:
				if (m_RootType != ItemLabel.Root.ISO)
				{
					m_RootType = ItemLabel.Root.ISO;
					SetPopuplistItem();
				}
				break;
			}
		}
	}

	private void OnMenuSelectionChange(string selection)
	{
		if (selection == m_AllStr)
		{
			m_ItemType = 0;
		}
		else
		{
			m_ItemType = ItemLabel.GetItemTypeByName(selection);
		}
		UpdateLeftList();
	}

	private void OnMenuListItemClick(int index)
	{
		if (index >= 0 && index < m_ItemDataList.Count)
		{
			List<GameObject> gos = FactoryReplicator.m_MenuContent.gridList.Gos;
			if (mListSelectedIndex != -1 && mListSelectedIndex < gos.Count)
			{
				UICompoundWndListItem component = gos[mListSelectedIndex].GetComponent<UICompoundWndListItem>();
				component.SetSelectmState(isSelected: false);
			}
			if (index < gos.Count)
			{
				UICompoundWndListItem component2 = gos[index].GetComponent<UICompoundWndListItem>();
				component2.SetSelectmState(isSelected: true);
			}
			mListSelectedIndex = index;
			UpdateCurItemScriptList(m_ItemDataList[index].id);
			SelectFirstScritItem();
		}
	}

	private void _updateQueryGridItems(int m_id)
	{
		if (FactoryReplicator == null)
		{
			return;
		}
		Replicator replicator = UIGraphControl.GetReplicator();
		if (replicator == null)
		{
			return;
		}
		FactoryReplicator.DestroyQueryItems();
		foreach (Replicator.KnownFormula knowFormula in replicator.knowFormulas)
		{
			Replicator.Formula formula = knowFormula.Get();
			if (formula == null)
			{
				continue;
			}
			for (int i = 0; i < formula.materials.Count; i++)
			{
				if (formula.materials[i].itemId == m_id)
				{
					_createQueryItems(formula.productItemId);
				}
			}
		}
	}

	private void _createQueryItems(int ID)
	{
		GameObject gameObject = FactoryReplicator.InstantiateQueryItem(string.Empty);
		UIGridItemCtrl component = gameObject.GetComponent<UIGridItemCtrl>();
		component.SetToolTipInfo(ListItemType.mItem, ID);
		UIEventListener component2 = gameObject.GetComponent<UIEventListener>();
		component2.onClick = OnQueryGridItemClick;
		ItemProto itemData = ItemProto.GetItemData(ID);
		component.SetCotent(itemData.icon);
	}

	private void OnQueryGridItemClick(GameObject go)
	{
		UIGridItemCtrl component = go.GetComponent<UIGridItemCtrl>();
		if (!(component == null))
		{
			int mItemId = component.mItemId;
			if (ReDrawGraph(mItemId))
			{
				_updateQueryGridItems(mItemId);
				_setCompundInfo(mItemId);
				FactoryReplicator.AddGraphHistory(mItemId);
			}
			FactoryReplicator.m_MiddleContent.graphScrollBox.Reposition();
		}
	}

	private void OnQuerySearchBtnClick(GameObject go)
	{
		if (FactoryReplicator.IsQueryInputValid())
		{
			m_RootType = ItemLabel.Root.all;
			FactoryReplicator.SetMenuCBChecked(0, check: true);
			SetPopuplistItem();
		}
		else
		{
			UpdateLeftList();
		}
	}

	private void NewOnQuerySearchBtnClick()
	{
		if (FactoryReplicator.IsQueryInputValid())
		{
			m_RootType = ItemLabel.Root.all;
			FactoryReplicator.SetMenuCBChecked(0, check: true);
			SetPopuplistItem();
		}
		else
		{
			UpdateLeftList();
		}
	}

	private void OnQueryClearBtnClick(GameObject go)
	{
		if (FactoryReplicator.IsQueryInputValid())
		{
			FactoryReplicator.m_MiddleContent.queryInput.text = string.Empty;
			UpdateLeftList();
		}
	}

	private void _initCompundGridItems()
	{
		if (!(m_RightGridItem != null))
		{
			GameObject gameObject = FactoryReplicator.InstantiateGridItem(string.Empty);
			m_RightGridItem = gameObject.GetComponent<UIGridItemCtrl>();
		}
	}

	private void _setCompundInfo(int id)
	{
		UIGraphNode rootNode = FactoryReplicator.m_MiddleContent.graphCtrl.rootNode;
		if (rootNode.mCtrl.mContentSprites[0].gameObject.activeSelf)
		{
			int itemID = rootNode.GetItemID();
			ItemProto itemData = ItemProto.GetItemData(itemID);
			m_RightGridItem.SetCotent(itemData.icon);
			m_RightGridItem.SetToolTipInfo(ListItemType.mItem, itemID);
		}
		else
		{
			m_RightGridItem.SetCotent(rootNode.mCtrl.mContentTexture.mainTexture);
			m_RightGridItem.SetToolTipInfo(ListItemType.mItem, rootNode.GetItemID());
		}
		FactoryReplicator.m_RightContent.countInput.text = rootNode.ms.m_productItemCount.ToString();
	}

	private int OnCountIputChanged(int count)
	{
		UIGraphControl graphCtrl = FactoryReplicator.m_MiddleContent.graphCtrl;
		int num = count;
		if (graphCtrl.rootNode != null)
		{
			if (num > 9999)
			{
				num = 9999;
			}
			int num2 = num % graphCtrl.rootNode.ms.m_productItemCount;
			if (num2 != 0)
			{
				num = num - num2 + graphCtrl.rootNode.ms.m_productItemCount;
			}
			if (num > 9999)
			{
				num -= graphCtrl.rootNode.ms.m_productItemCount;
			}
			graphCtrl.SetGraphCount(num);
		}
		return num;
	}

	private void OnCompoundBtnClick(GameObject go)
	{
		if (!GameConfig.IsMultiMode)
		{
			UIGraphControl graphCtrl = FactoryReplicator.m_MiddleContent.graphCtrl;
			if (!graphCtrl.isCanCreate())
			{
				return;
			}
			int itemID = graphCtrl.rootNode.GetItemID();
			int getCount = graphCtrl.rootNode.getCount;
			float num = graphCtrl.rootNode.ms.timeNeed * (float)getCount / (float)graphCtrl.rootNode.ms.m_productItemCount;
			if (RandomMapConfig.useSkillTree && GameUI.Instance.mSkillWndCtrl._SkillMgr != null)
			{
				num = GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckReduceTime(num);
			}
			if (!m_Factory.SetCompoudItem(itemID, getCount, num))
			{
				return;
			}
			for (int i = 0; i < graphCtrl.mGraphItemList.Count; i++)
			{
				if (graphCtrl.mGraphItemList[i].mPartent == graphCtrl.rootNode)
				{
					PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.Destroy(graphCtrl.mGraphItemList[i].GetItemID(), graphCtrl.mGraphItemList[i].needCount);
				}
			}
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mJoinCompoudingQueue.GetString(), ItemProto.GetItemData(itemID).GetName()));
			if (GameUI.Instance.mItemPackageCtrl != null)
			{
				GameUI.Instance.mItemPackageCtrl.ResetItem();
			}
			return;
		}
		UIGraphControl graphCtrl2 = FactoryReplicator.m_MiddleContent.graphCtrl;
		if (graphCtrl2.isCanCreate())
		{
			int id = graphCtrl2.rootNode.ms.id;
			int count = graphCtrl2.rootNode.getCount / graphCtrl2.rootNode.ms.m_productItemCount;
			if (m_Factory.Data.m_CompoudItems.Count < 8)
			{
				m_Factory._ColonyObj._Network.FCT_Compoud(id, count);
			}
		}
	}

	private bool ReDrawGraph(int itemID, int scirptIndex = 0)
	{
		if (FactoryReplicator == null)
		{
			return false;
		}
		AddScriptItemData(itemID);
		if (!m_Formulas.ContainsKey(itemID) || scirptIndex >= m_Formulas[itemID].Count || scirptIndex < 0)
		{
			return true;
		}
		UIGraphControl graphCtrl = FactoryReplicator.m_MiddleContent.graphCtrl;
		if (m_RootType != ItemLabel.Root.ISO && itemID != -1)
		{
			Replicator.KnownFormula knownFormula = m_Formulas[itemID][scirptIndex];
			Replicator.Formula formula = knownFormula.Get();
			ItemProto itemProto = m_ItemDataList.Find((ItemProto a) => a.id == itemID);
			if (formula == null || itemProto == null)
			{
				return false;
			}
			FactoryReplicator.ClearGraph();
			int lever_v = 0;
			UIGraphNode uIGraphNode = graphCtrl.AddGraphItem(lever_v, null, formula, itemProto.icon, "Icon");
			uIGraphNode.mTipCtrl.SetToolTipInfo(ListItemType.mItem, itemID);
			uIGraphNode.mCtrl.ItemClick += OnGraphItemClick;
			for (int i = 0; i < formula.materials.Count; i++)
			{
				if (formula.materials[i].itemId != 0)
				{
					ItemProto itemData = ItemProto.GetItemData(formula.materials[i].itemId);
					string[] icon = itemData.icon;
					UIGraphNode uIGraphNode2 = graphCtrl.AddGraphItem(lever_v, uIGraphNode, null, icon, "Icon");
					uIGraphNode2.mTipCtrl.SetToolTipInfo(ListItemType.mItem, formula.materials[i].itemId);
					uIGraphNode2.mCtrl.ItemClick += OnGraphItemClick;
				}
			}
		}
		FactoryReplicator.DrawGraph();
		return true;
	}

	private void OnGraphItemClick(int index)
	{
		if (index == -1)
		{
			return;
		}
		UIGraphControl graphCtrl = FactoryReplicator.m_MiddleContent.graphCtrl;
		int itemID = graphCtrl.mGraphItemList[index].GetItemID();
		if (ReDrawGraph(itemID))
		{
			_setCompundInfo(itemID);
			FactoryReplicator.AddGraphHistory(itemID);
		}
		else
		{
			if (graphCtrl.mSelectedIndex != -1)
			{
				graphCtrl.mGraphItemList[graphCtrl.mSelectedIndex].mCtrl.SetSelected(isSelected: false);
			}
			graphCtrl.mGraphItemList[index].mCtrl.SetSelected(isSelected: true);
			graphCtrl.mSelectedIndex = index;
		}
		_updateQueryGridItems(itemID);
	}

	private void OnGraphUseHistory(object history)
	{
		int num = (int)history;
		ReDrawGraph(num);
		_setCompundInfo(num);
	}

	public void Init()
	{
		FactoryReplicator.m_MenuContent.gridList.itemGoPool.Init();
		m_AllStr = PELocalization.GetString(10055);
	}

	private void Awake()
	{
	}

	private void Start()
	{
		_initCompundGridItems();
		FactoryReplicator.onMenuBtnClick = OnLeftMenuClick;
		FactoryReplicator.onMenueSelect = OnMenuSelectionChange;
		FactoryReplicator.onQuerySearchBtnClick = OnQuerySearchBtnClick;
		FactoryReplicator.onQueryClearBtnClick = OnQueryClearBtnClick;
		FactoryReplicator.onGraphUseHistory = OnGraphUseHistory;
		FactoryReplicator.onCountIputChanged = OnCountIputChanged;
		FactoryReplicator.onCompoundBtnClick = OnCompoundBtnClick;
		for (int i = 0; i < 8; i++)
		{
			_createCompoundItem();
		}
	}

	private void Update()
	{
		if (FactoryReplicator == null || m_Factory == null)
		{
			return;
		}
		UIGraphControl graphCtrl = FactoryReplicator.m_MiddleContent.graphCtrl;
		if (graphCtrl.isCanCreate())
		{
			FactoryReplicator.m_RightContent.compoundBtn.isEnabled = true;
		}
		else
		{
			FactoryReplicator.m_RightContent.compoundBtn.isEnabled = false;
		}
		for (int i = 0; i < m_CompoudItems.Count; i++)
		{
			if (i < m_Factory.CompoudItemsCount)
			{
				ItemProto itemData = ItemProto.GetItemData(m_Factory.Data.m_CompoudItems[i].itemID);
				string[] icon = itemData.icon;
				if (icon.Length != 0)
				{
					m_CompoudItems[i].IcomName = icon[0];
				}
				else
				{
					m_CompoudItems[i].IcomName = string.Empty;
				}
				m_CompoudItems[i].Count = m_Factory.Data.m_CompoudItems[i].itemCnt;
				m_CompoudItems[i].SliderValue = m_Factory.Data.m_CompoudItems[i].curTime / m_Factory.Data.m_CompoudItems[i].time;
				m_CompoudItems[i].ShowSlider = true;
			}
			else
			{
				m_CompoudItems[i].IcomName = "Null";
				m_CompoudItems[i].Count = 0;
				m_CompoudItems[i].SliderValue = 0f;
				m_CompoudItems[i].ShowSlider = false;
			}
		}
	}

	private void OnEnable()
	{
		if (m_Replicator != null)
		{
			m_Replicator.eventor.Subscribe(UpdateLeftListEventHandler);
			UpdateLeftList();
		}
	}

	private void OnDisable()
	{
		if (m_Replicator != null)
		{
			m_Replicator.eventor.Unsubscribe(UpdateLeftListEventHandler);
		}
	}

	public void OnCompoundBtnClickSuccess(int item_id, CSFactory entity)
	{
		if (m_Factory == entity)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mJoinCompoudingQueue.GetString(), ItemProto.GetItemData(item_id).GetName()));
		}
	}

	private void AddScriptItemData(int itemID)
	{
		if (!m_Formulas.ContainsKey(itemID))
		{
			List<Replicator.Formula> list = PeSingleton<Replicator.Formula.Mgr>.Instance.FindAllByProDuctID(itemID);
			if (list == null || list.Count <= 0)
			{
				return;
			}
			List<Replicator.KnownFormula> list2 = new List<Replicator.KnownFormula>();
			for (int i = 0; i < list.Count; i++)
			{
				Replicator.KnownFormula knownFormula = UIGraphControl.GetReplicator().GetKnownFormula(list[i].id);
				if (knownFormula != null)
				{
					list2.Add(knownFormula);
				}
			}
			ItemProto itemData = ItemProto.GetItemData(itemID);
			m_ItemDataList.Add(itemData);
			m_Formulas.Add(itemID, list2);
		}
		if (m_Formulas.ContainsKey(itemID) && itemID != m_CurItemID)
		{
			UpdateCurItemScriptList(itemID);
			SelectFirstScritItem(execEvent: false);
		}
	}

	private void RecoveryScriptItem()
	{
		if (mCurScriptItemList.Count > 0)
		{
			for (int i = 0; i < mCurScriptItemList.Count; i++)
			{
				mCurScriptItemList[i].Reset();
				mCurScriptItemList[i].gameObject.SetActive(value: false);
				mScriptItemPool.Enqueue(mCurScriptItemList[i]);
			}
			mCurScriptItemList.Clear();
		}
	}

	private UIScriptItem_N GetNewScriptItem()
	{
		UIScriptItem_N uIScriptItem_N = null;
		if (mScriptItemPool.Count > 0)
		{
			uIScriptItem_N = mScriptItemPool.Dequeue();
			uIScriptItem_N.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = Object.Instantiate(m_UIScriptItemPrefab.gameObject);
			gameObject.transform.parent = mScriptListParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			uIScriptItem_N = gameObject.GetComponent<UIScriptItem_N>();
		}
		return uIScriptItem_N;
	}

	private void UpdateCurItemScriptList(int itemID)
	{
		m_BackupScriptItem = null;
		RecoveryScriptItem();
		if (!m_Formulas.ContainsKey(itemID))
		{
			return;
		}
		m_CurItemID = itemID;
		int count = m_Formulas[itemID].Count;
		for (int i = 0; i < count; i++)
		{
			UIScriptItem_N item = GetNewScriptItem();
			item.UpdateInfo(itemID, i);
			item.SelectEvent = delegate(UIScriptItem_N scriptItem)
			{
				if (item != m_BackupScriptItem)
				{
					ScriptItemEvent(scriptItem.ItemID, scriptItem.ScriptIndex);
					if (null != m_BackupScriptItem)
					{
						m_BackupScriptItem.CanSelectItem();
					}
					m_BackupScriptItem = item;
				}
			};
			item.transform.localPosition = new Vector3(i * mScriptItemPaddingX, 0f, 0f);
			mCurScriptItemList.Add(item);
			if (count == 1)
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	private void ScriptItemEvent(int itemID, int scriptIndex)
	{
		if (m_Formulas.ContainsKey(itemID) && scriptIndex < m_Formulas[itemID].Count && scriptIndex >= 0)
		{
			Replicator.KnownFormula knownFormula = m_Formulas[itemID][scriptIndex];
			UIGraphControl.GetReplicator()?.SetKnownFormulaFlag(knownFormula.id);
			if (ReDrawGraph(itemID, scriptIndex))
			{
				_updateQueryGridItems(itemID);
				_setCompundInfo(itemID);
				FactoryReplicator.AddGraphHistory(itemID);
			}
			FactoryReplicator.m_MiddleContent.graphScrollBox.Reposition();
		}
	}

	private void SelectFirstScritItem(bool execEvent = true)
	{
		if (mCurScriptItemList.Count > 0)
		{
			mCurScriptItemList[0].SelectItem(execEvent);
			m_BackupScriptItem = mCurScriptItemList[0];
		}
	}
}
