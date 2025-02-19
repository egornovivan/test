using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;
using SkillSystem;
using uLink;
using UnityEngine;

public class SkNetworkInterface : NetworkInterface
{
	private delegate void SendActionEventHander(PEActionType type, PEActionParam obj);

	private Dictionary<PEActionType, SendActionEventHander> SendActionEvent = new Dictionary<PEActionType, SendActionEventHander>();

	private MotionMgrCmpt _mtCmpt;

	internal Motion_Move _move;

	private PeTrans _trans;

	private IKCmpt ikCmpt;

	private Vector3 _oldIkPos;

	private float lastRequestTime;

	protected MotionMgrCmpt MtCmpt
	{
		get
		{
			if (_mtCmpt == null && runner != null)
			{
				SkAliveEntity skEntityPE = runner.SkEntityPE;
				if (skEntityPE != null)
				{
					_mtCmpt = skEntityPE.Entity.GetCmpt<MotionMgrCmpt>();
				}
			}
			return _mtCmpt;
		}
	}

	protected PeTrans Trans
	{
		get
		{
			if (_trans == null && runner != null)
			{
				SkAliveEntity skEntityPE = runner.SkEntityPE;
				if (skEntityPE != null)
				{
					_trans = skEntityPE.Entity.GetCmpt<PeTrans>();
				}
			}
			return _trans;
		}
	}

	private IKCmpt IkCmpt
	{
		get
		{
			if (ikCmpt == null && runner != null)
			{
				SkAliveEntity skEntityPE = runner.SkEntityPE;
				if (skEntityPE != null)
				{
					ikCmpt = skEntityPE.Entity.GetCmpt<IKCmpt>();
				}
			}
			return ikCmpt;
		}
	}

