using UnityEngine;

namespace Pathea.GameLoader;

internal abstract class LoadPlayerSpawnPos : ModuleLoader
{
	public LoadPlayerSpawnPos(bool bNew)
		: base(bNew)
	{
	}

	protected override void Restore()
	{
		PeGameSummary peGameSummary = PeSingleton<PeGameSummary.Mgr>.Instance.Get();
		if (peGameSummary != null)
		{
			SetPos(peGameSummary.playerPos);
		}
	}

	protected void SetPos(Vector3 pos)
	{
		PeSingleton<PlayerSpawnPosProvider>.Instance.SetPos(pos);
	}
}
