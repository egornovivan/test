using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ColonyCheck : ColonyBase
{
	private const float DEFAULT_CHECK_CHANGCE = 1f;

	public const int MAX_WORKER_COUNT = 1;

	private CSCheckData _MyData;

	public List<ColonyNpc> patientNpc = new List<ColonyNpc>();

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

	public ColonyCheck(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSCheckData();
		_MyData = (CSCheckData)_RecordData;
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
		if (!IsWorking())
		{
			return;
		}
		CheckPatient();
		if (IsDoctorReady() && IsPatientReady())
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
				CheckFinish(patientNpc[0]);
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

	public void StartCounter(ColonyNpc npc)
	{
		m_CurTime = 0f;
		m_Time = CountFinalTime(npc);
	}

	private float CountFinalTime(ColonyNpc npc)
	{
		float diagnoseTime = GetDiagnoseTime(npc);
		return diagnoseTime * (1f + _worker.Values.ToList()[0].GetDiagnoseTimeSkill);
	}

	public float GetDiagnoseTime(ColonyNpc npc)
	{
		float num = 0f;
		if (npc._refNpc.AbnormalModule != null)
		{
			List<int> list = npc._refNpc.AbnormalModule.TreatedAbnormal().ToList();
			if (list.Count > 0)
			{
				num = AbnormalTypeTreatData.GetDiagnosingTime(list[0]);
				for (int i = 1; i < list.Count; i++)
				{
					float diagnosingTime = AbnormalTypeTreatData.GetDiagnosingTime(list[i]);
					if (diagnosingTime > num)
					{
						num = diagnosingTime;
					}
				}
			}
		}
		return num;
	}

	private void CheckFinish(ColonyNpc checkNpc)
	{
		m_CurTime = -1f;
		m_Time = -1f;
		List<CSTreatment> list = new List<CSTreatment>();
		Random random = new Random();
		if (random.NextDouble() > (double)(1f + _worker.Values.ToList()[0].GetDiagnoseChanceSkill))
		{
			list.Add(CSTreatment.GetRandomTreatment(checkNpc));
		}
		else
		{
			list = CSTreatment.CreateTreatment(checkNpc);
		}
		ColonyMgr.RemoveNpcTreatment(base.TeamId, checkNpc._npcID);
		if (list != null && list.Count > 0)
		{
			ColonyMgr.AddTreatment(_Network.TeamId, list);
		}
		patientNpc.Remove(checkNpc);
		_MyData.npcIds.Remove(checkNpc._npcID);
		IsNpcReady = false;
		_Network.RPCOthers(EPacketType.PT_CL_CHK_CheckFinish, checkNpc._npcID, list.ToArray());
	}

	public bool IsDoctorReady()
	{
		if (_worker.Values.Count == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsPatientReady()
	{
		return patientNpc.Count > 0 && IsNpcReady;
	}

	public void AddNpc(ColonyNpc npc)
	{
		if (!patientNpc.Contains(npc))
		{
			patientNpc.Add(npc);
			_MyData.npcIds.Add(npc._npcID);
			_Network.RPCOthers(EPacketType.PT_CL_CHK_FindMachine, _MyData.npcIds.ToArray());
			if (patientNpc.Count == 1)
			{
				_Network.RPCOthers(EPacketType.PT_CL_CHK_SetDiagnose);
			}
		}
	}

	public void RemoveDeadPatient(int npcId)
	{
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(npcId);
		if (!patientNpc.Contains(npcByID) || patientNpc.Count == 0)
		{
			_Network.RPCOthers(EPacketType.PT_CL_CHK_RemoveDeadNpc, npcId);
		}
		else if (patientNpc[0] == npcByID)
		{
			patientNpc.Remove(npcByID);
			_MyData.npcIds.Remove(npcByID._npcID);
			_Network.RPCOthers(EPacketType.PT_CL_CHK_RemoveDeadNpc, npcByID._npcID);
			IsNpcReady = false;
			if (patientNpc.Count > 0)
			{
				_Network.RPCOthers(EPacketType.PT_CL_CHK_SetDiagnose);
			}
		}
		else
		{
			patientNpc.Remove(npcByID);
			_MyData.npcIds.Remove(npcByID._npcID);
			_Network.RPCOthers(EPacketType.PT_CL_CHK_RemoveDeadNpc, npcByID._npcID);
		}
	}

	public void Start(ColonyNpc npc)
	{
		if (patientNpc.Count > 0 && patientNpc[0] == npc)
		{
			_MyData.isNpcReady = true;
			_Network.RPCOthers(EPacketType.PT_CL_CHK_TryStart, npc._npcID);
		}
	}

	public override void CombomData(BinaryWriter writer)
	{
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
	}
}
