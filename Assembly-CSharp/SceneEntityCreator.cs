using System.Collections;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class SceneEntityCreator : MonoBehaviour
{
	public static SceneEntityCreator self;

	private bool _bReadyToRefresh;

	private bool isNeedRefresh;

	private Dictionary<IntVector2, ISceneEntityMissionPoint> _missionEntityPoints = new Dictionary<IntVector2, ISceneEntityMissionPoint>();

	private PeTrans _playerTrans;

	public Transform PlayerTrans
	{
		get
		{
			if (_playerTrans == null)
			{
				_playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
			}
			return _playerTrans.trans;
		}
	}

	private void Awake()
	{
		self = this;
	}

	private void Update()
	{
		if (PeGameMgr.IsSingleBuild || (PeGameMgr.IsSingleAdventure && PeGameMgr.yirdName == AdventureScene.Dungen.ToString()) || SceneMan.self == null || SceneMan.self.Observer == null)
		{
			return;
		}
		isNeedRefresh |= SceneMan.self.CenterMoved;
		if (!_bReadyToRefresh || !isNeedRefresh)
		{
			return;
		}
		isNeedRefresh = false;
		SceneEntityPosRect.EntityPosToRectIdx(SceneMan.self.Observer.position, IntVector2.Tmp);
		int x = IntVector2.Tmp.x;
		int y = IntVector2.Tmp.y;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.FillPosRect(x + i, y + j);
			}
		}
	}

	public void New()
	{
		NpcEntityCreator.Init();
		MonsterEntityCreator.Init();
		DoodadEntityCreator.Init();
		PeSingleton<SceneEntityCreatorArchiver>.Instance.New();
		if (PeGameMgr.IsStory || PeGameMgr.IsTutorial)
		{
			if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
			{
				StartCoroutine(InitTutorialNpc());
			}
			else
			{
				DoodadEntityCreator.CreateStoryDoodads(bNew: true);
				StartCoroutine(InitStoryNpc());
			}
		}
		_bReadyToRefresh = true;
	}

	public void Restore()
	{
		NpcEntityCreator.Init();
		MonsterEntityCreator.Init();
		DoodadEntityCreator.Init();
		PeSingleton<SceneEntityCreatorArchiver>.Instance.Restore();
		if (PeGameMgr.IsStory && PeGameMgr.playerType != PeGameMgr.EPlayerType.Tutorial)
		{
			DoodadEntityCreator.CreateStoryDoodads(bNew: false);
		}
		_bReadyToRefresh = true;
	}

	private IEnumerator InitStoryNpc()
	{
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (PeGameMgr.IsStory)
		{
			NpcEntityCreator.CreateStoryLineNpc();
			NpcEntityCreator.CreateStoryRandNpc();
		}
		yield return 2;
	}

	private IEnumerator InitTutorialNpc()
	{
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		NpcEntityCreator.CreateTutorialLineNpc();
		yield return 2;
	}

	public void AddMissionPoint(int missionId, int targetId, int entityId = -1)
	{
		switch (MissionRepository.GetTargetType(targetId))
		{
		case TargetType.TargetType_TowerDif:
		{
			IntVector2 key2 = new IntVector2(missionId, targetId);
			ISceneEntityMissionPoint sceneEntityMissionPoint2 = new SceneEntityMissionPointTowerDefence(entityId);
			sceneEntityMissionPoint2.MissionId = missionId;
			sceneEntityMissionPoint2.TargetId = targetId;
			if (sceneEntityMissionPoint2.Start())
			{
				_missionEntityPoints[key2] = sceneEntityMissionPoint2;
			}
			break;
		}
		case TargetType.TargetType_KillMonster:
		{
			IntVector2 key = new IntVector2(missionId, targetId);
			ISceneEntityMissionPoint sceneEntityMissionPoint = new SceneEntityMissionPointMonsterKill();
			sceneEntityMissionPoint.MissionId = missionId;
			sceneEntityMissionPoint.TargetId = targetId;
			if (sceneEntityMissionPoint.Start())
			{
				_missionEntityPoints[key] = sceneEntityMissionPoint;
			}
			break;
		}
		}
	}

	public void RemoveMissionPoint(int missionId, int targetId)
	{
		if (targetId < 0)
		{
			List<IntVector2> list = new List<IntVector2>();
			foreach (KeyValuePair<IntVector2, ISceneEntityMissionPoint> missionEntityPoint in _missionEntityPoints)
			{
				if (missionEntityPoint.Key.x == missionId)
				{
					missionEntityPoint.Value.Stop();
					list.Add(missionEntityPoint.Key);
				}
			}
			{
				foreach (IntVector2 item in list)
				{
					_missionEntityPoints.Remove(item);
				}
				return;
			}
		}
		IntVector2 key = new IntVector2(missionId, targetId);
		if (_missionEntityPoints.TryGetValue(key, out var value))
		{
			value.Stop();
			_missionEntityPoints.Remove(key);
		}
	}

	public ISceneEntityMissionPoint GetMissionPoint(int missionId, int targetId)
	{
		IntVector2 key = new IntVector2(missionId, targetId);
		object result;
		if (_missionEntityPoints.ContainsKey(key))
		{
			ISceneEntityMissionPoint sceneEntityMissionPoint = _missionEntityPoints[key];
			result = sceneEntityMissionPoint;
		}
		else
		{
			result = null;
		}
		return (ISceneEntityMissionPoint)result;
	}

	public void SetSpawnPointActive(int id, bool active)
	{
		PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(id, active);
	}
}
