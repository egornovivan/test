using UnityEngine;

namespace Pathea;

public class PeGameMgrRunner : MonoBehaviour
{
	private void Awake()
	{
		ClearSingleton();
		PeGameMgr.Run();
		Object.Destroy(base.gameObject);
	}

	private static void ClearSingleton()
	{
		PeSingleton<MonoLikeSingletonMgr>.Instance.Clear();
	}
}
