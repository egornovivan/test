using PatheaScript;

namespace PatheaScriptExt;

public class EPlayerTalkToNpc : Event
{
	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		return false;
	}

	public override bool Filter(params object[] param)
	{
		return true;
	}

	public override string ToString()
	{
		return null;
	}
}
