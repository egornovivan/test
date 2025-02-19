using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;
using WhiteCat;

public class UIRepairWnd : UIBaseWnd
{
	private const int c_MultiplierOfCost = 1;

	public Grid_N mRepairItem;

	public UILabel mCurrentD;

	public UILabel mAddD;

	public UILabel m_CostTimeLabel;

	public N_ImageButton ResetBtn;

	public N_ImageButton RepairBtn;

	public UIGrid mCostItemGrid;

	public CSUI_MaterialGrid mPerfab;

	private List<CSUI_MaterialGrid> mItemList = new List<CSUI_MaterialGrid>();

	private CSRepairObject mRepairMachine;

	private Vector3 mMachinePos = Vector3.zero;

	private MapObjNetwork _net;

	private PlayerPackageCmpt m_PlayerPackageCmpt;

	public MapObjNetwork Net
	{
		get
		{
			return _net;
		}
		set
		{
			_net = value;
		}
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		mRepairItem.SetItemPlace(ItemPlaceType.IPT_Repair, 0);
		mRepairItem.onLeftMouseClicked = OnLeftMouseClicked;
		mRepairItem.onRightMouseClicked = OnRightMouseClicked;
		mRepairItem.onDropItem = OnDropItem;
	}

	protected override void OnClose()
	{
		_net = null;
		mMachinePos = Vector3.zero;
		base.OnClose();
	}

	public void OpenWnd(CSRepairObject repMachine)
	{
		Show();
		mRepairMachine = repMachine;
		mMachinePos = repMachine.GetComponentInParent<PeEntity>().transform.position;
		if (PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			m_PlayerPackageCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		}
		if (PeGameMgr.IsMulti)
		{
			_net = MapObjNetwork.GetNet(mRepairMachine.m_Entity.ID);
			if (_net != null)
			{
				Net.RequestItemList();
			}
			else
			{
				Debug.LogError("net id is error id = " + mRepairMachine.m_Entity.ID);
			}
			return;
		}
		if (mRepairMachine != null && mRepairMachine.m_Repair != null)
		{
			if (mRepairMachine.m_Repair.IsRepairingM)
			{
				if (mRepairMachine.m_Repair.onRepairedTimeUp == null)
				{
					mRepairMachine.m_Repair.onRepairedTimeUp = RepairComplate;
				}
				if (!mRepairMachine.m_Repair.IsRunning)
				{
					mRepairMachine.m_Repair.CounterToRunning();
				}
			}
			UpdateItem(mRepairMachine.m_Repair.m_Item);
		}
		else
		{
			UpdateBtnState();
		}
		TutorialData.AddActiveTutorialID(8);
	}

