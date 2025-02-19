using System;
using System.Collections.Generic;
using UnityEngine;

public class DunItemGenerator : MonoBehaviour
{
	public List<string> boxIdList = new List<string>();

	private Vector3 pos => base.transform.position;

	public void GenItem(List<IdWeight> idWeightList, System.Random rand)
	{
		if (idWeightList != null && idWeightList.Count != 0)
		{
			List<int> list = RandomDunGenUtil.PickIdFromWeightList(rand, idWeightList, 1);
			RandomItemMgr.Instance.TryGenItem(pos, list[0], rand);
		}
	}
}
