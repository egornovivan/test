using System.Data;
using System.IO;
using PETools;

public class SceneObjectDbData : DbRecordData
{
	public int Id;

	public int Type;

	public byte[] Data;

	public void ExportData(SceneObject obj)
	{
		mType = EDbOpType.OP_INSERT;
		Id = obj.Id;
		Type = (int)obj.Type;
		Data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, obj.Pos);
			BufferHelper.Serialize(w, obj.Scale);
			BufferHelper.Serialize(w, obj.Rot);
			BufferHelper.Serialize(w, obj.WorldId);
			BufferHelper.Serialize(w, obj.ProtoId);
		});
	}

	public void DeleteData(int id)
	{
		mType = EDbOpType.OP_DELETE;
		Id = id;
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO sceneobj(id,ver,type,data) VALUES(@id,@ver,@type,@data);";
		cmd.CommandType = CommandType.Text;
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
		dbDataParameter3.ParameterName = "@type";
		dbDataParameter3.DbType = DbType.Int32;
		dbDataParameter3.Value = Type;
		cmd.Parameters.Add(dbDataParameter3);
		IDbDataParameter dbDataParameter4 = cmd.CreateParameter();
		dbDataParameter4.ParameterName = "@data";
		dbDataParameter4.DbType = DbType.Binary;
		dbDataParameter4.Value = Data;
		cmd.Parameters.Add(dbDataParameter4);
		cmd.ExecuteNonQuery();
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandText = "DELETE FROM sceneobj WHERE id=@id;";
		cmd.CommandType = CommandType.Text;
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
		case EDbOpType.OP_INSERT:
			Insert(cmd);
			break;
		case EDbOpType.OP_DELETE:
			Delete(cmd);
			break;
		case EDbOpType.OP_UPDATE:
			break;
		}
	}
}
