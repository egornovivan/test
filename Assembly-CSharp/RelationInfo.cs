using System;
using Mono.Data.SqliteClient;

public class RelationInfo
{
	public string relationLevelEN;

	public string relationLevelCN;

	public bool warState;

	public bool specialMission;

	public bool normalMission;

	public bool canUseShop;

	public float shopPriceScale;

	public bool canUseBuilding;

	private static RelationInfo[] g_RelationInfos;

	public static RelationInfo GetData(ReputationSystem.ReputationLevel level)
	{
		return g_RelationInfos[(int)level];
	}

	public static void LoadData()
	{
		g_RelationInfos = new RelationInfo[9];
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("RelationInfo");
		sqliteDataReader.Read();
		int num = 0;
		while (sqliteDataReader.Read())
		{
			RelationInfo relationInfo = new RelationInfo();
			relationInfo.relationLevelEN = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RelationLevel"));
			relationInfo.relationLevelCN = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RelationLevel_CN"));
			relationInfo.warState = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("WarState"))) > 0;
			relationInfo.specialMission = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SpecialMission"))) > 0;
			relationInfo.normalMission = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NormalMission"))) > 0;
			relationInfo.canUseBuilding = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Building"))) > 0;
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Shop"));
			string[] array = @string.Split(',');
			relationInfo.canUseShop = Convert.ToInt32(array[0]) > 0;
			relationInfo.shopPriceScale = Convert.ToSingle(array[1]);
			g_RelationInfos[9 - ++num] = relationInfo;
		}
	}
}
