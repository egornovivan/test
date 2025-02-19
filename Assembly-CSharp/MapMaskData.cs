using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MapMaskData
{
	internal int mId;

	internal int mIconId;

	internal Vector3 mPosition;

	internal string mDescription;

	internal float mRadius;

	internal bool mIsCamp;

	public static List<MapMaskData> s_tblMaskData;

	public static void UnveilAll()
	{
		foreach (MapMaskData s_tblMaskDatum in s_tblMaskData)
		{
			s_tblMaskDatum.mRadius *= 100f;
		}
	}

	public static void LoadDate()
	{
		s_tblMaskData = new List<MapMaskData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MapIcon");
		while (sqliteDataReader.Read())
		{
			MapMaskData mapMaskData = new MapMaskData();
			mapMaskData.mId = Convert.ToInt32(sqliteDataReader.GetString(0));
			mapMaskData.mDescription = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(1)));
			string[] array = sqliteDataReader.GetString(2).Split(',');
			mapMaskData.mPosition = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			mapMaskData.mIconId = Convert.ToInt32(sqliteDataReader.GetString(3));
			mapMaskData.mRadius = Convert.ToSingle(sqliteDataReader.GetString(4));
			int num = Convert.ToInt32(sqliteDataReader.GetString(5));
			if (num == 1)
			{
				mapMaskData.mIsCamp = true;
			}
			else
			{
				mapMaskData.mIsCamp = false;
			}
			s_tblMaskData.Add(mapMaskData);
		}
	}
}
