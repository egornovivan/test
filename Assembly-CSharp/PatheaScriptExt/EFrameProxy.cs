using PatheaScript;

namespace PatheaScriptExt;

public class EFrameProxy : EventProxy
{
	public override void Tick()
	{
		Emit();
	}
}
