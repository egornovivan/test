using UnityEngine;

public class CameraSpringEffect : CamEffect
{
	public Transform m_Character;

	public Transform m_Bone;

	private Vector3 m_LastCharPos;

	private bool m_Ignore = true;

	private Vector3 s = Vector3.zero;

	private Vector3 v = Vector3.zero;

	private Vector3 a = Vector3.zero;

	private Vector3 f = Vector3.zero;

	private Vector3 s0 = Vector3.zero;

	public float k = 1f;

	public float cd = 2f;

	public float amp = 7f;

	public float sc = 4f;

	public float rot = 0.8f;

	public override void Do()
	{
		if (!(m_Bone == null) && !(m_Character == null))
		{
			Vector3 position = m_Bone.position;
			float magnitude = (m_TargetCam.transform.position - position).magnitude;
			if (!m_Ignore)
			{
				float num = Mathf.Clamp(Time.deltaTime, 0.001f, 0.025f) * sc;
				s0 = Vector3.Lerp(s0, position - m_LastCharPos, 0.3f);
				s0 = Vector3.ClampMagnitude(s0, 1f);
				a = (s0 - s) * k;
				f = -v * cd;
				v = (a + f) * num + v;
				s = v * num + s;
				s = Vector3.ClampMagnitude(s, magnitude / amp);
				Vector3 vector = -s * amp;
				vector = Vector3.ClampMagnitude(vector, magnitude);
				m_TargetCam.transform.position += vector;
			}
			m_LastCharPos = m_Bone.position;
			m_Ignore = false;
		}
	}
}
