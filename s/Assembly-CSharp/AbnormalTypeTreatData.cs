using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.SqliteClient;

public class AbnormalTypeTreatData
{
	public static bool debug = false;

	public int no;

	public int abnormalId;

	public int treatDescription;

	public List<int> itemId = new List<int>();

	public int itemNum;

	public bool isMedicalLab;

	public List<int> treatItemId = new List<int>();

	public int treatItemNum;

	public float diagnoseTime;

	public float treatTime;

	public int treatNum;

	public float restoreTime;

	public static Dictionary<int, AbnormalTypeTreatData> treatmentDataDict = new Dictionary<int, AbnormalTypeTreatData>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("abnormaltypetreat");
		while (sqliteDataReader.Read())
		{
			AbnormalTypeTreatData abnormalTypeTreatData = new AbnormalTypeTreatData();
			abnormalTypeTreatData.no = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("treatno")));
			abnormalTypeTreatData.abnormalId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("abnormalid")));
			abnormalTypeTreatData.treatDescription = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("translationid")));
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemid")).Split(',');
			string[] array2 = array;
			foreach (string value in array2)
			{
				abnormalTypeTreatData.itemId.Add(Convert.ToInt32(value));
			}
			abnormalTypeTreatData.itemNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemnumber")));
			if (Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ismedicallab"))) > 0)
			{
				abnormalTypeTreatData.isMedicalLab = true;
			}
			else
			{
				abnormalTypeTreatData.isMedicalLab = false;
			}
			array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("treatitemid")).Split(',');
			string[] array3 = array;
			foreach (string value2 in array3)
			{
				abnormalTypeTreatData.treatItemId.Add(Convert.ToInt32(value2));
			}
			abnormalTypeTreatData.treatItemNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("treatitemnumber")));
			abnormalTypeTreatData.treatNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("treatnumber")));
			if (!debug)
			{
				abnormalTypeTreatData.diagnoseTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("diagnosistime")));
				abnormalTypeTreatData.treatTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("treattime")));
				abnormalTypeTreatData.restoreTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("restoretime")));
			}
			else
			{
				abnormalTypeTreatData.diagnoseTime = 20f;
				abnormalTypeTreatData.treatTime = 20f;
				abnormalTypeTreatData.restoreTime = 20f;
			}
			treatmentDataDict.Add(abnormalTypeTreatData.abnormalId, abnormalTypeTreatData);
		}
	}

	public static AbnormalTypeTreatData GetTreatment(int abnormalId)
	{
		if (!treatmentDataDict.ContainsKey(abnormalId) || !treatmentDataDict[abnormalId].isMedicalLab)
		{
			return null;
		}
		return treatmentDataDict[abnormalId];
	}

	public static AbnormalTypeTreatData GetRandomData()
	{
		List<AbnormalTypeTreatData> list = treatmentDataDict.Values.ToList();
		int index = new Random().Next(list.Count());
		return list[index];
	}

	public static float GetDiagnosingTime(int abnormalId)
	{
		if (!treatmentDataDict.ContainsKey(abnormalId) || !treatmentDataDict[abnormalId].isMedicalLab)
		{
			return -1f;
		}
		return treatmentDataDict[abnormalId].diagnoseTime;
	}

	public static float GetRestoreTime(int abnormalId)
	{
		if (!treatmentDataDict.ContainsKey(abnormalId) || !treatmentDataDict[abnormalId].isMedicalLab)
		{
			return -1f;
		}
		return treatmentDataDict[abnormalId].restoreTime;
	}

	public static bool CanBeTreatInColony(int abnormalId)
	{
		if (!treatmentDataDict.ContainsKey(abnormalId))
		{
			return false;
		}
		return treatmentDataDict[abnormalId].isMedicalLab;
	}
}
