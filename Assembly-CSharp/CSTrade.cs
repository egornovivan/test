using System;
using System.Collections.Generic;
using System.IO;
using CSRecord;
using ItemAsset;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSTrade : CSElectric
{
	public Dictionary<int, int> campToTrade = new Dictionary<int, int>();

	public CSUI_TradingPost uiObj;

	public CSTradeInfo m_TInfo;

	private CSTradeData m_TData;

	public CounterScript updateCounter;

	private List<ItemObject> mBuyItemList = new List<ItemObject>();

	private List<ItemObject> mRepurchaseList = new List<ItemObject>();

	private Dictionary<int, stShopData> mShopList = new Dictionary<int, stShopData>();

	public override GameObject gameLogic
	{
		get
		{
			return base.gameLogic;
		}
		set
		{
			base.gameLogic = value;
			if (!(gameLogic != null))
			{
				return;
			}
			PEMachine component = gameLogic.GetComponent<PEMachine>();
			if (component != null)
			{
				for (int i = 0; i < m_WorkSpaces.Length; i++)
				{
					m_WorkSpaces[i].WorkMachine = component;
				}
			}
		}
	}

	public CSBuildingLogic BuildingLogic => gameLogic.GetComponent<CSBuildingLogic>();

	public CSTradeInfo Info
	{
		get
		{
			if (m_TInfo == null)
			{
				m_TInfo = m_Info as CSTradeInfo;
			}
			return m_TInfo;
		}
	}

	public CSTradeData Data
	{
		get
		{
			if (m_TData == null)
			{
				m_TData = m_Data as CSTradeData;
			}
			return m_TData;
		}
	}

	public Dictionary<int, stShopData> ShopList => mShopList;

	private int colonyMoney
	{
		get
		{
			if (base.m_MgCreator == null)
			{
				return 0;
			}
			return base.m_MgCreator.ColonyMoney;
		}
		set
		{
			if (base.m_MgCreator != null)
			{
				base.m_MgCreator.ColonyMoney = value;
			}
		}
	}

	public Money PlayerMoney
	{
		get
		{
			PackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
			return cmpt.money;
		}
	}

	public CSTrade(CSCreator creator)
	{
		m_Creator = creator;
		m_Type = 10;
		m_Workers = new CSPersonnel[1];
		m_WorkSpaces = new PersonnelSpace[1];
		for (int i = 0; i < m_WorkSpaces.Length; i++)
		{
			m_WorkSpaces[i] = new PersonnelSpace(this);
		}
		m_Grade = 3;
		if (base.IsMine)
		{
			BindEvent();
		}
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning;
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 10, ref refData) : MultiColonyManager.Instance.AssignData(ID, 10, ref refData, _ColonyObj));
		m_Data = refData as CSTradeData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			InitTradeData();
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
			InitTradeDataWithData();
		}
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		DestroySomeData();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
	}

	public List<IntVector2> GetTownInfo()
	{
		return PeSingleton<DetectedTownMgr>.Instance.detectedTowns;
	}

	public PeEntity GetTownNpc(VArtifactTown vat)
	{
		return new PeEntity();
	}

	public void InitTradeData()
	{
		UpdateTradeData();
	}

	public void InitTradeDataWithData()
	{
		UpdateTradeData();
	}

	public void UpdateTradeData()
	{
		bool flag = false;
		if (base.m_MgCreator != null)
		{
			foreach (int item in base.m_MgCreator.AddedStoreId)
			{
				if (!mShopList.ContainsKey(item))
				{
					mShopList.Add(item, new stShopData(-1, -1.0));
					flag = true;
				}
			}
			List<int> list = new List<int>();
			foreach (int key in mShopList.Keys)
			{
				if (!base.m_MgCreator.AddedStoreId.Contains(key))
				{
					list.Add(key);
				}
			}
			foreach (int item2 in list)
			{
				mShopList.Remove(item2);
				flag = true;
			}
		}
		if (flag)
		{
			UpdateBuyDataToUI();
		}
	}

	private void UnbindEvent()
	{
		if (uiObj != null)
		{
			if (base.m_MgCreator != null)
			{
				base.m_MgCreator.UnRegistStoreIdAddedEvent(AddSellItems);
				base.m_MgCreator.UnRegistUpdateAddedStoreIdEvent(UpdateTradeData);
				base.m_MgCreator.UnRegistUpdateMoneyEvent(UpdateMoneyToUI);
			}
			CSUI_TradingPost cSUI_TradingPost = uiObj;
			cSUI_TradingPost.BuyItemEvent = (Action<int, int>)Delegate.Remove(cSUI_TradingPost.BuyItemEvent, new Action<int, int>(BuyItem));
			CSUI_TradingPost cSUI_TradingPost2 = uiObj;
			cSUI_TradingPost2.SellItemEvent = (Action<int, int>)Delegate.Remove(cSUI_TradingPost2.SellItemEvent, new Action<int, int>(SellItem));
			CSUI_TradingPost cSUI_TradingPost3 = uiObj;
			cSUI_TradingPost3.RepurchaseItemEvent = (Action<int, int>)Delegate.Remove(cSUI_TradingPost3.RepurchaseItemEvent, new Action<int, int>(RepurchaseItem));
			CSUI_TradingPost cSUI_TradingPost4 = uiObj;
			cSUI_TradingPost4.RequestRefreshUIEvent = (Action)Delegate.Remove(cSUI_TradingPost4.RequestRefreshUIEvent, new Action(UpdateShop));
			uiObj = null;
		}
	}

	private void BindEvent()
	{
		if (uiObj == null && CSUI_MainWndCtrl.Instance != null)
		{
			if (base.m_MgCreator != null)
			{
				base.m_MgCreator.RegistStoreIdAddedEvent(AddSellItems);
				base.m_MgCreator.RegistUpdateAddedStoreIdEvent(UpdateTradeData);
				base.m_MgCreator.RegistUpdateMoneyEvent(UpdateMoneyToUI);
			}
			uiObj = CSUI_MainWndCtrl.Instance.TradingPostUI;
			CSUI_TradingPost cSUI_TradingPost = uiObj;
			cSUI_TradingPost.BuyItemEvent = (Action<int, int>)Delegate.Combine(cSUI_TradingPost.BuyItemEvent, new Action<int, int>(BuyItem));
			CSUI_TradingPost cSUI_TradingPost2 = uiObj;
			cSUI_TradingPost2.SellItemEvent = (Action<int, int>)Delegate.Combine(cSUI_TradingPost2.SellItemEvent, new Action<int, int>(SellItem));
			CSUI_TradingPost cSUI_TradingPost3 = uiObj;
			cSUI_TradingPost3.RepurchaseItemEvent = (Action<int, int>)Delegate.Combine(cSUI_TradingPost3.RepurchaseItemEvent, new Action<int, int>(RepurchaseItem));
			CSUI_TradingPost cSUI_TradingPost4 = uiObj;
			cSUI_TradingPost4.RequestRefreshUIEvent = (Action)Delegate.Combine(cSUI_TradingPost4.RequestRefreshUIEvent, new Action(UpdateShop));
		}
	}

	private void UpdateDataToUI(object Obj)
	{
		if (Obj != null)
		{
			(Obj as CSUI_TradingPost).UpdateUIData(mBuyItemList, mRepurchaseList, colonyMoney);
		}
	}

	private void UpdateBuyDataToUI(object Obj)
	{
		if (Obj != null)
		{
			(Obj as CSUI_TradingPost).UpdateBuyItemList(mBuyItemList);
			(Obj as CSUI_TradingPost).UpdateCurrency(colonyMoney);
		}
	}

	private void UpdateRepurchaseDataToUI(object Obj)
	{
		if (Obj != null)
		{
			(Obj as CSUI_TradingPost).UpdateRepurchaseList(mRepurchaseList);
			(Obj as CSUI_TradingPost).UpdateCurrency(colonyMoney);
		}
	}

	private void UpdateMoneyToUI(object Obj)
	{
		if (Obj != null)
		{
			(Obj as CSUI_TradingPost).UpdateCurrency(colonyMoney);
		}
	}

	public override void DestroySomeData()
	{
		if (base.IsMine)
		{
			UnbindEvent();
		}
	}

	public override void UpdateDataToUI()
	{
		if (base.IsMine)
		{
			BindEvent();
			UpdateDataToUI(uiObj);
		}
	}

	public void UpdateBuyDataToUI()
	{
		if (base.IsMine)
		{
			BindEvent();
			UpdateBuyDataToUI(uiObj);
		}
	}

	public void UpdateRepurchaseDataToUI()
	{
		if (base.IsMine)
		{
			BindEvent();
			UpdateRepurchaseDataToUI(uiObj);
		}
	}

	public void UpdateMoneyToUI()
	{
		if (base.IsMine)
		{
			BindEvent();
			UpdateMoneyToUI(uiObj);
		}
	}

	public void AddSellItems(List<int> addStoreId)
	{
		foreach (int item in addStoreId)
		{
			if (!mShopList.ContainsKey(item))
			{
				mShopList[item] = new stShopData(-1, -1.0);
			}
		}
		UpdateShop();
	}

	public void UpdateShop()
	{
		if (PeGameMgr.IsMulti)
		{
			UpdateDataToUI();
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_RequestShop);
			return;
		}
		mBuyItemList.Clear();
		bool flag = true;
		foreach (int key in mShopList.Keys)
		{
			ShopData shopData = ShopRespository.GetShopData(key);
			if (shopData == null)
			{
				continue;
			}
			flag = true;
			if (PeGameMgr.IsStory)
			{
				for (int i = 0; i < shopData.m_LimitMisIDList.Count; i++)
				{
					if (shopData.m_LimitType == 1)
					{
						if (MissionManager.Instance.HadCompleteMission(shopData.m_LimitMisIDList[i]))
						{
							break;
						}
					}
					else if (!MissionManager.Instance.HadCompleteMission(shopData.m_LimitMisIDList[i]))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			if (mShopList[key].CreateTime < 0.0)
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(shopData.m_ItemID);
				itemObject.stackCount = shopData.m_LimitNum;
				mShopList[key].ItemObjID = itemObject.instanceId;
				mShopList[key].CreateTime = GameTime.Timer.Second;
				mBuyItemList.Add(itemObject);
			}
			else if (GameTime.Timer.Second - mShopList[key].CreateTime > (double)shopData.m_RefreshTime)
			{
				ItemObject itemObject2;
				if (mShopList[key].ItemObjID >= 0)
				{
					itemObject2 = PeSingleton<ItemMgr>.Instance.Get(mShopList[key].ItemObjID);
					if (itemObject2 == null)
					{
						itemObject2 = PeSingleton<ItemMgr>.Instance.CreateItem(shopData.m_ItemID);
					}
					itemObject2.stackCount = shopData.m_LimitNum;
				}
				else
				{
					itemObject2 = PeSingleton<ItemMgr>.Instance.CreateItem(shopData.m_ItemID);
					itemObject2.stackCount = shopData.m_LimitNum;
				}
				mShopList[key].ItemObjID = itemObject2.instanceId;
				mShopList[key].CreateTime = GameTime.Timer.Second;
				mBuyItemList.Add(itemObject2);
			}
			else if (mShopList[key].ItemObjID >= 0)
			{
				ItemObject itemObject3 = PeSingleton<ItemMgr>.Instance.Get(mShopList[key].ItemObjID);
				if (itemObject3 == null)
				{
					itemObject3 = PeSingleton<ItemMgr>.Instance.CreateItem(shopData.m_ItemID);
				}
				mShopList[key].ItemObjID = itemObject3.instanceId;
				mBuyItemList.Add(itemObject3);
			}
		}
		UpdateDataToUI();
	}

	public void BuyItem(int instanceId, int count)
	{
		int num = -1;
		int num2 = 0;
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject == null)
		{
			UpdateShop();
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			num2 = count * Mathf.RoundToInt((float)ShopRespository.GetPriceBuyItemId(itemObject.protoId) * 1.15f);
			if (PlayerMoney.current < num2)
			{
				PeTipMsg.Register(PELocalization.GetString((!Money.Digital) ? 8000073 : 8000092), PeTipMsg.EMsgLevel.Warning);
				return;
			}
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_BuyItem, instanceId, count);
			return;
		}
		foreach (KeyValuePair<int, stShopData> mShop in mShopList)
		{
			if (mShop.Value.ItemObjID == instanceId)
			{
				num = mShop.Key;
				break;
			}
		}
		if (num < 0)
		{
			UpdateShop();
			return;
		}
		ShopData shopData = ShopRespository.GetShopData(num);
		if (shopData != null)
		{
			num2 = Mathf.RoundToInt((float)shopData.m_Price * 1.15f) * count;
			if (Buy(itemObject, count, shopData, isRepurchase: false))
			{
				colonyMoney += num2;
				RemoveBuyItem(itemObject, count);
				UpdateBuyDataToUI();
			}
		}
		else
		{
			PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
		}
	}

	public void RemoveBuyItem(ItemObject itemObj, int count)
	{
		if (itemObj.stackCount < count)
		{
			Debug.LogError("Remove num is big than item you have.");
		}
		else if (itemObj.GetCount() > count)
		{
			itemObj.DecreaseStackCount(count);
		}
		else
		{
			mBuyItemList.Remove(itemObj);
		}
	}

	public void RepurchaseItem(int instanceId, int count)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject == null)
		{
			UpdateShop();
			return;
		}
		if (!mRepurchaseList.Contains(itemObject))
		{
			UpdateShop();
			return;
		}
		int sellPrice = itemObject.GetSellPrice();
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_RepurchaseItem, instanceId, count);
		}
		else if (Buy(itemObject, count, null, isRepurchase: true))
		{
			sellPrice *= count;
			colonyMoney += sellPrice;
			RemoveRepurchase(itemObject, count);
			UpdateRepurchaseDataToUI();
		}
	}

	public void RemoveRepurchase(ItemObject itemObj, int count)
	{
		if (itemObj.GetCount() < count)
		{
			Debug.LogError("Remove num is big than item you have.");
		}
		else if (itemObj.GetCount() > count)
		{
			itemObj.DecreaseStackCount(count);
		}
		else
		{
			mRepurchaseList.Remove(itemObj);
		}
	}

	public bool Buy(ItemObject itemObj, int count, ShopData data, bool isRepurchase)
	{
		int num = (isRepurchase ? itemObj.GetSellPrice() : Mathf.RoundToInt((float)data.m_Price * 1.15f));
		int num2 = num * count;
		if (PlayerMoney.current < num2)
		{
			PeTipMsg.Register(PELocalization.GetString((!Money.Digital) ? 8000073 : 8000092), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		ItemObject itemObject = null;
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		if (!cmpt.package.CanAdd(itemObj.protoId, count))
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		if (itemObj.protoData.maxStackNum == 1)
		{
			int num3 = count;
			for (int i = 0; i < num3; i++)
			{
				count = 1;
				if (count < itemObj.GetCount())
				{
					itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(itemObj.protoId);
					itemObject.SetStackCount(count);
				}
				else
				{
					itemObject = itemObj;
					if (!isRepurchase)
					{
						mShopList[data.m_ID].ItemObjID = -1;
					}
				}
				cmpt.package.AddItem(itemObject, !isRepurchase);
			}
			PlayerMoney.current -= num2;
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		else
		{
			if (count == itemObj.GetCount() && !isRepurchase)
			{
				mShopList[data.m_ID].ItemObjID = -1;
			}
			cmpt.package.Add(itemObj.protoId, count, !isRepurchase);
			PlayerMoney.current -= num2;
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		return true;
	}

	public void ParseData(byte[] data, CSTradeData tradeData)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			int key = BufferHelper.ReadInt32(reader);
			int itemObjId = BufferHelper.ReadInt32(reader);
			double createTime = BufferHelper.ReadDouble(reader);
			tradeData.mShopList.Add(key, new stShopData(itemObjId, createTime));
		}
	}

	public void SellItem(int instanceId, int count)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject == null)
		{
			Debug.LogError("Sell item is null");
			return;
		}
		if (count > itemObject.GetCount())
		{
			Debug.LogError("not enough count");
			return;
		}
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return;
		}
		int protoId = itemObject.protoId;
		int num = itemObject.GetSellPrice() * count;
		if (colonyMoney < num)
		{
			new PeTipMsg(PELocalization.GetString(8000858), PeTipMsg.EMsgLevel.Error);
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_SellItem, instanceId, count);
			return;
		}
		colonyMoney -= num;
		ItemObject itemObject2;
		if (count == itemObject.stackCount)
		{
			itemObject2 = itemObject;
			cmpt.Remove(itemObject);
		}
		else
		{
			itemObject2 = PeSingleton<ItemMgr>.Instance.CreateItem(protoId);
			itemObject2.SetStackCount(count);
			cmpt.DestroyItem(instanceId, count);
		}
		AddRepurchase(itemObject2);
		cmpt.money.current += num;
		UpdateRepurchaseDataToUI();
	}

	public void AddRepurchase(ItemObject item)
	{
		mRepurchaseList.Add(item);
	}

	public void UpdateBuyResultMulti(int instanceId)
	{
		mBuyItemList.RemoveAll((ItemObject it) => it.instanceId == instanceId);
		UpdateBuyDataToUI();
	}

	public void UpdateSellResultMulti(int instanceId)
	{
		ItemObject item = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		mRepurchaseList.Add(item);
		UpdateRepurchaseDataToUI();
	}

	public void UpdateRepurchaseResultMulti(int instanceId)
	{
		mRepurchaseList.RemoveAll((ItemObject it) => it.instanceId == instanceId);
		UpdateRepurchaseDataToUI();
	}

	public void UpdateBuyItemMulti(List<int> instanceIdList)
	{
		bool flag = false;
		if (mBuyItemList.Count == instanceIdList.Count)
		{
			foreach (ItemObject mBuyItem in mBuyItemList)
			{
				if (!instanceIdList.Contains(mBuyItem.instanceId))
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		mBuyItemList.Clear();
		foreach (int instanceId in instanceIdList)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
			if (itemObject != null)
			{
				mBuyItemList.Add(itemObject);
			}
		}
		UpdateBuyDataToUI();
	}

	public void UpdateRepurchaseMulti(List<int> instanceIdList)
	{
		bool flag = false;
		if (mRepurchaseList.Count == instanceIdList.Count)
		{
			foreach (ItemObject mRepurchase in mRepurchaseList)
			{
				if (!instanceIdList.Contains(mRepurchase.instanceId))
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		mRepurchaseList.Clear();
		foreach (int instanceId in instanceIdList)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
			if (itemObject != null)
			{
				mRepurchaseList.Add(itemObject);
			}
		}
		UpdateRepurchaseDataToUI();
	}

	public void UpdateMoneyMulti(int money)
	{
		colonyMoney = money;
		UpdateMoneyToUI();
	}
}
