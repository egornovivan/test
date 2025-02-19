using ItemAsset;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcFollowerCut), "NpcFollowerCut")]
public class BTNpcFollowerCut : BTNormal
{
	private class Data
	{
		public float mStartCutTime;

		public bool HasActive;

		public Action_Fell mActionFell;
	}

	private Data m_Data = new Data();

	private GlobalTreeInfo treeInfo;

	private Vector3 cutPos;

	private float endCutWaitStartTime;

	private float END_TIME = 3f;

	private Vector3 GetStandPos(GlobalTreeInfo _GlobaltreeInfo, PeEntity player, PeEntity Npc)
	{
		if (_GlobaltreeInfo._treeInfo == null)
		{
			return Vector3.zero;
		}
		if (_GlobaltreeInfo.HasCreatPickPos)
		{
			if (_GlobaltreeInfo.AddCutter(Npc.Id, out var result))
			{
				return result;
			}
			return Vector3.zero;
		}
		GameObject gameObject = null;
		if (null != RSubTerrainMgr.Instance)
		{
			if (RSubTerrainMgr.HasCollider(_GlobaltreeInfo._treeInfo.m_protoTypeIdx))
			{
				gameObject = RSubTerrainMgr.Instance.GlobalPrototypeColliders[_GlobaltreeInfo._treeInfo.m_protoTypeIdx];
			}
		}
		else if (null != LSubTerrainMgr.Instance && LSubTerrainMgr.HasCollider(_GlobaltreeInfo._treeInfo.m_protoTypeIdx))
		{
			gameObject = LSubTerrainMgr.Instance.GlobalPrototypeColliders[_GlobaltreeInfo._treeInfo.m_protoTypeIdx];
		}
		if (gameObject == null)
		{
			return Vector3.zero;
		}
		CapsuleCollider component = gameObject.GetComponent<CapsuleCollider>();
		if (component == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = new Vector3(_GlobaltreeInfo._treeInfo.m_widthScale, _GlobaltreeInfo._treeInfo.m_heightScale, _GlobaltreeInfo._treeInfo.m_widthScale);
		Vector3 vector2 = new Vector3(component.center.x * vector.x, component.center.y * vector.y, component.center.z * vector.z);
		_GlobaltreeInfo.TreeCapCenterPos = new Vector3(_GlobaltreeInfo.WorldPos.x + vector2.x, _GlobaltreeInfo.WorldPos.y, _GlobaltreeInfo.WorldPos.z + vector2.z);
		Vector3 normalized = (player.peTrans.position - _GlobaltreeInfo.TreeCapCenterPos).normalized;
		if (_GlobaltreeInfo.CreatCutPos(_GlobaltreeInfo.TreeCapCenterPos, normalized, component.radius))
		{
			if (_GlobaltreeInfo.AddCutter(Npc.Id, out var result2))
			{
				return result2;
			}
			return Vector3.zero;
		}
		return Vector3.zero;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpc || !base.IsNpcFollower)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcFollowerCut)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsTool(base.entity, EeqSelect.tool))
		{
			SelectItem.EquipByObj(base.entity, base.entity.NpcCmpt.EqSelect.GetBetterToolObj());
		}
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsEnergy(base.entity, EeqSelect.energy))
		{
			SelectItem.EquipByObj(base.entity, base.entity.NpcCmpt.EqSelect.GetBetterEnergyObj());
		}
		if (base.entity.motionEquipment == null || base.entity.motionEquipment.axe == null)
		{
			EndFollowerCut();
			return BehaveResult.Failure;
		}
		if (base.entity.motionEquipment.axe is PEChainSaw && GetAttribute(AttribType.Energy) < float.Epsilon)
		{
			EndFollowerCut();
			return BehaveResult.Failure;
		}
		treeInfo = base.NpcMaster.peEntity.aliveEntity.treeInfo;
		if (treeInfo == null)
		{
			EndFollowerCut();
			return BehaveResult.Failure;
		}
		m_Data.mActionFell = SetGlobalTreeInfo(treeInfo);
		if (m_Data.mActionFell == null || !m_Data.mActionFell.CanDoAction())
		{
			EndFollowerCut();
			return BehaveResult.Failure;
		}
		cutPos = GetStandPos(treeInfo, base.NpcMaster.peEntity, base.entity);
		if (cutPos == Vector3.zero)
		{
			EndFollowerCut();
			return BehaveResult.Failure;
		}
		m_Data.mStartCutTime = Time.time;
		m_Data.HasActive = false;
		base.entity.NpcCmpt.AddTalkInfo(ENpcTalkType.Follower_cut, ENpcSpeakType.TopHead, canLoop: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (SkEntitySubTerrain.Instance.GetTreeHP(m_Data.mActionFell.treeInfo.WorldPos) <= 0f || !Enemy.IsNullOrInvalid(base.attackEnemy) || base.entity.motionEquipment.axe == null || base.entity.NpcCmpt.servantCallback || (base.entity.motionEquipment.axe != null && base.entity.motionEquipment.axe is PEChainSaw && base.entity.GetAttribute(AttribType.Energy) < float.Epsilon))
		{
			if (endCutWaitStartTime == 0f)
			{
				endCutWaitStartTime = Time.time;
			}
			if (Time.time - endCutWaitStartTime >= END_TIME)
			{
				endCutWaitStartTime = 0f;
				if (base.entity.motionEquipment.axe != null && base.entity.motionEquipment.axe is PEChainSaw)
				{
					SelectItem.TakeOffEquip(base.entity);
				}
				return BehaveResult.Failure;
			}
			EndAction(PEActionType.Fell);
			EndFollowerCut();
			ActiveWeapon(base.entity.motionEquipment.axe, active: false);
			return BehaveResult.Running;
		}
		if (m_Data.HasActive)
		{
			FaceDirection(treeInfo.TreeCapCenterPos - base.position);
			DoAction(PEActionType.Fell);
		}
		Debug.DrawRay(treeInfo.TreeCapCenterPos, Vector3.up * 10f, Color.red);
		Debug.DrawRay(cutPos, Vector3.up * 10f, Color.blue);
		Debug.DrawRay(treeInfo.WorldPos, Vector3.up * 10f, Color.yellow);
		if (IsReached(base.position, cutPos) && !m_Data.HasActive)
		{
			StopMove();
			ActiveWeapon(base.entity.motionEquipment.axe, active: true);
			FaceDirection(treeInfo.TreeCapCenterPos - base.position);
			DoAction(PEActionType.Fell);
			m_Data.HasActive = true;
		}
		else
		{
			if (Stucking())
			{
				StopMove();
				FaceDirection(treeInfo.TreeCapCenterPos - base.position);
				ActiveWeapon(base.entity.motionEquipment.axe, active: true);
				m_Data.HasActive = true;
			}
			if (!m_Data.HasActive)
			{
				MoveToPosition(cutPos, SpeedState.Run, avoid: false);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
	}
}
