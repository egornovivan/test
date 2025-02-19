using Mono.Data.SqliteClient;

namespace Pathea;

public static class PlayerProtoDb
{
	public static DbAttr dbAttr;

	public static DbAttr Get()
	{
		return dbAttr;
	}

	public static void Release()
	{
		dbAttr = null;
	}

	public static void Load()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("initprop");
		if (sqliteDataReader.Read())
		{
			dbAttr = new DbAttr();
			dbAttr.ReadFromDb(sqliteDataReader);
		}
	}
}
