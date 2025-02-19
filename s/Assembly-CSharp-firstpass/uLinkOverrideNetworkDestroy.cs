using uLink;
using UnityEngine;

[AddComponentMenu("uLink Utilities/Override Network Destroy")]
public class uLinkOverrideNetworkDestroy : uLink.MonoBehaviour
{
	public string broadcastMessage = "uLink_OnNetworkDestroy";

	public bool autoDestroyAfterMessage = true;

	private uLink.NetworkView mainNetworkView;

	private NetworkInstantiator.Destroyer oldDestroyer;

	protected void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
	{
		mainNetworkView = info.networkView;
		oldDestroyer = mainNetworkView.instantiator.destroyer;
		mainNetworkView.instantiator.destroyer = OverrideDestroyer;
	}

	private void OverrideDestroyer(uLink.NetworkView instance)
	{
		if (autoDestroyAfterMessage)
		{
			instance.BroadcastMessage(broadcastMessage, SendMessageOptions.DontRequireReceiver);
			Destroy();
		}
		else
		{
			instance.BroadcastMessage(broadcastMessage, SendMessageOptions.RequireReceiver);
		}
	}

	public void Destroy()
	{
		if (oldDestroyer != null)
		{
			oldDestroyer(mainNetworkView);
		}
	}
}
