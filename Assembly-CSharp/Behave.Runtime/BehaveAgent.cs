namespace Behave.Runtime;

public class BehaveAgent : IAgent
{
	private static BehaveAgent _Agent;

	public static BehaveAgent Agent
	{
		get
		{
			if (_Agent == null)
			{
				_Agent = new BehaveAgent();
			}
			return _Agent;
		}
	}

	public void Reset(Tree sender)
	{
	}

	public int SelectTopPriority(Tree sender, params int[] IDs)
	{
		return 0;
	}

	public BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Success;
	}
}
