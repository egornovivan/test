using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;

public class PublicData
{
	public static PublicData Self = new PublicData();

	public List<Replicator.KnownFormula> _storyForumlaList = new List<Replicator.KnownFormula>();

	public List<int> _storyMetalScan = new List<int>();

	public bool bChanged;

	public void LoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM publicdata;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			reader.GetInt32(reader.GetOrdinal("ver"));
			reader.GetInt32(reader.GetOrdinal("id"));
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			Serialize.Import(buff, delegate(BinaryReader r)
			{
				int num = BufferHelper.ReadInt32(r);
				for (int i = 0; i < num; i++)
				{
					int id = BufferHelper.ReadInt32(r);
					bool flag = BufferHelper.ReadBoolean(r);
					_storyForumlaList.Add(new Replicator.KnownFormula
					{
						id = id,
						flag = flag
					});
				}
				num = BufferHelper.ReadInt32(r);
				for (int j = 0; j < num; j++)
				{
					_storyMetalScan.Add(BufferHelper.ReadInt32(r));
				}
				MonsterHandbookData.Deserialize(BufferHelper.ReadBytes(r));
			});
		}
	}

	public void Save()
	{
		if (!bChanged)
		{
			return;
		}
		byte[] data = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, _storyForumlaList.Count);
			foreach (Replicator.KnownFormula storyForumla in _storyForumlaList)
			{
				BufferHelper.Serialize(w, storyForumla.id);
				BufferHelper.Serialize(w, storyForumla.flag);
			}
			BufferHelper.Serialize(w, _storyMetalScan.Count);
			foreach (int item in _storyMetalScan)
			{
				BufferHelper.Serialize(w, item);
			}
			BufferHelper.Serialize(w, MonsterHandbookData.Serialize());
		});
		PublicNetData publicNetData = new PublicNetData();
		publicNetData.ExportData(1, data);
		AsyncSqlite.AddRecord(publicNetData);
		bChanged = false;
	}
}
