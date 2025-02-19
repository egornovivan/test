using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AsyncSqlite : MonoBehaviour
{
	private static SqliteConnection _conn;

	private static SqliteConnection mWriteConn;

	private static string _connString;

	private static List<DbRecordData> mRecordList = new List<DbRecordData>(10);

	private static List<DbRecordData> mCacheRecord = new List<DbRecordData>(10);

	private static readonly object RecordLock = new object();

	private static readonly object DbLock = new object();

	private static Thread mDbProcess;

	private static ManualResetEvent s_StopEvent = new ManualResetEvent(initialState: false);

	private static bool _disposed;

	private static bool mExit;

	private static bool mDbInitialized;

	public static string ConnString => _connString;

	public static bool Disposed => _disposed;

	public static bool DbInitialized => mDbInitialized;

	private void Awake()
	{
		_disposed = true;
	}

	public static void OpenDB()
	{
		string text = Path.Combine(ServerConfig.ServerDir, "cache.pe");
		_connString = "URI=file:" + text;
		_conn = CreateConnection();
		_disposed = false;
		InitDB();
		mDbProcess = new Thread(DbProcessThread);
		mExit = false;
		mDbProcess.Start();
	}

	public static bool OpenDB2()
	{
		mWriteConn = CreateConnection();
		return mDbInitialized = null != mWriteConn;
	}

	private static void StopDB2()
	{
		mDbInitialized = false;
		if (mWriteConn != null)
		{
			mWriteConn.Close();
			mWriteConn = null;
		}
	}

	private static void InitDB()
	{
		string cmdText = "CREATE TABLE IF NOT EXISTS player(roleid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, rolename TEXT, steamid INTEGER, teamid INTEGER, worldid INTEGER, pos BLOB, playerdata BLOB);CREATE TABLE IF NOT EXISTS serverinfo(servername TEXT PRIMARY KEY UNIQUE, ver INTEGER, creatorrolename TEXT, pwd TEXT, mode INTEGER, type INTEGER, seed INTEGER, maxnpcid INTEGER,maxmonsterid INTEGER,maxitemid INTEGER, maxdoodadid INTEGER, curteamid INTEGER, maxteamnum INTEGER, numperteam INTEGER, terraintype INTEGER, vegetationtype INTEGER, scenceclimate INTEGER, hasmonster NUMERIC, serveruid INTEGER, unlimitedres NUMERIC,terrainheight INTEGER,mapsize INTEGER, riverdensity INTEGER, riverwidth INTEGER, plainheight INTEGER, flatness INTEGER, bridgemaxheight INTEGER,gamestarted NUMERIC,useskilltree NUMERIC,blobdata BLOB);CREATE TABLE IF NOT EXISTS areainfo(worldid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, servername TEXT, townarea BLOB, camparea BLOB);CREATE TABLE IF NOT EXISTS networkobj(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, externid INTEGER, groupnum INTEGER, teamnum INTEGER, dependtype INTEGER, itemtype INTEGER, blobdata BLOB);CREATE TABLE IF NOT EXISTS creationdata(objid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, seed INTEGER, hp NUMERIC, maxhp NUMERIC, fuel NUMERIC, maxfuel NUMERIC, hash INTEGER, steamid INTEGER);CREATE TABLE IF NOT EXISTS registerediso(hash INTEGER PRIMARY KEY UNIQUE, ver INTEGER, handle INTEGER, name TEXT, blobdata BLOB);CREATE TABLE IF NOT EXISTS itemdata(itemid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, blobdata BLOB);CREATE TABLE IF NOT EXISTS skilldata(skillid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, blobdata BLOB);CREATE TABLE IF NOT EXISTS itemobject(objid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS npcdata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, worldid INTEGER, basedata BLOB, customdata BLOB, missiondata BLOB, externdata BLOB);CREATE TABLE IF NOT EXISTS playermission(roleid TEXT PRIMARY KEY UNIQUE, ver INTEGER, pmdata BLOB, adrmdata BLOB);CREATE TABLE IF NOT EXISTS teammission(team TEXT PRIMARY KEY UNIQUE, ver INTEGER, pmdata BLOB, adrmdata BLOB, missionitems  BLOB);CREATE TABLE IF NOT EXISTS colony(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, itemid INTEGER, type INTEGER, durability INTEGER, currepairtime FLOAT, repairtime FLOAT, repairvalue FLOAT, curdeletetime FLOAT, deletetime FLOAT, data BLOB);CREATE TABLE IF NOT EXISTS colonydata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS railwaydata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS farmdata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS colonynpc(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, teamid INTEGER, state INTEGER, dwellingsid INTEGER, workroomid INTEGER, occupation INTEGER, workmode INTEGER, blobdata BLOB);CREATE TABLE IF NOT EXISTS skentitydata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS playergameworld(roleid INTEGER, ver INTEGER, worldid INTEGER, data BLOB, PRIMARY KEY(roleid, worldid));CREATE TABLE IF NOT EXISTS sceneobj(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, type INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS sceneid(servername TEXT PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS doodaddata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, objtype TEXT, data BLOB);CREATE TABLE IF NOT EXISTS publicdata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS versiondata(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, versionname TEXT,versionno INTEGER,blobdata BLOB);CREATE TABLE IF NOT EXISTS customnpcs(dummyid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS reputation(id INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);CREATE TABLE IF NOT EXISTS customdialogs(dummyid INTEGER PRIMARY KEY UNIQUE, ver INTEGER, data BLOB);";
		PEDbOp pEDbOp = NewWriteOp();
		pEDbOp.SetCmdText(cmdText);
		pEDbOp.Exec();
		pEDbOp = null;
	}

	public static void StopDB()
	{
		if (!Disposed)
		{
			_disposed = true;
			if (_conn != null)
			{
				_conn.Close();
				_conn = null;
			}
		}
		mExit = true;
		s_StopEvent.Reset();
		if (LogFilter.logDebug)
		{
			Debug.Log("Wait for db stop event");
		}
		s_StopEvent.WaitOne();
		if (LogFilter.logDebug)
		{
			Debug.Log("Get db stop event");
		}
		s_StopEvent.Close();
		s_StopEvent = null;
		mDbProcess = null;
	}

	private static void DbProcessThread()
	{
		if (!OpenDB2())
		{
			return;
		}
		while (DbInitialized)
		{
			PushRecord();
			if (mExit)
			{
				ExceCounter();
				break;
			}
			ExceCounter();
			Thread.Sleep(50);
		}
		StopDB2();
		if (LogFilter.logDebug)
		{
			Debug.Log("Set db stop event");
		}
		s_StopEvent.Set();
	}

	public static void AddRecord(DbRecordData record)
	{
		lock (RecordLock)
		{
			mCacheRecord.Add(record);
		}
	}

	private static void PushRecord()
	{
		if (mCacheRecord.Count <= 0)
		{
			return;
		}
		lock (RecordLock)
		{
			mRecordList.AddRange(mCacheRecord);
			mCacheRecord.Clear();
		}
	}

	private static void ExceCounter()
	{
		if (0 >= mRecordList.Count)
		{
			return;
		}
		lock (DbLock)
		{
			using IDbTransaction dbTransaction = mWriteConn.BeginTransaction();
			for (int i = 0; i < mRecordList.Count; i++)
			{
				ExceWrite(mRecordList[i]);
			}
			dbTransaction.Commit();
		}
		mRecordList.Clear();
	}

	public static SqliteConnection CreateConnection()
	{
		try
		{
			SqliteConnection sqliteConnection = new SqliteConnection(ConnString);
			sqliteConnection.Open();
			sqliteConnection.BusyTimeout = 30;
			return sqliteConnection;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	private static void ExceWrite(DbRecordData data)
	{
		IDbCommand dbCommand = null;
		try
		{
			dbCommand = mWriteConn.CreateCommand();
			data.Exce(dbCommand);
		}
		catch (Exception exception)
		{
			Debug.LogFormat("cmd:{0}", dbCommand.CommandText);
			Debug.LogException(exception);
		}
		finally
		{
			if (!object.Equals(dbCommand, null))
			{
				dbCommand.Dispose();
				dbCommand = null;
			}
		}
	}

	public static PEDbOp NewWriteOp()
	{
		try
		{
			if (_disposed)
			{
				return null;
			}
			SqliteCommand sqliteCommand = (SqliteCommand)_conn.CreateCommand();
			if (sqliteCommand == null)
			{
				return null;
			}
			PEDbOp pEDbOp = new PEDbOp();
			sqliteCommand.CommandType = CommandType.Text;
			pEDbOp.SetSqliteCmd(sqliteCommand);
			pEDbOp.SetReadOrWrite(1);
			return pEDbOp;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	public static PEDbOp NewReadOp()
	{
		try
		{
			if (_disposed)
			{
				return null;
			}
			SqliteCommand sqliteCommand = (SqliteCommand)_conn.CreateCommand();
			if (sqliteCommand == null)
			{
				return null;
			}
			PEDbOp pEDbOp = new PEDbOp();
			sqliteCommand.CommandType = CommandType.Text;
			pEDbOp.SetSqliteCmd(sqliteCommand);
			pEDbOp.SetReadOrWrite(0);
			return pEDbOp;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	public static void ExecuteNonQuery(SqliteCommand cmd)
	{
		if (object.Equals(null, cmd))
		{
			return;
		}
		lock (DbLock)
		{
			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exception)
			{
				Debug.LogErrorFormat("CMD execute error:[{0}]", cmd.CommandText);
				Debug.LogException(exception);
			}
		}
	}

	public static void ExecuteReader(SqliteCommand cmd, Action<SqliteDataReader> readerHandler)
	{
		SqliteDataReader sqliteDataReader = null;
		if (object.Equals(null, cmd) || readerHandler == null)
		{
			return;
		}
		lock (DbLock)
		{
			try
			{
				sqliteDataReader = cmd.ExecuteReader();
				if (sqliteDataReader != null)
				{
					readerHandler(sqliteDataReader);
				}
			}
			catch (Exception exception)
			{
				Debug.LogErrorFormat("CMD execute error:[{0}]", cmd.CommandText);
				Debug.LogException(exception);
			}
			finally
			{
				if (sqliteDataReader != null)
				{
					sqliteDataReader.Close();
					sqliteDataReader = null;
				}
			}
		}
	}
}
