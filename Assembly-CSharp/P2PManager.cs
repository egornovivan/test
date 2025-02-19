using System;
using System.Collections.Generic;
using Pathea;
using uLink;
using UnityEngine;

[RequireComponent(typeof(NetworkP2P))]
public class P2PManager : uLink.MonoBehaviour
{
	private static List<ConnectedServer> connectedServers = new List<ConnectedServer>();

	private static NetworkP2P p2p;

	public static event Action<string, int> OnServerDisconnectedEvent;

	public static event Action<string, int, int> OnServerRegisteredEvent;

	private void Awake()
	{
		if (null == p2p)
		{
			p2p = GetComponent<NetworkP2P>();
		}
	}

	private void Start()
	{
		MyServerController.OnServerCloseEvent += CloseServer;
		LoadServer.Start();
	}

	private void OnDestroy()
	{
		MyServerController.OnServerCloseEvent -= CloseServer;
	}

	private static void P2PRPC(string func, NetworkPeer peer, params object[] args)
	{
		try
		{
			p2p.RPC(func, peer, args);
		}
		catch (Exception ex)
		{
			if (LogFilter.logFatal)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", func, ex.Message, ex.StackTrace);
			}
		}
	}

	public void uLink_OnPeerDisconnected(NetworkPeer peer)
	{
		ConnectedServer connectedServer = connectedServers.Find((ConnectedServer it) => it.peer == peer);
		if (connectedServer != null)
		{
			connectedServers.Remove(connectedServer);
			if (P2PManager.OnServerDisconnectedEvent != null)
			{
				P2PManager.OnServerDisconnectedEvent(connectedServer.serverName, (int)connectedServer.sceneMode);
			}
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("{0} closed", connectedServer.serverName);
			}
		}
	}

	private void RPC_S2C_CreateNewServer(uLink.BitStream stream, NetworkP2PMessageInfo info)
	{
		try
		{
			string text = stream.Read<string>(new object[0]);
			int num = stream.Read<int>(new object[0]);
			int arg = stream.Read<int>(new object[0]);
			ConnectedServer connectedServer = connectedServers.Find((ConnectedServer it) => it.peer == info.sender);
			if (connectedServer == null)
			{
				ConnectedServer item = new ConnectedServer(text, num, info.sender);
				connectedServers.Add(item);
			}
			LoadServer.AddServer(text, num);
			if (P2PManager.OnServerRegisteredEvent != null)
			{
				P2PManager.OnServerRegisteredEvent(text, num, arg);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logFatal)
			{
				Debug.LogErrorFormat("RPC_S2C_CreateNewServer error\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
	}

	public static void CloseAllServer()
	{
		if (null == p2p)
		{
			return;
		}
		foreach (ConnectedServer connectedServer in connectedServers)
		{
			P2PRPC("RPC_C2S_CloseServer", connectedServer.peer);
		}
	}

	private static void CloseServer(string serverName, int sceneMode)
	{
		ConnectedServer connectedServer = connectedServers.Find((ConnectedServer it) => it.serverName.Equals(serverName) && it.sceneMode == (PeGameMgr.ESceneMode)sceneMode);
		if (connectedServer != null)
		{
			P2PRPC("RPC_C2S_CloseServer", connectedServer.peer);
		}
	}

	public static ConnectedServer GetServer(string srvName, int sceneMode)
	{
		return connectedServers.Find((ConnectedServer iter) => iter.serverName.Equals(srvName) && iter.sceneMode == (PeGameMgr.ESceneMode)sceneMode);
	}
}
