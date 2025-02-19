using System;
using UnityEngine;
using WhiteCat;

namespace Pathea;

[Serializable]
public class PECapsuleTrigger
{
	public enum AxisDir
	{
		X_Axis,
		Y_Axis,
		Z_Axis,
		Inverse_X_Axis,
		Inverse_Y_Axis,
		Inverse_Z_Axis
	}

	public Transform trans;

	public float radius = 0.2f;

	public float heigh = 0.5f;

	public Vector3 offset = Vector3.zero;

	public AxisDir axis = AxisDir.Y_Axis;

	private float scaledRadius;

	private Vector3 moveDir1;

	private Vector3 moveDir2;

	private Vector3 m_CenterPos1;

	private Vector3 m_CenterPos2;

	private Vector3 m_PosOffset1;

	private Vector3 m_PosOffset2;

	private Vector3 m_LastCenterPos1;

	private Vector3 m_LastCenterPos2;

	private Vector3 m_ParentCenterPos;

	public bool enable => null != trans;

	public void ResetInfo()
	{
		if (!(null == trans))
		{
			Vector3 vector = Vector3.up;
			switch (axis)
			{
			case AxisDir.X_Axis:
				vector = Vector3.right;
				break;
			case AxisDir.Z_Axis:
				vector = Vector3.forward;
				break;
			case AxisDir.Inverse_X_Axis:
				vector = Vector3.left;
				break;
			case AxisDir.Inverse_Y_Axis:
				vector = Vector3.down;
				break;
			case AxisDir.Inverse_Z_Axis:
				vector = Vector3.back;
				break;
			}
			m_PosOffset1 = radius * vector + offset;
			m_PosOffset2 = (heigh - radius) * vector + offset;
		}
	}

	public void Update(Vector3 centerPos)
	{
		if (enable)
		{
			scaledRadius = Mathf.Abs(radius * trans.lossyScale.x);
			m_LastCenterPos1 = m_CenterPos1;
			m_LastCenterPos2 = m_CenterPos2;
			m_CenterPos1 = trans.TransformPoint(m_PosOffset1);
			m_CenterPos2 = trans.TransformPoint(m_PosOffset2);
			moveDir1 = m_CenterPos1 - m_LastCenterPos1;
			moveDir2 = m_CenterPos2 - m_LastCenterPos2;
			m_ParentCenterPos = centerPos;
		}
	}

	private bool CheckCollision(Vector3 pos1, Vector3 pos2, PECapsuleTrigger other, out PECapsuleHitResult result)
	{
		Utility.ClosestPoint(pos1, pos2, other.m_CenterPos1, other.m_CenterPos2, out var pointA, out var pointB);
		Vector3 vector = pointB - pointA;
		float num = scaledRadius + other.scaledRadius;
		if (vector.sqrMagnitude < num * num)
		{
			result = new PECapsuleHitResult();
			result.selfTrans = trans;
			result.hitTrans = other.trans;
			result.hitPos = pointA + vector * scaledRadius / num;
			result.hitDir = (moveDir1 + moveDir2 + vector).normalized;
			result.distance = 0f;
			return true;
		}
		result = null;
		return false;
	}

	public bool CheckCollision(Vector3 pos1, Vector3 pos2, out PECapsuleHitResult result)
	{
		Utility.ClosestPoint(m_CenterPos1, m_CenterPos2, pos1, pos2, out var pointA, out var pointB);
		Vector3 vector = pointB - pointA;
		if (vector.sqrMagnitude < scaledRadius * scaledRadius)
		{
			result = new PECapsuleHitResult();
			result.selfTrans = trans;
			result.hitTrans = trans;
			result.hitDir = Vector3.Normalize(pos2 - pos1);
			result.hitPos = pointB - result.hitDir * Mathf.Sqrt(scaledRadius * scaledRadius - vector.sqrMagnitude);
			result.distance = 0f;
			return true;
		}
		result = null;
		return false;
	}

	public bool CheckCollision(PECapsuleTrigger other, out PECapsuleHitResult result)
	{
		int a = Mathf.FloorToInt(0.5f * moveDir1.magnitude / scaledRadius) + 1;
		int b = Mathf.FloorToInt(0.5f * moveDir2.magnitude / scaledRadius) + 1;
		Vector3 normalized = moveDir1.normalized;
		Vector3 normalized2 = moveDir2.normalized;
		float a2 = Vector3.Distance(m_LastCenterPos1, m_ParentCenterPos);
		float b2 = Vector3.Distance(m_CenterPos1, m_ParentCenterPos);
		float a3 = Vector3.Distance(m_LastCenterPos2, m_ParentCenterPos);
		float b3 = Vector3.Distance(m_CenterPos2, m_ParentCenterPos);
		int num = Mathf.Max(a, b);
		float num2 = ((num <= 1) ? num : (num - 1));
		for (int i = 0; i < num; i++)
		{
			float t = (float)i / num2;
			Vector3 vector = Vector3.Lerp(m_LastCenterPos1, m_CenterPos1, t);
			Vector3 vector2 = Vector3.Lerp(m_LastCenterPos2, m_CenterPos2, t);
			if (m_ParentCenterPos != Vector3.zero)
			{
				if (vector != m_ParentCenterPos)
				{
					vector = m_ParentCenterPos + (vector - m_ParentCenterPos).normalized * Mathf.Lerp(a2, b2, t);
				}
				if (vector2 != m_ParentCenterPos)
				{
					vector2 = m_ParentCenterPos + (vector2 - m_ParentCenterPos).normalized * Mathf.Lerp(a3, b3, t);
				}
			}
			if (CheckCollision(vector, vector2, other, out result))
			{
				return true;
			}
		}
		result = null;
		return false;
	}

	public bool GetClosestPos(Vector3 pos, out PECapsuleHitResult result)
	{
		result = new PECapsuleHitResult();
		result.hitPos = Vector3.zero;
		result.hitDir = Vector3.zero;
		result.selfTrans = trans;
		result.hitTrans = trans;
		result.distance = float.PositiveInfinity;
		if (null != trans)
		{
			Vector3 vector = Utility.ClosestPoint(m_CenterPos1, m_CenterPos2, pos);
			Vector3 vector2 = vector - pos;
			float magnitude = vector2.magnitude;
			result.hitDir = vector2.normalized;
			if (magnitude < scaledRadius)
			{
				result.hitPos = pos;
				result.distance = 0f;
				return true;
			}
			result.hitPos = vector - result.hitDir * scaledRadius;
			result.distance = magnitude - scaledRadius;
			return false;
		}
		return false;
	}
}
