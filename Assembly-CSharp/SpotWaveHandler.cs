using UnityEngine;

public class SpotWaveHandler : WaveHandler
{
	private WaveTracer m_Tracer;

	private bool m_Running;

	private float speed;

	private Material material;

	private float duration;

	private Transform trans;

	private float waterHeight;

	private float maxDuration;

	private float minScale;

	private float timeOffsetFactor;

	private float strength;

	private float frequency;

	private float defaultScale;

	private float defaultScaleFactor;

	private float scaleRate;

	private float _growingScale;

	public override void Init(WaveTracer tracer)
	{
		m_Tracer = tracer;
		m_Running = true;
		speed = tracer.WaveSpeed + Random.Range(0f, tracer.SpotWave.speedFactorRandom);
		waterHeight = m_Tracer.WaterAttribute.Height;
		maxDuration = m_Tracer.WaveDuration;
		minScale = m_Tracer.SpotWave.minScale;
		timeOffsetFactor = m_Tracer.TimeOffsetFactor;
		strength = m_Tracer.Strength;
		frequency = m_Tracer.Frequency;
		defaultScale = tracer.DefualtScale;
		defaultScaleFactor = tracer.DefualtScaleFactor;
		scaleRate = tracer.ScaleRate;
	}

	private void Awake()
	{
		trans = base.transform;
	}

	private void Start()
	{
		material = GetComponent<Renderer>().material;
	}

	private void LateUpdate()
	{
		if (m_Running)
		{
			if (duration < maxDuration)
			{
				duration += Time.deltaTime;
				_growingScale += scaleRate;
				float num = minScale + Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
				trans.localScale = new Vector3(num, num, 0f);
				material.SetFloat("_Speed", speed);
				material.SetFloat("_TimeFactor", duration + Mathf.Pow(timeOffsetFactor + 1f, 0.4f) - 1f);
				Vector3 position = trans.position;
				trans.position = new Vector3(position.x, waterHeight, position.z);
			}
			else
			{
				Object.Destroy(this);
				Recycle();
			}
			material.SetFloat("_Strength", strength);
			material.SetFloat("_Frequency", frequency);
		}
	}
}
