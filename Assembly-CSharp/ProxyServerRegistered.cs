using uLink;

public class ProxyServerRegistered : ServerRegistered
{
	public HostData ProxyServer;

	public override void AnalyseServer(HostData data, bool isLan)
	{
		base.AnalyseServer(data, isLan);
		ProxyServer = data;
		UseProxy = true;
	}
}
