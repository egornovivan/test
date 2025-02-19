using System.Collections.Generic;
using UnityEngine;

public class WaveScene : MonoBehaviour
{
	public enum ETexPrecision
	{
		Low = 0x200,
		Medium = 0x400,
		High = 0x800
	}

	private const int c_PoolGrowCount = 50;

	private static WaveScene s_Self;

	public Camera RenderCam;

	public int WaveLayer;

	public ETexPrecision TexurePrecision = ETexPrecision.Low;

	private List<WaveTracer> m_Tracers = new List<WaveTracer>();

	public Material SpotWaveMat;

	public Material LineWaveMat;

	private Material m_SpotWaveMat;

	private Material m_LineWaveMat;

	private GameObject m_Go;

	private Camera m_Cam;

	private RenderTexture m_RenderTex;

	private Queue<GameObject> m_GoPool;

	public int WaveCnt;

	public static WaveScene Self => s_Self;

	public RenderTexture RenderTarget => m_RenderTex;

	public void AddTracer(WaveTracer tracer)
	{
		m_Tracers.Add(tracer);
		tracer.curIntervalTime = tracer.IntervalTime + 0.001f;
	}

	private SpotWaveHandler _createSpotWave(WaveTracer tracer, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		if (!base.enabled)
		{
			return null;
		}
		GameObject go = GetGo();
		go.name = "spot Wave";
		go.GetComponent<Renderer>().material = m_SpotWaveMat;
		go.transform.position = pos;
		go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		SpotWaveHandler spotWaveHandler = go.AddComponent<SpotWaveHandler>();
		spotWaveHandler.onRecycle += RecycleGo;
		go.layer = WaveLayer;
		spotWaveHandler.Init(tracer);
		return spotWaveHandler;
	}

	private LineWaveHandler _createLineWave(WaveTracer tracer)
	{
		if (!base.enabled)
		{
			return null;
		}
		GameObject go = GetGo();
		go.name = "Line Wave";
		go.GetComponent<Renderer>().material = m_LineWaveMat;
		LineWaveHandler lineWaveHandler = go.AddComponent<LineWaveHandler>();
		lineWaveHandler.onRecycle += RecycleGo;
		go.layer = WaveLayer;
		lineWaveHandler.Init(tracer);
		return lineWaveHandler;
	}

	private GameObject GetGo()
	{
		if (m_GoPool.Count == 0)
		{
			for (int i = 0; i < 50; i++)
			{
				GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
				gameObject.name = "Wave Object";
				gameObject.transform.parent = base.transform;
				gameObject.GetComponent<Collider>().enabled = false;
				gameObject.gameObject.SetActive(value: false);
				m_GoPool.Enqueue(gameObject);
			}
		}
		GameObject gameObject2 = m_GoPool.Dequeue();
		gameObject2.SetActive(value: true);
		return gameObject2;
	}

	private void RecycleGo(GameObject go)
	{
		go.gameObject.SetActive(value: false);
		go.name = "Wave Object";
		m_GoPool.Enqueue(go);
	}

	private void Awake()
	{
		m_GoPool = new Queue<GameObject>();
		base.gameObject.layer = WaveLayer;
		if (RenderCam != null)
		{
			RenderCam.gameObject.SetActive(value: true);
			RenderCam.enabled = true;
		}
		m_SpotWaveMat = Object.Instantiate(SpotWaveMat);
		m_LineWaveMat = Object.Instantiate(LineWaveMat);
		s_Self = this;
	}

	private void Destroy()
	{
		if (m_RenderTex != null)
		{
			m_RenderTex.Release();
		}
	}

	private void Update()
	{
		if (m_Cam != RenderCam)
		{
			if (m_Cam != null)
			{
				m_Cam.targetTexture = null;
			}
			m_Cam = RenderCam;
			if (m_Cam != null)
			{
				RenderCam.cullingMask = 1 << WaveLayer;
				RenderCam.gameObject.SetActive(value: true);
				RenderCam.enabled = true;
				if (m_RenderTex != null)
				{
					if (m_RenderTex.width != (int)TexurePrecision)
					{
						m_RenderTex.Release();
						m_RenderTex = new RenderTexture((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
					}
				}
				else
				{
					m_RenderTex = new RenderTexture((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
				}
			}
		}
		if (m_Cam != null)
		{
			m_Cam.nearClipPlane = Camera.main.nearClipPlane;
			m_Cam.farClipPlane = Camera.main.farClipPlane;
			m_Cam.aspect = Camera.main.aspect;
			m_Cam.fieldOfView = Camera.main.fieldOfView;
			m_Cam.targetTexture = m_RenderTex;
		}
		for (int i = 0; i < m_Tracers.Count; i++)
		{
			WaveTracer waveTracer = m_Tracers[i];
			if (waveTracer.WaveType == EWaveType.Line)
			{
				if (!m_Tracers[i].enabled)
				{
					if (waveTracer.lastWave != null)
					{
						waveTracer.lastWave.LockTracer();
						waveTracer.lastWave = null;
					}
				}
				else if (waveTracer.AutoGenWave)
				{
					if (!waveTracer.canGen)
					{
						if (Vector3.Magnitude(waveTracer.prevPos - waveTracer.Position) > 0.05f)
						{
							waveTracer.canGen = true;
						}
					}
					else
					{
						waveTracer.prevPos = waveTracer.Position;
					}
					if (waveTracer.curIntervalTime >= waveTracer.IntervalTime)
					{
						if (waveTracer.lastWave != null)
						{
							waveTracer.lastWave.LockTracer();
							waveTracer.lastWave = null;
						}
						if (waveTracer.canGen)
						{
							waveTracer.lastWave = _createLineWave(waveTracer);
							waveTracer.curIntervalTime = 0f;
							waveTracer.canGen = false;
						}
					}
					else
					{
						waveTracer.curIntervalTime += Time.deltaTime;
					}
				}
				else
				{
					if (waveTracer.lastWave != null)
					{
						waveTracer.lastWave.LockTracer();
						waveTracer.lastWave = null;
					}
					waveTracer.curIntervalTime = waveTracer.IntervalTime + 0.001f;
					waveTracer.canGen = false;
				}
			}
			else if (waveTracer.WaveType == EWaveType.Spot && m_Tracers[i].enabled && waveTracer.AutoGenWave)
			{
				if (waveTracer.curIntervalTime >= waveTracer.IntervalTime)
				{
					Vector3 position = waveTracer.transform.position;
					position = new Vector3(position.x, 95.5f, position.z);
					_createSpotWave(waveTracer, position, Quaternion.Euler(new Vector3(90f, 0f, 0f)), Vector3.one);
					waveTracer.curIntervalTime = 0f;
					waveTracer.canGen = false;
				}
				else
				{
					waveTracer.curIntervalTime += Time.deltaTime;
				}
			}
		}
		WaveCnt = base.transform.childCount;
	}
}
