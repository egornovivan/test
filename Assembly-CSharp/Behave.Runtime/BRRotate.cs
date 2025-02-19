using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BRRotate), "RRotate")]
public class BRRotate : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.RRotate);
		if (GetRequest(EReqType.Rotate) is RQRotate rQRotate && rQRotate.CanRun())
		{
			SetRotation(rQRotate.rotation);
			StopMove();
			RemoveRequest(rQRotate);
		}
		return BehaveResult.Failure;
	}
}
