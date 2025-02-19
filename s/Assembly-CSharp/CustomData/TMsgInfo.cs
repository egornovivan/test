using uLink;

namespace CustomData;

public class TMsgInfo
{
	public EMsgType msgtype;

	public int sendRoleId;

	public int recvId;

	public string msg;

	public static void WriteMsg(BitStream stream, object obj, params object[] codecOptions)
	{
		TMsgInfo tMsgInfo = (TMsgInfo)obj;
		stream.Write(tMsgInfo.msgtype);
		stream.Write(tMsgInfo.sendRoleId);
		stream.Write(tMsgInfo.recvId);
		stream.Write(tMsgInfo.msg);
	}

	public static object ReadMsg(BitStream stream, params object[] codecOptions)
	{
		TMsgInfo tMsgInfo = new TMsgInfo();
		stream.TryRead<EMsgType>(out tMsgInfo.msgtype);
		stream.TryRead<int>(out tMsgInfo.sendRoleId);
		stream.TryRead<int>(out tMsgInfo.recvId);
		stream.TryRead<string>(out tMsgInfo.msg);
		return tMsgInfo;
	}
}
