using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_TradingPost : MonoBehaviour
{
	private const int m_Row = 6;

	private const int m_Column = 13;

	[SerializeField]
	private UISlicedSprite m_SpriteMeat;

	[SerializeField]
	private UISlicedSprite m_SpriteMoney;

	[SerializeField]
	private UILabel m_LbCurrency;

	[SerializeField]
	private Grid_N m_GridPrefab;

	[SerializeField]
	private UIGrid m_ItemGrid;

	[SerializeField]
	private UIPanel mSellOpLayer;

	[SerializeField]
	private Grid_N m_CurOpGrid;

	[SerializeField]
	private UIInput m_InputOpNum;

	[SerializeField]
	private UILabel m_LbTotal;

	[SerializeField]
	private UILabel m_LbPrice;

	[SerializeField]
	private UILabel m_BtnOp;

	[SerializeField]
	private GameObject m_MeshGo;

	[SerializeField]
	private UILabel m_LbPage;

	public Action RequestRefreshUIEvent;

	public Action<int, int> BuyItemEvent;

	public Action<int, int> RepurchaseItemEvent;

	public Action<int, int> SellItemEvent;

	private CSUI_LeftMenuItem m_CurMenuItem;

	private bool m_CanUseThis;

	private bool m_NewUseState;

	private int m_PageCount;

	private List<Grid_N> m_ItemGridList;

	private List<ItemObject> m_BuyItemList;

	private List<ItemObject> m_RepurchaseList;

	private int m_Currency;

	private int m_CurPageIndex;

	private int m_CurPackTab;

	private ItemObject m_CurOpItem;

	private ItemLabel.Root m_CurType;

	private int m_CurPrice;

	private bool m_IsBuy;

	private int m_MaxPage;

	private float m_OpDurNum;

	private float m_OpStarTime;

	private float m_LastOpTime;

	private float m_CurrentNum;

	private bool mAddBtnPress;

	private bool mSubBtnPress;

	private bool m_Init;

	private Vector3 m_PlayerPackagePos;

	private Vector3 m_ColonyPos;

	public bool IsShow => base.gameObject.activeInHierarchy;

	public bool IsShowAndCanUse => IsShow && m_CanUseThis;

	public int CurPackTab => m_CurPackTab;

	public bool CanUseThis => m_CanUseThis;

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		if (RequestRefreshUIEvent != null)
		{
			RequestRefreshUIEvent();
		}
		TryOpenPlayerPackagPanel();
	}

	private void OnDisable()
	{
		TryClosePlayerPackagePanel();
	}

	private void Update()
	{
		if (null != m_CurMenuItem)
		{
			m_NewUseState = !m_CurMenuItem.AssemblyLevelInsufficient && !m_CurMenuItem.NotHaveAssembly && !m_CurMenuItem.NotHaveElectricity && CSUI_MainWndCtrl.IsWorking(bShowMsg: false);
			if (m_NewUseState != m_CanUseThis)
			{
				m_CanUseThis = m_NewUseState;
				ShowMeshGo(!m_CanUseThis);
			}
		}
		if (m_CurOpItem == null)
		{
			return;
		}
		if (mAddBtnPress)
		{
			float num = Time.time - m_OpStarTime;
			if (num < 0.2f)
			{
				m_OpDurNum = 1f;
			}
			else if (num < 1f)
			{
				m_OpDurNum += 2f * Time.deltaTime;
			}
			else if (num < 2f)
			{
				m_OpDurNum += 4f * Time.deltaTime;
			}
			else if (num < 3f)
			{
				m_OpDurNum += 7f * Time.deltaTime;
			}
			else if (num < 4f)
			{
				m_OpDurNum += 11f * Time.deltaTime;
			}
			else if (num < 5f)
			{
				m_OpDurNum += 16f * Time.deltaTime;
			}
			else
			{
				m_OpDurNum += 20f * Time.deltaTime;
			}
			m_OpDurNum = Mathf.Clamp(m_OpDurNum + m_CurrentNum, 1f, m_CurOpItem.GetCount()) - m_CurrentNum;
			m_InputOpNum.text = ((int)(m_OpDurNum + m_CurrentNum)).ToString();
			m_LbTotal.text = (m_CurPrice * (int)(m_OpDurNum + m_CurrentNum)).ToString();
		}
		else if (mSubBtnPress)
		{
			float num2 = Time.time - m_OpStarTime;
			if (num2 < 0.5f)
			{
				m_OpDurNum = -1f;
			}
			else if (num2 < 1f)
			{
				m_OpDurNum -= 2f * Time.deltaTime;
			}
			else if (num2 < 2f)
			{
				m_OpDurNum -= 4f * Time.deltaTime;
			}
			else if (num2 < 3f)
			{
				m_OpDurNum -= 7f * Time.deltaTime;
			}
			else if (num2 < 4f)
			{
				m_OpDurNum -= 11f * Time.deltaTime;
			}
			else if (num2 < 5f)
			{
				m_OpDurNum -= 16f * Time.deltaTime;
			}
			else
			{
				m_OpDurNum -= 20f * Time.deltaTime;
			}
			m_OpDurNum = Mathf.Clamp(m_OpDurNum + m_CurrentNum, 1f, m_CurOpItem.GetCount()) - m_CurrentNum;
			m_InputOpNum.text = ((int)(m_OpDurNum + m_CurrentNum)).ToString();
			m_LbTotal.text = (m_CurPrice * (int)(m_OpDurNum + m_CurrentNum)).ToString();
		}
		else
		{
			if (string.Empty == m_InputOpNum.text)
			{
				m_CurrentNum = 1f;
			}
			else
			{
				m_CurrentNum = Mathf.Clamp(Convert.ToInt32(m_InputOpNum.text), 1, m_CurOpItem.GetCount());
			}
			if (!UICamera.inputHasFocus)
			{
				m_InputOpNum.text = m_CurrentNum.ToString();
				m_LbTotal.text = (m_CurPrice * (int)m_CurrentNum).ToString();
			}
		}
	}

	private void CheckCantWorkTip()
	{
		if (CSUI_MainWndCtrl.IsWorking() && null != m_CurMenuItem)
		{
			if (m_CurMenuItem.AssemblyLevelInsufficient)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkAssemblyLevelInsufficient.GetString(), CSUtils.GetEntityName(m_CurMenuItem.m_Type)), Color.red);
			}
			else if (m_CurMenuItem.NotHaveElectricity)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), CSUtils.GetEntityName(m_CurMenuItem.m_Type)), Color.red);
			}
		}
	}

	private void TryClosePlayerPackagePanel()
	{
		if (GameUI.Instance.mItemPackageCtrl.isShow)
		{
			GameUI.Instance.mItemPackageCtrl.Hide();
		}
		GameUI.Instance.mItemPackageCtrl.transform.localPosition = m_PlayerPackagePos;
	}

	private void TryOpenPlayerPackagPanel()
	{
		if (!GameUI.Instance.mItemPackageCtrl.isShow)
		{
			GameUI.Instance.mItemPackageCtrl.Show();
		}
		m_PlayerPackagePos = GameUI.Instance.mItemPackageCtrl.transform.localPosition;
		Vector3 playerPackagePos = m_PlayerPackagePos;
		playerPackagePos.x = GameUI.Instance.mCSUI_MainWndCtrl.transform.localPosition.x + 675f;
		playerPackagePos.y = GameUI.Instance.mCSUI_MainWndCtrl.transform.localPosition.y + 10f;
		GameUI.Instance.mItemPackageCtrl.transform.localPosition = playerPackagePos;
	}

	private void Init()
	{
		if (!m_Init)
		{
			m_BuyItemList = new List<ItemObject>();
			m_RepurchaseList = new List<ItemObject>();
			m_ItemGridList = new List<Grid_N>();
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			m_Currency = 0;
			m_CurType = ItemLabel.Root.all;
			m_PageCount = 78;
			for (int i = 0; i < m_PageCount; i++)
			{
				Grid_N grid_N = UnityEngine.Object.Instantiate(m_GridPrefab);
				grid_N.transform.parent = m_ItemGrid.transform;
				grid_N.transform.localPosition = Vector3.zero;
				grid_N.transform.localRotation = Quaternion.identity;
				grid_N.transform.localScale = Vector3.one;
				grid_N.onLeftMouseClicked = OnLeftMouseCliked;
				grid_N.onRightMouseClicked = OnRightMouseCliked;
				m_ItemGridList.Add(grid_N);
			}
			ShowOpPanel(isShow: false);
			m_CanUseThis = true;
			m_NewUseState = true;
			ShowMeshGo(show: false);
			UIEventListener.Get(m_MeshGo).onClick = delegate
			{
				CheckCantWorkTip();
			};
			m_Init = true;
		}
	}

	private Grid_N GetGridByItem(int instanceID)
	{
		return m_ItemGridList.Find((Grid_N a) => a.ItemObj != null && a.ItemObj.instanceId == instanceID);
	}

	private void UpdateGridsInfo()
	{
		List<ItemObject> list = new List<ItemObject>();
		list = ((m_CurPackTab != 0) ? m_RepurchaseList : ((m_CurType != 0) ? m_BuyItemList.FindAll((ItemObject a) => a.protoData != null && a.protoData.rootItemLabel == m_CurType) : m_BuyItemList));
		if (list != null && list.Count > 0)
		{
			m_MaxPage = (list.Count - 1) / m_PageCount;
			if (m_MaxPage < 0)
			{
				m_MaxPage = 0;
			}
			if (m_MaxPage < m_CurPageIndex)
			{
				m_CurPageIndex = m_MaxPage;
			}
		}
		else
		{
			m_CurPageIndex = 0;
			m_MaxPage = 0;
		}
		int num = 0;
		num = ((m_MaxPage != m_CurPageIndex) ? m_PageCount : (list.Count - m_CurPageIndex * m_PageCount));
		ItemObject itemObject = null;
		int num2 = m_CurPageIndex * m_PageCount;
		int num3 = num2;
		for (int i = 0; i < m_PageCount; i++)
		{
			num3 = num2 + i;
			itemObject = ((i >= num) ? null : list[num3]);
			UpdateGrid(m_ItemGridList[i], itemObject, num3);
		}
		UpdatePageInfo(m_CurPageIndex + 1, m_MaxPage + 1);
	}

	private void UpdateGrid(Grid_N grid, ItemObject item, int index)
	{
		grid.SetItem(item);
		grid.SetItemPlace(ItemPlaceType.IPT_ColonyTradingPost, index);
		grid.SetGridMask(GridMask.GM_Any);
	}

	private void UpdatePageInfo(int curPage, int maxPage)
	{
		m_LbPage.text = $"{m_CurPageIndex + 1}/{m_MaxPage + 1}";
		m_LbPage.MakePixelPerfect();
	}

	private void UpdateCurrency()
	{
		m_LbCurrency.text = m_Currency.ToString();
		if (!Money.Digital)
		{
			m_SpriteMeat.gameObject.SetActive(value: true);
			m_SpriteMoney.gameObject.SetActive(value: false);
		}
		else
		{
			m_SpriteMeat.gameObject.SetActive(value: false);
			m_SpriteMoney.gameObject.SetActive(value: true);
		}
		Vector3 localPosition = m_SpriteMeat.transform.localPosition;
		localPosition.x = m_LbCurrency.transform.localPosition.x - m_LbCurrency.relativeSize.x * (float)m_LbCurrency.font.size - 12f;
		m_SpriteMeat.transform.localPosition = localPosition;
		m_SpriteMoney.transform.localPosition = localPosition;
	}

	private void BuyAll()
	{
		if (m_CurOpItem != null)
		{
			BuyItem(m_CurOpItem.GetCount());
		}
	}

	private void BuyItem(int count)
	{
		if (m_CurOpItem == null || count > m_CurOpItem.GetCount())
		{
			return;
		}
		if (m_CurPackTab == 0)
		{
			if (BuyItemEvent != null)
			{
				BuyItemEvent(m_CurOpItem.instanceId, count);
				ResetOpInfo();
			}
		}
		else if (RepurchaseItemEvent != null)
		{
			RepurchaseItemEvent(m_CurOpItem.instanceId, count);
			ResetOpInfo();
		}
	}

	private void SellAll()
	{
		if (m_CurOpItem != null)
		{
			SellItem(m_CurOpItem.GetCount());
		}
	}

	private void SellItem(int count)
	{
		if (m_CurOpItem != null && count <= m_CurOpItem.GetCount() && SellItemEvent != null)
		{
			SellItemEvent(m_CurOpItem.instanceId, count);
			ResetOpInfo();
		}
	}

	private void ShowOpPanel(bool isShow)
	{
		mSellOpLayer.gameObject.SetActive(isShow);
	}

	private void UpdateOpPanelInfo()
	{
		if (m_CurOpItem != null)
		{
			ShowOpPanel(isShow: true);
			m_CurOpGrid.SetItem(m_CurOpItem);
			m_CurrentNum = 1f;
			if (m_CurPackTab == 0 && m_IsBuy)
			{
				m_CurPrice = Mathf.RoundToInt((float)m_CurOpItem.GetBuyPrice() * 1.15f);
			}
			else
			{
				m_CurPrice = m_CurOpItem.GetSellPrice();
			}
			m_InputOpNum.text = m_CurrentNum.ToString();
			m_LbTotal.text = m_CurPrice.ToString();
			m_LbPrice.text = m_CurPrice.ToString();
			m_BtnOp.text = PELocalization.GetString((!m_IsBuy) ? 8000555 : 8000556);
		}
	}

	private void ResetOpInfo()
	{
		ShowOpPanel(isShow: false);
		m_CurOpItem = null;
		m_CurrentNum = 1f;
		m_CurOpGrid.SetItem(null);
		if (!m_IsBuy && null != GameUI.Instance)
		{
			GameUI.Instance.mItemPackageCtrl.RestItemState();
		}
		m_IsBuy = false;
	}

	private void ShowMeshGo(bool show)
	{
		m_MeshGo.SetActive(show);
	}

	public void SetMenu(CSUI_LeftMenuItem curMenuItem)
	{
		m_CurMenuItem = curMenuItem;
	}

	public void SellItemByPakcage(ItemObject item)
	{
		if (!m_Init)
		{
			Init();
		}
		m_CurOpItem = item;
		m_IsBuy = false;
		UpdateOpPanelInfo();
	}

	public void SellAllItemByPackage(ItemObject item)
	{
		if (!m_Init)
		{
			Init();
		}
		m_CurOpItem = item;
		m_IsBuy = false;
		SellAll();
	}

	public void CancelSell()
	{
		ResetOpInfo();
	}

	private void OnLeftMouseCliked(Grid_N grid)
	{
		if (grid.ItemObj != null && !mSellOpLayer.gameObject.activeSelf)
		{
			if (grid.ItemObj.GetCount() == 0)
			{
				new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
				return;
			}
			m_CurOpItem = grid.ItemObj;
			m_IsBuy = true;
			UpdateOpPanelInfo();
			SelectItem_N.Instance.SetItem(null);
		}
	}

	private void OnRightMouseCliked(Grid_N grid)
	{
		if (grid.ItemObj != null && !mSellOpLayer.gameObject.activeSelf)
		{
			if (grid.ItemObj.GetCount() == 0)
			{
				new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
				return;
			}
			SelectItem_N.Instance.SetItem(null);
			m_CurOpItem = grid.ItemObj;
			int count = m_CurOpItem.GetCount();
			int num = 0;
			num = ((m_CurPackTab != 0) ? m_CurOpItem.GetSellPrice() : Mathf.RoundToInt((float)m_CurOpItem.GetBuyPrice() * 1.15f));
			string text = m_CurOpItem.protoData.GetName();
			string text2 = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000077), text + " X " + count, PELocalization.GetString(8000253), count * num);
			MessageBox_N.ShowYNBox(text2, BuyAll, ResetOpInfo);
		}
	}

	private void BtnAllOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.all;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnWeaponOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.weapon;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnEquipOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.equipment;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnToolOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.tool;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnTurretOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.turret;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnConsumOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.consumables;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnResoureOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.resoure;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnPartOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.part;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnDecorationOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			m_CurType = ItemLabel.Root.decoration;
			m_CurPackTab = 0;
			m_CurPageIndex = 0;
			UpdateGridsInfo();
		}
	}

	private void BtnRebuyOnClick()
	{
		m_CurPackTab = 1;
		m_CurPageIndex = 0;
		UpdateGridsInfo();
	}

	private void OnOpOkBtn()
	{
		if (m_CurOpItem != null)
		{
			if (m_IsBuy)
			{
				BuyItem((int)m_CurrentNum);
			}
			else
			{
				SellItem((int)m_CurrentNum);
			}
		}
	}

	private void OnOpCancelBtn()
	{
		ShowOpPanel(isShow: false);
		ResetOpInfo();
	}

	private void OnPageDown()
	{
		if (m_CurPageIndex > 0)
		{
			m_CurPageIndex--;
			UpdateGridsInfo();
		}
	}

	private void BtnLeftEndOnClick()
	{
		m_CurPageIndex = 0;
		UpdateGridsInfo();
	}

	private void OnPageUp()
	{
		if (m_CurPageIndex < m_MaxPage)
		{
			m_CurPageIndex++;
			UpdateGridsInfo();
		}
	}

	private void BtnRightEndOnClick()
	{
		m_CurPageIndex = m_MaxPage;
		UpdateGridsInfo();
	}

	private void OnAddBtnPress()
	{
		mAddBtnPress = true;
		m_OpStarTime = Time.time;
		m_OpDurNum = 0f;
	}

	private void OnAddBtnRelease()
	{
		mAddBtnPress = false;
		m_CurrentNum = (int)(m_CurrentNum + m_OpDurNum);
		m_OpDurNum = 0f;
		m_InputOpNum.text = ((int)m_CurrentNum).ToString();
		m_LbTotal.text = (m_CurPrice * (int)m_CurrentNum).ToString();
	}

	private void OnSubstructBtnPress()
	{
		mSubBtnPress = true;
		m_OpStarTime = Time.time;
		m_OpDurNum = 0f;
	}

	private void OnSubstructBtnRelease()
	{
		mSubBtnPress = false;
		m_CurrentNum = (int)(m_CurrentNum + m_OpDurNum);
		m_OpDurNum = 0f;
		m_InputOpNum.text = ((int)m_CurrentNum).ToString();
		m_LbTotal.text = (m_CurPrice * (int)m_CurrentNum).ToString();
	}

	public void UpdateUIData(List<ItemObject> buyItemList, List<ItemObject> repurchaseList, int currency)
	{
		if (buyItemList != null && repurchaseList != null)
		{
			UpdateBuyItemList(buyItemList);
			UpdateRepurchaseList(repurchaseList);
			UpdateCurrency(currency);
		}
	}

	public void UpdateBuyItemList(List<ItemObject> buyItemList)
	{
		if (buyItemList != null)
		{
			if (!m_Init)
			{
				Init();
			}
			m_BuyItemList.Clear();
			for (int i = 0; i < buyItemList.Count; i++)
			{
				m_BuyItemList.Add(buyItemList[i]);
			}
			UpdateGridsInfo();
		}
	}

	public void UpdateRepurchaseList(List<ItemObject> repurchaseList)
	{
		if (repurchaseList != null)
		{
			if (!m_Init)
			{
				Init();
			}
			m_RepurchaseList.Clear();
			for (int i = 0; i < repurchaseList.Count; i++)
			{
				m_RepurchaseList.Add(repurchaseList[i]);
			}
			UpdateGridsInfo();
		}
	}

	public void UpdateCurrency(int currency)
	{
		if (!m_Init)
		{
			Init();
		}
		m_Currency = currency;
		UpdateCurrency();
	}

	public void RefreshCurrentyShowGrids()
	{
		if (!m_Init)
		{
			Init();
		}
		UpdateGridsInfo();
	}
}
