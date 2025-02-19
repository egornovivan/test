using System.Collections;
using System.Collections.Generic;
using PETools;
using UnityEngine;

namespace Pathea.Projectile;

public class SPGRange : SkProjectileGroup
{
	public float minRange;

	public float maxRange;

	public float minHeight;

	public float maxHeight;

	public float spaceRadius;

	private List<Vector3> m_PointList;

	private bool IsGiveUp(Vector3 pos)
	{
		for (int i = 0; i < m_PointList.Count; i++)
		{
			if (PEUtil.SqrMagnitudeH(pos, m_PointList[i]) < spaceRadius * spaceRadius)
			{
				return true;
			}
		}
		return false;
	}

	private Vector3 GetPosition()
	{
		if (m_PointList == null)
		{
			m_PointList = new List<Vector3>();
		}
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomPositionOnGround = PEUtil.GetRandomPositionOnGround(base.transform.position, minRange, maxRange, minHeight, maxHeight, isResult: false);
			if (randomPositionOnGround != Vector3.zero && !IsGiveUp(randomPositionOnGround))
			{
				return randomPositionOnGround;
			}
		}
		return Vector3.zero;
	}

	private Quaternion GetRotation()
	{
		return Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
	}

	public IEnumerator SpawnProjectile()
	{
		Transform castTrans = GetCasterTrans();
		if (null == castTrans)
		{
			Debug.LogWarning("Can't find caster");
			yield break;
		}
		bool immediately = projectileInterval < float.Epsilon;
		for (int i = 0; i < projectileCount; i++)
		{
			Vector3 pos = GetPosition();
			Quaternion rot = GetRotation();
			if (!(pos == Vector3.zero))
			{
				if (m_Target != null)
				{
					Singleton<ProjectileBuilder>.Instance.Register(projectileID, castTrans, pos, rot, m_Target, i, immediately);
				}
				else if (m_TargetPosition != Vector3.zero)
				{
					Singleton<ProjectileBuilder>.Instance.Register(projectileID, castTrans, pos, rot, m_TargetPosition, i, immediately);
				}
				if (!immediately)
				{
					yield return new WaitForSeconds(projectileInterval);
				}
			}
		}
		Object.Destroy(base.gameObject);
	}

	public override void SetData(ProjectileData data, Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(data, caster, emitter, target, targetPosition, index);
		StartCoroutine(SpawnProjectile());
	}
}
