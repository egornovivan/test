using uLink;

public class PlayerBattleInfo
{
	public string Account;

	public string RoleName;

	public int Id;

	public BattleInfo Info;

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		PlayerBattleInfo playerBattleInfo = new PlayerBattleInfo();
		playerBattleInfo.Account = stream.Read<string>(new object[0]);
		playerBattleInfo.RoleName = stream.Read<string>(new object[0]);
		playerBattleInfo.Id = stream.Read<int>(new object[0]);
		playerBattleInfo.Info = stream.Read<BattleInfo>(new object[0]);
		return playerBattleInfo;
	}

	public static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		PlayerBattleInfo playerBattleInfo = (PlayerBattleInfo)value;
		stream.Write(playerBattleInfo.Account);
		stream.Write(playerBattleInfo.RoleName);
		stream.Write(playerBattleInfo.Id);
		stream.Write(playerBattleInfo.Info);
	}
}
