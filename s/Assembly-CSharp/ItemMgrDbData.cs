using System.Collections.Generic;
using System.Data;
using ItemAsset;

public class ItemMgrDbData : DbRecordData
{
	private List<ItemDbData> mItemList = new List<ItemDbData>();

	public void ExportData(IEnumerable<ItemObject> items)
	{
		mType = EDbOpType.OP_INSERT;
		foreach (ItemObject item in items)
		{
			ItemDbData itemDbData = new ItemDbData();
			itemDbData.ExportData(item);
			mItemList.Add(itemDbData);
		}
	}

	public void UpdateData(IEnumerable<ItemObject> items)
	{
		mType = EDbOpType.OP_UPDATE;
		foreach (ItemObject item in items)
		{
			ItemDbData itemDbData = new ItemDbData();
			itemDbData.UpdateData(item);
			mItemList.Add(itemDbData);
		}
	}

	public void DeleteData(IEnumerable<int> ids)
	{
		mType = EDbOpType.OP_DELETE;
		foreach (int id in ids)
		{
			ItemDbData itemDbData = new ItemDbData();
			itemDbData.DeleteData(id);
			mItemList.Add(itemDbData);
		}
	}

	private void Insert(IDbCommand cmd)
	{
		cmd.CommandText = "INSERT OR REPLACE INTO itemobject(objid,ver,data) VALUES(@objid,@ver,@data);";
		cmd.CommandType = CommandType.Text;
		cmd.Prepare();
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@objid";
		dbDataParameter.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@data";
		dbDataParameter3.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter3);
		foreach (ItemDbData mItem in mItemList)
		{
			dbDataParameter.Value = mItem.Id;
			dbDataParameter2.Value = 273;
			dbDataParameter3.Value = mItem.Data;
			cmd.ExecuteNonQuery();
		}
	}

	private void Update(IDbCommand cmd)
	{
		cmd.CommandText = "Update itemobject SET ver=@ver,data=@data WHERE objid=@objid;";
		cmd.CommandType = CommandType.Text;
		cmd.Prepare();
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@objid";
		dbDataParameter.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter);
		IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
		dbDataParameter2.ParameterName = "@ver";
		dbDataParameter2.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter2);
		IDbDataParameter dbDataParameter3 = cmd.CreateParameter();
		dbDataParameter3.ParameterName = "@data";
		dbDataParameter3.DbType = DbType.Binary;
		cmd.Parameters.Add(dbDataParameter3);
		foreach (ItemDbData mItem in mItemList)
		{
			dbDataParameter.Value = mItem.Id;
			dbDataParameter2.Value = 273;
			dbDataParameter3.Value = mItem.Data;
			cmd.ExecuteNonQuery();
		}
	}

	private void Delete(IDbCommand cmd)
	{
		cmd.CommandText = "DELETE FROM itemobject WHERE objid=@objid;";
		cmd.CommandType = CommandType.Text;
		cmd.Prepare();
		IDbDataParameter dbDataParameter = cmd.CreateParameter();
		dbDataParameter.ParameterName = "@objid";
		dbDataParameter.DbType = DbType.Int32;
		cmd.Parameters.Add(dbDataParameter);
		foreach (ItemDbData mItem in mItemList)
		{
			dbDataParameter.Value = mItem.Id;
			cmd.ExecuteNonQuery();
		}
	}

	public override void Exce(IDbCommand cmd)
	{
		switch (mType)
		{
		case EDbOpType.OP_UPDATE:
			Update(cmd);
			break;
		case EDbOpType.OP_INSERT:
			Insert(cmd);
			break;
		case EDbOpType.OP_DELETE:
			Delete(cmd);
			break;
		}
	}
}
