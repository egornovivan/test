using UnityEngine;

namespace Pathea;

public class FollowCmpt : PeCmpt
{
	private const float SmoothingTime = 0.5f;

	private SmoothFollower mSmoothFollower;

	[SerializeField]
	private Transform mTarget;

	private Motion_Move mMove;

	private PeTrans mTrans;

	private MotionMgrCmpt mMotionMgr;

	public float lostDis = float.MaxValue;

	public float setPosDis = float.MaxValue;

	public override void Awake()
	{
		base.Awake();
		mSmoothFollower = new SmoothFollower(0.5f);
	}

	public override void Start()
	{
		base.Start();
		mMove = base.Entity.GetCmpt<Motion_Move>();
		mTrans = base.Entity.peTrans;
		mMotionMgr = base.Entity.GetCmpt<MotionMgrCmpt>();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!(null == mMove) && !(null == mTarget))
		{
			float num = Vector3.Distance(mTarget.position, mTrans.position);
			if (num > setPosDis)
			{
				mTrans.position = mTarget.position;
				return;
			}
			if (num > lostDis)
			{
				mMotionMgr.EndAction(PEActionType.Move);
				return;
			}
			Vector3 targetPos = mSmoothFollower.Update(mTarget.position, Time.deltaTime);
			mMove.MoveTo(targetPos);
		}
	}

	public void Follow(PeTrans target, float height = 0f)
	{
		if (!(null == target))
		{
			Follow(target.trans, height);
		}
	}

	public void Defollow()
	{
		if (null != mTarget)
		{
			FollowPoint.DestroyFollowTrans(mTarget);
		}
	}

	private void Follow(Transform target, float height = 0f)
	{
		Defollow();
		if (null != target)
		{
			mTarget = FollowPoint.CreateFollowTrans(target, height);
			mSmoothFollower.Update(mTarget.position, Time.deltaTime, reset: true);
		}
		else
		{
			mTarget = null;
		}
	}
}
