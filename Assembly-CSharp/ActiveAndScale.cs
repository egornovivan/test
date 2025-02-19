using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActiveAndScale
{
	[Serializable]
	public class Action
	{
		public float startTime;

		public float endTime;

		public float colScaleStart;

		public float colScaleEnd;
	}

	[SerializeField]
	private List<Action> m_Actions;

	private Collider[] m_Cols;

	private Vector3[] m_DefaultScale;

	private Action m_CurrentAttack;

	private float m_Time;

	public void Init(MonoBehaviour mono)
	{
		m_Cols = mono.GetComponentsInChildren<Collider>(includeInactive: true);
		m_DefaultScale = new Vector3[m_Cols.Length];
		for (int i = 0; i < m_Cols.Length; i++)
		{
			ref Vector3 reference = ref m_DefaultScale[i];
			reference = m_Cols[i].transform.localScale;
		}
		ActiveCols(active: false);
	}

	public void UpdateAttackState(float deltaTime)
	{
		m_Time += deltaTime;
		if (m_Actions.Count <= 0 || !(m_Time > m_Actions[0].startTime))
		{
			return;
		}
		if (m_CurrentAttack == null)
		{
			m_CurrentAttack = m_Actions[0];
			ActiveCols(active: true);
		}
		if (m_Time > m_Actions[0].endTime)
		{
			m_CurrentAttack = null;
			if (m_Actions.Count <= 1 || m_Actions[1].startTime > m_Actions[0].endTime)
			{
				ActiveCols(active: false);
			}
			m_Actions.RemoveAt(0);
		}
		if (m_CurrentAttack != null)
		{
			ResetColScale(Mathf.Lerp(m_CurrentAttack.colScaleStart, m_CurrentAttack.colScaleEnd, (m_Time - m_CurrentAttack.startTime) / (m_CurrentAttack.endTime - m_CurrentAttack.startTime)));
		}
	}

	private void ActiveCols(bool active)
	{
		for (int i = 0; i < m_Cols.Length; i++)
		{
			m_Cols[i].gameObject.SetActive(active);
		}
	}

	private void ResetColScale(float scale)
	{
		for (int i = 0; i < m_Cols.Length; i++)
		{
			m_Cols[i].transform.localScale = scale * m_DefaultScale[i];
		}
	}
}
