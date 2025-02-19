using Pathea;
using PatheaScript;
using UnityEngine;

namespace PatheaScriptExt;

public class ActionMove : Action
{
	private Motion_Move mMove;

	private Vector3 mLocalDst;

	private float mRadius;

	private float mSpeed;

	public override bool Parse()
	{
		if (mInfo.Name != "STMT")
		{
			UnityEngine.Debug.LogError("error node:" + mInfo);
			return false;
		}
		VarRef npcId = PeType.GetNpcId(mInfo, mTrigger);
		int num = (int)npcId.Value;
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(num);
		if (null == peEntity)
		{
			UnityEngine.Debug.LogError("can't find entity:" + num);
			return false;
		}
		mMove = peEntity.GetCmpt<Motion_Move>();
		if (null == mMove)
		{
			UnityEngine.Debug.LogError("can't find move cmpt:");
			return false;
		}
		return true;
	}

	protected override bool OnInit()
	{
		if (!base.OnInit())
		{
			return false;
		}
		return true;
	}

	protected override TickResult OnTick()
	{
		if (base.OnTick() == TickResult.Finished)
		{
			return TickResult.Finished;
		}
		return TickResult.Running;
	}

	protected override void OnReset()
	{
		base.OnReset();
	}

	public override string ToString()
	{
		return $"MoveNpc";
	}
}
