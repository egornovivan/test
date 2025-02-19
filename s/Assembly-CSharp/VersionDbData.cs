using System.Data;
using System.IO;
using PETools;

public class VersionDbData : DbRecordData
{
	public int Id;

	public int VerNo;

	public string VerName;

	public byte[] Data;

	public void ExportData(int id, int no, string name)
	{
		mType = EDbOpType.OP_INSERT;
		Id = id;
		VerNo = no;
		VerName = name;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, id);
		});
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO versiondata(id,ver,versionname,versionno,blobdata) VALUES(@id,@ver,@versionname,@versionno,@blobdata);";
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
		dbDataParameter3.ParameterName = "@versionname";
		dbDataParameter3.DbType = DbType.String;
		dbDataParameter3.Value = VerName;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@versionno";
		dbDataParameter4.DbType = DbType.Int32;
		dbDataParameter4.Value = VerNo;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@blobdata";
		dbDataParameter5.DbType = DbType.Binary;
		dbDataParameter5.Value = Data;
		dbDataParameter5.Direction = ParameterDirection.Input;
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
