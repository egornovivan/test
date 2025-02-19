using System;
using System.Collections.Generic;
using Behave.Runtime;
using Mono.Data.SqliteClient;
using PETools;

namespace Pathea;

public class TowerProtoDb
{
	public struct TowerEffectData
	{
		public float hpPercent;

		public int effectID;

		public int audioID;
	}

	public class BulletData
	{
		[DbReader.DbField("needBlock", false)]
		public bool needBlock;

		[DbReader.DbField("bulletType", false)]
		public int bulletType;

		[DbReader.DbField("bulletID", false)]
		public int bulletId;

		[DbReader.DbField("bulletCost", false)]
		public int bulletCost;

		[DbReader.DbField("maxBullet", false)]
		public int bulletMax;

		[DbReader.DbField("energyCost", false)]
		public int energyCost;

		[DbReader.DbField("maxEnergy", false)]
		public int energyMax;

		[DbReader.DbField("skillID", false)]
		public int skillId;
	}

	public class Item
	{
		[DbReader.DbField("id", false)]
		public int id;

		[DbReader.DbField("icon", false)]
		public string icon;

		[DbReader.DbField("ENG_name", false)]
		public string name;

		[DbReader.DbField("assetbundle_path", false)]
		public string modelPath;

		[DbReader.DbField("behave_path", false)]
		public string behaveDataPath;

		[DbReader.DbField("identity", false)]
		public EIdentity eId;

		[DbReader.DbField("race", false)]
		public ERace eRace;

		[DbReader.DbField("HPLossEffect", false)]
		public string effect;

		public DbAttr dbAttr = new DbAttr();

		public BulletData bulletData;

		public List<TowerEffectData> effects;

		public void InitEffect()
		{
			effects = new List<TowerEffectData>();
			string[] array = PEUtil.ToArrayString(effect, ',');
			TowerEffectData item = default(TowerEffectData);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = PEUtil.ToArrayString(array[i], '_');
				item.hpPercent = Convert.ToSingle(array2[0]);
				item.effectID = Convert.ToInt32(array2[1]);
				item.audioID = Convert.ToInt32(array2[2]);
				effects.Add(item);
			}
		}
	}

	private static List<Item> sList = new List<Item>(50);

	public static void Load()
	{
		sList.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeTurret");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			item.bulletData = DbReader.ReadItem<BulletData>(sqliteDataReader);
			item.dbAttr.ReadFromDb(sqliteDataReader);
			item.InitEffect();
			BTResolver.RegisterToCache(item.behaveDataPath);
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList.Clear();
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => (item.id == id) ? true : false);
	}
}
