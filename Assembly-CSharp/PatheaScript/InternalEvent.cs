namespace PatheaScript;

public class InternalEvent
{
	public delegate void ScriptChanged(PsScript script);

	private static InternalEvent instance;

	public static InternalEvent Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new InternalEvent();
			}
			return instance;
		}
	}

	public event ScriptChanged eventScriptBegin;

	public event ScriptChanged eventScriptEnd;

	public void EmitScriptBegin(PsScript script)
	{
		if (this.eventScriptBegin != null)
		{
			this.eventScriptBegin(script);
		}
	}

	public void EmitScriptEnd(PsScript script)
	{
		if (this.eventScriptEnd != null)
		{
			this.eventScriptEnd(script);
		}
	}
}
