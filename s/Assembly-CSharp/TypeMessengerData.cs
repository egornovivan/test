using System.Collections.Generic;

public class TypeMessengerData
{
	public int m_TargetID;

	public int m_ScriptID;

	public string m_Desc;

	public int m_iNpc;

	public int m_iReplyNpc;

	public int m_ItemID;

	public int m_ItemNum;

	public List<int> m_ReceiveList;

	public MissionRand m_AdNpcRadius;

	public TypeMessengerData()
	{
		m_Desc = string.Empty;
		m_ReceiveList = new List<int>();
	}
}
