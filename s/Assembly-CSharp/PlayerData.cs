using System.Data;

public class PlayerData : DbRecordData
{
	public int Id;

	public string RoleName;

	public long SteamId;

	public int TeamId;

	public int WorldId;

	public byte[] Pos;

	public byte[] Data;

	public void ExportData(int id, string name, long steamId, int teamId, int worldId, byte[] pos, byte[] data)
	{
		mType = EDbOpType.OP_INSERT;
		Id = id;
		RoleName = name;
		SteamId = steamId;
		TeamId = teamId;
		WorldId = worldId;
		Pos = pos;
		Data = data;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO player VALUES(@roleid,@ver,@rolename,@steamid,@teamid,@worldid,@pos,@playerdata);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@roleid";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 272;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@rolename";
		dbDataParameter3.DbType = DbType.String;
		dbDataParameter3.Value = RoleName;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@steamid";
		dbDataParameter4.DbType = DbType.Int64;
		dbDataParameter4.Value = SteamId;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@teamid";
		dbDataParameter5.DbType = DbType.Int32;
		dbDataParameter5.Value = TeamId;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@worldid";
		dbDataParameter6.DbType = DbType.Int32;
		dbDataParameter6.Value = WorldId;
		cmd.Parameters.Add(dbDataParameter6);
		IDbDataParameter dbDataParameter7 = cmd.CreateParameter();
		dbDataParameter7.ParameterName = "@pos";
		dbDataParameter7.DbType = DbType.Binary;
		dbDataParameter7.Value = Pos;
		cmd.Parameters.Add(dbDataParameter7);
		IDbDataParameter dbDataParameter8 = cmd.CreateParameter();
		dbDataParameter8.ParameterName = "@playerdata";
		dbDataParameter8.DbType = DbType.Binary;
		dbDataParameter8.Value = Data;
		cmd.Parameters.Add(dbDataParameter8);
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
