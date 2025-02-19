namespace SkillSystem;

public class AttCond
{
	protected SkEntity mSkEntity;

	public AttCond(SkEntity skEntity)
	{
		mSkEntity = skEntity;
	}

	public virtual bool Check()
	{
		return false;
	}
}
