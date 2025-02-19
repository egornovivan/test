using uLink;

namespace CustomData;

public class TMsgInfo
{
	public EMsgType msgtype;

	public int sendRoleId;

	public int recvRoleId;

	public string msg;

	public static void WriteMsg(BitStream stream, object obj, params object[] codecOptions)
	{
		TMsgInfo tMsgInfo = (TMsgInfo)obj;
		stream.Write(tMsgInfo.msgtype);
		stream.Write(tMsgInfo.sendRoleId);
		stream.Write(tMsgInfo.recvRoleId);
		stream.Write(tMsgInfo.msg);
	}

	public static object ReadMsg(BitStream stream, params object[] codecOptions)
	{
		TMsgInfo tMsgInfo = new TMsgInfo();
		tMsgInfo.msgtype = stream.Read<EMsgType>(new object[0]);
		tMsgInfo.sendRoleId = stream.Read<int>(new object[0]);
		tMsgInfo.recvRoleId = stream.Read<int>(new object[0]);
		tMsgInfo.msg = stream.Read<string>(new object[0]);
		return tMsgInfo;
	}
}
