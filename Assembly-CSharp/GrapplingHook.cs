using System.Collections.Generic;
using SkillAsset;
using UnityEngine;

public class GrapplingHook : Gun
{
	private enum HookGunState
	{
		Null,
		Shooting,
		Climbing,
		Back
	}

	public Transform mHook;

	public List<Transform> mLineList;

	private List<Vector3> mLinePosList;

	public float mBulletSpeed = 100f;

	public float mPushSpeed = 100f;

	public Vector3 mLineDownF = 0.01f * Vector3.down;

	public int mFireSound;

	private Vector3 mOffsetPos = Vector3.up;

	private Vector3 mShootDir;

	private Vector3 mTargetPos;

	private int mCurrentMoveIndex;

	private float mCurrentMoveDis;

	private float mClimpOffset;

	private HookGunState mState;

	private void Start()
	{
		mLinePosList = new List<Vector3>();
		for (int i = 0; i < mLineList.Count; i++)
		{
			mLinePosList.Add(Vector3.zero);
		}
	}

	private void EndClimb()
	{
		List<string> list = new List<string>();
		list.Add("GrapplingEnd");
		mSkillRunner.ApplyAnim(list);
		if (null != mSkillRunner.GetComponent<Rigidbody>())
		{
			mSkillRunner.GetComponent<Rigidbody>().isKinematic = false;
		}
	}

	public override void RemoveEquipment()
	{
		EndClimb();
		base.RemoveEquipment();
	}

	private void FixedUpdate()
	{
		switch (mState)
		{
		case HookGunState.Shooting:
			UpdateBulletShootPos();
			break;
		case HookGunState.Back:
			UpdateBulletBackPos();
			break;
		case HookGunState.Climbing:
			if (null != mSkillRunner)
			{
				UpdateUserPos();
			}
			break;
		}
	}

	private void UpdateBulletShootPos()
	{
		mHook.transform.position = mTargetPos;
		float num = mBulletSpeed * Time.fixedDeltaTime;
		if (Physics.Raycast(mHook.transform.position, mShootDir, out var hitInfo) && hitInfo.distance < num)
		{
			if (hitInfo.transform.gameObject.layer == 12)
			{
				mTargetPos = hitInfo.point;
				mState = HookGunState.Climbing;
				List<string> list = new List<string>();
				list.Add("GrapplingStart");
				mSkillRunner.ApplyAnim(list);
				mClimpOffset = 1.5f;
				if (null != mSkillRunner.GetComponent<Rigidbody>())
				{
					mSkillRunner.GetComponent<Rigidbody>().isKinematic = true;
				}
				return;
			}
			if (!hitInfo.collider.isTrigger)
			{
				mState = HookGunState.Back;
				return;
			}
		}
		mTargetPos = (mHook.transform.position += num * mShootDir);
		if (Vector3.Distance(mHook.transform.position, mGunMuzzle[mCurrentIndex].End.position) > mRange)
		{
			mState = HookGunState.Back;
		}
		UpdateLinePos();
	}

	private void UpdateBulletBackPos()
	{
		mHook.transform.position = mTargetPos;
		float num = mBulletSpeed * Time.fixedDeltaTime;
		mShootDir = (mGunMuzzle[mCurrentIndex].End.position - mHook.transform.position).normalized;
		if (Physics.Raycast(mHook.transform.position, mShootDir, out var hitInfo) && hitInfo.distance < num)
		{
			mState = HookGunState.Null;
			for (int i = 0; i < mLineList.Count; i++)
			{
				mLineList[i].localPosition = Vector3.zero;
			}
			mHook.transform.localPosition = Vector3.zero;
			Invoke("EndClimb", 0.2f);
		}
		else
		{
			mTargetPos = (mHook.transform.position += num * mShootDir);
			UpdateLinePos();
		}
	}

	private void UpdateLinePos()
	{
		Vector3 vector = mHook.transform.position - mGunMuzzle[mCurrentIndex].End.position;
		for (int i = 0; i < mLineList.Count; i++)
		{
			Vector3 vector2 = vector * i / (mLineList.Count - 1);
			mLineList[i].position = mGunMuzzle[mCurrentIndex].End.position + vector2 + (1f - Mathf.Abs((float)i * 2f / (float)mLineList.Count - 1f) * Mathf.Abs((float)i * 2f / (float)mLineList.Count - 1f)) * vector.magnitude * mLineDownF;
			mLinePosList[i] = mLineList[i].position;
		}
	}

