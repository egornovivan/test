using System.Collections.Generic;

public class AdStoryData
{
	public int m_ID;

	public List<AdCreNPC> m_creNPC;

	public List<AdEnterArea> m_enterArea;

	public List<AdNPCMove> m_npcMove;

	public List<int> m_getMissionID;

	public List<int> m_comMissionID;

	public int m_showTip;

	public AdStoryData()
	{
		m_creNPC = new List<AdCreNPC>();
		m_enterArea = new List<AdEnterArea>();
		m_npcMove = new List<AdNPCMove>();
		m_getMissionID = new List<int>();
		m_comMissionID = new List<int>();
	}
}
