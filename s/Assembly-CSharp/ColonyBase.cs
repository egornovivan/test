using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

public abstract class ColonyBase
{
	public delegate void WorkerCountChangeEventHandler();

	private static bool isNewPutOut;

	private WorkerCountChangeEventHandler WorkerCountChangeEH;

	internal ColonyNetwork _Network;

	public CSObjectData _RecordData;

	private bool bUpdate;

	public int m_DefenceType = 5;

	public string _OpPlayerRoleName = string.Empty;

	protected Dictionary<int, ColonyNpc> _worker = new Dictionary<int, ColonyNpc>();

	private bool oldRunning;

	private float lastErrorTime;

	public static bool IsNewPutOut
	{
		get
		{
			return isNewPutOut;
		}
		set
		{
			isNewPutOut = value;
		}
	}

	public int Id => _Network.Id;

	public Vector3 Pos => _Network.Pos;

	public int TeamId => _Network.TeamId;

	public float Durability
	{
		get
		{
			return _RecordData.m_Durability;
		}
		set
		{
			_RecordData.m_Durability = value;
			if (_Network != null && _Network._skEntity != null)
			{
				_Network._skEntity.SetAttribute(AttribType.Hp, value);
			}
		}
	}

	public abstract int MaxWorkerCount { get; }

	public int GetWorkingCount => _worker.Keys.Count();

	public ColonyBase()
	{
		bUpdate = true;
	}

	public virtual bool IsWorking()
	{
		return false;
	}

	public virtual bool AddWorker(ColonyNpc npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (_worker.ContainsKey(npc._npcID))
		{
			return true;
		}
		if (_worker.Values.Count >= MaxWorkerCount)
		{
			return false;
		}
		_worker[npc._npcID] = npc;
		if (WorkerCountChangeEH != null)
		{
			WorkerCountChangeEH();
		}
		return true;
	}

	public virtual bool RemoveWorker(ColonyNpc npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (!_worker.ContainsKey(npc._npcID))
		{
			return true;
		}
		_worker.Remove(npc._npcID);
		npc.m_WorkRoomID = -1;
		if (WorkerCountChangeEH != null)
		{
			WorkerCountChangeEH();
		}
		return true;
	}

