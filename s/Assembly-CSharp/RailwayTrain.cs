using System.Collections.Generic;
using ItemAsset;
using Railway;
using UnityEngine;

public class RailwayTrain
{
	private List<int> m_SeatList;

	public Route mRoute;

	private int _setCount = 4;

	private ItemObject _trainItem;

	public Vector3 _positon;

	public Quaternion _rot;

	public RailwayTrain(ItemObject obj, Route route)
	{
		_trainItem = obj;
		m_SeatList = new List<int>();
		for (int i = 0; i < _setCount; i++)
		{
			m_SeatList.Add(0);
		}
	}

	public bool HasPassenger()
	{
		foreach (int seat in m_SeatList)
		{
			if (seat != 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool AddPassenger(int pas)
	{
		if (m_SeatList.Contains(pas))
		{
			return false;
		}
		for (int i = 0; i < m_SeatList.Count; i++)
		{
			if (m_SeatList[i] == 0)
			{
				m_SeatList[i] = pas;
				return true;
			}
		}
		return false;
	}

	public bool RemovePassenger(int pas)
	{
		for (int i = 0; i < m_SeatList.Count; i++)
		{
			if (m_SeatList[i] == pas)
			{
				m_SeatList[i] = 0;
				return true;
			}
		}
		return false;
	}

	public void ClearPassenger()
	{
		m_SeatList.ForEach(delegate(int s)
		{
			s = 0;
		});
	}
}
