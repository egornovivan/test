using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadTutorialPlayerSpawnPos : LoadPlayerSpawnPos
{
	public LoadTutorialPlayerSpawnPos(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SetPos(new Vector3(27f, 2f, 11f));
	}
}
