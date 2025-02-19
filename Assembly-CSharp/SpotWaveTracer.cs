using System.Collections.Generic;
using UnityEngine;

public class SpotWaveTracer : glWaveTracer
{
	private class Drawer
	{
		public bool Destroyed;

		private SpotWaveTracer m_Tracer;

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

		private float scale = 2f;

		private Vector3 center = Vector3.zero;

		private Vector3 v1;

		private Vector3 v2;

		private Vector3 v3;

		private Vector3 v4;

		private float _growingScale;

		public void Init(SpotWaveTracer tracer)
		{
			m_Tracer = tracer;
			m_Running = true;
			speed = tracer.WaveSpeed + Random.Range(0f, tracer.speedFactorRandom);
			waterHeight = m_Tracer.WaterHeight;
			maxDuration = m_Tracer.WaveDuration;
			minScale = m_Tracer.minScale;
			timeOffsetFactor = m_Tracer.TimeOffsetFactor;
			strength = m_Tracer.Strength;
			frequency = m_Tracer.Frequency;
			defaultScale = tracer.DefualtScale;
			defaultScaleFactor = tracer.DefualtScaleFactor;
			scaleRate = tracer.ScaleRate;
			center = m_Tracer.Position;
			material = Object.Instantiate(glWaveTracer.SpotMat);
		}

		public void Update()
		{
			if (m_Running)
			{
				if (duration < maxDuration)
				{
					duration += Time.deltaTime;
					_growingScale += scaleRate;
					scale = minScale + Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
				}
				else
				{
					Destroyed = true;
				}
				material.SetFloat("_Speed", speed);
				material.SetFloat("_TimeFactor", duration + Mathf.Pow(timeOffsetFactor + 1f, 0.4f) - 1f);
				material.SetFloat("_Strength", strength);
				material.SetFloat("_Frequency", frequency);
				Vector2 vector = new Vector2(1f, 0f);
				Vector2 vector2 = new Vector3(0f, 1f);
				Vector2 vector3 = new Vector2(center.x, center.z);
				v1 = vector3 - (vector + vector2) * scale * 0.5f;
				v2 = vector3 + (vector - vector2) * scale * 0.5f;
				v3 = vector3 + (vector + vector2) * scale * 0.5f;
				v4 = vector3 - (vector - vector2) * scale * 0.5f;
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

	public float ScaleRate = 0.01f;

	public float DefualtScale;

	public float DefualtScaleFactor = 2f;

	public float minScale = 1f;

	public float speedFactorRandom = 0.1f;

	private float curIntervalTime;

	private Drawer lastWave;

	private List<Drawer> m_Waves = new List<Drawer>();

	public override void CustomUpdate()
	{
		if (AutoGenWave)
		{
			if (curIntervalTime >= IntervalTime)
			{
				Drawer drawer = new Drawer();
				drawer.Init(this);
				m_Waves.Add(drawer);
				curIntervalTime = 0f;
			}
			else
			{
				curIntervalTime += Time.deltaTime;
			}
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
		foreach (Drawer wave in m_Waves)
		{
			wave.Draw();
		}
	}
}
