namespace ScenarioRTL;

public abstract class Condition : StatementObject
{
	protected int reqId;

	private static int maxReqId;

	protected bool isWaiting => reqId != 0;

	protected void BeginReq()
	{
		reqId = ++maxReqId;
		ConditionReq.AddReq(reqId);
	}

	protected void EndReq()
	{
		ConditionReq.RemoveReq(reqId);
		reqId = 0;
	}

	protected virtual void SendReq()
	{
	}

	protected bool? ReqCheck()
	{
		if (!isWaiting)
		{
			BeginReq();
			SendReq();
			return null;
		}
		bool? result = ConditionReq.GetResult(reqId);
		if (result.HasValue)
		{
			EndReq();
		}
		return result;
	}

	public abstract bool? Check();
}
