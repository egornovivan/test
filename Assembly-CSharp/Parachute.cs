using ItemAsset;
using SkillAsset;
using UnityEngine;

public class Parachute : Equipment
{
	private const float Gravity = 10f;

	public float mTurnOnSpeed = 10f;

	public GameObject mGlideObj;

	public int mSoundID;

	private bool mGlideActive;

	private Animator mAnimator;

	protected PhysicsCharacterMotor mPCM;

	public override void InitEquipment(SkillRunner runner, ItemObject item)
	{
		base.InitEquipment(runner, item);
		if (null != mGlideObj)
		{
			mAnimator = mGlideObj.GetComponent<Animator>();
		}
		mPCM = runner.GetComponent<PhysicsCharacterMotor>();
	}

	private void FixedUpdate()
	{
		if (null != mPCM && mGlideActive && mPCM.GetComponent<Rigidbody>().velocity.y < 0f - mTurnOnSpeed)
		{
			mPCM.GetComponent<Rigidbody>().AddForce(15f * Vector3.up, ForceMode.Acceleration);
		}
	}

	public void LateUpdate()
	{
		if (!mMainPlayerEquipment || !(null != mPCM))
		{
			return;
		}
		if (mGlideActive)
		{
			if (mPCM.GetComponent<Rigidbody>().isKinematic || mPCM.GetComponent<Rigidbody>().velocity.y > -1f)
			{
				EndParachute();
			}
		}
		else if (mPCM.GetComponent<Rigidbody>().velocity.y < 0f - mTurnOnSpeed)
		{
			StartParachute();
		}
	}

	public void StartParachute()
	{
		mGlideActive = true;
		if (null != mAnimator)
		{
			mAnimator.SetBool("Open", value: true);
			mAnimator.SetBool("Close", value: false);
			AudioManager.instance.Create(base.transform.position, mSoundID);
		}
	}

	public void EndParachute()
	{
		mGlideActive = false;
		if (null != mAnimator)
		{
			mAnimator.SetBool("Open", value: false);
			mAnimator.SetBool("Close", value: true);
		}
	}
}
