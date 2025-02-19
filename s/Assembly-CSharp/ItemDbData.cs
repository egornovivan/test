using System.Data;
using ItemAsset;

public class ItemDbData : DbRecordData
{
	public int Id;

	public byte[] Data;

	public void ExportData(ItemObject item)
	{
		mType = EDbOpType.OP_INSERT;
		Id = item.instanceId;
		Data = item.Export();
	}

	public void UpdateData(ItemObject item)
	{
		mType = EDbOpType.OP_UPDATE;
		Id = item.instanceId;
		Data = item.Export();
	}

	public void DeleteData(int id)
	{
		mType = EDbOpType.OP_DELETE;
		Id = id;
	}

	public override void Exce(IDbCommand cmd)
	{
	}
}
