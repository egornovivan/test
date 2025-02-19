using System;
using System.Collections.Generic;
using Behave.Runtime;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea;

public class MonsterProtoDb
{
	public class Item
	{
		public class AtkNumDb
		{
			public int mNumber;

			public int mChaseNumber;

			public bool mNeeedEqup;

			public int mMaxMeleeNum = 4;

			public static AtkNumDb Load(string str)
			{
				AtkNumDb atkNumDb = new AtkNumDb();
				string[] array = str.Split(',');
				if (array.Length != 3)
				{
					Debug.LogError("load AtkNum error:" + str);
				}
				else
				{
					atkNumDb.mNumber = Convert.ToInt32(array[0]);
					atkNumDb.mChaseNumber = Convert.ToInt32(array[1]);
					atkNumDb.mNeeedEqup = Convert.ToBoolean(Convert.ToInt32(array[2]));
				}
				return atkNumDb;
			}
		}

		[DbReader.DbField("id", false)]
		public int id;

		[DbReader.DbField("monster_icon", false)]
		public string icon;

		[DbReader.DbField("TranslationID", false)]
		public string nameID;

		[DbReader.DbField("Scale", false)]
		public float[] fScaleMinMax;

		[DbReader.DbField("SpawnHeight", false)]
		public float hOffset;

		[DbReader.DbField("assetbundle_path", false)]
		public string modelPath;

		[DbReader.DbField("model_name", false)]
		public string modelName;

		[DbReader.DbField("Area", false)]
		public int[] monsterAreaId;

		[DbReader.DbField("Canbepush", false)]
		public bool canBePush;

		[DbReader.DbField("behave_path", false)]
		public string behaveDataPath;

		[DbReader.DbField("EquipID", false)]
		public int[] initEquip;

		[DbReader.DbField("identity", false)]
		public EIdentity eId;

		[DbReader.DbField("race", false)]
		public ERace eRace;

		[DbReader.DbField("isBoss", false)]
		public bool isBoss;

		[DbReader.DbField("ReputationValueID", false)]
		public int repValId;

		[DbReader.DbField("environment", false)]
		public MovementField movementField;

		[DbReader.DbField("InjuredLv", false)]
		public int injuredLevel;

		[DbReader.DbField("InjuredState", false)]
		public float injuredState;

		[DbReader.DbField("EscapeProb", false)]
		public float escapeProb;

		[DbReader.DbField("loot", false)]
		public int dropItemId;

		[DbReader.DbField("DeathSoundID", false)]
		public int deathAudioID;

		[DbReader.DbField("YawpSoundMinDistance", false)]
		public int idleSoundDis;

		[DbReader.DbField("InitBuffList", false)]
		public int[] initBuff;

		[DbReader.DbField("deathbuff", false)]
		public string deathBuff;

		[DbReader.DbField("BeHitSound", false)]
		public int[] beHitSound;

		[DbReader.DbField("YawpSoundID", false)]
		public int[] idleSounds;

		[DbReader.DbField("AttackType", false)]
		public int attackType;

		[DbReader.DbField("Npc_id", false)]
		public int npcProtoID;

		[DbReader.DbField("Canberepelled", false)]
		public int RepulsedType;

		public AtkNumDb AtkDb;

		public DbAttr dbAttr = new DbAttr();

		public string name
		{
			get
			{
				int result = 0;
				if (int.TryParse(nameID, out result))
				{
					return PELocalization.GetString(result);
				}
				return string.Empty;
			}
		}

		[DbReader.DbField("AttackNum", false)]
		private string atkDbStr
		{
			set
			{
				AtkDb = AtkNumDb.Load(value);
			}
		}
	}

	private static List<Item> sList = new List<Item>(50);

	public static void Load()
	{
		sList.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeMonster");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			item.dbAttr.ReadFromDb(sqliteDataReader);
			BTResolver.RegisterToCache(item.behaveDataPath);
			MonsterXmlData.InitializeData(item.id, item.behaveDataPath);
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList.Clear();
	}

	public static Item Get(int id)
	{
		for (int i = 0; i < sList.Count; i++)
		{
			if (sList[i].id == id)
			{
				return sList[i];
			}
		}
		return null;
	}
}
