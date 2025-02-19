using UnityEngine;

public class OutLinesMesh : MonoBehaviour
{
	public Material mat;

	private void Start()
	{
	}

	private void Update()
	{
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		if (!(component == null))
		{
			Graphics.DrawMesh(component.mesh, base.transform.position, base.transform.rotation, mat, 0);
		}
	}
}
