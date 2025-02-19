using uLink;
using UnityEngine;

[AddComponentMenu("uLink Utilities/P2P Connector")]
[RequireComponent(typeof(uLinkNetworkP2P))]
public class uLinkP2PConnector : uLink.MonoBehaviour
{
	public string host = "127.0.0.1";

	public int port;

	public string incomingPassword = string.Empty;

	public float interval = 0.2f;

	public float connectingTimeout = 1f;

	private string cachedHost = string.Empty;

	private NetworkPeer target = NetworkPeer.unassigned;

	private float lastTimeConnecting = float.NaN;

	private NetworkP2P p2p;

	private void Awake()
	{
		p2p = base.networkP2P;
		if (base.enabled)
		{
			OnEnable();
		}
	}

	private void OnEnable()
	{
		if (!IsInvoking("KeepConnected"))
		{
			InvokeRepeating("KeepConnected", interval, interval);
		}
	}

	private void OnDisable()
	{
		CancelInvoke("KeepConnected");
	}

	private void KeepConnected()
	{
		if (string.IsNullOrEmpty(host) || port == 0)
		{
			return;
		}
		if (cachedHost != host || target.port != port)
		{
			cachedHost = host;
			target = new NetworkPeer(host, port);
		}
		switch (p2p.GetStatus(target))
		{
		case NetworkStatus.Disconnected:
			lastTimeConnecting = Time.time;
			p2p.Connect(target, incomingPassword);
			break;
		case NetworkStatus.Connecting:
			if (float.IsNaN(lastTimeConnecting))
			{
				lastTimeConnecting = Time.time;
			}
			else if (Time.time >= lastTimeConnecting + connectingTimeout)
			{
				p2p.CloseConnection(target, sendDisconnectionNotification: true);
			}
			break;
		default:
			lastTimeConnecting = float.NaN;
			break;
		}
	}
}
