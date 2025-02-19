using UnityEngine;

public class LineWaveHandler : WaveHandler
{
	private const float c_DefaultMeter = 1f;

	private Vector3 m_BeginPos;

	private WaveTracer m_Tracer;

	private Vector3 m_EndPos;

	private float m_CurTime;

	private bool m_Running;

	private Material material;

	private Transform trans;

	private float duration;

	private bool locked;

	private float waterHeight;

	private float strength;

	private float frequency;

	private float defaultScale;

	private float defaultScaleFactor;

	private float scaleRate;

	private float maxDuration;

	private float deltaTime;

	private float _growingScale;

	public override void Init(WaveTracer tracer)
	{
		m_Tracer = tracer;
		m_BeginPos = tracer.Position;
		m_Running = true;
		waterHeight = tracer.WaterAttribute.Height;
		strength = tracer.Strength;
		frequency = tracer.Frequency;
		defaultScale = tracer.DefualtScale;
		defaultScaleFactor = tracer.DefualtScaleFactor;
		scaleRate = tracer.ScaleRate;
		maxDuration = tracer.WaveDuration;
		deltaTime = tracer.LineWave.DeltaTime;
	}

	public void LockTracer()
	{
		locked = true;
	}

	private void Awake()
	{
		trans = base.transform;
	}

	private void Start()
	{
		material = GetComponent<Renderer>().material;
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		if (!m_Running)
		{
			return;
		}
		if (duration < maxDuration)
		{
			duration += Time.deltaTime;
			if (!locked)
			{
				m_EndPos = m_Tracer.Position;
			}
			m_BeginPos.y = waterHeight;
			m_EndPos.y = waterHeight;
			Vector3 vector = m_EndPos - m_BeginPos;
			float magnitude = new Vector2(vector.x, vector.z).magnitude;
			Vector3 normalized = vector.normalized;
			float num = magnitude / 2f;
			_growingScale += scaleRate;
			float b = 50f;
			float num2 = Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
			if (num > 0.5f)
			{
				num = 0.5f;
				trans.position = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + normalized * 0.1f;
				float num3 = Mathf.Min(num2 + 2f * magnitude + _growingScale, b);
				trans.localScale = new Vector3(num3, num3, 1f);
				trans.rotation = Quaternion.identity;
				trans.right = normalized;
				trans.Rotate(new Vector3(90f, 0f, 0f));
			}
			else if (magnitude < 0.01f)
			{
				trans.rotation = Quaternion.identity;
				float num4 = Mathf.Min(num2 + 2f + _growingScale, b);
				trans.localScale = new Vector3(num4, num4);
				trans.position = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + Vector3.right * (0.5f - num) * num4 * 0.5f;
				trans.Rotate(new Vector3(90f, 0f, 0f));
			}
			else
			{
				trans.rotation = Quaternion.identity;
				float num5 = Mathf.Min(num2 + 2f + _growingScale, b);
				trans.localScale = new Vector3(num5, num5, 1f);
				trans.position = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + normalized * (0.5f - num) * num5 * 0.5f;
				trans.right = normalized;
				trans.Rotate(new Vector3(90f, 0f, 0f));
			}
			material.SetFloat("_Distance", num * 2f);
			material.SetFloat("_Strength", strength);
			material.SetFloat("_Frequency", frequency);
			material.SetFloat("_TimeFactor", duration + Mathf.Pow(m_Tracer.TimeOffsetFactor + 1f, 0.4f) - 1f);
			material.SetFloat("_DeltaTime", deltaTime);
			material.SetFloat("_Speed", m_Tracer.WaveSpeed);
		}
		else
		{
			Object.Destroy(this);
			Recycle();
		}
	}
}
