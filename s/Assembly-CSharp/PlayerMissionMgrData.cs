using System.Collections.Generic;
using System.Data;

public class PlayerMissionMgrData : DbRecordData
{
	private List<PlayerMissionData> mMissions = new List<PlayerMissionData>();

	public void ExportData(Dictionary<int, PlayerMission> missions)
	{
		mType = EDbOpType.OP_INSERT;
		foreach (KeyValuePair<int, PlayerMission> mission in missions)
		{
			if (mission.Value.dirty)
			{
				PlayerMissionData playerMissionData = new PlayerMissionData();
				playerMissionData.ExportData(mission.Key, mission.Value);
				mMissions.Add(playerMissionData);
			}
		}
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO playermission VALUES(@roleid, @ver, @pmdata, @adrmdata);";
		cmd.CommandType = CommandType.Text;
		cmd.Prepare();
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@roleid";
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
		foreach (PlayerMissionData mMission in mMissions)
		{
			dbDataParameter.Value = mMission.Id;
			dbDataParameter2.Value = 272;
			dbDataParameter3.Value = mMission.Pmdata;
			dbDataParameter4.Value = mMission.AdrmData;
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
