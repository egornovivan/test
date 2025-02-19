using uLink;
using UnityEngine;

[AddComponentMenu("uLink Utilities/Instantiate For Others")]
[RequireComponent(typeof(uLinkNetworkView))]
public class uLinkInstantiateForOthers : uLink.MonoBehaviour
{
	public GameObject othersPrefab;

	public bool appendLoginData;

	private void Start()
	{
		if (uLink.Network.status == NetworkStatus.Connected)
		{
			uLink_OnConnectedToServer();
		}
	}

	private void uLink_OnConnectedToServer()
	{
		if (!(base.networkView.viewID != uLink.NetworkViewID.unassigned) && (!uLink.Network.isAuthoritativeServer || !uLink.Network.isClient))
		{
			Transform transform = base.transform;
			uLink.NetworkPlayer player = uLink.Network.player;
			uLink.NetworkViewID viewID = uLink.Network.AllocateViewID();
			object[] initialData = ((!appendLoginData) ? new object[0] : uLink.Network.loginData);
			if (player != uLink.NetworkPlayer.server)
			{
				uLink.Network.Instantiate(viewID, player, othersPrefab, null, othersPrefab, transform.position, transform.rotation, 0, initialData);
			}
			else
			{
				uLink.Network.Instantiate(viewID, player, othersPrefab, othersPrefab, null, transform.position, transform.rotation, 0, initialData);
			}
			base.networkView.SetViewID(viewID, player);
			base.networkView.SetInitialData(initialData);
		}
	}
}
