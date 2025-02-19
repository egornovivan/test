using System.Collections.Generic;
using System.Data;

public class ColonyNpcMgrData : DbRecordData
{
	private List<ColonyNpcData> npcList = new List<ColonyNpcData>();

	public void ExportData(IEnumerable<ColonyNpc> npcs)
	{
		mType = EDbOpType.OP_INSERT;
		foreach (ColonyNpc npc in npcs)
		{
			ColonyNpcData colonyNpcData = new ColonyNpcData();
			colonyNpcData.ExportData(npc);
			npcList.Add(colonyNpcData);
		}
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO colonynpc VALUES(@id, @ver, @teamid, @state, @dwellingsid, @workroomid, @occupation, @workmode, @blobdata);";
		cmd.CommandType = CommandType.Text;
		cmd.Prepare();
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 272;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@teamid";
		dbDataParameter3.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@state";
		dbDataParameter4.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@dwellingsid";
		dbDataParameter5.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@workroomid";
		dbDataParameter6.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter6);
		IDbDataParameter dbDataParameter7 = cmd.CreateParameter();
		dbDataParameter7.ParameterName = "@occupation";
		dbDataParameter7.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter7);
		IDbDataParameter dbDataParameter8 = cmd.CreateParameter();
		dbDataParameter8.ParameterName = "@workmode";
		dbDataParameter8.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter8);
		IDbDataParameter dbDataParameter9 = cmd.CreateParameter();
		dbDataParameter9.ParameterName = "@blobdata";
		dbDataParameter9.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter9);
		foreach (ColonyNpcData npc in npcList)
		{
			dbDataParameter.Value = npc.Id;
			dbDataParameter3.Value = npc.TeamId;
			dbDataParameter4.Value = npc.State;
			dbDataParameter5.Value = npc.DwellingsId;
			dbDataParameter6.Value = npc.WorkRoomId;
			dbDataParameter7.Value = npc.Occupation;
			dbDataParameter8.Value = npc.WorkMode;
			dbDataParameter9.Value = npc.Data;
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
