using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRFollowPath), "RFollowPath")]
public class BRFollowPath : BTNormal
{
	private int m_Index;

	private float hideStarTime;

	private float HIDE_TIME = 1f;

	private FindHidePos mfind;

	private Vector3 mdir;

	private int GetClosetPointIndex(RQFollowPath request, Vector3 position)
	{
		int result = -1;
		float num = float.PositiveInfinity;
		for (int i = 0; i < request.path.Length; i++)
		{
			float num2 = PEUtil.SqrMagnitudeH(request.path[i], position);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	private int GetBetterIndex(RQFollowPath request, Vector3 _position)
	{
		int num = 0;
		float num2 = PEUtil.Magnitude(request.path[num], _position);
		for (int i = num + 1; i < request.path.Length; i++)
		{
			float num3 = PEUtil.Magnitude(request.path[i], _position);
			if (num2 >= num3)
			{
				num = i;
				num2 = num3;
			}
		}
		if (num >= request.path.Length - 1)
		{
			return num;
		}
		int num4 = num + 1;
		float num5 = PEUtil.Magnitude(request.path[num], request.path[num4]);
		float num6 = PEUtil.Magnitude(request.path[num], _position);
		float num7 = PEUtil.Magnitude(request.path[num4], _position);
		float num8 = PEUtil.SqrMagnitudeH(request.path[num], _position);
		if (num8 < 0.0625f)
		{
			return num4;
		}
		if (num5 < num6 || num5 < num7)
		{
			return num;
		}
		return num4;
	}

	private BehaveResult Init(Tree sender)
	{
		if (base.attackEnemy != null)
		{
			return BehaveResult.Failure;
		}
		if (!(GetRequest(EReqType.FollowPath) is RQFollowPath rQFollowPath) || !rQFollowPath.CanRun())
		{
			return BehaveResult.Failure;
		}
		m_Index = GetClosetPointIndex(rQFollowPath, base.position);
		if (m_Index < 0 || m_Index >= rQFollowPath.path.Length)
		{
			return BehaveResult.Failure;
		}
		mfind = new FindHidePos(8f, needHide: false);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.RFollowPath);
		if (!(GetRequest(EReqType.FollowPath) is RQFollowPath rQFollowPath) || !rQFollowPath.CanRun())
		{
			return BehaveResult.Failure;
		}
		if (m_Index < 0 || m_Index >= rQFollowPath.path.Length)
		{
			return BehaveResult.Failure;
		}
		bool flag = NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Recourse);
		if (flag && base.entity.target != null)
		{
			base.entity.target.SetEnityCanAttack(canAttackOrNot: false);
		}
		if (flag && base.entity.NpcCmpt.HasEnemyLocked())
		{
			if (Time.time - hideStarTime > HIDE_TIME)
			{
				mdir = mfind.GetHideDir(PeSingleton<PeCreature>.Instance.mainPlayer.position, base.position, base.Enemies);
				hideStarTime = Time.time;
			}
			Vector3 pos = base.position + mdir.normalized * 10f;
			if (mfind.bNeedHide)
			{
				MoveToPosition(pos, SpeedState.Run);
			}
			else
			{
				StopMove();
				FaceDirection(PeSingleton<PeCreature>.Instance.mainPlayer.position - base.position);
			}
			if (NpcEatDb.IsContinueEat(base.entity, out var item) && base.entity.UseItem.GetCdByItemProtoId(item.protoId) < float.Epsilon)
			{
				UseItem(item);
			}
			return BehaveResult.Running;
		}
		if (base.AskStop)
		{
			StopMove();
			if (null != StroyManager.Instance)
			{
				FaceDirection(StroyManager.Instance.GetPlayerPos() - base.position);
			}
			return BehaveResult.Running;
		}
		m_Index = GetBetterIndex(rQFollowPath, base.position);
		if (m_Index == rQFollowPath.path.Length - 1 && IsReached(rQFollowPath.path[m_Index], base.position, Is3D: true, 2f))
		{
			if (!rQFollowPath.isLoop)
			{
				RemoveRequest(rQFollowPath);
				if (NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Recourse) && base.entity.target != null)
				{
					base.entity.target.SetEnityCanAttack(canAttackOrNot: true);
				}
				return BehaveResult.Success;
			}
			m_Index = 0;
		}
		if (Stucking(5f))
		{
			SetPosition(rQFollowPath.path[m_Index]);
		}
		MoveToPosition(rQFollowPath.path[m_Index], rQFollowPath.speedState);
		return BehaveResult.Running;
	}
}
