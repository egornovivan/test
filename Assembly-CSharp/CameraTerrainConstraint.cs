using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraTerrainConstraint : CamConstraint
{
	public LayerMask m_TerrainLayers;

	public float m_CastDistance = 80f;

	public override void Do()
	{
		Transform character = m_Controller.character;
		if (character == null)
		{
			return;
		}
		m_CastDistance = Vector3.Distance(m_TargetCam.transform.position, character.position);
		Vector3 position = m_TargetCam.transform.position;
		Vector3 forward = m_TargetCam.transform.forward;
		Vector3 origin = position + forward * m_CastDistance;
		Ray ray = new Ray(position, forward);
		Ray ray2 = new Ray(origin, -forward);
		Debug.DrawLine(ray.origin, ray.GetPoint(m_CastDistance), Color.yellow);
		float num = Mathf.Tan(m_TargetCam.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * m_TargetCam.nearClipPlane;
		float num2 = num * m_TargetCam.aspect;
		float num3 = Mathf.Sqrt(num * num + num2 * num2 + m_TargetCam.nearClipPlane * m_TargetCam.nearClipPlane);
		List<RaycastHit> list = RaycastAllFix(ray, m_CastDistance, m_TerrainLayers);
		List<RaycastHit> list2 = RaycastAllFix(ray2, m_CastDistance, m_TerrainLayers);
		List<RaycastHit> list3 = new List<RaycastHit>();
		foreach (RaycastHit item2 in list)
		{
			list3.Add(item2);
			Debug.DrawLine(item2.point + Vector3.left * 0.1f, item2.point + Vector3.right * 0.1f, Color.green);
			Debug.DrawLine(item2.point + Vector3.up * 0.1f, item2.point + Vector3.down * 0.1f, Color.green);
			Debug.DrawLine(item2.point + Vector3.forward * 0.1f, item2.point + Vector3.back * 0.1f, Color.green);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			RaycastHit value = list2[i];
			value.distance = m_CastDistance - list2[i].distance;
			list2[i] = value;
			list3.Add(list2[i]);
			Debug.DrawLine(list2[i].point + Vector3.left * 0.1f, list2[i].point + Vector3.right * 0.1f, Color.red);
			Debug.DrawLine(list2[i].point + Vector3.up * 0.1f, list2[i].point + Vector3.down * 0.1f, Color.red);
			Debug.DrawLine(list2[i].point + Vector3.forward * 0.1f, list2[i].point + Vector3.back * 0.1f, Color.red);
		}
		RaycastHit item = default(RaycastHit);
		item.distance = 0f;
		item.point = position;
		item.normal = forward;
		list3.Add(item);
		list3.Sort(RaycastHitCompare);
		bool flag = false;
		do
		{
			flag = false;
			int num4 = 0;
			while (num4 < list3.Count - 1)
			{
				bool flag2 = Vector3.Dot(list3[num4].normal, forward) < 0f;
				bool flag3 = Vector3.Dot(list3[num4 + 1].normal, forward) < 0f;
				float num5 = Mathf.Abs(list3[num4].distance - list3[num4 + 1].distance);
				if ((num5 < num3 && flag2 != flag3) || (flag2 && !flag3))
				{
					flag = true;
					list3.RemoveAt(num4);
					list3.RemoveAt(num4);
				}
				else
				{
					num4++;
				}
			}
		}
		while (flag);
		for (int num6 = list3.Count - 1; num6 >= 0; num6--)
		{
			if (Vector3.Dot(list3[num6].normal, forward) > 0f)
			{
				if (list3[num6].distance > 0.001f)
				{
					m_TargetCam.transform.position = ray.GetPoint(list3[num6].distance + 0.5f);
				}
				break;
			}
		}
	}

	private static int RaycastHitCompare(RaycastHit lhs, RaycastHit rhs)
	{
		return Mathf.RoundToInt((lhs.distance - rhs.distance) * 10000f);
	}

	private List<RaycastHit> RaycastAllFix(Ray ray, float dist, LayerMask lm)
	{
		bool flag = true;
		float num = 0f;
		List<RaycastHit> list = new List<RaycastHit>();
		int num2 = 0;
		while (num < dist && flag && num2++ <= 256)
		{
			Ray ray2 = new Ray(ray.GetPoint(num), ray.direction);
			if (dist - num - 0.01f > 0f)
			{
				flag = Physics.Raycast(ray2, out var hitInfo, dist - num - 0.01f, lm);
				if (flag)
				{
					hitInfo.distance = num + hitInfo.distance;
					list.Add(hitInfo);
					num = hitInfo.distance + 0.01f;
				}
				continue;
			}
			break;
		}
		return list;
	}
}
