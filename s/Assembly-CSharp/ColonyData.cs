using System.Data;
using System.IO;
using PETools;

public class ColonyData : DbRecordData
{
	public int Id;

	public int ExternId;

	public int TeamId;

	public int Type;

	public float Durability;

	public float CurRepairTime;

	public float RepairTime;

	public float RepairValue;

	public float CurDeleteTime;

	public float DeleteTime;

	public byte[] Data;

	public void ExportData(ColonyBase colony)
	{
		mType = EDbOpType.OP_INSERT;
		Id = colony._Network.Id;
		ExternId = colony._Network.ExternId;
		Type = colony._RecordData.dType;
		Durability = colony.Durability;
		CurRepairTime = colony._RecordData.m_CurRepairTime;
		RepairTime = colony._RecordData.m_RepairTime;
		RepairValue = colony._RecordData.m_RepairValue;
		CurDeleteTime = colony._RecordData.m_CurDeleteTime;
		DeleteTime = colony._RecordData.m_DeleteTime;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			colony.CombomData(w);
		});
	}

	public void DeleteData(ColonyBase colony)
	{
		mType = EDbOpType.OP_DELETE;
		Id = colony._Network.Id;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "INSERT OR REPLACE INTO colony(id,ver,itemid,type,durability,currepairtime,repairtime,repairvalue,curdeletetime,deletetime,data) VALUES(@id,@ver,@itemid,@type,@durability,@currepairtime,@repairtime,@repairvalue,@curdeletetime,@deletetime,@data);";
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 2016102100;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@itemid";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = ExternId;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@type";
		dbDataParameter4.DbType = DbType.Int32;
		dbDataParameter4.Value = Type;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@durability";
		dbDataParameter5.DbType = DbType.Single;
		dbDataParameter5.Value = Durability;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@currepairtime";
		dbDataParameter6.DbType = DbType.Single;
		dbDataParameter6.Value = CurRepairTime;
		cmd.Parameters.Add(dbDataParameter6);
		IDbDataParameter dbDataParameter7 = cmd.CreateParameter();
		dbDataParameter7.ParameterName = "@repairtime";
		dbDataParameter7.DbType = DbType.Single;
		dbDataParameter7.Value = RepairTime;
		cmd.Parameters.Add(dbDataParameter7);
		IDbDataParameter dbDataParameter8 = cmd.CreateParameter();
		dbDataParameter8.ParameterName = "@repairvalue";
		dbDataParameter8.DbType = DbType.Single;
		dbDataParameter8.Value = RepairValue;
		cmd.Parameters.Add(dbDataParameter8);
		IDbDataParameter dbDataParameter9 = cmd.CreateParameter();
		dbDataParameter9.ParameterName = "@curdeletetime";
		dbDataParameter9.DbType = DbType.Single;
		dbDataParameter9.Value = CurDeleteTime;
		cmd.Parameters.Add(dbDataParameter9);
		IDbDataParameter dbDataParameter10 = cmd.CreateParameter();
		dbDataParameter10.ParameterName = "@deletetime";
		dbDataParameter10.DbType = DbType.Single;
		dbDataParameter10.Value = DeleteTime;
		cmd.Parameters.Add(dbDataParameter10);
		IDbDataParameter dbDataParameter11 = cmd.CreateParameter();
		dbDataParameter11.ParameterName = "@data";
		dbDataParameter11.DbType = DbType.Binary;
		dbDataParameter11.Value = Data;
		cmd.Parameters.Add(dbDataParameter11);
		cmd.ExecuteNonQuery();
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "DELETE FROM colony WHERE id=@id;";
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
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
