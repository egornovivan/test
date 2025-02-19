using System;
using System.Collections.Generic;
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

	public int cureSkillId;

	public static List<AbnormalTypeTreatData> treatmentDatas = new List<AbnormalTypeTreatData>();

	public static AbnormalTypeTreatData GetDataById(int id)
	{
		int count = treatmentDatas.Count;
		for (int i = 0; i < count; i++)
		{
			if (treatmentDatas[i].abnormalId == id)
			{
				return treatmentDatas[i];
			}
		}
		return null;
	}

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
			abnormalTypeTreatData.cureSkillId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("skBuff_ID")));
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
			treatmentDatas.Add(abnormalTypeTreatData);
		}
	}

	public static AbnormalTypeTreatData GetTreatment(int abnormalId)
	{
		AbnormalTypeTreatData dataById = GetDataById(abnormalId);
		if (dataById == null || !dataById.isMedicalLab)
		{
			return null;
		}
		return dataById;
	}

	public static AbnormalTypeTreatData GetRandomData()
	{
		int index = new Random().Next(treatmentDatas.Count);
		return treatmentDatas[index];
	}

	public static float GetDiagnosingTime(int abnormalId)
	{
		AbnormalTypeTreatData dataById = GetDataById(abnormalId);
		if (dataById == null || !dataById.isMedicalLab)
		{
			return -1f;
		}
		return dataById.diagnoseTime;
	}

	public static float GetRestoreTime(int abnormalId)
	{
		AbnormalTypeTreatData dataById = GetDataById(abnormalId);
		if (dataById == null || !dataById.isMedicalLab)
		{
			return -1f;
		}
		return dataById.restoreTime;
	}

	public static bool CanBeTreatInColony(int abnormalId)
	{
		return GetDataById(abnormalId)?.isMedicalLab ?? false;
	}

	public static int GetCureSkillId(int abnormalId)
	{
		return GetDataById(abnormalId)?.cureSkillId ?? (-1);
	}
}
