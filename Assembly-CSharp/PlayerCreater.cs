using CustomCharactor;
using Pathea;
using UnityEngine;

public class PlayerCreater : MonoBehaviour
{
	private void Start()
	{
		PeSingleton<MainPlayer>.Instance.CreatePlayer(PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId(), 2f * Vector3.up, Quaternion.identity, Vector3.one, CustomDataMgr.Instance.Current);
		Object.Destroy(base.gameObject);
	}
}
