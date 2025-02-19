using System.Collections.Generic;

public class NPCType
{
	public const int AD_NPC = 1;

	public const int TOWN_NPC = 2;

	public const int BUILDING_NPC = 3;

	public const int STRD_NPC = 4;

	public const int ST_NPC = 5;

	public const int CUSTOM_NPC = 6;

	public const int AD_MAINNPC = 7;
}
public struct NpcType
{
	public List<int> npcs;

	public int type;
}
