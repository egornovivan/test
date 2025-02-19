using System.Collections.Generic;

public class TalkData
{
	public int m_ID;

	public int m_NpcID;

	public List<int> m_otherNpc;

	public string m_Content;

	public int m_SoundID;

	public string m_ClipName;

	public bool isRadio;

	public int needLangSkill;

	public object talkToNpcidOrVecter3;

	public object moveTonpcidOrvecter3;

	public int m_moveType;

	public List<int> m_endOtherNpc;

	public TalkData()
	{
		m_otherNpc = new List<int>();
		m_endOtherNpc = new List<int>();
	}
}
