using System.Collections.Generic;
using System.Linq;
using CSRecord;
using UnityEngine;

public class CSAssembly : CSEntity
{
	public CSAssemblyData m_AssemblyData;

	public CSAssemblyInfo m_AssemblyInfo;

	public Dictionary<CSConst.ObjectType, List<CSCommon>> m_BelongObjectsMap;

	private Dictionary<CSConst.ObjectType, int> m_ObjLimitMap;

	private CounterScript m_CSUpgrade;

	private int m_ErodeMapId;

	public bool isSearchingClod;

	public CSAssemblyData Data
	{
		get
		{
			if (m_AssemblyData == null)
			{
				m_AssemblyData = m_Data as CSAssemblyData;
			}
			return m_AssemblyData;
		}
	}

	public CSAssemblyInfo Info
	{
		get
		{
			if (m_AssemblyInfo == null)
			{
				m_AssemblyInfo = m_Info as CSAssemblyInfo;
			}
			return m_AssemblyInfo;
		}
	}

	public List<CSCommon> AllPowerPlants
	{
		get
		{
			List<CSCommon> list = new List<CSCommon>();
			list.AddRange(m_BelongObjectsMap[CSConst.ObjectType.PowerPlant_Coal]);
			list.AddRange(m_BelongObjectsMap[CSConst.ObjectType.PowerPlant_Fusion]);
			return list;
		}
	}

