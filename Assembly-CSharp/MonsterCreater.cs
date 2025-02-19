using Pathea;
using UnityEngine;

public class MonsterCreater : MonoBehaviour
{
	public enum ProtoType
	{
		Monster,
		Doodad,
		NpcRandom,
		NpcLine
	}

	public int protoID;

	public ProtoType protoType;

	public int colorId = -1;

	private void Start()
	{
		PeEntity peEntity = null;
		if (protoType == ProtoType.Doodad)
		{
			peEntity = DoodadEntityCreator.CreateDoodad(protoID, base.transform.position);
		}
		else if (protoType == ProtoType.NpcRandom)
		{
			peEntity = NpcEntityCreator.CreateNpc(protoID, base.transform.position);
		}
		else if (protoType == ProtoType.NpcLine)
		{
			NpcEntityCreator.CreateStoryLineNpcFromID(protoID, base.transform.position);
		}
		else
		{
			peEntity = MonsterEntityCreator.CreateAdMonster(protoID, base.transform.position, colorId, -1);
		}
		Object.Destroy(base.gameObject);
	}
}
