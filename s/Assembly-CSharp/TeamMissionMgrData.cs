using System.Collections.Generic;
using System.Data;

public class TeamMissionMgrData : DbRecordData
{
	private List<TeamMissionData> mMissions = new List<TeamMissionData>();

	public void ExportData(Dictionary<int, PlayerMission> missions)
	{
		mType = EDbOpType.OP_INSERT;
		foreach (KeyValuePair<int, PlayerMission> mission in missions)
		{
			if (mission.Value.dirty)
			{
				TeamMissionData teamMissionData = new TeamMissionData();
				teamMissionData.ExportData(mission.Key, mission.Value);
				mMissions.Add(teamMissionData);
			}
		}
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO teammission VALUES(@team, @ver, @pmdata, @adrmdata, @missionitems);";
		cmd.CommandType = CommandType.Text;
		cmd.Prepare();
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@team";
		dbDataParameter.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@pmdata";
		dbDataParameter3.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@adrmdata";
		dbDataParameter4.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@missionitems";
		dbDataParameter5.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter5);
		foreach (TeamMissionData mMission in mMissions)
		{
			dbDataParameter.Value = mMission.Id;
			dbDataParameter2.Value = 272;
			dbDataParameter3.Value = mMission.Pmdata;
			dbDataParameter4.Value = mMission.AdrmData;
			dbDataParameter5.Value = mMission.MissionItems;
			cmd.ExecuteNonQuery();
		}
	}

	public override void Exce(IDbCommand cmd)
	{
		EDbOpType eDbOpType = mType;
		if (eDbOpType == EDbOpType.OP_INSERT)
		{
			Insert(cmd);
		}
	}
}
