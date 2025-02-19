using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class PEHearing : PEPerception
{
	public float radius;

	public bool isIgnoreBlock;

	private List<PeEntity> m_Entities;

	private int layer;

	private ulong m_FrameCount;

	public List<PeEntity> Entities => m_Entities;

	public void AddBuff(float value, float time)
	{
		StartCoroutine(Buff(value, time));
	}

	private void Start()
	{
		m_Entities = new List<PeEntity>();
	}

	private void Update()
	{
		m_FrameCount++;
		Hearing();
	}

	private bool IsBlockEye(PeEntity entity)
	{
		if (entity == null)
		{
			return true;
		}
		if (isIgnoreBlock)
		{
			return false;
		}
		Ray ray = new Ray(base.transform.position, entity.centerPos - base.transform.position);
		float maxDistance = Vector3.Distance(base.transform.position, entity.centerPos);
		return Physics.Raycast(ray, maxDistance, blockLayer);
	}

	private bool IgnoreCollider(Collider col)
	{
		if (col.isTrigger || col.transform.IsChildOf(base.transform))
		{
			return true;
		}
		Vector3 position = base.transform.position;
		Vector3 direction = col.transform.position - position;
		if (Physics.Raycast(base.transform.position, direction, 2193408f))
		{
			return false;
		}
		return m_Entities.Find((PeEntity ret) => ret != null && col.transform.IsChildOf(ret.transform)) != null;
	}

	public void Hearing()
	{
		if ((m_FrameCount & 0xF) != 0L)
		{
			return;
		}
		m_Entities.Clear();
		Vector3 position = base.transform.position;
		List<PeEntity> entitiesWithView = PeSingleton<EntityMgr>.Instance.GetEntitiesWithView();
		int count = entitiesWithView.Count;
		for (int i = 0; i < count; i++)
		{
			PeEntity peEntity = entitiesWithView[i];
			if (peEntity != null && peEntity.hasView)
			{
				float num = radius + peEntity.maxRadius;
				if (PEUtil.SqrMagnitudeH(peEntity.position, position) <= num * num && !IsBlockEye(peEntity))
				{
					m_Entities.Add(peEntity);
				}
			}
		}
	}

	private IEnumerator Buff(float value, float time)
	{
		radius += value;
		yield return new WaitForSeconds(time);
		radius -= value;
	}

	public void OnDrawGizmosSelected()
	{
	}
}
