using System;
using UnityEngine;

public class WaveTracer : MonoBehaviour
{
	[Serializable]
	public class WaterAttr
	{
		public float Slope = 1f;

		public float Height = 95.5f;
	}

	[Serializable]
	public class LineWaveParam
	{
		public float DeltaTime = 0.5f;
	}

	[Serializable]
	public class SpotWaveParam
	{
		public float minScale = 1f;

		public float speedFactorRandom = 0.1f;
	}

	public WaveScene Scene;

	public EWaveType WaveType;

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

	public WaterAttr WaterAttribute = new WaterAttr();

	public LineWaveParam LineWave = new LineWaveParam();

	public SpotWaveParam SpotWave = new SpotWaveParam();

	[NonSerialized]
	public float curIntervalTime;

	[NonSerialized]
	public LineWaveHandler lastWave;

	[NonSerialized]
	public Vector3 prevPos = Vector3.zero;

	[NonSerialized]
	public bool canGen;

	public Vector3 Position => base.transform.position;

	private void Awake()
	{
		Scene = WaveScene.Self;
		if (Scene != null)
		{
			Scene.AddTracer(this);
		}
	}

	private void Start()
	{
		prevPos = Position;
	}
}
