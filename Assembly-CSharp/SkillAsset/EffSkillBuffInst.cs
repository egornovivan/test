namespace SkillAsset;

public class EffSkillBuffInst
{
	public EffSkillBuff m_buff;

	public CoroutineStoppable m_runner;

	public int m_skillId;

	public static EffSkillBuffInst TakeEffect(SkillRunner caster, SkillRunner buffHost, EffSkillBuff buff, int parentSkillId)
	{
		EffSkillBuffInst effSkillBuffInst = new EffSkillBuffInst();
		effSkillBuffInst.m_buff = buff;
		effSkillBuffInst.m_skillId = ((!(buff.m_timeActive > float.Epsilon)) ? parentSkillId : 0);
		effSkillBuffInst.m_runner = new CoroutineStoppable(buffHost, buff.Exec(caster, buffHost, effSkillBuffInst));
		return effSkillBuffInst;
	}
}
