using uLink;

namespace CustomData;

public class RoleInfoProxy
{
	public byte level = 1;

	public byte sex = 1;

	public int winrate;

	public int roleID = -1;

	public ulong steamId;

	public string name = "NickName";

	public float lobbyExp;

	public override string ToString()
	{
		return $"[RoleInfo][name:{name}, sex:{sex}, level:{level}]";
	}

	public static void WriteRoleInfoProxy(BitStream stream, object obj, params object[] codecOptions)
	{
		RoleInfoProxy roleInfoProxy = obj as RoleInfoProxy;
		stream.Write(roleInfoProxy.steamId);
		stream.Write(roleInfoProxy.level);
		stream.Write(roleInfoProxy.winrate);
		stream.Write(roleInfoProxy.roleID);
		stream.Write(roleInfoProxy.sex);
		stream.Write(roleInfoProxy.name);
		stream.Write(roleInfoProxy.lobbyExp);
	}

	public static object ReadRoleInfoProxy(BitStream stream, params object[] codecOptions)
	{
		RoleInfoProxy roleInfoProxy = new RoleInfoProxy();
		roleInfoProxy.steamId = stream.Read<ulong>(new object[0]);
		roleInfoProxy.level = stream.Read<byte>(new object[0]);
		roleInfoProxy.winrate = stream.Read<int>(new object[0]);
		roleInfoProxy.roleID = stream.Read<int>(new object[0]);
		roleInfoProxy.sex = stream.Read<byte>(new object[0]);
		roleInfoProxy.name = stream.Read<string>(new object[0]);
		roleInfoProxy.lobbyExp = stream.Read<float>(new object[0]);
		return roleInfoProxy;
	}
}
