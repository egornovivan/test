using System.Data;

public class SceneIdDbData : DbRecordData
{
	public string ServerName;

	public byte[] Data;

	public void ExportData(string name, byte[] data)
	{
		mType = EDbOpType.OP_INSERT;
		ServerName = name;
		Data = data;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO sceneid(servername,ver,data) VALUES(@servername,@ver,@data);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@servername";
		dbDataParameter.DbType = DbType.String;
		dbDataParameter.Value = ServerName;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 273;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@data";
		dbDataParameter3.DbType = DbType.Binary;
		dbDataParameter3.Value = Data;
		cmd.Parameters.Add(dbDataParameter3);
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
