using uLink;

namespace CustomData;

public class RoleInfo
{
	public ulong steamId;

	public int roleID = -1;

	public int winrate;

	public int deletedFlag;

	public byte level = 1;

	public byte sex = 1;

	public byte[] appearData;

	public byte[] nudeData;

	public string account = "NickName";

	public string name = "NickName";

	public float lobbyExp;

	public override string ToString()
	{
		return $"[RoleInfo][steamid:{steamId}, name:{name}, sex:{sex}, level:{level}]";
	}

	public static void WriteRoleInfo(BitStream stream, object obj, params object[] codecOptions)
	{
		RoleInfo roleInfo = (RoleInfo)obj;
		stream.Write(roleInfo.steamId);
		stream.Write(roleInfo.level);
		stream.Write(roleInfo.winrate);
		stream.Write(roleInfo.sex);
		stream.Write(roleInfo.name);
		stream.Write(roleInfo.deletedFlag);
		stream.Write(roleInfo.roleID);
		stream.Write(roleInfo.appearData);
		stream.Write(roleInfo.nudeData);
		stream.Write(roleInfo.lobbyExp);
	}

	public static object ReadRoleInfo(BitStream stream, params object[] codecOptions)
	{
		RoleInfo roleInfo = new RoleInfo();
		roleInfo.steamId = stream.Read<ulong>(new object[0]);
		roleInfo.level = stream.Read<byte>(new object[0]);
		roleInfo.winrate = stream.Read<int>(new object[0]);
		roleInfo.sex = stream.Read<byte>(new object[0]);
		roleInfo.name = stream.Read<string>(new object[0]);
		roleInfo.deletedFlag = stream.Read<int>(new object[0]);
		roleInfo.roleID = stream.Read<int>(new object[0]);
		roleInfo.appearData = stream.Read<byte[]>(new object[0]);
		roleInfo.nudeData = stream.Read<byte[]>(new object[0]);
		roleInfo.lobbyExp = stream.Read<float>(new object[0]);
		return roleInfo;
	}
}
