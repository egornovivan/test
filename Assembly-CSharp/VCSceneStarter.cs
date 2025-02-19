using UnityEngine;

public class VCSceneStarter : MonoBehaviour
{
	private void Awake()
	{
		LocalDatabase.LoadAllData();
		SurfExtractorsMan.CheckGenSurfExtractor();
	}

	private void LateUpdate()
	{
		SurfExtractorsMan.PostProc();
	}

	private void OnDestroy()
	{
		SurfExtractorsMan.CleanUp();
		LocalDatabase.FreeAllData();
	}
}
