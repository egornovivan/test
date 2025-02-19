using ScenarioRTL;

namespace PeCustom;

[Statement("SHOW SYSTEM UI", true)]
public class ShowSysuiAction : Action
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
			GameUI.Instance.mUIPlayerInfoCtrl.Show();
			break;
		case ESystemUI.ItemPackage:
			GameUI.Instance.mItemPackageCtrl.Show();
			break;
		case ESystemUI.Replicator:
			GameUI.Instance.mCompoundWndCtrl.Show();
			break;
		case ESystemUI.Phone:
			GameUI.Instance.mPhoneWnd.Show();
			break;
		case ESystemUI.CreationSystem:
			VCEditor.Open();
			break;
		}
		return true;
	}
}
