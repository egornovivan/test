using Pathea;
using UnityEngine;

namespace PeMap;

public class MaskTileRunner : MonoLikeSingleton<MaskTileRunner>
{
	private bool mUpdateTile;

	private PeTrans mTrans;

	private Vector3 playerPos
	{
		get
		{
			if (mTrans == null)
			{
				mTrans = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PeTrans>();
			}
			return mTrans.position;
		}
	}

	public void StartUpdate()
	{
		mUpdateTile = true;
	}
}
