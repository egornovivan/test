using System;
using System.Collections;
using System.Collections.Generic;
using Pathea.Effect;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_RopeGunShoot : PEAction
{
	private enum HookGunState
	{
		Null,
		Shooting,
		Climbing,
		Back
	}

	private const string ClimbAnim = "RopeGunClimb";

	private PERopeGun mRopeGun;

	private HookGunState mState;

	private Vector3 m_ShootDir;

	private int m_CurrentMoveIndex;

	private AudioController m_Audio;

	private float m_ExDoClimbTime;

	private Vector3 m_LastHookPos;

	private List<Vector3> mLinePosList = new List<Vector3>();

	public override PEActionType ActionType => PEActionType.RopeGunShoot;

	public HumanPhyCtrl phyCtrl { get; set; }

	public PERopeGun ropeGun
	{
		get
		{
			return mRopeGun;
		}
		set
		{
			mRopeGun = value;
			if (!(null != mRopeGun))
			{
				return;
			}
			int num = mLinePosList.Count - mRopeGun.lineList.Count;
			if (num < 0)
			{
				for (int i = 0; i < -num; i++)
				{
					mLinePosList.Add(Vector3.zero);
				}
			}
			else if (num > 0)
			{
				for (int j = 0; j < num; j++)
				{
					mLinePosList.RemoveAt(0);
				}
			}
		}
	}

	public override bool CanDoAction(PEActionParam para)
	{
		return null != mRopeGun && base.viewCmpt.hasView && null != phyCtrl;
	}

	public override void DoAction(PEActionParam para)
	{
		if (!(null == mRopeGun) && base.viewCmpt.hasView)
		{
			m_LastHookPos = mRopeGun.hook.position;
			base.motionMgr.SetMaskState(PEActionMask.RopeGunShoot, state: true);
			mState = HookGunState.Shooting;
			m_ShootDir = (base.ikCmpt.aimTargetPos - mRopeGun.gunMuzzle.position).normalized;
			m_CurrentMoveIndex = 0;
			PeCamera.SetFloat("Sensitivity Multiplier", 0f);
			AudioManager.instance.Create(mRopeGun.gunMuzzle.position, mRopeGun.fireSound);
		}
	}

	public override bool Update()
	{
		if (null == mRopeGun || null == phyCtrl || !base.viewCmpt.hasView || mState == HookGunState.Null)
		{
			OnEndAction();
			return true;
		}
		switch (mState)
		{
		case HookGunState.Shooting:
			UpdateShoot();
			break;
		case HookGunState.Climbing:
			UpdateClimbing();
			break;
		case HookGunState.Back:
			UpdateBack();
			break;
		}
		return false;
	}

	private void UpdateShoot()
	{
		float num = mRopeGun.bulletSpeed * Time.deltaTime;
		if (Physics.Raycast(m_LastHookPos, m_ShootDir, out var hitInfo, 10f, -1, QueryTriggerInteraction.Ignore) && hitInfo.distance <= num)
		{
			if (((1 << hitInfo.transform.gameObject.layer) & mRopeGun.effectLayer.value) != 0 && Vector3.SqrMagnitude(hitInfo.point - mRopeGun.gunMuzzle.position) >= mRopeGun.minDis * mRopeGun.minDis)
			{
				mRopeGun.hook.position = (m_LastHookPos = hitInfo.point);
				StartClimb();
			}
			else
			{
				mState = HookGunState.Back;
			}
			m_Audio = AudioManager.instance.Create(mRopeGun.gunMuzzle.position, mRopeGun.ropeSound, mRopeGun.gunMuzzle, isPlay: true, isDelete: false);
		}
		else
		{
			m_LastHookPos += num * m_ShootDir;
			if (Vector3.SqrMagnitude(m_LastHookPos - mRopeGun.gunMuzzle.position) >= mRopeGun.range * mRopeGun.range)
			{
				mState = HookGunState.Back;
				m_Audio = AudioManager.instance.Create(mRopeGun.gunMuzzle.position, mRopeGun.ropeSound, mRopeGun.gunMuzzle, isPlay: true, isDelete: false);
			}
		}
		UpdateLinePos();
	}

	private void UpdateClimbing()
	{
		Vector3 vector = Vector3.zero;
		while (m_CurrentMoveIndex < mLinePosList.Count - 1)
		{
			Vector3 vector2 = mLinePosList[m_CurrentMoveIndex + 1] - mLinePosList[m_CurrentMoveIndex];
			Vector3 vector3 = mRopeGun.gunMuzzle.position - mLinePosList[m_CurrentMoveIndex];
			Vector3 vector4 = Vector3.Project(vector3, vector2);
			if (vector4.sqrMagnitude < float.Epsilon || Vector3.Angle(vector4, vector2) > 90f || vector4.sqrMagnitude < vector2.sqrMagnitude)
			{
				vector = mLinePosList[m_CurrentMoveIndex] + vector4;
				break;
			}
			m_CurrentMoveIndex++;
		}
		if (phyCtrl.velocity.magnitude > 5f * mRopeGun.climbSpeed || Vector3.SqrMagnitude(base.trans.position - m_LastHookPos) > 40000f)
		{
			base.trans.position = m_LastHookPos + 2f * Vector3.up;
			mState = HookGunState.Null;
			ResetPhy();
			return;
		}
		if (m_CurrentMoveIndex == mLinePosList.Count - 1 || Vector3.SqrMagnitude(vector - m_LastHookPos) <= 0.25f || (Time.time >= m_ExDoClimbTime && phyCtrl.grounded))
		{
			mState = HookGunState.Null;
			ResetPhy();
			return;
		}
		mRopeGun.hook.position = m_LastHookPos;
		Vector3 to = vector - m_LastHookPos;
		Vector3 from = mRopeGun.gunMuzzle.position - m_LastHookPos;
		float num = Vector3.Angle(from, to);
		if (num > float.Epsilon)
		{
			Vector3 axis = Vector3.Cross(to.normalized, from.normalized);
			Quaternion quaternion = Quaternion.AngleAxis(num, axis);
			for (int i = m_CurrentMoveIndex; i < mLinePosList.Count - 1; i++)
			{
				Vector3 vector5 = mLinePosList[i] - m_LastHookPos;
				vector5 = quaternion * vector5;
				mLinePosList[i] = vector5 + m_LastHookPos;
			}
		}
		Vector3 desiredMovementDirection = Vector3.Normalize(mLinePosList[m_CurrentMoveIndex + 1] - mLinePosList[m_CurrentMoveIndex]);
		phyCtrl.m_SubAcc = ((!(desiredMovementDirection.y > 0f)) ? Vector3.zero : (Physics.gravity.y * Vector3.down));
		phyCtrl.ResetSpeed(((!(desiredMovementDirection.y > 0f)) ? mRopeGun.moveDownSpeedScale : 1f) * mRopeGun.climbSpeed);
		phyCtrl.desiredMovementDirection = desiredMovementDirection;
		for (int j = 0; j < mRopeGun.lineList.Count; j++)
		{
			if (j < m_CurrentMoveIndex + 1)
			{
				mRopeGun.lineList[j].localPosition = Vector3.zero;
			}
			else
			{
				mRopeGun.lineList[j].position = mLinePosList[j];
			}
		}
	}

	private void UpdateBack()
	{
		float num = mRopeGun.bulletSpeed * Time.deltaTime;
		Vector3 vector = mRopeGun.gunMuzzle.position - m_LastHookPos;
		if (vector.sqrMagnitude <= num * num)
		{
			mState = HookGunState.Null;
			mRopeGun.hook.localPosition = Vector3.zero;
			ResetLine();
		}
		else
		{
			m_LastHookPos += num * vector.normalized;
			UpdateLinePos();
		}
	}

	public override void EndImmediately()
	{
		OnEndAction();
	}

	private void UpdateLinePos()
	{
		Vector3 vector = m_LastHookPos - mRopeGun.gunMuzzle.position;
		int count = mRopeGun.lineList.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector2 = vector * i / (count - 1);
			List<Vector3> list = mLinePosList;
			int index = i;
			Vector3 vector3 = mRopeGun.gunMuzzle.position + vector2 + (1f - Mathf.Abs((float)i * 2f / (float)count - 1f) * Mathf.Abs((float)i * 2f / (float)count - 1f)) * vector.magnitude * mRopeGun.lineDownF;
			mRopeGun.lineList[i].position = vector3;
			list[index] = vector3;
		}
	}

	private void ResetLine()
	{
		if (null != ropeGun)
		{
			ropeGun.hook.localPosition = Vector3.zero;
			for (int i = 0; i < ropeGun.lineList.Count; i++)
			{
				ropeGun.lineList[i].localPosition = Vector3.zero;
			}
		}
	}

	private void OnEndAction()
	{
		base.motionMgr.SetMaskState(PEActionMask.RopeGunShoot, state: false);
		if (null != m_Audio)
		{
			UnityEngine.Object.Destroy(m_Audio.gameObject);
		}
		PeCamera.SetFloat("Sensitivity Multiplier", 1f);
		if (Vector3.SqrMagnitude(base.trans.position - m_LastHookPos) > 40000f)
		{
			base.trans.position = m_LastHookPos + 2f * Vector3.up;
		}
		base.motionMgr.StartCoroutine(FixPhyError());
		ResetPhy();
		ResetLine();
	}

	private void ResetPhy()
	{
		if (null != phyCtrl)
		{
			phyCtrl.useRopeGun = false;
			phyCtrl.m_SubAcc = Vector3.zero;
			phyCtrl.desiredMovementDirection = Vector3.zero;
			phyCtrl.ResetInertiaVelocity();
			phyCtrl.CancelMoveRequest();
			phyCtrl._rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
	}

	private void StartClimb()
	{
		mState = HookGunState.Climbing;
		base.anim.SetBool("RopeGunClimb", value: true);
		phyCtrl.m_SubAcc = Physics.gravity.y * Vector3.down;
		phyCtrl.useRopeGun = true;
		phyCtrl._rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		m_ExDoClimbTime = Time.time + mRopeGun.minClimbTime;
		Singleton<EffectBuilder>.Instance.Register(mRopeGun.hitEffectID, null, mRopeGun.hook.position, Quaternion.identity);
	}

	private IEnumerator FixPhyError()
	{
		Vector3 lastPos = base.trans.position;
		yield return new WaitForSeconds(0.5f);
		if (Vector3.Distance(base.trans.position, m_LastHookPos) > 100f)
		{
			base.trans.position = lastPos + Vector3.up;
		}
	}
}
