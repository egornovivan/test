using System;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using uLink;
using UnityEngine;

[RequireComponent(typeof(uLink.NetworkView))]
public class NetworkInterface : uLink.MonoBehaviour
{
	private static Dictionary<int, NetworkInterface> netIds = new Dictionary<int, NetworkInterface>();

	protected static Transform RootTrans;

	protected int _id;

	protected int _teamId;

	protected int _worldId;

	protected int lastAuthId;

	protected Dictionary<EPacketType, Action<uLink.BitStream, uLink.NetworkMessageInfo>> _actionEvent;

	protected uLink.NetworkView _netView;

	protected CommonInterface runner;

	public Vector3 _pos { get; set; }

	public Quaternion rot { get; set; }

	public bool canGetAuth { get; protected set; }

	public int Id => _id;

	public int TeamId => _teamId;

	public int WorldId => _worldId;

	public int authId { get; protected set; }

	public uLink.NetworkView OwnerView => _netView;

	public CommonInterface Runner => runner;

	public bool hasOwnerAuth => authId == PlayerNetwork.mainPlayerId;

	public bool hasAuth => authId > 0;

	public static bool IsClient => uLink.Network.isClient && uLink.Network.status == NetworkStatus.Connected && uLink.Network.peerType == uLink.NetworkPeerType.Client;

	public bool IsOwner => IsClient && _netView != null && _netView.isOwner;

	public bool IsProxy => IsClient && _netView != null && _netView.isProxy;

	protected static void Add(NetworkInterface obj)
	{
		if (!(null == obj))
		{
			netIds[obj.Id] = obj;
		}
	}

	protected static void Remove(int id)
	{
		if (ExistedId(id))
		{
			netIds.Remove(id);
		}
	}

	public static bool ExistedId(int id)
	{
		return netIds.ContainsKey(id);
	}

	public static NetworkInterface Get(int id)
	{
		return (!ExistedId(id)) ? null : netIds[id];
	}

	public static T Get<T>(int id) where T : NetworkInterface
	{
		return (!ExistedId(id)) ? ((T)null) : (netIds[id] as T);
	}

	public static List<T> Get<T>() where T : NetworkInterface
	{
		IEnumerable<T> source = from NetworkInterface iter in netIds.Values
			where iter is T
			select iter as T;
		return source.ToList();
	}

	public static void Connect(string host, int remotePort, string password, params object[] objs)
	{
		if (uLink.Network.status == NetworkStatus.Disconnected)
		{
			uLink.Network.Connect(host, remotePort, password, objs);
		}
	}

	public static void Connect(uLink.HostData host, string password, params object[] objs)
	{
		if (uLink.Network.status == NetworkStatus.Disconnected)
		{
			uLink.Network.Connect(host, password, objs);
		}
	}

	public static void Disconnect()
	{
		Disconnect(200);
	}

	private static void Disconnect(int timeout)
	{
		uLink.Network.Disconnect(timeout);
	}

	private void Awake()
	{
		_netView = GetComponent<uLink.NetworkView>();
		_actionEvent = new Dictionary<EPacketType, Action<uLink.BitStream, uLink.NetworkMessageInfo>>();
		OnPEAwake();
	}

	private void Start()
	{
		OnPEStart();
		Add(this);
	}

	private void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
	{
		OnPEInstantiate(info);
	}

	private void OnDestroy()
	{
		Remove(Id);
		OnPEDestroy();
	}

	public virtual void SetTeamId(int teamId)
	{
		_teamId = teamId;
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

	public void RPC(string func, params object[] args)
	{
		try
		{
			if (CheckPeer())
			{
				_netView.RPC(func, uLink.RPCMode.Server, args);
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

	public void RPCServer(params object[] args)
	{
		try
		{
			if (CheckPeer())
			{
				_netView.RPC("RPC_Sync", uLink.RPCMode.Server, args);
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

	public void URPCServer(params object[] objs)
	{
		try
		{
			if (CheckPeer())
			{
				_netView.UnreliableRPC("URPC_Sync", uLink.RPCMode.Server, objs);
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
		return IsClient && !(null == _netView) && !(_netView.viewID == uLink.NetworkViewID.unassigned);
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
				if (LogFilter.logError)
				{
					Debug.LogErrorFormat("Message:[{0}]\r\n{1}\r\n{2}\r\n{3}", GetType(), ePacketType, ex.Message, ex.StackTrace);
				}
			}
			else if (LogFilter.logDev)
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
				if (LogFilter.logError)
				{
					Debug.LogErrorFormat("Message:[{0}]\r\n{1}\r\n{2}\r\n{3}", GetType(), ePacketType, ex.Message, ex.StackTrace);
				}
			}
			else if (LogFilter.logDev)
			{
				Debug.LogWarningFormat("Message:[{0}]|[{1}] does not implement\r\n{2}", ePacketType, GetType(), ex.StackTrace);
			}
		}
	}

	protected virtual void OnPEAwake()
	{
		if (null == RootTrans)
		{
			GameObject gameObject = new GameObject("NetObjMgr");
			RootTrans = gameObject.transform;
			DontDestroyOnLoadCmpt component = gameObject.GetComponent<DontDestroyOnLoadCmpt>();
			if (null == component)
			{
				gameObject.AddComponent<DontDestroyOnLoadCmpt>();
			}
		}
		base.transform.parent = RootTrans;
	}

	protected virtual void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void OnPEStart()
	{
	}

	protected virtual void OnPEDestroy()
	{
		StopAllCoroutines();
		if (null != Runner)
		{
			Runner.InitNetworkLayer(null);
		}
		PeSingleton<EntityMgr>.Instance.Destroy(Id);
		ISceneObjAgent sceneObjById = SceneMan.GetSceneObjById(Id);
		if (sceneObjById != null)
		{
			SceneMan.RemoveSceneObj(sceneObjById);
		}
	}

	public virtual void OnPeMsg(EMsg msg, params object[] args)
	{
	}

	public virtual void OnSpawned(GameObject obj)
	{
		if (obj == null)
		{
			throw new Exception("GameObject can not be null");
		}
		runner = obj.GetComponentInChildren<CommonInterface>();
		if (null == runner)
		{
			runner = obj.AddComponent<CommonNetworkObject>();
		}
		Runner.InitNetworkLayer(this, obj);
	}

	public virtual void InitForceData()
	{
	}
}
