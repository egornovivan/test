using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class TRRaycast : Trajectory
{
	public float range;

	private Transform emit;

	private LineRenderer[] m_LineRenderers;

	private List<Vector3> m_HitPositions = new List<Vector3>();

	private void Start()
	{
		Emit(m_Emitter);
		m_LineRenderers = GetComponentsInChildren<LineRenderer>();
	}

	public void Emit(Transform emit)
	{
		this.emit = emit;
	}

	public override Vector3 Track(float deltaTime)
	{
		if (emit == null)
		{
			return Vector3.zero;
		}
		RaycastHit[] array = Physics.RaycastAll(emit.position, emit.forward, range, GameConfig.ProjectileDamageLayer);
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			Collider collider = raycastHit.collider;
			if (collider == null || collider.tag == "WorldCollider" || collider.transform.IsChildOf(base.transform) || (m_Emitter != null && collider.transform.IsChildOf(m_Emitter)))
			{
				continue;
			}
			PEDefenceTrigger component = collider.GetComponent<PEDefenceTrigger>();
			if (!(null == component) || !collider.isTrigger)
			{
				if (null == component || !component.RayCast(new Ray(emit.position, emit.forward), range, out var result) || result.distance < raycastHit.distance)
				{
					m_HitPositions.Add(raycastHit.point);
				}
				else
				{
					m_HitPositions.Add(result.hitPos);
				}
			}
		}
		Vector3 vector = emit.position + emit.forward * range;
		float num = 2f * range * 2f * range;
		for (int j = 0; j < m_HitPositions.Count; j++)
		{
			float sqrMagnitude = (m_HitPositions[j] - emit.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				vector = m_HitPositions[j];
			}
		}
		m_HitPositions.Clear();
		return vector - base.transform.position;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		if (m_Emitter != null)
		{
			return Quaternion.FromToRotation(base.transform.position, m_Emitter.position);
		}
		return base.transform.rotation;
	}

	public void Update()
	{
		for (int i = 0; i < m_LineRenderers.Length; i++)
		{
			if (emit != null && m_LineRenderers[i] != null)
			{
				if (base.isActive)
				{
					m_LineRenderers[i].SetPosition(0, emit.position);
					m_LineRenderers[i].SetPosition(1, base.transform.position);
				}
				else
				{
					m_LineRenderers[i].enabled = false;
				}
			}
		}
	}
}
