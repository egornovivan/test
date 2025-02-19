using System;
using uLink;
using UnityEngine;

[RequireComponent(typeof(uLinkP2PConnector))]
[DisallowMultipleComponent]
public class P2PManager : uLink.MonoBehaviour
{
	[SerializeField]
	private NetworkP2P netP2P;

	[SerializeField]
	private uLinkP2PConnector netP2PConn;

	[RuntimeInitializeOnLoadMethod]
	private static void OnLoad()
	{
		GameObject gameObject = new GameObject("P2PManager");
		gameObject.AddComponent<P2PManager>();
	}

	private void Awake()
	{
		if (null == netP2P)
		{
			netP2P = GetComponent<NetworkP2P>();
		}
		if (null == netP2PConn)
		{
			netP2PConn = GetComponent<uLinkP2PConnector>();
		}
	}

	private void Start()
	{
		if (null == netP2P)
		{
			netP2P = base.gameObject.AddComponent<NetworkP2P>();
		}
		netP2P.incomingPassword = "thisisincomingpassword";
		netP2P.listenPort = 0;
		netP2P.maxConnections = 8;
		if (null == netP2PConn)
		{
			netP2PConn = base.gameObject.AddComponent<uLinkP2PConnector>();
		}
		netP2PConn.host = "127.0.0.1";
		netP2PConn.port = 56789;
		netP2PConn.incomingPassword = "thisisincomingpassword";
		netP2PConn.interval = 0.2f;
		netP2PConn.connectingTimeout = 1f;
	}

	private void uLink_OnPeerConnected(NetworkPeer peer)
	{
		P2PRPC("RPC_S2C_CreateNewServer", peer, ServerConfig.ServerName, (int)ServerConfig.SceneMode, ServerConfig.ServerPort);
	}

	private void uLink_OnPeerDisconnected(NetworkPeer peer)
	{
		netP2P.CloseConnection(peer, sendDisconnectionNotification: true);
	}

	private void P2PRPC(string func, NetworkPeer peer, params object[] args)
	{
		try
		{
			netP2P.RPC(func, peer, args);
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogWarningFormat("{0}\r\n{1}\r\n{2}", func, ex.Message, ex.StackTrace);
			}
		}
	}

	[RPC]
	private void RPC_C2S_CloseServer()
	{
		GameServer.Quit();
	}
}
