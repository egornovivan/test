using Behave.Runtime;
using UnityEngine;

namespace Pathea;

public class NpcRobotDb
{
	public int mID = 9088;

	public int mFollowID = 9011;

	public string mName = "AavRobot";

	public float mMoveSpeed;

	public MovementField movementField = MovementField.Sky;

	public string robotModelPath = "Prefab/Human/AvaAzniv_robot.prefab";

	public string behaveDataPath = "AiPrefab/EntityData/Npc/Npc_Robot";

	public Vector3 startPos = new Vector3(0f, 0f, 0f);

	public static NpcRobotDb Instance;

	public static void Load()
	{
		Instance = new NpcRobotDb();
		BTResolver.RegisterToCache(Instance.behaveDataPath);
	}
}
