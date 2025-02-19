using System;
using UnityEngine;

public class B45ChunkGo : MonoBehaviour
{
	public B45ChunkData _data;

	public Mesh _mesh;

	public MeshCollider _meshCollider;

	public GameObject _transvoxelGo;

	public event DelegateB45ColliderCreated OnColliderCreated;

	public event DelegateB45ColliderCreated OnColliderDestroy;

	public void AttachEvents(DelegateB45ColliderCreated created = null, DelegateB45ColliderCreated destroy = null)
	{
		this.OnColliderCreated = (DelegateB45ColliderCreated)Delegate.Combine(this.OnColliderCreated, created);
		this.OnColliderDestroy = (DelegateB45ColliderCreated)Delegate.Combine(this.OnColliderDestroy, destroy);
	}

	public void DetachEvents(DelegateB45ColliderCreated created = null, DelegateB45ColliderCreated destroy = null)
	{
		this.OnColliderCreated = (DelegateB45ColliderCreated)Delegate.Remove(this.OnColliderCreated, created);
		this.OnColliderDestroy = (DelegateB45ColliderCreated)Delegate.Remove(this.OnColliderDestroy, destroy);
	}

	private void HandleColliderCreatedEvent()
	{
		if (this.OnColliderCreated != null && _mesh.vertexCount > 0)
		{
			this.OnColliderCreated(this);
		}
	}

	private void HandleColliderDestroyEvent()
	{
		if (this.OnColliderDestroy != null && _mesh.vertexCount > 0)
		{
			this.OnColliderDestroy(this);
		}
	}

	public void SetCollider()
	{
		_meshCollider.sharedMesh = null;
		_meshCollider.sharedMesh = _mesh;
		HandleColliderCreatedEvent();
	}

	public void Destroy()
	{
		HandleColliderDestroyEvent();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		if (_data != null)
		{
			_data._maskTrans = 0;
		}
		UnityEngine.Object.Destroy(_mesh);
		_mesh = null;
		_transvoxelGo = null;
		foreach (Transform item in base.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}
}