	private void UpdateUserPos()
	{
		Vector3 vector = mLinePosList[mCurrentMoveIndex];
		Vector3 vector2 = mLinePosList[mCurrentMoveIndex + 1];
		Vector3 vector3 = vector2 - vector;
		mOffsetPos = mSkillRunner.transform.position - mGunMuzzle[mCurrentIndex].End.position;
		float num = mPushSpeed * Time.fixedDeltaTime;
		mCurrentMoveDis += num;
		PhysicsCharacterMotor component = mSkillRunner.GetComponent<PhysicsCharacterMotor>();
		if (null != component)
		{
			if (!component.GetComponent<Rigidbody>().isKinematic)
			{
				component.GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			mClimpOffset -= Time.fixedDeltaTime;
			if (mClimpOffset < 0f && component.grounded)
			{
				mSkillRunner.transform.position = vector2;
				mState = HookGunState.Null;
				for (int i = 0; i < mLineList.Count; i++)
				{
					mLineList[i].localPosition = Vector3.zero;
				}
				mHook.transform.localPosition = Vector3.zero;
				Invoke("EndClimb", 0.2f);
				if (Vector3.Distance(vector2, mHook.position) < 1f)
				{
					Ray ray = new Ray(mHook.position + 0.5f * Vector3.up, mSkillRunner.transform.forward);
					if (!Physics.Raycast(ray, 0.5f))
					{
						mSkillRunner.transform.position = mHook.position + 0.5f * (Vector3.up + mSkillRunner.transform.forward);
					}
				}
				else
				{
					Ray ray2 = new Ray(mSkillRunner.transform.position + 0.5f * Vector3.up, mSkillRunner.transform.forward);
					if (!Physics.Raycast(ray2, 0.5f))
					{
						mSkillRunner.transform.position = mSkillRunner.transform.position + 0.5f * (Vector3.up + mSkillRunner.transform.forward);
					}
				}
				return;
			}
		}
		if (mCurrentMoveDis > vector3.magnitude)
		{
			if (mCurrentMoveIndex == mLinePosList.Count - 2)
			{
				mSkillRunner.transform.position = vector2;
				mState = HookGunState.Null;
				for (int j = 0; j < mLineList.Count; j++)
				{
					mLineList[j].localPosition = Vector3.zero;
				}
				mHook.transform.localPosition = Vector3.zero;
				Invoke("EndClimb", 0.2f);
				Ray ray3 = new Ray(mHook.position + 0.5f * Vector3.up, mSkillRunner.transform.forward);
				if (!Physics.Raycast(ray3, 0.5f))
				{
					mSkillRunner.transform.position = mHook.position + 0.5f * (Vector3.up + mSkillRunner.transform.forward);
				}
				return;
			}
			mCurrentMoveDis -= vector3.magnitude;
			mCurrentMoveIndex++;
			vector = mLinePosList[mCurrentMoveIndex];
			vector2 = mLinePosList[mCurrentMoveIndex + 1];
			vector3 = vector2 - vector;
		}
		mSkillRunner.transform.position = vector + vector3.normalized * mCurrentMoveDis + mOffsetPos;
		for (int k = 0; k < mLineList.Count; k++)
		{
			if (k < mCurrentMoveIndex + 1)
			{
				mLineList[k].position = mGunMuzzle[mCurrentIndex].End.position;
			}
			else
			{
				mLineList[k].position = mLinePosList[k];
			}
		}
	}

	public override bool CostSkill(ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
		{
			return false;
		}
		if (mState == HookGunState.Null && buttonDown)
		{
			(target.GetPosition() - mGunMuzzle[mCurrentIndex].End.position).Normalize();
			mState = HookGunState.Shooting;
			mTargetPos = mHook.transform.position;
			mCurrentMoveIndex = 0;
			mShootDir = (target.GetPosition() - mGunMuzzle[mCurrentIndex].End.position).normalized;
			mOffsetPos = mSkillRunner.transform.position - mGunMuzzle[mCurrentIndex].End.position;
			List<string> list = new List<string>();
			list.Add("HoldOnFire");
			mSkillRunner.ApplyAnim(list);
			mClimpOffset = 1.5f;
			mHuman.ApplyDurabilityReduce(0);
			AudioManager.instance.Create(base.transform.position, mFireSound);
			return true;
		}
		return false;
	}
}
