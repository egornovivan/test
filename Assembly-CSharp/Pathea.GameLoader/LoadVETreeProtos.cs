using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadVETreeProtos : ModuleLoader
{
	public LoadVETreeProtos(bool bNew)
		: base(bNew)
	{
	}

	private void Load()
	{
		string text = "VETreeProtos";
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(text));
		if (null == gameObject)
		{
			Debug.LogError("can't find tree protos prefab:" + text);
		}
	}

	protected override void New()
	{
		Load();
	}

	protected override void Restore()
	{
		Load();
	}
}
