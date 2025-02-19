namespace SkillSystem;

public class AttAction
{
	protected SkEntity mSkEntity;

	public AttAction(SkEntity skEntity)
	{
		mSkEntity = skEntity;
	}

	public virtual void Do()
	{
	}
}
