using System;
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

	public virtual GameObject CreateLogicGameObject(Action<Transform> initTransform)
	{
		if (mInstantiateGameObj == null)
		{
			return null;
		}
		return mInstantiateGameObj.CreateLogicGameObj(initTransform);
	}

	public virtual GameObject CreateViewGameObject(Action<Transform> initTransform)
	{
		if (mInstantiateGameObj == null)
		{
			return null;
		}
		return mInstantiateGameObj.CreateViewGameObj(initTransform);
	}

	public virtual GameObject CreateDraggingGameObject(Action<Transform> initTransform)
	{
		return mInstantiateGameObj.CreateDraggingGameObj(initTransform);
	}
}
