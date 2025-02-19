using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ColonyTent : ColonyBase
{
	public const int MAX_WORKER_COUNT = 1;

	private CSTentData _MyData;

	public List<ColonyNpc> patientNpc = new List<ColonyNpc>();

	public override int MaxWorkerCount => 1;

	private Sickbed[] AllBeds
	{
		get
		{
			return _MyData.allSickbeds;
		}
		set
		{
			_MyData.allSickbeds = value;
		}
	}

	public ColonyTent(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTentData();
		_MyData = (CSTentData)_RecordData;
		Sickbed[] allBeds = AllBeds;
		foreach (Sickbed sickbed in allBeds)
		{
			sickbed.tentBuilding = this;
		}
		LoadData();
	}

	public override void InitMyData()
	{
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public override void MyUpdate()
	{
		if (IsWorking() && IsDoctorReady())
		{
			CheckPatient();
			for (int i = 0; i < AllBeds.Count(); i++)
			{
				AllBeds[i].Update();
			}
		}
	}

	private void CheckPatient()
	{
		if (_MyData.npcIds.Count == patientNpc.Count)
		{
			return;
		}
		patientNpc.Clear();
		foreach (int npcId in _MyData.npcIds)
		{
			ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(npcId);
			if (npcByID != null)
			{
				patientNpc.Add(npcByID);
			}
		}
	}

	public float GetRestoreTime(ColonyNpc npc)
	{
		return ColonyMgr.FindTreatment(base.TeamId, npc._npcID, needTreat: true)?.restoreTime ?? 0f;
	}

	public float CountFinalTime(ColonyNpc npc)
	{
		if (Application.isEditor)
		{
			return 15f;
		}
		float restoreTime = GetRestoreTime(npc);
		return restoreTime * (1f + _worker.Values.ToList()[0].GetTentTimeSkill);
	}

	public void OnTentFinish(Sickbed sbed)
	{
		sbed.isNpcReady = false;
		sbed.StopCounter();
		patientNpc.Remove(sbed.Npc);
		_MyData.npcIds.Remove(sbed.Npc._npcID);
		_Network.RPCOthers(EPacketType.PT_CL_TET_TentFinish, sbed.npc._npcID);
		sbed.Npc = null;
		SyncSave();
	}

	public bool IsDoctorReady()
	{
		if (_worker.Values.Count == 0)
		{
			return false;
		}
		ColonyNpc colonyNpc = _worker.Values.ToArray()[0];
		return true;
	}

	public Sickbed FindEmptyBed(ColonyNpc npc)
	{
		Sickbed[] allBeds = AllBeds;
		foreach (Sickbed sickbed in allBeds)
		{
			if (sickbed.Npc == npc)
			{
				return sickbed;
			}
		}
		Sickbed[] allBeds2 = AllBeds;
		foreach (Sickbed sickbed2 in allBeds2)
		{
			if (sickbed2.Npc == null)
			{
				sickbed2.Npc = npc;
				return sickbed2;
			}
		}
		return null;
	}

	public void AddNpc(ColonyNpc npc)
	{
		if (!patientNpc.Contains(npc))
		{
			patientNpc.Add(npc);
			_MyData.npcIds.Add(npc._npcID);
			Sickbed sickbed = FindEmptyBed(npc);
			int num = -1;
			if (sickbed != null)
			{
				num = sickbed.bedId;
			}
			_Network.RPCOthers(EPacketType.PT_CL_TET_FindMachine, _MyData.npcIds.ToArray(), npc._npcID, num);
			if (patientNpc.Count <= 8)
			{
				_Network.RPCOthers(EPacketType.PT_CL_TET_SetTent, npc._npcID);
			}
		}
	}

	public void RemoveDeadNpc(int npcId)
	{
		ColonyNpc npc = ColonyNpcMgr.GetNpcByID(npcId);
		if (!patientNpc.Contains(npc))
		{
			_Network.RPCOthers(EPacketType.PT_CL_TET_RemoveDeadNpc, npcId);
			return;
		}
		if (patientNpc.FindIndex((ColonyNpc it) => it == npc) < 8)
		{
			patientNpc.Remove(npc);
			_MyData.npcIds.Remove(npc._npcID);
			Sickbed[] allBeds = AllBeds;
			foreach (Sickbed sickbed in allBeds)
			{
				if (sickbed.npc == npc)
				{
					sickbed.Npc = null;
					sickbed.StopCounter();
					sickbed.isNpcReady = false;
					sickbed.occupied = false;
				}
			}
			_Network.RPCOthers(EPacketType.PT_CL_TET_RemoveDeadNpc, npc._npcID);
			if (patientNpc.Count >= 8)
			{
				_Network.RPCOthers(EPacketType.PT_CL_TET_SetTent, patientNpc[7]._npcID);
			}
		}
		else
		{
			patientNpc.Remove(npc);
			_MyData.npcIds.Remove(npc._npcID);
			_Network.RPCOthers(EPacketType.PT_CL_TET_RemoveDeadNpc, npc._npcID);
		}
		SyncSave();
		ColonyMgr.RemoveNpcTreatment(base.TeamId, npc._npcID);
	}

	public void Start(ColonyNpc npc)
	{
		Sickbed[] allBeds = AllBeds;
		foreach (Sickbed sickbed in allBeds)
		{
			if (sickbed.npc == npc)
			{
				sickbed.isNpcReady = true;
				_Network.RPCOthers(EPacketType.PT_CL_TET_TryStart, npc._npcID);
				break;
			}
		}
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.npcIds.Count);
		for (int i = 0; i < _MyData.npcIds.Count; i++)
		{
			BufferHelper.Serialize(writer, _MyData.npcIds[i]);
		}
		for (int j = 0; j < 8; j++)
		{
			Sickbed sickbed = AllBeds[j];
			BufferHelper.Serialize(writer, sickbed.npcId);
			BufferHelper.Serialize(writer, sickbed.m_CurTime);
			BufferHelper.Serialize(writer, sickbed.m_Time);
			BufferHelper.Serialize(writer, sickbed.isNpcReady);
			BufferHelper.Serialize(writer, sickbed.IsOccupied);
		}
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			_MyData.npcIds.Add(BufferHelper.ReadInt32(reader));
		}
		for (int j = 0; j < 8; j++)
		{
			Sickbed sickbed = AllBeds[j];
			sickbed.npcId = BufferHelper.ReadInt32(reader);
			sickbed.m_CurTime = BufferHelper.ReadSingle(reader);
			sickbed.m_Time = BufferHelper.ReadSingle(reader);
			sickbed.isNpcReady = BufferHelper.ReadBoolean(reader);
			sickbed.occupied = BufferHelper.ReadBoolean(reader);
		}
	}

	public byte[] AllDataToByte()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		CombomData(writer);
		return memoryStream.ToArray();
	}

	public override void DestroySomeData()
	{
		base.DestroySomeData();
		patientNpc.Clear();
		_MyData.npcIds.Clear();
		Sickbed[] allBeds = AllBeds;
		foreach (Sickbed sickbed in allBeds)
		{
			sickbed.Npc = null;
			sickbed.StopCounter();
			sickbed.isNpcReady = false;
			sickbed.occupied = false;
		}
	}
}
