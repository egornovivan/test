using System.Data;

public class RegISODbData : DbRecordData
{
	public long HashCode;

	public long UGCHandle;

	public string Name;

	public byte[] Data;

	public void ExportData(long hashCode, long handle, string name, byte[] data)
	{
		mType = EDbOpType.OP_INSERT;
		HashCode = hashCode;
		UGCHandle = handle;
		Name = name;
		Data = data;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO registerediso VALUES(@hash,@ver,@handle,@name,@data);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@hash";
		dbDataParameter.DbType = DbType.Int64;
		dbDataParameter.Value = HashCode;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 272;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@handle";
		dbDataParameter3.DbType = DbType.Int64;
		dbDataParameter3.Value = UGCHandle;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@name";
		dbDataParameter4.DbType = DbType.String;
		dbDataParameter4.Value = Name;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@data";
		dbDataParameter5.DbType = DbType.Binary;
		dbDataParameter5.Value = Data;
		cmd.Parameters.Add(dbDataParameter5);
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
