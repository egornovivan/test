using UnityEngine;

public class VFTransVoxelGo : MonoBehaviour, IRecyclable
{
	public static Transform _defParent;

	public static Material _defMat;

	public static int _defLayer;

	public MeshRenderer _mr;

	public MeshFilter _mf;

	public int _faceMask;

	private void Awake()
	{
		_mf = base.gameObject.AddComponent<MeshFilter>();
		_mr = base.gameObject.AddComponent<MeshRenderer>();
		_mr.sharedMaterial = _defMat;
		_mf.sharedMesh = new Mesh();
		base.transform.parent = _defParent;
		base.gameObject.layer = _defLayer;
		base.name = "trans_";
	}

	public void OnRecycle()
	{
		_mf.mesh.Clear();
		_faceMask = 0;
		base.transform.parent = _defParent;
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		Object.Destroy(_mf.mesh);
	}
}
