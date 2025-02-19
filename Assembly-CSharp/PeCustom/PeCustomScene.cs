using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace PeCustom;

public class PeCustomScene : GLBehaviour
{
	public class SceneElement
	{
		public PeCustomScene scene => Self;

		public SceneElement()
		{
			if (this is IMonoLike ml)
			{
				scene.AddMonoLikeItems(ml);
			}
		}
	}

	private static PeCustomScene _self;

	private SpawnDataSource mSpawnData;

	private List<ISceneController> mControllers = new List<ISceneController>(5);

	private List<IMonoLike> mMlItems;

	public static PeCustomScene Self
	{
		get
		{
			if (_self == null)
			{
				GameObject gameObject = Resources.Load("Prefab/Custom/PeCustomScene") as GameObject;
				if (gameObject == null)
				{
					Debug.LogError("The Custom Scene manager load Failed");
					return null;
				}
				Object.Instantiate(gameObject);
			}
			return _self;
		}
	}

	public PeScenario scenario { get; private set; }

	public SpawnDataSource spawnData => mSpawnData;

	private void AddMonoLikeItems(IMonoLike ml)
	{
		mMlItems.Add(ml);
	}

	public void SceneRestore(YirdData yird)
	{
		mSpawnData.Restore(yird);
	}

	public void SceneNew(YirdData yird)
	{
		mSpawnData.New(yird);
	}

	public void ScenarioInit(CustomGameData data)
	{
		scenario.Init(data.missionDir, data.curPlayer.ID);
	}

	public void ScenarioRestore()
	{
		scenario.Restore();
	}

	public void Notify(ESceneNoification msg_type, params object[] data)
	{
		for (int i = 0; i < mControllers.Count; i++)
		{
			mControllers[i].OnNotification(msg_type, data);
		}
	}

	public void CreateAgent(SpawnPoint sp)
	{
		Notify(ESceneNoification.CreateAgent, sp);
	}

	public void RemoveSpawnPoint(SpawnPoint sp)
	{
		Notify(ESceneNoification.RemoveSpawnPoint, sp);
	}

	public void EnableSpawnPoint(SpawnPoint sp, bool enable)
	{
		Notify(ESceneNoification.EnableSpawnPoint, sp, enable);
	}

	public void MonsterDeadEvent(SceneEntityAgent agent, MonsterSpawnPoint msp)
	{
	}

	private void OnGUI()
	{
		for (int i = 0; i < mMlItems.Count; i++)
		{
			mMlItems[i].OnGUI();
		}
	}

	private void Awake()
	{
		_self = this;
		mMlItems = new List<IMonoLike>(10);
		mSpawnData = new SpawnDataSource();
		SceneAgentsContoller sceneAgentsContoller = new SceneAgentsContoller();
		sceneAgentsContoller.Binder.Bind(mSpawnData);
		mControllers.Add(sceneAgentsContoller);
		scenario = new PeScenario();
		sceneAgentsContoller.Binder.Bind(mSpawnData);
		mControllers.Add(scenario);
	}

	private void Start()
	{
		for (int i = 0; i < mMlItems.Count; i++)
		{
			mMlItems[i].Start();
		}
	}

	private void Update()
	{
		for (int i = 0; i < mMlItems.Count; i++)
		{
			mMlItems[i].Update();
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < mMlItems.Count; i++)
		{
			mMlItems[i].OnDestroy();
		}
	}

	public override void OnGL()
	{
	}
}
