using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace NaturalResAsset;

public class NaturalRes
{
	internal int m_id;

	internal List<int> mLevel;

	internal int mIllustrationId;

	internal int m_type;

	internal float m_duration;

	internal List<ResItemGot> m_itemsGot;

	internal ResExtraGot m_extraGot;

	internal ResExtraGot m_extraSpGot;

	internal float mFixedNum;

	internal float mSelfGetNum;

	internal int mGroundEffectID;

	internal int mGroundSoundID;

	public static List<NaturalRes> s_tblNaturalRes;

	public static void LoadData()
	{
		s_tblNaturalRes = new List<NaturalRes>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("resource");
		while (sqliteDataReader.Read())
		{
			NaturalRes naturalRes = new NaturalRes();
			naturalRes.m_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			naturalRes.mLevel = ToTexIdList(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("level")));
			naturalRes.mIllustrationId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("illustration")));
			naturalRes.m_type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("type")));
			naturalRes.m_duration = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("durability")));
			naturalRes.m_itemsGot = ToItemsGot(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("production")));
			naturalRes.m_extraGot = ToExtraGot(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("production")));
			naturalRes.m_extraSpGot = ToExtraSpGot(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("production")));
			naturalRes.mFixedNum = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("fixed_num")));
			naturalRes.mSelfGetNum = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("self_num")));
			s_tblNaturalRes.Add(naturalRes);
		}
	}

	public static bool MatchId(NaturalRes iter, int id)
	{
		return iter.m_id == id;
	}

	private static List<ResItemGot> ToItemsGot(string itemsGotDesc)
	{
		string[] array = itemsGotDesc.Split(';');
		string[] array2 = array[0].Split(',');
		List<ResItemGot> list = new List<ResItemGot>();
		for (int i = 1; i < array2.Length; i += 2)
		{
			ResItemGot resItemGot = new ResItemGot();
			resItemGot.m_id = Convert.ToInt32(array2[i - 1]);
			resItemGot.m_probablity = Convert.ToSingle(array2[i]);
			list.Add(resItemGot);
		}
		return list;
	}

	private static ResExtraGot ToExtraGot(string itemsGotDesc)
	{
		ResExtraGot resExtraGot = new ResExtraGot();
		resExtraGot.extraPercent = 0f;
		resExtraGot.m_extraGot = new List<ResItemGot>();
		string[] array = itemsGotDesc.Split(';');
		if (array.Length != 3)
		{
			return resExtraGot;
		}
		string[] array2 = array[1].Split(',');
		if (array2.Length < 3 || array2.Length % 2 == 0)
		{
			return resExtraGot;
		}
		resExtraGot.extraPercent = Convert.ToSingle(array2[0]);
		for (int i = 2; i < array2.Length; i += 2)
		{
			ResItemGot resItemGot = new ResItemGot();
			resItemGot.m_id = Convert.ToInt32(array2[i - 1]);
			resItemGot.m_probablity = Convert.ToSingle(array2[i]);
			resExtraGot.m_extraGot.Add(resItemGot);
		}
		return resExtraGot;
	}

	private static ResExtraGot ToExtraSpGot(string itemsGotDesc)
	{
		ResExtraGot resExtraGot = new ResExtraGot();
		resExtraGot.extraPercent = 0f;
		resExtraGot.m_extraGot = new List<ResItemGot>();
		string[] array = itemsGotDesc.Split(';');
		if (array.Length != 3)
		{
			return resExtraGot;
		}
		string[] array2 = array[2].Split(',');
		if (array2.Length < 3 || array2.Length % 2 == 0)
		{
			return resExtraGot;
		}
		resExtraGot.extraPercent = Convert.ToSingle(array2[0]);
		for (int i = 2; i < array2.Length; i += 2)
		{
			ResItemGot resItemGot = new ResItemGot();
			resItemGot.m_id = Convert.ToInt32(array2[i - 1]);
			resItemGot.m_probablity = Convert.ToSingle(array2[i]);
			resExtraGot.m_extraGot.Add(resItemGot);
		}
		return resExtraGot;
	}

	private static List<int> ToTexIdList(string texIdDesc)
	{
		string[] array = texIdDesc.Split(',');
		List<int> list = new List<int>();
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(Convert.ToInt32(array[i]));
		}
		return list;
	}

	public static NaturalRes GetTerrainResData(int vType)
	{
		return s_tblNaturalRes.Find((NaturalRes iterRes) => MatchId(iterRes, vType));
	}
}
