using System.Data;
using System.IO;
using PETools;

public class ColonyNpcData : DbRecordData
{
	public int Id;

	public int TeamId;

	public int State;

	public int DwellingsId;

	public int WorkRoomId;

	public int Occupation;

	public int WorkMode;

	public byte[] Data;

	public void ExportData(ColonyNpc npc)
	{
		mType = EDbOpType.OP_INSERT;
		Id = npc._npcID;
		TeamId = npc.TeamId;
		State = npc.m_State;
		DwellingsId = npc.m_DwellingsID;
		WorkRoomId = npc.m_WorkRoomID;
		Occupation = npc.m_Occupation;
		WorkMode = npc.m_WorkMode;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, npc.m_ProcessingIndex);
			BufferHelper.Serialize(w, npc.m_IsProcessing);
			BufferHelper.Serialize(w, npc.m_GuardPos);
			BufferHelper.Serialize(w, (int)npc.trainerType);
			BufferHelper.Serialize(w, (int)npc.trainingType);
			BufferHelper.Serialize(w, npc.IsTraining);
		});
	}

	public void DeleteData(int npcId)
	{
		mType = EDbOpType.OP_DELETE;
		Id = npcId;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO colonynpc VALUES(@id, @ver, @teamid, @state, @dwellingsid, @workroomid, @occupation, @workmode, @blobdata);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 273;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@teamid";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = TeamId;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@state";
		dbDataParameter4.DbType = DbType.Int32;
		dbDataParameter4.Value = State;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@dwellingsid";
		dbDataParameter5.DbType = DbType.Int32;
		dbDataParameter5.Value = DwellingsId;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@workroomid";
		dbDataParameter6.DbType = DbType.Int32;
		dbDataParameter6.Value = WorkRoomId;
		cmd.Parameters.Add(dbDataParameter6);
		IDbDataParameter dbDataParameter7 = cmd.CreateParameter();
		dbDataParameter7.ParameterName = "@occupation";
		dbDataParameter7.DbType = DbType.Int32;
		dbDataParameter7.Value = Occupation;
		cmd.Parameters.Add(dbDataParameter7);
		IDbDataParameter dbDataParameter8 = cmd.CreateParameter();
		dbDataParameter8.ParameterName = "@workmode";
		dbDataParameter8.DbType = DbType.Int32;
		dbDataParameter8.Value = WorkMode;
		cmd.Parameters.Add(dbDataParameter8);
		IDbDataParameter dbDataParameter9 = cmd.CreateParameter();
		dbDataParameter9.ParameterName = "@blobdata";
		dbDataParameter9.DbType = DbType.Binary;
		dbDataParameter9.Value = Data;
		cmd.Parameters.Add(dbDataParameter9);
		cmd.ExecuteNonQuery();
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandText = "DELETE FROM colonynpc WHERE id=@id;";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		cmd.ExecuteNonQuery();
	}

	public override void Exce(IDbCommand cmd)
	{
		switch (mType)
		{
		case EDbOpType.OP_DELETE:
			Delete(cmd);
			break;
		case EDbOpType.OP_INSERT:
			Insert(cmd);
			break;
		case EDbOpType.OP_UPDATE:
			break;
		}
	}
}
