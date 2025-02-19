using UnityEngine;

public class CameraShakeEffect : CamEffect
{
	public float m_Multiplier = 1f;

	public float m_AmplitudeY = 0.08f;

	public float m_AmplitudeXZ = 0.02f;

	public float m_AmplitudeRoll;

	public float m_Damper = 5f;

	public float m_Freq = 20f;

	public float m_FreqAdder;

	public bool m_ShakeNow;

	public AnimationCurve m_Curve;

	private float _currAmplitude_y;

	private float _currAmplitude_xz;

	private float _currAmplitude_r;

	private float _freq;

	private float _time;

	public override void Do()
	{
		if (m_ShakeNow)
		{
			m_ShakeNow = false;
			Shake();
		}
		float num = Mathf.Clamp01(1f - Time.deltaTime * m_Damper);
		_currAmplitude_y *= num;
		_currAmplitude_xz *= num;
		_currAmplitude_r *= num;
		_freq += m_FreqAdder * 0.02f;
		_time += _freq * 0.02f;
		float y = m_Multiplier * _currAmplitude_y * m_Curve.Evaluate(_time);
		float x = m_Multiplier * _currAmplitude_xz * m_Curve.Evaluate(_time * 0.81f + 0.2f);
		float z = m_Multiplier * _currAmplitude_xz * m_Curve.Evaluate(_time * 1.13f + 0.4f);
		float angle = m_Multiplier * _currAmplitude_r * m_Curve.Evaluate(_time);
		m_TargetCam.transform.position = m_TargetCam.transform.position + new Vector3(x, y, z);
		m_TargetCam.transform.Rotate(m_TargetCam.transform.forward, angle, Space.World);
	}

	public void Shake()
	{
		_currAmplitude_y = m_AmplitudeY;
		_currAmplitude_xz = m_AmplitudeXZ;
		_currAmplitude_r = m_AmplitudeRoll;
		_freq = m_Freq;
		_time = 0f;
	}
}
