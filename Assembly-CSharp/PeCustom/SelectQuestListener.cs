using ScenarioRTL;

namespace PeCustom;

[Statement("SELECT QUEST")]
public class SelectQuestListener : EventListener
{
	private int id;

	private OBJECT obj;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
		obj = Utility.ToObject(base.parameters["object"]);
	}

	public override void Listen()
	{
		if (GameUI.Instance != null && GameUI.Instance.mNpcDialog != null)
		{
			GameUI.Instance.mNpcDialog.dialogInterpreter.onQuestClick += OnQuestSelect;
		}
	}

	public override void Close()
	{
		if (GameUI.Instance != null && GameUI.Instance.mNpcDialog != null)
		{
			GameUI.Instance.mNpcDialog.dialogInterpreter.onQuestClick -= OnQuestSelect;
		}
	}

	private void OnQuestSelect(int world_index, int npo_id, int quest_id)
	{
		if (world_index == obj.Group && obj.Id == npo_id && quest_id == id)
		{
			Post();
		}
	}
}
