using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Wentfly : PEAction
{
	public override PEActionType ActionType => PEActionType.Wentfly;

	public HumanPhyCtrl phyCtrl { get; set; }

	public override void DoAction(PEActionParam para = null)
	{
		if (null == base.viewCmpt)
		{
			return;
		}
		base.motionMgr.SetMaskState(PEActionMask.Wentfly, state: true);
		PEActionParamVFNS pEActionParamVFNS = para as PEActionParamVFNS;
		Vector3 hitForce = pEActionParamVFNS.vec * pEActionParamVFNS.f;
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(pEActionParamVFNS.n);
		base.motionMgr.FreezePhyState(GetType(), v: true);
		if (null != peEntity)
		{
			Transform ragdollTransform = peEntity.biologyViewCmpt.GetRagdollTransform(pEActionParamVFNS.str);
			if (null != ragdollTransform)
			{
				RagdollHitInfo ragdollHitInfo = new RagdollHitInfo();
				ragdollHitInfo.hitTransform = ragdollTransform;
				ragdollHitInfo.hitPoint = ragdollTransform.position;
				ragdollHitInfo.hitForce = hitForce;
				ragdollHitInfo.hitNormal = -hitForce.normalized;
				base.viewCmpt.ActivateRagdoll(ragdollHitInfo, isGetupReady: false);
			}
		}
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
			phyCtrl.CancelMoveRequest();
		}
	}

	public override bool Update()
	{
		if ((null == base.viewCmpt || !base.viewCmpt.IsRagdoll) && !base.viewCmpt.IsRagdoll)
		{
			EndImmediately();
			return true;
		}
		if (null == base.viewCmpt || base.viewCmpt.IsReadyGetUp())
		{
			OnEndAction();
			base.motionMgr.DoActionImmediately(PEActionType.GetUp);
			return true;
		}
		return false;
	}

	public override void EndAction()
	{
		OnEndAction();
		base.motionMgr.DoActionImmediately(PEActionType.GetUp);
	}

	public override void EndImmediately()
	{
		OnEndAction();
	}

	private void OnEndAction()
	{
		base.motionMgr.SetMaskState(PEActionMask.Wentfly, state: false);
		base.motionMgr.FreezePhyState(GetType(), v: false);
	}
}
