using System.Collections.Generic;
using System.IO;
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

	public static List<CSTreatment> CreateTreatment(ColonyNpc npc)
	{
		List<CSTreatment> list = new List<CSTreatment>();
		if (npc._refNpc.AbnormalModule != null)
		{
			foreach (int item2 in npc._refNpc.AbnormalModule.TreatedAbnormal())
			{
				CSTreatment item = GenTreatmentFromDatabase(npc, item2);
				list.Add(item);
			}
		}
		return list;
	}

	public static CSTreatment GetRandomTreatment(ColonyNpc npc)
	{
		int num = AbnormalTypeTreatData.GetRandomData().abnormalId;
		return GenTreatmentFromDatabase(npc, num);
	}

	public static CSTreatment GenTreatmentFromDatabase(ColonyNpc npc, int abnormalId)
	{
		CSTreatment cSTreatment = new CSTreatment();
		AbnormalTypeTreatData treatment = AbnormalTypeTreatData.GetTreatment(abnormalId);
		cSTreatment.abnormalId = abnormalId;
		cSTreatment.npcName = npc._refNpc.name;
		cSTreatment.diseaseName = AbnormalData.GetData((PEAbnormalType)abnormalId).name;
		cSTreatment.treatName = PELocalization.GetString(treatment.treatDescription);
		cSTreatment.medicineList.Add(new ItemIdCount(treatment.treatItemId[0], treatment.treatItemNum));
		cSTreatment.npcId = npc._npcID;
		cSTreatment.needTreatTimes = treatment.treatNum;
		cSTreatment.treatTime = treatment.treatTime;
		cSTreatment.restoreTime = treatment.restoreTime;
		return cSTreatment;
	}

	public void InitFromRecord()
	{
		AbnormalTypeTreatData treatment = AbnormalTypeTreatData.GetTreatment(abnormalId);
		treatName = PELocalization.GetString(treatment.treatDescription);
		medicineList.Add(new ItemIdCount(treatment.treatItemId[0], treatment.treatItemNum));
		treatTime = treatment.treatTime;
		restoreTime = treatment.restoreTime;
	}

	public void _writeTreatmentData(BinaryWriter w)
	{
		w.Write(abnormalId);
		w.Write(npcId);
		w.Write(needTreatTimes);
	}

	public static CSTreatment _readTreatmentData(BinaryReader r, int version)
	{
		CSTreatment cSTreatment = new CSTreatment();
		cSTreatment.abnormalId = r.ReadInt32();
		cSTreatment.npcId = r.ReadInt32();
		cSTreatment.needTreatTimes = r.ReadInt32();
		cSTreatment.InitFromRecord();
		return cSTreatment;
	}

	public static object Deserialize(BitStream r, params object[] codecOptions)
	{
		CSTreatment cSTreatment = new CSTreatment();
		cSTreatment.abnormalId = r.ReadInt32();
		cSTreatment.npcId = r.ReadInt32();
		cSTreatment.needTreatTimes = r.ReadInt32();
		return cSTreatment;
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		CSTreatment cSTreatment = value as CSTreatment;
		stream.WriteInt32(cSTreatment.abnormalId);
		stream.WriteInt32(cSTreatment.npcId);
		stream.WriteInt32(cSTreatment.needTreatTimes);
	}

	public override bool Equals(object obj)
	{
		CSTreatment cSTreatment = obj as CSTreatment;
		return abnormalId == cSTreatment.abnormalId && npcId == cSTreatment.npcId && needTreatTimes == cSTreatment.needTreatTimes;
	}
}
