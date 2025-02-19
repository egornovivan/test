using System.Data;

public class TeamMissionData : DbRecordData
{
	public int Id;

	public byte[] Pmdata;

	public byte[] AdrmData;

	public byte[] MissionItems;

	public void ExportData(int id, PlayerMission mission)
	{
		mType = EDbOpType.OP_INSERT;
		Id = id;
		Pmdata = mission.Export(1);
		if (ServerConfig.IsStory)
		{
			AdrmData = RMRepository.Export(mission, Id);
		}
		else
		{
			AdrmData = AdRMRepository.Export(id, mission);
		}
		MissionItems = MissionPackageMgr.Export(id);
	}

	public override void Exce(IDbCommand cmd)
	{
	}
}
