using System.Data;

public abstract class DbRecordData
{
	protected EDbOpType mType;

	public abstract void Exce(IDbCommand cmd);
}
