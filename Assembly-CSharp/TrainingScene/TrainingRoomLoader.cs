using Pathfinding;
using UnityEngine;

namespace TrainingScene;

internal class TrainingRoomLoader
{
	public static void LoadTrainingRoom()
	{
		Object.Instantiate(Resources.Load("TrainingRoomLight"));
		Object.Instantiate(Resources.Load("TrainingRoomExitSign"));
		Object.Instantiate(Resources.Load("TrainingRoomLifts"));
		Object.Instantiate(Resources.Load("EpsilonIndi"));
		Object.Instantiate(Resources.Load("TrainingManager_New"));
		Object.Instantiate(Resources.Load("Prefab/Audio/bg_music_tutorial"));
		Object.Instantiate(Resources.Load<GameObject>("Prefab/Mission/MissionManager"));
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("scene_TrainingRoom"));
		Quaternion identity = Quaternion.identity;
		identity.eulerAngles = new Vector3(0f, -90f, 0f);
		gameObject.transform.rotation = identity;
		gameObject.transform.position = new Vector3(12f, 1.5f, 12f);
		LoadPathfinding(gameObject);
	}

	private static void LoadPathfinding(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (AstarPath.active != null)
		{
			if (AstarPath.active.transform.parent != null)
			{
				Object.Destroy(AstarPath.active.transform.parent.gameObject);
			}
			else
			{
				Object.Destroy(AstarPath.active.gameObject);
			}
		}
		Object.Instantiate(Resources.Load("Prefab/Pathfinder_Tutorial"));
		if (!(AstarPath.active != null))
		{
			return;
		}
		for (int i = 0; i < AstarPath.active.graphs.Length; i++)
		{
			if (AstarPath.active.graphs[i] is LayerGridGraph layerGridGraph)
			{
				layerGridGraph.center = new Vector3(12f, 1.5f, 12f);
			}
		}
		AstarPath.active.Scan();
	}
}
