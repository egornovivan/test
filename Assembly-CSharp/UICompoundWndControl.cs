using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UICompoundWndControl : UIBaseWnd
{
	public static Action OnShow;

	public UIPopupList mPopuplist;

	public UIScrollBar mListScrolBar;

	public Vector3 mQueryGridPos = new Vector3(30f, -30f, 0f);

	public GameObject mListBox;

	public GameObject mListItemPrefab;

	public GameObject mListGrid;

	public GameObject mLeftQueryBtn;

	public GameObject mRightQueryBtn;

	public UIInput mQueryInput;

	public UICheckbox ckboxAll;

	public UIGraphControl mGraphCtrl;

	public UIGridItemCtrl mButtomGrid;

	public GameObject mGridItemPrefab;

	public GameObject mQureGridContent;

	public BoxCollider mBtnBackBc;

	public BoxCollider mBtnForwdBc;

	public UIInput mBottomCountLb;

	public UISlider mCompoundSlider;

	public N_ImageButton mBtnCompound;

	public UISprite mBtnCompoundSpr;

	public BoxCollider mBtnClearBc;

	public UISlicedSprite mBtnClearSp;

	public UILabel mLbGraphInfo;

	public Transform mScriptListParent;

	public UIScriptItem_N m_UIScriptItemPrefab;

	public int mScriptItemPaddingX = 30;

	public UIEfficientGrid m_LeftList;

	public UICheckbox ckItemTrack;

	[SerializeField]
	private GameObject m_OpComponentParent;

	[SerializeField]
	private GameObject m_ProgressParent;

	[SerializeField]
	private UILabel m_ProgressLbl;

	[SerializeField]
	private UILabel m_AddToPackageLbl;

	private int CompoundFixedTimeCount;

	private Queue<UIScriptItem_N> mScriptItemPool = new Queue<UIScriptItem_N>();

	private List<UIScriptItem_N> mCurScriptItemList = new List<UIScriptItem_N>();

	private List<int> mGraphResetList = new List<int>();

	private int mGraphResetListIndex = -1;

	private int mListSelectedIndex = -1;

	public UIEfficientGrid m_QueryList;

	private int mQueryListFristIndex;

	private string m_AllStr = string.Empty;

	private ItemLabel.Root mRootType;

	private int mItemType;

	private AudioController m_CompoundAudioCtrl;

	private Replicator mReplicator;

	private bool isActiveClearBtn = true;

	private float mListBoxPos_Z;

	private List<Replicator.Formula> m_QueryFormula = new List<Replicator.Formula>();

	private Dictionary<int, List<Replicator.KnownFormula>> m_Formulas = new Dictionary<int, List<Replicator.KnownFormula>>();

	private List<ItemProto> m_ItemDataList = new List<ItemProto>();

	public bool _updateLeftList = true;

	private bool LockChange;

	private bool IsMoveLeft = true;

	private bool IsMoveQureyGridContentPos;

	private float mQureGridContentPos_x;

	private UIScriptItem_N m_BackupScriptItem;

	private int m_CurItemID;

	private List<int> m_CurScriptMatIDs = new List<int>();

	public bool IsCompounding
	{
		get
		{
			return replicator != null && null != replicator.runningReplicate;
		}
		set
		{
			UpdateComponentState();
			if (value && isShow)
			{
				GameUI.Instance.PlayCompoundAudioEffect();
			}
			else
			{
				GameUI.Instance.StopCompoundAudioEffect();
			}
		}
	}

	private Replicator replicator
	{
		get
		{
			if (mReplicator == null && null != PeSingleton<MainPlayer>.Instance.entity && null != PeSingleton<MainPlayer>.Instance.entity.replicatorCmpt)
			{
				mReplicator = PeSingleton<MainPlayer>.Instance.entity.replicatorCmpt.replicator;
				if (mReplicator != null)
				{
					Replicator obj = mReplicator;
					obj.onReplicateEnd = (Action)Delegate.Combine(obj.onReplicateEnd, new Action(OnEndRepulicate));
				}
			}
			return mReplicator;
		}
	}

	private void Awake()
	{
		mQureGridContentPos_x = mQureGridContent.transform.localPosition.x;
		UIEventListener.Get(mBottomCountLb.gameObject).onSelect = OnCountInputSelected;
	}

	private void Start()
	{
		InitWindow();
		m_AllStr = PELocalization.GetString(10055);
		mRootType = ItemLabel.Root.all;
		AfterLeftMeunChecked();
	}

	private void Update()
	{
		ChangemListBoxPos_Z();
		if (IsMoveQureyGridContentPos)
		{
			MoveQureyGridContentPos();
		}
	}

	private new void LateUpdate()
	{
		_updateLeftList = true;
		UpdatePoogressBar();
		UpdateCompundBtnState();
		UpdateClearBtnState();
	}

	public override void Show()
	{
		UpdateLeftList();
		mCompoundSlider.sliderValue = 0f;
		replicator.eventor.Subscribe(UpdateLeftListEventHandler);
		base.Show();
		if (OnShow != null)
		{
			OnShow();
		}
		if (IsCompounding)
		{
			GameUI.Instance.PlayCompoundAudioEffect();
			Replicator.RunningReplicate runningReplicate = replicator.runningReplicate;
			if (runningReplicate != null)
			{
				UpdateCurItemScriptList(runningReplicate.formula.productItemId);
				if (m_Formulas.ContainsKey(runningReplicate.formula.productItemId))
				{
					List<Replicator.KnownFormula> list = m_Formulas[runningReplicate.formula.productItemId];
					for (int i = 0; i < list.Count; i++)
					{
						if (runningReplicate.formulaID == list[i].id && mCurScriptItemList.Count > i)
						{
							mCurScriptItemList[i].SelectItem();
							m_BackupScriptItem = mCurScriptItemList[i];
							UpdateCompoundCount(runningReplicate.leftCount * runningReplicate.formula.m_productItemCount);
							break;
						}
					}
				}
				IsCompounding = true;
			}
		}
		UIItemsTrackCtrl mItemsTrackWnd = GameUI.Instance.mItemsTrackWnd;
		mItemsTrackWnd.ScriptTrackChanged = (Action<int, bool>)Delegate.Combine(mItemsTrackWnd.ScriptTrackChanged, new Action<int, bool>(OnScriptTrackChanged));
	}

	protected override void OnClose()
	{
		replicator.eventor.Unsubscribe(UpdateLeftListEventHandler);
		UIItemsTrackCtrl mItemsTrackWnd = GameUI.Instance.mItemsTrackWnd;
		mItemsTrackWnd.ScriptTrackChanged = (Action<int, bool>)Delegate.Remove(mItemsTrackWnd.ScriptTrackChanged, new Action<int, bool>(OnScriptTrackChanged));
		base.OnClose();
	}

	protected override void OnHide()
	{
		base.OnHide();
		GameUI.Instance.StopCompoundAudioEffect();
	}

	public override void OnCreate()
	{
		base.OnCreate();
		m_LeftList.itemGoPool.Init();
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		base.SelfWndType = UIEnum.WndType.Compound;
	}

	private void UpdateLeftListEventHandler(object sender, Replicator.EventArg e)
	{
		_updateLeftList = true;
		UpdateLeftList();
	}

	private void UpdateCompundBtnState()
	{
		if (mGraphCtrl.rootNode == null)
		{
			EnableBtnCompound(enable: false);
			return;
		}
		bool flag = false;
		if (mGraphCtrl.isCanCreate())
		{
			flag = mGraphCtrl.rootNode.ms.workSpace == 0;
		}
		EnableBtnCompound(IsCompounding || flag);
	}

	private void EnableBtnCompound(bool enable)
	{
		if (enable && !mBtnCompound.isEnabled)
		{
			mBtnCompound.isEnabled = true;
		}
		else if (!enable && mBtnCompound.isEnabled)
		{
			mBtnCompound.isEnabled = false;
		}
	}

	private void UpdateClearBtnState()
	{
		if (!isActiveClearBtn && mQueryInput.text.Length > 0)
		{
			mBtnClearBc.enabled = true;
			mBtnClearSp.color = new Color(1f, 1f, 1f, 1f);
			isActiveClearBtn = true;
		}
		else if (isActiveClearBtn && mQueryInput.text.Length == 0)
		{
			mBtnClearBc.enabled = false;
			mBtnClearSp.color = new Color(0.6f, 0.6f, 0.6f, 1f);
			isActiveClearBtn = false;
		}
	}

	private void UpdatePoogressBar()
	{
		if (!IsCompounding)
		{
			return;
		}
		if (mGraphCtrl.rootNode == null)
		{
			OnEndRepulicate();
			return;
		}
		Replicator.RunningReplicate runningReplicate = replicator.runningReplicate;
		if (runningReplicate != null)
		{
			mCompoundSlider.sliderValue = runningReplicate.runningTime / runningReplicate.formula.timeNeed;
			float num = runningReplicate.requestCount * runningReplicate.formula.m_productItemCount;
			float num2 = runningReplicate.leftCount * runningReplicate.formula.m_productItemCount;
			float num3 = runningReplicate.finishCount * runningReplicate.formula.m_productItemCount;
			m_ProgressLbl.text = $"{num - num2}/{num}";
			m_AddToPackageLbl.text = PELocalization.GetString(8000690) + (num - num2 - num3);
		}
	}

	private void GetCompoundItem()
	{
		Replicator replicator = this.replicator;
		if (!replicator.HasEnoughPackage(mGraphCtrl.rootNode.GetItemID(), mGraphCtrl.rootNode.getCount))
		{
			IsCompounding = false;
			CompoundFixedTimeCount = 0;
			mCompoundSlider.sliderValue = 0f;
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000050));
			return;
		}
		if (mGraphCtrl.isCanCreate())
		{
			if (GameConfig.IsMultiMode)
			{
				int mCurrentNum = mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount;
				PlayerNetwork.mainPlayer.RequestMergeSkill(mGraphCtrl.rootNode.ms.id, mCurrentNum);
			}
			else
			{
				replicator.Run(mGraphCtrl.rootNode.ms.id, mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount);
			}
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			for (int i = 0; i < m_CurScriptMatIDs.Count; i++)
			{
				MissionManager.Instance.ProcessCollectMissionByID(m_CurScriptMatIDs[i]);
			}
		}
		IsCompounding = false;
		CompoundFixedTimeCount = 0;
		mCompoundSlider.sliderValue = 0f;
	}

	private void ChangemListBoxPos_Z()
	{
		if (mPopuplist.isOpen)
		{
			mListBoxPos_Z = 2f;
		}
		else
		{
			mListBoxPos_Z = 0f;
		}
		if (mListBox.transform.localPosition.z != mListBoxPos_Z)
		{
			Vector3 localPosition = mListBox.transform.localPosition;
			localPosition.z = mListBoxPos_Z;
			mListBox.transform.localPosition = localPosition;
		}
	}

	private void QueryGridItems(int m_id)
	{
		mQureGridContentPos_x = 30f;
		mQueryListFristIndex = 0;
		mQureGridContent.transform.localPosition = mQueryGridPos;
		m_QueryFormula.Clear();
		Replicator replicator = this.replicator;
		if (replicator != null)
		{
			foreach (Replicator.KnownFormula knowFormula in replicator.knowFormulas)
			{
				if (knowFormula == null)
				{
					continue;
				}
				Replicator.Formula formula = knowFormula.Get();
				if (formula == null || formula.materials == null || formula.materials.Count <= 0)
				{
					continue;
				}
				for (int i = 0; i < formula.materials.Count; i++)
				{
					if (formula.materials[i].itemId == m_id)
					{
						m_QueryFormula.Add(formula);
						break;
					}
				}
			}
		}
		m_QueryList.UpdateList(m_QueryFormula.Count, SetQueryListContent, ClearQueryListContent);
		UpdateQueryGridBtnState();
	}

	private bool ReDrawGraph(int itemID, int scirptIndex = 0)
	{
		if (mGraphCtrl == null)
		{
			return false;
		}
		AddScriptItemData(itemID);
		if (!m_Formulas.ContainsKey(itemID) || scirptIndex >= m_Formulas[itemID].Count || scirptIndex < 0)
		{
			return true;
		}
		if (mRootType != ItemLabel.Root.ISO)
		{
			Replicator.KnownFormula knownFormula = m_Formulas[itemID][scirptIndex];
			Replicator.Formula formula = knownFormula.Get();
			ItemProto itemProto = m_ItemDataList.Find((ItemProto a) => a.id == itemID);
			if (formula == null || itemProto == null)
			{
				return false;
			}
			bool flag = ((formula.workSpace != 0) ? true : false);
			mLbGraphInfo.enabled = flag;
			mLbGraphInfo.text = ((!flag) ? string.Empty : PELocalization.GetString(8000151));
			mGraphCtrl.ClearGraph();
			int lever_v = 0;
			UIGraphNode uIGraphNode = mGraphCtrl.AddGraphItem(lever_v, null, formula, itemProto.icon, "Icon");
			uIGraphNode.mTipCtrl.SetToolTipInfo(ListItemType.mItem, itemID);
			uIGraphNode.mCtrl.ItemClick += GraphItemOnClick;
			m_CurScriptMatIDs.Clear();
			for (int i = 0; i < formula.materials.Count; i++)
			{
				if (formula.materials[i].itemId != 0)
				{
					m_CurScriptMatIDs.Add(formula.materials[i].itemId);
					ItemProto itemData = ItemProto.GetItemData(formula.materials[i].itemId);
					UIGraphNode uIGraphNode2 = mGraphCtrl.AddGraphItem(lever_v, uIGraphNode, null, itemData.icon, "Icon");
					uIGraphNode2.mTipCtrl.SetToolTipInfo(ListItemType.mItem, formula.materials[i].itemId);
					uIGraphNode2.mCtrl.ItemClick += GraphItemOnClick;
				}
			}
			UpdateItemsTrackState(formula);
		}
		mGraphCtrl.DrawGraph();
		return true;
	}

	private void UpdateComponentState()
	{
		if (IsCompounding)
		{
			mBtnCompoundSpr.spriteName = "Craft3";
			m_OpComponentParent.SetActive(value: false);
			m_ProgressParent.SetActive(value: true);
		}
		else
		{
			mBtnCompoundSpr.spriteName = "Craft1";
			m_OpComponentParent.SetActive(value: true);
			m_ProgressParent.SetActive(value: false);
		}
	}

	private void SetQueryListContent(int index, GameObject go)
	{
		UIGridItemCtrl component = go.GetComponent<UIGridItemCtrl>();
		component.mIndex = index;
		component.SetToolTipInfo(ListItemType.mItem, m_QueryFormula[index].productItemId);
		component.mItemClick -= GridListItemOnClick;
		component.mItemClick += GridListItemOnClick;
		ItemProto itemData = ItemProto.GetItemData(m_QueryFormula[index].productItemId);
		component.SetCotent(itemData.icon);
	}

	private void ClearQueryListContent(GameObject go)
	{
		UIGridItemCtrl component = go.GetComponent<UIGridItemCtrl>();
		if (!(component == null))
		{
			component.mItemClick -= ListItemOnClick;
		}
	}

	private void SetLeftListContent(int index, GameObject go)
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
			component.SetItem(itemProto.GetName(), itemProto.id, bNew, itemProto.icon, itemProto.GetName(), index, ListItemType.mItem);
			component.SetSelectmState(isSelected: false);
			component.SetTextColor(Color.white);
			component.mItemClick -= ListItemOnClick;
			component.mItemClick += ListItemOnClick;
		}
	}

	private void ClearLeftListContent(GameObject go)
	{
		UICompoundWndListItem component = go.GetComponent<UICompoundWndListItem>();
		if (!(component == null))
		{
			component.mItemClick -= ListItemOnClick;
		}
	}

	private void NewClearLeftListContent(GameObject go)
	{
		UICompoundWndListItem component = go.GetComponent<UICompoundWndListItem>();
		if (!(component == null))
		{
			component.mItemClick -= ListItemOnClick;
		}
	}

	public void UpdateLeftList(bool useSearch = false)
	{
		if (!_updateLeftList)
		{
			return;
		}
		_updateLeftList = false;
		mListSelectedIndex = -1;
		string text = mQueryInput.text;
		text = text.Replace("*", string.Empty);
		text = text.Replace("$", string.Empty);
		text = text.Replace("(", string.Empty);
		text = text.Replace(")", string.Empty);
		text = text.Replace("@", string.Empty);
		text = text.Replace("^", string.Empty);
		text = text.Replace("[", string.Empty);
		text = text.Replace("]", string.Empty);
		text = text.Replace("\\", string.Empty);
		mQueryInput.text = text;
		m_Formulas.Clear();
		m_ItemDataList.Clear();
		Replicator replicator = this.replicator;
		Dictionary<ItemProto, List<Replicator.KnownFormula>> dictionary = new Dictionary<ItemProto, List<Replicator.KnownFormula>>();
		foreach (Replicator.KnownFormula kf in replicator.knowFormulas)
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
			if (mRootType != ItemLabel.Root.ISO && (mRootType == ItemLabel.Root.all || (mRootType == item.rootItemLabel && (mItemType == 0 || mItemType == item.itemLabel))))
			{
				if (useSearch)
				{
					if (QueryItem(text, item.GetName()))
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
		m_LeftList.UpdateList(m_ItemDataList.Count, SetLeftListContent, ClearLeftListContent);
		mListScrolBar.scrollValue = 0f;
	}

	private void OnSelectionChange1(string selectedItemName)
	{
		if (!LockChange)
		{
			if (selectedItemName == m_AllStr)
			{
				mItemType = 0;
			}
			else
			{
				mItemType = ItemLabel.GetItemTypeByName(selectedItemName);
			}
			UpdateLeftList();
		}
	}

	private void SetPopuplistItem(bool useScerch)
	{
		LockChange = true;
		mPopuplist.items.Clear();
		mPopuplist.items.Add(m_AllStr);
		if (mRootType != 0)
		{
			mPopuplist.items.AddRange(ItemLabel.GetDirectChildrenName((int)mRootType));
		}
		if (mPopuplist.items.Count > 0)
		{
			mPopuplist.selection = mPopuplist.items[0];
		}
		mItemType = 0;
		UpdateLeftList(useScerch);
		LockChange = false;
	}

	private void AfterLeftMeunChecked(bool useScerch = false)
	{
		SetPopuplistItem(useScerch);
	}

	private void MoveQureyGridContentPos()
	{
		float x = mQureGridContent.transform.localPosition.x;
		if (IsMoveLeft)
		{
			if (mQureGridContentPos_x < x)
			{
				mQureGridContent.transform.localPosition = Vector3.Lerp(mQureGridContent.transform.localPosition, new Vector3(mQureGridContentPos_x, -30f, 0f), 0.3f);
				if (x - mQureGridContentPos_x < 3f)
				{
					mQureGridContent.transform.localPosition = new Vector3(mQureGridContentPos_x, -30f, 0f);
				}
			}
			else
			{
				IsMoveQureyGridContentPos = false;
			}
		}
		else if (mQureGridContentPos_x > x)
		{
			mQureGridContent.transform.localPosition = Vector3.Lerp(mQureGridContent.transform.localPosition, new Vector3(mQureGridContentPos_x, -30f, 0f), 0.3f);
			if (mQureGridContentPos_x - x < 3f)
			{
				mQureGridContent.transform.localPosition = new Vector3(mQureGridContentPos_x, -30f, 0f);
			}
		}
		else
		{
			IsMoveQureyGridContentPos = false;
		}
		m_QueryList.repositionVisibleNow = true;
	}

	private void UpdateQueryGridBtnState()
	{
		if (null == m_QueryList || m_QueryList.Gos == null)
		{
			return;
		}
		BoxCollider component = mLeftQueryBtn.GetComponent<BoxCollider>();
		BoxCollider component2 = mRightQueryBtn.GetComponent<BoxCollider>();
		UISlicedSprite componentInChildren = mLeftQueryBtn.GetComponentInChildren<UISlicedSprite>();
		UISlicedSprite componentInChildren2 = mRightQueryBtn.GetComponentInChildren<UISlicedSprite>();
		if (!(null == component) && !(null == component2) && !(null == componentInChildren) && !(null == componentInChildren2))
		{
			int count = m_QueryList.Gos.Count;
			if (count <= 10)
			{
				component.enabled = false;
				componentInChildren.color = new Color(0.6f, 0.6f, 0.6f, 1f);
				component2.enabled = false;
				componentInChildren2.color = new Color(0.6f, 0.6f, 0.6f, 1f);
			}
			else if (mQueryListFristIndex == 0)
			{
				component.enabled = false;
				componentInChildren.color = new Color(0.6f, 0.6f, 0.6f, 1f);
				component2.enabled = true;
				componentInChildren2.color = new Color(1f, 1f, 1f, 1f);
			}
			else if (count - mQueryListFristIndex > 0 && count - mQueryListFristIndex <= 10)
			{
				component.enabled = true;
				componentInChildren.color = new Color(1f, 1f, 1f, 1f);
				component2.enabled = false;
				componentInChildren2.color = new Color(0.6f, 0.6f, 0.6f, 1f);
			}
			else
			{
				component.enabled = true;
				componentInChildren.color = new Color(1f, 1f, 1f, 1f);
				component2.enabled = true;
				componentInChildren2.color = new Color(1f, 1f, 1f, 1f);
			}
		}
	}

	private void SetListBtnActive(bool isActive)
	{
		mLeftQueryBtn.SetActive(isActive);
		mRightQueryBtn.SetActive(isActive);
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

	private void SetBottomInfo()
	{
		UIGraphNode rootNode = mGraphCtrl.rootNode;
		if (rootNode.mCtrl.mContentSprites[0].gameObject.activeSelf)
		{
			int itemID = rootNode.GetItemID();
			ItemProto itemData = ItemProto.GetItemData(itemID);
			mButtomGrid.SetCotent(itemData.icon);
			mButtomGrid.SetToolTipInfo(ListItemType.mItem, itemID);
		}
		else
		{
			mButtomGrid.SetCotent(rootNode.mCtrl.mContentTexture.mainTexture);
			mButtomGrid.SetToolTipInfo(ListItemType.mItem, rootNode.GetItemID());
		}
		mBottomCountLb.text = rootNode.ms.m_productItemCount.ToString();
	}

	private void AddGraphResetList(int m_id)
	{
		if (mGraphResetList.Count > 10)
		{
			mGraphResetList.RemoveAt(0);
		}
		mGraphResetListIndex = mGraphResetList.Count;
		mGraphResetList.Add(m_id);
		if (mGraphResetList.Count > 1)
		{
			mBtnBackBc.enabled = true;
			mBtnForwdBc.enabled = false;
		}
	}

	private void OnScriptTrackChanged(int scriptID, bool add)
	{
		if (mGraphCtrl.rootNode != null && mGraphCtrl.rootNode.ms != null && mGraphCtrl.rootNode.ms.id == scriptID)
		{
			ckItemTrack.isChecked = add;
		}
	}

	private void UpdateItemsTrackState(Replicator.Formula ms)
	{
		bool isChecked = GameUI.Instance.mItemsTrackWnd.ContainsScript(ms.id);
		ckItemTrack.isChecked = isChecked;
	}

	private void OnItemTrackCk(bool isChecked)
	{
		if (mGraphCtrl.rootNode != null && mGraphCtrl.rootNode.ms != null)
		{
			if (isChecked)
			{
				int multiple = mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount;
				GameUI.Instance.mItemsTrackWnd.UpdateOrAddScript(mGraphCtrl.rootNode.ms, multiple);
			}
			else
			{
				GameUI.Instance.mItemsTrackWnd.RemoveScript(mGraphCtrl.rootNode.ms.id);
			}
		}
	}

	private void Ck0AllOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != 0)
		{
			mRootType = ItemLabel.Root.all;
			AfterLeftMeunChecked();
		}
	}

	private void Ck1WeaponOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.weapon)
		{
			mRootType = ItemLabel.Root.weapon;
			AfterLeftMeunChecked();
		}
	}

	private void Ck2EquipmentOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.equipment)
		{
			mRootType = ItemLabel.Root.equipment;
			AfterLeftMeunChecked();
		}
	}

	private void Ck3ToolOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.tool)
		{
			mRootType = ItemLabel.Root.tool;
			AfterLeftMeunChecked();
		}
	}

	private void Ck4TurretOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.turret)
		{
			mRootType = ItemLabel.Root.turret;
			AfterLeftMeunChecked();
		}
	}

	private void Ck5ConsumablesOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.consumables)
		{
			mRootType = ItemLabel.Root.consumables;
			AfterLeftMeunChecked();
		}
	}

	private void Ck6ResoureOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.resoure)
		{
			mRootType = ItemLabel.Root.resoure;
			AfterLeftMeunChecked();
		}
	}

	private void Ck7PartOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.part)
		{
			mRootType = ItemLabel.Root.part;
			AfterLeftMeunChecked();
		}
	}

	private void Ck8DecorationOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.decoration)
		{
			mRootType = ItemLabel.Root.decoration;
			AfterLeftMeunChecked();
		}
	}

	private void Ck9IsoOnClick()
	{
		if (Input.GetMouseButtonUp(0) && mRootType != ItemLabel.Root.ISO)
		{
			mRootType = ItemLabel.Root.ISO;
			AfterLeftMeunChecked();
		}
	}

	private void ListItemOnClick(int index)
	{
		if (!IsCompounding && index >= 0 && index < m_ItemDataList.Count)
		{
			List<GameObject> gos = m_LeftList.Gos;
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

	private void NewListItemOnClick(int index)
	{
	}

	private void GraphItemOnClick(int index)
	{
		if (index == -1 || IsCompounding)
		{
			return;
		}
		int itemID = mGraphCtrl.mGraphItemList[index].GetItemID();
		if (ReDrawGraph(itemID))
		{
			AddGraphResetList(itemID);
			SetBottomInfo();
		}
		else
		{
			if (mGraphCtrl.mSelectedIndex != -1)
			{
				mGraphCtrl.mGraphItemList[mGraphCtrl.mSelectedIndex].mCtrl.SetSelected(isSelected: false);
			}
			mGraphCtrl.mGraphItemList[index].mCtrl.SetSelected(isSelected: true);
			mGraphCtrl.mSelectedIndex = index;
		}
		QueryGridItems(itemID);
	}

	private void OnQueryBtnOnClick()
	{
		string text = mQueryInput.text;
		if (text.Length > 0)
		{
			AfterLeftMeunChecked(useScerch: true);
			return;
		}
		mQueryInput.text = string.Empty;
		UpdateLeftList();
	}

	private void ClearBtnOnClick()
	{
		if (mQueryInput.text.Length > 0)
		{
			mQueryInput.text = string.Empty;
			UpdateLeftList();
		}
	}

	private void BtnLeftOnClick()
	{
		if (m_QueryList.Gos.Count > 10 && !IsMoveQureyGridContentPos)
		{
			mQureGridContentPos_x += 520f;
			IsMoveLeft = false;
			mQueryListFristIndex -= 10;
			IsMoveQureyGridContentPos = true;
			UpdateQueryGridBtnState();
		}
	}

	private void BtnRightOnClick()
	{
		if (m_QueryList.Gos.Count > 1 && !IsMoveQureyGridContentPos)
		{
			mQureGridContentPos_x -= 520f;
			IsMoveLeft = true;
			mQueryListFristIndex += 10;
			IsMoveQureyGridContentPos = true;
			UpdateQueryGridBtnState();
		}
	}

	private void GridListItemOnClick(int index)
	{
		if (index != -1 && !IsCompounding)
		{
			int mItemId = m_QueryList.Gos[index].GetComponent<UIGridItemCtrl>().mItemId;
			if (ReDrawGraph(mItemId))
			{
				AddGraphResetList(mItemId);
				SetBottomInfo();
				QueryGridItems(mItemId);
			}
		}
	}

	private void GraphBtnBackOnClick()
	{
		if (!IsCompounding && mGraphResetListIndex > 0)
		{
			mGraphResetListIndex--;
			ReDrawGraph(mGraphResetList[mGraphResetListIndex]);
			if (mGraphResetListIndex == 0)
			{
				mBtnBackBc.enabled = false;
			}
			mBtnForwdBc.enabled = true;
		}
	}

	private void GraphBtnForwdOnClick()
	{
		if (!IsCompounding && mGraphResetListIndex >= 0 && mGraphResetListIndex < mGraphResetList.Count - 1)
		{
			mGraphResetListIndex++;
			ReDrawGraph(mGraphResetList[mGraphResetListIndex]);
			if (mGraphResetListIndex == mGraphResetList.Count - 1)
			{
				mBtnForwdBc.enabled = false;
			}
			mBtnBackBc.enabled = true;
		}
	}

	private void BtnAddOnClick()
	{
		if (IsCompounding || mGraphCtrl.rootNode == null || !IsNumber(mBottomCountLb.text))
		{
			return;
		}
		int num = Convert.ToInt32(mBottomCountLb.text);
		if (num < mGraphCtrl.GetMaxCount())
		{
			if (num + mGraphCtrl.rootNode.ms.m_productItemCount <= mGraphCtrl.GetMaxCount())
			{
				num += mGraphCtrl.rootNode.ms.m_productItemCount;
			}
			UpdateCompoundCount(num);
		}
	}

	private void OnSubstractBtnClick()
	{
		if (!IsCompounding && mGraphCtrl.rootNode != null && IsNumber(mBottomCountLb.text))
		{
			int num = Convert.ToInt32(mBottomCountLb.text);
			if (num > mGraphCtrl.rootNode.ms.m_productItemCount)
			{
				num -= mGraphCtrl.rootNode.ms.m_productItemCount;
			}
			UpdateCompoundCount(num);
		}
	}

	private void OnEndRepulicate()
	{
		IsCompounding = false;
		CompoundFixedTimeCount = 0;
		mCompoundSlider.sliderValue = 0f;
		m_ProgressLbl.text = string.Empty;
		m_AddToPackageLbl.text = string.Empty;
	}

	private void OnCountInputSelected(GameObject go, bool isSelect)
	{
		if (!isSelect && !IsCompounding && mGraphCtrl.rootNode != null && IsNumber(mBottomCountLb.text))
		{
			string text = mBottomCountLb.text;
			if (text.Trim().Length == 0)
			{
				mBottomCountLb.text = string.Empty;
				return;
			}
			int value = Convert.ToInt32(text);
			value = Mathf.Clamp(value, mGraphCtrl.rootNode.ms.m_productItemCount, mGraphCtrl.GetMaxCount());
			UpdateCompoundCount(value, immediateUpdateInputTxet: false);
		}
	}

	private void BtnMaxOnClick()
	{
		if (!IsCompounding && mGraphCtrl.rootNode != null)
		{
			UpdateCompoundCount(mGraphCtrl.GetMaxCount());
		}
	}

	private void BtnMinOnClick()
	{
		if (!IsCompounding && mGraphCtrl.rootNode != null)
		{
			UpdateCompoundCount(mGraphCtrl.GetMinCount());
		}
	}

	private void UpdateCompoundCount(int count, bool immediateUpdateInputTxet = true)
	{
		if (mGraphCtrl.rootNode != null)
		{
			if (count > 9999)
			{
				count = 9999;
			}
			int num = count % mGraphCtrl.rootNode.ms.m_productItemCount;
			if (num != 0)
			{
				count = count - num + mGraphCtrl.rootNode.ms.m_productItemCount;
			}
			if (count > 9999)
			{
				count -= mGraphCtrl.rootNode.ms.m_productItemCount;
			}
			mGraphCtrl.SetGraphCount(count);
			if (immediateUpdateInputTxet)
			{
				mBottomCountLb.text = count.ToString();
			}
			else
			{
				StartCoroutine(WiatUpdateInputIterator(count));
			}
		}
	}

	private IEnumerator WiatUpdateInputIterator(int count)
	{
		yield return null;
		mBottomCountLb.text = count.ToString();
	}

	private void BtnCompoundOnClick()
	{
		if (mGraphCtrl.rootNode == null)
		{
			return;
		}
		Replicator replicator = this.replicator;
		if (replicator == null)
		{
			return;
		}
		if (IsCompounding)
		{
			IsCompounding = !this.replicator.CancelReplicate(mGraphCtrl.rootNode.ms.id);
		}
		else if (!replicator.HasEnoughPackage(mGraphCtrl.rootNode.GetItemID(), mGraphCtrl.rootNode.getCount))
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000050));
		}
		else if (mGraphCtrl.isCanCreate())
		{
			CompoundFixedTimeCount = Convert.ToInt32((double)mGraphCtrl.rootNode.ms.timeNeed / 0.02);
			if (RandomMapConfig.useSkillTree && GameUI.Instance.mSkillWndCtrl._SkillMgr != null)
			{
				CompoundFixedTimeCount = (int)GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckReduceTime(CompoundFixedTimeCount);
			}
			this.replicator.StartReplicate(mGraphCtrl.rootNode.ms.id, mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount);
			IsCompounding = true;
		}
	}

	public bool IsNumber(string strNumber)
	{
		Regex regex = new Regex("[^0-9.-]");
		Regex regex2 = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
		Regex regex3 = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
		string text = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
		string text2 = "^([-]|[0-9])[0-9]*$";
		Regex regex4 = new Regex("(" + text + ")|(" + text2 + ")");
		return !regex.IsMatch(strNumber) && !regex2.IsMatch(strNumber) && !regex3.IsMatch(strNumber) && regex4.IsMatch(strNumber);
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

	private void ScriptItemEvent(int itemID, int scriptIndex)
	{
		if (m_Formulas.ContainsKey(itemID) && scriptIndex < m_Formulas[itemID].Count && scriptIndex >= 0)
		{
			Replicator.KnownFormula knownFormula = m_Formulas[itemID][scriptIndex];
			replicator?.SetKnownFormulaFlag(knownFormula.id);
			List<GameObject> gos = m_LeftList.Gos;
			if (mListSelectedIndex >= 0 && mListSelectedIndex < gos.Count)
			{
				Replicator.Formula formula = knownFormula.Get();
				Color textColor = ((formula.workSpace == 0 || 1 == 0) ? Color.white : Color.red);
				UICompoundWndListItem component = gos[mListSelectedIndex].GetComponent<UICompoundWndListItem>();
				component.SetTextColor(textColor);
			}
			if (ReDrawGraph(itemID, scriptIndex))
			{
				AddGraphResetList(itemID);
				SetBottomInfo();
				QueryGridItems(itemID);
			}
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
			GameObject gameObject = UnityEngine.Object.Instantiate(m_UIScriptItemPrefab.gameObject);
			gameObject.transform.parent = mScriptListParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			uIScriptItem_N = gameObject.GetComponent<UIScriptItem_N>();
		}
		return uIScriptItem_N;
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
				Replicator.KnownFormula knownFormula = replicator.GetKnownFormula(list[i].id);
				if (knownFormula == null && list[i].id != 193 && list[i].id != 520)
				{
					replicator.AddFormula(list[i].id);
					knownFormula = replicator.GetKnownFormula(list[i].id);
				}
				if (knownFormula != null)
				{
					list2.Add(knownFormula);
				}
			}
			ItemProto itemData = ItemProto.GetItemData(itemID);
			m_ItemDataList.Add(itemData);
			m_Formulas[itemID] = list2;
		}
		if (m_Formulas.ContainsKey(itemID) && itemID != m_CurItemID)
		{
			UpdateCurItemScriptList(itemID);
			SelectFirstScritItem(execEvent: false);
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
