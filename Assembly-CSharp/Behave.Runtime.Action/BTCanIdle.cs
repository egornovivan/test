using Pathea;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTCanIdle), "CanIdle")]
public class BTCanIdle : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.entity != null && base.entity.NpcCmpt != null && base.entity.NpcCmpt.csCanIdle && NpcThinkDb.CanDoing(base.entity, EThinkingType.Stroll))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
