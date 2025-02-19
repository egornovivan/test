using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class VersionMgr
{
	public const int COLONY_RECORD_ID = 0;

	public const int DROPITEM_RECORD_ID = 1;

	public const string COLONY_RECORD_NAME = "colony";

	public const int COLONY_VERSION000 = 2015111100;

	public const int COLONY_VERSION001 = 2016071500;

	public const int DROPITEM_VERSION000 = 2016011300;

	public static EConstVersion MAINTAIN_VER = EConstVersion.VER_2016101800;

	public static int colonyRecordVersion = 0;

	public static int currentColonyVersion = 2016071500;

	public static int dropItemRecordVersion = 0;

	public static string DROPITEM_RECORD_NAME = "networkobj";

	public static int currentDropItemVersion = 2016011300;

	public static Dictionary<int, VersionData> allVersionData = new Dictionary<int, VersionData>();

	public static bool IsCompatible(EConstVersion ver)
	{
		return MAINTAIN_VER >= ver;
	}

	public static void LoadVersion()
	{
		try
		{
			PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
			if (pEDbOp != null)
			{
				pEDbOp.SetCmdText("SELECT * FROM versiondata;");
				pEDbOp.BindReaderHandler(LoadComplete);
				pEDbOp.Exec();
				pEDbOp = null;
			}
		}
		catch (Exception exception)
		{
			if (LogFilter.logError)
			{
				Debug.LogException(exception);
			}
		}
	}

	public static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			VersionData versionData = new VersionData();
			versionData.id = reader.GetInt32(reader.GetOrdinal("id"));
			versionData.versionName = reader.GetString(reader.GetOrdinal("versionname"));
			versionData.versionNO = reader.GetInt32(reader.GetOrdinal("versionno"));
			allVersionData.Add(versionData.id, versionData);
		}
		LoadColonyVersion();
		LoadDropItemMgrVersion();
	}

	public static void LoadColonyVersion()
	{
		if (allVersionData.ContainsKey(0))
		{
			VersionData versionData = allVersionData[0];
			colonyRecordVersion = versionData.versionNO;
		}
	}

	public static void LoadDropItemMgrVersion()
	{
		if (allVersionData.ContainsKey(1))
		{
			VersionData versionData = allVersionData[1];
			dropItemRecordVersion = versionData.versionNO;
		}
	}

	public static void SaveAllVersion()
	{
		SaveColonyVersion();
		SaveDropItemVersion();
	}

	public static void SaveColonyVersion()
	{
		SaveGeneralVersion(0, "colony", currentColonyVersion);
	}

	public static void SaveDropItemVersion()
	{
		SaveGeneralVersion(1, DROPITEM_RECORD_NAME, currentDropItemVersion);
	}

	public static void SaveGeneralVersion(int id, string name, int no)
	{
		VersionDbData versionDbData = new VersionDbData();
		versionDbData.ExportData(id, no, name);
		AsyncSqlite.AddRecord(versionDbData);
	}
}