	public void RemoveAllWorker()
	{
		if (_worker.Values.Count <= 0)
		{
			return;
		}
		List<ColonyNpc> list = _worker.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			_worker.Remove(list[i]._npcID);
			list[i].m_WorkRoomID = -1;
			if (list[i]._refNpc != null)
			{
				list[i]._refNpc.RPCOthers(EPacketType.PT_CL_CLN_SetWorkRoomID, list[i].m_WorkRoomID);
			}
			if (WorkerCountChangeEH != null)
			{
				WorkerCountChangeEH();
			}
		}
	}

	public int GetWorkerAmount()
	{
		return _worker.Count;
	}

	public void SetWorkerCountChangeEventHandler(WorkerCountChangeEventHandler fun)
	{
		WorkerCountChangeEH = fun;
	}

	public void SetNetwork(ColonyNetwork network)
	{
		if (!(network == null))
		{
			_Network = network;
			_Network.runner = this;
			ColonyMgr._Instance.AddColonyItem(this);
		}
	}

	public void OnDestroy(object sender)
	{
		ColonyMgr._Instance.RemoveColonyItem(this);
		bUpdate = false;
	}

	public void OnDeath()
	{
		DestroySelf();
		SyncDelete();
		_Network.DestroyMe();
	}

	public virtual void DestroySelf()
	{
		RemoveAllWorker();
		DestroySomeData();
	}

	public virtual void DestroySomeData()
	{
	}

	public virtual void MyUpdate()
	{
		if (LogFilter.logDebug)
		{
			Debug.LogWarning("Enter virtual function MyUpdate");
		}
	}

	public void Update()
	{
		if (!bUpdate)
		{
			return;
		}
		try
		{
			OnRecycleTick();
			OnRepairTick(_RecordData.m_RepairValue, ColonyMgr.GetInfo(_Network.ExternId));
			MyUpdate();
			DoAfterUpdate();
		}
		catch (Exception ex)
		{
			if (lastErrorTime < Time.time)
			{
				lastErrorTime = Time.time + 6f;
				if (LogFilter.logError)
				{
					Debug.LogWarningFormat("{0}:[{1}] colony update got exception:{2}", Environment.TickCount, GetType(), ex);
				}
			}
		}
	}

	private void DoAfterUpdate()
	{
		if (oldRunning && !IsWorking())
		{
			DestroySomeData();
		}
		oldRunning = IsWorking();
	}

	public virtual void InitRecordData()
	{
		if (LogFilter.logDebug)
		{
			Debug.LogError("Enter virtual function InitRecordData");
		}
	}

	public virtual void CombomData(BinaryWriter writer)
	{
		if (LogFilter.logDebug)
		{
			Debug.LogError("Enter virtual function CombomData");
		}
	}

	public virtual void ParseData(byte[] data, int ver)
	{
		if (LogFilter.logDebug)
		{
			Debug.LogError("Enter virtual function ParseData");
		}
	}

	public virtual void InitMyData()
	{
		if (LogFilter.logDebug)
		{
			Debug.LogError("Enter virtual function InitMyData");
		}
	}

	public virtual void InitNpc()
	{
	}

	public virtual List<ItemIdCount> GetRequirements()
	{
		return null;
	}

	public virtual List<ItemIdCount> GetDesires()
	{
		return null;
	}

	public virtual bool MeetDemand(int protoId, int count)
	{
		return true;
	}

	public virtual void MeetDemands(List<ItemIdCount> itemList)
	{
	}

	public void InitBaseData(CSInfo csinfo)
	{
		Durability = csinfo.m_Durability;
		_RecordData.ID = _Network.Id;
		_RecordData.ItemID = _Network.ExternId;
		_RecordData.m_CurDeleteTime = -1f;
		_RecordData.m_CurRepairTime = -1f;
		_RecordData.m_DeleteTime = -1f;
		_RecordData.m_RepairTime = -1f;
		_RecordData.m_RepairValue = ComputeRepValue(csinfo);
	}

	public float ComputeRepValue(CSInfo csinfo)
	{
		if (csinfo.m_Durability == 0f)
		{
			return -1f;
		}
		float num = (csinfo.m_Durability - Durability) / csinfo.m_Durability;
		float num2 = csinfo.m_RepairTime * num;
		if (num2 == 0f)
		{
			return -1f;
		}
		return (csinfo.m_Durability - Durability) / num2;
	}

	public int GetItemId()
	{
		return _Network.ExternId;
	}

	public ItemObject GetItem()
	{
		return ItemManager.GetItemByID(_Network.Id);
	}

	public void SyncSave()
	{
		ColonyData colonyData = new ColonyData();
		colonyData.ExportData(this);
		AsyncSqlite.AddRecord(colonyData);
	}

	public void LoadComplete(SqliteDataReader dataReader)
	{
		bool flag = false;
		if (dataReader.Read())
		{
			int @int = dataReader.GetInt32(dataReader.GetOrdinal("ver"));
			_RecordData.dType = dataReader.GetInt32(dataReader.GetOrdinal("type"));
			_RecordData.m_CurDeleteTime = dataReader.GetFloat(dataReader.GetOrdinal("curdeletetime"));
			_RecordData.m_CurRepairTime = dataReader.GetFloat(dataReader.GetOrdinal("currepairtime"));
			_RecordData.m_DeleteTime = dataReader.GetFloat(dataReader.GetOrdinal("deletetime"));
			Durability = dataReader.GetFloat(dataReader.GetOrdinal("durability"));
			_RecordData.m_RepairTime = dataReader.GetFloat(dataReader.GetOrdinal("repairtime"));
			_RecordData.m_RepairValue = dataReader.GetFloat(dataReader.GetOrdinal("repairvalue"));
			byte[] data = (byte[])dataReader.GetValue(dataReader.GetOrdinal("data"));
			ParseData(data, @int);
			flag = true;
			InitNpc();
			if (@int != 2016102100)
			{
				SyncSave();
			}
		}
		if (!flag)
		{
			InitBaseData(ColonyMgr.GetInfo(_Network.ExternId));
			InitMyData();
			InitNpc();
			SyncSave();
		}
	}

	public bool CheckExist()
	{
		return false;
	}

	public void LoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM colony WHERE id=@id;");
			pEDbOp.BindParam("@id", _Network.Id);
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public virtual void DoMyContinue()
	{
		if (LogFilter.logDebug)
		{
			Debug.LogError("Enter virtual function DoMyContinue");
		}
	}

	public void SyncDelete()
	{
		ColonyData colonyData = new ColonyData();
		colonyData.DeleteData(this);
		AsyncSqlite.AddRecord(colonyData);
	}

	public void BeginRecycle(string roleName)
	{
		_OpPlayerRoleName = roleName;
		_RecordData.m_CurDeleteTime = 0f;
		_RecordData.m_DeleteTime = 20f;
		_Network.RPCOthers(EPacketType.PT_CL_BeginRecycle, _RecordData.m_DeleteTime);
	}

	public void RecycleItems(Player player, CSInfo csinfo)
	{
		ItemObject itemByID = ItemManager.GetItemByID(Id);
		Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(itemByID.protoId);
		if (formula == null)
		{
			return;
		}
		float num = 0f;
		num = _RecordData.m_Durability / csinfo.m_Durability;
		List<Replicator.Formula.Material> materials = formula.materials;
		foreach (Replicator.Formula.Material item in materials)
		{
			int num2 = Mathf.CeilToInt((float)item.itemCount * num);
			if (num2 > 0)
			{
				player.Package.AddItem(player.CreateItem(item.itemId, num2, syn: true));
			}
		}
		player.SyncPackageIndex();
		_Network.OnDeath();
	}

	public void EndRecycle()
	{
		_RecordData.m_CurDeleteTime = -1f;
		_RecordData.m_DeleteTime = -1f;
		Player player = Player.GetPlayer(_OpPlayerRoleName);
		if (player != null)
		{
			CSInfo info = ColonyMgr.GetInfo(_Network.ExternId);
			_Network.RPCOthers(EPacketType.PT_CL_EndRecycle, true);
			RecycleItems(player, info);
		}
		else
		{
			_Network.RPCOthers(EPacketType.PT_CL_EndRecycle, false);
		}
	}

	public void OnRecycleTick()
	{
		if (_RecordData.m_DeleteTime != -1f)
		{
			_RecordData.m_CurDeleteTime += 1f;
			if (_RecordData.m_DeleteTime <= _RecordData.m_CurDeleteTime)
			{
				EndRecycle();
			}
		}
	}

	public bool RepairItems(Player player, int protoTypeId)
	{
		if (null == player || _RecordData == null)
		{
			return false;
		}
		CSInfo info = ColonyMgr.GetInfo(protoTypeId);
		if (info == null)
		{
			return false;
		}
		float num = (info.m_Durability - Durability) / info.m_Durability;
		Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(protoTypeId);
		List<MaterialItem> repairMaterialList = ItemProto.GetRepairMaterialList(protoTypeId);
		if (repairMaterialList != null && repairMaterialList.Count > 0)
		{
			foreach (MaterialItem item in repairMaterialList)
			{
				int num2 = Mathf.CeilToInt((float)item.count * num);
				if (player.Package.GetItemCount(item.protoId) < num2)
				{
					return false;
				}
			}
			List<ItemObject> effItems = new List<ItemObject>();
			foreach (MaterialItem item2 in repairMaterialList)
			{
				int count = Mathf.CeilToInt((float)item2.count * num);
				player.Package.RemoveItem(item2.protoId, count, ref effItems);
			}
			player.SyncItemList(effItems);
			player.SyncPackageIndex();
			StartRepairCounter(info);
			return true;
		}
		if (formula != null && formula.materials != null && formula.materials.Count != 0)
		{
			foreach (Replicator.Formula.Material material in formula.materials)
			{
				int num3 = Mathf.CeilToInt((float)material.itemCount * num);
				if (player.Package.GetItemCount(material.itemId) < num3)
				{
					return false;
				}
			}
			List<ItemObject> effItems2 = new List<ItemObject>();
			foreach (Replicator.Formula.Material material2 in formula.materials)
			{
				int count2 = Mathf.CeilToInt((float)material2.itemCount * num);
				player.Package.RemoveItem(material2.itemId, count2, ref effItems2);
			}
			player.SyncItemList(effItems2);
			player.SyncPackageIndex();
			StartRepairCounter(info);
			return true;
		}
		return false;
	}

	public void StartRepairCounter(CSInfo csinfo)
	{
		float num = (csinfo.m_Durability - Durability) / csinfo.m_Durability;
		float num2 = csinfo.m_RepairTime * num;
		float repairValue = (csinfo.m_Durability - Durability) / num2;
		_RecordData.m_RepairValue = repairValue;
		_RecordData.m_CurRepairTime = 0f;
		_RecordData.m_RepairTime = num2;
		_Network.RPCOthers(EPacketType.PT_CL_RepairStart, _RecordData.m_CurRepairTime, _RecordData.m_RepairTime, _RecordData.m_RepairValue);
		SyncSave();
	}

	private void OnRepairTick(float deltaTime, CSInfo csinfo)
	{
		if (_RecordData.m_RepairTime != -1f)
		{
			_RecordData.m_CurRepairTime += 1f;
			Durability += _RecordData.m_RepairValue * deltaTime;
			Durability = Mathf.Min(Durability, csinfo.m_Durability);
			if (_RecordData.m_RepairTime <= _RecordData.m_CurRepairTime)
			{
				_RecordData.m_RepairTime = -1f;
				_RecordData.m_CurRepairTime = 0f;
				_Network.RPCOthers(EPacketType.PT_CL_RepairEnd, Durability);
				SyncSave();
			}
		}
	}

	internal void SyncItem(ItemObject item)
	{
		ChannelNetwork.SyncItem(_Network.WorldId, item);
	}

	internal void SyncCreationFuel(int objId, float fuel)
	{
		_Network.RPCOthers(EPacketType.PT_CL_SyncCreationFuel, objId, fuel);
	}

	internal void SyncCreationHP(int objId, float HP)
	{
		_Network.RPCOthers(EPacketType.PT_CL_SyncCreationHP, objId, HP);
	}

	public virtual void CreateData()
	{
	}

	protected virtual void UpdateTimeTick(float curTime)
	{
	}
}
