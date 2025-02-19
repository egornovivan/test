using System.Collections.Generic;

public class DungeonBaseData
{
	public int id;

	public int level;

	public List<IdWeight> landMonsterId;

	public List<IdWeight> waterMonsterId;

	public float monsterAmount;

	public float monsterBuff;

	public List<IdWeight> bossId;

	public List<IdWeight> bossWaterId;

	public float bossMonsterBuff;

	public List<IdWeight> minBossId;

	public List<IdWeight> minBossWaterId;

	public float minBossMonsterBuff;

	public List<IdWeight> itemId;

	public float itemAmount;

	public List<IdWeight> rareItemId;

	public float rareItemChance;

	public List<ItemIdCount> specifiedItems;

	public string dungeonFlowPath = "Prefab/Item/Rdungeon/DungeonFlow_Main";

	public int type;

	public bool IsTaskDungeon => level >= 100;

	public bool IsIron => type == 0;
}
