using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class PEVision : PEPerception
{
	public float radius;

	public float angle;

	public Vector3 axis = Vector3.forward;

	private List<PeEntity> m_Entities;

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
		Vision();
	}

	private bool IsBlockEye(PeEntity entity)
	{
		if (entity == null)
		{
			return true;
		}
		Ray ray = new Ray(base.transform.position, entity.centerPos - base.transform.position);
		float maxDistance = Vector3.Distance(base.transform.position, entity.centerPos);
		return Physics.Raycast(ray, maxDistance, blockLayer);
	}

	private bool InSight(PeEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		Vector3 to = entity.centerPos - base.transform.position;
		Vector3 from = base.transform.TransformDirection(axis);
		return Vector3.Angle(from, to) < angle;
	}

	public void Vision()
	{
		if (m_FrameCount % 20 != 0L)
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
				float num2 = PEUtil.SqrMagnitudeH(peEntity.position, position);
				if (num2 <= num * num && InSight(peEntity) && !IsBlockEye(peEntity) && !peEntity.IsSnake)
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
