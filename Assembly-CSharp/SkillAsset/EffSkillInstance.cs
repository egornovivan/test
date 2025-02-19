namespace SkillAsset;

public class EffSkillInstance
{
	public enum EffSection
	{
		None,
		Start,
		Running,
		Completed,
		Max
	}

	public EffSection m_section;

	public EffSkill m_data;

	public float m_timeStartPrep;

	public CoroutineStoppable m_runner;

	public CoroutineStoppable m_sharedRunner;

	public ISkillTarget mNextTarget;

	public bool mSkillCostTimeAdd;

	public static bool MatchId(EffSkillInstance iter, int id)
	{
		return iter.m_data.m_id == id;
	}

	public static bool MatchType(EffSkillInstance iter, short type)
	{
		return iter.m_data.m_cdInfo.m_type == type;
	}
}
