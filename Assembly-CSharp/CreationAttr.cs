using System;
using System.Collections.Generic;
using ItemAsset;
using UnityEngine;

[Serializable]
public class CreationAttr
{
	public const float DefaultHeight = 1.3f;

	public Vector3 m_CenterOfMass;

	public Dictionary<int, int> m_Cost;

	public ECreation m_Type;

	public float m_Volume;

	public float m_Weight;

	public float m_Durability;

	public float m_SellPrice;

	public Vector4 m_AtkHeight;

	public float m_Attack;

	public float m_Defense;

	public float m_MuzzleAtkInc;

	public float m_FireSpeed;

	public float m_Accuracy;

	public float m_DragCoef;

	public float m_MaxFuel;

	public List<VolumePoint> m_FluidDisplacement;

	public List<string> m_Errors;

	public List<string> m_Warnings;

	private List<int> changeIDList = new List<int>();

	public CreationAttr()
	{
		m_Cost = new Dictionary<int, int>();
		m_Errors = new List<string>();
		m_Warnings = new List<string>();
	}

	public void CheckCostId()
	{
		changeIDList.Clear();
		foreach (KeyValuePair<int, int> item in m_Cost)
		{
			if (item.Key > 10100001)
			{
				changeIDList.Add(item.Key);
			}
		}
		foreach (int changeID in changeIDList)
		{
			int idFromPin = ItemProto.Mgr.GetIdFromPin(changeID);
			if (idFromPin != -1)
			{
				m_Cost.Add(idFromPin, m_Cost[changeID]);
				m_Cost.Remove(changeID);
			}
		}
	}
}
