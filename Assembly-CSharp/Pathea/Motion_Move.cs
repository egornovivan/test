using System.Collections.Generic;
using Steer3D;
using UnityEngine;

namespace Pathea;

public abstract class Motion_Move : PeCmpt
{
	protected SpeedState m_SpeedState;

	protected MovementState m_State;

	protected MoveStyle m_Style;

	protected MoveMode m_Mode;

	protected static Stack<NetTranInfo> g_NetTranInfos = new Stack<NetTranInfo>();

	protected SteerAgent m_Steer;

	protected List<NetTranInfo> mNetTransInfos = new List<NetTranInfo>();

	public virtual SpeedState speed
	{
		get
		{
			return m_SpeedState;
		}
		set
		{
			m_SpeedState = value;
		}
	}

	public virtual MovementState state
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public virtual MoveStyle baseMoveStyle { get; set; }

	public virtual MoveStyle style
	{
		get
		{
			return m_Style;
		}
		set
		{
			m_Style = value;
		}
	}

	public virtual MoveMode mode
	{
		get
		{
			return m_Mode;
		}
		set
		{
			m_Mode = value;
		}
	}

	public virtual Vector3 velocity
	{
		get
		{
			return Vector3.zero;
		}
		set
		{
		}
	}

	public virtual Vector3 movement => Vector3.zero;

	public virtual bool grounded => false;

	public virtual float gravity
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	protected NetTranInfo GetNetTransInfo()
	{
		NetTranInfo netTranInfo = null;
		if (g_NetTranInfos.Count > 0)
		{
			netTranInfo = g_NetTranInfos.Pop();
		}
		if (netTranInfo == null)
		{
			netTranInfo = new NetTranInfo();
		}
		return netTranInfo;
	}

	protected void RecycleNetTranInfo(NetTranInfo info)
	{
		g_NetTranInfos.Push(info);
	}

	public abstract void Move(Vector3 dir, SpeedState state = SpeedState.Walk);

	public abstract void SetSpeed(float Speed);

	public abstract void MoveTo(Vector3 targetPos, SpeedState state = SpeedState.Walk, bool avoid = true);

	public abstract void NetMoveTo(Vector3 position, Vector3 moveVelocity, bool immediately = false);

	public abstract void NetRotateTo(Vector3 eulerAngle);

	public abstract void Jump();

	public virtual void Dodge(Vector3 dir)
	{
	}

	public virtual void RotateTo(Vector3 targetDir)
	{
	}

	public virtual void ApplyForce(Vector3 power, ForceMode mode)
	{
	}

	public virtual bool Stucking(float time)
	{
		return false;
	}

	public virtual void Stop()
	{
	}

	public Seek AlterSeekBehaviour(Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		return (!(m_Steer != null)) ? null : m_Steer.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight);
	}

	public Seek AlterSeekBehaviour(Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		return (!(m_Steer != null)) ? null : m_Steer.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight);
	}

	public void AddNetTransInfo(Vector3 pos, Vector3 rot, SpeedState speed, double controllerTime)
	{
		NetTranInfo netTransInfo = GetNetTransInfo();
		netTransInfo.pos = pos;
		netTransInfo.rot = rot;
		netTransInfo.speed = speed;
		netTransInfo.contrllerTime = controllerTime;
		mNetTransInfos.Add(netTransInfo);
	}
}
