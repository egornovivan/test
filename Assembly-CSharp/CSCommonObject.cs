using System.Collections.Generic;
using UnityEngine;

public class CSCommonObject : CSEntityObject
{
	public LocateCubeEffectHanlder m_LocEffectPrefab;

	private Dictionary<int, LocateCubeEffectHanlder> m_LocEffects = new Dictionary<int, LocateCubeEffectHanlder>();

	public void ShowWorkSpaceEffect()
	{
		if (m_LocEffectPrefab == null)
		{
			return;
		}
		if (m_Entity != null)
		{
			CSCommon cSCommon = m_Entity as CSCommon;
			if (cSCommon.WorkPoints == null)
			{
				return;
			}
			for (int i = 0; i < cSCommon.WorkPoints.works.Length; i++)
			{
				if (cSCommon.WorkPoints.works[i] != null && !m_LocEffects.ContainsKey(i))
				{
					LocateCubeEffectHanlder locateCubeEffectHanlder = Object.Instantiate(m_LocEffectPrefab);
					locateCubeEffectHanlder.transform.parent = base.transform;
					Vector3 position = cSCommon.WorkPoints.works[i].transform.position;
					position.y += locateCubeEffectHanlder.m_CubeLen * 0.5f * locateCubeEffectHanlder.m_MaxHeightScale + 0.05f;
					locateCubeEffectHanlder.transform.position = position;
					locateCubeEffectHanlder.transform.localRotation = Quaternion.identity;
					m_LocEffects.Add(i, locateCubeEffectHanlder);
				}
			}
			return;
		}
		for (int j = 0; j < m_WorkTrans.Length; j++)
		{
			if (!m_LocEffects.ContainsKey(j))
			{
				LocateCubeEffectHanlder locateCubeEffectHanlder2 = Object.Instantiate(m_LocEffectPrefab);
				locateCubeEffectHanlder2.transform.parent = base.transform;
				Vector3 position2 = m_WorkTrans[j].position;
				position2.y += locateCubeEffectHanlder2.m_CubeLen * 0.5f * locateCubeEffectHanlder2.m_MaxHeightScale + 0.05f;
				locateCubeEffectHanlder2.transform.position = position2;
				locateCubeEffectHanlder2.transform.localRotation = Quaternion.identity;
				m_LocEffects.Add(j, locateCubeEffectHanlder2);
			}
		}
	}

	public void HideWorkSpaceEffect()
	{
		if (m_LocEffectPrefab == null)
		{
			return;
		}
		foreach (LocateCubeEffectHanlder value in m_LocEffects.Values)
		{
			if (value != null)
			{
				Object.Destroy(value.gameObject);
			}
		}
		m_LocEffects.Clear();
	}

	protected new void Start()
	{
		base.Start();
	}

	protected new void Update()
	{
		base.Update();
	}
}
