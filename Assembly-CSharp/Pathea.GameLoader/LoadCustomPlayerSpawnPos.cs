using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadCustomPlayerSpawnPos : LoadPlayerSpawnPos
{
	private Vector3 mPos;

	public LoadCustomPlayerSpawnPos(bool bNew, Vector3 pos)
		: base(bNew)
	{
		mPos = pos;
	}

	protected override void New()
	{
		SetPos(mPos);
	}
}
