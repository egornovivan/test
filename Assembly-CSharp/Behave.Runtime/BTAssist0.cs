namespace Behave.Runtime;

[BehaveAction(typeof(BTAssist0), "Assist0")]
public class BTAssist0 : BTNormal
{
	private class Data
	{
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Failure;
	}
}
