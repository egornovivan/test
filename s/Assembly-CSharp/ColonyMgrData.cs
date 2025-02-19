using System.Data;
using System.IO;
using PETools;

public class ColonyMgrData : DbRecordData
{
	public int TeamId;

	public byte[] Data;

	public void ExportData(int teamId)
	{
		mType = EDbOpType.OP_INSERT;
		TeamId = teamId;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			ColonyMgr.CombomData(w, teamId);
		});
	}

	public void DeleteData(int teamId)
	{
		mType = EDbOpType.OP_DELETE;
		TeamId = teamId;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "INSERT OR REPLACE INTO colonydata(id,ver,data) VALUES(@id,@ver,@data);";
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = TeamId;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 2016102100;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@data";
		dbDataParameter3.DbType = DbType.Binary;
		dbDataParameter3.Value = Data;
		cmd.Parameters.Add(dbDataParameter3);
		cmd.ExecuteNonQuery();
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "DELETE FROM colonydata WHERE id=@id;";
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = TeamId;
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
