using System;
using System.Diagnostics;
using System.IO;
using Pathea;
using uLink;
using UnityEngine;

public class MyServerManager
{
	public static string LocalIp;

	public static string LocalPwd;

	public static string LocalName;

	public static int LocalPort;

	public static ServerRegistered LocalHost;

	public static event Action OnServerHostEvent;

	private static void OnServerHost()
	{
		if (MyServerManager.OnServerHostEvent != null)
		{
			MyServerManager.OnServerHostEvent();
		}
	}

	public static bool StartMyServer(MyServer ss)
	{
		if (ss != null)
		{
			if (string.IsNullOrEmpty(ss.gameName))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000018));
				UnityEngine.Debug.Log($"ServerName is null! gameName: {ss.gameName},gameMode:{ss.gameMode},gameType:{ss.gameType},seedStr:{ss.seedStr}");
				return false;
			}
			int num = NetworkUtility.FindAvailablePort(9900, 9915);
			if (!NetworkUtility.IsPortAvailable(num))
			{
				MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000498), num));
				return false;
			}
			string text = Path.Combine(Environment.CurrentDirectory, "Server");
			Directory.CreateDirectory(text);
			string text2 = Path.Combine(text, "ServerData");
			Directory.CreateDirectory(text2);
			string text3 = ((ss.gameMode != 4) ? Path.Combine(text2, "CommonGames") : Path.Combine(text2, "CustomGames"));
			Directory.CreateDirectory(text3);
			string text4 = Path.Combine(text3, ss.gameName);
			Directory.CreateDirectory(text4);
			string configFile = Path.Combine(text4, "config.json");
			ss.Create(configFile);
			string arg = MyServer.ReplaceStr(ss.gameName);
			string arguments = $"-batchmode startwithargs {arg}#{ss.gameMode}#{SteamMgr.steamId}";
			string empty = string.Empty;
			Process process = new Process();
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				empty = Path.Combine(text, "PE_Server.app/Contents/MacOS/PE_Server");
				process.StartInfo.FileName = empty;
				process.StartInfo.Arguments = arguments;
				break;
			case RuntimePlatform.LinuxPlayer:
				empty = Path.Combine(text, "PE_Server.x86_64");
				process.StartInfo.FileName = empty;
				process.StartInfo.Arguments = arguments;
				break;
			default:
				empty = Path.Combine(text, "PE_Server.exe");
				process.StartInfo.FileName = empty;
				process.StartInfo.Arguments = arguments;
				break;
			}
			if (!File.Exists(empty))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000061) + empty);
				UnityEngine.Debug.LogError(empty + " does not exists.");
				return false;
			}
			if (process.Start())
			{
				LocalIp = "127.0.0.1";
				LocalPort = num;
				LocalPwd = ss.gamePassword;
				LocalName = ss.gameName;
				LocalHost = null;
				OnServerHost();
				return true;
			}
		}
		return false;
	}

	public static bool StartCustomServer(MyServer srv)
	{
		if (LoadServer.ServerList.Contains(srv))
		{
			UnityEngine.Debug.Log("servername already existed!");
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000499));
			return false;
		}
		string text = Path.Combine(Environment.CurrentDirectory, "Server");
		Directory.CreateDirectory(text);
		string text2 = Path.Combine(text, "ServerData");
		Directory.CreateDirectory(text2);
		string text3 = Path.Combine(text2, "CustomGames");
		Directory.CreateDirectory(text3);
		string text4 = Path.Combine(text3, srv.gameName);
		Directory.CreateDirectory(text4);
		string text5 = Path.Combine(text4, "Scenario");
		Directory.CreateDirectory(text5);
		string text6 = Path.Combine(text4, "Worlds");
		Directory.CreateDirectory(text6);
		string destFileName = Path.Combine(text5, "ForceSettings.xml");
		string destFileName2 = Path.Combine(text5, "WorldSettings.xml");
		string destFileName3 = Path.Combine(text4, "MAP.uid");
		string path = Path.Combine(Environment.CurrentDirectory, "CustomGames");
		string text7 = Path.Combine(path, PeGameMgr.gameName);
		if (!Directory.Exists(text7))
		{
			UnityEngine.Debug.LogErrorFormat("Invalide custom game path:{0}", text7);
			return false;
		}
		string path2 = Path.Combine(text7, "Scenario");
		string text8 = Path.Combine(path2, "ForceSettings.xml");
		string text9 = Path.Combine(path2, "WorldSettings.xml");
		string text10 = Path.Combine(text7, "MAP.uid");
		if (!File.Exists(text10))
		{
			UnityEngine.Debug.LogErrorFormat("{0} does not exist", text10);
			return false;
		}
		if (!File.Exists(text8))
		{
			UnityEngine.Debug.LogErrorFormat("{0} does not exist", text8);
			return false;
		}
		if (!File.Exists(text9))
		{
			UnityEngine.Debug.LogErrorFormat("{0} does not exist", text9);
			return false;
		}
		string text11 = Path.Combine(text7, "Worlds");
		if (!Directory.Exists(text11))
		{
			UnityEngine.Debug.LogErrorFormat("Invalide custom worlds path:{0}", text11);
			return false;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(text11);
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		if (directories.Length <= 0)
		{
			UnityEngine.Debug.LogErrorFormat("No worlds exist");
			return false;
		}
		for (int i = 0; i < directories.Length; i++)
		{
			string path3 = Path.Combine(text11, directories[i].Name);
			string text12 = Path.Combine(path3, "WorldEntity.xml");
			if (!File.Exists(text12))
			{
				UnityEngine.Debug.LogErrorFormat("Invalide file:{0}", text12);
				return false;
			}
			string text13 = Path.Combine(text6, directories[i].Name);
			Directory.CreateDirectory(text13);
			string destFileName4 = Path.Combine(text13, "WorldEntity.xml");
			File.Copy(text12, destFileName4, overwrite: true);
		}
		File.Copy(text8, destFileName, overwrite: true);
		File.Copy(text9, destFileName2, overwrite: true);
		File.Copy(text10, destFileName3, overwrite: true);
		int num = NetworkUtility.FindAvailablePort(9900, 9915);
		if (!NetworkUtility.IsPortAvailable(num))
		{
			MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000498), num));
			return false;
		}
		string configFile = Path.Combine(text4, "config.json");
		srv.Create(configFile);
		string arg = MyServer.ReplaceStr(srv.gameName);
		string arguments = $"-batchmode startwithargs {arg}#{srv.gameMode}#{SteamMgr.steamId}";
		string empty = string.Empty;
		Process process = new Process();
		switch (Application.platform)
		{
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
			empty = Path.Combine(text, "PE_Server.app/Contents/MacOS/PE_Server");
			process.StartInfo.FileName = empty;
			process.StartInfo.Arguments = arguments;
			break;
		case RuntimePlatform.LinuxPlayer:
			empty = Path.Combine(text, "PE_Server.x86_64");
			process.StartInfo.FileName = empty;
			process.StartInfo.Arguments = arguments;
			break;
		default:
			empty = Path.Combine(text, "PE_Server.exe");
			process.StartInfo.FileName = empty;
			process.StartInfo.Arguments = arguments;
			break;
		}
		if (!File.Exists(empty))
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000061) + empty);
			UnityEngine.Debug.LogError(empty + " does not exists.");
			return false;
		}
		if (process.Start())
		{
			LocalIp = "127.0.0.1";
			LocalPort = num;
			LocalPwd = srv.gamePassword;
			LocalName = srv.gameName;
			LocalHost = null;
			OnServerHost();
			return true;
		}
		return false;
	}

	public static bool CreateNewServer(MyServer ss)
	{
		if (LoadServer.ServerList.Contains(ss))
		{
			UnityEngine.Debug.Log("servername already existed!");
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000499));
			return false;
		}
		if (ss != null)
		{
			int num = NetworkUtility.FindAvailablePort(9900, 9915);
			if (!NetworkUtility.IsPortAvailable(num))
			{
				MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000498), num));
				return false;
			}
			string text = Path.Combine(Environment.CurrentDirectory, "Server");
			Directory.CreateDirectory(text);
			string text2 = Path.Combine(text, "ServerData");
			Directory.CreateDirectory(text2);
			string text3 = Path.Combine(text2, "CommonGames");
			Directory.CreateDirectory(text3);
			string text4 = Path.Combine(text3, ss.gameName);
			Directory.CreateDirectory(text4);
			string configFile = Path.Combine(text4, "config.json");
			ss.Create(configFile);
			string arg = MyServer.ReplaceStr(ss.gameName);
			string arguments = $"-batchmode startwithargs {arg}#{ss.gameMode}#{SteamMgr.steamId}";
			string empty = string.Empty;
			Process process = new Process();
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				empty = Path.Combine(text, "PE_Server.app/Contents/MacOS/PE_Server");
				process.StartInfo.FileName = empty;
				process.StartInfo.Arguments = arguments;
				break;
			case RuntimePlatform.LinuxPlayer:
				empty = Path.Combine(text, "PE_Server.x86_64");
				process.StartInfo.FileName = empty;
				process.StartInfo.Arguments = arguments;
				break;
			default:
				empty = Path.Combine(text, "PE_Server.exe");
				process.StartInfo.FileName = empty;
				process.StartInfo.Arguments = arguments;
				break;
			}
			if (!File.Exists(empty))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000061) + empty);
				UnityEngine.Debug.LogError(empty + " does not exists.");
				return false;
			}
			if (process.Start())
			{
				LocalIp = "127.0.0.1";
				LocalPort = num;
				LocalPwd = ss.gamePassword;
				LocalName = ss.gameName;
				LocalHost = null;
				OnServerHost();
				return true;
			}
		}
		return false;
	}
}
