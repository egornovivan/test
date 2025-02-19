using Pathea;
using uLink;

public class ConnectedServer
{
	public PeGameMgr.ESceneMode sceneMode;

	public NetworkPeer peer;

	public string serverName;

	public ConnectedServer(string serverName, int sceneMode, NetworkPeer peer)
	{
		this.serverName = serverName;
		this.sceneMode = (PeGameMgr.ESceneMode)sceneMode;
		this.peer = peer;
	}
}
