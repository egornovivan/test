namespace Pathea;

public class TeamDb
{
	public static ChatTeamDb LoadchatTeamDb(PeEntity npc)
	{
		if (npc == null || npc.NpcCmpt == null || npc.NpcCmpt.TeamData == null)
		{
			return null;
		}
		ChatTeamDb chatTeamDb = new ChatTeamDb();
		chatTeamDb.Set(npc.NpcCmpt.TeamData);
		return chatTeamDb;
	}
}
