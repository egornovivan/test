using System.Data;

public class CustomDialogsData : DbRecordData
{
	public byte[] data;

	public void ExportData(byte[] dialogs)
	{
		mType = EDbOpType.OP_INSERT;
		data = dialogs;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO customdialogs(dummyid,ver,data) VALUES(1,@ver,@data);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@ver";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = 272;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@data";
		dbDataParameter2.DbType = DbType.Binary;
		dbDataParameter2.Value = data;
		cmd.Parameters.Add(dbDataParameter2);
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
