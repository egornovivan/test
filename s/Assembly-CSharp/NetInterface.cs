using System;
using System.Collections.Generic;
using System.Linq;
using uLink;
using UnityEngine;

[RequireComponent(typeof(uLink.NetworkView))]
public class NetInterface : uLink.MonoBehaviour
{
	protected Dictionary<EPacketType, Action<uLink.BitStream, uLink.NetworkMessageInfo>> _actionEvent;

	public uLink.NetworkView OwnerView => base.networkView;

	public uLink.NetworkViewID ViewID => (!(null != base.networkView)) ? uLink.NetworkViewID.unassigned : base.networkView.viewID;

	public static bool IsServer => uLink.Network.isServer && uLink.Network.peerType == uLink.NetworkPeerType.Server;

	public event Action<NetInterface> PEAwakeEvent;

	public event Action<NetInterface> PEInstantiateEvent;

	public event Action<NetInterface> PEStartEvent;

	public event Action<NetInterface> PEDestroyEvent;

	protected void SetParent(string name)
	{
	}

	public static void CloseConnection(uLink.NetworkPlayer peer)
	{
		uLink.Network.CloseConnection(peer, sendDisconnectionNotification: true, 5);
	}

	public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rot, NetworkGroup group, params object[] args)
	{
		return uLink.Network.Instantiate(prefab, pos, rot, group, args);
	}

	public static GameObject Instantiate(uLink.NetworkPlayer peer, GameObject proxyPrefab, GameObject ownerPrefab, GameObject serverPrefab, Vector3 pos, Quaternion rot, NetworkGroup group, params object[] args)
	{
		return uLink.Network.Instantiate(peer, proxyPrefab, ownerPrefab, serverPrefab, pos, rot, group, args);
	}

	public static void InitializeServer(int maxConnection, int startPort, int endPort)
	{
		InitializeServer(maxConnection, startPort, endPort, useProxy: false);
	}

	public static void InitializeServer(int maxConnection, int startPort, int endPort, bool useProxy)
	{
		uLink.NetworkConnectionError networkConnectionError = uLink.Network.InitializeServer(maxConnection, startPort, endPort, useProxy);
		if (networkConnectionError != 0)
		{
			Debug.LogError(networkConnectionError);
		}
	}

	public static void InitializeServer(int maxConnection, int port)
	{
		InitializeServer(maxConnection, port, useProxy: false);
	}

	public static void InitializeServer(int maxConnection, int port, bool useProxy)
	{
		uLink.NetworkConnectionError networkConnectionError = uLink.Network.InitializeServer(maxConnection, port, useProxy);
		if (networkConnectionError != 0)
		{
			Debug.LogError(networkConnectionError);
		}
	}

	public static void Disconnect()
	{
		uLink.Network.Disconnect();
	}

	public static void NetDestroy(NetInterface obj)
	{
		if (!(null == obj))
		{
			NetDestroy(obj.OwnerView);
		}
	}

	public static void NetDestroy(uLink.NetworkView view)
	{
		if (IsServer)
		{
			uLink.Network.Destroy(view);
		}
	}

	public static void AddPlayerToGroup(uLink.NetworkPlayer peer, NetworkGroup group)
	{
		if (IsServer)
		{
			uLink.Network.AddPlayerToGroup(peer, group);
		}
	}

	public static void RemovePlayerFromGroup(uLink.NetworkPlayer peer, NetworkGroup group)
	{
		if (IsServer)
		{
			uLink.Network.RemovePlayerFromGroup(peer, group);
		}
	}

	public static bool CheckPeer(uLink.NetworkPlayer peer)
	{
		return peer.isConnected && !peer.isUnassigned;
	}

	public void RPCPeer(uLink.NetworkPlayer peer, params object[] args)
	{
		try
		{
			if (CheckPeer() && CheckPeer(peer))
			{
				base.networkView.RPC("RPC_Sync", peer, args);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void RPCPeers(IEnumerable<uLink.NetworkPlayer> peers, params object[] args)
	{
		try
		{
			IEnumerable<uLink.NetworkPlayer> targets = peers.Where((uLink.NetworkPlayer peer) => CheckPeer(peer));
			if (CheckPeer())
			{
				base.networkView.RPC("RPC_Sync", targets, args);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void RPCOwner(params object[] args)
	{
		try
		{
			if (CheckPeer())
			{
				base.networkView.RPC("RPC_Sync", uLink.RPCMode.Owner, args);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void RPCOthers(params object[] args)
	{
		try
		{
			if (CheckPeer())
			{
				base.networkView.RPC("RPC_Sync", uLink.RPCMode.Others, args);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void RPCProxy(params object[] args)
	{
		try
		{
			if (CheckPeer())
			{
				base.networkView.RPC("RPC_Sync", uLink.RPCMode.OthersExceptOwner, args);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void RPCProxy(NetInterface excludePeer, params object[] obj)
	{
		try
		{
			if (CheckPeer())
			{
				IEnumerable<uLink.NetworkPlayer> targets = uLink.Network.connections.Where((uLink.NetworkPlayer peer) => !excludePeer.OwnerView.owner.Equals(peer) && CheckPeer(peer));
				base.networkView.RPC("RPC_Sync", targets, obj);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void RPCProxy(uLink.NetworkPlayer excludePeer, params object[] obj)
	{
		try
		{
			if (CheckPeer())
			{
				IEnumerable<uLink.NetworkPlayer> targets = uLink.Network.connections.Where((uLink.NetworkPlayer peer) => !excludePeer.Equals(peer) && CheckPeer(peer));
				base.networkView.RPC("RPC_Sync", targets, obj);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void URPCPeer(uLink.NetworkPlayer peer, params object[] obj)
	{
		try
		{
			if (CheckPeer() && CheckPeer(peer))
			{
				base.networkView.UnreliableRPC("URPC_Sync", peer, obj);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void URPCPeers(IEnumerable<uLink.NetworkPlayer> peers, params object[] obj)
	{
		try
		{
			if (CheckPeer())
			{
				IEnumerable<uLink.NetworkPlayer> targets = peers.Where((uLink.NetworkPlayer peer) => CheckPeer(peer));
				base.networkView.UnreliableRPC("URPC_Sync", targets, obj);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void URPCOwner(params object[] obj)
	{
		try
		{
			if (CheckPeer())
			{
				base.networkView.UnreliableRPC("URPC_Sync", uLink.RPCMode.Owner, obj);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void URPCOthers(params object[] obj)
	{
		try
		{
			if (CheckPeer())
			{
				base.networkView.UnreliableRPC("URPC_Sync", uLink.RPCMode.Others, obj);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	public void URPCProxy(params object[] obj)
	{
		try
		{
			if (CheckPeer())
			{
				base.networkView.UnreliableRPC("URPC_Sync", uLink.RPCMode.OthersExceptOwner, obj);
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), ex.Message, ex.StackTrace);
			}
		}
	}

	private bool CheckPeer()
	{
		return (!IsServer || !(null == base.networkView)) && !(base.networkView.viewID == uLink.NetworkViewID.unassigned);
	}

	protected void BindAction(EPacketType type, Action<uLink.BitStream, uLink.NetworkMessageInfo> action)
	{
		if (_actionEvent.ContainsKey(type))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("Replace msg handler:{0}", type);
			}
			_actionEvent.Remove(type);
		}
		if (LogFilter.logDev)
		{
			Debug.LogWarningFormat("Register msg handler:{0}", type);
		}
		_actionEvent.Add(type, action);
	}

	[RPC]
	protected void URPC_Sync(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		EPacketType ePacketType = EPacketType.PT_MAX;
		try
		{
			ePacketType = stream.Read<EPacketType>(new object[0]);
			_actionEvent[ePacketType](stream, info);
		}
		catch (Exception ex)
		{
			if (_actionEvent.ContainsKey(ePacketType))
			{
				if (LogFilter.logWarn)
				{
					Debug.LogErrorFormat("Message:[{0}]\r\n{1}\r\n{2}\r\n{3}", GetType(), ePacketType, ex.Message, ex.StackTrace);
				}
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Message:[{0}]|[{1}] does not implement\r\n{2}", ePacketType, GetType(), ex.StackTrace);
			}
		}
	}

	[RPC]
	protected void RPC_Sync(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		EPacketType ePacketType = EPacketType.PT_MAX;
		try
		{
			ePacketType = stream.Read<EPacketType>(new object[0]);
			_actionEvent[ePacketType](stream, info);
		}
		catch (Exception ex)
		{
			if (_actionEvent.ContainsKey(ePacketType))
			{
				if (LogFilter.logWarn)
				{
					Debug.LogErrorFormat("Message:[{0}]\r\n{1}\r\n{2}\r\n{3}", GetType(), ePacketType, ex.Message, ex.StackTrace);
				}
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarningFormat("Message:[{0}]|[{1}] does not implement\r\n{2}", ePacketType, GetType(), ex.StackTrace);
			}
		}
	}

	private void Awake()
	{
		_actionEvent = new Dictionary<EPacketType, Action<uLink.BitStream, uLink.NetworkMessageInfo>>();
		OnPEAwake();
		if (this.PEAwakeEvent != null)
		{
			this.PEAwakeEvent(this);
		}
	}

	private void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
	{
		OnPEInstantiate(info);
		if (this.PEInstantiateEvent != null)
		{
			this.PEInstantiateEvent(this);
		}
	}

	private void Start()
	{
		OnPEStart();
		if (this.PEStartEvent != null)
		{
			this.PEStartEvent(this);
		}
	}

	private void OnDestroy()
	{
		_actionEvent.Clear();
		if (this.PEDestroyEvent != null)
		{
			this.PEDestroyEvent(this);
		}
		OnPEDestroy();
	}

	public void AddPlayerToGroup(NetworkGroup group)
	{
		if (IsServer)
		{
			uLink.Network.AddPlayerToGroup(OwnerView.owner, group);
		}
	}

	public void RemovePlayerFromGroup(NetworkGroup group)
	{
		if (IsServer)
		{
			uLink.Network.RemovePlayerFromGroup(OwnerView.owner, group);
		}
	}

	protected virtual void OnPEAwake()
	{
	}

	protected virtual void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void OnPEStart()
	{
	}

	protected virtual void OnPEDestroy()
	{
	}
}
