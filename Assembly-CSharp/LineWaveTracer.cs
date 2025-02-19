using System.Collections.Generic;
using UnityEngine;

public class LineWaveTracer : glWaveTracer
{
	private class Drawer
	{
		public bool Destroyed;

		private Vector3 m_BeginPos;

		private LineWaveTracer m_Tracer;

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

		private Vector3 dircetion = Vector3.right;

		private float scale = 2f;

		private Vector3 center = Vector3.zero;

		private float _growingScale;

		private Vector2 v1;

		private Vector2 v2;

		private Vector2 v3;

		private Vector2 v4;

		public void Init(LineWaveTracer tracer)
		{
			m_Tracer = tracer;
			m_BeginPos = tracer.Position;
			m_Running = true;
			waterHeight = tracer.WaterHeight;
			strength = tracer.Strength;
			frequency = tracer.Frequency;
			defaultScale = tracer.DefualtScale;
			defaultScaleFactor = tracer.DefualtScaleFactor;
			scaleRate = tracer.ScaleRate;
			maxDuration = tracer.WaveDuration;
			deltaTime = tracer.DeltaTime;
			material = Object.Instantiate(glWaveTracer.LineMat);
		}

		public void LockTracer()
		{
			locked = true;
		}

		public void Update()
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
				if (magnitude < 0.02f)
				{
					dircetion = Vector3.right;
				}
				else
				{
					dircetion = vector.normalized;
				}
				float num = magnitude / 2f;
				_growingScale += scaleRate;
				float b = 50f;
				float num2 = Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
				if (num > 0.5f)
				{
					num = 0.5f;
					center = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + dircetion * 0.1f;
					scale = Mathf.Min(num2 + 2f * magnitude + _growingScale, b);
				}
				else
				{
					scale = Mathf.Min(num2 + 2f + _growingScale, b);
					center = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + dircetion * (0.5f - num) * scale * 0.5f;
				}
				material.SetFloat("_Distance", num * 2f);
				material.SetFloat("_Strength", strength);
				material.SetFloat("_Frequency", frequency);
				material.SetFloat("_TimeFactor", duration + Mathf.Pow(m_Tracer.TimeOffsetFactor + 1f, 0.4f) - 1f);
				material.SetFloat("_DeltaTime", deltaTime);
				material.SetFloat("_Speed", m_Tracer.WaveSpeed);
				Vector2 vector2 = new Vector2(dircetion.x, dircetion.z);
				Vector2 vector3 = new Vector3(vector2.y, 0f - vector2.x);
				Vector2 vector4 = new Vector2(center.x, center.z);
				v1 = vector4 - (vector2 + vector3) * scale * 0.5f;
				v2 = vector4 + (vector2 - vector3) * scale * 0.5f;
				v3 = vector4 + (vector2 + vector3) * scale * 0.5f;
				v4 = vector4 - (vector2 - vector3) * scale * 0.5f;
				if (Application.isEditor)
				{
					Vector3 vector5 = new Vector3(v1.x, waterHeight, v1.y);
					Vector3 vector6 = new Vector3(v2.x, waterHeight, v2.y);
					Vector3 vector7 = new Vector3(v3.x, waterHeight, v3.y);
					Vector3 vector8 = new Vector3(v4.x, waterHeight, v4.y);
					Debug.DrawLine(vector5, vector6, Color.yellow);
					Debug.DrawLine(vector6, vector7, Color.yellow);
					Debug.DrawLine(vector7, vector8, Color.yellow);
					Debug.DrawLine(vector8, vector5, Color.yellow);
				}
			}
			else
			{
				Destroyed = true;
			}
		}

		public void DestroySelf()
		{
			Object.Destroy(material);
		}

		public void Draw()
		{
			GL.PushMatrix();
			int passCount = material.passCount;
			for (int i = 0; i < passCount; i++)
			{
				material.SetPass(i);
				GL.Begin(7);
				GL.Color(Color.white);
				GL.TexCoord2(0f, 0f);
				GL.Vertex(new Vector3(v1.x, waterHeight, v1.y));
				GL.TexCoord2(1f, 0f);
				GL.Vertex(new Vector3(v2.x, waterHeight, v2.y));
				GL.TexCoord2(1f, 1f);
				GL.Vertex(new Vector3(v3.x, waterHeight, v3.y));
				GL.TexCoord2(0f, 1f);
				GL.Vertex(new Vector3(v4.x, waterHeight, v4.y));
				GL.End();
			}
			GL.PopMatrix();
		}
	}

	public bool AutoGenWave = true;

	public float WaveSpeed = 0.5f;

	public float WaveDuration = 5f;

	public float Frequency = 40f;

	public float Strength = 20f;

	public float TimeOffsetFactor = 2f;

	public float IntervalTime = 0.2f;

	public float DeltaTime = 0.5f;

	public float ScaleRate = 0.01f;

	public float DefualtScale;

	public float DefualtScaleFactor = 2f;

	private float curIntervalTime;

	private Drawer lastWave;

	private Vector3 prevPos = Vector3.zero;

	private bool canGen;

	public float Distance = 256f;

	private List<Drawer> m_Waves = new List<Drawer>();

	public Vector3 Pos
	{
		get
		{
			if (TracerTrans != null)
			{
				return TracerTrans.position;
			}
			return base.transform.position;
		}
	}

	public bool CheckValid()
	{
		if (Vector3.SqrMagnitude(Pos - WaveRenderer.transform.position) > Distance * Distance)
		{
			return false;
		}
		return true;
	}

	public override void CustomUpdate()
	{
		if (!CheckValid())
		{
			return;
		}
		if (AutoGenWave)
		{
			if (!canGen)
			{
				if (Vector3.Magnitude(prevPos - base.Position) > 0.05f)
				{
					canGen = true;
				}
			}
			else
			{
				prevPos = base.Position;
			}
			if (curIntervalTime >= IntervalTime)
			{
				if (lastWave != null)
				{
					lastWave.LockTracer();
					lastWave = null;
				}
				if (canGen)
				{
					lastWave = new Drawer();
					lastWave.Init(this);
					m_Waves.Add(lastWave);
					curIntervalTime = 0f;
					canGen = false;
				}
			}
			else
			{
				curIntervalTime += Time.deltaTime;
			}
		}
		else
		{
			if (lastWave != null)
			{
				lastWave.LockTracer();
				lastWave = null;
			}
			curIntervalTime = IntervalTime + 0.001f;
			canGen = false;
		}
		foreach (Drawer wave in m_Waves)
		{
			wave.Update();
		}
		int num = 0;
		while (num < m_Waves.Count)
		{
			if (m_Waves[num].Destroyed)
			{
				m_Waves[num].DestroySelf();
				m_Waves.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public override void Draw()
	{
		if (!CheckValid())
		{
			return;
		}
		foreach (Drawer wave in m_Waves)
		{
			wave.Draw();
		}
	}
}
