using System.Collections.Generic;
using CSRecord;
using Pathea;
using UnityEngine;

public class CSNoMgCreator : CSCreator
{
	private Dictionary<int, CSCommon> m_CommonEntities;

	public override int CreateEntity(CSEntityAttr attr, out CSEntity outEnti)
	{
		outEnti = null;
		if (attr.m_Type == 1)
		{
			Debug.LogWarning("Non-Managed Creator cant create the Assembly Entity.");
			return 5;
		}
		if (m_CommonEntities.ContainsKey(attr.m_InstanceId))
		{
			outEnti = m_CommonEntities[attr.m_InstanceId];
			outEnti.gameObject = attr.m_Obj;
			outEnti.Position = attr.m_Pos;
			outEnti.ItemID = attr.m_protoId;
			outEnti.BaseData.m_Alive = true;
			return 4;
		}
		CSCommon cSCommon = null;
		switch (attr.m_Type)
		{
		case 2:
		{
			cSCommon = new CSStorage();
			CSStorage cSStorage = cSCommon as CSStorage;
			cSStorage.m_Info = CSInfoMgr.m_StorageInfo;
			cSStorage.m_Creator = this;
			cSStorage.m_Power = attr.m_Power;
			cSStorage.m_Package.ExtendPackage(CSInfoMgr.m_StorageInfo.m_MaxItem, CSInfoMgr.m_StorageInfo.m_MaxEquip, CSInfoMgr.m_StorageInfo.m_MaxRecource, CSInfoMgr.m_StorageInfo.m_MaxArmor);
			break;
		}
		case 4:
		{
			cSCommon = new CSEnhance();
			CSEnhance cSEnhance = cSCommon as CSEnhance;
			cSEnhance.m_Creator = this;
			cSEnhance.m_Power = attr.m_Power;
			cSEnhance.m_Info = CSInfoMgr.m_EnhanceInfo;
			break;
		}
		case 5:
		{
			cSCommon = new CSRepair();
			CSRepair cSRepair = cSCommon as CSRepair;
			cSRepair.m_Creator = this;
			cSRepair.m_Power = attr.m_Power;
			cSRepair.m_Info = CSInfoMgr.m_RepairInfo;
			break;
		}
		case 6:
		{
			cSCommon = new CSRecycle();
			CSRecycle cSRecycle = cSCommon as CSRecycle;
			cSRecycle.m_Creator = this;
			cSRecycle.m_Power = attr.m_Power;
			cSRecycle.m_Info = CSInfoMgr.m_RecycleInfo;
			break;
		}
		case 21:
		{
			cSCommon = new CSDwellings();
			CSDwellings cSDwellings = cSCommon as CSDwellings;
			cSDwellings.m_Creator = this;
			cSDwellings.m_Power = attr.m_Power;
			cSDwellings.m_Info = CSInfoMgr.m_DwellingsInfo;
			break;
		}
		case 33:
		{
			cSCommon = new CSPPCoal();
			CSPPCoal cSPPCoal = cSCommon as CSPPCoal;
			cSPPCoal.m_Creator = this;
			cSPPCoal.m_Power = 10000f;
			cSPPCoal.m_RestPower = 10000f;
			cSPPCoal.m_Info = CSInfoMgr.m_ppCoal;
			break;
		}
		case 34:
		{
			cSCommon = new CSPPSolar();
			CSPPSolar cSPPSolar = cSCommon as CSPPSolar;
			cSPPSolar.m_Creator = this;
			cSPPSolar.m_Power = 10000f;
			cSPPSolar.m_RestPower = 10000f;
			cSPPSolar.m_Info = CSInfoMgr.m_ppCoal;
			break;
		}
		case 8:
			cSCommon = new CSFactory();
			cSCommon.m_Creator = this;
			cSCommon.m_Info = CSInfoMgr.m_FactoryInfo;
			break;
		}
		cSCommon.ID = attr.m_InstanceId;
		cSCommon.CreateData();
		cSCommon.gameObject = attr.m_Obj;
		cSCommon.Position = attr.m_Pos;
		cSCommon.ItemID = attr.m_protoId;
		outEnti = cSCommon;
		m_CommonEntities.Add(attr.m_InstanceId, cSCommon);
		return 4;
	}

	public override CSEntity RemoveEntity(int id, bool bRemoveData = true)
	{
		CSEntity cSEntity = null;
		if (m_CommonEntities.ContainsKey(id))
		{
			cSEntity = m_CommonEntities[id];
			cSEntity.BaseData.m_Alive = false;
			if (bRemoveData)
			{
				m_CommonEntities[id].RemoveData();
			}
			m_CommonEntities.Remove(id);
			ExecuteEvent(1002, cSEntity);
		}
		else
		{
			Debug.LogWarning("The Common Entity that you want to Remove is not contained!");
		}
		return cSEntity;
	}

	public override CSCommon GetCommonEntity(int ID)
	{
		if (m_CommonEntities.ContainsKey(ID))
		{
			return m_CommonEntities[ID];
		}
		return null;
	}

	public override int GetCommonEntityCnt()
	{
		return m_CommonEntities.Count;
	}

	public override Dictionary<int, CSCommon> GetCommonEntities()
	{
		return m_CommonEntities;
	}

	public override int CanCreate(int type, Vector3 pos)
	{
		if (type == 1)
		{
			return 5;
		}
		return 4;
	}

	public override bool AddNpc(PeEntity npc, bool bSetPos = false)
	{
		return false;
	}

	public override void RemoveNpc(PeEntity npc)
	{
	}

	public override CSPersonnel[] GetNpcs()
	{
		return null;
	}

	public override CSPersonnel GetNpc(int id)
	{
		return null;
	}

	private void Awake()
	{
		m_CommonEntities = new Dictionary<int, CSCommon>();
	}

	private void Start()
	{
		Dictionary<int, CSDefaultData> objectRecords = m_DataInst.GetObjectRecords();
		foreach (CSDefaultData value in objectRecords.Values)
		{
			if (value is CSObjectData { m_Alive: not false } cSObjectData)
			{
				CSEntityAttr attr = default(CSEntityAttr);
				attr.m_InstanceId = cSObjectData.ID;
				attr.m_Type = cSObjectData.dType;
				attr.m_Pos = cSObjectData.m_Position;
				attr.m_protoId = cSObjectData.ItemID;
				CSEntity outEnti = null;
				CreateEntity(attr, out outEnti);
			}
		}
	}

	private void Update()
	{
		foreach (KeyValuePair<int, CSCommon> commonEntity in m_CommonEntities)
		{
			commonEntity.Value.Update();
		}
	}
}
