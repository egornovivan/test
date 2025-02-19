using AnimFollow;
using PEIK;
using PETools;
using RootMotion.FinalIK;
using Steer3D;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class BiologyViewRoot : MonoBehaviour
{
	public PEModelController modelController;

	public PERagdollController ragdollController;

	public IK[] ikArray;

	public IKFlashLight ikFlashLight;

	public FullBodyBipedIK fbbik;

	public GrounderFBBIK grounderFBBIK;

	public HumanPhyCtrl humanPhyCtrl;

	public IKAimCtrl ikAimCtrl;

	public IKAnimEffectCtrl ikAnimEffectCtrl;

	public IKDrive ikDrive;

	public PEDefenceTrigger defenceTrigger;

	public PEPathfinder pathFinder;

	public PEMotor motor;

	public SteerAgent steerAgent;

	public AnimFollow_AF animFollow_AF;

	public BeatParam beatParam;

	public MoveParam moveParam;

	public PEBarrelController barrelController;

	public BillBoard billBoard;

	public ArmorBones armorBones;

	public PEVision[] visions;

	public PEHearing[] hears;

	public PENative native;

	public PEMonster monster;

	public void Reset()
	{
		modelController = PEUtil.GetCmpt<PEModelController>(base.transform);
		ragdollController = PEUtil.GetCmpt<PERagdollController>(base.transform);
		ikArray = PEUtil.GetCmpts<IK>(base.transform);
		ikFlashLight = PEUtil.GetCmpt<IKFlashLight>(base.transform);
		fbbik = PEUtil.GetCmpt<FullBodyBipedIK>(base.transform);
		grounderFBBIK = PEUtil.GetCmpt<GrounderFBBIK>(base.transform);
		humanPhyCtrl = PEUtil.GetCmpt<HumanPhyCtrl>(base.transform);
		ikAimCtrl = PEUtil.GetCmpt<IKAimCtrl>(base.transform);
		ikAnimEffectCtrl = PEUtil.GetCmpt<IKAnimEffectCtrl>(base.transform);
		ikDrive = PEUtil.GetCmpt<IKDrive>(base.transform);
		defenceTrigger = PEUtil.GetCmpt<PEDefenceTrigger>(base.transform);
		pathFinder = PEUtil.GetCmpt<PEPathfinder>(base.transform);
		motor = PEUtil.GetCmpt<PEMotor>(base.transform);
		steerAgent = PEUtil.GetCmpt<SteerAgent>(base.transform);
		animFollow_AF = PEUtil.GetCmpt<AnimFollow_AF>(base.transform);
		beatParam = PEUtil.GetCmpt<BeatParam>(base.transform);
		moveParam = PEUtil.GetCmpt<MoveParam>(base.transform);
		barrelController = PEUtil.GetCmpt<PEBarrelController>(base.transform);
		billBoard = PEUtil.GetCmpt<BillBoard>(base.transform);
		armorBones = PEUtil.GetCmpt<ArmorBones>(base.transform);
		visions = PEUtil.GetCmpts<PEVision>(base.transform);
		hears = PEUtil.GetCmpts<PEHearing>(base.transform);
		native = PEUtil.GetCmpt<PENative>(base.transform);
		monster = PEUtil.GetCmpt<PEMonster>(base.transform);
		if (null != modelController)
		{
			modelController.ResetModelInfo();
		}
		if (null != animFollow_AF)
		{
			animFollow_AF.ResetModelInfo();
		}
		if (null != ragdollController)
		{
			ragdollController.ResetRagdoll();
		}
	}
}
