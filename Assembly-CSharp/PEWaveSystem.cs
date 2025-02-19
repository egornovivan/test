using Pathea;
using UnityEngine;

public class PEWaveSystem : MonoBehaviour
{
	private static PEWaveSystem s_Self;

	private WaveRenderer m_WaveRenderer;

	private WaveRenderer m_GrassWaveRenderer;

	public static PEWaveSystem Self => s_Self;

	public WaveRenderer WaveRenderer => m_WaveRenderer;

	public WaveRenderer GrassWaveRenderer => m_GrassWaveRenderer;

	public RenderTexture Target => m_WaveRenderer.RenderTarget;

	public RenderTexture GrassTarget => m_GrassWaveRenderer.RenderTarget;

	private void Awake()
	{
		GameObject gameObject = new GameObject("Water Wave Camera");
		Camera camera = gameObject.AddComponent<Camera>();
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
		camera.cullingMask = 0;
		camera.depth = -2f;
		m_WaveRenderer = camera.gameObject.AddComponent<WaveRenderer>();
		m_WaveRenderer.CameraMode = WaveRenderer.ECameraMode.Perspective;
		gameObject = new GameObject("Grass Wave Camera");
		camera = gameObject.AddComponent<Camera>();
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
		camera.cullingMask = 0;
		camera.depth = -2f;
		m_GrassWaveRenderer = camera.gameObject.AddComponent<WaveRenderer>();
		m_GrassWaveRenderer.CameraMode = WaveRenderer.ECameraMode.Orthographical;
		s_Self = this;
	}

	private void Update()
	{
		PeGrassSystem.SetWaveTexture(GrassWaveRenderer.RenderTarget);
		if (GrassWaveRenderer.RenderTarget != null)
		{
			Vector4 zero = Vector4.zero;
			if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
			{
				Vector3 position = GrassWaveRenderer.transform.position;
				zero = new Vector4(position.x, position.z, GrassWaveRenderer.RenderTarget.width, GrassWaveRenderer.RenderTarget.height);
			}
			else
			{
				PeTrans peTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
				GrassWaveRenderer.FollowTrans = peTrans.trans;
				Vector3 position2 = GrassWaveRenderer.transform.position;
				zero = new Vector4(position2.x, position2.z, GrassWaveRenderer.RenderTarget.width, GrassWaveRenderer.RenderTarget.height);
			}
			PeGrassSystem.SetWaveCenter(zero);
		}
	}
}
