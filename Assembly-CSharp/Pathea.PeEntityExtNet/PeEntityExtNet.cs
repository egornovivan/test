namespace Pathea.PeEntityExtNet;

public static class PeEntityExtNet
{
	public static NetworkInterface GetNetwork(this PeEntity entity)
	{
		if (null == entity)
		{
			return null;
		}
		NetCmpt cmpt = entity.GetCmpt<NetCmpt>();
		if (null == cmpt)
		{
			return null;
		}
		return cmpt.network;
	}
}
