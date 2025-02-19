using Pathea;
using UnityEngine;

public class VFVoxelChunkGo : MonoBehaviour, IRecyclable
{
	public delegate void OnChunkColliderCreated(VFVoxelChunkGo chunk);

	public delegate void OnChunkColliderDestroy(VFVoxelChunkGo chunk);

	public delegate void OnChunkColliderRebuild(VFVoxelChunkGo chunk);

	public static Transform DefParent;

	public static Material DefMat;

	public static int DefLayer;

	private int _terType = int.MinValue;

	private bool _bPrimal;

	public MeshFilter Mf;

	public MeshRenderer Mr;

	public MeshCollider Mc;

	public VFVoxelChunkData Data;

	public VFTransVoxelGo TransvoxelGo;

	public VFVoxelChunkGo OriginalChunkGo;

	public bool IsMeshReady => Mf.sharedMesh != null && Mf.sharedMesh.vertexCount > 0;

	public bool IsColliderReady => OriginalChunkGo != null || Mc.sharedMesh != null;

	public bool IsOriginalGo => !Mr.enabled;

	public static event OnChunkColliderCreated CreateChunkColliderEvent;

	public static event OnChunkColliderDestroy DestroyChunkColliderEvent;

	public static event OnChunkColliderRebuild RebuildChunkColliderEvent;

	private void Awake()
	{
		Mf = base.gameObject.AddComponent<MeshFilter>();
		Mr = base.gameObject.AddComponent<MeshRenderer>();
		Mc = base.gameObject.AddComponent<MeshCollider>();
		Mr.sharedMaterial = DefMat;
		Mf.sharedMesh = new Mesh();
		base.gameObject.layer = DefLayer;
		base.transform.parent = DefParent;
		base.name = "chunk_";
	}

	public static VFVoxelChunkGo CreateChunkGo(IVxSurfExtractReq req, Material mat = null, int layer = 0)
	{
		if (req.FillMesh(null) == 0)
		{
			return null;
		}
		VFVoxelChunkGo go = VFGoPool<VFVoxelChunkGo>.GetGo();
		int num = req.FillMesh(go.Mf.sharedMesh);
		go._bPrimal = true;
		if (mat != null)
		{
			go.Mr.sharedMaterial = mat;
		}
		if (layer != 0)
		{
			go.gameObject.layer = layer;
		}
		go.gameObject.SetActive(value: true);
		while (num > 0)
		{
			VFVoxelChunkGo go2 = VFGoPool<VFVoxelChunkGo>.GetGo();
			num = req.FillMesh(go2.Mf.sharedMesh);
			go2.transform.parent = go.transform;
			go2.transform.localScale = Vector3.one;
			go2.transform.localPosition = Vector3.zero;
			if (mat != null)
			{
				go2.Mr.sharedMaterial = mat;
			}
			if (layer != 0)
			{
				go2.gameObject.layer = layer;
			}
			go2.gameObject.SetActive(value: true);
		}
		return go;
	}

	public static VFVoxelChunkGo CreateChunkGo(Mesh sharedMesh)
	{
		VFVoxelChunkGo go = VFGoPool<VFVoxelChunkGo>.GetGo();
		go.Mf.sharedMesh = sharedMesh;
		go._bPrimal = true;
		go.gameObject.SetActive(value: true);
		return go;
	}

	public void SetTransGo(IVxSurfExtractReq req, int faceMask)
	{
		if (TransvoxelGo != null)
		{
			VFGoPool<VFTransVoxelGo>.FreeGo(TransvoxelGo);
			TransvoxelGo = null;
		}
		if (faceMask != 0)
		{
			VFTransVoxelGo go = VFGoPool<VFTransVoxelGo>.GetGo();
			req.FillMesh(go._mf.sharedMesh);
			go._faceMask = faceMask;
			go.transform.parent = base.transform;
			go.transform.localPosition = Vector3.zero;
			go.gameObject.SetActive(value: true);
			TransvoxelGo = go;
		}
	}

	public void OnRecycle()
	{
		base.gameObject.SetActive(value: false);
		if (IsOriginalGo)
		{
			Mr.enabled = true;
		}
		else if (_bPrimal && Mc.sharedMesh != null)
		{
			OnColliderDestroy();
		}
		Mc.sharedMesh = null;
		Mf.sharedMesh.Clear();
		_terType = int.MinValue;
		_bPrimal = false;
		Data = null;
		if (OriginalChunkGo != null)
		{
			Debug.LogError("[VFChunkGo]Free original chunk go" + OriginalChunkGo.name);
			VFGoPool<VFVoxelChunkGo>.FreeGo(OriginalChunkGo);
			OriginalChunkGo = null;
		}
		if (TransvoxelGo != null)
		{
			VFGoPool<VFTransVoxelGo>.FreeGo(TransvoxelGo);
			TransvoxelGo = null;
		}
		Mr.sharedMaterial = DefMat;
		base.gameObject.layer = DefLayer;
		base.transform.parent = DefParent;
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			Transform child = base.transform.GetChild(num);
			VFGoPool<VFVoxelChunkGo>.FreeGo(child.GetComponent<VFVoxelChunkGo>());
		}
	}

	public void OnColliderReady()
	{
		bool flag = OriginalChunkGo == null;
		if (OriginalChunkGo != null)
		{
			VFGoPool<VFVoxelChunkGo>.FreeGo(OriginalChunkGo);
			OriginalChunkGo = null;
			if (VFVoxelChunkGo.RebuildChunkColliderEvent != null)
			{
				try
				{
					VFVoxelChunkGo.RebuildChunkColliderEvent(this);
					return;
				}
				catch
				{
					Debug.LogError("Error in RebuildChunkColliderEvent:" + base.transform.position);
					return;
				}
			}
			return;
		}
		if (Data != null)
		{
			Data.OnGoColliderReady();
		}
		if (VFVoxelChunkGo.CreateChunkColliderEvent == null || !flag)
		{
			return;
		}
		try
		{
			VFVoxelChunkGo.CreateChunkColliderEvent(this);
		}
		catch
		{
			Debug.LogError("Error in CreateChunkColliderEvent:" + base.transform.position);
		}
	}

	public void OnColliderDestroy()
	{
		if (!_bPrimal || VFVoxelChunkGo.DestroyChunkColliderEvent == null || !Mr.enabled)
		{
			return;
		}
		try
		{
			VFVoxelChunkGo.DestroyChunkColliderEvent(this);
		}
		catch
		{
			Debug.LogError("Error in DestroyChunkColliderEvent:" + base.transform.position);
		}
	}

	private void OnDestroy()
	{
		Mc.sharedMesh = null;
		_bPrimal = false;
		Data = null;
		TransvoxelGo = null;
		Object.Destroy(Mf.sharedMesh);
	}

	private void OnWillRenderObject()
	{
		if (!_bPrimal || base.gameObject.layer != VFVoxelWater.s_layer || VFVoxelWater.s_bSeaInSight)
		{
			return;
		}
		if (PeGameMgr.IsStory)
		{
			if (_terType == int.MinValue && Data != null && Data.LOD <= 0)
			{
				_terType = PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(new Vector2(base.transform.position.x, base.transform.position.z));
			}
			if (_terType == 27 || _terType == 28 || _terType == 24)
			{
				VFVoxelWater.s_bSeaInSight = true;
			}
		}
		else
		{
			VFVoxelWater.s_bSeaInSight = true;
		}
	}
}
