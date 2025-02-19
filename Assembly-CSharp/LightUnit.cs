using PETools;
using UnityEngine;

public class LightUnit : MonoBehaviour
{
	public Light lamp;

	private Transform m_Trans;

	private Vector3 m_Position;

	private Vector3 m_Forward;

	public LightShadows shadowsBak { get; private set; }

	public LightRenderMode renderModeBak { get; private set; }

	private void Start()
	{
		m_Trans = base.transform;
		lamp = GetComponent<Light>();
		shadowsBak = lamp.shadows;
		renderModeBak = lamp.renderMode;
		if (LightMgr.Instance != null)
		{
			LightMgr.Instance.Registerlight(this);
		}
	}

	private void LateUpdate()
	{
		m_Forward = m_Trans.forward;
		m_Position = m_Trans.position;
	}

	private void OnDestroy()
	{
		if (LightMgr.Instance != null)
		{
			LightMgr.Instance.RemoveLight(this);
		}
	}

	public Vector3 GetPositionOutOfLight(Vector3 pos)
	{
		if (lamp == null)
		{
			return Vector3.zero;
		}
		switch (lamp.type)
		{
		case LightType.Spot:
		{
			for (int i = 0; i < 10; i++)
			{
				Vector3 randomPositionOnGround = PEUtil.GetRandomPositionOnGround(m_Position, pos - m_Position, 0.5f * lamp.range, 2f * lamp.range, -90f, 90f);
				if (Vector3.Angle(m_Forward, randomPositionOnGround - m_Position) >= lamp.spotAngle)
				{
					return randomPositionOnGround;
				}
			}
			return PEUtil.GetRandomPositionOnGround(m_Position, pos - m_Position, 0.5f * lamp.range, 2f * lamp.range, -90f, 90f);
		}
		case LightType.Point:
			return PEUtil.GetRandomPositionOnGround(m_Position, pos - m_Position, 1.5f * lamp.range, 3f * lamp.range, -75f, 75f);
		case LightType.Area:
			return Vector3.zero;
		default:
			return Vector3.zero;
		}
	}

	public bool IsInLight(Vector3 point)
	{
		if (lamp == null || !lamp.isActiveAndEnabled)
		{
			return false;
		}
		switch (lamp.type)
		{
		case LightType.Spot:
		{
			float num = PEUtil.SqrMagnitude(m_Position, point);
			float num2 = Vector3.Angle(m_Forward, point - m_Position);
			return num <= lamp.range * lamp.range && num2 <= lamp.spotAngle;
		}
		case LightType.Directional:
			return false;
		case LightType.Point:
			return PEUtil.SqrMagnitude(m_Position, point) <= lamp.range * lamp.range;
		case LightType.Area:
			return false;
		default:
			return false;
		}
	}

	public bool IsInLight(Transform tr)
	{
		if (tr == null)
		{
			return false;
		}
		return IsInLight(tr.position);
	}

	public bool IsInLight(Transform tr, Bounds bounds)
	{
		if (tr == null || bounds.size == Vector3.zero || lamp == null)
		{
			return false;
		}
		if (PEUtil.SqrMagnitude(m_Position, tr.position, is3D: false) > lamp.range * 2f * lamp.range * 2f)
		{
			return false;
		}
		Vector3 center = bounds.center;
		Vector3 position = bounds.center + bounds.extents.y * new Vector3(0f, 1f, 0f);
		Vector3 position2 = bounds.center - bounds.extents.y * new Vector3(0f, 1f, 0f);
		if (IsInLight(tr.TransformPoint(center)))
		{
			return true;
		}
		if (IsInLight(tr.TransformPoint(position)))
		{
			return true;
		}
		if (IsInLight(tr.TransformPoint(position2)))
		{
			return true;
		}
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = bounds.center;
			if ((i & 1) == 0)
			{
				array[i] -= bounds.extents.x * new Vector3(1f, 0f, 0f);
			}
			else
			{
				array[i] += bounds.extents.x * new Vector3(1f, 0f, 0f);
			}
			if ((i & 2) == 0)
			{
				array[i] -= bounds.extents.y * new Vector3(0f, 1f, 0f);
			}
			else
			{
				array[i] += bounds.extents.y * new Vector3(0f, 1f, 0f);
			}
			if ((i & 4) == 0)
			{
				array[i] -= bounds.extents.z * new Vector3(0f, 0f, 1f);
			}
			else
			{
				array[i] += bounds.extents.z * new Vector3(0f, 0f, 1f);
			}
			ref Vector3 reference2 = ref array[i];
			reference2 = tr.TransformPoint(array[i]);
			if (IsInLight(array[i]))
			{
				return true;
			}
		}
		return false;
	}
}
