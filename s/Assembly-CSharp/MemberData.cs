using uLink;

public class MemberData
{
	private int mId;

	private NetworkPlayer mPeer;

	public int Id => mId;

	public NetworkPlayer Peer => mPeer;

	public MemberData(int id, NetworkPlayer peer)
	{
		mId = id;
		mPeer = peer;
	}
}
