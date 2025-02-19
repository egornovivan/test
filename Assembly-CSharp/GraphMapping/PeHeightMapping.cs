using UnityEngine;

namespace GraphMapping;

public class PeHeightMapping : GraphMap
{
	private const float m_maxInHeightMap = 955f;

	public override void LoadTexData(Texture2D tex)
	{
		if (tex == null)
		{
			Debug.Log("Load Height Texture falied!");
			return;
		}
		mGraphTexWidth = tex.width;
		mGraphTexHeight = tex.height;
		mDataSize_x = tex.width;
		mDataSize_y = tex.height;
		NewData();
		for (int i = 0; i < tex.width; i++)
		{
			for (int j = 0; j + 1 < tex.height; j++)
			{
				Color32 color = tex.GetPixel(i, j);
				mData[i][j] = color.g;
			}
		}
	}

	public float GetHeight(Vector2 postion, Vector2 worldSize)
	{
		if (mData == null)
		{
			Debug.LogError("Get Height Faleid!");
			return 0f;
		}
		IntVector2 tmp = IntVector2.Tmp;
		GetTexPos(postion, worldSize, tmp);
		if (tmp.x >= mDataSize_x || tmp.y >= mDataSize_y || tmp.x < 0 || tmp.y < 0)
		{
			Debug.LogError("postion is error! GetHeight Failed ");
			return 0f;
		}
		return (float)(int)mData[tmp.x][tmp.y] / 255f * 955f;
	}
}
