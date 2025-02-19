using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class StoryNpc
{
	public class NpcHadItems
	{
		public int _itemId;

		public int _itemNum;
	}

	public int _npcId;

	public int _prototypeNpc;

	public Vector3 _startPoint;

	public Vector3 _trainingPos;

	public RandomNpcDb.NpcMoney npcMoney;

	public int[] _equipments;

	public int[] _npcSkillIds;

	public int _revive;

	public List<NpcHadItems> _items = new List<NpcHadItems>();
}
