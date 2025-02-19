using System;
using UnityEngine;

namespace ItemAsset;

public class InstantiateCreation : Instantiate
{
	public override GameObject CreateViewGameObj(Action<Transform> initTransform)
	{
		return CreationMgr.InstantiateCreation(itemObj.protoId, itemObj.instanceId, init: false, initTransform);
	}

	public override GameObject CreateLogicGameObj(Action<Transform> initTransform)
	{
		return CreationMgr.InstantiateCreation(itemObj.protoId, itemObj.instanceId, init: true, initTransform);
	}

	public override GameObject CreateDraggingGameObj(Action<Transform> initTransform)
	{
		return CreationMgr.InstantiateCreation(itemObj.protoId, itemObj.instanceId, init: false, initTransform);
	}
}
