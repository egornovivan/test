using System.Collections.Generic;
using UnityEngine;

public class SceneStaticObjDependence : ISceneObjActivationDependence
{
	private int _nLastFrame;

	private List<ISceneObjAgent> _sceneObjs;

	private List<Vector3> _posToCheck = new List<Vector3>(128);

	private List<IBoundInScene> _boundsToCheck = new List<IBoundInScene>(128);

	public SceneStaticObjDependence(List<ISceneObjAgent> objs)
	{
		_sceneObjs = objs;
	}

	public bool IsDependableForAgent(ISceneObjAgent agent, ref EDependChunkType type)
	{
		int count = _sceneObjs.Count;
		if (Time.frameCount != _nLastFrame)
		{
			_nLastFrame = Time.frameCount;
			_posToCheck.Clear();
			_boundsToCheck.Clear();
			for (int i = 0; i < count; i++)
			{
				ISceneObjAgent sceneObjAgent = _sceneObjs[i];
				if (sceneObjAgent != null && !sceneObjAgent.NeedToActivate && sceneObjAgent.Go == null)
				{
					if (sceneObjAgent.Bound != null)
					{
						_boundsToCheck.Add(sceneObjAgent.Bound);
					}
					else
					{
						_posToCheck.Add(sceneObjAgent.Pos);
					}
				}
			}
		}
		count = _posToCheck.Count;
		for (int j = 0; j < count; j++)
		{
			if (Vector3.SqrMagnitude(_posToCheck[j] - agent.Pos) < 64f)
			{
				return false;
			}
		}
		count = _boundsToCheck.Count;
		for (int k = 0; k < count; k++)
		{
			if (_boundsToCheck[k].Contains(agent.Pos, agent.TstYOnActivate))
			{
				return false;
			}
		}
		return true;
	}
}
