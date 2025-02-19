using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace Pathea;

public class TowerProtoDb
{
	public class BulletData
	{
		public bool needBlock;

		public int bulletType;

		public int bulletId;

		public int bulletCost;

		public int bulletMax;

		public int energyCost;

		public int energyMax;

		public int skillId;
	}

	public class Item
	{
		public int id;

		public string icon;

		public string name;

		public string modelPath;

		public string behaveDataPath;

		public EIdentity eId;

		public ERace eRace;

		public DbAttr dbAttr = new DbAttr();

		public BulletData bulletData;
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeTurret");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item.id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("id"));
			item.icon = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("icon"));
			item.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ENG_name"));
			item.modelPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("assetbundle_path"));
			item.behaveDataPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("behave_path"));
			item.dbAttr.ReadFromDb(sqliteDataReader);
			item.eId = (EIdentity)(int)Enum.Parse(typeof(EIdentity), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("identity")));
			item.eRace = (ERace)(int)Enum.Parse(typeof(ERace), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("race")));
			BulletData bulletData = new BulletData();
			int @int = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("needBlock"));
			bulletData.needBlock = ((@int != 0) ? true : false);
			bulletData.bulletType = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("bulletType"));
			bulletData.bulletId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("bulletID"));
			bulletData.bulletCost = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("bulletCost"));
			bulletData.bulletMax = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("maxBullet"));
			bulletData.energyCost = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("energyCost"));
			bulletData.energyMax = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("maxEnergy"));
			bulletData.skillId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("skillID"));
			item.bulletData = bulletData;
			item.dbAttr.ReadFromDb(sqliteDataReader);
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList = null;
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => (item.id == id) ? true : false);
	}
}
