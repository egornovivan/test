using System.Collections;
using ItemAsset;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	private static PlayerManager self;

	public static PlayerManager Self => self;

	private void Awake()
	{
		self = this;
	}

	private void Start()
	{
		StartCoroutine(IntervalSave3());
		StartCoroutine(IntervalSave30());
		StartCoroutine(IntervalSave180());
		StartCoroutine(IntervalSave300());
		StartCoroutine(GameWorld.AsyncSendBlock());
	}

	private IEnumerator IntervalSave3()
	{
		while (true)
		{
			yield return new WaitForSeconds(3f);
			ItemManager.SaveItems();
		}
	}

	private IEnumerator IntervalSave30()
	{
		while (true)
		{
			yield return new WaitForSeconds(30f);
			yield return StartCoroutine(GameWorld.AsyncSave());
			ServerConfig.SyncSave();
			PublicData.Self.Save();
			DoodadMgr.SaveAll();
			ReputationSystem._self.Save();
		}
	}

	private IEnumerator IntervalSave180()
	{
		while (true)
		{
			yield return new WaitForSeconds(180f);
			MissionManager.Manager.SaveMissions();
		}
	}

	private IEnumerator IntervalSave300()
	{
		while (true)
		{
			yield return new WaitForSeconds(300f);
			ESceneMode sceneMode = ServerConfig.SceneMode;
			if (sceneMode == ESceneMode.Custom)
			{
				CustomGameData.Mgr.SaveCustomDialogs();
				SPTerrainEvent.SaveCustomNpc();
			}
		}
	}
}
