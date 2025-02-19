using System.Collections.Generic;

public class DungeonBaseData
{
	public int id;

	public int level;

	public List<IdWeight> landMonsterId;

	public List<IdWeight> waterMonsterId;

	public float monsterAmount;

	public int monsterBuff;

	public List<IdWeight> bossId;

	public List<IdWeight> bossWaterId;

	public int bossMonsterBuff;

	public List<IdWeight> minBossId;

	public List<IdWeight> minBossWaterId;

	public int minBossMonsterBuff;

	public List<IdWeight> itemId;

	public float itemAmount;

	public List<IdWeight> rareItemId;

	public float rareItemChance;

	public List<IdWeight> rareItemTags;

	public List<ItemIdCount> specifiedItems;

	public string dungeonFlowPath = "Prefab/Item/Rdungeon/DungeonFlow_Main";

	public int type;

	public static List<string> isoTagList = new List<string>();

	public bool IsTaskDungeon => level >= 100;

	public bool IsIron => type == 0;

	public DungeonType Type => (DungeonType)type;

	public static void InitIsoTagList()
	{
		isoTagList = new List<string>();
		isoTagList.Add("Creation");
		isoTagList.Add("Equipment");
		isoTagList.Add("Sword");
		isoTagList.Add("Axe");
		isoTagList.Add("Bow");
		isoTagList.Add("Shield");
		isoTagList.Add("Gun");
		isoTagList.Add("Carrier");
		isoTagList.Add("Vehicle");
		isoTagList.Add("Ship");
		isoTagList.Add("Aircraft");
		isoTagList.Add("Armor");
		isoTagList.Add("Head");
		isoTagList.Add("Body");
		isoTagList.Add("Arm And Leg");
		isoTagList.Add("Head And Foot");
		isoTagList.Add("Decoration");
		isoTagList.Add("Robot");
		isoTagList.Add("AI Turret");
		isoTagList.Add("Object");
	}
}
