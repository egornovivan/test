using System.Data;

public class PublicNetData : DbRecordData
{
	public int Id;

	public byte[] Data;

	public void ExportData(int id, byte[] data)
	{
		mType = EDbOpType.OP_INSERT;
		Id = id;
		Data = data;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO publicdata(id,ver,data) VALUES(@id,@ver,@data);";
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
