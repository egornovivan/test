using UnityEngine;

public class CameraHitEffect : CamEffect
{
	public Vector3 m_Dir;

	private Vector3 m_TargetPos;

	public Vector3 m_RotDir;

	public AnimationCurve m_Curve;

	public float m_Distance = 1f;

	public int m_Angle = -20;

	private float _time;

	public override void Do()
	{
		_time += 0.03f;
		float num = m_Distance * m_Curve.Evaluate(_time);
		float num2 = m_Curve.Evaluate(_time);
		m_TargetCam.transform.position = m_TargetCam.transform.position + num * m_Dir;
		m_TargetCam.transform.Rotate(m_RotDir, (float)m_Angle * num2, Space.World);
	}
}
