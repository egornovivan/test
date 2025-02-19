using UnityEngine;

namespace ItemAsset;

public class Drag : Cmpt
{
	private Instantiate mInstantiateGameObj;

	public override void Init()
	{
		base.Init();
		mInstantiateGameObj = itemObj.GetCmpt<Instantiate>();
		if (mInstantiateGameObj == null)
		{
			Debug.LogError("item:" + itemObj.protoId + ", drag need InstantiateGameObj");
		}
	}
}
