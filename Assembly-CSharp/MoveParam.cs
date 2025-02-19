using System.Collections.Generic;
using UnityEngine;

public class MoveParam : MonoBehaviour
{
	public List<MoveSpeed> m_MoveSpeedList;

	[Header(">>Common")]
	public AnimationCurve m_AngleSpeedScale;

	public static readonly float AutoMoveStopSqrDis = 0.1f;

	public AnimationCurve m_SpeedToLerpF;

	[Header(">>MoveParam")]
	public float m_MoveRotateSpeed = 5f;

	[Header(">>SprintParam")]
	public float m_MinStamina = 10f;

	public float m_StaminaCostSpeed = 20f;

	public float m_SprintRotateSpeed = 5f;

	public float m_SprintSizeScale = 0.15f;

	public float m_FastRotatScale = 30f;
}
