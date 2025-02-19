using System.Data;

public class AreaDbData : DbRecordData
{
	public int WorldId;

	public string Name;

	public byte[] TownData;

	public byte[] CampData;

	public void ExportData(GameWorld world)
	{
		mType = EDbOpType.OP_INSERT;
		WorldId = world.WorldId;
		Name = world.WorldName;
		TownData = world.GetTownAreaData();
		CampData = world.GetCampAreaData();
	}

	public override void Exce(IDbCommand cmd)
	{
	}
}