	public CSFarm Farm
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Farm].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Farm][0] as CSFarm;
		}
	}

	public List<CSCommon> Storages
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Storage].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Storage];
		}
	}

	public CSFactory Factory
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Factory].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Factory][0] as CSFactory;
		}
	}

	public List<CSCommon> Dwellings
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Dwelling].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Dwelling];
		}
	}

	public CSProcessing ProcessingFacility
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Processing].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Processing][0] as CSProcessing;
		}
	}

	public CSTraining TrainingCenter
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Train].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Train][0] as CSTraining;
		}
	}

	public CSTrade TradePost
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Trade].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Trade][0] as CSTrade;
		}
	}

	public CSMedicalCheck MedicalCheck
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Check].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Check][0] as CSMedicalCheck;
		}
	}

	public CSMedicalTreat MedicalTreat
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Treat].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Treat][0] as CSMedicalTreat;
		}
	}

	public CSMedicalTent MedicalTent
	{
		get
		{
			if (m_BelongObjectsMap[CSConst.ObjectType.Tent].Count == 0)
			{
				return null;
			}
			return m_BelongObjectsMap[CSConst.ObjectType.Tent][0] as CSMedicalTent;
		}
	}

	public bool isUpgrading => m_CSUpgrade != null;

	public int Level => Data.m_Level;

	public float Radius => Info.m_Levels[Level].radius;

	public float LargestRadius => Info.m_Levels[2].radius;

	public float UpgradeTime => Info.m_Levels[Level].upgradeTime;

	public int[] UpgradeItems => Info.m_Levels[Level].itemIDList.ToArray();

	public int[] UpgradeItemCnt => Info.m_Levels[Level].itemCnt.ToArray();

	public float damageCD => Info.m_Levels[Level].damageCD;

	public float damage => Info.m_Levels[Level].damage;

	public bool IsDangerous
	{
		get
		{
			if (gameObject == null)
			{
				return false;
			}
			CSAssemblyObject component = gameObject.GetComponent<CSAssemblyObject>();
			PolarShield curEnergySheild = component.CurEnergySheild;
			if (curEnergySheild != null)
			{
				return !curEnergySheild.IsEmpty;
			}
			return false;
		}
	}

	public bool bShowShield
	{
		get
		{
			return Data.m_ShowShield;
		}
		set
		{
			Data.m_ShowShield = value;
		}
	}

	public double MedicineResearchState
	{
		get
		{
			return Data.m_MedicineResearchState;
		}
		set
		{
			Data.m_MedicineResearchState = value;
		}
	}

	public int MedicineResearchTimes
	{
		get
		{
			return Data.m_MedicineResearchTimes;
		}
		set
		{
			Data.m_MedicineResearchTimes = value;
		}
	}

	public CSAssembly()
	{
		m_Type = 1;
		m_ObjLimitMap = new Dictionary<CSConst.ObjectType, int>();
		m_BelongObjectsMap = new Dictionary<CSConst.ObjectType, List<CSCommon>>();
		m_BelongObjectsMap.Add(CSConst.ObjectType.PowerPlant_Coal, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.PowerPlant_Fusion, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Storage, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Enhance, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Repair, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Recyle, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Dwelling, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Farm, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Factory, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Trade, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Processing, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Train, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Check, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Treat, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Tent, new List<CSCommon>());
		m_Grade = 0;
	}

	public override bool IsDoingJob()
	{
		return true;
	}

	public static bool IsPowerPlant(CSConst.ObjectType type)
	{
		return type == CSConst.ObjectType.PowerPlant_Coal || type == CSConst.ObjectType.PowerPlant_Fusion;
	}

	public bool SetLevel(int Level)
	{
		if (Level >= Info.m_Levels.Count || Level < 0)
		{
			return false;
		}
		Data.m_Level = Level;
		m_ObjLimitMap[CSConst.ObjectType.Assembly] = 1;
		m_ObjLimitMap[CSConst.ObjectType.Storage] = Info.m_Levels[Data.m_Level].storageCnt;
		m_ObjLimitMap[CSConst.ObjectType.PowerPlant_Coal] = Info.m_Levels[Data.m_Level].coalPlantCnt;
		m_ObjLimitMap[CSConst.ObjectType.PowerPlant_Fusion] = Info.m_Levels[Data.m_Level].fusionPlantCnt;
		m_ObjLimitMap[CSConst.ObjectType.Engineer] = Info.m_Levels[Data.m_Level].EngineeringCnt;
		m_ObjLimitMap[CSConst.ObjectType.Enhance] = Info.m_Levels[Data.m_Level].EnhanceMachineCnt;
		m_ObjLimitMap[CSConst.ObjectType.Repair] = Info.m_Levels[Data.m_Level].RepairMachineCnt;
		m_ObjLimitMap[CSConst.ObjectType.Recyle] = Info.m_Levels[Data.m_Level].RecycleMachineCnt;
		m_ObjLimitMap[CSConst.ObjectType.Dwelling] = Info.m_Levels[Data.m_Level].dwellingsCnt;
		m_ObjLimitMap[CSConst.ObjectType.Factory] = Info.m_Levels[Data.m_Level].factoryCnt;
		m_ObjLimitMap[CSConst.ObjectType.Farm] = Info.m_Levels[Data.m_Level].farmCnt;
		m_ObjLimitMap[CSConst.ObjectType.Trade] = Info.m_Levels[Data.m_Level].tradePostCnt;
		m_ObjLimitMap[CSConst.ObjectType.Processing] = Info.m_Levels[Data.m_Level].processingCnt;
		m_ObjLimitMap[CSConst.ObjectType.Train] = Info.m_Levels[Data.m_Level].trainCenterCnt;
		m_ObjLimitMap[CSConst.ObjectType.Check] = Info.m_Levels[Data.m_Level].medicalCheckCnt;
		m_ObjLimitMap[CSConst.ObjectType.Treat] = Info.m_Levels[Data.m_Level].medicalTreatCnt;
		m_ObjLimitMap[CSConst.ObjectType.Tent] = Info.m_Levels[Data.m_Level].medicalTentCnt;
		return true;
	}

	public void StartUpgradeCounter()
	{
		StartUpgradeCounter(0f, UpgradeTime);
	}

	public void StartUpgradeCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			if (m_CSUpgrade == null)
			{
				m_CSUpgrade = CSMain.Instance.CreateCounter(base.Name + " Upgrade", curTime, finalTime);
			}
			else
			{
				m_CSUpgrade.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				m_CSUpgrade.OnTimeUp = OnUpgraded;
			}
		}
	}

	public void OnUpgraded()
	{
		SetLevel(Data.m_Level + 1);
		ChangeState();
		RefreshErodeMap();
		RefreshAssemblyObject();
		ExcuteEvent(2003);
	}

	public void StopCounter()
	{
		CSMain.Instance.DestoryCounter(m_CSUpgrade);
		m_CSUpgrade = null;
	}

	public void SetCounter(float curCounter)
	{
		if (null != m_CSUpgrade)
		{
			m_CSUpgrade.SetCurCounter(curCounter);
		}
	}

	public bool IsOutOfLimit(CSConst.ObjectType type)
	{
		if (m_ObjLimitMap[type] <= m_BelongObjectsMap[type].Count)
		{
			return true;
		}
		return false;
	}

	public int GetEntityCnt(CSConst.ObjectType type)
	{
		return m_BelongObjectsMap[type].Count;
	}

	public int GetLimitCnt(CSConst.ObjectType type)
	{
		return m_ObjLimitMap[type];
	}

	public bool InRange(Vector3 pos)
	{
		float num = Vector3.Distance(pos, base.Position);
		if (num > Radius)
		{
			return false;
		}
		return true;
	}

	public bool InLargestRange(Vector3 pos)
	{
		float num = Vector3.Distance(pos, base.Position);
		if (num > Info.m_Levels[2].radius)
		{
			return false;
		}
		return true;
	}

	public bool OutOfCount(CSConst.ObjectType type)
	{
		int count = m_BelongObjectsMap[type].Count;
		if (count < m_ObjLimitMap[type])
		{
			return true;
		}
		return false;
	}

	public int[] GetLevelUpItem()
	{
		return Info.m_Levels[Level].itemIDList.ToArray();
	}

	public int[] GetLevelUpItemCnt()
	{
		return Info.m_Levels[Level].itemCnt.ToArray();
	}

	public int GetMaxLevel()
	{
		return Info.m_Levels.Count - 1;
	}

	public void InitErodeMap(Vector3 pos, float radius)
	{
		m_ErodeMapId = AIErodeMap.AddErode(pos, radius);
	}

	public void RemoveErodeMap()
	{
		AIErodeMap.RemoveErode(m_ErodeMapId);
	}

	public void RefreshErodeMap()
	{
		AIErodeMap.UpdateErode(m_ErodeMapId, base.Position, Radius);
	}

	public void RefreshAssemblyObject()
	{
		if (gameObject != null)
		{
			CSAssemblyObject component = gameObject.GetComponent<CSAssemblyObject>();
			if (component != null)
			{
				component.RefreshObject();
			}
		}
	}

	public List<CSCommon> GetBelongCommons()
	{
		List<CSCommon> list = new List<CSCommon>();
		foreach (List<CSCommon> value in m_BelongObjectsMap.Values)
		{
			list.AddRange(value);
		}
		return list;
	}

	public void AddBelongBuilding(CSConst.ObjectType type, CSCommon building)
	{
		m_BelongObjectsMap[type].Add(building);
		ExcuteEvent(2001, building);
	}

	public void RemoveBelongBuilding(CSConst.ObjectType type, CSCommon building)
	{
		m_BelongObjectsMap[type].Remove(building);
		ExcuteEvent(2002, building);
	}

	public int AttachCommonEntity(CSCommon csc)
	{
		if (csc.Assembly == this)
		{
			return 5;
		}
		if (!InRange(csc.Position))
		{
			return 2;
		}
		CSConst.ObjectType type = (CSConst.ObjectType)csc.m_Type;
		if (IsOutOfLimit(type))
		{
			return 3;
		}
		AddBelongBuilding((CSConst.ObjectType)csc.m_Type, csc);
		csc.Assembly = this;
		if (csc is CSElectric cSElectric)
		{
			foreach (CSCommon allPowerPlant in AllPowerPlants)
			{
				CSPowerPlant cSPowerPlant = allPowerPlant as CSPowerPlant;
				cSPowerPlant.AttachElectric(cSElectric);
				if (cSElectric.IsRunning)
				{
					break;
				}
			}
		}
		csc.ChangeState();
		return 4;
	}

	public void DetachCommonEntity(CSCommon csc)
	{
		csc.Assembly = null;
		RemoveBelongBuilding((CSConst.ObjectType)csc.m_Type, csc);
		if (csc is CSElectric { m_PowerPlant: not null } cSElectric)
		{
			cSElectric.m_PowerPlant.DetachElectric(cSElectric);
		}
		csc.ChangeState();
	}

	public override void ChangeState()
	{
		m_IsRunning = true;
		Dictionary<int, CSCommon> commonEntities = m_Creator.GetCommonEntities();
		List<int> list = commonEntities.Keys.ToList();
		list.Sort();
		foreach (int item in list)
		{
			if (commonEntities[item].Assembly == null)
			{
				AttachCommonEntity(commonEntities[item]);
			}
		}
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		foreach (List<CSCommon> value in m_BelongObjectsMap.Values)
		{
			CSCommon[] array = value.ToArray();
			CSCommon[] array2 = array;
			foreach (CSCommon csc in array2)
			{
				DetachCommonEntity(csc);
			}
		}
		if (m_CSUpgrade != null)
		{
			Object.Destroy(m_CSUpgrade);
		}
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 1, ref refData) : MultiColonyManager.Instance.AssignData(ID, 1, ref refData, _ColonyObj));
		m_Data = refData as CSAssemblyData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			SetLevel(0);
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
			StartUpgradeCounter(Data.m_CurUpgradeTime, Data.m_UpgradeTime);
			SetLevel(Data.m_Level);
		}
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
	}

	public override void Update()
	{
		base.Update();
		Data.m_TimeTicks = m_Creator.Timer.Tick;
		if (m_CSUpgrade != null)
		{
			Data.m_CurUpgradeTime = m_CSUpgrade.CurCounter;
			Data.m_UpgradeTime = m_CSUpgrade.FinalCounter;
		}
		else
		{
			Data.m_CurUpgradeTime = 0f;
			Data.m_UpgradeTime = -1f;
		}
	}
}
