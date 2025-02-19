using System.Collections.Generic;
using Pathea;
using PETools;
using RootMotion.FinalIK;
using UnityEngine;

public class IKCombat : MonoBehaviour
{
	private class IKData
	{
		public CCDIK ik;

		public IKCombatLimit limit;

		public Transform target;

		public string curve;

		public bool isPos;
	}

	private PeEntity m_Entity;

	private Animator m_Animator;

	private TargetCmpt m_Scanner;

	private List<IKData> m_IKDataList;

	private void Start()
	{
		m_Entity = PEUtil.GetComponent<PeEntity>(base.gameObject);
		m_Animator = GetComponent<Animator>();
		m_Scanner = GetComponent<TargetCmpt>();
		m_IKDataList = new List<IKData>();
	}

	private void Update()
	{
		foreach (IKData iKData in m_IKDataList)
		{
			if (!iKData.isPos)
			{
				iKData.isPos = true;
				iKData.ik.solver.SetIKPosition(GetIKPosition(iKData));
			}
			iKData.ik.solver.SetIKPositionWeight(m_Animator.GetFloat(iKData.curve));
		}
	}

	private Vector3 GetIKPosition(IKData ikData)
	{
		Vector3 vector = ikData.target.position - ikData.ik.transform.position;
		Vector3 pivot = ikData.limit.pivot;
		float num = Vector3.Angle(pivot, vector);
		if (num > ikData.limit.limit)
		{
			Vector3 axis = Vector3.Cross(pivot, vector);
			Vector3 vector2 = Quaternion.AngleAxis(ikData.limit.limit, axis) * pivot;
			return ikData.limit.transform.position + vector2.normalized * ikData.limit.distance;
		}
		return ikData.target.position;
	}

	private Transform GetIKTransform()
	{
		if (m_Scanner != null)
		{
			Enemy attackEnemy = m_Scanner.GetAttackEnemy();
			if (!Enemy.IsNullOrInvalid(attackEnemy))
			{
				return attackEnemy.CenterBone;
			}
		}
		return null;
	}

	private void ActivateCombatIK(string data)
	{
		string[] array = PEUtil.ToArrayString(data, '|');
		string[] array2 = PEUtil.ToArrayString(array[1], ',');
		string[] array3 = array2;
		foreach (string boneName in array3)
		{
			Transform child = PEUtil.GetChild(base.transform, boneName);
			if (!(child != null))
			{
				continue;
			}
			CCDIK ik = child.GetComponent<CCDIK>();
			if (ik != null)
			{
				IKData iKData = m_IKDataList.Find((IKData ret) => ret.ik == ik);
				if (iKData == null)
				{
					IKData iKData2 = new IKData();
					iKData2.ik = ik;
					iKData2.limit = iKData2.ik.GetComponent<IKCombatLimit>();
					iKData2.target = GetIKTransform();
					iKData2.curve = array[0];
					m_IKDataList.Add(iKData2);
				}
			}
		}
	}

	private void DeactivateCombatIK(string data)
	{
		string[] array = PEUtil.ToArrayString(data, '|');
		string[] array2 = PEUtil.ToArrayString(array[1], ',');
		string[] array3 = array2;
		foreach (string boneName in array3)
		{
			Transform child = PEUtil.GetChild(base.transform, boneName);
			if (!(child != null))
			{
				continue;
			}
			CCDIK ik = child.GetComponent<CCDIK>();
			if (ik != null)
			{
				IKData iKData = m_IKDataList.Find((IKData ret) => ret.ik == ik);
				if (iKData != null)
				{
					iKData.ik.solver.target = null;
					m_IKDataList.Remove(iKData);
				}
			}
		}
	}
}
