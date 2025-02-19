using System.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class RequestCmpt : PeCmpt, IPeMsg
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	private ViewCmpt m_View;

	private Request[] m_Request = new Request[15];

	private NetworkInterface _net;

	public NetworkInterface Net
	{
		get
		{
			if (base.Entity != null && PeGameMgr.IsMulti && _net == null)
			{
				_net = NetworkInterface.Get(base.Entity.Id);
			}
			return _net;
		}
	}

	private Request Create(EReqType type)
	{
		return type switch
		{
			EReqType.Idle => new RQIdle(), 
			EReqType.Animation => new RQAnimation(), 
			EReqType.MoveToPoint => new RQMoveToPoint(), 
			EReqType.TalkMove => new RQTalkMove(), 
			EReqType.FollowPath => new RQFollowPath(), 
			EReqType.FollowTarget => new RQFollowTarget(), 
			EReqType.Salvation => new RQSalvation(), 
			EReqType.Dialogue => new RQDialogue(), 
			EReqType.Translate => new RQTranslate(), 
			EReqType.Rotate => new RQRotate(), 
			EReqType.Attack => new RQAttack(), 
			EReqType.PauseAll => new RQPauseAll(), 
			EReqType.UseSkill => new RQUseSkill(), 
			EReqType.MAX => null, 
			_ => null, 
		};
	}

	public Request Register(EReqType type, params object[] objs)
	{
		Request request = null;
		switch (type)
		{
		case EReqType.Dialogue:
			request = new RQDialogue(objs);
			break;
		case EReqType.Idle:
			request = new RQIdle(objs);
			break;
		case EReqType.Animation:
			request = new RQAnimation(objs);
			break;
		case EReqType.MoveToPoint:
			request = new RQMoveToPoint(objs);
			break;
		case EReqType.TalkMove:
			request = new RQTalkMove(objs);
			break;
		case EReqType.FollowPath:
			request = new RQFollowPath(objs);
			break;
		case EReqType.FollowTarget:
			request = new RQFollowTarget(objs);
			break;
		case EReqType.Salvation:
			request = new RQSalvation(objs);
			break;
		case EReqType.Translate:
			request = new RQTranslate(objs);
			break;
		case EReqType.Rotate:
			request = new RQRotate(objs);
			break;
		case EReqType.Attack:
			request = new RQAttack();
			break;
		case EReqType.PauseAll:
			request = new RQPauseAll();
			break;
		case EReqType.UseSkill:
			request = new RQUseSkill();
			break;
		}
		if (!CalculateRelation(request))
		{
			return null;
		}
		AddRequest(request);
		return request;
	}

	public override void Start()
	{
		base.Start();
		m_View = GetComponent<ViewCmpt>();
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(0);
		w.Write(m_Request.Length);
		for (int i = 0; i < m_Request.Length; i++)
		{
			if (m_Request[i] == null)
			{
				w.Write(-1);
				continue;
			}
			w.Write((int)m_Request[i].Type);
			m_Request[i].Serialize(w);
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		r.ReadInt32();
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = r.ReadInt32();
			if (num2 != -1)
			{
				Request request = Create((EReqType)num2);
				request.Deserialize(r);
				if (request.Type != EReqType.Dialogue)
				{
					AddRequest(request);
				}
			}
		}
	}

	public Request GetRequest(EReqType type)
	{
		return m_Request[(int)type];
	}

	public void AddRequest(Request request)
	{
		if (m_Request[(int)request.Type] == null)
		{
			m_Request[(int)request.Type] = request;
		}
		if (PeGameMgr.IsSingle && base.Entity.BehaveCmpt != null && HasRequest())
		{
			base.Entity.BehaveCmpt.Excute();
		}
	}

	public void RemoveRequest(EReqType type)
	{
		if (type == EReqType.MoveToPoint && StroyManager.Instance != null && m_Request[(int)type] is RQMoveToPoint rQMoveToPoint)
		{
			bool trigger = PEUtil.MagnitudeH(base.Entity.position, rQMoveToPoint.position) < rQMoveToPoint.stopRadius;
			StroyManager.Instance.EntityReach(base.Entity, trigger);
		}
		if (PeGameMgr.IsMulti && base.Entity != null && base.Entity.netCmpt != null && type == EReqType.FollowPath && base.Entity.netCmpt.network.hasOwnerAuth)
		{
			base.Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, 14, 3, (m_Request[(int)type] as RQFollowPath).VerifPos);
		}
		if (PeGameMgr.IsMulti && base.Entity != null && base.Entity.netCmpt != null && type == EReqType.FollowTarget && !PeGameMgr.IsStory && base.Entity.netCmpt.network.hasOwnerAuth)
		{
			base.Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, 14, 4);
		}
		m_Request[(int)type] = null;
		if (PeGameMgr.IsSingle && base.Entity.BehaveCmpt != null && m_View != null && !m_View.hasView && !HasRequest())
		{
			base.Entity.BehaveCmpt.Stopbehave();
		}
	}

	public void RemoveRequest(Request request)
	{
		if (request != null)
		{
			if (request.Type == EReqType.MoveToPoint && StroyManager.Instance != null && request is RQMoveToPoint rQMoveToPoint)
			{
				bool trigger = PEUtil.MagnitudeH(base.Entity.position, rQMoveToPoint.position) < rQMoveToPoint.stopRadius;
				StroyManager.Instance.EntityReach(base.Entity, trigger);
			}
			if (m_Request[(int)request.Type] != null && m_Request[(int)request.Type].Equals(request))
			{
				m_Request[(int)request.Type] = null;
			}
			if (PeGameMgr.IsSingle && base.Entity.BehaveCmpt != null && m_View != null && !m_View.hasView && !HasRequest())
			{
				base.Entity.BehaveCmpt.Stopbehave();
			}
			if (PeGameMgr.IsMulti && base.Entity != null && base.Entity.netCmpt != null && request.Type == EReqType.FollowPath && base.Entity.netCmpt.network.hasOwnerAuth)
			{
				base.Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, 14, 3, (request as RQFollowPath).VerifPos);
			}
			if (PeGameMgr.IsMulti && base.Entity != null && base.Entity.netCmpt != null && request.Type == EReqType.FollowTarget && !PeGameMgr.IsStory && base.Entity.netCmpt.network.hasOwnerAuth)
			{
				base.Entity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp, 14, 4);
			}
		}
	}

	public bool Contains(EReqType type)
	{
		return m_Request[(int)type] != null;
	}

	public bool HasRequest()
	{
		int num = 15;
		for (int i = 0; i < num; i++)
		{
			if (m_Request[i] != null && m_Request[i].CanRun())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyRequest()
	{
		int num = 15;
		for (int i = 0; i < num; i++)
		{
			if (m_Request[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public int GetFollowID()
	{
		if (m_Request[4] != null)
		{
			return (m_Request[4] as RQFollowTarget).id;
		}
		return -1;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		int num = 15;
		for (int i = 0; i < num; i++)
		{
			if (m_Request[i] == null)
			{
				continue;
			}
			m_Request[i].RemoveMask(EReqMask.Blocking);
			for (int j = 0; j < num; j++)
			{
				if (m_Request[i] != null && m_Request[j] != null)
				{
					EReqRelation relation = m_Request[i].GetRelation(m_Request[j]);
					if (relation == EReqRelation.Block)
					{
						m_Request[j].Addmask(EReqMask.Blocking);
					}
					if (relation == EReqRelation.Blocked)
					{
						m_Request[i].Addmask(EReqMask.Blocking);
					}
				}
			}
		}
	}

	private bool CalculateRelation(Request request)
	{
		if (request == null)
		{
			return false;
		}
		int num = 15;
		for (int i = 0; i < num; i++)
		{
			if (m_Request[i] != null && request.GetRelation(m_Request[i]) == EReqRelation.Deleted)
			{
				return false;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (m_Request[j] != null && request.GetRelation(m_Request[j]) == EReqRelation.Delete)
			{
				RemoveRequest(m_Request[j]);
			}
		}
		return true;
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		if (msg == EMsg.Net_Proxy)
		{
		}
	}

	public void RequsetProtect()
	{
		if (Contains(EReqType.MoveToPoint) && base.Entity != null && base.Entity.NpcCmpt != null && GetRequest(EReqType.MoveToPoint) is RQMoveToPoint { position: var position })
		{
			StroyManager.Instance.EntityReach(base.Entity, trigger: true);
			base.Entity.NpcCmpt.Req_Remove(EReqType.MoveToPoint);
			base.Entity.NpcCmpt.Req_Translate(position);
		}
		if (Contains(EReqType.TalkMove) && base.Entity != null && base.Entity.NpcCmpt != null && GetRequest(EReqType.TalkMove) is RQTalkMove { position: var position2 })
		{
			base.Entity.NpcCmpt.Req_Remove(EReqType.TalkMove);
			base.Entity.NpcCmpt.Req_Translate(position2);
		}
		if (Contains(EReqType.FollowTarget) && base.Entity != null && base.Entity.NpcCmpt != null && GetRequest(EReqType.FollowTarget) is RQFollowTarget rQFollowTarget)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(rQFollowTarget.id);
			if (peEntity != null)
			{
				Vector3 position3 = peEntity.position;
				base.Entity.NpcCmpt.Req_Remove(EReqType.FollowTarget);
				base.Entity.NpcCmpt.Req_Translate(position3, adjust: true, lostController: false);
				base.Entity.NpcCmpt.Req_FollowTarget(peEntity.Id, rQFollowTarget.targetPos, rQFollowTarget.dirTargetID, rQFollowTarget.tgtRadiu);
			}
		}
	}

	public Request Req_UseSkill()
	{
		if (Net != null && !Net.hasOwnerAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 11);
		}
		return Register(EReqType.UseSkill);
	}

	public Request Req_Translate(Vector3 position)
	{
		if (Net != null && !Net.hasAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 7, position);
		}
		return Register(EReqType.Translate, position);
	}

	public Request Req_PlayAnimation(string name, float time)
	{
		if (Net != null && !Net.hasOwnerAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 1, name, time);
		}
		return Register(EReqType.Animation, name, time);
	}

	public Request Req_MoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
	{
		if (Net != null && !Net.hasOwnerAuth && base.Entity != null && base.Entity.viewCmpt != null && base.Entity.viewCmpt.hasView)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 2, position, stopRadius, isForce, (int)state);
		}
		else if (Net != null && !Net.hasAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 2, position, stopRadius, isForce, (int)state);
		}
		return Register(EReqType.MoveToPoint, position, stopRadius, isForce, state);
	}

	public Request Req_TalkMoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
	{
		if (Net != null && !Net.hasOwnerAuth && base.Entity != null && base.Entity.viewCmpt != null && base.Entity.viewCmpt.hasView)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 12, position, stopRadius, isForce, (int)state);
		}
		else if (Net != null && !Net.hasAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 12, position, stopRadius, isForce, (int)state);
		}
		return Register(EReqType.TalkMove, position, stopRadius, isForce, state);
	}

	public Request Req_FollowTarget(int targetId, bool bNet = false, bool lostController = true)
	{
		if (Net != null && !bNet)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 4, targetId);
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(targetId);
		if (peEntity == null || Contains(EReqType.FollowTarget))
		{
			return null;
		}
		return Register(EReqType.FollowTarget, targetId);
	}

	public Request Req_FollowPath(Vector3[] path, bool isLoop, SpeedState state = SpeedState.Run)
	{
		if (Net != null && !Net.hasOwnerAuth && base.Entity != null && base.Entity.viewCmpt != null && base.Entity.viewCmpt.hasView)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 3, path, isLoop, state);
		}
		return Register(EReqType.FollowPath, path, isLoop, state);
	}

	public Request Req_Rotation(Quaternion rotation)
	{
		return Register(EReqType.Rotate, rotation);
	}

	public Request Req_SetIdle(string name)
	{
		return Register(EReqType.Idle, name);
	}

	public Request Req_Dialogue(params object[] objs)
	{
		return Register(EReqType.Dialogue, objs);
	}

	public Request Req_PauseAll()
	{
		return Register(EReqType.PauseAll);
	}
}
