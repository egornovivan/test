using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;

public class CheatData
{
	public string cheatCode;

	public string successNotice;

	public int addType;

	public int itemID;

	public string isoName;

	private static CheatData[] g_Datas;

	public static void LoadData()
	{
		List<CheatData> list = new List<CheatData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("CheatData");
		while (sqliteDataReader.Read())
		{
			CheatData cheatData = new CheatData();
			cheatData.cheatCode = Db.GetString(sqliteDataReader, "Code").ToLower();
			cheatData.successNotice = PELocalization.GetString(Db.GetInt(sqliteDataReader, "SuccessedNotice"));
			cheatData.addType = Db.GetInt(sqliteDataReader, "AddType");
			cheatData.itemID = Db.GetInt(sqliteDataReader, "ItemID");
			cheatData.isoName = Db.GetString(sqliteDataReader, "ISOName");
			list.Add(cheatData);
		}
		g_Datas = list.ToArray();
	}

	public static CheatData GetData(string code)
	{
		string text = code.ToLower();
		for (int i = 0; i < g_Datas.Length; i++)
		{
			if (g_Datas[i].cheatCode == text)
			{
				return g_Datas[i];
			}
		}
		return null;
	}
}
