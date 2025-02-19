using System;
using System.Collections.Generic;
using UnityEngine;

public class Block45ChunkGo : MonoBehaviour, IRecyclable
{
	public static Transform _defParent;

	public static int _defLayer;

	public static Material[] _defMats;

	public MeshFilter _mf;

	public MeshRenderer _mr;

	public MeshCollider _mc;

	public Block45OctNode _data;

	public bool IsMeshReady => _mf.sharedMesh != null && _mf.sharedMesh.vertexCount > 0;

	public bool IsColliderReady => _mc.sharedMesh != null;

	private void Awake()
	{
		_mf = base.gameObject.AddComponent<MeshFilter>();
		_mr = base.gameObject.AddComponent<MeshRenderer>();
		_mc = base.gameObject.AddComponent<MeshCollider>();
		base.gameObject.layer = _defLayer;
		base.transform.parent = _defParent;
		base.name = "b45Chnk_";
	}

	public static Block45ChunkGo CreateChunkGo(IVxSurfExtractReq req, Transform parent = null)
	{
		if (req.FillMesh(null) == 0)
		{
			return null;
		}
		Block45ChunkGo go = VFGoPool<Block45ChunkGo>.GetGo();
		req.FillMesh(go._mf.mesh);
		if (req is SurfExtractReqB45 surfExtractReqB)
		{
			List<Material> list = new List<Material>();
			for (int i = 0; i < surfExtractReqB.matCnt; i++)
			{
				list.Add(_defMats[surfExtractReqB.materialMap[i]]);
			}
			go._mr.sharedMaterials = list.ToArray();
		}
		if (parent != null)
		{
			go.transform.parent = parent;
		}
		go.gameObject.SetActive(value: true);
		return go;
	}

	public void OnSetCollider()
	{
		_mc.sharedMesh = null;
		_mc.sharedMesh = _mf.sharedMesh;
		try
		{
			Block45Man.self.OnBlock45ColCreated(this);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception OnBlock45ColCreated:" + ex);
		}
	}

	public void OnRecycle()
	{
		base.gameObject.SetActive(value: false);
		if (_mc.sharedMesh != null)
		{
			try
			{
				Block45Man.self.OnBlock45ColDestroy(this);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception OnBlock45ColDestroy:" + ex);
			}
		}
		UnityEngine.Object.DestroyImmediate(_mf.sharedMesh);
		base.name = "b45Chnk_";
		if (base.transform.parent != _defParent)
		{
			base.gameObject.layer = _defLayer;
			base.transform.parent = _defParent;
		}
		_data = null;
	}
}
