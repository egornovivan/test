using System.Collections.Generic;
using CSRecord;
using Pathea;

public class MultiColonyManager
{
	private static MultiColonyManager mInstance;

	public Dictionary<int, List<ColonyNetwork>> ColonyData;

	public static MultiColonyManager Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new MultiColonyManager();
			}
			return mInstance;
		}
	}

	public List<ColonyNetwork> GetNetwork(int teamNum)
	{
		if (!ColonyData.ContainsKey(teamNum))
		{
			return null;
		}
		return ColonyData[teamNum];
	}

	public void AddNetworkData(ColonyNetwork colonyNetwork, int teamNum)
	{
		if (!ColonyData.ContainsKey(teamNum))
		{
			ColonyData.Add(teamNum, new List<ColonyNetwork>());
		}
		ColonyData[teamNum].Add(colonyNetwork);
	}

	public void AddNpcToColony(int id, int teamNum, int dwellingId)
	{
		CSMgCreator cSMgCreator = ((teamNum != BaseNetwork.MainPlayer.TeamId) ? (CSMain.Instance.MultiGetOtherCreator(teamNum) as CSMgCreator) : CSMain.s_MgCreator);
		PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(id);
		cSMgCreator.AddNpcInMultiMode(npc, dwellingId, bSetPos: true);
	}

	public static CSMgCreator GetCreator(int teamId, bool createNewIfNone = true)
	{
		if (teamId == BaseNetwork.MainPlayer.TeamId)
		{
			return CSMain.s_MgCreator;
		}
		return CSMain.Instance.MultiGetOtherCreator(teamId, createNewIfNone) as CSMgCreator;
	}

	public void AddDataToCreator(ColonyNetwork colonyNetwork, int teamNum)
	{
		CSMgCreator cSMgCreator = ((teamNum != BaseNetwork.MainPlayer.TeamId) ? (CSMain.Instance.MultiGetOtherCreator(teamNum) as CSMgCreator) : CSMain.s_MgCreator);
		cSMgCreator.teamNum = teamNum;
		CSObjectData recordData = colonyNetwork._ColonyObj._RecordData;
		if (recordData == null)
		{
			return;
		}
		CSEntityAttr attr = default(CSEntityAttr);
		attr.m_InstanceId = recordData.ID;
		attr.m_Type = recordData.dType;
		attr.m_Pos = recordData.m_Position;
		attr.m_protoId = recordData.ItemID;
		attr.m_Bound = recordData.m_Bounds;
		attr.m_ColonyBase = colonyNetwork._ColonyObj;
		CSEntity cSEntity = (colonyNetwork.m_Entity = cSMgCreator._createEntity(attr));
		if (recordData.dType == 1)
		{
			CSAssembly cSAssembly = cSEntity as CSAssembly;
			cSAssembly.ChangeState();
		}
		else
		{
			CSCommon cSCommon = cSEntity as CSCommon;
			if (cSMgCreator.Assembly != null)
			{
				cSMgCreator.Assembly.AttachCommonEntity(cSCommon);
			}
			if (cSEntity is CSDwellings)
			{
				cSEntity._Net.RPCServer(EPacketType.PT_CL_DWL_SyncNpc);
			}
			CSPersonnel[] npcs = cSMgCreator.GetNpcs();
			foreach (CSPersonnel cSPersonnel in npcs)
			{
				if (cSPersonnel.Data.m_WorkRoomID == attr.m_InstanceId && cSPersonnel.WorkRoom != cSCommon)
				{
					cSPersonnel.WorkRoom = cSCommon;
				}
			}
		}
		cSMgCreator.ExecuteEvent(1001, cSEntity);
	}

	public bool AssignData(int id, int type, ref CSDefaultData refData, ColonyBase _colony)
	{
		if (_colony != null && _colony._RecordData != null)
		{
			refData = _colony._RecordData;
			return false;
		}
		switch (type)
		{
		case 1:
			refData = new CSAssemblyData();
			break;
		case 2:
			refData = new CSStorageData();
			break;
		case 3:
			refData = new CSEngineerData();
			break;
		case 4:
			refData = new CSEnhanceData();
			break;
		case 5:
			refData = new CSRepairData();
			break;
		case 6:
			refData = new CSRecycleData();
			break;
		case 33:
			refData = new CSPPCoalData();
			break;
		case 34:
			refData = new CSPPSolarData();
			break;
		case 21:
			refData = new CSDwellingsData();
			break;
		case 7:
			refData = new CSFarmData();
			break;
		case 8:
			refData = new CSFactoryData();
			break;
		case 9:
			refData = new CSProcessingData();
			break;
		case 10:
			refData = new CSTradeData();
			break;
		case 11:
			refData = new CSTrainData();
			break;
		case 12:
			refData = new CSCheckData();
			break;
		case 13:
			refData = new CSTreatData();
			break;
		case 14:
			refData = new CSTentData();
			break;
		case 35:
			refData = new CSPPFusionData();
			break;
		default:
			refData = new CSDefaultData();
			break;
		}
		refData.ID = id;
		return true;
	}
}
