using ScenarioRTL;

namespace PeCustom;

[Statement("HIDE SYSTEM UI", true)]
public class HideSysuiAction : Action
{
	private ESystemUI sysui;

	protected override void OnCreate()
	{
		sysui = (ESystemUI)Utility.ToEnumInt(base.parameters["ui"]);
	}

	public override bool Logic()
	{
		switch (sysui)
		{
		case ESystemUI.CharacterInfo:
			GameUI.Instance.mUIPlayerInfoCtrl.Hide();
			break;
		case ESystemUI.ItemPackage:
			GameUI.Instance.mItemPackageCtrl.Hide();
			break;
		case ESystemUI.Replicator:
			GameUI.Instance.mCompoundWndCtrl.Hide();
			break;
		case ESystemUI.Phone:
			GameUI.Instance.mPhoneWnd.Hide();
			break;
		case ESystemUI.CreationSystem:
			VCEditor.Quit();
			break;
		}
		return true;
	}
}
