using Pathea;
using UnityEngine;

public static class VCGameMediator
{
	public static float SEVol => SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;

	public static void Update()
	{
		GameConfig.IsInVCE = VCEditor.s_Active;
		VCEditor.s_ConnectedToGame = PeSingleton<PeCreature>.Instance.mainPlayer != null;
		VCEditor.s_MultiplayerMode = GameConfig.IsMultiMode;
	}

	public static bool MeshVisible(CreationMeshLoader loader)
	{
		return true;
	}

	public static void SendIsoDataToServer(string name, string desc, byte[] preIso, byte[] isoData, string[] tags, bool sendToServer = true, ulong fileId = 0, bool free = false)
	{
		SteamWorkShop.SendFile(null, name, desc, preIso, isoData, tags, sendToServer, -1, fileId, free);
	}

	public static bool UseLandHeightMap()
	{
		return PeGameMgr.IsSingleStory;
	}

	public static void CloseGameMainCamera()
	{
		if (Camera.main != null)
		{
			VCEditor.s_OutsideCameraCullingMask = Camera.main.cullingMask;
			Camera.main.cullingMask = 0;
			if (PECameraMan.Instance != null)
			{
				PECameraMan.Instance.m_Controller.SaveParams();
			}
		}
	}

	public static void RevertGameMainCamera()
	{
		if (Camera.main != null)
		{
			Camera.main.cullingMask = VCEditor.s_OutsideCameraCullingMask;
			if (PECameraMan.Instance != null)
			{
				PECameraMan.Instance.m_Controller.SaveParams();
			}
		}
	}
}
