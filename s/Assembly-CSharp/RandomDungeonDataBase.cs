using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class RandomDungeonDataBase
{
	public const int LEVEL_MAX = 10;

	private static Dictionary<int, DungeonBaseData> dungeonData = new Dictionary<int, DungeonBaseData>();

	private static Dictionary<int, List<int>> levelId = new Dictionary<int, List<int>>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Rdungeon_monster_item");
		while (sqliteDataReader.Read())
		{
			DungeonBaseData dungeonBaseData = new DungeonBaseData();
			dungeonBaseData.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			dungeonBaseData.level = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("level")));
			dungeonBaseData.landMonsterId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("landmonsterid")));
			dungeonBaseData.waterMonsterId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("watermonsterid")));
			dungeonBaseData.monsterAmount = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsteramount")));
			dungeonBaseData.monsterBuff = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterbuff")));
			dungeonBaseData.bossId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bossid")));
			dungeonBaseData.bossWaterId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bosswaterid")));
			dungeonBaseData.bossMonsterBuff = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bossbuff")));
			dungeonBaseData.minBossId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("minbossid")));
			dungeonBaseData.minBossWaterId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("minbosswaterid")));
			dungeonBaseData.minBossMonsterBuff = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("minbossbuff")));
			dungeonBaseData.itemId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemid")));
			dungeonBaseData.itemAmount = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemamount")));
			dungeonBaseData.rareItemId = RandomDunGenUtil.GetIdWeightList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rareitemid")));
			dungeonBaseData.rareItemChance = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rareitemchance")));
			dungeonBaseData.specifiedItems = ItemIdCount.ParseStringToList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("specifieditems")));
			dungeonBaseData.dungeonFlowPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("dungeonflowpath"));
			dungeonBaseData.type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("type")));
			dungeonData.Add(dungeonBaseData.id, dungeonBaseData);
			if (!levelId.ContainsKey(dungeonBaseData.level))
			{
				levelId[dungeonBaseData.level] = new List<int>();
			}
			levelId[dungeonBaseData.level].Add(dungeonBaseData.id);
		}
	}

	public static DungeonBaseData GetDataFromLevel(int level)
	{
		if (!levelId.ContainsKey(level))
		{
			return null;
		}
		List<int> list = levelId[level];
		int key = list[new Random().Next(list.Count)];
		if (!dungeonData.ContainsKey(key))
		{
			return null;
		}
		return dungeonData[key];
	}

	public static DungeonBaseData GetDataFromId(int id)
	{
		if (!dungeonData.ContainsKey(id))
		{
			return null;
		}
		return dungeonData[id];
	}
}
