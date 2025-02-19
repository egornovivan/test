using uLink;

public class BattleInfo
{
	internal int _group;

	internal int _deathCount;

	internal int _killCount;

	internal float _point;

	internal int _meat;

	internal int _site;

	internal bool IsBattleOver()
	{
		return _killCount >= BattleConstData.Instance._win_kill || _point >= BattleConstData.Instance._win_point || _site >= BattleConstData.Instance._win_site;
	}

	internal void Update(BattleInfo info)
	{
		_deathCount = info._deathCount;
		_killCount = info._killCount;
		_meat = info._meat;
		_site = info._site;
		_point = info._point;
	}

	internal static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		BattleInfo battleInfo = new BattleInfo();
		battleInfo._group = stream.Read<int>(new object[0]);
		battleInfo._deathCount = stream.Read<int>(new object[0]);
		battleInfo._killCount = stream.Read<int>(new object[0]);
		battleInfo._meat = stream.Read<int>(new object[0]);
		battleInfo._site = stream.Read<int>(new object[0]);
		battleInfo._point = stream.Read<float>(new object[0]);
		return battleInfo;
	}

	internal static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		BattleInfo battleInfo = (BattleInfo)value;
		stream.Write(battleInfo._group);
		stream.Write(battleInfo._deathCount);
		stream.Write(battleInfo._killCount);
		stream.Write(battleInfo._meat);
		stream.Write(battleInfo._site);
		stream.Write(battleInfo._point);
	}
}
