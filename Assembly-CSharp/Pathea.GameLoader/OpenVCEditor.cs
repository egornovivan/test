using UnityEngine;

namespace Pathea.GameLoader;

internal class OpenVCEditor : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		VCEditor.Open();
		VCEditor.OnCloseFinally += delegate
		{
			Debug.Log("vceditor closed");
		};
	}
}
