using UnityEngine;

public class RandomDungeonData
{
	public int id;

	public int level;

	public int dungeonBaseDataId;

	public int seed = -1;

	public Vector3 enterPos = Vector3.zero;

	public Vector3 entrancePos = Vector3.zero;

	public Vector3 revivePos = Vector3.zero;

	public Vector3 genDunPos = Vector3.zero;

	public bool IsTaskDungeon
	{
		get
		{
			if (RandomDungeonDataBase.GetDataFromId(dungeonBaseDataId) == null)
			{
				return false;
			}
			return RandomDungeonDataBase.GetDataFromId(dungeonBaseDataId).IsTaskDungeon;
		}
	}

	public bool IsOpen => genDunPos != Vector3.zero;

	public bool GenFinish => seed != -1 && genDunPos != Vector3.zero;

	public RandomDungeonData()
	{
		id = RandomDunGenMgr.Instance.IdGenerator++;
	}

	public void SetPosByEnterPos(Vector3 entranceP, float dungeonY)
	{
		entrancePos = entranceP;
		enterPos = entrancePos + new Vector3(0f, 1f, 8f);
		revivePos = new Vector3(enterPos.x, dungeonY, enterPos.z);
		genDunPos = revivePos + new Vector3(0f, 0f, 0f);
	}

	public void SetPosByY(float dungeonY, Vector3 enterPos)
	{
		enterPos = enterPos;
		revivePos = new Vector3(enterPos.x, dungeonY, enterPos.z);
		genDunPos = revivePos + new Vector3(0f, 0f, 0f);
	}

	public void SetPosByGenPlayerPos(Vector3 genPlayerPos)
	{
		revivePos = genPlayerPos;
		genDunPos = revivePos + new Vector3(0f, 0f, 0f);
	}

	public void Close()
	{
		seed = -1;
		enterPos = Vector3.zero;
		entrancePos = Vector3.zero;
		revivePos = Vector3.zero;
		genDunPos = Vector3.zero;
	}

	public bool InDungeonPosY(float posY)
	{
		if (revivePos == Vector3.zero)
		{
			return false;
		}
		if (posY < revivePos.y + 100f && posY > revivePos.y - 100f)
		{
			return true;
		}
		return false;
	}
}
