using System.Collections.Generic;
using System.Data;

public class DoodadMgrData : DbRecordData
{
	private List<DoodadData> mDoodads = new List<DoodadData>();

	public void ExportData(List<IDoodad> doodads)
	{
		mType = EDbOpType.OP_INSERT;
		foreach (IDoodad doodad in doodads)
		{
			DoodadData doodadData = new DoodadData();
			doodadData.ExportData(doodad);
			mDoodads.Add(doodadData);
		}
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO doodaddata(id,ver,objtype,data) VALUES (@id,@ver,@objtype,@data);";
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
		dbDataParameter3.ParameterName = "@objtype";
		dbDataParameter3.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@data";
		dbDataParameter4.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter4);
		foreach (DoodadData mDoodad in mDoodads)
		{
			dbDataParameter.Value = mDoodad.Id;
			dbDataParameter3.Value = mDoodad.Type;
			dbDataParameter4.Value = mDoodad.Data;
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
