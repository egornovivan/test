using UnityEngine;

namespace Pathea;

public class PeSingletonRunner : MonoBehaviour
{
	private void Update()
	{
		PeSingleton<MonoLikeSingletonMgr>.Instance.Update();
	}

	private void OnDestroy()
	{
		PeSingleton<MonoLikeSingletonMgr>.Instance.OnDestroy();
	}

	public static void Launch()
	{
		new GameObject("PeSingletonRunner").AddComponent<PeSingletonRunner>();
	}
}
