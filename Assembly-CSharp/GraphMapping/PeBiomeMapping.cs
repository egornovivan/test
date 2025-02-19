using System;
using UnityEngine;

namespace GraphMapping;

public class PeBiomeMapping : GraphMap
{
	public override void LoadTexData(Texture2D tex)
	{
		if (tex == null)
		{
			Debug.Log("Load Biome Texture falied!");
			return;
		}
		mGraphTexWidth = tex.width;
		mGraphTexHeight = tex.height;
		mDataSize_x = tex.width;
		mDataSize_y = tex.height / 2;
		NewData();
		for (int i = 0; i < tex.width; i++)
		{
			for (int j = 0; j + 1 < tex.height; j += 2)
			{
				int num = Mathf.RoundToInt(tex.GetPixel(i, j).r * 255f / 20f);
				int num2 = Mathf.RoundToInt(tex.GetPixel(i, j + 1).r * 255f / 20f);
				byte b = (byte)(num2 + (num << 4));
				mData[i][Convert.ToInt32(j / 2)] = b;
			}
		}
	}

	public EBiome GetBiom(Vector2 postion, Vector2 worldSize)
	{
		if (mData == null)
		{
			Debug.LogError("Get Biome Faleid!");
			return EBiome.Grassland;
		}
		IntVector2 tmp = IntVector2.Tmp;
		GetTexPos(postion, worldSize, tmp);
		if (tmp.x >= mDataSize_x || tmp.y >> 1 >= mDataSize_y || tmp.x < 0 || tmp.y < 0)
		{
			Debug.LogError("postion is error! GetBiom Failed ");
			return EBiome.Grassland;
		}
		byte b = mData[tmp.x][tmp.y >> 1];
		return (((tmp.y & 1) != 0) ? (b & 0xF) : ((b & 0xF0) >> 4)) switch
		{
			0 => EBiome.Grassland, 
			1 => EBiome.Desert, 
			2 => EBiome.Mountainous, 
			3 => EBiome.Canyon, 
			4 => EBiome.Forest, 
			5 => EBiome.Jungle, 
			6 => EBiome.Marsh, 
			7 => EBiome.Volcano, 
			_ => EBiome.Grassland, 
		};
	}
}
