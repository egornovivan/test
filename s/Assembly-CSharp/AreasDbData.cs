using System.Collections.Generic;
using System.Data;

public class AreasDbData : DbRecordData
{
	public List<AreaDbData> mAreas = new List<AreaDbData>();

	public void ExportData(IEnumerable<GameWorld> worlds)
	{
		mType = EDbOpType.OP_INSERT;
		foreach (GameWorld world in worlds)
		{
			AreaDbData areaDbData = new AreaDbData();
			areaDbData.ExportData(world);
			mAreas.Add(areaDbData);
		}
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO areainfo(worldid,ver,servername,townarea,camparea) VALUES(@worldid,@ver,@servername,@townarea,@camparea);";
		cmd.CommandType = CommandType.Text;
		cmd.Prepare();
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@worldid";
		dbDataParameter.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@servername";
		dbDataParameter3.DbType = DbType.String;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@townarea";
		dbDataParameter4.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@camparea";
		dbDataParameter5.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter5);
		foreach (AreaDbData mArea in mAreas)
		{
			dbDataParameter.Value = mArea.WorldId;
			dbDataParameter2.Value = 273;
			dbDataParameter3.Value = mArea.Name;
			dbDataParameter4.Value = mArea.TownData;
			dbDataParameter5.Value = mArea.CampData;
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
