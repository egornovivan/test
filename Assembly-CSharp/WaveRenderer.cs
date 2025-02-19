using System.Collections.Generic;
using UnityEngine;

public class WaveRenderer : MonoBehaviour
{
	public enum ETexPrecision
	{
		Low = 0x200,
		Medium = 0x400,
		High = 0x800
	}

	public enum ECameraMode
	{
		Perspective,
		Orthographical
	}

	public ETexPrecision TexurePrecision = ETexPrecision.Low;

	public ECameraMode CameraMode;

	public Transform FollowTrans;

	public bool AutoFollow = true;

	private List<glWaveTracer> m_Tracers;

	private RenderTexture m_RenderTex;

	private Camera m_Cam;

	public static List<glWaveTracer> s_Tracers = new List<glWaveTracer>();

	private static bool s_DoInitUpdate = false;

	public RenderTexture RenderTarget => m_RenderTex;

	public void Add(glWaveTracer tracer)
	{
		m_Tracers.Add(tracer);
		tracer.WaveRenderer = this;
	}

	public void Remove(glWaveTracer tracer)
	{
		m_Tracers.Remove(tracer);
		tracer.WaveRenderer = this;
	}

	private void Awake()
	{
		m_Tracers = new List<glWaveTracer>();
		m_Cam = base.gameObject.GetComponent<Camera>();
		if (m_RenderTex != null)
		{
			if (m_RenderTex.width != (int)TexurePrecision)
			{
				RenderTexture.ReleaseTemporary(m_RenderTex);
				m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
			}
		}
		else
		{
			m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
		}
	}

	private void Update()
	{
		if (m_RenderTex != null)
		{
			if (m_RenderTex.width != (int)TexurePrecision)
			{
				RenderTexture.ReleaseTemporary(m_RenderTex);
				m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
			}
		}
		else
		{
			m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
		}
		m_Cam.clearFlags = CameraClearFlags.Color;
		m_Cam.backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
		if (CameraMode == ECameraMode.Perspective)
		{
			Camera camera = ((!(FollowTrans == null)) ? FollowTrans.gameObject.GetComponent<Camera>() : Camera.main);
			if (camera != null)
			{
				m_Cam.nearClipPlane = camera.nearClipPlane;
				m_Cam.farClipPlane = camera.farClipPlane;
				m_Cam.aspect = camera.aspect;
				m_Cam.fieldOfView = camera.fieldOfView;
			}
			m_Cam.orthographic = false;
			m_Cam.targetTexture = m_RenderTex;
		}
		else if (CameraMode == ECameraMode.Orthographical)
		{
			m_Cam.nearClipPlane = 0.3f;
			m_Cam.farClipPlane = 200f;
			m_Cam.orthographicSize = (float)TexurePrecision / 2f;
			m_Cam.orthographic = true;
			m_Cam.targetTexture = m_RenderTex;
		}
		if (!s_DoInitUpdate)
		{
			foreach (glWaveTracer s_Tracer in s_Tracers)
			{
				if (!(s_Tracer == null))
				{
					s_Tracer.InitUpdate();
				}
			}
			s_DoInitUpdate = true;
		}
		foreach (glWaveTracer tracer in m_Tracers)
		{
			if (!(tracer == null))
			{
				tracer.WaveRenderer = this;
				tracer.CustomUpdate();
			}
		}
	}

	private void LateUpdate()
	{
		if (CameraMode == ECameraMode.Perspective)
		{
			Camera camera = ((!(FollowTrans == null)) ? FollowTrans.gameObject.GetComponent<Camera>() : Camera.main);
			if (AutoFollow)
			{
				m_Cam.transform.position = camera.transform.position;
				m_Cam.transform.rotation = camera.transform.rotation;
			}
		}
		else if (CameraMode == ECameraMode.Orthographical && AutoFollow)
		{
			Transform transform = ((!(FollowTrans == null)) ? FollowTrans : Camera.main.transform);
			Vector3 position = transform.position;
			m_Cam.transform.position = new Vector3(position.x, position.y + 10f, position.z);
			m_Cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		}
		s_DoInitUpdate = false;
	}

	private void OnPostRender()
	{
		foreach (glWaveTracer tracer in m_Tracers)
		{
			if (tracer != null && tracer.gameObject.activeInHierarchy)
			{
				tracer.Draw();
			}
		}
	}
}
