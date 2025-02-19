using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class CSMain : MonoBehaviour
{
	public delegate void InitEvent();

	private static CSMain s_Instance;

	public Dictionary<int, CSCreator> otherCreators;

	public bool m_DebugGUI = true;

	[SerializeField]
	private GameObject m_CSCarrier;

	public List<CounterScript> m_CSList;

	public Dictionary<int, CSCreator> m_Creators;

	public static int CSEntityLayerIndex;

	private double lastCycle = -9999.0;

	private int counter;

	public static CSMain Instance => s_Instance;

	public static CSMgCreator s_MgCreator
	{
		get
		{
			CSCreator cSCreator = null;
			cSCreator = GetCreator(0);
			return (!(cSCreator == null)) ? (cSCreator as CSMgCreator) : null;
		}
	}

	public static CSNoMgCreator s_NoMgCreator
	{
		get
		{
			CSCreator creator = GetCreator(10000);
			return (!(creator == null)) ? (creator as CSNoMgCreator) : null;
		}
	}

	public static event InitEvent InitOperatItemEvent;

	public CounterScript CreateCounter(string csName, float curTime, float finalTime)
	{
		CounterScript counterScript = m_CSCarrier.AddComponent<CounterScript>();
		counterScript.m_Description = csName;
		counterScript.Init(curTime, finalTime);
		m_CSList.Add(counterScript);
		return counterScript;
	}

	public void DestoryCounter(CounterScript cs)
	{
		if (cs != null)
		{
			m_CSList.Remove(cs);
			Object.Destroy(cs);
		}
	}

	public void RemoveCounter(CounterScript cs)
	{
		if (cs != null)
		{
			m_CSList.Remove(cs);
		}
	}

	public void EndWithCounters()
	{
	}

	public CSCreator CreateCreator(int ID, string desc, CSConst.CreatorType type = CSConst.CreatorType.Managed)
	{
		if (m_Creators.ContainsKey(ID))
		{
			Debug.Log("This ID [" + ID + "] is exsit");
			return null;
		}
		CSCreator cSCreator = null;
		switch (type)
		{
		case CSConst.CreatorType.Managed:
		{
			GameObject gameObject2 = new GameObject();
			gameObject2.transform.parent = base.transform;
			gameObject2.name = desc;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			CSMgCreator cSMgCreator = gameObject2.AddComponent<CSMgCreator>();
			cSCreator = cSMgCreator;
			cSCreator.m_DataInst = CSDataMgr.CreateDataInst(ID, type);
			cSMgCreator.m_Clod = CSClodsMgr.CreateClod(ID);
			m_Creators.Add(ID, cSCreator);
			break;
		}
		case CSConst.CreatorType.NoManaged:
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.parent = base.transform;
			gameObject.name = desc;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			cSCreator = gameObject.AddComponent<CSNoMgCreator>();
			cSCreator.m_DataInst = CSDataMgr.CreateDataInst(ID, type);
			m_Creators.Add(ID, cSCreator);
			break;
		}
		}
		return cSCreator;
	}

	public static CSCreator GetCreator(int ID)
	{
		if (s_Instance == null)
		{
			return null;
		}
		if (s_Instance.m_Creators.ContainsKey(ID))
		{
			return s_Instance.m_Creators[ID];
		}
		Debug.Log("No Creator [" + ID + "]");
		return null;
	}

	public CSCreator MultiCreateCreator(int TeamNum)
	{
		if (otherCreators.ContainsKey(TeamNum))
		{
			Debug.Log("This TeamNum [" + TeamNum + "] is exsit");
			return null;
		}
		CSCreator cSCreator = null;
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = base.transform;
		gameObject.name = "Team " + TeamNum + " Managed Creator";
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		CSMgCreator cSMgCreator = gameObject.AddComponent<CSMgCreator>();
		cSCreator = cSMgCreator;
		CSConst.CreatorType type = CSConst.CreatorType.Managed;
		cSCreator.m_DataInst = CSDataMgr.CreateDataInst(TeamNum, type);
		cSMgCreator.m_Clod = CSClodsMgr.CreateClod(TeamNum);
		cSMgCreator.teamNum = TeamNum;
		otherCreators.Add(TeamNum, cSCreator);
		return cSCreator;
	}

	public CSCreator MultiGetOtherCreator(int TeamNum, bool createNewIfNone = true)
	{
		if (s_Instance == null)
		{
			return null;
		}
		if (s_Instance.otherCreators.ContainsKey(TeamNum))
		{
			return s_Instance.otherCreators[TeamNum];
		}
		if (createNewIfNone)
		{
			return MultiCreateCreator(TeamNum);
		}
		return null;
	}

	private void OnDirtyVoxel(Vector3 pos, byte terrainType)
	{
		foreach (KeyValuePair<int, CSCreator> creator in m_Creators)
		{
			CSMgCreator cSMgCreator = creator.Value as CSMgCreator;
			if (!(cSMgCreator == null) && cSMgCreator.Assembly != null && cSMgCreator.Assembly.InLargestRange(pos) && cSMgCreator.m_Clod != null)
			{
				Vector3 zero = Vector3.zero;
				zero = ((!Physics.Raycast(pos + new Vector3(0f, 1f, 0f), Vector3.down, out var hitInfo, 2f, 4096)) ? pos : hitInfo.point);
				if (!FarmManager.Instance.mPlantHelpMap.ContainsKey(new IntVec3(pos)))
				{
					cSMgCreator.m_Clod.AddClod(zero);
				}
				else
				{
					cSMgCreator.m_Clod.AddClod(zero, dirty: true);
				}
			}
		}
	}

	private void OnCreatePlant(FarmPlantLogic plant)
	{
		foreach (KeyValuePair<int, CSCreator> creator in m_Creators)
		{
			CSMgCreator cSMgCreator = creator.Value as CSMgCreator;
			if (!(cSMgCreator == null))
			{
				cSMgCreator.m_Clod.DirtyTheClod(plant.mPos, dirty: true);
			}
		}
	}

	private void OnRemovePlant(FarmPlantLogic plant)
	{
		foreach (KeyValuePair<int, CSCreator> creator in m_Creators)
		{
			CSMgCreator cSMgCreator = creator.Value as CSMgCreator;
			if (!(cSMgCreator == null))
			{
				cSMgCreator.m_Clod.DirtyTheClod(plant.mPos, dirty: false);
			}
		}
	}

	private void OnDigTerrain(IntVector3 pos)
	{
		foreach (KeyValuePair<int, CSCreator> creator in m_Creators)
		{
			CSMgCreator cSMgCreator = creator.Value as CSMgCreator;
			if (cSMgCreator == null)
			{
				continue;
			}
			cSMgCreator.m_Clod.DeleteClod(pos);
			VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.SafeRead(pos.x, pos.y - 1, pos.z);
			if ((vFVoxel.Type == 19 || vFVoxel.Type == 63) && vFVoxel.Volume > 128)
			{
				if (Physics.Raycast(pos, Vector3.down, out var hitInfo, 2f, 4096))
				{
					cSMgCreator.m_Clod.AddClod(hitInfo.point);
				}
				else
				{
					cSMgCreator.m_Clod.AddClod(new Vector3(pos.x, (float)pos.y - 0.7f, pos.z));
				}
			}
		}
	}

	private void OnDestroy()
	{
		CSDataMgr.Clear();
		DigTerrainManager.onDirtyVoxel -= OnDirtyVoxel;
		FarmManager.Instance.RemovePlantEvent -= OnRemovePlant;
		FarmManager.Instance.CreatePlantEvent -= OnCreatePlant;
		DigTerrainManager.onDigTerrain -= OnDigTerrain;
	}

	private void Awake()
	{
		if (s_Instance != null)
		{
			Debug.LogError("CSMain must be only one!");
		}
		else
		{
			s_Instance = this;
		}
		CSEntityLayerIndex = 13;
		m_Creators = new Dictionary<int, CSCreator>();
		if (GameConfig.IsMultiMode)
		{
			otherCreators = new Dictionary<int, CSCreator>();
		}
		CSClodMgr.Init();
		CSClodsMgr.Init();
		CSCreator cSCreator = CreateCreator(0, "Default Managed Creator");
		if (GameConfig.IsMultiMode)
		{
			cSCreator.teamNum = BaseNetwork.MainPlayer.TeamId;
			Debug.Log("Main Creator team: " + cSCreator.teamNum);
		}
		CreateCreator(10000, "Default Non-Managed Creator", CSConst.CreatorType.NoManaged);
		DigTerrainManager.onDirtyVoxel += OnDirtyVoxel;
		FarmManager.Instance.CreatePlantEvent += OnCreatePlant;
		FarmManager.Instance.RemovePlantEvent += OnRemovePlant;
		DigTerrainManager.onDigTerrain += OnDigTerrain;
		if (CSUI_MainWndCtrl.Instance != null)
		{
			CSUI_MainWndCtrl.Instance.Creator = cSCreator;
		}
		if (CSMain.InitOperatItemEvent != null)
		{
			CSMain.InitOperatItemEvent();
			CSMain.InitOperatItemEvent = null;
		}
	}

	private void OnGUI()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!PeGameMgr.IsSingle)
		{
			return;
		}
		counter++;
		if (counter > 240)
		{
			double cycleInDay = GameTime.Timer.CycleInDay;
			if (lastCycle > -2.0 && lastCycle < -0.25 && cycleInDay >= -0.25)
			{
				RefreshColonyMoney();
			}
			lastCycle = GameTime.Timer.CycleInDay;
			counter = 0;
		}
	}

	public IEnumerator SearchVaildClodForAssembly(CSAssembly assem)
	{
		if (assem == null || assem.isSearchingClod)
		{
			yield break;
		}
		assem.isSearchingClod = true;
		CSMgCreator mgCreator = assem.m_Creator as CSMgCreator;
		mgCreator.m_Clod.Clear();
		int width = Mathf.RoundToInt(assem.LargestRadius);
		int length = width;
		int height = width;
		Vector3 int_pos = new Vector3(Mathf.FloorToInt(assem.Position.x), Mathf.FloorToInt(assem.Position.y), Mathf.FloorToInt(assem.Position.z));
		Vector3 min_pos = int_pos - new Vector3(width, length, height);
		Vector3 max_pos = int_pos + new Vector3(width, length, height);
		Vector3 pos = min_pos;
		float sqrRadius = assem.LargestRadius * assem.LargestRadius;
		length *= 2;
		width *= 2;
		height *= 2;
		int raycast_count = 0;
		int break_count = 0;
		for (int i = 0; i < width; i++)
		{
			int j = 0;
			while (j < length)
			{
				Vector3 prv_pos = Vector3.zero;
				VFVoxel prv_voxel = default(VFVoxel);
				for (int k = 0; k < height; k++)
				{
					pos = min_pos + new Vector3(i, k, j);
					if ((pos - int_pos).sqrMagnitude > sqrRadius)
					{
						continue;
					}
					VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
					if (voxel.Volume < 128 && prv_voxel.Volume > 128 && (prv_voxel.Type == 19 || prv_voxel.Type == 63))
					{
						Vector3 clod_pos = Vector3.zero;
						if (Physics.Raycast(new Vector3(prv_pos.x, prv_pos.y + 1f, prv_pos.z), Vector3.down, out var rch, 2f, 4096))
						{
							raycast_count++;
							clod_pos = rch.point;
						}
						else
						{
							clod_pos = new Vector3(prv_pos.x, prv_pos.y + 0.4f, prv_pos.z);
						}
						if (!FarmManager.Instance.mPlantHelpMap.ContainsKey(new IntVec3(prv_pos)))
						{
							mgCreator.m_Clod.AddClod(clod_pos);
						}
						else
						{
							mgCreator.m_Clod.AddClod(clod_pos, dirty: true);
						}
					}
					prv_pos = pos;
					prv_voxel = voxel;
				}
				if (break_count >= 30 || raycast_count >= 30)
				{
					raycast_count = 0;
					break_count = 0;
					yield return 0;
					if (assem == null)
					{
						yield break;
					}
				}
				j++;
				break_count++;
			}
		}
		if (assem != null)
		{
			assem.isSearchingClod = false;
		}
		yield return 0;
	}

	public static void SinglePlayerCheckClod()
	{
		if (!(s_MgCreator == null) && s_MgCreator.Assembly != null && !s_MgCreator.Assembly.isSearchingClod)
		{
			s_Instance.StartCoroutine(s_Instance.SearchVaildClodForAssembly(s_MgCreator.Assembly));
		}
	}

	public static void RemoveNpc(PeEntity npc)
	{
		CSMgCreator cSMgCreator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return;
			}
			cSMgCreator = MultiColonyManager.GetCreator(networkInterface.TeamId);
		}
		else
		{
			cSMgCreator = s_MgCreator;
		}
		if (cSMgCreator != null)
		{
			cSMgCreator.RemoveNpc(npc);
		}
	}

	public static List<PeEntity> GetCSNpcs()
	{
		List<PeEntity> list = new List<PeEntity>();
		CSPersonnel[] npcs = s_MgCreator.GetNpcs();
		foreach (CSPersonnel cSPersonnel in npcs)
		{
			list.Add(cSPersonnel.NPC);
		}
		return list;
	}

	public static List<PeEntity> GetCSNpcs(CSCreator creator)
	{
		List<PeEntity> list = new List<PeEntity>();
		CSPersonnel[] npcs = creator.GetNpcs();
		foreach (CSPersonnel cSPersonnel in npcs)
		{
			list.Add(cSPersonnel.NPC);
		}
		return list;
	}

	public static List<PeEntity> GetCSBuildings(CSCreator creator)
	{
		List<PeEntity> list = new List<PeEntity>();
		CSMgCreator cSMgCreator = creator as CSMgCreator;
		if (cSMgCreator != null)
		{
			foreach (CSBuildingLogic value in cSMgCreator.allBuildingLogic.Values)
			{
				list.Add(value._peEntity);
			}
		}
		return list;
	}

	public static List<PeEntity> GetCSMainNpc()
	{
		List<PeEntity> list = new List<PeEntity>();
		foreach (CSPersonnel mainNpc in s_MgCreator.MainNpcs)
		{
			list.Add(mainNpc.NPC);
		}
		return list;
	}

	public static List<PeEntity> GetCSRandomNpc()
	{
		List<PeEntity> list = new List<PeEntity>();
		foreach (CSPersonnel randomNpc in s_MgCreator.RandomNpcs)
		{
			list.Add(randomNpc.NPC);
		}
		return list;
	}

	public static bool HasBuilding(int protoId, CSCreator creator)
	{
		CSMgCreator cSMgCreator = creator as CSMgCreator;
		if (cSMgCreator != null)
		{
			foreach (CSBuildingLogic value in cSMgCreator.allBuildingLogic.Values)
			{
				if (value != null && value.protoId == protoId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasBuilding(int protoId, CSCreator creator, out Vector3 pos)
	{
		CSMgCreator cSMgCreator = creator as CSMgCreator;
		pos = Vector3.zero;
		if (cSMgCreator != null)
		{
			foreach (CSBuildingLogic value in cSMgCreator.allBuildingLogic.Values)
			{
				if (value != null && value.protoId == protoId)
				{
					pos = value.transform.position;
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasCSAssembly()
	{
		if (s_MgCreator == null)
		{
			return false;
		}
		return s_MgCreator.Assembly != null;
	}

	public static bool GetAssemblyPos(out Vector3 pos)
	{
		pos = default(Vector3);
		if (HasCSAssembly())
		{
			pos = s_MgCreator.Assembly.Position;
			return true;
		}
		pos = Vector3.zero;
		return false;
	}

	public static CSAssembly GetAssemblyEntity()
	{
		return s_MgCreator.Assembly;
	}

	public static CSBuildingLogic GetAssemblyLogic()
	{
		CSAssembly assembly = s_MgCreator.Assembly;
		if (assembly == null)
		{
			return null;
		}
		if (assembly.gameLogic == null)
		{
			return null;
		}
		return assembly.gameLogic.GetComponent<CSBuildingLogic>();
	}

	public bool IsInAssemblyArea(Vector3 pos)
	{
		if (s_MgCreator.Assembly != null && s_MgCreator.Assembly.InRange(pos))
		{
			return true;
		}
		if (PeGameMgr.IsMulti)
		{
			foreach (CSCreator value in otherCreators.Values)
			{
				if (value.Assembly != null && value.Assembly.InRange(pos))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsInOtherAssemblyArea(Vector3 pos)
	{
		if (PeGameMgr.IsMulti)
		{
			foreach (CSCreator value in otherCreators.Values)
			{
				if (value.Assembly != null && value.Assembly.InRange(pos))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<CSAssembly> GetAllAssemblies()
	{
		List<CSAssembly> list = new List<CSAssembly>();
		if (s_MgCreator.Assembly != null)
		{
			list.Add(s_MgCreator.Assembly);
		}
		if (PeGameMgr.IsMulti)
		{
			foreach (CSCreator value in otherCreators.Values)
			{
				if (value.Assembly != null)
				{
					list.Add(value.Assembly);
				}
			}
		}
		return list;
	}

	public static int GetEmptyBedRoom()
	{
		if (!HasCSAssembly())
		{
			return 0;
		}
		List<CSCommon> dwellings = s_MgCreator.Assembly.Dwellings;
		if (dwellings == null)
		{
			return 0;
		}
		int num = 0;
		foreach (CSCommon item in dwellings)
		{
			if (item is CSDwellings cSDwellings)
			{
				num += cSDwellings.GetEmptySpace();
			}
		}
		return num;
	}

	public static CSPersonnel GetColonyNpc(int id)
	{
		CSMgCreator cSMgCreator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(id);
			if (networkInterface == null)
			{
				return null;
			}
			cSMgCreator = MultiColonyManager.GetCreator(networkInterface.TeamId);
		}
		else
		{
			cSMgCreator = s_MgCreator;
		}
		return cSMgCreator.GetNpc(id);
	}

	public static bool SetNpcFollower(PeEntity npc)
	{
		CSPersonnel colonyNpc = GetColonyNpc(npc.Id);
		if (colonyNpc != null)
		{
			if (colonyNpc.TrySetOccupation(4))
			{
				return npc.SetFollower(bFlag: true);
			}
			return false;
		}
		return npc.SetFollower(bFlag: true);
	}

	public static bool IsColonyNpc(int npcId)
	{
		return GetColonyNpc(npcId) != null;
	}

	public static CSMedicalCheck FindCheckMachine(PeEntity npc)
	{
		CSMgCreator cSMgCreator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return null;
			}
			cSMgCreator = MultiColonyManager.GetCreator(networkInterface.TeamId);
		}
		else
		{
			cSMgCreator = s_MgCreator;
		}
		if (cSMgCreator.Assembly == null)
		{
			return null;
		}
		if (cSMgCreator.Assembly.MedicalCheck != null && cSMgCreator.Assembly.MedicalCheck.IsRunning)
		{
			return cSMgCreator.Assembly.MedicalCheck;
		}
		return null;
	}

	public static CSMedicalTreat FindTreatMachine(PeEntity npc)
	{
		CSMgCreator cSMgCreator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return null;
			}
			cSMgCreator = MultiColonyManager.GetCreator(networkInterface.TeamId);
		}
		else
		{
			cSMgCreator = s_MgCreator;
		}
		if (cSMgCreator.Assembly == null)
		{
			return null;
		}
		if (cSMgCreator.Assembly.MedicalTreat != null && cSMgCreator.Assembly.MedicalTreat.IsRunning)
		{
			return cSMgCreator.Assembly.MedicalTreat;
		}
		return null;
	}

	public static CSMedicalTent FindTentMachine(PeEntity npc)
	{
		CSMgCreator cSMgCreator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return null;
			}
			cSMgCreator = MultiColonyManager.GetCreator(networkInterface.TeamId);
		}
		else
		{
			cSMgCreator = s_MgCreator;
		}
		if (cSMgCreator.Assembly == null)
		{
			return null;
		}
		if (cSMgCreator.Assembly.MedicalTent != null && cSMgCreator.Assembly.MedicalTent.IsRunning)
		{
			return cSMgCreator.Assembly.MedicalTent;
		}
		return null;
	}

	public static CSMedicalCheck FindMedicalCheck(out bool isReady, PeEntity npc)
	{
		isReady = false;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return null;
			}
			CSMgCreator creator = MultiColonyManager.GetCreator(networkInterface.TeamId);
			if (creator.Assembly == null)
			{
				return null;
			}
			if (creator.Assembly.MedicalCheck == null)
			{
				return null;
			}
			CSMedicalCheck medicalCheck = creator.Assembly.MedicalCheck;
			if (!medicalCheck.IsRunning)
			{
				return null;
			}
			isReady = medicalCheck.IsReady(npc);
			if (npc.GetCmpt<NpcCmpt>().illAbnormals != null && npc.GetCmpt<NpcCmpt>().illAbnormals.Count > 0)
			{
				medicalCheck._ColonyObj._Network.RPCServer(EPacketType.PT_CL_CHK_FindMachine, npc.Id);
			}
			return medicalCheck;
		}
		if (s_MgCreator.Assembly == null)
		{
			return null;
		}
		if (s_MgCreator.Assembly.MedicalCheck == null)
		{
			return null;
		}
		CSMedicalCheck medicalCheck2 = s_MgCreator.Assembly.MedicalCheck;
		if (!medicalCheck2.IsRunning)
		{
			return null;
		}
		isReady = medicalCheck2.IsReady(npc);
		medicalCheck2.AppointCheck(npc);
		return medicalCheck2;
	}

	public static bool TryGetCheck(PeEntity npc)
	{
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return false;
			}
			CSMgCreator creator = MultiColonyManager.GetCreator(networkInterface.TeamId);
			if (creator.Assembly == null)
			{
				return false;
			}
			if (creator.Assembly.MedicalCheck == null)
			{
				return false;
			}
			CSMedicalCheck medicalCheck = creator.Assembly.MedicalCheck;
			if (!medicalCheck.IsRunning)
			{
				return false;
			}
			medicalCheck._ColonyObj._Network.RPCServer(EPacketType.PT_CL_CHK_TryStart, npc.Id);
			return false;
		}
		if (s_MgCreator.Assembly == null)
		{
			return false;
		}
		if (s_MgCreator.Assembly.MedicalCheck == null)
		{
			return false;
		}
		CSMedicalCheck medicalCheck2 = s_MgCreator.Assembly.MedicalCheck;
		if (!medicalCheck2.IsRunning)
		{
			return false;
		}
		return medicalCheck2.StartCheck(npc);
	}

	public static CSMedicalTreat FindMedicalTreat(out bool isReady, PeEntity npc)
	{
		isReady = false;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return null;
			}
			CSMgCreator creator = MultiColonyManager.GetCreator(networkInterface.TeamId);
			if (creator.Assembly == null)
			{
				return null;
			}
			if (creator.Assembly.MedicalTreat == null)
			{
				return null;
			}
			CSMedicalTreat medicalTreat = creator.Assembly.MedicalTreat;
			if (!medicalTreat.IsRunning)
			{
				return null;
			}
			isReady = medicalTreat.IsReady(npc);
			medicalTreat._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRT_FindMachine, npc.Id);
			return medicalTreat;
		}
		if (s_MgCreator.Assembly == null)
		{
			return null;
		}
		if (s_MgCreator.Assembly.MedicalTreat == null)
		{
			return null;
		}
		CSMedicalTreat medicalTreat2 = s_MgCreator.Assembly.MedicalTreat;
		if (!medicalTreat2.IsRunning)
		{
			return null;
		}
		isReady = medicalTreat2.IsReady(npc);
		medicalTreat2.AppointTreat(npc);
		return medicalTreat2;
	}

	public static bool TryGetTreat(PeEntity npc)
	{
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return false;
			}
			CSMgCreator creator = MultiColonyManager.GetCreator(networkInterface.TeamId);
			if (creator.Assembly == null)
			{
				return false;
			}
			if (creator.Assembly.MedicalTreat == null)
			{
				return false;
			}
			CSMedicalTreat medicalTreat = creator.Assembly.MedicalTreat;
			if (!medicalTreat.IsRunning)
			{
				return false;
			}
			medicalTreat._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRT_TryStart, npc.Id);
			return false;
		}
		if (s_MgCreator.Assembly == null)
		{
			return false;
		}
		if (s_MgCreator.Assembly.MedicalTreat == null)
		{
			return false;
		}
		CSMedicalTreat medicalTreat2 = s_MgCreator.Assembly.MedicalTreat;
		if (!medicalTreat2.IsRunning)
		{
			return false;
		}
		return medicalTreat2.StartTreat(npc);
	}

	public static CSMedicalTent FindMedicalTent(out bool isReady, PeEntity npc, out Sickbed sickBed)
	{
		isReady = false;
		sickBed = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return null;
			}
			CSMgCreator creator = MultiColonyManager.GetCreator(networkInterface.TeamId);
			if (creator.Assembly == null)
			{
				return null;
			}
			if (creator.Assembly.MedicalTent == null)
			{
				return null;
			}
			CSMedicalTent medicalTent = creator.Assembly.MedicalTent;
			if (!medicalTent.IsRunning)
			{
				return null;
			}
			sickBed = medicalTent.CheckNpcBed(npc);
			if (sickBed == null)
			{
				medicalTent._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TET_FindMachine, npc.Id);
			}
			else
			{
				isReady = true;
			}
			return medicalTent;
		}
		if (s_MgCreator.Assembly == null)
		{
			return null;
		}
		if (s_MgCreator.Assembly.MedicalTent == null)
		{
			return null;
		}
		CSMedicalTent medicalTent2 = s_MgCreator.Assembly.MedicalTent;
		if (!medicalTent2.IsRunning)
		{
			return null;
		}
		isReady = medicalTent2.IsReady(npc, out sickBed);
		medicalTent2.AppointTent(npc);
		return medicalTent2;
	}

	public static bool TryGetTent(PeEntity npc)
	{
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return false;
			}
			CSMgCreator creator = MultiColonyManager.GetCreator(networkInterface.TeamId);
			if (creator.Assembly == null)
			{
				return false;
			}
			if (creator.Assembly.MedicalTent == null)
			{
				return false;
			}
			CSMedicalTent medicalTent = creator.Assembly.MedicalTent;
			if (!medicalTent.IsRunning)
			{
				return false;
			}
			medicalTent._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TET_TryStart, npc.Id);
			return false;
		}
		if (s_MgCreator.Assembly == null)
		{
			return false;
		}
		if (s_MgCreator.Assembly.MedicalTent == null)
		{
			return false;
		}
		CSMedicalTent medicalTent2 = s_MgCreator.Assembly.MedicalTent;
		if (!medicalTent2.IsRunning)
		{
			return false;
		}
		return medicalTent2.StartTent(npc);
	}

	public static List<CSTreatment> GetTreatmentList()
	{
		if (PeGameMgr.IsMulti)
		{
			return s_MgCreator.m_TreatmentList;
		}
		return s_MgCreator.m_TreatmentList;
	}

	public static void KickOutFromHospital(PeEntity npc)
	{
		CSMgCreator cSMgCreator = null;
		if (PeGameMgr.IsMulti)
		{
			if (PeGameMgr.IsCustom)
			{
				return;
			}
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return;
			}
			cSMgCreator = MultiColonyManager.GetCreator(networkInterface.TeamId);
		}
		else
		{
			cSMgCreator = s_MgCreator;
		}
		if (!(cSMgCreator == null) && cSMgCreator.Assembly != null)
		{
			if (cSMgCreator.Assembly.MedicalCheck != null && cSMgCreator.Assembly.MedicalCheck.IsRunning)
			{
				cSMgCreator.Assembly.MedicalCheck.RemoveDeadPatient(npc.Id);
			}
			if (cSMgCreator.Assembly.MedicalTreat != null && cSMgCreator.Assembly.MedicalTreat.IsRunning)
			{
				cSMgCreator.Assembly.MedicalTreat.RemoveDeadPatient(npc.Id);
			}
			if (cSMgCreator.Assembly.MedicalTent != null && cSMgCreator.Assembly.MedicalTent.IsRunning)
			{
				cSMgCreator.Assembly.MedicalTent.RemoveDeadPatient(npc.Id);
			}
			cSMgCreator.m_TreatmentList.RemoveAll((CSTreatment it) => it.npcId == npc.Id);
		}
	}

	public static void StopTraining(PeEntity npc)
	{
		CSMgCreator cSMgCreator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return;
			}
			cSMgCreator = MultiColonyManager.GetCreator(networkInterface.TeamId);
		}
		else
		{
			cSMgCreator = s_MgCreator;
		}
		if (!(cSMgCreator == null) && cSMgCreator.Assembly != null)
		{
		}
	}

	public static List<CSPersonnel> GetInstructorList()
	{
		CSMgCreator cSMgCreator = null;
		cSMgCreator = s_MgCreator;
		List<CSPersonnel> list = new List<CSPersonnel>();
		if (s_MgCreator.Assembly == null)
		{
			return null;
		}
		CSTraining trainingCenter = s_MgCreator.Assembly.TrainingCenter;
		if (trainingCenter == null)
		{
			return null;
		}
		foreach (int instructor in trainingCenter.InstructorList)
		{
			CSPersonnel npc = cSMgCreator.GetNpc(instructor);
			if (npc != null)
			{
				list.Add(npc);
			}
		}
		return list;
	}

	public static List<CSPersonnel> GetTraineeList()
	{
		CSMgCreator cSMgCreator = null;
		cSMgCreator = s_MgCreator;
		List<CSPersonnel> list = new List<CSPersonnel>();
		if (s_MgCreator.Assembly == null)
		{
			return null;
		}
		CSTraining trainingCenter = s_MgCreator.Assembly.TrainingCenter;
		if (trainingCenter == null)
		{
			return null;
		}
		foreach (int trainee in trainingCenter.TraineeList)
		{
			CSPersonnel npc = cSMgCreator.GetNpc(trainee);
			if (npc != null)
			{
				list.Add(npc);
			}
		}
		return list;
	}

	public static int GetInstructorCount()
	{
		if (s_MgCreator.Assembly == null)
		{
			return 0;
		}
		return s_MgCreator.Assembly.TrainingCenter?.InstructorList.Count ?? 0;
	}

	public static int GetTraineeCount()
	{
		if (s_MgCreator.Assembly == null)
		{
			return 0;
		}
		return s_MgCreator.Assembly.TrainingCenter?.TraineeList.Count ?? 0;
	}

	public static int GetInstructorMax()
	{
		return 8;
	}

	public static int GetTraineeMax()
	{
		return 8;
	}

	public static void AddTradeNpc(int npcId, List<int> storeIdList)
	{
		if (!s_MgCreator.AddedNpcId.Contains(npcId))
		{
			s_MgCreator.AddStoreId(storeIdList);
			s_MgCreator.AddedNpcId.Add(npcId);
		}
	}

	public void RefreshColonyMoney()
	{
		s_MgCreator.RefreshMoney();
	}
}
