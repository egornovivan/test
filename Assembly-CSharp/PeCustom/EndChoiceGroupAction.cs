using System.IO;
using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("END CHOICE GROUP")]
public class EndChoiceGroupAction : Action
{
	private bool _started = true;

	private bool _closeUIWnd;

	protected override void OnCreate()
	{
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null)
		{
			if (_started)
			{
				if (PeCustomScene.Self.scenario.dialogMgr.EndChooseGroup())
				{
					GameUI.Instance.mNPCSpeech.speechInterpreter.SetNpoEntity(PeSingleton<CreatureMgr>.Instance.mainPlayer);
					GameUI.Instance.mNPCSpeech.speechInterpreter.SetChoiceCount();
					GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClickForward += OnChioceClick;
					GameUI.Instance.mNPCSpeech.Show();
					GameUI.Instance.mNpcDialog.allowShow = false;
				}
				_started = false;
			}
			else if (_closeUIWnd)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public override void RestoreState(BinaryReader r)
	{
	}

	public override void StoreState(BinaryWriter w)
	{
	}

	private void OnChioceClick(int choice_id)
	{
		_closeUIWnd = true;
		GameUI.Instance.mNPCSpeech.Hide();
		GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClickForward -= OnChioceClick;
		GameUI.Instance.mNpcDialog.allowShow = true;
	}
}
