using Pathea;

public class PEAnimatorSkill : PEAnimatorEvent
{
	public int skillID;

	internal override void OnTrigger()
	{
		if ((!PeGameMgr.IsMulti || (!(null == base.Entity) && !(null == base.Entity.netCmpt) && base.Entity.netCmpt.IsController)) && base.Entity != null && skillID > 0)
		{
			if (base.Entity.attackEnemy != null)
			{
				base.Entity.StartSkill(base.Entity.attackEnemy.skTarget, skillID);
			}
			else
			{
				base.Entity.StartSkill(null, skillID);
			}
		}
	}
}
