using UnityEngine;

public class TestUnityObject : MonoBehaviour
{
	[SerializeField]
	private bool m_create;

	[SerializeField]
	private bool m_clean;

	private void Clean()
	{
		if (m_clean)
		{
			m_clean = false;
			Resources.UnloadUnusedAssets();
		}
	}

	private void Create()
	{
		if (m_create)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = new Vector3[10000];
		}
	}

	private void Update()
	{
		Create();
		Clean();
	}
}
