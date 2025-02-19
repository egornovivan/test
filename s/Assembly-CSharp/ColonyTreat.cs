using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class ColonyTreat : ColonyBase
{
	private const float DEFAULT_TREAT_CHANCE = 1f;

	public const int MAX_WORKER_COUNT = 1;

	private CSTreatData _MyData;

	public List<ColonyNpc> patientNpc = new List<ColonyNpc>();

	private CSTreatment treatmentInUse;

	public override int MaxWorkerCount => 1;

	private bool IsNpcReady
	{
		get
		{
			return _MyData.isNpcReady;
		}
		set
		{
			_MyData.isNpcReady = value;
		}
	}

	private float m_CurTime
	{
		get
		{
			return _MyData.m_CurTime;
		}
		set
		{
			_MyData.m_CurTime = value;
		}
	}

	private float m_Time
	{
		get
		{
			return _MyData.m_Time;
		}
		set
		{
			_MyData.m_Time = value;
		}
	}

	private int objId
	{
		get
		{
			return _MyData.m_ObjID;
		}
		set
		{
			_MyData.m_ObjID = value;
		}
	}

	public ColonyTreat(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTreatData();
		_MyData = (CSTreatData)_RecordData;
		LoadData();
	}

	public override void InitMyData()
	{
		if (objId == -1)
		{
		}
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
		if (!IsWorking())
		{
			return;
		}
		CheckPatient();
		if (IsDoctorReady() && IsPatientAndItemReady())
		{
			if (m_Time == -1f)
			{
				StartCounter(patientNpc[0]);
			}
			if (m_CurTime < m_Time)
			{
				m_CurTime += 1f;
			}
			if (m_CurTime >= m_Time)
			{
				TreatFinish(patientNpc[0]);
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

	public bool IsDoctorReady()
	{
		if (_worker.Values.Count == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsPatientAndItemReady()
	{
		if (!IsNpcReady || patientNpc.Count == 0)
		{
			return false;
		}
		if (treatmentInUse == null)
		{
			treatmentInUse = ColonyMgr.FindTreatment(base.TeamId, patientNpc[0]._npcID, needTreat: true);
			if (treatmentInUse == null)
			{
				patientNpc.Remove(patientNpc[0]);
				_MyData.npcIds.Remove(patientNpc[0]._npcID);
				_Network.RPCOthers(EPacketType.PT_CL_TRT_ResetNpcToCheck, patientNpc[0]._npcID);
				return false;
			}
		}
		if (m_CurTime < 0f && m_Time < 0f)
		{
			return true;
		}
		return true;
	}

	public void StartCounter(ColonyNpc npc)
	{
		m_CurTime = 0f;
		m_Time = CountFinalTime(npc);
		_Network.RPCOthers(EPacketType.PT_CL_TRT_StartTreatCounter);
	}

	public void DeleteMedicineItem()
	{
		ItemManager.RemoveItem(objId);
		_Network.RPCOthers(EPacketType.PT_CL_TRT_DeleteItem, objId);
		objId = -1;
	}

	private float CountFinalTime(ColonyNpc npc)
	{
		if (Application.isEditor)
		{
			return 15f;
		}
		float treatTime = GetTreatTime(npc);
		return treatTime * (1f + _worker.Values.ToList()[0].GetTreatTimeSkill);
	}

	private float GetTreatTime(ColonyNpc npc)
	{
		if (treatmentInUse.npcId != npc._npcID)
		{
			treatmentInUse = ColonyMgr.FindTreatment(base.TeamId, npc._npcID, needTreat: true);
		}
		if (treatmentInUse != null)
		{
			return treatmentInUse.treatTime;
		}
		return 0f;
	}

	private void TreatFinish(ColonyNpc npc)
	{
		m_CurTime = -1f;
		m_Time = -1f;
		bool flag = false;
		System.Random random = new System.Random();
		if (random.NextDouble() > (double)(1f + _worker.Values.ToList()[0].GetTreatChanceSkill))
		{
			flag = false;
		}
		else if (treatmentInUse != null && patientNpc[0]._refNpc.AbnormalModule != null)
		{
			flag = (patientNpc[0]._refNpc.AbnormalModule.TreatedAbnormal().Contains(treatmentInUse.abnormalId) ? true : false);
		}
		if (flag)
		{
			if (treatmentInUse != null)
			{
				treatmentInUse.needTreatTimes--;
				if (treatmentInUse.needTreatTimes <= 0)
				{
					ColonyMgr.UpdateTreatment(base.TeamId);
				}
			}
		}
		else
		{
			ColonyMgr.RemoveNpcTreatment(base.TeamId, patientNpc[0]._npcID);
		}
		patientNpc.Remove(npc);
		_MyData.npcIds.RemoveAll((int it) => it == npc._npcID);
		IsNpcReady = false;
		treatmentInUse = null;
		_Network.RPCOthers(EPacketType.PT_CL_TRT_TreatFinish, npc._npcID, flag);
	}

	public void AddNpc(ColonyNpc npc)
	{
		CSTreatment cSTreatment = ColonyMgr.FindTreatment(base.TeamId, npc._npcID, needTreat: true);
		if (cSTreatment != null)
		{
			if (!patientNpc.Contains(npc))
			{
				patientNpc.Add(npc);
				_MyData.npcIds.Add(npc._npcID);
				_Network.RPCOthers(EPacketType.PT_CL_TRT_FindMachine, _MyData.npcIds.ToArray());
				if (patientNpc.Count == 1)
				{
					treatmentInUse = cSTreatment;
					_Network.RPCOthers(EPacketType.PT_CL_TRT_SetTreat, treatmentInUse);
				}
			}
		}
		else
		{
			patientNpc.Remove(npc);
			_MyData.npcIds.Remove(npc._npcID);
			_Network.RPCOthers(EPacketType.PT_CL_TRT_ResetNpcToCheck, npc._npcID);
		}
	}

	public void RemoveDeadPatient(int npcId)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(npcId);
		if (!patientNpc.Contains(npcByID) || patientNpc.Count == 0)
		{
			_Network.RPCOthers(EPacketType.PT_CL_TRT_RemoveDeadNpc, npcId);
			return;
		}
		if (patientNpc[0] == npcByID)
		{
			IsNpcReady = false;
			patientNpc.Remove(npcByID);
			_MyData.npcIds.Remove(npcByID._npcID);
			_Network.RPCOthers(EPacketType.PT_CL_TRT_RemoveDeadNpc, npcByID._npcID);
			if (patientNpc.Count > 0)
			{
				treatmentInUse = ColonyMgr.FindTreatment(base.TeamId, patientNpc[0]._npcID, needTreat: true);
				if (treatmentInUse != null)
				{
					_Network.RPCOthers(EPacketType.PT_CL_TRT_SetTreat, treatmentInUse);
				}
			}
		}
		else
		{
			patientNpc.Remove(npcByID);
			_MyData.npcIds.Remove(npcByID._npcID);
			_Network.RPCOthers(EPacketType.PT_CL_TRT_RemoveDeadNpc, npcByID._npcID);
		}
		ColonyMgr.RemoveNpcTreatment(base.TeamId, npcByID._npcID);
	}

	public void SetItem(bool isMis, int instanceId, bool inOrOut, int index, int tabindex, Player sender)
	{
		if (inOrOut)
		{
			ItemObject itemByID = ItemManager.GetItemByID(instanceId);
			if (objId >= 0)
			{
				ItemObject itemByID2 = ItemManager.GetItemByID(objId);
				objId = instanceId;
				sender.Package.SetItem(itemByID2, index, tabindex, itemByID.protoData.category);
			}
			else
			{
				objId = instanceId;
				sender.Package.RemoveItem(itemByID);
			}
		}
		else
		{
			ItemObject itemByID3 = ItemManager.GetItemByID(objId);
			if (itemByID3 == null)
			{
				return;
			}
			sender.Package.SetItem(itemByID3, index, tabindex, itemByID3.protoData.category);
			objId = -1;
		}
		sender.SyncPackageIndex();
		if (objId != -1)
		{
			ItemObject itemByID4 = ItemManager.GetItemByID(objId);
			if (itemByID4 != null)
			{
				ChannelNetwork.SyncItem(_Network.WorldId, itemByID4);
			}
		}
		_Network.RPCOthers(EPacketType.PT_CL_TRT_SetItem, objId, inOrOut);
	}

	public void Start(ColonyNpc npc)
	{
		if (patientNpc.Count > 0 && patientNpc[0] == npc)
		{
			IsNpcReady = true;
			_Network.RPCOthers(EPacketType.PT_CL_TRT_TryStart, npc._npcID);
		}
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.m_ObjID);
		BufferHelper.Serialize(writer, _MyData.npcIds.Count);
		for (int i = 0; i < _MyData.npcIds.Count; i++)
		{
			BufferHelper.Serialize(writer, _MyData.npcIds[i]);
		}
		BufferHelper.Serialize(writer, _MyData.m_CurTime);
		BufferHelper.Serialize(writer, _MyData.m_Time);
		BufferHelper.Serialize(writer, _MyData.isNpcReady);
		BufferHelper.Serialize(writer, _MyData.occupied);
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		_MyData.m_ObjID = BufferHelper.ReadInt32(reader);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			_MyData.npcIds.Add(BufferHelper.ReadInt32(reader));
		}
		_MyData.m_CurTime = BufferHelper.ReadSingle(reader);
		_MyData.m_Time = BufferHelper.ReadSingle(reader);
		_MyData.isNpcReady = BufferHelper.ReadBoolean(reader);
		_MyData.occupied = BufferHelper.ReadBoolean(reader);
	}

	public override void DestroySomeData()
	{
		base.DestroySomeData();
		patientNpc.Clear();
		_MyData.npcIds.Clear();
		m_CurTime = -1f;
		m_Time = -1f;
		IsNpcReady = false;
		treatmentInUse = null;
	}
}
