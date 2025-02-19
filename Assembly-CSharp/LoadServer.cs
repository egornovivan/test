using System;
using System.Collections.Generic;
using System.IO;
using Pathea;

public class LoadServer
{
	public static List<string> ServerNames = new List<string>();

	public static List<MyServer> ServerList = new List<MyServer>();

	public static event Action<string, int> AddServerEventHandler;

	public static void Start()
	{
		LoadServers();
	}

	private static void LoadServers(string path)
	{
		string[] files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
		string[] array = files;
		foreach (string configFile in array)
		{
			MyServer myServer = new MyServer();
			myServer.Read(configFile);
			ServerNames.Add(myServer.gameName);
			ServerList.Add(myServer);
		}
	}

	public static void LoadServers()
	{
		ServerNames.Clear();
		ServerList.Clear();
		string text = Path.Combine(Environment.CurrentDirectory, "Server");
		Directory.CreateDirectory(text);
		string text2 = Path.Combine(text, "ServerData");
		Directory.CreateDirectory(text2);
		string path = Path.Combine(text2, "CommonGames");
		Directory.CreateDirectory(path);
		LoadServers(path);
		string path2 = Path.Combine(text2, "CustomGames");
		Directory.CreateDirectory(path2);
		LoadServers(path2);
	}

	public static bool AddServer(string gameName, int sceneMode)
	{
		string text = Path.Combine(Environment.CurrentDirectory, "Server");
		Directory.CreateDirectory(text);
		string text2 = Path.Combine(text, "ServerData");
		Directory.CreateDirectory(text2);
		string empty = string.Empty;
		PeGameMgr.ESceneMode eSceneMode = (PeGameMgr.ESceneMode)sceneMode;
		if (eSceneMode == PeGameMgr.ESceneMode.Custom)
		{
			empty = Path.Combine(text2, "CustomGames");
			Directory.CreateDirectory(empty);
		}
		else
		{
			empty = Path.Combine(text2, "CommonGames");
			Directory.CreateDirectory(empty);
		}
		string text3 = Path.Combine(empty, gameName);
		Directory.CreateDirectory(text3);
		string text4 = Path.Combine(text3, "config.json");
		if (File.Exists(text4))
		{
			MyServer myServer = ServerList.Find((MyServer iter) => iter.gameName.Equals(gameName) && iter.gameMode == sceneMode);
			if (myServer == null)
			{
				myServer = new MyServer();
				myServer.Read(text4);
				ServerNames.Add(myServer.gameName);
				ServerList.Add(myServer);
			}
			OnServerAdd(gameName, sceneMode);
			return true;
		}
		return false;
	}

	public static bool DeleteServer(string serverName, int sceneMode)
	{
		if (!ServerNames.Contains(serverName))
		{
			return false;
		}
		int num = ServerList.FindIndex((MyServer iter) => iter.gameName.Equals(serverName) && iter.gameMode == sceneMode);
		if (num == -1)
		{
			return false;
		}
		string text = Path.Combine(Environment.CurrentDirectory, "Server");
		Directory.CreateDirectory(text);
		string text2 = Path.Combine(text, "ServerData");
		Directory.CreateDirectory(text2);
		string empty = string.Empty;
		if (sceneMode == 4)
		{
			empty = Path.Combine(text2, "CustomGames");
			Directory.CreateDirectory(empty);
		}
		else
		{
			empty = Path.Combine(text2, "CommonGames");
			Directory.CreateDirectory(empty);
		}
		string path = Path.Combine(empty, serverName);
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
		}
		ServerList.RemoveAt(num);
		ServerNames.Remove(serverName);
		return true;
	}

	public static bool Exist(string serverName)
	{
		if (ServerNames.Contains(serverName))
		{
			return true;
		}
		return false;
	}

	public static MyServer GetServer(string serverName, int gameMode)
	{
		return ServerList.Find((MyServer it) => it.gameName == serverName && it.gameMode == gameMode);
	}

	private static void OnServerAdd(string srvName, int sceneMode)
	{
		if (LoadServer.AddServerEventHandler != null)
		{
			LoadServer.AddServerEventHandler(srvName, sceneMode);
		}
	}
}
