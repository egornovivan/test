using System.Data;

public class PlayerWorldData : DbRecordData
{
	public int Id;

	public int WorldId;

	public byte[] Data;

	public void ExportData(int id, int worldId, byte[] data)
	{
		mType = EDbOpType.OP_INSERT;
		Id = id;
		WorldId = worldId;
		Data = data;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO playergameworld(roleid,ver,worldid,data) VALUES(@roleid,@ver,@worldid,@data);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@roleid";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 273;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@worldid";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = WorldId;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@data";
		dbDataParameter4.DbType = DbType.Binary;
		dbDataParameter4.Value = Data;
		cmd.Parameters.Add(dbDataParameter4);
		cmd.ExecuteNonQuery();
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
