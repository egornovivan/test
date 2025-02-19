using System.Data;

public class DeleteCreationRgeDbData : DbRecordData
{
	public long _hash;

	public void DeleteData(long hash)
	{
		mType = EDbOpType.OP_DELETE;
		_hash = hash;
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandText = "DELETE FROM registerediso WHERE hash=@hashcode;";
		cmd.CommandType = CommandType.Text;
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@hashcode";
		dbDataParameter.DbType = DbType.Int64;
		dbDataParameter.Value = _hash;
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
