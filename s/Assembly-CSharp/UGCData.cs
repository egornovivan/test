using System.Data;

public class UGCData : DbRecordData
{
	public int Id;

	public int Seed;

	public float MaxHp;

	public float Hp;

	public float MaxFuel;

	public float Fuel;

	public long HashCode;

	public long SteamId;

	public void ExportData(CreationOriginData data)
	{
		mType = EDbOpType.OP_INSERT;
		Id = data.ObjectID;
		Seed = data.Seed;
		MaxHp = data.MaxHP;
		Hp = data.HP;
		MaxFuel = data.MaxFuel;
		Fuel = data.Fuel;
		HashCode = (long)data.HashCode;
		SteamId = (long)data.SteamId;
	}

	public void UpdateData(CreationNetwork net)
	{
		mType = EDbOpType.OP_UPDATE;
		Id = net.Id;
		Hp = net.HP;
		Fuel = net.Fuel;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO creationdata VALUES(@objid,@ver,@seed,@hp,@maxhp,@fuel,@maxfuel,@hash,@steamid);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@objid";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 272;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@seed";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = Seed;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@maxhp";
		dbDataParameter4.DbType = DbType.Single;
		dbDataParameter4.Value = MaxHp;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@hp";
		dbDataParameter5.DbType = DbType.Single;
		dbDataParameter5.Value = Hp;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@maxfuel";
		dbDataParameter6.DbType = DbType.Single;
		dbDataParameter6.Value = MaxFuel;
		cmd.Parameters.Add(dbDataParameter6);
		IDbDataParameter dbDataParameter7 = cmd.CreateParameter();
		dbDataParameter7.ParameterName = "@fuel";
		dbDataParameter7.DbType = DbType.Single;
		dbDataParameter7.Value = Fuel;
		cmd.Parameters.Add(dbDataParameter7);
		IDbDataParameter dbDataParameter8 = cmd.CreateParameter();
		dbDataParameter8.ParameterName = "@hash";
		dbDataParameter8.DbType = DbType.Int64;
		dbDataParameter8.Value = HashCode;
		cmd.Parameters.Add(dbDataParameter8);
		IDbDataParameter dbDataParameter9 = cmd.CreateParameter();
		dbDataParameter9.ParameterName = "@steamid";
		dbDataParameter9.DbType = DbType.Int64;
		dbDataParameter9.Value = SteamId;
		cmd.Parameters.Add(dbDataParameter9);
		cmd.ExecuteNonQuery();
	}

	private void Update(IDbCommand cmd)
	{
		cmd.CommandText = "UPDATE creationdata SET ver=@ver,hp=@hp,fuel=@fuel WHERE objid=@objid;";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@objid";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 272;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@hp";
		dbDataParameter3.DbType = DbType.Single;
		dbDataParameter3.Value = Hp;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@fuel";
		dbDataParameter4.DbType = DbType.Single;
		dbDataParameter4.Value = Fuel;
		cmd.Parameters.Add(dbDataParameter4);
		cmd.ExecuteNonQuery();
	}

	public override void Exce(IDbCommand cmd)
	{
		switch (mType)
		{
		case EDbOpType.OP_INSERT:
			Insert(cmd);
			break;
		case EDbOpType.OP_UPDATE:
			Update(cmd);
			break;
		}
	}
}
