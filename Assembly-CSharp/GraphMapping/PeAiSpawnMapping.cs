using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace GraphMapping;

public class PeAiSpawnMapping : GraphMap
{
	private const float m_maxInHeightMap = 955f;

	private Dictionary<int, byte> mColorMap;

	private bool LoadAiSpawnDb()
	{
		if (LocalDatabase.Instance == null)
		{
			Debug.LogError("Load AiSpawn Texture falied! LocalDatabase is null!");
			return false;
		}
		mColorMap = new Dictionary<int, byte>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("aispawn_story");
		while (sqliteDataReader.Read())
		{
			Color32 c = new Color32(sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("R")), sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("G")), sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("B")), sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("A")));
			mColorMap[Color32toInt(c)] = sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("Id"));
		}
		return true;
	}

	private int Color32toInt(Color32 c32)
	{
		return c32.r | (c32.g << 8) | (c32.b << 16);
	}

	private Color32 IntToColor32(int value)
	{
		byte r = (byte)(value & 0xFF);
		byte g = (byte)((value & 0xFF00) >> 8);
		byte b = (byte)((value & 0xFF0000) >> 16);
		return new Color32(r, g, b, byte.MaxValue);
	}

	private int GetColorDifference(int key1, int key2)
	{
		byte b = (byte)(key1 & 0xFF);
		byte b2 = (byte)((key1 & 0xFF00) >> 8);
		byte b3 = (byte)((key1 & 0xFF0000) >> 16);
		byte b4 = (byte)(key2 & 0xFF);
		byte b5 = (byte)((key2 & 0xFF00) >> 8);
		byte b6 = (byte)((key2 & 0xFF0000) >> 16);
		return Mathf.Abs(b - b4) + Mathf.Abs(b2 - b5) + Mathf.Abs(b3 - b6);
	}

	private byte GetNearMapID(int _key)
	{
		int key = 0;
		int num = int.MaxValue;
		foreach (KeyValuePair<int, byte> item in mColorMap)
		{
			int colorDifference = GetColorDifference(_key, item.Key);
			if (colorDifference < num)
			{
				key = item.Key;
				num = colorDifference;
			}
		}
		return mColorMap[key];
	}

	public override void LoadTexData(Texture2D tex)
	{
		if (tex == null)
		{
			Debug.LogError("Load AiSpawn Texture falied! texture is null!");
		}
		else
		{
			if (!LoadAiSpawnDb())
			{
				return;
			}
			mGraphTexWidth = tex.width;
			mGraphTexHeight = tex.height;
			mDataSize_x = tex.width;
			mDataSize_y = tex.height;
			NewData();
			int num = 0;
			for (int i = 0; i < tex.width; i++)
			{
				for (int j = 0; j < tex.height; j++)
				{
					Color32 c = tex.GetPixel(i, j);
					int key = Color32toInt(c);
					if (mColorMap.ContainsKey(key))
					{
						mData[i][j] = mColorMap[key];
						continue;
					}
					mData[i][j] = GetNearMapID(key);
					num++;
				}
			}
			Debug.Log("(Load AiSpawn) Error point count:" + num);
		}
	}

	public int GetAiSpawnMapId(Vector2 postion, Vector2 worldSize)
	{
		if (mData == null)
		{
			Debug.LogError("[Error]Get AiSpawn Faleid!");
			return -1;
		}
		IntVector2 tmp = IntVector2.Tmp;
		GetTexPos(postion, worldSize, tmp);
		if (tmp.x >= mDataSize_x || tmp.y >= mDataSize_y || tmp.x < 0 || tmp.y < 0)
		{
			Debug.LogError("postion is error! GetAiSpawnMapId Failed ");
			return 0;
		}
		return mData[tmp.x][tmp.y];
	}
}
