using Pathea;
using UnityEngine;

public class SceneStaticAssetAgent : SceneSerializableObjAgent
{
	public override bool NeedToActivate => false;

	public SceneStaticAssetAgent()
	{
	}

	public SceneStaticAssetAgent(string pathPre, string pathMain, Vector3 pos, Quaternion rotation, Vector3 scale)
		: base(pathPre, pathMain, pos, rotation, scale)
	{
	}

	public override void OnMainGoLoaded()
	{
		if (!(_go == null))
		{
			_go.transform.parent = PeSingleton<SceneAssetsMan>.Instance.RootObj.transform;
		}
	}
}