	public void SetItemByNet(MapObjNetwork net, int itemId)
	{
		if (!(_net != null) || !(_net == net))
		{
			return;
		}
		if (itemId != -1)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemId);
			if (itemObject != null)
			{
				mRepairMachine.m_Repair.m_Item = itemObject.GetCmpt<Repair>();
			}
		}
		else
		{
			mRepairMachine.m_Repair.m_Item = null;
			mRepairItem.SetItem(null);
		}
		UpdateItemForNet(_net);
	}

	public void UpdateItemForNet(MapObjNetwork net)
	{
		if (_net != null && _net == net && !(mRepairMachine == null) && mRepairMachine.m_Repair != null)
		{
			UpdateItem(mRepairMachine.m_Repair.m_Item);
			_net.RequestRepairTime();
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}

	public void SetCounterByNet(MapObjNetwork net, float curTime, float finalTime)
	{
		if (_net != null && _net == net)
		{
			mRepairMachine.m_Repair.StopCounter();
			mRepairMachine.m_Repair.onRepairedTimeUp = RepairComplate;
			mRepairMachine.m_Repair.StartCounter(curTime, finalTime);
			UpdateBtnState();
		}
	}

	public void OnLeftMouseClicked(Grid_N grid)
	{
		if (mRepairMachine.m_Repair == null || !mRepairMachine.m_Repair.IsRepairingM)
		{
			ActiveWnd();
			if (!PeGameMgr.IsMulti && mRepairItem.ItemObj != null)
			{
				SelectItem_N.Instance.SetItem(mRepairItem.ItemObj, ItemPlaceType.IPT_Repair);
			}
		}
	}

	public void OnRightMouseClicked(Grid_N grid)
	{
		if (mRepairMachine.m_Repair != null && mRepairMachine.m_Repair.IsRepairingM)
		{
			return;
		}
		ActiveWnd();
		if (PeGameMgr.IsMulti)
		{
			if (grid.ItemObj != null)
			{
				_net.GetItem(grid.ItemObj.instanceId);
			}
		}
		else
		{
			TryAddCurRepairItemToPlayerPackage();
		}
	}

	public void SendToPlayer()
	{
		if (mRepairItem.ItemObj == null)
		{
		}
	}

	public void RemoveItem()
	{
		mRepairMachine.m_Repair.m_Item = null;
		mRepairItem.SetItem(null);
		UpdateItem(null);
	}

	public void OnDropItem(Grid_N grid)
	{
		if ((mRepairMachine.m_Repair != null && mRepairMachine.m_Repair.IsRepairingM) || SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar || SelectItem_N.Instance.Place == ItemPlaceType.IPT_Repair)
		{
			return;
		}
		if (CreationHelper.GetCreationItemClass(SelectItem_N.Instance.ItemObj) != 0)
		{
			PeTipMsg.Register(PELocalization.GetString(8000843), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		ActiveWnd();
		Repair cmpt = SelectItem_N.Instance.ItemObj.GetCmpt<Repair>();
		if (cmpt == null || cmpt.protoData.repairMaterialList == null || cmpt.protoData.repairMaterialList.Count == 0)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			int[] itemlist = new int[1] { SelectItem_N.Instance.ItemObj.instanceId };
			_net.InsertItemList(itemlist);
			return;
		}
		if (mRepairItem.ItemObj == null)
		{
			UpdateItem(cmpt);
			SelectItem_N.Instance.RemoveOriginItem();
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		ItemPackage.ESlotType slotType = ItemPackage.GetSlotType(mRepairItem.ItemObj.protoId);
		ItemPackage.ESlotType slotType2 = ItemPackage.GetSlotType(cmpt.itemObj.protoId);
		if (slotType == slotType2 && null != SelectItem_N.Instance.Grid)
		{
			if (SelectItem_N.Instance.Grid.onGridsExchangeItem != null)
			{
				SelectItem_N.Instance.Grid.onGridsExchangeItem(SelectItem_N.Instance.Grid, mRepairItem.ItemObj);
				UpdateItem(cmpt);
				SelectItem_N.Instance.SetItem(null);
			}
		}
		else if (TryAddCurRepairItemToPlayerPackage())
		{
			UpdateItem(cmpt);
			SelectItem_N.Instance.RemoveOriginItem();
			SelectItem_N.Instance.SetItem(null);
		}
	}

	public void DropItemByNet(MapObjNetwork net, int _instanceId)
	{
		if (!(_net != null) || !(_net == net) || mRepairMachine == null)
		{
			return;
		}
		if (_instanceId != -1)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(_instanceId);
			if (itemObject != null)
			{
				UpdateItem(itemObject.GetCmpt<Repair>());
			}
			SelectItem_N.Instance.SetItem(null);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		else
		{
			mRepairMachine.m_Repair.m_Item = null;
			mRepairItem.SetItem(null);
			UpdateItem(null);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}

	public void ResetItemByNet(MapObjNetwork net, int _instanceId)
	{
		if (mRepairMachine.m_Repair != null && mRepairMachine.m_Repair.IsRepairingM && _net != null && _net == net && !(mRepairMachine == null) && _instanceId != -1)
		{
			Repair cmpt = PeSingleton<ItemMgr>.Instance.Get(_instanceId).GetCmpt<Repair>();
			if (cmpt == mRepairMachine.m_Repair.m_Item)
			{
				mRepairMachine.m_Repair.StopCounter();
				UpdateBtnState();
			}
		}
	}

	private bool TryAddCurRepairItemToPlayerPackage()
	{
		if (mRepairItem.ItemObj != null)
		{
			if (m_PlayerPackageCmpt.package.CanAdd(mRepairItem.ItemObj))
			{
				m_PlayerPackageCmpt.package.AddItem(mRepairItem.ItemObj);
				RemoveItem();
				GameUI.Instance.mItemPackageCtrl.ResetItem();
				return true;
			}
			MessageBox_N.ShowOkBox(PELocalization.GetString(9500312));
			return false;
		}
		return false;
	}

	private void UpdateItem(Repair obj)
	{
		if (obj != null)
		{
			mCurrentD.text = PELocalization.GetString(82220001) + ": " + Mathf.CeilToInt(obj.GetValue().current * 0.01f);
			mAddD.text = "(+" + Mathf.CeilToInt(obj.GetValue().ExpendValue * 0.01f) + ")";
		}
		else
		{
			mCurrentD.text = PELocalization.GetString(82220001) + ": 0";
			mAddD.text = "(+0)";
		}
		foreach (CSUI_MaterialGrid mItem in mItemList)
		{
			mItem.gameObject.SetActive(value: false);
			mItem.transform.parent = null;
			Object.Destroy(mItem.gameObject);
		}
		mItemList.Clear();
		if (obj == null)
		{
			UpdateBtnState();
			return;
		}
		mRepairItem.SetItem(obj.itemObj);
		mRepairMachine.m_Repair.m_Item = obj;
		foreach (MaterialItem repairMaterial in obj.protoData.repairMaterialList)
		{
			int num = Mathf.CeilToInt((float)(repairMaterial.count * 1) * (1f - obj.GetValue().percent));
			AddCostItem(repairMaterial.protoId, (num >= 0) ? num : 0);
		}
		mCostItemGrid.Reposition();
		UpdateBtnState();
	}

	private void AddCostItem(int id, int num)
	{
		CSUI_MaterialGrid cSUI_MaterialGrid = Object.Instantiate(mPerfab);
		cSUI_MaterialGrid.transform.parent = mCostItemGrid.transform;
		cSUI_MaterialGrid.transform.localPosition = Vector3.zero;
		cSUI_MaterialGrid.transform.rotation = Quaternion.identity;
		cSUI_MaterialGrid.transform.localScale = Vector3.one;
		cSUI_MaterialGrid.ItemID = id;
		cSUI_MaterialGrid.NeedCnt = num;
		cSUI_MaterialGrid.ItemNum = m_PlayerPackageCmpt.package.GetCount(id);
		mItemList.Add(cSUI_MaterialGrid);
	}

	private void UpdateBtnState()
	{
		if (null == mRepairMachine || mRepairMachine.m_Repair == null)
		{
			RepairBtn.isEnabled = false;
			ResetBtn.isEnabled = false;
		}
		else if (mRepairMachine.m_Repair.m_Item == null || mRepairMachine.m_Repair.m_Item.GetValue().ExpendValue == 0f)
		{
			RepairBtn.isEnabled = false;
			ResetBtn.isEnabled = false;
		}
		else if (mRepairMachine.m_Repair.IsRepairingM)
		{
			RepairBtn.isEnabled = false;
			ResetBtn.isEnabled = true;
		}
		else
		{
			RepairBtn.isEnabled = true;
			ResetBtn.isEnabled = false;
		}
	}

	private void OnRepairBtn()
	{
		if (!(mRepairItem == null) && mRepairItem.ItemObj != null)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000098), Repair);
		}
	}

	private void Repair()
	{
		Repair cmpt = mRepairItem.ItemObj.GetCmpt<Repair>();
		if (cmpt == null || cmpt.GetValue().IsCurrentMax())
		{
			return;
		}
		bool flag = true;
		string text = PELocalization.GetString(8000026) + " [ffff00]";
		foreach (MaterialItem repairMaterial in cmpt.protoData.repairMaterialList)
		{
			int num = Mathf.CeilToInt((float)(repairMaterial.count * 1) * (1f - cmpt.GetValue().percent));
			int count = m_PlayerPackageCmpt.package.GetCount(repairMaterial.protoId);
			if (count < num)
			{
				text = text + " " + PeSingleton<ItemProto.Mgr>.Instance.Get(repairMaterial.protoId).GetName() + string.Empty;
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			text = text + "[-] " + PELocalization.GetString(8000027);
			MessageBox_N.ShowOkBox(text);
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			if (mRepairItem.ItemObj != null)
			{
				_net.RequestRepair(mRepairItem.ItemObj.instanceId);
			}
			return;
		}
		foreach (MaterialItem repairMaterial2 in cmpt.protoData.repairMaterialList)
		{
			int count2 = Mathf.CeilToInt((float)(repairMaterial2.count * 1) * (1f - cmpt.GetValue().percent));
			m_PlayerPackageCmpt.package.Destroy(repairMaterial2.protoId, count2);
		}
		GameUI.Instance.mItemPackageCtrl.ResetItem();
		mRepairMachine.m_Repair.StartCounter();
		mRepairMachine.m_Repair.onRepairedTimeUp = RepairComplate;
		UpdateItem(cmpt);
	}

	private void OnResetBtn()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000100), Reset);
	}

	private void Reset()
	{
		if (!GameConfig.IsMultiMode)
		{
			if (null != mRepairMachine && mRepairMachine.m_Repair != null)
			{
				mRepairMachine.m_Repair.StopCounter();
				UpdateBtnState();
			}
		}
		else if (null != _net && null != mRepairItem && mRepairItem.ItemObj != null)
		{
			_net.RequestStopRepair(mRepairItem.ItemObj.instanceId);
		}
	}

	private void RepairComplate(Repair item)
	{
		Repair cmpt = mRepairItem.ItemObj.GetCmpt<Repair>();
		if (cmpt == item)
		{
			cmpt.Do();
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			UpdateItem(cmpt);
		}
	}

	private void Update()
	{
		if (null != GameUI.Instance.mMainPlayer && mMachinePos != Vector3.zero && Vector3.Distance(GameUI.Instance.mMainPlayer.position, mMachinePos) > 8f)
		{
			OnClose();
		}
		if (!(mRepairItem == null) && mRepairItem.ItemObj != null)
		{
			if (mRepairMachine.m_Repair != null && mRepairMachine.m_Repair.IsRepairingM)
			{
				m_CostTimeLabel.text = CSUtils.GetRealTimeMS((int)mRepairMachine.m_Repair.CostsTime);
			}
			else
			{
				m_CostTimeLabel.text = "00:00";
			}
		}
	}
}
