using UnityEngine;

public class TransvoxelGo : MonoBehaviour
{
	public Mesh _mesh;

	private void OnDestroy()
	{
		Object.Destroy(_mesh);
		_mesh = null;
	}
}
