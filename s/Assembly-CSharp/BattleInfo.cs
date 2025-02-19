using uLink;

public class BattleInfo
{
	public int TeamId;

	public int DeathCount;

	public int KillCount;

	public float Point;

	public int Money;

	public int SiteCount;

	public BattleInfo()
	{
	}

	public BattleInfo(int teamID)
	{
		TeamId = teamID;
	}

	internal bool IsBattleOver()
	{
		return KillCount >= BattleConstData.Instance._win_kill || Point >= BattleConstData.Instance._win_point || SiteCount >= BattleConstData.Instance._win_site;
	}

	public void Reset()
	{
		DeathCount = 0;
		KillCount = 0;
		Point = 0f;
		Money = 0;
		SiteCount = 0;
	}

	internal void Update(BattleInfo info)
	{
		DeathCount = info.DeathCount;
		KillCount = info.KillCount;
		Money = info.Money;
		SiteCount = info.SiteCount;
		Point = info.Point;
	}

	internal static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		BattleInfo battleInfo = new BattleInfo();
		battleInfo.TeamId = stream.Read<int>(new object[0]);
		battleInfo.DeathCount = stream.Read<int>(new object[0]);
		battleInfo.KillCount = stream.Read<int>(new object[0]);
		battleInfo.Money = stream.Read<int>(new object[0]);
		battleInfo.SiteCount = stream.Read<int>(new object[0]);
		battleInfo.Point = stream.Read<float>(new object[0]);
		return battleInfo;
	}

	internal static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		BattleInfo battleInfo = (BattleInfo)value;
		stream.Write(battleInfo.TeamId);
		stream.Write(battleInfo.DeathCount);
		stream.Write(battleInfo.KillCount);
		stream.Write(battleInfo.Money);
		stream.Write(battleInfo.SiteCount);
		stream.Write(battleInfo.Point);
	}
}
