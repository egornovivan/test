using System.Data;

public class PlayerMissionData : DbRecordData
{
	public int Id;

	public byte[] Pmdata;

	public byte[] AdrmData;

	public void ExportData(int id, PlayerMission mission)
	{
		mType = EDbOpType.OP_INSERT;
		Id = id;
		Pmdata = mission.Export(0);
		if (ServerConfig.IsStory)
		{
			AdrmData = RMRepository.Export(mission, Id);
		}
		else
		{
			AdrmData = AdRMRepository.Export(Id, mission);
		}
	}

	public override void Exce(IDbCommand cmd)
	{
	}
}
