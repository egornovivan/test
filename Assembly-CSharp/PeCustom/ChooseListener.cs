using ScenarioRTL;

namespace PeCustom;

[Statement("CHOOSE")]
public class ChooseListener : EventListener
{
	private int id;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
	}

	public override void Listen()
	{
		if (GameUI.Instance != null && GameUI.Instance.mNPCSpeech != null)
		{
			GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClick += OnSelectChoice;
		}
	}

	public override void Close()
	{
		if (GameUI.Instance != null && GameUI.Instance.mNPCSpeech != null)
		{
			GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClick -= OnSelectChoice;
		}
	}

	private void OnSelectChoice(int choice_id)
	{
		if (choice_id == id)
		{
			Post();
		}
	}
}
