using System.Collections.Generic;
using System.Data;
using System.IO;

public class CustomNpcData : DbRecordData
{
	public byte[] data;

	public void ExportData(Dictionary<int, List<CustomObjData>> customObjs)
	{
		mType = EDbOpType.OP_INSERT;
		data = BufferHelper.Export(delegate(BinaryWriter w)
		{
			w.Write(customObjs.Count);
			foreach (KeyValuePair<int, List<CustomObjData>> customObj in customObjs)
			{
				w.Write(customObj.Key);
				w.Write(customObj.Value.Count);
				foreach (CustomObjData item in customObj.Value)
				{
					w.Write(item.Id);
					w.Write(item.CustomId);
					w.Write(item.ProtoId);
					w.Write(item.DefaultPlayerId);
				}
			}
		});
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO customnpcs(dummyid,ver,data) VALUES(1,@ver,@data);";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@ver";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = 273;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@data";
		dbDataParameter2.DbType = DbType.Binary;
		dbDataParameter2.Value = data;
		cmd.Parameters.Add(dbDataParameter2);
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
