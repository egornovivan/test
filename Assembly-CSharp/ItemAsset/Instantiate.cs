using System;
using UnityEngine;

namespace ItemAsset;

public class Instantiate : Cmpt
{
	public string viewResPath => base.protoData.resourcePath;

	public string logicResPath => base.protoData.resourcePath1;

	public virtual GameObject CreateViewGameObj(Action<Transform> initTransform)
	{
		return CreateGameObj(viewResPath, initTransform);
	}

	public virtual GameObject CreateLogicGameObj(Action<Transform> initTransform)
	{
		return CreateGameObj(logicResPath, initTransform);
	}

	public virtual GameObject CreateDraggingGameObj(Action<Transform> initTransform)
	{
		return CreateViewGameObj(initTransform);
	}

	protected static GameObject CreateGameObj(string path, Action<Transform> initTransform)
	{
		if (!string.IsNullOrEmpty(path))
		{
			UnityEngine.Object @object = AssetsLoader.Instance.LoadPrefabImm(path);
			if (null != @object)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
				gameObject.name = @object.name;
				initTransform?.Invoke(gameObject.transform);
				return gameObject;
			}
		}
		return null;
	}
}
