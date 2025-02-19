using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class MultiStorySceneObjectManager : MonoBehaviour
{
	public class NumberOfScnene
	{
		public List<int> playerIDs;

		public SceneBasicObjAgent _obj;

		public Light _directionLight;

		public NumberOfScnene(SingleGameStory.StoryScene scene, SceneBasicObjAgent obj)
		{
			playerIDs = new List<int>(2);
			_obj = obj;
			switch (scene)
			{
			case SingleGameStory.StoryScene.PajaShip:
			{
				PajaShipMgr component2 = obj.Go.GetComponent<PajaShipMgr>();
				if (component2 != null)
				{
					_directionLight = component2.directionLight;
				}
				break;
			}
			case SingleGameStory.StoryScene.DienShip0:
			case SingleGameStory.StoryScene.DienShip1:
			case SingleGameStory.StoryScene.DienShip2:
			case SingleGameStory.StoryScene.DienShip3:
			case SingleGameStory.StoryScene.DienShip4:
			case SingleGameStory.StoryScene.DienShip5:
			{
				DienManager component = obj.Go.GetComponent<DienManager>();
				if (component != null)
				{
					_directionLight = component.directionLight;
				}
				break;
			}
			}
			RefreshState();
		}

		public void RefreshState()
		{
			if (playerIDs.Count == 0)
			{
				_obj.Go.SetActive(value: false);
				return;
			}
			_obj.Go.SetActive(value: true);
			if ((bool)_directionLight)
			{
				_directionLight.enabled = playerIDs.Contains(PlayerNetwork.mainPlayerId);
			}
		}
	}

	private static MultiStorySceneObjectManager _instance;

	private Dictionary<int, NumberOfScnene> _objects;

	public static MultiStorySceneObjectManager instance => _instance;

	private void Awake()
	{
		if (PeGameMgr.IsMultiStory)
		{
			_instance = this;
			LoadMultiStoryScenePrafeb();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void LoadMultiStoryScenePrafeb()
	{
		if (PeGameMgr.IsMultiStory)
		{
			_objects = new Dictionary<int, NumberOfScnene>();
			SingleGameStory.StoryScene storyScene = SingleGameStory.StoryScene.DienShip0;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/DienShip", string.Empty, new Vector3(14798f, 3f, 8344f), Quaternion.identity, Vector3.one)));
			storyScene = SingleGameStory.StoryScene.DienShip1;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/DienShip", string.Empty, new Vector3(16545.25f, 3.93f, 10645.7f), Quaternion.identity, Vector3.one)));
			storyScene = SingleGameStory.StoryScene.DienShip2;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/DienShip", string.Empty, new Vector3(2876f, 265.6f, 9750.3f), Quaternion.identity, Vector3.one)));
			storyScene = SingleGameStory.StoryScene.DienShip3;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/DienShip", string.Empty, new Vector3(13765.5f, 75.7f, 15242.7f), Quaternion.identity, Vector3.one)));
			storyScene = SingleGameStory.StoryScene.DienShip4;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/DienShip", string.Empty, new Vector3(12547.7f, 523.7f, 13485.5f), Quaternion.identity, Vector3.one)));
			storyScene = SingleGameStory.StoryScene.DienShip5;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/DienShip", string.Empty, new Vector3(7750.4f, 349.7f, 14712.8f), Quaternion.identity, Vector3.one)));
			storyScene = SingleGameStory.StoryScene.L1Ship;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/old_scene_boatinside", string.Empty, new Vector3(9661f, 88.8f, 12758f), Quaternion.identity, Vector3.one)));
			Quaternion identity = Quaternion.identity;
			identity.eulerAngles = new Vector3(352f, 55f, 0f);
			storyScene = SingleGameStory.StoryScene.PajaShip;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/paja_port_shipinside", string.Empty, new Vector3(1471f, -398.7f, 7928.3f), identity, Vector3.one)));
			identity.eulerAngles = new Vector3(0f, 180f, 0f);
			storyScene = SingleGameStory.StoryScene.LaunchCenter;
			_objects.Add((int)storyScene, new NumberOfScnene(storyScene, new SceneBasicObjAgent("Prefab/Other/paja_launch_center", string.Empty, new Vector3(1713f, -360f, 10402f), identity, Vector3.one)));
		}
	}

	public void RequestChangeScene(int playerID, int sceneID)
	{
		if (!PeGameMgr.IsMultiStory || _objects == null)
		{
			return;
		}
		foreach (KeyValuePair<int, NumberOfScnene> @object in _objects)
		{
			if (@object.Value.playerIDs.Contains(playerID))
			{
				@object.Value.playerIDs.Remove(playerID);
				@object.Value.RefreshState();
			}
			if (@object.Key == sceneID && !@object.Value.playerIDs.Contains(playerID))
			{
				@object.Value.playerIDs.Add(playerID);
				@object.Value.RefreshState();
			}
		}
	}
}
