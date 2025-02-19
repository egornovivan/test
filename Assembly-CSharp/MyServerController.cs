using System;
using System.IO;
using Pathea;
using UnityEngine;

public class MyServerController : MonoBehaviour
{
	public UIServerCtrl mServerCtrl;

	public static event Action<string, int> OnServerCloseEvent;

	private void Awake()
	{
		mServerCtrl.StartFunc += StartUIServerCtrl;
		mServerCtrl.BtnStart += StartServer;
		mServerCtrl.BtnDelete += DeleteServer;
		mServerCtrl.BtnClose += CloseServer;
		mServerCtrl.BtnRefresh += Refresh;
		mServerCtrl.checkListItem += SelectServerData;
		LoadServer.AddServerEventHandler += AddServer;
		P2PManager.OnServerDisconnectedEvent += SetServerOff;
	}

	private void OnDestroy()
	{
		mServerCtrl.StartFunc -= StartUIServerCtrl;
		mServerCtrl.BtnStart -= StartServer;
		mServerCtrl.BtnDelete -= DeleteServer;
		mServerCtrl.BtnClose -= CloseServer;
		mServerCtrl.BtnRefresh -= Refresh;
		mServerCtrl.checkListItem -= SelectServerData;
		LoadServer.AddServerEventHandler -= AddServer;
		P2PManager.OnServerDisconnectedEvent -= SetServerOff;
	}

	private void StartUIServerCtrl()
	{
		mServerCtrl.mList.mItems.Clear();
		foreach (MyServer server2 in LoadServer.ServerList)
		{
			ConnectedServer server = P2PManager.GetServer(server2.gameName, server2.gameMode);
			if (server != null)
			{
				mServerCtrl.mList.AddItem(server2.ToServerDataItem(), Color.green);
			}
			else
			{
				mServerCtrl.mList.AddItem(server2.ToServerDataItem());
			}
		}
		mServerCtrl.mList.UpdateList();
	}

	private void StartServer()
	{
		if (null == mServerCtrl || null == mServerCtrl.mList || mServerCtrl.mList.mSelectedIndex == -1)
		{
			return;
		}
		int mSelectedIndex = mServerCtrl.mList.mSelectedIndex;
		string text = mServerCtrl.mList.mItems[mSelectedIndex].mData[0];
		int num = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[mSelectedIndex].mData[2]);
		ConnectedServer server = P2PManager.GetServer(text, num);
		if (server != null)
		{
			Debug.Log("Server is running!");
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000497));
			return;
		}
		MyServer server2 = LoadServer.GetServer(text, num);
		if (server2 != null)
		{
			mServerCtrl.GetMyServerInfo(server2);
			if (server2.gameMode == 4)
			{
				PeGameMgr.mapUID = server2.uid;
				string path = Path.Combine(GameConfig.CustomDataDir, server2.mapName);
				PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(PeGameMgr.mapUID, path);
			}
			MyServerManager.StartMyServer(server2);
		}
	}

	private void AddServer(string serverName, int sceneMode)
	{
		if (mServerCtrl != null)
		{
			MyServer server = LoadServer.GetServer(serverName, sceneMode);
			if (server != null)
			{
				string mode = server.AdventureOrBuild();
				int num = mServerCtrl.mList.mItems.FindIndex((PageListItem it) => it.mData[0].Equals(serverName) && it.mData[2].Equals(mode));
				if (num == -1)
				{
					mServerCtrl.mList.AddItem(server.ToServerDataItem());
					mServerCtrl.mList.UpdateList();
				}
			}
		}
		SetServerOn(serverName, sceneMode);
	}

	private void DeleteServer()
	{
		if (null == mServerCtrl || mServerCtrl.mList.mSelectedIndex == -1)
		{
			return;
		}
		int mSelectedIndex = mServerCtrl.mList.mSelectedIndex;
		string serverName = mServerCtrl.mList.mItems[mSelectedIndex].mData[0];
		int sceneMode = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[mSelectedIndex].mData[2]);
		try
		{
			if (LoadServer.DeleteServer(serverName, sceneMode))
			{
				mServerCtrl.mList.mItems.RemoveAt(mSelectedIndex);
				mServerCtrl.mList.UpdateList();
			}
		}
		catch
		{
			Debug.Log("Server is running!");
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000497));
		}
	}

	public void SetServerOn(string serverName, int sceneMode)
	{
		if (mServerCtrl == null)
		{
			return;
		}
		MyServer server = LoadServer.GetServer(serverName, sceneMode);
		if (server != null)
		{
			string mode = server.AdventureOrBuild();
			int num = mServerCtrl.mList.mItems.FindIndex((PageListItem it) => it.mData[0].Equals(serverName) && it.mData[2].Equals(mode));
			if (num >= 0)
			{
				mServerCtrl.mList.SetColor(num, Color.green);
				mServerCtrl.mList.UpdateList();
			}
		}
	}

	private void SetServerOff(string serverName, int sceneMode)
	{
		if (mServerCtrl == null)
		{
			return;
		}
		MyServer server = LoadServer.GetServer(serverName, sceneMode);
		if (server != null)
		{
			string mode = server.AdventureOrBuild();
			int num = mServerCtrl.mList.mItems.FindIndex((PageListItem it) => it.mData[0].Equals(serverName) && it.mData[2].Equals(mode));
			if (num >= 0)
			{
				mServerCtrl.mList.SetColor(num, Color.white);
				mServerCtrl.mList.UpdateList();
			}
		}
	}

	private void CloseServer()
	{
		if (mServerCtrl.mList.mSelectedIndex != -1)
		{
			int mSelectedIndex = mServerCtrl.mList.mSelectedIndex;
			string arg = mServerCtrl.mList.mItems[mSelectedIndex].mData[0];
			int arg2 = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[mSelectedIndex].mData[2]);
			if (MyServerController.OnServerCloseEvent != null)
			{
				MyServerController.OnServerCloseEvent(arg, arg2);
			}
		}
	}

	private void Refresh()
	{
		LoadServer.LoadServers();
		StartUIServerCtrl();
	}

	private void SelectServerData(int index)
	{
		if (index != -1)
		{
			string serverName = mServerCtrl.mList.mItems[index].mData[0];
			int gameMode = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[index].mData[2]);
			MyServer server = LoadServer.GetServer(serverName, gameMode);
			if (server != null)
			{
				mServerCtrl.UpdateServerInfo(server);
			}
		}
	}
}
