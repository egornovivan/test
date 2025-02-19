using UnityEngine;

public class PEIsoCapture : MonoBehaviour
{
	public Camera captureCam;

	public RenderTexture photoRT;

	public BSB45Computer Computer;

	private bool m_Capture;

	private Transform m_MeshRoot;

	public void EnableCapture()
	{
		captureCam.gameObject.SetActive(value: true);
		m_Capture = true;
	}

	public void DisableCapture()
	{
		captureCam.gameObject.SetActive(value: false);
		m_Capture = false;
		for (int i = 0; i < Computer.transform.childCount; i++)
		{
			Object.Destroy(Computer.transform.GetChild(i).gameObject);
		}
	}

	private void Awake()
	{
		photoRT = new RenderTexture(64, 64, 8, RenderTextureFormat.ARGB32);
		captureCam.targetTexture = photoRT;
		m_MeshRoot = Computer.transform;
	}

	private void Update()
	{
		for (int i = 0; i < m_MeshRoot.childCount; i++)
		{
			GameObject gameObject = m_MeshRoot.GetChild(i).gameObject;
			gameObject.layer = m_MeshRoot.gameObject.layer;
		}
	}

	private void LateUpdate()
	{
		Camera main = Camera.main;
		if (!(main == null) && m_Capture)
		{
			captureCam.transform.position = main.transform.position;
			captureCam.transform.rotation = main.transform.rotation;
		}
	}

	private void OnDestroy()
	{
		if (photoRT != null)
		{
			photoRT.Release();
		}
	}
}
