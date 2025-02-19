using PatheaScript;

namespace PatheaScriptExt;

public class EUiClickedProxy : EventProxy
{
	public override bool Subscribe()
	{
		return true;
	}

	private void UiClicked(int id)
	{
		Emit(id);
	}

	public override void Unsubscribe()
	{
	}
}
