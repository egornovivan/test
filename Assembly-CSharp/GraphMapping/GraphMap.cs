using System;
using System.IO;
using UnityEngine;

namespace GraphMapping;

public abstract class GraphMap
{
	protected byte[][] mData;

	protected int mDataSize_x;

	protected int mDataSize_y;

	protected int mGraphTexWidth;

	protected int mGraphTexHeight;

	public abstract void LoadTexData(Texture2D tex);

	protected void GetTexPos(Vector2 postion, Vector2 worldSize, IntVector2 tpos)
	{
		float f = postion.x / worldSize.y * (float)mGraphTexWidth;
		float f2 = postion.y / worldSize.y * (float)mGraphTexHeight;
		tpos.x = Mathf.RoundToInt(f);
		tpos.y = Mathf.RoundToInt(f2);
	}

	protected virtual void NewData()
	{
		if (mData == null)
		{
			mData = new byte[mDataSize_x][];
			for (int i = 0; i < mDataSize_x; i++)
			{
				mData[i] = new byte[mDataSize_y];
			}
		}
	}

	public virtual byte[] Serialize()
	{
		if (mData == null)
		{
			return null;
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream(200);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(mGraphTexWidth);
				binaryWriter.Write(mGraphTexHeight);
				binaryWriter.Write(mDataSize_x);
				binaryWriter.Write(mDataSize_y);
				for (int i = 0; i < mDataSize_x; i++)
				{
					binaryWriter.Write(mData[i]);
				}
			}
			return memoryStream.ToArray();
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return null;
		}
	}

	public virtual bool Deserialize(byte[] buf)
	{
		try
		{
			MemoryStream input = new MemoryStream(buf, writable: false);
			using BinaryReader binaryReader = new BinaryReader(input);
			mGraphTexWidth = binaryReader.ReadInt32();
			mGraphTexHeight = binaryReader.ReadInt32();
			mDataSize_x = binaryReader.ReadInt32();
			mDataSize_y = binaryReader.ReadInt32();
			NewData();
			for (int i = 0; i < mDataSize_x; i++)
			{
				mData[i] = binaryReader.ReadBytes(mDataSize_y);
			}
			return true;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
	}
}
