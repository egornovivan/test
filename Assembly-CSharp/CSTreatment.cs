using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using uLink;

public class CSTreatment
{
	public int abnormalId;

	public int npcId;

	public string npcName;

	public int needTreatTimes;

	public string diseaseName;

	public string treatName;

	public List<ItemIdCount> medicineList = new List<ItemIdCount>();

	public float treatTime;

	public float restoreTime;

	public int treatState;

	public static List<CSTreatment> CreateTreatment(PeEntity npc)
	{
		List<int> list = new List<int>();
		foreach (PEAbnormalType illAbnormal in npc.GetCmpt<NpcCmpt>().illAbnormals)
		{
			list.Add((int)illAbnormal);
		}
		List<CSTreatment> list2 = new List<CSTreatment>();
		foreach (int item2 in list)
		{
			CSTreatment item = GenTreatmentFromDatabase(npc, item2);
			list2.Add(item);
		}
		return list2;
	}

	public static CSTreatment GetRandomTreatment(PeEntity npc)
	{
		int num = AbnormalTypeTreatData.GetRandomData().abnormalId;
		return GenTreatmentFromDatabase(npc, num);
	}

	public static CSTreatment GenTreatmentFromDatabase(PeEntity npc, int abnormalId)
	{
		CSTreatment cSTreatment = new CSTreatment();
		AbnormalTypeTreatData treatment = AbnormalTypeTreatData.GetTreatment(abnormalId);
		cSTreatment.abnormalId = abnormalId;
		cSTreatment.npcName = npc.ExtGetName();
		cSTreatment.diseaseName = AbnormalData.GetData((PEAbnormalType)abnormalId).name;
		cSTreatment.treatName = PELocalization.GetString(treatment.treatDescription);
		cSTreatment.medicineList.Add(new ItemIdCount(treatment.treatItemId[0], treatment.treatItemNum));
		cSTreatment.npcId = npc.Id;
		cSTreatment.needTreatTimes = treatment.treatNum;
		cSTreatment.treatTime = treatment.treatTime;
		cSTreatment.restoreTime = treatment.restoreTime;
		return cSTreatment;
	}

	public void InitFromRecord()
	{
		AbnormalTypeTreatData treatment = AbnormalTypeTreatData.GetTreatment(abnormalId);
		diseaseName = AbnormalData.GetData((PEAbnormalType)abnormalId).name;
		treatName = PELocalization.GetString(treatment.treatDescription);
		medicineList.Add(new ItemIdCount(treatment.treatItemId[0], treatment.treatItemNum));
		treatTime = treatment.treatTime;
		restoreTime = treatment.restoreTime;
	}

	public void _writeTreatmentData(BinaryWriter w)
	{
		w.Write(abnormalId);
		w.Write(npcId);
		w.Write(npcName);
		w.Write(needTreatTimes);
	}

	public static CSTreatment _readTreatmentData(BinaryReader r, int version)
	{
		CSTreatment cSTreatment = new CSTreatment();
		if (version >= 15091800)
		{
			cSTreatment.abnormalId = r.ReadInt32();
			cSTreatment.npcId = r.ReadInt32();
			cSTreatment.npcName = r.ReadString();
			cSTreatment.needTreatTimes = r.ReadInt32();
		}
		cSTreatment.InitFromRecord();
		return cSTreatment;
	}

	public static object Deserialize(BitStream r, params object[] codecOptions)
	{
		try
		{
			CSTreatment cSTreatment = new CSTreatment();
			cSTreatment.abnormalId = r.ReadInt32();
			cSTreatment.npcId = r.ReadInt32();
			cSTreatment.needTreatTimes = r.ReadInt32();
			AbnormalTypeTreatData treatment = AbnormalTypeTreatData.GetTreatment(cSTreatment.abnormalId);
			if (PeSingleton<EntityMgr>.Instance.Get(cSTreatment.npcId) == null)
			{
				cSTreatment.npcName = "?";
			}
			else
			{
				cSTreatment.npcName = PeSingleton<EntityMgr>.Instance.Get(cSTreatment.npcId).ExtGetName();
			}
			cSTreatment.diseaseName = AbnormalData.GetData((PEAbnormalType)cSTreatment.abnormalId).name;
			cSTreatment.treatName = PELocalization.GetString(treatment.treatDescription);
			cSTreatment.medicineList.Add(new ItemIdCount(treatment.treatItemId[0], treatment.treatItemNum));
			cSTreatment.treatTime = treatment.treatTime;
			cSTreatment.restoreTime = treatment.restoreTime;
			return cSTreatment;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		try
		{
			CSTreatment cSTreatment = value as CSTreatment;
			stream.WriteInt32(cSTreatment.abnormalId);
			stream.WriteInt32(cSTreatment.npcId);
			stream.WriteInt32(cSTreatment.needTreatTimes);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public override bool Equals(object obj)
	{
		CSTreatment cSTreatment = obj as CSTreatment;
		return abnormalId == cSTreatment.abnormalId && npcId == cSTreatment.npcId && needTreatTimes == cSTreatment.needTreatTimes;
	}

	public override int GetHashCode()
	{
		return (abnormalId * 73856093) ^ (npcId * 19349663) ^ (needTreatTimes * 83492791);
	}
}
