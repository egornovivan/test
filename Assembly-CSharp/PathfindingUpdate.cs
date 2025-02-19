using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class PathfindingUpdate : MonoBehaviour
{
	private Bounds _b45Bound = default(Bounds);

	private List<Bounds> _chunkBounds;

	private ProceduralGridMover _mover;

	private static readonly Vector3[] _ofsParts = new Vector3[4]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(1f, 0f, 0f) * 16f,
		new Vector3(0f, 0f, 1f) * 16f,
		new Vector3(1f, 0f, 1f) * 16f
	};

	private static readonly Vector3 _sizeParts = new Vector3(1f, 1f, 1f) * 16f;

	private void Awake()
	{
		_chunkBounds = new List<Bounds>();
		_mover = GetComponent<ProceduralGridMover>();
		if (Block45Man.self != null)
		{
			Block45Man.self.AttachEvents(OnBlock45ColliderBuild);
		}
		VFVoxelChunkGo.CreateChunkColliderEvent += OnChunkColliderCreated;
		VFVoxelChunkGo.RebuildChunkColliderEvent += OnChunkColliderRebuild;
		LSubTerrainMgr.OnTreeColliderCreated = (Action<GameObject>)Delegate.Combine(LSubTerrainMgr.OnTreeColliderCreated, new Action<GameObject>(OnTreeColliderCreated));
		LSubTerrainMgr.OnTreeColliderDestroy = (Action<GameObject>)Delegate.Combine(LSubTerrainMgr.OnTreeColliderDestroy, new Action<GameObject>(OnTreeColliderDestroy));
		StartCoroutine(UpdateAstarPathChunk());
		StartCoroutine(UpdateAstarPathBlock45());
	}

	private void OnDestroy()
	{
		VFVoxelChunkGo.CreateChunkColliderEvent -= OnChunkColliderCreated;
		VFVoxelChunkGo.RebuildChunkColliderEvent -= OnChunkColliderRebuild;
		if (Block45Man.self != null)
		{
			Block45Man.self.DetachEvents(OnBlock45ColliderBuild);
		}
		LSubTerrainMgr.OnTreeColliderCreated = (Action<GameObject>)Delegate.Remove(LSubTerrainMgr.OnTreeColliderCreated, new Action<GameObject>(OnTreeColliderCreated));
		LSubTerrainMgr.OnTreeColliderDestroy = (Action<GameObject>)Delegate.Remove(LSubTerrainMgr.OnTreeColliderDestroy, new Action<GameObject>(OnTreeColliderDestroy));
	}

	private void AddChunk(Bounds bound)
	{
		if (!Contains(bound))
		{
			_chunkBounds.Add(bound);
		}
	}

	private bool Contains(Bounds bound)
	{
		bool result = false;
		Vector3 min = bound.min;
		int num = Mathf.RoundToInt(min.x);
		int num2 = Mathf.RoundToInt(min.z);
		int count = _chunkBounds.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 min2 = _chunkBounds[i].min;
			if (Mathf.RoundToInt(min2.x) == num && Mathf.RoundToInt(min2.z) == num2)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void OnBlock45ColliderBuild(Block45ChunkGo vfGo)
	{
		if (vfGo != null && vfGo._mc != null)
		{
			if (_b45Bound.size != Vector3.zero)
			{
				_b45Bound.Encapsulate(vfGo._mc.bounds);
			}
			else
			{
				_b45Bound = vfGo._mc.bounds;
			}
		}
	}

	private void OnChunkColliderCreated(VFVoxelChunkGo chunk)
	{
		for (int i = 0; i < 4; i++)
		{
			Bounds bound = default(Bounds);
			Vector3 vector = chunk.transform.position + _ofsParts[i];
			bound.SetMinMax(vector, vector + _sizeParts);
			AddChunk(bound);
		}
	}

	private void OnChunkColliderRebuild(VFVoxelChunkGo chunk)
	{
		Vector3 position = chunk.transform.position;
		Vector3 max = chunk.transform.position + Vector3.one * 32f;
		Bounds bound = default(Bounds);
		bound.SetMinMax(position, max);
		AddChunk(bound);
	}

	private void OnTreeColliderCreated(GameObject obj)
	{
		Collider collider = ((!(obj != null)) ? null : obj.GetComponent<Collider>());
		if (collider != null)
		{
			AstarPath.active.UpdateGraphs(collider.bounds);
		}
	}

	private void OnTreeColliderDestroy(GameObject obj)
	{
		Collider collider = ((!(obj != null)) ? null : obj.GetComponent<Collider>());
		if (collider != null)
		{
			StartCoroutine(DelayUpdateGraph(collider.bounds, 0.5f));
		}
	}

	private IEnumerator DelayUpdateGraph(Bounds bounds, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		AstarPath.active.UpdateGraphs(bounds);
	}

	private IEnumerator UpdateAstarPathBlock45()
	{
		while (true)
		{
			if (_b45Bound.size != Vector3.zero)
			{
				AstarPath.active.UpdateGraphs(_b45Bound);
				_b45Bound = default(Bounds);
			}
			yield return new WaitForSeconds(2f);
		}
	}

	private IEnumerator UpdateAstarPathChunk()
	{
		while (true)
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer != null && PeSingleton<PeCreature>.Instance.mainPlayer.hasView)
			{
				if (_mover != null && _mover.target == null)
				{
					_mover.target = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.trans;
				}
				for (int i = _chunkBounds.Count - 1; i >= 0; i--)
				{
					AstarPath.active.UpdateGraphs(_chunkBounds[i]);
					_chunkBounds.RemoveAt(i);
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}
}
