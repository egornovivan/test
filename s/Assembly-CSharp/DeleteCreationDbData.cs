using System.Data;

public class DeleteCreationDbData : DbRecordData
{
	public int Id;

	public void DeleteData(int id)
	{
		mType = EDbOpType.OP_DELETE;
		Id = id;
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandText = "DELETE FROM creationdata WHERE objid=@objid;DELETE FROM itemobject WHERE objid=@objid;";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@objid";
		dbDataParameter.DbType = DbType.Int32;
		dbDataParameter.Value = Id;
		cmd.Parameters.Add(dbDataParameter);
		cmd.ExecuteNonQuery();
	}

	public override void Exce(IDbCommand cmd)
	{
		EDbOpType eDbOpType = mType;
		if (eDbOpType == EDbOpType.OP_DELETE)
		{
			Delete(cmd);
		}
	}
}
