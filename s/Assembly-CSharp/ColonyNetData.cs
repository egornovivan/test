using System.Data;
using System.IO;
using PETools;

public class ColonyNetData : DbRecordData
{
	public int Id;

	public int ExternId;

	public int TeamId;

	public int Type;

	public byte[] Data;

	public void ExportData(ColonyNetwork net)
	{
		mType = EDbOpType.OP_INSERT;
		Id = net.Id;
		ExternId = net.ExternId;
		TeamId = net.TeamId;
		Type = 5;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, net.transform.position);
			BufferHelper.Serialize(w, net.transform.rotation);
			BufferHelper.Serialize(w, net.ownerId);
			BufferHelper.Serialize(w, net.WorldId);
		});
	}

	public void ExportData(ColonyNetworkRenewData net)
	{
		mType = EDbOpType.OP_INSERT;
		Id = net.id;
		ExternId = net.externId;
		TeamId = net.teamId;
		Type = 5;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, net.pos);
			BufferHelper.Serialize(w, net.rot);
			BufferHelper.Serialize(w, net.ownerId);
			BufferHelper.Serialize(w, net.worldId);
		});
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = "INSERT OR REPLACE INTO networkobj(id,ver,externid,teamnum,itemtype,blobdata) VALUES(@id,@ver,@externid,@teamnum,@itemtype,@data);";
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
		dbDataParameter3.ParameterName = "@externid";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = ExternId;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@teamnum";
		dbDataParameter4.DbType = DbType.Int32;
		dbDataParameter4.Value = TeamId;
		cmd.Parameters.Add(dbDataParameter4);
		IDbDataParameter dbDataParameter5 = cmd.CreateParameter();
		dbDataParameter5.ParameterName = "@itemtype";
		dbDataParameter5.DbType = DbType.Int32;
		dbDataParameter5.Value = Type;
		cmd.Parameters.Add(dbDataParameter5);
		IDbDataParameter dbDataParameter6 = cmd.CreateParameter();
		dbDataParameter6.ParameterName = "@data";
		dbDataParameter6.DbType = DbType.Binary;
		dbDataParameter6.Value = Data;
		cmd.Parameters.Add(dbDataParameter6);
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
