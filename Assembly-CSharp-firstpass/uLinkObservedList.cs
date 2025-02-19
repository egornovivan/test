using uLink;
using UnityEngine;

[AddComponentMenu("uLink Utilities/Observed List")]
public class uLinkObservedList : uLink.MonoBehaviour
{
	public Component[] observedList;

	private NetworkObserved[] _list;

	protected void uLink_OnSerializeNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (observedList == null)
		{
			return;
		}
		_CheckList();
		NetworkObserved[] list = _list;
		for (int i = 0; i < list.Length; i++)
		{
			NetworkObserved networkObserved = list[i];
			if (networkObserved.serializeProxy != null)
			{
				networkObserved.serializeProxy(stream, info);
			}
		}
	}

	protected void uLink_OnSerializeNetworkViewOwner(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (observedList == null)
		{
			return;
		}
		_CheckList();
		NetworkObserved[] list = _list;
		for (int i = 0; i < list.Length; i++)
		{
			NetworkObserved networkObserved = list[i];
			if (networkObserved.serializeOwner != null)
			{
				networkObserved.serializeOwner(stream, info);
			}
		}
	}

	protected void uLink_OnHandoverNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (observedList == null)
		{
			return;
		}
		_CheckList();
		NetworkObserved[] list = _list;
		for (int i = 0; i < list.Length; i++)
		{
			NetworkObserved networkObserved = list[i];
			if (networkObserved.handover != null)
			{
				networkObserved.handover(stream, info);
			}
		}
	}

	private void _CheckList()
	{
		if (_list == null || _list.Length != observedList.Length)
		{
			_list = new NetworkObserved[observedList.Length];
			for (int i = 0; i < observedList.Length; i++)
			{
				ref NetworkObserved reference = ref _list[i];
				reference = new NetworkObserved(observedList[i]);
			}
		}
		else
		{
			for (int j = 0; j < observedList.Length; j++)
			{
				_list[j].UpdateBinding(observedList[j]);
			}
		}
	}
}
