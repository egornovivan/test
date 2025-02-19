namespace PatheaScript;

public class EventProxyScriptBegin : EventProxy
{
	private void ScriptBegin(PsScript script)
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
		InternalEvent.Instance.eventScriptBegin += ScriptBegin;
		return true;
	}

	public override void Unsubscribe()
	{
		InternalEvent.Instance.eventScriptBegin -= ScriptBegin;
		base.Unsubscribe();
	}
}
