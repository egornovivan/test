using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class PEBuilding : MonoBehaviour
{
	public Transform[] mTrans;

	public GameObject[] mObjs;

	public Transform[] mDoorPos;

	private BuildingMap m_Buildingmap;

	public CampFoodTimer mFoodTimer;

	private void Start()
	{
		m_Buildingmap = new BuildingMap();
		m_Buildingmap.LoadIn(mTrans);
	}

	private void Update()
	{
	}

	public Transform Occupy(int entityId)
	{
		if (m_Buildingmap == null)
		{
			m_Buildingmap = new BuildingMap();
			m_Buildingmap.LoadIn(mTrans);
		}
		return m_Buildingmap.OccupyTran(entityId);
	}

	public void Release(int entityId)
	{
		m_Buildingmap.ReleaseTran(entityId);
	}

	public bool SetFoodShowSlots(List<CheckSlot> slots)
	{
		if (mFoodTimer == null)
		{
			return false;
		}
		mFoodTimer.SetSlots(slots);
		return true;
	}
}
