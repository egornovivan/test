using uLink;

public class CreationOriginData
{
	internal int ObjectID;

	internal int Seed;

	internal float HP;

	internal float MaxHP;

	internal float Fuel;

	internal float MaxFuel;

	internal ulong HashCode;

	internal ulong SteamId;

	internal bool free;

	internal int instanceId;

	public CreationOriginData()
	{
		HP = -1f;
		Fuel = -1f;
		instanceId = 0;
	}

	public static void Serialize(BitStream stream, object obj, params object[] codecOptions)
	{
		CreationOriginData creationOriginData = (CreationOriginData)obj;
		stream.Write(creationOriginData.SteamId);
		stream.Write(creationOriginData.ObjectID);
		stream.Write(creationOriginData.HashCode);
		stream.Write(creationOriginData.Seed);
		stream.Write(creationOriginData.Fuel);
		stream.Write(creationOriginData.HP);
	}

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		CreationOriginData creationOriginData = new CreationOriginData();
		creationOriginData.SteamId = stream.Read<ulong>(new object[0]);
		creationOriginData.ObjectID = stream.Read<int>(new object[0]);
		creationOriginData.HashCode = stream.Read<ulong>(new object[0]);
		creationOriginData.Seed = stream.Read<int>(new object[0]);
		creationOriginData.Fuel = stream.Read<float>(new object[0]);
		creationOriginData.HP = stream.Read<float>(new object[0]);
		return creationOriginData;
	}

	public void Save()
	{
		UGCData uGCData = new UGCData();
		uGCData.ExportData(this);
		AsyncSqlite.AddRecord(uGCData);
	}
}
