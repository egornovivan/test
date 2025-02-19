using uLink;

public class BuildingID
{
	public int townId;

	public int buildingNo;

	public BuildingID()
	{
	}

	public BuildingID(int townId, int buildingNo)
	{
		this.townId = townId;
		this.buildingNo = buildingNo;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		BuildingID buildingID = (BuildingID)obj;
		return townId == buildingID.townId && buildingNo == buildingID.buildingNo;
	}

	internal static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		return new BuildingID(num, num2);
	}

	internal static void Serialize(BitStream stream, object value, params object[] codecOptions)
	{
		BuildingID buildingID = value as BuildingID;
		stream.Write(buildingID.townId);
		stream.Write(buildingID.buildingNo);
	}

	public override int GetHashCode()
	{
		return townId + (buildingNo << 16);
	}

	public override string ToString()
	{
		return $"[{townId}-{buildingNo}]";
	}
}
