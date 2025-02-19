using System.Data;
using System.IO;
using PETools;

public class NpcData : DbRecordData
{
	public int Id;

	public int WorldId;

	public byte[] BaseData;

	public byte[] CustomizedData;

	public byte[] ExternalData;

	public byte[] MissionData;

	public void ExportData(AiAdNpcNetwork net)
	{
		mType = EDbOpType.OP_INSERT;
		Id = net.Id;
		WorldId = net.WorldId;
		BaseData = Serialize.Export(delegate(BinaryWriter w)
		{
			net.ExportSpawnInfo(w);
		});
		CustomizedData = net.CustomizedData.Serialize();
		MissionData = net.mission.Serialize();
		ExternalData = Serialize.Export(delegate(BinaryWriter w)
		{
			net.ExportBaseInfo(w);
			net.ExportCmpt(w);
		});
	}

	public void UpdateData(AiAdNpcNetwork net)
	{
		mType = EDbOpType.OP_UPDATE;
		Id = net.Id;
		WorldId = net.WorldId;
		BaseData = Serialize.Export(delegate(BinaryWriter w)
		{
			net.ExportSpawnInfo(w);
		});
		CustomizedData = net.CustomizedData.Serialize();
		MissionData = net.mission.Serialize();
		ExternalData = Serialize.Export(delegate(BinaryWriter w)
		{
			net.ExportBaseInfo(w);
			net.ExportCmpt(w);
		});
	}

	public void DeleteData(AiAdNpcNetwork npc)
	{
		mType = EDbOpType.OP_DELETE;
		Id = npc.Id;
	}

	private void Update(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "UPDATE npcdata SET basedata=@basedata,externdata=@externdata,missiondata=@missiondata WHERE id=@id;";
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@basedata";
		dbDataParameter2.DbType = DbType.Binary;
		dbDataParameter2.Value = BaseData;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@externdata";
		dbDataParameter3.DbType = DbType.Binary;
		dbDataParameter3.Value = ExternalData;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@missiondata";
		dbDataParameter4.DbType = DbType.Binary;
		dbDataParameter4.Value = MissionData;
		cmd.Parameters.Add(dbDataParameter4);
		cmd.ExecuteNonQuery();
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "INSERT OR REPLACE INTO npcdata(id,ver,worldid,basedata,customdata,externdata,missiondata) VALUES(@id,@ver,@worldid,@basedata,@customdata,@externdata,@missiondata);";
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@id";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		dbDataParameter2.Value = 272;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@worldid";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = WorldId;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@basedata";
		dbDataParameter4.DbType = DbType.Binary;
		dbDataParameter4.Value = BaseData;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@customdata";
		dbDataParameter5.DbType = DbType.Binary;
		dbDataParameter5.Value = CustomizedData;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@externdata";
		dbDataParameter6.DbType = DbType.Binary;
		dbDataParameter6.Value = ExternalData;
		cmd.Parameters.Add(dbDataParameter6);
		IDbDataParameter dbDataParameter7 = cmd.CreateParameter();
		dbDataParameter7.ParameterName = "@missiondata";
		dbDataParameter7.DbType = DbType.Binary;
		dbDataParameter7.Value = MissionData;
		cmd.Parameters.Add(dbDataParameter7);
		cmd.ExecuteNonQuery();
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "DELETE FROM npcdata WHERE id=@id;";
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
			Update(cmd);
			break;
		}
	}
}
