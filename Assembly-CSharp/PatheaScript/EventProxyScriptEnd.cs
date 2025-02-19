namespace PatheaScript;

public class EventProxyScriptEnd : EventProxy
{
	private void ScriptEnd(PsScript script)
	{
		if (script != null)
		{
			Emit(script);
		}
	}

	public override bool Subscribe()
	{
		if (!base.Subscribe())
		{
			return false;
		}
		InternalEvent.Instance.eventScriptEnd += ScriptEnd;
		return true;
	}

	public override void Unsubscribe()
	{
		InternalEvent.Instance.eventScriptEnd -= ScriptEnd;
		base.Unsubscribe();
	}
}
