using System;
using CustomCharactor;
using PeCustom;
using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadCustomCreature : ModuleLoader
{
	public LoadCustomCreature(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<PeCreature>.Instance.New();
		PeSingleton<CreatureMgr>.Instance.New();
		PeSingleton<MainPlayer>.Instance.New();
		PeCreature instance = PeSingleton<PeCreature>.Instance;
		instance.destoryEntityEvent = (Action<int>)Delegate.Combine(instance.destoryEntityEvent, new Action<int>(PeSingleton<CreatureMgr>.Instance.OnPeCreatureDestroyEntity));
		Vector3 playerSpawnPos = GetPlayerSpawnPos();
		Debug.Log("player init pos:" + playerSpawnPos);
		PeEntity peEntity = PeSingleton<MainPlayer>.Instance.CreatePlayer(PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId(), playerSpawnPos, Quaternion.identity, Vector3.one, CustomDataMgr.Instance.Current);
		PeTrans peTrans = peEntity.peTrans;
		peTrans.position = playerSpawnPos;
	}

	protected override void Restore()
	{
		PeSingleton<PeCreature>.Instance.Restore();
		PeSingleton<CreatureMgr>.Instance.Restore();
		PeSingleton<MainPlayer>.Instance.Restore();
	}

	private Vector3 GetPlayerSpawnPos()
	{
		return PeSingleton<PlayerSpawnPosProvider>.Instance.GetPos();
	}
}
