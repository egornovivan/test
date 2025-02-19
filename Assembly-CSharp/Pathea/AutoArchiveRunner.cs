using System.Collections;
using UnityEngine;

namespace Pathea;

public class AutoArchiveRunner : MonoBehaviour
{
	private const float AutoSaveInterval = 300f;

	public static AutoArchiveRunner instance;

	public static void Init()
	{
		if (null == instance)
		{
			instance = new GameObject("AutoArchiveRunner").AddComponent<AutoArchiveRunner>();
		}
	}

	public static void Start()
	{
		instance.StartAutoSave();
	}

	public static void Stop()
	{
		instance.StopAutoSave();
	}

	public static void DestroySelf()
	{
		Object.Destroy(instance.gameObject);
		instance = null;
	}

	private void StartAutoSave()
	{
		StartCoroutine(AutoSave());
	}

	private void StopAutoSave()
	{
		StopAllCoroutines();
	}

	private IEnumerator AutoSave()
	{
		while (true)
		{
			yield return new WaitForSeconds(300f);
			PeSingleton<ArchiveMgr>.Instance.Save(ArchiveMgr.ESave.Min);
			yield return new WaitForSeconds(300f);
			PeSingleton<ArchiveMgr>.Instance.Save(ArchiveMgr.ESave.Auto2);
			yield return new WaitForSeconds(300f);
			PeSingleton<ArchiveMgr>.Instance.Save(ArchiveMgr.ESave.Auto3);
		}
	}

	private void OnApplicationQuit()
	{
		if (PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.GameScene && RandomDungenMgrData.InDungeon && PeGameMgr.IsSingleAdventure)
		{
			RandomDungenMgr.Instance.SaveInDungeon();
			RandomDungenMgr.Instance.DestroyDungeon();
		}
		QuitSave();
	}

	public static void QuitSave()
	{
		if (!(instance == null))
		{
			PeSingleton<ArchiveMgr>.Instance.QuitSave();
		}
	}
}