	protected event Action OnSkAttrInitEvent;

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		if (runner.SkEntityBase != null)
		{
			RPCServer(EPacketType.PT_InGame_SKSyncInitAttrs);
			RPCServer(EPacketType.PT_InGame_SKDAQueryEntityState);
		}
	}

	public bool IsStaticNet()
	{
		if (this is SubTerrainNetwork || this is VoxelTerrainNetwork)
		{
			return true;
		}
		return false;
	}

	protected void BindSkAction()
	{
		BindAction(EPacketType.PT_InGame_SKSyncAttr, RPC_SKSyncAttr);
		BindAction(EPacketType.PT_InGame_SKStartSkill, RPC_SKStartSkill);
		BindAction(EPacketType.PT_InGame_SKStopSkill, RPC_SKStopSkill);
		BindAction(EPacketType.PT_InGame_SKBLoop, RPC_SKBLoop);
		BindAction(EPacketType.PT_InGame_SKSyncInitAttrs, RPC_SKSyncInitAttrs);
		BindAction(EPacketType.PT_InGame_SKFellTree, RPC_SKFellTree);
		BindAction(EPacketType.PT_InGame_SKIKPos, RPC_SKIKPos);
		BindAction(EPacketType.PT_InGame_SKDAVVF, RPC_SKDAVVF);
		BindAction(EPacketType.PT_InGame_SKDAVFNS, RPC_SKDAVFNS);
		BindAction(EPacketType.PT_InGame_SKDANO, RPC_SKDANO);
		BindAction(EPacketType.PT_InGame_SKDAV, RPC_SKDAV);
		BindAction(EPacketType.PT_InGame_SKDAVVN, RPC_SKDAVVN);
		BindAction(EPacketType.PT_InGame_SKDAVQNS, RPC_SKDAVQNS);
		BindAction(EPacketType.PT_InGame_SKDAVQ, RPC_SKDAVQ);
		BindAction(EPacketType.PT_InGame_SKDAS, RPC_SKDAS);
		BindAction(EPacketType.PT_InGame_SKDAVQS, RPC_SKDAVQS);
		BindAction(EPacketType.PT_InGame_SKDAVVNN, RPC_SKDAVVNN);
		BindAction(EPacketType.PT_InGame_SKDAN, RPC_SKDAN);
		BindAction(EPacketType.PT_InGame_SKDAB, RPC_SKDAB);
		BindAction(EPacketType.PT_InGame_SKDAVQN, RPC_SKDAVQN);
		BindAction(EPacketType.PT_InGame_SKDAVFVFS, RPC_SKDAVFVFS);
		BindAction(EPacketType.PT_InGame_SKDAVQSN, RPC_SKDAVQSN);
		BindAction(EPacketType.PT_InGame_SKDAEndAction, RPC_SKDAEndAction);
		BindAction(EPacketType.PT_InGame_SKDAEndImmediately, RPC_SKDAEndImmediately);
		BindAction(EPacketType.PT_InGame_SKDAQueryEntityState, RPC_SKDAQueryEntityState);
		BindAction(EPacketType.PT_InGame_SKDAVQSNS, RPC_SKDAVQSNS);
		SendActionEvent.Add(PEActionType.Repulsed, SendSKDARepulsed);
		SendActionEvent.Add(PEActionType.Wentfly, SendSKDAWentfly);
		SendActionEvent.Add(PEActionType.Knocked, SendSKDAKnocked);
		SendActionEvent.Add(PEActionType.Death, SendSKDADeath);
		SendActionEvent.Add(PEActionType.GetUp, SendSKDAGetUp);
		SendActionEvent.Add(PEActionType.Revive, SendSKDARevive);
		SendActionEvent.Add(PEActionType.Step, SendSKDAStep);
		SendActionEvent.Add(PEActionType.EquipmentHold, SendSKDASwordHold);
		SendActionEvent.Add(PEActionType.HoldShield, SendSKDAHoldShield);
		SendActionEvent.Add(PEActionType.EquipmentPutOff, SendSKDASwordPutOff);
		SendActionEvent.Add(PEActionType.Fall, SendSKDAFall);
		SendActionEvent.Add(PEActionType.GunHold, SendSKDAGunHold);
		SendActionEvent.Add(PEActionType.GunReload, SendSKDAGunReload);
		SendActionEvent.Add(PEActionType.GunPutOff, SendSKDAGunPutOff);
		SendActionEvent.Add(PEActionType.BowHold, SendSKDABowHold);
		SendActionEvent.Add(PEActionType.BowPutOff, SendSKDABowPutOff);
		SendActionEvent.Add(PEActionType.BowShoot, SendSKDABowShoot);
		SendActionEvent.Add(PEActionType.BowReload, SendSKDABowReload);
		SendActionEvent.Add(PEActionType.Sleep, SendSKDASleep);
		SendActionEvent.Add(PEActionType.Eat, SendSKDAEat);
		SendActionEvent.Add(PEActionType.AimEquipHold, SendSKDAToolHold);
		SendActionEvent.Add(PEActionType.AimEquipPutOff, SendSKDAToolPutOff);
		SendActionEvent.Add(PEActionType.Dig, SendSKDADig);
		SendActionEvent.Add(PEActionType.Fell, SendSKDAFell);
		SendActionEvent.Add(PEActionType.Gather, SendSKDAGather);
		SendActionEvent.Add(PEActionType.PickUpItem, SendSKDAPickUpItem);
		SendActionEvent.Add(PEActionType.JetPack, SendSKDAJetPack);
		SendActionEvent.Add(PEActionType.Parachute, SendSKDAParachute);
		SendActionEvent.Add(PEActionType.Glider, SendSKDAGlider);
		SendActionEvent.Add(PEActionType.Climb, SendSKDAClimb);
		SendActionEvent.Add(PEActionType.Build, SendSKDABuild);
		SendActionEvent.Add(PEActionType.HoldFlashLight, SendSKDAHoldFlashLight);
		SendActionEvent.Add(PEActionType.Stuned, SendSKDAStuned);
		SendActionEvent.Add(PEActionType.Sit, SendSKDASit);
		SendActionEvent.Add(PEActionType.Operation, SendSKDAOperation);
		SendActionEvent.Add(PEActionType.Draw, SendSKDADraw);
		SendActionEvent.Add(PEActionType.TwoHandSwordHold, SendSKDATwoHandSwordHold);
		SendActionEvent.Add(PEActionType.TwoHandSwordPutOff, SendSKDATwoHandSwordPutOff);
		SendActionEvent.Add(PEActionType.Cure, SendSKDACure);
		SendActionEvent.Add(PEActionType.Leisure, SendSKDATalk);
		SendActionEvent.Add(PEActionType.AlienDeath, SendSKDAAlienDeath);
		SendActionEvent.Add(PEActionType.Ride, SendSKDARide);
		BindAction(EPacketType.PT_InGame_SkOnDamage, RPC_S2C_SkOnDamage);
		BindAction(EPacketType.PT_InGame_AbnormalConditionStart, RPC_S2C_AbnormalConditionStart);
		BindAction(EPacketType.PT_InGame_AbnormalConditionEnd, RPC_S2C_AbnormalConditionEnd);
		BindAction(EPacketType.PT_InGame_AbnormalCondition, RPC_S2C_AbnormalCondition);
		BindAction(EPacketType.PT_InGame_Jump, RPC_S2C_Jump);
		InvokeRepeating("SendIk", 0f, 0.1f);
	}

	internal void UpdateDriverStatus(CreationNetwork creation)
	{
		if (creation != null)
		{
			base.transform.position = creation.transform.position;
		}
	}

	private void SendIk()
	{
		if (base.hasOwnerAuth && IkCmpt != null && IkCmpt.aimActive && _oldIkPos != IkCmpt.aimTargetPos)
		{
			_oldIkPos = IkCmpt.aimTargetPos;
			RPCServer(EPacketType.PT_InGame_SKIKPos, _oldIkPos);
		}
	}

	protected void RequestAbnormalCondition()
	{
		RPCServer(EPacketType.PT_InGame_AbnormalCondition);
	}

	public void RequestJump(double jumpTime)
	{
		RPCServer(EPacketType.PT_InGame_Jump, jumpTime);
	}

	private void OnAttrChange(AttribType type, float value, int casterId, float dValue)
	{
		if (type == AttribType.Hp)
		{
			OnDamage(casterId, dValue);
		}
	}

	private void OnDamage(int casterId, float damage)
	{
		NetworkInterface networkInterface = NetworkInterface.Get<NetworkInterface>(casterId);
		SkEntity caster = ((!(null != networkInterface)) ? null : ((!(null != networkInterface.Runner)) ? null : networkInterface.Runner.SkEntityBase));
		if (base.Runner != null)
		{
			PESkEntity pESkEntity = base.Runner.SkEntityBase as PESkEntity;
			if (pESkEntity != null)
			{
				pESkEntity.DispatchHPChangeEvent(caster, damage);
			}
		}
	}

	public void SendDoAction(PEActionType type, PEActionParam obj)
	{
		if (base.hasOwnerAuth && SendActionEvent.ContainsKey(type))
		{
			SendActionEvent[type](type, obj);
		}
	}

	public void SendEndAction(PEActionType type)
	{
		if (base.hasOwnerAuth && SendActionEvent.ContainsKey(type))
		{
			RPCServer(EPacketType.PT_InGame_SKDAEndAction, type);
		}
	}

	public void SendEndImmediately(PEActionType type)
	{
		if (base.hasOwnerAuth && SendActionEvent.ContainsKey(type))
		{
			RPCServer(EPacketType.PT_InGame_SKDAEndImmediately, type);
		}
	}

	private void SendSKDARepulsed(PEActionType type, PEActionParam obj)
	{
		PEActionParamVVF pEActionParamVVF = obj as PEActionParamVVF;
		RPCServer(EPacketType.PT_InGame_SKDAVVF, type, pEActionParamVVF.vec1, pEActionParamVVF.vec2, pEActionParamVVF.f);
	}

	private void SendSKDAWentfly(PEActionType type, PEActionParam obj)
	{
		PEActionParamVFNS pEActionParamVFNS = obj as PEActionParamVFNS;
		RPCServer(EPacketType.PT_InGame_SKDAVFNS, type, pEActionParamVFNS.vec, pEActionParamVFNS.f, pEActionParamVFNS.n, pEActionParamVFNS.str);
	}

	private void SendSKDAKnocked(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDADeath(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGetUp(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDARevive(PEActionType type, PEActionParam obj)
	{
		PEActionParamB pEActionParamB = obj as PEActionParamB;
		RPCServer(EPacketType.PT_InGame_SKDAB, type, pEActionParamB.b);
	}

	private void SendSKDAStep(PEActionType type, PEActionParam obj)
	{
		PEActionParamV pEActionParamV = obj as PEActionParamV;
		RPCServer(EPacketType.PT_InGame_SKDAV, type, pEActionParamV.vec);
	}

	private void SendSKDASwordHold(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDASwordAttack(PEActionType type, PEActionParam obj)
	{
		PEActionParamVVN pEActionParamVVN = obj as PEActionParamVVN;
		RPCServer(EPacketType.PT_InGame_SKDAVVN, type, pEActionParamVVN.vec1, pEActionParamVVN.vec2, pEActionParamVVN.n);
	}

	private void SendSKDAHoldShield(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDASwordPutOff(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAFall(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGunHold(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGunFire(PEActionType type, PEActionParam obj)
	{
		PEActionParamB pEActionParamB = obj as PEActionParamB;
		RPCServer(EPacketType.PT_InGame_SKDAB, type, pEActionParamB.b);
	}

	private void SendSKDAGunReload(PEActionType type, PEActionParam obj)
	{
		PEActionParamN pEActionParamN = obj as PEActionParamN;
		RPCServer(EPacketType.PT_InGame_SKDAN, type, pEActionParamN.n);
	}

	private void SendSKDAGunPutOff(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowHold(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowPutOff(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowShoot(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowReload(PEActionType type, PEActionParam obj)
	{
		PEActionParamN pEActionParamN = obj as PEActionParamN;
		RPCServer(EPacketType.PT_InGame_SKDAN, type, pEActionParamN.n);
	}

	private void SendSKDASleep(PEActionType type, PEActionParam obj)
	{
		PEActionParamVQNS pEActionParamVQNS = obj as PEActionParamVQNS;
		RPCServer(EPacketType.PT_InGame_SKDAVQNS, type, pEActionParamVQNS.vec, pEActionParamVQNS.q, pEActionParamVQNS.n, pEActionParamVQNS.str);
	}

	private void SendSKDAEat(PEActionType type, PEActionParam obj)
	{
		PEActionParamS pEActionParamS = obj as PEActionParamS;
		RPCServer(EPacketType.PT_InGame_SKDAS, type, pEActionParamS.str);
	}

	private void SendSKDAToolHold(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAToolPutOff(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDADig(PEActionType type, PEActionParam obj)
	{
		PEActionParamV pEActionParamV = obj as PEActionParamV;
		RPCServer(EPacketType.PT_InGame_SKDAV, type, pEActionParamV.vec);
	}

	private void SendSKDAFell(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGather(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAPickUpItem(PEActionType type, PEActionParam obj)
	{
		PEActionParamVQ pEActionParamVQ = obj as PEActionParamVQ;
		RPCServer(EPacketType.PT_InGame_SKDAVQ, type, pEActionParamVQ.vec, pEActionParamVQ.q);
	}

	private void SendSKDAGetOnVehicle(PEActionType type, PEActionParam obj)
	{
		PEActionParamS pEActionParamS = obj as PEActionParamS;
		RPCServer(EPacketType.PT_InGame_SKDAS, type, pEActionParamS.str);
	}

	private void SendSKDAJetPack(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAParachute(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGlider(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAClimb(PEActionType type, PEActionParam obj)
	{
		PEActionParamVQN pEActionParamVQN = obj as PEActionParamVQN;
		RPCServer(EPacketType.PT_InGame_SKDAVQN, type, pEActionParamVQN.vec, pEActionParamVQN.q, pEActionParamVQN.n);
	}

	private void SendSKDABuild(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAHoldFlashLight(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAStuned(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDASit(PEActionType type, PEActionParam obj)
	{
		PEActionParamVQSN pEActionParamVQSN = obj as PEActionParamVQSN;
		RPCServer(EPacketType.PT_InGame_SKDAVQSN, type, pEActionParamVQSN.vec, pEActionParamVQSN.q, pEActionParamVQSN.str, pEActionParamVQSN.n);
	}

	private void SendSKDAOperation(PEActionType type, PEActionParam obj)
	{
		PEActionParamVQS pEActionParamVQS = obj as PEActionParamVQS;
		RPCServer(EPacketType.PT_InGame_SKDAVQS, type, pEActionParamVQS.vec, pEActionParamVQS.q, pEActionParamVQS.str);
	}

	private void SendSKDADraw(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDATwoHandSwordHold(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDATwoHandSwordPutOff(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDATwoHandSwordAttack(PEActionType type, PEActionParam obj)
	{
		PEActionParamVVNN pEActionParamVVNN = obj as PEActionParamVVNN;
		RPCServer(EPacketType.PT_InGame_SKDAVVNN, type, pEActionParamVVNN.vec1, pEActionParamVVNN.vec2, pEActionParamVVNN.n1, pEActionParamVVNN.n2);
	}

	private void SendSKDAJump(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDACure(PEActionType type, PEActionParam obj)
	{
		PEActionParamVFVFS pEActionParamVFVFS = obj as PEActionParamVFVFS;
		RPCServer(EPacketType.PT_InGame_SKDAVFVFS, type, pEActionParamVFVFS.vec1, pEActionParamVFVFS.f1, pEActionParamVFVFS.vec2, pEActionParamVFVFS.f2, pEActionParamVFVFS.str);
	}

	private void SendSKDATalk(PEActionType type, PEActionParam obj)
	{
		PEActionParamS pEActionParamS = obj as PEActionParamS;
		RPCServer(EPacketType.PT_InGame_SKDAS, type, pEActionParamS.str);
	}

	private void SendSKDAAlienDeath(PEActionType type, PEActionParam obj)
	{
		RPCServer(EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDARide(PEActionType type, PEActionParam obj)
	{
		PEActionParamVQSNS pEActionParamVQSNS = obj as PEActionParamVQSNS;
		RPCServer(EPacketType.PT_InGame_SKDAVQSNS, type, pEActionParamVQSNS.vec, pEActionParamVQSNS.q, pEActionParamVQSNS.strAnima, pEActionParamVQSNS.enitytID, pEActionParamVQSNS.boneStr);
	}

	protected void RPC_SKSyncAttr(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int type = stream.Read<byte>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (runner != null && runner.SkEntityBase != null)
		{
			float attribute = runner.SkEntityBase.GetAttribute(type, !flag);
			runner.SkEntityBase._attribs.FromNet = true;
			runner.SkEntityBase.SetAttribute(type, num, eventOff: false, flag, num2);
			runner.SkEntityBase._attribs.FromNet = false;
			if (!flag)
			{
				OnAttrChange((AttribType)type, runner.SkEntityBase.GetAttribute(type), num2, num - attribute);
			}
		}
	}

	protected void RPC_SKStartSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth)
		{
			return;
		}
		int id = stream.Read<int>(new object[0]);
		int id2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ISkParaNet para = null;
		if (flag)
		{
			float[] data = stream.Read<float[]>(new object[0]);
			para = SkParaNet.FromFloatArray(data);
		}
		if (runner == null)
		{
			return;
		}
		if (runner.SkEntityPE != null)
		{
			MotionMgrCmpt cmpt = runner.SkEntityPE.Entity.GetCmpt<MotionMgrCmpt>();
			if (null != cmpt)
			{
				cmpt.GetAction<Action_HandChangeEquipHold>()?.ChangeHoldState(holdEquip: true);
			}
			CommonInterface comByNetID = CommonInterface.GetComByNetID(id2);
			if (comByNetID != null)
			{
				runner.SkEntityBase.StartSkill(comByNetID.SkEntityBase, id, para);
			}
			else
			{
				runner.SkEntityBase.StartSkill(null, id, para);
			}
		}
		else if (runner.SkEntityBase != null)
		{
			CommonInterface comByNetID2 = CommonInterface.GetComByNetID(id2);
			if (comByNetID2 != null)
			{
				runner.SkEntityBase.StartSkill(comByNetID2.SkEntityBase, id, para);
			}
			else
			{
				runner.SkEntityBase.StartSkill(null, id, para);
			}
		}
	}

	protected void RPC_SKStopSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (runner != null && runner.SkEntityBase != null)
		{
			runner.SkEntityBase.CancelSkillById(id);
		}
	}

	protected void RPC_SKBLoop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		bool bLoop = stream.Read<bool>(new object[0]);
		if (!(runner != null) || !(runner.SkEntityPE != null))
		{
			return;
		}
		if (runner.SkEntityBase.GetSkInst(num) != null)
		{
			runner.SkEntityBase.SetBLoop(bLoop, num);
			return;
		}
		CommonInterface comByNetID = CommonInterface.GetComByNetID(id);
		if (comByNetID != null)
		{
			runner.SkEntityBase.StartSkill(comByNetID.SkEntityBase, runner.SkEntityBase.GetId());
		}
		else
		{
			runner.SkEntityBase.StartSkill(null, runner.SkEntityBase.GetId());
		}
	}

	protected void RPC_SKSyncInitAttrs(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>(new object[0]);
		if (runner != null && runner.SkEntityBase != null)
		{
			runner.SkEntityBase._attribs.FromNet = true;
			runner.SkEntityBase.Import(data);
			runner.SkEntityBase._attribs.FromNet = false;
		}
		else
		{
			Debug.LogError("RPC_SKSyncInitAttrs runner or SkEntityPE is null");
		}
		if (base.Runner != null && base.Runner.SkEntityBase != null)
		{
			base.Runner.SkEntityBase._attribs.LockModifyBySingle = false;
		}
		if (this.OnSkAttrInitEvent != null)
		{
			this.OnSkAttrInitEvent();
		}
	}

	protected void RPC_SKFellTree(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		stream.Read<float>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		TreeInfo treeInfo = null;
		if (null != LSubTerrainMgr.Instance)
		{
			int x_ = Mathf.FloorToInt(pos.x);
			int y_ = Mathf.FloorToInt(pos.y);
			int z_ = Mathf.FloorToInt(pos.z);
			List<GlobalTreeInfo> list = LSubTerrainMgr.Picking(new IntVector3(x_, y_, z_), includeTrees: true);
			if (list.Count <= 0)
			{
				return;
			}
			{
				foreach (GlobalTreeInfo item in list)
				{
					if (num <= 0f)
					{
						PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
						SkEntity skEntity = ((!(null == peEntity)) ? peEntity.skEntity : null);
						SkEntitySubTerrain.Instance.OnTreeCutDown(skEntity, item);
						DigTerrainManager.RemoveTree(item);
						if (base.IsOwner)
						{
							SkEntitySubTerrain.Instance.SetTreeHp(item.WorldPos, num);
						}
					}
					else if (base.IsOwner)
					{
						SkEntitySubTerrain.Instance.SetTreeHp(item.WorldPos, num);
					}
				}
				return;
			}
		}
		if (!(null != RSubTerrainMgr.Instance))
		{
			return;
		}
		RSubTerrSL.AddDeletedTree(pos);
		treeInfo = RSubTerrainMgr.TreesAtPosF(pos);
		if (treeInfo == null)
		{
			return;
		}
		GlobalTreeInfo globalTreeInfo = new GlobalTreeInfo(-1, treeInfo);
		if (num <= 0f)
		{
			PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(base.Id);
			SkEntity skEntity2 = ((!(null == peEntity2)) ? peEntity2.skEntity : null);
			SkEntitySubTerrain.Instance.OnTreeCutDown(skEntity2, globalTreeInfo);
			DigTerrainManager.RemoveTree(globalTreeInfo);
			if (base.IsOwner)
			{
				SkEntitySubTerrain.Instance.SetTreeHp(globalTreeInfo.WorldPos, num);
			}
		}
		else if (base.IsOwner)
		{
			SkEntitySubTerrain.Instance.SetTreeHp(globalTreeInfo.WorldPos, num);
		}
	}

	private void RPC_SKDAVVF(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVVF param = PEActionParamVVF.param;
		param.vec1 = stream.Read<Vector3>(new object[0]);
		param.vec2 = stream.Read<Vector3>(new object[0]);
		param.f = stream.Read<float>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVFNS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVFNS param = PEActionParamVFNS.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		param.f = stream.Read<float>(new object[0]);
		param.n = stream.Read<int>(new object[0]);
		param.str = stream.Read<string>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDANO(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type);
		}
	}

	private void RPC_SKDAV(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamV param = PEActionParamV.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVVN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVVN param = PEActionParamVVN.param;
		param.vec1 = stream.Read<Vector3>(new object[0]);
		param.vec2 = stream.Read<Vector3>(new object[0]);
		param.n = stream.Read<int>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVQNS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVQNS param = PEActionParamVQNS.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		param.q = stream.Read<Quaternion>(new object[0]);
		param.n = stream.Read<int>(new object[0]);
		param.str = stream.Read<string>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVQ(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVQ param = PEActionParamVQ.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		param.q = stream.Read<Quaternion>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamS param = PEActionParamS.param;
		param.str = stream.Read<string>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVQS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVQS param = PEActionParamVQS.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		param.q = stream.Read<Quaternion>(new object[0]);
		param.str = stream.Read<string>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVQSN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVQSN param = PEActionParamVQSN.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		param.q = stream.Read<Quaternion>(new object[0]);
		param.str = stream.Read<string>(new object[0]);
		param.n = stream.Read<int>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVVNN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVVNN param = PEActionParamVVNN.param;
		param.vec1 = stream.Read<Vector3>(new object[0]);
		param.vec2 = stream.Read<Vector3>(new object[0]);
		param.n1 = stream.Read<int>(new object[0]);
		param.n2 = stream.Read<int>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamN param = PEActionParamN.param;
		param.n = stream.Read<int>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamB param = PEActionParamB.param;
		param.b = stream.Read<bool>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVQN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVQN param = PEActionParamVQN.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		param.q = stream.Read<Quaternion>(new object[0]);
		param.n = stream.Read<int>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAVFVFS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVFVFS param = PEActionParamVFVFS.param;
		param.vec1 = stream.Read<Vector3>(new object[0]);
		param.f1 = stream.Read<float>(new object[0]);
		param.vec2 = stream.Read<Vector3>(new object[0]);
		param.f2 = stream.Read<float>(new object[0]);
		param.str = stream.Read<string>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKDAEndAction(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.EndAction(type);
		}
	}

	private void RPC_SKDAEndImmediately(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		if (MtCmpt != null)
		{
			MtCmpt.EndImmediately(type);
		}
	}

	private void RPC_SKDAQueryEntityState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		PEActionType pEActionType = PEActionType.None;
		if (!(MtCmpt != null))
		{
			return;
		}
		if ((num & 1) != 0)
		{
			pEActionType = PEActionType.Death;
		}
		else if ((num & 2) != 0)
		{
			pEActionType = PEActionType.EquipmentHold;
		}
		else if ((num & 4) != 0)
		{
			pEActionType = PEActionType.HoldShield;
		}
		else if ((num & 8) != 0)
		{
			pEActionType = PEActionType.GunHold;
		}
		else if ((num & 0x10) != 0)
		{
			pEActionType = PEActionType.BowHold;
		}
		else if ((num & 0x20) != 0)
		{
			pEActionType = PEActionType.AimEquipHold;
		}
		else if ((num & 0x40) != 0)
		{
			pEActionType = PEActionType.HoldFlashLight;
		}
		else if ((num & 0x80) != 0)
		{
			pEActionType = PEActionType.TwoHandSwordHold;
		}
		else
		{
			if ((num & 0x100) != 0)
			{
				pEActionType = PEActionType.Sit;
				PEActionParamVQSN param = PEActionParamVQSN.param;
				param.vec = base.transform.position;
				param.q = base.transform.rotation;
				param.n = 0;
				param.str = "SitOnChair";
				MtCmpt.DoActionImmediately(PEActionType.Sit, param);
				return;
			}
			if ((num & 0x200) != 0)
			{
				pEActionType = PEActionType.Sleep;
				PEActionParamVQNS param2 = PEActionParamVQNS.param;
				param2.vec = base.transform.position;
				param2.q = base.transform.rotation;
				param2.n = 0;
				param2.str = "Sleep";
				MtCmpt.DoActionImmediately(PEActionType.Sleep, param2);
				return;
			}
			if ((num & 0x400) != 0)
			{
				pEActionType = PEActionType.Cure;
			}
			else if ((num & 0x800) != 0)
			{
				pEActionType = PEActionType.Operation;
			}
		}
		if (pEActionType != 0)
		{
			MtCmpt.DoActionImmediately(pEActionType);
		}
	}

	private void RPC_SKDAVQSNS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType type = stream.Read<PEActionType>(new object[0]);
		PEActionParamVQSNS param = PEActionParamVQSNS.param;
		param.vec = stream.Read<Vector3>(new object[0]);
		param.q = stream.Read<Quaternion>(new object[0]);
		param.strAnima = stream.Read<string>(new object[0]);
		param.enitytID = stream.Read<int>(new object[0]);
		param.boneStr = stream.Read<string>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null != peEntity && peEntity.hasView && MtCmpt != null)
		{
			MtCmpt.DoActionImmediately(type, param);
		}
	}

	private void RPC_SKIKPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 aimTargetPos = stream.Read<Vector3>(new object[0]);
		if (IkCmpt != null)
		{
			IkCmpt.aimTargetPos = aimTargetPos;
		}
	}

	protected void RPC_S2C_AbnormalConditionStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth)
		{
			return;
		}
		int entityId = stream.Read<int>(new object[0]);
		int type = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (null == peEntity)
		{
			return;
		}
		AbnormalConditionCmpt cmpt = peEntity.GetCmpt<AbnormalConditionCmpt>();
		if (!(null == cmpt))
		{
			if (num == 1)
			{
				byte[] data = stream.Read<byte[]>(new object[0]);
				cmpt.NetApplyState((PEAbnormalType)type, data);
			}
			else
			{
				cmpt.NetApplyState((PEAbnormalType)type, null);
			}
		}
	}

	protected void RPC_S2C_AbnormalConditionEnd(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.hasOwnerAuth)
		{
			return;
		}
		int entityId = stream.Read<int>(new object[0]);
		int type = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (!(null == peEntity))
		{
			AbnormalConditionCmpt cmpt = peEntity.GetCmpt<AbnormalConditionCmpt>();
			if (!(null == cmpt))
			{
				cmpt.NetEndState((PEAbnormalType)type);
			}
		}
	}

	protected void RPC_S2C_AbnormalCondition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null == peEntity)
		{
			return;
		}
		AbnormalConditionCmpt acc = peEntity.GetCmpt<AbnormalConditionCmpt>();
		if (null == acc)
		{
			return;
		}
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			BufferHelper.ReadInt32(r);
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				int type = BufferHelper.ReadInt32(r);
				byte[] data = BufferHelper.ReadBytes(r);
				acc.NetApplyState((PEAbnormalType)type, data);
			}
		});
	}

	protected void RPC_S2C_Jump(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth)
		{
			double time = stream.Read<double>(new object[0]);
			if (_move != null && _move is Motion_Move_Human)
			{
				(_move as Motion_Move_Human).NetJump(time);
			}
		}
	}

	protected void RPC_S2C_SkOnDamage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		float hpChange = stream.Read<float>(new object[0]);
		NetworkInterface networkInterface = NetworkInterface.Get<NetworkInterface>(id);
		SkEntity caster = ((!(null != networkInterface)) ? null : ((!(null != networkInterface.Runner)) ? null : networkInterface.Runner.SkEntityBase));
		if (base.Runner != null)
		{
			PESkEntity pESkEntity = base.Runner.SkEntityBase as PESkEntity;
			if (pESkEntity != null)
			{
				pESkEntity.DispatchHPChangeEvent(caster, hpChange);
			}
		}
	}

	protected virtual void RPC_S2C_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void RPC_S2C_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void CheckAuthority()
	{
		if (null == PlayerNetwork.mainPlayer)
		{
			return;
		}
		if (Mathf.Abs(PlayerNetwork.mainPlayer._pos.x - base._pos.x) <= 128f && Mathf.Abs(PlayerNetwork.mainPlayer._pos.y - base._pos.y) <= 128f && Mathf.Abs(PlayerNetwork.mainPlayer._pos.z - base._pos.z) <= 128f)
		{
			base.canGetAuth = true;
			if (!base.hasAuth && lastRequestTime < Time.time)
			{
				RPCServer(EPacketType.PT_InGame_SetController);
				lastRequestTime = Time.time + 3f;
			}
			else
			{
				lastRequestTime = 0f;
			}
			return;
		}
		base.canGetAuth = false;
		lastRequestTime = 0f;
		if (base.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_LostController);
			base.authId = -1;
			ResetContorller();
		}
	}

	protected virtual IEnumerator AuthorityCheckCoroutine()
	{
		base.authId = -1;
		lastRequestTime = 0f;
		while (true)
		{
			CheckAuthority();
			yield return null;
		}
	}

	protected virtual void ResetContorller()
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null != peEntity)
		{
			NetCmpt cmpt = peEntity.GetCmpt<NetCmpt>();
			if (null != cmpt)
			{
				cmpt.SetController(base.hasOwnerAuth);
			}
		}
		if (base.hasOwnerAuth)
		{
			base.enabled = false;
		}
		else
		{
			base.enabled = true;
		}
		InitForceData();
	}
}
