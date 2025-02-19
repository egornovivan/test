using Pathea.Operate;
using PEIK;
using RootMotion.FinalIK;
using UnityEngine;

namespace Pathea;

public class Action_Ride : PEAction
{
	private string _animName;

	private bool _endAction;

	private PeEntity _monsterEntity;

	private TameMonsterManager _tameMonsterMgr;

	private FullBodyBipedIK _fullbodyBipedIK;

	private Transform _ridePosTrans;

	private int _monsterID;

	private bool _enityIsMe;

	private PERide _ride;

	private int _backupIKIterations = 1;

	private Interaction_Ride m_Interaction;

	public override PEActionType ActionType => PEActionType.Ride;

	public override void DoAction(PEActionParam para = null)
	{
		PEActionParamVQSNS pEActionParamVQSNS = para as PEActionParamVQSNS;
		base.motionMgr.SetMaskState(PEActionMask.Ride, state: true);
		_endAction = false;
		if (null != base.trans)
		{
			base.trans.position = pEActionParamVQSNS.vec;
			base.trans.rotation = pEActionParamVQSNS.q;
		}
		if (PeSingleton<EntityMgr>.Instance != null && pEActionParamVQSNS.enitytID > 0)
		{
			_monsterEntity = PeSingleton<EntityMgr>.Instance.Get(pEActionParamVQSNS.enitytID);
		}
		if ((bool)base.entity)
		{
			_fullbodyBipedIK = base.entity.GetComponentInChildren<FullBodyBipedIK>();
			if ((bool)_fullbodyBipedIK && _fullbodyBipedIK.solver != null)
			{
				_backupIKIterations = _fullbodyBipedIK.solver.iterations;
				_fullbodyBipedIK.solver.iterations = TameMonsterConfig.instance.IKIterationSize;
			}
		}
		if ((bool)base.entity && (bool)base.entity.operateCmpt && (bool)_monsterEntity && (bool)_monsterEntity.biologyViewCmpt && (bool)_monsterEntity.biologyViewCmpt.biologyViewRoot && (bool)_monsterEntity.biologyViewCmpt.biologyViewRoot.modelController)
		{
			PERides component = _monsterEntity.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<PERides>();
			if ((bool)component)
			{
				if (component.HasOperater(base.entity.operateCmpt))
				{
					_ride = component.GetRideByOperater(base.entity.operateCmpt);
				}
				else
				{
					_ride = component.GetUseable();
					if (null != _ride)
					{
						_ride.Operator = base.entity.operateCmpt;
						base.entity.operateCmpt.Operate = _ride;
					}
				}
			}
		}
		base.viewCmpt.ActivateCollider(value: false);
		base.motionMgr.FreezePhyState(GetType(), v: true);
		if (null != base.motionMgr.Entity.IKCmpt)
		{
			base.motionMgr.Entity.IKCmpt.EnableGroundFBBIK = false;
		}
		if (null != _monsterEntity.IKCmpt)
		{
			_monsterEntity.IKCmpt.ikEnable = false;
		}
		if ((bool)_monsterEntity)
		{
			_monsterID = _monsterEntity.Id;
		}
		if (null != base.anim)
		{
			_animName = pEActionParamVQSNS.strAnima;
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.SetBool(_animName, value: true);
		}
		if ((bool)base.entity && (bool)_monsterEntity)
		{
			m_Interaction = new Interaction_Ride();
			m_Interaction.Init(base.entity.transform, _monsterEntity.transform);
			m_Interaction.StartInteraction();
		}
		else
		{
			Debug.LogFormat("Ride ik open failed! player is null:{0} , monster is null:{1}", null == base.entity, null == _monsterEntity);
		}
		if (null != _monsterEntity && null != _monsterEntity.biologyViewCmpt)
		{
			_ridePosTrans = _monsterEntity.biologyViewCmpt.GetModelTransform(pEActionParamVQSNS.boneStr);
		}
		if ((bool)base.entity && base.entity.Id == PeSingleton<MainPlayer>.Instance.entityId)
		{
			if ((bool)_ridePosTrans && (bool)_monsterEntity)
			{
				_tameMonsterMgr = base.entity.gameObject.AddComponent<TameMonsterManager>();
				_tameMonsterMgr.LoadTameSucceed(base.entity, _monsterEntity, _ridePosTrans, _monsterEntity.monstermountCtrl.ctrlType != ECtrlType.Mount);
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork.RequestAddRideMonster(_monsterID);
				}
			}
			_enityIsMe = true;
		}
		else
		{
			_enityIsMe = false;
		}
	}

	public override bool Update()
	{
		if (!_endAction)
		{
			if (!_enityIsMe && null != _ridePosTrans)
			{
				base.entity.peTrans.position = _ridePosTrans.position + TameMonsterConfig.instance.IkRideOffset;
			}
			if (_enityIsMe && (null == _monsterEntity || _monsterEntity.IsDeath() || _monsterEntity.isRagdoll || null == base.entity || base.entity.IsDeath() || base.entity.isRagdoll) && (bool)base.entity && (bool)_ride)
			{
				_ride.StopOperate(base.entity.operateCmpt, EOperationMask.Ride);
			}
		}
		return false;
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
		}
		OnEndAction();
	}

	private void OnEndAction()
	{
		if ((bool)base.entity && (bool)base.entity.operateCmpt)
		{
			base.entity.operateCmpt.Operate = null;
		}
		if ((bool)_ride)
		{
			_ride.Operator = null;
		}
		if (_enityIsMe)
		{
			if ((bool)_tameMonsterMgr)
			{
				_tameMonsterMgr.ResetInfo();
				Object.Destroy(_tameMonsterMgr);
			}
			if ((bool)base.entity && (bool)base.entity.mountCmpt)
			{
				base.entity.mountCmpt.DelMount();
			}
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.RequestDelMountMonster(_monsterID);
			}
		}
		if ((bool)_fullbodyBipedIK && _fullbodyBipedIK.solver != null)
		{
			_fullbodyBipedIK.solver.iterations = _backupIKIterations;
		}
		if (m_Interaction != null)
		{
			m_Interaction.EndInteraction();
			m_Interaction = null;
		}
		if (null != base.anim)
		{
			base.anim.SetBool(_animName, value: false);
		}
		if (_enityIsMe && (bool)base.trans && (bool)_monsterEntity && (bool)_monsterEntity.peTrans && (bool)_monsterEntity.peTrans.realTrans)
		{
			base.trans.position = new Vector3(base.trans.position.x, base.trans.position.y + _monsterEntity.peTrans.bound.size.y * 0.5f, base.trans.position.z);
		}
		base.motionMgr.FreezePhyState(GetType(), v: false);
		base.motionMgr.SetMaskState(PEActionMask.Ride, state: false);
		if (null != base.motionMgr.Entity.IKCmpt)
		{
			base.motionMgr.Entity.IKCmpt.EnableGroundFBBIK = true;
		}
		if (null != _monsterEntity.IKCmpt)
		{
			_monsterEntity.IKCmpt.ikEnable = true;
		}
		base.viewCmpt.ActivateCollider(value: true);
		_endAction = true;
	}
}
