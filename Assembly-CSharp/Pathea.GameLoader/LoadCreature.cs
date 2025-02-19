using CustomCharactor;
using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadCreature : ModuleLoader
{
	public LoadCreature(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<PeCreature>.Instance.New();
		PeSingleton<MainPlayer>.Instance.New();
		Vector3 playerSpawnPos = GetPlayerSpawnPos();
		Debug.Log("player init pos:" + playerSpawnPos);
		PeSingleton<MainPlayer>.Instance.CreatePlayer(PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId(), playerSpawnPos, Quaternion.identity, Vector3.one, CustomDataMgr.Instance.Current);
	}

	protected override void Restore()
	{
		PeSingleton<PeCreature>.Instance.Restore();
		PeSingleton<MainPlayer>.Instance.Restore();
	}

	private Vector3 GetPlayerSpawnPos()
	{
		return PeSingleton<PlayerSpawnPosProvider>.Instance.GetPos();
	}
}
