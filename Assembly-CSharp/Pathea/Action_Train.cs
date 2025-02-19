namespace Pathea;

public class Action_Train : PEAction
{
	private string mAnim;

	public override PEActionType ActionType => PEActionType.GetOnTrain;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.GetOnTrain, state: true);
		PEActionParamS pEActionParamS = para as PEActionParamS;
		mAnim = pEActionParamS.str;
		if (null != base.equipCmpt)
		{
			base.equipCmpt.HideEquipmentByVehicle(hide: true);
		}
		if (!string.IsNullOrEmpty(mAnim) && null != base.anim)
		{
			base.anim.SetBool(mAnim, value: true);
		}
		if (null != base.viewCmpt)
		{
			base.viewCmpt.ActivateInjured(value: false);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
		if (base.motionMgr.Entity == PeSingleton<MainPlayer>.Instance.entity)
		{
			PeCamera.SetBool("OnMonorail", value: true);
		}
	}

	public override bool Update()
	{
		return false;
	}

	public override void OnModelDestroy()
	{
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.GetOnTrain, state: false);
		if (null != base.equipCmpt)
		{
			base.equipCmpt.HideEquipmentByVehicle(hide: false);
		}
		if (!string.IsNullOrEmpty(mAnim) && null != base.anim)
		{
			base.anim.SetBool(mAnim, value: false);
		}
		if (null != base.viewCmpt)
		{
			base.viewCmpt.ActivateInjured(value: true);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = true;
		}
		if (base.motionMgr.Entity == PeSingleton<MainPlayer>.Instance.entity)
		{
			PeCamera.SetBool("OnMonorail", value: false);
		}
	}
}
