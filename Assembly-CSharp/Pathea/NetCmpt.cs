namespace Pathea;

public class NetCmpt : PeCmpt, IPeMsg
{
	public NetworkInterface network;

	public bool IsPlayer => network is PlayerNetwork;

	public bool IsController => !(null == network) && network.hasOwnerAuth;

	public override void Awake()
	{
		base.Awake();
		base.Entity.netCmpt = this;
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		network.OnPeMsg(msg, args);
	}

	public void SetController(bool isController)
	{
		if (isController)
		{
			base.Entity.SendMsg(EMsg.Net_Controller);
		}
		else
		{
			base.Entity.SendMsg(EMsg.Net_Proxy);
		}
	}

	public void RequestUseItem(int objId)
	{
		if (network is PlayerNetwork)
		{
			PlayerNetwork playerNetwork = (PlayerNetwork)network;
			playerNetwork.RequestUseItem(objId);
		}
		else if (network is AiAdNpcNetwork)
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)network;
			aiAdNpcNetwork.RequestNpcUseItem(objId);
		}
	}
}
