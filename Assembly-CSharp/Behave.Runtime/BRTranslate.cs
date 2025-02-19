using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRTranslate), "RTranslate")]
public class BRTranslate : BTNormal
{
	private float translateTime;

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.RTranslate);
		if (GetRequest(EReqType.Translate) is RQTranslate rQTranslate && rQTranslate.CanRun())
		{
			if (translateTime == 0f)
			{
				translateTime = Time.time;
				SetPosition(rQTranslate.position, rQTranslate.adjust);
			}
			if (!base.hasModel && Time.time - translateTime < 5f)
			{
				return BehaveResult.Running;
			}
			ClearNpcMount();
			StopMove();
			RemoveRequest(rQTranslate);
			translateTime = 0f;
		}
		return BehaveResult.Failure;
	}
}
