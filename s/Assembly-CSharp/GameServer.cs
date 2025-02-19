using System;
using CustomData;
using UnityEngine;

public class GameServer : MonoBehaviour
{
	private void Awake()
	{
		try
		{
			Application.targetFrameRate = 60;
			CustomCodecRegister.Register();
			ServerConfig.InitConfig();
			ThreadHelper.Instance.CheckSingletonServer();
			AsyncSqlite.OpenDB();
			ServerConfig.InitFromDB();
			uLinkNetwork.SetServerState(EServerStatus.Prepared);
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogWarningFormat("{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
			Quit();
		}
	}

	private void OnApplicationQuit()
	{
		NetInterface.Disconnect();
		LoadManager.Save();
		AsyncSqlite.StopDB();
	}

	public static void Quit()
	{
		Application.Quit();
	}
}
