using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSFarm : CSElectric
{
	public delegate void PlantEvent(FarmPlantLogic plant);

	public delegate void PlantListEvent(List<FarmPlantLogic> plant);

	public delegate void ClearPlantEvent();

	public const int TOOL_INDEX_WATER = 0;

	public const int TOOL_INDEX_INSECTICIDE = 1;

	public const int PLANTS_SEEDS_COUNT = 12;

	public const int PLANTS_TOOLS_COUNT = 2;

	public const int MAX_WORKER_COUNT = 8;

	public const int CYCLE_MIN_WATER = 100;

	public const int CYCLE_MIN_INSECTICIDE = 100;

	public const int CYCLE_ADD_WATER = 88;

	public const int CYCLE_ADD_INSECTICIDE = 88;

	private const double VarPerOp = 30.0;

	public float curFarmerGrowRate;

	private CSFarmData m_FData;

	public CSFarmInfo m_FInfo;

	private Dictionary<int, FarmPlantLogic> m_Plants;

	private List<int> m_WateringIds;

	private List<int> m_CleaningIds;

	private List<int> m_DeadIds;

	private List<int> m_RipedIds;

	private static int layer = 2107392;

	private int m_PlantSequence;

	private int refreshCounter;

	public float FarmerGrowRate => curFarmerGrowRate;

	public CSFarmData Data
	{
		get
		{
			if (m_FData == null)
			{
				m_FData = m_Data as CSFarmData;
			}
			return m_FData;
		}
	}

	public CSFarmInfo Info
	{
		get
		{
			if (m_FInfo == null)
			{
				m_FInfo = m_Info as CSFarmInfo;
			}
			return m_FInfo;
		}
	}

	public Dictionary<int, FarmPlantLogic> Plants => m_Plants;

	public event PlantEvent CreatePlantEvent;

	public event PlantEvent RemovePlantEvent;

	public event PlantListEvent CreatePlantListEvent;

	public event ClearPlantEvent ClearAllPlantEvent;

	public CSFarm()
	{
		m_Type = 7;
		m_Workers = new CSPersonnel[8];
		m_WorkSpaces = new PersonnelSpace[1];
		m_WorkSpaces[0] = new PersonnelSpace(this);
		m_Plants = new Dictionary<int, FarmPlantLogic>();
		m_WateringIds = new List<int>();
		m_CleaningIds = new List<int>();
		m_RipedIds = new List<int>();
		m_DeadIds = new List<int>();
		m_Grade = 3;
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning;
	}

	public FarmPlantLogic AssignOutWateringPlant()
	{
		if (m_WateringIds.Count == 0)
		{
			return null;
		}
		FarmPlantLogic result = m_Plants[m_WateringIds[0]];
		m_WateringIds.RemoveAt(0);
		return result;
	}

	public void RestoreWateringPlant(FarmPlantLogic plant)
	{
		if (m_Plants.ContainsKey(plant.mPlantInstanceId) && plant.NeedWater)
		{
			m_WateringIds.Add(plant.mPlantInstanceId);
		}
	}

	public FarmPlantLogic AssignOutCleaningPlant()
	{
		if (m_CleaningIds.Count == 0)
		{
			return null;
		}
		FarmPlantLogic result = m_Plants[m_CleaningIds[0]];
		m_CleaningIds.RemoveAt(0);
		return result;
	}

	public void RestoreCleaningPlant(FarmPlantLogic plant)
	{
		if (m_Plants.ContainsKey(plant.mPlantInstanceId) && plant.NeedWater)
		{
			m_CleaningIds.Add(plant.mPlantInstanceId);
		}
	}

	public FarmPlantLogic AssignOutDeadPlant()
	{
		if (m_DeadIds.Count == 0)
		{
			return null;
		}
		FarmPlantLogic result = m_Plants[m_DeadIds[0]];
		m_DeadIds.RemoveAt(0);
		return result;
	}

	public FarmPlantLogic AssignOutRipePlant()
	{
		if (m_RipedIds.Count == 0)
		{
			return null;
		}
		FarmPlantLogic result = m_Plants[m_RipedIds[0]];
		m_RipedIds.RemoveAt(0);
		return result;
	}

	public void RestoreRipePlant(FarmPlantLogic plant)
	{
		if (m_Plants.ContainsKey(plant.mPlantInstanceId) && plant.IsRipe)
		{
			m_RipedIds.Add(plant.mPlantInstanceId);
		}
	}

	public void SetPlantSeed(int index, ItemObject item)
	{
		if (index < 0 || index >= 12)
		{
			Debug.LogError("Index is out of range!");
		}
		else if (item != null)
		{
			Data.m_PlantSeeds[index] = item.instanceId;
		}
		else
		{
			Data.m_PlantSeeds.Remove(index);
		}
	}

	public ItemObject GetPlantSeed(int index)
	{
		if (index < 0 || index >= 12)
		{
			Debug.LogError("Index is out of range!");
			return null;
		}
		if (Data.m_PlantSeeds.ContainsKey(index))
		{
			return PeSingleton<ItemMgr>.Instance.Get(Data.m_PlantSeeds[index]);
		}
		return null;
	}

	public void SetPlantTool(int index, ItemObject item)
	{
		if (index < 0 || index >= 2)
		{
			Debug.LogError("Index is out of range");
		}
		else if (item != null)
		{
			Data.m_Tools[index] = item.instanceId;
		}
		else
		{
			Data.m_Tools.Remove(index);
		}
	}

	public ItemObject GetPlantTool(int index)
	{
		if (index < 0 || index >= 2)
		{
			Debug.LogError("Index is out of range!");
			return null;
		}
		if (Data.m_Tools.ContainsKey(index))
		{
			return PeSingleton<ItemMgr>.Instance.Get(Data.m_Tools[index]);
		}
		return null;
	}

	public bool HasPlantSeed()
	{
		ItemObject itemObject = null;
		for (int i = 0; i < 12; i++)
		{
			ItemObject plantSeed = GetPlantSeed(i);
			if (plantSeed != null)
			{
				itemObject = plantSeed;
				break;
			}
		}
		return itemObject != null;
	}

	public int GetPlantSeedId()
	{
		for (int i = 0; i < 12; i++)
		{
			ItemObject plantSeed = GetPlantSeed(i);
			if (plantSeed != null)
			{
				return plantSeed.protoId;
			}
		}
		return -1;
	}

	public bool checkRroundCanPlant(int plantItemid, Vector3 pos)
	{
		Bounds plantBounds = PlantInfo.GetPlantBounds(plantItemid, pos);
		float radius = Mathf.Max(plantBounds.extents.x, plantBounds.extents.z);
		Collider[] array = Physics.OverlapSphere(pos, radius, layer);
		if (array != null && array.Length != 0)
		{
			return false;
		}
		foreach (int key in m_Plants.Keys)
		{
			if (plantBounds.Intersects(m_Plants[key].mPlantBounds))
			{
				return false;
			}
		}
		return true;
	}

	public FarmPlantLogic PlantTo(Vector3 pos)
	{
		FarmPlantLogic result = null;
		if (Data.m_SequentialPlanting)
		{
			int plantSequence = m_PlantSequence;
			for (int i = m_PlantSequence; i < 12; i++)
			{
				ItemObject plantSeed = GetPlantSeed(i);
				if (plantSeed != null)
				{
					result = _plant(plantSeed, pos, i);
					m_PlantSequence++;
					if (m_PlantSequence >= 12)
					{
						m_PlantSequence = 11;
					}
					break;
				}
			}
			if (plantSequence == m_PlantSequence)
			{
				for (int j = 0; j < plantSequence; j++)
				{
					ItemObject plantSeed2 = GetPlantSeed(j);
					if (plantSeed2 != null)
					{
						result = _plant(plantSeed2, pos, j);
						m_PlantSequence = j + 1;
						if (m_PlantSequence >= 12)
						{
							m_PlantSequence = 11;
						}
						break;
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < 12; k++)
			{
				ItemObject plantSeed3 = GetPlantSeed(k);
				if (plantSeed3 != null)
				{
					result = _plant(plantSeed3, pos, k);
					break;
				}
			}
		}
		return result;
	}

	private FarmPlantLogic _plant(ItemObject io, Vector3 pos, int index)
	{
		DragArticleAgent dragArticleAgent = DragArticleAgent.PutItemByProroId(io.protoId, pos, Quaternion.identity);
		io.DecreaseStackCount(1);
		if (io.GetCount() <= 0)
		{
			PeSingleton<ItemMgr>.Instance.DestroyItem(io.instanceId);
			SetPlantSeed(index, null);
		}
		ExcuteEvent(6001, index);
		return dragArticleAgent.itemLogic as FarmPlantLogic;
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!PeGameMgr.IsMulti) ? m_Creator.m_DataInst.AssignData(ID, 7, ref refData) : MultiColonyManager.Instance.AssignData(ID, 7, ref refData, _ColonyObj));
		m_Data = refData as CSFarmData;
		InitNPC();
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		}
	}

	public void InitNPC()
	{
		CSMgCreator cSMgCreator = m_Creator as CSMgCreator;
		if (!(cSMgCreator != null))
		{
			return;
		}
		foreach (CSPersonnel farmer in cSMgCreator.Farmers)
		{
			if (AddWorker(farmer))
			{
				farmer.WorkRoom = this;
			}
		}
	}

	public override void RemoveData()
	{
	}

	public override void ChangeState()
	{
		base.ChangeState();
		if (m_IsRunning)
		{
			if (FarmManager.Instance == null)
			{
				Debug.Log("FarmManager is missing?");
			}
			else
			{
				RefreshPlant();
			}
		}
		else if (FarmManager.Instance != null)
		{
			FarmManager.Instance.CreatePlantEvent -= OnCreatePlant;
			FarmManager.Instance.RemovePlantEvent -= OnRemovePlant;
			FarmPlantLogic.UnregisterEventListener(PlantEventListener);
		}
	}

	private void RefreshPlant()
	{
		m_Plants.Clear();
		m_WateringIds.Clear();
		m_CleaningIds.Clear();
		m_RipedIds.Clear();
		m_DeadIds.Clear();
		if (this.ClearAllPlantEvent != null)
		{
			this.ClearAllPlantEvent();
		}
		Dictionary<int, FarmPlantLogic> dictionary = new Dictionary<int, FarmPlantLogic>();
		foreach (KeyValuePair<int, FarmPlantLogic> item in FarmManager.Instance.mPlantMap)
		{
			if (base.Assembly.InRange(item.Value.mPos))
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		List<FarmPlantLogic> list = new List<FarmPlantLogic>();
		foreach (KeyValuePair<int, FarmPlantLogic> item2 in dictionary)
		{
			if (item2.Value.IsRipe)
			{
				m_RipedIds.Add(item2.Key);
			}
			else if (item2.Value.mDead)
			{
				m_DeadIds.Add(item2.Key);
			}
			else
			{
				if (item2.Value.NeedWater)
				{
					m_WateringIds.Add(item2.Key);
				}
				if (item2.Value.NeedClean)
				{
					m_CleaningIds.Add(item2.Key);
				}
			}
			list.Add(item2.Value);
		}
		if (this.CreatePlantListEvent != null)
		{
			this.CreatePlantListEvent(list);
		}
		m_Plants = dictionary;
		if (FarmManager.Instance != null)
		{
			FarmManager.Instance.CreatePlantEvent -= OnCreatePlant;
			FarmManager.Instance.RemovePlantEvent -= OnRemovePlant;
			FarmPlantLogic.UnregisterEventListener(PlantEventListener);
			FarmManager.Instance.CreatePlantEvent += OnCreatePlant;
			FarmManager.Instance.RemovePlantEvent += OnRemovePlant;
			FarmPlantLogic.RegisterEventListener(PlantEventListener);
		}
	}

	public override void Update()
	{
		base.Update();
		if (base.Assembly != null)
		{
			if (refreshCounter % 60 == 0)
			{
				UpdateFarmerGrowRate();
			}
			refreshCounter++;
			if (refreshCounter >= 600)
			{
				RefreshPlant();
				refreshCounter = 0;
			}
		}
	}

	public void UpdateFarmerGrowRate()
	{
		curFarmerGrowRate = GetWorkerParam();
	}

	public override float GetWorkerParam()
	{
		float num = 0f;
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null)
			{
				num += cSPersonnel.GetFarmingSkill;
			}
		}
		return num;
	}

	public override bool NeedWorkers()
	{
		if (m_WateringIds.Count != 0 && GetPlantTool(0) != null)
		{
			return true;
		}
		if (m_CleaningIds.Count != 0 && GetPlantTool(1) != null)
		{
			return true;
		}
		if (m_RipedIds.Count != 0)
		{
			return true;
		}
		ItemObject itemObject = null;
		for (int i = 0; i < 12; i++)
		{
			itemObject = GetPlantSeed(i);
			if (itemObject != null)
			{
				break;
			}
		}
		if (itemObject != null)
		{
		}
		return base.NeedWorkers();
	}

	public override void UpdateDataToUI()
	{
		if (GameUI.Instance != null)
		{
			GameUI.Instance.mCSUI_MainWndCtrl.FarmUI.RefreshTools();
		}
	}

	public override List<ItemIdCount> GetRequirements()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		if (GetPlantTool(0) == null || GetPlantTool(0).GetCount() < 100)
		{
			list.Add(new ItemIdCount(1003, 88));
		}
		if (GetPlantTool(1) == null || GetPlantTool(1).GetCount() < 100)
		{
			list.Add(new ItemIdCount(1002, 88));
		}
		return list;
	}

	public override bool MeetDemand(int protoId, int count)
	{
		if (protoId == 1003)
		{
			ItemObject plantTool = GetPlantTool(0);
			if (plantTool != null)
			{
				plantTool.IncreaseStackCount(count);
			}
			else
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(protoId);
				itemObject.SetStackCount(count);
				SetPlantTool(0, itemObject);
			}
		}
		if (protoId == 1002)
		{
			ItemObject plantTool2 = GetPlantTool(1);
			if (plantTool2 != null)
			{
				plantTool2.IncreaseStackCount(count);
			}
			else
			{
				ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.CreateItem(protoId);
				itemObject2.SetStackCount(count);
				SetPlantTool(1, itemObject2);
			}
		}
		UpdateDataToUI();
		return true;
	}

	public override bool MeetDemand(ItemIdCount supplyItem)
	{
		return MeetDemand(supplyItem.protoId, supplyItem.count);
	}

	public override bool MeetDemands(List<ItemIdCount> supplyItems)
	{
		foreach (ItemIdCount supplyItem in supplyItems)
		{
			if (supplyItem.protoId == 1003)
			{
				ItemObject plantTool = GetPlantTool(0);
				if (plantTool != null)
				{
					plantTool.IncreaseStackCount(supplyItem.count);
				}
				else
				{
					ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(supplyItem.protoId);
					itemObject.SetStackCount(supplyItem.count);
					SetPlantTool(0, itemObject);
				}
			}
			if (supplyItem.protoId == 1002)
			{
				ItemObject plantTool2 = GetPlantTool(1);
				if (plantTool2 != null)
				{
					plantTool2.IncreaseStackCount(supplyItem.count);
					continue;
				}
				ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.CreateItem(supplyItem.protoId);
				itemObject2.SetStackCount(supplyItem.count);
				SetPlantTool(1, itemObject2);
			}
		}
		UpdateDataToUI();
		return true;
	}

	private void OnCreatePlant(FarmPlantLogic plant)
	{
		if (base.Assembly != null && !m_Plants.ContainsKey(plant.mPlantInstanceId) && base.Assembly.InRange(plant.mPos))
		{
			m_Plants.Add(plant.mPlantInstanceId, plant);
			if (this.CreatePlantEvent != null)
			{
				this.CreatePlantEvent(plant);
			}
		}
	}

	private void OnRemovePlant(FarmPlantLogic plant)
	{
		if (this.RemovePlantEvent != null)
		{
			this.RemovePlantEvent(plant);
		}
		m_Plants.Remove(plant.mPlantInstanceId);
		m_WateringIds.Remove(plant.mPlantInstanceId);
		m_CleaningIds.Remove(plant.mPlantInstanceId);
		m_RipedIds.Remove(plant.mPlantInstanceId);
		m_DeadIds.Remove(plant.mPlantInstanceId);
	}

	private void PlantEventListener(FarmPlantLogic plant, int event_type)
	{
		if (!m_Plants.ContainsKey(plant.mPlantInstanceId))
		{
			return;
		}
		switch (event_type)
		{
		case 1:
			if (!m_WateringIds.Contains(plant.mPlantInstanceId))
			{
				m_WateringIds.Add(plant.mPlantInstanceId);
			}
			break;
		case 2:
			m_WateringIds.Remove(plant.mPlantInstanceId);
			break;
		case 3:
			if (!m_CleaningIds.Contains(plant.mPlantInstanceId))
			{
				m_CleaningIds.Add(plant.mPlantInstanceId);
			}
			break;
		case 4:
			m_CleaningIds.Remove(plant.mPlantInstanceId);
			break;
		case 5:
			if (!m_DeadIds.Contains(plant.mPlantInstanceId))
			{
				m_DeadIds.Add(plant.mPlantInstanceId);
			}
			break;
		case 6:
			if (!m_RipedIds.Contains(plant.mPlantInstanceId))
			{
				m_RipedIds.Add(plant.mPlantInstanceId);
			}
			break;
		}
	}
}
