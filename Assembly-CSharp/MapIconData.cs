using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class MapIconData
{
	public int mId;

	public string mIconName;

	public MapMaskType mMaskType;

	public static List<MapIconData> s_tblIconInfo;

	public static List<MapIconData> s_tblCustomInfo;

	public static void LoadDate()
	{
		s_tblIconInfo = new List<MapIconData>();
		s_tblCustomInfo = new List<MapIconData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Icon");
		while (sqliteDataReader.Read())
		{
			MapIconData mapIconData = new MapIconData();
			mapIconData.mId = Convert.ToInt32(sqliteDataReader.GetString(0));
			mapIconData.mIconName = sqliteDataReader.GetString(1);
			mapIconData.mMaskType = (MapMaskType)Convert.ToInt32(sqliteDataReader.GetString(2));
			if (mapIconData.mMaskType == MapMaskType.Custom)
			{
				s_tblCustomInfo.Add(mapIconData);
			}
			s_tblIconInfo.Add(mapIconData);
		}
	}
}
