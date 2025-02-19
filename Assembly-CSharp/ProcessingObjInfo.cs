using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class ProcessingObjInfo
{
	public int protoId;

	public int tab;

	public int max;

	public float time;

	private static Dictionary<int, ProcessingObjInfo> pobInfoDict = new Dictionary<int, ProcessingObjInfo>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("collectfield");
		while (sqliteDataReader.Read())
		{
			ProcessingObjInfo processingObjInfo = new ProcessingObjInfo();
			processingObjInfo.protoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemid")));
			processingObjInfo.tab = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("tab")));
			processingObjInfo.max = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("max")));
			processingObjInfo.time = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("time")));
			pobInfoDict.Add(processingObjInfo.protoId, processingObjInfo);
		}
	}

	public static float GetPobTime(int protoId)
	{
		if (!pobInfoDict.ContainsKey(protoId))
		{
			return -1f;
		}
		return pobInfoDict[protoId].time / (float)pobInfoDict[protoId].max;
	}

	public static int GetPobMax(int protoId)
	{
		if (!pobInfoDict.ContainsKey(protoId))
		{
			return -1;
		}
		return pobInfoDict[protoId].max;
	}

	public static ICollection<ProcessingObjInfo> GetAllInfo()
	{
		return pobInfoDict.Values;
	}
}
