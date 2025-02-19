using System;
using System.Collections.Generic;
using UnityEngine;

public class VCEComponentTool : MonoBehaviour
{
	public bool m_IsBrush;

	public bool m_InEditor;

	public GameObject m_ToolGroup;

	public GLComponentBound m_SelBound;

	public Transform m_DrawPivot;

	public Transform m_MassCenter;

	public int m_Phase;

	public List<GameObject> m_ModelPhases;

	[NonSerialized]
	public VCComponentData m_Data;

	public Vector3 WorldMassCenter => (!(m_MassCenter != null)) ? base.transform.position : m_MassCenter.position;

	private void Start()
	{
		if (m_InEditor)
		{
			SetLayer();
			SetPhase();
		}
	}

	private void Update()
	{
		SetPhase();
		if (!m_InEditor)
		{
			UnityEngine.Object.Destroy(m_ToolGroup);
			UnityEngine.Object.Destroy(this);
		}
	}

	public void SetPivotPos(Vector3 vec)
	{
		Vector3 position = m_DrawPivot.position;
		Vector3 position2 = base.transform.position;
		Vector3 vector = vec - position;
		Vector3 position3 = position2 + vector;
		base.transform.position = position3;
	}

	public void SetLayer()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			transform.gameObject.layer = ((!m_InEditor) ? VCConfig.s_ProductLayer : VCConfig.s_EditorLayer);
		}
	}

	public void SetPhase()
	{
		if (m_ModelPhases.Count <= 0)
		{
			return;
		}
		GameObject gameObject = null;
		gameObject = m_ModelPhases[m_Phase % m_ModelPhases.Count];
		foreach (GameObject modelPhase in m_ModelPhases)
		{
			if (modelPhase != null)
			{
				modelPhase.SetActive(modelPhase == gameObject);
				if (!m_InEditor && modelPhase != gameObject)
				{
					UnityEngine.Object.Destroy(modelPhase);
				}
			}
		}
	}

	public void SetPhase(int phase)
	{
		m_Phase = phase;
		SetPhase();
	}
}
